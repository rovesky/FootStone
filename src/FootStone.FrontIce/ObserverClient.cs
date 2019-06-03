using FootStone.Core.GrainInterfaces;
using Orleans;
using System.Threading.Tasks;

namespace FootStone.FrontIce
{
    public class ObserverClient<TObserver>
        where TObserver : IGrainObserver
    {
        private IObserverComponent<TObserver> grain;
        private TObserver observer;
        private TObserver observerRef;
        private IClusterClient client;


        public ObserverClient(IClusterClient client)
        {
            this.client = client;
        }

        public async Task Subscribe(IObserverComponent<TObserver> grain, TObserver observerObj)
        {
            if (observer == null)
            {
                this.grain = grain;
                observer = observerObj;
                observerRef = await client.CreateObjectReference<TObserver>(observer);
                await grain.Subscribe(observerRef);
            }
        }

        public async Task Unsubscribe()
        {
            if (observerRef != null)
            {
                await grain.Unsubscribe(observerRef);
            }
        }
    }
}
