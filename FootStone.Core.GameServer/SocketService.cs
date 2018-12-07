using FootStone.Core.FrontIce;
using FootStone.Core.GrainInterfaces;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Concurrency;
using Orleans.Core;
using Orleans.Runtime;
using Orleans.Services;
using Orleans.Streams;
using Pomelo;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FootStone.Core.GameServer
{
    [Reentrant]
    public class SocketService : GrainService,ISocketServiceClient
    {
        readonly IGrainFactory GrainFactory;

      //  private NetworkIce network = new NetworkIce();
        private int operationTimes = 0;
        private IStreamProvider streamProvider;

        public SocketService(IServiceProvider services, IGrainIdentity id, Silo silo, ILoggerFactory loggerFactory, IGrainFactory grainFactory) 
            : base(id, silo, loggerFactory)
        {         
            GrainFactory = grainFactory;
        }

        public Task AddOptionTime(int time)
        {
            operationTimes += time;
            return Task.CompletedTask;
        }

        public override Task Init(IServiceProvider serviceProvider)
        {
            Console.WriteLine("----------IceService Init!");
          //  FastStreamConfig fastStreamConfig = new FastStreamConfig();
            //启动FastStream
         //   FastStream.init(class_config.Get("faststream"));
           
            //  network.Init(Global.Instance.MainArgs);
            return base.Init(serviceProvider);
        }

        public override  Task Start()
        {
            streamProvider = Global.OrleansClient.GetStreamProvider("Zone");
            // FastStream.instance().Start();
            return base.Start();
        }

        public override Task Stop()
        {
          //  FastStream.instance().Stop();
            return base.Stop();
        }
    }
}
