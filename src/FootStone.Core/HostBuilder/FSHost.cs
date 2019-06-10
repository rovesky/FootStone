using System;
using System.Threading;
using System.Threading.Tasks;
using Orleans.Hosting;

namespace FootStone.Core
{
    internal class FSHost : IFSHost
    {
        private ISiloHost siloHost;

        public FSHost(ISiloHost siloHost)
        {
            this.siloHost = siloHost;
        }

        public IServiceProvider Services => siloHost.Services;

        public Task Stopped => siloHost.Stopped;

        public void Dispose()
        {
            siloHost.Dispose();
        }

        public async Task StartAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
             await siloHost.StartAsync(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            await siloHost.StopAsync(cancellationToken);
        }
    }
}