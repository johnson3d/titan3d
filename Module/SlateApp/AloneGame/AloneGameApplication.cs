using EngineNS;
using EngineNS.Bricks.Network.RPC;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AloneGame
{
    public partial class TtAloneGameApplication : EngineNS.GamePlay.TtGameApplication
    {
        public static TtAloneGameApplication Instance
        {
            get
            {
                return TtEngine.Instance.GfxDevice.SlateApplication as TtAloneGameApplication;
            }
        }

        public TtGameForm GameForm = null;
        float mAutoRenderDocCaptureSeconds = -1.0f;
        float mAutoRenderDocQuitSeconds = -1.0f;
        bool mAutoRenderDocCaptureRequested;
        bool mAutoRenderDocQuitRequested;
        System.Threading.Timer mAutoRenderDocCaptureTimer;
        System.Threading.Timer mAutoRenderDocQuitTimer;

        public override async EngineNS.Thread.Async.TtTask<bool> InitializeApplication(EngineNS.NxRHI.TtGpuDevice rc, RName rpName)
        {
            var args = Environment.GetCommandLineArgs();
            mAutoRenderDocCaptureSeconds = ReadFloatArgument(args, "AutoRenderDocCaptureSeconds=", -1.0f);
            mAutoRenderDocQuitSeconds = ReadFloatArgument(args, "AutoRenderDocQuitSeconds=", -1.0f);

            await base.InitializeApplication(rc, rpName);
            StartAutoRenderDocTimers();

            //GameForm = new TtGameForm();
            //await GameForm.Initialize();
            //GameForm.Visible = true;

            return true;
        }
        void StartAutoRenderDocTimers()
        {
            if (mAutoRenderDocCaptureSeconds >= 0.0f)
            {
                mAutoRenderDocCaptureTimer = new System.Threading.Timer(_ => RequestAutoRenderDocCapture(),
                    null,
                    TimeSpan.FromSeconds(mAutoRenderDocCaptureSeconds),
                    System.Threading.Timeout.InfiniteTimeSpan);
            }
            if (mAutoRenderDocQuitSeconds >= 0.0f)
            {
                mAutoRenderDocQuitTimer = new System.Threading.Timer(_ => RequestAutoQuit(),
                    null,
                    TimeSpan.FromSeconds(mAutoRenderDocQuitSeconds),
                    System.Threading.Timeout.InfiniteTimeSpan);
            }
        }
        static float ReadFloatArgument(string[] args, string name, float fallback)
        {
            var value = TtEngine.FindArgument(args, name);
            if (string.IsNullOrWhiteSpace(value))
                return fallback;

            if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
                return result;

            return fallback;
        }
        public override void TickLogic(float ellapse)
        {
            base.TickLogic(ellapse);

        }
        void RequestAutoRenderDocCapture()
        {
            if (mAutoRenderDocCaptureRequested)
                return;

            mAutoRenderDocCaptureRequested = true;

            unsafe
            {
                IRenderDocTool.GetInstance().SetGpuDevice(TtEngine.Instance.GfxDevice.RenderContext.mCoreObject);
                IRenderDocTool.GetInstance().SetActiveWindow(TtEngine.Instance.GfxDevice.SlateApplication.NativeWindow.HWindow.ToPointer());
            }
            TtEngine.Instance.GfxDevice.RenderQueue.RemainingCaptureFrames = 0;
            TtEngine.Instance.GfxDevice.RenderQueue.CaptureRenderDocFrame = true;

            EngineNS.Profiler.Log.WriteLine<EngineNS.Profiler.TtGraphicsGategory>(
                EngineNS.Profiler.ELogTag.Info,
                $"Auto RenderDoc capture requested after {mAutoRenderDocCaptureSeconds.ToString("F2", CultureInfo.InvariantCulture)}s");
        }
        void RequestAutoQuit()
        {
            if (mAutoRenderDocQuitRequested)
                return;

            mAutoRenderDocQuitRequested = true;
            TtEngine.Instance.PostQuitMessage();
        }
        public override void Cleanup()
        {
            mAutoRenderDocCaptureTimer?.Dispose();
            mAutoRenderDocCaptureTimer = null;
            mAutoRenderDocQuitTimer?.Dispose();
            mAutoRenderDocQuitTimer = null;
            base.Cleanup();
        }
    }
}
