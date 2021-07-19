using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay.Scene
{
    public class UNodeData : IO.ISerializer
    {
        public virtual void OnPreRead(object tagObject, object hostObject, bool fromXml) { }
        public virtual void OnPropertyRead(object root, System.Reflection.PropertyInfo prop, bool fromXml) { }
        public int GetStructSize()
        {
            //return System.Runtime.InteropServices.Marshal.SizeOf(this.GetType());
            return 1024;
        }
        public UNode Host { get; set; }
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
                Host?.UpdateAbsTransform();
                Host?.UpdateAABB();
            }
        }        
    }
    public partial class UNode
    {
        [Flags]
        public enum ENodeStyles
        {
            VisibleMeshProvider = (1 << 0),
            VisibleFollowParent = (1 << 1),
            HitproxyMasks = (1 << 2) | (1 << 3),//value: 0,1,2 NoProxy,RootProxy,FollowProxy
            ScaleChildren = (1 << 4),
            CastShadow = (1 << 5),
            AcceptShadow = (1 << 6),
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
        public bool IsScaleChildren
        {
            get
            {
                return HasStyle(ENodeStyles.ScaleChildren);
            }
            set
            {
                if (value)
                {
                    SetStyle(ENodeStyles.ScaleChildren);
                }
                else
                {
                    UnsetStyle(ENodeStyles.ScaleChildren);
                }
            }
        }
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
        public UNode(UNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            NodeData = data;
            SetStyle(ENodeStyles.VisibleMeshProvider);
            
            if (NodeData!=null)
            {
                if (NodeData.Placement == null && placementType != null)
                {
                    var args = new object[] { this };
                    NodeData.Placement = Rtti.UTypeDescManager.CreateInstance(placementType, args) as UPlacementBase;
                    switch (bvType)
                    {
                        case EBoundVolumeType.None:
                            {
                                NodeData.BoundVolume = null;
                            }
                            break;
                        case EBoundVolumeType.Box:
                            {
                                var boxBV = new UBoxBV(this);
                                boxBV.LocalAABB = new BoundingBox(Vector3.Zero);
                                NodeData.BoundVolume = boxBV;
                            }
                            break;
                        case EBoundVolumeType.Sphere:
                            {
                                var sphereBV = new USphereBV(this);
                                sphereBV.Center = Vector3.Zero;
                                sphereBV.Radius = 1;
                                NodeData.BoundVolume = sphereBV;
                            }
                            break;
                    }

                    Placement.SetTransform(ref Transform.Identity);
                }
                UpdateAABB();
                UpdateAbsTransform();
            }
        }
        UInt32 mId = UInt32.MaxValue;
        public UInt32 Id
        {
            get => mId;
            internal set => mId = value;
        }
        public virtual string NodeName
        {
            get
            {
                if (mNodeData != null)
                {
                    return mNodeData.Name;
                }
                return mId.ToString();
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

                if (mParent != null)
                {
                    mParent.Children.Remove(this);
                }
                mParent = value;
                var newScene = GetNearestParentScene();
                if (newScene != ParentScene)
                {//切换了UScene，重新在新的UScene中分配一个ID
                    ParentScene?.FreeId(this);
                    ParentScene = newScene;
                    ParentScene?.AllocId(this);
                }
                if (mParent != null)
                {
                    if (mParent.Children.Contains(this) == false)
                        mParent.Children.Add(this);
                }

                if (Placement != null)
                {
                    Placement.Position = Placement.Position;
                }
            }
        }
        private void UnsafeNullParent()
        {
            ParentScene?.FreeId(this);
            ParentScene = null;
            mParent = null;
        }
        public UScene ParentScene
        {
            get;
            set;
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
        public UNode FindFirstChild(string name)
        {
            foreach (var i in Children)
            {
                if (i.NodeName == name)
                    return i;
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
        public List<UNode> Children { get; } = new List<UNode>();
        UNodeData mNodeData = null;
        public UNodeData NodeData
        {
            get=>mNodeData;
            set
            {
                if (mNodeData != null)
                {
                    mNodeData.Host = null;
                }
                mNodeData = value;
                if (mNodeData != null)
                {
                    mNodeData.Host = this;
                }
                if (Placement != null)
                {
                    UpdateAABB();
                    UpdateAbsTransform();
                }
            }
        }
        public UPlacementBase Placement
        {
            get { return NodeData?.Placement; }
        }
        public UBoundVolume BoundVolume
        {
            get { return NodeData?.BoundVolume; }
        }
        public const uint NodeDescAttributeFlags = 1;
        public unsafe void SaveChildNode(UScene scene, EngineNS.XndHolder xnd, EngineNS.XndNode node)
        {
            foreach(var i in Children)
            {
                var typeStr = Rtti.UTypeDesc.TypeStr(i.GetType());
                using (var nd = xnd.NewNode(typeStr, 1, 0))
                {
                    node.AddNode(nd);

                    if (i.NodeData != null)
                    {
                        using (var dataAttr = xnd.NewAttribute(Rtti.UTypeDesc.TypeStr(i.NodeData.GetType()), 1, NodeDescAttributeFlags))
                        {
                            var attrProxy = new EngineNS.IO.XndAttributeWriter(dataAttr);

                            var ar = new EngineNS.IO.AuxWriter<EngineNS.IO.XndAttributeWriter>(attrProxy);
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
        public unsafe bool LoadChildNode(UScene scene, EngineNS.XndNode node)
        {
            for(uint i = 0; i < node.GetNumOfNode(); i++)
            {
                var cld = node.GetNode(i);
                var cldTypeStr = cld.NativeSuper.Name;
                
                var attr = cld.FindFirstAttributeByFlags(NodeDescAttributeFlags);
                if (attr.NativePointer == IntPtr.Zero)
                {
                    continue;
                }
                UNodeData nodeData = Rtti.UTypeDescManager.CreateInstance(Rtti.UTypeDesc.TypeOf(attr.Name)) as UNodeData;
                var nd = scene.NewNode(cldTypeStr, nodeData, EBoundVolumeType.None, null);
                var ar = attr.GetReader(scene);
                IO.ISerializer data = nodeData;
                try
                {
                    ar.Tag = nd;
                    ar.ReadTo(data, this);
                    nd.NodeData = data as UNodeData;

                    nd.OnNodeLoaded();
                }
                catch (Exception ex)
                {
                    Profiler.Log.WriteException(ex);
                    Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "IO", $"Scene({scene.AssetName}): Node({nd.NodeData?.Name}) load failed");
                }
                ar.Tag = null;
                attr.ReleaseReader(ref ar);

                nd.Parent = this;
                nd.LoadChildNode(scene, cld);
            }
            return true;
        }
        public virtual void OnNodeLoaded()
        {
            
        }
        public virtual void OnSceneLoaded()
        {
            this.HitproxyType = this.HitproxyType;
        }
        public delegate bool FOnVisitNode(UNode node, object arg);
        public bool DFS_VisitNodeTree(FOnVisitNode visitor, object arg)
        {
            if (visitor(this, arg))
            {
                return true;
            }
            foreach(var i in Children)
            {
                if (i.DFS_VisitNodeTree(visitor, arg))
                {
                    return true;
                }
            }
            return false;
        }

        #region Hierarchical Collide
        public BoundingBox AABB;//包含Child的AABB
        protected virtual void OnAbsTransformChanged()
        {

        }
        public void UpdateAbsTransform()
        {
            if (NodeData == null || Placement == null)
                return;
            if (Parent == null)
            {
                Placement.AbsTransform = Placement.Transform;
                Placement.AbsTransformInv = Matrix.Invert(ref Placement.mAbsTransform);
                OnAbsTransformChanged();
                return;
            }
            else
            {
                Placement.AbsTransform = Placement.Transform * Parent.Placement.AbsTransform;                
                Placement.AbsTransformInv = Matrix.Invert(ref Placement.mAbsTransform);
                OnAbsTransformChanged();
                Parent.UpdateAbsTransform();
            }
        }
        public void UpdateAABB()
        {
            if (NodeData == null || BoundVolume == null || Placement == null)
                return;
            if (BoundVolume != null && BoundVolume.mLocalAABB.IsEmpty() == false)
            {
                //BoundingBox.Transform(ref BoundVolume.mLocalAABB, ref Placement.AbsTransform, out AABB);
                AABB = BoundVolume.mLocalAABB;
                AABB.Maximum = AABB.Maximum * Placement.Scale;
                AABB.Minimum = AABB.Minimum * Placement.Scale;
            }
            else
            {
                AABB.InitEmptyBox();
            }
            foreach (var i in Children)
            {
                BoundingBox tmp;
                //BoundingBox.Transform(ref i.AABB, ref i.Placement.AbsTransform, out tmp);
                var cldTrans = i.Placement.Transform;
                cldTrans.NoScale();
                BoundingBox.Transform(ref i.AABB, ref cldTrans, out tmp);
                AABB = BoundingBox.Merge(AABB, tmp);
            }
            if (Parent != null)
            {
                Parent.UpdateAABB();
            }
        }
        public bool LineCheck(in Vector3 start, in Vector3 end, ref VHitResult result)
        {
            float Near, Far;
            Vector3 vNear = new Vector3();
            Vector3 vFar = new Vector3();
            var invMatrix = Placement.AbsTransformInv;
            //invMatrix.NoScale();
            var localStart = Vector3.TransformCoordinate(start, invMatrix);
            var localEnd = Vector3.TransformCoordinate(end, invMatrix);
            unsafe
            {
                fixed(BoundingBox* pBox = &AABB)
                {
                    var dir = localEnd - localStart;
                    if (AABB.IsEmpty()==false && IDllImportApi.v3dxLineIntersectBox3(&Near, &vNear, &Far, &vFar, &localStart, &dir, pBox) == 0)
                    {
                        return false;
                    }
                    if (OnLineCheckTriangle(in start, in end, ref result))
                    {
                        //result.Position = Vector3.TransformCoordinate(vNear, Placement.AbsTransform);
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
        public virtual bool OnLineCheckTriangle(in Vector3 start, in Vector3 end, ref VHitResult result)
        {
            //todo: perface test            
            return false;
        }
        #endregion

        public virtual void OnGatherVisibleMeshes(UWorld.UVisParameter rp)
        {

        }

        #region GamePlay
        public virtual void TickLogic()
        {
            if (OnTickLogic() == false)
                return;

            //foreach(var i in Children)
            for (int i = 0; i < Children.Count; i++)
            {
                Children[i].TickLogic();
            }
        }
        public virtual bool OnTickLogic()
        {
            return true;
        }
        #endregion
    }
    public partial class ULightWeightNodeBase : UNode
    {
        public ULightWeightNodeBase(UNodeData data, EBoundVolumeType bvType, Type placementType)
            : base(data, bvType, placementType)
        {

        }
        public override UNode Parent
        {
            set
            {
                if (mParent == value)
                    return;
                var newScene = value.GetNearestParentScene();
                if (newScene != ParentScene)
                {
                    Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "UNode", $"{GetType().FullName}:{NodeName} cann't move to another UScene");
                    return;
                }
                if (mParent != null)
                {
                    mParent.Children.Remove(this);
                }                
                mParent = value;
                if (mParent != null)
                {
                    if (mParent.Children.Contains(this) == false)
                        mParent.Children.Add(this);
                }
            } 
        }
    }
}
