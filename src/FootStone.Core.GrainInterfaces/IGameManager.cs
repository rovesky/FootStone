using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FootStone.Core.GrainInterfaces
{
    public interface IGameManager 
    {
        Task<GameState> GetGameState();
        Task EanbleGame();
        Task DisableGame();
    }
}
