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
    public class NettyGameService : GrainService, INettyService
    {
        private readonly IGrainFactory GrainFactory;

        private NLog.Logger logger = LogManager.GetCurrentClassLogger();

        private GameServerNetty server = new GameServerNetty();

        public NettyGameService(IServiceProvider services, IGrainIdentity id, Silo silo, ILoggerFactory loggerFactory, IGrainFactory grainFactory)
            : base(id, silo, loggerFactory)
        {
            GrainFactory = grainFactory;
        }

        public NettyOptions options { get; private set; }

        public override Task Init(IServiceProvider serviceProvider)
        {
            logger.Info("Game DotNetty Service Init!");

            options = serviceProvider.GetService<IOptions<NettyOptions>>().Value;
            server.Init();

            return base.Init(serviceProvider);
        }

        public async override Task Start()
        {
            logger.Info("Game DotNetty Service Start!");

            await server.Start(options.GamePort);

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
