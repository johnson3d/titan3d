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
        List<string> mMacroValues = new List<string>();
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
            drawcall.mCoreObject.BindSRVAll(index, Manager.GBuffers.GBufferSRV[0].mCoreObject);
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
        public override async System.Threading.Tasks.Task Initialize(IRenderPolicy policy, Shader.UShadingEnv shading, EPixelFormat rtFmt, EPixelFormat dsFmt, float x, float y)
        {
            await base.Initialize(policy, shading, rtFmt, dsFmt, x, y);

            OutputShaderResourceViews[0].SRV = GBuffers.GBufferSRV[0];
        }
    }
    public class UMobileEditorFSPolicy : UMobileFSPolicy
    {
        public override RHI.CShaderResourceView GetFinalShowRSV()
        {
            return EditorFinalNode.GBuffers.GBufferSRV[0];
        }

        #region GetHitproxy
        public Common.UPickedProxiableManager PickedProxiableManager { get; protected set; } = new Common.UPickedProxiableManager();
        public IProxiable GetHitproxy(UInt32 MouseX, UInt32 MouseY)
        {
            return HitproxyNode.GetHitproxy(MouseX, MouseY);
        }

        public UInt32 GetHitProxyID(UInt32 MouseX, UInt32 MouseY)
        {
            return HitproxyNode.GetHitProxyID(MouseX, MouseY);
        }
        #endregion

        public Shadow.UShadowMapNode mShadowMapNode = new Shadow.UShadowMapNode();
        public UEditorFinalNode EditorFinalNode = new UEditorFinalNode();
        public Common.UHitproxyNode HitproxyNode = new Common.UHitproxyNode();
        public Common.UPickedNode PickedNode = new Common.UPickedNode();
        public Common.UPickBlurNode PickBlurNode = new Common.UPickBlurNode();
        public Common.UPickHollowNode PickHollowNode = new Common.UPickHollowNode();

        public override async System.Threading.Tasks.Task Initialize(float x, float y)
        {
            await base.Initialize(x, y);

            await HitproxyNode.Initialize(this, x, y);

            PickedNode.PickedManager = PickedProxiableManager;
            await PickedNode.Initialize(this, x, y);

            await PickBlurNode.Initialize(this, UEngine.Instance.ShadingEnvManager.GetShadingEnv<Common.UPickBlurShading>(), EPixelFormat.PXF_R16G16_FLOAT, EPixelFormat.PXF_UNKNOWN, x, y);
            PickBlurNode.ScreenDrawPolicy.TagObject = this;

            await PickHollowNode.Initialize(this, UEngine.Instance.ShadingEnvManager.GetShadingEnv<Common.UPickHollowShading>(), EPixelFormat.PXF_R16G16_FLOAT, EPixelFormat.PXF_UNKNOWN, x, y);
            PickHollowNode.ScreenDrawPolicy.TagObject = this;

            await EditorFinalNode.Initialize(this, UEngine.Instance.ShadingEnvManager.GetShadingEnv<UEditorFinalShading>(), EPixelFormat.PXF_R8G8B8A8_UNORM, EPixelFormat.PXF_UNKNOWN, x, y);
            EditorFinalNode.ScreenDrawPolicy.TagObject = this;

            mShadowMapNode.Initialize(x, y);
        }
        public override void OnResize(float x, float y)
        {
            base.OnResize(x, y);

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

            HitproxyNode.Cleanup();

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
        public unsafe override void TickLogic()
        {
            var app = UEngine.Instance.GfxDevice.MainWindow as Graphics.Pipeline.USlateApplication;
            if (app != null)
            {
                mShadowMapNode.TickLogic(app.GetWorldViewportSlate().World, this, true);

                var cBuffer = GBuffers.PerViewportCBuffer;
                if (cBuffer != null)
                {
                    cBuffer.SetValue(cBuffer.PerViewportIndexer.gFadeParam, ref mShadowMapNode.mFadeParam);
                    cBuffer.SetValue(cBuffer.PerViewportIndexer.gShadowTransitionScale, ref mShadowMapNode.mShadowTransitionScale);
                    cBuffer.SetValue(cBuffer.PerViewportIndexer.gShadowMapSizeAndRcp, ref mShadowMapNode.mShadowMapSizeAndRcp);
                    cBuffer.SetValue(cBuffer.PerViewportIndexer.gViewer2ShadowMtx, ref mShadowMapNode.mViewer2ShadowMtx);
                    cBuffer.SetValue(cBuffer.PerViewportIndexer.gShadowDistance, ref mShadowMapNode.mShadowDistance);

                    var dirLight = app.GetWorldViewportSlate().World.DirectionLight;
                    //dirLight.mDirection = MathHelper.RandomDirection();
                    var dir = dirLight.mDirection;
                    var gDirLightDirection_Leak = new Vector4(dir.X, dir.Y, dir.Z, dirLight.mSunLightLeak);
                    cBuffer.SetValue(cBuffer.PerViewportIndexer.gDirLightDirection_Leak, ref gDirLightDirection_Leak);
                    var gDirLightColor_Intensity = new Vector4(dirLight.mSunLightColor.X, dirLight.mSunLightColor.Y, dirLight.mSunLightColor.Z, dirLight.mSunLightIntensity);
                    cBuffer.SetValue(cBuffer.PerViewportIndexer.gDirLightColor_Intensity, ref gDirLightColor_Intensity);

                    cBuffer.SetValue(cBuffer.PerViewportIndexer.mSkyLightColor, ref dirLight.mSkyLightColor);
                    cBuffer.SetValue(cBuffer.PerViewportIndexer.mGroundLightColor, ref dirLight.mGroundLightColor);
                }
            }

            BasePassNode.TickLogic(this);

            HitproxyNode.TickLogic(this);

            PickedNode?.TickLogic(this);

            PickBlurNode?.TickLogic();

            PickHollowNode?.TickLogic();

            EditorFinalNode?.TickLogic();
        }
        public unsafe override void TickRender()
        {
            var app = UEngine.Instance.GfxDevice.MainWindow as Graphics.Pipeline.USlateApplication;
            if (app != null)
            {
                mShadowMapNode.TickRender();
            }

            BasePassNode.TickRender(this);

            HitproxyNode.TickRender(this);

            PickedNode?.TickRender(this);
            
            PickBlurNode?.TickRender();

            PickHollowNode?.TickRender();

            EditorFinalNode?.TickRender();
        }
        public unsafe override void TickSync()
        {
            var app = UEngine.Instance.GfxDevice.MainWindow as Graphics.Pipeline.USlateApplication;
            if (app != null)
            {
                mShadowMapNode.TickSync();
            }

            BasePassNode.TickSync(this);

            HitproxyNode.TickSync(this);

            PickedNode?.TickSync(this);

            PickBlurNode?.TickSync();

            PickHollowNode?.TickSync();

            EditorFinalNode.TickSync();

            GBuffers?.Camera?.mCoreObject.UpdateConstBufferData(UEngine.Instance.GfxDevice.RenderContext.mCoreObject, 1);
        }
    }
}
