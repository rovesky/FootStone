using FootStone.Core.GrainInterfaces;
using Orleans.Runtime.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace FootStone.Core.GameServer
{
    public class IceServiceClient : GrainServiceClient<IIceService>, IIceService
    {
        public IceServiceClient(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

    }
}
