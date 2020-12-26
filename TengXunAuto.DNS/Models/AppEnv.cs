using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TengXunAuto.DNS.Models
{
    public class AppEnv
    {
        private static SiteConfig site;

        public static SiteConfig Site
        {
            get
            {
                return site;
            }
            set
            {
                site = value;
            }
        }

        public static SiteConfig DevelopSite { get => develop_site; set => develop_site = value; }

        private static SiteConfig develop_site;


    }
}
