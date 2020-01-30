using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.RenderPolicy
{
    public class CGfxFramePass
    {
        public CGfxFramePass(CRenderContext rc, string debugName)
        {
            mCmdList = new CCommandList[2];

            EngineNS.CCommandListDesc CmdListDesc = new EngineNS.CCommandListDesc();
            mCmdList[0] = rc.CreateCommandList(CmdListDesc);
            mCmdList[1] = rc.CreateCommandList(CmdListDesc);

            if (!string.IsNullOrEmpty(debugName))
            {
                mCmdList[0].DebugName = debugName;
                mCmdList[1].DebugName = debugName;
            }
        }
        ~CGfxFramePass()
        {
            Cleanup();
        }
        public void Cleanup()
        {
            if (mCmdList != null)
            {
                foreach (var i in mCmdList)
                {
                    i.Cleanup();
                }
                mCmdList = null;
            }
            mBaseSceneView = null;
        }
        private CCommandList[] mCmdList = null;
        public CCommandList CommitingCMDs
        {
            get
            {
                if (mCmdList == null)
                    return null;
                return mCmdList[0];
            }
        }
        public CCommandList RenderingCMDs
        {
            get
            {
                if (mCmdList == null)
                    return null;
                return mCmdList[1];
            }
        }
        public CRenderPassDesc mRenderPassDesc = new CRenderPassDesc();
        public View.CGfxSceneView mBaseSceneView;

        private class RLayerParameter
        {
            public View.ERenderLayer Layer;
            public PrebuildPassIndex PassIndex;
            public CGfxShadingEnv ShadingEnv;
        }
        public void RemoveLayer(View.ERenderLayer layer)
        {
            mLayerParameter.Remove(layer);
        }
        public void SetRLayerParameter(View.ERenderLayer layer, PrebuildPassIndex passIndex, CGfxShadingEnv shadingEnv)
        {
            RLayerParameter parameter;
            if (mLayerParameter.TryGetValue(layer, out parameter)==false)
            {
                parameter = new RLayerParameter();
                mLayerParameter.Add(layer, parameter);
            }
            parameter.Layer = layer;
            parameter.PassIndex = passIndex;
            parameter.ShadingEnv = shadingEnv;
        }
        public void SetPassBuiltCallBack(CCommandList.FOnPassBuilt fun)
        {
            if(mCmdList!=null)
            {
                mCmdList[0].SetPassBuiltCallBack(fun);
                mCmdList[1].SetPassBuiltCallBack(fun);
            }
        }
        public void SetGraphicsProfiler(CGraphicsProfiler profiler)
        {
            if (mCmdList != null)
            {
                mCmdList[0].SetGraphicsProfiler(profiler);
                mCmdList[1].SetGraphicsProfiler(profiler);
            }
        }
        private Dictionary<View.ERenderLayer, RLayerParameter> mLayerParameter = new Dictionary<View.ERenderLayer, RLayerParameter>();
        public CPass TickLogic(CGfxCamera Camera, View.CGfxSceneView view, CRenderContext rc, int DPLimitter, bool GraphicsDebug)
        {
            var CmdList = CommitingCMDs;
            if (CmdList == null)
                return null;
            using (var i = mLayerParameter.GetEnumerator())
            {
                while(i.MoveNext())
                {
                    var parameter = i.Current.Value;
                    mBaseSceneView.CookSpecRenderLayerDataToPass(rc, parameter.Layer, Camera, parameter.ShadingEnv, parameter.PassIndex);
                    mBaseSceneView.PushSpecRenderLayerDataToRHI(CmdList, parameter.Layer);
                }
            }

            CmdList.BeginCommand();
            CmdList.BeginRenderPass(mRenderPassDesc, mBaseSceneView.FrameBuffer);

            if (BeforeBuildRenderPass != null)
                BeforeBuildRenderPass(Camera, view, rc, CmdList, DPLimitter, GraphicsDebug);

            var LatestPass = CmdList.BuildRenderPass(DPLimitter, GraphicsDebug);

            if (AfterBuildRenderPass != null)
                AfterBuildRenderPass(Camera, view, rc, CmdList, DPLimitter, GraphicsDebug);

            CmdList.EndRenderPass();
            CmdList.EndCommand();

            return LatestPass;
        }
        public delegate void FBeforeBuildRenderPass(CGfxCamera Camera, View.CGfxSceneView view, CRenderContext rc, CCommandList cmd, int DPLimitter, bool GraphicsDebug);
        public delegate void FAfterBuildRenderPass(CGfxCamera Camera, View.CGfxSceneView view, CRenderContext rc, CCommandList cmd, int DPLimitter, bool GraphicsDebug);
        public FBeforeBuildRenderPass BeforeBuildRenderPass;
        public FAfterBuildRenderPass AfterBuildRenderPass;
        public void TickRender(CRenderContext RHICtx)
        {
            RenderingCMDs?.Commit(RHICtx);
        }
        public void TickSync()
        {
            if (mCmdList == null)
                return;

            var Temp = mCmdList[0];
            mCmdList[0] = mCmdList[1];
            mCmdList[1] = Temp;
        }
    }

    public class CGfxPostprocessPass
    {
        ~CGfxPostprocessPass()
        {
            Cleanup();
        }
        public void Cleanup()
        {
            if (mCmdList != null)
            {
                foreach (var i in mCmdList)
                {
                    i.Cleanup();
                }
                mCmdList = null;
            }
            mScreenView = null;
        }
        public View.CGfxScreenView mScreenView;
        public CCommandList[] mCmdList;
        public CCommandList CommitingCMDs
        {
            get
            {
                if (mCmdList == null)
                    return null;
                return mCmdList[0];
            }
        }
        public CCommandList RenderingCMDs
        {
            get
            {
                if (mCmdList == null)
                    return null;
                return mCmdList[1];
            }
        }
        public CRenderPassDesc mRenderPassDesc;
        private Mesh.CGfxMesh mScreenAlignedTriangle;
        private CGfxShadingEnv mShadingEnv;
        public async System.Threading.Tasks.Task<bool> Init(CRenderContext rc, CSwapChain SwapChain,
            View.CGfxScreenViewDesc ViewInfo, CGfxShadingEnv ShadingEnv, RName MtlInst, string debugName)
        {
            mCmdList = new CCommandList[2];

            EngineNS.CCommandListDesc CmdListDesc = new EngineNS.CCommandListDesc();
            mCmdList[0] = rc.CreateCommandList(CmdListDesc);
            mCmdList[1] = rc.CreateCommandList(CmdListDesc);
            mCmdList[0].DebugName = debugName;
            mCmdList[1].DebugName = debugName;

            var ScreenAlignedTriangle = CEngine.Instance.MeshPrimitivesManager.GetMeshPrimitives(rc, CEngineDesc.ScreenAlignedTriangleName, true);
            mScreenAlignedTriangle = CEngine.Instance.MeshManager.CreateMesh(rc, ScreenAlignedTriangle);
            var mtl = await CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, MtlInst);
            mScreenAlignedTriangle.SetMaterialInstance(rc, 0,
                mtl,
                CEngine.Instance.PrebuildPassData.DefaultShadingEnvs);
            //await mScreenAlignedTriangle.AwaitEffects();

            mShadingEnv = ShadingEnv;
            mScreenView = new View.CGfxScreenView();
            return await mScreenView.Init(rc, SwapChain, ViewInfo, ShadingEnv, mtl, mScreenAlignedTriangle);
        }
        public async System.Threading.Tasks.Task<bool> Init2(CRenderContext rc, CGfxShadingEnv ShadingEnv, RName MtlInst, View.CGfxScreenView view)
        {
            mCmdList = new CCommandList[2];

            EngineNS.CCommandListDesc CmdListDesc = new EngineNS.CCommandListDesc();
            mCmdList[0] = rc.CreateCommandList(CmdListDesc);
            mCmdList[1] = rc.CreateCommandList(CmdListDesc);

            var ScreenAlignedTriangle = CEngine.Instance.MeshPrimitivesManager.GetMeshPrimitives(rc, CEngineDesc.ScreenAlignedTriangleName, true);
            mScreenAlignedTriangle = CEngine.Instance.MeshManager.CreateMesh(rc, ScreenAlignedTriangle);
            var mtl = await CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, MtlInst);
            mScreenAlignedTriangle.SetMaterialInstance(rc, 0,
                mtl,
                CEngine.Instance.PrebuildPassData.DefaultShadingEnvs);

            mShadingEnv = ShadingEnv;
            mScreenView = view;
            return true;
        }
        public void TickLogic(CGfxCamera Camera, View.CGfxSceneView view, CRenderContext RHICtx)
        {
            var CmdList = CommitingCMDs;
            if (mScreenAlignedTriangle != null)
            {
                mScreenView.CookViewportMeshToPass(RHICtx, mShadingEnv, Camera, mScreenAlignedTriangle);
                mScreenView.PushPassToRHI(CmdList);
            }
        }
        public void TickRender(CRenderContext RHICtx)
        {
            var CmdList = RenderingCMDs;
            CmdList.BeginCommand();
            CmdList.BeginRenderPass(mRenderPassDesc, mScreenView.FrameBuffer);
            CmdList.BuildRenderPass();
            CmdList.EndRenderPass();
            CmdList.EndCommand();
            CmdList.Commit(RHICtx);
        }
        public void TickSync()
        {
            if (mCmdList == null)
                return;

            var Temp = mCmdList[0];
            mCmdList[0] = mCmdList[1];
            mCmdList[1] = Temp;
        }
        public void SetPassBuiltCallBack(CCommandList.FOnPassBuilt fun)
        {
            if (mCmdList != null)
            {
                mCmdList[0].SetPassBuiltCallBack(fun);
                mCmdList[1].SetPassBuiltCallBack(fun);
            }
        }
        public void SetGraphicsProfiler(CGraphicsProfiler profiler)
        {
            if (mCmdList != null)
            {
                mCmdList[0].SetGraphicsProfiler(profiler);
                mCmdList[1].SetGraphicsProfiler(profiler);
            }
        }
    }
}
