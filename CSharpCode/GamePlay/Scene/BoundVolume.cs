using EngineNS.Bricks.Collision.Octree;
using EngineNS.GamePlay.Scene;
using EngineNS.Profiler;
using System;
using System.Collections.Generic;
using System.Text;
using static EngineNS.GamePlay.TtWorld;

namespace EngineNS.GamePlay.Scene
{
    public enum EBoundVolumeType
    {
        None,
        Box,
        Sphere,
    }

    public class TtBoundVolume : IO.BaseSerializer, IDisposable
    {
        public TtBoundVolume()
        {
            
        }
        public virtual void Dispose()
        {
            HostNode = null;
        }
        public virtual EBoundVolumeType BVType
        {
            get => EBoundVolumeType.None;
        }
        public override void OnPreRead(object tagObject, object hostObject, bool fromXml)
        {
            HostNode = tagObject as TtNode;
        }
        public TtNode HostNode { get; set; } = null;
        public BoundingBox mLocalAABB;
        public BoundingBox LocalAABB
        {
            get => mLocalAABB;
            set
            {
                mLocalAABB = value;
                OnVolumeChanged();
            }
        }
        public DBoundingBox AABB;//包含HostNode Child的AABB
        private DBoundingBox mAbsAABB;//经过AbsTransform变换的AABB
        public ref DBoundingBox AbsAABB
        {
            get
            {
                if (HostNode.EntityManager == null || HostNode.Id < 0)
                {
                    return ref mAbsAABB;
                }
                return ref (HostNode.EntityManager as TtWorldEntityManager).BoundingValues.GetValue(HostNode.Id);
            }
        }
        protected virtual void OnVolumeChanged()
        {
            HostNode.UpdateAABB();
        }
    }
    public class UBoxBV : TtBoundVolume
    {     
        public Vector3 mExtent = new Vector3(1,1,1);
        public override EBoundVolumeType BVType
        {
            get => EBoundVolumeType.Box;
        }
        protected override void OnVolumeChanged()
        {
            HostNode.UpdateAABB();
        }
    }
    public class USphereBV : TtBoundVolume
    {
        Vector3 mCenter;
        [Rtti.Meta("")]
        public Vector3 Center
        {
            get => mCenter;
            set
            {
                mCenter = value;
                OnVolumeChanged();
            }
        }
        float mRadius = 1;
        [Rtti.Meta("")]
        public float Radius
        {
            get => mRadius;
            set
            {
                mRadius = value;
                OnVolumeChanged();
            }
        }
        public override EBoundVolumeType BVType
        {
            get => EBoundVolumeType.Sphere;
        }
        protected override void OnVolumeChanged()
        {
            var extent = new Vector3(Radius);
            mLocalAABB = new BoundingBox(mCenter - extent, mCenter + extent);
            AABB.FromSingle(in mLocalAABB);
            HostNode.UpdateAABB();
        }
    }


    public class TtCullingSystem : ECS.ISystem
    {
        public GamePlay.TtWorld World;
        public GamePlay.TtWorld.TtVisParameter VisParameter;
        public bool IsParallel { get; set; } = true;
        public void Process(ECS.TtEntityManager manager, float deltaTime)
        {
            var values = (manager as TtWorldEntityManager).BoundingValues;

            if (IsParallel == false)
            {
                var t1 = Support.TtTime.HighPrecision_GetTickCount();
                {
                    int Count = 0;
                    for (int i = 0; i<manager.Entities.Count; i++)
                    {
                        var node = manager.GetEntity<TtNode>(i);
                        if (node == null)
                            continue;
                        if (node.HashVisual==false)
                            continue;
                        if (VisParameter.CullType == TtWorld.TtVisParameter.EVisCull.Shadow && node.IsCastShadow == false)
                            continue;
                        Count++;

                        var bv = node.BoundVolume;
                        if (node.RootNode != World.Root)
                            continue;
                        if (VisParameter.OnVisitNode != null && VisParameter.OnVisitNode(bv.HostNode, VisParameter) == false)
                            continue;
                        if (node.HasStyle(TtNode.ENodeStyles.VisibleFollowParent))
                            continue;

                        ref var aabb = ref values.GetValue(node.Id);
                        if (!node.HasStyle(TtNode.ENodeStyles.VisibleAlways))
                        {
                            if (VisParameter.CullCamera != null)
                            {
                                if (VisParameter.DontFrustumCull == false)
                                {
                                    var type = VisParameter.CullCamera.WhichContainTypeFast(World, in aabb, true);
                                    if (type == CONTAIN_TYPE.CONTAIN_TEST_OUTER)
                                        continue;
                                }
                            }
                            else
                            {
                                var ct = DBoundingBox.Contains(in VisParameter.CullBox, in aabb);
                                if (ct == ContainmentType.Disjoint)
                                    continue;
                            }
                        }

                        if (node.HasStyle(TtNode.ENodeStyles.Invisible) == false)
                        {
                            node.OnGatherVisibleMeshes(VisParameter);
                        }
                        if (node.HasStyle(TtNode.ENodeStyles.ChildrenInvisible) == false)
                        {
                            node.GatherFollowVisibleMeshes(VisParameter);
                        }
                    }
                }
                var t2 = Support.TtTime.HighPrecision_GetTickCount();
                if (t2-t1>100)
                {

                }
            }
            else
            {
                var t3 = Support.TtTime.HighPrecision_GetTickCount();
                //var taskGroupNum = Math.Min(TtEngine.Instance.EventPoster.PooledThreadNum, 16);
                TtEngine.Instance.EventPoster.ParallelFor(manager.Entities.Count, static (i, state) =>
                {
                    var pThis = state.GetForArgument0<TtCullingSystem>();
                    var manager = pThis.World.EntityManager;
                    var VisParameter = pThis.VisParameter;
                    var World = pThis.World;
                    var values = manager.BoundingValues;

                    var node = manager.GetEntity<TtNode>(i);
                    if (node == null)
                        return;
                    if (node.HashVisual==false)
                        return;
                    if (VisParameter.CullType == TtWorld.TtVisParameter.EVisCull.Shadow && node.IsCastShadow == false)
                        return;

                    var bv = node.BoundVolume;
                    if (node.RootNode != World.Root)
                        return;
                    if (VisParameter.OnVisitNode != null && VisParameter.OnVisitNode(bv.HostNode, VisParameter) == false)
                        return;
                    if (node.HasStyle(TtNode.ENodeStyles.VisibleFollowParent))
                        return;

                    ref var aabb = ref values.GetValue(node.Id);
                    if (!node.HasStyle(TtNode.ENodeStyles.VisibleAlways))
                    {
                        if (VisParameter.CullCamera != null)
                        {
                            if (VisParameter.DontFrustumCull == false)
                            {
                                var type = VisParameter.CullCamera.WhichContainTypeFast(World, in aabb, true);
                                if (type == CONTAIN_TYPE.CONTAIN_TEST_OUTER)
                                    return;
                            }   
                        }
                        else
                        {
                            var ct = DBoundingBox.Contains(in VisParameter.CullBox, in aabb);
                            if (ct == ContainmentType.Disjoint)
                                return;
                        }
                    }

                    if (node.HasStyle(TtNode.ENodeStyles.Invisible) == false)
                    {
                        node.OnGatherVisibleMeshes(VisParameter);
                    }
                    if (node.HasStyle(TtNode.ENodeStyles.ChildrenInvisible) == false)
                    {
                        node.GatherFollowVisibleMeshes(VisParameter);
                    }
                }, -1, this);
                var t4 = Support.TtTime.HighPrecision_GetTickCount();
                if (t4-t3>100)
                {

                }
            }
        }
    }
    public class TtWorldEntityManager : ECS.TtEntityManager
    {
        internal ECS.TtComponentValues<DBoundingBox> BoundingValues = new ECS.TtComponentValues<DBoundingBox>();
        internal TtCullingSystem CullingSystem = new TtCullingSystem();
        public TtWorldEntityManager()
        {
            this.RegisterComponent(BoundingValues);
            this.Systems.Add(CullingSystem);
        }
    }
}

namespace EngineNS.GamePlay
{
    partial class TtWorld : INotifyHost
    {
        public Scene.TtWorldEntityManager EntityManager { get; private set; }
        #region Octree
        private TtCollideOctree mCollideOctree = new TtCollideOctree();
        public TtCollideOctree CollideOctree
        {
            get => mCollideOctree;
        }
        #endregion
        #region Volume BVH
        /// <summary>
        /// Wrapper that sits inside the volume BVH and weakly references a TtVolumeBaseNode.
        /// Prevents the BVH from holding strong references to scene nodes.
        /// </summary>
        public class TtBvhVolumeEntry
        {
            public WeakReference<Scene.TtVolumeBaseNode> WeakVolume;
            public int ProxyId = Bricks.Collision.BVH.TtDynamicBVH<TtBvhVolumeEntry>.NullNode;

            public TtBvhVolumeEntry(Scene.TtVolumeBaseNode volume)
            {
                WeakVolume = new WeakReference<Scene.TtVolumeBaseNode>(volume);
            }

            public bool TryGetVolume(out Scene.TtVolumeBaseNode volume)
            {
                return WeakVolume.TryGetTarget(out volume);
            }
        }

        // ── Volume spatial index ──
        Bricks.Collision.BVH.TtDynamicBVH<TtBvhVolumeEntry> mVolumeBvh
            = new Bricks.Collision.BVH.TtDynamicBVH<TtBvhVolumeEntry>(16, 0.5f, 2.0f);
        public Bricks.Collision.BVH.TtDynamicBVH<TtBvhVolumeEntry> VolumeBvh { get => mVolumeBvh; }

        // Parallel lists: mAllVolumes[i] weakly references the node, mAllVolumeEntries[i] is the BVH wrapper.
        // When a WeakRef dies, we use the corresponding entry's ProxyId to clean up the BVH leaf.
        readonly List<WeakReference<Scene.TtVolumeBaseNode>> mAllVolumes = new List<WeakReference<Scene.TtVolumeBaseNode>>();
        readonly List<TtBvhVolumeEntry> mAllVolumeEntries = new List<TtBvhVolumeEntry>();

        internal void RegisterVolume(Scene.TtVolumeBaseNode volume)
        {
            if (volume.BvhProxyId != Bricks.Collision.BVH.TtDynamicBVH<TtBvhVolumeEntry>.NullNode)
                return;
            var entry = new TtBvhVolumeEntry(volume);
            volume.VolumeEntry = entry;
            var volData = volume.GetNodeData<Scene.TtVolumeBaseNode.TtVolumeBaseData>();
            if (volData != null && volData.IsUnbound)
            {
                var hugeBox = new Aabb(DVector3.Zero, new Vector3(1e6f));
                volume.BvhProxyId = mVolumeBvh.InsertProxy(in hugeBox, entry);
            }
            else
            {
                var aabb = new Aabb(in volume.BoundVolume.AbsAABB);
                volume.BvhProxyId = mVolumeBvh.InsertProxy(in aabb, entry);
            }
            entry.ProxyId = volume.BvhProxyId;
            mAllVolumes.Add(new WeakReference<Scene.TtVolumeBaseNode>(volume));
            mAllVolumeEntries.Add(entry);
        }

        internal void UnregisterVolume(Scene.TtVolumeBaseNode volume)
        {
            if (volume.BvhProxyId == Bricks.Collision.BVH.TtDynamicBVH<TtBvhVolumeEntry>.NullNode)
                return;
            mVolumeBvh.RemoveProxy(volume.BvhProxyId);
            volume.BvhProxyId = Bricks.Collision.BVH.TtDynamicBVH<TtBvhVolumeEntry>.NullNode;
            volume.VolumeEntry = null;
            for (int i = mAllVolumes.Count - 1; i >= 0; i--)
            {
                if (!mAllVolumes[i].TryGetTarget(out var target) || target == volume)
                {
                    mAllVolumes.RemoveAt(i);
                    mAllVolumeEntries.RemoveAt(i);
                }
            }
        }

        internal void UpdateVolumeProxy(Scene.TtVolumeBaseNode volume)
        {
            if (volume.BvhProxyId == Bricks.Collision.BVH.TtDynamicBVH<TtBvhVolumeEntry>.NullNode)
                return;
            var aabb = new Aabb(in volume.BoundVolume.AbsAABB);
            mVolumeBvh.UpdateProxy(volume.BvhProxyId, in aabb, DVector3.Zero);
        }

        /// <summary>
        /// Query all volumes that overlap a world-space point (typically the camera position).
        /// Results are appended to <paramref name="results"/>.
        /// </summary>
        public void QueryVolumes(in DVector3 point, List<Scene.TtVolumeBaseNode> results)
        {
            mVolumeBvh.QueryPoint(in point, (int proxyId, TtBvhVolumeEntry entry) =>
            {
                if (entry.TryGetVolume(out var vol))
                    results.Add(vol);
                return true;
            });
        }

        /// <summary>
        /// Query all volumes of a specific subtype that overlap a world-space point.
        /// </summary>
        public void QueryVolumes<T>(in DVector3 point, List<T> results) where T : Scene.TtVolumeBaseNode
        {
            mVolumeBvh.QueryPoint(in point, (int proxyId, TtBvhVolumeEntry entry) =>
            {
                if (entry.TryGetVolume(out var vol) && vol is T typed)
                    results.Add(typed);
                return true;
            });
        }

        /// <summary>
        /// Release LUT textures on PostProcessVolumes that haven't been used recently.
        /// Called once per frame from HdrNode to reclaim VRAM for idle volumes.
        /// </summary>
        public void TickVolumeLutCleanup()
        {
            for (int i = mAllVolumes.Count - 1; i >= 0; i--)
            {
                if (!mAllVolumes[i].TryGetTarget(out var volume))
                {
                    // Volume was GC'd — purge its BVH leaf via the entry's cached ProxyId
                    var entry = mAllVolumeEntries[i];
                    if (entry.ProxyId != Bricks.Collision.BVH.TtDynamicBVH<TtBvhVolumeEntry>.NullNode)
                    {
                        mVolumeBvh.RemoveProxy(entry.ProxyId);
                        entry.ProxyId = Bricks.Collision.BVH.TtDynamicBVH<TtBvhVolumeEntry>.NullNode;
                    }
                    mAllVolumes.RemoveAt(i);
                    mAllVolumeEntries.RemoveAt(i);
                    continue;
                }
                if (volume is Scene.TtPostProcessVolumeNode ppVol)
                    ppVol.ReleaseLutIfStale();
            }
        }
        #endregion
        public void OnHostNotify(object host, in FHostNotify notify)
        {
            switch (notify.Info)
            {
                case "OnSceneLoaded":
                    {
                        
                    }
                    break;
                case "OnNodeMove":
                    {
                    }
                    break;
            }
        }
    }
}

