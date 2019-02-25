using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FootStone.Core.GrainInterfaces
{
    public interface IGameManager
    {
        Task<GameInfo> GetGameInfo();
        Task EanbleGame();
        Task DisableGame();
    }
}
