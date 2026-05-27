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
    public partial class TtDeferredDirLightingShading : Shader.TtGraphicsShadingEnv
    {
        #region Permutation
        public TtPermutationItem DisableAO
        {
            get;
            set;
        }
        public TtPermutationItem DisablePointLights
        {
            get;
            set;
        }
        public TtPermutationItem DisableSunshaft
        {
            get;
            set;
        }
        public TtPermutationItem DisableBloom
        {
            get;
            set;
        }
        public TtPermutationItem DisableHdr
        {
            get;
            set;
        }
        public TtPermutationItem EnableRimLight
        {
            get;
            set;
        }
        public TtPermutationItem EnableSeparatedSpecular
        {
            get;
            set;
        }
        [Category("Option")]
        public bool IsEnableRimLight
        {
            get
            {
                return EnableRimLight.Value.GetValue(EnableRimLight) == 1;
            }
            set
            {
                EnableRimLight.SetValue(value);
                this.UpdatePermutation().AddWaitTask();
            }
        }
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "EDebugShowMode")]
        public enum EDebugShowMode : uint
        {
            None = 0,
            N,
            NoH,
            LoH,
            NoV,
            VoH,
            NoL,
            Specular,
            Num,
        }
        public TtPermutationItem DebugShowModePermutation
        {
            get;
            set;
        }
        [Category("Option")]
        public EDebugShowMode DebugShowMode
        {
            get
            {
                return (EDebugShowMode)DebugShowModePermutation.Value.GetValue(DebugShowModePermutation);
            }
            set
            {
                DebugShowModePermutation.SetValue((uint)value);
                this.UpdatePermutation().AddWaitTask();
            }
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

            DisableAO = this.PushPermutation<Shader.EPermutation_Bool>("ENV_DISABLE_AO", (int)Shader.EPermutation_Bool.BitWidth);
            DisablePointLights = this.PushPermutation<Shader.EPermutation_Bool>("ENV_GRID_LIGHTS", (int)Shader.EPermutation_Bool.BitWidth);
            DisableSunshaft = this.PushPermutation<Shader.EPermutation_Bool>("ENV_DISABLE_SUNSHAFT", (int)Shader.EPermutation_Bool.BitWidth);
            DisableBloom = this.PushPermutation<Shader.EPermutation_Bool>("ENV_DISABLE_BLOOM", (int)Shader.EPermutation_Bool.BitWidth);
            DisableHdr = this.PushPermutation<Shader.EPermutation_Bool>("ENV_DISABLE_HDR", (int)Shader.EPermutation_Bool.BitWidth);
            EnableRimLight = this.PushPermutation<Shader.EPermutation_Bool>("ENV_ENABLE_RIMLIGHT", (int)Shader.EPermutation_Bool.BitWidth);
            EnableSeparatedSpecular = this.PushPermutation<Shader.EPermutation_Bool>("ENV_ENABLE_SEPARATED_SPECULAR", (int)Shader.EPermutation_Bool.BitWidth);

            DisableAO.SetValue((int)Shader.EPermutation_Bool.FalseValue);
            DisablePointLights.SetValue((int)Shader.EPermutation_Bool.TrueValue);
            DisableSunshaft.SetValue((int)Shader.EPermutation_Bool.TrueValue);
            DisableBloom.SetValue((int)Shader.EPermutation_Bool.TrueValue);
            DisableHdr.SetValue((int)Shader.EPermutation_Bool.TrueValue);
            EnableRimLight.SetValue((int)Shader.EPermutation_Bool.FalseValue);
            EnableSeparatedSpecular.SetValue((int)Shader.EPermutation_Bool.FalseValue);

            DebugShowModePermutation = this.PushPermutation<EDebugShowMode>("ENV_EDebugShowMode", GetBitWidth((int)EDebugShowMode.Num));
            DebugShowModePermutation.SetValue((int)EDebugShowMode.None);

            ShadowModePermutation = this.PushPermutation<EShadowMode>("ENV_EShadowMode", GetBitWidth((int)EShadowMode.Num));
            ShadowModePermutation.SetValue((int)EShadowMode.Csm);

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
                EPixelShaderInput.PST_LightMap,
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
                drawcall.BindSRV(index, TtEngine.Instance.GetPreIntegratedDFSrv(cmd));
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
            index = drawcall.FindBinder("cbShadingEnv");
            if (index.IsValidPointer)
            {
                if (dirLightingNode.CBShadingEnv == null)
                {
                    dirLightingNode.CBShadingEnv = TtEngine.Instance.GfxDevice.RenderContext.CreateCBV(index);
                }
                dirLightingNode.CBShadingEnv.SetValue("RimPower", dirLightingNode.RimPower);
                dirLightingNode.CBShadingEnv.SetValue("RimIntensity", dirLightingNode.RimIntensity);
                drawcall.BindCBV(index, dirLightingNode.CBShadingEnv);
            }
        }
        public void SetDisableAO(bool value)
        {
            DisableAO.SetValue(value);
            UpdatePermutation().AddWaitTask();
        }
        public void SetDisableSunShaft(bool value)
        {
            DisableSunshaft.SetValue(value);
            UpdatePermutation().AddWaitTask();
        }
        public void SetDisableBloom(bool value)
        {
            DisableBloom.SetValue(value);
            UpdatePermutation().AddWaitTask();
        }
        public void SetDisableHDR(bool value)
        {
            DisableHdr.SetValue(value);
            UpdatePermutation().AddWaitTask();
        }
        public void SetDisablePointLights(bool value)
        {
            DisablePointLights.SetValue(value);
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

        public TtRenderGraphPin SpecularPinOut = TtRenderGraphPin.CreateOutput("Specular", true, EPixelFormat.PXF_R16G16B16A16_FLOAT, NxRHI.EBufferType.BFT_RTV | NxRHI.EBufferType.BFT_SRV);

        public NxRHI.TtCbView CBShadingEnv;
        [Category("Shading")]
        public float RimPower { get; set; } = 5.0f;
        [Category("Shading")]
        public float RimIntensity { get; set; } = 0.5f;

        public TtDeferredDirLightingNode()
        {
            Name = "DeferredDirLightingNode";
        }
        public override void Dispose()
        {
            CoreSDK.DisposeObject(ref CBShadingEnv);
            base.Dispose();
        }
        public override void InitNodePins()
        {
            ResultPinOut.Attachement.Format = EPixelFormat.PXF_R16G16B16A16_FLOAT;
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
        }
        public bool IsSeparatedSpecularEnabled => SpecularPinOut.FindOutLinkers().Count > 0;
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
                //base.CreateGBuffers(policy, format);
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
        public override void FrameBuild(Graphics.Pipeline.TtRenderPolicy policy)
        {
            base.FrameBuild(policy);
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

            if (SpecularPinOut.FindOutLinkers().Count > 0)
            {
                mBasePassShading.SetEnableSeparatedSpecular(true);
            }
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
        }
        [Category("Option")]
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
                var desc = new NxRHI.FTextureDesc();
                desc.SetDefault();
                desc.Format = EPixelFormat.PXF_R16G16_UNORM;
                desc.Width = 128;
                desc.Height = 32;

                PreIntegratedDFTexture = TtEngine.Instance.GfxDevice.RenderContext.CreateTexture(in desc);
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

                var fp = new NxRHI.FSubResourceFootPrint();
                fp.SetDefault();
                fp.Format = PreIntegratedDFTexture.mCoreObject.Desc.Format;
                fp.Width = PreIntegratedDFTexture.mCoreObject.Desc.Width;
                fp.Height = PreIntegratedDFTexture.mCoreObject.Desc.Height;
                fp.Depth = 1;
                fp.RowPitch = (uint)fp.Width * sizeof(UInt16) * 2;
                uint BufferSize = 128 * 32 * (uint)sizeof(UInt16) * 2;
                fp.TotalSize = BufferSize;

                unsafe
                {
                    PreIntegratedDFTexture.UpdateGpuData(cmd, 0, PreIntegratedDFData.ToPointer(), &fp);
                }
            }

            return PreIntegratedDFSrv;
        }

        TtTexture PreIntegratedDFTexture = null;
        TtSrView PreIntegratedDFSrv = null;
    }
}
