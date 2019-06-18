using FootStone.Core;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Orleans.Messaging;

namespace FootStone.FrontNetty
{
    public class NettyFrontService : IClientService
    {
        private NetworkServerNetty frontSever = new NetworkServerNetty();
        private NetworkClientNetty frontClient = new NetworkClientNetty();
        private NettyOptions options;

        public Task Init(IServiceProvider serviceProvider)
        {
            this.options = serviceProvider.GetService<IOptions<NettyOptions>>().Value;

            frontSever.Init();
            frontClient.Init();
            return Task.CompletedTask;
        }

        public async Task Start()
        {

            //连接所有的silo
            var gatewayProvider = (IGatewayListProvider)Global.OrleansClient.ServiceProvider.GetService(typeof(IGatewayListProvider));
            IList<Uri> gateways = await gatewayProvider.GetGateways();
            foreach(var uri in gateways)
            {
               await frontClient.ConnectNettyAsync(uri.Host, uri.Port, "");
            }

            await frontSever.Start(options.Port);
       
        }

        public async Task Stop()
        {         
            await frontSever.Stop();

            await frontClient.Fini();
        }
    }
}
