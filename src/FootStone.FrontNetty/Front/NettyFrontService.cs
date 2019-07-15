using DotNetty.Transport.Channels;
using NLog;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FootStone.FrontNetty
{
    public class NettyFrontService //: IFrontService
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private NettyFrontOptions frontOptions;

        private FrontServer frontServer = new FrontServer();
        private GameClient  gameClient = new GameClient();

        private IChannelManager playerChannels = new ChannelManager();    

        private ConcurrentDictionary<string, IChannel> gameChannels = new ConcurrentDictionary<string, IChannel>();
        private HashSet<string> gameCreating = new HashSet<string>();

        public Task Init(NettyFrontOptions frontOptions)
        {
            //  frontOptions = serviceProvider.GetService<IOptions<NettyFrontOptions>>().Value;

            this.frontOptions = frontOptions;
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

            //如果找不到game channel
            if (gameChannel == null )
            {
                //如果还没有创建出该连接，开始创建连接
                if (!gameCreating.Contains(gameServerId))
                {
                    gameCreating.Add(gameServerId);
                    gameChannel = await gameClient.ConnectGameServerAsync(gameServerId);
                    gameChannels.TryAdd(gameServerId, gameChannel);
                    gameCreating.Remove(gameServerId);
                }
                //如果已经在创建连接，开始不停尝试获取该连接
                else
                {
                    var time = 0;
                    do
                    {
                        gameChannels.TryGetValue(gameServerId, out gameChannel);
                        await Task.Delay(10);
                        time++;
                    }
                    while (gameChannel == null && time < 10);
                }
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
