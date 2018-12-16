//using CommonLang.Log;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace Pomelo
{

    public class FastStreamConfig
    {
        public string host;
        public int port = 3360;
    }
    public interface IFastSession
    {
        string ConnectorId { get; }
    }

    /// <summary>
    /// morefunFastStream管理器
    /// </summary>
    public class FastStream //: FastStream
    {
        private static readonly FastStream instance = new FastStream();

        private FastStream()
        {
        }

        public static FastStream Instance
        {
            get
            {
                return instance;
            }
        }



        /*-------------------------------------Composer--------------------------------------------------------*/
        /// <summary>
        /// 数据合成bytes
        /// </summary>
        class Composer
        {

            private const int LEFT_SHIFT_BITS = 1 << 7;

            private MemoryStream stream;
            public Composer(string uid,string instanceId, ArraySegment<byte> data)
            {
                byte[] bytesUid = System.Text.UTF8Encoding.UTF8.GetBytes(uid);
                byte[] bytesInstanceId = System.Text.UTF8Encoding.UTF8.GetBytes(instanceId);

                int contentSize = 8 + bytesUid.Length + bytesInstanceId.Length +  data.Count;
                int lengthSize = calLengthSize(contentSize);

                this.stream = new MemoryStream(lengthSize + contentSize);


                //composer head
                writeLength(contentSize, lengthSize);
                //协议 head
                writeU16((ushort)bytesUid.Length);
                writeU16((ushort)bytesInstanceId.Length);
                writeU32((uint)data.Count);

                //uid
                writeBytes(bytesUid, 0, bytesUid.Length);
                //instanceId
                writeBytes(bytesInstanceId, 0, bytesInstanceId.Length);
                //data
                writeBytes(data.Array, data.Offset, data.Count);

            }

            public byte[] getBytes()
            {
                return stream.GetBuffer();
            }

            public static int calLengthSize(int length)
            {
                int res = 0;
                while (length > 0)
                {
                    length = length >> 7;
                    res++;
                }

                return res;
            }


            public void writeLength(int data, int size)
            {
                int offset = size - 1, b;
                byte[] bytes = new byte[size];
                for (; offset >= 0; offset--)
                {
                    b = data % LEFT_SHIFT_BITS;
                    if (offset < size - 1)
                    {
                        b |= 0x80;
                    }
                    bytes[offset] = (byte)b;
                    data = data >> 7;
                }
                stream.Write(bytes, 0, bytes.Length);
            }

            public void writeU16(UInt16 value)
            {
                stream.WriteByte((byte)(value));
                stream.WriteByte((byte)(value >> 8));
            }

            public void writeU32(UInt32 value)
            {
                stream.WriteByte((byte)(value));
                stream.WriteByte((byte)(value >> 8));
                stream.WriteByte((byte)(value >> 16));
                stream.WriteByte((byte)(value >> 24));
            }



            public void writeBytes(byte[] value, int offset, int length)
            {
                stream.Write(value, offset, length);
            }
        }




        /*-------------------------------------Transporter--------------------------------------------------------*/

        /// <summary>
        /// 数据传输协议
        /// </summary>
        class Transporter
        {
            /// <summary>
            /// 传输状态
            /// </summary>
            enum TransportState
            {
                readHead = 1,		// on read head
                readBody = 2,		// on read body
                closed = 3			// connection closed, will ignore all the message and wait for clean up
            }


            /*-------------------------------------TransportState--------------------------------------------------------*/

            //读取buffer
            class StateObject
            {
                public const int BufferSize = 1024;
                internal byte[] buffer = new byte[BufferSize];
            }

            public const int HeadLength = 6;

            private TcpClient socket;
            private Action<FastStreamRequest> messageProcesser;

            //Used for get message
            private StateObject stateObject = new StateObject();
            private TransportState transportState;
            private IAsyncResult asyncReceive;
            private IAsyncResult asyncSend;
            //  private bool onSending = false;
            //  private bool onReceiving = false;
            private byte[] headBuffer = new byte[HeadLength];
            private byte[] buffer;
            private int bufferOffset = 0;
            private int pkgLength = 0;
            internal Action onDisconnect = null;



            /// <summary>
            /// 日志
            /// </summary>
        //    private Logger log = LoggerFactory.GetLogger("Transporter");


            public bool Connected
            {
                get { return this.socket.Connected; }
            }

            public Transporter(TcpClient socket, Action<FastStreamRequest> processer)
            {
                this.socket = socket;
                this.socket.NoDelay = true;
                this.messageProcesser = processer;
                transportState = TransportState.readHead;
            }

            public void start()
            {
                this.receive();
            }

            public void send(byte[] buffer)
            {
                if (this.transportState != TransportState.closed)
                {

                    this.asyncSend = socket.GetStream().BeginWrite(buffer, 0, buffer.Length, new AsyncCallback(sendCallback), socket);

                    //  this.onSending = true;
                }
            }

            private void sendCallback(IAsyncResult asyncSend)
            {
                if (this.transportState == TransportState.closed)
                    return;

                socket.GetStream().EndWrite(asyncSend);

                // Console.WriteLine("socket send end:"+ CUtils.CurrentTimeMS);    
                //  this.onSending = false;
            }

            public void receive()
            {
                this.asyncReceive = socket.GetStream().BeginRead(stateObject.buffer, 0, stateObject.buffer.Length, new AsyncCallback(endReceive), stateObject);
                // this.onReceiving = true;
            }

            internal void close()
            {
                this.transportState = TransportState.closed;
                /*try{
                    if(this.onReceiving) socket.EndReceive (this.asyncReceive);
                    if(this.onSending) socket.EndSend(this.asyncSend);
                }catch (Exception e){
                    Console.WriteLine(e.Message);
                }*/
            }

            private void endReceive(IAsyncResult asyncReceive)
            {
                if (this.transportState == TransportState.closed)
                    return;
                StateObject state = (StateObject)asyncReceive.AsyncState;
                TcpClient socket = this.socket;

                try
                {
                    int length = socket.GetStream().EndRead(asyncReceive);

                    //  this.onReceiving = false;

                    if (length > 0)
                    {
                        processBytes(state.buffer, 0, length);
                        //Receive next message
                        if (this.transportState != TransportState.closed)
                            receive();
                    }
                    else
                    {
                        if (this.onDisconnect != null)
                            this.onDisconnect();
                    }

                }
                catch (Exception e)
                {
                  //  log.Error(e.Message, e);
                    if (this.onDisconnect != null)
                        this.onDisconnect();
                }
            }

            /// <summary>
            /// 处理数据
            /// </summary>
            /// <param name="bytes"></param>
            /// <param name="offset"></param>
            /// <param name="limit"></param>
            internal void processBytes(byte[] bytes, int offset, int limit)
            {
                if (this.transportState == TransportState.readHead)
                {
                    readHead(bytes, offset, limit);
                }
                else if (this.transportState == TransportState.readBody)
                {
                    readBody(bytes, offset, limit);
                }
            }

            /// <summary>
            /// 从包头获取包长度
            /// </summary>
            /// <param name="header"></param>
            /// <returns></returns>
            private int getBodyLengthFromHeader(byte[] header)
            {
                try
                {

                    BinaryReader headReader = new BinaryReader(new MemoryStream(header, 0, HeadLength));
                    int idLength = headReader.ReadInt16();
                    int dataLength = headReader.ReadInt32();

                    return idLength + dataLength;
                }
                catch (Exception e)
                {
                //    log.Error(e.Message, e);
                }

                return 0;

            }

            /// <summary>
            /// 读取包头
            /// </summary>
            /// <param name="bytes"></param>
            /// <param name="offset"></param>
            /// <param name="limit"></param>
            /// <returns></returns>
            private bool readHead(byte[] bytes, int offset, int limit)
            {
                int length = limit - offset;
                int headNum = HeadLength - bufferOffset;

                if (length >= headNum)
                {
                    Buffer.BlockCopy(bytes, offset, headBuffer, bufferOffset, headNum);
                    //Get package length
                    pkgLength = getBodyLengthFromHeader(headBuffer);

                    //Init message buffer
                    buffer = new byte[HeadLength + pkgLength];

                    Buffer.BlockCopy(headBuffer, 0, buffer, 0, HeadLength);
                    offset += headNum;
                    bufferOffset = HeadLength;
                    this.transportState = TransportState.readBody;

                    if (offset <= limit) processBytes(bytes, offset, limit);
                    return true;
                }
                else
                {
                    Buffer.BlockCopy(bytes, offset, headBuffer, bufferOffset, length);
                    bufferOffset += length;
                    return false;
                }
            }

            /// <summary>
            /// 从byte转换为request
            /// </summary>
            /// <param name="header"></param>
            /// <param name="buffer"></param>
            /// <returns></returns>
            private FastStreamRequest resolveRequestInfo(byte[] header, byte[] buffer)
            {
                try
                {
                    BinaryReader headReader = new BinaryReader(new MemoryStream(header, 0, HeadLength));
                    int idLength = headReader.ReadInt16();
                    int dataLength = headReader.ReadInt32();

                    BinaryReader bodyReader = new BinaryReader(new MemoryStream(buffer, HeadLength, buffer.Length - HeadLength));

                    string id = Encoding.UTF8.GetString(bodyReader.ReadBytes(idLength));
                    return new FastStreamRequest(id, bodyReader.ReadBytes(dataLength));

                }
                catch (Exception e)
                {
                 //   log.Error(e.Message, e);
                }

                return null;
            }

            /// <summary>
            /// 读取包体
            /// </summary>
            /// <param name="bytes"></param>
            /// <param name="offset"></param>
            /// <param name="limit"></param>
            private void readBody(byte[] bytes, int offset, int limit)
            {
                int length = pkgLength + HeadLength - bufferOffset;
                if ((offset + length) <= limit)
                {

                    Buffer.BlockCopy(bytes, offset, buffer, bufferOffset, length);
                    offset += length;

                    //回调消息处理函数
                    this.messageProcesser.Invoke(resolveRequestInfo(headBuffer, buffer));

                    this.bufferOffset = 0;
                    this.pkgLength = 0;

                    if (this.transportState != TransportState.closed)
                        this.transportState = TransportState.readHead;
                    if (offset < limit)
                        processBytes(bytes, offset, limit);
                }
                else
                {
                    Buffer.BlockCopy(bytes, offset, buffer, bufferOffset, limit - offset);

                    bufferOffset += limit - offset;
                    this.transportState = TransportState.readBody;
                }
            }


            private void print(byte[] bytes, int offset, int length)
            {
                for (int i = offset; i < length; i++)
                    Console.Write(Convert.ToString(bytes[i], 16) + " ");
                Console.WriteLine();
            }
        }

        /*-------------------------------------FastStreamRequest--------------------------------------------------------*/

        /// <summary>
        /// fastStream 请求结构
        /// </summary>
        class FastStreamRequest
        {

            private string playerId;
            private byte[] data;


            public string PlayerId
            {
                get { return playerId; }

            }

            public byte[] Data
            {
                get { return data; }

            }


            public FastStreamRequest(string playerId, byte[] data)
            {
                this.playerId = playerId;
                this.data = data;
            }
        }

        /*-------------------------------------FastStreamSession--------------------------------------------------------*/

        /// <summary>
        /// session
        /// </summary>
        class FastStreamSession : IFastSession
        {

            /// <summary>
            /// connector 服务器id
            /// </summary>
            private string connectorId;

            /// <summary>
            /// 发送队列
            /// </summary>
            private Queue<object> sendQueue = new Queue<object>();

            /// <summary>
            /// 数据传输协议
            /// </summary>
            private Transporter transporter;


            /// <summary>
            /// 发送线程
            /// </summary>
            private Thread sendThread;


            /// <summary>
            /// 新请求接收事件
            /// </summary>
            public delegate void NewRequestReceivedHandler(FastStreamSession session, FastStreamRequest request);
            public event NewRequestReceivedHandler NewRequestReceived;

            /// <summary>
            /// session关闭事件
            /// </summary>
            public delegate void SessionClosedHandler();
            public event SessionClosedHandler SessionClosed;

            /// <summary>
            /// 日志
            /// </summary>
            //private Logger log = LoggerFactory.GetLogger("FastStreamSession");

            public FastStreamSession(TcpClient client)
            {
                this.transporter = new Transporter(client, (request) =>
                {
                    NewRequestReceived(this, request);
                });

                this.transporter.onDisconnect = () =>
                {
                    OnSessionClosed("");

                };
            }


            public void start()
            {

                OnSessionStarted();

                this.transporter.start();
            }



            protected void OnSessionStarted()
            {
                //      Logger.Debug("OnSessionStarted");
                this.sendThread = new Thread(new ThreadStart(this.runSend));
                this.sendThread.IsBackground = true;
                this.sendThread.Start();
            }


            protected void OnSessionClosed(string reason)
            {
                //清理
                lock (this.sendQueue)
                {
                    this.sendQueue.Clear();
                }
                if (this.sendThread != null)
                {
                    try
                    {
                        this.sendThread.Join(1000);
                    }
                    catch (Exception err)
                    {
                 //       log.Error(err.Message, err);
                    }
                    this.sendThread = null;
                }
                SessionClosed();
            }

            /// <summary>
            /// 发送数据,目前是通过消息队列来实现
            /// </summary>
            /// <param name="data"></param>
            /// <param name="offset"></param>
            /// <param name="length"></param>
            public void Send(byte[] data, int offset, int length)
            {
                lock (this.sendQueue)
                {
                    this.sendQueue.Enqueue(data);


                    // 通知写线程开始工作。
                    Monitor.PulseAll(this.sendQueue);
                }

            }

            /// <summary>
            /// 发送线程
            /// </summary>
            private void runSend()
            {
                while (transporter.Connected)
                {

                    byte[] data = null;
                    lock (this.sendQueue)
                    {
                        //if (this.sendQueue.Count >= 100)
                        //{
                        //    log.Error("begin send -- this.sendQueue.Count :" + this.sendQueue.Count);
                        //}

                        if (this.sendQueue.Count > 0)
                        {
                            data = (byte[])this.sendQueue.Dequeue();
                        }
                        else
                        {
                            Monitor.Wait(this.sendQueue, 100);
                        }
                    }

                    if (data != null)
                    {

                        try
                        {
                            transporter.send(data);
                        }
                        catch (Exception e)
                        {
                  //          log.Error(e.Message, e);
                        }
                    }
                }
              //  log.Error("send thread exit!");
            }


            public string ConnectorId
            {
                get { return connectorId; }
                set { connectorId = value; }
            }

        }

        /*-------------------------------------FastStreamServer--------------------------------------------------------*/
        /// <summary>
        /// server
        /// </summary>
        class FastStreamServer
        {
            public delegate void SessionClosedHandler(FastStreamSession session, string reason);

            public event SessionClosedHandler SessionClosed;


            public delegate void NewRequestReceivedHandler(FastStreamSession session, FastStreamRequest request);

            public event NewRequestReceivedHandler NewRequestReceived;


            private TcpListener listener;

            private List<FastStreamSession> clients = new List<FastStreamSession>();
            // private bool disposed = false;

            private bool isRunning;

            public void Setup(int port)
            {
                listener = new TcpListener(IPAddress.Any, port);
                //listener.AllowNatTraversal(true);
            }
            public void Start()
            {
                if (!isRunning)
                {
                    isRunning = true;
                    listener.Start();
                    listener.BeginAcceptTcpClient(
                      new AsyncCallback(HandleTcpClientAccepted), listener);
                }
            }

            private void HandleTcpClientAccepted(IAsyncResult ar)
            {
                if (isRunning)
                {
                    TcpListener tcpListener = (TcpListener)ar.AsyncState;
                    TcpClient tcpClient = tcpListener.EndAcceptTcpClient(ar);
                    FastStreamSession internalClient = new FastStreamSession(tcpClient);

                    internalClient.NewRequestReceived += (client, request) =>
                    {
                        this.NewRequestReceived(client, request);
                    };

                    internalClient.SessionClosed += () =>
                    {
                        lock (this.clients)
                        {
                            this.clients.Remove(internalClient);

                        }
                        this.SessionClosed(internalClient, "");
                    };

                    internalClient.start();

                    lock (this.clients)
                    {
                        this.clients.Add(internalClient);
                        //       RaiseClientConnected(tcpClient);
                    }


                    tcpListener.BeginAcceptTcpClient(
                      new AsyncCallback(HandleTcpClientAccepted), ar.AsyncState);
                }
            }



            public void Stop()
            {

            }

        }

        /// <summary>
        /// 网络服务
        /// </summary>
        private FastStreamServer server = new FastStreamServer();


        /// <summary>
        /// 网络连接
        /// </summary>
        /// 
        private ConcurrentDictionary<string, FastStreamSession> sessions = new ConcurrentDictionary<string, FastStreamSession>();

        private FastStreamConfig config;
        /// <summary>
        /// 日志
        /// </summary>
        //  private Logger log = LoggerFactory.GetLogger("MorefunFastStream");

        //   private IZone zone = null;

        public  void Init(FastStreamConfig config)
        {
            this.config = config;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="config">配置</param>   
        public  void Start()
        {
      //      log.Info("start on port:" + config.port);
    //        this.zone = zone;

            //监听事件
            this.server.Setup(config.port);
            this.server.NewRequestReceived += appServer_NewRequestReceived;
            this.server.SessionClosed += server_SessionClosed;

            //启动网络服务
            this.server.Start();
        }



        /// <summary>
        /// 网络断开
        /// </summary>
        /// <param name="session"></param>
        /// <param name="value"></param>
        private void server_SessionClosed(FastStreamSession session, string reason)
        {
          //  log.Error("session closed:" + session.ConnectorId + ",reason:" + reason);
            FastStreamSession outValue;
            sessions.TryRemove(session.ConnectorId, out outValue);
        }

        /// <summary>
        /// 收到数据包
        /// </summary>
        /// <param name="session"></param>
        /// <param name="requestInfo"></param>
        private void appServer_NewRequestReceived(FastStreamSession session, FastStreamRequest requestInfo)
        {
            try
            {
                //注意要捕获异常
                if (requestInfo.PlayerId.Equals("connetorId"))
                {
                    //connetorId为握手协议,表示了session的身份

                    string connetorId = Encoding.UTF8.GetString(requestInfo.Data);

                 //   log.Info(connetorId + " connected!");
                    session.ConnectorId = connetorId;
                    sessions.AddOrUpdate(connetorId, session, (key, oldValue) => session);

                    //test 回复
                    //byte[] sendData = new byte[90] 
                    //{ 
                    //110, 200, 21, 110, 200, 21, 110, 200, 21,
                    //110, 200, 21, 110, 200, 21, 110, 200, 21,
                    //110, 200, 21, 110, 200, 21, 110, 200, 21,
                    //110, 200, 21, 110, 200, 21, 110, 200, 21,
                    //110, 200, 21, 110, 200, 21, 110, 200, 21,
                    //110, 200, 21, 110, 200, 21, 110, 200, 21,
                    //110, 200, 21, 110, 200, 21, 110, 200, 21,
                    //110, 200, 21, 110, 200, 21, 110, 200, 21,
                    //110, 200, 21, 110, 200, 21, 110, 200, 21,
                    //110, 200, 21, 110, 200, 21, 110, 200, 21};

                    //for (int i = 0; i < 1000; ++i)
                    //{
                    //    Send(connetorId, "good"+i, sendData);
                    //}
                }

                else
                {
                    //发送数据到场景
                    //Task.Run(() =>
                    // {
                    try
                    {

                //        zone.PlayerReceive(requestInfo.PlayerId, requestInfo.Data);
                    }
                    catch (Exception e)
                    {
               //         log.Warn(e.Message + e.StackTrace, e);
                    }
                    //});

                }
            }
            catch (Exception e)
            {
         //       log.Error(e.Message, e);
            }

        }

        /// <summary>
        /// 停止服务
        /// </summary>
        public  void Stop()
        {
            this.server.Stop();

        }


        //发送数据到connector服务器
        public  void Send(IFastSession connetorId, string uid, string instanceId,ArraySegment<byte> data)
        {
            FastStreamSession socket = connetorId as FastStreamSession;
            Composer composer = new Composer(uid, instanceId,data);
            byte[] sendData = composer.getBytes();
      //      log.Debug("player:" + uid + " socket.Send");
            socket.Send(sendData, 0, sendData.Length);

        }

        public  IFastSession GetSessionByID(string sessionID)
        {
            if (!sessions.ContainsKey(sessionID))
            {
                throw new Exception("connetor server " + sessionID + " not find!");
            }
            FastStreamSession socket;
            if (!sessions.TryGetValue(sessionID, out socket))
            {
                throw new Exception("connetor server " + sessionID + "TryGetValue fail!");
            }
            return socket;
        }
    }
}

