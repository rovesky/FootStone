using FootStone.Core.GrainInterfaces;
using FootStone.Game;
using FootStone.GrainInterfaces;
using Newtonsoft.Json;
using NLog;
using Orleans;
using Orleans.Providers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FootStone.Core
{
    
    public class AccountState
    {
        public string account;
        public string password;
      //  public string curPlayerId;
        public string token;
      //  public Dictionary<int,List<PlayerShortInfo>> players;      
    }

    [StorageProvider(ProviderName = "memory1")]
    public class AccountGrain : FSGrain<AccountState,IAccountObserver>, IAccountGrain
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();
        private string curPlayerId;

        public AccountGrain()
        {        
            
        }
        public async override Task OnActivateAsync()
        {     

            await base.OnActivateAsync();
        }

        public override Task OnDeactivateAsync()
        {
            return base.OnDeactivateAsync();
        }
               
        public Task Login(string sessionId,string account,string pwd)
        {           

            if (State.account == null)
            {
                throw new AccountException("account is not registered!");
            }

            if(!(State.account.Equals(account)
                && State.password.Equals(pwd)))
            {
                throw new AccountException("account or password is not  valid!");
            }
            State.token = Guid.NewGuid().ToString();


            //通知所有session已经登录成功         
            Notify((s) =>
            {
                s.AccountLogined(sessionId);
            });
            return WriteStateAsync();
        }

        public Task Register(RegisterInfo info)
        {
            logger.Debug("Begin RegisterRequest:"+ info.account);
            if (State.account != null)
            {
                throw new AccountException("account is registered!");
            }

            State.account = info.account;
            State.password = info.password;

           // throw new AccountException("test Exception!");
            return WriteStateAsync();
        }

        public Task setCurPlayerId(string playerId)
        {
            curPlayerId = playerId;
            return Task.CompletedTask;
            
        }
    }    
}
