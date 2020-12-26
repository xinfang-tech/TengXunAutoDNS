using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TengXunAuto.DNS.Models;

namespace TengXunAuto.DNS
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {

                    AppEnv.Site = hostContext.Configuration.GetSection("SiteConfig").Get<SiteConfig>();
                    AppEnv.DevelopSite = hostContext.Configuration.GetSection("DevelopSite").Get<SiteConfig>();
                    if (hostContext.HostingEnvironment.IsDevelopment())
                    {
                        AppEnv.Site = AppEnv.DevelopSite;
                    }

                    services.AddHostedService<Worker>();
                });
    }
}
