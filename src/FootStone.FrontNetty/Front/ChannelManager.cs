using DotNetty.Transport.Channels;
using System;
using System.Collections.Concurrent;

namespace FootStone.FrontNetty
{

    class PlayerChannel
    {
        public IChannel ClientChannel { get; set; }
        public IChannel GameChannel { get; set; }
    }


    class ChannelManager :IChannelManager
    {   
     //   private static Logger logger = LogManager.GetCurrentClassLogger();

        private ConcurrentDictionary<string, PlayerChannel> channels = new ConcurrentDictionary<string, PlayerChannel>();
    
        public ChannelManager()
        {

        }    
              
        public void AddChannel(string id, IChannel clientChannel,IChannel gameChannel)
        {
            var playerChannel = new PlayerChannel();
            playerChannel.ClientChannel = clientChannel;
            playerChannel.GameChannel = gameChannel;
            channels.TryAdd(id, playerChannel);
        }

        public void RemoveChannel(string id)
        {
            PlayerChannel value;
            channels.TryRemove(id, out value);
        }
            

        public int GetChannelCount()
        {
            return channels.Count;
        }

        public void BindGameChannel(string id, IChannel channel)
        {
            PlayerChannel playerChannel = null;
            channels.TryGetValue(id, out playerChannel);

            if (playerChannel == null)
                throw new Exception($"{id} BindGameChannel error: playerChannel == null");

            playerChannel.GameChannel = channel;
        }

        public IChannel GetClientChannel(string id)
        {
            PlayerChannel playerChannel = null;
            channels.TryGetValue(id, out playerChannel);

            if (playerChannel == null)
                throw new Exception($"{id} GetClientChannel error: playerChannel == null");

            return playerChannel.ClientChannel;
        }

        public IChannel GetGameChannel(string id)
        {
            PlayerChannel playerChannel = null;
            channels.TryGetValue(id, out playerChannel);

            if (playerChannel == null)
                throw new Exception($"{id} GetGameChannel error: playerChannel == null");

            return playerChannel.ClientChannel;
        }

        public void CloseChannelByGameChannel(IChannel gameChannel)
        {
            foreach(var playerChannel in channels.Values)
            {
                if(playerChannel.GameChannel == gameChannel)
                {
                    playerChannel.ClientChannel.CloseAsync();
                }
            }
        }
    }
}
