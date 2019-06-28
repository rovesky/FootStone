using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Concurrency;
using Orleans.Core;
using Orleans.Runtime;
using System;
using System.Threading.Tasks;

namespace FootStone.FrontNetty
{
    [Reentrant]
    public class NettyFrontGrainService : GrainService, INettyService
    {
        private readonly IGrainFactory GrainFactory;

        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private NettyFrontService service = new NettyFrontService();

        public NettyFrontGrainService(IServiceProvider services, IGrainIdentity id, Silo silo, ILoggerFactory loggerFactory, IGrainFactory grainFactory)
            : base(id, silo, loggerFactory)
        {
            GrainFactory = grainFactory;
        }


        public override Task Init(IServiceProvider serviceProvider)
        {
           // logger.Info("Front DotNetty Service Init!");
   
            service.Init(serviceProvider);
            return base.Init(serviceProvider);
        }

        public async override Task Start()
        {
          //  logger.Info("Game DotNetty Service Start!");

            await service.Start();
            await base.Start();
        }

        public async override Task Stop()
        {
          //  logger.Info("Game DotNetty Service Stop!");
            await service.Stop();
            await base.Stop();
        }
    }
}
