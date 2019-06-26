using DotNetty.Transport.Channels;

namespace FootStone.FrontNetty
{
    public interface IChannelManager
    {  
        void AddChannel(string id, IChannel channel);

        void RemoveChannel(string id);

        IChannel GetChannel(string id);

        int GetChannelCount();     
    }
}