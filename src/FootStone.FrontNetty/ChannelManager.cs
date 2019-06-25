using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace FootStone.FrontNetty
{
    public class ChannelManager
    {
        private static readonly ChannelManager instance = new ChannelManager();

        private static Logger logger = LogManager.GetCurrentClassLogger();

        private static int msgCount = 0;

        private ChannelManager()
        {
        }

        public static ChannelManager Instance
        {
            get
            {
                return instance;
            }
        }


        private ConcurrentDictionary<string, IChannel> playerChannels = new ConcurrentDictionary<string, IChannel>();
        private ConcurrentDictionary<string, IChannel> siloChannels = new ConcurrentDictionary<string, IChannel>();

        public void AddPlayerChannel(string id, IChannel channel)
        {
         //   logger.Debug("AddPlayerChannel:" + id);
            this.playerChannels[id] = channel;
        }

        public void RemovePlayerChannel(string id)
        {
          //  logger.Debug("RemovePlayerChannel:" + id);
            IChannel value;
            playerChannels.TryRemove(id, out value);            
        }

        public IChannel GetPlayerChannel(string id)
        {
          //  logger.Debug("GetPlayerChannel:" + id);
            return this.playerChannels[id];
        }

        public int GetPlayerChannelCount()
        {       
            return this.playerChannels.Count;
        }

        public void AddSiloChannel(string id, IChannel channel)
        {
            //   logger.Debug("AddPlayerChannel:" + id);
            this.siloChannels[id] = channel;
        }

        public void RemoveSiloChannel(string id)
        {
            //  logger.Debug("RemovePlayerChannel:" + id);
            IChannel value;
            siloChannels.TryRemove(id, out value);
        }

        public IChannel GetSiloChannel(string id)
        {
            //  logger.Debug("GetPlayerChannel:" + id);
            return this.siloChannels[id];
        }

        public int GetSiloChannelCount()
        {
            return this.siloChannels.Count;
        }

        public void FlushAllSiloChannel()
        {
           foreach(var channel in this.siloChannels.Values)
            {
                channel.Flush();
            }
        }

    }
}
