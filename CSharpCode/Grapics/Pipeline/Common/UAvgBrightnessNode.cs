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
            AddInputOutput(GpuScenePinInOut, EGpuBufferViewType.GBVT_Uav);
            AddInput(ColorPinIn, EGpuBufferViewType.GBVT_Srv);
        }
        public readonly UInt32_3 Dispatch_SetupDimArray1 = new UInt32_3(1, 1, 1);
        public readonly UInt32_3 Dispatch_SetupDimArray2 = new UInt32_3(32, 32, 1);

        public Graphics.Pipeline.UDrawBuffers BasePass = new Graphics.Pipeline.UDrawBuffers();

        private RHI.CShaderDesc CSDesc_SetupAvgBrightness;
        private RHI.CComputeShader CS_SetupAvgBrightness;
        private RHI.CComputeDrawcall SetupAvgBrightnessDrawcall;

        private RHI.CShaderDesc CSDesc_CountAvgBrightness;
        private RHI.CComputeShader CS_CountAvgBrightness;
        private RHI.CComputeDrawcall CountAvgBrightnessDrawcall;
        public override async System.Threading.Tasks.Task Initialize(URenderPolicy policy, string debugName)
        {
            await Thread.AsyncDummyClass.DummyFunc();

            var rc = UEngine.Instance.GfxDevice.RenderContext;
            BasePass.Initialize(rc, debugName);

            var defines = new RHI.CShaderDefinitions();
            defines.mCoreObject.AddDefine("DispatchX", $"{Dispatch_SetupDimArray2.X}");
            defines.mCoreObject.AddDefine("DispatchY", $"{Dispatch_SetupDimArray2.Y}");
            defines.mCoreObject.AddDefine("DispatchZ", $"{Dispatch_SetupDimArray2.Z}");
            CSDesc_CountAvgBrightness = rc.CreateShaderDesc(RName.GetRName("Shaders/Compute/ScreenSpace/AvgBrightness.compute", RName.ERNameType.Engine),
                "CS_CountAvgBrightness", EShaderType.EST_ComputeShader, defines, null);
            CS_CountAvgBrightness = rc.CreateComputeShader(CSDesc_CountAvgBrightness);

            defines.mCoreObject.AddDefine("DispatchX", $"{Dispatch_SetupDimArray2.X}");
            defines.mCoreObject.AddDefine("DispatchY", $"{Dispatch_SetupDimArray2.Y}");
            defines.mCoreObject.AddDefine("DispatchZ", $"{Dispatch_SetupDimArray2.Z}");
            CSDesc_SetupAvgBrightness = rc.CreateShaderDesc(RName.GetRName("Shaders/Compute/ScreenSpace/AvgBrightness.compute", RName.ERNameType.Engine),
                "CS_SetupAvgBrightness", EShaderType.EST_ComputeShader, defines, null);
            CS_SetupAvgBrightness = rc.CreateComputeShader(CSDesc_SetupAvgBrightness);

            ResetComputeDrawcall(policy);
        }
        private unsafe void ResetComputeDrawcall(URenderPolicy policy)
        {
            if (CS_SetupAvgBrightness == null)
                return;
            var rc = UEngine.Instance.GfxDevice.RenderContext;
            var gpuScene = policy.GetGpuSceneNode();
            
            SetupAvgBrightnessDrawcall = rc.CreateComputeDrawcall();
            SetupAvgBrightnessDrawcall.mCoreObject.SetComputeShader(CS_SetupAvgBrightness.mCoreObject);
            SetupAvgBrightnessDrawcall.mCoreObject.SetDispatch(1, 1, 1);
            
            var srvIdx = CSDesc_SetupAvgBrightness.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_CBuffer, "cbPerFrame");
            if (srvIdx != (IShaderBinder*)0)
            {
                SetupAvgBrightnessDrawcall.mCoreObject.GetCBufferResources().BindCS(srvIdx->m_CSBindPoint, UEngine.Instance.GfxDevice.PerFrameCBuffer.mCoreObject);
            }
            srvIdx = CSDesc_SetupAvgBrightness.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_CBuffer, "cbPerGpuScene");
            if (srvIdx != (IShaderBinder*)0)
            {
                SetupAvgBrightnessDrawcall.mCoreObject.GetCBufferResources().BindCS(srvIdx->m_CSBindPoint, gpuScene.PerGpuSceneCBuffer.mCoreObject);
            }

            //var lightSRV = policy.QuerySRV("LightRT");
            //if (lightSRV != null)
            {
                CountAvgBrightnessDrawcall = rc.CreateComputeDrawcall();
                CountAvgBrightnessDrawcall.mCoreObject.SetComputeShader(CS_CountAvgBrightness.mCoreObject);
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
        public unsafe override void TickLogic(GamePlay.UWorld world, URenderPolicy policy, bool bClear)
        {
            var gpuScene = policy.GetGpuSceneNode();
            
            var cmd = BasePass.DrawCmdList;
            #region Setup
            {
                var srvIdx = CSDesc_SetupAvgBrightness.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Uav, "GpuSceneDesc");
                if (srvIdx != (IShaderBinder*)0)
                {
                    var attachment = this.GetAttachBuffer(GpuScenePinInOut);
                    SetupAvgBrightnessDrawcall.mCoreObject.GetUavResources().BindCS(srvIdx->m_CSBindPoint, attachment.Uav.mCoreObject);
                }
                SetupAvgBrightnessDrawcall.BuildPass(cmd);
                //cmd.SetComputeShader(CS_SetupAvgBrightness.mCoreObject);
                //var srvIdx = CSDesc_SetupAvgBrightness.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Uav, "GpuSceneDesc");
                //if (srvIdx != (IShaderBinder*)0)
                //{
                //    cmd.CSSetUnorderedAccessView(srvIdx->m_CSBindPoint, gpuScene.GpuSceneDescUAV.mCoreObject, &nUavInitialCounts);
                //}
                //srvIdx = CSDesc_SetupAvgBrightness.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_CBuffer, "cbPerFrame");
                //if (srvIdx != (IShaderBinder*)0)
                //{
                //    cmd.CSSetConstantBuffer(srvIdx->m_CSBindPoint, UEngine.Instance.GfxDevice.PerFrameCBuffer.mCoreObject);
                //}
                //srvIdx = CSDesc_SetupAvgBrightness.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_CBuffer, "cbPerGpuScene");
                //if (srvIdx != (IShaderBinder*)0)
                //{
                //    cmd.CSSetConstantBuffer(srvIdx->m_CSBindPoint, gpuScene.PerGpuSceneCBuffer.mCoreObject);
                //}
                //cmd.CSDispatch(1, 1, 1);
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
                    var srvIdx = CSDesc_CountAvgBrightness.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Srv, "TargetBuffer");
                    if (srvIdx != (IShaderBinder*)0)
                    {
                        CountAvgBrightnessDrawcall.mCoreObject.GetShaderRViewResources().BindCS(srvIdx->CSBindPoint, lightSRV.mCoreObject);
                    }
                    CountAvgBrightnessDrawcall.mCoreObject.SetDispatch(
                        CoreDefine.Roundup(targetWidth, Dispatch_SetupDimArray2.X), 
                        CoreDefine.Roundup(targetHeight, Dispatch_SetupDimArray2.Y), 
                        1);
                    CountAvgBrightnessDrawcall.BuildPass(cmd);
                    //uint targetWidth = (uint)lightSRV.PicDesc.Width;
                    //uint targetHeight = (uint)lightSRV.PicDesc.Height;
                    //cmd.SetComputeShader(CS_CountAvgBrightness.mCoreObject);
                    //var srvIdx = CSDesc_CountAvgBrightness.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Srv, "TargetBuffer");
                    //if (srvIdx != (IShaderBinder*)0)
                    //{
                    //    cmd.CSSetShaderResource(srvIdx->CSBindPoint, lightSRV.mCoreObject);
                    //}
                    //srvIdx = CSDesc_SetupAvgBrightness.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Uav, "GpuSceneDesc");
                    //if (srvIdx != (IShaderBinder*)0)
                    //{
                    //    cmd.CSSetUnorderedAccessView(srvIdx->m_CSBindPoint, gpuScene.GpuSceneDescUAV.mCoreObject, &nUavInitialCounts);
                    //}
                    //cmd.CSDispatch(CoreDefine.Roundup(targetWidth, Dispatch_SetupDimArray2.x), CoreDefine.Roundup(targetHeight, Dispatch_SetupDimArray2.y), 1);
                }
            }
            #endregion

            if (cmd.BeginCommand())
            {
                cmd.EndCommand();
            }
        }
        public override void TickRender(URenderPolicy policy)
        {
            var rc = UEngine.Instance.GfxDevice.RenderContext;

            var cmdlist_hp = BasePass.CommitCmdList.mCoreObject;
            cmdlist_hp.Commit(rc.mCoreObject);
        }
        public override void TickSync(URenderPolicy policy)
        {
            BasePass.SwapBuffer();
        }
    }
}
