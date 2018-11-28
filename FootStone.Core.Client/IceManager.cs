//using com.XSanGo.Protocol;
//using Glacier2;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;

//namespace XSanGo.Client.Net
//{
//    //class GameSessionCallbackI : GameSessionCallbackDisp_
//    //{
//    //    IceManager mgr;
//    //    public GameSessionCallbackI(IceManager mgr)
//    //    {
//    //        this.mgr = mgr;
//    //    }

//    //    //这个回调不可用
//    //    public override void timeSyncronized(string time, Ice.Current current__)
//    //    {
//    //        //同步服务器时间
//    //        //GameDataManager.Instance.serverTime = DateTime.Parse(time);
//    //    }

//    //    public override void close(string note, Ice.Current current__)
//    //    {
//    //        mgr.disconnect();
//    //    }
//    //}

//    public class IceLogger : Ice.Logger
//    {
//        public IceLogger()
//        {
//        }

//        public Ice.Logger cloneWithPrefix(string prefix)
//        {
//            throw new NotImplementedException();
//        }

//        public void error(string message)
//        {
//            Console.WriteLine(message);
//        }

//        public string getPrefix()
//        {
//            throw new NotImplementedException();
//        }

//        public void print(string message)
//        {
//            Console.WriteLine(message);
//        }

//        public void trace(string category, string message)
//        {
//            Console.WriteLine(category + message);
//        }

//        public void warning(string message)
//        {
//            Console.WriteLine(message);
//        }
//    }

//    public class IceManager : SessionCallback
//    {
//        public string username;
//        public string password;

//        Ice.InitializationData initData;
//        public SessionFactoryHelper _factory;
//        public Glacier2.SessionHelper _helper;
//      //  public GameSessionPrx _session;

//        public Ice.AsyncResult lastRequest;
//        public Ice.Exception lastException;

//        //public RolePrx role;
//        //public CopyPrx pve;
//        //public RankListPrx rank;
//        //public ChatPrx chat;
//        //public ArenaRankPrx pvp;
//        //public TimeBattlePrx timeBattle;

//        public IceLogger logger;

//      //  public RoleView[] roleView;

//        public enum State
//        {
//            Disconnected,
//            Connecting,
//            Connected,
//            Loading,
//            Loaded,
//        };

//        public delegate void DelegateStateChange(State oldState);
//        public DelegateStateChange onStateChange;
//        private State _state;

//        //当前连接状态
//        public State state
//        {
//            get
//            {
//                return _state;
//            }
//            internal set
//            {
//                State os = _state;
//                _state = value;
//                if (onStateChange != null) onStateChange(os);
//            }
//        }

//        public IceManager()
//        {
//            state = State.Disconnected;
//        }

//        public void Initialize(Ice.InitializationData initData)
//        {
//            this.initData = initData;
//            initData.logger = this.logger;
//        }

//        public void connect()
//        {
//            state = State.Connecting;

//            if (_helper != null)
//            {
//                _helper.destroy();
//            }
//            _helper = new SessionHelper(this, initData);
//            _helper.connect(username, password, null);
//        }

//        public void InitSession()
//        {
//            state = State.Loading;

//            GameSessionCallbackPrx callbackPrx = GameSessionCallbackPrxHelper.checkedCast(
//                                                    _helper.objectAdapter().addWithUUID(new GameSessionCallbackI(this)));

//            _session.begin_setCallback(callbackPrx).whenCompleted(() =>
//                {
//                    _session.begin_getRole().whenCompleted((RolePrx prx) =>
//                    {
//                        role = prx;

//                        pve = CopyPrxHelper.uncheckedCast(role, "copy");
//                        pvp = ArenaRankPrxHelper.uncheckedCast(role, "arenaRank");
//                        chat = ChatPrxHelper.uncheckedCast(role, "chat");
//                        rank = RankListPrxHelper.uncheckedCast(role, "rankList");
//                        timeBattle = TimeBattlePrxHelper.uncheckedCast(role, "timeBattle");

//                        GetRoleData();
//                    },
//                    (Ice.Exception ex) =>
//                    {
//                        onSessionException(ex);
//                    });
//                },
//                (Ice.Exception ex) =>
//                {
//                    onSessionException(ex);
//                });
//        }

//        public void GetRoleData()
//        {
//            role.begin_getRoleViewList().whenCompleted((RoleView[] ret__) =>
//            {
//                this.roleView = ret__;
//                state = State.Loaded;
//            },
//            (Ice.Exception ex) =>
//            {
//                onSessionException(ex);
//            });
//        }

//        public void disconnect()
//        {
//            state = State.Disconnected;
//            if (_helper != null)
//            {
//                _helper.destroy();
//            }
//        }

//        public void onSessionException(Ice.Exception e, bool force_disconnect = false)
//        {
//            System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(); // get call stack
//            System.Diagnostics.StackFrame[] stackFrames = stackTrace.GetFrames();  // get method calls (frames)

//            // write call stack method names
//            foreach (System.Diagnostics.StackFrame stackFrame in stackFrames)
//            {
//                Console.WriteLine(stackFrame.GetMethod().Name);   // write method name
//            }

//            if (e is Ice.ConnectionLostException)
//            {
//                force_disconnect = true;
//            }
//            else if (e is Ice.TimeoutException)
//            {
//                force_disconnect = true;
//            }
//            else if (e is Ice.CommunicatorDestroyedException)
//            {
//                force_disconnect = true;
//            }

//            Console.Write(e.ToString());

//            if (force_disconnect)
//            {
//                disconnect();
//            }
//        }

//        public void connectFailed(SessionHelper session, Exception ex)
//        {
//            if (session != _helper) return;

//            state = State.Disconnected;
//        }

//        public void connected(SessionHelper session)
//        {
//            if (session != _helper) return;

//            state = State.Connected;

//            GameSessionPrxHelper h = new GameSessionPrxHelper();
//            h.copyFrom__(session.session());
//            _session = h;

//            state = State.Connected;

//            InitSession();
//        }

//        public void createdCommunicator(SessionHelper session)
//        {
//            if (session != _helper) return;
//        }

//        public void disconnected(SessionHelper session)
//        {
//            if (session != _helper) return;
//            state = State.Disconnected;
//        }
//    }
//}
