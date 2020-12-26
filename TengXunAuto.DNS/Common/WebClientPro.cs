using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TengXunAuto.DNS.Common
{
    public class WebClientPro : WebClient
    {
        public int Timeout { get; set; }
        public Cookie cookie { get; set; }
        public WebClientPro(int timeout = 2000, Cookie _cookie = null)
        {
            Timeout = timeout;
            Encoding = Encoding.UTF8;
            cookie = _cookie;
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            HttpWebRequest request = (HttpWebRequest)base.GetWebRequest(address);
            request.Timeout = Timeout;
            request.ReadWriteTimeout = Timeout;

            if (cookie != null)
            {
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.Add(cookie);
            }

            return request;
        }
    }
}
