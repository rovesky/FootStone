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
                Ice.InitializationData initData = new Ice.InitializationData();

                initData.properties = Ice.Util.createProperties();
                // initData.properties.setProperty("Ice.ACM.Client", "0");
                // initData.properties.setProperty("Ice.RetryIntervals", "-1");
                initData.properties.setProperty("Ice.FactoryAssemblies", "client");
                initData.properties.setProperty("Ice.Trace.Network", "0");
                initData.properties.setProperty("Player.Proxy", "player:tcp -h localhost -p 12000");

                //
                // using statement - communicator is automatically destroyed
                // at the end of this statement
                //
                using (var communicator = Ice.Util.initialize(initData))
                {
                    var player = PlayerPrxHelper.checkedCast(communicator.propertyToProxy("Player.Proxy"));

                    player.begin_getPlayerInfo(Guid.NewGuid().ToString()).whenCompleted(
                                (playerInfo) => {
                                    Console.Error.WriteLine(playerInfo.Name);
                                },
                                (Ice.Exception ex) => {

                                });

                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);

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
