using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using NLog.Web;

namespace StripeApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                            .UseStartup<Startup>()
                            // .ConfigureAppConfiguration((hostingContext, config) =>
                            // {
                            //     var env = hostingContext.HostingEnvironment;
                            //     var logConfigPath = "nlog.config";
                            //     if (env.IsStaging() || env.IsProduction())
                            //     {
                            //         logConfigPath = "nlog.config"; // For production purposes, change here
                            //     }
                            //     NLogBuilder.ConfigureNLog(logConfigPath);
                            // })
                            .UseNLog(); 
                });
    }
}
