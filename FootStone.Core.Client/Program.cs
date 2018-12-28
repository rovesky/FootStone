using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Handlers.Logging;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using FootStone.GrainInterfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FootStone.Core.Client
{
    class Program
    {
        class SocketNettyHandler : ChannelHandlerAdapter
        {
            private IByteBuffer initialMessage;

            public SocketNettyHandler()
            {
                this.initialMessage = Unpooled.Buffer(100);
                byte[] messageBytes = Encoding.UTF8.GetBytes("Hello world");
                this.initialMessage.WriteBytes(messageBytes);
            }

            public override void ChannelActive(IChannelHandlerContext context) => 
                context.WriteAndFlushAsync(this.initialMessage);


            public override void ChannelRead(IChannelHandlerContext context, object message)
            {
                // context.
                var buffer = message as IByteBuffer;
               // buffer.Array
                if (buffer != null)
                {
                    Console.WriteLine("Received from server: " + context.Channel.Id.ToString());
                }
             //   context.WriteAsync(message);
            }

            public override void ChannelReadComplete(IChannelHandlerContext context) => context.Flush();

            public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
            {
                Console.WriteLine("Exception: " + exception);
                context.CloseAsync();
            }
        }

        static async Task<IChannel> ConnectNettyAsync(string host,int port)
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

                        pipeline.AddLast("echo", new SocketNettyHandler());
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


        static void Main(string[] args)
        {
            //  Test();
            try
            {
              //  ConnectNettyAsync("127.0.0.1", 8007).Wait();
             
                Test(1).Wait();
                Console.WriteLine("OK!");
            }
            catch(Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
            }
        }

        private static async Task Test(int count)
        {
            NetworkIce.Instance.Init("192.168.3.28", 4061);
            for (int i = 0; i < count; ++i)
            {
                runTest(i, 1000);
                await Task.Delay(20);
            }
            Console.Out.WriteLine("all session created:" + count);
        }

        private static async Task runTest(int index,int count)
        {
            var sessionId = "session" + index;
            var account = "account"+index;
            var password = "111111";
            var playerName = "player"+index;
         
            var sessionPrx = await NetworkIce.Instance.CreateSession(sessionId);
          //  Console.Out.WriteLine("NetworkIce.Instance.CreateSession ok:"+ account);

            var accountPrx = AccountPrxHelper.uncheckedCast(sessionPrx, "account");
            try
            {
                await accountPrx.RegisterRequestAsync(new RegisterInfo(account, password));
                Console.Out.WriteLine("RegisterRequest ok:" + account);
            }
            catch(Exception ex)
            {
                Console.Out.WriteLine("RegisterRequest fail:" + ex.Message);
            }
         

            await accountPrx.LoginRequestAsync(new LoginInfo(account, password));
            Console.Out.WriteLine("LoginRequest ok:" + account);

            List<ServerInfo> servers = await accountPrx.GetServerListRequestAsync();

            if(servers.Count == 0)
            {
                Console.Error.WriteLine("server list is empty!");
                return ;
            }

            var serverId = servers[0].id;

            List<PlayerShortInfo> players = await accountPrx.GetPlayerListRequestAsync(serverId);
            if (players.Count == 0)
            {
                var playerId = await accountPrx.CreatePlayerRequestAsync(playerName, serverId);
                players = await accountPrx.GetPlayerListRequestAsync(serverId);         
            }

            await accountPrx.SelectPlayerRequestAsync(players[0].playerId);

            var playerPrx = IPlayerPrxHelper.uncheckedCast(sessionPrx, "player");
            var roleMasterPrx = IRoleMasterPrxHelper.uncheckedCast(sessionPrx, "roleMaster");
            var zonePrx = IZonePrxHelper.uncheckedCast(sessionPrx, "zone");

            var playerInfo = await playerPrx.GetPlayerInfoAsync();
            var endPoint = await zonePrx.PlayerEnterAsync(playerInfo.zoneId);
            Console.Out.WriteLine("ConnectNetty begin(" + endPoint.ip+":"+endPoint.port+")");

            var channel = await ConnectNettyAsync(endPoint.ip, endPoint.port);
            Console.Out.WriteLine("PlayerBind begin:" + channel.Id.AsLongText());

            await zonePrx.PlayerBindChannelAsync(channel.Id.AsLongText());
            Console.Out.WriteLine("PlayerBind end:" + channel.Id.AsLongText());

            // channel.Id.AsLongText
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
            Console.Out.WriteLine("playerInfo:" + JsonConvert.SerializeObject(playerInfo));
            await channel.CloseAsync();
        }

      
    }
}
