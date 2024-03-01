using EngineNS.Bricks.VXGI;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Common
{
    public class UHzbNode : Graphics.Pipeline.TtRenderGraphNode
    {
        public TtRenderGraphPin DepthPinIn = TtRenderGraphPin.CreateInput("Depth");
        public TtRenderGraphPin HzbPinOut = TtRenderGraphPin.CreateOutput("Hzb", false, EPixelFormat.PXF_UNKNOWN);
        public UHzbNode()
        {
            Name = "Hzb";
        }
        public override void InitNodePins()
        {
            AddInput(DepthPinIn, NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_DSV);
            HzbPinOut.LifeMode = TtAttachBuffer.ELifeMode.Imported;
            AddOutput(HzbPinOut, NxRHI.EBufferType.BFT_SRV);
        }
        public override void FrameBuild(Graphics.Pipeline.URenderPolicy policy)
        {
            var hzbBuffer = RenderGraph.AttachmentCache.ImportAttachment(HzbPinOut);
            hzbBuffer.Buffer = HzbTexture;
            hzbBuffer.Srv = HzbSRV;
        }
        public readonly Vector3ui Dispatch_SetupDimArray2 = new Vector3ui(32, 32, 1);

        public NxRHI.UTexture HzbTexture;
        public NxRHI.USrView HzbSRV;
        public NxRHI.UUaView[] HzbMipsUAVs;

        private NxRHI.UComputeEffect Setup;
        private NxRHI.UComputeDraw SetupDrawcall;

        private NxRHI.UComputeEffect DownSample;
        private NxRHI.UComputeDraw[] MipsDrawcalls;
        
        public async override System.Threading.Tasks.Task Initialize(URenderPolicy policy, string debugName)
        {
            await Thread.TtAsyncDummyClass.DummyFunc();

            var rc = UEngine.Instance.GfxDevice.RenderContext;

            BasePass.Initialize(rc, debugName);

            var defines = new NxRHI.UShaderDefinitions();
            defines.mCoreObject.AddDefine("DispatchX", $"{Dispatch_SetupDimArray2.X}");
            defines.mCoreObject.AddDefine("DispatchY", $"{Dispatch_SetupDimArray2.Y}");
            defines.mCoreObject.AddDefine("DispatchZ", $"{Dispatch_SetupDimArray2.Z}");

            Setup = await UEngine.Instance.GfxDevice.EffectManager.GetComputeEffect(RName.GetRName("Shaders/Compute/GpuDriven/Hzb.compute", RName.ERNameType.Engine),
                "CS_Setup", NxRHI.EShaderType.SDT_ComputeShader, null, defines, null);

            DownSample = await UEngine.Instance.GfxDevice.EffectManager.GetComputeEffect(RName.GetRName("Shaders/Compute/GpuDriven/Hzb.compute", RName.ERNameType.Engine),
                "CS_DownSample", NxRHI.EShaderType.SDT_ComputeShader, null, defines, null);

            //ResetComputeDrawcall(policy);
        }
        private unsafe void ResetComputeDrawcall(URenderPolicy policy)
        {
            if (Setup == null)
                return;

            if (SetupDrawcall == null)
                SetupDrawcall = UEngine.Instance.GfxDevice.RenderContext.CreateComputeDraw();

            SetupDrawcall.SetComputeEffect(Setup);
            SetupDrawcall.SetDispatch(MaxSRVWidth / Dispatch_SetupDimArray2.X, MaxSRVHeight / Dispatch_SetupDimArray2.Y, 1);

            var srvIdx = SetupDrawcall.FindBinder(NxRHI.EShaderBindType.SBT_UAV, "DstBuffer");
            if (srvIdx.IsValidPointer)
            {
                SetupDrawcall.BindUav(srvIdx, HzbMipsUAVs[0]);
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
            MipsDrawcalls = new NxRHI.UComputeDraw[HzbMipsUAVs.Length - 1];
            uint width = MaxSRVWidth;
            uint height = MaxSRVHeight;
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
                var drawcall = UEngine.Instance.GfxDevice.RenderContext.CreateComputeDraw();
                CoreSDK.DisposeObject(ref MipsDrawcalls[i - 1]);
                MipsDrawcalls[i - 1] = drawcall;
                drawcall.SetComputeEffect(DownSample);
                drawcall.SetDispatch(MathHelper.Roundup(width, Dispatch_SetupDimArray2.X), MathHelper.Roundup(height, Dispatch_SetupDimArray2.Y), 1);
                srvIdx = drawcall.FindBinder(NxRHI.EShaderBindType.SBT_UAV, "SrcBuffer");
                if (srvIdx.IsValidPointer)
                {
                    drawcall.BindUav(srvIdx, HzbMipsUAVs[i - 1]);
                }

                srvIdx = drawcall.FindBinder(NxRHI.EShaderBindType.SBT_UAV, "DstBuffer");
                if (srvIdx.IsValidPointer)
                {
                    drawcall.BindUav(srvIdx, HzbMipsUAVs[i]);
                }
            }
        }
        public override void Dispose()
        {
            CoreSDK.DisposeObject(ref SetupDrawcall);
            if (MipsDrawcalls != null)
            {
                for (int i = 0; i < MipsDrawcalls.Length; i++)
                {
                    CoreSDK.DisposeObject(ref MipsDrawcalls[i]);
                }
                MipsDrawcalls = null;
            }
            if (HzbMipsUAVs != null)
            {
                for (int i = 0; i < HzbMipsUAVs.Length; i++)
                {
                    CoreSDK.DisposeObject(ref HzbMipsUAVs[i]);
                }
                HzbMipsUAVs = null;
            }
            CoreSDK.DisposeObject(ref HzbTexture);
            CoreSDK.DisposeObject(ref HzbSRV);

            base.Dispose();
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

            HzbMipsUAVs = new NxRHI.UUaView[NxRHI.USrView.CalcMipLevel((int)x, (int)y, true)];
            var dsTexDesc = new NxRHI.FTextureDesc();
            dsTexDesc.SetDefault();
            dsTexDesc.Width = (uint)x;
            dsTexDesc.Height = (uint)y;
            dsTexDesc.MipLevels = (uint)HzbMipsUAVs.Length;
            dsTexDesc.Format = EPixelFormat.PXF_R16G16_TYPELESS;
            dsTexDesc.BindFlags = NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_UAV;
            HzbTexture?.Dispose();
            HzbTexture = rc.CreateTexture(in dsTexDesc);

            var srvDesc = new NxRHI.FSrvDesc();
            srvDesc.SetTexture2D();
            srvDesc.Type = NxRHI.ESrvType.ST_Texture2D;
            srvDesc.Format = EPixelFormat.PXF_R16G16_FLOAT;
            srvDesc.Texture2D.MipLevels = dsTexDesc.MipLevels;
            HzbSRV?.Dispose();
            HzbSRV = rc.CreateSRV(HzbTexture, in srvDesc);

            for (int i = 0; i < HzbMipsUAVs.Length; i++)
            {
                var uavDesc = new NxRHI.FUavDesc();
                uavDesc.SetTexture2D();
                uavDesc.Format = EPixelFormat.PXF_R16G16_FLOAT;
                uavDesc.Texture2D.MipSlice = (uint)i;
                HzbMipsUAVs[i] = rc.CreateUAV(HzbTexture, in uavDesc);
            }

            ResetComputeDrawcall(policy);
        }
        [ThreadStatic]
        private static Profiler.TimeScope ScopeTick = Profiler.TimeScopeManager.GetTimeScope(typeof(UHzbNode), nameof(TickLogic));
        public override unsafe void TickLogic(GamePlay.UWorld world, Graphics.Pipeline.URenderPolicy policy, bool bClear)
        {
            if (SetupDrawcall == null)
                return;
            using (new Profiler.TimeScopeHelper(ScopeTick))
            {
                if (Setup == null)
                    return;
                var cmd = BasePass.DrawCmdList;
                using (new NxRHI.TtCmdListScope(cmd))
                {
                    var srvIdx = SetupDrawcall.FindBinder(NxRHI.EShaderBindType.SBT_SRV, "DepthBuffer");
                    if (srvIdx.IsValidPointer)
                    {
                        var depth = this.GetAttachBuffer(this.DepthPinIn).Srv;
                        SetupDrawcall.BindSrv(srvIdx, depth);
                    }
                    cmd.PushGpuDraw(SetupDrawcall);

                    if (MipsDrawcalls != null)
                    {
                        for (int i = 0; i < MipsDrawcalls.Length; i++)
                        {
                            cmd.PushGpuDraw(MipsDrawcalls[i]);
                        }
                    }
                    cmd.FlushDraws();
                }

                UEngine.Instance.GfxDevice.RenderCmdQueue.QueueCmdlist(cmd);
            }   
        }
    }
}
