using EngineNS.Bricks.VXGI;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Common
{
    [Bricks.CodeBuilder.ContextMenu("Hzb", "Hzb", Bricks.RenderPolicyEditor.UPolicyGraph.RGDEditorKeyword)]
    [Rtti.Meta(NameAlias = new string[] { "EngineNS.Graphics.Pipeline.Common.UHzbNode@EngineCore", "EngineNS.Graphics.Pipeline.Common.UHzbNode" })]
    public class TtHzbNode : Graphics.Pipeline.TtRenderGraphNode
    {
        public TtRenderGraphPin DepthPinIn = TtRenderGraphPin.CreateInput("Depth");
        public TtRenderGraphPin HzbPinOut = TtRenderGraphPin.CreateOutput("Hzb", false, EPixelFormat.PXF_UNKNOWN);
        public TtHzbNode()
        {
            Name = "Hzb";
        }
        public override void InitNodePins()
        {
            AddInput(DepthPinIn, NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_DSV);
            HzbPinOut.LifeMode = TtAttachBuffer.ELifeMode.Imported;
            AddOutput(HzbPinOut, NxRHI.EBufferType.BFT_SRV);
        }
        public override void FrameBuild(Graphics.Pipeline.TtRenderPolicy policy)
        {
            var hzbBuffer = RenderGraph.AttachmentCache.ImportAttachment(HzbPinOut);
            hzbBuffer.GpuResource = HzbTexture;
            hzbBuffer.Srv = HzbSRV;
        }
        public readonly Vector3ui Dispatch_SetupDimArray2 = new Vector3ui(32, 32, 1);

        public NxRHI.TtTexture HzbTexture;
        public NxRHI.TtSrView HzbSRV;
        public NxRHI.TtUaView[] HzbMipsUAVs;

        public class SetupShading : Graphics.Pipeline.Shader.TtComputeShadingEnv
        {
            public override Vector3ui DispatchArg
            {
                get => new Vector3ui(32, 32, 1);
            }
            public SetupShading()
            {
                CodeName = RName.GetRName("Shaders/Compute/GpuDriven/Hzb.compute", RName.ERNameType.Engine);
                MainName = "CS_Setup";

                this.UpdatePermutation();
            }
            protected override void EnvShadingDefines(in FPermutationId id, NxRHI.TtShaderDefinitions defines)
            {
                base.EnvShadingDefines(in id, defines);
            }
            public override void OnDrawCall(NxRHI.TtComputeDraw drawcall, Graphics.Pipeline.TtRenderPolicy policy)
            {
                var node = drawcall.TagObject as TtHzbNode;

                
            }
        }
        private SetupShading Setup;
        private NxRHI.TtComputeDraw SetupDrawcall;

        public class DownSampleShading : Graphics.Pipeline.Shader.TtComputeShadingEnv
        {
            public override Vector3ui DispatchArg
            {
                get => new Vector3ui(32, 32, 1);
            }
            public DownSampleShading()
            {
                CodeName = RName.GetRName("Shaders/Compute/GpuDriven/Hzb.compute", RName.ERNameType.Engine);
                MainName = "CS_DownSample";

                this.UpdatePermutation();
            }
            protected override void EnvShadingDefines(in FPermutationId id, NxRHI.TtShaderDefinitions defines)
            {
                base.EnvShadingDefines(in id, defines);
            }
            public override void OnDrawCall(NxRHI.TtComputeDraw drawcall, Graphics.Pipeline.TtRenderPolicy policy)
            {
                var node = drawcall.TagObject as TtHzbNode;


            }
        }
        private DownSampleShading DownSample;
        private NxRHI.TtComputeDraw[] MipsDrawcalls;
        
        public async override System.Threading.Tasks.Task Initialize(TtRenderPolicy policy, string debugName)
        {
            await Thread.TtAsyncDummyClass.DummyFunc();

            var rc = TtEngine.Instance.GfxDevice.RenderContext;

            BasePass.Initialize(rc, debugName);

            var defines = new NxRHI.TtShaderDefinitions();
            defines.mCoreObject.AddDefine("DispatchX", $"{Dispatch_SetupDimArray2.X}");
            defines.mCoreObject.AddDefine("DispatchY", $"{Dispatch_SetupDimArray2.Y}");
            defines.mCoreObject.AddDefine("DispatchZ", $"{Dispatch_SetupDimArray2.Z}");

            Setup = await TtEngine.Instance.ShadingEnvManager.GetShadingEnv<SetupShading>();

            DownSample = await TtEngine.Instance.ShadingEnvManager.GetShadingEnv<DownSampleShading>();

            //ResetComputeDrawcall(policy);
        }
        private unsafe void ResetComputeDrawcall(TtRenderPolicy policy)
        {
            if (Setup == null)
                return;

            if (SetupDrawcall == null)
                SetupDrawcall = TtEngine.Instance.GfxDevice.RenderContext.CreateComputeDraw();

            Setup.SetDrawcallDispatch(this, policy, SetupDrawcall, MaxSRVWidth / Dispatch_SetupDimArray2.X, MaxSRVHeight / Dispatch_SetupDimArray2.Y, 1, false);
            //SetupDrawcall.SetComputeEffect(Setup.CurrentEffect);
            //SetupDrawcall.SetDispatch(MaxSRVWidth / Dispatch_SetupDimArray2.X, MaxSRVHeight / Dispatch_SetupDimArray2.Y, 1);

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
            MipsDrawcalls = new NxRHI.TtComputeDraw[HzbMipsUAVs.Length - 1];
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
                var drawcall = TtEngine.Instance.GfxDevice.RenderContext.CreateComputeDraw();
                CoreSDK.DisposeObject(ref MipsDrawcalls[i - 1]);
                MipsDrawcalls[i - 1] = drawcall;
                DownSample.SetDrawcallDispatch(this, policy, drawcall, width, height, 1, true);
                //drawcall.SetComputeEffect(DownSample);
                //drawcall.SetDispatch(MathHelper.Roundup(width, Dispatch_SetupDimArray2.X), MathHelper.Roundup(height, Dispatch_SetupDimArray2.Y), 1);
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
        public override unsafe void OnResize(TtRenderPolicy policy, float x, float y)
        {
            if (x == 1 || y == 1)
                return;
            var rc = TtEngine.Instance.GfxDevice.RenderContext;

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

            HzbMipsUAVs = new NxRHI.TtUaView[NxRHI.TtSrView.CalcMipLevel((int)x, (int)y, true)];
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
        private static Profiler.TimeScope mScopeTick;
        private static Profiler.TimeScope ScopeTick
        {
            get
            {
                if (mScopeTick == null)
                    mScopeTick = new Profiler.TimeScope(typeof(TtHzbNode), nameof(TickLogic));
                return mScopeTick;
            }
        }
        public override unsafe void TickLogic(GamePlay.TtWorld world, Graphics.Pipeline.TtRenderPolicy policy, bool bClear)
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

                policy.CommitCommandList(cmd);
            }   
        }
    }
}
