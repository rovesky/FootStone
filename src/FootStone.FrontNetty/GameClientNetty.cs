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

    //struct Point2D
    //{
    //    int x;
    //    int y;
    //}

    //[Serializable]
    //struct Move
    //{
    //    byte direction;
    //    byte speed;
    //}

    class GameClientHandler : ChannelHandlerAdapter
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public GameClientHandler()
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
                int length = buffer.ReadInt();
                var playerId = buffer.ReadString(length, Encoding.UTF8);

                logger.Debug($"Send Data to client:{playerId}");
                IChannel channel = ChannelManager.Instance.GetChannel(playerId + "c");
                channel.WriteAndFlushAsync(buffer);
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

    public class GameClientNetty
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private Bootstrap bootstrap;
        private MultithreadEventLoopGroup group;

        public GameClientNetty()
        {

        }

        public void Init()
        {
            group = new MultithreadEventLoopGroup();
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
                        pipeline.AddLast("echo", new GameClientHandler());
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

        public async Task Fini()
        {
            if (group != null)
            {
                await group.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));
            }


        }
        public async Task<IChannel> ConnectNettyAsync(string host, int port)
        {
            var channel = await bootstrap.ConnectAsync(new IPEndPoint(IPAddress.Parse(host), port));

            var siloId = host + ":" + port;

            var message = channel.Allocator.Buffer(40);
            message.WriteUnsignedShort(1);
            message.WriteUnsignedShort((ushort)siloId.Length);
            message.WriteString(siloId, Encoding.UTF8);
            await channel.WriteAndFlushAsync(message);

            logger.Debug($"Netty Add Silo：{siloId}");
            ChannelManager.Instance.AddChannel(siloId, channel);

            return channel;
        }

    }
}
