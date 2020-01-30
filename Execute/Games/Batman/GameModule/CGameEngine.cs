using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using EngineNS;
using System.Runtime.InteropServices;

namespace Game
{
    public class CGameEngine : EngineNS.CEngine
    {
        public CGameEngine()
        {
            
        }
        public static CGameEngine GameEngine
        {
            get
            {
                return (CGameEngine)Instance;
            }
        }
        public override async System.Threading.Tasks.Task InitDefaultResource(EngineNS.GamePlay.GGameInstanceDesc gameDesc)
        {
            //PrebuildPassData.DefaultShadingEnvs = PrebuildPassData.InitPbrShadingEnvs(RenderContext);
            //PrebuildPassData.DefaultShadingEnvs = PrebuildPassData.InitNprShadingEnvs(RenderContext);
            PrebuildPassData.DefaultShadingEnvs = PrebuildPassData.InitEditorMobileShadingEnv(RenderContext);

            await base.InitDefaultResource(gameDesc);

            CEngine.UseInstancing = true;
            //this.EngineTimeScale = 0.1f;
        }
        public override async System.Threading.Tasks.Task<bool> OnEngineInited()
        {
            await base.OnEngineInited();

            var desc = new SuperSocket.SocketServerBaseDesc();
            desc.MaxConnect = 16;
            
            //await this.RemoteServices.InitServer(desc);
            //await this.RemoteServices.InitClient("127.0.0.1", 2020);

            //var rn = EngineNS.RName.GetRName("GameTable/perfview.cfg");
            //this.Stat.PViewer.LoadReportLists(rn);
            return true;
        }
        public override void TickSync()
        {
            //this.EngineTimeScale = 0.1f;
            base.TickSync();
#if PWindow
            if (CEngine.Instance.RemoteServices != null)
            {
                var conn = CEngine.Instance.RemoteServices.Client;
                var pkg = new EngineNS.Bricks.RemoteServices.RemoteServicesHelper.S2C_Test.ArgumentData();
                pkg.A = 100;
                pkg.B = 0.2f;
                EngineNS.Bricks.RemoteServices.RemoteServicesHelper.S2C_Test.Instance.DoCall(
                    ref pkg,
                    EngineNS.Bricks.NetCore.ERouteTarget.Self);
                System.GC.Collect(0);
            }
#endif
        }
    }
}
