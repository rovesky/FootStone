using FootStone.Core.GrainInterfaces;
using FootStone.GrainInterfaces;
using Ice;
using Orleans.Streams;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FootStone.Core.FrontIce
{
    internal class StreamObserver : IAsyncObserver<byte[]>
    {
        private Guid id;
        private IZonePushPrx push;

        public StreamObserver(Guid id)
        {
            this.id = id;
        }

        public StreamObserver(IZonePushPrx push)
        {
            this.push = push;
        }

        public Task OnCompletedAsync()
        {
            Console.Out.WriteLine(id + " receive completed");
            return Task.CompletedTask;
        }

        public Task OnErrorAsync(System.Exception ex)
        {
            Console.Out.WriteLine(id + " receive error:" + ex.Message);
            return Task.CompletedTask;
        }

        public Task OnNextAsync(byte[] item, StreamSequenceToken token = null)
        {
            Console.Out.WriteLine(id + " receive bytes:" + item.Length);
            push.begin_ZoneSync(item);
            return Task.CompletedTask;
        }
    }

    public class ZoneI : IZoneDisp_, IServantBase
    {
        private SessionI sessionI;
        private StreamSubscriptionHandle<byte[]> streamHandler;

        public object GrainFactory { get; private set; }

        public ZoneI(SessionI sessionI)
        {
            this.sessionI = sessionI;
        }

        public void Destroy()
        {
            if(streamHandler != null)
            {
                streamHandler.UnsubscribeAsync();
                streamHandler = null;
            }             
        }

        public async Task AddObserver()
        {
  
            Console.Out.WriteLine("add zonePush:" + sessionI.Account);
            try
            {
                // var t1 = Task.Run(async () => {
                IZonePushPrx push = (IZonePushPrx)IZonePushPrxHelper.uncheckedCast(sessionI.SessionPushPrx, "zonePush").ice_oneway();

                var streamProvider = Global.OrleansClient.GetStreamProvider("Zone");

                var stream = streamProvider.GetStream<byte[]>(sessionI.PlayerId, "ZonePlayer");
             
                streamHandler = await stream.SubscribeAsync(new StreamObserver(push));
                //  });
                //  await t1;
            }
            catch (System.Exception e)
            {
                Console.Error.WriteLine("zonePush:" + e.Message);
            }
        }
        public override void Move(byte[] data, Current current = null)
        {
            throw new NotImplementedException();
        }

        public override async void PlayerEnter(string zoneId, Current current = null)
        {
            try
            {
                await AddObserver();

                var zoneGrain = Global.OrleansClient.GetGrain<IZoneGrain>(Guid.Parse(zoneId));

                await zoneGrain.PlayerEnter(sessionI.PlayerId);
                
            }
            catch (System.Exception e)
            {
                Console.Error.WriteLine(e.Message);
            }
        }
    }
}
