using FootStone.Core.GrainInterfaces;
using FootStone.GrainInterfaces;
using Ice;
using System;
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

    public class AccountI : AccountDisp_,IDisposable
    {
        private SessionI sessionI;
        private IAccountObserver accountObserver;

        public AccountI(SessionI sessionI)
        {
            this.sessionI = sessionI;           
        }
              

        public async override Task<string> CreatePlayerAsync(string name, int serverId, Current current = null)
        {

            try
            {
                var id = Guid.NewGuid();
                var player = Global.Instance.OrleansClient.GetGrain<IPlayerGrain>(id);
                await player.CreatePlayer(name, serverId);
                sessionI.PlayerId = id;
                return id.ToString();
            }
            catch (System.Exception ex)
            {
                Console.Error.WriteLine(ex);
                throw ex;
            }
        }

        public void Dispose()
        {
            if (accountObserver != null)
            {
                Console.Out.WriteLine("accountObserver Unsubscribe begin");
                var account = Global.Instance.OrleansClient.GetGrain<IAccountGrain>(sessionI.Account);
                account.UnsubscribeForAccount(accountObserver);
                accountObserver = null;
            }
        }

        public async override Task LoginRequestAsync(LoginInfo info, Current current = null)
        {

            try
            {
                var accountGrain = Global.Instance.OrleansClient.GetGrain<IAccountGrain>(info.account);

                accountObserver = await Global.Instance.OrleansClient.
                    CreateObjectReference<IAccountObserver>(new AccountObserver(sessionI));
                await accountGrain.SubscribeForAccount(accountObserver );

                await accountGrain.LoginRequest(sessionI.Id, info);
                sessionI.Account = info.account;
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
