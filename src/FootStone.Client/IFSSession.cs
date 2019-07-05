using DotNetty.Transport.Channels;
using FootStone.GrainInterfaces;
using System;

namespace FootStone.Client
{
    public interface IFSSession
    {
        string GetId();

        ISessionPrx GetSessionPrx();

        IChannel GetStreamChannel();

        event EventHandler OnDestroyed;

        void  Destory();
    }
}
