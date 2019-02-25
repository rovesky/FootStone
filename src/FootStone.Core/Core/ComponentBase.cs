﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FootStone.Core
{
    public abstract class ComponentBase : IComponent
    {
        protected FootStoneGrain Grain { get; }


        protected ComponentBase(FootStoneGrain grain)
        {
            Grain = grain;
        }

        public abstract Task Fini();
        public abstract Task Init();

      
    }
}
