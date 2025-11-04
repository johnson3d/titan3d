using Assimp;
using EngineNS.Bricks.VXGI;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Common
{
    [Bricks.CodeBuilder.ContextMenu("AvgBrightness", "AvgBrightness", Bricks.RenderPolicyEditor.UPolicyGraph.RGDEditorKeyword)]
    [Rtti.Meta("",NameAlias = new string[] { "EngineNS.Graphics.Pipeline.Common.UAvgBrightnessNode@EngineCore", "EngineNS.Graphics.Pipeline.Common.UAvgBrightnessNode" })]
    public class TtAvgBrightnessNode : TAuxRenderGraphNode<TtAvgBrightnessNode>
    {
        public override void Dispose()
        {
            CoreSDK.DisposeObject(ref SetupAvgBrightnessDrawcall);
            CoreSDK.DisposeObject(ref CountAvgBrightnessDrawcall);
            base.Dispose();
        }
        public TtRenderGraphPin GpuScenePinInOut = TtRenderGraphPin.CreateInputOutput("GpuScene", NxRHI.EBufferType.BFT_UAV);
        public TtRenderGraphPin ColorPinIn = TtRenderGraphPin.CreateInputOutput("Color", NxRHI.EBufferType.BFT_SRV);
        public TtAvgBrightnessNode()
        {
            Name = "AvgBrightnessNode";
        }
        public override void InitNodePins()
        {
            AddInputOutput(GpuScenePinInOut);
            AddInputOutput(ColorPinIn);
        }
        public static readonly Vector3ui Dispatch_SetupDimArray1 = new Vector3ui(1, 1, 1);
        public static readonly Vector3ui Dispatch_SetupDimArray2 = new Vector3ui(32, 32, 1);

        public class SetupAvgBrightnessShading : Graphics.Pipeline.Shader.TtComputeShadingEnv
        {
            public override Vector3ui DispatchArg
            {
                get => new Vector3ui(32, 32, 1);
            }
            public SetupAvgBrightnessShading()
            {
                CodeName = RName.GetRName("Shaders/Compute/ScreenSpace/AvgBrightness.compute", RName.ERNameType.Engine);
                MainName = "CS_SetupAvgBrightness";

                this.UpdatePermutation();
            }
            protected override void EnvShadingDefines(in FPermutationId id, NxRHI.TtShaderDefinitions defines)
            {
                base.EnvShadingDefines(in id, defines);
            }
            public override void OnDrawCall(NxRHI.TtComputeDraw drawcall, Graphics.Pipeline.TtRenderPolicy policy)
            {
                var gpuScene = policy.GetGpuSceneNode();
                var node = drawcall.TagObject as TtAvgBrightnessNode;
                var srvIdx = drawcall.FindBinder(NxRHI.EShaderBindType.SBT_CBV, "cbPerFrame");
                if (srvIdx.IsValidPointer)
                {
                    drawcall.BindCBV(srvIdx, TtEngine.Instance.GfxDevice.PerFrameCBuffer);
                }
                srvIdx = drawcall.FindBinder(NxRHI.EShaderBindType.SBT_CBV, "cbPerGpuScene");
                if (srvIdx.IsValidPointer)
                {
                    drawcall.BindCBV(srvIdx, gpuScene.PerGpuSceneCbv);
                }
                srvIdx = drawcall.FindBinder(NxRHI.EShaderBindType.SBT_UAV, "GpuSceneDesc");
                if (srvIdx.IsValidPointer)
                {
                    var attachment = node.GetAttachBuffer(node.GpuScenePinInOut);
                    drawcall.BindUav(srvIdx, attachment.Uav);
                }
            }
        }
        private SetupAvgBrightnessShading SetupAvgBrightness;
        private NxRHI.TtComputeDraw SetupAvgBrightnessDrawcall;

        public class CountAvgBrightnessShading : Graphics.Pipeline.Shader.TtComputeShadingEnv
        {
            public override Vector3ui DispatchArg
            {
                get => new Vector3ui(32, 32, 1);
            }
            public CountAvgBrightnessShading()
            {
                CodeName = RName.GetRName("Shaders/Compute/ScreenSpace/AvgBrightness.compute", RName.ERNameType.Engine);
                MainName = "CS_CountAvgBrightness";

                this.UpdatePermutation();
            }
            protected override void EnvShadingDefines(in FPermutationId id, NxRHI.TtShaderDefinitions defines)
            {
                base.EnvShadingDefines(in id, defines);
            }
            public override void OnDrawCall(NxRHI.TtComputeDraw drawcall, Graphics.Pipeline.TtRenderPolicy policy)
            {
                var node = drawcall.TagObject as TtAvgBrightnessNode;
                var attachment = node.GetAttachBuffer(node.ColorPinIn);
                var srvIdx = drawcall.FindBinder(NxRHI.EShaderBindType.SBT_SRV, "TargetBuffer");
                if (srvIdx.IsValidPointer)
                {
                    drawcall.BindSrv(srvIdx, attachment.Srv);
                }
                srvIdx = drawcall.FindBinder(NxRHI.EShaderBindType.SBT_UAV, "GpuSceneDesc");
                if (srvIdx.IsValidPointer)
                {
                    attachment = node.GetAttachBuffer(node.GpuScenePinInOut);
                    drawcall.BindUav(srvIdx, attachment.Uav);
                }
                srvIdx = drawcall.FindBinder(NxRHI.EShaderBindType.SBT_CBV, "ParameterBuffer");
                if (srvIdx.IsValidPointer)
                {
                    if (node.ParameterCBuffer == null)
                    {
                        node.ParameterCBuffer = TtEngine.Instance.GfxDevice.RenderContext.CreateCBV(srvIdx);
                        node.MinValidLuminance = node.MinValidLuminance;
                        node.MaxValidLuminance = node.MaxValidLuminance;
                        node.LuminancePower = node.LuminancePower;
                    }
                    drawcall.BindCBV(srvIdx, node.ParameterCBuffer);
                }
            }
        }
        private CountAvgBrightnessShading CountAvgBrightness;
        private NxRHI.TtComputeDraw CountAvgBrightnessDrawcall;
        private NxRHI.TtCbView ParameterCBuffer;
        float mMinValidLuminance = 0.01f;
        [Rtti.Meta("")]
        public float MinValidLuminance
        {
            get => mMinValidLuminance;
            set 
            { 
                mMinValidLuminance = value;
                ParameterCBuffer.SetValue("MinValidLuminance", value);
            }
        }
        float mMaxValidLuminance = 3.5f;
        [Rtti.Meta("")]
        public float MaxValidLuminance
        {
            get => mMaxValidLuminance;
            set
            {
                mMaxValidLuminance = value;
                ParameterCBuffer.SetValue("MaxValidLuminance", value);
            }
        }
        float mLuminancePower = 0.8f;
        [Rtti.Meta("")]
        public float LuminancePower
        {
            get => mLuminancePower;
            set
            {
                mLuminancePower = value;
                ParameterCBuffer.SetValue("LuminancePower", value);
            }
        }
        public override async Thread.Async.TtTask Initialize(TtRenderPolicy policy, string debugName)
        {
            await Thread.TtAsyncDummyClass.DummyFunc();

            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            
            CountAvgBrightness = await TtEngine.Instance.ShadingEnvManager.GetShadingEnv<CountAvgBrightnessShading>();
            SetupAvgBrightness = await TtEngine.Instance.ShadingEnvManager.GetShadingEnv<SetupAvgBrightnessShading>();

            ResetComputeDrawcall(policy);
        }
        private unsafe void ResetComputeDrawcall(TtRenderPolicy policy)
        {
            if (SetupAvgBrightness == null)
                return;
            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            var gpuScene = policy.GetGpuSceneNode();

            CoreSDK.DisposeObject(ref SetupAvgBrightnessDrawcall);
            SetupAvgBrightnessDrawcall = rc.CreateComputeDraw();
            
            //var lightSRV = policy.QuerySRV("LightRT");
            //if (lightSRV != null)
            {
                CoreSDK.DisposeObject(ref CountAvgBrightnessDrawcall);
                CountAvgBrightnessDrawcall = rc.CreateComputeDraw();
            }
        }
        public override void OnResize(TtRenderPolicy policy, float x, float y)
        {
            ResetComputeDrawcall(policy);
        }
        public unsafe override void TickLogic(GamePlay.TtWorld world, TtRenderPolicy policy, NxRHI.TtCommandList frameCmdList, bool bClear)
        {
            var gpuScene = policy.GetGpuSceneNode();

            var cmd = TtEngine.Instance.GfxDevice.RenderContext.CmdListManager.GetCmdList();

            using (new NxRHI.TtCmdListScope(cmd, "AvgBrightness"))
            {
                #region Setup
                {
                    SetupAvgBrightness.SetDrawcallDispatch(this, policy, SetupAvgBrightnessDrawcall, 1, 1, 1, true);
                    cmd.PushGpuDraw(SetupAvgBrightnessDrawcall);
                }
                #endregion

                #region Count
                {
                    if (CountAvgBrightnessDrawcall != null)
                    {
                        var attachment = this.GetAttachBuffer(this.ColorPinIn);
                        uint targetWidth = (uint)attachment.BufferDesc.Width;
                        uint targetHeight = (uint)attachment.BufferDesc.Height;
                        CountAvgBrightness.SetDrawcallDispatch(this, policy, CountAvgBrightnessDrawcall,
                            targetWidth,
                            targetHeight,
                            1, true);
                        //CountAvgBrightnessDrawcall.Commit(cmd);
                        cmd.PushGpuDraw(CountAvgBrightnessDrawcall);
                    }
                }
                #endregion

                cmd.FlushDraws();
            }

            policy.CommitCommandList(cmd, "AvgBrightness");
        }
        
    }
}
