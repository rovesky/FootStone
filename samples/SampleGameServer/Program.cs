using FootStone.Core;
using FootStone.Core.GrainInterfaces;
using FootStone.FrontIce;
using FootStone.FrontNetty;
using FootStone.Grains;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using SampleFrontIce;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SampleGameServer
{

    public class Program
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        static string IP_START = "192.168.0";
        static string mysqlConnectCluster = "server=192.168.0.128;user id=root;password=654321#;database=footstone;MaximumPoolsize=50";
        static string mysqlConnectStorage = "server=192.168.0.120;user id=root;password=456789;database=footstone_storage;MaximumPoolsize=100";

        // static string mysqlConnectCluster = "server=192.168.0.128;user id=root;password=198292;database=footstone_cluster;MaximumPoolsize=50";
        // static string mysqlConnectStorage = "server=192.168.1.128;user id=root;password=654321#;database=footstonestorage;MaximumPoolsize=50";

        public static string GetLocalIP()
        {

            string name = Dns.GetHostName();
            IPAddress[] ipadrlist = Dns.GetHostAddresses(name);

           
            foreach (IPAddress ipa in ipadrlist)
            {
                logger.Info($"{ipa.ToString()}");
                if (ipa.ToString().StartsWith(IP_START))
                {
                    return ipa.ToString();
                }

            }
            return IPAddress.Loopback.ToString();
        }

        public static int Main(string[] args)
        {
            Startup(args).Wait();      
            return 0;
        }

        private static Task Startup(string[] args)
        {
            try
            {
                return new HostBuilder()

                   //配置orlean的Silo
                   .UseOrleans(silo =>
                   {
                       //    .UseLocalhostClustering()
                       //    .UseDevelopmentClustering(primarySiloEndpoint)
                       silo.UseAdoNetClustering(options =>
                       {
                           options.ConnectionString = mysqlConnectCluster;
                           options.Invariant = "MySql.Data.MySqlClient";
                       })
                       .Configure<ClusterOptions>(options =>
                       {
                           options.ClusterId = "lsj";
                           options.ServiceId = "FootStone";
                       })
                       .ConfigureEndpoints(IPAddress.Parse(GetLocalIP()), 11111, 30000)
                       //.Configure<StatisticsOptions>(options =>
                       //{
                       //    options.LogWriteInterval = TimeSpan.FromSeconds(10);
                       //    options.CollectionLevel = Orleans.Runtime.Configuration.StatisticsLevel.Critical;
                       //})
                       .AddStartupTask(async (IServiceProvider services, CancellationToken cancellation) =>
                       {
                           var grainFactory = services.GetRequiredService<IGrainFactory>();
                           var grain = grainFactory.GetGrain<IWorldGrain>("1");
                           await grain.Init();

                       })
                       .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(WorldGrain).Assembly).WithReferences())
                       .AddMemoryGrainStorage("memory1")
                       .AddAdoNetGrainStorage("ado1", options =>
                       {

                           options.UseJsonFormat = true;
                           options.ConnectionString = mysqlConnectStorage;
                           options.Invariant = "MySql.Data.MySqlClient";
                       })
                       //.AddMemoryGrainStorage("PubSubStore")
                       //.AddSimpleMessageStreamProvider("Zone", cfg =>
                       //{
                       //    cfg.FireAndForgetDelivery = true;
                       //})     
                       ;
                   })
                   .ConfigureLogging(builder =>
                   {
                       builder.AddProvider(new NLogLoggerProvider());
                   })
                   .ConfigureServices(services =>
                   {
                       services.Configure<ConsoleLifetimeOptions>(options =>
                       {
                           options.SuppressStatusMessages = true;
                       });
                   })
                   //添加Netty Game支持
                   .UseGameNetty(options =>
                   {
                       options.Port = 8017;
                       options.Recv = ZoneNetttyData.Instance;
                   })
                   ////添加ICE Front支持
                   //.AddFrontIce()
                   ////添加Netty Front支持
                   //.AddFrontNetty(options =>
                   //{
                   //    options.FrontPort = 8007;
                   //    options.GamePort = 8017;
                   //})
                   .RunConsoleAsync();

            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Console.ReadLine();
                return Task.CompletedTask;
            }
         
        }     

    }
}
