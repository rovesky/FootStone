using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Common.Utilities;
using DotNetty.Handlers.Logging;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using FootStone.ProtocolNetty;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace FootStone.Client
{
    public static class ByteBufferExtensions
    {

        public static IByteBuffer WritePoint2D(this IByteBuffer buffer, Point2D point2d)
        {
            return buffer.WriteFloat(point2d.x)
                         .WriteFloat(point2d.y);

        }
    }

    [Serializable]
    public struct Point2D
    {
       public  float x;
       public  float y;
   
    }

    [Serializable]
    public struct Move
    {
        public static ushort Type = 0x03;

        public byte direction;
        public byte speed;
        public Point2D point;

        public void Encoder(IByteBuffer buffer)
        {
            buffer.WriteUnsignedShort(Type)
                .WriteByte(direction)
                .WriteByte(speed)
                .WritePoint2D(point);
        }
    }

    class SocketNettyHandler : ChannelHandlerAdapter
    {
   
        private static Logger logger = LogManager.GetCurrentClassLogger();

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

                MessageType type = (MessageType)buffer.ReadUnsignedShort();
                if(type == MessageType.PlayerHandshake)
                {
                    tcsConnected.SetResult(null);
                }
                else if(type == MessageType.PlayerBindGame)
                {
                    tcsBindSiloed.SetResult(null);
                }
                else if (type == MessageType.Ping)
                {
                    var now = DateTime.Now.Ticks;
                    var pingTime = buffer.ReadLong();
                    var timer = (now - pingTime) / 10000;
                    logger.Debug($"ping value:{timer}ms");
                   // tcsBindSiloed.SetResult(null);
                }
                else if (type == MessageType.Data)
                {
                    logger.Debug("recevie Data!");
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



    public class NetworkNetty
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();


        private Bootstrap bootstrap;
        private MultithreadEventLoopGroup group;
        private System.Timers.Timer pingTimer;
      //  private int msgCount = 0;

       // private int printCount = 0;

        public NetworkNetty(Bootstrap bootstrap)
        {
            this.bootstrap = bootstrap;
        }

        public async Task Start()
        {
            group = new MultithreadEventLoopGroup();

            try
            {
              //  this.bootstrap = bootstrap;

                bootstrap
                    .Group(group)
                    .Channel<TcpSocketChannel>()
                    .Option(ChannelOption.Allocator, PooledByteBufferAllocator.Default)
                    .Option(ChannelOption.TcpNodelay, true)
                    .Handler(new ActionChannelInitializer<ISocketChannel>(channel =>
                    {
                        IChannelPipeline pipeline = channel.Pipeline;

                        // pipeline.AddLast(new LoggingHandler());
                        pipeline.AddLast("framing-enc", new LengthFieldPrepender(2));
                        pipeline.AddLast("framing-dec", new LengthFieldBasedFrameDecoder(ushort.MaxValue, 0, 2, 0, 2));
                        pipeline.AddLast("socket", new SocketNettyHandler());
                    }));

                //pingTimer = new System.Timers.Timer();
                //pingTimer.AutoReset = true;
                //pingTimer.Interval = 100;
                //pingTimer.Enabled = true;
                //pingTimer.Elapsed += (_1, _2) =>
                //{
                //    try
                //    {
                //        IByteBuffer[] buffers = SocketNettyHandler.msgQueue.ToArray();
                //     //   SocketNettyHandler.msgQueue.Clear();

              
                //       // logger.Info($"Received buffers.Length: {buffers.Length}");

                //        if (buffers.Length > 0)
                //        {
                //            msgCount += buffers.Length;
                //            printCount++;
                //            if (printCount% 10 == 0)
                //            {
                //                logger.Info("Received from server msg count: " + msgCount + ",msg length:" + buffers[0].ReadableBytes);
                //            }


                //            foreach (var buffer in buffers)
                //            {
                //                ReferenceCountUtil.Release(buffer);
                //            }
                //        }
                //    }
                //    catch (Exception e)
                //    {
                //        logger.Error(e);
                //    }
                //};
                //pingTimer.Start();
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

        public async Task  Stop()
        {
            if(group!= null)
            {
                await group.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));
            }

          
        }

        /// <summary>
        /// 建立连接
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IChannel> ConnectAsync(string host, int port, string id)
        {
            var channel =  await bootstrap.ConnectAsync(new IPEndPoint(IPAddress.Parse(host), port));

            var message = channel.Allocator.DirectBuffer();
            message.WriteUnsignedShort((ushort)MessageType.PlayerHandshake);
            message.WriteStringShortUtf8(id);     
            await channel.WriteAndFlushAsync(message);

            var handler = channel.Pipeline.Get<SocketNettyHandler>();
            await handler.tcsConnected.Task;          

            return channel;
        }

        public async Task BindGameServer(IChannel channel, string playerId,string gameServerId)
        {          
            var data = channel.Allocator.DirectBuffer();
            data.WriteUnsignedShort((ushort)MessageType.PlayerBindGame);

            data.WriteStringShortUtf8(playerId);
            data.WriteStringShortUtf8(gameServerId);
            await channel.WriteAndFlushAsync(data);

            var handler = channel.Pipeline.Get<SocketNettyHandler>();
            await handler.tcsBindSiloed.Task;        
        }

        public void Update()
        {

        }

        public async Task SendMessage(IChannel channel, string message)
        {
            var data = channel.Allocator.DirectBuffer(100);
            data.WriteUnsignedShort((ushort)MessageType.Data);
            data.WriteUnsignedShort((ushort)(message.Length+2));
            data.WriteUnsignedShort((ushort)message.Length);
            data.WriteString(message, Encoding.UTF8);

            await channel.WriteAndFlushAsync(data);
        }

        public async Task SendMove(IChannel channel,ushort actorId)
        {
            var data = channel.Allocator.DirectBuffer(16);
            data.WriteUnsignedShort((ushort)MessageType.Data);
            data.WriteUnsignedShort((ushort)14);

            data.WriteUnsignedShort(actorId);            

            var move = new Move();
            move.direction = 1;
            move.speed = 10;
            move.point.x = 10.6f;
            move.point.y = 300.1f;
            move.Encoder(data);
            await channel.WriteAndFlushAsync(data);
        }

        public async Task SendPing(IChannel channel, ushort actorId)
        {
            var data = channel.Allocator.DirectBuffer(4);
            data.WriteUnsignedShort((ushort)MessageType.Ping);
            data.WriteLong(DateTime.Now.Ticks);   

            await channel.WriteAndFlushAsync(data);
        }
    }
}
