//using FootStone.FrontServer;
using FootStone.Core.GameServer;
using FootStone.GrainInterfaces;
using FootStone.Grains;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Runtime;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace AdventureSetup
{
    class Program
    {

        static string IP_START = "192.168.206";
        static string mysqlConnectStr = "server=192.168.3.14;user id=root;password=654321#;database=footstone";
        static string mysqlConnectStr1 = "server=192.168.3.14;user id=root;password=654321#;database=footstonestorage";

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
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string mapFileName = Path.Combine(path, "AdventureMap.json");

            //switch (args.Length)
            //{
            //    default:
            //        Console.WriteLine("*** Invalid command line arguments.");
            //        return -1;
            //    case 0:
            //        break;
            //    case 1:
            //        mapFileName = args[0];
            //        break;
            //}

            if (!File.Exists(mapFileName))
            {
                Console.WriteLine("*** File not found: {0}", mapFileName);
                return -2;
            }

            
           // var primarySiloEndpoint = new IPEndPoint(IPAddress.Parse(PRIMARY_SILO), 11111);

            var silo = new SiloHostBuilder()
            //    .UseLocalhostClustering()
            //    .UseDevelopmentClustering(primarySiloEndpoint)
                .UseAdoNetClustering(options =>
                {
                    options.ConnectionString = mysqlConnectStr;
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
                    //options.AdvertisedIPAddress = IPAddress.Parse(Dns.GetHostName());
                    //   options.AdvertisedIPAddress = IPAddress.;
                    // The socket used for silo-to-silo will bind to this endpoint
                    //options.GatewayListeningEndpoint = new IPEndPoint(IPAddress.Any, 40000);
                    // The socket used by the gateway will bind to this endpoint
                    // options.SiloListeningEndpoint = new IPEndPoint(IPAddress.Any, 50000);

                })
                //  .Configure<EndpointOptions>(options => options.AdvertisedIPAddress = IPAddress.Any)
                .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(RoomGrain).Assembly).WithReferences())
                .ConfigureLogging(logging => logging.AddConsole())
                .AddMemoryGrainStorage("memory1")
                .AddAdoNetGrainStorage("ado1", options =>
                 {
                  
                     options.UseJsonFormat = true;
                     options.ConnectionString = mysqlConnectStr1;
                     options.Invariant = "MySql.Data.MySqlClient";
                 })
                .Build();

            //var gateways = new IPEndPoint[SILOS.Length];
            //for (int i = 0; i < SILOS.Length; ++i)
            //{
            //    gateways[i] = new IPEndPoint(IPAddress.Parse(SILOS[i]), 30000);
            //};

            var client = new ClientBuilder()
                    //.UseLocalhostClustering()
                    // .UseStaticClustering(gateways)
                    .UseAdoNetClustering(options =>
                    {
                        options.ConnectionString = mysqlConnectStr;
                        options.Invariant = "MySql.Data.MySqlClient";
                    })
                    .Configure<ClusterOptions>(options =>
                    {
                        options.ClusterId = "lsj";
                        options.ServiceId = "FootStone";
                    })
                    .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(IRoomGrain).Assembly).WithReferences())
                    .ConfigureLogging(logging => logging.AddConsole())
                    .Build();

            Global.Instance.OrleansClient = client;
            InitGridIce(args);
            RunAsync(silo, client, mapFileName).Wait();

            Console.ReadLine();

            StopAsync(silo, client).Wait();

            return 0;
        }

        static async Task RunAsync(ISiloHost silo, IClusterClient client, string mapFileName)
        {
            await silo.StartAsync();
            await client.Connect();
            

            Console.WriteLine("Map file name is '{0}'.", mapFileName);
            Console.WriteLine("Setting up Adventure, please wait ...");
            Adventure adventure = new Adventure(client);
            adventure.Configure(mapFileName).Wait();
            Console.WriteLine("Adventure setup completed.");
        }

        static async Task StopAsync(ISiloHost silo, IClusterClient client)
        {
            await client.Close();
            await silo.StopAsync();
        }

        static void InitIce(string[] args)
        {
            Thread th = new Thread(new ThreadStart(() =>
            {

                try
                {
                    //
                    // using statement - communicator is automatically destroyed
                    // at the end of this statement
                    //
                    Ice.InitializationData initData = new Ice.InitializationData();

                    initData.properties = Ice.Util.createProperties();
                    initData.properties.setProperty("Ice.Warn.Connections", "1");
                    initData.properties.setProperty("Ice.Trace.Network", "1");
                    initData.properties.setProperty("SessionFactory.Endpoints", "tcp -h "+GetLocalIP()+" -p 12000");

                    using (var communicator = Ice.Util.initialize(initData))
                    {
                        if (args.Length > 0)
                        {
                            Console.Error.WriteLine("too many arguments");

                        }
                        else
                        {                  


                            //               var adapter = communicator.createObjectAdapter("Player");
                            var adapter = communicator.createObjectAdapter("SessionFactory");
                            adapter.add(new SessionFactoryI(), Ice.Util.stringToIdentity("SessionFactory"));
                            
                            adapter.activate();
                            Console.WriteLine("ice inited!");

                            communicator.waitForShutdown();

                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);

                }

            })); //创建线程                     
            th.Start(); //启动线程       
        }

        static void InitGridIce(string[] args)
        {
            Thread th = new Thread(new ThreadStart(() =>
            {

                try
                {               
                    using (var communicator = Ice.Util.initialize(ref args))
                    {
                        if (args.Length > 0)
                        {
                            Console.Error.WriteLine("too many arguments");

                        }
                        else
                        {           
                            var adapter = communicator.createObjectAdapter("Player");
                            var properties = communicator.getProperties();
                            var id = Ice.Util.stringToIdentity(properties.getProperty("Identity"));
                            adapter.add(new PlayerI(properties.getProperty("Ice.ProgramName")),id);

                            adapter.activate();
                            Console.WriteLine("ice inited!");

                            communicator.waitForShutdown();

                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);

                }

            })); //创建线程                     
            th.Start(); //启动线程       
        }
    }
}
