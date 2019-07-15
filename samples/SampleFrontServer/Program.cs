using FootStone.FrontIce;
using FootStone.FrontNetty;
using FootStone.GrainInterfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using SampleFrontIce;
using System;
using System.Threading.Tasks;

namespace FootStone.Core.FrontServer
{
    class Program
    {
        static string mysqlConnectCluster = "server=192.168.0.128;user id=root;password=654321#;database=footstone;MaximumPoolsize=50";
        //static string mysqlConnectCluster = "server=192.168.3.28;user id=root;password=198292;database=footstone_cluster;MaximumPoolsize=50";

        private static Logger logger = LogManager.GetCurrentClassLogger();
      

        static void Main(string[] args)
        {
           
            try
            {
                Startup(args).Wait();
            }
            catch(Exception ex)
            {
                logger.Error(ex);
                Console.ReadLine();
            }
         
        }

        private static Task Startup(string[] args)
        {
            Console.Title = nameof(FrontServer);

            return new HostBuilder()
               //使用Orleans client
               .UseOrleansClient(clientBuilder =>
               {
                   clientBuilder
                        .UseAdoNetClustering(options =>
                        {
                            options.ConnectionString = mysqlConnectCluster;
                            options.Invariant = "MySql.Data.MySqlClient";
                        })
                        .Configure<ClusterOptions>(options =>
                        {
                            options.ClusterId = "lsj";
                            options.ServiceId = "FootStone";
                        })
                        .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(IAccountGrain).Assembly).WithReferences())
                        ;
               })
               //使用Ice
               .UseFrontIce()
               //使用Netty
               .UseFrontNetty(options =>
               {
                   options.FrontPort = 8007;
                   options.GamePort = 8017;
               })
               .ConfigureServices(services => services
   
                   .Configure<ConsoleLifetimeOptions>(_ =>
                   {
                       _.SuppressStatusMessages = true;
                   }))

               .ConfigureLogging(builder => builder.AddProvider(new NLogLoggerProvider()))

               .RunConsoleAsync();
        }
    }
}
