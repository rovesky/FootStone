using FootStone.Core.GrainInterfaces;
using Orleans.Runtime.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FootStone.Core.GameServer
{
    public class IceServiceClient : GrainServiceClient<IIceServiceClient>, IIceServiceClient
    {
        public IceServiceClient(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }


        public Task AddOptionTime(int time) =>  GrainService.AddOptionTime(time);
        
    }
}
