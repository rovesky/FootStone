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
                        pipeline.AddLast("echo", new FrontServerHandler());
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

            logger.Info("DotNetty Stopped!");
        }
    }


    class FrontServerHandler : ChannelHandlerAdapter
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private string playerId;
       
        public FrontServerHandler()
        {
   
        }    

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {

            if (message is IByteBuffer buffer)
            {
                ushort type = buffer.ReadUnsignedShort();
                if (type == 1)
                {
                    int length = buffer.ReadUnsignedShort();
                    playerId = buffer.ReadString(length, Encoding.UTF8);
                    logger.Debug("Player Handshake:" + playerId);

                    ChannelManager.Instance.AddChannel(playerId + "c", context.Channel);
                }
                else if (type == 2)
                {
                    int length = buffer.ReadUnsignedShort();
                    var siloId = buffer.ReadString(length, Encoding.UTF8);

                    logger.Debug($"Player:{playerId} bind silo:{siloId}");
                    IChannel siloChannel = ChannelManager.Instance.GetChannel(siloId);
                    ChannelManager.Instance.AddChannel(playerId + "s", siloChannel);
                }
                else if (type == 10)
                {
                    logger.Debug($"Recieve Message:{playerId}");
                    IChannel siloChannel = ChannelManager.Instance.GetChannel(playerId + "s");

                    //   buffer.SetReaderIndex(0);

                    var data = siloChannel.Allocator.Buffer(40);
                    data.WriteUnsignedShort(10);
                    data.WriteUnsignedShort((ushort)playerId.Length);
                    data.WriteString(playerId, Encoding.UTF8);

                    data.WriteUnsignedShort((ushort)playerId.Length);
                    data.WriteString(playerId, Encoding.UTF8);

                    siloChannel.WriteAndFlushAsync(data);
                }
                else
                {
                    logger.Error($"unkown type:{type}");
                }
            }

            base.ChannelRead(context, message);

        }


        public override void HandlerRemoved(IChannelHandlerContext context)
        {
  
            if (playerId != null)
            {          
                ChannelManager.Instance.RemoveChannel(playerId + "c");
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
