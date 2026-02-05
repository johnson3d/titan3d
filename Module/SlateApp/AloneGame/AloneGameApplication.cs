using EngineNS;
using EngineNS.Bricks.Network.RPC;
using System;
using System.Collections.Generic;
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
        public override async EngineNS.Thread.Async.TtTask<bool> InitializeApplication(EngineNS.NxRHI.TtGpuDevice rc, RName rpName)
        {
            await base.InitializeApplication(rc, rpName);

            //GameForm = new TtGameForm();
            //await GameForm.Initialize();
            //GameForm.Visible = true;

            return true;
        }
        public override void Cleanup()
        {
            base.Cleanup();
        }
    }
}
