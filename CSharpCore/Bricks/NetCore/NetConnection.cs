using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.NetCore
{
    public abstract class NetConnection
    {
        public RPCRouter Router;
        public int mLimitLevel;
        public virtual bool Connected
        {
            get;
        } = false;
        public virtual async System.Threading.Tasks.Task<bool> Connect(string strHostIp, UInt16 nPort, int timeOutMillisecond = 3000)
        {
            await Thread.AsyncDummyClass.DummyFunc();
            return false;
        }
        public abstract void Update();

        public abstract void SendBuffer(IntPtr ptr, int count);

        public virtual void Disconnect() { }

        public abstract System.UInt16 Port { get; }

        public abstract string IpAddress { get; }        
    }
}
