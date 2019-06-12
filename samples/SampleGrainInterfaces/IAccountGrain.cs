using FootStone.Core.GrainInterfaces;
using Orleans;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FootStone.GrainInterfaces
{
    public interface IAccountGrain :  IGrainWithStringKey, IObserverManager<IAccountObserver>
    {
        Task Login(string sessionId,string account,string pwd);

        Task Register(RegisterInfo info);

        Task setCurPlayerId(string playerId);

        Task CreatePlayer(int gameId,PlayerShortInfo info);

        Task<List<PlayerShortInfo>> GetPlayersShortInfo(int gameId);
    }
}
