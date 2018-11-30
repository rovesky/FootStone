using FootStone.Core.FrontIce;
using FootStone.Core.GrainInterfaces;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Concurrency;
using Orleans.Core;
using Orleans.Runtime;
using Orleans.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FootStone.Core.GameServer
{
    [Reentrant]
    public class IceService : GrainService,IIceService
    {
        readonly IGrainFactory GrainFactory;

        private NetworkIce network = new NetworkIce();

        public IceService(IServiceProvider services, IGrainIdentity id, Silo silo, ILoggerFactory loggerFactory, IGrainFactory grainFactory) : base(id, silo, loggerFactory)
        {         
            GrainFactory = grainFactory;
        }
        public override Task Init(IServiceProvider serviceProvider)
        {
            Console.WriteLine("----------IceService Init!");
           
            network.Init(Global.Instance.MainArgs);
            return base.Init(serviceProvider);
        }

        public override async Task Start()
        {
            Console.WriteLine("-----------IceService Start!");
            network.Start();
            await base.Start();
        }

        public override Task Stop()
        {
            network.Stop();
            return base.Stop();
        }
    }
}
