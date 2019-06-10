using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Hosting;
using Orleans.Runtime;

namespace FootStone.Core
{
    internal class FSClient : IFSClient
    {
        private IClusterClient clusterClient;

        public FSClient(IClusterClient clusterClient)
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
            var clientServices = Services.GetServices<IClientService>();
            foreach (var clientService in clientServices)
            {
                await clientService.Init(Services);
                await clientService.Start();
            }

            await clusterClient.Connect();
        }

        public async Task StopAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var clientServices = Services.GetServices<IClientService>();
            foreach (var clientService in clientServices)
            {
                await clientService.Stop();
            }

            await clusterClient.Close();
        }
    }
}