using System.Linq;
using System.Web;
using Sitecore.Web;

namespace Sitemap.Xml.Extensions
{
    public static class SiteService
    {
        public static SiteInfo GetSiteFromHostName(this HttpContext context)
        {
            return Sitecore.Configuration.Factory.GetSiteInfoList()
               .FirstOrDefault(i => i.HostName.ToLower().Split('|').Contains(context.Request.Url.Host.ToLower()));
        }
    }
}
