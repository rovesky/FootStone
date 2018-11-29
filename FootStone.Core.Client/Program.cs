using FootStone.GrainInterfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootStone.client
{
    class Program
    {
        static void Test()
        {
            try
            {
                string IP = "192.168.3.14";
                int port = 4061;
                Ice.InitializationData initData = new Ice.InitializationData();

                initData.properties = Ice.Util.createProperties();
                // initData.properties.setProperty("Ice.ACM.Client", "0");
                // initData.properties.setProperty("Ice.RetryIntervals", "-1");
                //initData.properties.setProperty("Ice.FactoryAssemblies", "client");
                
                initData.properties.setProperty("Ice.Trace.Network", "0");
               // initData.properties.setProperty("Player.Proxy", "player:tcp -h 192.168.206.1 -p 12000");
                initData.properties.setProperty("Ice.Default.Locator", "FootStone/Locator:default -h " + IP + " -p " + port);

                //
                // using statement - communicator is automatically destroyed
                // at the end of this statement
                //
                using (var communicator = Ice.Util.initialize(initData))
                {
                   // var player = PlayerPrxHelper.checkedCast(communicator.propertyToProxy("Player.Proxy"));

                    PlayerPrx playerPrx = null;
                    try
                    {
                        playerPrx = PlayerPrxHelper.checkedCast(communicator.stringToProxy("player"));
                    }
                    catch (Ice.NotRegisteredException)
                    {
                        var query =
                            IceGrid.QueryPrxHelper.checkedCast(communicator.stringToProxy("FootStone/Query"));
                        playerPrx = PlayerPrxHelper.checkedCast(query.findObjectByType("::FootStone::GrainInterfaces::Player"));
                    }
                    if (playerPrx == null)
                    {
                        Console.WriteLine("couldn't find a `::Player' object");
                        return;
                    }


                    for (int i = 0; i < 1; ++i)
                    {
                        doMethod(playerPrx);
                       
                        //player.begin_getPlayerInfo().whenCompleted(
                        //            (playerInfo) =>
                        //            {
                        //                Console.Error.WriteLine(playerInfo.name);
                        //                player.begin_setPlayerName(playerInfo.id,playerInfo.name+"_y").whenCompleted(
                        //                   () => {
                        //                      player.begin_getPlayerInfo(playerInfo.id).whenCompleted(
                        //                          (playerInfo1) =>
                        //                          {
                        //                              Console.Error.WriteLine(playerInfo1.name);
                        //                          },
                        //                          (Ice.Exception ex) =>
                        //                                   {
                        //                                       Console.Error.WriteLine(ex.Message);
                        //                                   });
                        //                         },
                        //                   (Ice.Exception ex) =>
                        //                   {
                        //                       Console.Error.WriteLine(ex.Message);
                        //                   });
                        //            },
                        //            (Ice.Exception ex) =>
                        //            {
                        //                Console.Error.WriteLine(ex.Message);
                        //            });
                    }
                    communicator.waitForShutdown();

                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);

            }
        }

        static async void doMethod(PlayerPrx player)
        {
            try
            {
                var playerInfo = await player.getPlayerInfoAsync(Guid.NewGuid().ToString());
                Console.Error.WriteLine(playerInfo.name);
                await player.setPlayerNameAsync(playerInfo.id, playerInfo.name + "_y");

                playerInfo = await player.getPlayerInfoAsync(playerInfo.id);
                Console.Error.WriteLine(playerInfo.name);
            }
            catch(Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
            }
        }
        static void Main(string[] args)
        {
            Test();

            var line = Console.In.ReadLine();

        }

        private static int Test1(string[] args)
        {
            int status = 0;

            try
            {
                //
                // using statement - communicator is automatically destroyed
                // at the end of this statement
                //
                using (var communicator = Ice.Util.initialize(ref args, "config.client"))
                {
                    //
                    // Destroy the communicator on Ctrl+C or Ctrl+Break
                    //
                    Console.CancelKeyPress += (sender, eventArgs) => communicator.destroy();

                    if (args.Length > 0)
                    {
                        Console.Error.WriteLine("too many arguments");
                        status = 1;
                    }
                    else
                    {
                        status = run(communicator);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                status = 1;
            }
            return status;
        }

        private static int run(Ice.Communicator communicator)
        {
            var player = PlayerPrxHelper.checkedCast(communicator.propertyToProxy("Player.Proxy"));
            if (player == null)
            {
                Console.Error.WriteLine("invalid proxy");
                return 1;
            }

            menu();

            string line = null;
            do
            {
                try
                {
                    Console.Out.Write("==> ");
                    Console.Out.Flush();
                    line = Console.In.ReadLine();
                    if (line == null)
                    {
                        break;
                    }
                    if (line.Equals("i"))
                    {
                      //  player.sayHello(0);
                    }
                    else if (line.Equals("d"))
                    {
                        helloAsync(player);
                    }
                    else if (line.Equals("s"))
                    {
                      //  player.shutdown();
                    }
                    else if (line.Equals("x"))
                    {
                        // Nothing to do
                    }
                    else if (line.Equals("?"))
                    {
                        menu();
                    }
                    else
                    {
                        Console.Out.WriteLine("unknown command `" + line + "'");
                        menu();
                    }
                }
                catch (Ice.Exception ex)
                {
                    Console.Error.WriteLine(ex);
                }
            }
            while (!line.Equals("x"));

            return 0;
        }


        private static async void helloAsync(PlayerPrx player)
        {
            try
            {

               var playerInfo = await player.getPlayerInfoAsync(Guid.NewGuid().ToString());
                Console.WriteLine(JsonConvert.SerializeObject(playerInfo));
            }
            catch (PlayerNotExsit)
            {
                Console.Error.WriteLine("PlayerNotExsit");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("sayHello AMI call failed:");
                Console.Error.WriteLine(ex);
            }
        }

        private static void menu()
        {
            Console.Out.WriteLine(
                "usage:\n" +
                "i: send immediate greeting\n" +
                "d: send delayed greeting\n" +
                "s: shutdown server\n" +
                "x: exit\n" +
                "?: help\n");
        }
    }
}
