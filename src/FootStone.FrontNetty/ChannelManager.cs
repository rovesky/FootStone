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

        private static NLog.Logger logger = LogManager.GetCurrentClassLogger();

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


        private ConcurrentDictionary<string, IPlayerChannel> channels = new ConcurrentDictionary<string, IPlayerChannel>();

        public  void AddChannel(string id, IPlayerChannel channel)
        {
            logger.Debug("AddChannel:" + id);
            this.channels[id] = channel;
        }

        public void RemoveChannel(string id)
        {
            logger.Debug("RemoveChannel:" + id);
            IPlayerChannel value;
            channels.TryRemove(id, out value);            
        }

        public IPlayerChannel GetChannel(string id)
        {
            logger.Debug("FindChannel:" + id);
            return this.channels[id];
        }

        public int GetChannelCount()
        {       
            return this.channels.Count;
        }
        public void Send(string id,byte[] data)
        {
            Interlocked.Increment(ref msgCount);
            if (msgCount % 10000 == 0)
            {
                logger.Debug("send msg count:" + msgCount);
            }

            IPlayerChannel channel;
            this.channels.TryGetValue(id, out channel);
            if(channel != null)
            {
                channel.Send(data);
            }
        }
    }
}
