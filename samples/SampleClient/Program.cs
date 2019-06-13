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
using NLog;

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
                Console.WriteLine("ChannelActive: " + this.playerId);
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

                        pipeline.AddLast(new LoggingHandler());
                        pipeline.AddLast("framing-enc", new LengthFieldPrepender(2));
                        pipeline.AddLast("framing-dec", new LengthFieldBasedFrameDecoder(ushort.MaxValue, 0, 2, 0, 2));

                        pipeline.AddLast("echo", new SocketNettyHandler(playerId));
                    }));

                return await bootstrap.ConnectAsync(new IPEndPoint(IPAddress.Parse(host),port));
            }
            catch(Exception ex)
            {
                Console.Error.WriteLine(ex.Message+ex.StackTrace);
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
             
                Test(count).Wait();
                //logger.Info("Test OK!");

                Console.ReadLine();
            }
            catch(Exception ex)
            {
                logger.Error(ex);
            }
        }

        private static List<NetworkIceClient> clients = new List<NetworkIceClient>();

        private static async Task Test(int count)
        {
            //  NetworkIceClient.Instance.Init("192.168.0.128", 4061);
            NetworkIceClient client = new NetworkIceClient();
            client.Init("192.168.0.128", 4061);
            clients.Add(client);
            for (int i = 0; i < count; ++i)
            {
                if(i%100 == 0)
                {
                    client = new NetworkIceClient();
                    client.Init("192.168.0.128", 4061);
                    clients.Add(client);
                }
                runTest(i, 30, client);
                await Task.Delay(20);
            }
            logger.Info("all session created:" + count);
        }

        private static async Task runTest(int index,int count, NetworkIceClient iceClient)
        {
            try
            {
                //NetworkIceClient iceClient;
                //if (newClient)
                //{
                //    iceClient = new NetworkIceClient();
                //    iceClient.Init("192.168.0.128", 4061);
                //}
                //else
                //{
                //    iceClient = NetworkIceClient.Instance;
                //}             
               
                
                //   index = 86;
                var sessionId = "session" + index;
                var account = "account" + index;
                var password = "111111";
                var playerName = "player" + index;

                var sessionPrx = await iceClient.CreateSession(sessionId);
                //  Console.Out.WriteLine("NetworkIce.Instance.CreateSession ok:"+ account);

                var accountPrx = AccountPrxHelper.uncheckedCast(sessionPrx, "account");
                try
                {
                    await accountPrx.RegisterRequestAsync(account,new RegisterInfo(account, password));
                    logger.Info("RegisterRequest ok:" + account);
                }
                catch (Exception ex)
                {
                    logger.Info("RegisterRequest fail:" + ex.Message);
                }

             //   await accountPrx.TestLoginRequestAsync("11", "22", new Sample.SampleLoginData("code1"));

                await accountPrx.LoginRequestAsync(account, password);
                logger.Info("LoginRequest ok:" + account);

                var worldPrx = WorldPrxHelper.uncheckedCast(sessionPrx, "world");
                List<ServerInfo> servers = await worldPrx.GetServerListRequestAsync();

                if (servers.Count == 0)
                {
                    Console.Error.WriteLine("server list is empty!");
                    return;
                }

                var serverId = servers[0].id;

                List<PlayerShortInfo> players = await worldPrx.GetPlayerListRequestAsync(serverId);
                var playerPrx = IPlayerPrxHelper.uncheckedCast(sessionPrx, "player");

                if (players.Count == 0)
                {
                    var playerId = await playerPrx.CreatePlayerRequestAsync(serverId, new PlayerCreateInfo(playerName, 1));
                    players = await worldPrx.GetPlayerListRequestAsync(serverId);
                }

           //     var playerId = await playerPrx.CreatePlayerRequestAsync(serverId, new PlayerCreateInfo(playerName, 1));

              //  await playerPrx.GetPlayerInfoAsync();

              //  await playerPrx.SelectPlayerRequestAsync(playerId);
                await playerPrx.SelectPlayerRequestAsync(players[0].playerId);

                var roleMasterPrx = IRoleMasterPrxHelper.uncheckedCast(sessionPrx, "roleMaster");
                var zonePrx = IZonePrxHelper.uncheckedCast(sessionPrx, "zone");

                var playerInfo = await playerPrx.GetPlayerInfoAsync();
                //var endPoint = await zonePrx.PlayerEnterAsync(playerInfo.zoneId);
                // Console.Out.WriteLine("ConnectNetty begin(" + endPoint.ip + ":" + endPoint.port + ")");

                // var channel = await ConnectNettyAsync(endPoint.ip, endPoint.port, playerInfo.id);
                //  Console.Out.WriteLine("ConnectNetty end(" + endPoint.ip + ":" + endPoint.port + ")");

                //   Console.Out.WriteLine("PlayerBind begin:" + channel.Id.AsLongText());

                //  await zonePrx.PlayerBindChannelAsync(channel.Id.AsLongText());
                //  Console.Out.WriteLine("PlayerBind end:" + channel.Id.AsLongText());

                // channel.Id.AsLongText

                logger.Info("playerPrx begin!" );
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
                logger.Info("playerInfo:" + JsonConvert.SerializeObject(playerInfo));

                logger.Info("playerPrx end!");
                sessionPrx.begin_Destroy();
                iceClient.Fini();
                //  await channel.CloseAsync();
            }
            catch(System.Exception e)
            {
                logger.Error(e);
            }
        }

      
    }
}
