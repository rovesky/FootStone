using FootStone.Core.GrainInterfaces;
using FootStone.FrontIce;
using FootStone.GrainInterfaces;
using Ice;
using Orleans;
using System;
using System.Threading.Tasks;
using System.Timers;

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
        private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private Session session;
        private IClusterClient orleansClient;      

        private IZonePushPrx zonePushPrx;
        private Timer moveTimer;

        public object GrainFactory { get; private set; }

        private IZoneGrain zoneGrain;
        private byte[] data;

        public ZoneI(Session session, IClusterClient orleansClient)
        {
            this.session = session;
            this.orleansClient = orleansClient;
        }

        public string GetFacet()
        {
            return nameof(IZonePrx);
        }
      
        public void Dispose()
        {
            if(moveTimer != null)
            {
                moveTimer.Close();
            }

            if (zoneGrain != null)
            {
                zoneGrain.PlayerLeave(this.session.PlayerId);
            }
        }
      

        public async override Task<EndPointZone> BindZoneAsync(string zoneId, string playerId, Current current = null)
        {
            var zoneGuid = Guid.Parse(zoneId);
            zoneGrain = orleansClient.GetGrain<IZoneGrain>(zoneGuid);

            var ret = await zoneGrain.GetEndPoint();

            session.Bind("zoneId", zoneId);

            zonePushPrx = session.UncheckedCastPush(IZonePushPrxHelper.uncheckedCast);
                       
            moveTimer = new System.Timers.Timer();
            moveTimer.AutoReset = true;
            moveTimer.Interval = 33;
            moveTimer.Enabled = true;
            moveTimer.Elapsed += (_1, _2) =>
            {
                if (data != null)
                {
                  //  logger.Debug("zonePushPrx.RecvData!");
                    zonePushPrx.RecvData(data);
                }
            };
            moveTimer.Start();
            return ret;
        }

        public async override Task PlayerEnterAsync(Current current = null)
        {          
            await zoneGrain.PlayerEnter(session.PlayerId, current.adapter.GetHashCode().ToString());
        }

        public async override Task PlayerLeaveAsync(Current current = null)
        {
            await zoneGrain.PlayerLeave(session.PlayerId);
        }

        public override Task SendDataAsync(byte[] data, Current current = null)
        {
            this.data = data;
            return Task.CompletedTask;
           // zonePushPrx.RecvData(data);
        }
    }
}
