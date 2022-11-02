using EngineNS.Bricks.VXGI;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Common
{
    public class USceenSpaceNode : URenderGraphNode
    {
        public Common.URenderGraphPin ResultPinOut = Common.URenderGraphPin.CreateOutput("Result", true, EPixelFormat.PXF_R8G8B8A8_UNORM);
        public USceenSpaceNode()
        {
            Name = "USceenSpaceNode";
        }
        public override void InitNodePins()
        {
            AddOutput(ResultPinOut, NxRHI.EBufferType.BFT_RTV | NxRHI.EBufferType.BFT_SRV);
        }
        public Graphics.Mesh.UMesh ScreenMesh;
        public Shader.CommanShading.UBasePassPolicy ScreenDrawPolicy;
        public UGraphicsBuffers GBuffers { get; protected set; } = new UGraphicsBuffers();
        public UDrawBuffers BasePass = new UDrawBuffers();
        public NxRHI.URenderPass RenderPass;
        public unsafe override void Cleanup()
        {
            GBuffers?.Cleanup();
            GBuffers = null;

            base.Cleanup();
        }
        public string DebugName;
        public override async System.Threading.Tasks.Task Initialize(URenderPolicy policy, string debugName)
        {
            var rc = UEngine.Instance.GfxDevice.RenderContext;

            ScreenDrawPolicy = new Shader.CommanShading.UBasePassPolicy();
            await ScreenDrawPolicy.Initialize(null);
            //ScreenDrawPolicy.mBasePassShading = shading;
            ScreenDrawPolicy.TagObject = policy;

            var PassDesc = new NxRHI.FRenderPassDesc();
            unsafe
            {
                PassDesc.NumOfMRT = 1;
                PassDesc.AttachmentMRTs[0].Format = ResultPinOut.Attachement.Format;
                PassDesc.AttachmentMRTs[0].Samples = 1;
                PassDesc.AttachmentMRTs[0].LoadAction = NxRHI.EFrameBufferLoadAction.LoadActionClear;
                PassDesc.AttachmentMRTs[0].StoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
                //PassDesc.m_AttachmentDepthStencil.Format = dsFmt;
                //PassDesc.m_AttachmentDepthStencil.Samples = 1;
                //PassDesc.m_AttachmentDepthStencil.LoadAction = NxRHI.EFrameBufferLoadAction.LoadActionClear;
                //PassDesc.m_AttachmentDepthStencil.StoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
                //PassDesc.m_AttachmentDepthStencil.StencilLoadAction = NxRHI.EFrameBufferLoadAction.LoadActionClear;
                //PassDesc.m_AttachmentDepthStencil.StencilStoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
                //PassDesc.mFBClearColorRT0 = new Color4(0, 0, 0, 0);
                //PassDesc.mDepthClearValue = 1.0f;
                //PassDesc.mStencilClearValue = 0u;
            }

            RenderPass = UEngine.Instance.GfxDevice.RenderPassManager.GetPipelineState<NxRHI.FRenderPassDesc>(rc, in PassDesc); 

            GBuffers.Initialize(policy, RenderPass);
            GBuffers.SetRenderTarget(policy, 0, ResultPinOut);
            GBuffers.TargetViewIdentifier = new UGraphicsBuffers.UTargetViewIdentifier();

            BasePass.Initialize(rc, debugName);
            DebugName = debugName;

            var materials = new Graphics.Pipeline.Shader.UMaterial[1];
            materials[0] = UEngine.Instance.GfxDevice.MaterialManager.ScreenMaterial;
            if (materials[0] == null)
                return;

            var mesh = new Graphics.Mesh.UMesh();
            var rect = Graphics.Mesh.UMeshDataProvider.MakeRect2D(-1, -1, 2, 2, 0.5F, false);
            var rectMesh = rect.ToMesh();
            var ok = mesh.Initialize(rectMesh, materials, Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMesh>.TypeDesc);
            if (ok)
            {
                ScreenMesh = mesh;
            }
        }
        public override void OnResize(URenderPolicy policy, float x, float y)
        {
            if (GBuffers != null && RenderPass != null)
                GBuffers.OnResize(x, y);
        }
        public unsafe void ClearGBuffer(URenderPolicy policy)
        {
            var cmdlist = BasePass.DrawCmdList;
            if(cmdlist.BeginCommand())
            {
                var passClears = new NxRHI.FRenderPassClears();
                passClears.SetDefault();
                passClears.SetClearColor(0, new Color4(0, 0, 0, 0));
                GBuffers.BuildFrameBuffers(policy);
                cmdlist.BeginPass(GBuffers.FrameBuffers, in passClears, "ClearScreen");
                cmdlist.FlushDraws();
                cmdlist.EndPass();
                cmdlist.EndCommand();
            }
        }
        [ThreadStatic]
        private static Profiler.TimeScope ScopeTick = Profiler.TimeScopeManager.GetTimeScope(typeof(USceenSpaceNode), nameof(TickLogic));
        public override unsafe void TickLogic(GamePlay.UWorld world, URenderPolicy policy, bool bClear)
        {
            using (new Profiler.TimeScopeHelper(ScopeTick))
            {
                var cmdlist = BasePass.DrawCmdList;
                if (ScreenMesh != null)
                {
                    cmdlist.ResetGpuDraws();
                    for (int j = 0; j < ScreenMesh.Atoms.Length; j++)
                    {
                        var drawcall = ScreenMesh.GetDrawCall(GBuffers, j, ScreenDrawPolicy, Graphics.Pipeline.URenderPolicy.EShadingType.BasePass, this);
                        if (drawcall == null)
                            continue;
                        drawcall.BindCBuffer(drawcall.Effect.BindIndexer.cbPerViewport, GBuffers.PerViewportCBuffer);
                        drawcall.BindCBuffer(drawcall.Effect.BindIndexer.cbPerCamera, policy.DefaultCamera.PerCameraCBuffer);
                        cmdlist.PushGpuDraw(drawcall.mCoreObject);
                    }
                }
                if (cmdlist.BeginCommand())
                {
                    cmdlist.SetViewport(in GBuffers.Viewport);
                    var passClears = new NxRHI.FRenderPassClears();
                    passClears.SetDefault();
                    passClears.SetClearColor(0, new Color4(0, 0, 0, 0));
                    GBuffers.BuildFrameBuffers(policy);
                    cmdlist.BeginPass(GBuffers.FrameBuffers, in passClears, DebugName);
                    cmdlist.FlushDraws();
                    cmdlist.EndPass();
                    cmdlist.EndCommand();
                }
                UEngine.Instance.GfxDevice.RenderCmdQueue.QueueCmdlist(cmdlist);
            }   
        }
        public override unsafe void TickSync(URenderPolicy policy)
        {
            BasePass.SwapBuffer();
        }
    }
}
