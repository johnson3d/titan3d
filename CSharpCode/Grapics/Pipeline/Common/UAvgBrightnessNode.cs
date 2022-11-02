using EngineNS.Bricks.VXGI;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Common
{
    public class UAvgBrightnessNode : Graphics.Pipeline.Common.URenderGraphNode
    {
        public Common.URenderGraphPin GpuScenePinInOut = Common.URenderGraphPin.CreateInputOutput("GpuScene");
        public Common.URenderGraphPin ColorPinIn = Common.URenderGraphPin.CreateInput("Color");
        public UAvgBrightnessNode()
        {
            Name = "AvgBrightnessNode";
        }
        public override void InitNodePins()
        {
            AddInputOutput(GpuScenePinInOut, NxRHI.EBufferType.BFT_UAV);
            AddInput(ColorPinIn, NxRHI.EBufferType.BFT_SRV);
        }
        public readonly UInt32_3 Dispatch_SetupDimArray1 = new UInt32_3(1, 1, 1);
        public readonly UInt32_3 Dispatch_SetupDimArray2 = new UInt32_3(32, 32, 1);

        public Graphics.Pipeline.UDrawBuffers BasePass = new Graphics.Pipeline.UDrawBuffers();

        private NxRHI.UComputeEffect SetupAvgBrightness;
        private NxRHI.UComputeDraw SetupAvgBrightnessDrawcall;

        private NxRHI.UComputeEffect CountAvgBrightness;
        private NxRHI.UComputeDraw CountAvgBrightnessDrawcall;
        public override async System.Threading.Tasks.Task Initialize(URenderPolicy policy, string debugName)
        {
            await Thread.AsyncDummyClass.DummyFunc();

            var rc = UEngine.Instance.GfxDevice.RenderContext;
            BasePass.Initialize(rc, debugName);

            var defines = new NxRHI.UShaderDefinitions();
            defines.mCoreObject.AddDefine("DispatchX", $"{Dispatch_SetupDimArray2.X}");
            defines.mCoreObject.AddDefine("DispatchY", $"{Dispatch_SetupDimArray2.Y}");
            defines.mCoreObject.AddDefine("DispatchZ", $"{Dispatch_SetupDimArray2.Z}");
            CountAvgBrightness = UEngine.Instance.GfxDevice.EffectManager.GetComputeEffect(RName.GetRName("Shaders/Compute/ScreenSpace/AvgBrightness.compute", RName.ERNameType.Engine).Address,
                "CS_CountAvgBrightness", NxRHI.EShaderType.SDT_ComputeShader, null, null, null, defines, null);

            defines.mCoreObject.AddDefine("DispatchX", $"{Dispatch_SetupDimArray2.X}");
            defines.mCoreObject.AddDefine("DispatchY", $"{Dispatch_SetupDimArray2.Y}");
            defines.mCoreObject.AddDefine("DispatchZ", $"{Dispatch_SetupDimArray2.Z}");
            SetupAvgBrightness = UEngine.Instance.GfxDevice.EffectManager.GetComputeEffect(RName.GetRName("Shaders/Compute/ScreenSpace/AvgBrightness.compute", RName.ERNameType.Engine).Address,
                "CS_SetupAvgBrightness", NxRHI.EShaderType.SDT_ComputeShader, null, null ,null , defines, null);

            ResetComputeDrawcall(policy);
        }
        private unsafe void ResetComputeDrawcall(URenderPolicy policy)
        {
            if (SetupAvgBrightness == null)
                return;
            var rc = UEngine.Instance.GfxDevice.RenderContext;
            var gpuScene = policy.GetGpuSceneNode();
            
            SetupAvgBrightnessDrawcall = rc.CreateComputeDraw();
            SetupAvgBrightnessDrawcall.SetComputeEffect(SetupAvgBrightness);
            SetupAvgBrightnessDrawcall.SetDispatch(1, 1, 1);

            var srvIdx = SetupAvgBrightnessDrawcall.FindBinder(NxRHI.EShaderBindType.SBT_CBuffer, "cbPerFrame");
            if (srvIdx.IsValidPointer)
            {
                SetupAvgBrightnessDrawcall.BindCBuffer(srvIdx, UEngine.Instance.GfxDevice.PerFrameCBuffer);
            }
            srvIdx = SetupAvgBrightnessDrawcall.FindBinder(NxRHI.EShaderBindType.SBT_CBuffer, "cbPerGpuScene");
            if (srvIdx.IsValidPointer)
            {
                SetupAvgBrightnessDrawcall.BindCBuffer(srvIdx, gpuScene.PerGpuSceneCBuffer);
            }

            //var lightSRV = policy.QuerySRV("LightRT");
            //if (lightSRV != null)
            {
                CountAvgBrightnessDrawcall = rc.CreateComputeDraw();
                CountAvgBrightnessDrawcall.SetComputeEffect(CountAvgBrightness);
            }
        }
        public override void Cleanup()
        {
            base.Cleanup();
        }
        public override void OnResize(URenderPolicy policy, float x, float y)
        {
            ResetComputeDrawcall(policy);
        }
        [ThreadStatic]
        private static Profiler.TimeScope ScopeTick = Profiler.TimeScopeManager.GetTimeScope(typeof(UAvgBrightnessNode), nameof(TickLogic));
        public unsafe override void TickLogic(GamePlay.UWorld world, URenderPolicy policy, bool bClear)
        {
            using (new Profiler.TimeScopeHelper(ScopeTick))
            {
                var gpuScene = policy.GetGpuSceneNode();

                var cmd = BasePass.DrawCmdList;
                #region Setup
                {
                    var srvIdx = SetupAvgBrightnessDrawcall.FindBinder(NxRHI.EShaderBindType.SBT_UAV, "GpuSceneDesc");
                    if (srvIdx.IsValidPointer)
                    {
                        var attachment = this.GetAttachBuffer(GpuScenePinInOut);
                        SetupAvgBrightnessDrawcall.BindUav(srvIdx, attachment.Uav);
                    }
                    SetupAvgBrightnessDrawcall.Commit(cmd);
                }
                #endregion

                #region Count
                {
                    if (CountAvgBrightnessDrawcall != null)
                    {
                        var attachment = this.GetAttachBuffer(ColorPinIn);
                        var lightSRV = attachment.Srv;
                        uint targetWidth = (uint)lightSRV.PicDesc.Width;
                        uint targetHeight = (uint)lightSRV.PicDesc.Height;
                        var srvIdx = CountAvgBrightnessDrawcall.FindBinder(NxRHI.EShaderBindType.SBT_SRV, "TargetBuffer");
                        if (srvIdx.IsValidPointer)
                        {
                            CountAvgBrightnessDrawcall.BindSrv(srvIdx, lightSRV);
                        }
                        CountAvgBrightnessDrawcall.SetDispatch(
                            CoreDefine.Roundup(targetWidth, Dispatch_SetupDimArray2.X),
                            CoreDefine.Roundup(targetHeight, Dispatch_SetupDimArray2.Y),
                            1);
                        CountAvgBrightnessDrawcall.Commit(cmd);
                    }
                }
                #endregion

                if (cmd.BeginCommand())
                {
                    cmd.EndCommand();
                }
                UEngine.Instance.GfxDevice.RenderCmdQueue.QueueCmdlist(cmd);
            }   
        }
        
        public override void TickSync(URenderPolicy policy)
        {
            BasePass.SwapBuffer();
        }
    }
}
