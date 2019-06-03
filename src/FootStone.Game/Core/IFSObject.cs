using Orleans;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FootStone.Game
{
    public interface IFSGrain : IFSObject
    {

        IGrainFactory GrainFactory { get; }

        Grain Grain { get; }
    }



    public interface IFSObject
    {
    

        void AddComponent(IComponent component);

        void RemoveComponent(IComponent component);

        T FindComponent<T>();
    }
}
