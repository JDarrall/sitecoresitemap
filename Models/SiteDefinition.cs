using System.Collections.Generic;

namespace Sitemap.Xml.Models
{
    public sealed class SiteDefinition
    {
        public bool EmbedLanguage { get; set; }
        public bool UseDisplayName { get; set; }
        public string IndexName { get; set; }
        public string SiteName { get; set; }
        public string DisplayInSitemapFieldName { get; set; }
        public string SitemapUrlFieldName { get; set; }
        public bool UseHttps { get; set; }
        public string HostName { get; set; }
        public List<string> Templates { get; set; }

        public string LeftPart => (UseHttps ? "https://" : "http://") + HostName;
    }
}
