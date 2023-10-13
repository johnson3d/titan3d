using EngineNS.GamePlay;
using EngineNS.NxRHI;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Common
{
    //https://www.irimsky.top/archives/301/
    public class TtAntiAliasingShading : Shader.UGraphicsShadingEnv
    {
        public TtAntiAliasingShading()
        {
            CodeName = RName.GetRName("shaders/ShadingEnv/AAShading.cginc", RName.ERNameType.Engine);

            TypeAA = this.PushPermutation<Graphics.Pipeline.URenderPolicy.ETypeAA>("ENV_TypeAA", (int)Graphics.Pipeline.URenderPolicy.ETypeAA.TypeCount);

            TypeAA.SetValue((int)Graphics.Pipeline.URenderPolicy.ETypeAA.None);

            this.UpdatePermutation();
        }
        public override NxRHI.EVertexStreamType[] GetNeedStreams()
        {
            return new NxRHI.EVertexStreamType[] { NxRHI.EVertexStreamType.VST_Position,
                NxRHI.EVertexStreamType.VST_UV,};
        }
        protected override void EnvShadingDefines(in FPermutationId id, UShaderDefinitions defines)
        {
            defines.AddDefine("ETypeAA_None", (int)Graphics.Pipeline.URenderPolicy.ETypeAA.None);
            defines.AddDefine("TETypeAA_Fsaa", (int)Graphics.Pipeline.URenderPolicy.ETypeAA.Fsaa);
            defines.AddDefine("TETypeAA_Taa", (int)Graphics.Pipeline.URenderPolicy.ETypeAA.Taa);
        }
        public UPermutationItem TypeAA
        {
            get;
            set;
        }
        private void OnDrawcallTAA(NxRHI.UGraphicDraw drawcall, URenderPolicy deferredPolicy, TtAntiAliasingNode aaNode)
        {
            if (deferredPolicy.TypeAA == URenderPolicy.ETypeAA.Taa)
            {
                var index = drawcall.FindBinder("ColorBuffer");
                if (index.IsValidPointer)
                {
                    var attachBuffer = aaNode.GetAttachBuffer(aaNode.ColorPinIn);
                    drawcall.BindSRV(index, attachBuffer.Srv);
                }
                else
                {
                    TypeAA.SetValue((uint)URenderPolicy.ETypeAA.Taa);
                    this.UpdatePermutation();
                }
                index = drawcall.FindBinder("Samp_ColorBuffer");
                if (index.IsValidPointer)
                    drawcall.BindSampler(index, UEngine.Instance.GfxDevice.SamplerStateManager.LinearClampState);

                index = drawcall.FindBinder("DepthBuffer");
                if (index.IsValidPointer)
                {
                    var attachBuffer = aaNode.GetAttachBuffer(aaNode.DepthPinIn);
                    drawcall.BindSRV(index, attachBuffer.Srv);
                }
                index = drawcall.FindBinder("Samp_DepthBuffer");
                if (index.IsValidPointer)
                    drawcall.BindSampler(index, UEngine.Instance.GfxDevice.SamplerStateManager.PointState);

                index = drawcall.FindBinder("MotionBuffer");
                if (index.IsValidPointer)
                {
                    var attachBuffer = aaNode.GetAttachBuffer(aaNode.MotionVectorPinIn);
                    drawcall.BindSRV(index, attachBuffer.Srv);
                }
                index = drawcall.FindBinder("Samp_MotionBuffer");
                if (index.IsValidPointer)
                    drawcall.BindSampler(index, UEngine.Instance.GfxDevice.SamplerStateManager.PointState);

                index = drawcall.FindBinder("PrevColorBuffer");
                if (index.IsValidPointer)
                {
                    var attachBuffer = aaNode.GetAttachBuffer(aaNode.PreColorPinIn);
                    drawcall.BindSRV(index, attachBuffer.Srv);
                }
                else
                {
                    TypeAA.SetValue((uint)URenderPolicy.ETypeAA.Taa);
                    this.UpdatePermutation();
                }
                index = drawcall.FindBinder("Samp_PrevColorBuffer");
                if (index.IsValidPointer)
                    drawcall.BindSampler(index, UEngine.Instance.GfxDevice.SamplerStateManager.LinearClampState);

                index = drawcall.FindBinder("PrevDepthBuffer");
                if (index.IsValidPointer)
                {
                    var attachBuffer = aaNode.GetAttachBuffer(aaNode.PreDepthPinIn);
                    drawcall.BindSRV(index, attachBuffer.Srv);
                }
                index = drawcall.FindBinder("Samp_PrevDepthBuffer");
                if (index.IsValidPointer)
                    drawcall.BindSampler(index, UEngine.Instance.GfxDevice.SamplerStateManager.PointState);


                index = drawcall.FindBinder("cbShadingEnv");
                if (index.IsValidPointer)
                {
                    if (aaNode.CBShadingEnv == null)
                    {
                        aaNode.CBShadingEnv = UEngine.Instance.GfxDevice.RenderContext.CreateCBV(index);
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
                    TypeAA.SetValue((uint)URenderPolicy.ETypeAA.Taa);
                    this.UpdatePermutation();
                }
                index = drawcall.FindBinder("Samp_ColorBuffer");
                if (index.IsValidPointer)
                    drawcall.BindSampler(index, UEngine.Instance.GfxDevice.SamplerStateManager.PointState);
            }

        }
        public unsafe override void OnDrawCall(NxRHI.ICommandList cmd, Pipeline.URenderPolicy.EShadingType shadingType, NxRHI.UGraphicDraw drawcall, URenderPolicy policy, Mesh.UMesh mesh)
        {
            base.OnDrawCall(cmd, shadingType, drawcall, policy, mesh);

            var pipelinPolicy = policy.TagObject as URenderPolicy;

            var aaNode = drawcall.TagObject as Common.TtAntiAliasingNode;
            if (aaNode == null)
                aaNode = pipelinPolicy.FindFirstNode<Common.TtAntiAliasingNode>();

            OnDrawcallTAA(drawcall, pipelinPolicy, aaNode);
        }
    }
    public class TtAntiAliasingNode : USceenSpaceNode
    {
        public Common.URenderGraphPin ColorPinIn = Common.URenderGraphPin.CreateInput("Color");
        public Common.URenderGraphPin PreColorPinIn = Common.URenderGraphPin.CreateInput("PreColor");
        public Common.URenderGraphPin DepthPinIn = Common.URenderGraphPin.CreateInput("Depth");
        public Common.URenderGraphPin PreDepthPinIn = Common.URenderGraphPin.CreateInput("PreDepth");
        public Common.URenderGraphPin MotionVectorPinIn = Common.URenderGraphPin.CreateInput("MotionVector");

        public NxRHI.UCopyDraw mCopyColorDrawcall;
        public NxRHI.UCopyDraw mCopyDepthDrawcall;
        public UDrawBuffers CopyPass = new UDrawBuffers();

        public UAttachBuffer[] ResultBuffer = new UAttachBuffer[2];
        public UAttachBuffer PreColor { get => ResultBuffer[0]; }
        public UAttachBuffer PreDepth { get => ResultBuffer[1]; }
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
        public override async System.Threading.Tasks.Task Initialize(URenderPolicy policy, string debugName)
        {
            await base.Initialize(policy, debugName);

            var rc = UEngine.Instance.GfxDevice.RenderContext;

            ScreenDrawPolicy.mBasePassShading = UEngine.Instance.ShadingEnvManager.GetShadingEnv<TtAntiAliasingShading>();

            mCopyColorDrawcall = UEngine.Instance.GfxDevice.RenderContext.CreateCopyDraw();
            mCopyDepthDrawcall = UEngine.Instance.GfxDevice.RenderContext.CreateCopyDraw();

            CopyPass.Initialize(rc, debugName + ".CopyPrev");
        }

        public NxRHI.UCbView CBShadingEnv;

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
        private void TickSyncTAA(URenderPolicy policy)
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
            if (policy.TypeAA == URenderPolicy.ETypeAA.Taa)
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

        public override void FrameBuild(URenderPolicy policy)
        {
            base.FrameBuild(policy);
        }

        public override void BeforeTickLogic(URenderPolicy policy)
        {
            if (policy.TypeAA == URenderPolicy.ETypeAA.None)
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
            if (policy.TypeAA == URenderPolicy.ETypeAA.Taa)
            {
                if ((PreColor == null || PreDepth == null) || (PreColor.BufferDesc.Format != ResultPinOut.Attachement.Format
                 || PreColor.BufferDesc.Width != ResultPinOut.Attachement.Width
                 || PreColor.BufferDesc.Height != ResultPinOut.Attachement.Height))
                {
                    CoreSDK.DisposeObject(ref ResultBuffer[0]);
                    CoreSDK.DisposeObject(ref ResultBuffer[1]);
                    ResultBuffer[0] = new UAttachBuffer();
                    ResultBuffer[1] = new UAttachBuffer();
                    ResultBuffer[0].BufferDesc = ResultPinOut.Attachement.BufferDesc;
                    ResultBuffer[0].CreateBufferViews(in ResultBuffer[0].BufferDesc);

                    ResultBuffer[1].BufferDesc = ResultPinOut.Attachement.BufferDesc;
                    ResultBuffer[1].CreateBufferViews(in ResultBuffer[0].BufferDesc);
                }
            }
        }
        public override void TickLogic(UWorld world, URenderPolicy policy, bool bClear)
        {
            switch (policy.TypeAA)
            {
                case URenderPolicy.ETypeAA.None:
                    break;
                case URenderPolicy.ETypeAA.Taa:
                    PreColorPinIn.ImportedBuffer = PreColor;
                    base.TickLogic(world, policy, bClear);
                    TickCopyLogic();
                    break;
                case URenderPolicy.ETypeAA.Fsaa:
                    base.TickLogic(world, policy, bClear);
                    break;
            }
        }

        public void CopyAttachBuff(Common.URenderGraphPin SrcPin, UAttachBuffer DesAttachBuffer, NxRHI.UCopyDraw CopyDrawcall, NxRHI.UCommandList DrawCommandList)
        {
            var srcPin = GetAttachBuffer(SrcPin);

            if (srcPin.Buffer.GetType() == typeof(NxRHI.UBuffer) && DesAttachBuffer.Buffer.GetType() == typeof(NxRHI.UBuffer))
            {
                CopyDrawcall.Mode = NxRHI.ECopyDrawMode.CDM_Buffer2Buffer;
            }
            else if (srcPin.Buffer.GetType() == typeof(NxRHI.UTexture) && DesAttachBuffer.Buffer.GetType() == typeof(NxRHI.UTexture))
            {
                CopyDrawcall.Mode = NxRHI.ECopyDrawMode.CDM_Texture2Texture;
            }
            else if (srcPin.Buffer.GetType() == typeof(NxRHI.UTexture) && DesAttachBuffer.Buffer.GetType() == typeof(NxRHI.UBuffer))
            {
                CopyDrawcall.Mode = NxRHI.ECopyDrawMode.CDM_Texture2Buffer;
            }
            else if (srcPin.Buffer.GetType() == typeof(NxRHI.UTexture) && DesAttachBuffer.Buffer.GetType() == typeof(NxRHI.UBuffer))
            {
                CopyDrawcall.Mode = NxRHI.ECopyDrawMode.CDM_Buffer2Texture;
            }
            CopyDrawcall.BindSrc(srcPin.Buffer);
            CopyDrawcall.BindDest(DesAttachBuffer.Buffer);

            DrawCommandList.PushGpuDraw(CopyDrawcall);
            //var fp = new NxRHI.FSubResourceFootPrint();
            //fp.SetDefault();
            //mCopyDrawcall.mCoreObject.FootPrint = fp;
        }

        [ThreadStatic]
        private static Profiler.TimeScope ScopeTick = Profiler.TimeScopeManager.GetTimeScope(typeof(TtAntiAliasingNode), nameof(TickCopyLogic));
        public unsafe void TickCopyLogic()
        {
            using (new Profiler.TimeScopeHelper(ScopeTick))
            {
                if (mCopyColorDrawcall == null || mCopyDepthDrawcall == null)
                    return;

                var cmdlist = CopyPass.DrawCmdList;
                cmdlist.BeginCommand();
                {
                    CopyAttachBuff(ResultPinOut, PreColor, mCopyColorDrawcall, cmdlist);
                }
                cmdlist.FlushDraws();
                cmdlist.EndCommand();
                UEngine.Instance.GfxDevice.RenderCmdQueue.QueueCmdlist(cmdlist);
            }
        }
        public override void TickSync(URenderPolicy policy)
        {
            if (policy.TypeAA == URenderPolicy.ETypeAA.None)
            {
                return;
            }
            base.TickSync(policy);

            TickSyncTAA(policy);
        }
    }
}
