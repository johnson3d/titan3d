using Assimp;
using EngineNS.Bricks.CodeBuilder;
using EngineNS.Bricks.GpuDriven;
using EngineNS.Profiler;
using NPOI.SS.Formula.Functions;
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
    [Rtti.Meta(NameAlias = new string[] { "EngineNS.GamePlay.Scene.UNodeData@EngineCore" })]
    public class TtNodeData : IO.BaseSerializer
    {
        public int GetStructSize()
        {
            //return System.Runtime.InteropServices.Marshal.SizeOf(this.GetType());
            return 1024;
        }
        public bool IsDirty { get; set; } = false;
        public void CheckDirty(TtNode node)
        {
            if (IsDirty)
            {
                node.UpdateAbsTransform();
                IsDirty = false;
            }
        }
        [Rtti.Meta]
        public TtNode.ENodeStyles NodeStyles { get; set; } = 0;
        [Rtti.Meta(Order = 0)]
        public string Name { get; set; }
        TtBoundVolume mBoundVolume;
        [Rtti.Meta(Order = 1)]
        public TtBoundVolume BoundVolume
        {
            get => mBoundVolume;
            set => mBoundVolume = value;
        }
        TtPlacementBase mPlacement = null;
        [Rtti.Meta(Order = 2)]
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
                SetStyle((TtNode.ENodeStyles)flags);
            }
        }
    }
    [Rtti.Meta()]
    [EGui.Controls.PropertyGrid.PGCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    public partial class TtNode
    {
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
            if (OctreeNode != null)
            {
                OctreeNode.Remove(this);
                OctreeOwner = null;
            }
        }
        [Rtti.Meta]
        public void DisposeWithChildren()
        {
            foreach (var i in Children)
            {
                i.UnsafeNullParent();
                i.DisposeWithChildren();
            }
            Children.Clear();
            //this.UpdateAABB();
            this.Dispose();
        }
        public const string EditorKeyword = "UNode";
        public virtual async Thread.Async.TtTask<bool> InitializeNode(GamePlay.TtWorld world, TtNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            NodeData = data;

            if (NodeData != null)
            {
                if (NodeData.Placement == null && placementType != null)
                {
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
                UpdateAABB();
                UpdateAbsTransform();
            }

            return true;
        }

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
            DBoundingBox.TransformNoScale(in AABB, in Placement.AbsTransform, out outVal);
        }
        [Flags]
        public enum ENodeStyles : uint
        {
            VisibleMeshProvider = (1 << 0),//deprecated
            VisibleFollowParent = (1 << 1),
            HitproxyMasks = (1 << 2) | (1 << 3),//value: 0,1,2 NoProxy,RootProxy,FollowProxy
            DiscardAABB = (1 << 4),
            CastShadow = (1 << 5),
            AcceptShadow = (1 << 6),
            HideBoundShape = (1 << 7),
            NoPickedDraw = (1 << 8),
            SelfInvisible = (1 << 9),
            ChildrenInvisible = (1 << 10),
            SceneManaged = (1 << 11),
            Transient = (1 << 12),
            NoTick = (1 << 13),
            NoUsed0 = (1 << 14),
            ForceGatherNode = (1 << 15),
            BuildNavMesh = (1 << 16),
            EnableHitproxyInGame = (1 << 17),
            IsCollide = (1 << 18),
            Invisible = SelfInvisible | ChildrenInvisible,
        }
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
        [Category("Option")]
        public bool IsNoTick
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
        public virtual bool IsSceneManaged
        {
            get
            {
                return HasStyle(ENodeStyles.SceneManaged);
            }
            set
            {
                if (value)
                {
                    if (ParentScene != null && this.SceneId == UInt32.MaxValue)
                    {
                        if (ParentScene.AllocId(this))
                        {
                            SetStyle(ENodeStyles.SceneManaged);
                        }
                    }
                }
                else
                {
                    if (this.SceneId != UInt32.MaxValue)
                    {
                        ParentScene?.FreeId(this);
                    }
                    UnsetStyle(ENodeStyles.SceneManaged);
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
        public virtual bool IsSceneManagedType()
        {
            return false;
        }

        [Rtti.Meta]
        public virtual Guid NodeId
        {
            get => Guid.Empty;
            set { }
        }

        public TtNode FindNode(in Guid nodeId)
        {
            if(nodeId == Guid.Empty)
                return null;
            for(int i=0; i<Children.Count; i++)
            {
                if(Children[i].NodeId == nodeId)
                    return Children[i];
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
        [Rtti.Meta]
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
                    if (IsSceneManagedType() && IsSceneManaged)
                    {
                        oldScene?.FreeId(this);
                        newScene?.AllocId(this);
                    }
                    ParentSceneChanged(oldScene, newScene);
                }

                if (value != null && Placement != null)
                {
                    Placement.Position = Placement.Position;
                }
            }
        }
        [Rtti.Meta]
        public TtScene ParentScene
        {
            get
            {
                return GetNearestParentScene();
            }
        }
        public TtWorld GetWorld()
        {
            var scene = ParentScene;
            if (scene != null)
                return scene.World;
            return null;
        }
        [Rtti.Meta(Flags = Rtti.MetaAttribute.EMetaFlags.NoSerializable | Rtti.MetaAttribute.EMetaFlags.MacrossReadOnly)]
        public TtWorld HostWorld { get => GetWorld(); }
        public List<TtNode> Children { get; } = new List<TtNode>();
        TtNodeData mNodeData = null;
        TtNodeData mTemplateNodeData = null;
        public void SetPrefabTemplate(TtNodeData data)
        {
            mTemplateNodeData = data;
            mIsPrefab = true;
            OnSetPrefabTemplate();
            OnAbsTransformChanged();
        }
        protected virtual void OnSetPrefabTemplate()
        {

        }
        internal bool mIsPrefab = false;
        [Category("Option")]
        public bool IsPrefab
        {
            get => mIsPrefab;// && (mTemplateNodeData != null);
            set
            {
                mIsPrefab = value;
            }
        }
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
        public ref DBoundingBox AABB
        {
            get
            {
                if (BoundVolume == null)
                    return ref Empty;
                else
                    return ref BoundVolume.AABB;
            }
        }
        public ref DBoundingBox AbsAABB
        {
            get
            {
                if (BoundVolume == null)
                    return ref Empty;
                else
                    return ref BoundVolume.AbsAABB;
            }
        }
        #endregion

        #region Virtual Interface
        private void ParentChanged(TtNode prev, TtNode cur)
        {
            OnParentChanged(prev, cur);
        }
        protected virtual void OnParentChanged(TtNode prev, TtNode cur)
        {

        }
        private void ParentSceneChanged(TtScene prev, TtScene cur)
        {
            OnParentSceneChanged(prev, cur);
            foreach(var i in Children)
            {
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
        #endregion

        #region Link
        protected virtual void UnsafeNullParent()
        {
            if (ParentScene != null)
            {
                ParentScene.FreeId(this);
            }
            mParent = null;
        }
        public void ClearChildren()
        {
            foreach(var i in Children)
            {
                i.UnsafeNullParent();
            }
            Children.Clear();
            UpdateAABB();
        }
        [Rtti.Meta]
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

        public TtNode FindFirstChild<T>(string name = null, bool bRecursive = false)
        {
            foreach (var i in Children)
            {
                if (i.GetType() == typeof(T) || i.GetType().IsSubclassOf(typeof(T)))
                {
                    if (name == null)
                        return i;
                    else
                    {
                        if (i.NodeName.Contains(name))
                            return i;
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
        }
        public unsafe void SaveChildNode(TtNode scene, EngineNS.XndHolder xnd, EngineNS.XndNode node)
        {
            foreach(var i in Children)
            {
                if (i.HasStyle(ENodeStyles.Transient))
                    continue;

                var typeStr = Rtti.TtTypeDesc.TypeStr(i.GetType());
                using (var nd = xnd.NewNode(typeStr, 1, 0))
                {
                    node.AddNode(nd);

                    if (i.NodeData != null)
                    {
                        uint nodeFlags = (uint)ENodeFlags.IsNodeDesc;
                        if (this.mIsPrefab)
                        {
                            nodeFlags |= (uint)ENodeFlags.IgnoreNodeDesc;
                        }
                        using (var dataAttr = xnd.NewAttribute(Rtti.TtTypeDesc.TypeStr(i.NodeData.GetType()), 1, nodeFlags))
                        {
                            var attrProxy = new EngineNS.IO.TtXndAttributeWriter(dataAttr);

                            var ar = new EngineNS.IO.AuxWriter<EngineNS.IO.TtXndAttributeWriter>(attrProxy);
                            dataAttr.BeginWrite((ulong)NodeData.GetStructSize() * 2);
                            ar.Write(i.NodeData);
                            dataAttr.EndWrite();

                            nd.AddAttribute(dataAttr);
                        }   
                    }
                    i.SaveChildNode(scene, xnd, nd);
                }   
            }
        }
        public async System.Threading.Tasks.Task<bool> LoadChildNode(GamePlay.TtWorld world, TtNode scene, EngineNS.XndNode node, bool bTryFindNode)
        {
            var isNoAABB = this.HasStyle(ENodeStyles.DiscardAABB);
            this.SetStyle(ENodeStyles.DiscardAABB);
            for (uint i = 0; i < node.GetNumOfNode(); i++)
            {
                var cld = node.GetNode(i);
                var cldTypeStr = cld.NativeSuper.Name;
                
                var attr = cld.FindFirstAttributeByFlags((uint)ENodeFlags.IsNodeDesc);
                if (attr.NativePointer == IntPtr.Zero)
                {
                    continue;
                }
                if ((attr.Flags & (uint)ENodeFlags.IgnoreNodeDesc) != 0)
                {
                    this.mIsPrefab = true;
                }
                var nodeTypeDesc = Rtti.TtTypeDesc.TypeOf(attr.Name);
                TtNodeData nodeData = Rtti.TtTypeDescManager.CreateInstance(nodeTypeDesc) as TtNodeData;
                var nd = Rtti.TtTypeDescManager.CreateInstance(Rtti.TtTypeDesc.TypeOf(cldTypeStr)) as TtNode;
                if (nd == null || nodeData == null)
                {
                    Profiler.Log.WriteLine<Profiler.TtGameplayGategory>(Profiler.ELogTag.Warning, $"SceneNode Load failed: NodeDataType={attr.Name}, NodeData={cldTypeStr}");
                    continue;
                }
                
                using (var ar = attr.GetReader(nd))
                {
                    IO.ISerializer data = nodeData;
                    try
                    {
                        //ar.Tag = nd;
                        System.Type placementType = typeof(TtPlacement);
                        if (this.mIsPrefab == false)
                        {
                            if (false == ar.ReadTo(data, this))
                            {
                                continue;
                            }

                            if (bTryFindNode)
                            {
                                var old = FindFirstChild(nodeData.Name, nd.GetType());
                                if (old != null)
                                {
                                    var typeDesc = Rtti.TtTypeDesc.TypeOf(nodeData.GetType());
                                    var meta = Rtti.TtClassMetaManager.Instance.GetMeta(typeDesc);
                                    meta.CopyObjectMetaField(old.NodeData, nodeData);
                                    nodeData = old.NodeData;
                                    nodeData.IsDirty = true;
                                }
                            }
                        }

                        var ok = await nd.InitializeNode(world, nodeData, EBoundVolumeType.Box, placementType);
                        if (ok == false)
                        {
                            Profiler.Log.WriteLine<Profiler.TtNetCategory>(Profiler.ELogTag.Warning, $"SceneNode Load Initialize failed: NodeDataType={attr.Name}, NodeData={cldTypeStr}");
                            continue;
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
                        Profiler.Log.WriteLine<Profiler.TtNetCategory>(Profiler.ELogTag.Warning, $"Scene({scene}): Node({nd.NodeData?.Name}) load failed");
                    }
                }
                if (nd.Placement != null)
                {
                    nd.Parent = this;
                    await nd.LoadChildNode(world, scene, cld, bTryFindNode);
                }
                await nd.OnNodeLoaded(this);
            }
            if (isNoAABB == false)
            {
                this.UnsetStyle(ENodeStyles.DiscardAABB);
                this.UpdateAABB();
            }
            return true;
        }
        public virtual async Thread.Async.TtTask OnNodeLoaded(TtNode parent)
        {
            
        }
        public virtual void OnSceneLoaded()
        {
            this.HitproxyType = this.HitproxyType;
            this.IsSceneManaged = this.IsSceneManaged;
        }
        #endregion

        #region Hierarchical Collide
        protected virtual void OnAbsTransformChanged()
        {

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
                    DBoundingBox.TransformNoScale(in AABB, in Placement.AbsTransform, out AbsAABB);
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
                AABB.FromSingle(in BoundVolume.mLocalAABB);
                AABB.Maximum = AABB.Maximum * Placement.Scale;
                AABB.Minimum = AABB.Minimum * Placement.Scale;
            }
            else
            {
                AABB.InitEmptyBox();
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
                        DBoundingBox.TransformNoScale(in i.AABB, in matrix, out tmp);
                        //BoundingBox.Transform(in i.AABB, in uplc.mTransform, out tmp);
                        AABB = DBoundingBox.Merge(in AABB, in tmp);
                    }
                    else
                    {
                        DBoundingBox tmp;
                        //var trans = i.Placement.TransformData;
                        //var trans = i.Placement.TransformData.ToMatrixNoScale();
                        var trans = i.Placement.TransformData;
                        DBoundingBox.TransformNoScale(in i.AABB, in trans, out tmp);
                        AABB = DBoundingBox.Merge(in AABB, in tmp);
                    }
                }
                else
                {
                    AABB = DBoundingBox.Merge(AABB, i.AABB);
                }
            }

            if (AABB.IsEmpty())
            {
                AABB.Minimum = DVector3.Zero;
                AABB.Maximum = DVector3.Zero;
            }
            if (BoundVolume != null)
                DBoundingBox.TransformNoScale(in AABB, in Placement.AbsTransform, out AbsAABB);
            if (Parent != null)
            {
                Parent.UpdateAABB();
            }
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
                fixed (DBoundingBox* pBox = &AABB)
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
                    }
                    else
                    {
                        foreach (var i in Children)
                        {
                            if (i.LineCheck(in start, in end, ref result) == true)
                            {
                                return true;
                            }
                        }
                        return false;
                    }
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
            return TtOnTickLogicScope<TtNode>.Scope;
        }
        public delegate bool FVisitNode(TtNode node, object arg);
        public bool IterateNodes(FVisitNode fn, object arg)
        {
            if (fn(this, arg) == false)
                return false;
            foreach (var i in Children)
            {
                if (i.IterateNodes(fn, arg) == false)
                    return false;
            }
            return true;
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
            return true;
        }
        [Rtti.Meta]
        public void RemoveFromWorld()
        {
            Parent = null;
            SetIsTickable(false, true);
            this.IsCollide = false;
            if(this.OctreeNode != null)
            {
                OctreeNode.Remove(this);
            }
        }
        public void SetIsTickable(bool isTickable, bool isRecursive)
        {
            IsNoTick = !isTickable;
            if(isRecursive)
            {
                foreach(var child in Children)
                {
                    child.SetIsTickable(isTickable, isRecursive);
                }
            }
        }
        #endregion

        public async Thread.Async.TtTask<TtNode> CloneNode(TtWorld world)
        {
            var data = Rtti.TtTypeDescManager.CreateInstance(this.NodeData.GetType()) as TtNodeData;
            //var meta = Rtti.TtClassMetaManager.Instance.GetMeta(Rtti.TtTypeDesc.TypeOf(this.NodeData.GetType()));
            //meta.CopyObjectMetaField(data, NodeData);
            TtEngine.Instance.DataCopyer.DataCopy(data, NodeData);
            var node = Rtti.TtTypeDescManager.CreateInstance(this.GetType()) as TtNode;
            data.BoundVolume.HostNode = node;
            data.Placement.HostNode = node;
            await node.InitializeNode(world, data, data.BoundVolume.BVType, data.Placement?.GetType());
            node.Placement.Position = this.Placement.Position;
            node.Placement.Quat = this.Placement.Quat;
            node.Placement.Scale = this.Placement.Scale;

            foreach (var i in Children)
            {
                var cn = await i.CloneNode(world);
                cn.Parent = node;
                await cn.OnNodeLoaded(node);
            }
            return node;
        }
    }
    public partial class TtSceneActorNode : TtNode
    {
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
    public partial class TtGpuSceneNode : TtSceneActorNode
    {
        public TtGpuSceneNode()
        {
            IsSceneManaged = true;
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
        public override async Thread.Async.TtTask<bool> InitializeNode(GamePlay.TtWorld world, TtNodeData data, EBoundVolumeType bvType, Type placementType)            
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
                    Profiler.Log.WriteLine<Profiler.TtGameplayGategory>(Profiler.ELogTag.Warning, "UNode", $"{GetType().FullName}:{NodeName} cann't move to another UScene");
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
		private static EngineNS.Macross.TtMacrossBreak macross_break_DisposeWithChildren_2609910045 = new EngineNS.Macross.TtMacrossBreak("EngineNS.GamePlay.Scene.TtNode->void DisposeWithChildren()");
		public unsafe void macross_DisposeWithChildren (string nodeName) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
				}
			}
			DisposeWithChildren();
			macross_break_DisposeWithChildren_2609910045.TryBreak();
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_FindFirstChild_26975848 = new EngineNS.Macross.TtMacrossBreak("EngineNS.GamePlay.Scene.TtNode->TtNode FindFirstChild(string name, System.Type type, bool bRecursive)");
		public unsafe TtNode macross_FindFirstChild (string nodeName, string name, System.Type type, bool bRecursive) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":name", name);
					stackframe.SetWatchVariable(nodeName + ":type", type);
					stackframe.SetWatchVariable(nodeName + ":bRecursive", bRecursive);
				}
			}
			var _return_value = FindFirstChild(name, type, bRecursive);
			macross_break_FindFirstChild_26975848.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_RemoveFromWorld_2609910045 = new EngineNS.Macross.TtMacrossBreak("EngineNS.GamePlay.Scene.TtNode->void RemoveFromWorld()");
		public unsafe void macross_RemoveFromWorld (string nodeName) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
				}
			}
			RemoveFromWorld();
			macross_break_RemoveFromWorld_2609910045.TryBreak();
		}
	}
}
#endregion//TitanEngine_AutoGen_Macross
#endif//TitanEngine_AutoGen_Macross