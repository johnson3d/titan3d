using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Common
{
    public class UHzbNode : Graphics.Pipeline.Common.URenderGraphNode
    {
        public Common.URenderGraphPin DepthPinIn = Common.URenderGraphPin.CreateInput("Depth");
        public Common.URenderGraphPin HzbPinOut = Common.URenderGraphPin.CreateOutput("Hzb", false, EPixelFormat.PXF_UNKNOWN);
        public UHzbNode()
        {
            Name = "Hzb";
        }
        public override void InitNodePins()
        {
            AddInput(DepthPinIn, EGpuBufferViewType.GBVT_Srv | EGpuBufferViewType.GBVT_Dsv);
            HzbPinOut.LifeMode = UAttachBuffer.ELifeMode.Imported;
            AddOutput(HzbPinOut, EGpuBufferViewType.GBVT_Srv);
        }
        public override void FrameBuild()
        {
            var hzbBuffer = RenderGraph.AttachmentCache.ImportAttachment(HzbPinOut);
            hzbBuffer.Buffer = HzbTexture;
            hzbBuffer.Srv = HzbSRV;
        }
        public readonly UInt32_3 Dispatch_SetupDimArray2 = new UInt32_3(32, 32, 1);

        public RHI.CTexture2D HzbTexture;
        public RHI.CShaderResourceView HzbSRV;
        public RHI.CUnorderedAccessView[] HzbMipsUAVs;

        public Graphics.Pipeline.UDrawBuffers BasePass = new Graphics.Pipeline.UDrawBuffers();

        private Graphics.Pipeline.Shader.UShader Setup;
        private RHI.CComputeDrawcall SetupDrawcall;

        private Graphics.Pipeline.Shader.UShader DownSample;
        private RHI.CComputeDrawcall[] MipsDrawcalls;
        ~UHzbNode()
        {
            Cleanup();
        }
        public async override System.Threading.Tasks.Task Initialize(URenderPolicy policy, string debugName)
        {
            await Thread.AsyncDummyClass.DummyFunc();

            var rc = UEngine.Instance.GfxDevice.RenderContext;

            BasePass.Initialize(rc, debugName);

            var defines = new RHI.CShaderDefinitions();
            defines.mCoreObject.AddDefine("DispatchX", $"{Dispatch_SetupDimArray2.X}");
            defines.mCoreObject.AddDefine("DispatchY", $"{Dispatch_SetupDimArray2.Y}");
            defines.mCoreObject.AddDefine("DispatchZ", $"{Dispatch_SetupDimArray2.Z}");

            Setup = UEngine.Instance.GfxDevice.EffectManager.GetShader(RName.GetRName("Shaders/Compute/GpuDriven/Hzb.compute", RName.ERNameType.Engine),
                "CS_Setup", EShaderType.EST_ComputeShader, defines);

            DownSample = UEngine.Instance.GfxDevice.EffectManager.GetShader(RName.GetRName("Shaders/Compute/GpuDriven/Hzb.compute", RName.ERNameType.Engine),
                "CS_DownSample", EShaderType.EST_ComputeShader, defines);

            SetupDrawcall = rc.CreateComputeDrawcall();
            //ResetComputeDrawcall(policy);
        }
        private unsafe void ResetComputeDrawcall(URenderPolicy policy)
        {
            if (Setup == null)
                return;
            SetupDrawcall.mCoreObject.SetComputeShader(Setup.CS_Shader.mCoreObject);
            SetupDrawcall.mCoreObject.SetDispatch(MaxSRVWidth / Dispatch_SetupDimArray2.X, MaxSRVHeight / Dispatch_SetupDimArray2.Y, 1);

            var srvIdx = Setup.Desc.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Uav, "DstBuffer");
            if (srvIdx != (IShaderBinder*)0)
            {
                SetupDrawcall.mCoreObject.GetUavResources().BindCS(srvIdx->m_CSBindPoint, HzbMipsUAVs[0].mCoreObject);
            }

            if (HzbMipsUAVs == null)
                return;

            if (MipsDrawcalls != null)
            {
                foreach(var i in MipsDrawcalls)
                {
                    i.Dispose();
                }
            }
            MipsDrawcalls = new RHI.CComputeDrawcall[HzbMipsUAVs.Length - 1];
            uint width = MaxSRVWidth / 2;
            uint height = MaxSRVHeight / 2;
            for (int i = 1; i < HzbMipsUAVs.Length; i++)
            {
                if (width > 1)
                {
                    width = width / 2;
                }
                else
                {
                    width = 1;
                }
                if (height > 1)
                {
                    height = height / 2;
                }
                else
                {
                    height = 1;
                }
                var drawcall = UEngine.Instance.GfxDevice.RenderContext.CreateComputeDrawcall();
                MipsDrawcalls[i - 1] = drawcall;
                drawcall.mCoreObject.SetComputeShader(DownSample.CS_Shader.mCoreObject);
                drawcall.mCoreObject.SetDispatch(CoreDefine.Roundup(width, Dispatch_SetupDimArray2.X), CoreDefine.Roundup(height, Dispatch_SetupDimArray2.Y), 1);
                srvIdx = DownSample.Desc.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Uav, "SrcBuffer");
                if (srvIdx != (IShaderBinder*)0)
                {
                    drawcall.mCoreObject.GetUavResources().BindCS(srvIdx->m_CSBindPoint, HzbMipsUAVs[i - 1].mCoreObject);
                }

                srvIdx = DownSample.Desc.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Uav, "DstBuffer");
                if (srvIdx != (IShaderBinder*)0)
                {
                    drawcall.mCoreObject.GetUavResources().BindCS(srvIdx->m_CSBindPoint, HzbMipsUAVs[i].mCoreObject);
                }
            }
        }
        public override void Cleanup()
        {
            if (HzbMipsUAVs != null)
            {
                for (int i = 0; i < HzbMipsUAVs.Length; i++)
                {
                    HzbMipsUAVs[i]?.Dispose();
                }
                HzbMipsUAVs = null;
            }
            HzbTexture?.Dispose();
            HzbTexture = null;

            HzbSRV?.Dispose();
            HzbSRV = null;

            base.Cleanup();
        }
        uint MaxSRVWidth;
        uint MaxSRVHeight;
        public override unsafe void OnResize(URenderPolicy policy, float x, float y)
        {
            if (x == 1 || y == 1)
                return;
            var rc = UEngine.Instance.GfxDevice.RenderContext;

            x /= 2;
            y /= 2;
            MaxSRVWidth = (uint)x;
            MaxSRVHeight = (uint)y;

            if (HzbMipsUAVs != null)
            {
                for (int i = 0; i < HzbMipsUAVs.Length; i++)
                {
                    HzbMipsUAVs[i]?.Dispose();
                }
            }

            HzbMipsUAVs = new RHI.CUnorderedAccessView[RHI.CShaderResourceView.CalcMipLevel((int)x, (int)y, true)];
            var dsTexDesc = new ITexture2DDesc();
            dsTexDesc.SetDefault();
            dsTexDesc.Width = (uint)x;
            dsTexDesc.Height = (uint)y;
            dsTexDesc.MipLevels = (uint)HzbMipsUAVs.Length;
            dsTexDesc.Format = EPixelFormat.PXF_R16G16_TYPELESS;
            dsTexDesc.BindFlags = (UInt32)(EBindFlags.BF_SHADER_RES | EBindFlags.BF_UNORDERED_ACCESS);
            HzbTexture?.Dispose();
            HzbTexture = rc.CreateTexture2D(in dsTexDesc);

            var srvDesc = new IShaderResourceViewDesc();
            srvDesc.SetTexture2D();
            srvDesc.Type = ESrvType.ST_Texture2D;
            srvDesc.Format = EPixelFormat.PXF_R16G16_UNORM;
            srvDesc.mGpuBuffer = HzbTexture.mCoreObject;
            srvDesc.Texture2D.MipLevels = dsTexDesc.MipLevels;
            HzbSRV?.Dispose();
            HzbSRV = rc.CreateShaderResourceView(in srvDesc);

            for (int i = 0; i < HzbMipsUAVs.Length; i++)
            {
                var uavDesc = new IUnorderedAccessViewDesc();
                uavDesc.SetTexture2D();
                uavDesc.Format = EPixelFormat.PXF_R16G16_UNORM;
                uavDesc.Texture2D.MipSlice = (uint)i;
                HzbMipsUAVs[i] = rc.CreateUnorderedAccessView(HzbTexture.mCoreObject, in uavDesc);
            }

            ResetComputeDrawcall(policy);
        }
        public override unsafe void TickLogic(GamePlay.UWorld world, Graphics.Pipeline.URenderPolicy policy, bool bClear)
        {
            if (Setup == null)
                return;
            var cmd = BasePass.DrawCmdList;

            var srvIdx = Setup.Desc.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Srv, "DepthBuffer");
            if (srvIdx != (IShaderBinder*)0)
            {
                var depth = this.GetAttachBuffer(this.DepthPinIn).Srv;
                SetupDrawcall.mCoreObject.GetShaderRViewResources().BindCS(srvIdx->CSBindPoint, depth.mCoreObject);
            }
            SetupDrawcall.BuildPass(cmd);

            if (MipsDrawcalls != null)
            {
                for (int i = 0; i < MipsDrawcalls.Length; i++)
                {
                    MipsDrawcalls[i].BuildPass(cmd);
                }
            }
            //cmd.SetComputeShader(CS_Setup.mCoreObject);
            //var srvIdx  = CSDesc_Setup.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Srv, "DepthBuffer");
            //if (srvIdx != (IShaderBinder*)0)
            //{
            //    var depth = policy.GetBasePassNode().GBuffers.DepthStencilSRV;
            //    cmd.CSSetShaderResource(srvIdx->CSBindPoint, depth.mCoreObject);
            //}

            //srvIdx = CSDesc_Setup.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_CBuffer, "DstBuffer");
            //if (srvIdx != (IShaderBinder*)0)
            //{
            //    cmd.CSSetUnorderedAccessView(srvIdx->m_CSBindPoint, HzbMipsUAVs[0].mCoreObject, &nUavInitialCounts);
            //}

            //cmd.CSDispatch(MaxSRVWidth / Dispatch_SetupDimArray2.x, MaxSRVHeight / Dispatch_SetupDimArray2.y, 1);

            //uint width = MaxSRVWidth/2;
            //uint height = MaxSRVHeight/2;
            //for (int i = 1; i < HzbMipsUAVs.Length; i++)
            //{
            //    cmd.SetComputeShader(CS_DownSample.mCoreObject);
            //    var srvIdx = CSDesc_DownSample.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Uav, "SrcBuffer");
            //    if (srvIdx != (IShaderBinder*)0)
            //    {
            //        cmd.CSSetUnorderedAccessView(srvIdx->m_CSBindPoint, HzbMipsUAVs[i - 1].mCoreObject, &nUavInitialCounts);
            //    }

            //    srvIdx = CSDesc_DownSample.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Uav, "DstBuffer");
            //    if (srvIdx != (IShaderBinder*)0)
            //    {
            //        cmd.CSSetUnorderedAccessView(srvIdx->m_CSBindPoint, HzbMipsUAVs[i].mCoreObject, &nUavInitialCounts);
            //    }
                
            //    cmd.CSDispatch(CoreDefine.Roundup(width, Dispatch_SetupDimArray2.x), CoreDefine.Roundup(height, Dispatch_SetupDimArray2.y), 1);

            //    if (width > 1)
            //    {
            //        width = width / 2;
            //    }
            //    else
            //    {
            //        width = 1;
            //    }
            //    if (height > 1)
            //    {
            //        height = height / 2;
            //    }
            //    else
            //    {
            //        height = 1;
            //    }
            //}

            if (cmd.BeginCommand())
            {
                cmd.EndCommand();
            }
        }
        public unsafe override void TickRender(Graphics.Pipeline.URenderPolicy policy)
        {
            var rc = UEngine.Instance.GfxDevice.RenderContext;

            var cmdlist_hp = BasePass.CommitCmdList.mCoreObject;
            cmdlist_hp.Commit(rc.mCoreObject);
        }
        public unsafe override void TickSync(Graphics.Pipeline.URenderPolicy policy)
        {
            BasePass.SwapBuffer();
        }
    }
}
