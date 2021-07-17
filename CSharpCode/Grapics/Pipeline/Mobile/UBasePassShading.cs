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
            disable_Shadow.Name = "ENV_DISABLE_SHADOW";
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
        List<string> mMacroValues = new List<string>();
        public RHI.CConstantBuffer PerShadingCBuffer;
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
            [RHI.CShaderProgram.ShaderVar(VarType = "CBuffer")]
            public uint cbPerShadingEnv;
            [RHI.CShaderProgram.ShaderVar(VarType = "Texture")]
            public uint gEnvMap;
            [RHI.CShaderProgram.ShaderVar(VarType = "Texture")]
            public uint gShadowMap;
            [RHI.CShaderProgram.ShaderVar(VarType = "Var", CBuffer = "cbPerShadingEnv")]
            public int gEnvMapMaxMipLevel;
            [RHI.CShaderProgram.ShaderVar(VarType = "Var", CBuffer = "cbPerShadingEnv")]
            public int gEyeEnvMapMaxMipLevel;
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
            var indexer = GetVarIndexer(drawcall);
            if (indexer.cbPerShadingEnv != 0xFFFFFFFF && PerShadingCBuffer == null)
            {
                PerShadingCBuffer = UEngine.Instance.GfxDevice.RenderContext.CreateConstantBuffer(drawcall.Effect.ShaderProgram, indexer.cbPerShadingEnv);
                float EnvMapMaxMipLevel = 1.0f;
                PerShadingCBuffer.SetValue(indexer.gEnvMapMaxMipLevel, ref EnvMapMaxMipLevel);
                float EyeEnvMapMaxMipLevel = 1.0f;
                PerShadingCBuffer.SetValue(indexer.gEyeEnvMapMaxMipLevel, ref EyeEnvMapMaxMipLevel);
            }
            if (PerShadingCBuffer != null)
                drawcall.mCoreObject.BindCBufferAll(indexer.cbPerShadingEnv, PerShadingCBuffer.mCoreObject);
        }
        public unsafe override void OnDrawCall(Pipeline.IRenderPolicy.EShadingType shadingType, RHI.CDrawCall drawcall, IRenderPolicy policy, Mesh.UMesh mesh)
        {
            base.OnDrawCall(shadingType, drawcall, policy, mesh);

            var indexer = GetVarIndexer(drawcall);

            var mobilePolicy = policy as Mobile.UMobileEditorFSPolicy;

            if (mobilePolicy.EnvMapSRV != null)
            {
                drawcall.mCoreObject.BindSRVAll(indexer.gEnvMap, mobilePolicy.EnvMapSRV.mCoreObject);

                if (PerShadingCBuffer != null)
                {
                    float gEnvMapMaxMipLevel = mobilePolicy.EnvMapSRV.PicDesc.MipLevel - 1;
                    PerShadingCBuffer.SetValue(indexer.gEnvMapMaxMipLevel, ref gEnvMapMaxMipLevel);
                }
            }

            if (mobilePolicy.mShadowMapNode.GBuffers.DepthStencilSRV != null)
            {
                drawcall.mCoreObject.BindSRVAll(indexer.gShadowMap, mobilePolicy.mShadowMapNode.GBuffers.DepthStencilSRV.mCoreObject);
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
    public class UMobileBasePassNode : Common.UBasePassNode
    {
        public UBasePassOpaque mBasePassShading;
        public UPassDrawBuffers BasePass = new UPassDrawBuffers();
        public RenderPassDesc PassDesc = new RenderPassDesc();
        
        public async System.Threading.Tasks.Task Initialize(IRenderPolicy policy, Shader.UShadingEnv shading, EPixelFormat rtFmt, EPixelFormat dsFmt, float x, float y)
        {
            await Thread.AsyncDummyClass.DummyFunc();

            var rc = UEngine.Instance.GfxDevice.RenderContext;
            BasePass.Initialize(rc);

            GBuffers.SwapChainIndex = -1;
            GBuffers.Initialize(1, EPixelFormat.PXF_D24_UNORM_S8_UINT, (uint)x, (uint)y);
            GBuffers.CreateGBuffer(0, EPixelFormat.PXF_R16G16B16A16_FLOAT, (uint)x, (uint)y);
            GBuffers.TargetViewIdentifier = new UGraphicsBuffers.UTargetViewIdentifier();

            PassDesc.mFBLoadAction_Color = FrameBufferLoadAction.LoadActionClear;
            PassDesc.mFBStoreAction_Color = FrameBufferStoreAction.StoreActionStore;
            PassDesc.mFBClearColorRT0 = new Color4(1, 0, 0, 0);
            PassDesc.mFBLoadAction_Depth = FrameBufferLoadAction.LoadActionClear;
            PassDesc.mFBStoreAction_Depth = FrameBufferStoreAction.StoreActionStore;
            PassDesc.mDepthClearValue = 1.0f;
            PassDesc.mFBLoadAction_Stencil = FrameBufferLoadAction.LoadActionClear;
            PassDesc.mFBStoreAction_Stencil = FrameBufferStoreAction.StoreActionStore;
            PassDesc.mStencilClearValue = 0u;

            mBasePassShading = UEngine.Instance.ShadingEnvManager.GetShadingEnv<Pipeline.Mobile.UBasePassOpaque>();
        }
        public virtual void Cleanup()
        {
            GBuffers?.Cleanup();
            GBuffers = null;
        }
        public virtual void OnResize(float x, float y)
        {
            if (GBuffers != null)
            {
                GBuffers.OnResize(x, y);
            }
        }
        public virtual void TickLogic(IRenderPolicy policy)
        {
            BasePass.ClearMeshDrawPassArray();
            BasePass.SetViewport(GBuffers.ViewPort);

            foreach (var i in policy.VisibleMeshes)
            {
                if (i.Atoms == null)
                    continue;

                if (i.HostNode != null)
                {
                    mBasePassShading.SetDisableShadow(!i.HostNode.IsAcceptShadow);
                }

                for (int j = 0; j < i.Atoms.Length; j++)
                {
                    var drawcall = i.GetDrawCall(GBuffers, j, policy, IRenderPolicy.EShadingType.BasePass);
                    if (drawcall != null)
                    {
                        GBuffers.SureCBuffer(drawcall.Effect, "UMobileEditorFSPolicy");
                        drawcall.BindGBuffer(GBuffers);

                        var layer = i.Atoms[j].Material.RenderLayer;
                        BasePass.PushDrawCall(layer, drawcall);
                    }
                }
            }

            BasePass.BuildRenderPass(ref PassDesc, GBuffers.FrameBuffers);
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
