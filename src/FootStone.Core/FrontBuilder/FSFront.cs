using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Hosting;
using Orleans.Runtime;

namespace FootStone.Core
{
    internal class FSFront : IFSFront
    {
        private IClusterClient clusterClient;

        public FSFront(IClusterClient clusterClient)
        {
            this.clusterClient = clusterClient;
        }

        public IServiceProvider Services => clusterClient.ServiceProvider;

        public IClusterClient ClusterClient => clusterClient;

        //  public Task Stopped => siloHost.Stopped;

        public void Dispose()
        {
            clusterClient.Dispose();
        }

        public async Task StartAsync(CancellationToken cancellationToken = default(CancellationToken))
        {          
            await clusterClient.Connect();

            var clientServices = Services.GetServices<IFrontService>();
            foreach (var clientService in clientServices)
            {
                await clientService.Init(Services);
                await clientService.Start();
            }

        }

        public async Task StopAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var clientServices = Services.GetServices<IFrontService>();
            foreach (var clientService in clientServices)
            {
                await clientService.Stop();
            }

            await clusterClient.Close();
        }
    }
}