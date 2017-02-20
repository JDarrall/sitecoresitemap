using System;

namespace Sitemap.Xml.Models
{
    public sealed class UrlDefinition
    {
        public string Location { get; private set; }
        public DateTime LastModified { get; private set; }

        public UrlDefinition(string location, DateTime lastModified)
        {
            this.Location = location;
            this.LastModified = lastModified;
        }
    }
}
