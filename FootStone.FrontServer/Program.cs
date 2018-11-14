using FootStone.GrainInterfaces;
using Orleans;
using System;
using System.Net;
using Orleans.Runtime;
using Microsoft.Extensions.Logging;
using Orleans.Configuration;
using System.Threading;

namespace FootStone.FrontServer
{
    class Program
    {
        static void Main(string[] args)
        {
            InitIce(args);
            InitOrleans();
        }

        private static void InitOrleans()
        {
            var client = new ClientBuilder()
                .UseLocalhostClustering()
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = "dev";
                    options.ServiceId = "AdventureApp";
                })
                .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(IRoomGrain).Assembly).WithReferences())
                .Build();

            client.Connect().Wait();

            Global.Instance.OrleansClient = client;

            //            Console.WriteLine(@"
            //  ___      _                 _                  
            // / _ \    | |               | |                 
            /// /_\ \ __| |_   _____ _ __ | |_ _   _ _ __ ___ 
            //|  _  |/ _` \ \ / / _ \ '_ \| __| | | | '__/ _ \
            //| | | | (_| |\ V /  __/ | | | |_| |_| | | |  __/
            //\_| |_/\__,_| \_/ \___|_| |_|\__|\__,_|_|  \___|");

            //            Console.WriteLine();
            Console.WriteLine("orleans inited!");




            string name = Console.ReadLine();
            //  player.SetName(name).Wait();
            //  var room1 = client.GetGrain<IRoomGrain>(0);
            //  player.SetRoomGrain(room1).Wait();

            //  Console.WriteLine(player.Play("look").Result);

            //string result = "Start";

            //    while (result != "")
            //    {
            //        string command = Console.ReadLine();

            //        //      result = player.Play(command).Result;
            //        Console.WriteLine(result);
            //    }
            //}
            //finally
            //{
            //    //  player.Die().Wait();
            //    Console.WriteLine("Game over!");
            //}
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
