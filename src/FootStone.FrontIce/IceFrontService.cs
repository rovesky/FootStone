using FootStone.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NLog;
using Orleans.Concurrency;
using System;
using System.Threading.Tasks;

namespace FootStone.FrontIce
{
    [Reentrant]
    public class IceFrontService :IClientService
    {
        private NLog.Logger logger = LogManager.GetCurrentClassLogger();
        private NetworkIce network = new NetworkIce();

        public IceFrontService()          
        {

        }

        public  Task Init(IServiceProvider serviceProvider)
        {
            logger.Info("IceFrontService Init!");
            var options =  serviceProvider.GetService<IOptions<IceOptions>>().Value;
            network.Init(options);
            return Task.CompletedTask;
        }

        public  Task Start()
        {
            network.Start();      
            logger.Info("IceFrontService Started!");
            return Task.CompletedTask;
        }

        public  Task Stop()
        {        
            network.Stop();
            logger.Info("IceFrontService Stopped!");
            return Task.CompletedTask;
        }
    }

}
