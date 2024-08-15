using EngineNS.Bricks.Input.Control;
using EngineNS.Bricks.Input.Device.Keyboard;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Editor
{
    public partial class UPIEModule : UModule<TtEngine>
    {
        public override void TickModule(TtEngine engine)
        {
            
        }
        public override void TickLogic(TtEngine engine)
        {
            if (engine.GameInstance == null)
                return;
            engine.GameInstance.Tick(engine.ElapseTickCountMS);
        }
        public override void Cleanup(TtEngine engine)
        {
            if (engine.GameInstance == null)
                return;

            engine.GameInstance.BeginDestroy();
            engine.GameInstance = null;
        }
    }

    public class UPIEController : IRootForm
    {
        bool mVisible = false;
        public bool Visible 
        {
            get => mVisible;
            set
            {
                mVisible = value;
            }
        }
        public uint DockId { get; set; }
        public ImGuiWindowClass DockKeyClass { get; }
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;

        RName.PGRNameAttribute mRNameEditor = new RName.PGRNameAttribute();
        RName mCurrentName;

        public UPIEController()
        {
            TtEngine.RootFormManager.RegRootForm(this);
        }

        public void Dispose()
        {
        }

        public async Thread.Async.TtTask<bool> Initialize()
        {
            if (!await mRNameEditor.Initialize())
                return false;

            mCurrentName = TtEngine.Instance.Config.PlayGameName;
            mRNameEditor.FilterExts = Bricks.CodeBuilder.UMacross.AssetExt;
            mRNameEditor.MacrossType = typeof(GamePlay.UMacrossGame);

            return true;
        }
        bool[] mToolBtn_IsMouseDown = new bool[4];
        bool[] mToolBtn_IsMouseHover = new bool[4];
        public void OnDraw()
        {
            var result = EGui.UIProxy.DockProxy.BeginMainForm("PIEController", this, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (result)
            {
                var drawList = ImGuiAPI.GetWindowDrawList();
                //EGui.UIProxy.Toolbar.BeginToolbar(drawList);

                //if (EGui.UIProxy.ToolbarIconButtonProxy.DrawButton(drawList, ref mToolBtn_IsMouseDown[1], ref mToolBtn_IsMouseHover[1], null, "Play"))
                //{
                //    var task = OnPlayGame(TtEngine.Instance.Config.PlayGameName);
                //}
                //if (EGui.UIProxy.ToolbarIconButtonProxy.DrawButton(drawList, ref mToolBtn_IsMouseDown[2], ref mToolBtn_IsMouseHover[2], null, "Stop"))
                //{

                //}

                var info = new EGui.Controls.PropertyGrid.PGCustomValueEditorAttribute.EditorInfo()
                {
                    Name = mCurrentName?.Name,
                    Value = mCurrentName,
                    Readonly = false,
                };

                object newValue;
                ImGuiAPI.SetNextItemWidth(200);
                mRNameEditor.OnDraw(info, out newValue);
                mCurrentName = (RName)newValue;

                Vector2 sz = new Vector2(-1, 40);
                if (ImGuiAPI.Button("Play", sz))
                {
                    OnPlayGame(mCurrentName);
                }
                if (ImGuiAPI.Button("Stop", sz))
                {

                }

                //EGui.UIProxy.Toolbar.EndToolbar();
            }
            EGui.UIProxy.DockProxy.EndMainForm(result);
        }

        void OnPlayGame(RName assetName)
        {
            if (TtEngine.Instance.GameInstance != null)
                return;
            TtEngine.Instance.EventPoster.RunOn(async (state) =>
            {
                return await TtEngine.Instance.StartPlayInEditor(TtEngine.Instance.GfxDevice.SlateApplication, assetName);
            }, Thread.Async.EAsyncTarget.Logic);
        }
    }
}

namespace EngineNS
{
    public partial class TtEngine
    {
        static TtEngine()
        {
            switch (Version.Major)
            {
                case 6:
                    DotNetVersion = "net6.0";
                    break;
                case 7:
                    DotNetVersion = "net7.0";
                    break;
                case 8:
                    DotNetVersion = "net8.0";
                    break;
            }
        }

        public Editor.UPIEModule PIEModule { get; } = new Editor.UPIEModule();
        public readonly static System.Version Version = System.Environment.Version;
        public static string DotNetVersion { get; private set; } = "net7.0";
        public virtual async System.Threading.Tasks.Task<bool> StartPlayInEditor(USlateApplication application, RName main)
        {
            if (this.GameInstance != null)
                return false;

            var root = TtEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.Execute);
            TtEngine.Instance.MacrossModule.ReloadAssembly(root + $"/{DotNetVersion}/GameProject.dll");

            this.GameInstance = new GamePlay.UGameInstance();
            this.GameInstance.WorldViewportSlate.Title = $"Game:{main.Name}";

            this.GameInstance.McObject.Name = main;
            var ret = await this.GameInstance.BeginPlay();
            if (ret == false)
            {
                Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Error, $"{main} BeginPlay failed!");
                this.GameInstance = null;
                return false;
            }
            
            TtEngine.Instance.TickableManager.AddTickable(this.GameInstance);

            TtEngine.Instance.InputSystem.Mouse.ShowCursor = false;
            var esc = IControl.Create<UKey>(new UKey.UKeyData() { Keycode = Bricks.Input.Keycode.KEY_ESCAPE });
            esc.TriggerPress += (ITriggerControl sender)=>
                                {
                                    TtEngine.Instance.InputSystem.Mouse.ShowCursor = true;
                                    EndPlayInEditor();
                                };

            var outOfMouse = IControl.Create<UKey>(new UKey.UKeyData() { Keycode = Bricks.Input.Keycode.KEY_F1 });
            outOfMouse.TriggerPress += (ITriggerControl sender) =>
            {
                TtEngine.Instance.InputSystem.Mouse.ShowCursor = !TtEngine.Instance.InputSystem.Mouse.ShowCursor;
            };

            return ret;
        }

        public void EndPlayInEditor()
        {
            var wr =  EndPlayInEditor_Impl();
            if (wr == null)
                return;
            for (int i = 0; wr.IsAlive && (i < 10); i++)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

            if (wr.IsAlive)
            {
                Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Warning, "EndPIE: GameInstance is alive");
            }
            else
            {
                for (int i = 0; i < 10; i++)
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
            }
        }
        private WeakReference EndPlayInEditor_Impl()
        {
            if (this.GameInstance == null)
                return null;

            TtEngine.Instance?.TickableManager.RemoveTickable(this.GameInstance);
            this.GameInstance.BeginDestroy();
            var wr = new WeakReference(this.GameInstance);
            this.GameInstance = null;

            return wr;
        }
    }
}