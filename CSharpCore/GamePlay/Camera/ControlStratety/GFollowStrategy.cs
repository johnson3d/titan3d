using EngineNS.GamePlay.Component;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay.Camera.ControlStratety
{
    public class GFollowStrategy : GCameraControlStrategy
    {
        public IPlaceable HostCameraComponent { get; set; } = null;
        public override void OnPerform(GCamera camera)
        {
            camera.Position = HostCameraComponent.Placement.WorldLocation;
            camera.Rotation = HostCameraComponent.Placement.WorldRotation;
        }
    }
}
