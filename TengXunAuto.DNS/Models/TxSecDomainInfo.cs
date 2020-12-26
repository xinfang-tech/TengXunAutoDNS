using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TengXunAuto.DNS.Models
{
    public class TxSecDomainInfo
    {
        //public ObjectId _id { get; set; }
        /// <summary>
        /// 允许 阻止
        /// </summary>
        public string Action { get; set; }
        /// <summary>
        /// 域名
        /// </summary>
        public string DomainName { get; set; }
        /// <summary>
        /// 域名Ttl
        /// </summary>
        public int DomainTtl { get; set; }
        /// <summary>
        /// 大区名称
        /// </summary>
        public string Region { get; set; }
        public string PortList { get; set; }
        /// <summary>
        /// 公司机器IP
        /// </summary>
        public string ServerIp { get; set; }
        /// <summary>
        /// 修改前IP
        /// </summary>
        public string DomainOldIp { get; set; }
        /// <summary>
        /// 腾讯云 SecurityGroupId
        /// </summary>
        public string TxSecurityGroupId { get; set; }
        /// <summary>
        /// 腾讯云 SecurityGroupId
        /// </summary>
        public string TxSecurityGroupName { get; set; }
        /// <summary>
        /// 本次修改IP
        /// </summary>
        public string DomainNewIp { get; set; }
        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime Cdate { get; set; }
        /// <summary>
        /// TCP UDP
        /// </summary>
        public string Protocol { get; set; }
        public string Remarks { get; set; }
    }
}
