using EngineNS.Graphics.Mesh;
using EngineNS.Graphics.Pipeline.Common;
using EngineNS.Graphics.Pipeline.Shader;
using EngineNS.NxRHI;
using System;
using System.ComponentModel;

namespace EngineNS.Graphics.Pipeline.Deferred
{
    public class TtSSSBlurShading : Shader.TtGraphicsShadingEnv
    {
        public TtSSSBlurShading()
        {
            CodeName = RName.GetRName("shaders/ShadingEnv/Deferred/SSSBlur.cginc", RName.ERNameType.Engine);
            this.UpdatePermutation().AddWaitTask();
        }
        public override NxRHI.EVertexStreamType[] GetNeedStreams()
        {
            return new NxRHI.EVertexStreamType[] {
                NxRHI.EVertexStreamType.VST_Position,
                NxRHI.EVertexStreamType.VST_UV,
            };
        }
        public override EPixelShaderInput[] GetPSNeedInputs()
        {
            return new EPixelShaderInput[] {
                EPixelShaderInput.PST_Position,
                EPixelShaderInput.PST_UV,
            };
        }
        public override void OnDrawCall(NxRHI.ICommandList cmd, NxRHI.TtGraphicDraw drawcall, TtRenderPolicy policy, TtRenderMesh.TtAtom atom)
        {
            var node = drawcall.TagObject as TtSSSBlurNode;
            if (node != null)
            {
                node.OnDrawCall(this, cmd, drawcall, policy, atom);
            }
        }
    }

    /// <summary>
    /// Separable SSS blur node: performs horizontal blur then vertical blur + specular compose.
    /// Inputs: DirLighting diffuse (Color), DirLighting separated specular (Specular), GBuffer RT3 (for ShadingMode mask).
    /// Output: final composited color (blur(diffuse) + specular).
    /// </summary>
    [Bricks.CodeBuilder.ContextMenu("SSSBlur", "Deferred\\SSSBlur", Bricks.RenderPolicyEditor.TtPolicyGraph.RGDEditorKeyword)]
    [Rtti.Meta("")]
    public partial class TtSSSBlurNode : TAuxSceenSpaceNode<TtSSSBlurNode>
    {
        public TtRenderGraphPin ColorPinIn = TtRenderGraphPin.CreateInput("Color", NxRHI.EBufferType.BFT_SRV);
        public TtRenderGraphPin SpecularPinIn = TtRenderGraphPin.CreateInput("Specular", NxRHI.EBufferType.BFT_SRV);
        public TtRenderGraphPin GBufferRT0PinIn = TtRenderGraphPin.CreateInput("GBufferRT0", NxRHI.EBufferType.BFT_SRV);
        public TtRenderGraphPin GBufferRT3PinIn = TtRenderGraphPin.CreateInput("GBufferRT3", NxRHI.EBufferType.BFT_SRV);

        public NxRHI.TtCbView CBSSSBlur;

        [Category("SSS")]
        [Rtti.Meta("")]
        public float SSSWidth { get; set; } = 1.0f;

        [Browsable(false)]
        public TtDeferredDirLightingNode DirLightingNode { get; private set; }

        private int mCurrentPassIndex;
        private NxRHI.TtSrView mIntermediateSrv;
        private TtRenderGraphPin mHorizontalPinOut = TtRenderGraphPin.CreateOutput("HorizontalBlur", true, EPixelFormat.PXF_R16G16B16A16_FLOAT, NxRHI.EBufferType.BFT_RTV | NxRHI.EBufferType.BFT_SRV);
        private TtGraphicsBuffers mHorizontalGBuffers = new TtGraphicsBuffers();
        private NxRHI.TtRenderPass mHorizontalRenderPass;

        public TtSSSBlurNode()
        {
            Name = "SSSBlurNode";
        }
        public override void Dispose()
        {
            CoreSDK.DisposeObject(ref CBSSSBlur);
            mHorizontalGBuffers?.Dispose();
            mHorizontalGBuffers = null;
            base.Dispose();
        }
        public override void InitNodePins()
        {
            ResultPinOut.Attachement.Format = EPixelFormat.PXF_R16G16B16A16_FLOAT;
            base.InitNodePins();

            AddInput(ColorPinIn);
            AddInput(SpecularPinIn);
            AddInput(GBufferRT0PinIn);
            AddInput(GBufferRT3PinIn);
        }
        public TtSSSBlurShading mSSSBlurShading;
        public override TtGraphicsShadingEnv GetPassShading(TtRenderMesh.TtAtom atom = null)
        {
            return mSSSBlurShading;
        }
        public override async Thread.Async.TtTask Initialize(TtRenderPolicy policy, string debugName)
        {
            await base.Initialize(policy, debugName);
            mSSSBlurShading = await TtShadingEnv.CreateShadingEnv<TtSSSBlurShading>();

            // Trace ColorPinIn connection back to DirLightingNode for SubsurfaceProfile access
            var linker = ColorPinIn.FindInLinker();
            if (linker != null)
            {
                DirLightingNode = linker.OutPin.HostNode as TtDeferredDirLightingNode;
            }

            CreateHorizontalPassGBuffers(policy);
        }
        private unsafe void CreateHorizontalPassGBuffers(TtRenderPolicy policy)
        {
            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            var PassDesc = new NxRHI.FRenderPassDesc();
            PassDesc.NumOfMRT = 1;
            PassDesc.AttachmentMRTs[0].Format = EPixelFormat.PXF_R16G16B16A16_FLOAT;
            PassDesc.AttachmentMRTs[0].Samples = 1;
            PassDesc.AttachmentMRTs[0].LoadAction = NxRHI.EFrameBufferLoadAction.LoadActionClear;
            PassDesc.AttachmentMRTs[0].StoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
            mHorizontalRenderPass = TtEngine.Instance.GfxDevice.RenderPassManager.GetPipelineState<NxRHI.FRenderPassDesc>(rc, in PassDesc);

            mHorizontalGBuffers.Initialize(policy, mHorizontalRenderPass);
            mHorizontalGBuffers.SetRenderTarget(policy, 0, mHorizontalPinOut);
            mHorizontalGBuffers.TargetViewIdentifier = TargetViewId;
        }
        public override void OnResize(TtRenderPolicy policy, float x, float y)
        {
            base.OnResize(policy, x, y);

            mHorizontalPinOut.Attachement.Width = (uint)(x * OutputScaleFactor);
            mHorizontalPinOut.Attachement.Height = (uint)(y * OutputScaleFactor);
            if (mHorizontalGBuffers != null)
            {
                mHorizontalGBuffers.SetSize(x * OutputScaleFactor, y * OutputScaleFactor);
            }
        }
        public override void OnDrawCall(Shader.TtGraphicsShadingEnv shading, NxRHI.ICommandList cmd, NxRHI.TtGraphicDraw drawcall, TtRenderPolicy policy, Mesh.TtRenderMesh.TtAtom atom)
        {
            var index = drawcall.FindBinder("ColorBuffer");
            if (index.IsValidPointer)
            {
                if (mCurrentPassIndex == 0)
                {
                    var attachBuffer = GetAttachBuffer(ColorPinIn);
                    drawcall.BindSRV(index, attachBuffer.Srv);
                }
                else
                {
                    var attachBuffer = GetAttachBuffer(mHorizontalPinOut);
                    drawcall.BindSRV(index, attachBuffer.Srv);
                }
            }
            index = drawcall.FindBinder("Samp_ColorBuffer");
            if (index.IsValidPointer)
                drawcall.BindSampler(index, TtEngine.Instance.GfxDevice.SamplerStateManager.LinearClampState);

            index = drawcall.FindBinder("SpecularBuffer");
            if (index.IsValidPointer)
            {
                var attachBuffer = GetAttachBuffer(SpecularPinIn);
                drawcall.BindSRV(index, attachBuffer.Srv);
            }
            index = drawcall.FindBinder("Samp_SpecularBuffer");
            if (index.IsValidPointer)
                drawcall.BindSampler(index, TtEngine.Instance.GfxDevice.SamplerStateManager.LinearClampState);

            index = drawcall.FindBinder("GBufferRT0");
            if (index.IsValidPointer)
            {
                var attachBuffer = GetAttachBuffer(GBufferRT0PinIn);
                drawcall.BindSRV(index, attachBuffer.Srv);
            }
            index = drawcall.FindBinder("Samp_GBufferRT0");
            if (index.IsValidPointer)
                drawcall.BindSampler(index, TtEngine.Instance.GfxDevice.SamplerStateManager.PointState);

            index = drawcall.FindBinder("GBufferRT3");
            if (index.IsValidPointer)
            {
                var attachBuffer = GetAttachBuffer(GBufferRT3PinIn);
                drawcall.BindSRV(index, attachBuffer.Srv);
            }
            index = drawcall.FindBinder("Samp_GBufferRT3");
            if (index.IsValidPointer)
                drawcall.BindSampler(index, TtEngine.Instance.GfxDevice.SamplerStateManager.PointState);

            index = drawcall.FindBinder("SubsurfaceProfiles");
            if (index.IsValidPointer)
            {
                var node = drawcall.TagObject as TtSSSBlurNode;
                var profileMgr = TtEngine.Instance.GfxDevice.SubsurfaceProfileManager;
                if (profileMgr?.ProfileSRV != null)
                    drawcall.BindSRV(index, profileMgr.ProfileSRV);
            }

            index = drawcall.FindBinder("cbSSSBlur");
            if (index.IsValidPointer)
            {
                if (CBSSSBlur == null)
                {
                    CBSSSBlur = TtEngine.Instance.GfxDevice.RenderContext.CreateCBV(index);
                }
                drawcall.BindCBV(index, CBSSSBlur);
            }

            index = drawcall.FindBinder("cbPerCamera");
            if (index.IsValidPointer)
            {
                drawcall.BindCBV(index, policy.DefaultCamera.PerCameraCBuffer);
            }
        }
        public unsafe override void Tick(GamePlay.TtWorld world, TtRenderPolicy policy, NxRHI.TtCommandList frameCmdList, bool bClear)
        {
            var buffer = this.FindAttachBuffer(ColorPinIn);
            if (buffer == null)
                return;

            float invWidth = 1.0f / (float)buffer.BufferDesc.Width;
            float invHeight = 1.0f / (float)buffer.BufferDesc.Height;

            GBuffers?.SetViewportCBuffer(world, policy);

            var cmdlist = TtEngine.Instance.GfxDevice.RenderContext.CmdListManager.GetCmdList();
            using (new NxRHI.TtCmdListScope(cmdlist, "SSSBlur"))
            {
                var passClears = new NxRHI.FRenderPassClears();
                passClears.SetDefault();
                passClears.SetClearColor(0, new Color4f(0, 0, 0, 0));

                var scissor = new NxRHI.FScissorRect();
                scissor.MinX = 0;
                scissor.MinY = 0;
                scissor.MaxX = (int)GBuffers.Viewport.Width;
                scissor.MaxY = (int)GBuffers.Viewport.Height;

                // Pass 0: Horizontal blur → mHorizontalPinOut
                mCurrentPassIndex = 0;
                if (CBSSSBlur != null)
                {
                    CBSSSBlur.SetValue("BlurDirection", new Vector2(invWidth, 0));
                    CBSSSBlur.SetValue("SSSWidth", SSSWidth);
                    CBSSSBlur.SetValue("SSSPassIndex", 0);
                }

                mHorizontalGBuffers.BuildFrameBuffers(policy);
                cmdlist.SetViewport(in GBuffers.Viewport);
                cmdlist.SetScissor(in scissor);
                cmdlist.BeginPass(mHorizontalGBuffers.FrameBuffers, in passClears, "SSSBlur_H");
                if (ScreenMesh != null)
                {
                    foreach (var sub in ScreenMesh.SubMeshes)
                    {
                        foreach (var atom in sub.Atoms)
                        {
                            var drawcall = atom.GetDrawCall(cmdlist.mCoreObject, mHorizontalGBuffers, policy, this, true);
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

                // Pass 1: Vertical blur + compose specular → ResultPinOut (GBuffers)
                mCurrentPassIndex = 1;
                if (CBSSSBlur != null)
                {
                    CBSSSBlur.SetValue("BlurDirection", new Vector2(0, invHeight));
                    CBSSSBlur.SetValue("SSSWidth", SSSWidth);
                    CBSSSBlur.SetValue("SSSPassIndex", 1);
                }

                GBuffers.BuildFrameBuffers(policy);
                cmdlist.SetViewport(in GBuffers.Viewport);
                cmdlist.SetScissor(in scissor);
                cmdlist.BeginPass(GBuffers.FrameBuffers, in passClears, "SSSBlur_V");
                if (ScreenMesh != null)
                {
                    foreach (var sub in ScreenMesh.SubMeshes)
                    {
                        foreach (var atom in sub.Atoms)
                        {
                            var drawcall = atom.GetDrawCall(cmdlist.mCoreObject, GBuffers, policy, this, true);
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

            policy.CommitCommandList(cmdlist, "SSSBlur");
        }
    }
}
