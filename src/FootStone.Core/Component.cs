using System;
using System.Collections.Generic;
using System.Text;

namespace FootStone.Core
{
    public abstract class Component : IComponent
    {
        public abstract void Fini();
        public abstract void Init();
    }
}
