using EngineNS.GamePlay;
using EngineNS.Graphics.Mesh;
using EngineNS.Graphics.Pipeline.Shader;
using EngineNS.NxRHI;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Common
{
    //https://www.irimsky.top/archives/301/
    public class TtAntiAliasingShading : Shader.TtGraphicsShadingEnv
    {
        public TtAntiAliasingShading()
        {
            CodeName = RName.GetRName("shaders/ShadingEnv/AAShading.cginc", RName.ERNameType.Engine);

            TypeAA = this.PushPermutation<Graphics.Pipeline.TtRenderPolicy.ETypeAA>("ENV_TypeAA", (int)Graphics.Pipeline.TtRenderPolicy.ETypeAA.TypeCount);

            TypeAA.SetValue((int)Graphics.Pipeline.TtRenderPolicy.ETypeAA.None);

            this.UpdatePermutation();
        }
        public override NxRHI.EVertexStreamType[] GetNeedStreams()
        {
            return new NxRHI.EVertexStreamType[] { NxRHI.EVertexStreamType.VST_Position,
                NxRHI.EVertexStreamType.VST_UV,};
        }
        protected override void EnvShadingDefines(in FPermutationId id, TtShaderDefinitions defines)
        {
            defines.AddDefine("ETypeAA_None", (int)Graphics.Pipeline.TtRenderPolicy.ETypeAA.None);
            defines.AddDefine("TETypeAA_Fsaa", (int)Graphics.Pipeline.TtRenderPolicy.ETypeAA.Fsaa);
            defines.AddDefine("TETypeAA_Taa", (int)Graphics.Pipeline.TtRenderPolicy.ETypeAA.Taa);
        }
        public UPermutationItem TypeAA
        {
            get;
            set;
        }
        private void OnDrawcallTAA(NxRHI.TtGraphicDraw drawcall, TtRenderPolicy deferredPolicy, TtAntiAliasingNode aaNode)
        {
            if (deferredPolicy.TypeAA == TtRenderPolicy.ETypeAA.Taa)
            {
                var index = drawcall.FindBinder("ColorBuffer");
                if (index.IsValidPointer)
                {
                    var attachBuffer = aaNode.GetAttachBuffer(aaNode.ColorPinIn);
                    drawcall.BindSRV(index, attachBuffer.Srv);
                }
                else
                {
                    TypeAA.SetValue((uint)TtRenderPolicy.ETypeAA.Taa);
                    this.UpdatePermutation();
                }
                index = drawcall.FindBinder("Samp_ColorBuffer");
                if (index.IsValidPointer)
                    drawcall.BindSampler(index, TtEngine.Instance.GfxDevice.SamplerStateManager.LinearClampState);

                index = drawcall.FindBinder("DepthBuffer");
                if (index.IsValidPointer)
                {
                    var attachBuffer = aaNode.GetAttachBuffer(aaNode.DepthPinIn);
                    drawcall.BindSRV(index, attachBuffer.Srv);
                }
                index = drawcall.FindBinder("Samp_DepthBuffer");
                if (index.IsValidPointer)
                    drawcall.BindSampler(index, TtEngine.Instance.GfxDevice.SamplerStateManager.PointState);

                index = drawcall.FindBinder("MotionBuffer");
                if (index.IsValidPointer)
                {
                    var attachBuffer = aaNode.GetAttachBuffer(aaNode.MotionVectorPinIn);
                    drawcall.BindSRV(index, attachBuffer.Srv);
                }
                index = drawcall.FindBinder("Samp_MotionBuffer");
                if (index.IsValidPointer)
                    drawcall.BindSampler(index, TtEngine.Instance.GfxDevice.SamplerStateManager.PointState);

                index = drawcall.FindBinder("PrevColorBuffer");
                if (index.IsValidPointer)
                {
                    var attachBuffer = aaNode.GetAttachBuffer(aaNode.PreColorPinIn);
                    drawcall.BindSRV(index, attachBuffer.Srv);
                }
                else
                {
                    TypeAA.SetValue((uint)TtRenderPolicy.ETypeAA.Taa);
                    this.UpdatePermutation();
                }
                index = drawcall.FindBinder("Samp_PrevColorBuffer");
                if (index.IsValidPointer)
                    drawcall.BindSampler(index, TtEngine.Instance.GfxDevice.SamplerStateManager.LinearClampState);

                index = drawcall.FindBinder("PrevDepthBuffer");
                if (index.IsValidPointer)
                {
                    var attachBuffer = aaNode.GetAttachBuffer(aaNode.PreDepthPinIn);
                    drawcall.BindSRV(index, attachBuffer.Srv);
                }
                index = drawcall.FindBinder("Samp_PrevDepthBuffer");
                if (index.IsValidPointer)
                    drawcall.BindSampler(index, TtEngine.Instance.GfxDevice.SamplerStateManager.PointState);


                index = drawcall.FindBinder("cbShadingEnv");
                if (index.IsValidPointer)
                {
                    if (aaNode.CBShadingEnv == null)
                    {
                        aaNode.CBShadingEnv = TtEngine.Instance.GfxDevice.RenderContext.CreateCBV(index);
                        var jitterUV = deferredPolicy.DefaultCamera.mCoreObject.GetJitterUV();
                        aaNode.CBShadingEnv.SetValue("JitterUV", in jitterUV);
                    }
                    drawcall.BindCBuffer(index, aaNode.CBShadingEnv);
                }
            }
            else
            {
                var index = drawcall.FindBinder("ColorBuffer");
                if (index.IsValidPointer)
                {
                    var attachBuffer = aaNode.GetAttachBuffer(aaNode.ColorPinIn);
                    drawcall.BindSRV(index, attachBuffer.Srv);
                }
                else
                {
                    TypeAA.SetValue((uint)TtRenderPolicy.ETypeAA.Taa);
                    this.UpdatePermutation();
                }
                index = drawcall.FindBinder("Samp_ColorBuffer");
                if (index.IsValidPointer)
                    drawcall.BindSampler(index, TtEngine.Instance.GfxDevice.SamplerStateManager.PointState);
            }

        }
        public unsafe override void OnDrawCall(NxRHI.ICommandList cmd, NxRHI.TtGraphicDraw drawcall, TtRenderPolicy policy, Mesh.TtMesh.TtAtom atom)
        {
            base.OnDrawCall(cmd, drawcall, policy, atom);

            var aaNode = drawcall.TagObject as Common.TtAntiAliasingNode;

            OnDrawcallTAA(drawcall, policy, aaNode);
        }
    }
    [Bricks.CodeBuilder.ContextMenu("AntiAliasing", "Post\\AntiAliasing", Bricks.RenderPolicyEditor.UPolicyGraph.RGDEditorKeyword)]
    public class TtAntiAliasingNode : TtSceenSpaceNode
    {
        public TtRenderGraphPin ColorPinIn = TtRenderGraphPin.CreateInput("Color");
        public TtRenderGraphPin PreColorPinIn = TtRenderGraphPin.CreateInput("PreColor");
        public TtRenderGraphPin DepthPinIn = TtRenderGraphPin.CreateInput("Depth");
        public TtRenderGraphPin PreDepthPinIn = TtRenderGraphPin.CreateInput("PreDepth");
        public TtRenderGraphPin MotionVectorPinIn = TtRenderGraphPin.CreateInput("MotionVector");

        public NxRHI.TtCopyDraw mCopyColorDrawcall;
        public NxRHI.TtCopyDraw mCopyDepthDrawcall;
        public UDrawBuffers CopyPass = new UDrawBuffers();

        public TtAttachBuffer[] ResultBuffer = new TtAttachBuffer[2];
        public TtAttachBuffer PreColor { get => ResultBuffer[0]; }
        public TtAttachBuffer PreDepth { get => ResultBuffer[1]; }
        public TtAntiAliasingNode()
        {
            Name = "TaaNode";
        }
        public override void InitNodePins()
        {
            AddInput(ColorPinIn, NxRHI.EBufferType.BFT_SRV);
            AddInput(PreColorPinIn, NxRHI.EBufferType.BFT_SRV);

            AddInput(DepthPinIn, NxRHI.EBufferType.BFT_SRV);
            AddInput(PreDepthPinIn, NxRHI.EBufferType.BFT_SRV);

            AddInput(MotionVectorPinIn, NxRHI.EBufferType.BFT_SRV);

            base.InitNodePins();
        }
        public TtAntiAliasingShading mBasePassShading;
        public override TtGraphicsShadingEnv GetPassShading(TtMesh.TtAtom atom = null)
        {
            return mBasePassShading;
        }
        public override async System.Threading.Tasks.Task Initialize(TtRenderPolicy policy, string debugName)
        {
            await base.Initialize(policy, debugName);

            var rc = TtEngine.Instance.GfxDevice.RenderContext;

            mBasePassShading = await TtEngine.Instance.ShadingEnvManager.GetShadingEnv<TtAntiAliasingShading>();

            mCopyColorDrawcall = TtEngine.Instance.GfxDevice.RenderContext.CreateCopyDraw();
            mCopyDepthDrawcall = TtEngine.Instance.GfxDevice.RenderContext.CreateCopyDraw();

            CopyPass.Initialize(rc, debugName + ".CopyPrev");
        }

        public NxRHI.TtCbView CBShadingEnv;

        //double[] mOffsetHaltonSequencer;
        //private double[] OffsetHaltonSequencer
        //{
        //    get
        //    {
        //        if (mOffsetHaltonSequencer == null)
        //            mOffsetHaltonSequencer = MathHelper.GenHaltonSequence(5);
        //        return mOffsetHaltonSequencer;
        //    }
        //}
        private Vector2[] OffsetHaltonSequencer = new Vector2[]
        {
            //new Vector2(0.5f, 0.5f),
            //new Vector2(0.75f, 0.5f),
            //new Vector2(0.25f, 0.5f),
            //new Vector2(0.5f, 0.75f),
            //new Vector2(0.5f, 0.25f),

            new Vector2(0.5f, 1.0f / 3),
            new Vector2(0.25f, 2.0f / 3),
            new Vector2(0.75f, 1.0f / 9),
            new Vector2(0.125f, 4.0f / 9),
            new Vector2(0.625f, 7.0f / 9),
            new Vector2(0.375f, 2.0f / 9),
            new Vector2(0.875f, 5.0f / 9),
            new Vector2(0.0625f, 8.0f / 9),
        };
        private int CurrentOffsetIndex = 0;
        public float TaaBlendAlpha { get; set; } = 0.05f;
        private void TickSyncTAA(TtRenderPolicy policy)
        {
            if (CBShadingEnv != null)
            {
                var jitterUV = policy.DefaultCamera.mCoreObject.GetJitterUV();
                jitterUV.Y = -jitterUV.Y;
                CBShadingEnv.SetValue("JitterUV", in jitterUV);
                //if (CurrentOffsetIndex % 2 == 1)
                //    TaaBlendAlpha = 0.0f;
                //else
                //    TaaBlendAlpha = 1.0f;
                CBShadingEnv.SetValue("TaaBlendAlpha", TaaBlendAlpha);
            }

            CurrentOffsetIndex++;
            CurrentOffsetIndex = CurrentOffsetIndex % OffsetHaltonSequencer.Length;
            if (policy.TypeAA == TtRenderPolicy.ETypeAA.Taa)
            {
                Vector2 offset = OffsetHaltonSequencer[CurrentOffsetIndex];
                policy.DefaultCamera.JitterOffset = offset;
            }
            else
            {
                policy.DefaultCamera.JitterOffset = new Vector2(0.5f, 0.5f);
            }
            CopyPass.SwapBuffer();
        }

        public override void FrameBuild(TtRenderPolicy policy)
        {
            base.FrameBuild(policy);
        }

        public override void BeforeTickLogic(TtRenderPolicy policy)
        {
            if (policy.TypeAA == TtRenderPolicy.ETypeAA.None)
            {
                this.MoveAttachment(ColorPinIn, ResultPinOut);
                return;
            }
            var buffer = this.FindAttachBuffer(ColorPinIn);
            if (buffer != null)
            {
                if (ResultPinOut.Attachement.Format != buffer.BufferDesc.Format)
                {
                    this.CreateGBuffers(policy, buffer.BufferDesc.Format);
                    ResultPinOut.Attachement.Format = buffer.BufferDesc.Format;
                }
            }
            if (policy.TypeAA == TtRenderPolicy.ETypeAA.Taa)
            {
                if ((PreColor == null || PreDepth == null) || (PreColor.BufferDesc.Format != ResultPinOut.Attachement.Format
                 || PreColor.BufferDesc.Width != ResultPinOut.Attachement.Width
                 || PreColor.BufferDesc.Height != ResultPinOut.Attachement.Height))
                {
                    CoreSDK.DisposeObject(ref ResultBuffer[0]);
                    CoreSDK.DisposeObject(ref ResultBuffer[1]);
                    ResultBuffer[0] = new TtAttachBuffer();
                    ResultBuffer[1] = new TtAttachBuffer();
                    ResultBuffer[0].BufferDesc = ResultPinOut.Attachement.BufferDesc;
                    ResultBuffer[0].CreateBufferViews(in ResultBuffer[0].BufferDesc);

                    ResultBuffer[1].BufferDesc = ResultPinOut.Attachement.BufferDesc;
                    ResultBuffer[1].CreateBufferViews(in ResultBuffer[0].BufferDesc);
                }
            }
        }
        public override void TickLogic(TtWorld world, TtRenderPolicy policy, bool bClear)
        {
            switch (policy.TypeAA)
            {
                case TtRenderPolicy.ETypeAA.None:
                    break;
                case TtRenderPolicy.ETypeAA.Taa:
                    PreColorPinIn.ImportedBuffer = PreColor;
                    base.TickLogic(world, policy, bClear);
                    TickCopyLogic(policy);
                    break;
                case TtRenderPolicy.ETypeAA.Fsaa:
                    base.TickLogic(world, policy, bClear);
                    break;
            }
        }

        public void CopyAttachBuff(TtRenderGraphPin SrcPin, TtAttachBuffer DesAttachBuffer, NxRHI.TtCopyDraw CopyDrawcall, NxRHI.UCommandList DrawCommandList)
        {
            var srcPin = GetAttachBuffer(SrcPin);

            if (srcPin.GpuResource.GetType() == typeof(NxRHI.TtBuffer) && DesAttachBuffer.GpuResource.GetType() == typeof(NxRHI.TtBuffer))
            {
                CopyDrawcall.Mode = NxRHI.ECopyDrawMode.CDM_Buffer2Buffer;
            }
            else if (srcPin.GpuResource.GetType() == typeof(NxRHI.TtTexture) && DesAttachBuffer.GpuResource.GetType() == typeof(NxRHI.TtTexture))
            {
                CopyDrawcall.Mode = NxRHI.ECopyDrawMode.CDM_Texture2Texture;
            }
            else if (srcPin.GpuResource.GetType() == typeof(NxRHI.TtTexture) && DesAttachBuffer.GpuResource.GetType() == typeof(NxRHI.TtBuffer))
            {
                CopyDrawcall.Mode = NxRHI.ECopyDrawMode.CDM_Texture2Buffer;
            }
            else if (srcPin.GpuResource.GetType() == typeof(NxRHI.TtTexture) && DesAttachBuffer.GpuResource.GetType() == typeof(NxRHI.TtBuffer))
            {
                CopyDrawcall.Mode = NxRHI.ECopyDrawMode.CDM_Buffer2Texture;
            }
            CopyDrawcall.BindSrc(srcPin.GpuResource);
            CopyDrawcall.BindDest(DesAttachBuffer.GpuResource);

            DrawCommandList.PushGpuDraw(CopyDrawcall);
            //var fp = new NxRHI.FSubResourceFootPrint();
            //fp.SetDefault();
            //mCopyDrawcall.mCoreObject.FootPrint = fp;
        }

        [ThreadStatic]
        private static Profiler.TimeScope mScopeTick;
        private static Profiler.TimeScope ScopeTick
        {
            get
            {
                if (mScopeTick == null)
                    mScopeTick = new Profiler.TimeScope(typeof(TtAntiAliasingNode), nameof(TickCopyLogic));
                return mScopeTick;
            }
        }
        public unsafe void TickCopyLogic(TtRenderPolicy policy)
        {
            using (new Profiler.TimeScopeHelper(ScopeTick))
            {
                if (mCopyColorDrawcall == null || mCopyDepthDrawcall == null)
                    return;

                var cmdlist = CopyPass.DrawCmdList;
                using (new NxRHI.TtCmdListScope(cmdlist))
                {
                    CopyAttachBuff(ResultPinOut, PreColor, mCopyColorDrawcall, cmdlist);
                    cmdlist.FlushDraws();
                }
                policy.CommitCommandList(cmdlist);
            }
        }
        public override void TickSync(TtRenderPolicy policy)
        {
            if (policy.TypeAA == TtRenderPolicy.ETypeAA.None)
            {
                return;
            }
            base.TickSync(policy);

            TickSyncTAA(policy);
        }
    }
}
