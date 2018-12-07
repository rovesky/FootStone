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
    public partial class ServerGrain : Grain, IServerGrain
    {
     //   private ObserverSubscriptionManager<IPlayerObserver> subscribers;

        readonly IIceServiceClient IceServiceClient;

        public ServerGrain(IGrainActivationContext grainActivationContext, IIceServiceClient iceServiceClient)
        {
            IceServiceClient = iceServiceClient;
        }
               

        public override Task OnActivateAsync()
        {
     //       subscribers = new ObserverSubscriptionManager<IPlayerObserver>();
       
            return Task.CompletedTask;
        }

        public override Task OnDeactivateAsync()
        {
    //        subscribers.Clear();
            return Task.CompletedTask;
        }    


        public Task PlayerOnline(Guid playerId)
        {
            throw new NotImplementedException();
        }

        public Task PlayerOffline(Guid playerId)
        {
            throw new NotImplementedException();
        }

        public Task GetOnlinePlayersByLevel(int level)
        {
            throw new NotImplementedException();
        }
    }
}
