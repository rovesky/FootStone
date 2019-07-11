using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using FootStone.ProtocolNetty;
using NLog;
using System;

namespace FootStone.Client
{
    class FSSocketHandler : ChannelHandlerAdapter
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();

        public FSSocketHandler()
        {
        
        }

        public override void ChannelActive(IChannelHandlerContext context)
        {
            base.ChannelActive(context);
        }


        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            var buffer = message as IByteBuffer;
            if (buffer != null)
            {
                //logger.Debug("recevie Data!");
                FSTcpSocketChannel channel = context.Channel as FSTcpSocketChannel;
                MessageType type = (MessageType)buffer.ReadUnsignedShort();
                if (type == MessageType.BindGameServer)
                {
                    channel.BindGameServerResponse();                 
                }
                else if (type == MessageType.Ping)
                {
                    var pingTime = buffer.ReadLong();
                    channel.PingResponse(pingTime);           
                }
                else if (type == MessageType.Data)
                {
                    //   logger.Debug("recevie Data!");
                    //   msgQueue.Enqueue(buffer);
                    channel.RecvData(buffer);
                  //  return;
                }

            }
            base.ChannelRead(context, message);
        }
             

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            logger.Error(exception);
            context.CloseAsync();

            base.ExceptionCaught(context, exception);
        }
    }   
}
