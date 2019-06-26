using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Common.Utilities;
using DotNetty.Handlers.Logging;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace FootStone.Core.Client
{

    struct Point2D
    {
        int x;
        int y;
    }

    [Serializable]
    struct Move
    {
        byte direction;
        byte speed;


    }

    class SocketNettyHandler : ChannelHandlerAdapter
    {
   
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
     //   private static int msgCount = 0;
        public static int playerCount = 0;

        public static ConcurrentQueue<IByteBuffer> msgQueue = new ConcurrentQueue<IByteBuffer>();

        public TaskCompletionSource<object> tcsConnected = new TaskCompletionSource<object>();
        public TaskCompletionSource<object> tcsBindSiloed = new TaskCompletionSource<object>();

        public SocketNettyHandler()
        {         
            Interlocked.Increment(ref playerCount);
            logger.Debug($"new SocketNettyHandler:{playerCount}! ");
        }

        public override void ChannelActive(IChannelHandlerContext context)
        {

            base.ChannelActive(context);
        }


        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            var buffer = message as IByteBuffer;
            if (buffer != null)
            {           
             
                ushort type = buffer.ReadUnsignedShort();
                if(type == 1)
                {
                    tcsConnected.SetResult(null);
                }
                else if(type == 2)
                {
                    tcsBindSiloed.SetResult(null);
                }
                else if (type == 10)
                {
                    msgQueue.Enqueue(buffer);
                    return;
                }
             
            }
            base.ChannelRead(context, message);
        }

       // public override void ChannelReadComplete(IChannelHandlerContext context) => context.Flush();

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            logger.Error(exception);
            context.CloseAsync();

            base.ExceptionCaught(context, exception);
        }
    }



    public class NetworkClientNetty
    {
        private Bootstrap bootstrap;
        private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private MultithreadEventLoopGroup group;
        private System.Timers.Timer pingTimer;
        private int msgCount = 0;

        public NetworkClientNetty()
        {

        }

        public void Init()
        {
            group = new MultithreadEventLoopGroup();

            try
            {
                bootstrap = new Bootstrap();
                bootstrap
                    .Group(group)
                    .Channel<TcpSocketChannel>()
                    .Option(ChannelOption.Allocator, PooledByteBufferAllocator.Default)
                  //  .Option(ChannelOption.TcpNodelay, true)
                    .Handler(new ActionChannelInitializer<ISocketChannel>(channel =>
                    {
                        IChannelPipeline pipeline = channel.Pipeline;

                        // pipeline.AddLast(new LoggingHandler());
                        pipeline.AddLast("framing-enc", new LengthFieldPrepender(2));
                        pipeline.AddLast("framing-dec", new LengthFieldBasedFrameDecoder(ushort.MaxValue, 0, 2, 0, 2));
                        pipeline.AddLast("echo", new SocketNettyHandler());
                    }));

                pingTimer = new System.Timers.Timer();
                pingTimer.AutoReset = true;
                pingTimer.Interval = 10;
                pingTimer.Enabled = true;
                pingTimer.Elapsed += (_1, _2) =>
                {
                    try
                    {
                        IByteBuffer[] buffers = SocketNettyHandler.msgQueue.ToArray();
                        SocketNettyHandler.msgQueue.Clear();

                        foreach (var buffer in buffers)
                        {
                            msgCount++;

                            if (msgCount % (10 * SocketNettyHandler.playerCount) == 0)
                            {
                                logger.Info("Received from server msg count: " + msgCount + ",msg length:" + buffer.ReadableBytes);
                            }

                            //int len = buffer.ReadableBytes;
                            //for (int i = 0; i < len / 7; ++i)
                            //{
                            //    var size = buffer.ReadUnsignedShort();
                            //    var msg = buffer.ReadString(size, Encoding.UTF8);

                            //    logger.Debug($"{msgCount}-{i}Received from server msg:{msg}");
                            //}
                            ReferenceCountUtil.Release(buffer);
              
                        }
                    }
                    catch(Exception e)
                    {
                        logger.Error(e);
                    }
                };
                pingTimer.Start();
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

        public async Task  Fini()
        {
            if(group!= null)
            {
                await group.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));
            }

          
        }
        public async Task<IChannel> ConnectNettyAsync(string host, int port, string playerId)
        {
            var channel =  await bootstrap.ConnectAsync(new IPEndPoint(IPAddress.Parse(host), port));
      
            var message = channel.Allocator.Buffer(50);
            message.WriteUnsignedShort(1);
            message.WriteUnsignedShort((ushort)playerId.Length);
            message.WriteString(playerId, Encoding.UTF8);
            await channel.WriteAndFlushAsync(message);

            var handler = channel.Pipeline.Get<SocketNettyHandler>();
            await handler.tcsConnected.Task;          

            return channel;
        }

        public async Task BindGameServer(IChannel channel, string playerId,string gameServerId)
        {          
            var data = channel.Allocator.Buffer(100);
            data.WriteUnsignedShort(2);

            data.WriteUnsignedShort((ushort)playerId.Length);
            data.WriteString(playerId, Encoding.UTF8);

            data.WriteUnsignedShort((ushort)gameServerId.Length);
            data.WriteString(gameServerId, Encoding.UTF8);
            await channel.WriteAndFlushAsync(data);

            var handler = channel.Pipeline.Get<SocketNettyHandler>();
            await handler.tcsBindSiloed.Task;        
        }

        public async Task SendMessage(IChannel channel, string message)
        {
            var data = channel.Allocator.Buffer(100);
            data.WriteUnsignedShort(10);
            data.WriteUnsignedShort((ushort)(message.Length+2));
            data.WriteUnsignedShort((ushort)message.Length);
            data.WriteString(message, Encoding.UTF8);

            await channel.WriteAndFlushAsync(data);
        }
    }
}
