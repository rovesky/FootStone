using Orleans;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FootStone.Core
{
    public abstract class FootStoneGrain : Grain, IIncomingGrainCallFilter
    {
        private Dictionary<Type, IComponent> components = new Dictionary<Type, IComponent>();

        public new IGrainFactory GrainFactory
        {
            get
            {
                return base.GrainFactory;
            }
        }

        public override async Task OnActivateAsync()
        {
            await InitAllComponent();

            await base.OnActivateAsync();
        }

        private async Task InitAllComponent()
        {
            foreach(var com in components.Values)
            {
                await com.Init();
            }
        }

        protected  void  AddComponent(IComponent component)
        {
            
            Type type = component.GetType();  
            foreach (var typeI in type.GetInterfaces()){              
                if (!components.ContainsKey(typeI))
                    components.Add(typeI, component);
            }
        }

        protected void RemoveComponent(IComponent component)
        {
            Type type = component.GetType();
            foreach (var typeI in type.GetInterfaces())
            {
                if (components.ContainsKey(typeI))
                    components.Remove(typeI);
            }
        }

        protected T FindComponent<T>()
        {
            return (T)components[typeof(T)];
        }

      

        public async Task Invoke(IIncomingGrainCallContext context)
        {
            try
            {
                await context.Invoke();
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }

            //foreach(var com in components.Values)
            //{
            //    foreach(var m in com.GetType().GetMethods())
            //    {
            //        if(m.Name == context.InterfaceMethod.Name)
            //        {
            //            context.Result =  m.Invoke(com, context.Arguments);
            //        }
            //    }
            //}   
        }



    }
}
