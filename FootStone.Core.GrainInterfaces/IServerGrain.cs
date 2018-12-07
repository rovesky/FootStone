using Orleans;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FootStone.Core.GrainInterfaces
{
    public interface IServerGrain : IGrainWithIntegerKey
    {
        Task PlayerOnline(Guid playerId);

        Task PlayerOffline(Guid playerId);

        Task GetOnlinePlayersByLevel(int level);
    }
}
