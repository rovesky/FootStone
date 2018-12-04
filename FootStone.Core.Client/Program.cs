using FootStone.GrainInterfaces;
using Network;
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
        //static void Test()
        //{
        //    try
        //    {
        //        string IP = "192.168.3.14";
        //        int port = 4061;
        //        Ice.InitializationData initData = new Ice.InitializationData();

        //        initData.properties = Ice.Util.createProperties();
        //        // initData.properties.setProperty("Ice.ACM.Client", "0");
        //        // initData.properties.setProperty("Ice.RetryIntervals", "-1");
        //        //initData.properties.setProperty("Ice.FactoryAssemblies", "client");

        //        initData.properties.setProperty("Ice.Trace.Network", "0");
        //       // initData.properties.setProperty("Player.Proxy", "player:tcp -h 192.168.206.1 -p 12000");
        //        initData.properties.setProperty("Ice.Default.Locator", "FootStone/Locator:default -h " + IP + " -p " + port);

        //        //
        //        // using statement - communicator is automatically destroyed
        //        // at the end of this statement
        //        //
        //        using (var communicator = Ice.Util.initialize(initData))
        //        {
        //           // var player = PlayerPrxHelper.checkedCast(communicator.propertyToProxy("Player.Proxy"));

        //            PlayerPrx playerPrx = null;
        //            try
        //            {
        //                playerPrx = PlayerPrxHelper.checkedCast(communicator.stringToProxy("player"));
        //            }
        //            catch (Ice.NotRegisteredException)
        //            {
        //                var query =
        //                    IceGrid.QueryPrxHelper.checkedCast(communicator.stringToProxy("FootStone/Query"));
        //                playerPrx = PlayerPrxHelper.checkedCast(query.findObjectByType("::FootStone::GrainInterfaces::Player"));
        //            }
        //            if (playerPrx == null)
        //            {
        //                Console.WriteLine("couldn't find a `::Player' object");
        //                return;
        //            }


        //            for (int i = 0; i < 1; ++i)
        //            {
        //                doMethod(playerPrx);

        //                //player.begin_getPlayerInfo().whenCompleted(
        //                //            (playerInfo) =>
        //                //            {
        //                //                Console.Error.WriteLine(playerInfo.name);
        //                //                player.begin_setPlayerName(playerInfo.id,playerInfo.name+"_y").whenCompleted(
        //                //                   () => {
        //                //                      player.begin_getPlayerInfo(playerInfo.id).whenCompleted(
        //                //                          (playerInfo1) =>
        //                //                          {
        //                //                              Console.Error.WriteLine(playerInfo1.name);
        //                //                          },
        //                //                          (Ice.Exception ex) =>
        //                //                                   {
        //                //                                       Console.Error.WriteLine(ex.Message);
        //                //                                   });
        //                //                         },
        //                //                   (Ice.Exception ex) =>
        //                //                   {
        //                //                       Console.Error.WriteLine(ex.Message);
        //                //                   });
        //                //            },
        //                //            (Ice.Exception ex) =>
        //                //            {
        //                //                Console.Error.WriteLine(ex.Message);
        //                //            });
        //            }
        //            communicator.waitForShutdown();

        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.Error.WriteLine(ex);

        //    }
        //}

        //static async void doMethod(PlayerPrx player)
        //{
        //    try
        //    {
        //        var playerInfo = await player.getPlayerInfoAsync(Guid.NewGuid().ToString());
        //        Console.Error.WriteLine(playerInfo.name);
        //        await player.setPlayerNameAsync(playerInfo.id, playerInfo.name + "_y");

        //        playerInfo = await player.getPlayerInfoAsync(playerInfo.id);
        //        Console.Error.WriteLine(playerInfo.name);
        //    }
        //    catch(Exception ex)
        //    {
        //        Console.Error.WriteLine(ex.Message);
        //    }
        //}
        static void Main(string[] args)
        {
            //  Test();
            try
            {

                NetworkIce.Instance.Init("192.168.3.14", 4061);
                run1(NetworkIce.Instance.Communicator).Wait();
            }
            catch(Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
            }
        }

        private static async Task run1(Ice.Communicator communicator)
        {
            var account = "a1";
            var password = "111111";
            var playerName = "player1";
         
            var sessionPrx = await NetworkIce.Instance.CreateSession("");
            Console.Error.WriteLine("NetworkIce.Instance.CreateSession ok!");

            var accountPrx = AccountPrxHelper.uncheckedCast(sessionPrx, "account");
            try
            {
                await accountPrx.RegisterRequestAsync(new RegisterInfo(account, password));
                Console.Error.WriteLine("RegisterRequest ok:" + account);
            }
            catch(Exception ex)
            {
                Console.Error.WriteLine("RegisterRequest fail:" + ex.Message);
            }
         

            await accountPrx.LoginRequestAsync(new LoginInfo(account, password));
            Console.Error.WriteLine("LoginRequest ok:" + account);

            List<ServerInfo> servers = await accountPrx.GetServerListRequestAsync();

            if(servers.Count == 0)
            {
                Console.Error.WriteLine("server list is empty!");
                return ;
            }

            var serverId = servers[0].id;

            List<PlayerShortInfo> players = await accountPrx.GetPlayerListRequestAsync(serverId);
            if (players.Count == 0)
            {
                var playerId = await accountPrx.CreatePlayerRequestAsync(playerName, serverId);
                players = await accountPrx.GetPlayerListRequestAsync(serverId);         
            }

            await accountPrx.SelectPlayerRequestAsync(players[0].playerId);

            var playerPrx = PlayerPrxHelper.uncheckedCast(sessionPrx, "player");
            var playerInfo = await playerPrx.GetPlayerInfoAsync();
            Console.Error.WriteLine("playerInfo:" + JsonConvert.SerializeObject(playerInfo));

        }

        private static async Task<int> run(Ice.Communicator communicator)
        {
            string name;
            do
            {
                Console.Out.Write("Please enter your name ==> ");
                Console.Out.Flush();

                name = Console.In.ReadLine();
                if (name == null)
                {
                    return 1;
                }
                name = name.Trim();
            }
            while (name.Length == 0);
            var sessionPrx = await NetworkIce.Instance.CreateSession(name);



            menu();

            bool destroy = true;
            bool shutdown = false;
            while (true)
            {
                try
                {
                    Console.Out.Write("==> ");
                    Console.Out.Flush();
                    string line = Console.In.ReadLine();
                    if (line == null)
                    {
                        break;
                    }

                    if (line.Length > 0 && Char.IsDigit(line[0]))
                    {
                        //int index = Int32.Parse(line);
                        //if (index < hellos.Count)
                        //{
                        //    var hello = hellos[index];
                        //    hello.sayHello();
                        //}
                        //else
                        //{
                        //    Console.Out.WriteLine("Index is too high. " + hellos.Count +
                        //                          " hello objects exist so far.\n" +
                        //                          "Use `c' to create a new hello object.");
                        //}
                    }
                    else if (line.Equals("register"))
                    {
                        var accountPrx = AccountPrxHelper.uncheckedCast(sessionPrx, "account");
                        await accountPrx.RegisterRequestAsync(new RegisterInfo(name, "111111"));
                        //hellos.Add(session.createHello());
                        Console.Out.WriteLine("RegisterRequestAsync ok: " + name);
                    }
                    else if (line.Equals("player"))
                    {
                        var playerPrx = PlayerPrxHelper.uncheckedCast(sessionPrx, "player");
                        await playerPrx.GetPlayerInfoAsync();
                        //hellos.Add(session.createHello());
                        Console.Out.WriteLine("GetPlayerInfoAsync ok: " + name);
                    }
                    else if (line.Equals("s"))
                    {
                        destroy = false;
                        shutdown = true;
                        break;
                    }
                    else if (line.Equals("x"))
                    {
                        break;
                    }
                    else if (line.Equals("t"))
                    {
                        destroy = false;
                        break;
                    }
                    else if (line.Equals("?"))
                    {
                        menu();
                    }
                    else
                    {
                        Console.Out.WriteLine("Unknown command `" + line + "'.");
                        menu();
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.Message);
                }
            }

            if (destroy)
            {
                sessionPrx.Destroy();
            }
            if (shutdown)
            {
                NetworkIce.Instance.SessionFactoryPrx.Shutdown();
            }
            return 0;
        }

        private static void menu()
        {
            Console.Out.WriteLine(
                "usage:\n" +
                "register:     register account\n" +
                "player:     getPlayerInfo\n" +              
                "s:     shutdown the server and exit\n" +
                "x:     exit\n" +
                "t:     exit without destroying the session\n" +
                "?:     help\n");
        }

        //private static int Test1(string[] args)
        //{
        //    int status = 0;

        //    try
        //    {
        //        //
        //        // using statement - communicator is automatically destroyed
        //        // at the end of this statement
        //        //
        //        using (var communicator = Ice.Util.initialize(ref args, "config.client"))
        //        {
        //            //
        //            // Destroy the communicator on Ctrl+C or Ctrl+Break
        //            //
        //            Console.CancelKeyPress += (sender, eventArgs) => communicator.destroy();

        //            if (args.Length > 0)
        //            {
        //                Console.Error.WriteLine("too many arguments");
        //                status = 1;
        //            }
        //            else
        //            {
        //                status = run(communicator);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.Error.WriteLine(ex);
        //        status = 1;
        //    }
        //    return status;
        //}

        //private static int run(Ice.Communicator communicator)
        //{
        //    var player = PlayerPrxHelper.checkedCast(communicator.propertyToProxy("Player.Proxy"));
        //    if (player == null)
        //    {
        //        Console.Error.WriteLine("invalid proxy");
        //        return 1;
        //    }

        //    menu();

        //    string line = null;
        //    do
        //    {
        //        try
        //        {
        //            Console.Out.Write("==> ");
        //            Console.Out.Flush();
        //            line = Console.In.ReadLine();
        //            if (line == null)
        //            {
        //                break;
        //            }
        //            if (line.Equals("i"))
        //            {
        //              //  player.sayHello(0);
        //            }
        //            else if (line.Equals("d"))
        //            {
        //                helloAsync(player);
        //            }
        //            else if (line.Equals("s"))
        //            {
        //              //  player.shutdown();
        //            }
        //            else if (line.Equals("x"))
        //            {
        //                // Nothing to do
        //            }
        //            else if (line.Equals("?"))
        //            {
        //                menu();
        //            }
        //            else
        //            {
        //                Console.Out.WriteLine("unknown command `" + line + "'");
        //                menu();
        //            }
        //        }
        //        catch (Ice.Exception ex)
        //        {
        //            Console.Error.WriteLine(ex);
        //        }
        //    }
        //    while (!line.Equals("x"));

        //    return 0;
        //}


        //private static async void helloAsync(PlayerPrx player)
        //{
        //    try
        //    {

        //       var playerInfo = await player.getPlayerInfoAsync(Guid.NewGuid().ToString());
        //        Console.WriteLine(JsonConvert.SerializeObject(playerInfo));
        //    }
        //    catch (PlayerNotExsit)
        //    {
        //        Console.Error.WriteLine("PlayerNotExsit");
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.Error.WriteLine("sayHello AMI call failed:");
        //        Console.Error.WriteLine(ex);
        //    }
        //}

        //private static void menu()
        //{
        //    Console.Out.WriteLine(
        //        "usage:\n" +
        //        "i: send immediate greeting\n" +
        //        "d: send delayed greeting\n" +
        //        "s: shutdown server\n" +
        //        "x: exit\n" +
        //        "?: help\n");
        //}
    }
}
