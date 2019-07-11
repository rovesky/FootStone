using DotNetty.Transport.Channels.Sockets;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FootStone.Client
{
    public delegate void EventRecvData(byte[] data);

    public interface IFSChannel : ISocketChannel
    {
        void SendData(byte[] bytes);

        //Task Handshake(string id);
        //void HandshakeResponse();

        Task BindGameServer(string id, string gameServerId);
      ///  void BindGameServerResponse();

        Task<long> Ping(long time);

        
        event EventRecvData eventRecvData;
      //  void PingResponse(long pingTime);
    }
}
