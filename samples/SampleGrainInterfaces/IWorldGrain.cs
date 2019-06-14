using FootStone.GrainInterfaces;
using Orleans;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FootStone.Core.GrainInterfaces
{
    public interface IWorldGrain : IGrainWithStringKey
    {

        Task Init(string configRoot);

        Task<List<ServerInfo>> GetServerList();

        Task<List<PlayerShortInfo>> GetPlayerInfoShortList(string account,int gameId);

        Task<string> DispatchZone(string playerId, int gameId);
    }
}
