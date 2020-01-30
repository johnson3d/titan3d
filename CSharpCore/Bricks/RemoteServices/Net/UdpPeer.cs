using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace EngineNS.Bricks.RemoteServices.Net
{
    public class UdpPeer
    {
        private UdpClient mRecv;
        private UdpClient mSend;
        private ManualResetEvent mRecvDone = new ManualResetEvent(false);
        private UInt16 mPort;
        private IPEndPoint mRecvEP;
        public class Peer
        {
            public IPEndPoint LinkPoint;
            public int PeerId;
            public Queue<byte[]> Packages = new Queue<byte[]>();
        }
        public Dictionary<string, Peer> Peers
        {
            get;
        } = new Dictionary<string, Peer>();
        public void Init(int id, UInt16 port = 11000)
        {
            mPort = port;
            mRecvEP = new IPEndPoint(IPAddress.Any, mPort);
            SendCallBack = new AsyncCallback(this.SendCallback);
            var epRecv = new IPEndPoint(IPAddress.Any, port);    //设置服务器端口，IP是本程序所在PC的内网IP
            var epSend = new IPEndPoint(IPAddress.Any, port);    //设置客户端，任意IP，任意端口号
            mRecv = new UdpClient(epRecv);   //绑定设置的服务器端口和IP

            var ctxThread = new Thread.ContextThread();
            ctxThread.StartThread("UdpRecv", (ctx) =>
            {
                IAsyncResult iar = mRecv.BeginReceive(RecvCallBack, null);
                mRecvDone.WaitOne();
            });
            mSend = new UdpClient(epSend);
        }
        private AsyncCallback RecvCallBack = null;
        private void RecvCallback(IAsyncResult iar)
        {
            if (iar.IsCompleted)
            {
                Byte[] receiveBytes = mRecv.EndReceive(iar, ref mRecvEP);
                Peer p;
                if(Peers.TryGetValue(mRecvEP.ToString(), out p)==false)
                {
                    p = new Peer();
                    p.LinkPoint = new IPEndPoint(mRecvEP.Address, mRecvEP.Port);
                    Peers.Add(p.LinkPoint.ToString(), p);
                }
                p.Packages.Enqueue(receiveBytes);
                mRecvDone.Set();
            }
        }
        public IPEndPoint FindEP(int peerId)
        {
            foreach(var i in Peers)
            {
                if (i.Value.PeerId == peerId)
                    return i.Value.LinkPoint;
            }
            return null;
        }
        public void SendBuffer(IPEndPoint ep, byte[] data, int length)
        {
            mSend.BeginSend(data, length, ep, SendCallBack, null);
        }
        private AsyncCallback SendCallBack = null;
        private void SendCallback(IAsyncResult iar)
        {
            int sendCount = mSend.EndSend(iar);
            if (sendCount == 0)
            {
                Console.WriteLine("Send a message failure...");
            }
        }
    }
}
