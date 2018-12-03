using FootStone.Core.GrainInterfaces;
using FootStone.GrainInterfaces;
using Orleans;
using Orleans.Providers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FootStone.Core.Grains
{
    public class AccountPlayerInfo
    {
        public string playerId;
        public string name;

        public AccountPlayerInfo(string playerId, string name)
        {
            this.playerId = playerId;
            this.name = name;
        }
    }
    public class AccountState
    {
        public string account;
        public string password;
        public string token;
        public Dictionary<int,List<AccountPlayerInfo>> players;
      
    }

    [StorageProvider(ProviderName = "memory1")]
    public class AccountGrain : Grain<AccountState>, IAccountGrain
    {
        private ObserverSubscriptionManager<IAccountObserver> subscribers;

        public override Task OnActivateAsync()
        {
            subscribers = new ObserverSubscriptionManager<IAccountObserver>();

            return Task.CompletedTask;
        }

        public override Task OnDeactivateAsync()
        {
            subscribers.Clear();
            return Task.CompletedTask;
        }

        public Task SubscribeForAccount(IAccountObserver subscriber)
        {
            if (!subscribers.IsSubscribed(subscriber))
            {
                subscribers.Subscribe(subscriber);              
            }
            return Task.CompletedTask;
        }

        public Task UnsubscribeForAccount(IAccountObserver subscriber)
        {
            if (subscribers.IsSubscribed(subscriber))
            {
                Console.Out.WriteLine("accountObserver Unsubscribe end");
                subscribers.Unsubscribe(subscriber);
            }
            return Task.CompletedTask;
        }


        public Task LoginRequest(string sessionId,LoginInfo info)
        {
            if(State.account == null)
            {
                throw new AccountException("account is not registered!");
            }
            if(!(State.account.Equals(info.account)
                && State.password.Equals(info.password)))
            {
                throw new AccountException("account or password is not  valid!");
            }
            State.token = Guid.NewGuid().ToString();

            //通知所有session已经登录成功
            subscribers.Notify((s) =>
            {
                s.AccountLogined(sessionId);
            });
            return WriteStateAsync();
        }

        public Task RegisterRequest(RegisterInfo info)
        {
            Console.WriteLine("Begin RegisterRequest:"+ info.account);
            if (State.account != null)
            {
                throw new AccountException("account is registered!");
            }

            State.account = info.account;
            State.password = info.password;

            return WriteStateAsync();
        }

        public async Task<string> CreatePlayer(string name, int serverId)
        {
            var playerId = Guid.NewGuid();
            var playerGrain = GrainFactory.GetGrain<IPlayerGrain>(playerId);
            await playerGrain.InitPlayer(name, serverId);

            if(State.players == null)
            {
                State.players = new Dictionary<int, List<AccountPlayerInfo>>();               
            }
            if (!State.players.ContainsKey(serverId))
            {                
                State.players.Add(serverId, new List<AccountPlayerInfo>());
            }
            State.players[serverId].Add(new AccountPlayerInfo(playerId.ToString(), name));

            return playerId.ToString();
        }
    }
}
