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
            return System.Runtime.InteropServices.Marshal.SizeOf(this.GetType());
        }
        public UNode Host { get; set; }
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
        [Rtti.Meta]
        public ENodeStyles NodeStyles { get; set; } = 0;
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
        public bool IsCastShadow
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
        public bool IsAcceptShadow
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
            UpdateAABB();
            UpdateAbsTransform();

            SetStyle(ENodeStyles.VisibleMeshProvider);
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
            }
        }
        public UPlacementBase Placement
        {
            get { return NodeData.Placement; }
        }
        public UBoundVolume BoundVolume
        {
            get { return NodeData.BoundVolume; }
        }
        public unsafe void SaveXndNode(UScene scene, EngineNS.XndHolder xnd, EngineNS.XndNode node)
        {
            if (NodeData != null)
            {
                var dataAttr = new EngineNS.XndAttribute(xnd.NewAttribute("NodeData", 1, 0));
                node.AddAttribute(dataAttr);
                var attrProxy = new EngineNS.IO.XndAttributeWriter(dataAttr);

                var ar = new EngineNS.IO.AuxWriter<EngineNS.IO.XndAttributeWriter>(attrProxy);
                dataAttr.BeginWrite((ulong)NodeData.GetStructSize() * 2);
                ar.Write(NodeData);
                dataAttr.EndWrite();
            }

            foreach(var i in Children)
            {
                var typeStr = Rtti.UTypeDescManager.Instance.GetTypeStringFromType(i.GetType());
                var nd = new EngineNS.XndNode(xnd.NewNode(typeStr, 1, 0));
                node.AddNode(nd);
                SaveXndNode(scene, xnd, nd);
            }
        }
        public unsafe bool LoadXndNode(UScene scene, EngineNS.XndNode node)
        {
            var dataAttr = node.FindFirstAttribute("NodeData");
            if (dataAttr.NativePointer != IntPtr.Zero)
            {
                var attr = new EngineNS.XndAttribute(dataAttr);
                var ar = attr.GetReader(scene);
                IO.ISerializer data;
                ar.Read(out data, this);
                attr.ReleaseReader(ref ar);

                NodeData = data as UNodeData;
            }
            
            for(uint i = 0; i < node.GetNumOfNode(); i++)
            {
                var cld = new EngineNS.XndNode(node.GetNode(i));
                var cldTypeStr = cld.NativeSuper.Name;
                var nd = scene.NewNode(cldTypeStr, null, EBoundVolumeType.Box, typeof(GamePlay.UPlacement));
                if (LoadXndNode(scene, cld))
                {
                    Children.Add(nd);
                }
            }
            return true;
        }
        public virtual void OnNodeLoaded()
        {

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
            if (Parent == null)
            {
                Placement.AbsTransform = Placement.Transform;
                Placement.AbsTransformInv = Matrix.Invert(ref Placement.AbsTransform);
                OnAbsTransformChanged();
                return;
            }
            else
            {
                if (Parent.IsScaleChildren)
                {
                    Placement.AbsTransform = Placement.Transform * Parent.Placement.AbsTransform;
                }
                else
                {
                    var noScaleTM = Parent.Placement.AbsTransform;
                    noScaleTM.NoScale();
                    Placement.AbsTransform = Placement.Transform * noScaleTM;
                }
                Placement.AbsTransformInv = Matrix.Invert(ref Placement.AbsTransform);
                OnAbsTransformChanged();
                Parent.UpdateAbsTransform();
            }
        }
        public void UpdateAABB()
        {
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
        #endregion

        public virtual void OnGatherVisibleMeshes(UWorld.UVisParameter rp)
        {

        }
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
