using DotNetty.Transport.Bootstrapping;
using Ice;
using System.Threading;
using System.Threading.Tasks;

namespace FootStone.Client
{
    public class FSClient : IFSClient
    {
   
        private ClientIce clientIce;
        private ClientNetty clientNetty;

        public FSClient(Ice.InitializationData iceInitData, Bootstrap nettyBootstrap)
        {
            clientIce = new ClientIce(iceInitData);
            clientNetty = new ClientNetty(nettyBootstrap);
  
        }

        public async Task<IFSSession> CreateSession(string ip, int port, string id)
        {
            var session = await clientIce.CreateSession(ip, port, id);
            if(clientNetty != null)
            {
                
            }

            return new FSSession();
        }

        public async Task StartAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            await clientIce.Start();
            await clientNetty.Start();
        }

        public async Task StopAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            await clientIce.Stop();
            await clientNetty.Stop();
            
        }

        public void Update()
        {
            clientIce.Update();
            clientNetty.Update();
        }
    }
}