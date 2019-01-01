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
        private string  name;
        private IZonePushPrx push;
        private int count = 0;


        public StreamObserver(IZonePushPrx push, string name)
        {
            this.push = push;
            this.name = name;
        }

        public Task OnCompletedAsync()
        {
            Console.Out.WriteLine(name + " receive completed");
            return Task.CompletedTask;
        }

        public Task OnErrorAsync(System.Exception ex)
        {
            Console.Out.WriteLine(name + " receive error:" + ex.Message);
            return Task.CompletedTask;
        }

        public Task OnNextAsync(byte[] item, StreamSequenceToken token = null)
        {

           
            if (Global.ZoneMsgCount % 330000 == 0)
            {
               Console.Out.WriteLine("zone msg count:" + Global.ZoneMsgCount);
            }
            //  count++;
            Global.ZoneMsgCount++;

           
            //Console.Out.WriteLine(" receive bytes:" + item.Length);
            //   push.begin_ZoneSync(item);
            return Task.CompletedTask;
        }
    }

    public class ZoneI : IZoneDisp_, IServantBase
    {
        private SessionI sessionI;
        private StreamSubscriptionHandle<byte[]> playerStreamHandler;
        private StreamSubscriptionHandle<byte[]> zoneStreamHandler;

        public object GrainFactory { get; private set; }

        public ZoneI(SessionI sessionI)
        {
            this.sessionI = sessionI;
        }

        public void Destroy()
        {
        
            // await AddObserver(zoneGuid);
            var zoneId = (string)this.sessionI.GetAttribute("zoneId");
            var zoneGrain = Global.OrleansClient.GetGrain<IZoneGrain>(Guid.Parse(zoneId));

            zoneGrain.PlayerLeave(this.sessionI.PlayerId);
         

            if (playerStreamHandler != null)
            {
                playerStreamHandler.UnsubscribeAsync();
                playerStreamHandler = null;
            }

            if (zoneStreamHandler != null)
            {
                zoneStreamHandler.UnsubscribeAsync();
                zoneStreamHandler = null;
            }
        }

        public async Task AddObserver(Guid zoneId)
        {
  
            Console.Out.WriteLine("add zonePush:" + sessionI.Account);
            try
            {
                // var t1 = Task.Run(async () => {
                IZonePushPrx push = (IZonePushPrx)IZonePushPrxHelper.uncheckedCast(sessionI.SessionPushPrx, "zonePush").ice_oneway();
               // var connection = await push.ice_getConnectionAsync();
               
                var streamProvider = Global.OrleansClient.GetStreamProvider("Zone");

                var playerStream = streamProvider.GetStream<byte[]>(sessionI.PlayerId, "ZonePlayer");
                var zoneStream = streamProvider.GetStream<byte[]>(zoneId, "Zone");

                zoneStreamHandler = await zoneStream.SubscribeAsync(new StreamObserver(push,"zone"));
                playerStreamHandler = await playerStream.SubscribeAsync(new StreamObserver(push,"player"));

                //  });
                //  await t1;
            }
            catch (System.Exception e)
            {
                Console.Error.WriteLine("zonePush:" + e.Message);
            }
        }
  

        public async override Task<EndPointZone> PlayerEnterAsync(string zoneId, Current current = null)
        {
            try
            {
                var zoneGuid = Guid.Parse(zoneId);
               // await AddObserver(zoneGuid);

                var zoneGrain = Global.OrleansClient.GetGrain<IZoneGrain>(zoneGuid);
                var ret = await zoneGrain.PlayerEnter(sessionI.PlayerId);
                this.sessionI.SetAttribute("zoneId", zoneId);
                return ret;

            }
            catch (System.Exception e)
            {
                Console.Error.WriteLine(e.Message);
                throw e;
            }
        }

        public override void Move(byte[] data, Current current = null)
        {
            throw new NotImplementedException();
        }


        //public override Task MoveAsync(byte[] data, Current current = null)
        //{
        //    throw new NotImplementedException();
        //}

        //public async override Task PlayerBindChannelAsync(string channelId, Current current = null)
        //{
        //    try
        //    {

        //        var zoneGuid = Guid.Parse((string)sessionI.GetAttribute("zoneId"));               

        //        var zoneGrain = Global.OrleansClient.GetGrain<IZoneGrain>(zoneGuid);
        //        await zoneGrain.PlayerBindChannel(sessionI.PlayerId,channelId);

        //    }
        //    catch (System.Exception e)
        //    {
        //        Console.Error.WriteLine(e.Message);
        //        throw e;
        //    }
        //}
    }
}
