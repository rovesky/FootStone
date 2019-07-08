using DotNetty.Transport.Channels;
using FootStone.Client;
using FootStone.GrainInterfaces;
using FootStone.ProtocolNetty;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Ice;
using System.Threading;

namespace SampleClient
{

    internal class ZonePushI : IZonePushDisp_,IServerPush
    {      
        private int count = 0;
        public ZonePushI()
        {
           
        }
        private NLog.Logger logger = LogManager.GetCurrentClassLogger();
        private SessionPushI sessionPushI;

        public override void ZoneSync(byte[] data, Current current = null)
        {
            //count++;
            //if (count % 330 == 0)
            //{
            //    logger.Info(name + " zone sync:" + count);
            //}
        }

        public string GetFacet()
        {
            return "zonePush";
        }       

        public void setSessionPushI(SessionPushI sessionPushI)
        {
            this.sessionPushI = sessionPushI;
        }
    }

    internal class PlayerPushI : IPlayerPushDisp_, IServerPush
    {    
        private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private SessionPushI sessionPushI;

        public PlayerPushI()
        {

        }

        public string GetFacet()
        {
           return "playerPush";
        }

        public void setSessionPushI(SessionPushI sessionPushI)
        {
            this.sessionPushI = sessionPushI;
        }

        public override void hpChanged(int hp, Current current = null)
        {
            logger.Info(sessionPushI.Account + " hp changed !");
            //Test.HpChangeCount++;
            //if (Test.HpChangeCount % 1000 == 0)
            //{
            //    logger.Info(name + " hp changed::" + Test.HpChangeCount);
            //}

            //      logger.Info(name+" hp changed:" + hp);
        }      
    }

    public class NetworkNew
    {
        private static NLog.Logger logger = LogManager.GetCurrentClassLogger();

        public async Task Test(string ip, int port, int count, ushort startIndex, bool needNetty)
        {
            logger.Info($"begin test,count:${count},startIndex:{startIndex},needNetty:{needNetty}");

            var client = new FSClientBuilder()
                .IceOptions(iceOptions =>
                {
                    iceOptions.PushObjects = new List<Ice.Object>();
                    iceOptions.PushObjects.Add(new PlayerPushI());
                    iceOptions.PushObjects.Add(new ZonePushI());
                })
                .NettyOptions(nettyOptions =>
                {
                    nettyOptions.Port = 8007;
                })
                .Build();

            Thread thread = new Thread(new ThreadStart(  () =>
            {
                do
                {
                    client.Update();   

                    Thread.Sleep(33);
      
                } while (true);
            }));
            thread.Start();


            await client.StartAsync();

            for (ushort i = startIndex; i < startIndex + count; ++i)
            {
                var sessionId = "session" + i;
                var session = await client.CreateSession(ip, port, sessionId);
             
                RunSession(session, i, 20, needNetty);
                await Task.Delay(20);
            }
            logger.Info("all session created:" + count);
        }

        private  async Task RunSession(IFSSession session,ushort index, int count,bool needNetty)
        {
            var account = "account" + index;
            var password = "111111";
            var playerName = "player" + index;

        //    bool sessionDestroyed = false;

            try
            {
                session.OnDestroyed += (sender, e) =>
                {
               //     sessionDestroyed = true;
                    logger.Info($"session:{session.GetId()} destroyed!");
                };

                //获取SessionPrx
                var sessionPrx = session.GetSessionPrx();

                //注册账号
                var accountPrx = AccountPrxHelper.uncheckedCast(sessionPrx, "account");
                try
                {
                    await accountPrx.RegisterRequestAsync(account, new RegisterInfo(account, password));
                    logger.Debug("RegisterRequest ok:" + account);
                }
                catch (Ice.Exception ex)
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

                if (needNetty)
                {
                    await RunNetty(session,zonePrx,playerInfo,index);
                }

                logger.Info($"{account} playerPrx begin!");
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
                session.Destory();

            }
            catch (System.Exception e)
            {
                logger.Error(account + ":" + e.ToString());
            }
        }

        private async Task RunNetty(IFSSession session,IZonePrx zonePrx,PlayerInfo playerInfo,int index)
        {
           
            System.Timers.Timer moveTimer = null;
            System.Timers.Timer pingTimer = null;


            //绑定Zone
            var endPoint = await zonePrx.BindZoneAsync(playerInfo.zoneId, playerInfo.playerId);
            var gameServerId = ProtocolNettyUtility.Endpoint2GameServerId(endPoint.ip, endPoint.port);

            var  channel = session.GetStreamChannel();
            await channel.BindGameServer(playerInfo.playerId, gameServerId);

            //进入Zone
            await zonePrx.PlayerEnterAsync();

            //发送move消息
            moveTimer = new System.Timers.Timer();
            moveTimer.AutoReset = true;
            moveTimer.Interval = 500;
            moveTimer.Enabled = true;
            moveTimer.Elapsed += (_1, _2) =>
            {
                var data = channel.Allocator.DirectBuffer(16);
                data.WriteUnsignedShort((ushort)MessageType.Data);
                data.WriteUnsignedShort((ushort)14);
                data.WriteUnsignedShort((ushort)index);

                var move = new Move();
                move.direction = 1;
                move.speed = 10;
                move.point.x = 10.6f;
                move.point.y = 300.1f;
                move.Encoder(data);
                channel.WriteAndFlushAsync(data);

              //  logger.Debug($"send move!");
            };
            moveTimer.Start();

            //发送ping消息
            pingTimer = new System.Timers.Timer();
            pingTimer.AutoReset = true;
            pingTimer.Interval = 2000;
            pingTimer.Enabled = true;
            pingTimer.Elapsed += async (_1, _2) =>
            {
              //  logger.Debug($"send ping!");
                var pingTime = await channel.Ping(DateTime.Now.Ticks);
                var now = DateTime.Now.Ticks;
                var value = (now - pingTime) / 10000;
                logger.Debug($"ping:{value}ms");
            };
            pingTimer.Start();

        }
    }
}
