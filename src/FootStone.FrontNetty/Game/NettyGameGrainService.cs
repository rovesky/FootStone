using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog;
using Orleans;
using Orleans.Concurrency;
using Orleans.Core;
using Orleans.Runtime;
using System;
using System.Threading.Tasks;

namespace FootStone.FrontNetty
{
    [Reentrant]
    public class NettyGameGrainService : GrainService, INettyService
    {
        private readonly IGrainFactory GrainFactory;

        private NLog.Logger logger = LogManager.GetCurrentClassLogger();

        private GameServer server = new GameServer();

        public NettyGameGrainService(IServiceProvider services, IGrainIdentity id, Silo silo, ILoggerFactory loggerFactory, IGrainFactory grainFactory)
            : base(id, silo, loggerFactory)
        {
            GrainFactory = grainFactory;
        }

        public NettyGameOptions options { get; private set; }

        public override Task Init(IServiceProvider serviceProvider)
        {
            logger.Info("Game DotNetty Service Init!");

            options = serviceProvider.GetService<IOptions<NettyGameOptions>>().Value;
            server.Init(options.Recv);

            return base.Init(serviceProvider);
        }

        public async override Task Start()
        {
            logger.Info("Game DotNetty Service Start!");

            await server.Start(options.Port);

            await base.Start();
        }

        public async override Task Stop()
        {
            logger.Info("Game DotNetty Service Stop!");
            await server.Stop();

            await base.Stop();
        }
    }
}
