﻿using NLog;
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

        /// <summary>
        /// 创建session
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IFSSession> CreateSession(string ip, int port, string id)
        {
            var sessionIce = await networkIce.CreateSession(ip, port, id);

            //创建netty连接
            IFSChannel channel = null;
            if(networkNetty != null)
            {
                var host = parseHost(sessionIce.SessionPrx.ice_getConnection().getEndpoint().ToString());      
                channel = await networkNetty.ConnectAsync(host,id);         
            }

            return new FSSession(id,sessionIce, channel);
        }

        /// <summary>
        /// 启动客户端网络
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task StartAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            await networkIce.Start();
            await networkNetty.Start();
        }

        /// <summary>
        /// 停止客户端网络
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task StopAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            await networkIce.Stop();
            await networkNetty.Stop();
            
        }

        /// <summary>
        /// 定时刷新，注意必须要用主线程调用这个函数，以保证所有的网络回调都是在主线程运行
        /// </summary>
        public void Update()
        {
            networkIce.Update();
            networkNetty.Update();
        }
    }
}