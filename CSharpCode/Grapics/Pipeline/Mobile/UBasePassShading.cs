using EngineNS.Graphics.Mesh;
using EngineNS.Graphics.Pipeline.Shader;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Mobile
{
    public class UBasePassShading : Shader.UShadingEnv
    {
        public UBasePassShading()
        {
            var disable_AO = new MacroDefine();//0
            disable_AO.Name = "ENV_DISABLE_AO";
            disable_AO.Values.Add("0");
            disable_AO.Values.Add("1");
            MacroDefines.Add(disable_AO);

            var disable_PointLights = new MacroDefine();//1
            disable_PointLights.Name = "ENV_DISABLE_POINTLIGHTS";
            disable_PointLights.Values.Add("0");
            disable_PointLights.Values.Add("1");
            MacroDefines.Add(disable_PointLights);

            var disable_Shadow = new MacroDefine();//2
            disable_Shadow.Name = "DISABLE_SHADOW_ALL";
            disable_Shadow.Values.Add("0");
            disable_Shadow.Values.Add("1");
            MacroDefines.Add(disable_Shadow);

            var mode_editor = new MacroDefine();//3
            mode_editor.Name = "MODE_EDITOR";
            mode_editor.Values.Add("0");
            mode_editor.Values.Add("1");
            MacroDefines.Add(mode_editor);

            UpdatePermutationBitMask();

            mMacroValues.Add("0");//disable_AO = 0
            mMacroValues.Add("0");//disalbe_PointLights = 0
            mMacroValues.Add("0");//disalbe_Shadow = 0
            mMacroValues.Add("0");//mode_editor = 0
            
            UpdatePermutation(mMacroValues);
        }
        public override EVertexSteamType[] GetNeedStreams()
        {
            return new EVertexSteamType[] { EVertexSteamType.VST_Position,
                EVertexSteamType.VST_Normal,
                EVertexSteamType.VST_Tangent,
                EVertexSteamType.VST_Color,
                EVertexSteamType.VST_LightMap,
                EVertexSteamType.VST_UV,};
        }
        public void SetDisableAO(bool value)
        {
            mMacroValues[0] = value ? "1" : "0";
            UpdatePermutation(mMacroValues);
        }
        public void SetDisablePointLights(bool value)
        {
            mMacroValues[1] = value ? "1" : "0";
            UpdatePermutation(mMacroValues);
        }
        public void SetDisableShadow(bool value)
        {
            mMacroValues[2] = value ? "1" : "0";
            UpdatePermutation(mMacroValues);
        }
        public override bool IsValidPermutation(Shader.UMaterial mtl, uint permutation)
        {
            var ao_value = GetDefineValue(permutation, "ENV_DISABLE_AO");
            var pointlights_value = GetDefineValue(permutation, "ENV_DISABLE_POINTLIGHTS");
            if (mtl.LightingMode != Shader.UMaterial.ELightingMode.Stand)
            {
                if(ao_value == "1" || pointlights_value == "1")
                    return false;
            }
            return true;
        }
        public class VarIndexer : RHI.CShaderProgram.IShaderVarIndexer
        {
            [RHI.CShaderProgram.ShaderVar(VarType = "Texture")]
            public uint gEnvMap;
            [RHI.CShaderProgram.ShaderVar(VarType = "Texture")]
            public uint gShadowMap;
            [RHI.CShaderProgram.ShaderVar(VarType = "Sampler")]
            public uint Samp_gEnvMap;
            [RHI.CShaderProgram.ShaderVar(VarType = "Sampler")]
            public uint Samp_gShadowMap;
        }

        protected virtual VarIndexer GetVarIndexer(RHI.CDrawCall drawcall)
        {
            VarIndexer indexer = (VarIndexer)drawcall.Effect.TagObject;
            if (drawcall.Effect.TagObject == null)
            {
                indexer = new VarIndexer();
                indexer.UpdateIndex(drawcall.Effect.ShaderProgram);
                drawcall.Effect.TagObject = indexer;
            }
            return indexer;
        }
        public unsafe override void OnBuildDrawCall(IRenderPolicy policy, RHI.CDrawCall drawcall)
        {
        }
        public unsafe override void OnDrawCall(Pipeline.IRenderPolicy.EShadingType shadingType, RHI.CDrawCall drawcall, IRenderPolicy policy, Mesh.UMesh mesh)
        {
            base.OnDrawCall(shadingType, drawcall, policy, mesh);

            var Manager = policy as Mobile.UMobileEditorFSPolicy;
            if (Manager != null)
            {
                var indexer = GetVarIndexer(drawcall);
                if (Manager.EnvMapSRV != null)
                {
                    drawcall.mCoreObject.BindShaderSrv(indexer.gEnvMap, Manager.EnvMapSRV.mCoreObject);
                    drawcall.mCoreObject.BindShaderSampler(indexer.Samp_gEnvMap, UEngine.Instance.GfxDevice.SamplerStateManager.DefaultState.mCoreObject);
                }

                if (Manager.mShadowMapNode != null && Manager.mShadowMapNode.GBuffers.GetDepthStencilSRV() != null)
                {
                    drawcall.mCoreObject.BindShaderSrv(indexer.gShadowMap, Manager.mShadowMapNode.GBuffers.GetDepthStencilSRV().mCoreObject);
                    drawcall.mCoreObject.BindShaderSampler(indexer.Samp_gShadowMap, UEngine.Instance.GfxDevice.SamplerStateManager.DefaultState.mCoreObject);
                }

                var index = drawcall.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_CBuffer, "cbPerGpuScene");
                if (!CoreSDK.IsNullPointer(index))
                    drawcall.mCoreObject.BindShaderCBuffer(index, Manager.GpuSceneNode.PerGpuSceneCBuffer.mCoreObject);

                index = drawcall.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Srv, "TilingBuffer");
                if (!CoreSDK.IsNullPointer(index))
                    drawcall.mCoreObject.BindShaderSrv(index, Manager.ScreenTilingNode.TileSRV.mCoreObject);

                index = drawcall.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Srv, "GpuScene_PointLights");
                if (!CoreSDK.IsNullPointer(index))
                    drawcall.mCoreObject.BindShaderSrv(index, Manager.GpuSceneNode.PointLights.DataSRV.mCoreObject);
            }
        }
    }
    public class UBasePassOpaque : UBasePassShading
    {
        public UBasePassOpaque()
        {
            CodeName = RName.GetRName("shaders/ShadingEnv/Mobile/MobileOpaque.cginc", RName.ERNameType.Engine);
        }
    }
    public class UBasePassTranslucent : UBasePassShading
    {
        public UBasePassTranslucent()
        {
            CodeName = RName.GetRName("shaders/ShadingEnv/Mobile/MobileTranslucent.cginc", RName.ERNameType.Engine);
        }
    }
    public class UMobileOpaqueNode : Common.UBasePassNode
    {
        public UBasePassOpaque mOpaqueShading;
        public UPassDrawBuffers BasePass = new UPassDrawBuffers();
        public RHI.CRenderPass RenderPass;
        public RHI.CRenderPass GizmosRenderPass;

        public override async System.Threading.Tasks.Task Initialize(IRenderPolicy policy, Shader.UShadingEnv shading, EPixelFormat rtFmt, EPixelFormat dsFmt, float x, float y, string debugName)
        {
            await Thread.AsyncDummyClass.DummyFunc();

            var rc = UEngine.Instance.GfxDevice.RenderContext;
            BasePass.Initialize(rc, debugName);

            var PassDesc = new IRenderPassDesc();
            unsafe
            {
                PassDesc.NumOfMRT = 1;
                PassDesc.AttachmentMRTs[0].IsSwapChain = 0;
                PassDesc.AttachmentMRTs[0].Format = rtFmt;
                PassDesc.AttachmentMRTs[0].Samples = 1;
                PassDesc.AttachmentMRTs[0].LoadAction = FrameBufferLoadAction.LoadActionClear;
                PassDesc.AttachmentMRTs[0].StoreAction = FrameBufferStoreAction.StoreActionStore;
                PassDesc.m_AttachmentDepthStencil.Format = dsFmt;
                PassDesc.m_AttachmentDepthStencil.Samples = 1;
                PassDesc.m_AttachmentDepthStencil.LoadAction = FrameBufferLoadAction.LoadActionClear;
                PassDesc.m_AttachmentDepthStencil.StoreAction = FrameBufferStoreAction.StoreActionStore;
                PassDesc.m_AttachmentDepthStencil.StencilLoadAction = FrameBufferLoadAction.LoadActionClear;
                PassDesc.m_AttachmentDepthStencil.StencilStoreAction = FrameBufferStoreAction.StoreActionStore;
                //PassDesc.mFBClearColorRT0 = new Color4(1, 0, 0, 0);
                //PassDesc.mDepthClearValue = 1.0f;
                //PassDesc.mStencilClearValue = 0u;
            }
            RenderPass = UEngine.Instance.GfxDevice.RenderPassManager.GetPipelineState<IRenderPassDesc>(rc, in PassDesc);
            var GizmosPassDesc = new IRenderPassDesc();
            unsafe
            {
                GizmosPassDesc.NumOfMRT = 1;
                GizmosPassDesc.AttachmentMRTs[0].IsSwapChain = 0;
                GizmosPassDesc.AttachmentMRTs[0].Format = rtFmt;
                GizmosPassDesc.AttachmentMRTs[0].Samples = 1;
                GizmosPassDesc.AttachmentMRTs[0].LoadAction = FrameBufferLoadAction.LoadActionDontCare;
                GizmosPassDesc.AttachmentMRTs[0].StoreAction = FrameBufferStoreAction.StoreActionStore;
                GizmosPassDesc.m_AttachmentDepthStencil.Format = dsFmt;
                GizmosPassDesc.m_AttachmentDepthStencil.Samples = 1;
                GizmosPassDesc.m_AttachmentDepthStencil.LoadAction = FrameBufferLoadAction.LoadActionClear;
                GizmosPassDesc.m_AttachmentDepthStencil.StoreAction = FrameBufferStoreAction.StoreActionStore;
                GizmosPassDesc.m_AttachmentDepthStencil.StencilLoadAction = FrameBufferLoadAction.LoadActionClear;
                GizmosPassDesc.m_AttachmentDepthStencil.StencilStoreAction = FrameBufferStoreAction.StoreActionStore;
                //GizmosPassDesc.mFBClearColorRT0 = new Color4(1, 0, 0, 0);
                //GizmosPassDesc.mDepthClearValue = 1.0f;
                //GizmosPassDesc.mStencilClearValue = 0u;
            }
            GizmosRenderPass = UEngine.Instance.GfxDevice.RenderPassManager.GetPipelineState<IRenderPassDesc>(rc, in GizmosPassDesc);

            GBuffers.Initialize(RenderPass, policy.Camera, 1, dsFmt, (uint)x, (uint)y);
            GBuffers.CreateGBuffer(0, rtFmt, (uint)x, (uint)y);
            GBuffers.UpdateFrameBuffers(x, y);
            GBuffers.TargetViewIdentifier = new UGraphicsBuffers.UTargetViewIdentifier();

            GGizmosBuffers.Initialize(GizmosRenderPass, policy.Camera, 1, dsFmt, (uint)x, (uint)y);
            GGizmosBuffers.SetGBuffer(0, GBuffers.GetGBufferSRV(0), true);
            GGizmosBuffers.UpdateFrameBuffers(x, y);
            GGizmosBuffers.TargetViewIdentifier = GBuffers.TargetViewIdentifier;
            GGizmosBuffers.Camera = GBuffers.Camera;

            //mBasePassShading = shading as Pipeline.Mobile.UBasePassOpaque;
            mOpaqueShading = UEngine.Instance.ShadingEnvManager.GetShadingEnv<UBasePassOpaque>();
        }
        public virtual void Cleanup()
        {
            GBuffers?.Cleanup();
            GBuffers = null;

            GGizmosBuffers?.Cleanup();
            GGizmosBuffers = null;
        }
        public override void OnResize(IRenderPolicy policy, float x, float y)
        {
            if (GBuffers != null)
            {
                GBuffers.OnResize(x, y);
            }
            if (GGizmosBuffers != null)
            {
                GGizmosBuffers.SetGBuffer(0, GBuffers.GetGBufferSRV(0), true);                
                GGizmosBuffers.OnResize(x, y);
            }
        }
        protected void SetCBuffer(GamePlay.UWorld world, RHI.CConstantBuffer cBuffer, UMobileFSPolicy mobilePolicy)
        {
            cBuffer.SetValue(cBuffer.PerViewportIndexer.gFadeParam, in mobilePolicy.mShadowMapNode.mFadeParam);
            cBuffer.SetValue(cBuffer.PerViewportIndexer.gShadowTransitionScale, in mobilePolicy.mShadowMapNode.mShadowTransitionScale);
            cBuffer.SetValue(cBuffer.PerViewportIndexer.gShadowMapSizeAndRcp, in mobilePolicy.mShadowMapNode.mShadowMapSizeAndRcp);
            cBuffer.SetValue(cBuffer.PerViewportIndexer.gViewer2ShadowMtx, in mobilePolicy.mShadowMapNode.mViewer2ShadowMtx);
            cBuffer.SetValue(cBuffer.PerViewportIndexer.gShadowDistance, in mobilePolicy.mShadowMapNode.mShadowDistance);

            var dirLight = world.DirectionLight;
            //dirLight.mDirection = MathHelper.RandomDirection();
            var dir = dirLight.mDirection;
            var gDirLightDirection_Leak = new Vector4(dir.X, dir.Y, dir.Z, dirLight.mSunLightLeak);
            cBuffer.SetValue(cBuffer.PerViewportIndexer.gDirLightDirection_Leak, in gDirLightDirection_Leak);
            var gDirLightColor_Intensity = new Vector4(dirLight.mSunLightColor.X, dirLight.mSunLightColor.Y, dirLight.mSunLightColor.Z, dirLight.mSunLightIntensity);
            cBuffer.SetValue(cBuffer.PerViewportIndexer.gDirLightColor_Intensity, in gDirLightColor_Intensity);

            cBuffer.SetValue(cBuffer.PerViewportIndexer.mSkyLightColor, in dirLight.mSkyLightColor);
            cBuffer.SetValue(cBuffer.PerViewportIndexer.mGroundLightColor, in dirLight.mGroundLightColor);

            float EnvMapMaxMipLevel = 1.0f;
            cBuffer.SetValue(cBuffer.PerViewportIndexer.gEnvMapMaxMipLevel, in EnvMapMaxMipLevel);
            cBuffer.SetValue(cBuffer.PerViewportIndexer.gEyeEnvMapMaxMipLevel, in EnvMapMaxMipLevel);
        }
        public override void TickLogic(GamePlay.UWorld world, IRenderPolicy policy, bool bClear)
        {
            var mobilePolicy = policy as UMobileFSPolicy;
            var cBuffer = GBuffers.PerViewportCBuffer;
            if (mobilePolicy != null)
            {
                if (cBuffer != null)
                    SetCBuffer(world, cBuffer, mobilePolicy);
            }

            BasePass.ClearMeshDrawPassArray();
            BasePass.SetViewport(GBuffers.ViewPort);

            foreach (var i in policy.VisibleMeshes)
            {
                if (i.Atoms == null)
                    continue;

                for (int j = 0; j < i.Atoms.Length; j++)
                {
                    var layer = i.Atoms[j].Material.RenderLayer;
                    if (layer != ERenderLayer.RL_Opaque)
                        continue;
                    var drawcall = i.GetDrawCall(GBuffers, j, policy, IRenderPolicy.EShadingType.BasePass, this);
                    if (drawcall != null)
                    {
                        drawcall.BindGBuffer(GBuffers);
                        //GGizmosBuffers.PerViewportCBuffer = GBuffers.PerViewportCBuffer;

                        BasePass.PushDrawCall(layer, drawcall);
                    }
                }
            }

            var cmdlist = BasePass.PassBuffers[(int)ERenderLayer.RL_Opaque].DrawCmdList.mCoreObject;

            var passClears = new IRenderPassClears();
            passClears.SetDefault();
            passClears.SetClearColor(0, new Color4(1, 0, 0, 0));
            cmdlist.BeginRenderPass(GBuffers.FrameBuffers.mCoreObject, in passClears, ERenderLayer.RL_Opaque.ToString());
            cmdlist.BuildRenderPass(0);
            cmdlist.EndRenderPass();
            cmdlist.EndCommand();
        }
        public override void TickRender(IRenderPolicy policy)
        {
            var rc = UEngine.Instance.GfxDevice.RenderContext;
            BasePass.Commit(rc);
        }
        public override void TickSync(IRenderPolicy policy)
        {
            BasePass.SwapBuffer();
            GBuffers?.Camera?.mCoreObject.UpdateConstBufferData(UEngine.Instance.GfxDevice.RenderContext.mCoreObject, 1);
        }
    }

    public class UMobileTranslucentNode : UMobileOpaqueNode
    {
        public UBasePassTranslucent mTranslucentShading;
        public override async System.Threading.Tasks.Task Initialize(IRenderPolicy policy, Shader.UShadingEnv shading, EPixelFormat rtFmt, EPixelFormat dsFmt, float x, float y, string debugName)
        {
            await Thread.AsyncDummyClass.DummyFunc();

            var rc = UEngine.Instance.GfxDevice.RenderContext;
            BasePass.Initialize(rc, debugName);

            var PassDesc = new IRenderPassDesc();
            unsafe
            {
                PassDesc.NumOfMRT = 1;
                PassDesc.AttachmentMRTs[0].IsSwapChain = 0;
                PassDesc.AttachmentMRTs[0].Format = rtFmt;
                PassDesc.AttachmentMRTs[0].Samples = 1;
                PassDesc.AttachmentMRTs[0].LoadAction = FrameBufferLoadAction.LoadActionDontCare;
                PassDesc.AttachmentMRTs[0].StoreAction = FrameBufferStoreAction.StoreActionStore;
                PassDesc.m_AttachmentDepthStencil.Format = dsFmt;
                PassDesc.m_AttachmentDepthStencil.Samples = 1;
                PassDesc.m_AttachmentDepthStencil.LoadAction = FrameBufferLoadAction.LoadActionDontCare;
                PassDesc.m_AttachmentDepthStencil.StoreAction = FrameBufferStoreAction.StoreActionStore;
                PassDesc.m_AttachmentDepthStencil.StencilLoadAction = FrameBufferLoadAction.LoadActionDontCare;
                PassDesc.m_AttachmentDepthStencil.StencilStoreAction = FrameBufferStoreAction.StoreActionStore;
                //PassDesc.mFBClearColorRT0 = new Color4(1, 0, 0, 0);
                //PassDesc.mDepthClearValue = 1.0f;
                //PassDesc.mStencilClearValue = 0u;
            }
            RenderPass = UEngine.Instance.GfxDevice.RenderPassManager.GetPipelineState<IRenderPassDesc>(rc, in PassDesc);

            var GizmosPassDesc = new IRenderPassDesc();
            unsafe
            {
                GizmosPassDesc.NumOfMRT = 1;
                GizmosPassDesc.AttachmentMRTs[0].IsSwapChain = 0;
                GizmosPassDesc.AttachmentMRTs[0].Format = rtFmt;
                GizmosPassDesc.AttachmentMRTs[0].Samples = 1;
                GizmosPassDesc.AttachmentMRTs[0].LoadAction = FrameBufferLoadAction.LoadActionDontCare;
                GizmosPassDesc.AttachmentMRTs[0].StoreAction = FrameBufferStoreAction.StoreActionStore;
                GizmosPassDesc.m_AttachmentDepthStencil.Format = dsFmt;
                GizmosPassDesc.m_AttachmentDepthStencil.Samples = 1;
                GizmosPassDesc.m_AttachmentDepthStencil.LoadAction = FrameBufferLoadAction.LoadActionClear;
                GizmosPassDesc.m_AttachmentDepthStencil.StoreAction = FrameBufferStoreAction.StoreActionStore;
                GizmosPassDesc.m_AttachmentDepthStencil.StencilLoadAction = FrameBufferLoadAction.LoadActionClear;
                GizmosPassDesc.m_AttachmentDepthStencil.StencilStoreAction = FrameBufferStoreAction.StoreActionStore;
                //GizmosPassDesc.mFBClearColorRT0 = new Color4(1, 0, 0, 0);
                //GizmosPassDesc.mDepthClearValue = 1.0f;
                //GizmosPassDesc.mStencilClearValue = 0u;
            }
            GizmosRenderPass = UEngine.Instance.GfxDevice.RenderPassManager.GetPipelineState<IRenderPassDesc>(rc, in GizmosPassDesc);

            var opaqueNode = policy.GetBasePassNode();

            GBuffers.Initialize(RenderPass, policy.Camera, 1, opaqueNode.GBuffers.DepthStencilView, opaqueNode.GBuffers.GetDepthStencilSRV(), (uint)x, (uint)y);
            GBuffers.SetGBuffer(0, opaqueNode.GBuffers.GetGBufferSRV(0));
            GBuffers.UpdateFrameBuffers(x, y);
            GBuffers.TargetViewIdentifier = opaqueNode.GBuffers.TargetViewIdentifier;
            
            GGizmosBuffers.Initialize(GizmosRenderPass, policy.Camera, 1, dsFmt, (uint)x, (uint)y);
            GGizmosBuffers.SetGBuffer(0, GBuffers.GetGBufferSRV(0), true);
            GGizmosBuffers.UpdateFrameBuffers(x, y);
            GGizmosBuffers.TargetViewIdentifier = GBuffers.TargetViewIdentifier;
            GGizmosBuffers.Camera = GBuffers.Camera;

            mOpaqueShading = UEngine.Instance.ShadingEnvManager.GetShadingEnv<UBasePassOpaque>();
            mTranslucentShading = UEngine.Instance.ShadingEnvManager.GetShadingEnv<UBasePassTranslucent>();
        }
        public override void Cleanup()
        {
            GBuffers?.Cleanup();
            GBuffers = null;

            GGizmosBuffers?.Cleanup();
            GGizmosBuffers = null;
        }
        public override void OnResize(IRenderPolicy policy, float x, float y)
        {
            var realGBuffer = policy.GetBasePassNode().GBuffers;
            if (GBuffers != null)
            {
                GBuffers.SetDepthStencilBuffer(realGBuffer.DepthStencilView, realGBuffer.GetDepthStencilSRV());
                GBuffers.SetGBuffer(0, realGBuffer.GetGBufferSRV(0), true);                
                GBuffers.OnResize(x, y);
            }
            if (GGizmosBuffers != null)
            {
                GGizmosBuffers.SetGBuffer(0, GBuffers.GetGBufferSRV(0), true);                
                GGizmosBuffers.OnResize(x, y);
            }
        }
        public override void TickLogic(GamePlay.UWorld world, IRenderPolicy policy, bool bClear)
        {
            var mobilePolicy = policy as UMobileFSPolicy;
            var cBuffer = GBuffers.PerViewportCBuffer;
            if (mobilePolicy != null)
            {
                if (cBuffer != null)
                    SetCBuffer(world, cBuffer, mobilePolicy);
            }

            BasePass.ClearMeshDrawPassArray();
            BasePass.SetViewport(GBuffers.ViewPort);

            foreach (var i in policy.VisibleMeshes)
            {
                if (i.Atoms == null)
                    continue;

                for (int j = 0; j < i.Atoms.Length; j++)
                {
                    var layer = i.Atoms[j].Material.RenderLayer;
                    if (layer == ERenderLayer.RL_Opaque)
                        continue;
                    var drawcall = i.GetDrawCall(GBuffers, j, policy, IRenderPolicy.EShadingType.BasePass, this);
                    if (drawcall != null)
                    {
                        drawcall.BindGBuffer(GBuffers);
                        //GGizmosBuffers.PerViewportCBuffer = GBuffers.PerViewportCBuffer;

                        BasePass.PushDrawCall(layer, drawcall);
                    }
                }
            }
            var passClears = new IRenderPassClears();
            passClears.SetDefault();
            passClears.SetClearColor(0, new Color4(1, 0, 0, 0));
            BasePass.BuildTranslucentRenderPass(in passClears, GBuffers.FrameBuffers, GGizmosBuffers.FrameBuffers);
        }
        public override void TickRender(IRenderPolicy policy)
        {
            var rc = UEngine.Instance.GfxDevice.RenderContext;
            BasePass.Commit(rc);
        }
        public override void TickSync(IRenderPolicy policy)
        {
            BasePass.SwapBuffer();
        }
    }
}

namespace EngineNS.UTest
{
    [UTest.UTest]
    public class UTest_ShadingEnv
    {
        public void UnitTestEntrance()
        {
            var env = UEngine.Instance.ShadingEnvManager.GetShadingEnv<Graphics.Pipeline.Mobile.UBasePassOpaque>();
            uint permuationId;
            var values = new List<string>();
            values.Add("1");
            values.Add("1");
            values.Add("0");
            values.Add("0");
            env.GetPermutation(values, out permuationId);
            UnitTestManager.TAssert(permuationId == 3, "");

            values.Clear();
            values.Add("0");
            values.Add("1");
            values.Add("0");
            values.Add("0");
            env.GetPermutation(values, out permuationId);
            UnitTestManager.TAssert(permuationId == 2, "");
        }
    }
}
