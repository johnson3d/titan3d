using EngineNS.Graphics.Mesh;
using EngineNS.Graphics.Pipeline.Common;
using EngineNS.Graphics.Pipeline.Shader;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Deferred
{
    public partial class UDeferredDirLightingShading : Shader.UGraphicsShadingEnv
    {
        #region Permutation
        public UPermutationItem DisableAO
        {
            get;
            set;
        }
        public UPermutationItem DisablePointLights
        {
            get;
            set;
        }
        public UPermutationItem DisableShadow
        {
            get;
            set;
        }
        public UPermutationItem DisableSunshaft
        {
            get;
            set;
        }
        public UPermutationItem DisableBloom
        {
            get;
            set;
        }
        public UPermutationItem DisableHdr
        {
            get;
            set;
        }
        #endregion
        public UDeferredDirLightingShading()
        {
            CodeName = RName.GetRName("shaders/ShadingEnv/Deferred/DeferredDirLighting.cginc", RName.ERNameType.Engine);

            this.BeginPermutaion();

            DisableAO = this.PushPermutation<Shader.EPermutation_Bool>("ENV_DISABLE_AO", (int)Shader.EPermutation_Bool.BitWidth);
            DisablePointLights = this.PushPermutation<Shader.EPermutation_Bool>("ENV_DISABLE_POINTLIGHTS", (int)Shader.EPermutation_Bool.BitWidth);
            DisableShadow = this.PushPermutation<Shader.EPermutation_Bool>("DISABLE_SHADOW_ALL", (int)Shader.EPermutation_Bool.BitWidth);
            DisableSunshaft = this.PushPermutation<Shader.EPermutation_Bool>("ENV_DISABLE_SUNSHAFT", (int)Shader.EPermutation_Bool.BitWidth);
            DisableBloom = this.PushPermutation<Shader.EPermutation_Bool>("ENV_DISABLE_BLOOM", (int)Shader.EPermutation_Bool.BitWidth);
            DisableHdr = this.PushPermutation<Shader.EPermutation_Bool>("ENV_DISABLE_HDR", (int)Shader.EPermutation_Bool.BitWidth);

            DisableAO.SetValue((int)Shader.EPermutation_Bool.FalseValue);
            DisableShadow.SetValue((int)Shader.EPermutation_Bool.FalseValue);
            DisablePointLights.SetValue((int)Shader.EPermutation_Bool.FalseValue);

            DisableSunshaft.SetValue((int)Shader.EPermutation_Bool.TrueValue);
            DisableBloom.SetValue((int)Shader.EPermutation_Bool.TrueValue);
            DisableHdr.SetValue((int)Shader.EPermutation_Bool.TrueValue);

            this.UpdatePermutation();
        }
        public override NxRHI.EVertexStreamType[] GetNeedStreams()
        {
            return new NxRHI.EVertexStreamType[] { NxRHI.EVertexStreamType.VST_Position,
                NxRHI.EVertexStreamType.VST_UV,};
        }
        public unsafe override void OnBuildDrawCall(URenderPolicy policy, NxRHI.UGraphicDraw drawcall)
        {
        }
        public unsafe override void OnDrawCall(NxRHI.ICommandList cmd, NxRHI.UGraphicDraw drawcall, URenderPolicy policy, Mesh.TtMesh.TtAtom atom)
        {
            base.OnDrawCall(cmd, drawcall, policy, atom);

            var dirLightingNode = drawcall.TagObject as UDeferredDirLightingNode;

            var index = drawcall.FindBinder("cbPerGpuScene");
            if (index.IsValidPointer)
            {
                //drawcall.mCoreObject.BindShaderCBuffer(index, Manager.GetGpuSceneNode().PerGpuSceneCBuffer.mCoreObject);
                var attachBuffer = dirLightingNode.GetAttachBuffer(dirLightingNode.GpuScenePinIn);
                drawcall.BindCBuffer(index, attachBuffer.Cbv);
            }

            #region MRT
            index = drawcall.FindBinder("GBufferRT0");
            if (index.IsValidPointer)
            {
                var attachBuffer = dirLightingNode.GetAttachBuffer(dirLightingNode.Rt0PinIn);
                drawcall.BindSRV(index, attachBuffer.Srv);
            }
            index = drawcall.FindBinder("Samp_GBufferRT0");
            if (index.IsValidPointer)
                drawcall.BindSampler(index, UEngine.Instance.GfxDevice.SamplerStateManager.LinearClampState);

            index = drawcall.FindBinder("GBufferRT1");
            if (index.IsValidPointer)
            {
                var attachBuffer = dirLightingNode.GetAttachBuffer(dirLightingNode.Rt1PinIn);
                drawcall.BindSRV(index, attachBuffer.Srv);
            }
            index = drawcall.FindBinder("Samp_GBufferRT1");
            if (index.IsValidPointer)
                drawcall.BindSampler(index, UEngine.Instance.GfxDevice.SamplerStateManager.PointState);

            index = drawcall.FindBinder("GBufferRT2");
            if (index.IsValidPointer)
            {
                var attachBuffer = dirLightingNode.GetAttachBuffer(dirLightingNode.Rt2PinIn);
                drawcall.BindSRV(index, attachBuffer.Srv);
            }
            index = drawcall.FindBinder("Samp_GBufferRT2");
            if (index.IsValidPointer)
                drawcall.BindSampler(index, UEngine.Instance.GfxDevice.SamplerStateManager.PointState);

            index = drawcall.FindBinder("GBufferRT3");
            if (index.IsValidPointer)
            {
                var attachBuffer = dirLightingNode.GetAttachBuffer(dirLightingNode.Rt3PinIn);
                drawcall.BindSRV(index, attachBuffer.Srv);
            }
            index = drawcall.FindBinder("Samp_GBufferRT3");
            if (index.IsValidPointer)
                drawcall.BindSampler(index, UEngine.Instance.GfxDevice.SamplerStateManager.PointState);

            index = drawcall.FindBinder("DepthBuffer");
            if (index.IsValidPointer)
            {
                var attachBuffer = dirLightingNode.GetAttachBuffer(dirLightingNode.DepthStencilPinIn);
                drawcall.BindSRV(index, attachBuffer.Srv);
            }
            index = drawcall.FindBinder("Samp_DepthBuffer");
            if (index.IsValidPointer)
                drawcall.BindSampler(index, UEngine.Instance.GfxDevice.SamplerStateManager.PointState);
            #endregion

            #region shadow
            index = drawcall.FindBinder("GShadowMap");
            if (index.IsValidPointer)
            {
                var attachBuffer = dirLightingNode.GetAttachBuffer(dirLightingNode.ShadowMapPinIn);
                drawcall.BindSRV(index, attachBuffer.Srv);
            }
            index = drawcall.FindBinder("Samp_GShadowMap");
            if (index.IsValidPointer)
                drawcall.BindSampler(index, UEngine.Instance.GfxDevice.SamplerStateManager.LinearClampState);
            #endregion

            #region effect
            index = drawcall.FindBinder("gEnvMap");
            if (index.IsValidPointer)
            {
                var attachBuffer = dirLightingNode.GetAttachBuffer(dirLightingNode.EnvMapPinIn);
                drawcall.BindSRV(index, attachBuffer.Srv);
            }
            index = drawcall.FindBinder("Samp_gEnvMap");
            if (index.IsValidPointer)
                drawcall.BindSampler(index, UEngine.Instance.GfxDevice.SamplerStateManager.DefaultState);

            index = drawcall.FindBinder("GVignette");
            if (index.IsValidPointer)
            {
                var attachBuffer = dirLightingNode.GetAttachBuffer(dirLightingNode.VignettePinIn);
                drawcall.BindSRV(index, attachBuffer.Srv);
            }
            index = drawcall.FindBinder("Samp_GVignette");
            if (index.IsValidPointer)
                drawcall.BindSampler(index, UEngine.Instance.GfxDevice.SamplerStateManager.DefaultState);
            #endregion

            #region MultiLights
            index = drawcall.FindBinder("TilingBuffer");
            if (index.IsValidPointer)
            {
                var attachBuffer = dirLightingNode.GetAttachBuffer(dirLightingNode.TileScreenPinIn);
                drawcall.BindSRV(index, attachBuffer.Srv);
            }

            index = drawcall.FindBinder("GpuScene_PointLights");
            if (index.IsValidPointer)
            {
                var attachBuffer = dirLightingNode.GetAttachBuffer(dirLightingNode.PointLightsPinIn);
                if (attachBuffer.Srv != null)
                    drawcall.BindSRV(index, attachBuffer.Srv);
            }
            #endregion

            index = drawcall.FindBinder("cbPerCamera");
            if (index.IsValidPointer)
            {
                drawcall.BindCBuffer(index, policy.DefaultCamera.PerCameraCBuffer);
            }
        }
        public void SetDisableShadow(bool value)
        {
            DisableShadow.SetValue(value);
            UpdatePermutation();
        }
        public void SetDisableAO(bool value)
        {
            DisableAO.SetValue(value);
            UpdatePermutation();
        }
        public void SetDisableSunShaft(bool value)
        {
            DisableSunshaft.SetValue(value);
            UpdatePermutation();
        }
        public void SetDisableBloom(bool value)
        {
            DisableBloom.SetValue(value);
            UpdatePermutation();
        }
        public void SetDisableHDR(bool value)
        {
            DisableHdr.SetValue(value);
            UpdatePermutation();
        }
        public void SetDisablePointLights(bool value)
        {
            DisablePointLights.SetValue(value);
            UpdatePermutation();
        }
    }
    [Bricks.CodeBuilder.ContextMenu("DirLighting", "Deferred\\DirLighting", Bricks.RenderPolicyEditor.UPolicyGraph.RGDEditorKeyword)]
    public partial class UDeferredDirLightingNode : Common.USceenSpaceNode
    {
        public TtRenderGraphPin Rt0PinIn = TtRenderGraphPin.CreateInput("MRT0");
        public TtRenderGraphPin Rt1PinIn = TtRenderGraphPin.CreateInput("MRT1");
        public TtRenderGraphPin Rt2PinIn = TtRenderGraphPin.CreateInput("MRT2");
        public TtRenderGraphPin Rt3PinIn = TtRenderGraphPin.CreateInputOutput("MRT3");
        public TtRenderGraphPin DepthStencilPinIn = TtRenderGraphPin.CreateInputOutput("DepthStencil");
        
        public TtRenderGraphPin ShadowMapPinIn = TtRenderGraphPin.CreateInput("ShadowMap");
        public TtRenderGraphPin EnvMapPinIn = TtRenderGraphPin.CreateInput("EnvMap");
        public TtRenderGraphPin VignettePinIn = TtRenderGraphPin.CreateInput("Vignette");
        public TtRenderGraphPin TileScreenPinIn = TtRenderGraphPin.CreateInput("TileScreen");
        public TtRenderGraphPin GpuScenePinIn = TtRenderGraphPin.CreateInput("GpuScene");
        public TtRenderGraphPin PointLightsPinIn = TtRenderGraphPin.CreateInput("PointLights");

        public UDeferredDirLightingNode()
        {
            Name = "UDeferredDirLightingNode";
        }
        public override void InitNodePins()
        {
            AddInput(Rt0PinIn, NxRHI.EBufferType.BFT_SRV);
            AddInput(Rt1PinIn, NxRHI.EBufferType.BFT_SRV);
            AddInput(Rt2PinIn, NxRHI.EBufferType.BFT_SRV);
            AddInputOutput(Rt3PinIn, NxRHI.EBufferType.BFT_SRV);
            //Rt3PinIn.IsAllowInputNull = true;
            AddInputOutput(DepthStencilPinIn, NxRHI.EBufferType.BFT_SRV);
            AddInput(ShadowMapPinIn, NxRHI.EBufferType.BFT_SRV);
            AddInput(EnvMapPinIn, NxRHI.EBufferType.BFT_SRV);
            AddInput(VignettePinIn, NxRHI.EBufferType.BFT_SRV);
            AddInput(TileScreenPinIn, NxRHI.EBufferType.BFT_SRV);
            AddInput(PointLightsPinIn, NxRHI.EBufferType.BFT_SRV);
            AddInput(GpuScenePinIn, NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_UAV);

            ResultPinOut.Attachement.Format = EPixelFormat.PXF_R16G16B16A16_FLOAT;
            base.InitNodePins();
        }
        public override void FrameBuild(Graphics.Pipeline.URenderPolicy policy)
        {
            base.FrameBuild(policy);
        }
        public UDeferredDirLightingShading mBasePassShading;
        public override UGraphicsShadingEnv GetPassShading(TtMesh.TtAtom atom = null)
        {
            return mBasePassShading;
        }
        public override async System.Threading.Tasks.Task Initialize(URenderPolicy policy, string debugName)
        {
            await base.Initialize(policy, debugName);
            mBasePassShading = UEngine.Instance.ShadingEnvManager.GetShadingEnv<UDeferredDirLightingShading>();
        }
        [ThreadStatic]
        private static Profiler.TimeScope ScopeTick = Profiler.TimeScopeManager.GetTimeScope(typeof(UDeferredDirLightingNode), nameof(TickLogic));
        public override void TickLogic(GamePlay.UWorld world, URenderPolicy policy, bool bClear)
        {
            using (new Profiler.TimeScopeHelper(ScopeTick))
            {
                GBuffers?.SetViewportCBuffer(world, policy);
                base.TickLogic(world, policy, bClear);
            }
        }
        public override void TickSync(URenderPolicy policy)
        {
            base.TickSync(policy);
        }
    }
}
