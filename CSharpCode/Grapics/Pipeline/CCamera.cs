using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline
{
    public class CCamera : AuxPtrType<ICamera>
    {
        public CCamera()
        {
            mCoreObject = ICamera.CreateInstance();
        }
        RHI.CConstantBuffer mPerCameraCBuffer;
        public void SureCBuffer(Shader.UEffect effect, string debugName)
        {
            if (effect.CBPerCameraIndex != 0xFFFFFFFF)
            {
                var gpuProgram = effect.ShaderProgram;
                if (PerCameraCBuffer == null)
                {
                    PerCameraCBuffer = UEngine.Instance.GfxDevice.RenderContext.CreateConstantBuffer(gpuProgram, effect.CBPerCameraIndex);
                    PerCameraCBuffer.mCoreObject.NativeSuper.SetDebugName($"{debugName}: Camera");
                }
            }
        }
        public RHI.CConstantBuffer PerCameraCBuffer
        {
            get => mPerCameraCBuffer;
            set
            {
                mPerCameraCBuffer = value;
                if (value != null)
                {
                    unsafe
                    {
                        mCoreObject.BindConstBuffer(UEngine.Instance.GfxDevice.RenderContext.mCoreObject, value.mCoreObject);
                    }
                }
            }
        }
    }

    public enum ECameraAxis
    {
        Forward,
        Up,
        Right,
    }
    public interface ICameraController
    {
        CCamera Camera
        {
            get;
            set;
        }
        void Rotate(ECameraAxis axis, float angle, bool rotLookAt = false);
        void Move(ECameraAxis axis, float step, bool moveWithLookAt = false);
    }
}
