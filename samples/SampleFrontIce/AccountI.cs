using FootStone.Core;
using FootStone.FrontIce;
using FootStone.GrainInterfaces;
using Ice;
using NLog;
using System;
using System.Threading.Tasks;

namespace SampleFrontIce
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
        private NLog.Logger logger = LogManager.GetCurrentClassLogger();

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


        public async void Dispose()
        {
            await observer.Unsubscribe();
        }


        public async override Task LoginRequestAsync(string account, string pwd, Current current = null)
        {
            //获取accountGrain
            accountGrain = Global.OrleansClient.GetGrain<IAccountGrain>(account);

            await observer.Subscribe(accountGrain, new AccountObserver(session));

            await accountGrain.Login(session.Id, account, pwd);

            session.Account = account;
        }

        public async override Task RegisterRequestAsync(string account, RegisterInfo info, Current current = null)
        {
            var accountGrain = Global.OrleansClient.GetGrain<IAccountGrain>(account);

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
