using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay.Camera.Modifier
{
     public class GFovCameraModifier : GCameraModifier
    {
        public float TargetFov { get; set; } = MathHelper.V_PI / 2.25f;

        float mOriginFov = 0.0f;
        public override void OnStartExecution(GCamera camera)
        {
            base.OnStartExecution(camera);
            mOriginFov = camera.Fov;
            Status = ModifierStatus.Running;
        }
        public override void OnPerform(GCamera camera)
        {
            base.OnPerform(camera);
            camera.Fov = MathHelper.FloatLerp(mOriginFov, TargetFov, mElapseTime / Duration);
        }
    }
}
