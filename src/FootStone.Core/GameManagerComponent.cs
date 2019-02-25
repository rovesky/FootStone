using FootStone.Core.GrainInterfaces;
using FootStone.GrainInterfaces;
using Orleans;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FootStone.Core
{
    public class GameManagerComponent : ComponentBase,IGameManager
    {
        private GameInfo gameInfo;

        public GameManagerComponent(FootStoneGrain grain):
            base(grain)
        {
           
        }

        public override Task Init()
        {
            gameInfo = new GameInfo(1);
            return Task.CompletedTask;
        }

        public override Task Fini()
        {
            throw new NotImplementedException();
        }



        public Task DisableGame()
        {
            throw new NotImplementedException();
        }

        public Task EanbleGame()
        {
            throw new NotImplementedException();
        }

      

        public Task<GameInfo> GetGameInfo()
        {
            return Task.FromResult(gameInfo);
        }

     
    }
}
