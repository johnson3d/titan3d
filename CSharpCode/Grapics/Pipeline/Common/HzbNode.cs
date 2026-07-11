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
        // 全 mip 的 SRV, 给下游 (ReSTIR HiZ ray-march) 用 SampleLevel(LOD) 跨 mip 采样.
        public NxRHI.TtSrView HzbSRV;
        // 每个 mip 一份独立 UAV (MipSlice = i, 单 subresource). 用于 CS_Setup 写 mip0
        // 与 CS_DownSample 写 mip i.
        public NxRHI.TtUaView[] HzbMipsUAVs;
        // 每个 mip 一份独立 SRV (MostDetailedMip = i, MipLevels = 1, 单 subresource).
        // 用于 CS_DownSample 把"上一级 mip"作为 SRV 读 (与 DstBuffer UAV 解耦, 避免
        // RWTexture2D 同槽位冲突, 详见 hzb.compute SrcBuffer 注释).
        public NxRHI.TtSrView[] HzbMipsSRVs;

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
                // HZB 现在直接存 raw NDC-Z, 不再做线性化, 不需要 cbPerCamera.
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

                if (node.HzbMipsSRVs == null || srcMip >= node.HzbMipsSRVs.Length) return;

                // SrcBuffer 是 Texture2D<float> (SRV), DstBuffer 是 RWTexture2D<float> (UAV).
                // 必须分别走 SBT_SRV / SBT_UAV 才能避免两个 RWTexture2D 互相覆盖到同一份默认
                // 视图的历史 BUG (详见 hzb.compute 顶部 SrcBuffer 注释).
                drawcall.BindSrv("SrcBuffer", node.HzbMipsSRVs[srcMip]);
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
            if (HzbMipsSRVs != null)
            {
                for (int i = 0; i < HzbMipsSRVs.Length; i++)
                    CoreSDK.DisposeObject(ref HzbMipsSRVs[i]);
                HzbMipsSRVs = null;
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

            // 释放旧的 per-mip UAV / SRV. 注意: 这两个数组的元素顺序与 mip index 一一对应.
            if (HzbMipsUAVs != null)
            {
                for (int i = 0; i < HzbMipsUAVs.Length; i++)
                    CoreSDK.DisposeObject(ref HzbMipsUAVs[i]);
            }
            if (HzbMipsSRVs != null)
            {
                for (int i = 0; i < HzbMipsSRVs.Length; i++)
                    CoreSDK.DisposeObject(ref HzbMipsSRVs[i]);
            }

            int mipCount = NxRHI.TtSrView.CalcMipLevel((int)mMip0Width, (int)mMip0Height, true, 1);
            HzbMipsUAVs = new NxRHI.TtUaView[mipCount];
            HzbMipsSRVs = new NxRHI.TtSrView[mipCount];

            var dsTexDesc = new NxRHI.FTextureDesc();
            dsTexDesc.SetDefault();
            dsTexDesc.Width = mMip0Width;
            dsTexDesc.Height = mMip0Height;
            dsTexDesc.MipLevels = (uint)HzbMipsUAVs.Length;
            dsTexDesc.Format = EPixelFormat.PXF_R32_TYPELESS;
            dsTexDesc.BindFlags = NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_UAV;

            CoreSDK.DisposeObject(ref HzbSRV);
            CoreSDK.DisposeObject(ref HzbTexture);
            HzbTexture = rc.CreateTexture(in dsTexDesc);

            // 全 mip SRV: 下游 (ReSTIR HiZ) 用 SampleLevel(LOD = mipClamped) 跨 mip 采样,
            // 必须覆盖 [0, MipLevels). SetTexture2D() 后必须显式填 MipLevels / MostDetailedMip,
            // 否则 D3D12 报 DXGI_ERROR_INVALID_CALL (踩过坑, 见 DenoiseNode 同位置注释).
            var srvDesc = new NxRHI.FSrvDesc();
            srvDesc.SetTexture2D();
            srvDesc.Type = NxRHI.ESrvType.ST_Texture2D;
            srvDesc.Format = EPixelFormat.PXF_R32_FLOAT;
            srvDesc.Texture2D.MostDetailedMip = 0;
            srvDesc.Texture2D.MipLevels = dsTexDesc.MipLevels;
            HzbSRV = rc.CreateSRV(HzbTexture, in srvDesc);

            // Per-mip UAV: 每张 UAV 只看一个 mip subresource (MipSlice = i), 用作 DownSample
            // pass 的 DstBuffer 与 Setup pass 的 mip0 写入目标.
            for (int i = 0; i < HzbMipsUAVs.Length; i++)
            {
                var uavDesc = new NxRHI.FUavDesc();
                uavDesc.SetTexture2D();
                uavDesc.Format = EPixelFormat.PXF_R32_FLOAT;
                uavDesc.Texture2D.MipSlice = (uint)i;
                HzbMipsUAVs[i] = rc.CreateUAV(HzbTexture, in uavDesc);
            }

            // Per-mip SRV: 每张 SRV 只看一个 mip subresource (MostDetailedMip = i, MipLevels = 1),
            // 用作 DownSample pass 的 SrcBuffer (= 上一级 mip). 必须用 SRV 而不是再用 UAV,
            // 否则两个 RWTexture2D binder 会被引擎绑到同一份默认视图, 覆盖全 mip, mip 链全断.
            for (int i = 0; i < HzbMipsSRVs.Length; i++)
            {
                var mipSrvDesc = new NxRHI.FSrvDesc();
                mipSrvDesc.SetTexture2D();
                mipSrvDesc.Type = NxRHI.ESrvType.ST_Texture2D;
                mipSrvDesc.Format = EPixelFormat.PXF_R32_FLOAT;
                mipSrvDesc.Texture2D.MostDetailedMip = (uint)i;
                mipSrvDesc.Texture2D.MipLevels = 1;
                HzbMipsSRVs[i] = rc.CreateSRV(HzbTexture, in mipSrvDesc);
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
            var cmd = NxRHI.TtCommandList.GetCmdList();
            using (new NxRHI.TtCmdListScope(cmd, "Hzb"))
            {
                // ---------- Setup pass: depth -> mip0 ----------
                // SetDrawcallDispatch(..., bRoundupXYZ=true) 内部会自动按 DispatchArg
                // (= shader 的 [numthreads(DispatchX, DispatchY, DispatchZ)]) ceil 取整,
                // 所以这里直接传"线程总数" (= mip 像素数), 不能在外面再 Roundup, 否则
                // 双重除法会把 dispatch 退化成 (1,1,1) (踩过坑, RenderDoc 实证: mip 链
                // 每级只有左上角 32x32 像素被处理, 其余全是 0).
                // 参考: DenoiseNode.cs L515, ReSTIRGINode.cs L653 都是直接传 w,h,1,true.
                mSetup.SetDrawcallDispatch(this, policy, mSetupDrawcall, mMip0Width, mMip0Height, 1, true);
                cmd.PushGpuDraw(mSetupDrawcall);

                //cmd.PushAction((EngineNS.NxRHI.ICommandList cmd, void* arg1) =>
                //{
                //    HzbTexture.IsAutoTransition = false;
                //    for (int i = 0; i < mMipsDrawcalls.Length; i++)
                //    {
                //        cmd.SetTextureBarrier(HzbTexture.mCoreObject, (uint)i, 1,
                //            NxRHI.EPipelineStage.PPLS_ALL_COMMANDS, NxRHI.EPipelineStage.PPLS_ALL_COMMANDS,
                //            NxRHI.EGpuResourceState.GRS_Uav, NxRHI.EGpuResourceState.GRS_Uav);
                //    }
                //}, (void*)IntPtr.Zero);

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

                        // 同 Setup pass: 传线程总数, 由引擎按 DispatchArg ceil 取整.
                        mDownSample.SetDrawcallDispatch(this, policy, mMipsDrawcalls[i], mipW, mipH, 1, true);
                        //cmd.PushAction((EngineNS.NxRHI.ICommandList cmd, void* arg1) =>
                        //{
                        //    cmd.SetTextureBarrier(HzbTexture.mCoreObject, (uint)i, 1,
                        //        NxRHI.EPipelineStage.PPLS_ALL_COMMANDS, NxRHI.EPipelineStage.PPLS_ALL_COMMANDS,
                        //        NxRHI.EGpuResourceState.GRS_Uav, NxRHI.EGpuResourceState.GRS_GenericRead);
                        //}, (void*)IntPtr.Zero);
                        cmd.PushGpuDraw(mMipsDrawcalls[i]);
                    }
                }

                //cmd.PushAction((EngineNS.NxRHI.ICommandList cmd, void* arg1) =>
                //{
                //    HzbTexture.IsAutoTransition = true;
                //    for (int i = 0; i < mMipsDrawcalls.Length; i++)
                //    {
                //        cmd.SetTextureBarrier(HzbTexture.mCoreObject, (uint)i, 1,
                //            NxRHI.EPipelineStage.PPLS_ALL_COMMANDS, NxRHI.EPipelineStage.PPLS_ALL_COMMANDS,
                //            NxRHI.EGpuResourceState.GRS_GenericRead, NxRHI.EGpuResourceState.GRS_Uav);
                //    }
                //}, (void*)IntPtr.Zero);

                cmd.FlushDraws();
            }

            policy.CommitCommandList(cmd, "Hzb");
        }
    }
}
