using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using EngineNS.GamePlay.Actor;

namespace EngineNS.GamePlay.Component
{
    public interface IPlaceable
    {
        GPlacementComponent Placement { get; set; }
        void OnPlacementChanged(GPlacementComponent placement);
        //不作用于物理
        //void OnPlacementChangedUninfluencePhysics(GPlacementComponent placement);
    }
    
    [Rtti.MetaClassAttribute]
    [Editor.Editor_MacrossClassAttribute(ECSType.Common, Editor.Editor_MacrossClassAttribute.enMacrossType.Inheritable | Editor.Editor_MacrossClassAttribute.enMacrossType.Useable)]
    [GamePlay.Component.CustomConstructionParamsAttribute(typeof(GPlacementComponentInitializer), "位置组件", "Placement", "PlacementComponent", true)]
    public class GPlacementComponent : GComponent
    {
        [Rtti.MetaClassAttribute]
        public class GPlacementComponentInitializer : GComponentInitializer
        {
            public Vector3 mLocation = Vector3.Zero;
            public Vector3 mScale = Vector3.UnitXYZ;
            public Quaternion mRotation = Quaternion.Identity;
            [Rtti.MetaData]
            public Vector3 Location
            {
                get { return mLocation; }
                set { mLocation = value; }
            }
            [Rtti.MetaData]
            public Vector3 Scale
            {
                get { return mScale; }
                set { mScale = value; }
            }
            [Rtti.MetaData]
            public Quaternion Rotation
            {
                get { return mRotation; }
                set { mRotation = value; }
            }
            [Rtti.MetaData]
            public bool InheritRotation
            {
                get; set;
            } = true;
            [Rtti.MetaData]
            public bool InheritScale
            {
                get; set;
            } = true;
        }
        public GPlacementComponentInitializer mPlacementData = new GPlacementComponentInitializer();
        public override async System.Threading.Tasks.Task<bool> SetInitializer(CRenderContext rc, IEntity host, IComponentContainer hostContainer, GComponentInitializer v)
        {
            await base.SetInitializer(rc, host, hostContainer, v);
            mPlacementData = v as GPlacementComponentInitializer;
            var iPlacement = mHostContainer as IPlaceable;
            if (iPlacement != null)
            {
                mPlaceableHost = iPlacement;
                iPlacement.Placement = this;
            }
            mOrientation = Rotation * -Vector3.UnitZ;
            mWorldOrientation = WorldRotation* -Vector3.UnitZ;
            UpdateData();
            return true;
        }
        public GPlacementComponent()
        {
            OnlyForGame = false;
            this.Initializer = mPlacementData;
            mPlacementData.mLocation = Vector3.Zero;
            mPlacementData.mScale.SetValue(1, 1, 1);
            mPlacementData.mRotation = Quaternion.Identity;
            UpdateData();
        }
        public override string SpecialName
        {
            get
            {
                return typeof(GPlacementComponent).FullName;
            }
            set
            {

            }
        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public bool InheritRotation
        {
            get => mPlacementData.InheritRotation;
            set
            {
                mPlacementData.InheritRotation = value;
                UpdateData();
                OnPropertyChanged("InheritRotation");
            }
        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public bool InheritScale
        {
            get => mPlacementData.InheritScale;
            set
            {
                mPlacementData.InheritScale = value;
                UpdateData();
                OnPropertyChanged("InheritScale");
            }
        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Vector3 WorldLocation
        {
            get { return WorldMatrix.Translation; }
            set
            {
                var lT = value - ParentWorldMatrix.Translation;
                if (mPlacementData.mLocation == lT)
                    return;
                mPlacementData.mLocation = lT;
                UpdateData();
                OnPropertyChanged("Location");
            }
        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Vector3 WorldScale
        {
            get { return WorldMatrix.Scale; }
            set
            {
                var lS = value / ParentWorldMatrix.Scale;
                if (mPlacementData.mScale == lS)
                    return;
                mPlacementData.mScale = lS;
                UpdateData();
                OnPropertyChanged("Scale");
            }
        }
        Vector3 mOrientation = -Vector3.UnitZ;
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Vector3 Orientation
        {
            get { return mOrientation; }
        }
        Vector3 mWorldOrientation = -Vector3.UnitZ;
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Vector3 WorldOrientation
        {
            get { return mWorldOrientation; }
        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Quaternion WorldRotation
        {
            get
            {
                return WorldMatrix.Rotation;
            }
            set
            {
                var lR = value * ParentWorldMatrix.Rotation.Inverse();
                if (mPlacementData.mRotation == lR)
                    return;
                mPlacementData.mRotation = lR;
                mWorldOrientation = value * -Vector3.UnitZ;
                UpdateData();
                OnPropertyChanged("Rotation");
            }
        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Vector3 Location
        {
            get { return mPlacementData.mLocation; }
            set
            {
                if (mPlacementData.mLocation == value)
                    return;
                mPlacementData.mLocation = value;
                UpdateData();
                OnPropertyChanged("Location");
            }
        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Vector3 Scale
        {
            get { return mPlacementData.mScale; }
            set
            {
                if (mPlacementData.mScale == value)
                    return;
                mPlacementData.mScale = value;
                UpdateData();
                OnPropertyChanged("Scale");
            }
        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Quaternion Rotation
        {
            get { return mPlacementData.mRotation; }
            set
            {
                if (mPlacementData.mRotation == value)
                    return;
                mPlacementData.mRotation = value;
                mOrientation = value * -Vector3.UnitZ;
                UpdateData();
                OnPropertyChanged("Rotation");
            }
        }

        public override IComponentContainer HostContainer
        {
            get => base.HostContainer;
            set
            {
                base.HostContainer = value;
                if (value is IPlaceable)
                {
                    PlaceableHost = value as IPlaceable;
                }
            }
        }
        IPlaceable mPlaceableHost = null;
        public IPlaceable PlaceableHost
        {
            get
            {
                return mPlaceableHost;
            }
            set
            {
                mPlaceableHost = value;
            }
        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public void FaceToDiretion(Vector3 dir)
        {
            dir.Normalize();
            Vector3 pos, scale;
            Quaternion rotation;
            this.ParentWorldMatrix.Decompose(out scale, out rotation, out pos);
            var targetRotation = Quaternion.GetQuaternionUp(Vector3.UnitZ, dir);
            Rotation = targetRotation * rotation.Inverse();
        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public void FaceToCamera(Graphics.CGfxCamera camera)
        {
            Vector3 pos, scale;
            Quaternion rotation;
            this.ParentWorldMatrix.Decompose(out scale, out rotation, out pos);
            var targetRotation = Quaternion.GetQuaternionUp(Vector3.UnitZ, camera.CameraData.Direction);
            Rotation = targetRotation * rotation.Inverse();
        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public void FaceToCameraY(Graphics.CGfxCamera camera)
        {
            var faceTo = camera.CameraData.Direction;
            faceTo.Y = 0;
            faceTo.Normalize();
            Vector3 pos, scale;
            Quaternion rotation;
            this.ParentWorldMatrix.Decompose(out scale, out rotation, out pos);
            var targetRotation = Quaternion.GetQuaternionUp(Vector3.UnitZ, faceTo);
            Rotation = targetRotation * rotation.Inverse();
        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public void FaceToCameraLookAtY(Graphics.CGfxCamera camera)
        {
            var faceTo = this.Location - camera.CameraData.Position;
            faceTo.Y = 0;
            faceTo.Normalize();
            Vector3 pos, scale;
            Quaternion rotation;
            this.ParentWorldMatrix.Decompose(out scale, out rotation, out pos);
            var targetRotation = Quaternion.GetQuaternion(Vector3.UnitZ, faceTo);
            Rotation = targetRotation * rotation.Inverse();
        }
        public void SetMatrix(ref Matrix matrix)
        {
            matrix.Decompose(out mPlacementData.mScale, out mPlacementData.mRotation, out mPlacementData.mLocation);
            //UpdateData();
            Transform = matrix;
            //var iPlacement = mPlaceableHost as IPlaceable;
            mPlaceableHost?.OnPlacementChanged(this);
            OnPropertyChanged("Matrix");
            UpdateActorAABB();
        }
        protected virtual void UpdateData()
        {
            Matrix.Transformation(mPlacementData.mScale, mPlacementData.mRotation, mPlacementData.mLocation, out Transform);
            // var iPlacement = mHostContainer as IPlaceable;
            mPlaceableHost?.OnPlacementChanged(this);
            UpdateActorAABB();
        }
        public Matrix Transform = Matrix.Identity;
        public virtual Matrix ParentWorldMatrix
        {
            get
            {
                if (mPlaceableHost == null)
                    return Matrix.Identity;

                Matrix tempMatrix;
                if (mPlaceableHost == Host)
                {
                    if (Host.Parent == null || Host.Parent.Placement == null)
                        return Matrix.Identity;
                    else
                    {
                        tempMatrix = Host.Parent.Placement.WorldMatrix;
                    }
                }
                else
                {
                    IPlaceable host = null;
                    var comp = mPlaceableHost as GComponent;
                    if (comp == null)
                    {
                        return Matrix.Identity;
                    }

                    host = comp.HostContainer as IPlaceable;

                    if (host == null || host.Placement == null)
                        return Matrix.Identity;
                    tempMatrix = host.Placement.WorldMatrix;

                }
                if (!InheritRotation)
                {
                    tempMatrix.NoRotation();
                }
                if (!InheritScale)
                {
                    tempMatrix.NoScale();
                }
                return tempMatrix;
            }
        }
        public virtual Matrix WorldMatrix
        {
            get
            {
                if (mPlaceableHost == null)
                    return Transform;
                return Transform * ParentWorldMatrix;
            }
        }

        public Matrix mDrawTransform = Matrix.Identity;
        public Vector3 mDrawPosition;
        public Vector3 mDrawScale;
        public Quaternion mDrawRotation;
        public Matrix DrawTransform
        {
            get { return mDrawTransform; }
            set
            {
                mDrawTransform = value;
                mDrawTransform.Decompose(out mDrawScale, out mDrawRotation, out mDrawPosition);
            }
        }

        //dir normalized
        public void TryMove(ref Vector3 delta, float minDist, float elapsedTime/*秒*/)
        {
            for (int i = 0; i < mHostContainer.Components.Count; ++i)
            {
                if (mHostContainer.Components[i].OnTryMove(this, ref delta, minDist, elapsedTime))
                    return;
            }
            Location = mPlacementData.mLocation + delta;
        }

        public BoundingBox ActorAABB;
        public void UpdateActorAABB()
        {
            if (Host != null)
            {
                Host.GetAABB(ref ActorAABB);
                for(int i = 0; i < Host.Children.Count; i++)
                {
                    Host.Children[i].Placement.UpdateActorAABB();
                }
            }
        }
    }
}
