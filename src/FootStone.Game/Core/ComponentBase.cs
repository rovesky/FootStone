using Orleans;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FootStone.Game
{
    public abstract class ComponentBase : IComponent
    {
        protected IFSObject FSObject { get; }


        protected ComponentBase(IFSObject obj)
        {
            FSObject = obj;
        }



        public abstract Task Fini();
        public abstract Task Init();

      
    }
}
