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
      
   
        static void Main(string[] args)
        {
            //  Test();
            try
            {
                Test(1).Wait();
            }
            catch(Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
            }
        }

        private static async Task Test(int count)
        {
            NetworkIce.Instance.Init("192.168.3.14", 4061);
            for (int i = 0; i < count; ++i)
            {
                runTest(i, 1000);
                await Task.Delay(20);
            }
            Console.Out.WriteLine("all session created:" + count);
        }

        private static async Task runTest(int index,int count)
        {
            var sessionId = "session" + index;
            var account = "account"+index;
            var password = "111111";
            var playerName = "player"+index;
         
            var sessionPrx = await NetworkIce.Instance.CreateSession(sessionId);
          //  Console.Out.WriteLine("NetworkIce.Instance.CreateSession ok:"+ account);

            var accountPrx = AccountPrxHelper.uncheckedCast(sessionPrx, "account");
            try
            {
                await accountPrx.RegisterRequestAsync(new RegisterInfo(account, password));
                Console.Out.WriteLine("RegisterRequest ok:" + account);
            }
            catch(Exception ex)
            {
                Console.Out.WriteLine("RegisterRequest fail:" + ex.Message);
            }
         

            await accountPrx.LoginRequestAsync(new LoginInfo(account, password));
            Console.Out.WriteLine("LoginRequest ok:" + account);

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

            var playerPrx = IPlayerPrxHelper.uncheckedCast(sessionPrx, "player");
            var roleMasterPrx = IRoleMasterPrxHelper.uncheckedCast(sessionPrx, "roleMaster");

            var playerInfo = await playerPrx.GetPlayerInfoAsync();
            MasterProperty property;
            for (int i = 0; i < count; ++i)
            {              
                await playerPrx.SetPlayerNameAsync(playerName + "_" + i);
                property = await roleMasterPrx.GetPropertyAsync();
                Console.Out.WriteLine("property" + JsonConvert.SerializeObject(property));
                playerInfo = await playerPrx.GetPlayerInfoAsync();
        
                await Task.Delay(2000);
            }
            Console.Out.WriteLine("playerInfo:" + JsonConvert.SerializeObject(playerInfo));

        }

        private static async Task<int> run()
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
                        var playerPrx = IPlayerPrxHelper.uncheckedCast(sessionPrx, "player");
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
            //    NetworkIce.Instance.SessionFactoryPrx.Shutdown();
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
