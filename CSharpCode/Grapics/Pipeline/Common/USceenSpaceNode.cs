using EngineNS.Bricks.VXGI;
using Org.BouncyCastle.Asn1.Mozilla;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Common
{
    [Rtti.Meta(NameAlias = new string[] { "EngineNS.Graphics.Pipeline.Common.USceenSpaceNode@EngineCore", "EngineNS.Graphics.Pipeline.Common.USceenSpaceNode" })]
    public class TtSceenSpaceNode : TtRenderGraphNode
    {
        public override void Dispose()
        {
            CoreSDK.DisposeObject(ref ScreenMesh);
            GBuffers?.Dispose();
            GBuffers = null;
            base.Dispose();
        }
        public TtRenderGraphPin ResultPinOut = TtRenderGraphPin.CreateOutput("Result", true, EPixelFormat.PXF_R8G8B8A8_UNORM);
        public TtSceenSpaceNode()
        {
            Name = "USceenSpaceNode";
        }
        public override void InitNodePins()
        {
            AddOutput(ResultPinOut, NxRHI.EBufferType.BFT_RTV | NxRHI.EBufferType.BFT_SRV);
        }
        [Rtti.Meta]
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

            BasePass.Initialize(rc, debugName + ".BasePass");
            DebugName = debugName;

            var materials = new Graphics.Pipeline.Shader.TtMaterial[1];
            materials[0] = TtEngine.Instance.GfxDevice.MaterialManager.ScreenMaterial;
            if (materials[0] == null)
                return;

            var mesh = new Graphics.Mesh.TtMesh();
            var rect = Graphics.Mesh.UMeshDataProvider.MakeRect2D(-1, -1, 2, 2, 0.5F, false);
            var rectMesh = rect.ToMesh();
            var ok = mesh.Initialize(rectMesh, materials, Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMesh>.TypeDesc);
            if (ok)
            {
                ScreenMesh = mesh;
            }
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
        public unsafe void ClearGBuffer(TtRenderPolicy policy)
        {
            var cmdlist = BasePass.DrawCmdList;
            using (new NxRHI.TtCmdListScope(cmdlist))
            {
                var passClears = new NxRHI.FRenderPassClears();
                passClears.SetDefault();
                passClears.SetClearColor(0, new Color4f(0, 0, 0, 0));
                GBuffers.BuildFrameBuffers(policy);
                cmdlist.BeginPass(GBuffers.FrameBuffers, in passClears, "ClearScreen");
                cmdlist.FlushDraws();
                cmdlist.EndPass();
            }
        }
        public override void FrameBuild(Graphics.Pipeline.TtRenderPolicy policy)
        {
            base.FrameBuild(policy);
        }
        [ThreadStatic]
        private static Profiler.TimeScope mScopeTick;
        private static Profiler.TimeScope ScopeTick
        {
            get
            {
                if (mScopeTick == null)
                    mScopeTick = new Profiler.TimeScope(typeof(TtSceenSpaceNode), nameof(TickLogic));
                return mScopeTick;
            }
        }
        public override unsafe void TickLogic(GamePlay.TtWorld world, TtRenderPolicy policy, bool bClear)
        {
            using (new Profiler.TimeScopeHelper(ScopeTick))
            {
                var cmdlist = BasePass.DrawCmdList;
                using (new NxRHI.TtCmdListScope(cmdlist))
                {
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
                                drawcall.BindCBuffer(drawcall.Effect.BindIndexer.cbPerViewport, GBuffers.PerViewportCBuffer);
                                drawcall.BindCBuffer(drawcall.Effect.BindIndexer.cbPerCamera, policy.DefaultCamera.PerCameraCBuffer);
                                cmdlist.PushGpuDraw(drawcall);
                            }
                        }
                    }
                    {
                        cmdlist.SetViewport(in GBuffers.Viewport);
                        cmdlist.SetScissor(0, (NxRHI.FScissorRect*)0);
                        var passClears = new NxRHI.FRenderPassClears();
                        passClears.SetDefault();
                        passClears.SetClearColor(0, new Color4f(0, 0, 0, 0));
                        GBuffers.BuildFrameBuffers(policy);
                        cmdlist.BeginPass(GBuffers.FrameBuffers, in passClears, DebugName);
                        cmdlist.FlushDraws();
                        cmdlist.EndPass();
                    }
                }
                policy.CommitCommandList(cmdlist);
            }   
        }
    }
}
