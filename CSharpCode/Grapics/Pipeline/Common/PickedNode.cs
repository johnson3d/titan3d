using EngineNS.Graphics.Mesh;
using EngineNS.Graphics.Pipeline.Shader;
using EngineNS.NxRHI;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Common
{
    public class TtPickSetupShading : Shader.TtGraphicsShadingEnv
    {
        public TtPickSetupShading()
        {
            CodeName = RName.GetRName("shaders/ShadingEnv/Sys/pick/pick_setup.cginc", RName.ERNameType.Engine);
        }
        public override NxRHI.EVertexStreamType[] GetNeedStreams()
        {
            return new NxRHI.EVertexStreamType[] { NxRHI.EVertexStreamType.VST_Position,
                NxRHI.EVertexStreamType.VST_Normal,
                NxRHI.EVertexStreamType.VST_Tangent,};
        }
        public unsafe override void OnDrawCall(ICommandList cmd, TtGraphicDraw drawcall, TtRenderPolicy policy, TtRenderMesh.TtAtom atom)
        {
            if (atom.Material.Blend.RenderTarget[0].BlendEnable == 1)
            {
                var mtl = atom.Material;
                NxRHI.FGpuPipelineDesc pipelineDesc = mtl.PipelineDesc;
                pipelineDesc.m_Blend.RenderTarget[0].BlendEnable = 0;
                var pipeline = TtEngine.Instance.GfxDevice.PipelineManager.GetPipelineState(TtEngine.Instance.GfxDevice.RenderContext, in pipelineDesc);
                drawcall.BindPipeline(pipeline);
            }
        }
    }
    [Bricks.CodeBuilder.ContextMenu("Picked", "Pick\\Picked", Bricks.RenderPolicyEditor.TtPolicyGraph.RGDEditorKeyword)]
    [Rtti.Meta("", NameAlias = new string[] { "EngineNS.Graphics.Pipeline.Common.UPickedNode@EngineCore", "EngineNS.Graphics.Pipeline.Common.UPickedNode" })]
    public class TtPickedNode : TAuxRenderGraphNode<TtPickedNode>
    {
        public TtRenderGraphPin PickedPinOut = TtRenderGraphPin.CreateOutput("Picked", false, EPixelFormat.PXF_R16G16_FLOAT, NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_RTV);
        public TtRenderGraphPin DepthPinOut = TtRenderGraphPin.CreateOutput("Depth", false, EPixelFormat.PXF_D16_UNORM, NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_DSV);
        public TtPickedNode()
        {
            Name = "PickedNode";
        }
        public override void InitNodePins()
        {
            AddOutput(PickedPinOut);
            AddOutput(DepthPinOut);
        }
        public override void OnResize(TtRenderPolicy policy, float x, float y)
        {
            float scaleFactor = 1.0f;
            var hitProxyNode = policy.FindFirstNode<TtHitproxyNode>();
            if (hitProxyNode != null)
            {
                scaleFactor = hitProxyNode.ScaleFactor;
            }

            PickedPinOut.Attachement.Width = (uint)(x * scaleFactor);
            PickedPinOut.Attachement.Height = (uint)(y * scaleFactor);

            DepthPinOut.Attachement.Width = (uint)(x * scaleFactor);
            DepthPinOut.Attachement.Height = (uint)(y * scaleFactor);

            if (PickedBuffer != null)
                PickedBuffer.SetSize(x * scaleFactor, y * scaleFactor);
        }
        public TtPickedProxiableManager PickedManager;
        public TtPickSetupShading PickedShading = null;
        public TtGraphicsBuffers PickedBuffer { get; protected set; } = new TtGraphicsBuffers();
        public NxRHI.TtRenderPass RenderPass;
        public override TtGraphicsShadingEnv GetPassShading(TtRenderMesh.TtAtom atom = null)
        {
            return PickedShading;
        }
        public async override Thread.Async.TtTask Initialize(TtRenderPolicy policy, string debugName)
        {
            await Thread.TtAsyncDummyClass.DummyFunc();
            PickedShading = await Graphics.Pipeline.Shader.TtShadingEnv.CreateShadingEnv<TtPickSetupShading>();

            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            
            var PassDesc = new NxRHI.FRenderPassDesc();
            unsafe
            {
                PassDesc.NumOfMRT = 1;
                PassDesc.AttachmentMRTs[0].Format = PickedPinOut.Attachement.Format;
                PassDesc.AttachmentMRTs[0].Samples = 1;
                PassDesc.AttachmentMRTs[0].LoadAction = NxRHI.EFrameBufferLoadAction.LoadActionClear;
                PassDesc.AttachmentMRTs[0].StoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
                PassDesc.m_AttachmentDepthStencil.Format = DepthPinOut.Attachement.Format;
                PassDesc.m_AttachmentDepthStencil.Samples = 1;
                PassDesc.m_AttachmentDepthStencil.LoadAction = NxRHI.EFrameBufferLoadAction.LoadActionClear;
                PassDesc.m_AttachmentDepthStencil.StoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
                PassDesc.m_AttachmentDepthStencil.StencilLoadAction = NxRHI.EFrameBufferLoadAction.LoadActionClear;
                PassDesc.m_AttachmentDepthStencil.StencilStoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
                //PassDesc.mFBClearColorRT0 = new Color4f(1, 0, 1, 0);
                //PassDesc.mDepthClearValue = 1.0f;
                //PassDesc.mStencilClearValue = 0u;
            }
            RenderPass = TtEngine.Instance.GfxDevice.RenderPassManager.GetPipelineState<NxRHI.FRenderPassDesc>(rc, in PassDesc); 

            PickedBuffer.Initialize(policy, RenderPass);
            PickedBuffer.SetRenderTarget(policy, 0, PickedPinOut);
            PickedBuffer.SetDepthStencil(policy, DepthPinOut);

            PickedBuffer.TargetViewIdentifier = new TtGraphicsBuffers.TtTargetViewIdentifier();// policy.DefaultCamera.TargetViewIdentifier;

            PickedManager = policy.PickedProxiableManager;
        }
        public override void Dispose()
        {
            PickedBuffer?.Dispose();
            PickedBuffer = null;

            base.Dispose();
        }
        List<Mesh.TtRenderMesh> mPickedMeshes = new List<Mesh.TtRenderMesh>();
        
        public override unsafe void Tick(GamePlay.TtWorld world, TtRenderPolicy policy, NxRHI.TtCommandList frameCmdList, bool bClear)
        {
            mPickedMeshes.Clear();
            policy.SetOptionData("PickedManager", PickedManager);
            if (PickedManager.PickedProxies.Count == 0)
            {
                return;
            }
            var cmdlist = TtEngine.Instance.GfxDevice.RenderContext.CmdListManager.GetCmdList();
            using (new NxRHI.TtCmdListScope(cmdlist, "Pick"))
            {
                cmdlist.SetViewport(in PickedBuffer.Viewport);
                var scissor = new NxRHI.FScissorRect();
                scissor.MinX = 0;
                scissor.MinY = 0;
                scissor.MaxX = (int)PickedBuffer.Viewport.Width;
                scissor.MaxY = (int)PickedBuffer.Viewport.Height;
                cmdlist.SetScissor(in scissor);
                var passClears = new NxRHI.FRenderPassClears();
                passClears.SetDefault();
                passClears.SetClearColor(0, new Color4f(1, 0, 1, 0));
                PickedBuffer.BuildFrameBuffers(policy);
                cmdlist.BeginPass(PickedBuffer.FrameBuffers, in passClears, "Picked");
                foreach (var i in PickedManager.PickedProxies)
                {
                    if (i is GamePlay.Scene.TtNode node && node.IsEditorVisibleInHierarchy == false)
                        continue;
                    i.GetHitProxyDrawMesh(mPickedMeshes);
                }
                foreach (var mesh in mPickedMeshes)
                {
                    if (mesh == null)
                        continue;
                    foreach (var i in mesh.SubMeshes)
                    {
                        foreach (var k in i.Atoms)
                        {
                            var drawcall = k.GetDrawCall(cmdlist.mCoreObject, PickedBuffer, policy, this);
                            if (drawcall != null)
                            {
                                if (PickedBuffer.PerViewportCBuffer != null)
                                    drawcall.BindCBV(drawcall.Effect.BindIndexer.cbPerViewport, PickedBuffer.PerViewportCBuffer);
                                if (policy.DefaultCamera.PerCameraCBuffer != null)
                                    drawcall.BindCBV(drawcall.Effect.BindIndexer.cbPerCamera, policy.DefaultCamera.PerCameraCBuffer);

                                cmdlist.PushGpuDraw(drawcall);
                            }
                        }
                    }
                }
                cmdlist.FlushDraws();
                cmdlist.EndPass();
            }

            policy.CommitCommandList(cmdlist, "Pick");
        }
    }
}
