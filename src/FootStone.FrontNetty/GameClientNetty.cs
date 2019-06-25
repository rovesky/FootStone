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
    class GameClientHandler : ChannelHandlerAdapter
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public GameClientHandler()
        {

        }


        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            try
            {
                var buffer = message as IByteBuffer;
                if (buffer != null)
                {
                    buffer.ReadUnsignedShort();
                    var playerId = buffer.ReadStringShortUtf8();
                    IChannel channel = ChannelManager.Instance.GetChannel(playerId);
                    buffer.DiscardReadBytes();
                    channel.WriteAndFlushAsync(buffer);

                    logger.Debug($"Send Data to client:{playerId}");
                    return;
                }
            }
            catch (Exception e)
            {

            }
            base.ChannelRead(context, message);

        }

        public override void HandlerRemoved(IChannelHandlerContext context)
        {
            // if (siloId != null)
            // {
            logger.Debug($"HandlerRemoved:{context.Name}");
            ChannelManager.Instance.RemoveChannel(context.Name);
            //    siloId = null;
           // }
            base.HandlerRemoved(context);
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
            ChannelManager.Instance.AddChannel(siloId, channel);

            //var message = channel.Allocator.DirectBuffer(4+siloId.Length);
            //message.WriteUnsignedShort((ushort)MessageType.SiloHandShake);         
            //message.WriteStringShortUtf8(siloId);
            //await channel.WriteAndFlushAsync(message);

            logger.Debug($"Netty Add Silo：{siloId}");
       

            return channel;
        }

    }
}
