using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Common.Utilities;
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
  
        private IChannelManager frontChannels;
        private IChannelManager gameChannels;

      //  private string gameServerId;

        public string GameServerId { get; internal set; }

        public GameClientHandler(IChannelManager[] channelManagers)
        {
            this.frontChannels = channelManagers[0]; 
            this.gameChannels = channelManagers[1];
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
                    IChannel channel = frontChannels.GetChannel(playerId);
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
                    //   logger.Debug($"Send Data to client:{playerId}");
                    return;
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
            logger.Warn($"Front HandlerAdded:{GameServerId}");
          //  gameChannels.AddChannel(GameServerId, context.Channel);
            base.HandlerAdded(context);
        }

        public override void HandlerRemoved(IChannelHandlerContext context)
        {       
            logger.Warn($"Front HandlerRemoved:{GameServerId}");
            gameChannels.RemoveChannel(GameServerId);      
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

        public void Init(IChannelManager[] channelManager)
        {
            group = new MultithreadEventLoopGroup();
            try
            {
                bootstrap = new Bootstrap();
                bootstrap
                    .Group(group)
                    .Channel<TcpSocketChannel>()
                    .Option(ChannelOption.Allocator, PooledByteBufferAllocator.Default)
                    .Option(ChannelOption.TcpNodelay, false)
                    .Option(ChannelOption.SoSndbuf, 512 * 1024)
                    .Option(ChannelOption.SoRcvbuf, 512 * 1024)
                    .Handler(new ActionChannelInitializer<ISocketChannel>(channel =>
                    {
                        IChannelPipeline pipeline = channel.Pipeline;
                 
                        // pipeline.AddLast(new LoggingHandler());
                        pipeline.AddLast("framing-enc", new LengthFieldPrepender(2));
                        pipeline.AddLast("framing-dec", new LengthFieldBasedFrameDecoder(ushort.MaxValue, 0, 2, 0, 2));
                        pipeline.AddLast("game-client", new GameClientHandler(channelManager));
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
        public async Task<IChannel> ConnectNettyAsync(string gameServerId)
        {
            var splits = gameServerId.Split(':');
            var channel = await bootstrap.ConnectAsync(new IPEndPoint(IPAddress.Parse(splits[0]), int.Parse(splits[1])));

            var handler = channel.Pipeline.Get<GameClientHandler>();
            handler.GameServerId = gameServerId;
        //    var siloId = host + ":" + port;
        //    ChannelManager.Instance.AddSiloChannel(siloId, channel);
        //   logger.Debug($"Netty Add Silo：{siloId}");      
            return channel;
        }

    }
}
