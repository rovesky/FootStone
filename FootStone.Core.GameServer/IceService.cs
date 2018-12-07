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
        private IStreamProvider streamProvider;

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

        public async Task AddPlayer(Guid id)
        {
            try
            {
                var t1=Task.Run( async()=>{
                    streamProvider = Global.OrleansClient.GetStreamProvider("Zone");
                    var stream = streamProvider.GetStream<byte[]>(id, "ZonePlayer");
                    await stream.SubscribeAsync(new StreamObserver(id));
                });
                await t1;
            }
            catch(Exception e)
            {
                Console.Error.WriteLine("AddPlayer:"+e.Message);
            }
        //    return Task.CompletedTask;

        }

        public override Task Init(IServiceProvider serviceProvider)
        {
            Console.WriteLine("----------IceService Init!");
           
            network.Init(Global.MainArgs);
            return base.Init(serviceProvider);
        }

        public override async Task Start()
        {
            Console.WriteLine("-----------IceService Start!");
          
            network.Start();
            RegisterTimer((s) =>
            {
                Console.Out.WriteLine("operation times:" + operationTimes);
                return Task.CompletedTask;
            }
              , null
              , TimeSpan.FromSeconds(10)
              , TimeSpan.FromSeconds(10));
            await base.Start();
        }

        public override Task Stop()
        {
            network.Stop();
            return base.Stop();
        }
    }

    internal class StreamObserver : IAsyncObserver<byte[]>
    {
        private Guid id;

        public StreamObserver(Guid id)
        {
            this.id = id;
        }

        public Task OnCompletedAsync()
        {
            Console.Out.WriteLine(id + " receive completed");
            return Task.CompletedTask;
        }

        public Task OnErrorAsync(Exception ex)
        {
            Console.Out.WriteLine(id + " receive error:" + ex.Message);
            return Task.CompletedTask;
        }

        public Task OnNextAsync(byte[] item, StreamSequenceToken token = null)
        {
            Console.Out.WriteLine(id + " receive bytes:"+item.Length);
            return Task.CompletedTask;
        }
    }
}
