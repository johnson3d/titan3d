using Assimp;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay.Scene
{
    public enum EBoundVolumeType
    {
        None,
        Box,
        Sphere,
    }

    public class TtBoundVolume : IO.ISerializer, IDisposable
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
        public virtual void OnPreRead(object tagObject, object hostObject, bool fromXml)
        {
            HostNode = tagObject as TtNode;
        }
        public virtual void OnPropertyRead(object root, string prop, bool fromXml) { }
        public virtual void OnPostRead(object tagObject, object hostObject, bool fromXml) { }
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
                return ref (HostNode.EntityManager as TtEntityManager).BoundingValues.GetValue(HostNode.Id);
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
        public bool IsParalle { get; set; } = true;
        public void Process(ECS.TtEntityManager manager, float deltaTime)
        {
            var values = (manager as TtEntityManager).BoundingValues;

            if (IsParalle == false)
            {
                var t1 = Support.TtTime.HighPrecision_GetTickCount();
                VisParameter.ClearVisibles();
                {
                    int Count = 0;
                    for (int i = 0; i<manager.Entities.Count; i++)
                    {
                        var node = manager.GetEntity<TtNode>(i);
                        if (node == null)
                            continue;
                        if (node.HashVisual==false)
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
                                var type = VisParameter.CullCamera.WhichContainTypeFast(World, in aabb, true);
                                if (type == CONTAIN_TYPE.CONTAIN_TEST_OUTER)
                                    continue;
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
                VisParameter.ClearVisibles();
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
                            var type = VisParameter.CullCamera.WhichContainTypeFast(World, in aabb, true);
                            if (type == CONTAIN_TYPE.CONTAIN_TEST_OUTER)
                                return;
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
    public class TtEntityManager : ECS.TtEntityManager
    {
        internal ECS.TtComponentValues<DBoundingBox> BoundingValues = new ECS.TtComponentValues<DBoundingBox>();
        internal TtCullingSystem CullingSystem = new TtCullingSystem();
        public TtEntityManager()
        {
            this.RegisterComponent(BoundingValues);
            this.Systems.Add(CullingSystem);
        }
    }
}

namespace EngineNS.GamePlay
{
    partial class TtWorld
    {
        public Scene.TtEntityManager EntityManager { get; private set; }
    }
}

