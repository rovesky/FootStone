using FootStone.GrainInterfaces;
using Ice;
using System;
using System.Collections.Generic;
using System.Text;

namespace FootStone.Client
{
    public interface IFSSession
    {
        ISessionPrx GetSessionPrx();

        event EventHandler OnDestroyed;

        void  Destory();
    }
}
