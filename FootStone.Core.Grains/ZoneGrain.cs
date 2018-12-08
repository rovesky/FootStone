using FootStone.Core.GrainInterfaces;
using FootStone.GrainInterfaces;
using Orleans;
using Orleans.Providers;
using Orleans.Runtime;
using Orleans.Streams;
using Pomelo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootStone.Grains
{
    
    class ZonePlayer
    {
        public Guid id;
        public IAsyncStream<byte[]> stream;
      

        public ZonePlayer(Guid id, IAsyncStream<byte[]> stream)
        {
            this.id = id;
            this.stream = stream;
        }
    }
 
    public  class ZoneGrain : Grain, IZoneGrain
    {

       // readonly ISocketServiceClient SocketServiceClient;
     
       
        private Dictionary<Guid, ZonePlayer> players = new Dictionary<Guid, ZonePlayer>();
        private IStreamProvider streamProvider;
        private IAsyncStream<byte[]> zoneStream;

        //public ZoneGrain(IGrainActivationContext grainActivationContext, ISocketServiceClient socketServiceClient)
        //{
        //    SocketServiceClient = socketServiceClient;
        //}


        public override Task OnActivateAsync()
        {
            streamProvider = GetStreamProvider("Zone");
            zoneStream = streamProvider.GetStream<byte[]>(this.GetPrimaryKey(), "Zone");
            int size = 200;
            var bytes = new byte[size];
            for (int i = 0; i < size; ++i) 
            {
                bytes[i] = 0x01;
            }
            RegisterTimer((s) =>
                 {

                     try
                     {
                         zoneStream.OnNextAsync(bytes);
                         int i = 0;
                         foreach (ZonePlayer player in players.Values)
                         {
                             // Console.Out.WriteLine(player.id+" send msg!");
                             if (i % 10 == 0)
                             {
                                 player.stream.OnNextAsync(bytes);
                             }
                             i++;
                         }
                     }
                     catch(Exception e)
                     {
                         Console.Error.WriteLine(e.Message);
                     }
                    
                     return Task.CompletedTask;
                 }
                 , null
                 , TimeSpan.FromMilliseconds(33)
                 , TimeSpan.FromMilliseconds(33));

            return Task.CompletedTask;
        }

        public override Task OnDeactivateAsync()
        {
    //        subscribers.Clear();
            return Task.CompletedTask;
        }

        public Task PlayerEnter(Guid playerId)
        {          
            var stream = streamProvider.GetStream<byte[]>(playerId, "ZonePlayer");
            players.Add(playerId, new ZonePlayer(playerId, stream));
            Console.Out.WriteLine(this.GetPrimaryKey() + " zone player count:" + players.Count);
            return Task.CompletedTask;
        }

        public Task PlayerLeave(Guid playerId)
        {
            players.Remove(playerId);
            return Task.CompletedTask;
        }
    }
}
