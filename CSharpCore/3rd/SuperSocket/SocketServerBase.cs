using SuperSocket.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security.Authentication;

namespace SuperSocket
{
    public delegate void NewSessionHandler(Socket client, ISocketSession session);
    public delegate void CloseSessionHandler(ISocketSession session);

    /// <summary>
    /// The interface for socket session which requires negotiation before communication
    /// </summary>
    interface INegotiateSocketSession
    {
        /// <summary>
        /// Start negotiates
        /// </summary>
        void Negotiate();

        /// <summary>
        /// Gets a value indicating whether this <see cref="INegotiateSocketSession" /> is result.
        /// </summary>
        /// <value>
        ///   <c>true</c> if result; otherwise, <c>false</c>.
        /// </value>
        bool Result { get; }


        /// <summary>
        /// Gets the app session.
        /// </summary>
        /// <value>
        /// The app session.
        /// </value>
        IAppSession AppSession { get; }

        /// <summary>
        /// Occurs when [negotiate completed].
        /// </summary>
        event EventHandler NegotiateCompleted;
    }

    public class SocketServerBaseDesc
    {
        public string Name
        {
            get;
            set;
        } = "server name";
        public string ServerType
        {
            get;
            set;
        } = "";
        public string Ip
        {
            get;
            set;
        } = "any";
        public UInt16 Port
        {
            get;
            set;
        } = 2020;
        public int MaxConnect
        {
            get;
            set;
        } = 1000;
    }
    public class SocketServerBase
    {
        protected object SyncRoot = new object();

        public IAppServer AppServer { get; private set; }

        public bool IsRunning { get; protected set; }

        //protected ListenerInfo[] ListenerInfos { get; private set; }

        //protected List<ISocketListener> Listeners { get; private set; }

        /// <summary>
        /// Gets the sending queue manager.
        /// </summary>
        /// <value>
        /// The sending queue manager.
        /// </value>
        internal ISmartPool<SendingQueue> SendingQueuePool { get; private set; }

        public event NewSessionHandler NewClientAccepted;
        public event CloseSessionHandler CloseClientConnect;

        protected bool IsStopped { get; set; }

        private BufferManager _bufferManager;

        private ConcurrentStack<SocketAsyncEventArgsProxy> _readWritePool;

        private Socket _socket
        {
            get;
            set;
        }

        public SocketServerBaseDesc Desc
        {
            get;
            set;
        }
        public SocketServerBase(SocketServerBaseDesc config)
        {
            Desc = config;
            if (config == null)
                throw new ArgumentNullException();
            if (_socket != null)
                throw new Exception("socket server exists.");

            try
            {
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _socket.Bind(new IPEndPoint(IPAddress.TryParse(config.Ip, out IPAddress address) ? address : IPAddress.Any, config.Port));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool Start()
        {
            int bufferSize = 0;//AppServer.Config.ReceiveBufferSize;

            int maxConnection = Desc.MaxConnect;

            if (bufferSize <= 0)
                bufferSize = 1024 * 4;

            _bufferManager = new BufferManager(bufferSize * maxConnection, bufferSize);

            try
            {
                _bufferManager.InitBuffer();
            }
            catch (Exception e)
            {
                throw e;
                //AppServer.Logger.Error("Failed to allocate buffer for async socket communication, may because there is no enough memory, please decrease maxConnectionNumber in configuration!", e);
                //return false;
            }

            var sendingQueuePool = new SmartPool<SendingQueue>();
            sendingQueuePool.Initialize(Math.Max(maxConnection / 6, 256),
                    Math.Max(maxConnection * 2, 256),
                    new SendingQueueSourceCreator(5));

            SendingQueuePool = sendingQueuePool;

            // preallocate pool of SocketAsyncEventArgs objects
            SocketAsyncEventArgs socketEventArg;

            var socketArgsProxyList = new List<SocketAsyncEventArgsProxy>(maxConnection);

            for (int i = 0; i < maxConnection; i++)
            {
                //Pre-allocate a set of reusable SocketAsyncEventArgs
                socketEventArg = new SocketAsyncEventArgs();
                _bufferManager.SetBuffer(socketEventArg);

                socketArgsProxyList.Add(new SocketAsyncEventArgsProxy(socketEventArg));
            }

            _readWritePool = new ConcurrentStack<SocketAsyncEventArgsProxy>(socketArgsProxyList);



            _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);

            SocketAsyncEventArgs acceptEventArg = new SocketAsyncEventArgs();
            //m_AcceptSAE = acceptEventArg;
            acceptEventArg.Completed += AcceptEventArg_Completed;
            _socket.Listen(100);

            if (!_socket.AcceptAsync(acceptEventArg))
                ProcessAccept(acceptEventArg);

            IsRunning = true;
            return true;
        }

        public void Stop()
        {
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
        }
        private void AcceptEventArg_Completed(object sender, SocketAsyncEventArgs e)
        {
            ProcessAccept(e);
        }

        void ProcessAccept(SocketAsyncEventArgs e)
        {
            Socket socket = null;

            if (e.SocketError != SocketError.Success)
            {
                var errorCode = (int)e.SocketError;

                //The listen socket was closed
                if (errorCode == 995 || errorCode == 10004 || errorCode == 10038)
                    return;

                //OnError(new SocketException(errorCode));
            }
            else
            {
                socket = e.AcceptSocket;
            }

            e.AcceptSocket = null;

            bool willRaiseEvent = false;

            try
            {
                willRaiseEvent = _socket.AcceptAsync(e);
            }
            catch (ObjectDisposedException)
            {
                //The listener was stopped
                //Do nothing
                //make sure ProcessAccept won't be executed in this thread
                willRaiseEvent = true;
            }
            catch (NullReferenceException)
            {
                //The listener was stopped
                //Do nothing
                //make sure ProcessAccept won't be executed in this thread
                willRaiseEvent = true;
            }
            catch (Exception exc)
            {
                EngineNS.Profiler.Log.WriteException(exc);
                //OnError(exc);
                //make sure ProcessAccept won't be executed in this thread
                willRaiseEvent = true;
            }

            if (socket != null)
                OnNewClientAccepted(socket, null);

            if (!willRaiseEvent)
                ProcessAccept(e);
        }

        protected void OnNewClientAccepted(Socket client, object state)
        {
            if (IsStopped)
                return;

            ProcessNewClient(client, SslProtocols.None);
        }

        private IAppSession ProcessNewClient(Socket socket, SslProtocols security)
        {
            if (!_readWritePool.TryPop(out SocketAsyncEventArgsProxy result))
            {
                AppServer.AsyncRun(() => socket.Shutdown(SocketShutdown.Both));
                return null;
            }

            ISocketSession socketSession = new AsyncSocketSession(socket, result);

            socketSession.InitializeSendingQueue(this.SendingQueuePool);

            socketSession.Initialize(null);

            socketSession.Start();


            //var session = CreateSession(socket, socketSession);


            //if (session == null)
            //{
            //    result.Reset();
            //    this._readWritePool.Push(result);
            //    AppServer.AsyncRun(() => socket.Shutdown(SocketShutdown.Both));
            //    return null;
            //}

            socketSession.Closed += SessionClosed;

            //var negotiateSession = socketSession as INegotiateSocketSession;

            //if (negotiateSession == null)
            //{
            //    if (RegisterSession(session))
            //    {
            //        AppServer.AsyncRun(() => socketSession.Start());
            //    }

            //    return session;
            //}

            //negotiateSession.NegotiateCompleted += OnSocketSessionNegotiateCompleted;
            //negotiateSession.Negotiate();

            NewClientAccepted(socket, socketSession);
            return null;
        }

        private void OnSocketSessionNegotiateCompleted(object sender, EventArgs e)
        {
            var socketSession = sender as ISocketSession;
            var negotiateSession = socketSession as INegotiateSocketSession;

            if (!negotiateSession.Result)
            {
                socketSession.Close(CloseReason.SocketError);
                return;
            }

            if (RegisterSession(negotiateSession.AppSession))
            {
                AppServer.AsyncRun(() => socketSession.Start());
            }
        }

        void SessionClosed(ISocketSession session, CloseReason reason)
        {
            var socketSession = session as IAsyncSocketSessionBase;
            if (socketSession == null)
                return;

            var proxy = socketSession.SocketAsyncProxy;
            proxy.Reset();
            var args = proxy.SocketEventArgs;

            //var serverState = AppServer.State;
            var pool = this._readWritePool;

            if (pool == null) // || serverState == ServerState.Stopping || serverState == ServerState.NotStarted)
            {
                if (!Environment.HasShutdownStarted)
                    args.Dispose();
                return;
            }

            if (proxy.OrigOffset != args.Offset)
            {
                args.SetBuffer(proxy.OrigOffset, 4096);
            }

            if (!proxy.IsRecyclable)
            {
                //cannot be recycled, so release the resource and don't return it to the pool
                args.Dispose();
                return;
            }

            pool.Push(proxy);

            if(CloseClientConnect!=null)
                CloseClientConnect(session);
        }

        protected IAppSession CreateSession(Socket client, ISocketSession session)
        {
            //if (m_SendTimeOut > 0)
            client.SendTimeout = 1000;// m_SendTimeOut;

            //if (m_ReceiveBufferSize > 0)
            client.ReceiveBufferSize = 4096; // m_ReceiveBufferSize;

            //if (m_SendBufferSize > 0)
            client.SendBufferSize = 4096;// m_SendBufferSize;

            //if (!Platform.SupportSocketIOControlByCodeEnum)
            //    client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, m_KeepAliveOptionValues);
            //else
            //    client.IOControl(IOControlCode.KeepAliveValues, m_KeepAliveOptionValues, m_KeepAliveOptionOutValues);

            client.NoDelay = true;
            client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);

            return this.AppServer.CreateAppSession(session);
        }

        private bool RegisterSession(IAppSession appSession)
        {
            if (AppServer.RegisterSession(appSession))
                return true;

            appSession.SocketSession.Close(CloseReason.InternalError);
            return false;
        }


    }
}
