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


        private ConcurrentDictionary<string, IChannel> channels = new ConcurrentDictionary<string, IChannel>();

        public  void AddChannel(string id, IChannel channel)
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
        public void Send(string id,byte[] data)
        {
            Interlocked.Increment(ref msgCount);
            if (msgCount % 10000 == 0)
            {
                logger.Debug("send msg count:" + msgCount);
            }

            IChannel channel;
            this.channels.TryGetValue(id, out channel);
            if(channel != null)
            {
                IByteBuffer byteBuffer = Unpooled.Buffer(data.Length);
                byteBuffer.WriteBytes(data);
                channel.WriteAndFlushAsync(byteBuffer);
            }
        }
    }
}
