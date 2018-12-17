using DotNetty.Transport.Channels;
using FootStone.Core.Grains;
using System;
using System.Collections.Generic;
using System.Text;

namespace FootStone.Core.GameServer
{
    class PlayerChannel : IPlayerChannel
    {
        private IChannel channel;

        public PlayerChannel(IChannel channel)
        {
            this.channel = channel;
        }

        public void Send(byte[] data)
        {
            channel.WriteAsync(data);
        }
    }
}
