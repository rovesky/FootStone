using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using FootStone.Core.GrainInterfaces;
using FootStone.FrontNetty;
using FootStone.GrainInterfaces;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;

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
using System.Threading;
using System.Collections.Concurrent;

namespace FootStone.Core
{

    class ZonePlayer
    {
        public Guid  id;
        public IChannel channel;     

        public ZonePlayer(Guid id, IChannel channel)
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

        private int nettyPort;
        private Random random = new Random();


        private ConcurrentQueue<byte[]> msgQueue = new ConcurrentQueue<byte[]>();

       

        public ZoneGrain(IGrainActivationContext grainActivationContext)
        {  
         
            SiloAddressInfo = grainActivationContext.GrainType.GetProperty("SiloAddress",
                BindingFlags.Instance | BindingFlags.NonPublic);        
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
            var options = ServiceProvider.GetService<IOptions<NettyGameOptions>>().Value;
            nettyPort = options.Port;
 
            RegisterTimer((s) =>
                 {
                     try
                     {

                         if (msgQueue.Count > 0)
                         {
                             List<byte[]> datas = msgQueue.ToList();
                             msgQueue.Clear();
                         
                             IChannel channel = null;

                             foreach (ZonePlayer player in players.Values)
                             {
                                 //var size = random.Next() % 200;
                                 //  if (size < 100)
                                 //  {

                                 channel = player.channel;
                                 //Ìí¼Ó°üÍ·
                                 var msg = player.channel.Allocator.DirectBuffer(4 + player.id.ToString().Length);
                                 msg.WriteUnsignedShort((ushort)MessageType.Data);
                                 msg.WriteStringShortUtf8(player.id.ToString());
                                 msg.WriteUnsignedShort((ushort)MessageType.Data);
                                 foreach (var data in datas)
                                 {
                                     msg.WriteBytes(data);
                                 }                               
                                 //  player.channel.WriteAndFlushAsync(msg);

                                 player.channel.WriteAsync(msg);

                                 //   logger.Debug($"Zone send data:{player.id.ToString()},size:{msg.ReadableBytes}" +
                                 //     $",threadId:{Thread.CurrentThread.ManagedThreadId}!");
                                 //  }
                             }
                          
                             if(channel != null)
                             {
                                 channel.Flush();
                             }
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

            return Task.FromResult(new EndPointZone(siloAddress.Endpoint.Address.ToString(), nettyPort));
        }


        public Task PlayerEnter(Guid playerId,string frontId)
        {
            logger.Debug($"zone {this.GetPrimaryKey().ToString()} ," +
                $"PlayerEnter:{playerId.ToString()} ," +
                $"zone player count:{ players.Count}");

            if (!players.ContainsKey(playerId))
            {
              
                ZoneNetttyData.Instance.BindPlayerZone(playerId.ToString(), this);

                IChannel channel = ZoneNetttyData.Instance.GetChannel(playerId.ToString());
                players.Add(playerId, new ZonePlayer(playerId, channel));
            }

            return Task.CompletedTask;
        }

        public Task PlayerLeave(Guid playerId)
        {
            logger.Debug($"zone {this.GetPrimaryKey().ToString()} ,PlayerLeave:{playerId.ToString()} ,zone player count:{ players.Count}");
            ZoneNetttyData.Instance.UnBindPlayerZone(playerId.ToString());
            ZoneNetttyData.Instance.RemoveChannel(playerId.ToString());
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

        public void Recv(string playerId,byte[] data)
        {
            logger.Debug($"Zone recv data:{playerId},data.len:{data.Length}£¬threadId:{Thread.CurrentThread.ManagedThreadId}!");
            msgQueue.Enqueue(data);
           // recvBuffer.WriteBytes(data);
        }
    }
}
