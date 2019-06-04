using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FootStone.Game
{
    public class FSObject : IFSObject
    {
        private Dictionary<Type, IFSComponent> components = new Dictionary<Type, IFSComponent>();

        public void AddComponent(IFSComponent component)
        {

            Type type = component.GetType();
            foreach (var typeI in type.GetInterfaces())
            {
                if (!components.ContainsKey(typeI))
                    components.Add(typeI, component);
            }
        }

        public void RemoveComponent(IFSComponent component)
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
}
