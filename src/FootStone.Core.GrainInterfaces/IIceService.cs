using Orleans.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FootStone.Core.GrainInterfaces
{
    public interface IIceService : IGrainService
    {
        Task AddOptionTime(int time);

        Task<Guid> GetZone(Guid playerId);
    }
}
