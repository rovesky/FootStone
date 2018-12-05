using FootStone.Core.FrontIce;
using FootStone.Core.GrainInterfaces;
using FootStone.GrainInterfaces;
using FootStone.Grains;
using Microsoft.Extensions.DependencyInjection;
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

namespace FootStone.Core.GameServer
{
    class Program
    {

        static string IP_START = "192.168.3";
        static string mysqlConnectCluster = "server=192.168.3.14;user id=root;password=654321#;database=footstone;MaximumPoolsize=50";
        static string mysqlConnectStorage = "server=192.168.3.14;user id=root;password=654321#;database=footstonestorage;MaximumPoolsize=50";

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
                Global.Instance.MainArgs = args;

                var silo = new SiloHostBuilder()
                    //    .UseLocalhostClustering()
                    //    .UseDevelopmentClustering(primarySiloEndpoint)
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
                        logging.AddConsole();
                        logging.SetMinimumLevel(LogLevel.Warning);
                    })
                    .AddMemoryGrainStorage("memory1")
                    .AddAdoNetGrainStorage("ado1", options =>
                     {

                         options.UseJsonFormat = true;
                         options.ConnectionString = mysqlConnectStorage;           
                         options.Invariant = "MySql.Data.MySqlClient";
                     })
                    .AddGrainService<IceService>()
                    .ConfigureServices(s =>
                    {
                        // Register Client of GrainService
                        s.AddSingleton<IIceServiceClient, IceServiceClient>();
                    })
                    //.ConfigureServices(s =>
                    //{
                    //    // Register Client of GrainService
                    //    s.AddSingleton<IIceService, IceService>();
                    //})
                    .EnableDirectClient()
                    .Build();


                //var client = new ClientBuilder()
                //        //.UseLocalhostClustering()
                //        // .UseStaticClustering(gateways)
                //        .UseAdoNetClustering(options =>
                //        {
                //            options.ConnectionString = mysqlConnectCluster;
                //            options.Invariant = "MySql.Data.MySqlClient";
                //        })
                //        .Configure<ClusterOptions>(options =>
                //        {
                //            options.ClusterId = "lsj";
                //            options.ServiceId = "FootStone";
                //        })
                //        //    .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(IRoomGrain).Assembly).WithReferences())
                //        .ConfigureLogging(logging =>
                //        {
                //            logging.AddConsole();
                //            logging.SetMinimumLevel(LogLevel.Warning);
                //        })
                //        .Build();
                var client = silo.Services.GetRequiredService<IClusterClient>();
                Global.Instance.OrleansClient = client;               
                RunAsync(silo, client, mapFileName).Wait();

                Console.ReadLine();

                StopAsync(silo, client).Wait();

                
            }
            catch(Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
            }
            return 0;
        }

        static async Task RunAsync(ISiloHost silo, IClusterClient client, string mapFileName)
        {
            await silo.StartAsync();
         //   await client.Connect();
            

            //Console.WriteLine("Map file name is '{0}'.", mapFileName);
            //Console.WriteLine("Setting up Adventure, please wait ...");
            //Adventure adventure = new Adventure(client);
            //adventure.Configure(mapFileName).Wait();
            Console.WriteLine("FootStone Start completed.");
        }

        static async Task StopAsync(ISiloHost silo, IClusterClient client)
        {
          //  await client.Close();
            await silo.StopAsync();
        }
       
    }
}
