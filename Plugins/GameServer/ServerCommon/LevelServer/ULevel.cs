using EngineNS.GamePlay;
using EngineNS.Plugins.CSCommon;
using EngineNS.Plugins.ServerCommon;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Plugins.LevelServer
{
    public partial class ULevel
    {
        public RName AssetName { get; set; }
        public EngineNS.Plugins.CSCommon.USyncActorManager<CSCommon.ISyncActor> SyncActorManager { get; } = new EngineNS.Plugins.CSCommon.USyncActorManager<CSCommon.ISyncActor>();
        public uint EnterActor(CSCommon.ISyncActor actor, ESyncIdType type)
        {
            actor.SyncId = SyncActorManager.AllocSyncId(type);
            if (SyncActorManager.RegSyncActor(actor))
            {
                return actor.SyncId;
            }
            return uint.MaxValue;
        }
        public bool LeaveActor(CSCommon.ISyncActor actor, ESyncIdType type)
        {
            var ret = SyncActorManager.UnregSyncActor(actor.SyncId);
            if (ret == false)
                return false;
            if (type == ESyncIdType.Dynamic)
            {
                ret = SyncActorManager.FreeDynamicSyncId(actor.SyncId);
                if (ret == false)
                    return false;
            }
            return true;
        }
        [ThreadStatic]
        static List<CSCommon.ISyncActor> mGhostPlacementChangedActors = new List<CSCommon.ISyncActor>();
        [ThreadStatic]
        static List<CSCommon.ISyncActor> mGhostAutoSyncDataChangedActors = new List<CSCommon.ISyncActor>();
        private void TickSyncActors()
        {
            mGhostPlacementChangedActors.Clear();
            mGhostAutoSyncDataChangedActors.Clear();
            foreach (var i in SyncActorManager.SyncActors)
            {
                var clt = i.Value as ULevelClient;
                if (clt != null && clt.Placement != null)
                {//placement changed
                    mGhostPlacementChangedActors.Add(clt);

                    if (clt.AutoSyncData.IsDirty)
                    {
                        mGhostAutoSyncDataChangedActors.Add(clt);
                    }
                }
                i.Value.Tick();
            }
            if(mGhostPlacementChangedActors.Count > 0)
            {
                using (var writer = EngineNS.IO.UMemWriter.CreateInstance())
                {
                    var ar = new EngineNS.IO.AuxWriter<EngineNS.IO.UMemWriter>(writer);
                    ar.Write((UInt16)mGhostPlacementChangedActors.Count);
                    for (int i = 0; i < mGhostPlacementChangedActors.Count; i++)
                    {
                        ar.Write(mGhostPlacementChangedActors[i].SyncId);
                        ar.Write(in mGhostPlacementChangedActors[i].Placement.TransformRef);
                    }
                    var rpcArg = new Bricks.Network.RPC.FRpcCallArg();
                    rpcArg.NetConnect = ULevelServer.Instance.ClientAllConnects;
                    RobotClient.URobot_RpcCaller.GhostsUpdatePlacement(writer, in rpcArg);
                }
            }
            if (mGhostAutoSyncDataChangedActors.Count > 0)
            {
                using (var writer = EngineNS.IO.UMemWriter.CreateInstance())
                {
                    var ar = new EngineNS.IO.AuxWriter<EngineNS.IO.UMemWriter>(writer);
                    ar.Write((UInt16)mGhostAutoSyncDataChangedActors.Count);
                    for (int i = 0; i < mGhostAutoSyncDataChangedActors.Count; i++)
                    {
                        ar.Write(mGhostAutoSyncDataChangedActors[i].SyncId);
                        var clt = mGhostAutoSyncDataChangedActors[i] as ULevelClient;
                        EngineNS.Bricks.Network.AutoSync.FSyncHelper.BuildModify(clt.AutoSyncData, ar);
                    }
                    var rpcArg = new Bricks.Network.RPC.FRpcCallArg();
                    rpcArg.NetConnect = ULevelServer.Instance.ClientAllConnects;
                    RobotClient.URobot_RpcCaller.GhostsAutoSync(writer, in rpcArg);
                }   
            }
        }
        public void Tick()
        {
            TickSyncActors();
        }
    }
    public class ULevelManager
    {
        public Dictionary<Guid, ULevel> Levels = new Dictionary<Guid, ULevel>();
        public bool RegLevel(in Guid id, ULevel level)
        {
            if (Levels.TryGetValue(id, out var v))
            {
                if (v.AssetName != level.AssetName)
                {
                    return false;
                }
            }
            else
            {
                Levels.Add(id, level);
            }
            return true;
        }
        public ULevel FindLevel(in Guid id)
        {
            if (Levels.TryGetValue(id, out var v))
            {
                return v;
            }
            return null;
        }
        public void Tick()
        {
            foreach (var i in Levels)
            {
                i.Value.Tick();
            }
        }
    }
}
