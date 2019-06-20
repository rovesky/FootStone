using FootStone.Core.GrainInterfaces;
using FootStone.GrainInterfaces;
using Orleans;
using Orleans.Core;
using Orleans.Providers;
using Orleans.Runtime;
using Orleans.Streams;
using SampleGameServer.Room;
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
        public Guid  id;
        public IPlayerChannel channel;     

        public ZonePlayer(Guid id, IPlayerChannel channel)
        {
            this.id = id;
            this.channel = channel;      
        }
    }
    public class ZoneState
    {
        string id;
    }
 
    public  class ZoneGrain : Grain, IZoneGrain,IZoneStream
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
                                 player.channel.Send(bytes);
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
              siloAddress.Endpoint.Port));
        }


        public Task PlayerEnter(Guid playerId)
        {
            logger.Debug($"zone {this.GetPrimaryKey().ToString()} ,PlayerEnter:{playerId.ToString()} ,zone player count:{ players.Count}");

            if (!players.ContainsKey(playerId))
            {
             //   IPlayerChannel channel = ChannelManager.Instance.GetChannel(playerId.ToString());
             //   players.Add(playerId, new ZonePlayer(playerId, channel));
            }

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

        public void Send(byte[] data)
        {
            throw new NotImplementedException();
        }

        public void Recv(byte[] data)
        {
            throw new NotImplementedException();
        }
    }
}
