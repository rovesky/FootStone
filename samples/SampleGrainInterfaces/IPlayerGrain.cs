using FootStone.Core.GrainInterfaces;
using Orleans;
using System.Threading.Tasks;

namespace FootStone.GrainInterfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IPlayerGrain : IGrainWithGuidKey, IObserverManager<IPlayerObserver>
    {

        Task CreatePlayer(string account, int serverId, PlayerCreateInfo info);

        Task PlayerOnline();

        Task PlayerOffline();

        Task Ping();

        Task SetPlayerName(string name);

        Task<PlayerInfo> GetPlayerInfo();
    }
}
