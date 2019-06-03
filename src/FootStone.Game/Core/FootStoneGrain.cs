using Orleans;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FootStone.Core.GrainInterfaces;

namespace FootStone.Game
{
    public class FSObject : IFSObject
    {
        private Dictionary<Type, IComponent> components = new Dictionary<Type, IComponent>();

        public void AddComponent(IComponent component)
        {

            Type type = component.GetType();
            foreach (var typeI in type.GetInterfaces())
            {
                if (!components.ContainsKey(typeI))
                    components.Add(typeI, component);
            }
        }

        public void RemoveComponent(IComponent component)
        {
            Type type = component.GetType();
            foreach (var typeI in type.GetInterfaces())
            {
                if (components.ContainsKey(typeI))
                    components.Remove(typeI);
            }
        }

        public T FindComponent<T>()
        {
            return (T)components[typeof(T)];
        }

        public async Task InitAllComponent()
        {
            foreach (var com in components.Values)
            {
                await com.Init();
            }
        }
    }

    //public abstract class FootStoneGrain : Grain,  IFSGrain
    //{

    //    private FSObject FSOject  { get; set; }

    //    public new IGrainFactory GrainFactory
    //    {
    //        get
    //        {
    //            return base.GrainFactory;
    //        }
    //    }

    //    public Grain Grain => this;

    //    public override async Task OnActivateAsync()
    //    {
    //        await FSOject.InitAllComponent();

    //        await base.OnActivateAsync();
    //    }
     

    //    public async Task Invoke(IIncomingGrainCallContext context)
    //    {
    //        try
    //        {
    //            await context.Invoke();
    //        }
    //        catch (Exception e)
    //        {
    //            Console.WriteLine(e);
    //        }

    //        //foreach(var com in components.Values)
    //        //{
    //        //    foreach(var m in com.GetType().GetMethods())
    //        //    {
    //        //        if(m.Name == context.InterfaceMethod.Name)
    //        //        {
    //        //            context.Result =  m.Invoke(com, context.Arguments);
    //        //        }
    //        //    }
    //        //}   
    //    }

    //    //public async Task  InitAllComponent()
    //    //{
    //    //    await ((IFSObject)FSOject).InitAllComponent();
    //    //}

    //    public void AddComponent(IComponent component)
    //    {
    //        ((IFSObject)FSOject).AddComponent(component);
    //    }

    //    public void RemoveComponent(IComponent component)
    //    {
    //        ((IFSObject)FSOject).RemoveComponent(component);
    //    }

    //    public T FindComponent<T>()
    //    {
    //        return ((IFSObject)FSOject).FindComponent<T>();
    //    }    
    //}


    public class FootStoneGrain<TGrainState> : Grain<TGrainState>, IFSGrain where TGrainState : new()
    {
        protected FSObject fSOject = new FSObject();

        public new IGrainFactory GrainFactory
        {
            get
            {
                return base.GrainFactory;
            }
        }

        public Grain Grain => this; 

        public override async Task OnActivateAsync()
        {

            await base.OnActivateAsync();

            await fSOject.InitAllComponent();
        }


        public void AddComponent(IComponent component)
        {
            fSOject.AddComponent(component);
        }

        public void RemoveComponent(IComponent component)
        {
            fSOject.RemoveComponent(component);
        }

        public T FindComponent<T>()
        {
            return fSOject.FindComponent<T>();
        }

    }

    public class FootStoneGrain : FootStoneGrain<int>
    {
        public new Grain Grain => this;
    }




    public class FootStoneGrain<TGrainState, TObserver> : FootStoneGrain<TGrainState>, IObserverComponent<TObserver>
        where TGrainState : new()
        where TObserver : IGrainObserver
    {
        protected FootStoneGrain()
        {

        }

        public override async Task OnActivateAsync()
        {
            AddComponent(new ObserverComponent<TObserver>(this));
            await base.OnActivateAsync();          
        }        

        public Task Subscribe(TObserver subscriber)
        {
            var observer = FindComponent<IObserverComponent<TObserver>>();
            return observer.Subscribe(subscriber);
        }

        public Task Unsubscribe(TObserver subscriber)
        {
            var observer = FindComponent<IObserverComponent<TObserver>>();
            return observer.Unsubscribe(subscriber);

        }

        public Task Notify(Action<TObserver> notification)
        {
            var observer = FindComponent<IObserverComponent<TObserver>>();
            return observer.Notify(notification);
        }
    }
  
}
