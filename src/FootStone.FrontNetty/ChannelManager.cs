using DotNetty.Transport.Channels;
using System.Collections.Concurrent;

namespace FootStone.FrontNetty
{
    public class ChannelManager :IChannelManager
    {   
     //   private static Logger logger = LogManager.GetCurrentClassLogger();

        private ConcurrentDictionary<string, IChannel> channels = new ConcurrentDictionary<string, IChannel>();
    
        public ChannelManager()
        {

        }    

      
        public void AddChannel(string id, IChannel channel)
        {
            channels.TryAdd(id, channel);
        }

        public void RemoveChannel(string id)
        {    
            IChannel value;
            channels.TryRemove(id, out value);
        }

        public IChannel GetChannel(string id)
        {
            IChannel channel = null;
            channels.TryGetValue(id, out channel);
            return channel;
        }

        public int GetChannelCount()
        {
            return channels.Count;
        }
    }
}
