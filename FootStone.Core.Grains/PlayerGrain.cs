using FootStone.Core.GrainInterfaces;
using FootStone.GrainInterfaces;
using Orleans;
using Orleans.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootStone.Grains
{
    //public class PlayerState
    //{
      
    //   public string name;
    //   public int level;        
    //}
    [StorageProvider(ProviderName= "memory1")]
    public class PlayerGrain : Grain<PlayerInfo>, IPlayerGrain
    {
        private ObserverSubscriptionManager<IPlayerObserver> subscribers;

        public override Task OnActivateAsync()
        {
            subscribers = new ObserverSubscriptionManager<IPlayerObserver>();
       
            return Task.CompletedTask;
        }

        public override Task OnDeactivateAsync()
        {
            subscribers.Clear();
            return Task.CompletedTask;
        }
             

        public Task SubscribeForPlayerUpdates(IPlayerObserver subscriber)
        {
            if (!subscribers.IsSubscribed(subscriber))
            {
                subscribers.Subscribe(subscriber);
                RegisterTimer((s) =>
                {
                    State.playerMaster.hp++;
                    subscribers.Notify((t) =>
                    {
                        t.HpChanged(State.playerMaster.hp);
                    });
                    return Task.CompletedTask;
                }
                , null
                , TimeSpan.FromSeconds(10)
                , TimeSpan.FromSeconds(10));
            }
            return Task.CompletedTask;
        }

        public Task UnsubscribeForPlayerUpdates(IPlayerObserver subscriber)
        {
            if (subscribers.IsSubscribed(subscriber))
            {
                subscribers.Unsubscribe(subscriber);
            }
            return Task.CompletedTask;
        }

        public Task<PlayerInfo> GetPlayerInfo()
        {
            return Task.FromResult(this.State);
        }

        public async Task CreatePlayer(string name, int serverId)
        {
            this.State.id = this.GetPrimaryKey().ToString();
            this.State.name = name;
            this.State.serverId = serverId;
            this.State.items = new List<Item>();
            this.State.items.Add(new Item("1", "item1", 1));
            this.State.items.Add(new Item("2", "item2", 2));
            this.State.playerMaster.hp = 10;
            this.State.playerMaster.mp = 10;
            Console.WriteLine("create player:" + this.State.name);
            await WriteStateAsync();
        }

        public async Task SetPlayerName(string name)
        {
            this.State.name = name;
            await WriteStateAsync();
        }
    }
}
