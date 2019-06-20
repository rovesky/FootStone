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
    public class GameServerNetty
    {
       
        private IChannel boundChannel;
        private MultithreadEventLoopGroup bossGroup;
        private MultithreadEventLoopGroup workerGroup;

        private Logger logger = LogManager.GetCurrentClassLogger();
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
                    //  .Option(ChannelOption.SoBacklog, 100)
                    //  .Option(ChannelOption.SoSndbuf, 100)
                    //  .Option(ChannelOption.TcpNodelay, true)
                    //  .Handler(new LoggingHandler("SRV-LSTN"))
                    .Option(ChannelOption.Allocator, PooledByteBufferAllocator.Default)
                    .ChildHandler(new ActionChannelInitializer<IChannel>(channel =>
                    {
                        IChannelPipeline pipeline = channel.Pipeline;

                        //    pipeline.AddLast(new LoggingHandler("SRV-CONN"));
                        pipeline.AddLast("framing-enc", new LengthFieldPrepender(2));
                        pipeline.AddLast("framing-dec", new LengthFieldBasedFrameDecoder(ushort.MaxValue, 0, 2, 0, 2));
                        pipeline.AddLast("echo", new GameServerHandler());
                    }));

                logger.Info("DotNetty Inited!");
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
                logger.Info($"DotNetty Started:{port}");
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

            logger.Info("DotNetty Stopped!");
        }
    }


    class GameServerHandler : ChannelHandlerAdapter
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private string siloId;
       
        public GameServerHandler()
        {
   
        }    

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            if (message is IByteBuffer buffer)
            { 
                ushort type = buffer.ReadUnsignedShort();

                switch ((MessageType)type)
                {
                    case MessageType.SiloHandShake:
                        {
                            ushort length = buffer.ReadUnsignedShort();
                            siloId = buffer.ReadString(length, Encoding.UTF8);
                            logger.Debug("Game Handshake:" + siloId);
                            ChannelManager.Instance.AddChannel(siloId, context.Channel);                         
                            break;
                        }
                    case MessageType.Data:
                        {
                            ushort length = buffer.ReadUnsignedShort();
                            var playerId = buffer.ReadString(length, Encoding.UTF8);                           

                            length = buffer.ReadUnsignedShort();
                            var m = buffer.ReadString(length, Encoding.UTF8);
                            logger.Debug($"Recieve message:{m} from player:{playerId},disType{0},send back!");

                            buffer.ResetReaderIndex();
                            context.Channel.WriteAndFlushAsync(buffer);
                            return;
                        }
                    default:
                        logger.Error($"Unknown type:{type}!");
                        break;
                }
            }

            base.ChannelRead(context, message);
        }


        public override void HandlerRemoved(IChannelHandlerContext context)
        {  
            if (siloId != null)
            {          
                ChannelManager.Instance.RemoveChannel(siloId);
                siloId = null;
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
