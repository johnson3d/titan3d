using EngineNS;

namespace LocalizationTool
{
    // 多国化工具应用入口。参照 BrickManager：继承 TtSlateApplication + ITickable，
    // 在初始化时创建并注册编辑器根窗口。
    public partial class TtLocalizationApplication : TtSlateApplication, ITickable
    {
        #region ITickable
        public int GetTickOrder()
        {
            return 0;
        }

        public void TickLogic(float ellapse)
        {
        }

        public void TickRender(float ellapse)
        {
        }

        public void TickBeginFrame(float ellapse)
        {
        }

        public void TickSync(float ellapse)
        {
        }
        #endregion

        public static TtLocalizationApplication Instance
        {
            get
            {
                return TtEngine.Instance.GfxDevice.SlateApplication as TtLocalizationApplication;
            }
        }

        public TtLocalizationEditor LocalizationEditor = null;

        public override async EngineNS.Thread.Async.TtTask<bool> InitializeApplication(EngineNS.NxRHI.TtGpuDevice rc, RName rpName)
        {
            await base.InitializeApplication(rc, rpName);

            LocalizationEditor = new TtLocalizationEditor();
            await LocalizationEditor.Initialize();
            LocalizationEditor.Visible = true;

            TtEngine.Instance.TickableManager.AddTickable(this);
            Console.WriteLine("LocalizationTool application started.");
            return true;
        }

        public override void Cleanup()
        {
            TtEngine.Instance.TickableManager.RemoveTickable(this);
            EngineNS.CoreSDK.DisposeObject(ref LocalizationEditor);
            base.Cleanup();
        }
    }
}
