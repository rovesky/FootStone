using FootStone.Core;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Orleans.Messaging;
using Microsoft.Extensions.Logging;
using NLog;
using System.Timers;

namespace FootStone.FrontNetty
{
    public class NettyFrontService : IClientService
    {
        private FrontServerNetty frontSever = new FrontServerNetty();
        private GameClientNetty frontClient = new GameClientNetty();
       // private NettyGameOptions gameOptions;
        private NettyFrontOptions frontOptions;
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private Timer pingTimer;

        public Task Init(IServiceProvider serviceProvider)
        {
           // this.gameOptions = serviceProvider.GetService<IOptions<NettyGameOptions>>().Value;
            this.frontOptions = serviceProvider.GetService<IOptions<NettyFrontOptions>>().Value;

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
               await frontClient.ConnectNettyAsync(uri.Host, frontOptions.GamePort);
            }

            //flush channel
            pingTimer = new System.Timers.Timer();
            pingTimer.AutoReset = true;
            pingTimer.Interval = 33;
            pingTimer.Enabled = true;
            pingTimer.Elapsed += (_1, _2) =>
            {
                ChannelManager.Instance.FlushAllSiloChannel();
            };
            pingTimer.Start();

            logger.Info("netty connect all silo ok!");
            await frontSever.Start(frontOptions.FrontPort);       


        }

        public async Task Stop()
        {         
            if(pingTimer != null)
            {
                pingTimer.Stop();
            }
            await frontSever.Stop();

            await frontClient.Fini();
        }
    }
}
