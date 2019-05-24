using FootStone.GrainInterfaces;
using Orleans;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FootStone.GrainInterfaces
{
    public interface IAccountGrain : IGrainWithStringKey
    {
        Task Login(string sessionId, LoginInfo info);

        Task Register(RegisterInfo info);

        Task<List<ServerInfo>> GetServerList();

        Task<List<PlayerShortInfo>> GetPlayerInfoShortList(int serverId);

        Task  SelectPlayer(string playerId);

        Task<string> CreatePlayer(string name, int serverId);

        Task SubscribeForAccount(IAccountObserver subscriber);

        Task UnsubscribeForAccount(IAccountObserver subscriber);
    }
}
