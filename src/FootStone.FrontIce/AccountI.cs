using FootStone.Core.GrainInterfaces;
using FootStone.GrainInterfaces;
using Ice;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FootStone.Core.FrontIce
{

    /// <summary>
    /// Observer class that implements the observer interface. Need to pass a grain reference to an instance of this class to subscribe for updates.
    /// </summary>
    class AccountObserver : IAccountObserver
    {
        private SessionI sessionI;

        public AccountObserver(SessionI sessionI)
        {

            this.sessionI = sessionI;
        }

        public void AccountLogined(string id)
        {
            Console.Out.WriteLine("AccountLogined:" + id);
            if (!id.Equals(sessionI.Id))
            {
                Console.Out.WriteLine("sessionI.CollocProxy.Destroy");
                sessionI.SessionPushPrx.ice_getConnection().close(ConnectionClose.Forcefully);
            }           
        }
    }

    public class AccountI : AccountDisp_, IServantBase
    {
        private SessionI sessionI;
        private IAccountObserver accountObserver;
        private IAccountObserver accountObserverRef;

        public AccountI(SessionI sessionI)
        {
            this.sessionI = sessionI;           
        }

        private async Task AddObserver(IAccountGrain accountGrain,string account)
        {
           
            if (accountObserver == null)
            {
                Console.Out.WriteLine("add AccountPush:" + account);
                accountObserver = new AccountObserver(sessionI);
                accountObserverRef = await Global.OrleansClient.
                    CreateObjectReference<IAccountObserver>(accountObserver);            
                await accountGrain.SubscribeForAccount(accountObserverRef);
            }
        }

        public void Destroy()
        {
            if (accountObserver != null)
            {
                Console.Out.WriteLine("accountObserver Unsubscribe begin");
                var accountGrain = Global.OrleansClient.GetGrain<IAccountGrain>(sessionI.Account);
                accountGrain.UnsubscribeForAccount(accountObserverRef);
                accountObserver = null;
                accountObserverRef = null;
            }
        }
      

        public async override Task<string> CreatePlayerRequestAsync(string name, int serverId, Current current = null)
        {
            try
            {               
                var accountGrain = Global.OrleansClient.GetGrain<IAccountGrain>(sessionI.Account);
                var playerId = await accountGrain.CreatePlayer(name, serverId);           
                return playerId;
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
                sessionI.Account = info.account;
                var accountGrain = Global.OrleansClient.GetGrain<IAccountGrain>(info.account);

                await AddObserver(accountGrain, info.account);

                await accountGrain.Login(sessionI.Id, info);
                
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
                var accountGrain = Global.OrleansClient.GetGrain<IAccountGrain>(info.account);
          //      Console.WriteLine("------"+serverName+ ":Register Account="+info.account);
                await accountGrain.Register(info);               
            }
            catch (System.Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                throw ex;
            }
        }

        public async override Task<List<ServerInfo>> GetServerListRequestAsync(Current current = null)
        {
            try
            {
                var accountGrain = Global.OrleansClient.GetGrain<IAccountGrain>(sessionI.Account);
                return await accountGrain.GetServerList();
            }
            catch (System.Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                throw ex;
            }
        }

        public async override Task<List<PlayerShortInfo>> GetPlayerListRequestAsync(int serverId, Current current = null)
        {
            try
            {
                var accountGrain = Global.OrleansClient.GetGrain<IAccountGrain>(sessionI.Account);
                return await accountGrain.GetPlayerInfoShortList(serverId);
            }
            catch (System.Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                throw ex;
            }
        }

        public async override Task SelectPlayerRequestAsync(string PlayerId, Current current = null)
        {
            try
            {
                var accountGrain = Global.OrleansClient.GetGrain<IAccountGrain>(sessionI.Account);
                await accountGrain.SelectPlayer(PlayerId);

                sessionI.PlayerId = Guid.Parse(PlayerId);

                PlayerI playerI = (PlayerI)current.adapter.findFacet(current.id, "player");
                await playerI.AddObserver();                
            }
            catch (System.Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                throw ex;
            }
        }

        public override Task TestLoginRequestAsync(string account, string pwd, LoginData data, Current current = null)
        {

            var type = data.GetType();

            Console.WriteLine("TestLoginRequestAsync:" + type.Name);
            return Task.CompletedTask;
        }
    }
}
