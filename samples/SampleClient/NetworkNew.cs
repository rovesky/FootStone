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
       // private string name;
        private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private SessionPushI sessionPushI;

        public PlayerPushI()
        {
           // this.name = name;
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
        private Timer updateTimer;

        public async Task Test(string ip, int port, int count, ushort startIndex, bool needNetty)
        {
            logger.Info($"begin test,count:${count},startIndex:{startIndex},needNetty:{needNetty}");

            var client = new FSClientBuilder()
                .IceOptions(iceOptions =>
                {
                    iceOptions.PushObjects = new List<Ice.Object>();
                    iceOptions.PushObjects.Add(new PlayerPushI());
                    iceOptions.PushObjects.Add(new ZonePushI());
                    //    initData.properties.setProperty("SessionFactory.Proxy", "SessionFactory:default -h "+ IP + " -p " + port +" -t 10000");
                    //    initData.properties.setProperty("Ice.Default.Locator", "FootStone/Locator:default -h " + ip + " -p " + port);

                })
                .NettyOptions(bootstrap =>
                {

                })
                .Build();

            updateTimer = new System.Timers.Timer();
            updateTimer.AutoReset = true;
            updateTimer.Interval = 33;
            updateTimer.Enabled = true;
            updateTimer.Elapsed += (_1, _2) =>
            {
                client.Update();
            };
            updateTimer.Start();
            await client.StartAsync();

            for (ushort i = startIndex; i < startIndex + count; ++i)
            {
                var sessionId = "session" + i;
                var session = await client.CreateSession(ip, port, sessionId);

                session.OnDestroyed += (sender, e) =>
                {
                    logger.Info($"session:{session.GetId()}");
                };
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

            try
            {
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

                //var connection = accountPrx.ice_getConnection();
                //logger.Debug("accountPrx:" + connection.getInfo().connectionId + " session connection: ACM=" +
                //       connection.getACM().ToString() + ",Endpoint=" + connection.getEndpoint().ToString());

                //选择服务器
                var worldPrx = WorldPrxHelper.uncheckedCast(sessionPrx, "world");
                //worldPrx.ice_fixed(sessionPrx.ice_getConnection());

                //logger.Debug("WorldPrx:" + worldPrx.ice_getConnectionId());

                //connection = worldPrx.ice_getConnection();
                //logger.Debug("WorldPrx:" + connection.getInfo().connectionId + " session connection: ACM=" +
                //       connection.getACM().ToString() + ",Endpoint=" + connection.getEndpoint().ToString());

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
                    await RunNetty(session);
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

        private async Task RunNetty(IFSSession session)
        {

            IChannel channel = null;
            System.Timers.Timer moveTimer = null;
            System.Timers.Timer pingTimer = null;
            //if (netty != null)
            //{
            //    //连接Netty
            //    var host = parseHost(sessionPrx.ice_getConnection().getEndpoint().ToString());
            //    logger.Debug("ConnectNetty begin(" + host + ")");
            //    channel = await netty.ConnectNettyAsync(host, 8007, playerInfo.playerId);
            //    logger.Debug("ConnectNetty end(" + host + ")");

            //    //绑定Zone
            //    var endPoint = await zonePrx.BindZoneAsync(playerInfo.zoneId, playerInfo.playerId);
            //    var gameServerId = ProtocolNettyUtility.Endpoint2GameServerId(endPoint.ip, endPoint.port);
            //    await netty.BindGameServer(channel, playerInfo.playerId, gameServerId);

            //    //进入Zone
            //    await zonePrx.PlayerEnterAsync();

            //    //发送move消息
            //    moveTimer = new System.Timers.Timer();
            //    moveTimer.AutoReset = true;
            //    moveTimer.Interval = 500;
            //    moveTimer.Enabled = true;
            //    moveTimer.Elapsed += (_1, _2) =>
            //    {
            //        netty.SendMove(channel, index);
            //    };
            //    moveTimer.Start();

            //    //发送ping消息
            //    pingTimer = new System.Timers.Timer();
            //    pingTimer.AutoReset = true;
            //    pingTimer.Interval = 2000;
            //    pingTimer.Enabled = true;
            //    pingTimer.Elapsed += (_1, _2) =>
            //    {
            //        netty.SendPing(channel, index);
            //    };
            //    pingTimer.Start();
            //}
        }
    }
}
