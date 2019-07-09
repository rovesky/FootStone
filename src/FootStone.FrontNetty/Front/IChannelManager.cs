using DotNetty.Transport.Channels;

namespace FootStone.FrontNetty
{
    interface IChannelManager
    {  
        void AddChannel(string id, IChannel clientChannel,IChannel gameChannel);

        void BindGameChannel(string id, IChannel channel);
        void RemoveChannel(string id);

        IChannel GetClientChannel(string id);
        IChannel GetGameChannel(string id);

        int GetChannelCount();

        void CloseChannelByGameChannel(IChannel gameChannel);
    }
}