using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using NLog;
using System;
using System.Net;
using System.Threading.Tasks;

namespace FootStone.Client
{

    public class NetworkNetty
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private Bootstrap bootstrap;
        private MultithreadEventLoopGroup group;
        private NettyClientOptions options;

        public NetworkNetty(NettyClientOptions options)
        {
            this.options = options;
        }

        public async Task Start()
        {
            group = new MultithreadEventLoopGroup();

            try
            {
                bootstrap = new Bootstrap()
                    .Group(group)
                      //     .Channel<TcpSocketChannel>()
                    .Channel<FSTcpSocketChannel>()
                    .Option(ChannelOption.Allocator, PooledByteBufferAllocator.Default)
                    .Option(ChannelOption.TcpNodelay, true)              
                    .Handler(new ActionChannelInitializer<IFSChannel>(channel =>
                    {
                        IChannelPipeline pipeline = channel.Pipeline;

                        // pipeline.AddLast(new LoggingHandler());
                        pipeline.AddLast("framing-enc", new LengthFieldPrepender(2));
                        pipeline.AddLast("framing-dec", new LengthFieldBasedFrameDecoder(ushort.MaxValue, 0, 2, 0, 2));
                        pipeline.AddLast("socket", new FSSocketHandler());
                    }));
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        public async Task  Stop()
        {
            if(group!= null)
            {
                await group.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));
            }          
        }

        public void Update()
        {

        }
        

        /// <summary>
        /// 建立连接
        /// </summary>
        /// <param name="host"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IFSChannel> ConnectAsync(string host, string id)
        {
            var channel =  await bootstrap.ConnectAsync(new IPEndPoint(IPAddress.Parse(host), options.Port)) as IFSChannel;
            return channel ;
        }
    }
}
