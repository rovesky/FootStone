using Orleans;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FootStone.Game
{
    public abstract class ComponentBase : IFSComponent
    {
        protected IFSGrain FSGrain { get; }


        protected ComponentBase(IFSGrain grain)
        {
            FSGrain = grain;
        }

        public abstract Task Fini();
        public abstract Task Init();      
    }
}
