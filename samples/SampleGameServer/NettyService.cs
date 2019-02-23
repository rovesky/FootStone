using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Handlers.Logging;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using FootStone.Core.GrainInterfaces;
using FootStone.Core.Grains;
using FootStone.Protocol;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Concurrency;
using Orleans.Core;
using Orleans.Runtime;
using Orleans.Streams;
using System;
using System.Text;
using System.Threading.Tasks;

namespace FootStone.Core.GameServer
{
    [Reentrant]
    public class NettyService : GrainService,INettyServiceClient
    {
        readonly IGrainFactory GrainFactory;

   
    //    private int operationTimes = 0;
        private IStreamProvider streamProvider;
        private IChannel boundChannel;
        private MultithreadEventLoopGroup bossGroup;
        private MultithreadEventLoopGroup workerGroup;

        public NettyService(IServiceProvider services, IGrainIdentity id, Silo silo, ILoggerFactory loggerFactory, IGrainFactory grainFactory) 
            : base(id, silo, loggerFactory)
        {         
            GrainFactory = grainFactory;
        }

        public Task AddOptionTime(int time)
        {
          //  operationTimes += time;
            return Task.CompletedTask;
        }

        public override Task Init(IServiceProvider serviceProvider)
        {
            Console.WriteLine("----------SocketService Init!");

            ////启动FastStream
            //FastStreamConfig fastStreamConfig = new FastStreamConfig();       
            //fastStreamConfig.host = this.Silo.Endpoint.Address.ToString();
            //fastStreamConfig.port = 20010;
            //FastStream.Instance.Init(fastStreamConfig);
         
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
                    .Handler(new LoggingHandler("SRV-LSTN"))
                    .ChildHandler(new ActionChannelInitializer<IChannel>(channel =>
                    {
                        IChannelPipeline pipeline = channel.Pipeline;

                        pipeline.AddLast(new LoggingHandler("SRV-CONN"));
                        pipeline.AddLast("framing-enc", new LengthFieldPrepender(2));
                        pipeline.AddLast("framing-dec", new LengthFieldBasedFrameDecoder(ushort.MaxValue, 0, 2, 0, 2));

                        pipeline.AddLast("echo", new SocketServerHandler());
                    }));

                string host = Silo.Endpoint.Address.ToString();
                boundChannel = await bootstrap.BindAsync(
                    // host,
                     8007);
                Console.Out.WriteLine("netty started!");
            }
            catch(Exception ex)
            {
                Console.Error.WriteLine(ex.StackTrace);
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
           
            await boundChannel.CloseAsync();
            await Task.WhenAll(
                 bossGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)),
                 workerGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)));

            await base.Stop();        

        }
       
    }


    class SocketServerHandler : ChannelHandlerAdapter
    {
        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            Console.Out.WriteLine("ChannelRead:" + message);
            //if (message is MsgHandShakeRequest handshake)
            //{
            //    Console.Out.WriteLine("handshake:" + handshake.playerId);
            //    ChannelManager.Instance.AddChannel(
            //        handshake.playerId,
            //        new PlayerChannel(context.Channel));
            //}
            // context.
          //  var buffer = message as IByteBuffer;
            if (message is IByteBuffer buffer)
            {
                ushort type = buffer.ReadUnsignedShort();
                if(type == 1)
                {
                    ushort length = buffer.ReadUnsignedShort();
                    string playerId = buffer.ReadBytes(length).ToString(Encoding.UTF8);
                    Console.Out.WriteLine("handshake:" + playerId);
                    ChannelManager.Instance.AddChannel(
                        playerId,
                        new PlayerChannel(context.Channel));
                }

                //Console.WriteLine("Received from client: " + buffer.ToString(Encoding.UTF8));


           // buffer.rel
            }
            
            //context.WriteAsync(message);
        }

        //public override void ChannelRegistered(IChannelHandlerContext context)
        //{

        //    Console.Out.WriteLine("ChannelRegistered:" + context.Channel.Id.AsLongText());
        //    //ChannelManager.Instance.AddChannel(
        //    //    context.Channel.Id.AsLongText(),
        //    //    new PlayerChannel(context.Channel));
        //    base.ChannelRegistered(context);
        //}

        //public override void ChannelUnregistered(IChannelHandlerContext context)
        //{
        //  //  ChannelManager.Instance.RemoveChannel(context.Channel.Id.AsLongText());
        //}

        public override void ChannelReadComplete(IChannelHandlerContext context) => context.Flush();

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            Console.WriteLine("Exception: " + exception);
            context.CloseAsync();
        }
    }
}
