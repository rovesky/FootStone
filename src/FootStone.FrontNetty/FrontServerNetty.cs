using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using NLog;
using System;
using System.Text;
using System.Threading.Tasks;

namespace FootStone.FrontNetty
{
    public class FrontServerNetty
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private IChannel boundChannel;
        private MultithreadEventLoopGroup bossGroup;
        private MultithreadEventLoopGroup workerGroup;
        private ServerBootstrap bootstrap;

        public void Init()
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
                        pipeline.AddLast("echo", new FrontServerHandler());
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

        public async  Task Stop()
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
        private IChannel siloChannel;

        public FrontServerHandler()
        {
   
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {

            if (message is IByteBuffer buffer)
            {
                ushort type = buffer.ReadUnsignedShort();

                switch ((MessageType)type)
                {
                    case MessageType.PlayerHandshake:
                        {
                            playerId = buffer.ReadStringShortUtf8();
                            ChannelManager.Instance.AddPlayerChannel(playerId, context.Channel);

                            buffer.ResetReaderIndex();
                            context.Channel.WriteAndFlushAsync(buffer);                         

                            logger.Debug("Player Handshake:" + playerId);
                            return;
                        }
                    case MessageType.PlayerBindSilo:
                        {
                            var playerId = buffer.ReadStringShortUtf8();
                            var siloId = buffer.ReadStringShortUtf8();
                            siloChannel = ChannelManager.Instance.GetSiloChannel(siloId);

                            buffer.ResetReaderIndex();
                            siloChannel.WriteAndFlushAsync(buffer);
                            logger.Debug($"Player:{playerId} bind silo:{siloId}");
                            return;
                        }
                    case MessageType.Data:
                        {
                            //添加包头
                            var header = context.Allocator.DirectBuffer(4 + playerId.Length);
                            header.WriteUnsignedShort((ushort)MessageType.Data);
                            header.WriteStringShortUtf8(playerId);

                            buffer.DiscardReadBytes();
                            var comBuff = context.Allocator.CompositeDirectBuffer();
                            comBuff.AddComponents(true, header, buffer);

                            siloChannel.WriteAndFlushAsync(comBuff);
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
                ChannelManager.Instance.RemoveSiloChannel(playerId);
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
