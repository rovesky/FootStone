using FootStone.Core.GrainInterfaces;
using FootStone.GrainInterfaces;
using Orleans;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FootStone.Core
{
    public class PlayerManagerComponent : ComponentBase,IPlayerManager,IPlayerObserver
    {
        private Dictionary<Guid, GamePlayerState> players = new Dictionary<Guid, GamePlayerState>();

        public PlayerManagerComponent(FootStoneGrain grain):
            base(grain)
        {
           
        }

    

        public override async Task Init()
        {
      
        }

        public override Task Fini()
        {
            throw new NotImplementedException();
        }


        public void HpChanged(int hp)
        {
            
        }

        public void LevelChanged(Guid playerId, int newLevel)
        {
            var info = this.players[playerId];
            info.level = newLevel;
            Console.WriteLine($"{info.name} new level {info.level}");
        }

        public async Task PlayerEnter(GamePlayerState info)
        {
            Guid id = Guid.Parse(info.id);

            players.Add(id, info);

            var playerGrain = Grain.GrainFactory.GetGrain<IPlayerGrain>(id);

            await playerGrain.SubscribeForPlayerUpdates(Grain as IPlayerObserver);
        }

        public async Task PlayerLeave(Guid playerId)
        {
            var playerGrain = Grain.GrainFactory.GetGrain<IPlayerGrain>(playerId);

            await playerGrain.UnsubscribeForPlayerUpdates(Grain as IPlayerObserver);

            players.Remove(playerId);
        }

        public Task<List<GamePlayerState>> GetOnlinePlayersByLevel(int level)
        {
            throw new NotImplementedException();
        }
    }
}
