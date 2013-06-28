using System.Collections.Generic;
using ServiceStack.Configuration;

namespace MvcApplication1.App_Start
{
    public class AppConfig
    {
        public AppConfig(IResourceManager appSettings)
        {
            this.Env = appSettings.Get("Env", Env.Local);
            this.EnableCdn = appSettings.Get("EnableCdn", false);
            this.CdnPrefix = appSettings.Get("CdnPrefix", "");
            this.AdminUserNames = appSettings.Get("AdminUserNames", new List<string>());
        }
        
        public Env Env { get; set; }
        public bool EnableCdn { get; set; }
        public string CdnPrefix { get; set; }
        public List<string> AdminUserNames { get; set; }
    }
}