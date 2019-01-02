using FootStone.Core.FrontIce;
using FootStone.Core.GrainInterfaces;
using Microsoft.Extensions.Logging;
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

namespace FootStone.Core.GameServer
{
    [Reentrant]
    public class IceService : GrainService,IIceServiceClient
    {
        readonly IGrainFactory GrainFactory;

        private NetworkIce network = new NetworkIce();
        private int operationTimes = 0;

        private int playerCount = 0;
        private List<Guid> zones = new List<Guid>();
        private bool isEnableIce = false;
        //    private IStreamProvider streamProvider;

        public IceService(IServiceProvider services, IGrainIdentity id, Silo silo, ILoggerFactory loggerFactory, IGrainFactory grainFactory) 
            : base(id, silo, loggerFactory)
        {         
            GrainFactory = grainFactory;
        }

        public Task AddOptionTime(int time)
        {
            operationTimes += time;
            return Task.CompletedTask;
        }

        //public  Task AddPlayer(Guid id)
        //{
        //    //try
        //    //{
        //    //    var t1=Task.Run( async()=>{
        //    //        streamProvider = Global.OrleansClient.GetStreamProvider("Zone");
        //    //        var stream = streamProvider.GetStream<byte[]>(id, "ZonePlayer");
        //    //        await stream.SubscribeAsync(new StreamObserver(id));
        //    //    });
        //    //    await t1;
        //    //}
        //    //catch(Exception e)
        //    //{
        //    //    Console.Error.WriteLine("AddPlayer:"+e.Message);
        //    //}
        //   return Task.CompletedTask;

        //}

        public Task<Guid> GetZone(Guid playerId)
        {          
            if(playerCount%100 == 0){
                zones.Add(Guid.NewGuid());
                Console.Out.WriteLine("create new zone,zone count:" + zones.Count);
            }
            playerCount++;
            return Task.FromResult(zones[zones.Count - 1]);
        }

        public override Task Init(IServiceProvider serviceProvider)
        {
            if (isEnableIce)
            {
                Console.WriteLine("----------IceService Init!");
                network.Init(Global.MainArgs);
            }
            return base.Init(serviceProvider);
        }

        public override async Task Start()
        {
            if (isEnableIce)
            {
                Console.WriteLine("-----------IceService Start!");
                network.Start();
            }

            RegisterTimer((s) =>
            {
                Console.Out.WriteLine("operation times:" + operationTimes);
                Console.Out.WriteLine("zone count:" + zones.Count);
                return Task.CompletedTask;
            }
              , null
              , TimeSpan.FromSeconds(10)
              , TimeSpan.FromSeconds(10));
            await base.Start();
        }

        public override Task Stop()
        {
           // this.Silo
            if (isEnableIce)
            {
                network.Stop();
            }
            return base.Stop();
        }
    }

  
}
