using FootStone.Core;
using FootStone.Core.GrainInterfaces;
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

    public abstract partial class GameGrain : FootStoneGrain, IGameManager
    {      

        public Task DisableGame() 
        {
            return FindComponent<IGameManager>().DisableGame();
        }

        public Task EanbleGame()
        {
            return FindComponent<IGameManager>().EanbleGame();
        }

        public Task<GameInfo> GetGameInfo()
        {
            return FindComponent<IGameManager>().GetGameInfo();
        }



    }
}