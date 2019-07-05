using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using Ice;
using NLog;
using System.Threading;
using System.Threading.Tasks;

namespace FootStone.Client
{
    public class FSClient : IFSClient
    {
        private static NLog.Logger logger = LogManager.GetCurrentClassLogger();

        private NetworkIce networkIce;
        private NetworkNetty networkNetty;

        public FSClient(IceClientOptions iceInitData, Bootstrap nettyBootstrap)
        {
            networkIce = new NetworkIce(iceInitData);
            networkNetty = new NetworkNetty(nettyBootstrap);
  
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
            IChannel channel = null;
            if(networkNetty != null)
            {
                var host = parseHost(sessionIce.SessionPrx.ice_getConnection().getEndpoint().ToString());
            //    logger.Debug("ConnectNetty begin(" + host + ")");
                channel = await networkNetty.ConnectAsync(host, 8007, id);
           //     logger.Debug("ConnectNetty end(" + host + ")");
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