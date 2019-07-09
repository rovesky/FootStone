using DotNetty.Transport.Channels.Sockets;
using FootStone.ProtocolNetty;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FootStone.Client
{
    public class FSTcpSocketChannel : TcpSocketChannel, IFSChannel
    {
        public TaskCompletionSource<object> tcsHandshake = new TaskCompletionSource<object>();
        public TaskCompletionSource<object> tcsBindGameServer = new TaskCompletionSource<object>();
        public TaskCompletionSource<object> tcsPing;// =   

        public async Task Handshake(string id)
        {          
            var message = Allocator.DirectBuffer();
         //   message.WriteUnsignedShort((ushort)MessageType.PlayerHandshake);
            message.WriteStringShortUtf8(id);
            await WriteAndFlushAsync(message);
            
            await tcsHandshake.Task;
        }

        public void HandshakeResponse()
        {
            tcsHandshake.SetResult(null);
        }

        public void BindGameServerResponse()
        {
            tcsBindGameServer.SetResult(null);
        }
      

        public async Task BindGameServer(string id, string gameServerId)
        {
            var data = Allocator.DirectBuffer();
            data.WriteUnsignedShort((ushort)MessageType.BindGameServer);
            data.WriteStringShortUtf8(id);
            data.WriteStringShortUtf8(gameServerId);

            WriteAndFlushAsync(data);
       
            await tcsBindGameServer.Task;
        }


        public async Task<long> Ping(long time)
        {
            tcsPing = new TaskCompletionSource<object>();
            var data = Allocator.DirectBuffer(6);
            data.WriteUnsignedShort((ushort)MessageType.Ping);
            data.WriteLong(DateTime.Now.Ticks);

            await WriteAndFlushAsync(data);

            return (long)await tcsPing.Task;
        }

        public void PingResponse(long pingTime)
        {
            tcsPing.SetResult(pingTime);
        }
        
       

        public void SendData(byte[] bytes)
        {
            var data = Allocator.DirectBuffer(16);
            data.WriteUnsignedShort((ushort)MessageType.Data);
            data.WriteUnsignedShort((ushort)bytes.Length);
            data.WriteBytes(bytes);

            WriteAndFlushAsync(data);
        }
    }
}
