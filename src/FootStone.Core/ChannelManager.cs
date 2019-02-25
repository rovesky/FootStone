using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace FootStone.Core
{
    public class ChannelManager
    {
        private static readonly ChannelManager instance = new ChannelManager();

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
            Console.Out.WriteLine("AddChannel:" + id);
            this.channels[id] = channel;
        }

        public void RemoveChannel(string id)
        {
            Console.Out.WriteLine("RemoveChannel:" + id);
            IPlayerChannel value;
            this.channels.Remove(id, out value);            
        }

        public IPlayerChannel GetChannel(string id)
        {
            Console.Out.WriteLine("FindChannel:" + id);
            return this.channels[id];
        }

        public void Send(string id,byte[] data)
        {
            Interlocked.Increment(ref msgCount);
            if (msgCount % 10000 == 0)
            {
                Console.Out.WriteLine("send msg count:" + msgCount);
            }

          //  this.channels.ContainsKey

            IPlayerChannel channel = this.channels[id];
            if(channel != null)
            {
                channel.Send(data);
            }
        }
    }
}
