using FootStone.Core.FrontIce;
using FootStone.FrontIce;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using System;
using System.Net;
using System.Threading.Tasks;

namespace FootStone.Core.GameServer
{



    class Program
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        static string IP_START = "192.168.0";
        static string mysqlConnectCluster = "server=192.168.0.128;user id=root;password=654321#;database=footstone;MaximumPoolsize=50";
        static string mysqlConnectStorage = "server=192.168.0.128;user id=root;password=654321#;database=footstonestorage;MaximumPoolsize=50";

        // static string mysqlConnectCluster = "server=192.168.0.128;user id=root;password=198292;database=footstone_cluster;MaximumPoolsize=50";
        // static string mysqlConnectStorage = "server=192.168.1.128;user id=root;password=654321#;database=footstonestorage;MaximumPoolsize=50";

        public static string GetLocalIP()
        {

            string name = Dns.GetHostName();
            IPAddress[] ipadrlist = Dns.GetHostAddresses(name);
            foreach (IPAddress ipa in ipadrlist)
            {
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
                        .Configure<EndpointOptions>(options =>
                        {
                            // Port to use for Silo-to-Silo
                            options.SiloPort = 11111;
                            // Port to use for the gateway
                            options.GatewayPort = 30000;
                            // IP Address to advertise in the cluster
                            options.AdvertisedIPAddress = IPAddress.Parse(GetLocalIP());
                        })
                        //  .Configure<EndpointOptions>(options => options.AdvertisedIPAddress = IPAddress.Any)
                        //.ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(RoomGrain).Assembly).WithReferences())
                        .ConfigureLogging(logging =>
                        {
                            // logging.AddConsole();
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
                        // .AddGrainService<IceService>()
                        .AddGrainService<NettyService>()
                        .Configure<NettyOptions>(options =>
                        {
                            options.Port = 8007;
                        })
                        // .ConfigureServices(s =>
                        // {
                        //    // Register Client of GrainService
                        //     s.AddSingleton<IIceService, IceService>();
                        ////     s.AddSingleton<INettyServiceClient, NettyServiceClient>();
                        // })
                        .AddMemoryGrainStorage("PubSubStore")
                        .AddSimpleMessageStreamProvider("Zone", cfg =>
                        {
                            cfg.FireAndForgetDelivery = true;
                        })
                        .EnableDirectClient();
                    })

                    //添加Ice支持
                    .AddFrontIce(options =>
                    {
                        options.ConfigFile = "config";

                        options.FacetTypes.Add(typeof(AccountI));
                        options.FacetTypes.Add(typeof(WorldI));
                        options.FacetTypes.Add(typeof(PlayerI));
                        options.FacetTypes.Add(typeof(RoleMasterI));
                        options.FacetTypes.Add(typeof(ZoneI));

                        //var logger = LogManager.GetLogger("Ice");
                        //logger.Info("ICE ERROR!!!!");
                        //options.Logger = new NLoggerI(logger);

                    })
                    .Build();

                logger.Info("FSHost builded!");

                //throw new Exception("test");
                //logger.Error(new Exception("test").ToString());
                Global.FSHost = footStone;

                var iceService = footStone.Services.GetService<IceService>();

                RunAsync(footStone).Wait();
                do
                {
                    string exit = Console.ReadLine();
                    if (exit.Equals("exit"))
                    {
                        StopAsync(footStone).Wait();
                        break;
                    }
                } while (true);
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
            //   await client.Connect();

            //ITestGrain test = Global.OrleansClient.GetGrain<ITestGrain>(0);
            //await test.Test();
       //     ISampleGameGrain game = Global.OrleansClient.GetGrain<ISampleGameGrain>(1);
       //     await game.PlayerLeave(Guid.NewGuid());

      //      var info = await game.GetGameState();

      //      await game.SampleBattleBegin();

        //    IWorldGrain world = Global.OrleansClient.GetGrain<IWorldGrain>("1");
        //    await world.Init(System.Environment.CurrentDirectory);

            //Console.WriteLine("Map file name is '{0}'.", mapFileName);
            //Console.WriteLine("Setting up Adventure, please wait ...");
            //Adventure adventure = new Adventure(client);
            //adventure.Configure(mapFileName).Wait();
            logger.Info("FSHost start completed!");
        }

        static async Task StopAsync(IFSHost fs)
        {
            await fs.StopAsync();
        }

    }
}
