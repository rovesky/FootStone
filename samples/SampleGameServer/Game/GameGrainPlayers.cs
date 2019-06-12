using FootStone.Core;
using FootStone.Core.GrainInterfaces;
using FootStone.Game;
using FootStone.GrainInterfaces;
using Orleans;
using Orleans.Providers;
using Orleans.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootStone.Core
{

    public abstract partial class GameGrain : FSGrain, IPlayerManager, IPlayerObserver
    {

        public Task PlayerEnter(GamePlayerState info)
        {
            return FindComponent<IPlayerManager>().PlayerEnter(info);
        }

        public Task PlayerLeave(Guid playerId)
        {
            return FindComponent<IPlayerManager>().PlayerLeave(playerId);
        }

        public Task<List<GamePlayerState>> GetOnlinePlayersByLevel(int level)
        {
            return FindComponent<IPlayerManager>().GetOnlinePlayersByLevel(level);          
        }

        public void HpChanged(int hp)
        {
            FindComponent<IPlayerObserver>().HpChanged(hp);  
        }

        public void LevelChanged(Guid playerId, int newLevel)
        {        
            FindComponent<IPlayerObserver>().LevelChanged(playerId, newLevel);            
        }

        public Task<List<GamePlayerState>> GetPlayersByAccount(string account)
        {
            return FindComponent<IPlayerManager>().GetPlayersByAccount(account);
        }
    }
}