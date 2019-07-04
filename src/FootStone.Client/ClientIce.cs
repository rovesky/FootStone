using FootStone.GrainInterfaces;
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
    public class ClientIce
    {
        private static NLog.Logger logger = LogManager.GetCurrentClassLogger();

        //private static ClientIce _instance;
        //public static ClientIce Instance
        //{
        //    get
        //    {
        //        if (_instance == null)
        //        {
        //            _instance = new ClientIce();
        //        }
        //        return _instance;
        //    }
        //}
      

        /// <summary>

        /// </summary>
        public  ClientIce(InitializationData iceInitData) {

            this.iceInitData = iceInitData;
        }

        private List<Action>     actions = new List<Action>();
        private InitializationData iceInitData;

        public ObjectAdapter Adapter { get; private set; }
        public Communicator Communicator { get; private set; }
     
   
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

        public async Task Start()
        {
            try
            {           
                iceInitData.logger = new NLoggerI(LogManager.GetLogger("Ice"));

                iceInitData.dispatcher = delegate (Action action, Connection connection)
                {
                    lock (this)
                    {
                        actions.Add(action);
                    }
                };

                Communicator = Util.initialize(iceInitData);
             
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
        public async Task<ISessionPrx> CreateSession(string account,string ip,int port)
        {
            
            //获取session factory
            ISessionFactoryPrx sessionFactoryPrx = null;
            try
            {               
                //  Communicator.setDefaultLocator()
                sessionFactoryPrx = ISessionFactoryPrxHelper
                   .uncheckedCast(Communicator.stringToProxy("sessionFactory")
                   .ice_connectionId(account)
                   .ice_timeout(15000));               
            }
            catch (Ice.NotRegisteredException e)
            {
                logger.Error(e);
                throw e;
            }                

            //建立网络连接
            await sessionFactoryPrx.ice_pingAsync();

            //获取session
            var sessionPrx = (ISessionPrx)(await sessionFactoryPrx
                .CreateSessionAsync(account, "")).ice_connectionId(account);

            //设置心跳为30秒
            Connection connection = await sessionPrx.ice_getConnectionAsync();
            connection.setACM(30, ACMClose.CloseOff, ACMHeartbeat.HeartbeatAlways);
            connection.setCloseCallback( _ =>
            {
                logger.Warn($"{account} connecton closed!");
            });

            logger.Debug(connection.getInfo().connectionId+" session connection: ACM=" +
                      connection.getACM().ToString() + ",Endpoint=" + connection.getEndpoint().ToString());
            

            //注册push回调对象           
            var proxy = ISessionPushPrxHelper.uncheckedCast(Adapter.addWithUUID(new SessionPushI()));
            foreach (var proto in pushProtos)
            {
                var pushObj = (Ice.Object)proto.Clone();
                IServerPush serverPush = (IServerPush)pushObj;
                Adapter.addFacet(pushObj, proxy.ice_getIdentity(), serverPush.GetFacet());
            }
     
            // Associate the object adapter with the bidirectional connection.
            connection.setAdapter(Adapter);

            // Provide the proxy of the callback receiver object to the server and wait for
            // shutdown.               
            await sessionPrx.AddPushAsync(proxy);
            return sessionPrx;
        }

    }

    //internal class ZonePushI : IZonePushDisp_
    //{
    //    private string name;

    //    private int count = 0;

    //    public ZonePushI(string name)
    //    {
    //        this.name = name;
    //    }
    //    private  NLog.Logger logger = LogManager.GetCurrentClassLogger();

    //    public override void ZoneSync(byte[] data, Current current = null)
    //    {
    //        count++;
    //        if (count % 330 == 0)
    //        {
    //            logger.Info(name + " zone sync:" + count);
    //        }
    //    }
    //}

    //internal class PlayerPushI : IPlayerPushDisp_
    //{
    //    private string name;
    //    private NLog.Logger logger = LogManager.GetCurrentClassLogger();
    //    public PlayerPushI(string name)
    //    {
    //        this.name = name;
    //    }
               
    //    public override void hpChanged(int hp, Current current = null)
    //    {

    //        Test.HpChangeCount++;
    //        if (Test.HpChangeCount % 1000 == 0)
    //        {
    //            logger.Info(name + " hp changed::" + Test.HpChangeCount);
    //        }

    //        //      logger.Info(name+" hp changed:" + hp);
    //    }
    //}

    internal class SessionPushI : ISessionPushDisp_
    {
        public override void SessionDestroyed(Current current = null)
        {
            throw new NotImplementedException();
        }
    }
}
