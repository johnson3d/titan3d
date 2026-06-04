using Assimp;
using EngineNS.Bricks.CodeBuilder;
using EngineNS.Bricks.GpuDriven;
using EngineNS.Bricks.WorldSimulator;
using EngineNS.Graphics.Pipeline;
using EngineNS.Profiler;
using EngineNS.Thread.Async;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace EngineNS.GamePlay.Scene
{
    public class TtNodeAttribute : Attribute
    {
        public System.Type NodeDataType = typeof(TtNodeData);
        public string DefaultNamePrefix = "Node";
    }
    [Rtti.Meta("",NameAlias = new string[] { "EngineNS.GamePlay.Scene.UNodeData@EngineCore" })]
    [EGui.Controls.PropertyGrid.TtCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    public partial class TtNodeData : IO.BaseSerializer
    {
        public int GetStructSize()
        {
            //return System.Runtime.InteropServices.Marshal.SizeOf(this.GetType());
            return 1024;
        }
        [Rtti.Meta("")]
        public TtNode.ENodeStyles NodeStyles { get; set; } = 0;
        [Rtti.Meta("",Order = 0)]
        public string Name { get; set; }
        TtBoundVolume mBoundVolume;
        [Rtti.Meta("",Order = 1)]
        public TtBoundVolume BoundVolume
        {
            get => mBoundVolume;
            set => mBoundVolume = value;
        }
        TtPlacementBase mPlacement = null;
        [Rtti.Meta("",Order = 2)]
        public TtPlacementBase Placement
        {
            get => mPlacement;
            set
            {
                mPlacement = value;
            }
        }
        public bool HasStyle(TtNode.ENodeStyles style)
        {
            return (NodeStyles & style) == style;
        }
        public void SetStyle(TtNode.ENodeStyles style)
        {
            NodeStyles |= style;
        }
        public void UnsetStyle(TtNode.ENodeStyles style)
        {
            NodeStyles &= ~style;
        }
        public Graphics.Pipeline.TtHitProxy.EHitproxyType HitproxyType
        {
            get
            {
                return (Graphics.Pipeline.TtHitProxy.EHitproxyType)((uint)(NodeStyles & TtNode.ENodeStyles.HitproxyMasks) >> 2);
            }
            set
            {
                uint flags = (((uint)value & ((uint)TtNode.ENodeStyles.HitproxyMasks >> 2)) << 2);
                UnsetStyle(TtNode.ENodeStyles.HitproxyMasks);
                SetStyle((TtNode.ENodeStyles)flags);
            }
        }
    }
    [Rtti.Meta("")]
    [EGui.Controls.PropertyGrid.TtCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    public partial class TtNode : ECS.IEntity
    {
        #region ECS
        public ECS.TtEntityManager EntityManager { get; set; } = null;
        public int Id { get; set; } = -1;
        public ECS.TtComponentValues<T> GetComponentValues<T>() where T : struct
        {
            if (EntityManager==null)
                return null;

            return EntityManager.FindComponentValues<T>();
        }
        public void OnAddToManager()
        {
            if (BoundVolume!=null)
            {
                (EntityManager as TtWorldEntityManager).BoundingValues.SetValue(Id, in BoundVolume.AbsAABB);
            }
        }
        public void OnRemoveFromManager()
        {
            if (BoundVolume!=null)
            {
                BoundVolume.AbsAABB = (EntityManager as TtWorldEntityManager).BoundingValues.GetValue(Id);
            }
        }
        #endregion

        static int mNodeAliveNumber = 0;
        public static int NodeAliveNumber
        {
            get => mNodeAliveNumber;
        }
        public TtNode()
        {
            System.Threading.Interlocked.Increment(ref mNodeAliveNumber);
        }
        ~TtNode()
        {
            TtEngine.Instance?.GfxDevice.HitproxyManager.UnmapProxy(this);
            System.Threading.Interlocked.Decrement(ref mNodeAliveNumber);
        }
        public struct FTreeCopyStat
        {
            public void Reset()
            {
                SuccessNode = 0;
                FailureNode = 0;
            }
            public int SuccessNode;
            public int FailureNode;
        }
        protected virtual void OnNodeCopyTreeData(TtNode src, ref FTreeCopyStat stat)
        {

        }
        public static void NodeTreeCopyData(TtWorld world, TtNode tar, TtNode src, ref FTreeCopyStat stat)
        {
            var type = src.GetType();
            if (tar.GetType() != type)
            {
                stat.FailureNode++;
                return;
            }
            stat.SuccessNode++;
            TtEngine.Instance.DataCopyer.DataCopy(tar.NodeData, src.NodeData);
            if (tar.NodeData.Placement != null)
                tar.NodeData.Placement.HostNode = tar;
            if (tar.NodeData.BoundVolume != null)
                tar.NodeData.BoundVolume.HostNode = tar;
            //TtEngine.Instance.TaskCollector.AddWaitTask(
            //    tar.InitializeNode(world, tar.NodeData, tar.NodeData.BoundVolume.BVType, tar.Placement.GetType())
            //    );
            
            if (tar.Children.Count != src.Children.Count)
            {
                stat.FailureNode++;
                return;
            }
            for (int i = 0; i < src.Children.Count; i++)
            {
                NodeTreeCopyData(world, tar.Children[i], src.Children[i], ref stat);
            }
            tar.OnNodeCopyTreeData(src, ref stat);
        }
        public virtual void Dispose()
        {
            if (EntityManager!=null)
            {
                EntityManager.RemoveEntity(Id);
                EntityManager = null;
            }
            if (mWorld != null && mWorld.CollideOctree != null)
            {
                mWorld.CollideOctree.Remove(this);
            }
            this.Behavior?.DestroyNode(this);
            this.BoundVolume?.Dispose();
        }
        [Rtti.Meta("")]
        public void DisposeWithChildren()
        {
            var copy = new List<TtNode>(Children);
            foreach (var i in copy)
            {
                i.Parent = null;
                i.DisposeWithChildren();
            }
            Children.Clear();
            //this.UpdateAABB();
            this.Dispose();
        }
        public static async Thread.Async.TtTask<TtNode> ConcreateNode(TtWorld world, TtNode tarNode, TtNode node, object extArg)
        {
            TtNode result = tarNode;
            bool isNewObj = false;
            if (result == null)
            {
                result = Rtti.TtTypeDescManager.CreateInstance(node.GetType()) as TtNode;
                isNewObj = true;
            }
            else
            {
                System.Diagnostics.Debug.Assert(tarNode.GetType() == node.GetType());
            }
            var nd = Rtti.TtTypeDescManager.CreateInstance(node.NodeData.GetType()) as TtNodeData;
            var meta = Rtti.TtClassMetaManager.Instance.GetMeta(Rtti.TtTypeDesc.TypeOf(node.NodeData.GetType()));
            meta.CopyObjectMetaField(nd, node.NodeData);
            nd.Placement.HostNode = result;
            nd.BoundVolume.HostNode = result;
            await result.InitializeNode(world, nd, node.BoundVolumeType, node.Placement.GetType());
            result.SetPrefabTemplate(node.NodeData);

            foreach (var i in node.Children)
            {
                var cnodeTar = result.FindFirstChild(i.NodeName, i.GetType());
                var cnode = await ConcreateNode(world, cnodeTar, i, extArg);
                cnode.Parent = result;
            }

            if (isNewObj)
                await result.OnPostInitNode(result.Parent, extArg);

            return result;
        }
        public const string EditorKeyword = "UNode";
        protected virtual async Thread.Async.TtTask<bool> InitializeNode(GamePlay.TtWorld world, TtNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            mWorld = world;
            NodeData = data;

            if (NodeData != null)
            {
                if (NodeData.Placement == null)
                {
                    if (placementType == null)
                    {
                        placementType = typeof(TtPlacement);
                    }
                    NodeData.Placement = Rtti.TtTypeDescManager.CreateInstance(placementType) as TtPlacementBase;
                    NodeData.Placement.HostNode = this;
                    switch (bvType)
                    {
                        case EBoundVolumeType.None:
                            {
                                NodeData.BoundVolume = null;
                            }
                            break;
                        case EBoundVolumeType.Box:
                            {
                                var boxBV = new UBoxBV();
                                boxBV.HostNode = this;
                                boxBV.LocalAABB = new BoundingBox(Vector3.Zero);
                                NodeData.BoundVolume = boxBV;
                            }
                            break;
                        case EBoundVolumeType.Sphere:
                            {
                                var sphereBV = new USphereBV();
                                sphereBV.HostNode = this;
                                sphereBV.Center = Vector3.Zero;
                                sphereBV.Radius = 1;
                                NodeData.BoundVolume = sphereBV;
                            }
                            break;
                    }

                    Placement.SetTransform(in FTransform.Identity);
                }
                else
                {
                    NodeData.Placement.HostNode = this;
                    if (NodeData.BoundVolume != null)
                        NodeData.BoundVolume.HostNode = this;
                }
                UpdateAABB();
                UpdateAbsTransform();
            }

            if (NodeData.BehaviorName != null)
            {
                mBehaviorGetter = new TtBehaviorGetter();
            }

            this.UnsetStyle(ENodeStyles.NoTick);
            return true;
        }
        //Callback: Children ready!
        protected virtual async Thread.Async.TtTask OnPostInitNode(TtNode parent, object extArg)
        {
            Behavior?.BeginPlay(this);
        }
        public static TtNodeAttribute GetNodeAttribute(System.Type type)
        {
            return type.GetCustomAttribute(typeof(TtNodeAttribute)) as TtNodeAttribute;
        }
        public static TtNodeAttribute GetNodeAttribute<T>() where T : TtNode
        {
            return typeof(T).GetCustomAttribute(typeof(TtNodeAttribute)) as TtNodeAttribute;
        }
        #region Creator
        public static TtNodeData CreateNodeData(System.Type nodeType)
        {
            var attr = nodeType.GetCustomAttribute(typeof(TtNodeAttribute)) as TtNodeAttribute;
            if (attr == null || attr.NodeDataType == null)
            {
                return null;
            }
            return Rtti.TtTypeDescManager.CreateInstance(attr.NodeDataType) as TtNodeData;
        }
        public static TtNodeData CreateNodeData<T>() where T : TtNode
        {
            var attr = typeof(T).GetCustomAttribute(typeof(TtNodeAttribute)) as TtNodeAttribute;
            if (attr == null || attr.NodeDataType == null)
            {
                return null;
            }
            return Rtti.TtTypeDescManager.CreateInstance(attr.NodeDataType) as TtNodeData;
        }
        public delegate TtTask FPostSpawnNode<T>(T node);
        public static async Thread.Async.TtTask<T> SpawnNode<T>(TtNode parent, FPostSpawnNode<T> postAction, TtNodeData data = null, EBoundVolumeType bvType = EBoundVolumeType.Box, Type placementType = null, TtWorld world = null, object extArg = null)
            where T : TtNode, new()
        {
            var result = new T();
            if (data == null)
            {
                data = CreateNodeData<T>();
                if (data == null)
                {
                    return null;
                }
            }
            if (placementType == null)
            {
                placementType = typeof(TtPlacement);
            }
            if (world == null)
            {
                if (parent == null)
                    return null;
                world = parent.GetWorld();
            }
            await result.InitializeNode(world, data, bvType, placementType);
            result.Parent = parent;
            if (postAction != null)
                await postAction(result);
            await result.OnPostInitNode(parent, extArg);
            return result;
        }
        public delegate TtTask FPostSpawnNode(TtNode node);
        [Rtti.Meta("")]
        public static async Thread.Async.TtTask<TtNode> SpawnNode(TtNode parent, System.Type nodeType, FPostSpawnNode postAction, TtNodeData data = null, EBoundVolumeType bvType = EBoundVolumeType.Box, Type placementType = null, TtWorld world = null, object extArg = null)
        {
            if (nodeType.IsSubclassOf(typeof(TtNode)) == false)
            {
                return null;
            }
            var result = Rtti.TtTypeDescManager.CreateInstance(nodeType) as TtNode;
            if (data == null)
            {
                data = CreateNodeData(nodeType);
                if (data == null)
                {
                    return null;
                }
            }
            if (placementType == null)
            {
                placementType = typeof(TtPlacement);
            }
            if (world == null)
            {
                if (parent == null)
                    return null;
                world = parent.GetWorld();
            }
            await result.InitializeNode(world, data, bvType, placementType);
            result.Parent = parent;
            if (postAction != null)
                await postAction(result);
            await result.OnPostInitNode(parent, extArg);
            return result;
        }
        #endregion
        public EBoundVolumeType BoundVolumeType
        {
            get
            {
                if (NodeData.BoundVolume == null)
                    return EBoundVolumeType.None;
                else if (NodeData.BoundVolume is UBoxBV)
                    return EBoundVolumeType.Box;
                else if (NodeData.BoundVolume is USphereBV)
                    return EBoundVolumeType.Sphere;
                return EBoundVolumeType.None;
            }
        }
        public void GetWorldSpaceBoundingBox(out DBoundingBox outVal)
        {
            //var matrix = this.Placement.AbsTransform.ToMatrixNoScale();
            //BoundingBox.Transform(in AABB, in matrix, out outVal);            
            DBoundingBox.TransformNoScale(in RefAABB, in Placement.AbsTransform, out outVal);
        }
        [Flags]
        public enum ENodeStyles : uint
        {
            VisibleAlways = (1 << 0),
            VisibleFollowParent = (1 << 1),
            HitproxyMasks = (1 << 2) | (1 << 3),//value: 0,1,2 NoProxy,RootProxy,FollowProxy
            DiscardAABB = (1 << 4),
            CastShadow = (1 << 5),
            AcceptShadow = (1 << 6),
            HideBoundShape = (1 << 7),
            NoPickedDraw = (1 << 8),
            SelfInvisible = (1 << 9),
            ChildrenInvisible = (1 << 10),
            Reserved_0 = (1 << 11), //SceneManaged = (1 << 11),
            Transient = (1 << 12),
            NoTick = (1 << 13),
            ParallelTick = (1 << 14),
            ForceGatherNode = (1 << 15),
            BuildNavMesh = (1 << 16),
            EnableHitproxyInGame = (1 << 17),
            IsCollide = (1 << 18),
            Invisible = SelfInvisible | ChildrenInvisible,
        }
        [Flags]
        public enum ENodeRuntimeStyles : uint
        {
            IsDirty = (1 << 0),
            IsSelected = (1 << 1),
            IsPrefab = (1 << 2),
        }
        public uint RuntimeStyles { get; set; } = 0;
        public ENodeStyles NodeStyles
        {
            get
            {
                if (NodeData == null)
                    return (ENodeStyles)0;
                return NodeData.NodeStyles;
            }
            set
            {
                if (NodeData != null)
                {
                    NodeData.NodeStyles = value;
                }
            }
        }
        public bool SetVisible(bool visible, bool bRecursive = false)
        {
            if (visible)
            {
                UnsetStyle(ENodeStyles.SelfInvisible);
            }
            else
            {
                SetStyle(ENodeStyles.SelfInvisible);
            }
            if (bRecursive)
            {
                foreach (var i in Children)
                {
                    i.SetVisible(visible, true);
                }
            }
            return true;
        }
        #region Styles
        public bool HasStyle(ENodeStyles style)
        {
            return (NodeStyles & style) == style;
        }
        public void SetStyle(ENodeStyles style)
        {
            NodeStyles |= style;
        }
        public void UnsetStyle(ENodeStyles style)
        {
            NodeStyles &= ~style;
        }
        #region RuntimeStyle
        public void UnsetRuntimeStyle(ENodeRuntimeStyles style)
        {
            RuntimeStyles &= ~(uint)style;
        }
        public bool HasRuntimeStyle(ENodeRuntimeStyles style)
        {
            return (RuntimeStyles & (uint)style) == (uint)style;
        }
        public void SetRuntimeStyle(ENodeRuntimeStyles style)
        {
            RuntimeStyles |= (uint)style;
        }
        [Category("Option")]
        public bool IsPrefab
        {
            get => this.HasRuntimeStyle(ENodeRuntimeStyles.IsPrefab);// && (mTemplateNodeData != null);
            set
            {
                if (value)
                {
                    this.SetRuntimeStyle(ENodeRuntimeStyles.IsPrefab);
                }
                else
                {
                    this.UnsetRuntimeStyle(ENodeRuntimeStyles.IsPrefab);
                }
            }
        }
        [Browsable(false)]
        public bool Selected
        {
            get => HasRuntimeStyle(ENodeRuntimeStyles.IsSelected);
            set
            {
                if (value)
                {
                    SetRuntimeStyle(ENodeRuntimeStyles.IsSelected);
                }
                else
                {
                    UnsetRuntimeStyle(ENodeRuntimeStyles.IsSelected);
                }
            }
        }
        [Browsable(false)]
        public bool IsDirty 
        {
            get => HasRuntimeStyle(ENodeRuntimeStyles.IsDirty);
            set
            {
                if (value)
                    SetRuntimeStyle(ENodeRuntimeStyles.IsDirty);
                else
                    UnsetRuntimeStyle(ENodeRuntimeStyles.IsDirty);
            }
        }
        public void CheckDirty()
        {
            if (IsDirty)
            {
                UpdateAbsTransform();
                IsDirty = false;
            }
        }
        #endregion

        [Category("Option")]
        public virtual bool IsNoTick
        {
            get
            {
                return HasStyle(ENodeStyles.NoTick);
            }
            set
            {
                if (value)
                {
                    SetStyle(ENodeStyles.NoTick);
                }
                else
                {
                    UnsetStyle(ENodeStyles.NoTick);
                }
            }
        }
        [Category("Option")]
        public virtual bool IsParallelTick
        {
            get
            {
                return HasStyle(ENodeStyles.ParallelTick);
            }
            set
            {
                if (value)
                {
                    SetStyle(ENodeStyles.ParallelTick);
                }
                else
                {
                    UnsetStyle(ENodeStyles.ParallelTick);
                }
            }
        }
        [Category("Option")]
        public virtual bool IsVisibleAlways
        {
            get
            {
                return HasStyle(ENodeStyles.VisibleAlways);
            }
            set
            {
                if (value)
                {
                    SetStyle(ENodeStyles.VisibleAlways);
                }
                else
                {
                    UnsetStyle(ENodeStyles.VisibleAlways);
                }
            }
        }
        [Category("Option")]
        public virtual bool IsVisibleFollowParent
        {
            get
            {
                return HasStyle(ENodeStyles.VisibleFollowParent);
            }
            set
            {
                if (value)
                {
                    SetStyle(ENodeStyles.VisibleFollowParent);
                }
                else
                {
                    UnsetStyle(ENodeStyles.VisibleFollowParent);
                }
            }
        }
        [Category("Option")]
        public virtual bool IsCastShadow
        {
            get
            {
                return HasStyle(ENodeStyles.CastShadow);
            }
            set
            {
                if (value)
                {
                    SetStyle(ENodeStyles.CastShadow);
                }
                else
                {
                    UnsetStyle(ENodeStyles.CastShadow);
                }
            }
        }
        [Category("Option")]
        public virtual bool IsAcceptShadow
        {
            get
            {
                return HasStyle(ENodeStyles.AcceptShadow);
            }
            set
            {
                if (value)
                {
                    SetStyle(ENodeStyles.AcceptShadow);
                }
                else
                {
                    UnsetStyle(ENodeStyles.AcceptShadow);
                }
            }
        }
        [Category("Option")]
        public virtual bool IsForceGatherNode
        {
            get
            {
                return HasStyle(ENodeStyles.ForceGatherNode);
            }
            set
            {
                if (value)
                {
                    SetStyle(ENodeStyles.ForceGatherNode);
                }
                else
                {
                    UnsetStyle(ENodeStyles.ForceGatherNode);
                }
            }
        }
        [Category("Option")]
        public virtual bool IsBuildNavMesh
        {
            get
            {
                return HasStyle(ENodeStyles.BuildNavMesh);
            }
            set
            {
                if (value)
                {
                    SetStyle(ENodeStyles.BuildNavMesh);
                }
                else
                {
                    UnsetStyle(ENodeStyles.BuildNavMesh);
                }
            }
        }
        [Category("Option")]
        public virtual bool IsEnableHitproxyInGame
        {
            get
            {
                return HasStyle(ENodeStyles.EnableHitproxyInGame);
            }
            set
            {
                if (value)
                {
                    SetStyle(ENodeStyles.EnableHitproxyInGame);
                }
                else
                {
                    UnsetStyle(ENodeStyles.EnableHitproxyInGame);
                }
            }
        }
        [Category("Option")]
        public virtual bool IsCollide
        {
            get
            {
                return HasStyle(ENodeStyles.IsCollide);
            }
            set
            {
                if (value)
                {
                    SetStyle(ENodeStyles.IsCollide);
                }
                else
                {
                    UnsetStyle(ENodeStyles.IsCollide);
                }
            }
        }
        #endregion

        #region BaseFields
        public virtual bool HashVisual
        {
            get => false;
        }
        public virtual bool IsSceneManagedType()
        {
            return false;
        }

        [Rtti.Meta("")]
        public virtual Guid NodeId
        {
            get => Guid.Empty;
            set { }
        }
        public string SaveHash { get; set; } = null;

        public TtNode FindNode(in Guid nodeId, bool bRecursive)
        {
            if(nodeId == Guid.Empty)
                return null;
            for(int i=0; i<Children.Count; i++)
            {
                if (Children[i].NodeId == nodeId)
                {
                    return Children[i];
                }
                else
                {
                    if (bRecursive)
                    {
                        var fd = Children[i].FindNode(in nodeId, true);
                        if (fd!=null)
                            return fd;
                    }
                }
            }
            
            return null;
        }

        public virtual UInt32 SceneId
        {
            get => UInt32.MaxValue;
            internal set { }
        }
        [Category("Option")]
        public virtual string NodeName
        {
            get
            {
                if (NodeData != null)
                {
                    return NodeData.Name;
                }
                return SceneId.ToString();
            }
            set
            {
                if (NodeData != null)
                {
                    NodeData.Name = value;
                }
            }
        }
        WeakReference<TtNode> mParent;
        //protected TtNode mParent;
        [Rtti.Meta("")]
        public virtual TtNode Parent
        {
            get
            {
                if (mParent == null)
                    return null;
                if (mParent.TryGetTarget(out var result))
                    return result;
                return null;
            }
            set
            {
                var oldParent = Parent;
                if (oldParent == value)
                    return;
                var oldScene = GetNearestParentScene();
                try
                {
                    if (value != null)
                    {
                        System.Threading.Monitor.Enter(value);
                    }
                    if (oldParent != null)
                    {
                        oldParent.Children.Remove(this);
                    }
                    if (value != null)
                    {
                        mParent = new WeakReference<TtNode>(value);
                        if (value.Children.Contains(this) == false)
                            value.Children.Add(this);
                    }
                    else
                    {
                        mParent = null;
                    }
                    ParentChanged(oldParent, value);

                    var newScene = GetNearestParentScene();
                    if (oldScene != newScene)
                    {
                        ParentSceneChanged(oldScene, newScene);
                    }

                    if (value != null && Placement != null)
                    {
                        Placement.Position = Placement.Position;
                    }
                }
                finally
                {
                    if (value != null)
                    {
                        System.Threading.Monitor.Exit(value);
                    }
                }
            }
        }
        [Rtti.Meta("")]
        public TtNode RootNode
        {
            get
            {
                if(Parent == null)
                    return this;
                return Parent.RootNode;
            }
        }
        [Rtti.Meta("")]
        public TtScene ParentScene
        {
            get
            {
                return GetNearestParentScene();
            }
        }
        TtWorld mWorld = null;
        public TtWorld GetWorld()
        {
            return mWorld;
        }
        [Rtti.Meta("",Flags = Rtti.MetaAttribute.EMetaFlags.NoSerializable | Rtti.MetaAttribute.EMetaFlags.MacrossReadOnly)]
        public TtWorld HostWorld { get => GetWorld(); }
        public List<TtNode> Children { get; } = new List<TtNode>();
        TtNodeData mNodeData = null;
        TtNodeData mTemplateNodeData = null;
        public void SetPrefabTemplate(TtNodeData data)
        {
            mTemplateNodeData = data;
            this.SetRuntimeStyle(ENodeRuntimeStyles.IsPrefab);
            OnSetPrefabTemplate();
            OnAbsTransformChanged();
        }
        protected virtual void OnSetPrefabTemplate()
        {

        }
        [Category("Option")]
        public TtNodeData NodeData
        {
            get
            {
                if (IsPrefab && mTemplateNodeData != null)
                {
                    return mTemplateNodeData;
                }   
                return mNodeData;
            }
            set
            {
                mNodeData = value;
                if (Placement != null)
                {
                    UpdateAABB();
                    UpdateAbsTransform();
                }
            }
        }
        public T GetNodeData<T>() where T : TtNodeData
        {
            return NodeData as T;
        }
        [Category("Option")]
        public DVector3 Location
        {
            get
            {
                if (Placement == null)
                    return DVector3.Zero;
                return Placement.AbsTransform.Position;
            }
        }
        [Category("Option")]
        public virtual TtPlacementBase Placement
        {
            get { return NodeData?.Placement; }
        }
        [Category("Option")]
        public TtBoundVolume BoundVolume
        {
            get { return NodeData?.BoundVolume; }
        }
        private static DBoundingBox Empty = DBoundingBox.Empty;
        public ref DBoundingBox RefAABB
        {
            get
            {
                if (BoundVolume == null)
                    return ref Empty;
                else
                    return ref BoundVolume.AABB;
            }
        }
        public DBoundingBox AABB
        {
            get => RefAABB;
        }
        public ref DBoundingBox RefAbsAABB
        {
            get
            {
                if (BoundVolume == null)
                    return ref Empty;
                else
                    return ref BoundVolume.AbsAABB;
            }
        }
        public DBoundingBox AbsAABB
        {
            get => RefAbsAABB;
        }
        #endregion

        #region Virtual Interface
        private void ParentChanged(TtNode prev, TtNode cur)
        {
            OnParentChanged(prev, cur);
            this.IterateNodes((node, arg) =>
            {
                node.OnRootParentChanged(this, prev, cur);
                return true;
            }, null);
        }
        protected virtual void OnParentChanged(TtNode prev, TtNode cur)
        {

        }
        protected virtual void OnRootParentChanged(TtNode root, TtNode prev, TtNode cur)
        {
            var world = GetWorld();
            if (world != null && world.EntityManager != null && this.BoundVolume!=null)
            {   
                if (cur == null)
                {
                    world.EntityManager.RemoveEntity(this.Id);
                }
                else
                {
                    world.EntityManager.AddEntity(this);
                }
            }
        }
        private void ParentSceneChanged(TtScene prev, TtScene cur)
        {
            OnParentSceneChanged(prev, cur);
            foreach(var i in Children)
            {
                if (i is TtScene)
                    continue;
                i.ParentSceneChanged(prev, cur);
            }
        }
        protected virtual void OnParentSceneChanged(TtScene prev, TtScene cur)
        {

        }
        public virtual bool TryTreeGatherVisibleMeshes(TtWorld.TtVisParameter rp)
        {
            return true;
        }
        public virtual void OnGatherVisibleMeshes(TtWorld.TtVisParameter rp)
        {
            rp.AddVisibleNode(this);
        }
        public virtual void GatherFollowVisibleMeshes(TtWorld.TtVisParameter rp)
        {
            foreach(var i in Children)
            {
                if (i.HasStyle(ENodeStyles.VisibleFollowParent) && i.TryTreeGatherVisibleMeshes(rp))
                {
                    i.OnGatherVisibleMeshes(rp);
                    i.GatherFollowVisibleMeshes(rp);
                }
            }
        }
        #endregion

        #region Link
        public void ClearChildren()
        {
            var copy = new List<TtNode>(Children);
            foreach (var i in copy)
            {
                i.ClearChildren();

                i.OnRemoveFromWorld();
                if (mWorld != null && mWorld.CollideOctree != null)
                {
                    mWorld.CollideOctree.Remove(i);
                }
                i.Parent = null;
            }
            Children.Clear();
            UpdateAABB();
        }
        [Rtti.Meta("")]
        public TtNode FindFirstChild(string name,
            [Rtti.MetaParameter(FilterType = typeof(TtNode), ConvertOutArguments = Rtti.MetaParameterAttribute.EArgumentFilter.R)]
            System.Type type = null, bool bRecursive = false)
        {
            if(!string.IsNullOrEmpty(name))
            {
                foreach (var i in Children)
                {
                    if (i.NodeName == null)
                        continue;
                    if (i.NodeName.Contains(name))
                    {
                        if (type == null)
                        {
                            return i;
                        }
                        else
                        {
                            if (type == i.GetType())
                                return i;
                        }
                    }
                }
            }
            else
            {
                foreach (var i in Children)
                {
                    if (i.GetType() == type || i.GetType().IsSubclassOf(type))
                    {
                        return i;
                    }
                }
            }
            
            if (bRecursive)
            {
                foreach (var i in Children)
                {
                    var result = i.FindFirstChild(name, type, true);
                    if (result != null)
                        return result;
                }
            }
            return null;
        }

        public T FindFirstChild<T>(string name = null, bool bRecursive = false) where T : TtNode
        {
            foreach (var i in Children)
            {
                if (i.GetType() == typeof(T) || i.GetType().IsSubclassOf(typeof(T)))
                {
                    if (name == null)
                        return i as T;
                    else
                    {
                        if (i.NodeName.Contains(name))
                            return i as T;
                    }
                }
            }
            if (bRecursive)
            {
                foreach (var i in Children)
                {
                    var result = i.FindFirstChild<T>(name, true);
                    if (result != null)
                        return result;
                }
            }
            return null;
        }
        /// <summary>
        /// 从场景树中移除本节点. 对应的 .node 磁盘文件不会立即删除,
        /// 而是记录到所属 TtScene 的 PendingDeleteNodeFiles 列表,
        /// 等 TtScene.SaveAssetTo 时才真正删除. 这样"删了不保存关闭"
        /// 不会丢失节点文件.
        /// Outliner 右键菜单 Delete 和键盘 Delete 键统一走此入口.
        /// </summary>
        public void DeleteFromScene()
        {
            var scene = GetNearestParentScene();
            Parent = null;
            if (scene?.AssetName != null)
            {
                var file = NodeId.ToString() + NodeExt;
                var nodefiles = IO.TtFileManager.GetFiles(scene.AssetName.Address + "/nodes", file, true);
                foreach (var nodefile in nodefiles)
                {
                    scene.PendingDeleteNodeFiles.Add(nodefile);
                }
            }
            Dispose();
        }

        public TtScene GetNearestParentScene()
        {
            if (GetType() == typeof(TtScene) || GetType().IsSubclassOf(typeof(TtScene)))
                return this as TtScene;
            if (Parent != null)
                return Parent.GetNearestParentScene();
            else
                return null;
        }
        public delegate bool FOnVisitNode(TtNode node, object arg);
        public bool DFS_VisitNodeTree(FOnVisitNode visitor, object arg)
        {
            if (!HasStyle(ENodeStyles.SelfInvisible) && visitor(this, arg))
            {
                return true;
            }
            if (!HasStyle(ENodeStyles.ChildrenInvisible))
            {
                for(int i=Children.Count - 1; i>=0; i--)
                {
                    if (Children[i].DFS_VisitNodeTree(visitor, arg))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        #endregion

        #region Save&Load
        [Flags]
        public enum ENodeFlags : uint
        {
            IsNodeDesc = 1,
            IgnoreNodeDesc = (1 << 1),
            IsBehaviorData = (1 << 2),
        }
        protected virtual void OnBeforeSaveNodeData()
        {

        }
        //public unsafe void SaveChildNode(TtNode scene, EngineNS.XndHolder xnd, EngineNS.XndNode node)
        //{
        //    foreach(var i in Children)
        //    {
        //        if (i.HasStyle(ENodeStyles.Transient))
        //            continue;

        //        var typeStr = Rtti.TtTypeDesc.TypeStr(i.GetType());
        //        using (var nd = xnd.NewNode(typeStr, 1, 0))
        //        {
        //            node.AddNode(nd);

        //            if (i.NodeData != null)
        //            {
        //                uint nodeFlags = (uint)ENodeFlags.IsNodeDesc;
        //                if (this.IsPrefab)
        //                {
        //                    nodeFlags |= (uint)ENodeFlags.IgnoreNodeDesc;
        //                }
        //                var dataAttr = nd.GetOrAddAttribute(Rtti.TtTypeDesc.TypeStr(i.NodeData.GetType()), 1, nodeFlags, true);

        //                //using (var dataAttr = xnd.NewAttribute(Rtti.TtTypeDesc.TypeStr(i.NodeData.GetType()), 1, nodeFlags))
        //                using (var ar = dataAttr.GetWriter((ulong)NodeData.GetStructSize() * 2))
        //                {
        //                    i.OnBeforeSaveNodeData();
        //                    ar.Write(i.NodeData);
        //                }

        //                if (i.Behavior != null)
        //                {
        //                    dataAttr = nd.GetOrAddAttribute(Rtti.TtTypeDesc.TypeStr(i.Behavior.GetType()), 1, (uint)ENodeFlags.IsBehaviorData, true);
        //                    using (var ar = dataAttr.GetWriter((ulong)NodeData.GetStructSize() * 2))
        //                    {
        //                        ar.Write(i.Behavior);
        //                    }
        //                }
        //            }
        //            i.SaveChildNode(scene, xnd, nd);
        //        }   
        //    }
        //}

        //public async Thread.Async.TtTask<bool> LoadChildNode(GamePlay.TtWorld world, TtNode scene, EngineNS.XndNode node, bool bTryFindNode)
        //{
        //    var isNoAABB = this.HasStyle(ENodeStyles.DiscardAABB);
        //    this.SetStyle(ENodeStyles.DiscardAABB);
        //    for (uint i = 0; i < node.GetNumOfNode(); i++)
        //    {
        //        var cld = node.GetNode(i);
        //        var cldTypeStr = cld.NativeSuper.Name;
                
        //        var attr = cld.FindFirstAttributeByFlags((uint)ENodeFlags.IsNodeDesc);
        //        if (attr.NativePointer == IntPtr.Zero)
        //        {
        //            continue;
        //        }
        //        if ((attr.Flags & (uint)ENodeFlags.IgnoreNodeDesc) != 0)
        //        {
        //            this.IsPrefab = true;
        //        }
        //        var nodeTypeDesc = Rtti.TtTypeDesc.TypeOf(attr.Name);
        //        TtNodeData nodeData = Rtti.TtTypeDescManager.CreateInstance(nodeTypeDesc) as TtNodeData;
        //        var nd = Rtti.TtTypeDescManager.CreateInstance(Rtti.TtTypeDesc.TypeOf(cldTypeStr)) as TtNode;
        //        if (nd == null || nodeData == null)
        //        {
        //            Profiler.Log.WriteLine<Profiler.TtGameplayGategory>(Profiler.ELogTag.Warning, $"SceneNode Load failed: NodeDataType={attr.Name}, NodeData={cldTypeStr}");
        //            continue;
        //        }
                
        //        using (var ar = attr.GetReader(nd))
        //        {
        //            IO.ISerializer data = nodeData;
        //            try
        //            {
        //                //ar.Tag = nd;
        //                System.Type placementType = typeof(TtPlacement);
        //                if (this.IsPrefab == false)
        //                {
        //                    if (false == ar.ReadTo(data, this))
        //                    {
        //                        continue;
        //                    }

        //                    if (bTryFindNode)
        //                    {
        //                        var old = FindFirstChild(nodeData.Name, nd.GetType());
        //                        if (old != null)
        //                        {
        //                            var typeDesc = Rtti.TtTypeDesc.TypeOf(nodeData.GetType());
        //                            var meta = Rtti.TtClassMetaManager.Instance.GetMeta(typeDesc);
        //                            meta.CopyObjectMetaField(old.NodeData, nodeData);
        //                            nodeData = old.NodeData;
        //                            nd.IsDirty = true;
        //                        }
        //                    }
        //                }

        //                var ok = await nd.InitializeNode(world, nodeData, EBoundVolumeType.Box, placementType);
        //                if (ok == false)
        //                {
        //                    Profiler.Log.WriteLine<Profiler.TtNetCategory>(Profiler.ELogTag.Warning, $"SceneNode Load Initialize failed: NodeDataType={attr.Name}, NodeData={cldTypeStr}");
        //                    continue;
        //                }
        //                var sn = scene as TtScene;
        //                if (sn != null)
        //                {
        //                    sn.NumOfLoadedNode++;
        //                }
        //                //nd.NodeData = data as UNodeData;
        //                //nd.OnNodeLoaded(this);
        //            }
        //            catch (Exception ex)
        //            {
        //                Profiler.Log.WriteException(ex);
        //                Profiler.Log.WriteLine<Profiler.TtNetCategory>(Profiler.ELogTag.Warning, $"Scene({scene}): Node({nd.NodeData?.Name}) load failed");
        //            }
        //        }

        //        if (nd.Behavior != null)
        //        {
        //            attr = cld.FindFirstAttributeByFlags((uint)ENodeFlags.IsBehaviorData);
        //            if (attr.NativePointer != IntPtr.Zero)
        //            {
        //                var bhvType = Rtti.TtTypeDesc.TypeOf(attr.Name);
        //                var bhv = Rtti.TtTypeDescManager.CreateInstance(bhvType) as TtBehavior;
        //                try
        //                {
        //                    using (var ar = attr.GetReader(nd))
        //                    {
        //                        IO.ISerializer ro = bhv;
        //                        ar.ReadTo(ro);
        //                    }

        //                    var typeStr = Rtti.TtTypeDesc.TypeStr(nd.Behavior.GetType());
        //                    var meta = Rtti.TtClassMetaManager.Instance.GetMeta(typeStr);
        //                    if (meta != null)
        //                    {
        //                        meta.CopyObjectMetaField(nd.Behavior, bhv);
        //                    }
        //                }
        //                catch (Exception ex)
        //                {
        //                    Profiler.Log.WriteException(ex);
        //                }
        //            }
        //        }
                    
        //        if (nd.Placement != null)
        //        {
        //            nd.Parent = this;
        //            await nd.LoadChildNode(world, scene, cld, bTryFindNode);
        //        }
        //        await nd.OnPostInitNode(this, null);
        //    }
        //    if (isNoAABB == false)
        //    {
        //        this.UnsetStyle(ENodeStyles.DiscardAABB);
        //        this.UpdateAABB();
        //    }
        //    return true;
        //}
        public unsafe void SaveNodeTree(TtNode scene, EngineNS.XndHolder xnd, EngineNS.XndNode node, bool bSaveChildren)
        {
            //System.Diagnostics.Debug.Assert(node.NativeSuper.Name==Rtti.TtTypeDesc.TypeOf(this.GetType()).TypeString);
            node.NativeSuper.SetName(Rtti.TtTypeDesc.TypeOf(this.GetType()).TypeString);
            if (this.NodeData != null)
            {
                uint nodeFlags = (uint)ENodeFlags.IsNodeDesc;
                if (this.IsPrefab)
                {
                    nodeFlags |= (uint)ENodeFlags.IgnoreNodeDesc;
                }
                var dataAttr = node.GetOrAddAttribute(Rtti.TtTypeDesc.TypeStr(this.NodeData.GetType()), 1, nodeFlags, true);

                //using (var dataAttr = xnd.NewAttribute(Rtti.TtTypeDesc.TypeStr(i.NodeData.GetType()), 1, nodeFlags))
                using (var ar = dataAttr.GetWriter((ulong)NodeData.GetStructSize() * 2))
                {
                    this.OnBeforeSaveNodeData();
                    ar.Write(this.NodeData);
                }

                if (this.Behavior != null)
                {
                    dataAttr = node.GetOrAddAttribute(Rtti.TtTypeDesc.TypeStr(this.Behavior.GetType()), 1, (uint)ENodeFlags.IsBehaviorData, true);
                    using (var ar = dataAttr.GetWriter((ulong)NodeData.GetStructSize() * 2))
                    {
                        ar.Write(this.Behavior);
                    }
                }
            }
            if (bSaveChildren)
            {
                foreach (var i in Children)
                {
                    //var typeStr = Rtti.TtTypeDesc.TypeStr(i.GetType());
                    using (var cld = xnd.NewNode("", 1, 0))
                    {
                        node.AddNode(cld);
                        i.SaveNodeTree(scene, xnd, cld, bSaveChildren);
                    }
                }
            }
        }
        public static async Thread.Async.TtTask<TtNode> LoadNodeTree(TtWorld world, TtScene scene, TtNode parent, EngineNS.XndNode node, bool bTryFindNode, object extArg, bool bLoadChildren = true)
        {
            var attr = node.FindFirstAttributeByFlags((uint)ENodeFlags.IsNodeDesc);
            if (attr.NativePointer == IntPtr.Zero)
            {
                return null;
            }
            var nodeTypeStr = node.NativeSuper.Name;
            var nodeTypeDesc = Rtti.TtTypeDesc.TypeOf(attr.Name);
            TtNodeData nodeData = Rtti.TtTypeDescManager.CreateInstance(nodeTypeDesc) as TtNodeData;
            var result = Rtti.TtTypeDescManager.CreateInstance(Rtti.TtTypeDesc.TypeOf(nodeTypeStr)) as TtNode;
            if (result == null || nodeData == null)
            {
                Profiler.Log.WriteLine<Profiler.TtGameplayGategory>(Profiler.ELogTag.Warning, $"SceneNode Load failed: NodeDataType={attr.Name}, NodeData={nodeTypeStr}");
                return null;
            }
            if ((attr.Flags & (uint)ENodeFlags.IgnoreNodeDesc) != 0)
            {
                result.IsPrefab = true;
            }

            if (scene==null && result is TtScene)
            {
                scene = result as TtScene;
            }
            
            using (var ar = attr.GetReader(result))
            {
                IO.ISerializer data = nodeData;
                try
                {
                    //ar.Tag = nd;
                    System.Type placementType = typeof(TtPlacement);
                    if (result.IsPrefab == false)
                    {
                        if (false == ar.ReadTo(data, parent))
                        {
                            return null;
                        }

                        if (bTryFindNode)
                        {
                            var old = parent.FindFirstChild(nodeData.Name, result.GetType());
                            if (old != null)
                            {
                                var typeDesc = Rtti.TtTypeDesc.TypeOf(nodeData.GetType());
                                var meta = Rtti.TtClassMetaManager.Instance.GetMeta(typeDesc);
                                meta.CopyObjectMetaField(old.NodeData, nodeData);
                                nodeData = old.NodeData;
                                result.IsDirty = true;
                            }
                        }
                    }

                    var ok = await result.InitializeNode(world, nodeData, EBoundVolumeType.Box, placementType);
                    if (ok == false)
                    {
                        Profiler.Log.WriteLine<Profiler.TtNetCategory>(Profiler.ELogTag.Warning, $"SceneNode Load Initialize failed: NodeDataType={attr.Name}, NodeData={nodeTypeStr}");
                        return null;
                    }
                    var sn = scene as TtScene;
                    if (sn != null)
                    {
                        sn.NumOfLoadedNode++;
                    }
                    //nd.NodeData = data as UNodeData;
                    //nd.OnNodeLoaded(this);
                }
                catch (Exception ex)
                {
                    Profiler.Log.WriteException(ex);
                    Profiler.Log.WriteLine<Profiler.TtNetCategory>(Profiler.ELogTag.Warning, $"Scene({scene}): Node({result.NodeData?.Name}) load failed");
                }
            }

            if (result.Behavior != null)
            {
                attr = node.FindFirstAttributeByFlags((uint)ENodeFlags.IsBehaviorData);
                if (attr.NativePointer != IntPtr.Zero)
                {
                    var bhvType = Rtti.TtTypeDesc.TypeOf(attr.Name);
                    var bhv = Rtti.TtTypeDescManager.CreateInstance(bhvType) as TtBehavior;
                    try
                    {
                        using (var ar = attr.GetReader(result))
                        {
                            IO.ISerializer ro = bhv;
                            ar.ReadTo(ro);
                        }

                        var typeStr = Rtti.TtTypeDesc.TypeStr(result.Behavior.GetType());
                        var meta = Rtti.TtClassMetaManager.Instance.GetMeta(typeStr);
                        if (meta != null)
                        {
                            meta.CopyObjectMetaField(result.Behavior, bhv);
                        }
                    }
                    catch (Exception ex)
                    {
                        Profiler.Log.WriteException(ex);
                    }
                }
            }
            result.Parent = parent;

            if (bLoadChildren)
            {
                for (uint i = 0; i < node.GetNumOfNode(); i++)
                {
                    await LoadNodeTree(world, scene, result, node.GetNode(i), bTryFindNode, extArg);
                }
            }

            await result.OnPostInitNode(parent, extArg);
            return result;
        }
        public virtual void OnSceneLoaded()
        {
            this.HitproxyType = this.HitproxyType;
        }
        #endregion

        #region Hierarchical Collide
        protected virtual void OnAbsTransformChanged()
        {

        }
        protected virtual void OnAbsAABBChanged()
        {
            var world = GetWorld();
            if (world == null || world.CollideOctree == null || world.CollideOctree.mOctree == null)
                return;
            var aabb = new Aabb(RefAbsAABB);
            if (OctreeEntry != null)
            {
                world.CollideOctree.Move(this, in aabb);
            }
            else
            {
                world.CollideOctree.Add(this, aabb);
            }
        }
        public void UpdateAbsTransform()
        {
            //if (NodeData == null || Placement == null)
            if (Placement == null)
                return;
            if (Parent == null)
            {
                Placement.AbsTransform = Placement.TransformData;
                //var matrix = Placement.AbsTransform.ToMatrixNoScale();
                //Placement.AbsTransformInv = Matrix.Invert(in matrix);
                OnAbsTransformChanged();
            }
            else
            {
                if(Placement.IsIdentity)
                {
                    Placement.AbsTransform = Parent.Placement.AbsTransform;
                }
                else
                {
                    var transform = Placement.TransformData;
                    if (Parent.Placement.InheritScale)
                    {
                        FTransform.Multiply(out Placement.AbsTransform, in transform, in Parent.Placement.AbsTransform);
                    }
                    else
                    {
                        FTransform.MultiplyNoParentScale(out Placement.AbsTransform, in transform, in Parent.Placement.AbsTransform);
                    }
                    //Placement.AbsTransform = Placement.TransformData Transform * Parent.Placement.AbsTransform;
                }
                //var matrix = Placement.AbsTransform.ToMatrixNoScale();
                //Placement.AbsTransformInv = Matrix.Invert(in matrix);
                OnAbsTransformChanged();
                if (BoundVolume != null)
                    DBoundingBox.TransformNoScale(in BoundVolume.AABB, in Placement.AbsTransform, out RefAbsAABB);
            }
            UpdateChildrenAbsTransform();
        }
        public void UpdateChildrenAbsTransform()
        {
            foreach (var i in Children)
            {
                i.UpdateAbsTransform();
            }
        }
        public void UpdateTreeAABB()
        {
            foreach (var i in Children)
            {
                i.UpdateTreeAABB();
            }
            UpdateAABB();
        }
        public virtual void UpdateAABB()
        {
            if (NodeData == null || BoundVolume == null || Placement == null || HasStyle(ENodeStyles.DiscardAABB))
                return;
            if (BoundVolume != null && BoundVolume.mLocalAABB.IsEmpty() == false)
            {
                //BoundingBox.Transform(ref BoundVolume.mLocalAABB, ref Placement.AbsTransform, out AABB);
                RefAABB.FromSingle(in BoundVolume.mLocalAABB);
                RefAABB.Maximum = RefAABB.Maximum * Placement.Scale;
                RefAABB.Minimum = RefAABB.Minimum * Placement.Scale;
            }
            else
            {
                RefAABB.InitEmptyBox();
            }
            foreach (var i in Children)
            {
                if (i.HasStyle(ENodeStyles.DiscardAABB))
                    continue;
                if (i.Placement == null)
                    continue;
                if (i.Placement.IsIdentity == false)
                {
                    var uplc = i.Placement as TtPlacement;
                    if (uplc != null)
                    {
                        DBoundingBox tmp;
                        //var matrix = uplc.TransformData.ToMatrixNoScale();
                        var matrix = uplc.TransformData;
                        DBoundingBox.TransformNoScale(in i.RefAABB, in matrix, out tmp);
                        //BoundingBox.Transform(in i.AABB, in uplc.mTransform, out tmp);
                        RefAABB = DBoundingBox.Merge(in RefAABB, in tmp);
                    }
                    else
                    {
                        DBoundingBox tmp;
                        //var trans = i.Placement.TransformData;
                        //var trans = i.Placement.TransformData.ToMatrixNoScale();
                        var trans = i.Placement.TransformData;
                        DBoundingBox.TransformNoScale(in i.RefAABB, in trans, out tmp);
                        RefAABB = DBoundingBox.Merge(in RefAABB, in tmp);
                    }
                }
                else
                {
                    RefAABB = DBoundingBox.Merge(RefAABB, i.RefAABB);
                }
            }

            if (RefAABB.IsEmpty())
            {
                RefAABB.Minimum = DVector3.Zero;
                RefAABB.Maximum = DVector3.Zero;
            }
            if (BoundVolume != null)
                DBoundingBox.TransformNoScale(in RefAABB, in Placement.AbsTransform, out RefAbsAABB);
            if (Parent != null)
            {
                Parent.UpdateAABB();
            }

            OnAbsAABBChanged();
        }
        public bool LineCheck(in DVector3 start, in DVector3 end, ref VHitResult result)
        {
            var localStart = Placement.AbsTransform.InverseTransformPositionNoScale(in start);
            var localEnd = Placement.AbsTransform.InverseTransformPositionNoScale(in end);
            //var localStart = Vector3.TransformCoordinate(start, Placement.AbsTransformInv);
            //var localEnd = Vector3.TransformCoordinate(end, Placement.AbsTransformInv);
            //if (!Vector3.Equals(in localStart1, in localStart, 0.001f) ||
            //    !Vector3.Equals(in localEnd1, in localEnd, 0.001f))
            //{
            //    int xxx = 0;
            //}
            unsafe
            {
                //var aabb = AABB.ToSingleAABB();
                fixed (DBoundingBox* pBox = &RefAABB)
                {
                    //BoundingBox* pBox = &aabb;
                    var dir = localEnd - localStart;
                    //if (/*AABB.IsEmpty()==false && */IDllImportApi.v3dxLineIntersectBox3(&Near, &vNear, &Far, &vFar, &localStart, &dir, pBox) == 0)
                    //{
                    //    return false;
                    //}
                    if (IsTreeContain(&localStart, &dir, pBox) == false)
                    {
                        return false;
                    }
                    if (OnLineCheckTriangle(in localStart, in localEnd, ref result))
                    {
                        result.Position = Placement.AbsTransform.TransformPositionNoScale(result.m_Position);
                        result.Normal = Placement.AbsTransform.TransformVector3(in result.m_Normal);
                        //result.Position = Vector3.TransformCoordinate(result.Position, Placement.AbsTransform);
                        //result.Normal = Vector3.TransformNormal(result.Normal, Placement.AbsTransform);
                        return true;
                    }//管好自己就好，children也会拍平进入TtWorld.Octree的
                    //else
                    //{
                    //    foreach (var i in Children)
                    //    {
                    //        if (i.LineCheck(in start, in end, ref result) == true)
                    //        {
                    //            return true;
                    //        }
                    //    }
                    //    return false;
                    //}
                    return false;
                }
            }
        }
        public virtual unsafe bool IsTreeContain(DVector3* localStart, DVector3* dir, DBoundingBox* pBox)
        {
            double Near, Far;
            var vNear = new DVector3();
            var vFar = new DVector3();
            if (/*AABB.IsEmpty()==false && */IDllImportApi.v3dxLineIntersectDBox3(&Near, &vNear, &Far, &vFar, localStart, dir, pBox) == 0)
            {
                return false;
            }
            return true;
        }
        public virtual bool OnLineCheckTriangle(in DVector3 start, in DVector3 end, ref VHitResult result)
        {
            //todo: perface test            
            return false;
        }
        #endregion

        #region GamePlay
        public class TtNodeTickParameters
        {
            public GamePlay.TtWorld World;
            public Graphics.Pipeline.TtRenderPolicy Policy;
            public Graphics.Pipeline.IRenderViewport Viewport;
            public object Tag;
            public bool IsTickChildren = true;
        }
        public class TtOnTickLogicScope<T> : TtTypeScope<T, TtOnTickLogicScope<T>.OnTickLogic>
        {
            public class OnTickLogic
            {
            }
        }
        public virtual Profiler.TimeScope GetScopeTickLogic()
        {
            return null;
            //return TtOnTickLogicScope<TtNode>.Scope;
        }
        
        public delegate bool FVisitNode(TtNode node, object arg);
        public bool IterateNodes(FVisitNode fn, object arg)
        {
            if (fn(this, arg) == false)
                return false;
            var tempChildren = Children;
            foreach (var i in tempChildren)
            {
                if (i.IterateNodes(fn, arg) == false)
                    return false;
            }
            return true;
        }
        public class TtIterateParameters
        {
            public FVisitNode Callback;
            public object Arg;
            public int NodeNumLimit = 100;
        }
        public void ParallelIterateChildren(TtIterateParameters it)
        {
            var microThread = Children.Count / it.NodeNumLimit;
            TtEngine.Instance.EventPoster.ParallelFor(Children.Count, static (index, state) =>
            {
                var This = state.GetForArgument0<TtNode>();
                var it = state.GetForArgument1<TtIterateParameters>();
                it.Callback(This.Children[index], it.Arg);
            }, microThread, this, it);
            foreach (var i in Children)
            {
                if (i.Children.Count == 0)
                    continue;
                i.ParallelIterateChildren(it);
            }
        }
        private class TtNodeBFSParameters
        {
            public int TaskNum;
            public List<TtNode> InputNodes = new();
            public List<TtNode> OutputNodes = new();
            public FVisitNode Callback;
            public object Arg;
            public void Reset()
            {
                InputNodes.Clear();
                OutputNodes.Clear();
                Callback = null;
                Arg = null;
                TaskNum = 0;
            }
        }
        [ThreadStatic]
        static TtNodeBFSParameters mNodeBFSParameters = null;
        static TtNodeBFSParameters NodeBFSParameters
        {
            get
            {
                if (mNodeBFSParameters == null)
                    mNodeBFSParameters = new TtNodeBFSParameters();
                return mNodeBFSParameters;
            }
        }
        public void IterateNodesBFS(FVisitNode fn, object arg, int NumOfParallelLimit = 1)
        {
            if (fn(this, arg) == false) 
                return;

            NodeBFSParameters.Callback = fn;
            NodeBFSParameters.Arg = arg;

            NodeBFSParameters.InputNodes.Clear();
            NodeBFSParameters.OutputNodes.Clear();
            NodeBFSParameters.InputNodes.AddRange(Children);
            
            while (NodeBFSParameters.InputNodes.Count > 0)
            {
                if (NodeBFSParameters.InputNodes.Count > NumOfParallelLimit)
                {
                    var numTask = Math.Max(1, NodeBFSParameters.InputNodes.Count / NumOfParallelLimit);
                    NodeBFSParameters.TaskNum = numTask;
                    TtEngine.Instance.EventPoster.ParallelFor(NodeBFSParameters.InputNodes.Count, static (nn, state) =>
                    {
                        var parameters = state.GetForArgument0<TtNodeBFSParameters>();

                        var node = parameters.InputNodes[nn];
                        var t = parameters.Callback(node, parameters.Arg);
                        if (t == true && node.Children.Count > 0)
                        {
                            lock (parameters.OutputNodes)
                            {
                                parameters.OutputNodes.AddRange(node.Children);
                            }
                        }
                    }, numTask, NodeBFSParameters);
                }
                else
                {
                    foreach(var i in NodeBFSParameters.InputNodes)
                    {
                        if (fn(i, arg) == false)
                            continue;
                        if (i.Children.Count > 0)
                            NodeBFSParameters.OutputNodes.AddRange(i.Children);
                    }
                }
                NodeBFSParameters.InputNodes.Clear();
                CoreSDK.Swap(ref NodeBFSParameters.InputNodes, ref NodeBFSParameters.OutputNodes);
            }
            NodeBFSParameters.Reset();
        }
        public void TickLogic(TtNodeTickParameters args)
        {
            if (this.IsNoTick)
                return;
            if (OnTickLogic(args) == false)
                return;

            //foreach(var i in Children)
            if (args.IsTickChildren)
            {
                for (int i = 0; i < Children.Count; i++)
                {
                    Children[i].TickLogic(args);
                }
            }
        }
        public virtual bool OnTickLogic(TtNodeTickParameters args)
        {
            this.Behavior?.Tick(this);
            return true;
        }
        [Rtti.Meta("")]
        public void RemoveFromWorld()
        {
            Parent = null;
            mWorld?.CollideOctree.Remove(this);
            OnRemoveFromWorld();
            mWorld = null;
        }
        protected virtual void OnRemoveFromWorld()
        {

        }
        #endregion

        public async Thread.Async.TtTask<TtNode> CloneNode(TtWorld world, object extArg)
        {
            var data = Rtti.TtTypeDescManager.CreateInstance(this.NodeData.GetType()) as TtNodeData;
            //var meta = Rtti.TtClassMetaManager.Instance.GetMeta(Rtti.TtTypeDesc.TypeOf(this.NodeData.GetType()));
            //meta.CopyObjectMetaField(data, NodeData);
            TtEngine.Instance.DataCopyer.DataCopy(data, NodeData);
            var node = Rtti.TtTypeDescManager.CreateInstance(this.GetType()) as TtNode;
            if (data.BoundVolume != null)
                data.BoundVolume.HostNode = node;
            data.Placement.HostNode = node;
            await node.InitializeNode(world, data, data.BoundVolume != null ? data.BoundVolume.BVType : EBoundVolumeType.None, data.Placement?.GetType());
            node.Placement.Position = this.Placement.Position;
            node.Placement.Quat = this.Placement.Quat;
            node.Placement.Scale = this.Placement.Scale;

            foreach (var i in Children)
            {
                var cn = await i.CloneNode(world, extArg);
                cn.Parent = node;
                await cn.OnPostInitNode(node, extArg);
            }

            return node;
        }
    }
    public partial class TtSceneActorNode : TtNode
    {
        protected Guid mNodeId = Guid.NewGuid();
        public override Guid NodeId
        {
            get => mNodeId;
            set
            {
                mNodeId = value;
            }
        }

        UInt32 mSceneId = UInt32.MaxValue;
        public override UInt32 SceneId
        {
            get => mSceneId;
            internal set => mSceneId = value;
        }
        public override bool IsSceneManagedType()
        {
            return true;
        }
        protected uint CameralOffsetSerialId = 0;
        public void UpdateCameralOffset(TtWorld world)
        {
            if (world.CameralOffsetSerialId != CameralOffsetSerialId)
            {
                CameralOffsetSerialId = world.CameralOffsetSerialId;
                OnCameralOffsetChanged(world);
            }
        }
        protected virtual void OnCameralOffsetChanged(TtWorld world)
        {

        }
    }
    public partial class TtVisual : TtSceneActorNode
    {
        public override bool HashVisual => true;
        public virtual Graphics.Mesh.TtRenderMesh RenderMesh
        {
            get { return null; }
            set
            { 
            }
        }
    }
    public partial class TtGpuSceneNode : TtVisual
    {
        public TtGpuSceneNode()
        {
            
        }
        public int GpuSceneIndex = -1;
        
        public List<TtClusteredMesh> ClusteredMeshs = new List<TtClusteredMesh>();

        public void BuildClusterBuffer()
        { 
        }
    }
    [Bricks.CodeBuilder.ContextMenu("SubTree", "SubTree", TtNode.EditorKeyword)]
    public partial class TtSubTreeRootNode : TtSceneActorNode
    {
        public override unsafe bool IsTreeContain(DVector3* localStart, DVector3* dir, DBoundingBox* pBox)
        {
            return true;
        }
    }

    public partial class TtLightWeightNodeBase : TtNode
    {
        public override bool IsSceneManagedType()
        {
            return false;
        }
        protected override async Thread.Async.TtTask<bool> InitializeNode(GamePlay.TtWorld world, TtNodeData data, EBoundVolumeType bvType, Type placementType)            
        {
            return await base.InitializeNode(world, data, bvType, placementType);
        }
        public override TtNode Parent
        {
            set
            {
                if (Parent == value)
                    return;
                var newScene = value?.GetNearestParentScene();
                if (newScene != null && ParentScene != null && newScene != ParentScene)
                {
                    Profiler.Log.WriteLine<Profiler.TtGameplayGategory>(Profiler.ELogTag.Warning, "TtNode", $"{GetType().FullName}:{NodeName} cann't move to another TtScene");
                    return;
                }
                base.Parent = value;
            } 
        }
    }
}


#if TitanEngine_AutoGen_Macross
#region TitanEngine_AutoGen_Macross


namespace EngineNS.GamePlay.Scene
{
	partial class TtNode
	{
		public unsafe void macross_DisposeWithChildren (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			DisposeWithChildren();
		}
		public static async Thread.Async.TtTask<TtNode> macross_SpawnNode (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, TtNode parent, System.Type nodeType, FPostSpawnNode postAction, TtNodeData data, EBoundVolumeType bvType, Type placementType, TtWorld world, object extArg) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			var _return_value = await SpawnNode(parent, nodeType, postAction, data, bvType, placementType, world, extArg);
			return _return_value;
		}
		public unsafe TtNode macross_FindFirstChild (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, string name, System.Type type, bool bRecursive) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			var _return_value = FindFirstChild(name, type, bRecursive);
			return _return_value;
		}
		public unsafe void macross_RemoveFromWorld (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			RemoveFromWorld();
		}
	}
}
#endregion//TitanEngine_AutoGen_Macross
#endif//TitanEngine_AutoGen_Macross