using FootStone.Core.GrainInterfaces;
using FootStone.FrontIce;
using FootStone.FrontNetty;
using FootStone.GrainInterfaces;
using Microsoft.Extensions.DependencyInjection;
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

namespace FootStone.Core.GameServer
{

    class Program
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        static string IP_START = "192.168.0";
        static string mysqlConnectCluster = "server=192.168.0.128;user id=root;password=654321#;database=footstone;MaximumPoolsize=50";
        static string mysqlConnectStorage = "server=192.168.0.120;user id=root;password=456789;database=footstone_storage;MaximumPoolsize=100";

        // static string mysqlConnectCluster = "server=192.168.0.128;user id=root;password=198292;database=footstone_cluster;MaximumPoolsize=50";
        // static string mysqlConnectStorage = "server=192.168.1.128;user id=root;password=654321#;database=footstonestorage;MaximumPoolsize=50";

        static readonly ManualResetEvent _siloStopped = new ManualResetEvent(false);      
        static bool siloStopping = false;
        static readonly object syncLock = new object();

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

        static int Main(string[] args)
        {
            try
            {
              
                Console.CancelKeyPress += (s, a) =>
                {
                    a.Cancel = true;
                    /// Don't allow the following code to repeat if the user presses Ctrl+C repeatedly.
                    lock (syncLock)
                    {
                        if (!siloStopping)
                        {
                            siloStopping = true;
                            Task.Run(StopAsync).Ignore();
                        }
                    }
                };


                var footStone = new FSHostBuilder()

                    //配置orlean的Silo
                    .ConfigureSilo(silo =>
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
                        .ConfigureEndpoints(IPAddress.Parse(GetLocalIP()),11111, 30000)                      
                        //.Configure<StatisticsOptions>(options =>
                        //{
                        //    options.LogWriteInterval = TimeSpan.FromSeconds(10);
                        //    options.CollectionLevel = Orleans.Runtime.Configuration.StatisticsLevel.Critical;
                        //})
                        .AddStartupTask(async (IServiceProvider services, CancellationToken cancellation) =>
                        {
                            var grainFactory = services.GetRequiredService<IGrainFactory>();             
                            var grain = grainFactory.GetGrain<IWorldGrain>("1");
                            await grain.Init(Environment.CurrentDirectory);

                        })
                        //  .Configure<EndpointOptions>(options => options.AdvertisedIPAddress = IPAddress.Any)
                        //  .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(IAccountGrain).Assembly).WithReferences())
                        .ConfigureLogging(logging =>
                        {               
                            //logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Warning);
                            logging.AddProvider(new NLogLoggerProvider());
                        })
                        .AddMemoryGrainStorage("memory1")
                        .AddAdoNetGrainStorage("ado1", options =>
                         {

                             options.UseJsonFormat = true;
                             options.ConnectionString = mysqlConnectStorage;
                             options.Invariant = "MySql.Data.MySqlClient";
                         })     
                        .AddGrainService<NettyService>()
                        .Configure<NettyOptions>(options =>
                        {
                            options.Port = 8007;
                        })           
                        //.AddMemoryGrainStorage("PubSubStore")
                        //.AddSimpleMessageStreamProvider("Zone", cfg =>
                        //{
                        //    cfg.FireAndForgetDelivery = true;
                        //})
                        .EnableDirectClient();
                    })
                    //添加Ice支持
                    .AddFrontIce()
                    .Build();

                logger.Info("FSHost builded!");

                Global.FSHost = footStone;

                RunAsync(footStone).Wait();

                _siloStopped.WaitOne();
             
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Console.ReadLine();
            }
            return 0;
        }

        static async Task RunAsync(IFSHost fs)
        {
            await fs.StartAsync();      
            logger.Info("FSHost start completed!");
        }

        static async Task StopAsync()
        {
            await Global.FSHost.StopAsync();
            _siloStopped.Set();
        }

    }
}
