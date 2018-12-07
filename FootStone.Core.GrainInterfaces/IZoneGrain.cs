using Orleans;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FootStone.Core.GrainInterfaces
{
    public interface IZoneGrain : IGrainWithGuidKey
    {
        Task PlayerEnter(Guid playerId);

        Task PlayerLeave(Guid playerId);

        //Task GetOnlinePlayersByLevel(int level);
    }
}
