using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NLog;
using System.Threading;
using System.Threading.Tasks;

namespace FootStone.FrontNetty
{

    public class NettyGameHostedService : IHostedService
    {
        private NLog.Logger logger = LogManager.GetCurrentClassLogger();
        private GameServer server = new GameServer();      

        public NettyGameOptions options { get; private set; }

        public NettyGameHostedService(IOptions<NettyGameOptions> options)
        {
            this.options = options.Value;          

            server.Init(this.options.Recv);
            logger.Info("Game DotNetty Service Inited!");
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {

            await server.Start(options.Port);
            logger.Info("Game DotNetty Service Started!");

        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {           
            await server.Stop();
            logger.Info("Game DotNetty Service Stopped!");
        }
    }
}
