using FootStone.GrainInterfaces;
using Orleans;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FootStone.Core.GrainInterfaces
{
    public interface IZoneGrain : IGrainWithGuidKey
    {
        Task<EndPointZone> PlayerEnter(Guid playerId);

        Task PlayerLeave(Guid playerId);
     //   Task PlayerBindChannel(Guid playerId, string channelId);

        //Task GetOnlinePlayersByLevel(int level);
    }
}
