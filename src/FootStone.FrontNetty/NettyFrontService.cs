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
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private FrontServerNetty frontServer = new FrontServerNetty();
        private GameClientNetty  gameClient = new GameClientNetty();

        private IChannelManager cmFront = new ChannelManager();
        private IChannelManager cmGame = new ChannelManager();

        private NettyFrontOptions frontOptions; 


        public Task Init(IServiceProvider serviceProvider)
        {
            frontOptions = serviceProvider.GetService<IOptions<NettyFrontOptions>>().Value;

            frontServer.Init(cmFront);

            gameClient.Init(cmGame);
    
            return Task.CompletedTask;
        }

        public async Task Start()
        {
            //连接所有的silo
            //var gatewayProvider = (IGatewayListProvider)Global.OrleansClient.ServiceProvider.GetService(typeof(IGatewayListProvider));
            //IList<Uri> gateways = await gatewayProvider.GetGateways();
            //foreach(var uri in gateways)
            //{
            //   await gameClient.ConnectNettyAsync(uri.Host, frontOptions.GamePort);
            //}

            //flush channel
            //pingTimer = new Timer();
            //pingTimer.AutoReset = true;
            //pingTimer.Interval = 33;
            //pingTimer.Enabled = true;
            //pingTimer.Elapsed += (_1, _2) =>
            //{
            //    try
            //    {
            //        ChannelManager.Instance.FlushAllSiloChannel();
            //    }
            //    catch(Exception e)
            //    {
            //        logger.Error(e);
            //    }
            //};
            // pingTimer.Start();

           // logger.Info("netty connect all silo ok!");

            //监听玩家绑定game服的事件
            frontServer.eventBindPlayerAndGameServer += async (playerId, gameServerId) =>
            {
                var gameChannel = cmGame.GetChannel(gameServerId);

                //如果找不到gameChannel，新建一个连接
                if (gameChannel == null)
                {
                    var splits = gameServerId.Split(':');
                    gameChannel = await gameClient.ConnectNettyAsync(splits[0], int.Parse(splits[1]));
                    cmGame.AddChannel(gameServerId, gameChannel);
                }

                //发送到silo
                var msg = gameChannel.Allocator.DirectBuffer();
                msg.WriteUnsignedShort((ushort)MessageType.PlayerBindGame);
                msg.WriteStringShortUtf8(playerId);
                // msg.WriteStringShortUtf8(siloId);
                gameChannel.WriteAndFlushAsync(msg);
            };

            await frontServer.Start(frontOptions.FrontPort);      
        }

        public async Task Stop()
        {         
            //if(pingTimer != null)
            //{
            //    pingTimer.Stop();
            //}

            await frontServer.Fini();
            await gameClient.Fini();
        }
    }
}
