using EngineNS;

namespace BrickManager
{
    public partial class TtBrickManagerApplication : TtSlateApplication, ITickable
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

        public static TtBrickManagerApplication Instance
        {
            get
            {
                return TtEngine.Instance.GfxDevice.SlateApplication as TtBrickManagerApplication;
            }
        }

        public TtBrickManagerEditor BrickManagerEditor = null;

        public override async EngineNS.Thread.Async.TtTask<bool> InitializeApplication(EngineNS.NxRHI.TtGpuDevice rc, RName rpName)
        {
            await base.InitializeApplication(rc, rpName);

            BrickManagerEditor = new TtBrickManagerEditor();
            await BrickManagerEditor.Initialize();
            BrickManagerEditor.Visible = true;

            TtEngine.Instance.TickableManager.AddTickable(this);
            Console.WriteLine("BrickManager application started.");
            return true;
        }

        public override void Cleanup()
        {
            TtEngine.Instance.TickableManager.RemoveTickable(this);
            EngineNS.CoreSDK.DisposeObject(ref BrickManagerEditor);
            base.Cleanup();
        }
    }
}
