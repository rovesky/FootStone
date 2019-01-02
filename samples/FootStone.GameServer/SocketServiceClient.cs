using FootStone.Core.GrainInterfaces;
using Orleans.Runtime.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FootStone.Core.GameServer
{
    public class SocketServiceClient : GrainServiceClient<ISocketServiceClient>, ISocketServiceClient
    {
        public SocketServiceClient(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }


        public Task AddOptionTime(int time) =>  GrainService.AddOptionTime(time);

       
    }
}
