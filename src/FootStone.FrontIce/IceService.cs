using FootStone.Core.GrainInterfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Concurrency;
using Orleans.Core;
using Orleans.Runtime;
using Orleans.Services;
using Orleans.Streams;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FootStone.Core.FrontIce
{
    [Reentrant]
    public class IceService : GrainService, IIceService
    {
        readonly IGrainFactory GrainFactory;

        private NetworkIce network = new NetworkIce();

        public IceService(IServiceProvider services, IGrainIdentity id, Silo silo, ILoggerFactory loggerFactory, IGrainFactory grainFactory)
            : base(id, silo, loggerFactory)
        {
            GrainFactory = grainFactory;
        }

        public override Task Init(IServiceProvider serviceProvider)
        {
            
            Console.WriteLine("----------IceService Init!");
            var options =  serviceProvider.GetService<IOptions<IceOptions>>().Value;

            var servants = serviceProvider.GetServices<IServantBase>();

            network.Init(options.ConfigFile, servants);

            return base.Init(serviceProvider);
        }

        public override async Task Start()
        {

            Console.WriteLine("-----------IceService Start!");
            await base.Start();
        }

        public override Task Stop()
        {
            Console.WriteLine("-----------IceService Stop!");

            network.Stop();
            return base.Stop();
        }
    }

}
