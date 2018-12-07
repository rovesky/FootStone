using FootStone.Core.GrainInterfaces;
using Orleans;
using System.Threading.Tasks;

namespace FootStone.GrainInterfaces
{
    /// <summary>
    /// A player is, well, there's really no other good name...
    /// </summary>
    public interface IPlayerGrain : IGrainWithGuidKey
    {     

        Task SetPlayerName(string name);
        Task<PlayerInfo> GetPlayerInfo();



        Task InitPlayer(string name,int serverId);

        Task SubscribeForPlayerUpdates(IPlayerObserver subscriber);
        Task UnsubscribeForPlayerUpdates(IPlayerObserver subscriber);
    }
}
