using FootStone.Core.GrainInterfaces;
using FootStone.GrainInterfaces;
using Orleans;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FootStone.Core
{
    public class GameComponent<TState> :
        ComponentBase,
        IGameManager
        where TState:GameState

    {
        private TState gameState;

        public GameComponent(FootStoneGrain grain, TState state):
            base(grain)
        {
            this.gameState = state;
        }

        public override Task Init()
        {           
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

      

        public Task<GameState> GetGameState()
        {
            return Task.FromResult((GameState)gameState);
        }

     
    }
}
