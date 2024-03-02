using System;
using System.Collections.Generic;
using System.ComponentModel;
using EngineNS.GamePlay;
using EngineNS.Graphics.Mesh;
using EngineNS.Graphics.Pipeline.Shader;
using EngineNS.NxRHI;

namespace EngineNS.Graphics.Pipeline.Common.Post
{
    //https://zhuanlan.zhihu.com/p/465854298
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 16)]
    struct FSunShaftStruct
    {
        public void SetDefault()
        {
            LumThreshold = 0.8f;
            DepthThreshole = 0.99f;
            SunPosition.W = 1.0f;
            BlurDecay = 0.5f;
            BlurSampleCount = 4;
            BlurRadius4 = new Vector4(1, 1, 0, 0);
        }
        public Vector4 BlurRadius4;
        public Vector4 SunPosition;

        public float LumThreshold;
        public float DepthThreshole;
        public float BlurDecay;
        public int BlurSampleCount;
    }
    public class TtDepthThresholeShading : Shader.UGraphicsShadingEnv
    {
        public TtDepthThresholeShading()
        {
            CodeName = RName.GetRName("shaders/ShadingEnv/Post/SunShaft/DepthThresholeShading.cginc", RName.ERNameType.Engine);

            this.UpdatePermutation();
        }
        public override NxRHI.EVertexStreamType[] GetNeedStreams()
        {
            return new NxRHI.EVertexStreamType[] { NxRHI.EVertexStreamType.VST_Position,
                NxRHI.EVertexStreamType.VST_UV,};
        }
        public override void OnDrawCall(NxRHI.ICommandList cmd, UGraphicDraw drawcall, URenderPolicy policy, TtMesh.TtAtom atom)
        {
            var aaNode = drawcall.TagObject as TtSunShaftDepthThresholeNode;
            if (aaNode == null)
            {
                var pipelinePolicy = policy.TagObject as URenderPolicy;
                aaNode = pipelinePolicy.FindFirstNode<TtSunShaftDepthThresholeNode>();
            }

            var index = drawcall.FindBinder("ColorBuffer");
            if (index.IsValidPointer)
            {
                var attachBuffer = aaNode.GetAttachBuffer(aaNode.ColorPinIn);
                drawcall.BindSRV(index, attachBuffer.Srv);
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

            index = drawcall.FindBinder("cbShadingEnv");
            if (index.IsValidPointer)
            {
                if (aaNode.CBShadingEnv == null)
                {
                    aaNode.CBShadingEnv = UEngine.Instance.GfxDevice.RenderContext.CreateCBV(index);
                }
                drawcall.BindCBuffer(index, aaNode.CBShadingEnv);
            }

            base.OnDrawCall(cmd, drawcall, policy, atom);
        }
    }
    [Bricks.CodeBuilder.ContextMenu("SunShaftDepthThreshole", "Post\\SunShaftDepthThreshole", Bricks.RenderPolicyEditor.UPolicyGraph.RGDEditorKeyword)]
    public class TtSunShaftDepthThresholeNode : USceenSpaceNode
    {
        public TtRenderGraphPin ColorPinIn = TtRenderGraphPin.CreateInput("Color");
        public TtRenderGraphPin DepthPinIn = TtRenderGraphPin.CreateInput("Depth");
        public TtSunShaftDepthThresholeNode()
        {
            Name = "SunShaftDepthThreshole";

            mSunShaftStruct.SetDefault();
        }
        public override void InitNodePins()
        {
            AddInput(ColorPinIn, NxRHI.EBufferType.BFT_SRV);
            AddInput(DepthPinIn, NxRHI.EBufferType.BFT_SRV);

            base.InitNodePins();
            ResultPinOut.Attachement.Format = EPixelFormat.PXF_R16_FLOAT;
        }
        public TtDepthThresholeShading mBasePassShading;
        public override UGraphicsShadingEnv GetPassShading(TtMesh.TtAtom atom = null)
        {
            return mBasePassShading;
        }
        public override async System.Threading.Tasks.Task Initialize(URenderPolicy policy, string debugName)
        {
            await base.Initialize(policy, debugName);
            mBasePassShading = UEngine.Instance.ShadingEnvManager.GetShadingEnv<TtDepthThresholeShading>();
        }

        FSunShaftStruct mSunShaftStruct = new FSunShaftStruct();
        [Category("Option")]
        [Rtti.Meta]
        [EGui.Controls.PropertyGrid.PGValueRange(0, 1)]
        [EGui.Controls.PropertyGrid.PGValueChangeStep(0.001f)]
        public float DepthThreshole
        {
            get => mSunShaftStruct.DepthThreshole;
            set => mSunShaftStruct.DepthThreshole = value;
        }
        [Category("Option")]
        [Rtti.Meta]
        [EGui.Controls.PropertyGrid.PGValueRange(0, 32)]
        [EGui.Controls.PropertyGrid.PGValueChangeStep(0.1f)]
        public float LumThreshold
        {
            get => mSunShaftStruct.LumThreshold;
            set
            {
                mSunShaftStruct.LumThreshold = value;
            }
        }
        [Category("Option")]
        [Rtti.Meta]
        public float MaxBlurRadius
        {
            get => mSunShaftStruct.SunPosition.W;
            set
            {
                mSunShaftStruct.SunPosition.W = value;
            }
        }
        public NxRHI.UCbView CBShadingEnv;
        public override void TickLogic(UWorld world, URenderPolicy policy, bool bClear)
        {
            base.TickLogic(world, policy, bClear);

            var toViewport = policy.DefaultCamera.GetViewProjection();
            var clipPos = Vector3.Transform(new Vector3(100,100,100), in toViewport);
            mSunShaftStruct.SunPosition.X = clipPos.X / clipPos.W * 0.5f + 0.5f;
            mSunShaftStruct.SunPosition.Y = clipPos.Y / clipPos.W * 0.5f + 0.5f;
            mSunShaftStruct.SunPosition.Z = clipPos.Z / clipPos.W;
            mSunShaftStruct.SunPosition.X = 0.5f;
            mSunShaftStruct.SunPosition.Y = 0.5f;
            if (CBShadingEnv != null)
            {
                CBShadingEnv.SetValue("SunShaftStruct", in mSunShaftStruct);
            }
        }
        public override void TickSync(URenderPolicy policy)
        {
            base.TickSync(policy);
        }
        public override void BeforeTickLogic(URenderPolicy policy)
        {
            //var buffer = this.FindAttachBuffer(ColorPinIn);
            //if (buffer != null)
            //{
            //    //ResultPinOut.Attachement.Format = buffer.BufferDesc.Format;
            //    ResultPinOut.Attachement.Width = buffer.BufferDesc.Width / 2;
            //    ResultPinOut.Attachement.Height = buffer.BufferDesc.Height / 2;
            //}
        }
    }

    public class TtRadialBlurShading : Shader.UGraphicsShadingEnv
    {
        public TtRadialBlurShading()
        {
            CodeName = RName.GetRName("shaders/ShadingEnv/Post/SunShaft/RadialBlurShading.cginc", RName.ERNameType.Engine);

            this.UpdatePermutation();
        }
        public override NxRHI.EVertexStreamType[] GetNeedStreams()
        {
            return new NxRHI.EVertexStreamType[] { NxRHI.EVertexStreamType.VST_Position,
                NxRHI.EVertexStreamType.VST_UV,};
        }
        public override void OnDrawCall(NxRHI.ICommandList cmd, UGraphicDraw drawcall, URenderPolicy policy, TtMesh.TtAtom atom)
        {
            var aaNode = drawcall.TagObject as TtSunShaftRadialBlurNode;
            if (aaNode == null)
            {
                System.Diagnostics.Debug.Assert(false);
            }

            var index = drawcall.FindBinder("ColorBuffer");
            if (index.IsValidPointer)
            {
                var attachBuffer = aaNode.GetAttachBuffer(aaNode.ColorPinIn);
                drawcall.BindSRV(index, attachBuffer.Srv);
            }
            index = drawcall.FindBinder("Samp_ColorBuffer");
            if (index.IsValidPointer)
                drawcall.BindSampler(index, UEngine.Instance.GfxDevice.SamplerStateManager.LinearClampState);

            index = drawcall.FindBinder("cbShadingEnv");
            if (index.IsValidPointer)
            {
                if (aaNode.CBShadingEnv == null)
                {
                    aaNode.CBShadingEnv = UEngine.Instance.GfxDevice.RenderContext.CreateCBV(index);
                }
                drawcall.BindCBuffer(index, aaNode.CBShadingEnv);
            }

            base.OnDrawCall(cmd, drawcall, policy, atom);
        }
    }
    [Bricks.CodeBuilder.ContextMenu("SunShaftRadialBlur", "Post\\SunShaftRadialBlur", Bricks.RenderPolicyEditor.UPolicyGraph.RGDEditorKeyword)]
    public class TtSunShaftRadialBlurNode : USceenSpaceNode
    {
        public TtRenderGraphPin ColorPinIn = TtRenderGraphPin.CreateInput("Color");
        public TtSunShaftRadialBlurNode()
        {
            Name = "SunShaftRadialBlurNode";

            mSunShaftStruct.SetDefault();
        }
        public override void InitNodePins()
        {
            AddInput(ColorPinIn, NxRHI.EBufferType.BFT_SRV);

            base.InitNodePins();
            ResultPinOut.IsAutoResize = false;
            ResultPinOut.Attachement.Format = EPixelFormat.PXF_R16_FLOAT;
        }
        public TtRadialBlurShading mBasePassShading;
        public override UGraphicsShadingEnv GetPassShading(TtMesh.TtAtom atom = null)
        {
            return mBasePassShading;
        }
        public override async System.Threading.Tasks.Task Initialize(URenderPolicy policy, string debugName)
        {
            await base.Initialize(policy, debugName);
            mBasePassShading = UEngine.Instance.ShadingEnvManager.GetShadingEnv<TtRadialBlurShading>();
        }

        FSunShaftStruct mSunShaftStruct = new FSunShaftStruct();
        
        [Category("Option")]
        [Rtti.Meta]
        [EGui.Controls.PropertyGrid.PGValueRange(0, 1)]
        [EGui.Controls.PropertyGrid.PGValueChangeStep(0.001f)]
        public float BlurDecay
        {
            get => mSunShaftStruct.BlurDecay;
            set
            {
                mSunShaftStruct.BlurDecay = value;
            }
        }
        [Category("Option")]
        [Rtti.Meta]
        [EGui.Controls.PropertyGrid.PGValueRange(0, 32)]
        [EGui.Controls.PropertyGrid.PGValueChangeStep(0.1f)]
        public Vector2 BlurRadius
        {
            get => new Vector2(mSunShaftStruct.BlurRadius4.X, mSunShaftStruct.BlurRadius4.Y);
            set
            {
                mSunShaftStruct.BlurRadius4.X = value.X;
                mSunShaftStruct.BlurRadius4.Y = value.Y;
            }
        }
        public NxRHI.UCbView CBShadingEnv;
        public override void TickLogic(UWorld world, URenderPolicy policy, bool bClear)
        {
            var toViewport = policy.DefaultCamera.GetViewProjection();
            var clipPos = Vector3.Transform(new Vector3(100, 100, 100), in toViewport);
            mSunShaftStruct.SunPosition.X = clipPos.X / clipPos.W * 0.5f + 0.5f;
            mSunShaftStruct.SunPosition.Y = clipPos.Y / clipPos.W * 0.5f + 0.5f;
            mSunShaftStruct.SunPosition.Z = clipPos.Z / clipPos.W;
            mSunShaftStruct.SunPosition.X = 0.5f;
            mSunShaftStruct.SunPosition.Y = 0.5f;
            if (CBShadingEnv != null)
            {
                CBShadingEnv.SetValue("SunShaftStruct", in mSunShaftStruct);
            }
            base.TickLogic(world, policy, bClear);
        }
        public override void TickSync(URenderPolicy policy)
        {
            base.TickSync(policy);
        }
        public override void BeforeTickLogic(URenderPolicy policy)
        {
            var buffer = this.FindAttachBuffer(ColorPinIn);
            if (buffer != null)
            {
                GBuffers.SetSize(buffer.BufferDesc.Width, buffer.BufferDesc.Height);
                
                ResultPinOut.Attachement.Format = buffer.BufferDesc.Format;
                ResultPinOut.Attachement.Width = buffer.BufferDesc.Width;
                ResultPinOut.Attachement.Height = buffer.BufferDesc.Height;
            }
        }
    }
}
