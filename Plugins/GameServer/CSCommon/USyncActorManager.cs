using EngineNS.GamePlay;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Plugins.CSCommon
{
    public interface ISyncActor
    {
        uint SyncId { get; set; }
        TtPlacementBase Placement { get; }
        void Tick();
    }
    public enum ESyncIdType
    {
        Persistance,//Monster,Npc...
        Dynamic,
    }
    public class USyncActorManager<T> where T : class, ISyncActor
    {// one instance per level
        private uint mPersistanceSyncIdAllocator = 0;
        public const int MaxDynamicCount = UInt16.MaxValue;
        public const uint DynamicStart = uint.MaxValue - UInt16.MaxValue;
        private bool[] DynamicSyncIdStates = new bool[MaxDynamicCount];

        public USyncActorManager()
        {
            for (int i = 0; i < MaxDynamicCount; i++)
            {
                DynamicSyncIdStates[i] = false;
            }
        }
        public uint AllocSyncId(ESyncIdType type)
        {
            lock(this)
            {
                switch (type)
                {
                    case ESyncIdType.Persistance:
                        System.Diagnostics.Debug.Assert(mPersistanceSyncIdAllocator < DynamicStart);
                        return mPersistanceSyncIdAllocator++;
                    case ESyncIdType.Dynamic:
                        {
                            for (uint i = 0; i < MaxDynamicCount; i++)
                            {
                                if (DynamicSyncIdStates[i] == false)
                                {
                                    DynamicSyncIdStates[i] = true;
                                    return DynamicStart + i;
                                }
                            }
                        }
                        break;
                }
                return uint.MaxValue;
            }
        }
        public bool FreeDynamicSyncId(uint id)
        {
            lock(this)
            {
                if (id < DynamicStart)
                    return false;
                id -= DynamicStart;
                if (DynamicSyncIdStates[id] == false)
                    return false;
                DynamicSyncIdStates[id] = false;
                return true;
            }
        }
        public Dictionary<uint, T> SyncActors { get; } = new Dictionary<uint, T>();
        public T FindSyncActor(uint id)
        {
            if (SyncActors.TryGetValue(id, out var actor))
                return actor;
            return null;
        }
        public bool RegSyncActor(T actor)
        {
            if (SyncActors.TryGetValue(actor.SyncId, out var syncActor))
            {
                return actor == syncActor;
            }
            else
            {
                SyncActors.Add(actor.SyncId, actor);
                return true;
            }
        }
        public bool UnregSyncActor(uint id)
        {
            return SyncActors.Remove(id);
        }
    }
}
