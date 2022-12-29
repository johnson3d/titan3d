using EngineNS.Bricks.Input.Control;
using EngineNS.Bricks.Input.Device.Keyboard;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Editor
{
    public partial class UPIEModule : UModule<UEngine>
    {
        public override void Tick(UEngine engine)
        {
            if (engine.GameInstance == null)
                return;
            engine.GameInstance.Tick(engine.ElapseTickCount);
        }
        public override void Cleanup(UEngine engine)
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
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;

        RName.PGRNameAttribute mRNameEditor = new RName.PGRNameAttribute();
        RName mCurrentName;

        public UPIEController()
        {
            UEngine.RootFormManager.RegRootForm(this);
        }

        public void Cleanup()
        {
        }

        public async Task<bool> Initialize()
        {
            if (!await mRNameEditor.Initialize())
                return false;

            mCurrentName = UEngine.Instance.Config.PlayGameName;
            mRNameEditor.FilterExts = Bricks.CodeBuilder.UMacross.AssetExt;
            mRNameEditor.MacrossType = typeof(GamePlay.UMacrossGame);

            return true;
        }
        bool[] mToolBtn_IsMouseDown = new bool[4];
        bool[] mToolBtn_IsMouseHover = new bool[4];
        public void OnDraw()
        {
            if(ImGuiAPI.Begin("PIEController", ref mVisible, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                var drawList = ImGuiAPI.GetWindowDrawList();
                //EGui.UIProxy.Toolbar.BeginToolbar(drawList);

                //if (EGui.UIProxy.ToolbarIconButtonProxy.DrawButton(drawList, ref mToolBtn_IsMouseDown[1], ref mToolBtn_IsMouseHover[1], null, "Play"))
                //{
                //    var task = OnPlayGame(UEngine.Instance.Config.PlayGameName);
                //}
                //if (EGui.UIProxy.ToolbarIconButtonProxy.DrawButton(drawList, ref mToolBtn_IsMouseDown[2], ref mToolBtn_IsMouseHover[2], null, "Stop"))
                //{

                //}

                var info = new EGui.Controls.PropertyGrid.PGCustomValueEditorAttribute.EditorInfo()
                {
                    Name = mCurrentName.Name,
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
                    _ = OnPlayGame(mCurrentName);
                }
                if (ImGuiAPI.Button("Stop", sz))
                {

                }

                //EGui.UIProxy.Toolbar.EndToolbar();
            }
            ImGuiAPI.End();
        }

        async System.Threading.Tasks.Task OnPlayGame(RName assetName)
        {
            await UEngine.Instance.StartPlayInEditor(UEngine.Instance.GfxDevice.SlateApplication, assetName);
        }
    }
}

namespace EngineNS
{
    public partial class UEngine
    {

        public Editor.UPIEModule PIEModule { get; } = new Editor.UPIEModule();
        public const string DotNetVersion = "net6.0";
        public virtual async System.Threading.Tasks.Task<bool> StartPlayInEditor(USlateApplication application, RName main)
        {
            if (this.GameInstance != null)
                return false;

            var root = UEngine.Instance.FileManager.GetRoot(IO.FileManager.ERootDir.Execute);
            UEngine.Instance.MacrossModule.ReloadAssembly(root + $"/{DotNetVersion}/GameProject.dll");

            this.GameInstance = new GamePlay.UGameInstance();
            this.GameInstance.WorldViewportSlate.Title = $"Game:{main.Name}";

            this.GameInstance.McObject.Name = main;
            var ret = await this.GameInstance.BeginPlay();
            
            UEngine.Instance.TickableManager.AddTickable(this.GameInstance);

            UEngine.Instance.InputSystem.Mouse.ShowCursor = false;
            var esc = IControl.Create<UKey>(new UKey.UKeyData() { Keycode = Bricks.Input.Keycode.KEY_ESCAPE });
            esc.TriggerPress += (ITriggerControl sender)=>
                                {
                                    UEngine.Instance.InputSystem.Mouse.ShowCursor = true;
                                    EndPlayInEditor();
                                };

            var outOfMouse = IControl.Create<UKey>(new UKey.UKeyData() { Keycode = Bricks.Input.Keycode.KEY_F1 });
            outOfMouse.TriggerPress += (ITriggerControl sender) =>
            {
                UEngine.Instance.InputSystem.Mouse.ShowCursor = !UEngine.Instance.InputSystem.Mouse.ShowCursor;
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
                Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "Core", "EndPIE: GameInstance is alive");
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

            UEngine.Instance?.TickableManager.RemoveTickable(this.GameInstance);
            this.GameInstance.BeginDestroy();
            var wr = new WeakReference(this.GameInstance);
            this.GameInstance = null;

            return wr;
        }
    }
}