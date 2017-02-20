using Sitecore.ContentSearch;
using Sitecore.ContentSearch.SearchTypes;
using Sitecore.Sites;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using Sitecore.Globalization;
using Sitecore.Links;
using Sitecore.Xml;
using Sitemap.Xml.Models;

namespace Sitemap.Xml.Pipelines
{
    public class DefaultSitemapXmlProcessor : CreateSitemapXmlProcessor
    {
        #region fields
        private string indexName;
        private readonly string _secure = "https";
        private readonly string _insecure = "http";
        #endregion

        public DefaultSitemapXmlProcessor()
        {
        }

        public DefaultSitemapXmlProcessor(string indexName)
        {
            this.indexName = indexName;
        }

        private IEnumerable<UrlDefinition> ProcessSite(Sitecore.Data.Items.Item homeItem, SiteDefinition def, Sitecore.Globalization.Language language)
        {
            IProviderSearchContext ctx;
            if (string.IsNullOrEmpty(this.indexName))
                ctx = ContentSearchManager.GetIndex((SitecoreIndexableItem)homeItem).CreateSearchContext();
            else
                ctx = ContentSearchManager.GetIndex(this.indexName).CreateSearchContext();

            try
            {
                var results = ctx.GetQueryable<SitemapResultItem>()
                    .Where(i => i.Paths.Contains(homeItem.ID) && i.Language == language.Name);

                var items = results
                    .Select(i => Sitecore.Configuration.Factory.GetDatabase(i.DatabaseName).GetItem(i.ItemId, Language.Parse(i.Language), Sitecore.Data.Version.Latest))
                    .ToList().Where(i => i.Fields["Sitemap display"] != null && i.Fields["Sitemap display"].Value.Equals("1"));


                List<UrlDefinition> allUrlDefinitions = new List<UrlDefinition>();

                var options = GetUrlOptions(def, language);

                foreach (var item in items)
                {
                    if (item.Versions.Count > 0)
                    {
                        allUrlDefinitions.Add(new UrlDefinition(LinkManager.GetItemUrl(item, options), item.Statistics.Updated));
                    }
                }


                var configuredSites = Sitecore.Configuration.Factory.GetConfigNode("sitemap/wildcards").ChildNodes;
                foreach (XmlNode tmpl in configuredSites)
                {
                    if (tmpl.Name == "wildcard")
                    {
                        var rootItem = Sitecore.Context.Database.GetItem(XmlUtil.GetAttribute("rootItemId", tmpl));
                        var urlParentItem = Sitecore.Context.Database.GetItem(XmlUtil.GetAttribute("urlParentItem", tmpl));
                        var wildcardResults = ctx.GetQueryable<SitemapResultItem>().Where(i => i.Paths.Contains(rootItem.ID) && i.Language == language.Name);

                        var wildcardItems = wildcardResults
                            .Select(i => Sitecore.Configuration.Factory.GetDatabase(i.DatabaseName).GetItem(i.ItemId, Language.Parse(i.Language), Sitecore.Data.Version.Latest))
                            .ToList().Where(i => i.Fields["Sitemap display"] != null && i.Fields["Sitemap display"].Value.Equals("1"));

                        foreach (var item in wildcardItems)
                        {
                            if (item.Versions.Count > 0)
                            {
                                var wildcardItemUrl = LinkManager.GetItemUrl(urlParentItem, options) + "/" + item.Name.ToLower().Replace(" ", "-");
                                allUrlDefinitions.Add(new UrlDefinition(wildcardItemUrl, item.Statistics.Updated));
                            }

                        }

                    }
                }
                return allUrlDefinitions;

            }
            finally
            {
                ctx.Dispose();
            }
        }

        private static UrlOptions GetUrlOptions(SiteDefinition def, Language language)
        {
            var options = Sitecore.Links.UrlOptions.DefaultOptions;
            options.SiteResolving = Sitecore.Configuration.Settings.Rendering.SiteResolving;
            options.Site = SiteContext.GetSite(def.SiteName);
            options.LanguageEmbedding = def.EmbedLanguage ? Sitecore.Links.LanguageEmbedding.Always : Sitecore.Links.LanguageEmbedding.Never;
            options.AlwaysIncludeServerUrl = true;
            options.Language = language;
            options.AddAspxExtension = false;
            options.LowercaseUrls = true;
            return options;
        }

        public override void Process(CreateSitemapXmlArgs args)
        {
            var langs = Sitecore.Data.Managers.LanguageManager.GetLanguages(Sitecore.Context.Database);
            var homeItem = Sitecore.Context.Database.GetItem(args.Site.RootPath + args.Site.StartItem);
            var request = HttpContext.Current.Request;

            if (!request.IsSecureConnection)
            {
                HttpContext.Current.Response.RedirectPermanent(GetHttpsUrl(), true);    
            }

            var def = this.Configuration[args.Site.Name];
            if (def.EmbedLanguage)
            {
                foreach (var lang in langs)
                    args.Nodes.AddRange(ProcessSite(homeItem, def, lang));
            }
            else
            {
                args.Nodes.AddRange(ProcessSite(homeItem, def, Sitecore.Context.Language));
            }
        }

        private string GetHttpsUrl()
        {
            var pattern = new Regex(_insecure, RegexOptions.IgnoreCase);
            return pattern.Replace(HttpContext.Current.Request.Url.AbsoluteUri, _secure, 1);
        }
    }

    class SitemapResultItem : SearchResultItem
    {
        [IndexField("_templates")]
        public IEnumerable<Sitecore.Data.ID> AllTemplates { get; set; }
    }
}
