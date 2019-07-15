using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using Orleans;

namespace FootStone.Core
{
    public class ClusterClientOptions
    {
       public   Action<IClientBuilder> configureDelegate { get; set; }
    }

    public class ClusterClientHostedService : IHostedService
    {
        //   private readonly ILogger<ClusterClientHostedService> _logger;
     

        private static Logger logger = LogManager.GetCurrentClassLogger();

        public ClusterClientHostedService(IClientBuilder clientBuilder)
        {
         
            Client = clientBuilder.Build();

            logger.Debug("client:" + Client);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            logger.Info("Connecting...");

            var retries = 100;
            await Client.Connect(async error =>
            {
                if (--retries < 0)
                {
                    logger.Error("Could not connect to the cluster: {@Message}", error.Message);
                    return false;
                }
                else
                {
                    logger.Warn(error, "Error Connecting: {@Message}", error.Message);
                }

                try
                {
                    await Task.Delay(5000, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    return false;
                }

                return true;
            });

            logger.Info("Connected.");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            var cancellation = new TaskCompletionSource<bool>();
            cancellationToken.Register(() => cancellation.TrySetCanceled(cancellationToken));

            return Task.WhenAny(Client.Close(), cancellation.Task);
        }

        public IClusterClient Client { get; }
    }
}
