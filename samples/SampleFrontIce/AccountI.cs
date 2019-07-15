using FootStone.Core;
using FootStone.FrontIce;
using FootStone.GrainInterfaces;
using Ice;
using NLog;
using Orleans;
using System;
using System.Threading.Tasks;

namespace SampleFrontIce
{

    /// <summary>
    /// Observer class that implements the observer interface. Need to pass a grain reference to an instance of this class to subscribe for updates.
    /// </summary>
    class AccountObserver : IAccountObserver
    {
        private Session sessionI;

        private NLog.Logger logger = LogManager.GetCurrentClassLogger();

        public AccountObserver(Session sessionI)
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


    public class AccountI : IAccountDisp_, IServantBase
    {
        private Session session;
        private IClusterClient orleansClient;      

        private ObserverClient<IAccountObserver> observer ;
        private IAccountGrain accountGrain;
        private NLog.Logger logger = LogManager.GetCurrentClassLogger();

        public AccountI(Session session, IClusterClient orleansClient)
        {
            this.session = session;
            this.orleansClient = orleansClient;

            observer = new ObserverClient<IAccountObserver>(orleansClient);
        }


        public string GetFacet()
        {
            return nameof(IAccountPrx);
        }      


        public  void Dispose()
        {
             observer.Unsubscribe();
        }


        public async override Task LoginRequestAsync(string account, string pwd, Current current = null)
        {
            //获取accountGrain
            accountGrain = orleansClient.GetGrain<IAccountGrain>(account);

            await observer.Subscribe(accountGrain, new AccountObserver(session));

            await accountGrain.Login(session.Id, account, pwd);

            session.Account = account;
        }

        public async override Task RegisterRequestAsync(string account, RegisterInfo info, Current current = null)
        {
            var accountGrain = orleansClient.GetGrain<IAccountGrain>(account);

            await accountGrain.Register(info);
        }

        public override Task TestLoginRequestAsync(string account, string pwd, LoginData data, Current current = null)
        {
            var type = data.GetType();

            Console.WriteLine("TestLoginRequestAsync:" + type.Name);
            return Task.CompletedTask;
        }

    }
}
