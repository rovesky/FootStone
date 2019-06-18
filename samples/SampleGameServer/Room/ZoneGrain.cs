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

namespace FootStone.Core
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
        private NLog.Logger logger = NLog.LogManager.GetLogger("FootStone.Core.ZoneGrain");
        private Dictionary<Guid, ZonePlayer> players = new Dictionary<Guid, ZonePlayer>();

        private Random random = new Random();
        public ZoneGrain(IGrainActivationContext grainActivationContext)
        {
            SiloAddressInfo = grainActivationContext.GrainType.GetProperty("SiloAddress", BindingFlags.Instance | BindingFlags.NonPublic);
        
        }

        public PropertyInfo SiloAddressInfo { get; }


        public byte[] randomBytes(int size)
        {
           // int size = random.Next() % maxSize;
            var bytes = new byte[size];
            for (int i = 0; i < size; ++i)
            {
                bytes[i] = (byte)i;
            }
            return bytes;
        }

        public override Task OnActivateAsync()
        {
            List<byte[]> datas = new List<byte[]>();
            for (int i = 0; i < 100; ++i) {
                datas.Add(randomBytes(i));
            }

            RegisterTimer((s) =>
                 {
                     try
                     {
                         int i = 0;
                         foreach (ZonePlayer player in players.Values)
                         {
                           var size = random.Next() % 200;
                             if (size < 100)
                             {
                                 var bytes = datas[size];
                                 ChannelManager.Instance.Send(player.id.ToString(), bytes);
                             }
                             i++;
                         }
                     }
                     catch (Exception e)
                     {
                         logger.Error(e);
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
       

        public Task<EndPointZone> GetEndPoint()
        {
            var siloAddress = (SiloAddress)SiloAddressInfo.GetValue(this);

            return Task.FromResult(new EndPointZone(
              siloAddress.Endpoint.Address.ToString(),
              8007));
        }


        public Task PlayerEnter(Guid playerId)
        {
            logger.Debug($"zone {this.GetPrimaryKey().ToString()} ,PlayerEnter:{playerId.ToString()} ,zone player count:{ players.Count}");

            if (!players.ContainsKey(playerId))
                players.Add(playerId, new ZonePlayer(playerId, null)); 

            return Task.CompletedTask;
        }

        public Task PlayerLeave(Guid playerId)
        {
            logger.Debug($"zone {this.GetPrimaryKey().ToString()} ,PlayerLeave:{playerId.ToString()} ,zone player count:{ players.Count}");

            players.Remove(playerId);
            return Task.CompletedTask;
        }

        public Task<int> GetPlayerCount()
        {
            return Task.FromResult(players.Count);
        }
    }
}
