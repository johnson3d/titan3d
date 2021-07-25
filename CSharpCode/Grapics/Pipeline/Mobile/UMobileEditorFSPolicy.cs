using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Mobile
{
    public class UEditorFinalShading : Shader.UShadingEnv
    {
        public UEditorFinalShading()
        {
            CodeName = RName.GetRName("shaders/ShadingEnv/Mobile/MobileCopyEditor.cginc", RName.ERNameType.Engine);

            var disable_AO = new MacroDefine();//0
            disable_AO.Name = "ENV_DISABLE_AO";
            disable_AO.Values.Add("0");
            disable_AO.Values.Add("1");
            MacroDefines.Add(disable_AO);

            var disable_Sunshaft = new MacroDefine();//1
            disable_Sunshaft.Name = "ENV_DISABLE_SUNSHAFT";
            disable_Sunshaft.Values.Add("0");
            disable_Sunshaft.Values.Add("1");
            MacroDefines.Add(disable_Sunshaft);

            var disable_Bloom = new MacroDefine();//2
            disable_Bloom.Name = "ENV_DISABLE_BLOOM";
            disable_Bloom.Values.Add("0");
            disable_Bloom.Values.Add("1");
            MacroDefines.Add(disable_Bloom);

            var disable_Hdr = new MacroDefine();//2
            disable_Hdr.Name = "ENV_DISABLE_HDR";
            disable_Hdr.Values.Add("0");
            disable_Hdr.Values.Add("1");
            MacroDefines.Add(disable_Hdr);

            UpdatePermutationBitMask();

            mMacroValues.Add("0");//disable_AO = 0
            mMacroValues.Add("1");//disable_Sunshaft = 1
            mMacroValues.Add("1");//disable_Bloom = 1
            mMacroValues.Add("1");//disable_Hdr = 1

            UpdatePermutation(mMacroValues);
        }
        public void SetDisableAO(bool value)
        {
            mMacroValues[0] = value ? "1" : "0";
            UpdatePermutation(mMacroValues);
        }
        public void SetDisableSunShaft(bool value)
        {
            mMacroValues[1] = value ? "1" : "0";
            UpdatePermutation(mMacroValues);
        }
        public void SetDisableBloom(bool value)
        {
            mMacroValues[2] = value ? "1" : "0";
            UpdatePermutation(mMacroValues);
        }
        public void SetDisableHDR(bool value)
        {
            mMacroValues[3] = value ? "1" : "0";
            UpdatePermutation(mMacroValues);
        }
        public unsafe override void OnBuildDrawCall(RHI.CDrawCall drawcall)
        {
        }
        public unsafe override void OnDrawCall(Pipeline.IRenderPolicy.EShadingType shadingType, RHI.CDrawCall drawcall, IRenderPolicy policy, Mesh.UMesh mesh)
        {
            base.OnDrawCall(shadingType, drawcall, policy, mesh);

            //var Manager = policy.TagObject as UEditorFinalProcessor;
            var Manager = policy.TagObject as UMobileEditorFSPolicy;

            var gpuProgram = drawcall.Effect.ShaderProgram;
            var index = drawcall.mCoreObject.FindSRVIndex("gBaseSceneView");
            drawcall.mCoreObject.BindSRVAll(index, Manager.GetBasePassNode().GBuffers.GBufferSRV[0].mCoreObject);
            //drawcall.mCoreObject.BindSRVAll(index, Manager.InputShaderResourceViews[0].SRV.mCoreObject);

            index = drawcall.mCoreObject.FindSRVIndex("gPickedTex");
            drawcall.mCoreObject.BindSRVAll(index, Manager.PickHollowNode.GBuffers.GBufferSRV[0].mCoreObject);
            //drawcall.mCoreObject.BindSRVAll(index, Manager.InputShaderResourceViews[1].SRV.mCoreObject);
        }
    }
    public class UEditorFinalNode : Common.USceenSpaceNode
    {
        public UEditorFinalNode()
        {
            InputGpuBuffers = null;
            InputShaderResourceViews = new Common.URenderGraphSRV[2];
            InputShaderResourceViews[0] = new Common.URenderGraphSRV();
            InputShaderResourceViews[0].Name = "BaseSceneView";

            InputShaderResourceViews[1] = new Common.URenderGraphSRV();
            InputShaderResourceViews[1].Name = "PickedTex";

            OutputGpuBuffers = null;

            OutputShaderResourceViews = new Common.URenderGraphSRV[1];
            OutputShaderResourceViews[0] = new Common.URenderGraphSRV();
            OutputShaderResourceViews[0].Name = "Final";
        }
        public override async System.Threading.Tasks.Task Initialize(IRenderPolicy policy, Shader.UShadingEnv shading, EPixelFormat rtFmt, EPixelFormat dsFmt, float x, float y, string debugName)
        {
            await base.Initialize(policy, shading, rtFmt, dsFmt, x, y, debugName);

            OutputShaderResourceViews[0].SRV = GBuffers.GBufferSRV[0];
        }
    }
    public class UMobileEditorFSPolicy : UMobileFSPolicy
    {
        public override RHI.CShaderResourceView GetFinalShowRSV()
        {
            return EditorFinalNode.GBuffers.GBufferSRV[0];
        }
        public override bool DisableAO
        {
            get => mDisableAO;
            set
            {
                mDisableAO = value;
                var finalShading = EditorFinalNode.ScreenDrawPolicy.mBasePassShading as UEditorFinalShading;
                if (finalShading != null)
                {
                    finalShading.SetDisableAO(value);
                }
            }
        }
        #region GetHitproxy
        public IProxiable GetHitproxy(UInt32 MouseX, UInt32 MouseY)
        {
            return HitproxyNode.GetHitproxy(MouseX, MouseY);
        }

        public UInt32 GetHitProxyID(UInt32 MouseX, UInt32 MouseY)
        {
            return HitproxyNode.GetHitProxyID(MouseX, MouseY);
        }
        #endregion

        public UEditorFinalNode EditorFinalNode = new UEditorFinalNode();
        public Common.UHitproxyNode HitproxyNode = new Common.UHitproxyNode();
        public Common.UPickedNode PickedNode = new Common.UPickedNode();
        public Common.UPickBlurNode PickBlurNode = new Common.UPickBlurNode();
        public Common.UPickHollowNode PickHollowNode = new Common.UPickHollowNode();

        public override async System.Threading.Tasks.Task Initialize(float x, float y)
        {
            EnvMapSRV = await UEngine.Instance.GfxDevice.TextureManager.GetTexture(RName.GetRName("utest/texture/default_envmap.srv"));

            await BasePassNode.Initialize(this, UEngine.Instance.ShadingEnvManager.GetShadingEnv<Pipeline.Mobile.UBasePassOpaque>(), EPixelFormat.PXF_R16G16B16A16_FLOAT, EPixelFormat.PXF_D24_UNORM_S8_UINT, x, y);

            await HitproxyNode.Initialize(this, UEngine.Instance.ShadingEnvManager.GetShadingEnv<Common.UHitproxyShading>(), EPixelFormat.PXF_R8G8B8A8_UNORM, EPixelFormat.PXF_D24_UNORM_S8_UINT, x, y, "Hitproxy");

            await PickedNode.Initialize(this, UEngine.Instance.ShadingEnvManager.GetShadingEnv<Common.UPickSetupShading>(), EPixelFormat.PXF_R16G16_FLOAT, EPixelFormat.PXF_D24_UNORM_S8_UINT, x, y, "PickedNode");

            await PickBlurNode.Initialize(this, UEngine.Instance.ShadingEnvManager.GetShadingEnv<Common.UPickBlurShading>(), EPixelFormat.PXF_R16G16_FLOAT, EPixelFormat.PXF_UNKNOWN, x, y, "PickBlur");
            
            await PickHollowNode.Initialize(this, UEngine.Instance.ShadingEnvManager.GetShadingEnv<Common.UPickHollowShading>(), EPixelFormat.PXF_R16G16_FLOAT, EPixelFormat.PXF_UNKNOWN, x, y, "PickHollow");
            
            await EditorFinalNode.Initialize(this, UEngine.Instance.ShadingEnvManager.GetShadingEnv<UEditorFinalShading>(), EPixelFormat.PXF_R8G8B8A8_UNORM, EPixelFormat.PXF_UNKNOWN, x, y, "EditorFinal");
            
            await mShadowMapNode.Initialize(this, UEngine.Instance.ShadingEnvManager.GetShadingEnv<Shadow.UShadowShading>(), EPixelFormat.PXF_UNKNOWN, EPixelFormat.PXF_D16_UNORM, x, y, "ShadowDepth");
        }
        public override void OnResize(float x, float y)
        {
            BasePassNode.OnResize(x, y);

            HitproxyNode?.OnResize(x, y);

            PickedNode?.OnResize(x, y);

            PickBlurNode?.OnResize(x, y);

            PickHollowNode?.OnResize(x, y);

            EditorFinalNode?.OnResize(x, y);
        }
        public unsafe override void Cleanup()
        {
            mShadowMapNode?.Cleanup();
            mShadowMapNode = null;

            PickedNode?.Cleanup();
            PickedNode = null;

            PickBlurNode?.Cleanup();
            PickBlurNode = null;

            PickHollowNode?.Cleanup();
            PickHollowNode = null;

            EditorFinalNode?.Cleanup();
            EditorFinalNode = null;

            HitproxyNode?.Cleanup();
            HitproxyNode = null;

            base.Cleanup();
        }
        //Build DrawCall的时候调用，如果本渲染策略不提供指定的EShadingType，那么UAtom内的s对应的Drawcall就不会产生出来
        public override Shader.UShadingEnv GetPassShading(EShadingType type, Mesh.UMesh mesh, int atom)
        {
            switch (type)
            {
                case EShadingType.BasePass:
                    {
                        switch(mesh.Atoms[atom].Material.RenderLayer)
                        {
                            case ERenderLayer.RL_Opaque:
                                return BasePassNode.mBasePassShading;
                            case ERenderLayer.RL_Translucent:
                                return BasePassNode.mTranslucentShading;
                            default:
                                break;
                        }
                    }
                    return BasePassNode.mBasePassShading;
                case EShadingType.DepthPass:
                    return mShadowMapNode.mShadowShading;
                case EShadingType.HitproxyPass:
                    return HitproxyNode.mHitproxyShading;
                case EShadingType.Picked:
                    return PickedNode.PickedShading;
            }
            return null;
        }
        //渲染DrawCall的时候调用，如果产生了对应的ShadingType的Drawcall，则会callback到这里设置一些这个shading的特殊参数
        public override void OnDrawCall(Pipeline.IRenderPolicy.EShadingType shadingType, RHI.CDrawCall drawcall, Mesh.UMesh mesh, int atom)
        {
            //drawcall.Effect.ShadingEnv
            base.OnDrawCall(shadingType, drawcall, mesh, atom);
        }
        public unsafe override void TickLogic(GamePlay.UWorld world)
        {
            if (this.DisableShadow == false)
                mShadowMapNode?.TickLogic(world, this, true);

            BasePassNode?.TickLogic(world, this, true);

            HitproxyNode?.TickLogic(world, this, true);

            PickedNode?.TickLogic(world, this, true);

            PickBlurNode?.TickLogic(world, this, true);

            PickHollowNode?.TickLogic(world, this, true);

            EditorFinalNode?.TickLogic(world, this, true);
        }
        public unsafe override void TickRender()
        {
            if (this.DisableShadow == false)
                mShadowMapNode?.TickRender();

            BasePassNode?.TickRender(this);

            HitproxyNode?.TickRender(this);

            PickedNode?.TickRender(this);
            
            PickBlurNode?.TickRender();

            PickHollowNode?.TickRender();

            EditorFinalNode?.TickRender();
        }
        public unsafe override void TickSync()
        {
            if (this.DisableShadow == false)
                mShadowMapNode?.TickSync();

            BasePassNode?.TickSync(this);

            HitproxyNode?.TickSync(this);

            PickedNode?.TickSync(this);

            PickBlurNode?.TickSync();

            PickHollowNode?.TickSync();

            EditorFinalNode?.TickSync();
        }
    }
}
