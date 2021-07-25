using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay
{
    public partial class UPlacementBase : IO.ISerializer
    {
        public UPlacementBase(Scene.UNode node)
        {
            HostNode = node;
        }
        public virtual void OnPreRead(object tagObject, object hostObject, bool fromXml)
        {
            HostNode = tagObject as Scene.UNode;
        }
        public virtual void OnPropertyRead(object root, System.Reflection.PropertyInfo prop, bool fromXml) { }
        public virtual Vector3 Position
        {
            get
            {
                return Vector3.Zero;
            }
            set
            {
            }
        }
        public virtual Vector3 Scale
        {
            get
            {
                return Vector3.UnitXYZ;
            }
            set
            {

            }
        }
        public virtual Quaternion Quat
        {
            get
            {
                return Quaternion.Identity;
            }
            set
            {

            }
        }
        public virtual Matrix Transform
        {
            get
            {
                return Matrix.Identity;
            }
        }
        public virtual bool IsIdentity
        {
            get { return true; }
        }
        public virtual bool HasScale
        {
            get { return false; }
        }
        public virtual void SetTransform(ref Vector3 pos, ref Vector3 scale, ref Quaternion quat)
        {
            
        }
        public virtual void SetTransform(ref Transform data)
        {

        }
        
        public Scene.UNode HostNode { get; private set; }
        public Matrix mAbsTransform = Matrix.Identity;
        public virtual Matrix AbsTransform
        {
            get
            {
                return mAbsTransform;
            }
            set
            {
                mAbsTransform = value;

                AbsTransformWithScale = value;
            }
        }
        public Matrix AbsTransformInv = new Matrix();
        public Matrix AbsTransformWithScale = new Matrix();
    }

    public partial class UScalePlacement : UPlacementBase
    {
        public UScalePlacement(Scene.UNode hostNode)
            : base(hostNode)
        {   
        }
        Vector3 mScale = Vector3.UnitXYZ;
        public override Vector3 Scale
        {
            get => mScale;
            set
            {
                mScale = value;
                UpdateData();
            }
        }
        public override bool HasScale
        {
            get { return true; }
        }
        public override bool IsIdentity
        {
            get { return false; }
        }
        void UpdateData()
        {
            mTransform = Matrix.Scaling(mScale);
            HostNode.UpdateAbsTransform();
            HostNode.UpdateAABB();
            if (HostNode.Parent != null)
                HostNode.Parent.UpdateAABB();
        }
        public Matrix mTransform = new Matrix();
        public override Matrix Transform
        {
            get
            {
                return mTransform;
            }
        }
        public override Matrix AbsTransform
        {
            get
            {
                return mAbsTransform;
            }
            set
            {
                AbsTransformWithScale = value;
                AbsTransformWithScale.M11 = value.M11 * Scale.X;
                AbsTransformWithScale.M12 = value.M12 * Scale.X;
                AbsTransformWithScale.M13 = value.M13 * Scale.X;

                AbsTransformWithScale.M21 = value.M21 * Scale.Y;
                AbsTransformWithScale.M22 = value.M22 * Scale.Y;
                AbsTransformWithScale.M23 = value.M23 * Scale.Y;

                AbsTransformWithScale.M31 = value.M31 * Scale.Z;
                AbsTransformWithScale.M32 = value.M32 * Scale.Z;
                AbsTransformWithScale.M33 = value.M33 * Scale.Z;
                
                mAbsTransform = AbsTransformWithScale;
            }
        }
    }
    public partial class UPlacement : UPlacementBase
    {
        public UPlacement(Scene.UNode hostNode)
            : base(hostNode)
        {
            mTransformData.InitData();
            mIsIdentity = true;
        }
        protected bool mIsIdentity;
        public override bool IsIdentity
        {
            get { return mIsIdentity; }
        }
        Transform mTransformData;
        [Rtti.Meta(Order = 0)]
        public Transform TransformData
        {
            get => mTransformData;
            set
            {
                mTransformData = value;
                UpdateData();
            }
        }
        public override Vector3 Position
        {
            get => mTransformData.mPosition;
            set
            {
                mTransformData.mPosition = value;
                UpdateData();
            }
        }
        public override Vector3 Scale
        {
            get => mTransformData.mScale;
            set
            {
                mTransformData.mScale = value;
                UpdateData();
            }
        }
        public override Quaternion Quat
        {
            get => mTransformData.mQuat;
            set
            {
                mTransformData.mQuat = value;
                UpdateData();
            }
        }
        public override bool HasScale
        {
            get { return mTransformData.HasScale; }
        }
        public Matrix mTransform = new Matrix();
        public override Matrix Transform
        {
            get
            {
                return mTransform;
            }
        }
        public override Matrix AbsTransform
        {
            get
            {
                return mAbsTransform;
            }
            set
            {
                mAbsTransform = value;

                AbsTransformWithScale = value;
                AbsTransformWithScale.M11 = value.M11 * Scale.X;
                AbsTransformWithScale.M12 = value.M12 * Scale.X;
                AbsTransformWithScale.M13 = value.M13 * Scale.X;

                AbsTransformWithScale.M21 = value.M21 * Scale.Y;
                AbsTransformWithScale.M22 = value.M22 * Scale.Y;
                AbsTransformWithScale.M23 = value.M23 * Scale.Y;

                AbsTransformWithScale.M31 = value.M31 * Scale.Z;
                AbsTransformWithScale.M32 = value.M32 * Scale.Z;
                AbsTransformWithScale.M33 = value.M33 * Scale.Z;

                mAbsTransform = InheritScale ? AbsTransformWithScale : value;
            }
        }
        public bool InheritScale = false;
        private void UpdateData()
        {
            mTransform = Matrix.Transformation(mTransformData.mQuat, mTransformData.mPosition);
            mIsIdentity = mTransformData.IsIdentity;
            HostNode.UpdateAbsTransform();
            HostNode.UpdateAABB();
            if (HostNode.Parent != null)
                HostNode.Parent.UpdateAABB();
        }

        public override void SetTransform(ref Vector3 pos, ref Vector3 scale, ref Quaternion quat)
        {
            mTransformData.Position = pos;
            mTransformData.Scale = scale;
            mTransformData.Quat = quat;

            UpdateData();
        }
        public override void SetTransform(ref Transform data)
        {
            mTransformData = data;
            UpdateData();
        }
    }
}
