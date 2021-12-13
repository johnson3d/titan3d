using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Deferred
{
    public class UOpaqueShading : Shader.UShadingEnv
    {
        public UOpaqueShading()
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

            mMacroValues.Add("1");//disable_AO = 0
            mMacroValues.Add("0");//disalbe_PointLights = 0
            mMacroValues.Add("0");//disalbe_Shadow = 0
            mMacroValues.Add("0");//mode_editor = 0

            UpdatePermutation(mMacroValues);
            CodeName = RName.GetRName("shaders/ShadingEnv/Mobile/MobileOpaque.cginc", RName.ERNameType.Engine);
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
    }
    public class UTranslucentShading : Shader.UShadingEnv
    {
        public UTranslucentShading()
        {
            CodeName = RName.GetRName("shaders/ShadingEnv/Deferred/DeferredTranslucent.cginc", RName.ERNameType.Engine);
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
    }
    public class UForwordNode : Common.UBasePassNode
    {
        public UOpaqueShading mOpaqueShading;
        public UTranslucentShading mTranslucentShading;
        public UPassDrawBuffers BasePass = new UPassDrawBuffers();
        public RHI.CRenderPass RenderPass;
        public RHI.CRenderPass GizmosRenderPass;

        public override async System.Threading.Tasks.Task Initialize(IRenderPolicy policy, Shader.UShadingEnv shading, EPixelFormat rtFmt, EPixelFormat dsFmt, float x, float y, string debugName)
        {
            await Thread.AsyncDummyClass.DummyFunc();

            var dfPolicy = policy as UDeferredPolicy;
            var rc = UEngine.Instance.GfxDevice.RenderContext;
            BasePass.Initialize(rc, "ForwordPass");

            var PassDesc = new IRenderPassDesc();
            unsafe
            {
                PassDesc.NumOfMRT = 1;
                PassDesc.AttachmentMRTs[0].Format = rtFmt;
                PassDesc.AttachmentMRTs[0].Samples = 1;
                PassDesc.AttachmentMRTs[0].LoadAction = FrameBufferLoadAction.LoadActionDontCare;
                PassDesc.AttachmentMRTs[0].StoreAction = FrameBufferStoreAction.StoreActionStore;
                PassDesc.m_AttachmentDepthStencil.Format = dfPolicy.BasePassNode.GBuffers.GetDepthStencilSRV().mCoreObject.GetFormat(); //dsFmt;
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

            var realGBuffer = dfPolicy.DirLightingNode.GBuffers;
            GBuffers.Initialize(RenderPass, dfPolicy.Camera, 1, dfPolicy.BasePassNode.GBuffers.DepthStencilView, dfPolicy.BasePassNode.GBuffers.GetDepthStencilSRV(), (uint)x, (uint)y);
            GBuffers.SetGBuffer(0, realGBuffer.GetGBufferSRV(0), true);
            GBuffers.TargetViewIdentifier = realGBuffer.TargetViewIdentifier;
            GBuffers.UpdateFrameBuffers(x, y);

            GGizmosBuffers.Initialize(GizmosRenderPass, policy.Camera, 1, dsFmt, (uint)x, (uint)y);
            GGizmosBuffers.SetGBuffer(0, GBuffers.GetGBufferSRV(0), true);
            GGizmosBuffers.TargetViewIdentifier = GBuffers.TargetViewIdentifier;
            GGizmosBuffers.Camera = GBuffers.Camera;
            GGizmosBuffers.UpdateFrameBuffers(x, y);

            mOpaqueShading = UEngine.Instance.ShadingEnvManager.GetShadingEnv<UOpaqueShading>();
            mTranslucentShading = UEngine.Instance.ShadingEnvManager.GetShadingEnv<UTranslucentShading>();
        }
        public virtual void Cleanup()
        {
            if (mOpaqueShading == null)
                return;
            GBuffers?.Cleanup();
            GBuffers = null;

            GGizmosBuffers?.Cleanup();
            GGizmosBuffers = null;
        }
        public override void OnResize(IRenderPolicy policy, float x, float y)
        {
            if (mOpaqueShading == null)
                return;
            var dfPolicy = policy as UDeferredPolicy;
            var realGBuffer = dfPolicy.DirLightingNode.GBuffers;
            if (GBuffers != null)
            {
                GBuffers.SetDepthStencilBuffer(dfPolicy.BasePassNode.GBuffers.DepthStencilView, dfPolicy.BasePassNode.GBuffers.GetDepthStencilSRV());
                GBuffers.SetGBuffer(0, realGBuffer.GetGBufferSRV(0), true);                
                GBuffers.OnResize(x, y);

                if (GGizmosBuffers != null)
                {
                    GGizmosBuffers.SetGBuffer(0, GBuffers.GetGBufferSRV(0), true);
                    GGizmosBuffers.OnResize(x, y);
                }
            }
        }
        public override void TickLogic(GamePlay.UWorld world, IRenderPolicy policy, bool bClear)
        {
            if (mOpaqueShading == null)
                return;
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

            var passClear = new IRenderPassClears();
            passClear.SetDefault();
            passClear.SetClearColor(0, new Color4(1, 0, 0, 0));
            BasePass.BuildRenderPass(in passClear, GBuffers.FrameBuffers, GGizmosBuffers.FrameBuffers);
        }
        public override void TickRender(IRenderPolicy policy)
        {
            if (mOpaqueShading == null)
                return;
            var rc = UEngine.Instance.GfxDevice.RenderContext;
            BasePass.Commit(rc);
        }
        public override void TickSync(IRenderPolicy policy)
        {
            if (mOpaqueShading == null)
                return;
            BasePass.SwapBuffer();
            //GBuffers?.Camera?.mCoreObject.UpdateConstBufferData(UEngine.Instance.GfxDevice.RenderContext.mCoreObject, 1);
        }
    }
}
