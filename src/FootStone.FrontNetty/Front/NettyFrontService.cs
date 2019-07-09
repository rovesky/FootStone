using DotNetty.Transport.Channels;
using FootStone.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace FootStone.FrontNetty
{
    public class NettyFrontService : IFrontService
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private FrontServer frontServer = new FrontServer();
        private GameClient  gameClient = new GameClient();

        private IChannelManager playerChannels = new ChannelManager();    
        private ConcurrentDictionary<string, IChannel> gameChannels = new ConcurrentDictionary<string, IChannel>();
        private NettyFrontOptions frontOptions; 
        
        public Task Init(IServiceProvider serviceProvider)
        {
            frontOptions = serviceProvider.GetService<IOptions<NettyFrontOptions>>().Value;
    
            frontServer.Init(this, playerChannels);
            gameClient.Init(this,playerChannels);
    
            return Task.CompletedTask;
        }

        public async Task Start()
        {
            await frontServer.Start(frontOptions.FrontPort);      
        }

        public async Task Stop()
        {                  
            await frontServer.Fini();
            await gameClient.Fini();
        }

        public async Task<IChannel> GetGameChannelAsync(string gameServerId)
        {
            IChannel gameChannel = null;
            gameChannels.TryGetValue(gameServerId, out gameChannel);

            //如果找不到game channel，新建一个连接
            if (gameChannel == null)
            {
                gameChannel = await gameClient.ConnectGameServerAsync(gameServerId);
                gameChannels.TryAdd(gameServerId, gameChannel);
            }
            return gameChannel;
        }

        public void GameChannelRemove(string gameServerId)
        {
            IChannel gameChannel = null;
            gameChannels.TryRemove(gameServerId, out gameChannel);

            //断开所有通过该game channel的client channel
            if(gameChannel != null)
            {
                playerChannels.CloseChannelByGameChannel(gameChannel);
            }
        }
    }
}
