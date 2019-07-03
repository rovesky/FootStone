using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using FootStone.ProtocolNetty;
using NLog;
using System;
using System.Text;
using System.Threading.Tasks;

namespace FootStone.FrontNetty
{
    class FrontServer
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private IChannel boundChannel;
        private MultithreadEventLoopGroup bossGroup;
        private MultithreadEventLoopGroup workerGroup;
        private ServerBootstrap bootstrap;


        public delegate void delegateBindPlayerAndGameServer(string playerId,string siloId);
        public event delegateBindPlayerAndGameServer eventBindPlayerAndGameServer;

        public void EmitEventBindPlayerAndGameServer(string playerId,string siloId)
        {
            eventBindPlayerAndGameServer(playerId, siloId);
        }

        public void Init(IChannelManager[] channelManager)
        {
            bossGroup = new MultithreadEventLoopGroup(1);
            workerGroup = new MultithreadEventLoopGroup(4);

            try
            {
                bootstrap = new ServerBootstrap();
                bootstrap.Group(bossGroup, workerGroup)
                    .Channel<TcpServerSocketChannel>()
                    .Option(ChannelOption.TcpNodelay, true)
                    .Option(ChannelOption.Allocator, PooledByteBufferAllocator.Default)
                    .ChildHandler(new ActionChannelInitializer<IChannel>(channel =>
                    {
                        IChannelPipeline pipeline = channel.Pipeline;
                        //    pipeline.AddLast(new LoggingHandler("SRV-CONN"));
                        pipeline.AddLast("framing-enc", new LengthFieldPrepender(2));
                        pipeline.AddLast("framing-dec", new LengthFieldBasedFrameDecoder(ushort.MaxValue, 0, 2, 0, 2));
                        pipeline.AddLast("front-server", new FrontServerHandler(this,channelManager));
                    }));

                logger.Info("DotNetty FrontServer Inited!");
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }


        public async Task Start(int port)
        {
            try
            {
                boundChannel = await bootstrap.BindAsync(port);
                logger.Info($"DotNetty FrontServer Started:{port}");
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        public async  Task Fini()
        {        
            await boundChannel.CloseAsync();
            await Task.WhenAll(
                 bossGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)),
                 workerGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)));

            logger.Info("DotNetty FrontServer Stopped!");
        }
    }


    class FrontServerHandler : ChannelHandlerAdapter
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private string playerId;
     
        private FrontServer frontServer;
        private IChannelManager frontChannels;
        private IChannelManager gameChannels;

        private string gameServerId;
        private IChannel gameServerChannel;

        public FrontServerHandler(FrontServer frontServer,IChannelManager[] channelManagers)
        {
            this.frontServer = frontServer;
            this.frontChannels = channelManagers[0];
            this.gameChannels = channelManagers[1];
        }
  

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {

            if (message is IByteBuffer buffer)
            {
                //读取消息类型
                ushort type = buffer.ReadUnsignedShort();

                switch ((MessageType)type)
                {
                    case MessageType.PlayerHandshake:
                        {
                            playerId = buffer.ReadStringShortUtf8();                          
                            buffer.ResetReaderIndex();
                            context.Channel.WriteAndFlushAsync(buffer);
                            frontChannels.AddChannel(playerId, context.Channel);
                            logger.Info("Player Handshake:" + playerId);
                            return;
                        }
                    case MessageType.PlayerBindGame:
                        {
                            var playerId = buffer.ReadStringShortUtf8();
                            gameServerId = buffer.ReadStringShortUtf8();
                            frontServer.EmitEventBindPlayerAndGameServer(playerId, gameServerId);
                            logger.Debug($"Player:{playerId} bind gameServer:{gameServerId}");
                            break;
                        }
                    case MessageType.Ping:
                        {
                            //添加包头
                            var header = context.Allocator.DirectBuffer(4 + playerId.Length);
                            header.WriteUnsignedShort(type);
                            header.WriteStringShortUtf8(playerId);

                            buffer.ResetReaderIndex();
                            var comBuff = context.Allocator.CompositeDirectBuffer();
                            comBuff.AddComponents(true, header, buffer);

                            if (gameServerChannel == null)
                            {
                                gameServerChannel = gameChannels.GetChannel(gameServerId);
                            }
                            gameServerChannel.WriteAndFlushAsync(comBuff);
                            return;
                        }

                    case MessageType.Data:                  
                        {
                            //添加包头
                            var header = context.Allocator.DirectBuffer(4 + playerId.Length);
                            header.WriteUnsignedShort(type);
                            header.WriteStringShortUtf8(playerId);

                            buffer.DiscardReadBytes();
                            var comBuff = context.Allocator.CompositeDirectBuffer();
                            comBuff.AddComponents(true, header, buffer);

                            if(gameServerChannel == null)
                            {
                                gameServerChannel = gameChannels.GetChannel(gameServerId);
                            }
                            gameServerChannel.WriteAndFlushAsync(comBuff);
                            //siloChannel.WriteAsync(comBuff);
                            //  logger.Debug($"Recieve Message:{playerId},buff.ReadableBytes:{comBuff.ReadableBytes}," +
                            //   $"buff.WritableBytes:{comBuff.WritableBytes},buff.Capacity: {comBuff.Capacity}");
                            return;
                        }
                    default:
                        {
                            logger.Error($"unkown type:{type}");
                            break;
                        }
                }
            }
            base.ChannelRead(context, message);
        }


        public override void HandlerRemoved(IChannelHandlerContext context)
        {  
            if (playerId != null)
            {          
                frontChannels.RemoveChannel(playerId);
                playerId = null;
            }
            base.HandlerRemoved(context);
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            if (exception is System.Net.Sockets.SocketException)
            {
                logger.Info(exception);
            }
            else
            {
                logger.Error(exception);
            }
            context.CloseAsync();
        }
    }
}
