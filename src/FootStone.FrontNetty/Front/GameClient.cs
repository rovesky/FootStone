using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using FootStone.ProtocolNetty;
using NLog;
using System;
using System.Threading.Tasks;

namespace FootStone.FrontNetty
{
    class GameClientHandler : ChannelHandlerAdapter
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private NettyFrontService frontService;
        private IChannelManager playerChannels;
        public string GameServerId { get; internal set; }

        public GameClientHandler(NettyFrontService frontService, IChannelManager playerChannels)
        {
            this.frontService = frontService;
            this.playerChannels = playerChannels;       
        }


        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            try
            {
                var buffer = message as IByteBuffer;
                if (buffer != null)
                {
                    ushort type = buffer.ReadUnsignedShort();
                    var playerId = buffer.ReadStringShortUtf8();
                    IChannel channel = playerChannels.GetClientChannel(playerId);

                    if (channel != null)
                    {
                        buffer.DiscardReadBytes();

                        switch ((MessageType)type)
                        {
                            case MessageType.Data:
                                //   ReferenceCountUtil.Release(buffer);
                                channel.WriteAndFlushAsync(buffer);
                                break;
                            default:
                                channel.WriteAndFlushAsync(buffer);
                                break;
                        }
                      //  logger.Debug($"Send Data to client:{playerId},type:{type}");
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                logger.Warn(e);
            }
            base.ChannelRead(context, message);

        }

        public override void HandlerAdded(IChannelHandlerContext context)
        {
            logger.Warn($"GameClient HandlerAdded:{GameServerId}");
    
            base.HandlerAdded(context);
        }

        public override void HandlerRemoved(IChannelHandlerContext context)
        {       
            logger.Warn($"GameClient HandlerRemoved:{GameServerId}");

            frontService.GameChannelRemove(GameServerId);
            base.HandlerRemoved(context);
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            logger.Error(exception);
            context.CloseAsync();
            base.ExceptionCaught(context, exception);
        }
    }

    class GameClient
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private Bootstrap bootstrap;
        private MultithreadEventLoopGroup group;

        public GameClient()
        {

        }

        public void Init(NettyFrontService frontService,IChannelManager playerChannels)
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
                    .Option(ChannelOption.SoSndbuf, 1 * 1024 * 1024)
                    .Option(ChannelOption.SoRcvbuf, 1 * 1024 * 1024)
                    .Handler(new ActionChannelInitializer<ISocketChannel>(channel =>
                    {
                        IChannelPipeline pipeline = channel.Pipeline;
                 
                        // pipeline.AddLast(new LoggingHandler());
                        pipeline.AddLast("framing-enc", new LengthFieldPrepender(2));
                        pipeline.AddLast("framing-dec", new LengthFieldBasedFrameDecoder(ushort.MaxValue, 0, 2, 0, 2));
                        pipeline.AddLast("game-client", new GameClientHandler(frontService,playerChannels));
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
        public async Task<IChannel> ConnectGameServerAsync(string gameServerId)
        {
            var endpoint = ProtocolNettyUtility.GameServerId2Endpoint(gameServerId);
            var channel = await bootstrap.ConnectAsync(endpoint);

            var handler = channel.Pipeline.Get<GameClientHandler>();
            handler.GameServerId = gameServerId;  
            return channel;
        }

    }
}
