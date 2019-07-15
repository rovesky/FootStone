using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NLog;
using Orleans;
using System.Threading;
using System.Threading.Tasks;

namespace FootStone.FrontIce
{
    public class IceFrontHostedService : IHostedService
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private FrontServer network = new FrontServer();

        public IceFrontHostedService(IOptions<IceOptions> options,IClusterClient client)          
        {
            logger.Info("IceFrontService Init!");         
            network.Init(options.Value,client);          
        }     

        public Task StartAsync(CancellationToken cancellationToken)
        {
            network.Start();
            logger.Info("IceFrontService Started!");
            return Task.CompletedTask;
        }             

        public Task StopAsync(CancellationToken cancellationToken)
        {
            network.Stop();
            logger.Info("IceFrontService Stopped!");
            return Task.CompletedTask;
        }
    }

}
