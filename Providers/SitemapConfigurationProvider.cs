using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using Sitecore.Web;
using Sitecore.Xml;
using Sitemap.Xml.Extensions;
using Sitemap.Xml.Models;

namespace Sitemap.Xml.Providers
{
    public class SitemapConfigurationProvider
    {
        private Dictionary<string, SiteDefinition> _configuration;

        protected Dictionary<string, SiteDefinition> Configuration
        {
            get
            {
                if (_configuration == null)
                {
                    _configuration = new Dictionary<string, SiteDefinition>();
                    foreach (XmlNode node in Sitecore.Configuration.Factory.GetConfigNodes("sitemap/site"))
                    {
                        var def = SiteDefinition(node);

                        _configuration.Add(XmlUtil.GetAttribute("name", node), def);
                    }
                }
                return _configuration;
            }
        }

        private static SiteDefinition SiteDefinition(XmlNode node)
        {
            var def = new SiteDefinition
            {
                EmbedLanguage = true,
                SiteName = XmlUtil.GetAttribute("name", node),
            };

            var embed = XmlUtil.GetAttribute("embedLanguage", node);
            if (!string.IsNullOrEmpty(embed))
            {
                bool embedVal;
                if (!bool.TryParse(embed, out embedVal))
                    embedVal = true;
                def.EmbedLanguage = embedVal;
            }

            var useDisplayName = XmlUtil.GetAttribute("useDisplayName", node);
            if (!string.IsNullOrEmpty(useDisplayName))
            {
                bool useDisplay;
                if (!bool.TryParse(useDisplayName, out useDisplay))
                    useDisplay = false;

                def.UseDisplayName = useDisplay;
            }

            var indexName = XmlUtil.GetAttribute("IndexName", node);
            def.IndexName = !string.IsNullOrEmpty(indexName) ? indexName : "sitecore_web_index";

            var fieldName = XmlUtil.GetAttribute("DisplayInSitemapFieldName", node);
            def.DisplayInSitemapFieldName = !string.IsNullOrEmpty(fieldName) ? fieldName : "Display in Sitemap";

            var urlFieldName = XmlUtil.GetAttribute("SitemapUrlFieldname", node);
            def.SitemapUrlFieldName = !string.IsNullOrEmpty(urlFieldName) ? urlFieldName : "Sitemap Url";

            var useHttps = XmlUtil.GetAttribute("UseHttps", node);
            def.UseHttps = useHttps != null && useHttps.Equals("true");

            var hostName = XmlUtil.GetAttribute("HostName", node);
            def.HostName = !string.IsNullOrEmpty(hostName) ? hostName : HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);

            var templateNode = node.SelectSingleNode("Templates");
            if (templateNode == null) return def;

            foreach (XmlNode template in templateNode.ChildNodes)
            {
                def.Templates.Add(template.InnerText);
            }
            return def;
        }

        public SiteDefinition ProvideConfiguration(SiteInfo siteInfo)
        {
            return Sitecore.Configuration.Factory.GetConfigNodes("sitemap/site")
                .Cast<XmlNode>().Where(site => XmlUtil.GetAttribute("name", site)
                .Equals(siteInfo.Name)).Select(SiteDefinition).FirstOrDefault();
        }
    }
}
