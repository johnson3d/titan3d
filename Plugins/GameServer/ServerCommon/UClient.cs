using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using EngineNS.Bricks.Network.RPC;

namespace EngineNS.Plugins.ServerCommon
{
    public class UClient
    {
        public UInt16 ClientIndex { get; internal set; }
        public string UserName { get; set; }
        public Guid SessionId { get; set; }
        public System.DateTime LoginTime { get; set; }

        public virtual void Tick()
        {

        }
    }
    public class UClientManager
    {
        public UClient[] Clients = new UClient[UInt16.MaxValue];
        public UClient GetClient(UInt16 index)
        {
            if (index == UInt16.MaxValue)
                return null;
            return Clients[index];
        }
        public int ClientCount { get; private set; }
        public UClient FindClient(in Guid sessionId)
        {
            for (int i = 0; i < UInt16.MaxValue; i++)
            {
                if (Clients[i] != null && Clients[i].SessionId == sessionId)
                {
                    return Clients[i];
                }
            }
            return null;
        }
        public UClient RegClient<T>(in Guid sessionId) where T : UClient, new()
        {
            for (int i = 0; i < UInt16.MaxValue; i++)
            {
                if (Clients[i] != null && Clients[i].SessionId == sessionId)
                {
                    return Clients[i];
                }
            }

            for (int i = 0; i < UInt16.MaxValue; i++)
            {
                if (Clients[i] == null)
                {
                    Clients[i] = new T();
                    Clients[i].ClientIndex = (UInt16)i;
                    Clients[i].SessionId = sessionId;
                    ClientCount++;
                    return Clients[i];
                }
            }
            return null;
        }
        public UClient UnregClient(in Guid sessionId)
        {
            for (int i = 0; i < UInt16.MaxValue; i++)
            {
                if (Clients[i].SessionId == sessionId)
                {
                    var saved = Clients[i];
                    Clients[i] = null;
                    ClientCount--;
                    return saved;
                }
            }
            return null;
        }
    }
}
