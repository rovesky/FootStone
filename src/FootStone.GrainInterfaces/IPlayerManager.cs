using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;

namespace FootStone.Core.GrainInterfaces
{
    public interface IPlayerManager
    {        
        Task PlayerEnter(GamePlayerState state);

        Task PlayerLeave(Guid playerId);

        Task<List<GamePlayerState>> GetOnlinePlayersByLevel(int level);

        Task<List<GamePlayerState>> GetPlayersByAccount(string account);

        Task<int> GetPlayerCount();


    }
}
