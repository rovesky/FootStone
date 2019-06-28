using DotNetty.Transport.Channels;

namespace FootStone.FrontNetty
{
    interface IChannelManager
    {  
        void AddChannel(string id, IChannel channel);

        void RemoveChannel(string id);

        IChannel GetChannel(string id);

        int GetChannelCount();     
    }
}