using System;
using System.Threading.Tasks;

namespace FootStone.Core.GrainInterfaces
{
    public interface IObserverComponent<T>
    {

        Task Subscribe(T subscriber);

        Task Unsubscribe(T subscriber);

        Task Notify(Action<T> notification);

    }
}