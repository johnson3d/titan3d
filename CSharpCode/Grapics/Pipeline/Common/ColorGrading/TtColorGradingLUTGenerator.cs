using EngineNS.Graphics.Pipeline.Shader;
using EngineNS.NxRHI;
using System;

namespace EngineNS.Graphics.Pipeline.Common.ColorGrading
{
    /// <summary>
    /// Compute ShadingEnv that drives ColorGradingLUT.compute.
    /// </summary>
    internal class TtColorGradingLUTShading : TtComputeShadingEnv
    {
        public override Vector3ui DispatchArg => new Vector3ui(4, 4, 4);

        public TtColorGradingLUTShading()
        {
            CodeName = RName.GetRName("Shaders/Compute/ScreenSpace/ColorGradingLUT.compute", RName.ERNameType.Engine);
            MainName = "CS_GenerateLUT";
            UpdatePermutation().AddWaitTask();
        }

        public override void OnDrawCall(TtComputeDraw drawcall, TtRenderPolicy policy)
        {
            var generator = drawcall.TagObject as TtColorGradingLUTGenerator;
            if (generator == null)
                return;

            var uavBinder = drawcall.FindBinder(EShaderBindType.SBT_UAV, "OutputLUT");
            if (uavBinder.IsValidPointer)
                drawcall.BindUav(uavBinder, generator.LutUav);

            var cbBinder = drawcall.FindBinder(EShaderBindType.SBT_CBV, "cbColorGrading");
            if (cbBinder.IsValidPointer)
                drawcall.BindCBV(cbBinder, generator.GetOrCreateCBuffer(cbBinder));
        }
    }

    /// <summary>
    /// Self-contained LUT generator. Owns the 3D texture, UAV, SRV and compute dispatch.
    /// HdrNode creates one instance and calls <see cref="UpdateIfDirty"/> each frame.
    /// Not a RenderGraphNode — no pins, no graph wiring needed.
    /// </summary>
    public class TtColorGradingLUTGenerator : IDisposable
    {
        // ── 3D LUT Resources ──
        TtTexture mLutTexture;
        TtSrView mLutSrv;
        TtUaView mLutUav;

        public TtSrView LutSrv => mLutSrv;
        internal TtUaView LutUav => mLutUav;

        // ── Compute ──
        TtColorGradingLUTShading mShading;
        TtComputeDraw mComputeDraw;

        // ── CBuffer ──
        TtCbView mCBuffer;
        int mCachedSettingsHash;
        int mCurrentLutSize;

        // ── Current blended settings (written by HdrNode each frame) ──
        TtColorGradingSettings mActiveSettings = new TtColorGradingSettings();

        public bool IsInitialized => mShading != null && mLutUav != null;

        public async Thread.Async.TtTask Initialize()
        {
            mShading = new TtColorGradingLUTShading();
            await mShading.UpdatePermutation();

            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            mComputeDraw = rc.CreateComputeDraw();
            mComputeDraw.TagObject = this;

            CreateLutTexture(mActiveSettings.LutSize);
        }

        void CreateLutTexture(int lutSize)
        {
            lutSize = Math.Clamp(lutSize, 16, 64);
            mCurrentLutSize = lutSize;

            var rc = TtEngine.Instance.GfxDevice.RenderContext;

            if (mLutUav != null) { mLutUav.Dispose(); mLutUav = null; }
            if (mLutSrv != null) { mLutSrv.Dispose(); mLutSrv = null; }
            if (mLutTexture != null) { mLutTexture.Dispose(); mLutTexture = null; }

            var texDesc = new FTextureDesc();
            texDesc.SetDefault();
            texDesc.Width = (uint)lutSize;
            texDesc.Height = (uint)lutSize;
            texDesc.Depth = (uint)lutSize;
            texDesc.MipLevels = 1;
            texDesc.Format = EPixelFormat.PXF_R16G16B16A16_FLOAT;
            texDesc.BindFlags = EBufferType.BFT_SRV | EBufferType.BFT_UAV;
            mLutTexture = rc.CreateTexture(in texDesc);

            var srvDesc = new FSrvDesc();
            srvDesc.SetTexture3D();
            srvDesc.Format = EPixelFormat.PXF_R16G16B16A16_FLOAT;
            srvDesc.Texture3D.MipLevels = 1;
            mLutSrv = rc.CreateSRV(mLutTexture, in srvDesc);

            var uavDesc = new FUavDesc();
            uavDesc.ViewDimension = EDimensionUAV.UAV_DIMENSION_TEXTURE3D;
            uavDesc.Format = EPixelFormat.PXF_R16G16B16A16_FLOAT;
            uavDesc.Texture3D.MipSlice = 0;
            uavDesc.Texture3D.FirstWSlice = 0;
            uavDesc.Texture3D.WSize = (uint)lutSize;
            mLutUav = rc.CreateUAV(mLutTexture, in uavDesc);

            mCachedSettingsHash = 0;
        }

        /// <summary>
        /// Push new blended settings and dispatch compute if parameters changed.
        /// Called by HdrNode.Tick each frame.
        /// </summary>
        /// <param name="settings">The blended color grading settings for this frame.</param>
        /// <param name="policy">Current render policy (needed for SetDrawcallDispatch).</param>
        public void UpdateIfDirty(TtColorGradingSettings settings, TtRenderPolicy policy)
        {
            if (!IsInitialized || settings == null)
                return;

            mActiveSettings = settings;

            if (settings.LutSize != mCurrentLutSize)
                CreateLutTexture(settings.LutSize);

            int currentHash = settings.GetSettingsHash();
            if (currentHash == mCachedSettingsHash)
                return;
            mCachedSettingsHash = currentHash;

            uint groupCount = (uint)mCurrentLutSize;
            mShading.SetDrawcallDispatch(this, policy, mComputeDraw, groupCount, groupCount, groupCount, true);

            var cmd = TtEngine.Instance.GfxDevice.RenderContext.CmdListManager.GetCmdList();
            using (new TtCmdListScope(cmd, "ColorGradingLUT"))
            {
                cmd.PushGpuDraw(mComputeDraw);
                cmd.FlushDraws();
            }
            policy.CommitCommandList(cmd, "ColorGradingLUT");
        }

        internal TtCbView GetOrCreateCBuffer(FShaderBinder binder)
        {
            if (mCBuffer == null)
            {
                mCBuffer = TtEngine.Instance.GfxDevice.RenderContext.CreateCBV(binder);
            }

            WriteAllCBufferFields(mActiveSettings, (float)mCurrentLutSize);
            mCBuffer.FlushDirty();
            return mCBuffer;
        }

        void WriteAllCBufferFields(TtColorGradingSettings s, float lutSize)
        {
            mCBuffer.SetValue("WhiteTemp", s.WhiteTemp);
            mCBuffer.SetValue("WhiteTint", s.WhiteTint);
            mCBuffer.SetValue("ShadowsMax", s.ShadowsMax);
            mCBuffer.SetValue("HighlightsMin", s.HighlightsMin);
            mCBuffer.SetValue("HighlightsMax", s.HighlightsMax);
            mCBuffer.SetValue("LUTSize", lutSize);

            var v = s.ColorSaturation; mCBuffer.SetValue("ColorSaturation", in v);
            v = s.ColorContrast; mCBuffer.SetValue("ColorContrast", in v);
            v = s.ColorGamma; mCBuffer.SetValue("ColorGamma", in v);
            v = s.ColorGain; mCBuffer.SetValue("ColorGain", in v);
            v = s.ColorOffset; mCBuffer.SetValue("ColorOffset", in v);

            v = s.ColorSaturationShadows; mCBuffer.SetValue("ColorSaturationShadows", in v);
            v = s.ColorContrastShadows; mCBuffer.SetValue("ColorContrastShadows", in v);
            v = s.ColorGammaShadows; mCBuffer.SetValue("ColorGammaShadows", in v);
            v = s.ColorGainShadows; mCBuffer.SetValue("ColorGainShadows", in v);
            v = s.ColorOffsetShadows; mCBuffer.SetValue("ColorOffsetShadows", in v);

            v = s.ColorSaturationMidtones; mCBuffer.SetValue("ColorSaturationMidtones", in v);
            v = s.ColorContrastMidtones; mCBuffer.SetValue("ColorContrastMidtones", in v);
            v = s.ColorGammaMidtones; mCBuffer.SetValue("ColorGammaMidtones", in v);
            v = s.ColorGainMidtones; mCBuffer.SetValue("ColorGainMidtones", in v);
            v = s.ColorOffsetMidtones; mCBuffer.SetValue("ColorOffsetMidtones", in v);

            v = s.ColorSaturationHighlights; mCBuffer.SetValue("ColorSaturationHighlights", in v);
            v = s.ColorContrastHighlights; mCBuffer.SetValue("ColorContrastHighlights", in v);
            v = s.ColorGammaHighlights; mCBuffer.SetValue("ColorGammaHighlights", in v);
            v = s.ColorGainHighlights; mCBuffer.SetValue("ColorGainHighlights", in v);
            v = s.ColorOffsetHighlights; mCBuffer.SetValue("ColorOffsetHighlights", in v);
        }

        public void Dispose()
        {
            CoreSDK.DisposeObject(ref mComputeDraw);
            CoreSDK.DisposeObject(ref mCBuffer);

            if (mLutUav != null) { mLutUav.Dispose(); mLutUav = null; }
            if (mLutSrv != null) { mLutSrv.Dispose(); mLutSrv = null; }
            if (mLutTexture != null) { mLutTexture.Dispose(); mLutTexture = null; }
        }
    }
}
