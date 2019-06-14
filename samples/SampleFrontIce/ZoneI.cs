using FootStone.Core;
using FootStone.Core.GrainInterfaces;
using FootStone.FrontIce;
using FootStone.GrainInterfaces;
using Ice;
using Orleans.Streams;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SampleFrontIce
{
    //internal class StreamObserver : IAsyncObserver<byte[]>
    //{
    //    private string  name;
    //    private IZonePushPrx push;
    //    private int count = 0;


    //    public StreamObserver(IZonePushPrx push, string name)
    //    {
    //        this.push = push;
    //        this.name = name;
    //    }

    //    public Task OnCompletedAsync()
    //    {
    //        Console.Out.WriteLine(name + " receive completed");
    //        return Task.CompletedTask;
    //    }

    //    public Task OnErrorAsync(System.Exception ex)
    //    {
    //        Console.Out.WriteLine(name + " receive error:" + ex.Message);
    //        return Task.CompletedTask;
    //    }

    //    public Task OnNextAsync(byte[] item, StreamSequenceToken token = null)
    //    {

           
    //        //if (Global.ZoneMsgCount % 330000 == 0)
    //        //{
    //        //   Console.Out.WriteLine("zone msg count:" + Global.ZoneMsgCount);
    //        //}
    //        //  count++;
    //     //   Global.ZoneMsgCount++;

           
    //        //Console.Out.WriteLine(" receive bytes:" + item.Length);
    //        //   push.begin_ZoneSync(item);
    //        return Task.CompletedTask;
    //    }
    //}

    public class ZoneI : IZoneDisp_, IServantBase
    {
        private SessionI sessionI;

        public object GrainFactory { get; private set; }

        private IZoneGrain zoneGrain;

        public string GetFacet()
        {
            return "zone";
        }

        public void setSessionI(SessionI sessionI)
        {
            this.sessionI = sessionI;
        }
        public void Dispose()
        {

            if (zoneGrain != null)
            {
                zoneGrain.PlayerLeave(this.sessionI.PlayerId);
            }
        }

        //public async Task AddObserver(Guid zoneId)
        //{

        //  //  Console.Out.WriteLine("add zonePush:" + sessionI.Account);
        //    try
        //    {
        //        // var t1 = Task.Run(async () => {
        //        IZonePushPrx push = (IZonePushPrx)IZonePushPrxHelper.uncheckedCast(sessionI.SessionPushPrx, "zonePush").ice_oneway();
        //       // var connection = await push.ice_getConnectionAsync();

        //        var streamProvider = Global.OrleansClient.GetStreamProvider("Zone");

        //        var playerStream = streamProvider.GetStream<byte[]>(sessionI.PlayerId, "ZonePlayer");
        //        var zoneStream = streamProvider.GetStream<byte[]>(zoneId, "Zone");

        //        zoneStreamHandler = await zoneStream.SubscribeAsync(new StreamObserver(push,"zone"));
        //        playerStreamHandler = await playerStream.SubscribeAsync(new StreamObserver(push,"player"));

        //        //  });
        //        //  await t1;
        //    }
        //    catch (System.Exception e)
        //    {
        //        Console.Error.WriteLine("zonePush:" + e.Message);
        //    }
        //}      

        public async override Task<EndPointZone> BindZoneAsync(string zoneId, string playerId, Current current = null)
        {
            var zoneGuid = Guid.Parse(zoneId);
            zoneGrain = Global.OrleansClient.GetGrain<IZoneGrain>(zoneGuid);

            var ret = await zoneGrain.GetEndPoint();

            sessionI.Bind("zoneId", zoneId);
            return ret;
        }

        public async override Task PlayerEnterAsync(Current current = null)
        {
            await zoneGrain.PlayerEnter(sessionI.PlayerId);
        }

        public async override Task PlayerLeaveAsync(Current current = null)
        {
            await zoneGrain.PlayerLeave(sessionI.PlayerId);
        }        
    }
}
