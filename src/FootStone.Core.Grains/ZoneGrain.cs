using FootStone.Core.GrainInterfaces;
using FootStone.GrainInterfaces;
using Orleans;
using Orleans.Core;
using Orleans.Providers;
using Orleans.Runtime;
using Orleans.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FootStone.Core.Grains
{

    class ZonePlayer
    {
        public Guid id;
        public string channelId;
        public IAsyncStream<byte[]> stream;


        public ZonePlayer(Guid id, IAsyncStream<byte[]> stream)
        {
            this.id = id;
            this.stream = stream;
        }
    }
    public class ZoneState
    {
        string id;
    }
 
    public  class ZoneGrain : Grain, IZoneGrain
    {

       // readonly ISocketServiceClient SocketServiceClient;
     
       
        private Dictionary<Guid, ZonePlayer> players = new Dictionary<Guid, ZonePlayer>();
        private IStreamProvider streamProvider;
        private IAsyncStream<byte[]> zoneStream;


        //public ZoneGrain(IGrainIdentity identity, IGrainRuntime runtime)
        //  : base(identity, runtime)
        //{

        //    this.Runtime1 = runtime;

        //}

        //public IGrainRuntime Runtime1 { get; }

        public ZoneGrain(IGrainActivationContext grainActivationContext, INettyServiceClient socketServiceClient)
        {
            SiloAddressInfo = grainActivationContext.GrainType.GetProperty("SiloAddress", BindingFlags.Instance | BindingFlags.NonPublic);
          
            SocketServiceClient = socketServiceClient;
        }

        public PropertyInfo SiloAddressInfo { get; }
       // public SiloAddress SiloAddress1 { get; set; }
        public INettyServiceClient SocketServiceClient { get; }

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
                        // zoneStream.OnNextAsync(bytes);
                         int i = 0;
                         foreach (ZonePlayer player in players.Values)
                         {
                             ChannelManager.Instance.Send(player.id.ToString(), bytes);
                             i++;
                         }
                     }
                     catch(Exception e)
                     {
                       //  Console.Error.WriteLine(e.Message);
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
            return Task.CompletedTask;
        }

        //public Task PlayerBindChannel(Guid playerId, string channelId)
        //{
        //    if (!players.ContainsKey(playerId))
        //    {
        //        throw new Exception("");
        //    }

        //    var player = players[playerId];
        //    player.channelId = channelId;
        //    return Task.CompletedTask;
        //}

        public Task<EndPointZone> PlayerEnter(Guid playerId)
        {          
            var stream = streamProvider.GetStream<byte[]>(playerId, "ZonePlayer");
            try
            {
                players.Add(playerId, new ZonePlayer(playerId, stream));
            }
            catch(Exception e)
            {

            }

            var siloAddress = (SiloAddress)SiloAddressInfo.GetValue(this);

            Console.Out.WriteLine("zone silo addr:"+siloAddress.Endpoint.Address);
          
            //   Console.Out.WriteLine(this.GetPrimaryKey() + " zone player count:" + players.Count);

            return Task.FromResult(new EndPointZone(
                siloAddress.Endpoint.Address.ToString(),
                8007));
        }

        public Task PlayerLeave(Guid playerId)
        {
            Console.Out.WriteLine("zone PlayerLeave:" + playerId.ToString());

            players.Remove(playerId);
            return Task.CompletedTask;
        }
    }
}
