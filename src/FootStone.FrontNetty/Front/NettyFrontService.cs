using FootStone.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NLog;
using System;
using System.Threading.Tasks;

namespace FootStone.FrontNetty
{
    public class NettyFrontService : IClientService
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private FrontServer frontServer = new FrontServer();
        private GameClient  gameClient = new GameClient();

        private IChannelManager frontChannels = new ChannelManager();
        private IChannelManager gameChannels = new ChannelManager();

        private NettyFrontOptions frontOptions; 


        public Task Init(IServiceProvider serviceProvider)
        {
            frontOptions = serviceProvider.GetService<IOptions<NettyFrontOptions>>().Value;

            var channelManagers = new IChannelManager[2];
            channelManagers[0] = frontChannels;
            channelManagers[1] = gameChannels;

            frontServer.Init(channelManagers);
            gameClient.Init(channelManagers);
    
            return Task.CompletedTask;
        }

        public async Task Start()
        {

            //监听事件:玩家绑定game服
            frontServer.eventBindPlayerAndGameServer += async (playerId, gameServerId) =>
            {
                var gameChannel = gameChannels.GetChannel(gameServerId);

                //如果找不到game channel，新建一个连接
                if (gameChannel == null)
                {
                    gameChannel = await gameClient.ConnectGameServerAsync(gameServerId);
                    gameChannels.AddChannel(gameServerId, gameChannel);
                }

                //发送到GameServer
                var msg = gameChannel.Allocator.DirectBuffer();
                msg.WriteUnsignedShort((ushort)MessageType.PlayerBindGame);
                msg.WriteStringShortUtf8(playerId);        
                gameChannel.WriteAndFlushAsync(msg);
            };

            await frontServer.Start(frontOptions.FrontPort);      
        }

        public async Task Stop()
        {        
          
            await frontServer.Fini();
            await gameClient.Fini();
        }
    }
}
