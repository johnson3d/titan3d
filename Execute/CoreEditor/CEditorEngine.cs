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
    class CEditorEngine : EngineNS.CEngine
    {
        public CEditorEngine()
        {
            Instance = this;
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
        public bool InitSystem(MainWindow window)
        {
            window.MainCtrl.PG.Instance = this;
            EngineNS.ERHIType rhiType = EngineNS.ERHIType.RHT_D3D11;//RHT_OGL;//
            EngineNS.CIPlatform.Instance.IsEditor = true;

            var ok = EngineNS.CEngine.Instance.GfxInitEngine(rhiType, 0, window.MainCtrl.VP1.DrawHandle);
            if (ok == false)
            {
                return false;
            }

            var rc = EngineNS.CEngine.Instance.RenderContext;

            var gameDesc = new EngineNS.Editor.CEditorInstanceDesc();
            gameDesc.SceneName = gameDesc.DefaultMapName;

            InitDefaultResource(gameDesc);

            var srv = CEngine.Instance.TextureManager.GetShaderRView(rc, RName.GetRName("Texture/noused.png"));

            ScopeTickRender.Enable = true;
            ScopeTickLogic.Enable = true;
            ScopeTickSync.Enable = true;

            window.MainCtrl.VP1.ViewPortName = "MainEditorView";

            if (true)
            {
                window.MainCtrl.VP1.OnWindowCreated = delegate (EditorCommon.ViewPort.ViewPortControl vp)
                {
                    if (EditorInited == true)
                        return;
                    EditorInited = true;
                    var editor = this.GameEditorInstance as CEditorInstance;
                    editor?.InitEditor(window, gameDesc);
                };

            }

            return true;
        }
        public override async System.Threading.Tasks.Task<bool> OnEngineInited()
        {
            await base.OnEngineInited();

            var desc = new SuperSocket.SocketServerBaseDesc();
            desc.MaxConnect = 16;
            await this.RemoteServices.InitServer(desc);
            await this.RemoteServices.InitClient("127.0.0.1", 2020);

            return true;
        }
        public void OnWindowsSizeChanged(MainWindow window)
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
