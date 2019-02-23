using Orleans.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace FootStone.Core.GrainInterfaces
{
    public interface INettyServiceClient : IGrainServiceClient<INettyService>, INettyService
    {
    }
}
