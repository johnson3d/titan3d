using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EngineNS.NxRHI;
using EngineNS.GamePlay;
using EngineNS.Graphics.Mesh;

namespace EngineNS.Graphics.Pipeline.Common.Post
{
    //FSR2: https://www.kindem.xyz/post/56/
    //weight with lanczos
    public class TtFsrUpSampleShading : Shader.TtComputeShadingEnv
    {
        public override Vector3ui DispatchArg 
        {
            get => new Vector3ui(64, 1, 1);
        }
        public UPermutationItem TypeUpSampleMode
        {
            get;
            set;
        }
        public enum EUpSampleMode
        {
            Bilinear,
            EASU,
            //RCAS,
            TypeCount
        }
        public TtFsrUpSampleShading()
        {
            CodeName = RName.GetRName("shaders/ShadingEnv/Post/FsrShading.compute", RName.ERNameType.Engine);
            MainName = "CS_FsrMain";

            TypeUpSampleMode = this.PushPermutation<EUpSampleMode>("ENV_TypeUpSampleMode", (int)EUpSampleMode.TypeCount);

            TypeUpSampleMode.SetValue((int)EUpSampleMode.EASU);

            this.UpdatePermutation();
        }
        protected override void EnvShadingDefines(in FPermutationId id, TtShaderDefinitions defines)
        {
            base.EnvShadingDefines(in id, defines);

            defines.AddDefine("USE_RCAS", (int)0);
            defines.AddDefine("UpSampleMode_Bilinear", (int)EUpSampleMode.Bilinear);
            defines.AddDefine("UpSampleMode_EASU", (int)EUpSampleMode.EASU);
        }
        public override void OnDrawCall(NxRHI.TtComputeDraw drawcall, TtRenderPolicy policy)
        {
            var aaNode = drawcall.TagObject as TtFsrNode;
            if (aaNode == null)
            {
                var pipelinePolicy = policy;
                aaNode = pipelinePolicy.FindFirstNode<TtFsrNode>();
            }

            var index = drawcall.FindBinder(EShaderBindType.SBT_SRV, "ColorBuffer");
            if (index.IsValidPointer)
            {
                var attachBuffer = aaNode.GetAttachBuffer(aaNode.ColorPinIn);
                drawcall.BindSrv(index, attachBuffer.Srv);
            }
            index = drawcall.FindBinder(EShaderBindType.SBT_Sampler, "Samp_ColorBuffer");
            if (index.IsValidPointer)
                drawcall.BindSampler(index, TtEngine.Instance.GfxDevice.SamplerStateManager.LinearClampState);
            index = drawcall.FindBinder(EShaderBindType.SBT_UAV, "OutputTexture");
            if (index.IsValidPointer)
            {
                var attachBuffer = aaNode.GetAttachBuffer(aaNode.UpSamplePinOut);
                drawcall.BindUav(index, attachBuffer.Uav);
            }
            index = drawcall.FindBinder(NxRHI.EShaderBindType.SBT_CBuffer, "cbShadingEnv");
            if (index.IsValidPointer)
            {
                if (aaNode.CBShadingEnv == null)
                {
                    aaNode.CBShadingEnv = TtEngine.Instance.GfxDevice.RenderContext.CreateCBV(index);
                }
                drawcall.BindCBuffer(index, aaNode.CBShadingEnv);
            }
        }
    }
    public class TtRCASShading : Shader.TtComputeShadingEnv
    {
        public override Vector3ui DispatchArg
        {
            get => new Vector3ui(64, 1, 1);
        }
        public TtRCASShading()
        {
            CodeName = RName.GetRName("shaders/ShadingEnv/Post/FsrShading.compute", RName.ERNameType.Engine);
            MainName = "CS_FsrMain";

            this.UpdatePermutation();
        }
        protected override void EnvShadingDefines(in FPermutationId id, TtShaderDefinitions defines)
        {
            base.EnvShadingDefines(in id, defines);

            defines.AddDefine("USE_RCAS", (int)1);
        }
        public override void OnDrawCall(NxRHI.TtComputeDraw drawcall, TtRenderPolicy policy)
        {
            var aaNode = drawcall.TagObject as TtFsrNode;

            var index = drawcall.FindBinder(EShaderBindType.SBT_SRV, "ColorBuffer");
            if (index.IsValidPointer)
            {
                var attachBuffer = aaNode.GetAttachBuffer(aaNode.UpSamplePinOut);
                drawcall.BindSrv(index, attachBuffer.Srv);
            }
            index = drawcall.FindBinder(EShaderBindType.SBT_Sampler, "Samp_ColorBuffer");
            if (index.IsValidPointer)
                drawcall.BindSampler(index, TtEngine.Instance.GfxDevice.SamplerStateManager.LinearClampState);
            index = drawcall.FindBinder(EShaderBindType.SBT_UAV, "OutputTexture");
            if (index.IsValidPointer)
            {
                var attachBuffer = aaNode.GetAttachBuffer(aaNode.RcasPinOut);
                drawcall.BindUav(index, attachBuffer.Uav);
            }
            index = drawcall.FindBinder(NxRHI.EShaderBindType.SBT_CBuffer, "cbShadingEnv");
            if (index.IsValidPointer)
            {
                if (aaNode.CBShadingEnv == null)
                {
                    aaNode.CBShadingEnv = TtEngine.Instance.GfxDevice.RenderContext.CreateCBV(index);
                }
                drawcall.BindCBuffer(index, aaNode.CBShadingEnv);
            }
        }
    }
    [Bricks.CodeBuilder.ContextMenu("Fsr", "Post\\Fsr", Bricks.RenderPolicyEditor.UPolicyGraph.RGDEditorKeyword)]
    public class TtFsrNode : TtRenderGraphNode
    {
        public TtRenderGraphPin ColorPinIn = TtRenderGraphPin.CreateInputOutput("Color");
        public TtRenderGraphPin UpSamplePinOut = TtRenderGraphPin.CreateOutput("UpSample", false, EPixelFormat.PXF_R8G8B8A8_UNORM);
        public TtRenderGraphPin RcasPinOut = TtRenderGraphPin.CreateOutput("Rcas", false, EPixelFormat.PXF_R8G8B8A8_UNORM);
        [Rtti.Meta]
        public float Scale { get; set; } = 2.0f;
        public NxRHI.TtCbView CBShadingEnv;
        public TtFsrUpSampleShading UpSampleShadingEnv;
        public TtRCASShading RCASShading;
        private NxRHI.TtComputeDraw UpSampleDrawcall;
        private NxRHI.TtComputeDraw RCASDrawcall;
        public TtFsrNode()
        {
            Name = "FsrNode";

            mFsrStruct.SetDefault();
        }
        public override void Dispose()
        {
            CoreSDK.DisposeObject(ref UpSampleDrawcall);
            base.Dispose();
        }
        public override void InitNodePins()
        {
            AddInputOutput(ColorPinIn, NxRHI.EBufferType.BFT_SRV);
            AddOutput(UpSamplePinOut, NxRHI.EBufferType.BFT_UAV | NxRHI.EBufferType.BFT_SRV);
            AddOutput(RcasPinOut, NxRHI.EBufferType.BFT_UAV | NxRHI.EBufferType.BFT_SRV);

            base.InitNodePins();
        }
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 16)]
        struct FFsrStruct
        {
            public void SetDefault()
            {
                
            }
            public Vector4 Const0;
            public Vector4 Const1;
            public Vector4 Const2;
            public Vector4 Const3;
            public Vector4 Sample;
        }
        FFsrStruct mFsrStruct;
        public override async Task Initialize(TtRenderPolicy policy, string debugName)
        {
            await base.Initialize(policy, debugName);
            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            BasePass.Initialize(rc, debugName);

            CoreSDK.DisposeObject(ref UpSampleDrawcall);
            UpSampleDrawcall = rc.CreateComputeDraw();
            UpSampleShadingEnv = await TtEngine.Instance.ShadingEnvManager.GetShadingEnv<TtFsrUpSampleShading>();
            RCASDrawcall = rc.CreateComputeDraw();
            RCASShading = await TtEngine.Instance.ShadingEnvManager.GetShadingEnv<TtRCASShading>();
        }
        public override void OnResize(TtRenderPolicy policy, float x, float y)
        {
            base.OnResize(policy, x, y);            
        }
        public override void BeforeTickLogic(TtRenderPolicy policy)
        {
            base.BeforeTickLogic(policy);

            var src = GetAttachBuffer(ColorPinIn);
            if (src != null)
            {
                UpSamplePinOut.Attachement.Format = src.BufferDesc.Format;
                UpSamplePinOut.Attachement.Width = (uint)(Scale * src.BufferDesc.Width);
                UpSamplePinOut.Attachement.Height = (uint)(Scale * src.BufferDesc.Height);

                RcasPinOut.Attachement.Format = src.BufferDesc.Format;
                RcasPinOut.Attachement.Width = (uint)(Scale * src.BufferDesc.Width);
                RcasPinOut.Attachement.Height = (uint)(Scale * src.BufferDesc.Height);
                
                FsrEasuCon(ref mFsrStruct.Const0, ref mFsrStruct.Const1, ref mFsrStruct.Const2, ref mFsrStruct.Const3,
                    src.BufferDesc.Width, src.BufferDesc.Height, src.BufferDesc.Width, src.BufferDesc.Height,
                    UpSamplePinOut.Attachement.Width, UpSamplePinOut.Attachement.Height);

                if (CBShadingEnv != null)
                {
                    CBShadingEnv.SetValue("Const0", mFsrStruct.Const0);
                    CBShadingEnv.SetValue("Const1", mFsrStruct.Const1);
                    CBShadingEnv.SetValue("Const2", mFsrStruct.Const2);
                    CBShadingEnv.SetValue("Const3", mFsrStruct.Const3);
                    CBShadingEnv.SetValue("Sample", mFsrStruct.Sample);
                }
            }
        }
        public override void TickLogic(TtWorld world, TtRenderPolicy policy, bool bClear)
        {
            const uint threadGroupWorkRegionDim = 16;
            var dispatchX = MathHelper.Roundup(UpSamplePinOut.Attachement.Width, threadGroupWorkRegionDim);
            var dispatchY = MathHelper.Roundup(UpSamplePinOut.Attachement.Height, threadGroupWorkRegionDim);

            var cmd = BasePass.DrawCmdList;
            using (new NxRHI.TtCmdListScope(cmd))
            {
                UpSampleShadingEnv.SetDrawcallDispatch(this, policy, UpSampleDrawcall, dispatchX,
                            dispatchY, 1, false);
                //UpSampleDrawcall.Commit(cmd);
                cmd.PushGpuDraw(UpSampleDrawcall);
                RCASShading.SetDrawcallDispatch(this, policy, RCASDrawcall, dispatchX,
                                dispatchY, 1, false);
                //RCASDrawcall.Commit(cmd);
                cmd.PushGpuDraw(RCASDrawcall);
                cmd.FlushDraws();
            }
            policy.CommitCommandList(cmd);
        }
        private static float ARcpF1(float v)
        {
            return 1 / v;
        }
        public static void FsrEasuCon(
            ref Vector4 con0,
            ref Vector4 con1,
            ref Vector4 con2,
            ref Vector4 con3,
            // This the rendered image resolution being upscaled
            float inputViewportInPixelsX,
            float inputViewportInPixelsY,
            // This is the resolution of the resource containing the input image (useful for dynamic resolution)
            float inputSizeInPixelsX,
            float inputSizeInPixelsY,
            // This is the display resolution which the input image gets upscaled to
            float outputSizeInPixelsX,
            float outputSizeInPixelsY)
        {
            // Output integer position to a pixel position in viewport.
            con0[0] = (float)(inputViewportInPixelsX * ARcpF1(outputSizeInPixelsX));
            con0[1] = (float)(inputViewportInPixelsY * ARcpF1(outputSizeInPixelsY));
            con0[2] = (float)(0.5f * inputViewportInPixelsX * ARcpF1(outputSizeInPixelsX) - 0.5f);
            con0[3] = (float)(0.5f * inputViewportInPixelsY * ARcpF1(outputSizeInPixelsY) - 0.5f);
            // Viewport pixel position to normalized image space.
            // This is used to get upper-left of 'F' tap.
            con1[0] = (float)(ARcpF1(inputSizeInPixelsX));
            con1[1] = (float)(ARcpF1(inputSizeInPixelsY));
            // Centers of gather4, first offset from upper-left of 'F'.
            //      +---+---+
            //      |   |   |
            //      +--(0)--+
            //      | b | c |
            //  +---F---+---+---+
            //  | e | f | g | h |
            //  +--(1)--+--(2)--+
            //  | i | j | k | l |
            //  +---+---+---+---+
            //      | n | o |
            //      +--(3)--+
            //      |   |   |
            //      +---+---+
            con1[2] = (float)((1.0f) * ARcpF1(inputSizeInPixelsX));
            con1[3] = (float)((-1.0f) * ARcpF1(inputSizeInPixelsY));
            // These are from (0) instead of 'F'.
            con2[0] = (float)((-1.0f) * ARcpF1(inputSizeInPixelsX));
            con2[1] = (float)((2.0f) * ARcpF1(inputSizeInPixelsY));
            con2[2] = (float)((1.0f) * ARcpF1(inputSizeInPixelsX));
            con2[3] = (float)((2.0f) * ARcpF1(inputSizeInPixelsY));
            con3[0] = (float)((0.0f) * ARcpF1(inputSizeInPixelsX));
            con3[1] = (float)((4.0f) * ARcpF1(inputSizeInPixelsY));
            con3[2] = con3[3] = 0;
        }
    }
}
