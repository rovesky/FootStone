using FootStone.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NLog;
using Orleans.Concurrency;
using System;
using System.Threading.Tasks;

namespace FootStone.FrontIce
{
    public class IceFrontService :IFrontService
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private FrontServer network = new FrontServer();

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
