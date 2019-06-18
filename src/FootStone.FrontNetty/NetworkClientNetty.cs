using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Handlers.Logging;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using NLog;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FootStone.FrontNetty
{

    struct Point2D
    {
        int x;
        int y;
    }

    [Serializable]
    struct Move
    {
        byte direction;
        byte speed;


    }

    class SocketNettyHandler : ChannelHandlerAdapter
    {
   
        private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private static int msgCount = 0;
        private static int playerCount = 0;

        public SocketNettyHandler()
        {         
            Interlocked.Increment(ref playerCount);
            logger.Info($"new SocketNettyHandler:{playerCount}! ");
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
                Interlocked.Increment(ref msgCount);
                if (msgCount % (30 * playerCount) == 0)
                {
                    logger.Info("Received from server msg count: " + msgCount + ",msg length:" + buffer.Capacity);
                }
            }
            base.ChannelRead(context, message);
        }

        public override void ChannelReadComplete(IChannelHandlerContext context) => context.Flush();

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            logger.Error(exception);
            context.CloseAsync();

            base.ExceptionCaught(context, exception);
        }
    }



    public class NetworkClientNetty
    {
        private Bootstrap bootstrap;
        private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private MultithreadEventLoopGroup group;

        public NetworkClientNetty()
        {

        }

        public void Init()
        {
            group = new MultithreadEventLoopGroup();

            //X509Certificate2 cert = null;
            //string targetHost = null;
            //if (ClientSettings.IsSsl)
            //{
            //    cert = new X509Certificate2(Path.Combine(ExampleHelper.ProcessDirectory, "dotnetty.com.pfx"), "password");
            //    targetHost = cert.GetNameInfo(X509NameType.DnsName, false);
            //}
            try
            {
                bootstrap = new Bootstrap();
                bootstrap
                    .Group(group)
                    .Channel<TcpSocketChannel>()
                    .Option(ChannelOption.Allocator, PooledByteBufferAllocator.Default)
                    .Option(ChannelOption.TcpNodelay, true)
                    .Handler(new ActionChannelInitializer<ISocketChannel>(channel =>
                    {
                        IChannelPipeline pipeline = channel.Pipeline;

                        // pipeline.AddLast(new LoggingHandler());
                        pipeline.AddLast("framing-enc", new LengthFieldPrepender(2));
                        pipeline.AddLast("framing-dec", new LengthFieldBasedFrameDecoder(ushort.MaxValue, 0, 2, 0, 2));
                        pipeline.AddLast("echo", new SocketNettyHandler());
                    }));
            }

            catch (Exception ex)
            {
                 logger.Error(ex);

            }
            finally
            {
                //  await group.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));
            }
        }

        public async Task  Fini()
        {
            if(group!= null)
            {
                await group.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));
            }

          
        }
        public async Task<IChannel> ConnectNettyAsync(string host, int port, string playerId)
        {
            var channel =  await bootstrap.ConnectAsync(new IPEndPoint(IPAddress.Parse(host), port));
      
          //  var initialMessage = Unpooled.Buffer(100);
            var initialMessage = channel.Allocator.Buffer(50);
            byte[] messageBytes = Encoding.UTF8.GetBytes(playerId);
            initialMessage.WriteUnsignedShort(1);
            initialMessage.WriteUnsignedShort((ushort)messageBytes.Length);
            initialMessage.WriteBytes(messageBytes);

            await channel.WriteAndFlushAsync(initialMessage);

            return channel;
        }

    }
}
