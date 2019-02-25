using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;

namespace FootStone.Core.GrainInterfaces
{
    public interface IPlayerManager
    {        
        Task PlayerEnter(GamePlayerInfo info);

        Task PlayerLeave(Guid playerId);

        Task<List<GamePlayerInfo>> GetOnlinePlayersByLevel(int level);
    }
}
