
using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
//using System.Collections.Concurrent;

namespace EngineNS.Bricks.RemoteServices.Net
{
    /// <summary>
    /// 线程安全队列
    /// </summary>
    /// <typeparam name="T">模板类型</typeparam>
    public class ThreadSafeQueue<T>
    {
        Queue<T> mQueueData = new Queue<T>();

        public void _VeryDangerousReplace(Queue<T> data)
        {
            mQueueData = data;
        }

        public int Count
        {
            get 
            { 
                lock(this)
                    return mQueueData.Count; 
            }
        }
        public bool IsEmpty
        {
            get 
            {
                lock (this) 
                    return mQueueData.Count == 0;
            } 
        }

        public void Clear()
        {
            lock (this)
            {
                mQueueData.Clear();
            }
        }

        public T Dequeue()
        {

            lock (this)
            {
                return mQueueData.Dequeue();
            }
        }

        public void Enqueue(T item)
        {
            lock (this)
            {
                mQueueData.Enqueue(item);
            }
        }

        public bool TryDequeue(out T result)
        {
            lock (this)
            {
                if (mQueueData.Count == 0)
                {
                    result = default(T);
                    return false;
                }
                result = mQueueData.Dequeue();
                return true;
            }
        }
    }

    /// <summary>
    /// 线程安全堆栈
    /// </summary>
    /// <typeparam name="T">模板类型</typeparam>
    public class ThreadSafeStack<T>
    {
        Stack<T> mStackData = new Stack<T>();

        public int Count 
        {
            get
            {
                lock (this)
                    return mStackData.Count;
            }
        }

        public void Push(T item)
        {
            lock (this)
            {
                mStackData.Push(item);
            }
        }

        public bool TryPop(out T result)
        {
            lock (this)
            {
                if (mStackData.Count == 0)
                {
                    result = default(T);
                    return false;
                }
                result = mStackData.Pop();
                return true;
            }
        }
    }

    public delegate void FOnDisconnected(TcpClient pClient);
    
    public delegate void FOnReceiveData(TcpClient pClient, byte[] pData, int nLength, Int64 recvTime);
    //internal class SocketOption
    //{
    //    static SocketOption()
    //    {
    //        Socket socket = null;
    //        try
    //        {
    //            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    //            socket.IOControl(IOControlCode.KeepAliveValues, new byte[0], null);
    //            SupportSocketIOControl = true;
    //        }
    //        catch (NotSupportedException)
    //        {
    //            SupportSocketIOControl = false;
    //        }
    //        catch (NotImplementedException)
    //        {
    //            SupportSocketIOControl = false;
    //        }
    //        catch (Exception e)
    //        {
    //            SupportSocketIOControl = true;
    //        }
    //        finally
    //        {
    //            try
    //            {
    //                if (socket != null)
    //                    socket.Close();
    //            }
    //            catch { }
    //        }
    //    }

    //    public static bool SupportSocketIOControl { get; private set; }
    //}

    public class TcpClient : NetConnection
    {
        System.Net.Sockets.Socket mSocket = null;
        public FOnDisconnected OnDisconnected;
        public FOnReceiveData OnReceiveData;

        protected Queue<RecvData> mRecvQueue = new Queue<RecvData>();

        public string hostIp;
        public UInt16 port;
        bool mRun = false;

        public override System.UInt16 Port { get { return port; } }

        public override string IpAddress { get { return hostIp; } }
        
        public class ReceiveBuffer
        {
            public byte[] buffer = new byte[102400];//大小合适。
            public int count = 0;
            public void Reset()
            {
                count = 0;
            }
        }

        //public TcpClient()
        //{
        //    keepAliveOptionValues = new byte[sizeof(uint) * 3];
        //    //whether enable KeepAlive
        //    BitConverter.GetBytes((uint)1).CopyTo(keepAliveOptionValues, 0);
        //    //how long will start first keep alive
        //    BitConverter.GetBytes((uint)3000).CopyTo(keepAliveOptionValues, sizeof(uint));
        //    //keep alive interval
        //    BitConverter.GetBytes((uint)100).CopyTo(keepAliveOptionValues, sizeof(uint) * 2);
        //}

        ReceiveBuffer mRcvBuffer = new ReceiveBuffer();

        System.Threading.ManualResetEvent TimeoutObject = new System.Threading.ManualResetEvent(false);
        private void CallBackMethod(IAsyncResult asyncresult)
        {
            try
            {
                var tcpclient = asyncresult.AsyncState as System.Net.Sockets.Socket;

                if (tcpclient.Connected)
                {
                    tcpclient.EndConnect(asyncresult);
                }
                else
                {
                    tcpclient.EndConnect(asyncresult);
                }
            }
            catch (Exception ex)
            {
                Profiler.Log.WriteException(ex);
            }
            finally
            {
                TimeoutObject.Set();
            }
        }
        public override async System.Threading.Tasks.Task<bool> Connect(string strHostIp, UInt16 nPort, int timeOutMillisecond = 3000)
        {
            await Thread.AsyncDummyClass.DummyFunc();
            Profiler.Log.WriteLine(Profiler.ELogTag.Info, "RPC", $"TcpClient.Connect:{strHostIp},{nPort}");
            Close();

            hostIp = strHostIp;
            port = nPort;
            IPEndPoint ipe = new IPEndPoint(IPAddress.Parse(hostIp), port);

            try
            {
                mSocket = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                //if (!SocketOption.SupportSocketIOControl)
                //    mSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, keepAliveOptionValues);
                //else
                //    mSocket.IOControl(IOControlCode.KeepAliveValues, keepAliveOptionValues, null);
                //mSocket.NoDelay = true;
                //mSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);

                //if (false)
                //{
                //    mSocket.Connect(ipe);
                //}
                //else
                {
                    TimeoutObject.Reset();
                    mSocket.BeginConnect(ipe, CallBackMethod, mSocket);
                    //阻塞当前线程             
                    TimeoutObject.WaitOne(timeOutMillisecond, false);
                    if (mSocket.Connected==false)
                    {
                        mSocket.Close();
                        Profiler.Log.WriteLine(Profiler.ELogTag.Info, "RPC", $"TcpClient Connect {ipe} Timeout({timeOutMillisecond})");
                    }
                }
            }
            catch (Exception e)
            {
                Profiler.Log.WriteException(e);
            }

            if (mSocket.Connected == false)
            {
                return false;
            }
            else
            {
                mRun = true;
                //mRecvThread = new System.Threading.Thread(this.RecvPackageProc);
                //mRecvThread.Name = "TcpClient";
                //mRecvThread.Start();
                try
                {
                    //ReceiveBuffer buffer = new ReceiveBuffer();
                    //mSocket.BeginReceive(buffer.buffer, buffer.count, buffer.buffer.Length - buffer.count, SocketFlags.None, EndReceive, buffer);
                    mRcvBuffer.Reset();
                    mSocket.BeginReceive(mRcvBuffer.buffer, mRcvBuffer.count, mRcvBuffer.buffer.Length - mRcvBuffer.count, SocketFlags.None, EndReceive, mRcvBuffer);
                }
                catch (System.Exception ex)
                {
                    Profiler.Log.WriteException(ex);
                }
                return true;
            }
        }
        protected void EndReceive(System.IAsyncResult ar)
        {
            var buffer = ar.AsyncState as ReceiveBuffer;
            if (buffer == null)
            {
                //关闭连接
                Close();
                return;
            }
            //var socket = buffer.Tag as System.Net.Sockets.Socket;
            //if (socket == null)
            //    return -2;
            System.Net.Sockets.SocketError error;
            try
            {
                if (mSocket == null)
                    return;
                if (!mSocket.Connected)
                {
                    //关闭连接
                    Close();
                    return;
                }
                int result = mSocket.EndReceive(ar, out error);
                if(result < 1)
                {
                    //关闭连接
                    Close();
                    return;
                }
                buffer.count += result;
                unsafe
                { 
                    fixed (byte* ptr = &buffer.buffer[0])
                    {
                        OnReceiveEventHandler((IntPtr)ptr, buffer.count);
                    }
                }
                //ReceiveBuffer buffer1 = new ReceiveBuffer();
                //mSocket.BeginReceive(buffer1.buffer, buffer1.count, buffer1.buffer.Length - buffer1.count, SocketFlags.None, EndReceive, buffer1);
                mRcvBuffer.Reset();
                mSocket.BeginReceive(mRcvBuffer.buffer, mRcvBuffer.count, mRcvBuffer.buffer.Length - mRcvBuffer.count, SocketFlags.None, EndReceive, mRcvBuffer);
                return;
            }
            catch (System.Exception ex)
            {
                Profiler.Log.WriteException(ex);
            }
            //关闭连接
            Close();
            return;
        }

        //void RecvPackageProc()
        //{
        //    try
        //    {
        //        byte[] recvBuffer = new byte[UInt16.MaxValue];
        //        while (mRun)
        //        {
        //            var recvBytes = mSocket.Receive(recvBuffer, recvBuffer.Length, 0);
        //            if (recvBytes > 0)
        //            {
        //                unsafe
        //                {
        //                    fixed (byte* ptr = &recvBuffer[0])
        //                    {
        //                        OnReceiveEventHandler((IntPtr)ptr, recvBytes);
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                System.Threading.Thread.Sleep(0);
        //            }
        //        }
        //    }
        //    catch (ArgumentNullException ane)
        //    {
        //        Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
        //    }
        //    catch (SocketException se)
        //    {
        //        Console.WriteLine("SocketException : {0}", se.ToString());
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine("Unexpected exception : {0}", e.ToString());
        //    }
        //    finally
        //    {
        //        mRun = false;
        //        mPkgBuilder.ResetPacket();
        //    }
        //}

        public int RecvPacketNumLimitter = 256;
        public int RecvPacketNumPerFrame = 10;
        public int ProcPackNumber;
        public int PacketNumber;
        public int KickedWeakPkgNum;
        public override void Update()
        {
            Update2();

            if (mSendingBuffer.mPos1 - mSendingBuffer.mHasSend > 0)// 当前发送缓冲区有数据并且上一次发送完毕就再次发送
            { 
                if (System.Threading.Interlocked.CompareExchange(ref mIsSendFinish, 0, 1) == 1)
                    InternalSend();
            }
        }
        public void Update1()
        {
            try
            {
                RecvData vBytes = null;
                PacketNumber = mRecvQueue.Count;
                int WeakPkgNum = 0;
                if (PacketNumber > RecvPacketNumLimitter)
                {
                    lock (mRecvQueue)
                    {
                        for (int i = 0; i < PacketNumber; i++)
                        {
                            vBytes = mRecvQueue.Dequeue();
                            if (vBytes == null)
                                continue;
                            unsafe
                            {
                                fixed (byte* pPkg = &vBytes.PkgData[0])
                                {
                                    if (((RPCHeader*)pPkg)->IsWeakPkg())
                                    {
                                        WeakPkgNum++;
                                        continue;
                                    }
                                    else
                                    {
                                        mRecvQueue.Enqueue(vBytes);
                                    }
                                }
                            }
                        }
                    }
                }

                KickedWeakPkgNum = WeakPkgNum;
                
                int nPacket = mRecvQueue.Count;
                if (nPacket > RecvPacketNumPerFrame)
                    nPacket = RecvPacketNumPerFrame;

                int nProcNumber = 0;
                if (OnReceiveData != null)
                {
                    while (mRecvQueue.Count > 0 && nPacket > 0)
                    {
                        if (mRecvQueue.Count == 0)
                            continue;
                        nPacket--;

                        lock (mRecvQueue)
                        {
                            vBytes = mRecvQueue.Dequeue();
                        }
                        if (vBytes == null)
                            break;
                        nProcNumber++;

                        OnReceiveData(this, vBytes.PkgData, vBytes.Length, vBytes.RecvTime);
                        vBytes.Dispose();
                    }
                }
                ProcPackNumber = nProcNumber;
            }
            catch (System.Exception ex)
            {
                Profiler.Log.WriteException(ex);
            }

            if (mRun == false && this.Connected==false)
            {
                if(OnDisconnected!=null)
                    OnDisconnected(this);
            }
        }

        public void Update2()
        {
            try
            {
                RecvData vBytes = null;
                PacketNumber = mRecvQueue.Count;
                int WeakPkgNum = 0;
                int nProcNumber = 0;
                bool DiscardWeak = false;
                if (PacketNumber > RecvPacketNumLimitter)
                    DiscardWeak = true;

                lock (mRecvQueue)
                {
                    for (int i = 0; i < PacketNumber; i++)
                    {
                        if (mRecvQueue.Count == 0)
                            break;
                        vBytes = mRecvQueue.Dequeue();
                        if (vBytes == null)
                            continue;
                        if (DiscardWeak)
                        {
                            unsafe
                            {
                                fixed (byte* pPkg = &vBytes.PkgData[0])
                                {
                                    if (((RPCHeader*)pPkg)->IsWeakPkg())
                                    {
                                        WeakPkgNum++;
                                        continue;
                                    }
                                }
                            }
                        }

                        nProcNumber++;
                        if (OnReceiveData != null)
                            OnReceiveData(this, vBytes.PkgData, vBytes.Length, vBytes.RecvTime);
                        vBytes.Dispose();
                    }
                }

                KickedWeakPkgNum = WeakPkgNum;
                ProcPackNumber = nProcNumber;
            }
            catch (System.Exception ex)
            {
                Profiler.Log.WriteException(ex);
            }

            if (mRun == false && this.Connected==false)
            {
                if (OnDisconnected != null)
                {
                    OnDisconnected(this);
                }
            }
        }

        public int SendPacketSizeLimitter = 1024 * 1024;//超过0.5M的发送缓冲，需要忽略若包的发送请求
        public int TotalSendSize = 0;

        private Queue<SendingBuffer> mSendBufferQueue = new Queue<SendingBuffer> ();
        private SendingBuffer mSendingBuffer = new SendingBuffer();
        private int mIsSendFinish = 1;
        
        public override void SendBuffer(IntPtr ptr, int count)
        {
            unsafe
            {
                var pHeader = (RPCHeader*)ptr;
                pHeader->PackageSize = (ushort)count;
            }

            lock (mSendingBuffer)
            {
                var buffer = mSendingBuffer;
                if (buffer.mPos1 + count > buffer.mBuffer.Length)
                {
                    while (mIsSendFinish == 1)
                        break;
                    //InternalSend();
                }
                //lock (mSendBufferQueue)
                //{
                unsafe
                {
                    fixed(byte* dest = &buffer.mBuffer[buffer.mPos1])
                    {
                        CoreSDK.SDK_Memory_Copy(dest, (void*)ptr, (UInt32)count);
                    }
                }
                //Array.Copy(data, offset, buffer.mBuffer, buffer.mPos1, count);
                buffer.mPos1 += count;
                //}
            }

            //if (System.Threading.Interlocked.CompareExchange(ref mIsSendFinish, 0, 1) == 1)
            //{
            //    InternalSend();
            //}
        }

        void InternalSend()
        {
            var sendingBuffer = mSendingBuffer;

            if (mSendBufferQueue.Count > 0)
                mSendingBuffer = mSendBufferQueue.Dequeue();
            else
                mSendingBuffer = new SendingBuffer();

            StartSend(sendingBuffer);
        }

        void StartSend(SendingBuffer sendingBuffer)
        {
            //lock (mSendBufferQueue)
            //{
                try
                {
                    mSocket?.BeginSend(sendingBuffer.mBuffer, sendingBuffer.mHasSend, sendingBuffer.mPos1 - sendingBuffer.mHasSend, SocketFlags.None, SendCallback, sendingBuffer);
                }
                catch (Exception e)
                {
                    Close();
                    Profiler.Log.WriteException(e);
                }
            //}
        }

        void SendCallback(IAsyncResult ar)
        {
            var sendingBuffer = ar.AsyncState as SendingBuffer;

            int send = 0;
            try
            {
                send = mSocket.EndSend(ar);
            }
            catch (Exception e)
            {
                Close();
                Profiler.Log.WriteException(e);
                return;
            }
            sendingBuffer.mHasSend += send;
            if (sendingBuffer.mHasSend < sendingBuffer.mPos1)
                StartSend(sendingBuffer);
            else
            {
                sendingBuffer.mHasSend = 0;
                sendingBuffer.mPos1 = 0;
                mSendBufferQueue.Enqueue(sendingBuffer);
                //if (mSendingBuffer.mPos1 - mSendingBuffer.mHasSend > 0)
                //    InternalSend();
                //else
                System.Threading.Interlocked.Exchange(ref mIsSendFinish, 1);
            }
        }

        internal class SendingBuffer
        {
            internal byte[] mBuffer = new byte[64 * 1024];
            internal int mHasSend = 0;
            internal int mPos1 = 0;
        }

        public virtual void Close()
        {
            mRun = false;
            System.Threading.Interlocked.Exchange(ref mIsSendFinish, 1);
            try
            {
                if (mSocket != null)// && mSocket.Connected)
                {
                    //mSocket.Disconnect(true);
                    //状元说，第一次看到在close里面写Disconnect的
                    mSocket.Close();
                    mSocket = null;
                    
                }   
            }
            catch (Exception ex)
            {
                mSocket = null;
                Profiler.Log.WriteException(ex);
            }
        }

        public virtual async System.Threading.Tasks.Task Reconnect()
        {
            await Connect(hostIp, port);
        }

        public override bool Connected
        {
            get
            {
                if (mSocket == null)
                    return false;
                return mSocket.Connected;
            }
        }

        //CSUtility.Net.NetState mState = CSUtility.Net.NetState.Invalid;
        //public virtual CSUtility.Net.NetState State
        //{
        //    get { return mState; }
        //}

        #region 回调转换
        public bool DiscardAllPacket
        {
            get;
            set;
        } = false;
        private void OnPacketOK(RecvData rcvData)
        {
            if (DiscardAllPacket)
                return;
            lock (mRecvQueue)
            {
                mRecvQueue.Enqueue(rcvData);
            }
        }
        Net.PkgPoolManager mPkgPoolMgr = new Net.PkgPoolManager();
        protected PacketBuilder mPkgBuilder = new PacketBuilder();
        private void OnReceiveEventHandler(IntPtr pData, int length)
        {
            unsafe
            {
                bool result = mPkgBuilder.ParsePackage((byte*)pData.ToPointer(), (UInt32)length, this.OnPacketOK, mPkgPoolMgr);
                //if (result==false)
                //{
                //    Log.FileLog.WriteLine("TcpClient [{0}:{1}] ParsePackage Failed", hostIp, port);
                //}
            }
        }
        #endregion
    }    
}