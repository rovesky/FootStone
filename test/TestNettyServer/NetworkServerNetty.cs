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

namespace TestNettyServer
{
    public class NetworkServerNetty
    {
       
        private IChannel boundChannel;
        private MultithreadEventLoopGroup bossGroup;
        private MultithreadEventLoopGroup workerGroup;

        private NLog.Logger logger = LogManager.GetCurrentClassLogger();
            

        public  void Init()
        {
           logger.Info("DotNetty Init!");        
        }

        private byte[] randomBytes(int size)
        {
          //  size = size *100;
            var bytes = new byte[size];
            for (int i = 0; i < size; ++i)
            {
                bytes[i] = (byte)i;
            }
            return bytes;
        }

        public async  Task Start(string host,int port)
        {
            bossGroup = new MultithreadEventLoopGroup(1);
            workerGroup = new MultithreadEventLoopGroup(4);

            try
            {

                List<byte[]> datas = new List<byte[]>();
                for (int i = 0; i < 100; ++i)
                {
                    datas.Add(randomBytes(i));
                }


                var bootstrap = new ServerBootstrap();
                bootstrap.Group(bossGroup, workerGroup)
                         .Channel<TcpServerSocketChannel>()
                //     .Option(ChannelOption.SoBacklog, 100)
                    //  .Option(ChannelOption.SoSndbuf, 100)
                    // .Option(ChannelOption.TcpNodelay, true)
                    //  .Handler(new LoggingHandler("SRV-LSTN"))
                    .Option(ChannelOption.Allocator, PooledByteBufferAllocator.Default)
                    .ChildHandler(new ActionChannelInitializer<IChannel>(channel =>
                    {
                        IChannelPipeline pipeline = channel.Pipeline;

                     //    pipeline.AddLast(new LoggingHandler("SRV-CONN"));
                        pipeline.AddLast("framing-enc", new LengthFieldPrepender(2));
                        pipeline.AddLast("framing-dec", new LengthFieldBasedFrameDecoder(ushort.MaxValue, 0, 2, 0, 2));
                        pipeline.AddLast("echo", new SocketServerHandler(datas));
                    }));


                //    boundChannel = await bootstrap.BindAsync(new IPEndPoint(IPAddress.Parse(host), port));

                boundChannel = await bootstrap.BindAsync(port);
                logger.Info($"DotNetty Started:{port}" );
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }        
        }

        public async  Task Stop()
        {
            logger.Info("DotNetty Stopped!");
            await boundChannel.CloseAsync();
            await Task.WhenAll(
                 bossGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)),
                 workerGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)));          
        }
    }


    class SocketServerHandler : ChannelHandlerAdapter
    {
        private NLog.Logger logger = LogManager.GetCurrentClassLogger();

        private string playerId;
        private System.Timers.Timer pingTimer;

        private static int count = 0;
        private static int msgCount = 0;
        private Random random;
        private List<byte[]> datas;

        public SocketServerHandler(List<byte[]> datas)
        {
            this.random = new Random();
            this.datas = datas;
            //  logger.Warn("SocketServerHandler Constructor!");
        }

    

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
         //   logger.Debug("ChannelRead:" + message);

            if (message is IByteBuffer buffer)
            {
                ushort type = buffer.ReadUnsignedShort();
                if (type == 1)
                {
                    ushort length = buffer.ReadUnsignedShort();
                    playerId = buffer.ReadBytes(length).ToString(Encoding.UTF8);
                    logger.Debug("Handshake:" + playerId);

                    Interlocked.Increment(ref count);

                    pingTimer = new System.Timers.Timer();
                    pingTimer.AutoReset = true;
                    pingTimer.Interval = 33;
                    pingTimer.Enabled = true;
                    pingTimer.Elapsed += (sender, e)=>{

                        var rand = random.Next() % 200;
                        if (rand < 60)
                        {
                            int size = rand ;
                            var data = datas[size];
                         //   IByteBuffer byteBuffer = Unpooled.DirectBuffer(size);                         
                            IByteBuffer byteBuffer = context.Allocator.DirectBuffer(size);
                            byteBuffer.WriteBytes(data);
                            context.WriteAndFlushAsync(byteBuffer);

                            Interlocked.Increment(ref msgCount);
                            if (msgCount % (30 * count) == 0)
                            {
                                logger.Info("Send to client msg count: " + msgCount + ",msg length:" + byteBuffer.Capacity);
                            }  
                        }
                    };
                    pingTimer.Start();
                }
            }

            base.ChannelRead(context, message);

        }
     

        public override void HandlerRemoved(IChannelHandlerContext context)
        {
            if (pingTimer != null)
                pingTimer.Close();
            if (playerId != null)
            {
                Interlocked.Decrement(ref count);
                logger.Debug($"HandlerRemoved:{count}");
                playerId = null;
            }
            base.HandlerRemoved(context);
        }

    //    public override void ChannelReadComplete(IChannelHandlerContext context) => context.Flush();

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
