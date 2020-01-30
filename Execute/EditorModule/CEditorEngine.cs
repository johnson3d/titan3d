using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using EngineNS;
using EngineNS.Editor;

namespace CoreEditor
{
    public class CEditorEngine : EngineNS.CEngine
    {
        public CEditorEngine()
        {
            
        }
        public static CEditorEngine EditorEngine
        {
            get
            {
                return (CEditorEngine)Instance;
            }
        }
        public override void Cleanup()
        {
            var ed = GameEditorInstance as CEditorInstance;
            ed?.Cleanup();

            base.Cleanup();
        }

        bool EditorInited = false;
        public bool InitSystem(EngineNS.ERHIType rhiType, IntPtr window, bool debugGraphicsLayer = false)
        {
            //rhiType = ERHIType.RHT_OGL;
            //debugGraphicsLayer = true;
            var ok = EngineNS.CEngine.Instance.GfxInitEngine(rhiType, 0, window, debugGraphicsLayer);
            if (ok == false)
            {
                return false;
            }

            var rc = EngineNS.CEngine.Instance.RenderContext;

            ScopeTickRender.Enable = true;
            ScopeTickLogic.Enable = true;

            return true;
        }
        public async System.Threading.Tasks.Task OnMainWindowCreated(EditorCommon.ViewPort.ViewPortControl editViewport)
        {
            if (EditorInited == true)
                return;
            EditorInited = true;

            var gameDesc = new EngineNS.Editor.CEditorInstanceDesc();
            gameDesc.SceneName = gameDesc.DefaultMapName;

            await InitDefaultResource(gameDesc);
            EngineNS.Thread.Async.TaskLoader.Release(ref WaitContext, this);

            var editor = GameEditorInstance as CEditorInstance;
            await editor?.InitEditor(editViewport, gameDesc);
        }
        public override async System.Threading.Tasks.Task InitDefaultResource(EngineNS.GamePlay.GGameInstanceDesc gameDesc)
        {
            //PrebuildPassData.DefaultShadingEnvs = PrebuildPassData.InitFullShadingEnvs(RenderContext);
            PrebuildPassData.DefaultShadingEnvs = PrebuildPassData.InitEditorMobileShadingEnv(RenderContext);

            //await EngineNS.Graphics.CGfxEffectDesc.CheckRebuildShaders();

            if (Desc.LoadAllShaders)
            {
                await EngineNS.Graphics.CGfxEffectDesc.LoadAllShaders(this.RenderContext);
            }

            //这个Rebuild Shader cache的行为，还是在编辑器发布游戏的时候做吧。
            await EngineNS.Graphics.CGfxEffectDesc.BuildCachesWhenCleaned(this.RenderContext);

            await base.InitDefaultResource(gameDesc);

            
            //PrebuildPassData.DefaultShadingEnvs = PrebuildPassData.InitNprShadingEnvs(RenderContext);
        }
        public override async System.Threading.Tasks.Task<bool> OnEngineInited()
        {
            await base.OnEngineInited();

            var desc = new SuperSocket.SocketServerBaseDesc();
            desc.MaxConnect = 16;
            //desc.Ip = CEngine.Instance.Desc.ProfilerServerIp;
            desc.Port = CEngine.Instance.Desc.ProfilerServerPort;
            await this.RemoteServices.InitServer(desc);

            var rn = EngineNS.RName.GetRName("GameTable/perfview.cfg");
            this.Stat.PViewer.LoadReportLists(rn);

            //await this.RemoteServices.InitClient("127.0.0.1", 2020);
            //this.Stat.PViewer.IsReporting = true;

            return true;
        }
        public override void OnGameStarted()
        {
            var editor = this.GameEditorInstance as CEditorInstance;
            editor.World.DefaultScene.NeedTick = false;
        }
        public override void OnGameStoped()
        {
            var editor = this.GameEditorInstance as CEditorInstance;
            editor.World.DefaultScene.NeedTick = true;
        }
        public void OnWindowsSizeChanged(System.Windows.Window window)
        {
            if (EngineNS.CEngine.Instance == null)
                return;

            //if (GameInstance != null)
            //{
            //    ((Game.CGameInstance)this.GameInstance).OnWindowsSizeChanged((UInt32)window.MainCtrl.VP1.ActualWidth, (UInt32)window.MainCtrl.VP1.ActualHeight);
            //    //((Game.CGameInstance)this.GameInstance).OnWindowsSizeChanged((UInt32)window.DrawPanel.Width, (UInt32)window.DrawPanel.Height);
            //}
        }

        System.DateTime mLastTime;
        public void EditorTick()
        {
            this.ThreadEditor.Tick();

            var now = System.DateTime.Now;
            var delta = now - mLastTime;
            EditorCommon.TickManager.Instance.Tick((Int64)delta.TotalMilliseconds);
            mLastTime = now;
        }
    }
}
