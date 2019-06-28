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
    public class GameServer
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private IChannel boundChannel;
        private MultithreadEventLoopGroup bossGroup;
        private MultithreadEventLoopGroup workerGroup;
        private ServerBootstrap bootstrap;

        public void Init(IRecvData recv)
        {
            bossGroup = new MultithreadEventLoopGroup(1);
            workerGroup = new MultithreadEventLoopGroup(4);

            try
            {               
                bootstrap = new ServerBootstrap();
                bootstrap.Group(bossGroup, workerGroup)
                         .Channel<TcpServerSocketChannel>()
                    //  .Option(ChannelOption.SoBacklog, 100)                  
                        .Option(ChannelOption.TcpNodelay, false)                   
                        .Option(ChannelOption.SoSndbuf, 512*1024)
                        .Option(ChannelOption.SoRcvbuf, 512*1024)
                    //  .Handler(new LoggingHandler("SRV-LSTN"))
                    .Option(ChannelOption.Allocator, PooledByteBufferAllocator.Default)
                    .ChildHandler(new ActionChannelInitializer<IChannel>(channel =>
                    {
                        IChannelPipeline pipeline = channel.Pipeline;

                        pipeline.AddLast("framing-enc", new LengthFieldPrepender(2));
                        pipeline.AddLast("framing-dec", new LengthFieldBasedFrameDecoder(ushort.MaxValue, 0, 2, 0, 2));
                        pipeline.AddLast("game-server", new GameServerHandler(recv));
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
        private string frontId;

        private IRecvData recv;

        public GameServerHandler(IRecvData recv)
        {
            this.recv = recv;   
        }            

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            try
            {
                if (message is IByteBuffer buffer)
                {
                    ushort type = buffer.ReadUnsignedShort();

                    switch ((MessageType)type)
                    {
                        case MessageType.PlayerBindGame:
                            {
                                var playerId = buffer.ReadStringShortUtf8();
                                recv.BindChannel(playerId, context.Channel);

                                //添加包头
                                var header = context.Allocator.DirectBuffer(4 + playerId.Length);
                                header.WriteUnsignedShort((ushort)MessageType.PlayerBindGame);
                                header.WriteStringShortUtf8(playerId);

                                buffer.ResetReaderIndex();
                                var comBuff = context.Allocator.CompositeDirectBuffer();
                                comBuff.AddComponents(true, header, buffer);

                                context.Channel.WriteAndFlushAsync(comBuff);

                                logger.Debug($"Game PlayerBindSilo:{playerId}!");
                                return;
                            }
                        case MessageType.Data:
                            {
                                var playerId = buffer.ReadStringShortUtf8();
                              //  logger.Debug($"Game recv data,player:{playerId},size:{buffer.Capacity}!");
                                recv.Recv(playerId, buffer);
                                break;
                            }
                        default:
                            logger.Error($"Unknown type:{type}!");
                            break;
                    }
                }
            }
            catch(Exception e)
            {
                logger.Error(e);

            }
            base.ChannelRead(context, message);
        }

        public override void HandlerAdded(IChannelHandlerContext context)
        {
            frontId = context.Channel.RemoteAddress.ToString();
           // ChannelManager.Instance.AddPlayerChannel(frontId,context.Channel);

            logger.Warn($"Game HandlerAdded:{frontId}!");
            base.HandlerAdded(context);
        }

        public override void HandlerRemoved(IChannelHandlerContext context)
        {  
            if (frontId != null)
            {
                logger.Warn($"Game HandlerRemoved:{frontId}!");
            //    ChannelManager.Instance.RemovePlayerChannel(frontId);
                frontId = null;
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
