using System;
using System.Collections.Generic;
using System.ComponentModel;
using EngineNS.GamePlay;
using EngineNS.Graphics.Mesh;
using EngineNS.Graphics.Pipeline.Shader;
using EngineNS.Rtti;

namespace EngineNS.Graphics.Pipeline.Common.Post
{
    public class TtGaussShading : Shader.TtGraphicsShadingEnv
    {
        public TtGaussShading()
        {
            CodeName = RName.GetRName("shaders/ShadingEnv/Post/GaussShading.cginc", RName.ERNameType.Engine);

            this.UpdatePermutation();
        }
        public override NxRHI.EVertexStreamType[] GetNeedStreams()
        {
            return new NxRHI.EVertexStreamType[] { NxRHI.EVertexStreamType.VST_Position,
                NxRHI.EVertexStreamType.VST_UV,};
        }
        public override void OnDrawCall(NxRHI.ICommandList cmd, NxRHI.TtGraphicDraw drawcall, TtRenderPolicy policy, Graphics.Mesh.TtMesh.TtAtom atom)
        {
            var aaNode = drawcall.TagObject as TtGaussNode;
            if (aaNode == null)
            {
                var pipelinePolicy = policy.TagObject as TtRenderPolicy;
                aaNode = pipelinePolicy.FindFirstNode<TtGaussNode>();
            }

            var index = drawcall.FindBinder("ColorBuffer");
            if (index.IsValidPointer)
            {
                var attachBuffer = aaNode.GetAttachBuffer(aaNode.ColorPinIn);
                drawcall.BindSRV(index, attachBuffer.Srv);
            }
            index = drawcall.FindBinder("Samp_ColorBuffer");
            if (index.IsValidPointer)
                drawcall.BindSampler(index, TtEngine.Instance.GfxDevice.SamplerStateManager.LinearClampState);

            index = drawcall.FindBinder("cbShadingEnv");
            if (index.IsValidPointer)
            {
                if (aaNode.CBShadingEnv == null)
                {
                    aaNode.CBShadingEnv = TtEngine.Instance.GfxDevice.RenderContext.CreateCBV(index);
                }
                drawcall.BindCBuffer(index, aaNode.CBShadingEnv);
            }

            base.OnDrawCall(cmd, drawcall, policy, atom);
        }
    }
    [Bricks.CodeBuilder.ContextMenu("Gauss", "Post\\Gauss", Bricks.RenderPolicyEditor.UPolicyGraph.RGDEditorKeyword)]
    public class TtGaussNode : TtSceenSpaceNode
    {
        public TtRenderGraphPin ColorPinIn = TtRenderGraphPin.CreateInput("Color");
        public TtGaussNode()
        {
            Name = "GaussNode";

            mGaussStruct.SetDefault();
        }
        public override void InitNodePins()
        {
            AddInput(ColorPinIn, NxRHI.EBufferType.BFT_SRV);

            base.InitNodePins();
        }
        public TtGaussShading mBasePassShading;
        public override TtGraphicsShadingEnv GetPassShading(TtMesh.TtAtom atom = null)
        {
            return mBasePassShading;
        }
        public override async System.Threading.Tasks.Task Initialize(TtRenderPolicy policy, string debugName)
        {
            await base.Initialize(policy, debugName);
            mBasePassShading = await TtEngine.Instance.ShadingEnvManager.GetShadingEnv<TtGaussShading>();
        }
        public override void OnResize(TtRenderPolicy policy, float x, float y)
        {
            base.OnResize(policy, x, y);
        }
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 16)]
        struct FGaussStruct
        {
            public void SetDefault()
            {
                Stride = 1.0f;
                BlurSize = 5;
                BlurSigma = 1.0f;
            }
            public Vector2 StrideUV;
            public float Stride;
            public int BlurSize;

            public float BlurSigma;
        }
        FGaussStruct mGaussStruct;
        [Category("Option")]
        [Rtti.Meta]
        public float Stride
        {
            get => mGaussStruct.Stride;
            set => mGaussStruct.Stride = value;
        }
        [Category("Option")]
        [Rtti.Meta]
        public int BlurSize
        {
            get => mGaussStruct.BlurSize;
            set => mGaussStruct.BlurSize = value;
        }
        [Category("Option")]
        [Rtti.Meta]
        public float BlurSigma
        {
            get => mGaussStruct.BlurSigma;
            set => mGaussStruct.BlurSigma = value;
        }
        public NxRHI.TtCbView CBShadingEnv;
        public override void TickLogic(TtWorld world, TtRenderPolicy policy, bool bClear)
        {
            base.TickLogic(world, policy, bClear);
            if (CBShadingEnv != null)
            {
                var buffer = this.FindAttachBuffer(ColorPinIn);
                if (buffer != null)
                {
                    mGaussStruct.StrideUV.X = mGaussStruct.Stride * (1.0f / (float)buffer.BufferDesc.Width);
                    mGaussStruct.StrideUV.Y = mGaussStruct.Stride * (1.0f / (float)buffer.BufferDesc.Height);
                }
                else
                {
                    mGaussStruct.StrideUV.X = mGaussStruct.Stride * (1.0f / (float)policy.DefaultCamera.Width);
                    mGaussStruct.StrideUV.Y = mGaussStruct.Stride * (1.0f / (float)policy.DefaultCamera.Height);
                }
                CBShadingEnv.SetValue("GaussStruct", in mGaussStruct);
            }
        }
        public override void BeforeTickLogic(TtRenderPolicy policy)
        {
            var buffer = this.FindAttachBuffer(ColorPinIn);
            if (buffer != null)
            {
                if (ResultPinOut.Attachement.Format != buffer.BufferDesc.Format)
                {
                    this.CreateGBuffers(policy, buffer.BufferDesc.Format);
                    ResultPinOut.Attachement.Format = buffer.BufferDesc.Format;
                }
            }
        }
    }

    public class TtGaussAdditiveShading : Shader.TtGraphicsShadingEnv
    {
        public TtGaussAdditiveShading()
        {
            CodeName = RName.GetRName("shaders/ShadingEnv/Post/GaussAdditiveShading.cginc", RName.ERNameType.Engine);

            this.UpdatePermutation();
        }
        public override NxRHI.EVertexStreamType[] GetNeedStreams()
        {
            return new NxRHI.EVertexStreamType[] { NxRHI.EVertexStreamType.VST_Position,
                NxRHI.EVertexStreamType.VST_UV,};
        }
        public override void OnDrawCall(NxRHI.ICommandList cmd, NxRHI.TtGraphicDraw drawcall, TtRenderPolicy policy, Graphics.Mesh.TtMesh.TtAtom atom)
        {
            var aaNode = drawcall.TagObject as TtGaussAdditiveNode;
            if (aaNode == null)
            {
                var pipelinePolicy = policy.TagObject as TtRenderPolicy;
                aaNode = pipelinePolicy.FindFirstNode<TtGaussAdditiveNode>();
            }

            var index = drawcall.FindBinder("Color1Buffer");
            if (index.IsValidPointer)
            {
                var attachBuffer = aaNode.GetAttachBuffer(aaNode.Color1PinIn);
                drawcall.BindSRV(index, attachBuffer.Srv);
            }
            index = drawcall.FindBinder("Samp_Color1Buffer");
            if (index.IsValidPointer)
                drawcall.BindSampler(index, TtEngine.Instance.GfxDevice.SamplerStateManager.LinearClampState);

            index = drawcall.FindBinder("Color2Buffer");
            if (index.IsValidPointer)
            {
                var attachBuffer = aaNode.GetAttachBuffer(aaNode.Color2PinIn);
                drawcall.BindSRV(index, attachBuffer.Srv);
            }
            index = drawcall.FindBinder("Samp_Color2Buffer");
            if (index.IsValidPointer)
                drawcall.BindSampler(index, TtEngine.Instance.GfxDevice.SamplerStateManager.LinearClampState);

            index = drawcall.FindBinder("cbShadingEnv");
            if (index.IsValidPointer)
            {
                if (aaNode.CBShadingEnv == null)
                {
                    aaNode.CBShadingEnv = TtEngine.Instance.GfxDevice.RenderContext.CreateCBV(index);
                }
                drawcall.BindCBuffer(index, aaNode.CBShadingEnv);
            }

            base.OnDrawCall(cmd, drawcall, policy, atom);
        }
    }
    [Bricks.CodeBuilder.ContextMenu("GaussAdditive", "Post\\GaussAdditive", Bricks.RenderPolicyEditor.UPolicyGraph.RGDEditorKeyword)]
    public class TtGaussAdditiveNode : TtSceenSpaceNode
    {
        public TtRenderGraphPin Color1PinIn = TtRenderGraphPin.CreateInput("Color1");
        public TtRenderGraphPin Color2PinIn = TtRenderGraphPin.CreateInput("Color2");
        public TtGaussAdditiveNode()
        {
            Name = "GaussAdditiveNode";

            mGaussStruct.SetDefault();
        }
        public override void InitNodePins()
        {
            AddInput(Color1PinIn, NxRHI.EBufferType.BFT_SRV);
            AddInput(Color2PinIn, NxRHI.EBufferType.BFT_SRV);

            base.InitNodePins();
        }
        public TtGaussAdditiveShading mBasePassShading;
        public override TtGraphicsShadingEnv GetPassShading(TtMesh.TtAtom atom = null)
        {
            return mBasePassShading;
        }
        public override async System.Threading.Tasks.Task Initialize(TtRenderPolicy policy, string debugName)
        {
            await base.Initialize(policy, debugName);
            mBasePassShading = await TtEngine.Instance.ShadingEnvManager.GetShadingEnv<TtGaussAdditiveShading>();
        }
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 16)]
        struct FGaussStruct
        {
            public void SetDefault()
            {
                Stride1 = 1.0f;
                Stride2 = 1.0f;
                BlurSize = 5;
                BlurSigma = 1.0f;
            }
            public Vector2 StrideUV1;
            public Vector2 StrideUV2;
            public float Stride1;
            public float Stride2;
            public int BlurSize;
            public float BlurSigma;
        }
        FGaussStruct mGaussStruct;
        [Category("Option")]
        public float Stride1
        {
            get => mGaussStruct.Stride1;
            set => mGaussStruct.Stride1 = value;
        }
        [Category("Option")]
        public float Stride2
        {
            get => mGaussStruct.Stride2;
            set => mGaussStruct.Stride2 = value;
        }
        [Category("Option")]
        public int BlurSize
        {
            get => mGaussStruct.BlurSize;
            set => mGaussStruct.BlurSize = value;
        }
        [Category("Option")]
        public float BlurSigma
        {
            get => mGaussStruct.BlurSigma;
            set => mGaussStruct.BlurSigma = value;
        }
        public NxRHI.TtCbView CBShadingEnv;
        public override void TickLogic(TtWorld world, TtRenderPolicy policy, bool bClear)
        {
            base.TickLogic(world, policy, bClear);
            if (CBShadingEnv != null)
            {
                var buffer = this.FindAttachBuffer(Color1PinIn);
                if (buffer != null)
                {
                    mGaussStruct.StrideUV1.X = mGaussStruct.Stride1 * (1.0f / (float)buffer.BufferDesc.Width);
                    mGaussStruct.StrideUV1.Y = mGaussStruct.Stride1 * (1.0f / (float)buffer.BufferDesc.Height);
                }
                else
                {
                    mGaussStruct.StrideUV1.X = mGaussStruct.Stride1 * (1.0f / (float)policy.DefaultCamera.Width);
                    mGaussStruct.StrideUV1.Y = mGaussStruct.Stride1 * (1.0f / (float)policy.DefaultCamera.Height);
                }
                buffer = this.FindAttachBuffer(Color2PinIn);
                if (buffer != null)
                {
                    mGaussStruct.StrideUV2.X = mGaussStruct.Stride2 * (1.0f / (float)buffer.BufferDesc.Width);
                    mGaussStruct.StrideUV2.Y = mGaussStruct.Stride2 * (1.0f / (float)buffer.BufferDesc.Height);
                }
                else
                {
                    mGaussStruct.StrideUV2.X = mGaussStruct.Stride2 * (1.0f / (float)policy.DefaultCamera.Width);
                    mGaussStruct.StrideUV2.Y = mGaussStruct.Stride2 * (1.0f / (float)policy.DefaultCamera.Height);
                }
                CBShadingEnv.SetValue("GaussStruct", in mGaussStruct);
            }
        }
        public override void TickSync(TtRenderPolicy policy)
        {
            base.TickSync(policy);
        }
        public override void BeforeTickLogic(TtRenderPolicy policy)
        {
            var buffer = this.FindAttachBuffer(Color1PinIn);
            if (buffer != null)
            {
                if (ResultPinOut.Attachement.Format != buffer.BufferDesc.Format)
                {
                    this.CreateGBuffers(policy, buffer.BufferDesc.Format);
                    ResultPinOut.Attachement.Format = buffer.BufferDesc.Format;
                }
            }
        }
    }
}
