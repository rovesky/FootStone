using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using FootStone.FrontNetty;
using NLog;
using SampleGameServer.Room;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace FootStone.Core
{
    public class ZoneNetttyData : IRecvData
    {
        private static readonly ZoneNetttyData instance = new ZoneNetttyData();

        private static Logger logger = LogManager.GetCurrentClassLogger();

        private static int msgCount = 0;

        private ZoneNetttyData()
        {
        }

        public static ZoneNetttyData Instance
        {
            get
            {
                return instance;
            }
        }


        private ConcurrentDictionary<string, IChannel> channels = new ConcurrentDictionary<string, IChannel>();       

        public void AddChannel(string id, IChannel channel)
        {
            logger.Debug("AddChannel:" + id);
            this.channels[id] = channel;
        }

        public void RemoveChannel(string id)
        {
            logger.Debug("RemoveChannel:" + id);
            IChannel value;
            channels.TryRemove(id, out value);            
        }

        public IChannel GetChannel(string id)
        {
            logger.Debug("FindChannel:" + id);
            return this.channels[id];
        }

        public int GetChannelCount()
        {       
            return this.channels.Count;
        }

        private ConcurrentDictionary<string, IZoneStream> zones = new ConcurrentDictionary<string, IZoneStream>();

        public void BindPlayerZone(string playerId, IZoneStream zone)
        {
            this.zones[playerId] = zone;
        }

        public void UnBindPlayerZone(string channels)
        {
            IZoneStream value;
            zones.TryRemove(channels, out value);
        }

        public IZoneStream getPlayerZone(string playerId)
        {         
            IZoneStream value;
            zones.TryGetValue(playerId, out value);
            return value;
        }

        public void Recv(string playerId, IByteBuffer data, IChannel channel)
        {
            logger.Debug($"Player{playerId}Recv Data!");
            IZoneStream zoneStream = getPlayerZone(playerId);
            if (zoneStream != null)
            {
                var len = data.ReadUnsignedShort();
                byte[] bytes = new byte[len];
                data.ReadBytes(bytes);
                zoneStream.Recv(playerId,bytes);
            }
        }

        public void BindChannel(string playerId, IChannel channel)
        {
            if (!this.channels.ContainsKey(playerId))
            {
                AddChannel(playerId, channel);
            }
            else
            {
                this.channels[playerId] = channel;
            }
        }
    }
}
