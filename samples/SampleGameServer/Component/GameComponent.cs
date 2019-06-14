using FootStone.Core.GrainInterfaces;
using FootStone.Game;
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

        public GameComponent(IFSGrain grain, TState state):
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
            return Task.CompletedTask;
        }



        public Task DisableGame()
        {
            return Task.CompletedTask;
        }

        public Task EanbleGame()
        {
            return Task.CompletedTask;
        }
      

        public Task<GameState> GetGameState()
        {
            return Task.FromResult((GameState)gameState);
        }

     
    }
}
