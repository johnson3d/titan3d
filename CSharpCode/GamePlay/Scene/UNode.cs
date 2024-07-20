using Assimp;
using EngineNS.Bricks.GpuDriven;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace EngineNS.GamePlay.Scene
{
    public class UNodeAttribute : Attribute
    {
        public System.Type NodeDataType = typeof(UNodeData);
        public string DefaultNamePrefix = "Node";
    }
    public class UNodeData : IO.BaseSerializer
    {
        public int GetStructSize()
        {
            //return System.Runtime.InteropServices.Marshal.SizeOf(this.GetType());
            return 1024;
        }
        public bool IsDirty { get; set; } = false;
        public void CheckDirty(UNode node)
        {
            if (IsDirty)
            {
                node.UpdateAbsTransform();
                IsDirty = false;
            }
        }
        [Rtti.Meta]
        public UNode.ENodeStyles NodeStyles { get; set; } = 0;
        [Rtti.Meta(Order = 0)]
        public string Name { get; set; }
        UBoundVolume mBoundVolume;
        [Rtti.Meta(Order = 1)]
        public UBoundVolume BoundVolume
        {
            get => mBoundVolume;
            set => mBoundVolume = value;
        }
        UPlacementBase mPlacement = null;
        [Rtti.Meta(Order = 2)]
        public UPlacementBase Placement
        {
            get => mPlacement;
            set
            {
                mPlacement = value;
            }
        }

        public bool HasStyle(UNode.ENodeStyles style)
        {
            return (NodeStyles & style) == style;
        }
        public void SetStyle(UNode.ENodeStyles style)
        {
            NodeStyles |= style;
        }
        public void UnsetStyle(UNode.ENodeStyles style)
        {
            NodeStyles &= ~style;
        }
    }
    [Rtti.Meta()]
    [EGui.Controls.PropertyGrid.PGCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    public partial class UNode : IDisposable
    {
        public virtual void Dispose()
        {

        }
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
        public virtual async Thread.Async.TtTask<bool> InitializeNode(GamePlay.UWorld world, UNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            NodeData = data;

            if (NodeData != null)
            {
                if (NodeData.Placement == null && placementType != null)
                {
                    NodeData.Placement = Rtti.UTypeDescManager.CreateInstance(placementType) as UPlacementBase;
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

            if (this.HasStyle(ENodeStyles.NotRegActiveNode) == false)
            {
                world.RegActiveNode(this);
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
        public enum ENodeStyles
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
            NotRegActiveNode = (1 << 14),
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
                    SetStyle(ENodeStyles.SceneManaged);
                    if (this.SceneId == UInt32.MaxValue)
                    {
                        ParentScene?.AllocId(this);
                    }
                }
                else
                {
                    UnsetStyle(ENodeStyles.SceneManaged);
                    if (this.SceneId != UInt32.MaxValue)
                    {
                        ParentScene?.FreeId(this);
                    }
                }
            }
        }
        #endregion

        #region BaseFields
        public virtual bool IsSceneManagedType()
        {
            return false;
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
        protected UNode mParent;
        public virtual UNode Parent
        {
            get => mParent;
            set
            {
                if (mParent == value)
                    return;
                var oldScene = GetNearestParentScene();

                if (mParent != null)
                {
                    mParent.Children.Remove(this);
                }
                var oldParent = mParent;
                mParent = value;
                if (mParent != null)
                {
                    if (mParent.Children.Contains(this) == false)
                        mParent.Children.Add(this);
                }
                ParentChanged(oldParent, mParent);

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

                if (mParent !=null && Placement != null)
                {
                    Placement.Position = Placement.Position;
                }
            }
        }
        public UScene ParentScene
        {
            get
            {
                return GetNearestParentScene();
            }
        }
        public UWorld GetWorld()
        {
            var scene = ParentScene;
            if (scene != null)
                return scene.World;
            return null;
        }
        [Rtti.Meta(Flags = Rtti.MetaAttribute.EMetaFlags.Unserializable | Rtti.MetaAttribute.EMetaFlags.MacrossReadOnly)]
        public UWorld HostWorld { get => GetWorld(); }
        public List<UNode> Children { get; } = new List<UNode>();
        UNodeData mNodeData = null;
        UNodeData mTemplateNodeData = null;
        public void SetPrefabTemplate(UNodeData data)
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
        public UNodeData NodeData
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
        public T GetNodeData<T>() where T : UNodeData
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
        public virtual UPlacementBase Placement
        {
            get { return NodeData?.Placement; }
        }
        [Category("Option")]
        public UBoundVolume BoundVolume
        {
            get { return NodeData?.BoundVolume; }
        }
        public DBoundingBox AABB;//包含Child的AABB
        #endregion

        #region Virtual Interface
        private void ParentChanged(UNode prev, UNode cur)
        {
            OnParentChanged(prev, cur);
        }
        protected virtual void OnParentChanged(UNode prev, UNode cur)
        {

        }
        private void ParentSceneChanged(UScene prev, UScene cur)
        {
            OnParentSceneChanged(prev, cur);
            foreach(var i in Children)
            {
                i.ParentSceneChanged(prev, cur);
            }
        }
        protected virtual void OnParentSceneChanged(UScene prev, UScene cur)
        {

        }
        public virtual void OnGatherVisibleMeshes(UWorld.UVisParameter rp)
        {
            if (rp.VisibleNodes != null)
            {
                rp.VisibleNodes.Add(this);
            }
        }
        #endregion

        #region Link
        private void UnsafeNullParent()
        {
            ParentScene?.FreeId(this);
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
        public UNode FindFirstChild(string name, System.Type type = null, bool bRecursive = false)
        {
            foreach (var i in Children)
            {
                if (i.NodeName == name)
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

        public UNode FindFirstChild<T>(string name = null, bool bRecursive = false)
        {
            foreach (var i in Children)
            {
                if (i.GetType() == typeof(T) || i.GetType().IsSubclassOf(typeof(T)))
                {
                    if (name == null)
                        return i;
                    else
                    {
                        if (i.NodeName == name)
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
        public UScene GetNearestParentScene()
        {
            if (GetType() == typeof(UScene) || GetType().IsSubclassOf(typeof(UScene)))
                return this as UScene;
            if (mParent != null)
                return mParent.GetNearestParentScene();
            else
                return null;
        }
        public delegate bool FOnVisitNode(UNode node, object arg);
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
        public unsafe void SaveChildNode(UNode scene, EngineNS.XndHolder xnd, EngineNS.XndNode node)
        {
            foreach(var i in Children)
            {
                if (i.HasStyle(ENodeStyles.Transient))
                    continue;

                var typeStr = Rtti.UTypeDesc.TypeStr(i.GetType());
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
                        using (var dataAttr = xnd.NewAttribute(Rtti.UTypeDesc.TypeStr(i.NodeData.GetType()), 1, nodeFlags))
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
        public async System.Threading.Tasks.Task<bool> LoadChildNode(GamePlay.UWorld world, UNode scene, EngineNS.XndNode node, bool bTryFindNode)
        {
            for(uint i = 0; i < node.GetNumOfNode(); i++)
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
                var nodeTypeDesc = Rtti.UTypeDesc.TypeOf(attr.Name);
                UNodeData nodeData = Rtti.UTypeDescManager.CreateInstance(nodeTypeDesc) as UNodeData;
                var nd = Rtti.UTypeDescManager.CreateInstance(Rtti.UTypeDesc.TypeOf(cldTypeStr)) as UNode;
                if (nd == null || nodeData == null)
                {
                    Profiler.Log.WriteLine(Profiler.ELogTag.Warning, bTryFindNode ? "Prefab" : "Scene", $"SceneNode Load failed: NodeDataType={attr.Name}, NodeData={cldTypeStr}");
                    continue;
                }
                
                using (var ar = attr.GetReader(nd))
                {
                    IO.ISerializer data = nodeData;
                    try
                    {
                        //ar.Tag = nd;
                        System.Type placementType = typeof(UPlacement);
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
                                    var typeDesc = Rtti.UTypeDesc.TypeOf(nodeData.GetType());
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
                            Profiler.Log.WriteLine(Profiler.ELogTag.Warning, bTryFindNode ? "Prefab" : "Scene", $"SceneNode Load Initialize failed: NodeDataType={attr.Name}, NodeData={cldTypeStr}");
                            continue;
                        }
                        //nd.NodeData = data as UNodeData;
                        //nd.OnNodeLoaded(this);
                    }
                    catch (Exception ex)
                    {
                        Profiler.Log.WriteException(ex);
                        Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "IO", $"Scene({scene}): Node({nd.NodeData?.Name}) load failed");
                    }
                }
                if (nd.Placement != null)
                {
                    nd.Parent = this;
                    await nd.LoadChildNode(world, scene, cld, bTryFindNode);
                }
                nd.OnNodeLoaded(this);
            }
            return true;
        }
        public virtual void OnNodeLoaded(UNode parent)
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
        public virtual void UpdateAABB()
        {
            if (NodeData == null || BoundVolume == null || Placement == null)
                return;
            if (BoundVolume != null && BoundVolume.mLocalAABB.IsEmpty() == false && HasStyle(ENodeStyles.DiscardAABB) == false)
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
                    var uplc = i.Placement as UPlacement;
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
            public GamePlay.UWorld World;
            public Graphics.Pipeline.URenderPolicy Policy;
            public bool IsTickChildren = true;
        }
        public virtual void TickLogic(TtNodeTickParameters args)
        {
            if (this.IsNoTick)
                return;
            if (OnTickLogic(args.World, args.Policy) == false)
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
        public virtual bool OnTickLogic(GamePlay.UWorld world, Graphics.Pipeline.URenderPolicy policy)
        {
            return true;
        }
        #endregion

        public async System.Threading.Tasks.Task<UNode> CloneNode(UWorld world)
        {
            var data = Rtti.UTypeDescManager.CreateInstance(this.NodeData.GetType()) as UNodeData;
            var meta = Rtti.TtClassMetaManager.Instance.GetMeta(Rtti.UTypeDesc.TypeOf(this.NodeData.GetType()));
            meta.CopyObjectMetaField(data, NodeData);
            var node = Rtti.UTypeDescManager.CreateInstance(this.GetType()) as UNode;
            EBoundVolumeType bvType = EBoundVolumeType.None;
            if (NodeData.BoundVolume != null)
            {
                var t = NodeData.BoundVolume.GetType();
                if (t == typeof(UBoxBV))
                {
                    bvType = EBoundVolumeType.Box;
                }
                else if (t == typeof(USphereBV))
                {
                    bvType = EBoundVolumeType.Sphere;
                }
            }
            await node.InitializeNode(world, data, bvType, this.Placement?.GetType());
            node.Placement.Position = this.Placement.Position;
            node.Placement.Quat = this.Placement.Quat;
            node.Placement.Scale = this.Placement.Scale;
            //node.BoundVolume
            return node;
        }
    }
    public partial class USceneActorNode : UNode
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
        public virtual Guid ActorId
        {
            get { return Guid.Empty; }
        }
        protected uint CameralOffsetSerialId = 0;
        public void UpdateCameralOffset(UWorld world)
        {
            if (world.CameralOffsetSerialId != CameralOffsetSerialId)
            {
                CameralOffsetSerialId = world.CameralOffsetSerialId;
                OnCameralOffsetChanged(world);
            }
        }
        protected virtual void OnCameralOffsetChanged(UWorld world)
        {

        }
    }
    public partial class TtGpuSceneNode : USceneActorNode
    {
        public int GpuSceneIndex = -1;
        
        public List<TtClusteredMesh> ClusteredMeshs = new List<TtClusteredMesh>();

        public void BuildClusterBuffer()
        { 
        }
    }
    [Bricks.CodeBuilder.ContextMenu("SubTree", "SubTree", UNode.EditorKeyword)]
    public partial class USubTreeRootNode : USceneActorNode
    {
        public override unsafe bool IsTreeContain(DVector3* localStart, DVector3* dir, DBoundingBox* pBox)
        {
            return true;
        }
    }

    public partial class ULightWeightNodeBase : UNode
    {
        public override bool IsSceneManagedType()
        {
            return false;
        }
        public override async Thread.Async.TtTask<bool> InitializeNode(GamePlay.UWorld world, UNodeData data, EBoundVolumeType bvType, Type placementType)            
        {
            return await base.InitializeNode(world, data, bvType, placementType);
        }
        public override UNode Parent
        {
            set
            {
                if (mParent == value)
                    return;
                var newScene = value.GetNearestParentScene();
                if (ParentScene != null && newScene != ParentScene)
                {
                    Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "UNode", $"{GetType().FullName}:{NodeName} cann't move to another UScene");
                    return;
                }
                base.Parent = value;
            } 
        }
    }
}
