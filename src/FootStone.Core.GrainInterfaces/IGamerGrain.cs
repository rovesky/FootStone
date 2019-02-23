using Orleans;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FootStone.Core.GrainInterfaces
{
    public interface IGameGrain : IGrainWithIntegerKey
    {
        Task PlayerEnter(GamePlayerInfo info);

        Task PlayerLeave(Guid playerId);

        Task<List<GamePlayerInfo>> GetOnlinePlayersByLevel(int level);

        Task<GameInfo> GetGameInfo();
        Task EanbleGame();
        Task DisableGame();
    }
}
