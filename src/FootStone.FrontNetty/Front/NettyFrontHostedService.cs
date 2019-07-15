using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Threading;
using System.Threading.Tasks;

namespace FootStone.FrontNetty
{
    public class NettyFrontHostedService : IHostedService
    {
       
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private NettyFrontService service = new NettyFrontService();

        public NettyFrontHostedService(IOptions<NettyFrontOptions> options)
        {
            service.Init(options.Value);
        }    
             

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await service.Start();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await service.Stop();           
        }
    }
}
