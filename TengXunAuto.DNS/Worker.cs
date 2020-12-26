using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;
using TencentCloud.Common;
using TencentCloud.Common.Profile;
using TencentCloud.Vpc.V20170312;
using TencentCloud.Vpc.V20170312.Models;
using TengXunAuto.DNS.Models;
using TengXunAuto.DNS.Common;

namespace TengXunAuto.DNS
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        public static Credential cred = new Credential();

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            cred = new Credential
            {
                SecretId = AppEnv.Site.TxCloudAppInfo.SecretId,
                SecretKey = AppEnv.Site.TxCloudAppInfo.SecretKey,
            };

            ClientProfile clientProfile = new ClientProfile();
            HttpProfile httpProfile = new HttpProfile();
            httpProfile.Endpoint = ("vpc.tencentcloudapi.com");
            clientProfile.HttpProfile = httpProfile;
            string region = "ap-shanghai"; 
            VpcClient client = new VpcClient(cred, region, clientProfile);


            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("DNS Worker running at: {time}", DateTimeOffset.Now);


                var securityGroupList = await GetSecRuleList(client);

                Console.WriteLine("安全组总量：" + securityGroupList.Count + " time:" + DateTime.Now);

                foreach (var groupId in securityGroupList)
                {
                    SecurityGroupPolicy[] egress = new SecurityGroupPolicy[0];
                    try
                    {
                        var reqRule = DescribeSecurityGroupPoliciesRequest.FromJsonString<DescribeSecurityGroupPoliciesRequest>("{\"SecurityGroupId\":\"" + groupId.SecurityGroupId + "\"}");
                        DescribeSecurityGroupPoliciesResponse respRule = await client.DescribeSecurityGroupPolicies(reqRule).ConfigureAwait(false);

                        egress = respRule.SecurityGroupPolicySet.Egress;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("DescribeSecurityGroupPolicies " + ex.ToString());

                        continue;
                    }


                    //显示全部安全规则
                    foreach (var egr in egress)
                    {
                        string ruleDetail = groupId.SecurityGroupId + "|" + groupId.SecurityGroupDesc + "|" + egr.CidrBlock + "|" + egr.Port + "|" + egr.Protocol + "|" + egr.Action + "|" + egr.PolicyDescription;
                        ruleDetail = ruleDetail.Replace(",", "，").Replace("|", ",");
                        Console.WriteLine(ruleDetail);
                    }

                    //安全组名字前缀为 “Auto_”的，可以查找域名对应IP，然后增加到安全组中
                    if (!groupId.SecurityGroupName.StartsWith("Auto_"))
                    {
                        continue;
                    }
                    string domainName = groupId.SecurityGroupName.Replace("Auto_", "");

                    List<TxDnsModel> txDnsList = new List<TxDnsModel>();
                    if (domainName.IndexOf(',') > -1)
                    {
                        var domList = domainName.Split(',');
                        for (int i = 0; i < domList.Length; i++)
                        {
                            if (string.IsNullOrWhiteSpace(domList[i]))
                            {
                                continue;
                            }
                            var dnsList = DomainHelper.GetDnsListByDomain(domList[i]);
                            txDnsList.AddRange(dnsList);
                        }
                    }
                    else
                    {
                        txDnsList = DomainHelper.GetDnsListByDomain(domainName);
                    }


                    var egrList = egress.Select(tmp => tmp.CidrBlock).ToList();
                    var portList = egress.Select(tmp => tmp.Port).ToArray();

                    var newDndsList = txDnsList.Select(tmp => tmp.ip).ToList();
                    foreach (var itemDns in txDnsList)
                    {
                        foreach (var egr in egress)
                        {
                            if (itemDns.ip == egr.CidrBlock && newDndsList.Any(tmp => tmp == egr.CidrBlock))
                            {
                                newDndsList.Remove(egr.CidrBlock);
                            }
                        }
                    }

                    if (!newDndsList.Any())
                    {
                        continue;
                    }

                    foreach (var itemIp in newDndsList)
                    {
                        Console.WriteLine("ADD ITEM:" + itemIp);

                        TxSecDomainInfo model = new TxSecDomainInfo()
                        {
                            DomainName = domainName,
                            PortList = string.Join(',', portList.Distinct()),
                            Region = region,
                            ServerIp = "",
                            DomainNewIp = itemIp,
                            DomainTtl = 100,
                            Protocol = "TCP",
                            DomainOldIp = "",
                            TxSecurityGroupId = groupId.SecurityGroupId,
                            Remarks = "DNS",
                            Cdate = DateTime.Now,
                            Action = "ACCEPT"
                        };

                        var res = await AddSecRule(model);
                        Console.WriteLine(res);
                    }

                    Thread.Sleep(100);
                }

                //每10分钟跑一次
                await Task.Delay(1000 * 60 * 10, stoppingToken);
            }
        }

        public async Task<List<SecurityGroup>> GetSecRuleList(VpcClient client)
        {

            List<SecurityGroup> securityGroupList = new List<SecurityGroup>();
            try
            {

                DescribeSecurityGroupsRequest req = new DescribeSecurityGroupsRequest();
                string strParams = "{\"Limit\":100}";
                req = DescribeSecurityGroupsRequest.FromJsonString<DescribeSecurityGroupsRequest>(strParams);
                DescribeSecurityGroupsResponse resp = await client.DescribeSecurityGroups(req).
                    ConfigureAwait(false);


                securityGroupList.AddRange(resp.SecurityGroupSet);

                if (resp.TotalCount > 100)
                {
                    for (ulong i = 100; i < resp.TotalCount.Value; i = i + 100)
                    {
                        strParams = "{\"Limit\":100,\"Offset\":\"" + i + "\"}";
                        req = DescribeSecurityGroupsRequest.FromJsonString<DescribeSecurityGroupsRequest>(strParams);
                        var respPartner = await client.DescribeSecurityGroups(req);

                        securityGroupList.AddRange(respPartner.SecurityGroupSet);
                    }
                }
                return securityGroupList;
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetSecRuleList " + ex.ToString());
                return securityGroupList;
            }
        }

        public async Task<bool> AddSecRule(TxSecDomainInfo model)
        {
            try
            {
                var egressList = new List<SecurityGroupPolicy>();
                egressList.Add(
                    new SecurityGroupPolicy()
                    {
                        PolicyIndex = 0,
                        Action = model.Action,
                        Protocol = model.Protocol,
                        Port = model.PortList,
                        CidrBlock = model.DomainNewIp,
                        PolicyDescription = model.Remarks + " " + model.DomainName + ":" + model.PortList + "_" + DateTime.Now.ToString()
                    });

                SecurityGroupPolicySet policySet = new SecurityGroupPolicySet()
                {
                    Version = "0",
                    Egress = egressList.ToArray()
                };


                string groupId = model.TxSecurityGroupId;

                ClientProfile clientProfile = new ClientProfile();
                HttpProfile httpProfile = new HttpProfile();
                httpProfile.Endpoint = ("vpc.tencentcloudapi.com");
                clientProfile.HttpProfile = httpProfile;
                VpcClient client = new VpcClient(cred, model.Region, clientProfile);

                var reqRule = DescribeSecurityGroupPoliciesRequest.FromJsonString<DescribeSecurityGroupPoliciesRequest>("{\"SecurityGroupId\":\"" + groupId + "\"}");
                DescribeSecurityGroupPoliciesResponse respRule = await client.DescribeSecurityGroupPolicies(reqRule).
                    ConfigureAwait(false);

                string apiVersion = respRule.SecurityGroupPolicySet.Version;

                policySet.Version = apiVersion;

                if (respRule.SecurityGroupPolicySet.Egress.Length == 100)
                {
                    Console.WriteLine(model.DomainName + " EgressMaxCount:" + respRule.SecurityGroupPolicySet.Egress.Length);
                    return false;
                }

                CreateSecurityGroupPoliciesRequest req = new CreateSecurityGroupPoliciesRequest();
                string strParams = "{\"SecurityGroupId\":\"" + groupId + "\",\"SecurityGroupPolicySet\":" + JsonConvert.SerializeObject(policySet) + "}";

                req = CreateSecurityGroupPoliciesRequest.FromJsonString<CreateSecurityGroupPoliciesRequest>(strParams);
                CreateSecurityGroupPoliciesResponse resp = await client.CreateSecurityGroupPolicies(req);
                Console.WriteLine(model.TxSecurityGroupId + " " + AbstractModel.ToJsonString(resp));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
            return true;
        }
    }
}
