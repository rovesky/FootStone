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

        //public static int playerCount = 0;

        //public static ConcurrentQueue<IByteBuffer> msgQueue = new ConcurrentQueue<IByteBuffer>();

        //public TaskCompletionSource<object> tcsConnected = new TaskCompletionSource<object>();
        //public TaskCompletionSource<object> tcsBindSiloed = new TaskCompletionSource<object>();

        public FSSocketHandler()
        {
          //  Interlocked.Increment(ref playerCount);
         //   logger.Debug($"new FSSocketHandler:{playerCount}! ");
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
                logger.Debug("recevie Data!");
                MessageType type = (MessageType)buffer.ReadUnsignedShort();
                if (type == MessageType.PlayerHandshake)
                {
                    (context.Channel as IFSChannel).HandshakeResponse(); 
                }
                else if (type == MessageType.PlayerBindGame)
                {
                    (context.Channel as IFSChannel).BindGameServerResponse();                 
                }
                else if (type == MessageType.Ping)
                {
                  
               //     var now = DateTime.Now.Ticks;
                    var pingTime = buffer.ReadLong();
                    (context.Channel as IFSChannel).PingResponse(pingTime);
                 //   var timer = (now - pingTime) / 10000;
                 //   logger.Debug($"ping value:{timer}ms");
                    // tcsBindSiloed.SetResult(null);
                }
                else if (type == MessageType.Data)
                {
                    logger.Debug("recevie Data!");
                 //   msgQueue.Enqueue(buffer);
                    return;
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
