using EngineNS.Bricks.AdvanceShadow;
using EngineNS.Graphics.Mesh;
using EngineNS.Graphics.Pipeline.Common;
using EngineNS.Graphics.Pipeline.Shader;
using EngineNS.Graphics.Pipeline.Shadow;
using EngineNS.NxRHI;
using Microsoft.CodeAnalysis.Host;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Deferred
{
    /// <summary>
    /// Contact shadow 的三种工作模式 (互斥):
    /// None      - 关闭 contact shadow
    /// InputNode - 使用外部 TtContactShadowNode (独立 compute pass, 连 ContactShadowPinIn)
    /// Inline    - 在 DeferredDirLighting.cginc 内联 ray march (零额外带宽)
    /// </summary>
    [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "EContactShadowMode")]
    public enum EContactShadowMode : uint
    {
        None = 0,
        InputNode,
        Inline,

        TypeCount,
    }
    public partial class TtDeferredDirLightingShading : Shader.TtGraphicsShadingEnv
    {
        #region Permutation
        public TtPermutationItem EnableLocalLights
        {
            get;
            set;
        }
        public TtPermutationItem EnableSeparatedSpecular
        {
            get;
            set;
        }
        public TtPermutationItem ContactShadowMode
        {
            get;
            set;
        }
        // ENV_INLINE_CS_USE_HZB: inline contact shadow uses HZB-accelerated ray march when HzbPinIn is connected.
        public TtPermutationItem InlineContactShadowHzb
        {
            get;
            set;
        }
        public TtPermutationItem EnableSSAO
        {
            get;
            set;
        }
        public TtPermutationItem ShadowModePermutation
        {
            get;
            set;
        }
        [Category("Option")]
        public EShadowMode ShadowMode
        {
            get
            {
                return (EShadowMode)ShadowModePermutation.Value.GetValue(ShadowModePermutation);
            }
            set
            {
                ShadowModePermutation.SetValue((uint)value);
                this.UpdatePermutation().AddWaitTask();
            }
        }
        #endregion
        public TtDeferredDirLightingShading()
        {
            CodeName = RName.GetRName("shaders/ShadingEnv/Deferred/DeferredDirLighting.cginc", RName.ERNameType.Engine);

            this.BeginPermutaion();

            EnableLocalLights = this.PushPermutation<Shader.EPermutation_Bool>("ENV_LOCAL_LIGHTS", (int)Shader.EPermutation_Bool.BitWidth);
            EnableSeparatedSpecular = this.PushPermutation<Shader.EPermutation_Bool>("ENV_ENABLE_SEPARATED_SPECULAR", (int)Shader.EPermutation_Bool.BitWidth);

            EnableLocalLights.SetValue((int)Shader.EPermutation_Bool.TrueValue);
            EnableSeparatedSpecular.SetValue((int)Shader.EPermutation_Bool.FalseValue);

            ShadowModePermutation = this.PushPermutation<EShadowMode>("ENV_EShadowMode", GetBitWidth((int)EShadowMode.Num));
            ShadowModePermutation.SetValue((int)EShadowMode.Csm);

            // ETypeAA-style single enum permutation: shader compares ENV_ContactShadowMode == EContactShadowMode_xxx.
            ContactShadowMode = this.PushPermutation<EContactShadowMode>("ENV_ContactShadowMode", (int)EContactShadowMode.TypeCount);
            ContactShadowMode.SetValue((uint)EContactShadowMode.None);

            // Inline contact shadow HZB acceleration switch (driven by HzbPinIn connection in FrameBuild).
            InlineContactShadowHzb = this.PushPermutation<Shader.EPermutation_Bool>("ENV_INLINE_CS_USE_HZB", (int)Shader.EPermutation_Bool.BitWidth);
            InlineContactShadowHzb.SetValue((int)Shader.EPermutation_Bool.FalseValue);

            EnableSSAO = this.PushPermutation<Shader.EPermutation_Bool>("ENV_ENABLE_SSAO", (int)Shader.EPermutation_Bool.BitWidth);
            EnableSSAO.SetValue((int)Shader.EPermutation_Bool.FalseValue);

            this.UpdatePermutation().AddWaitTask();
        }
        public override NxRHI.EVertexStreamType[] GetNeedStreams()
        {
            return new NxRHI.EVertexStreamType[] { 
                NxRHI.EVertexStreamType.VST_Position,
                NxRHI.EVertexStreamType.VST_UV,
            };
        }
        public override EPixelShaderInput[] GetPSNeedInputs()
        {
            return new EPixelShaderInput[] {
                EPixelShaderInput.PST_Position,
                EPixelShaderInput.PST_UV,
                EPixelShaderInput.PST_ExtraUV,
            };
        }
        public unsafe override void OnBuildDrawCall(TtRenderPolicy policy, NxRHI.TtGraphicDraw drawcall)
        {
        }
        public unsafe override void OnDrawCall(NxRHI.ICommandList cmd, NxRHI.TtGraphicDraw drawcall, TtRenderPolicy policy, Mesh.TtRenderMesh.TtAtom atom)
        {
            base.OnDrawCall(cmd, drawcall, policy, atom);

            var dirLightingNode = drawcall.TagObject as TtDeferredDirLightingNode;

            var index = drawcall.FindBinder("cbPerGpuScene");
            if (index.IsValidPointer)
            {
                //drawcall.mCoreObject.BindShaderCBuffer(index, Manager.GetGpuSceneNode().PerGpuSceneCBuffer.mCoreObject);
                var attachBuffer = dirLightingNode.GetAttachBuffer(dirLightingNode.GpuScenePinIn);
                drawcall.BindCBV(index, attachBuffer.Cbv);
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
                drawcall.BindSampler(index, TtEngine.Instance.GfxDevice.SamplerStateManager.LinearClampState);

            index = drawcall.FindBinder("GBufferRT1");
            if (index.IsValidPointer)
            {
                var attachBuffer = dirLightingNode.GetAttachBuffer(dirLightingNode.Rt1PinIn);
                drawcall.BindSRV(index, attachBuffer.Srv);
            }
            index = drawcall.FindBinder("Samp_GBufferRT1");
            if (index.IsValidPointer)
                drawcall.BindSampler(index, TtEngine.Instance.GfxDevice.SamplerStateManager.PointState);

            index = drawcall.FindBinder("GBufferRT2");
            if (index.IsValidPointer)
            {
                var attachBuffer = dirLightingNode.GetAttachBuffer(dirLightingNode.Rt2PinIn);
                drawcall.BindSRV(index, attachBuffer.Srv);
            }
            index = drawcall.FindBinder("Samp_GBufferRT2");
            if (index.IsValidPointer)
                drawcall.BindSampler(index, TtEngine.Instance.GfxDevice.SamplerStateManager.PointState);

            index = drawcall.FindBinder("GBufferRT3");
            if (index.IsValidPointer)
            {
                var attachBuffer = dirLightingNode.GetAttachBuffer(dirLightingNode.Rt3PinIn);
                drawcall.BindSRV(index, attachBuffer.Srv);
            }
            index = drawcall.FindBinder("Samp_GBufferRT3");
            if (index.IsValidPointer)
                drawcall.BindSampler(index, TtEngine.Instance.GfxDevice.SamplerStateManager.PointState);

            index = drawcall.FindBinder("DepthBuffer");
            if (index.IsValidPointer)
            {
                var attachBuffer = dirLightingNode.GetAttachBuffer(dirLightingNode.DepthStencilPinIn);
                drawcall.BindSRV(index, attachBuffer.Srv);
            }
            index = drawcall.FindBinder("Samp_DepthBuffer");
            if (index.IsValidPointer)
                drawcall.BindSampler(index, TtEngine.Instance.GfxDevice.SamplerStateManager.PointState);

            index = drawcall.FindBinder("StencilBuffer");
            if (index.IsValidPointer)
            {
                var attachBuffer = dirLightingNode.GetAttachBuffer(dirLightingNode.DepthStencilPinIn);
                if (attachBuffer?.StencilSrv != null)
                    drawcall.BindSRV(index, attachBuffer.StencilSrv);
            }
            #endregion

            #region shadow
            if (dirLightingNode.mBasePassShading.ShadowMode == EShadowMode.Advance)
            {
                var advShadowNode = dirLightingNode.AdvanceShadowMapNode;
                if (advShadowNode != null && advShadowNode.Enable)
                {
                    dirLightingNode.AdvanceShadowMapNode.OnDirLightingDrawCall(cmd, drawcall, policy, atom);
                }
            }
            else if (dirLightingNode.mBasePassShading.ShadowMode == EShadowMode.Csm)
            {
                index = drawcall.FindBinder("GShadowMap");
                if (index.IsValidPointer)
                {
                    var attachBuffer = dirLightingNode.GetAttachBuffer(dirLightingNode.ShadowMapPinIn);
                    drawcall.BindSRV(index, attachBuffer.Srv);
                }
                index = drawcall.FindBinder("Samp_GShadowMap");
                if (index.IsValidPointer)
                    drawcall.BindSampler(index, TtEngine.Instance.GfxDevice.SamplerStateManager.LinearClampState);
            }
            #endregion

            #region contact shadow
            index = drawcall.FindBinder("GContactShadow");
            if (index.IsValidPointer)
            {
                var attachBuffer = dirLightingNode.GetAttachBuffer(dirLightingNode.ContactShadowPinIn);
                if (attachBuffer?.Srv != null)
                    drawcall.BindSRV(index, attachBuffer.Srv);
            }
            index = drawcall.FindBinder("Samp_GContactShadow");
            if (index.IsValidPointer)
                drawcall.BindSampler(index, TtEngine.Instance.GfxDevice.SamplerStateManager.PointState);
            #endregion

            #region inline contact shadow
            index = drawcall.FindBinder("cbInlineContactShadow");
            if (index.IsValidPointer)
                drawcall.BindCBV(index, dirLightingNode.GetOrCreateInlineContactShadowCBuffer(index));

            // HZB for inline contact shadow ray march (ENV_INLINE_CS_USE_HZB == 1)
            index = drawcall.FindBinder("GHzbTexture");
            if (index.IsValidPointer)
            {
                var attachBuffer = dirLightingNode.GetAttachBuffer(dirLightingNode.HzbPinIn);
                if (attachBuffer?.Srv != null)
                    drawcall.BindSRV(index, attachBuffer.Srv);
            }
            index = drawcall.FindBinder("Samp_GHzbTexture");
            if (index.IsValidPointer)
                drawcall.BindSampler(index, TtEngine.Instance.GfxDevice.SamplerStateManager.PointState);
            #endregion

            #region SSAO
            index = drawcall.FindBinder("GSSAOTexture");
            if (index.IsValidPointer)
            {
                var attachBuffer = dirLightingNode.GetAttachBuffer(dirLightingNode.SSAOPinIn);
                if (attachBuffer?.Srv != null)
                    drawcall.BindSRV(index, attachBuffer.Srv);
            }
            index = drawcall.FindBinder("Samp_GSSAOTexture");
            if (index.IsValidPointer)
                drawcall.BindSampler(index, TtEngine.Instance.GfxDevice.SamplerStateManager.LinearClampState);
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
                drawcall.BindSampler(index, TtEngine.Instance.GfxDevice.SamplerStateManager.LinearClampState);

            index = drawcall.FindBinder("gPreIntegratedGF");
            if (index.IsValidPointer)
            {
                var srv = TtEngine.Instance.GetPreIntegratedDFSrv(cmd);
                if (srv != null)
                    drawcall.BindSRV(index, srv);
            }
            index = drawcall.FindBinder("Samp_gPreIntegratedGF");
            if (index.IsValidPointer)
            {
                drawcall.BindSampler(index, TtEngine.Instance.GfxDevice.SamplerStateManager.LinearClampState);
            }

            index = drawcall.FindBinder("GVignette");
            if (index.IsValidPointer)
            {
                var attachBuffer = dirLightingNode.GetAttachBuffer(dirLightingNode.VignettePinIn);
                drawcall.BindSRV(index, attachBuffer.Srv);
            }
            index = drawcall.FindBinder("Samp_GVignette");
            if (index.IsValidPointer)
                drawcall.BindSampler(index, TtEngine.Instance.GfxDevice.SamplerStateManager.LinearClampState);
            #endregion

            #region MultiLights
            index = drawcall.FindBinder("GpuScene_PointLights");
            if (index.IsValidPointer)
            {
                var attachBuffer = dirLightingNode.GetAttachBuffer(dirLightingNode.PointLightsPinIn);
                if (attachBuffer?.Srv != null)
                    drawcall.BindSRV(index, attachBuffer.Srv);
            }

            index = drawcall.FindBinder("GpuScene_SpotLights");
            if (index.IsValidPointer)
            {
                var attachBuffer = dirLightingNode.GetAttachBuffer(dirLightingNode.SpotLightsPinIn);
                if (attachBuffer?.Srv != null)
                    drawcall.BindSRV(index, attachBuffer.Srv);
            }

            index = drawcall.FindBinder("PointGridHeaders");
            if (index.IsValidPointer)
            {
                var attachBuffer = dirLightingNode.GetAttachBuffer(dirLightingNode.PointGridHeadersPinIn);
                if (attachBuffer?.Srv != null)
                    drawcall.BindSRV(index, attachBuffer.Srv);
            }

            index = drawcall.FindBinder("PointGridDataIndices");
            if (index.IsValidPointer)
            {
                var attachBuffer = dirLightingNode.GetAttachBuffer(dirLightingNode.PointGridDataIndicesPinIn);
                if (attachBuffer?.Srv != null)
                    drawcall.BindSRV(index, attachBuffer.Srv);
            }

            index = drawcall.FindBinder("SpotGridHeaders");
            if (index.IsValidPointer)
            {
                var attachBuffer = dirLightingNode.GetAttachBuffer(dirLightingNode.SpotGridHeadersPinIn);
                if (attachBuffer?.Srv != null)
                    drawcall.BindSRV(index, attachBuffer.Srv);
            }

            index = drawcall.FindBinder("SpotGridDataIndices");
            if (index.IsValidPointer)
            {
                var attachBuffer = dirLightingNode.GetAttachBuffer(dirLightingNode.SpotGridDataIndicesPinIn);
                if (attachBuffer?.Srv != null)
                    drawcall.BindSRV(index, attachBuffer.Srv);
            }

            index = drawcall.FindBinder("cbFrustumGrid");
            if (index.IsValidPointer)
            {
                var gridNode = policy.FindFirstNode<Common.TtFrustumGrid3DNode>();
                if (gridNode?.PerFrustumGridCbv != null)
                    drawcall.BindCBV(index, gridNode.PerFrustumGridCbv);
            }
            #endregion

            #region SubsurfaceProfiles
            index = drawcall.FindBinder("SubsurfaceProfiles");
            if (index.IsValidPointer)
            {
                var profileMgr = TtEngine.Instance.GfxDevice.SubsurfaceProfileManager;
                if (profileMgr?.ProfileSRV != null)
                    drawcall.BindSRV(index, profileMgr.ProfileSRV);
            }
            #endregion

            index = drawcall.FindBinder("cbPerCamera");
            if (index.IsValidPointer)
            {
                drawcall.BindCBV(index, policy.DefaultCamera.PerCameraCBuffer);
            }

        }
        public void SetEnableLocalLights(bool value)
        {
            EnableLocalLights.SetValue(value);
            UpdatePermutation().AddWaitTask();
        }
        public void SetEnableSeparatedSpecular(bool value)
        {
            EnableSeparatedSpecular.SetValue(value);
            UpdatePermutation().AddWaitTask();
        }
    }
    [Bricks.CodeBuilder.ContextMenu("DirLighting", "Deferred\\DirLighting", Bricks.RenderPolicyEditor.TtPolicyGraph.RGDEditorKeyword)]
    [Rtti.Meta("",NameAlias = new string[] { "EngineNS.Graphics.Pipeline.Deferred.UDeferredDirLightingNode@EngineCore", "EngineNS.Graphics.Pipeline.Deferred.UDeferredDirLightingNode" })]
    public partial class TtDeferredDirLightingNode : TAuxSceenSpaceNode<TtDeferredDirLightingNode>
    {
        public TtRenderGraphPin Rt0PinIn = TtRenderGraphPin.CreateInput("MRT0", NxRHI.EBufferType.BFT_SRV);
        public TtRenderGraphPin Rt1PinIn = TtRenderGraphPin.CreateInput("MRT1", NxRHI.EBufferType.BFT_SRV);
        public TtRenderGraphPin Rt2PinIn = TtRenderGraphPin.CreateInput("MRT2", NxRHI.EBufferType.BFT_SRV);
        public TtRenderGraphPin Rt3PinIn = TtRenderGraphPin.CreateInputOutput("MRT3", NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_RTV);
        public TtRenderGraphPin DepthStencilPinIn = TtRenderGraphPin.CreateInputOutput("DepthStencil", NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_DSV);
        
        public TtRenderGraphPin ShadowMapPinIn = TtRenderGraphPin.CreateInput("ShadowMap", NxRHI.EBufferType.BFT_SRV);
        public TtRenderGraphPin EnvMapPinIn = TtRenderGraphPin.CreateInput("EnvMap", NxRHI.EBufferType.BFT_SRV);
        public TtRenderGraphPin VignettePinIn = TtRenderGraphPin.CreateInput("Vignette", NxRHI.EBufferType.BFT_SRV);
        public TtRenderGraphPin GpuScenePinIn = TtRenderGraphPin.CreateInput("GpuScene", NxRHI.EBufferType.BFT_SRV);
        
        public TtRenderGraphPin PointLightsPinIn = TtRenderGraphPin.CreateInput("PointLights", NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_UAV);
        public TtRenderGraphPin PointGridHeadersPinIn = TtRenderGraphPin.CreateInput("PointGridHeaders", NxRHI.EBufferType.BFT_SRV);
        public TtRenderGraphPin PointGridDataIndicesPinIn = TtRenderGraphPin.CreateInput("PointGridDataIndices", NxRHI.EBufferType.BFT_SRV);
        
        public TtRenderGraphPin SpotLightsPinIn = TtRenderGraphPin.CreateInput("SpotLights", NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_UAV);
        public TtRenderGraphPin SpotGridHeadersPinIn = TtRenderGraphPin.CreateInput("SpotGridHeaders", NxRHI.EBufferType.BFT_SRV);
        public TtRenderGraphPin SpotGridDataIndicesPinIn = TtRenderGraphPin.CreateInput("SpotGridDataIndices", NxRHI.EBufferType.BFT_SRV);

        public TtRenderGraphPin RtAdvShadowIn = TtRenderGraphPin.CreateInput("AdvShadow", NxRHI.EBufferType.BFT_NONE);

        public TtRenderGraphPin ContactShadowPinIn = TtRenderGraphPin.CreateInput("ContactShadow", NxRHI.EBufferType.BFT_SRV);

        public TtRenderGraphPin HzbPinIn = TtRenderGraphPin.CreateInput("Hzb", NxRHI.EBufferType.BFT_SRV);

        public TtRenderGraphPin SSAOPinIn = TtRenderGraphPin.CreateInput("SSAO", NxRHI.EBufferType.BFT_SRV);

        public TtRenderGraphPin SpecularPinOut = TtRenderGraphPin.CreateOutput("Specular", true, EPixelFormat.PXF_R16G16B16A16_FLOAT, NxRHI.EBufferType.BFT_RTV | NxRHI.EBufferType.BFT_SRV);

        public TtDeferredDirLightingNode()
        {
            Name = "DeferredDirLightingNode";
        }
        public override void Dispose()
        {
            CoreSDK.DisposeObject(ref mInlineContactShadowCBuffer);
            base.Dispose();
        }
        public override void InitNodePins()
        {
            ResultPinOut.Attachement.Format = EPixelFormat.PXF_R11G11B10_FLOAT;
            SpecularPinOut.Attachement.Format = EPixelFormat.PXF_R16G16B16A16_FLOAT;
            base.InitNodePins();

            AddOutput(SpecularPinOut);
            //SpecularPinOut.IsAllowInputNull = true;
            AddInput(Rt0PinIn);
            AddInput(Rt1PinIn);
            AddInput(Rt2PinIn);
            AddInputOutput(Rt3PinIn);
            //Rt3PinIn.IsAllowInputNull = true;
            AddInputOutput(DepthStencilPinIn);
            AddInput(EnvMapPinIn);
            AddInput(VignettePinIn);
            AddInput(GpuScenePinIn);

            AddInput(PointLightsPinIn);
            AddInput(PointGridHeadersPinIn);
            AddInput(PointGridDataIndicesPinIn);

            AddInput(SpotLightsPinIn);
            AddInput(SpotGridHeadersPinIn);
            AddInput(SpotGridDataIndicesPinIn);

            //2选1，优先使用AdvShadow
            AddInput(ShadowMapPinIn);
            ShadowMapPinIn.IsAllowInputNull = true;
            AddInput(RtAdvShadowIn);
            RtAdvShadowIn.IsAllowInputNull = true;
            RtAdvShadowIn.LinkType = "AdvShadow";

            AddInput(ContactShadowPinIn);
            ContactShadowPinIn.IsAllowInputNull = true;

            AddInput(HzbPinIn);
            HzbPinIn.IsAllowInputNull = true;

            AddInput(SSAOPinIn);
            SSAOPinIn.IsAllowInputNull = true;
        }
        public bool IsSeparatedSpecularEnabled => SpecularPinOut.FindOutLinkers().Count > 0;

        // Contact shadow mode is owned by the render policy (policy.ContactShadowMode).
        // The connected input node (when present) is resolved in Initialize.
        [Category("Shading")]
        public TtContactShadowNode ContactShadowNode { get; set; }

        // ---- Inline Contact Shadow parameters (used when ContactShadowMode == Inline) ----
        [Category("Inline Contact Shadow")]
        public float InlineContactShadowLength { get; set; } = 0.5f;
        [Category("Inline Contact Shadow")]
        public int InlineContactShadowNumSteps { get; set; } = 12;
        [Category("Inline Contact Shadow")]
        public float InlineContactShadowDepthBias { get; set; } = 0.001f;
        [Category("Inline Contact Shadow")]
        public float InlineContactShadowFadeDistance { get; set; } = 50.0f;
        [Category("Inline Contact Shadow")]
        public float InlineContactShadowFadeLength { get; set; } = 20.0f;
        [Category("Inline Contact Shadow")]
        public float InlineContactShadowIntensity { get; set; } = 0.8f;
        TtCbView mInlineContactShadowCBuffer;

        /// <summary>
        /// §1.1 compliant: first CreateCBV fills all fields + MarkDirty + FlushDirty.
        /// </summary>
        public TtCbView GetOrCreateInlineContactShadowCBuffer(FEffectBinder binder)
        {
            if (mInlineContactShadowCBuffer == null)
            {
                mInlineContactShadowCBuffer = TtEngine.Instance.GfxDevice.RenderContext.CreateCBV(binder);
                FillInlineContactShadowCBuffer(mInlineContactShadowCBuffer);
                mInlineContactShadowCBuffer.MarkDirty();
                mInlineContactShadowCBuffer.FlushDirty();
                return mInlineContactShadowCBuffer;
            }
            FillInlineContactShadowCBuffer(mInlineContactShadowCBuffer);
            return mInlineContactShadowCBuffer;
        }

        void FillInlineContactShadowCBuffer(TtCbView cb)
        {
            cb.SetValue("InlineCS_NumSteps", InlineContactShadowNumSteps);
            cb.SetValue("InlineCS_Length", InlineContactShadowLength);
            cb.SetValue("InlineCS_DepthBias", InlineContactShadowDepthBias);
            cb.SetValue("InlineCS_FadeDistance", InlineContactShadowFadeDistance);
            cb.SetValue("InlineCS_FadeLength", InlineContactShadowFadeLength);
            cb.SetValue("InlineCS_Intensity", InlineContactShadowIntensity);
            cb.SetValue("InlineCS_FrameIndex", (uint)TtEngine.Instance.FrameCount);
        }
        internal bool IsSSAOConnected;
        internal bool IsHzbConnected;
        public override unsafe TtGraphicsBuffers CreateGBuffers(TtRenderPolicy policy, EPixelFormat format)
        {
            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            var PassDesc = new NxRHI.FRenderPassDesc();

            if (IsSeparatedSpecularEnabled)
            {
                PassDesc.NumOfMRT = 2;
                PassDesc.AttachmentMRTs[0].Format = format;
                PassDesc.AttachmentMRTs[0].Samples = 1;
                PassDesc.AttachmentMRTs[0].LoadAction = NxRHI.EFrameBufferLoadAction.LoadActionClear;
                PassDesc.AttachmentMRTs[0].StoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
                PassDesc.AttachmentMRTs[1].Format = SpecularPinOut.Attachement.Format;
                PassDesc.AttachmentMRTs[1].Samples = 1;
                PassDesc.AttachmentMRTs[1].LoadAction = NxRHI.EFrameBufferLoadAction.LoadActionClear;
                PassDesc.AttachmentMRTs[1].StoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;

                RenderPass = TtEngine.Instance.GfxDevice.RenderPassManager.GetPipelineState<NxRHI.FRenderPassDesc>(rc, in PassDesc);
                GBuffers.Initialize(policy, RenderPass);
                GBuffers.SetRenderTarget(policy, 0, ResultPinOut);
                GBuffers.SetRenderTarget(policy, 1, SpecularPinOut);
            }
            else
            {
                PassDesc.NumOfMRT = 1;
                PassDesc.AttachmentMRTs[0].Format = format;
                PassDesc.AttachmentMRTs[0].Samples = 1;
                PassDesc.AttachmentMRTs[0].LoadAction = NxRHI.EFrameBufferLoadAction.LoadActionClear;
                PassDesc.AttachmentMRTs[0].StoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;

                RenderPass = TtEngine.Instance.GfxDevice.RenderPassManager.GetPipelineState<NxRHI.FRenderPassDesc>(rc, in PassDesc);
                GBuffers.Initialize(policy, RenderPass);
                GBuffers.SetRenderTarget(policy, 0, ResultPinOut);
            }

            GBuffers.TargetViewIdentifier = TargetViewId;
            return GBuffers;
        }
        public override void OnResize(TtRenderPolicy policy, float x, float y)
        {
            if (GBuffers != null && RenderPass != null)
            {
                GBuffers.SetSize(x * OutputScaleFactor, y * OutputScaleFactor);
                ResultPinOut.Attachement.Width = (uint)(x * OutputScaleFactor);
                ResultPinOut.Attachement.Height = (uint)(y * OutputScaleFactor);
                if (IsSeparatedSpecularEnabled)
                {
                    SpecularPinOut.Attachement.Width = (uint)(x * OutputScaleFactor);
                    SpecularPinOut.Attachement.Height = (uint)(y * OutputScaleFactor);
                }
            }
        }
        public TtDeferredDirLightingShading mBasePassShading;
        [Category("Option")]
        public TtDeferredDirLightingShading BasePassShading
        {
            get
            {
                return mBasePassShading;
            }
        }
        public override TtGraphicsShadingEnv GetPassShading(TtRenderMesh.TtAtom atom = null)
        {
            return mBasePassShading;
        }
        public TtShadowMapNode ShadowMapNode;
        public Bricks.AdvanceShadow.TtAdvanceShadowMapNode AdvanceShadowMapNode;
        public override async Thread.Async.TtTask Initialize(TtRenderPolicy policy, string debugName)
        {
            mBasePassShading = await Graphics.Pipeline.Shader.TtShadingEnv.CreateShadingEnv<TtDeferredDirLightingShading>();
            mBasePassShading.EnableLocalLights.SetValue(policy.EnableLocalLights);

            // Enable separated specular MRT when SSSBlur is connected to SpecularPinOut.
            // This lets DirLighting output diffuse(RT0) + specular(RT1) so SSSBlur can
            // blur only diffuse and compose specular back afterwards.
            if (IsSeparatedSpecularEnabled)
            {
                mBasePassShading.SetEnableSeparatedSpecular(true);
            }
            await mBasePassShading.UpdatePermutation();

            await base.Initialize(policy, debugName);

            if (RtAdvShadowIn.FindInLinker() is var linker && linker != null)
            {
                AdvanceShadowMapNode = linker.OutPin.HostNode as Bricks.AdvanceShadow.TtAdvanceShadowMapNode;
            }
            if (ShadowMapPinIn.FindInLinker() is var linker2 && linker2 != null)
            {
                ShadowMapNode = linker2.OutPin.HostNode as TtShadowMapNode;
            }

            if (AdvanceShadowMapNode != null)
            {
                AdvanceShadowMapNode.Enable = true;
                mBasePassShading.ShadowMode = EShadowMode.Advance;
                if (ShadowMapNode != null)
                    ShadowMapNode.Enable = false;
            }
            else if (ShadowMapNode != null)
            {
                mBasePassShading.ShadowMode = EShadowMode.Csm;
                ShadowMapNode.Enable = true;
                if (AdvanceShadowMapNode != null)
                    AdvanceShadowMapNode.Enable = false;
            }
            else
            {
                mBasePassShading.ShadowMode = EShadowMode.None;
            }

            {
                var contactlinker = ContactShadowPinIn.FindInLinker();
                ContactShadowNode = contactlinker?.OutPin.HostNode as TtContactShadowNode;

                // policy 选了 InputNode 但没有连接 TtContactShadowNode -> 运行期会自动退回 Inline, 这里启动时提醒一次
                if (policy.ContactShadowMode == EContactShadowMode.InputNode && ContactShadowNode == null)
                {
                    Profiler.Log.WriteLine<Profiler.TtGraphicsGategory>(
                        Profiler.ELogTag.Warning,
                        $"DeferredDirLighting({debugName}): policy.ContactShadowMode=InputNode but no TtContactShadowNode connected to ContactShadowPinIn, falling back to Inline mode.");
                }
            }

            // Inline contact shadow HZB acceleration: enabled at FrameBuild when in Inline mode + HzbPinIn connected.
            IsHzbConnected = (HzbPinIn.FindInLinker() != null);

            // SSAO: enable permutation if pin is connected
            IsSSAOConnected = (SSAOPinIn.FindInLinker() != null);
            if (IsSSAOConnected)
            {
                mBasePassShading.EnableSSAO.SetValue((int)Shader.EPermutation_Bool.TrueValue);
                mBasePassShading.UpdatePermutation().AddWaitTask();
            }
        }
        [Category("Shading")]
        public EShadowMode ShadowMode
        {
            get => mBasePassShading.ShadowMode;
            set
            {
                if (value == EShadowMode.Advance)
                {
                    if (AdvanceShadowMapNode != null)
                        AdvanceShadowMapNode.Enable = true;
                    else
                        return;
                    if (ShadowMapNode != null)
                        ShadowMapNode.Enable = false;
                }
                else if (value == EShadowMode.Csm)
                {
                    if (ShadowMapNode != null)
                        ShadowMapNode.Enable = true;
                    else
                        return;
                    if (AdvanceShadowMapNode != null)
                        AdvanceShadowMapNode.Enable = false;
                }
                else
                {
                    if (AdvanceShadowMapNode != null)
                        AdvanceShadowMapNode.Enable = false;
                    if (ShadowMapNode != null)
                        ShadowMapNode.Enable = false;
                }
                mBasePassShading.ShadowMode = value;
            }
        }
        public override void FrameBuild(Graphics.Pipeline.TtRenderPolicy policy)
        {
            // Drive the single tri-state contact shadow permutation (ETypeAA-style enum mode).
            // Mode is owned by the render policy (policy.ContactShadowMode); the node only falls
            // back InputNode -> Inline locally when no TtContactShadowNode is connected.
            if (mBasePassShading != null)
            {
                EContactShadowMode desiredMode = policy.ContactShadowMode;
                if (desiredMode == EContactShadowMode.InputNode && ContactShadowNode == null)
                    desiredMode = EContactShadowMode.Inline;

                if (mBasePassShading.ContactShadowMode.Value.GetValue(mBasePassShading.ContactShadowMode) != (uint)desiredMode)
                {
                    mBasePassShading.ContactShadowMode.SetValue((uint)desiredMode);
                    mBasePassShading.UpdatePermutation().AddWaitTask();
                }

                if (ContactShadowNode != null)
                    ContactShadowNode.Enable = (desiredMode == EContactShadowMode.InputNode);

                // Inline contact shadow HZB variant: only meaningful in Inline mode with HZB connected.
                bool useInlineHzb = (desiredMode == EContactShadowMode.Inline) && IsHzbConnected;
                var desiredHzb = useInlineHzb
                    ? (uint)Shader.EPermutation_Bool.TrueValue
                    : (uint)Shader.EPermutation_Bool.FalseValue;
                if (mBasePassShading.InlineContactShadowHzb.Value.GetValue(mBasePassShading.InlineContactShadowHzb) != desiredHzb)
                {
                    mBasePassShading.InlineContactShadowHzb.SetValue(desiredHzb);
                    mBasePassShading.UpdatePermutation().AddWaitTask();
                }
            }

            // Drive SSAO permutation by pin connection + policy
            if (mBasePassShading != null)
            {
                bool shouldEnableSSAO = IsSSAOConnected && policy.EnableAO;
                var desired = shouldEnableSSAO
                    ? (uint)Shader.EPermutation_Bool.TrueValue
                    : (uint)Shader.EPermutation_Bool.FalseValue;
                if (mBasePassShading.EnableSSAO.Value.GetValue(mBasePassShading.EnableSSAO) != desired)
                {
                    mBasePassShading.EnableSSAO.SetValue(desired);
                    mBasePassShading.UpdatePermutation().AddWaitTask();
                }
            }

            ShadowMode = policy.ShadowMode;
        }
        public override void Tick(GamePlay.TtWorld world, TtRenderPolicy policy, NxRHI.TtCommandList frameCmdList, bool bClear)
        {
            GBuffers?.SetViewportCBuffer(world, policy);
            base.Tick(world, policy, frameCmdList, bClear);
        }
        public override void TickSync(TtRenderPolicy policy)
        {
            base.TickSync(policy);
        }
    }
}

namespace EngineNS
{
    partial class TtEngine : IDisposable
    {
        public unsafe void Dispose()
        {
            if (PreIntegratedDFData != IntPtr.Zero)
            {
                CoreSDK.Free(PreIntegratedDFData.ToPointer());
                PreIntegratedDFData = IntPtr.Zero;
            }

            PreIntegratedDFTexture?.Dispose();
            PreIntegratedDFTexture = null;
            PreIntegratedDFSrv?.Dispose();
            PreIntegratedDFSrv = null;
        }

        uint ReverseBits(uint Bits)
        {
            Bits = (Bits << 16) | (Bits >> 16);
            Bits = ((Bits & 0x00ff00ff) << 8) | ((Bits & 0xff00ff00) >> 8);
            Bits = ((Bits & 0x0f0f0f0f) << 4) | ((Bits & 0xf0f0f0f0) >> 4);
            Bits = ((Bits & 0x33333333) << 2) | ((Bits & 0xcccccccc) >> 2);
            Bits = ((Bits & 0x55555555) << 1) | ((Bits & 0xaaaaaaaa) >> 1);
            return Bits;
        }
        IntPtr PreIntegratedDFData;
        void InitPreIntegratedDF()
        {
            unsafe
            {
                int Y = 32;
                int X = 128;
                uint BufferSize = (uint)X * (uint)Y * (uint)sizeof(UInt16) * 2;
                var pAddr = (UInt16*)CoreSDK.Alloc(BufferSize, "PreIntegratedDFTexture", 0);
                for (int y = 0; y < Y; y++)
                {
                    float Roughness = (float)(y + 0.5f) / Y;
                    float m = Roughness * Roughness;
                    float m2 = m * m;

                    for (int x = 0; x < X; x++)
                    {
                        float NoV = (float)(x + 0.5f) / X;

                        Vector3 V;
                        V.X = MathHelper.Sqrt(1.0f - NoV * NoV);    // sin
                        V.Y = 0.0f;
                        V.Z = NoV;                              // cos

                        float A = 0.0f;
                        float B = 0.0f;
                        float C = 0.0f;

                        const uint NumSamples = 128;
                        for (uint i = 0; i < NumSamples; i++)
                        {
                            float E1 = (float)i / NumSamples;
                            double v1 = ReverseBits(i);
                            double v2 = 4294967296;
                            float E2 = (float)(v1 / v2);

                            {
                                float Phi = 2.0f * MathHelper.PI * E1;
                                float CosPhi = MathHelper.Cos(Phi);
                                float SinPhi = MathHelper.Sin(Phi);
                                float CosTheta = MathHelper.Sqrt((1.0f - E2) / (1.0f + (m2 - 1.0f) * E2));
                                float SinTheta = MathHelper.Sqrt(1.0f - CosTheta * CosTheta);

                                Vector3 H = new Vector3(SinTheta * MathHelper.Cos(Phi), SinTheta * MathHelper.Sin(Phi), CosTheta);
                                Vector3 L = 2.0f * Vector3.Dot(V, H) * H - V;

                                float NoL = MathHelper.Max(L.Z, 0.0f);
                                float NoH = MathHelper.Max(H.Z, 0.0f);
                                float VoH = MathHelper.Max(Vector3.Dot(V, H), 0.0f);

                                if (NoL > 0.0f)
                                {
                                    float Vis_SmithV = NoL * (NoV * (1 - m) + m);
                                    float Vis_SmithL = NoV * (NoL * (1 - m) + m);
                                    float Vis = 0.5f / (Vis_SmithV + Vis_SmithL);

                                    float NoL_Vis_PDF = NoL * Vis * (4.0f * VoH / NoH);
                                    float Fc = 1.0f - VoH;

                                    Fc *= (Fc * Fc) * (Fc * Fc);
                                    A += NoL_Vis_PDF * (1.0f - Fc);
                                    B += NoL_Vis_PDF * Fc;
                                }
                            }

                            {
                                float Phi = 2.0f * MathHelper.PI * E1;
                                float CosPhi = MathHelper.Cos(Phi);
                                float SinPhi = MathHelper.Sin(Phi);
                                float CosTheta = MathHelper.Sqrt(E2);
                                float SinTheta = MathHelper.Sqrt(1.0f - CosTheta * CosTheta);

                                Vector3 L = new Vector3(SinTheta * MathHelper.Cos(Phi), SinTheta * MathHelper.Sin(Phi), CosTheta);
                                Vector3 H = (V + L);
                                H.Normalize();

                                float NoL = MathHelper.Max(L.Z, 0.0f);
                                float NoH = MathHelper.Max(H.Z, 0.0f);
                                float VoH = MathHelper.Max(Vector3.Dot(V, H), 0.0f);

                                float FD90 = 0.5f + 2.0f * VoH * VoH * Roughness;
                                float FdV = 1.0f + (FD90 - 1.0f) * MathHelper.Pow(1.0f - NoV, 5);
                                float FdL = 1.0f + (FD90 - 1.0f) * MathHelper.Pow(1.0f - NoL, 5);
                                C += FdV * FdL;// * ( 1.0f - 0.3333f * Roughness );
                            }
                        }
                        A /= NumSamples;
                        B /= NumSamples;
                        C /= NumSamples;

                        //if (Desc.Format == PF_G16R16)
                        {
                            pAddr[(x + y * X) * 2] = (UInt16)(MathHelper.Clamp(A, 0.0f, 1.0f) * 65535.0f + 0.5f);
                            pAddr[(x + y * X) * 2 + 1] = (UInt16)(MathHelper.Clamp(B, 0.0f, 1.0f) * 65535.0f + 0.5f);
                        }
                    }
                }
                PreIntegratedDFData = (IntPtr)pAddr;
            }
        }

        public TtSrView GetPreIntegratedDFSrv(NxRHI.ICommandList cmd)
        {
            if (PreIntegratedDFTexture == null || PreIntegratedDFSrv == null)
            {
                if (PreIntegratedDFData == IntPtr.Zero)
                {
                    InitPreIntegratedDF();
                }
                if (PreIntegratedDFData == IntPtr.Zero)
                {
                    Profiler.Log.WriteLine<Profiler.TtGraphicsGategory>(
                        Profiler.ELogTag.Warning,
                        "PreIntegratedDFData is null, skip PreIntegratedDF SRV creation.");
                    return null;
                }

                var desc = new NxRHI.FTextureDesc();
                desc.SetDefault();
                desc.Format = EPixelFormat.PXF_R16G16_UNORM;
                desc.Width = 128;
                desc.Height = 32;
                uint rowPitch = desc.Width * sizeof(UInt16) * 2;
                uint totalSize = rowPitch * desc.Height;

                unsafe
                {
                    var initData = new NxRHI.FMappedSubResource();
                    initData.SetDefault();
                    initData.pData = PreIntegratedDFData.ToPointer();
                    initData.RowPitch = rowPitch;
                    initData.DepthPitch = totalSize;
                    desc.InitData = &initData;
                    PreIntegratedDFTexture = TtEngine.Instance.GfxDevice.RenderContext.CreateTexture(in desc);
                }
                if (PreIntegratedDFTexture == null)
                {
                    Profiler.Log.WriteLine<Profiler.TtGraphicsGategory>(
                        Profiler.ELogTag.Warning,
                        "Create PreIntegratedDFTexture failed.");
                    return null;
                }

                var srvDesc = new NxRHI.FSrvDesc();
                //srvDesc.SetTexture2DArray();
                //srvDesc.Format = desc.Format;
                //srvDesc.Texture2DArray.ArraySize = desc.ArraySize;
                //srvDesc.Texture2DArray.FirstArraySlice = 0;
                //srvDesc.Texture2DArray.MipLevels = desc.MipLevels;
                //srvDesc.Texture2DArray.MostDetailedMip = 0;
                srvDesc.SetTexture2D();
                srvDesc.Format = desc.Format;
                srvDesc.Texture2D.MipLevels = desc.MipLevels;
                PreIntegratedDFSrv = TtEngine.Instance.GfxDevice.RenderContext.CreateSRV(PreIntegratedDFTexture, in srvDesc);
            }

            return PreIntegratedDFSrv;
        }

        TtTexture PreIntegratedDFTexture = null;
        TtSrView PreIntegratedDFSrv = null;
    }
}
