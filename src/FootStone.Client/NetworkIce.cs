﻿using FootStone.GrainInterfaces;
using Ice;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace FootStone.Client
{   
    public class  SessionIce
    {
     
        public SessionIce(ISessionPrx sessionPrx, SessionPushI sessionPush)
        {
            this.SessionPrx = sessionPrx;
            this.SessionPush = sessionPush;
        }

        public ISessionPrx SessionPrx { get; set; }
        public SessionPushI SessionPush { get; set; }
    }

    class NetworkIce
    {
        private static NLog.Logger logger = LogManager.GetCurrentClassLogger();       

        public  NetworkIce(IceClientOptions iceClientOptions) {

            this.iceClientOptions = iceClientOptions;
        }

        private List<Action>     actions = new List<Action>();
        private IceClientOptions iceClientOptions;

        protected ObjectAdapter Adapter { get; private set; }
        protected Communicator  Communicator { get; private set; }    
   

        public async Task Start()
        {
            try
            {
                var initData = new InitializationData();

                if (iceClientOptions.Properties != null)
                {
                    initData.properties = iceClientOptions.Properties;
                }
                else
                {
                    initData.properties = Util.createProperties();
                }
                //initData.properties.setProperty("Ice.ACM.Client.Heartbeat", "Always");
                //initData.properties.setProperty("Ice.RetryIntervals", "-1");
                initData.properties.setProperty("Ice.FactoryAssemblies", "client");
                initData.properties.setProperty("Ice.Trace.Network", "1");
                initData.properties.setProperty("Ice.Default.Timeout", "15000");

                initData.logger = new NLoggerIce(LogManager.GetLogger("Ice"));

                initData.dispatcher = delegate (Action action, Connection connection)
                {
                    lock (this)
                    {
                        actions.Add(action);
                    }
                };

                Communicator = Util.initialize(initData);
             
                Adapter = Communicator.createObjectAdapter("");

                Thread thread = new Thread(new ThreadStart(() =>
                {
                    Communicator.waitForShutdown();
                    logger.Info("ice closed!");
                }));
                thread.Start();
            }
            catch (System.Exception ex)
            {
                logger.Error(ex);
            }
        }

        /// <summary>
        /// </summary>
        public async Task Stop()
        {
            Communicator.shutdown();
            logger.Info("ice shutdown!");
        }


        /// <summary>
        /// 创建session
        /// </summary>
        /// <param name="account"></param>
        /// <param name="pushProtos"></param>
        /// <returns></returns>        
        public async Task<SessionIce> CreateSession(string ip, int port, string account)
        {
            //设置locator
            var locator = LocatorPrxHelper.uncheckedCast(Communicator
                .stringToProxy("FootStone/Locator:default -h " + ip + " -p " + port));
            Communicator.setDefaultLocator(locator);

            //获取session factory
            var sessionFactoryPrx = ISessionFactoryPrxHelper
               .uncheckedCast(Communicator.stringToProxy("sessionFactory")
               .ice_locatorCacheTimeout(0)
               .ice_connectionId(account)
               .ice_timeout(15000));


            //建立网络连接,设置心跳为30秒
            Connection connection = await sessionFactoryPrx.ice_getConnectionAsync();
            connection.setACM(30, ACMClose.CloseOff, ACMHeartbeat.HeartbeatAlways);
            connection.setCloseCallback(_ =>
            {
                logger.Warn($"{account} connecton closed!");
            });

            logger.Debug(connection.getInfo().connectionId + " session connection:Endpoint=" + connection.getEndpoint().ToString());

            //创建session,并且将connection设置为sessionFactory的connection
            var sessionPrx = ISessionPrxHelper.uncheckedCast((await sessionFactoryPrx.CreateSessionAsync(account, ""))
               .ice_fixed(connection));

            //注册push回调对象          
            var sessionPushI = new SessionPushI(account);
            var proxy = ISessionPushPrxHelper.uncheckedCast(Adapter.addWithUUID(sessionPushI));
            foreach (var proto in iceClientOptions.PushObjects)
            {
                var pushObj = (Ice.Object)proto.Clone();
                IServerPush serverPush = (IServerPush)pushObj;
                serverPush.setSessionPushI(sessionPushI);
                Adapter.addFacet(pushObj, proxy.ice_getIdentity(), serverPush.GetFacet());
            }

            // Associate the object adapter with the bidirectional connection.
            connection.setAdapter(Adapter);

            // Provide the proxy of the callback receiver object to the server and wait for
            // shutdown.               
            await sessionPrx.AddPushAsync(proxy);

            return new SessionIce(sessionPrx, sessionPushI);
           // return sessionPrx;
        }


        /// <summary>
        /// 由主线程调用
        /// </summary>
        public void Update()
        {
            Action[] array;
            lock (this)
            {
                array = actions.ToArray();
                actions.Clear();
            }
            foreach (Action each in array)
            {
                each();
            }
        }
    }



    public class SessionPushI : ISessionPushDisp_
    {
        public string Account { get; private set; }


        internal SessionPushI(string account)
        {
            this.Account = account;
      
        }

        public event EventHandler OnDestroyed;

        public override void SessionDestroyed(Current current = null)
        {
            OnDestroyed(this, null);
        }
    }
}
