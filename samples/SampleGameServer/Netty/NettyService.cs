using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Handlers.Logging;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using FootStone.Core.GrainInterfaces;
using FootStone.FrontNetty;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog;
using Orleans;
using Orleans.Concurrency;
using Orleans.Core;
using Orleans.Runtime;
using Orleans.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FootStone.Core.GameServer
{
    [Reentrant]
    public class NettyService : GrainService,INettyService
    {
        private readonly IGrainFactory GrainFactory; 
        private IChannel boundChannel;
        private MultithreadEventLoopGroup bossGroup;
        private MultithreadEventLoopGroup workerGroup;

        private NLog.Logger logger = LogManager.GetCurrentClassLogger();

        public NettyService(IServiceProvider services, IGrainIdentity id, Silo silo, ILoggerFactory loggerFactory, IGrainFactory grainFactory) 
            : base(id, silo, loggerFactory)
        {         
            GrainFactory = grainFactory;
        }

        public NettyOptions options { get; private set; }

        //public Task AddOptionTime(int time)
        //{
        //  //  operationTimes += time;
        //    return Task.CompletedTask;
        //}

        public override Task Init(IServiceProvider serviceProvider)
        {
            logger.Info("DotNetty Init!");           

            this.options = serviceProvider.GetService<IOptions<NettyOptions>>().Value;

            return base.Init(serviceProvider);
        }
      
        public async override  Task Start()
        {
            bossGroup = new MultithreadEventLoopGroup(1);
            workerGroup = new MultithreadEventLoopGroup();

            try
            {
                var bootstrap = new ServerBootstrap();
                bootstrap.Group(bossGroup, workerGroup);
                bootstrap.Channel<TcpServerSocketChannel>();

                bootstrap
                    .Option(ChannelOption.SoBacklog, 100)
                   // .Option(ChannelOption.TcpNodelay, true)
                   // .Handler(new LoggingHandler("SRV-LSTN"))
                    .ChildHandler(new ActionChannelInitializer<IChannel>(channel =>
                    {
                        IChannelPipeline pipeline = channel.Pipeline;

                    //    pipeline.AddLast(new LoggingHandler("SRV-CONN"));
                        pipeline.AddLast("framing-enc", new LengthFieldPrepender(2));
                        pipeline.AddLast("framing-dec", new LengthFieldBasedFrameDecoder(ushort.MaxValue, 0, 2, 0, 2));

                        pipeline.AddLast("echo", new SocketServerHandler());
                    }));

                string host = Silo.Endpoint.Address.ToString();
                boundChannel = await bootstrap.BindAsync(
                     // host,
                     options.Port);
                logger.Info("DotNetty Started:"+ options.Port);          
            }
            catch(Exception ex)
            {
                logger.Error(ex);
            }
            finally
            {
            //    await Task.WhenAll(
            //        bossGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)),
            //        workerGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)));
            }
         
        }

        public async override Task Stop()
        {
            logger.Info("DotNetty Stopped!");
            await boundChannel.CloseAsync();
            await Task.WhenAll(
                 bossGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)),
                 workerGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)));

            await base.Stop();      
        }       
    }


    class SocketServerHandler : ChannelHandlerAdapter
    {
        private NLog.Logger logger = LogManager.GetCurrentClassLogger();

        // private Dictionary<string, string> playerIds = new Dictionary<string, string>();
        private string playerId;

        private static int count = 0;

        public SocketServerHandler()
        {
          //  logger.Warn("SocketServerHandler Constructor!");
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            logger.Debug("ChannelRead:" + message);
         
            if (message is IByteBuffer buffer)
            {
                ushort type = buffer.ReadUnsignedShort();
                if(type == 1)
                {
                    ushort length = buffer.ReadUnsignedShort();
                    playerId = buffer.ReadBytes(length).ToString(Encoding.UTF8);
                    logger.Debug("handshake:" + playerId);

                    //   playerIds.Add(context.Channel.Id.AsShortText(), playerId);
                    //   logger.Warn($"add playerIds:{context.Channel.Id.AsShortText()},playerId:{playerId},total:{playerIds.Count}");
                    Interlocked.Increment(ref count);
                    ChannelManager.Instance.AddChannel(
                        playerId,
                        new PlayerChannel(context.Channel));
                }
                //context.WriteAsync(message);
                //Console.WriteLine("Received from client: " + buffer.ToString(Encoding.UTF8));
            }            
          
        }

        //public override void ChannelRegistered(IChannelHandlerContext context)
        //{

        //    Console.Out.WriteLine("ChannelRegistered:" + context.Channel.Id.AsLongText());
        //    //ChannelManager.Instance.AddChannel(
        //    //    context.Channel.Id.AsLongText(),
        //    //    new PlayerChannel(context.Channel));
        //    base.ChannelRegistered(context);
        //}

        public override void ChannelUnregistered(IChannelHandlerContext context)
        {
            if (playerId != null)
            {
                Interlocked.Decrement(ref count);
                logger.Debug($"ChannelUnregistered:{count}");

                ChannelManager.Instance.RemoveChannel(playerId);
                playerId = null;
            }
        }

        public override void ChannelReadComplete(IChannelHandlerContext context) => context.Flush();

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            if (exception is System.Net.Sockets.SocketException)
            {
                logger.Info(exception);

                if (playerId != null)
                {
                    Interlocked.Decrement(ref count);
                    logger.Debug($"ExceptionCaught:{count}");

                    ChannelManager.Instance.RemoveChannel(playerId);
                    playerId = null;
                }
            }
            else
            {
                logger.Error(exception);
            }


            context.CloseAsync();
        }
    }
}
