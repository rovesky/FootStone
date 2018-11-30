using FootStone.Core.GrainInterfaces;
using FootStone.GrainInterfaces;
using Ice;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FootStone.Core.FrontIce
{
    public class AccountI : AccountDisp_
    {   

        public AccountI()
        {
            
        }

        public async override Task<string> CreatePlayerAsync(string name, int serverId, Current current = null)
        {

            try
            {
                var id = Guid.NewGuid();
                var player = Global.Instance.OrleansClient.GetGrain<IPlayerGrain>(id);
                await player.CreatePlayer(name, serverId);
                return id.ToString();      
            }
            catch (System.Exception ex)
            {
                Console.Error.WriteLine(ex);
                throw ex;
            }
        }

        public async override Task LoginRequestAsync(LoginInfo info, Current current = null)
        {

            try
            {
                var account = Global.Instance.OrleansClient.GetGrain<IAccountGrain>(info.account);
                await account.LoginRequest(info);
            }
            catch (System.Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                throw ex;
            }
        }

        public async override Task RegisterRequestAsync(RegisterInfo info, Current current = null)
        {
            try
            {
                var account = Global.Instance.OrleansClient.GetGrain<IAccountGrain>(info.account);
          //      Console.WriteLine("------"+serverName+ ":Register Account="+info.account);
                await account.RegisterRequest(info);               
            }
            catch (System.Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                throw ex;
            }
        }
    }
}
