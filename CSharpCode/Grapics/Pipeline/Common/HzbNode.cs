using EngineNS.Bricks.VXGI;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Common
{
    [Bricks.CodeBuilder.ContextMenu("Hzb", "Hzb", Bricks.RenderPolicyEditor.TtPolicyGraph.RGDEditorKeyword)]
    [Rtti.Meta("",NameAlias = new string[] { "EngineNS.Graphics.Pipeline.Common.UHzbNode@EngineCore", "EngineNS.Graphics.Pipeline.Common.UHzbNode" })]
    public class TtHzbNode : TAuxRenderGraphNode<TtHzbNode>
    {
        // Hzb 只读深度, 不写深度, 因此只需 BFT_SRV. 之前同时声明 BFT_DSV 会让 RenderGraph
        // 误认为本节点要写 depth, 影响 barrier / 资源状态决策.
        public TtRenderGraphPin DepthPinIn = TtRenderGraphPin.CreateInput("Depth", NxRHI.EBufferType.BFT_SRV);
        public TtRenderGraphPin HzbPinOut = TtRenderGraphPin.CreateOutput("Hzb", false, EPixelFormat.PXF_UNKNOWN, NxRHI.EBufferType.BFT_SRV);

        // Compute thread group 尺寸. 必须通过 EnvShadingDefines 注入到 shader, 让
        // [numthreads(DispatchX, DispatchY, DispatchZ)] 拿到一致的常量, 否则 shader 编译失败.
        public const uint DispatchSizeX = 32;
        public const uint DispatchSizeY = 32;
        public const uint DispatchSizeZ = 1;

        public NxRHI.TtTexture HzbTexture;
        public NxRHI.TtSrView HzbSRV;
        public NxRHI.TtUaView[] HzbMipsUAVs;

        TtAttachBuffer HzbAttachement = new TtAttachBuffer();

        // OnDrawCall 阶段需要的"当前要绑什么". Tick 在调 SetDrawcallDispatch 之前
        // 把这两个字段写好, ShadingEnv.OnDrawCall 通过 drawcall.TagObject 拿回节点
        // 后从这里读出来 Bind. 详见 documents/coding/CodingGuidelines.md §3.6.
        // mCurrentMipIndex 仅 DownSample drawcall 用 (= 当前要写入的 dst mip 下标,
        // src 自然是 mCurrentMipIndex - 1).
        NxRHI.TtSrView mCurrentDepthSrv;
        int mCurrentMipIndex;

        public TtHzbNode()
        {
            Name = "Hzb";
        }

        public override void InitNodePins()
        {
            AddInput(DepthPinIn);
            HzbPinOut.LifeMode = TtAttachBuffer.ELifeMode.Imported;
            AddOutput(HzbPinOut);
        }

        public override void FrameBuild(Graphics.Pipeline.TtRenderPolicy policy)
        {
            var hzbBuffer = RenderGraph.AttachmentCache.ImportAttachment(HzbPinOut, HzbAttachement);
            hzbBuffer.GpuResource = HzbTexture;
            hzbBuffer.Srv = HzbSRV;
        }

        public class SetupShading : Graphics.Pipeline.Shader.TtComputeShadingEnv
        {
            public override Vector3ui DispatchArg => new Vector3ui(DispatchSizeX, DispatchSizeY, DispatchSizeZ);

            public SetupShading()
            {
                CodeName = RName.GetRName("Shaders/Compute/GpuDriven/Hzb.compute", RName.ERNameType.Engine);
                MainName = "CS_Setup";

                this.UpdatePermutation().AddWaitTask();
            }

            protected override void EnvShadingDefines(in FPermutationId id, NxRHI.TtShaderDefinitions defines)
            {
                base.EnvShadingDefines(in id, defines);
                // shader 端 [numthreads(DispatchX, DispatchY, DispatchZ)] 必须靠这三个 define 才能编译通过.
                defines.mCoreObject.AddDefine("DispatchX", $"{DispatchSizeX}");
                defines.mCoreObject.AddDefine("DispatchY", $"{DispatchSizeY}");
                defines.mCoreObject.AddDefine("DispatchZ", $"{DispatchSizeZ}");
            }

            public override void OnDrawCall(NxRHI.TtComputeDraw drawcall, Graphics.Pipeline.TtRenderPolicy policy)
            {
                var node = drawcall.TagObject as TtHzbNode;
                if (node == null) return;
                if (node.mCurrentDepthSrv == null) return;
                if (node.HzbMipsUAVs == null || node.HzbMipsUAVs.Length == 0) return;

                drawcall.BindSrv("DepthBuffer", node.mCurrentDepthSrv);
                drawcall.BindUav("DstBuffer", node.HzbMipsUAVs[0]);
                // shader 的 LinearFromDepth 需要 cbPerCamera.ZNear / ZFar / 投影信息.
                // (DownSample pass 不需要 cbPerCamera, 已经在 linear z 空间.)
                drawcall.BindCBV("cbPerCamera", policy.DefaultCamera.PerCameraCBuffer);
            }
        }

        public class DownSampleShading : Graphics.Pipeline.Shader.TtComputeShadingEnv
        {
            public override Vector3ui DispatchArg => new Vector3ui(DispatchSizeX, DispatchSizeY, DispatchSizeZ);

            public DownSampleShading()
            {
                CodeName = RName.GetRName("Shaders/Compute/GpuDriven/Hzb.compute", RName.ERNameType.Engine);
                MainName = "CS_DownSample";

                this.UpdatePermutation().AddWaitTask();
            }
            protected override void EnvShadingDefines(in FPermutationId id, NxRHI.TtShaderDefinitions defines)
            {
                base.EnvShadingDefines(in id, defines);
                defines.mCoreObject.AddDefine("DispatchX", $"{DispatchSizeX}");
                defines.mCoreObject.AddDefine("DispatchY", $"{DispatchSizeY}");
                defines.mCoreObject.AddDefine("DispatchZ", $"{DispatchSizeZ}");
            }

            public override void OnDrawCall(NxRHI.TtComputeDraw drawcall, Graphics.Pipeline.TtRenderPolicy policy)
            {
                var node = drawcall.TagObject as TtHzbNode;
                if (node == null) return;
                if (node.HzbMipsUAVs == null) return;

                int dstMip = node.mCurrentMipIndex;
                int srcMip = dstMip - 1;
                if (srcMip < 0 || dstMip >= node.HzbMipsUAVs.Length) return;

                drawcall.BindUav("SrcBuffer", node.HzbMipsUAVs[srcMip]);
                drawcall.BindUav("DstBuffer", node.HzbMipsUAVs[dstMip]);
            }
        }

        SetupShading mSetup;
        DownSampleShading mDownSample;
        NxRHI.TtComputeDraw mSetupDrawcall;
        NxRHI.TtComputeDraw[] mMipsDrawcalls;
        
        public async override Thread.Async.TtTask Initialize(TtRenderPolicy policy, string debugName)
        {
            await Thread.TtAsyncDummyClass.DummyFunc();

            mSetup = await Graphics.Pipeline.Shader.TtShadingEnv.CreateShadingEnv<SetupShading>();
            mDownSample = await Graphics.Pipeline.Shader.TtShadingEnv.CreateShadingEnv<DownSampleShading>();
        }

        public override void Dispose()
        {
            CoreSDK.DisposeObject(ref HzbAttachement);
            CoreSDK.DisposeObject(ref mSetupDrawcall);
            if (mMipsDrawcalls != null)
            {
                for (int i = 0; i < mMipsDrawcalls.Length; i++)
                    CoreSDK.DisposeObject(ref mMipsDrawcalls[i]);
                mMipsDrawcalls = null;
            }
            if (HzbMipsUAVs != null)
            {
                for (int i = 0; i < HzbMipsUAVs.Length; i++)
                    CoreSDK.DisposeObject(ref HzbMipsUAVs[i]);
                HzbMipsUAVs = null;
            }
            CoreSDK.DisposeObject(ref HzbSRV);
            CoreSDK.DisposeObject(ref HzbTexture);

            base.Dispose();
        }

        // mip0 (= 屏幕分辨率 / 2) 的尺寸. 由 OnResize 写入, Tick 用它推每一级 mip 的 dispatch 尺寸.
        uint mMip0Width;
        uint mMip0Height;

        public override unsafe void OnResize(TtRenderPolicy policy, float x, float y)
        {
            // 太小直接跳过 (mip0 = x/2, 当 x <= 1 时算出来是 0). 用 <= 1 比 == 1 更稳健.
            if (x <= 1.0f || y <= 1.0f)
                return;

            var rc = TtEngine.Instance.GfxDevice.RenderContext;

            // mip0 = 屏幕分辨率的一半. shader CS_Setup 直接 id*2 采样 depth, 因此 depth
            // 必须是 mip0 的 2 倍.
            mMip0Width = (uint)(x / 2.0f);
            mMip0Height = (uint)(y / 2.0f);
            if (mMip0Width == 0 || mMip0Height == 0)
                return;

            // 释放旧的 mip UAV.
            if (HzbMipsUAVs != null)
            {
                for (int i = 0; i < HzbMipsUAVs.Length; i++)
                    CoreSDK.DisposeObject(ref HzbMipsUAVs[i]);
            }

            HzbMipsUAVs = new NxRHI.TtUaView[NxRHI.TtSrView.CalcMipLevel((int)mMip0Width, (int)mMip0Height, true, 1)];

            var dsTexDesc = new NxRHI.FTextureDesc();
            dsTexDesc.SetDefault();
            dsTexDesc.Width = mMip0Width;
            dsTexDesc.Height = mMip0Height;
            dsTexDesc.MipLevels = (uint)HzbMipsUAVs.Length;
            dsTexDesc.Format = EPixelFormat.PXF_R16G16_TYPELESS;
            dsTexDesc.BindFlags = NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_UAV;

            CoreSDK.DisposeObject(ref HzbSRV);
            CoreSDK.DisposeObject(ref HzbTexture);
            HzbTexture = rc.CreateTexture(in dsTexDesc);

            var srvDesc = new NxRHI.FSrvDesc();
            srvDesc.SetTexture2D();
            srvDesc.Type = NxRHI.ESrvType.ST_Texture2D;
            srvDesc.Format = EPixelFormat.PXF_R16G16_FLOAT;
            srvDesc.Texture2D.MipLevels = dsTexDesc.MipLevels;
            HzbSRV = rc.CreateSRV(HzbTexture, in srvDesc);

            for (int i = 0; i < HzbMipsUAVs.Length; i++)
            {
                var uavDesc = new NxRHI.FUavDesc();
                uavDesc.SetTexture2D();
                uavDesc.Format = EPixelFormat.PXF_R16G16_FLOAT;
                uavDesc.Texture2D.MipSlice = (uint)i;
                HzbMipsUAVs[i] = rc.CreateUAV(HzbTexture, in uavDesc);
            }

            // 同步给 RenderGraph 中 import 的 attach buffer, 避免下游拿到的 Srv 指向已 dispose 的旧资源.
            // FrameBuild 通常只在 graph build 时跑一次, OnResize 后必须主动同步.
            HzbAttachement.GpuResource = HzbTexture;
            HzbAttachement.Srv = HzbSRV;

            ResetComputeDrawcall(policy);
        }

        // 仅创建 / 重建 drawcall 实例并打 TagObject. 不在这里做任何 BindXxx 也不 SetDrawcallDispatch
        // (那两件事必须在 Tick 阶段每帧调, 才能保证 OnDrawCall 拿到当前正确的 mCurrentDepthSrv /
        // mCurrentMipIndex). 详见 documents/coding/CodingGuidelines.md §3.6.
        unsafe void ResetComputeDrawcall(TtRenderPolicy policy)
        {
            if (mSetup == null || HzbMipsUAVs == null)
                return;

            var rc = TtEngine.Instance.GfxDevice.RenderContext;

            if (mSetupDrawcall == null)
            {
                mSetupDrawcall = rc.CreateComputeDraw();
                mSetupDrawcall.TagObject = this;
            }

            // 重建 downsample drawcalls (数量 = mip 数 - 1, 因为 mip0 由 Setup 处理).
            if (mMipsDrawcalls != null)
            {
                for (int i = 0; i < mMipsDrawcalls.Length; i++)
                    CoreSDK.DisposeObject(ref mMipsDrawcalls[i]);
            }

            int downSampleCount = HzbMipsUAVs.Length - 1;
            if (downSampleCount <= 0)
            {
                mMipsDrawcalls = null;
                return;
            }

            mMipsDrawcalls = new NxRHI.TtComputeDraw[downSampleCount];
            for (int i = 0; i < downSampleCount; i++)
            {
                var dc = rc.CreateComputeDraw();
                dc.TagObject = this;
                mMipsDrawcalls[i] = dc;
            }
        }

        public override unsafe void Tick(GamePlay.TtWorld world, Graphics.Pipeline.TtRenderPolicy policy, NxRHI.TtCommandList frameCmdList, bool bClear)
        {
            if (mSetup == null || mSetupDrawcall == null || HzbMipsUAVs == null || HzbMipsUAVs.Length == 0)
                return;

            if (TtEngine.Instance.GfxDevice.RenderContext.RhiType == NxRHI.ERhiType.RHI_D3D11)
            {
                // 已知 bug: dx11 下 shader 里访问 Texture2D<float> DepthBuffer 会触发 device remove,
                // 暂时绕过, hzb 在 dx11 路径下不工作. TODO: 排查根因后去掉这条早退.
                return;
            }

            var depthAttach = GetAttachBuffer(DepthPinIn);
            if (depthAttach == null || depthAttach.Srv == null)
                return;
            mCurrentDepthSrv = depthAttach.Srv;

            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            var cmd = rc.CmdListManager.GetCmdList();
            using (new NxRHI.TtCmdListScope(cmd, "Hzb"))
            {
                // ---------- Setup pass: depth -> mip0 ----------
                // dispatch 覆盖 mip0 的全部像素, 按 thread group 尺寸 ceil 取整.
                uint setupGx = MathHelper.Roundup(mMip0Width, DispatchSizeX);
                uint setupGy = MathHelper.Roundup(mMip0Height, DispatchSizeY);
                mSetup.SetDrawcallDispatch(this, policy, mSetupDrawcall, setupGx, setupGy, 1, false);
                cmd.PushGpuDraw(mSetupDrawcall);

                // ---------- DownSample chain: mip[i-1] -> mip[i] ----------
                if (mMipsDrawcalls != null && mDownSample != null)
                {
                    uint mipW = mMip0Width;
                    uint mipH = mMip0Height;
                    for (int i = 0; i < mMipsDrawcalls.Length; i++)
                    {
                        // 第 i 个 drawcall 处理 mip (i+1): src = mip[i], dst = mip[i+1].
                        mipW = mipW > 1 ? mipW / 2 : 1;
                        mipH = mipH > 1 ? mipH / 2 : 1;

                        mCurrentMipIndex = i + 1;

                        uint gx = MathHelper.Roundup(mipW, DispatchSizeX);
                        uint gy = MathHelper.Roundup(mipH, DispatchSizeY);
                        mDownSample.SetDrawcallDispatch(this, policy, mMipsDrawcalls[i], gx, gy, 1, true);
                        cmd.PushGpuDraw(mMipsDrawcalls[i]);
                    }
                }

                cmd.FlushDraws();
            }

            policy.CommitCommandList(cmd, "Hzb");
        }
    }
}
