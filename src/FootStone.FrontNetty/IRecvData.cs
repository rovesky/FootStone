using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using System;
using System.Collections.Generic;
using System.Text;

namespace FootStone.FrontNetty
{
    public interface IRecvData
    {
        void Recv(string playerId, IByteBuffer data, IChannel channel);
        void BindChannel(string playerId, IChannel channel);
    }
}
