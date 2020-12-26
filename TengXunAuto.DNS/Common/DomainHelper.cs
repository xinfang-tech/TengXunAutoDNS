using System;
using System.Collections.Generic;
using System.Text;
using TengXunAuto.DNS.Models;

namespace TengXunAuto.DNS.Common
{
    public class DomainHelper
    {
        /// <summary>
        /// 域名拿DNS信息 目前用了两个数据源 119和114
        /// </summary>
        /// <param name="domain"></param>
        /// <returns></returns>
        public static List<TxDnsModel> GetDnsListByDomain(string domain)
        {
            List<TxDnsModel> txDnsList = new List<TxDnsModel>();

            // 119.29.29.29 查询
            try
            {
                string dnsSearchUrl = "http://119.29.29.29/d?ttl=1&dn=" + domain;

                WebClientPro webClient = new WebClientPro();
                var dnsIp = webClient.DownloadString(dnsSearchUrl);
                //Console.WriteLine(domain + ":" + dnsIp);
                if (string.IsNullOrWhiteSpace(dnsIp))
                {
                    return txDnsList;
                }

                var dnsRes = dnsIp.Split(',');
                if (dnsRes.Length != 2)
                {
                    return txDnsList;
                }
                var ipList = dnsRes[0].Split(';');
                if (ipList.Length == 0)
                {
                    return txDnsList;
                }
                int dnsTtl = int.Parse(dnsRes[1]);

                foreach (var ip in ipList)
                {
                    //Console.WriteLine("dns119 ip:" + ip + " ttl:" + dnsTtl);
                    txDnsList.Add(new TxDnsModel(ip, dnsTtl));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(" Dns 119.29.29.29 Err:" + ex.ToString());
            }



            // 119.29.29.29 如果未查询到，则从114再次查询
            if (txDnsList.Count == 0)
            {

                try
                {
                    string dnsSearchUrl = "http://114.114.114.114/d?ttl=y&type=a&dn=" + domain;
                    WebClientPro webClient = new WebClientPro();
                    var dnsIp = webClient.DownloadString(dnsSearchUrl);
                    //Console.WriteLine(dnsIp);
                    if (string.IsNullOrWhiteSpace(dnsIp))
                    {
                        return txDnsList;
                    }

                    var dnsRes = dnsIp.Split(',');
                    if (dnsRes.Length != 2)
                    {
                        return txDnsList;
                    }
                    var ipList = dnsRes[0].Split(';');
                    if (ipList.Length == 0)
                    {
                        return txDnsList;
                    }
                    int dnsTtl = int.Parse(dnsRes[1]);

                    foreach (var ip in ipList)
                    {
                        Console.WriteLine("dns114 ip:" + ip + " ttl:" + dnsTtl);
                        txDnsList.Add(new TxDnsModel(ip, dnsTtl));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(" Dns114.114.114.114 Err:" + ex.ToString());
                }

            }
            return txDnsList;
        }



    }
}
