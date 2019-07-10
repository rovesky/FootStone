﻿using FootStone.Core;
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
            return typeof(IZonePrx).Name;
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
            await zoneGrain.PlayerEnter(sessionI.PlayerId, current.adapter.GetHashCode().ToString());
        }

        public async override Task PlayerLeaveAsync(Current current = null)
        {
            await zoneGrain.PlayerLeave(sessionI.PlayerId);
        }        
    }
}
