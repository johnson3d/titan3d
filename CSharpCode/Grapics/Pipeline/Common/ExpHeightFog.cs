using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace EngineNS.Graphics.Pipeline.Common
{
    //https://zhuanlan.zhihu.com/p/76627240
    //https://zhuanlan.zhihu.com/p/497976692
    public partial class TtFogShading
    {
        private void OnDrawcallEHF(NxRHI.UGraphicDraw drawcall, URenderPolicy deferredPolicy, TtFogNode aaNode)
        {
            if ((uint)deferredPolicy.TypeFog != TypeFog.GetValue())
            {
                TypeFog.SetValue((uint)deferredPolicy.TypeFog);
                this.UpdatePermutation();
            }
            if (deferredPolicy.TypeFog != URenderPolicy.ETypeFog.None)
            {
                var index = drawcall.FindBinder("ColorBuffer");
                if (index.IsValidPointer)
                {
                    var attachBuffer = aaNode.GetAttachBuffer(aaNode.ColorPinIn);
                    drawcall.BindSRV(index, attachBuffer.Srv);
                }
                index = drawcall.FindBinder("Samp_ColorBuffer");
                if (index.IsValidPointer)
                    drawcall.BindSampler(index, UEngine.Instance.GfxDevice.SamplerStateManager.PointState);

                index = drawcall.FindBinder("DepthBuffer");
                if (index.IsValidPointer)
                {
                    var attachBuffer = aaNode.GetAttachBuffer(aaNode.DepthPinIn);
                    drawcall.BindSRV(index, attachBuffer.Srv);
                }
                index = drawcall.FindBinder("Samp_DepthBuffer");
                if (index.IsValidPointer)
                    drawcall.BindSampler(index, UEngine.Instance.GfxDevice.SamplerStateManager.PointState);

                index = drawcall.FindBinder("cbShadingEnv");
                if (index.IsValidPointer)
                {
                    if (aaNode.CBShadingEnv == null)
                    {
                        aaNode.CBShadingEnv = UEngine.Instance.GfxDevice.RenderContext.CreateCBV(index);
                    }
                    drawcall.BindCBuffer(index, aaNode.CBShadingEnv);
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
                    drawcall.BindSampler(index, UEngine.Instance.GfxDevice.SamplerStateManager.PointState);
            }
        }
    }
    [Bricks.CodeBuilder.ContextMenu("Fog", "Post\\Fog", Bricks.RenderPolicyEditor.UPolicyGraph.RGDEditorKeyword)]
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
                FogColor = Color3f.FromColor(Color4b.GreenYellow);
                MinFogOpacity = 0.0f;

                FogDensity = 0.004f;
                FogEnd = 100.0f;
                FogHeightFalloff = 0.022f;
                StartDistance = 0.0f;

                InscatterColor = Color3f.FromColor(Color4b.PaleVioletRed);
                InscatteringExponent = 0.01f;

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
        [Category("Exponent")]
        [EGui.Controls.PropertyGrid.Color3PickerEditor()]
        public Color3f FogColor
        {
            get => mFogStruct.FogColor;
            set
            {
                mFogStruct.FogColor = value;
            }
        }
        [Category("Exponent")]
        public float MinFogOpacity
        {
            get => mFogStruct.MinFogOpacity;
            set => mFogStruct.MinFogOpacity = value;
        }

        [Category("Exponent")]
        [EGui.Controls.PropertyGrid.PGValueRange(0, 1000.0)]
        [EGui.Controls.PropertyGrid.PGValueChangeStep(0.0001f)]
        public float FogDensity { get => mFogStruct.FogDensity; set => mFogStruct.FogDensity = value; }

        [Category("Exponent")]
        public float FogEnd { get => mFogStruct.FogEnd; set => mFogStruct.FogEnd = value; }
        
        [Category("Exponent")]
        public float StartDistance { get => mFogStruct.StartDistance; set => mFogStruct.StartDistance = value; }
        
        [Category("Exponent")]
        [EGui.Controls.PropertyGrid.PGValueRange(0, 1000.0)]
        [EGui.Controls.PropertyGrid.PGValueChangeStep(0.0001f)]
        public float FogHeightFalloff { get => mFogStruct.FogHeightFalloff; set => mFogStruct.FogHeightFalloff = value; }

        #region inscatter
        [Category("Exponent")]
        public Vector3 LightPosition { get => mFogStruct.LightPosition; set => mFogStruct.LightPosition = value; }

        [Category("Exponent")]
        [EGui.Controls.PropertyGrid.Color3PickerEditor()]
        public Color3f InscatterColor
        {
            get => mFogStruct.InscatterColor;
            set
            {
                mFogStruct.InscatterColor = value;
            }
        }
        [Category("Exponent")]
        public float InscatteringExponent { get => mFogStruct.InscatteringExponent; set => mFogStruct.InscatteringExponent = value; }
        [Category("Exponent")]
        public float InscatterStartDistance { get => mFogStruct.InscatterStartDistance; set => mFogStruct.InscatterStartDistance = value; }
        #endregion

        private void UpdateFogStruct(UCamera camera)
        {
            if (CBShadingEnv != null)
            {
                CBShadingEnv.SetValue("FogStruct", in mFogStruct);
            }
        }

        private void TickSyncEHF(URenderPolicy policy)
        {
            UpdateFogStruct(policy.DefaultCamera);
        }
    }
}
