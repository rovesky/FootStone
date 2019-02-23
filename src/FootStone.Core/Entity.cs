using System;
using System.Collections.Generic;
using System.Text;
using Orleans;

namespace FootStone.Core
{
    public abstract class Entity : Grain,IEntity
    {
        private Dictionary<string, Component> components = new Dictionary<string, Component>();


        public void AddComponent(string type, Component component)
        {
            components.Add(type, component);
        }
    }
}
