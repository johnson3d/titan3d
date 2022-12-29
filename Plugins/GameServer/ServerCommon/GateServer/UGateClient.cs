using System;
using System.Collections.Generic;

namespace EngineNS.Plugins.GateServer
{
    public class UGateClient : ServerCommon.UClient
    {
        public Bricks.Network.INetConnect ClientConnect { get; set; }
        public Bricks.Network.INetConnect LevelConnect { get; set; }
        public UInt16 IndexInLevel { get; set; } = UInt16.MaxValue;
        public UInt16 IndexInRoot { get; set; } = UInt16.MaxValue;
    }
}
