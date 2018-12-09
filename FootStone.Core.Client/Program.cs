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
                Test(2000).Wait();
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
            var zonePrx = IZonePrxHelper.uncheckedCast(sessionPrx, "zone");

            var playerInfo = await playerPrx.GetPlayerInfoAsync();
            await zonePrx.PlayerEnterAsync(playerInfo.zoneId);

            MasterProperty property;
            for (int i = 0; i < count; ++i)
            {              
                await playerPrx.SetPlayerNameAsync(playerName + "_" + i);
                await Task.Delay(3000);
                property = await roleMasterPrx.GetPropertyAsync();
                await Task.Delay(5000);
                //     Console.Out.WriteLine("property" + JsonConvert.SerializeObject(property));
                playerInfo = await playerPrx.GetPlayerInfoAsync();        
                await Task.Delay(10000);
            }
            Console.Out.WriteLine("playerInfo:" + JsonConvert.SerializeObject(playerInfo));

        }

      
    }
}
