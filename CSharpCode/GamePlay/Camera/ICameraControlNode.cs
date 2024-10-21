using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay.Camera
{
    public interface ICameraControlNode
    {
        public TtGamePlayCamera Camera { get; }
        public void AddDelta(in FRotator delta);
        public void AddYaw(float delta);
        public void AddPitch(float delta);
        public void AddRoll(float delta);
    }
}
