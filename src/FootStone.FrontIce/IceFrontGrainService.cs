using FootStone.Core.GrainInterfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog;
using Orleans.Concurrency;
using Orleans.Core;
using Orleans.Runtime;
using System;
using System.Threading.Tasks;

namespace FootStone.FrontIce
{
    [Reentrant]
    public class IceFrontGrainService : GrainService, IIceService
    {
        private NLog.Logger logger = LogManager.GetCurrentClassLogger();
        private FrontServer network = new FrontServer();

        public IceFrontGrainService(IServiceProvider services, IGrainIdentity id, Silo silo, ILoggerFactory loggerFactory)
            : base(id, silo, loggerFactory)
        {

        }

        public override Task Init(IServiceProvider serviceProvider)
        {
            logger.Info("IceService Init!");
            var options =  serviceProvider.GetService<IOptions<IceOptions>>().Value;
            network.Init(options);

            return base.Init(serviceProvider);
        }

        public override async Task Start()
        {       
            await base.Start();
            logger.Info("IceService Started!");
        }

        public override Task Stop()
        {        
            network.Stop();
            logger.Info("IceService Stopped!");
            return base.Stop();
        }
    }

}
