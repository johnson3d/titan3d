﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using EngineNS.GamePlay;
using EngineNS.Graphics.Mesh;
using EngineNS.Graphics.Pipeline.Shader;
using EngineNS.NxRHI;

namespace EngineNS.Graphics.Pipeline.Common.Post
{
    public class TtLuminanceThresholeShading : Shader.TtGraphicsShadingEnv
    {
        public TtLuminanceThresholeShading()
        {
            CodeName = RName.GetRName("shaders/ShadingEnv/Post/LuminanceThresholeShading.cginc", RName.ERNameType.Engine);

            this.UpdatePermutation();
        }
        public override NxRHI.EVertexStreamType[] GetNeedStreams()
        {
            return new NxRHI.EVertexStreamType[] { NxRHI.EVertexStreamType.VST_Position,
                NxRHI.EVertexStreamType.VST_UV,};
        }
        protected override void EnvShadingDefines(in FPermutationId id, TtShaderDefinitions defines)
        {
            defines.AddDefine("ENV_OUT_COLOR", "1");
        }
        public override void OnDrawCall(NxRHI.ICommandList cmd, TtGraphicDraw drawcall, TtRenderPolicy policy, TtMesh.TtAtom atom)
        {
            var aaNode = drawcall.TagObject as TtLuminanceThresholeNode;
            if (aaNode == null)
            {
                var pipelinePolicy = policy.TagObject as TtRenderPolicy;
                aaNode = pipelinePolicy.FindFirstNode<TtLuminanceThresholeNode>();
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
    [Bricks.CodeBuilder.ContextMenu("LuminanceThreshole", "Post\\LuminanceThreshole", Bricks.RenderPolicyEditor.UPolicyGraph.RGDEditorKeyword)]
    public class TtLuminanceThresholeNode : TtSceenSpaceNode
    {
        public TtRenderGraphPin ColorPinIn = TtRenderGraphPin.CreateInput("Color");
        public TtLuminanceThresholeNode()
        {
            Name = "LuminanceThresholeNode";

            mLuminanceThresholeStruct.SetDefault();
        }
        public override void InitNodePins()
        {
            AddInput(ColorPinIn, NxRHI.EBufferType.BFT_SRV);

            base.InitNodePins();
        }
        public TtLuminanceThresholeShading mBasePassShading;
        public override TtGraphicsShadingEnv GetPassShading(TtMesh.TtAtom atom = null)
        {
            return mBasePassShading;
        }
        public override async System.Threading.Tasks.Task Initialize(TtRenderPolicy policy, string debugName)
        {
            await base.Initialize(policy, debugName);
            mBasePassShading = await TtEngine.Instance.ShadingEnvManager.GetShadingEnv<TtLuminanceThresholeShading>();
        }
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 16)]
        struct FLuminanceThresholeStruct
        {
            public void SetDefault()
            {
                Threshole = 1.0f;
            }
            public float Threshole;
        }
        FLuminanceThresholeStruct mLuminanceThresholeStruct = new FLuminanceThresholeStruct();
        [Category("Option")]
        [Rtti.Meta]
        public float Threshole
        {
            get => mLuminanceThresholeStruct.Threshole;
            set => mLuminanceThresholeStruct.Threshole = value;
        }
        public NxRHI.TtCbView CBShadingEnv;
        public override void TickLogic(TtWorld world, TtRenderPolicy policy, bool bClear)
        {
            base.TickLogic(world, policy, bClear);
            if (CBShadingEnv != null)
            {
                CBShadingEnv.SetValue("LuminanceThresholeStruct", in mLuminanceThresholeStruct);
            }
        }
        public override void TickSync(TtRenderPolicy policy)
        {
            base.TickSync(policy);
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

    public class TtLuminanceThresholeOutLumShading : TtLuminanceThresholeShading
    {
        public override string ToString()
        {
            return base.ToString() + "[OutLum]";
        }
        protected override void EnvShadingDefines(in FPermutationId id, TtShaderDefinitions defines)
        {
            defines.AddDefine("ENV_OUT_LUMINANCE", "1");
        }
    }
    [Bricks.CodeBuilder.ContextMenu("LuminanceThresholeOutLum", "Post\\LuminanceThresholeOutLum", Bricks.RenderPolicyEditor.UPolicyGraph.RGDEditorKeyword)]
    public class TtLuminanceThresholeOutLumNode : TtLuminanceThresholeNode
    {
        public TtLuminanceThresholeOutLumNode()
        {
            Name = "LuminanceThresholeOutLumNode";
        }
        public TtLuminanceThresholeOutLumShading mLuminanceShading;
        public override TtGraphicsShadingEnv GetPassShading(TtMesh.TtAtom atom = null)
        {
            return mLuminanceShading;
        }
        public override async System.Threading.Tasks.Task Initialize(TtRenderPolicy policy, string debugName)
        {
            await base.Initialize(policy, debugName);
            mLuminanceShading = await TtEngine.Instance.ShadingEnvManager.GetShadingEnv<TtLuminanceThresholeOutLumShading>();
        }
        public override void BeforeTickLogic(TtRenderPolicy policy)
        {
            var buffer = this.FindAttachBuffer(ColorPinIn);
            if (buffer != null)
            {
                //ResultPinOut.Attachement.Format = EPixelFormat.PXF_R16_FLOAT;
                if (ResultPinOut.Attachement.Format != EPixelFormat.PXF_R16_FLOAT)
                {
                    this.CreateGBuffers(policy, EPixelFormat.PXF_R16_FLOAT);
                    ResultPinOut.Attachement.Format = EPixelFormat.PXF_R16_FLOAT;
                }
            }
        }
    }
}
