using System;
using System.Collections.Generic;
using System.Text;
using EngineNS.GamePlay.Component;
using System.Threading.Tasks;
using EngineNS.GamePlay.Actor;

namespace EngineNS.GamePlay.Camera
{
    [GamePlay.Component.CustomConstructionParamsAttribute(typeof(FollowCameraComponent.FollowCameraComponentInitializer), "跟随组件", "Camera", "FollowCameraComponent")]
    public class FollowCameraComponent : CameraComponent
    {
        [Rtti.MetaClassAttribute]
        public class FollowCameraComponentInitializer : CameraComponentInitializer
        {
            [Rtti.MetaData]
            public float UpLimitAngleInRad
            {
                get;
                set;
            } = 30.0f*MathHelper.Deg2Rad;
            [Rtti.MetaData]
            public float DownLimitAngleInRad
            {
                get;
                set;
            } = 40.0f * MathHelper.Deg2Rad;
            [Rtti.MetaData]
            public bool LookAtOffsetKeepRotation { get; set; } = true;
        }
        private FollowCameraComponentInitializer FCCInitializer
        {
            get
            {
                return this.Initializer as FollowCameraComponentInitializer;
            }
        }
        public override async Task<bool> SetInitializer(CRenderContext rc, IEntity host, IComponentContainer hostContainer, GComponentInitializer v)
        {
            var result = await base.SetInitializer(rc, host, hostContainer, v);
            if (result == false)
                return false;

            if (FCCInitializer != null)
            {
                UpLimitAngleInRad = FCCInitializer.UpLimitAngleInRad;
                DownLimitAngleInRad = FCCInitializer.DownLimitAngleInRad;
            }
            return true;
        }
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float UpLimitAngleInDeg
        {
            get => UpLimitAngleInRad * MathHelper.Rad2Deg;
            set => UpLimitAngleInRad = value * MathHelper.Deg2Rad;
        }
        public float UpLimitAngleInRad
        {
            get
            {
                if (FCCInitializer != null)
                    return FCCInitializer.UpLimitAngleInRad;
                return 0;
            }
            set
            {
                if(FCCInitializer!=null)
                    FCCInitializer.UpLimitAngleInRad = value;
            }
        }
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float DownLimitAngleInDeg
        {
            get => DownLimitAngleInRad * MathHelper.Rad2Deg;
            set => DownLimitAngleInRad = value * MathHelper.Deg2Rad;
        }
        public float DownLimitAngleInRad
        {
            get
            {
                if (FCCInitializer != null)
                    return FCCInitializer.DownLimitAngleInRad;
                return 0;
            }
            set
            {
                if (FCCInitializer != null)
                    FCCInitializer.DownLimitAngleInRad = value;
            }
        }
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public bool LookAtOffsetKeepRotation
        {
            get => FCCInitializer.LookAtOffsetKeepRotation;
            set => FCCInitializer.LookAtOffsetKeepRotation = value;
        }
        public Vector3 CurrentLookAtOffset = Vector3.Zero;
        public override void UpdateCamera(GPlacementComponent placement)
        {
            var lookAt = placement.Location + CurrentLookAtOffset;
           // var location = placement.Location + LookAtOffset - DesireDirection * mPlacementComponent.Location.Length();
            var up = EngineNS.Vector3.Dot(Camera.CameraData.Up, EngineNS.Vector3.UnitY) * EngineNS.Vector3.UnitY;
            up.Normalize();
            Camera.LookAtLH(DesirePosition, lookAt, up);
            
        }
        public override void PreCalculateCameraData(GPlacementComponent placement)
        {
            if (mYawPitchRoll != Vector3.Zero)
            {
                var curTarget2Camera = -Camera.CameraData.Direction;
                var yMat = EngineNS.Matrix.RotationAxis(Camera.CameraData.Up, mYawPitchRoll.Y);
                var xMat = EngineNS.Matrix.RotationAxis(Camera.CameraData.Right, mYawPitchRoll.X);
                var mat = yMat * xMat;
                var target2Camera = EngineNS.Vector3.TransformNormal(curTarget2Camera, mat);
                target2Camera.Normalize();
                DesireDirection = -target2Camera;
            }

            Vector3 up = Vector3.UnitY;
            var quat = Quaternion.GetQuaternion(up, -DesireDirection);
            var lookAt = DeampLookAtOffset;
            if (quat.Angle  < UpLimitAngleInRad)
            {
                quat = Quaternion.RotationAxis(quat.Axis, UpLimitAngleInRad);
                DesireDirection = -(quat * up);
            }
            else if(quat.Angle >MathHelper.PI - DownLimitAngleInRad)
            {
                quat = Quaternion.RotationAxis(quat.Axis, MathHelper.PI - DownLimitAngleInRad);
                DesireDirection = -(quat * up);
                
            }
            if(LookAtOffsetKeepRotation)
                lookAt = placement.Rotation * lookAt;
            CurrentLookAtOffset = lookAt;
            DesirePosition = placement.Location + CurrentLookAtOffset - DesireDirection * mPlacementComponent.Location.Length();
            base.PreCalculateCameraData(placement);
        }
    }
}
