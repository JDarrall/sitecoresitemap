using System;
using System.Web;
using Sitecore;
using Sitecore.Data.Events;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Events;
using Sitecore.Links;
using Sitecore.Sites;
using Sitemap.Xml.Extensions;
using Sitemap.Xml.Models;
using Sitemap.Xml.Providers;

namespace Sitemap.Xml.Handlers
{
    public class SitemapUrlSaveEventHandler
    {
        protected void OnItemSaved(object sender, EventArgs args)
        {
            var provider = new SitemapConfigurationProvider();

            Assert.IsNotNull(sender, "sender is null");
            Assert.IsNotNull(args, "args is null");

            using (new EventDisabler())
            {
                var item = Event.ExtractParameter(args, 0) as Item;                

                if (item?.Database != null && item.Database.Name != "master") return;

                var site = HttpContext.Current.GetSiteFromHostName();
                var sitemapArgs = provider.ProvideConfiguration(site);

                if (sitemapArgs == null || item?.Fields[sitemapArgs.SitemapUrlFieldName] == null
                    || item.Fields[sitemapArgs.DisplayInSitemapFieldName]?.Value != "1") return;

                UpdateUrlField(sitemapArgs, item);
            }
        }

        private static void UpdateUrlField([NotNull]SiteDefinition sitemapArgs, [NotNull]Item item)
        {
            var options = UrlOptions.DefaultOptions;
            options.AlwaysIncludeServerUrl = false;
            options.Site = SiteContext.GetSite(sitemapArgs.SiteName);
            options.UseDisplayName = sitemapArgs.UseDisplayName;
            options.LanguageEmbedding = sitemapArgs.EmbedLanguage ? LanguageEmbedding.Always : LanguageEmbedding.Never;
           
            using (new EditContext(item))
            {
                item[sitemapArgs.SitemapUrlFieldName] = sitemapArgs.LeftPart + LinkManager.GetItemUrl(item, options);
            }
        }
    }
}
