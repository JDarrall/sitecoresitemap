using Sitecore.Xml;
using System.Collections.Generic;
using System.Xml;
using Sitemap.Xml.Models;

namespace Sitemap.Xml.Pipelines
{
    public abstract class CreateSitemapXmlProcessor
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

                        var templateNode = node.SelectSingleNode("Templates");
                        if (templateNode != null)
                        {
                            foreach (XmlNode template in templateNode.ChildNodes)
                            {
                                def.Templates.Add(template.InnerText);
                            }
                        }                      

                        _configuration.Add(XmlUtil.GetAttribute("name", node), def);
                    }
                }
                return _configuration;
            }
        }

        public abstract void Process(CreateSitemapXmlArgs args);
    }
}
