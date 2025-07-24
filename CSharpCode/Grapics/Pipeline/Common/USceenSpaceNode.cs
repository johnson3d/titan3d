using EngineNS.Bricks.VXGI;
using Org.BouncyCastle.Asn1.Mozilla;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Common
{
    [Rtti.Meta("",NameAlias = new string[] { "EngineNS.Graphics.Pipeline.Common.USceenSpaceNode@EngineCore", "EngineNS.Graphics.Pipeline.Common.USceenSpaceNode" })]
    public abstract class TtSceenSpaceNode : TtRenderGraphNode
    {
        public override void Dispose()
        {
            CoreSDK.DisposeObject(ref ScreenMesh);
            GBuffers?.Dispose();
            GBuffers = null;
            base.Dispose();
        }
        public TtRenderGraphPin ResultPinOut = TtRenderGraphPin.CreateOutput("Result", true, EPixelFormat.PXF_R8G8B8A8_UNORM, NxRHI.EBufferType.BFT_RTV | NxRHI.EBufferType.BFT_SRV);
        public TtSceenSpaceNode()
        {
            Name = "USceenSpaceNode";
        }
        public override void InitNodePins()
        {
            AddOutput(ResultPinOut);
        }
        [Rtti.Meta("")]
        [Category("Option")]
        public float OutputScaleFactor { get; set; } = 1.0f;
        public Graphics.Mesh.TtMesh ScreenMesh;
        public TtGraphicsBuffers GBuffers { get; protected set; } = new TtGraphicsBuffers();
        public NxRHI.TtRenderPass RenderPass;
        public string DebugName;
        //public override Graphics.Pipeline.Shader.UGraphicsShadingEnv GetPassShading(URenderPolicy.EShadingType type = URenderPolicy.EShadingType.Count, Graphics.Mesh.TtMesh.TtAtom atom = null) abstract;
        public override async System.Threading.Tasks.Task Initialize(TtRenderPolicy policy, string debugName)
        {
            var rc = TtEngine.Instance.GfxDevice.RenderContext;

            CreateGBuffers(policy, ResultPinOut.Attachement.Format);

            DebugName = debugName;

            ScreenMesh = CreateScreenMesh();
            //var materials = new Graphics.Pipeline.Shader.TtMaterial[1];
            //materials[0] = TtEngine.Instance.GfxDevice.MaterialManager.ScreenMaterial;
            //if (materials[0] == null)
            //    return;

            //var mesh = new Graphics.Mesh.TtMesh();
            //var rect = Graphics.Mesh.TtMeshDataProvider.MakeRect2D(-1, -1, 2, 2, 0.5F, false);
            //var rectMesh = rect.ToMesh();
            //var ok = mesh.Initialize(rectMesh, materials, Rtti.TtTypeDescGetter<Graphics.Mesh.TtMdfStaticMesh>.TypeDesc);
            //if (ok)
            //{
            //    ScreenMesh = mesh;
            //}
        }
        public static Graphics.Mesh.TtMesh CreateScreenMesh()
        {
            var materials = new Graphics.Pipeline.Shader.TtMaterial[1];
            materials[0] = TtEngine.Instance.GfxDevice.MaterialManager.ScreenMaterial;
            if (materials[0] == null)
                return null;

            var mesh = new Graphics.Mesh.TtMesh();
            var rect = Graphics.Mesh.TtMeshDataProvider.MakeRect2D(-1, -1, 2, 2, 0.5F, false);
            var rectMesh = rect.ToMesh();
            var ok = mesh.Initialize(rectMesh, materials, Rtti.TtTypeDescGetter<Graphics.Mesh.TtMdfStaticMesh>.TypeDesc);
            if (ok)
            {
                return mesh;
            }
            return null;
        }
        public TtGraphicsBuffers.TtTargetViewIdentifier TargetViewId = new TtGraphicsBuffers.TtTargetViewIdentifier();
        public virtual unsafe TtGraphicsBuffers CreateGBuffers(TtRenderPolicy policy, EPixelFormat format)
        {
            var PassDesc = new NxRHI.FRenderPassDesc();

            PassDesc.NumOfMRT = 1;
            PassDesc.AttachmentMRTs[0].Format = format;
            PassDesc.AttachmentMRTs[0].Samples = 1;
            PassDesc.AttachmentMRTs[0].LoadAction = NxRHI.EFrameBufferLoadAction.LoadActionClear;
            PassDesc.AttachmentMRTs[0].StoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
            //PassDesc.m_AttachmentDepthStencil.Format = dsFmt;
            //PassDesc.m_AttachmentDepthStencil.Samples = 1;
            //PassDesc.m_AttachmentDepthStencil.LoadAction = NxRHI.EFrameBufferLoadAction.LoadActionClear;
            //PassDesc.m_AttachmentDepthStencil.StoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
            //PassDesc.m_AttachmentDepthStencil.StencilLoadAction = NxRHI.EFrameBufferLoadAction.LoadActionClear;
            //PassDesc.m_AttachmentDepthStencil.StencilStoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
            //PassDesc.mFBClearColorRT0 = new Color4f(0, 0, 0, 0);
            //PassDesc.mDepthClearValue = 1.0f;
            //PassDesc.mStencilClearValue = 0u;

            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            RenderPass = TtEngine.Instance.GfxDevice.RenderPassManager.GetPipelineState<NxRHI.FRenderPassDesc>(rc, in PassDesc);

            GBuffers.Initialize(policy, RenderPass);
            GBuffers.SetRenderTarget(policy, 0, ResultPinOut);
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
            }
        }
        public override void FrameBuild(Graphics.Pipeline.TtRenderPolicy policy)
        {
            base.FrameBuild(policy);
        }
        public override unsafe void TickLogic(GamePlay.TtWorld world, TtRenderPolicy policy, NxRHI.TtCommandList frameCmdList, bool bClear)
        {
            var cmdlist = TtEngine.Instance.GfxDevice.RenderContext.CmdListManager.GetCmdList();
            using (new NxRHI.TtCmdListScope(cmdlist))
            {
                cmdlist.SetViewport(in GBuffers.Viewport);
                var scissor = new NxRHI.FScissorRect();
                scissor.MinX = 0;
                scissor.MinY = 0;
                scissor.MaxX = (int)GBuffers.Viewport.Width;
                scissor.MaxY = (int)GBuffers.Viewport.Height;
                cmdlist.SetScissor(in scissor);
                var passClears = new NxRHI.FRenderPassClears();
                passClears.SetDefault();
                passClears.SetClearColor(0, new Color4f(0, 0, 0, 0));
                GBuffers.BuildFrameBuffers(policy);
                cmdlist.BeginPass(GBuffers.FrameBuffers, in passClears, DebugName);
                if (ScreenMesh != null)
                {
                    foreach (var i in ScreenMesh.SubMeshes)
                    {
                        foreach (var j in i.Atoms)
                        {
                            var drawcall = j.GetDrawCall(cmdlist.mCoreObject, GBuffers, policy, this);
                            if (drawcall == null)
                                continue;
                            drawcall.TagObject = this;
                            drawcall.BindCBV(drawcall.Effect.BindIndexer.cbPerViewport, GBuffers.PerViewportCBuffer);
                            drawcall.BindCBV(drawcall.Effect.BindIndexer.cbPerCamera, policy.DefaultCamera.PerCameraCBuffer);
                            cmdlist.PushGpuDraw(drawcall);
                        }
                    }
                }
                cmdlist.FlushDraws();
                cmdlist.EndPass();
            }
            policy.CommitCommandList(cmdlist);
        }
    }

    public abstract class TAuxSceenSpaceNode<T> : TtSceenSpaceNode
    {
        [ThreadStatic]
        public static Profiler.TimeScope mRDGTickLogicScope = null;
        public override ref Profiler.TimeScope GetThreadStaticRDGTickLogicScope()
        {
            return ref mRDGTickLogicScope;
        }
    }
}
