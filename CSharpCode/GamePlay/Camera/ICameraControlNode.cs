using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay.Camera
{
    public interface ICameraControlNode
    {
        public UCamera Camera { get; }
        public void AddDelta(Vector3 delta);
        public void AddYaw(float delta);
        public void AddPitch(float delta);
        public void AddRoll(float delta);
    }
}
