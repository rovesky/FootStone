using FootStone.Core.GrainInterfaces;
using FootStone.GrainInterfaces;
using Orleans;
using Orleans.Providers;
using Orleans.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootStone.Grains
{

    [StorageProvider(ProviderName= "memory1")]
    public partial class PlayerGrain : Grain<PlayerInfo>, IPlayerGrain
    {
        private ObserverSubscriptionManager<IPlayerObserver> subscribers;
        private IZoneGrain zoneGrain;
        private bool isOnline;
        readonly IIceServiceClient IceServiceClient;

        public PlayerGrain(IGrainActivationContext grainActivationContext, IIceServiceClient iceServiceClient)
        {
      
            IceServiceClient = iceServiceClient;
        }

        public override Task OnActivateAsync()
        {
            subscribers = new ObserverSubscriptionManager<IPlayerObserver>();

            //try
            //{
            //    zoneGrain = this.GrainFactory.GetGrain<IZoneGrain>(Guid.NewGuid());

            //    await zoneGrain.PlayerEnter(this.GetPrimaryKey());
            //    await IceServiceClient.AddPlayer(this.GetPrimaryKey());
            //}
            //catch (Exception e)
            //{
            //    Console.Error.WriteLine(e.Message);
            //}

                  

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
                    State.roleMaster.property.intel++;
                    subscribers.Notify((t) =>
                    {
                        t.HpChanged(State.roleMaster.property.intel);
                    });
                    //    return Task.CompletedTask;
                    return WriteStateAsync();
                }
                , null
                , TimeSpan.FromSeconds(10)
                , TimeSpan.FromSeconds(10));
            }
            isOnline = true;
            return Task.CompletedTask;
        }

        public Task UnsubscribeForPlayerUpdates(IPlayerObserver subscriber)
        {
            if (subscribers.IsSubscribed(subscriber))
            {
                Console.Out.WriteLine("playerObserver Unsubscribe end");
                subscribers.Unsubscribe(subscriber);
            }
            isOnline = false;
            return Task.CompletedTask;
        }

        public  Task<PlayerInfo> GetPlayerInfo()
        {
          
            //     IceServiceClient.AddOptionTime(1);
          //  return this.State;
            return Task.FromResult(this.State);
        }

        public async Task InitPlayer(string name, int serverId)
        {
            this.State.id = this.GetPrimaryKey().ToString();
            this.State.name = name;
            this.State.serverId = serverId;

            this.State.zoneId = (await IceServiceClient.GetZone(this.GetPrimaryKey())).ToString();
            this.State.items = new List<Item>();
            this.State.items.Add(new Item("1", "item1", 1));
            this.State.items.Add(new Item("2", "item2", 2));
            this.State.roleMaster.property.intel = 10;
            this.State.roleMaster.property.str= 10;
            this.State.roleMaster.property.agil = 10;
            Console.WriteLine("create player:" + this.State.name);
            await WriteStateAsync();
        }

        public  Task SetPlayerName(string name)
        {
            this.State.name = name;
         //   IceServiceClient.AddOptionTime(1);
            return Task.CompletedTask;
           // await WriteStateAsync();
        }
    }
}
