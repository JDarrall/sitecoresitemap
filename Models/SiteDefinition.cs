using System.Collections.Generic;

namespace Sitemap.Xml.Models
{
    public sealed class SiteDefinition
    {
        public bool EmbedLanguage { get; set; }
        public bool UseDisplayName { get; set; }
        public string IndexName { get; set; }
        public string SiteName { get; set; }
        public string FieldName { get; set; }
        public List<string> Templates { get; set; } 
    }
}
