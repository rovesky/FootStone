using Orleans;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FootStone.Core.GrainInterfaces;
using Orleans.Providers;

namespace FootStone.Game
{
   
    /// <summary>
    /// 支持存储版本的FootStoneGrain
    /// </summary>
    /// <typeparam name="TGrainState"></typeparam>
    public class FSGrain<TGrainState> : Grain<TGrainState>, IFSGrain where TGrainState : new()
    {
        protected FSObject fSObject = new FSObject();

        public new IGrainFactory GrainFactory
        {
            get
            {
                return base.GrainFactory;
            }
        }

        public override async Task OnActivateAsync()
        {       

            await fSObject.InitAllComponent();

            await base.OnActivateAsync();
        }


        public void AddComponent(IFSComponent component)
        {
            fSObject.AddComponent(component);
        }

        public void RemoveComponent(IFSComponent component)
        {
            fSObject.RemoveComponent(component);
        }

        public T FindComponent<T>()
        {
            return fSObject.FindComponent<T>();
        }

    }

    public struct EmptyState
    {

    }

    /// <summary>
    /// 无存储版本的FootStoneGrain
    /// </summary>
    [StorageProvider(ProviderName = "memory1")]
    public class FSGrain : FSGrain<EmptyState>
    {
    
    }


    /// <summary>
    /// 支持前端观察者版本的FootStoneGrain
    /// </summary>
    /// <typeparam name="TGrainState"></typeparam>
    /// <typeparam name="TObserver"></typeparam>
    public class FSGrain<TGrainState, TObserver> : FSGrain<TGrainState>, IObserverManager<TObserver>
        where TGrainState : new()
        where TObserver : IGrainObserver
    {

        private IObserverManager<TObserver> Observer { get; }

        protected FSGrain()
        {
            AddComponent(new ObserverManager<TObserver>(this));
            Observer = FindComponent<IObserverManager<TObserver>>();
        }


        public override async Task OnActivateAsync()
        {
        
            await base.OnActivateAsync();
        }

        public Task Subscribe(TObserver subscriber)
        {
            return Observer.Subscribe(subscriber);
        }

        public Task Unsubscribe(TObserver subscriber)
        {
            return Observer.Unsubscribe(subscriber);
        }

        public Task ClearObserver()
        {
            return Observer.ClearObserver();
        }

        public Task Notify(Action<TObserver> notification)
        {
            return Observer.Notify(notification);
        }
    }
  
}
