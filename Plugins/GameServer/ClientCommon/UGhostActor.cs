using EngineNS.GamePlay;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Plugins.ClientCommon
{
    public class UGhostActor : CSCommon.ISyncActor
    {
        public uint SyncId { get; set; }
        public UPlacementBase Placement { get; } = new UPlacement();
        public void Tick()
        {

        }
        public CSCommon.UClientAutoSyncData AutoSyncData { get; } = new CSCommon.UClientAutoSyncData();
    }
}
