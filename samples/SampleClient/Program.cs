using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Handlers.Logging;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using FootStone.GrainInterfaces;
using Newtonsoft.Json;
using FootStone.Protocol;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Timers;
using NLog;
using FootStone.FrontNetty;

namespace FootStone.Core.Client
{
    class Program
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        class SocketNettyHandler : ChannelHandlerAdapter
        {
            // private MsgHandShakeRequest initialMessage;
            private IByteBuffer initialMessage;
            private string playerId;

            private static int msgCount = 0;

            public SocketNettyHandler(string playerId)
            {
                //  this.initialMessage = new IByteBuffer();
                //   this.initialMessage.playerId = playerId;
                this.playerId = playerId;
                this.initialMessage = Unpooled.Buffer(100);
                byte[] messageBytes = Encoding.UTF8.GetBytes(playerId);
                this.initialMessage.WriteUnsignedShort(1);
                this.initialMessage.WriteUnsignedShort((ushort)messageBytes.Length);
                this.initialMessage.WriteBytes(messageBytes);
            }

            public override void ChannelActive(IChannelHandlerContext context)
            {
             //   Console.WriteLine("ChannelActive: " + this.playerId);
                context.WriteAndFlushAsync(this.initialMessage);
            }
            

            public override void ChannelRead(IChannelHandlerContext context, object message)
            {          
                var buffer = message as IByteBuffer;
                if (buffer != null)
                {
                    Interlocked.Increment(ref msgCount);
                    if (msgCount % 10000 == 0)                    {

                        logger.Info("Received from server msg count: " + msgCount + ",msg length:" + buffer.Capacity);
                    }                  
                }               
            }

            public override void ChannelReadComplete(IChannelHandlerContext context) => context.Flush();

            public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
            {
                logger.Error(exception);
                context.CloseAsync();
            }
        }

        static private string parseHost(string endPoint)
        {
            var strs =  endPoint.Split(' ');
            for(int i = 0; i < strs.Length; ++i)
            {
                if(strs[i] == "-h")
                {
                    return strs[i + 1];
                }
            }
            return "";
        }

        static async Task<IChannel> ConnectNettyAsync(string host,int port,string playerId)
        {
         //   ExampleHelper.SetConsoleLogger();

            var group = new MultithreadEventLoopGroup();

            //X509Certificate2 cert = null;
            //string targetHost = null;
            //if (ClientSettings.IsSsl)
            //{
            //    cert = new X509Certificate2(Path.Combine(ExampleHelper.ProcessDirectory, "dotnetty.com.pfx"), "password");
            //    targetHost = cert.GetNameInfo(X509NameType.DnsName, false);
            //}
            try
            {
                var bootstrap = new Bootstrap();
                bootstrap
                    .Group(group)
                    .Channel<TcpSocketChannel>()
                    .Option(ChannelOption.TcpNodelay, true)
                    .Handler(new ActionChannelInitializer<ISocketChannel>(channel =>
                    {
                        IChannelPipeline pipeline = channel.Pipeline;

                       // pipeline.AddLast(new LoggingHandler());
                        pipeline.AddLast("framing-enc", new LengthFieldPrepender(2));
                        pipeline.AddLast("framing-dec", new LengthFieldBasedFrameDecoder(ushort.MaxValue, 0, 2, 0, 2));

                        pipeline.AddLast("echo", new SocketNettyHandler(playerId));
                    }));

                return await bootstrap.ConnectAsync(new IPEndPoint(IPAddress.Parse(host),port));
            }
            catch(Exception ex)
            {
                logger.Error(ex);
                return null;
            }
            finally
            {
              //  await group.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));
            }
        }


        static  void Main(string[] args)
        {
            //  Test();
            try
            {
                //  ConnectNettyAsync("127.0.0.1", 8007,"11").Wait();
                int count = args.Length > 0? int.Parse(args[0]) : 1;
                ushort startIndex = args.Length > 1 ? ushort.Parse(args[1]) : (ushort)0;
                bool needNetty = args.Length > 2 ? bool.Parse(args[2]) : true;

                Test(count, startIndex, needNetty).Wait();
                //logger.Info("Test OK!");

                Console.ReadLine();
            }
            catch(Exception ex)
            {
                logger.Error(ex);
            }
        }

        private static List<NetworkClientIce> clients = new List<NetworkClientIce>();

        private static async Task Test(int count,ushort startIndex, bool needNetty)
        {       
            NetworkClientIce clientIce = new NetworkClientIce();          
            clientIce.Init("192.168.0.128", 4061);
            clients.Add(clientIce);

            NetworkClientNetty clientNetty = null;
            if (needNetty)
            {
                clientNetty = new NetworkClientNetty();
                clientNetty.Init();
            }

            for (ushort i = startIndex; i < startIndex+ count; ++i)
            {
                if(i%100 == 0)
                {
                    clientIce = new NetworkClientIce();
                    clientIce.Init("192.168.0.128", 4061);
                    clients.Add(clientIce);
                }
                runTest(i, 20, clientIce, clientNetty);
                await Task.Delay(20);
            }
            logger.Info("all session created:" + count);
        }

        private static async Task runTest(ushort index,int count, NetworkClientIce iceClient,NetworkClientNetty netty)
        {
            try
            {              
                var sessionId = "session" + index;
                var account = "account" + index;
                var password = "111111";
                var playerName = "player" + index;

                //创建连接
                var sessionPrx = await iceClient.CreateSession(sessionId);
                //  Console.Out.WriteLine("NetworkIce.Instance.CreateSession ok:"+ account);

                //注册账号
                var accountPrx = AccountPrxHelper.uncheckedCast(sessionPrx, "account");
                try
                {
                    await accountPrx.RegisterRequestAsync(account,new RegisterInfo(account, password));
                    logger.Debug("RegisterRequest ok:" + account);
                }
                catch (Exception ex)
                {
                    logger.Debug("RegisterRequest fail:" + ex.Message);
                }

                //账号登录
                await accountPrx.LoginRequestAsync(account, password);
                logger.Debug("LoginRequest ok:" + account);

                //选择服务器
                var worldPrx = WorldPrxHelper.uncheckedCast(sessionPrx, "world");
                List<ServerInfo> servers = await worldPrx.GetServerListRequestAsync();

                if (servers.Count == 0)
                {
                    Console.Error.WriteLine("server list is empty!");
                    return;
                }

                //选取第一个服务器
                var serverId = servers[0].id;

                //获取角色列表
                List<PlayerShortInfo> players = await worldPrx.GetPlayerListRequestAsync(serverId);
                var playerPrx = IPlayerPrxHelper.uncheckedCast(sessionPrx, "player");

                //如果角色列表为0，创建新角色
                if (players.Count == 0)
                {
                    var playerId = await playerPrx.CreatePlayerRequestAsync(serverId, new PlayerCreateInfo(playerName, 1));
                    players = await worldPrx.GetPlayerListRequestAsync(serverId);
                }
                //选择第一个角色
                await playerPrx.SelectPlayerRequestAsync(players[0].playerId);

                var roleMasterPrx = IRoleMasterPrxHelper.uncheckedCast(sessionPrx, "roleMaster");
                var zonePrx = IZonePrxHelper.uncheckedCast(sessionPrx, "zone");

                //获取角色信息
                var playerInfo = await playerPrx.GetPlayerInfoAsync();
                logger.Debug($"{account} playerInfo:" + JsonConvert.SerializeObject(playerInfo));

                IChannel channel = null;
                System.Timers.Timer pingTimer = null;
                if (netty != null)
                {
                    //连接Netty
                    var host = parseHost(sessionPrx.ice_getConnection().getEndpoint().ToString());
                    logger.Debug("ConnectNetty begin(" + host + ")");
                    channel = await netty.ConnectNettyAsync(host, 8007, playerInfo.playerId);
                    logger.Debug("ConnectNetty end(" + host + ")");

                    //绑定Zone
                    var endPoint = await zonePrx.BindZoneAsync(playerInfo.zoneId, playerInfo.playerId);
                    var gameServerId = FrontNettyUtility.Endpoint2GameServerId(endPoint.ip ,endPoint.port);
                    await netty.BindGameServer(channel, playerInfo.playerId, gameServerId);     
               
                    //进入Zone
                    await zonePrx.PlayerEnterAsync();

                    //发送消息
                    pingTimer = new System.Timers.Timer();
                    pingTimer.AutoReset = true;
                    pingTimer.Interval = 500;
                    pingTimer.Enabled = true;
                    pingTimer.Elapsed += (_1,_2)=>
                    {
                        netty.SendMove(channel,index);
                    };
                    pingTimer.Start();                 
                }

                logger.Info($"{account} playerPrx begin!" );
                MasterProperty property;
                for (int i = 0; i < count; ++i)
                {
                    await playerPrx.SetPlayerNameAsync(playerName + "_" + i);
                    await Task.Delay(3000);
                    property = await roleMasterPrx.GetPropertyAsync();
                    await Task.Delay(5000);
                    //     Console.Out.WriteLine("property" + JsonConvert.SerializeObject(property));
                    playerInfo = await playerPrx.GetPlayerInfoAsync();
                    await Task.Delay(10000);
                }
                logger.Info($"{account} playerInfo:" + JsonConvert.SerializeObject(playerInfo));

                logger.Info($"{account} playerPrx end!");
                sessionPrx.begin_Destroy();
                if (channel != null)
                {
                    await channel.CloseAsync();
                }

                if(pingTimer != null)
                {
                    pingTimer.Stop();
                }
                iceClient.Fini();
              
            }
            catch(System.Exception e)
            {
                logger.Error(e);
            }
        }      
    }
}
