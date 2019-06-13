using FootStone.Core.GrainInterfaces;
using Orleans;
using System;
using System.Threading.Tasks;

namespace FootStone.Game
{
    /// <summary>
    /// 观察者组件，包装了Orleans的ObserverSubscriptionManager类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObserverManager<T> : ComponentBase, IObserverManager<T> where T : IGrainObserver
    {
        private ObserverSubscriptionManager<T> subscribers;


        public ObserverManager(IFSGrain grain) : base(grain)
        {

        }

        public override Task Fini()
        {
            subscribers.Clear();
            subscribers = null;
            return Task.CompletedTask;
        }

        public override Task Init()
        {
            subscribers = new ObserverSubscriptionManager<T>();
            return Task.CompletedTask;
        }
        

        public Task Subscribe(T subscriber)
        {
            if (!subscribers.IsSubscribed(subscriber))
            {
                subscribers.Subscribe(subscriber);
            }
            return Task.CompletedTask;
        }

        public Task Unsubscribe(T subscriber)
        {
            if (subscribers.IsSubscribed(subscriber))
            {            
                subscribers.Unsubscribe(subscriber);
            }
            return Task.CompletedTask;
        }

        public Task Notify(Action<T> notification)
        {
            subscribers.Notify(notification);
            return Task.CompletedTask;
        }
   
        public Task ClearObserver()
        {
            subscribers.Clear();
            return Task.CompletedTask;
        }
    }
}
