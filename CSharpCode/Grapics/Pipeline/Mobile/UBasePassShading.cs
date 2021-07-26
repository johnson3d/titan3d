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
        public unsafe override void OnBuildDrawCall(RHI.CDrawCall drawcall)
        {
        }
        public unsafe override void OnDrawCall(Pipeline.IRenderPolicy.EShadingType shadingType, RHI.CDrawCall drawcall, IRenderPolicy policy, Mesh.UMesh mesh)
        {
            base.OnDrawCall(shadingType, drawcall, policy, mesh);

            var mobilePolicy = policy as Mobile.UMobileFSPolicy;
            if (mobilePolicy != null)
            {
                var indexer = GetVarIndexer(drawcall);
                if (mobilePolicy.EnvMapSRV != null)
                {
                    drawcall.mCoreObject.BindSRVAll(indexer.gEnvMap, mobilePolicy.EnvMapSRV.mCoreObject);
                }

                if (mobilePolicy.mShadowMapNode != null && mobilePolicy.mShadowMapNode.GBuffers.DepthStencilSRV != null)
                {
                    drawcall.mCoreObject.BindSRVAll(indexer.gShadowMap, mobilePolicy.mShadowMapNode.GBuffers.DepthStencilSRV.mCoreObject);
                }
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
    public class UMobileBasePassNode : Common.UBasePassNode
    {
        public UBasePassOpaque mBasePassShading;
        public UBasePassTranslucent mTranslucentShading;
        public UPassDrawBuffers BasePass = new UPassDrawBuffers();
        public RenderPassDesc PassDesc = new RenderPassDesc();
        public RenderPassDesc GizmosPassDesc = new RenderPassDesc();

        public async System.Threading.Tasks.Task Initialize(IRenderPolicy policy, Shader.UShadingEnv shading, EPixelFormat rtFmt, EPixelFormat dsFmt, float x, float y)
        {
            await Thread.AsyncDummyClass.DummyFunc();

            var rc = UEngine.Instance.GfxDevice.RenderContext;
            BasePass.Initialize(rc, "BasePass");

            GBuffers.SwapChainIndex = -1;
            GBuffers.Initialize(1, dsFmt, (uint)x, (uint)y);
            GBuffers.CreateGBuffer(0, rtFmt, (uint)x, (uint)y);
            GBuffers.TargetViewIdentifier = new UGraphicsBuffers.UTargetViewIdentifier();

            GGizmosBuffers.SwapChainIndex = -1;
            GGizmosBuffers.Initialize(1, dsFmt, (uint)x, (uint)y);
            GGizmosBuffers.SetGBuffer(0, GBuffers.GBufferSRV[0], true);
            GGizmosBuffers.TargetViewIdentifier = GBuffers.TargetViewIdentifier;
            GGizmosBuffers.Camera = GBuffers.Camera;

            PassDesc.mFBLoadAction_Color = FrameBufferLoadAction.LoadActionClear;
            PassDesc.mFBStoreAction_Color = FrameBufferStoreAction.StoreActionStore;
            PassDesc.mFBClearColorRT0 = new Color4(1, 0, 0, 0);
            PassDesc.mFBLoadAction_Depth = FrameBufferLoadAction.LoadActionClear;
            PassDesc.mFBStoreAction_Depth = FrameBufferStoreAction.StoreActionStore;
            PassDesc.mDepthClearValue = 1.0f;
            PassDesc.mFBLoadAction_Stencil = FrameBufferLoadAction.LoadActionClear;
            PassDesc.mFBStoreAction_Stencil = FrameBufferStoreAction.StoreActionStore;
            PassDesc.mStencilClearValue = 0u;

            GizmosPassDesc.mFBLoadAction_Color = FrameBufferLoadAction.LoadActionDontCare;
            GizmosPassDesc.mFBStoreAction_Color = FrameBufferStoreAction.StoreActionDontCare;
            GizmosPassDesc.mFBClearColorRT0 = new Color4(1, 0, 0, 0);
            GizmosPassDesc.mFBLoadAction_Depth = FrameBufferLoadAction.LoadActionClear;
            GizmosPassDesc.mFBStoreAction_Depth = FrameBufferStoreAction.StoreActionStore;
            GizmosPassDesc.mDepthClearValue = 1.0f;
            GizmosPassDesc.mFBLoadAction_Stencil = FrameBufferLoadAction.LoadActionClear;
            GizmosPassDesc.mFBStoreAction_Stencil = FrameBufferStoreAction.StoreActionStore;
            GizmosPassDesc.mStencilClearValue = 0u;

            mBasePassShading = shading as Pipeline.Mobile.UBasePassOpaque;
            mTranslucentShading = UEngine.Instance.ShadingEnvManager.GetShadingEnv<Pipeline.Mobile.UBasePassTranslucent>();
        }
        public virtual void Cleanup()
        {
            GBuffers?.Cleanup();
            GBuffers = null;

            GGizmosBuffers?.Cleanup();
            GGizmosBuffers = null;
        }
        public virtual void OnResize(float x, float y)
        {
            if (GBuffers != null)
            {
                GBuffers.OnResize(x, y);
            }
            if (GGizmosBuffers != null)
            {
                GGizmosBuffers.SetGBuffer(0, GBuffers.GBufferSRV[0], true);
                GGizmosBuffers.OnResize(x, y);
            }
        }
        private void SetCBuffer(GamePlay.UWorld world, RHI.CConstantBuffer cBuffer, UMobileFSPolicy mobilePolicy)
        {
            cBuffer.SetValue(cBuffer.PerViewportIndexer.gFadeParam, ref mobilePolicy.mShadowMapNode.mFadeParam);
            cBuffer.SetValue(cBuffer.PerViewportIndexer.gShadowTransitionScale, ref mobilePolicy.mShadowMapNode.mShadowTransitionScale);
            cBuffer.SetValue(cBuffer.PerViewportIndexer.gShadowMapSizeAndRcp, ref mobilePolicy.mShadowMapNode.mShadowMapSizeAndRcp);
            cBuffer.SetValue(cBuffer.PerViewportIndexer.gViewer2ShadowMtx, ref mobilePolicy.mShadowMapNode.mViewer2ShadowMtx);
            cBuffer.SetValue(cBuffer.PerViewportIndexer.gShadowDistance, ref mobilePolicy.mShadowMapNode.mShadowDistance);

            var dirLight = world.DirectionLight;
            //dirLight.mDirection = MathHelper.RandomDirection();
            var dir = dirLight.mDirection;
            var gDirLightDirection_Leak = new Vector4(dir.X, dir.Y, dir.Z, dirLight.mSunLightLeak);
            cBuffer.SetValue(cBuffer.PerViewportIndexer.gDirLightDirection_Leak, ref gDirLightDirection_Leak);
            var gDirLightColor_Intensity = new Vector4(dirLight.mSunLightColor.X, dirLight.mSunLightColor.Y, dirLight.mSunLightColor.Z, dirLight.mSunLightIntensity);
            cBuffer.SetValue(cBuffer.PerViewportIndexer.gDirLightColor_Intensity, ref gDirLightColor_Intensity);

            cBuffer.SetValue(cBuffer.PerViewportIndexer.mSkyLightColor, ref dirLight.mSkyLightColor);
            cBuffer.SetValue(cBuffer.PerViewportIndexer.mGroundLightColor, ref dirLight.mGroundLightColor);

            float EnvMapMaxMipLevel = 1.0f;
            cBuffer.SetValue(cBuffer.PerViewportIndexer.gEnvMapMaxMipLevel, ref EnvMapMaxMipLevel);
            cBuffer.SetValue(cBuffer.PerViewportIndexer.gEyeEnvMapMaxMipLevel, ref EnvMapMaxMipLevel);
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
                    var drawcall = i.GetDrawCall(GBuffers, j, policy, IRenderPolicy.EShadingType.BasePass);
                    if (drawcall != null)
                    {
                        GBuffers.SureCBuffer(drawcall.Effect, "UMobileEditorFSPolicy");
                        drawcall.BindGBuffer(GBuffers);
                        //GGizmosBuffers.PerViewportCBuffer = GBuffers.PerViewportCBuffer;

                        var layer = i.Atoms[j].Material.RenderLayer;
                        BasePass.PushDrawCall(layer, drawcall);
                    }
                }
            }

            BasePass.BuildRenderPass(ref PassDesc, GBuffers.FrameBuffers, ref GizmosPassDesc, GGizmosBuffers.FrameBuffers);
        }
        public virtual void TickRender(IRenderPolicy policy)
        {
            var rc = UEngine.Instance.GfxDevice.RenderContext;
            BasePass.Commit(rc);
        }
        public virtual void TickSync(IRenderPolicy policy)
        {
            BasePass.SwapBuffer();
            GBuffers?.Camera?.mCoreObject.UpdateConstBufferData(UEngine.Instance.GfxDevice.RenderContext.mCoreObject, 1);
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
