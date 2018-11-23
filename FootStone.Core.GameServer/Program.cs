//using FootStone.FrontServer;
using FootStone.FrontServer;
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
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace AdventureSetup
{
    class Program
    {
        static int Main(string [] args)
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string mapFileName = Path.Combine (path, "AdventureMap.json");

            switch (args.Length)
            {
                default:
                    Console.WriteLine("*** Invalid command line arguments.");
                    return -1;
                case 0:
                    break;
                case 1:
                    mapFileName = args[0];
                    break;
            }

            if (!File.Exists(mapFileName))
            {
                Console.WriteLine("*** File not found: {0}", mapFileName);
                return -2;
            }

            var silo = new SiloHostBuilder()
                .UseLocalhostClustering()
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = "dev";
                    options.ServiceId = "AdventureApp";
                })
                .Configure<EndpointOptions>(options => options.AdvertisedIPAddress = IPAddress.Loopback)
                .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(RoomGrain).Assembly).WithReferences())
                .ConfigureLogging(logging => logging.AddConsole())
                .AddMemoryGrainStorage("memory1")
                //.AddAdoNetGrainStorage("ado1",options=>
                //{
                //    options.ConnectionString = "";
                //    options.UseJsonFormat = true;
                //    options.Invariant = "MySql.Data.MySqlClient";
                //})
                .Build();

            var client = new ClientBuilder()
                .UseLocalhostClustering()
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = "dev";
                    options.ServiceId = "AdventureApp";
                })
                .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(IRoomGrain).Assembly).WithReferences())
                .ConfigureLogging(logging => logging.AddConsole())
                .Build();

            Global.Instance.OrleansClient = client;
            InitIce(args);
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
                    using (var communicator = Ice.Util.initialize(ref args, "config.server"))
                    {
                        if (args.Length > 0)
                        {
                            Console.Error.WriteLine("too many arguments");

                        }
                        else
                        {
                            //  var workQueue = new WorkQueue();

                            //
                            // Shutdown the communicator and destroy the workqueue on Ctrl+C or Ctrl+Break
                            // (shutdown always with Cancel = true)
                            //                



                            var adapter = communicator.createObjectAdapter("Player");
                            adapter.add(new PlayerI(), Ice.Util.stringToIdentity("player"));


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
