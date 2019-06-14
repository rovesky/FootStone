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

    public  partial class GameGrain : FSGrain<GameState>, IGameGrain 
    {      

        public Task DisableGame() 
        {
            return FindComponent<IGameManager>().DisableGame();
        }

        public Task EanbleGame()
        {
            return FindComponent<IGameManager>().EanbleGame();
        }

        public Task<GameState> GetGameState()
        {
            return FindComponent<IGameManager>().GetGameState();
        }



    }
}