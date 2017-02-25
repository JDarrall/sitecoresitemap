using System.Collections.Generic;
using System.Linq;
using Sitecore;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.SearchTypes;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Globalization;
using Sitemap.Xml.Models;

namespace Sitemap.Xml.Pipelines
{
    public class DefaultSitemapXmlProcessor : CreateSitemapXmlProcessor
    {
        #region fields
        private readonly string indexName;
        #endregion

        public DefaultSitemapXmlProcessor()
        {
        }

        public DefaultSitemapXmlProcessor(string indexName)
        {
            this.indexName = indexName;
        }

        private IEnumerable<UrlDefinition> ProcessSite(Item homeItem, SiteDefinition def, Language language)
        {
            IProviderSearchContext ctx;
            if (string.IsNullOrEmpty(indexName))
                ctx = ContentSearchManager.GetIndex((SitecoreIndexableItem)homeItem).CreateSearchContext();
            else
                ctx = ContentSearchManager.GetIndex(indexName).CreateSearchContext();

            try
            {
                var results = ctx.GetQueryable<SitemapResultItem>()
                    .Where(i => i.Paths.Contains(homeItem.ID) && i.Language == language.Name);

                List<UrlDefinition> allUrlDefinitions = new List<UrlDefinition>();

                foreach (var item in results)
                {                   
                     allUrlDefinitions.Add(new UrlDefinition(item.SitemapUrl, item.Updated));
                }


                //TODO Wildcard Urls
                //var configuredSites = Sitecore.Configuration.Factory.GetConfigNode("sitemap/wildcards").ChildNodes;
                //foreach (XmlNode tmpl in configuredSites)
                //{
                //    if (tmpl.Name == "wildcard")
                //    {
                //        var rootItem = Sitecore.Context.Database.GetItem(XmlUtil.GetAttribute("rootItemId", tmpl));
                //        var urlParentItem = Sitecore.Context.Database.GetItem(XmlUtil.GetAttribute("urlParentItem", tmpl));
                //        var wildcardResults = ctx.GetQueryable<SitemapResultItem>().Where(i => i.Paths.Contains(rootItem.ID) && i.Language == language.Name);

                //        var wildcardItems = wildcardResults
                //            .Select(i => Sitecore.Configuration.Factory.GetDatabase(i.DatabaseName).GetItem(i.ItemId, Language.Parse(i.Language), Sitecore.Data.Version.Latest))
                //            .ToList().Where(i => i.Fields["Sitemap display"] != null && i.Fields["Sitemap display"].Value.Equals("1"));

                //        foreach (var item in wildcardItems)
                //        {
                //            if (item.Versions.Count > 0)
                //            {
                //                var wildcardItemUrl = LinkManager.GetItemUrl(urlParentItem) + "/" + item.Name.ToLower().Replace(" ", "-");
                //                allUrlDefinitions.Add(new UrlDefinition(wildcardItemUrl, item.Statistics.Updated));
                //            }

                //        }

                //    }
                //}
                return allUrlDefinitions;

            }
            finally
            {
                ctx.Dispose();
            }
        }

        public override void Process(CreateSitemapXmlArgs args)
        {
            var langs = LanguageManager.GetLanguages(Context.Database);
            var homeItem = Context.Database.GetItem(args.Site.RootPath + args.Site.StartItem);
            var def = Configuration[args.Site.Name];
            if (def.EmbedLanguage)
            {
                foreach (var lang in langs)
                    args.Nodes.AddRange(ProcessSite(homeItem, def, lang));
            }
            else
            {
                args.Nodes.AddRange(ProcessSite(homeItem, def, Context.Language));
            }
        }
    }

    class SitemapResultItem : SearchResultItem
    {
        [IndexField("_templates")]
        public IEnumerable<ID> AllTemplates { get; set; }

        public string SitemapUrl { get; set; }
    }
}
