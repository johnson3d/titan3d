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

        public virtual void SetTransform(ref Vector3 pos, ref Vector3 scale, ref Quaternion quat)
        {
            
        }
        public virtual void SetTransform(ref Transform data)
        {

        }
        
        public Scene.UNode HostNode { get; private set; }
        public Matrix AbsTransform = new Matrix();
        public Matrix AbsTransformInv = new Matrix();
    }
    public partial class UPlacement : UPlacementBase
    {
        public UPlacement(Scene.UNode hostNode)
            : base(hostNode)
        {
            mTransformData.InitData();
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
        public Matrix mTransform = new Matrix();
        public override Matrix Transform
        {
            get
            {
                return mTransform;
            }
        }
        
        private void UpdateData()
        {
            mTransform = Matrix.Transformation(mTransformData.mScale, mTransformData.mQuat, mTransformData.mPosition);
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
