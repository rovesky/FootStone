using FootStone.GrainInterfaces;
using Ice;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace Network
{
    public class NetworkIce
    {
        private static NetworkIce _instance;

        /// <summary>
        /// 私有化构造函数，使得类不可通过new来创建实例
        /// </summary>
        private NetworkIce() { }

        private List<Action>     actions = new List<Action>();

        public ObjectAdapter Adapter { get; private set; }

        private Ice.Communicator communicator;

      //  public SessionFactoryPrx SessionFactoryPrx { get; private set; }
      //  public SessionPrx SessionPrx { get; private set; }
     //   public AccountPrx accountPrx { get; private set; }

      //  public PlayerPrx PlayerPrx {  get ;  set; }


        public static NetworkIce Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new NetworkIce();
                }
                return _instance;
            }
        }

        public Communicator Communicator
        {
            get
            {
                return communicator;
            }

            set
            {
                communicator = value;
            }
        }

        public string PlayerId { get; internal set; }

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

        public  void Init(string IP,int port)
        {
            try
            {
                Ice.InitializationData initData = new Ice.InitializationData();

                initData.properties = Ice.Util.createProperties();
                //initData.properties.setProperty("Ice.ACM.Client.Heartbeat", "Always");
                //initData.properties.setProperty("Ice.RetryIntervals", "-1");
                initData.properties.setProperty("Ice.FactoryAssemblies", "client");
                initData.properties.setProperty("Ice.Trace.Network", "1");
                initData.properties.setProperty("Ice.Default.Timeout", "15");
                //    initData.properties.setProperty("SessionFactory.Proxy", "SessionFactory:default -h "+ IP + " -p " + port +" -t 10000");
                initData.properties.setProperty("Ice.Default.Locator", "FootStone/Locator:default -h " + IP + " -p " + port);

              
                //initData.dispatcher = delegate (System.Action action, Ice.Connection connection)
                //{
                //    lock (this)
                //    {
                //        actions.Add(action);
                //    }                
                //};

                communicator = Ice.Util.initialize(initData);
                Adapter = communicator.createObjectAdapter("");


                //    Communicator.w
                Thread thread = new Thread(new ThreadStart(() =>
                {
                    Communicator.waitForShutdown();
                }));
                thread.Start();
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
             
        public async Task<SessionPrx> CreateSession(string name)
        {
            var sessionFactoryPrx = (SessionFactoryPrx)SessionFactoryPrxHelper
                .uncheckedCast(communicator.stringToProxy("sessionFactory"))
                .ice_connectionId(name)
                ;
            await sessionFactoryPrx.ice_getConnectionAsync();
            var sessionPrx = (SessionPrx)(await sessionFactoryPrx
                .CreateSessionAsync(name, "")).ice_connectionId(name);
          
            Connection connection = await sessionPrx.ice_getConnectionAsync();
            connection.setACM(30, Ice.ACMClose.CloseOff, Ice.ACMHeartbeat.HeartbeatAlways);
            Console.WriteLine(connection.getInfo().connectionId+" session connection: ACM=" +
                JsonConvert.SerializeObject(connection.getACM())
                + ",Endpoint=" + JsonConvert.SerializeObject(connection.getEndpoint()));

          
            // Register the callback receiver servant with the object adapter     
            
            var proxy = SessionPushPrxHelper.uncheckedCast(Adapter.addWithUUID(new SessionPushI()));
            Adapter.addFacet(new PlayerPushI(name), proxy.ice_getIdentity(), "playerPush");
            // Associate the object adapter with the bidirectional connection.
            connection.setAdapter(Adapter);

            // Provide the proxy of the callback receiver object to the server and wait for
            // shutdown.               
            await sessionPrx.AddPushAsync(proxy);
            return sessionPrx;
        }

    }

    internal class PlayerPushI : PlayerPushDisp_
    {
        private string name;

        public PlayerPushI(string name)
        {
            this.name = name;
        }

        public override void hpChanged(int hp, Current current = null)
        {
      //      Console.Out.WriteLine(name+" hp changed:" + hp);
        }
    }

    internal class SessionPushI : SessionPushDisp_
    {
        public override void SessionDestroyed(Current current = null)
        {
            throw new NotImplementedException();
        }
    }
}
