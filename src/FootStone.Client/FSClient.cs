using DotNetty.Transport.Channels;
using NLog;
using System.Threading;
using System.Threading.Tasks;

namespace FootStone.Client
{
    public class FSClient : IFSClient
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private NetworkIce networkIce;
        private NetworkNetty networkNetty;

        public FSClient(IceClientOptions iceOptions, NettyClientOptions nettyOptions)
        {
            networkIce = new NetworkIce(iceOptions);
            networkNetty = new NetworkNetty(nettyOptions);  
        }

        private string parseHost(string endPoint)
        {
            var strs = endPoint.Split(' ');
            for (int i = 0; i < strs.Length; ++i)
            {
                if (strs[i] == "-h")
                {
                    return strs[i + 1];
                }
            }
            return "";
        }

        public async Task<IFSSession> CreateSession(string ip, int port, string id)
        {
            var sessionIce = await networkIce.CreateSession(ip, port, id);

            //创建netty连接
            IChannel channel = null;
            if(networkNetty != null)
            {
                var host = parseHost(sessionIce.SessionPrx.ice_getConnection().getEndpoint().ToString());      
                channel = await networkNetty.ConnectAsync(host,id);         
            }

            return new FSSession(id,sessionIce, channel);
        }

        public async Task StartAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            await networkIce.Start();
            await networkNetty.Start();
        }

        public async Task StopAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            await networkIce.Stop();
            await networkNetty.Stop();
            
        }

        public void Update()
        {
            networkIce.Update();
            networkNetty.Update();
        }
    }
}