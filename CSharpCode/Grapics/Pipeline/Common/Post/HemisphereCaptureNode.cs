using EngineNS.GamePlay;
using EngineNS.Graphics.Pipeline.Shader;
using EngineNS.NxRHI;
using System;
using System.ComponentModel;

namespace EngineNS.Graphics.Pipeline.Common.Post
{
    public class TtHemisphereCaptureShading : TtComputeShadingEnv
    {
        public override Vector3ui DispatchArg => new Vector3ui(8, 8, 1);

        public TtHemisphereCaptureShading()
        {
            CodeName = RName.GetRName("Shaders/ShadingEnv/Post/HemisphereCaptureGen.compute", RName.ERNameType.Engine);
            MainName = "CS_GenerateHemisphereMap";
            UpdatePermutation().AddWaitTask();
        }

        public override void OnDrawCall(TtComputeDraw drawcall, TtRenderPolicy policy)
        {
            var node = drawcall.TagObject as TtHemisphereCaptureNode;
            if (node == null)
                return;

            drawcall.BindSrv("InputSkyCapture", node.GetInputCaptureSrv());
            drawcall.BindUav("OutputHemisphereMap", node.GetOutputHemisphereUav());
            drawcall.BindSampler("SampLinearClamp", TtEngine.Instance.GfxDevice.SamplerStateManager.LinearClampState);

            var cbBinder = drawcall.FindBinder(EShaderBindType.SBT_CBV, "cbHemisphereCapture");
            if (cbBinder.IsValidPointer)
                drawcall.BindCBV(cbBinder, node.GetOrCreateCBuffer(cbBinder));
        }
    }

    [Bricks.CodeBuilder.ContextMenu("HemisphereCapture", "Post\\HemisphereCapture", Bricks.RenderPolicyEditor.TtPolicyGraph.RGDEditorKeyword)]
    public class TtHemisphereCaptureNode : TAuxRenderGraphNode<TtHemisphereCaptureNode>
    {
        // ---------- Pins ----------
        public TtRenderGraphPin SkyCaptureIn = TtRenderGraphPin.CreateInput("SkyCapture", EBufferType.BFT_SRV);
        public TtRenderGraphPin HemisphereMapOut = TtRenderGraphPin.CreateOutput(
            "HemisphereMap", true, EPixelFormat.PXF_R11G11B10_FLOAT,
            EBufferType.BFT_SRV | EBufferType.BFT_UAV);

        // ---------- Tunable ----------
        [Category("HemisphereCapture")]
        [Rtti.Meta("")]
        public uint OutputResolution { get; set; } = 256;

        [Category("HemisphereCapture")]
        [Rtti.Meta("")]
        public float CaptureIntensity { get; set; } = 1.0f;

        [Category("HemisphereCapture")]
        [Rtti.Meta("")]
        public float MaxMipLevel { get; set; } = 5.0f;

        [Category("HemisphereCapture")]
        [Rtti.Meta("")]
        public Color4f SkyTintColor { get; set; } = new Color4f(1.0f, 1.0f, 1.0f, 1.0f);

        [Category("HemisphereCapture")]
        [Rtti.Meta("")]
        public Matrix CaptureRotation { get; set; } = Matrix.Identity;

        // ---------- Internal ----------
        TtHemisphereCaptureShading mShading;
        TtComputeDraw mDrawCall;
        TtCbView mCBuffer;
        uint mCurrentResolution = 0;

        // 输入/输出暂存 (Tick 写, OnDrawCall 读)
        TtSrView mInputCaptureSrv;
        TtUaView mOutputHemisphereUav;

        public TtSrView GetInputCaptureSrv() => mInputCaptureSrv;
        public TtUaView GetOutputHemisphereUav() => mOutputHemisphereUav;

        public TtHemisphereCaptureNode()
        {
            Name = "HemisphereCaptureNode";
        }

        public override void InitNodePins()
        {
            AddInput(SkyCaptureIn);
            AddOutput(HemisphereMapOut);
            base.InitNodePins();
        }

        public override async Thread.Async.TtTask Initialize(TtRenderPolicy policy, string debugName)
        {
            await base.Initialize(policy, debugName);
            mShading = await TtShadingEnv.CreateShadingEnv<TtHemisphereCaptureShading>();

            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            mDrawCall = rc.CreateComputeDraw();
            mDrawCall.TagObject = this;
        }

        public override void Dispose()
        {
            CoreSDK.DisposeObject(ref mDrawCall);
            CoreSDK.DisposeObject(ref mCBuffer);
            base.Dispose();
        }

        public override void OnResize(TtRenderPolicy policy, float x, float y)
        {
            uint resolution = Math.Max(OutputResolution, 16u);
            if (resolution == mCurrentResolution)
                return;
            mCurrentResolution = resolution;

            HemisphereMapOut.Attachement.Width = resolution;
            HemisphereMapOut.Attachement.Height = resolution;

            // 释放旧 cbuffer, 下次 OnDrawCall 会重建
            CoreSDK.DisposeObject(ref mCBuffer);
        }

        public TtCbView GetOrCreateCBuffer(FShaderBinder binder)
        {
            uint resolution = Math.Max(OutputResolution, 16u);
            if (mCBuffer == null)
            {
                mCBuffer = TtEngine.Instance.GfxDevice.RenderContext.CreateCBV(binder);
                mCBuffer.SetValue("OutputWidth", resolution);
                mCBuffer.SetValue("OutputHeight", resolution);
                mCBuffer.SetValue("CaptureIntensity", CaptureIntensity);
                mCBuffer.SetValue("MaxMipLevel", MaxMipLevel);
                var tint = new Vector4(SkyTintColor.Red, SkyTintColor.Green, SkyTintColor.Blue, SkyTintColor.Alpha);
                mCBuffer.SetValue("SkyTintColor", in tint);
                var rotation = CaptureRotation;
                mCBuffer.SetValue("CaptureRotation", in rotation);
                mCBuffer.MarkDirty();
                mCBuffer.FlushDirty();
                return mCBuffer;
            }

            mCBuffer.SetValue("OutputWidth", resolution);
            mCBuffer.SetValue("OutputHeight", resolution);
            mCBuffer.SetValue("CaptureIntensity", CaptureIntensity);
            mCBuffer.SetValue("MaxMipLevel", MaxMipLevel);
            var tintVal = new Vector4(SkyTintColor.Red, SkyTintColor.Green, SkyTintColor.Blue, SkyTintColor.Alpha);
            mCBuffer.SetValue("SkyTintColor", in tintVal);
            var rotationVal = CaptureRotation;
            mCBuffer.SetValue("CaptureRotation", in rotationVal);
            return mCBuffer;
        }

        public override unsafe void Tick(TtWorld world, TtRenderPolicy policy, TtCommandList frameCmdList, bool bClear)
        {
            if (mShading == null || !mShading.IsReady)
                return;

            var inputAttach = GetAttachBuffer(SkyCaptureIn);
            if (inputAttach == null || inputAttach.Srv == null)
                return;

            var outputAttach = GetAttachBuffer(HemisphereMapOut);
            if (outputAttach == null || outputAttach.Uav == null)
                return;

            uint resolution = Math.Max(OutputResolution, 16u);
            if (resolution != mCurrentResolution)
                OnResize(policy, resolution, resolution);

            // 暂存 SRV/UAV 供 OnDrawCall 读取
            mInputCaptureSrv = inputAttach.Srv;
            mOutputHemisphereUav = outputAttach.Uav;

            var cmd = NxRHI.TtCommandList.GetCmdList();
            using (new TtCmdListScope(cmd, "HemisphereCapture"))
            {
                mShading.SetDrawcallDispatch(this, policy, mDrawCall, resolution, resolution, 1, true);
                cmd.PushGpuDraw(mDrawCall);
                cmd.FlushDraws();
            }
            policy.CommitCommandList(cmd, "HemisphereCapture");
        }
    }
}
