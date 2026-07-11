using EngineNS.GamePlay;
using EngineNS.Graphics.Pipeline;
using EngineNS.Graphics.Pipeline.Shader;
using EngineNS.NxRHI;
using System;
using System.ComponentModel;

namespace EngineNS.Bricks.FX.Water
{
    /// <summary>
    /// 浅水方程边界模式
    /// </summary>
    public enum ESWEBoundaryMode : uint
    {
        Open = 0,
        Reflecting = 1,
    }

    // =========================================================================
    // ShadingEnv - SWE Compute Dispatch
    // 绑定 PrevState (SRV) + CurrState (UAV) + cbSWE, 每帧 dispatch 一次
    // =========================================================================
    public class TtSWEComputeShading : TtComputeShadingEnv
    {
        public override Vector3ui DispatchArg => new Vector3ui(8, 8, 1);

        public TtSWEComputeShading()
        {
            CodeName = RName.GetRName("Shaders/Bricks/FX/SWEWater.compute", RName.ERNameType.Engine);
            MainName = "CS_Main";
            UpdatePermutation().AddWaitTask();
        }

        public override void OnDrawCall(TtComputeDraw drawcall, TtRenderPolicy policy)
        {
            var node = drawcall.TagObject as TtSWEComputeNode;
            if (node == null)
                return;

            drawcall.BindSrv("PrevState", node.GetPrevStateSrv());
            drawcall.BindUav("CurrState", node.GetCurrStateUav());
            drawcall.BindSampler("Samp_PointClamp",
                TtEngine.Instance.GfxDevice.SamplerStateManager.PointState);

            var cbBinder = drawcall.FindBinder(EShaderBindType.SBT_CBV, "cbSWE");
            if (cbBinder.IsValidPointer)
                drawcall.BindCBV(cbBinder, node.GetOrCreateCBuffer(cbBinder));
        }
    }

    // =========================================================================
    // TtSWEComputeNode - 迷你 RDG 中唯一的 Compute 节点
    // 管理 ping-pong 状态纹理 + CBuffer + Dispatch
    // =========================================================================
    public class TtSWEComputeNode : TAuxRenderGraphNode<TtSWEComputeNode>
    {
        // ---------- 模拟参数 (由 TtSWEWaterNode 设置) ----------
        public uint SimResolution { get; set; } = 128;
        public float DomainSize { get; set; } = 128.0f;
        public float TimeStep { get; set; } = 1.0f / 60.0f;
        public float Gravity { get; set; } = 9.81f;
        public float Damping { get; set; } = 0.995f;
        public ESWEBoundaryMode BoundaryMode { get; set; } = ESWEBoundaryMode.Open;
        public float BaseWaterHeight { get; set; } = 1.0f;

        // ---------- 扰动请求 (外部每帧设置) ----------
        public bool PendingDisturb { get; set; } = false;
        public float DisturbPosX { get; set; }
        public float DisturbPosZ { get; set; }
        public float DisturbRadius { get; set; } = 3.0f;
        public float DisturbStrength { get; set; } = 1.0f;

        // ---------- Internal ----------
        TtSWEComputeShading mShading;
        TtComputeDraw mDrawCall;
        TtCbView mCBuffer;

        // Ping-pong 状态纹理: RGBA32F, R=h, G=u, B=v
        TtTexture[] mStateTextures = new TtTexture[2];
        TtSrView[] mStateSrv = new TtSrView[2];
        TtUaView[] mStateUav = new TtUaView[2];
        uint mCurrentWriteSlot = 0; // 当帧写入的 slot

        // 暴露给外部消费 (材质节点采样)
        public TtSrView HeightMapSrv => mStateSrv[mCurrentWriteSlot];
        public TtTexture HeightMapTexture => mStateTextures[mCurrentWriteSlot];

        // 供 OnDrawCall 读取的暂存字段
        public TtSrView GetPrevStateSrv() => mStateSrv[mCurrentWriteSlot ^ 1];
        public TtUaView GetCurrStateUav() => mStateUav[mCurrentWriteSlot];

        public TtSWEComputeNode()
        {
            Name = "SWECompute";
        }

        public TtRenderGraphPin ResultPinOut = TtRenderGraphPin.CreateOutput(
            "Result", false, EPixelFormat.PXF_R32G32B32A32_FLOAT,
            EBufferType.BFT_SRV | EBufferType.BFT_UAV);

        public override void InitNodePins()
        {
            AddOutput(ResultPinOut);
        }

        public override async Thread.Async.TtTask Initialize(TtRenderPolicy policy, string debugName)
        {
            await base.Initialize(policy, debugName);
            mShading = await TtShadingEnv.CreateShadingEnv<TtSWEComputeShading>();

            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            mDrawCall = rc.CreateComputeDraw();
            mDrawCall.TagObject = this;

            CreateStateTextures();
        }

        public override void Dispose()
        {
            CoreSDK.DisposeObject(ref mDrawCall);
            CoreSDK.DisposeObject(ref mCBuffer);
            ReleaseStateTextures();
            base.Dispose();
        }

        void CreateStateTextures()
        {
            ReleaseStateTextures();
            var rc = TtEngine.Instance.GfxDevice.RenderContext;

            for (int i = 0; i < 2; i++)
            {
                var texDesc = new FTextureDesc();
                texDesc.SetDefault();
                texDesc.Width = SimResolution;
                texDesc.Height = SimResolution;
                texDesc.MipLevels = 1;
                texDesc.ArraySize = 1;
                texDesc.Format = EPixelFormat.PXF_R32G32B32A32_FLOAT;
                texDesc.BindFlags = EBufferType.BFT_SRV | EBufferType.BFT_UAV;

                mStateTextures[i] = rc.CreateTexture(in texDesc);
                mStateTextures[i].SetDebugName($"SWEState_{i}");

                var srvDesc = new FSrvDesc();
                srvDesc.SetTexture2D();
                srvDesc.Format = texDesc.Format;
                srvDesc.Texture2D.MipLevels = 1;
                srvDesc.Texture2D.MostDetailedMip = 0;
                mStateSrv[i] = rc.CreateSRV(mStateTextures[i], in srvDesc);

                var uavDesc = new FUavDesc();
                uavDesc.SetTexture2D();
                uavDesc.Format = texDesc.Format;
                uavDesc.Texture2D.MipSlice = 0;
                mStateUav[i] = rc.CreateUAV(mStateTextures[i], in uavDesc);
            }
        }

        void ReleaseStateTextures()
        {
            for (int i = 0; i < 2; i++)
            {
                if (mStateUav[i] != null) { mStateUav[i].Dispose(); mStateUav[i] = null; }
                if (mStateSrv[i] != null) { mStateSrv[i].Dispose(); mStateSrv[i] = null; }
                if (mStateTextures[i] != null) { mStateTextures[i].Dispose(); mStateTextures[i] = null; }
            }
        }

        /// <summary>
        /// §1.1 合规: 首次 CreateCBV 全字段 SetValue + MarkDirty + FlushDirty
        /// </summary>
        public TtCbView GetOrCreateCBuffer(FShaderBinder binder)
        {
            if (mCBuffer == null)
            {
                mCBuffer = TtEngine.Instance.GfxDevice.RenderContext.CreateCBV(binder);
                FillCBufferValues(mCBuffer);
                mCBuffer.MarkDirty();
                mCBuffer.FlushDirty();
                return mCBuffer;
            }
            FillCBufferValues(mCBuffer);
            return mCBuffer;
        }

        void FillCBufferValues(TtCbView cb)
        {
            var resolution = new Vector2ui(SimResolution, SimResolution);
            cb.SetValue("Resolution", in resolution);
            cb.SetValue("DomainSize", DomainSize);
            cb.SetValue("Dt", TimeStep);
            cb.SetValue("Gravity", Gravity);
            cb.SetValue("Damping", Damping);
            cb.SetValue("BoundaryMode", (uint)BoundaryMode);
            cb.SetValue("BaseWaterHeight", BaseWaterHeight);
            cb.SetValue("DisturbPosX", DisturbPosX);
            cb.SetValue("DisturbPosZ", DisturbPosZ);
            cb.SetValue("DisturbRadius", DisturbRadius);
            cb.SetValue("DisturbStrength", DisturbStrength);
            uint enableDisturb = PendingDisturb ? 1u : 0u;
            cb.SetValue("EnableDisturb", enableDisturb);
        }

        public override unsafe void Tick(TtWorld world, TtRenderPolicy policy, TtCommandList frameCmdList, bool bClear)
        {
            if (mShading == null || !mShading.IsReady)
                return;

            // Ping-pong: 读上一帧, 写当帧
            mCurrentWriteSlot ^= 1;

            var cmd = TtCommandList.GetCmdList();
            using (new TtCmdListScope(cmd, "SWECompute"))
            {
                mShading.SetDrawcallDispatch(this, policy, mDrawCall,
                    SimResolution, SimResolution, 1, true);
                cmd.PushGpuDraw(mDrawCall);

                cmd.FlushDraws();
            }
            policy.CommitCommandList(cmd);

            // 扰动是一次性的, dispatch 后清除
            PendingDisturb = false;
        }
    }
}
