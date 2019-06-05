using FootStone.Core.GrainInterfaces;
using FootStone.FrontIce;
using FootStone.GrainInterfaces;
using Ice;
using NLog;
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

        private NLog.Logger logger = LogManager.GetCurrentClassLogger();

        public AccountObserver(SessionI sessionI)
        {

            this.sessionI = sessionI;
        }

        public void AccountLogined(string id)
        {
            logger.Debug("AccountLogined:" + id);
            if (!id.Equals(sessionI.Id))
            {
                logger.Debug("sessionI.CollocProxy.Destroy");
                sessionI.SessionPushPrx.ice_getConnection().close(ConnectionClose.Forcefully);
            }           
        }
    }



    public class AccountI : AccountDisp_, IServantBase
    {
        private SessionI session;

        private ObserverClient<IAccountObserver> observer = new ObserverClient<IAccountObserver>(Global.OrleansClient);
       
        private IAccountGrain accountGrain;

        public AccountI()
        {

        }

        public string GetFacet()
        {
            return "account";
        }

        public void setSessionI(SessionI sessionI)
        {
            this.session = sessionI;
        }


        public void Destroy()
        {
            observer.Unsubscribe();
        }

             


        public async override Task LoginRequestAsync(string account,string pwd, Current current = null)
        {
            //获取accountGrain
            accountGrain = Global.OrleansClient.GetGrain<IAccountGrain>(account);
           
            await observer.Subscribe(accountGrain, new AccountObserver(session));

            await accountGrain.Login(session.Id, account,pwd);

            session.Bind("account", account);
        }

        public async override Task RegisterRequestAsync(string account ,RegisterInfo info, Current current = null)
        {
            try
            {
                var accountGrain = Global.OrleansClient.GetGrain<IAccountGrain>(account);
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
            return await accountGrain.GetServerList();
        }


        public async override Task<string> CreatePlayerRequestAsync(string name, int serverId, Current current = null)
        {
            try
            {
                var playerId = await accountGrain.CreatePlayer(name, serverId);
                return playerId;
            }
            catch(System.Exception ex)
            {
                Console.WriteLine(ex);
                throw ex;
            }

        }


        public async override Task<List<PlayerShortInfo>> GetPlayerListRequestAsync(int serverId, Current current = null)
        {
            return await this.accountGrain.GetPlayerInfoShortList(serverId);
        }

        public async override Task SelectPlayerRequestAsync(string PlayerId, Current current = null)
        {
            try
            {
              //  var accountGrain = Global.OrleansClient.GetGrain<IAccountGrain>(session.Account);
                await this.accountGrain.SelectPlayer(PlayerId);

                session.Bind("PlayerId", Guid.Parse(PlayerId));
                session.PlayerId = session.Get<Guid>("PlayerId");

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
