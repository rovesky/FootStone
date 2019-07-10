using Ice;
using System;
using System.Threading.Tasks;

namespace FootStone.Client
{
    public interface IFSSession
    {
        string GetId();
        
        T UncheckedCast<T>(Func<ObjectPrx,string,T> uncheckedCast) where T : ObjectPrx;

        Task<IFSChannel> createStreamChannel();

        event EventHandler OnDestroyed;

        void  Destory();
    }
}
