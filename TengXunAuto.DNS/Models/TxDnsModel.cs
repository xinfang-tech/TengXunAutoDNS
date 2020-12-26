using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TengXunAuto.DNS.Models
{
    public class TxDnsModel
    {
        public TxDnsModel(string _ip, int _ttl)
        {
            ip = _ip;
            ttl = _ttl;
        }

        public string ip { get; set; }
        public int ttl { get; set; }
    }
}
