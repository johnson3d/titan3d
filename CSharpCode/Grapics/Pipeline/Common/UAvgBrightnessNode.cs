using EngineNS.Bricks.VXGI;
using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Common
{
    [Bricks.CodeBuilder.ContextMenu("AvgBrightness", "AvgBrightness", Bricks.RenderPolicyEditor.UPolicyGraph.RGDEditorKeyword)]
    public class UAvgBrightnessNode : Graphics.Pipeline.TtRenderGraphNode
    {
        public override void Dispose()
        {
            CoreSDK.DisposeObject(ref SetupAvgBrightnessDrawcall);
            CoreSDK.DisposeObject(ref CountAvgBrightnessDrawcall);
            base.Dispose();
        }
        public TtRenderGraphPin GpuScenePinInOut = TtRenderGraphPin.CreateInputOutput("GpuScene");
        public TtRenderGraphPin ColorPinIn = TtRenderGraphPin.CreateInput("Color");
        public UAvgBrightnessNode()
        {
            Name = "AvgBrightnessNode";
        }
        public override void InitNodePins()
        {
            AddInputOutput(GpuScenePinInOut, NxRHI.EBufferType.BFT_UAV);
            AddInput(ColorPinIn, NxRHI.EBufferType.BFT_SRV);
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
            protected override void EnvShadingDefines(in FPermutationId id, NxRHI.UShaderDefinitions defines)
            {
                base.EnvShadingDefines(in id, defines);
            }
            public override void OnDrawCall(NxRHI.UComputeDraw drawcall, Graphics.Pipeline.TtRenderPolicy policy)
            {
                var gpuScene = policy.GetGpuSceneNode();
                var node = drawcall.TagObject as UAvgBrightnessNode;
                var srvIdx = drawcall.FindBinder(NxRHI.EShaderBindType.SBT_CBuffer, "cbPerFrame");
                if (srvIdx.IsValidPointer)
                {
                    drawcall.BindCBuffer(srvIdx, TtEngine.Instance.GfxDevice.PerFrameCBuffer);
                }
                srvIdx = drawcall.FindBinder(NxRHI.EShaderBindType.SBT_CBuffer, "cbPerGpuScene");
                if (srvIdx.IsValidPointer)
                {
                    drawcall.BindCBuffer(srvIdx, gpuScene.PerGpuSceneCbv);
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
        private NxRHI.UComputeDraw SetupAvgBrightnessDrawcall;

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
            protected override void EnvShadingDefines(in FPermutationId id, NxRHI.UShaderDefinitions defines)
            {
                base.EnvShadingDefines(in id, defines);
            }
            public override void OnDrawCall(NxRHI.UComputeDraw drawcall, Graphics.Pipeline.TtRenderPolicy policy)
            {
                var node = drawcall.TagObject as UAvgBrightnessNode;
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
            }
        }
        private CountAvgBrightnessShading CountAvgBrightness;
        private NxRHI.UComputeDraw CountAvgBrightnessDrawcall;
        public override async System.Threading.Tasks.Task Initialize(TtRenderPolicy policy, string debugName)
        {
            await Thread.TtAsyncDummyClass.DummyFunc();

            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            BasePass.Initialize(rc, debugName + ".BasePass");

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
        [ThreadStatic]
        private static Profiler.TimeScope ScopeTick = Profiler.TimeScopeManager.GetTimeScope(typeof(UAvgBrightnessNode), nameof(TickLogic));
        public unsafe override void TickLogic(GamePlay.UWorld world, TtRenderPolicy policy, bool bClear)
        {
            using (new Profiler.TimeScopeHelper(ScopeTick))
            {
                var gpuScene = policy.GetGpuSceneNode();

                var cmd = BasePass.DrawCmdList;

                using (new NxRHI.TtCmdListScope(cmd))
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

                policy.CommitCommandList(cmd);
            }   
        }
        
    }
}
