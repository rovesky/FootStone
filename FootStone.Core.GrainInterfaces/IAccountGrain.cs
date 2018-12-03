using FootStone.GrainInterfaces;
using Orleans;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FootStone.Core.GrainInterfaces
{
    public interface IAccountGrain : IGrainWithStringKey
    {
        Task LoginRequest(string sessionId, LoginInfo info);

        Task RegisterRequest(RegisterInfo info);

      //  Task<string> GetPlayerList();

        Task<string> CreatePlayer(string name, int serverId);

        Task SubscribeForAccount(IAccountObserver subscriber);

        Task UnsubscribeForAccount(IAccountObserver subscriber);
    }
}
