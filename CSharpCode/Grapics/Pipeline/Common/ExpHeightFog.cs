using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace EngineNS.Graphics.Pipeline.Common
{
    //https://zhuanlan.zhihu.com/p/76627240
    //https://zhuanlan.zhihu.com/p/497976692
    public partial class TtFogShading
    {
        private void OnDrawcallEHF(NxRHI.TtGraphicDraw drawcall, TtRenderPolicy deferredPolicy, TtFogNode aaNode)
        {
            if ((uint)deferredPolicy.TypeFog != TypeFog.GetValue())
            {
                TypeFog.SetValue((uint)deferredPolicy.TypeFog);
                this.UpdatePermutation().AddWaitTask();
            }
            if (deferredPolicy.TypeFog != TtRenderPolicy.ETypeFog.None)
            {
                var index = drawcall.FindBinder("ColorBuffer");
                if (index.IsValidPointer)
                {
                    var attachBuffer = aaNode.GetAttachBuffer(aaNode.ColorPinIn);
                    drawcall.BindSRV(index, attachBuffer.Srv);
                }
                index = drawcall.FindBinder("Samp_ColorBuffer");
                if (index.IsValidPointer)
                    drawcall.BindSampler(index, TtEngine.Instance.GfxDevice.SamplerStateManager.PointState);

                index = drawcall.FindBinder("DepthBuffer");
                if (index.IsValidPointer)
                {
                    var attachBuffer = aaNode.GetAttachBuffer(aaNode.DepthPinIn);
                    drawcall.BindSRV(index, attachBuffer.Srv);
                }
                index = drawcall.FindBinder("Samp_DepthBuffer");
                if (index.IsValidPointer)
                    drawcall.BindSampler(index, TtEngine.Instance.GfxDevice.SamplerStateManager.PointState);

                // Noise 输入允许悬空: 有则绑定, 无则跳过(volume_cloud.rpolicy 不接 Noise 也能跑)
                index = drawcall.FindBinder("NoiseBuffer");
                if (index.IsValidPointer)
                {
                    var noiseBuffer = aaNode.GetAttachBuffer(aaNode.NoisePinIn);
                    if (noiseBuffer != null)
                        drawcall.BindSRV(index, noiseBuffer.Srv);
                }
                index = drawcall.FindBinder("Samp_NoiseBuffer");
                if (index.IsValidPointer)
                    drawcall.BindSampler(index, TtEngine.Instance.GfxDevice.SamplerStateManager.DefaultState);

                index = drawcall.FindBinder("cbShadingEnv");
                if (index.IsValidPointer)
                {
                    if (aaNode.CBShadingEnv == null)
                    {
                        aaNode.CBShadingEnv = TtEngine.Instance.GfxDevice.RenderContext.CreateCBV(index);
                        // §1.1: CreateCBV 后立即全字段 SetValue + MarkDirty + FlushDirty, 避免首帧读到未初始化数据
                        aaNode.InitFogCBuffer();
                    }
                    drawcall.BindCBV(index, aaNode.CBShadingEnv);
                }
            }
            else
            {
                var index = drawcall.FindBinder("ColorBuffer");
                if (index.IsValidPointer)
                {
                    var attachBuffer = aaNode.GetAttachBuffer(aaNode.ColorPinIn);
                    drawcall.BindSRV(index, attachBuffer.Srv);
                }
                index = drawcall.FindBinder("Samp_ColorBuffer");
                if (index.IsValidPointer)
                    drawcall.BindSampler(index, TtEngine.Instance.GfxDevice.SamplerStateManager.PointState);
            }
        }
    }
    [Bricks.CodeBuilder.ContextMenu("Fog", "Post\\Fog", Bricks.RenderPolicyEditor.TtPolicyGraph.RGDEditorKeyword)]
    public partial class TtFogNode
    {
        private void TtFogNode_InitExpHeight()
        {
            mFogStruct.SetDefault();
        }
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 16)]
        public struct FFogStruct
        {
            public void SetDefault()
            {
                FogColor = new Color3f(0.5f, 0.6f, 0.7f);
                MinFogOpacity = 0.0f;

                FogDensity = 0.004f;
                FogEnd = 100.0f;
                FogHeightFalloff = 0.022f;
                StartDistance = 0.0f;

                InscatterColor = new Color3f(1.0f, 0.9f, 0.7f);
                InscatteringExponent = 4.0f;

                LightPosition = Vector3.Zero;
                InscatterStartDistance = 0;
            }
            public Color3f FogColor;
            public float MinFogOpacity; 

            public float FogDensity;
            public float FogEnd;
            public float FogHeightFalloff;
            public float StartDistance;

            public Color3f InscatterColor;
            public float InscatteringExponent;

            public Vector3 LightPosition;
            public float InscatterStartDistance;
        }

        FFogStruct mFogStruct;
        [System.ComponentModel.Category("Exponent")]
        [EGui.Controls.PropertyGrid.TtColor3PickerEditor()]
        public Color3f FogColor
        {
            get => mFogStruct.FogColor;
            set
            {
                mFogStruct.FogColor = value;
            }
        }
        [System.ComponentModel.Category("Exponent")]
        public float MinFogOpacity
        {
            get => mFogStruct.MinFogOpacity;
            set => mFogStruct.MinFogOpacity = value;
        }

        [System.ComponentModel.Category("Exponent")]
        [EGui.Controls.PropertyGrid.TtValueRange(0, 1000.0)]
        [EGui.Controls.PropertyGrid.TtValueChangeStep(0.0001f)]
        public float FogDensity { get => mFogStruct.FogDensity; set => mFogStruct.FogDensity = value; }

        [System.ComponentModel.Category("Exponent")]
        public float FogEnd { get => mFogStruct.FogEnd; set => mFogStruct.FogEnd = value; }
        
        [System.ComponentModel.Category("Exponent")]
        public float StartDistance { get => mFogStruct.StartDistance; set => mFogStruct.StartDistance = value; }
        
        [System.ComponentModel.Category("Exponent")]
        [EGui.Controls.PropertyGrid.TtValueRange(0, 1000.0)]
        [EGui.Controls.PropertyGrid.TtValueChangeStep(0.0001f)]
        public float FogHeightFalloff { get => mFogStruct.FogHeightFalloff; set => mFogStruct.FogHeightFalloff = value; }

        #region inscatter
        [System.ComponentModel.Category("Exponent")]
        public Vector3 LightPosition { get => mFogStruct.LightPosition; set => mFogStruct.LightPosition = value; }

        [System.ComponentModel.Category("Exponent")]
        [EGui.Controls.PropertyGrid.TtColor3PickerEditor()]
        public Color3f InscatterColor
        {
            get => mFogStruct.InscatterColor;
            set
            {
                mFogStruct.InscatterColor = value;
            }
        }
        [System.ComponentModel.Category("Exponent")]
        public float InscatteringExponent { get => mFogStruct.InscatteringExponent; set => mFogStruct.InscatteringExponent = value; }
        [System.ComponentModel.Category("Exponent")]
        public float InscatterStartDistance { get => mFogStruct.InscatterStartDistance; set => mFogStruct.InscatterStartDistance = value; }
        #endregion

        // §1.1: 首帧创建 cbuffer 后立即把所有字段写满并同步 flush, 避免 GPU 读到未初始化数据
        internal void InitFogCBuffer()
        {
            if (CBShadingEnv == null)
                return;
            CBShadingEnv.SetValue("FogStruct", in mFogStruct);
            CBShadingEnv.MarkDirty();
            CBShadingEnv.FlushDirty();
        }

        // 把世界主光方向同步到雾的方向性内散射参数(LightPosition 当作"指向太阳的方向"用)
        internal void SyncSunToFog(GamePlay.TtWorld world)
        {
            var sun = world?.GetSun();
            if (sun == null)
                return;
            // DirectionLight.Direction 是光线传播方向, 取反即为指向太阳的方向
            mFogStruct.LightPosition = -sun.DirectionLight.Direction;
        }

        private void UpdateFogStruct(TtCamera camera)
        {
            if (CBShadingEnv != null)
            {
                CBShadingEnv.SetValue("FogStruct", in mFogStruct);
            }
        }

        private void TickSyncEHF(TtRenderPolicy policy)
        {
            UpdateFogStruct(policy.DefaultCamera);
        }
    }
}
