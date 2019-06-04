using Orleans;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FootStone.Game
{
    public interface IFSObject
    {
        void AddComponent(IFSComponent component);

        void RemoveComponent(IFSComponent component);

        T FindComponent<T>();
    }
}
