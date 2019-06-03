using Orleans;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FootStone.Core.GrainInterfaces;

namespace FootStone.Game
{
    public class ObserverComponent<T> : ComponentBase, IObserverComponent<T> where T : IGrainObserver
    {
        private ObserverSubscriptionManager<T> subscribers;


        public ObserverComponent(IFSObject grain) : base(grain)
        {

        }

        public override Task Fini()
        {
            subscribers.Clear();
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
                Console.Out.WriteLine("accountObserver Unsubscribe end");
                subscribers.Unsubscribe(subscriber);
            }
            return Task.CompletedTask;
        }

        public Task Notify(Action<T> notification)
        {
            subscribers.Notify(notification);
            return Task.CompletedTask;
        }
   
    }
}
