using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.GamePlay.Camera
{
    public enum eCameraAxis
    {
        Forward,
        Up,
        Right,
    }
    public interface CameraController
    {
        EngineNS.Graphics.CGfxCamera Camera
        {
            get;
            set;
        }
        void Rotate(eCameraAxis axis, float angle, bool rotLookAt = false);
        void Move(eCameraAxis axis, float step, bool moveWithLookAt = false);
    }
}
