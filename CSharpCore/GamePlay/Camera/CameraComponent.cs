using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using EngineNS.GamePlay.Actor;
using EngineNS.GamePlay.Component;
using EngineNS.Graphics;

namespace EngineNS.GamePlay.Camera
{
    [GamePlay.Component.CustomConstructionParamsAttribute(typeof(CameraComponent.CameraComponentInitializer), "基本摄像机组件", "Camera", "CameraComponent")]
    [Editor.Editor_ComponentClassIconAttribute("icon/cameraactor_64x.txpic", RName.enRNameType.Editor)]
    public class CameraComponent : GamePlay.Component.GComponent, GamePlay.Camera.CameraController
    {
        [Rtti.MetaClassAttribute]
        public class CameraComponentInitializer : GamePlay.Component.GComponent.GComponentInitializer
        {
            [Rtti.MetaData]
            public Vector3 Location
            {
                get;
                set;
            } = Vector3.UnitX;
            [Rtti.MetaData]
            public Quaternion Rotation
            {
                get;
                set;
            }
            [Rtti.MetaData]
            public Vector3 LookAtOffset
            {
                get;
                set;
            }
            [Rtti.MetaData]
            public Vector3 Direction
            {
                get;
                set;
            } = -Vector3.UnitZ;
        }
        private CameraComponentInitializer CCInitializer
        {
            get
            {
                return this.Initializer as CameraComponentInitializer;
            }
        }
        public override async Task<bool> SetInitializer(CRenderContext rc, IEntity host, IComponentContainer hostContainer, GComponentInitializer v)
        {
            var result = await base.SetInitializer(rc, host, hostContainer, v);
            if (result == false)
                return false;

            if (CCInitializer != null)
            {
                Location = CCInitializer.Location;
                Rotation = CCInitializer.Rotation;
                Direction = CCInitializer.Direction;
            }
            return true;
        }
        protected GPlacementComponent mPlacementComponent = new GPlacementComponent();

        public Vector3 Location
        {
            get => mPlacementComponent.Location;
            set
            {
                mPlacementComponent.Location = value;
                if(CCInitializer!=null)
                    CCInitializer.Location = value;
            }
        }
        public Quaternion Rotation
        {
            get => mPlacementComponent.Rotation;
            set
            {
                mPlacementComponent.Rotation = value;
                if (CCInitializer != null)
                    CCInitializer.Rotation = value;
            }
        }
        [Browsable(false)]
        public Vector3 WorldLocation
        {
            get => Camera.CameraData.Position;
        }
        [Browsable(false)]
        public Vector3 Orientation { get; protected set; } = -Vector3.UnitZ;
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Browsable(false)]
        public Vector3 CameraDirection
        {
            get
            {
                if (Camera != null)
                    return Camera.CameraData.Direction;
                return Vector3.UnitX;
            }
        }
        protected Vector3 mYawPitchRoll = Vector3.Zero;
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Browsable(false)]
        public float Yaw
        {
            get => mYawPitchRoll.Y;
            set
            {
                mYawPitchRoll.Y = SmoothInput(mYawPitchRoll.Y, value, mInputInertia.Y);
            }
        }
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Browsable(false)]
        public float Pitch
        {
            get => mYawPitchRoll.X;
            set
            {
                mYawPitchRoll.X = SmoothInput(mYawPitchRoll.X, value, mInputInertia.X);
            }
        }
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Browsable(false)]
        public float Roll
        {
            get => mYawPitchRoll.Z;
            set
            {
                mYawPitchRoll.Z = SmoothInput(mYawPitchRoll.Z, value, mInputInertia.Z);
            }
        }
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Vector3 LookAtOffset
        {
            get
            {
                if (CCInitializer != null)
                    return CCInitializer.LookAtOffset;
                return Vector3.Zero;
            }
            set
            {
                if (CCInitializer != null)
                    CCInitializer.LookAtOffset = value;
            }
        }
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public void SetLookAtOffsetImmediately(Vector3 offset)
        {
            LookAtOffset = offset;
            DeampLookAtOffset = offset;
        }
        [Browsable(false)]
        public Vector3 DeampLookAtOffset { get; set; } = Vector3.Zero;
        public float LookAtOffsetDeamp = 0.2f;
        
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
        [Browsable(false)]
        public CGfxCamera Camera
        {
            get;
            set;
        }
        public CameraComponent()
        {
            Initializer = new GComponentInitializer();
            Camera = new CGfxCamera();
            Camera.DebugName = "CameraComponent";
            Camera.Init(CEngine.Instance.RenderContext, false);
            Camera.PerspectiveFovLH(Camera.mDefaultFoV, 100, 100, 0.1f, 1000.0f);
            var eye = new EngineNS.Vector3();
            eye.SetValue(0.0f, 0.0f, -3.6f);
            var at = new EngineNS.Vector3();
            at.SetValue(0.0f, 0.0f, 0.0f);
            var up = new EngineNS.Vector3();
            up.SetValue(0.0f, 1.0f, 0.0f);
            Camera.LookAtLH(eye, at, up);
        }

        public void SetCamera(Graphics.CGfxCamera camera)
        {
            Camera = camera;
        }
        public override void OnAdded()
        {
            if (Camera != null)
            {

            }
            base.OnAdded();
        }
        public override void OnUpdateDrawMatrix(ref Matrix drawMatrix)
        {
            if (Camera == null)
                return;
            Vector3 loc = Vector3.Zero;
            Vector3 scale = Vector3.UnitXYZ;
            Quaternion rot = Quaternion.Identity;
            var matrix = Matrix.Translate(mPlacementComponent.Location) * drawMatrix;
            matrix.Decompose(out scale, out rot, out loc);
        }
        Vector3 cameraUp = Vector3.UnitY;
        bool mCameraDataCalculated = false;
        public override void Tick(GPlacementComponent placement)
        {
            base.Tick(placement);
            if (Camera == null)
                return;
            if (DeampLookAtOffset != LookAtOffset)
            {
                DeampLookAtOffset = Vector3.Lerp(DeampLookAtOffset, LookAtOffset, LookAtOffsetDeamp);
            }
            //每帧只计算一遍，这里计算，或者手动计算
            if (!mCameraDataCalculated)
                PreCalculateCameraData(placement);

            UpdateCamera(placement);
            Orientation = Camera.CameraData.Direction;
            mYawPitchRoll = Vector3.Zero;
            mCameraDataCalculated = false;
        }
        public virtual void UpdateCamera(GPlacementComponent placement)
        {

        }

        bool mNeedInertia = true;
        Vector3 mInputInertia = Vector3.UnitXYZ * 0.6f;
        float SmoothInput(float from, float to, float factor)
        {
            if (mNeedInertia)
                return MathHelper.FloatLerp(from, to, factor);
            return from;
        }
        public Vector3 Direction
        {
            get
            {
                if (CCInitializer != null)
                    return CCInitializer.Direction;
                return Vector3.Zero;
            }
            set
            {
                if (CCInitializer != null)
                {
                    CCInitializer.Direction = value;
                    DesireDirection = value;
                }
            }
        } 
        [Browsable(false)]
        //[Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Vector3 DesireDirection { get; set; } = -Vector3.UnitZ;
        [Browsable(false)]
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Vector3 DesirePosition { get; set; } = Vector3.Zero;
        public virtual void PreCalculateCameraData(GPlacementComponent placement)
        {
            mCameraDataCalculated = true;
        }

        public void Move(eCameraAxis axis, float step, bool moveWithLookAt = false)
        {

        }

        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public void Rotate(eCameraAxis axis, float angle, bool rotLookAt = false)
        {
            if (Camera == null)
                return;
            //var pos = Camera.RenderData.Position;
            //var lookAt = GetLookAt();
        }



    }
}
