using EngineNS.Bricks.Input.Control;
using EngineNS.Bricks.Input.Device.Keyboard;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Editor
{
    public partial class TtPIEModule : TtModule<TtEngine>
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

    public class TtPIEController : IRootForm
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

        public TtPIEController()
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

            if (TtEngine.Instance.DynConfigData.TryGetConfig<RName>("LastPIEName", out var cfgName))
            {
                mCurrentName = cfgName;
            }
            else
            {
                mCurrentName = TtEngine.Instance.Config.PlayGameName;
            }
            mRNameEditor.FilterExts = Bricks.CodeBuilder.TtMacross.AssetExt;
            mRNameEditor.MacrossType = typeof(GamePlay.TtMacrossGame);

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

                var info = new EGui.Controls.PropertyGrid.PGCustomValueEditorAttribute.EditorInfo()
                {
                    Name = mCurrentName?.Name,
                    Value = mCurrentName,
                    Readonly = false,
                };

                object newValue;
                ImGuiAPI.SetNextItemWidth(200);
                mRNameEditor.OnDraw(info, out newValue);
                if (mCurrentName != (RName)newValue)
                {
                    mCurrentName = (RName)newValue;
                    TtEngine.Instance.DynConfigData.SetConfig("LastPIEName", mCurrentName);
                }

                Vector2 sz = new Vector2(-1, 40);
                if (ImGuiAPI.Button("Play", sz))
                {
                    OnPlayGame(mCurrentName);
                }
                if (ImGuiAPI.Button("Stop", sz))
                {

                }
                if (TtEngine.Instance.GameInstance!=null)
                {
                    for (int i = 0; i<TtEngine.Instance.MultiGameInstances.Length; i++)
                    {
                        if (TtEngine.Instance.MultiGameInstances[i]==null)
                        {
                            if (ImGuiAPI.Button("Run Multi Game", sz))
                            {
                                TtEngine.Instance.EventPoster.RunOn(async (state) =>
                                {
                                    TtEngine.Instance.PlayMode = EPlayMode.PlayerInEditor;
                                    var ret = await TtEngine.Instance.StartMultiPlayInEditor(TtEngine.Instance.GfxDevice.SlateApplication, mCurrentName, i);
                                    if (ret == false)
                                    {
                                        Profiler.Log.WriteLine<Profiler.TtGameplayGategory>(Profiler.ELogTag.Error, $"StartMultiPlayInEditor {mCurrentName}_{i} failed!");
                                    }
                                    return ret;
                                }, Thread.Async.EAsyncTarget.Logic);
                            }
                            break;
                        }
                    }
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
                TtEngine.Instance.PlayMode = EPlayMode.PlayerInEditor;
                var ret = await TtEngine.Instance.StartPlayInEditor(TtEngine.Instance.GfxDevice.SlateApplication, assetName);
                if (ret == false)
                {
                    TtEngine.Instance.EndPlayInEditor();
                    TtEngine.Instance.PlayMode = EPlayMode.Editor;
                }
                return ret;
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

        public Editor.TtPIEModule PIEModule { get; } = new Editor.TtPIEModule();
        public readonly static System.Version Version = System.Environment.Version;
        public static string DotNetVersion { get; private set; } = "?";
        public virtual async Thread.Async.TtTask<bool> StartPlayInEditor(TtSlateApplication application, RName main)
        {
            if (this.GameInstance != null)
                return false;

            var root = TtEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.Execute);
            TtEngine.Instance.MacrossModule.ReloadAssembly(root + $"/{DotNetVersion}/GameProject.dll", Config.IsTryUnloadMacrossAssembly);

            var gameInstance = new GamePlay.TtGameInstance();
            gameInstance.WorldViewportSlate.Title = $"Game:{main.Name}";
            gameInstance.WorldViewportSlate.MultiGameIndex = -1;

            gameInstance.McObject.Name = main;
            var ret = await gameInstance.BeginPlay();
            if (ret == false)
            {
                Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Error, $"{main} BeginPlay failed!");
                return false;
            }
            
            TtEngine.Instance.InputSystem.Mouse.ShowCursor = true;
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

            TtEngine.Instance.TickableManager.AddTickable(gameInstance);
            this.GameInstance = gameInstance;
            return ret;
        }
        public void EndPlayInEditor()
        {
            if (this.GameInstance == null)
                return;

            for (int i = 0; i<MultiGameInstances.Length; i++)
            {
                if (MultiGameInstances[i] == null)
                    continue;
                EndMultiPlayInEditor(i);
            }

            Thread.TtContextThread.CurrentContext.FlushAllThreadEvents();
            AwaitEndPlayInEditor().WaitCompletedAndDispose();
            for (int i = 0; i < 5; i++)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            //AwaitEndPlayInEditor().AddWaitTask((task)=>
            //{
            //    TtEngine.Instance.EventPoster.RunOn(static (state) =>
            //    {
            //        for (int i = 0; i < 5; i++)
            //        {
            //            GC.Collect();
            //            GC.WaitForPendingFinalizers();
            //        }
            //        return true;
            //    }, Thread.Async.EAsyncTarget.Main);
            //});
        }
        private async Thread.Async.TtTask AwaitEndPlayInEditor()
        {
            //等待 GameSemaphore, 确保BeginPlay已经结束
            await TtEngine.Instance.EventPoster.AwaitSemaphore(this.GameInstance.GameSemaphore);

            EndPlayInEditorImpl();
        }
        private void EndPlayInEditorImpl()
        {
            if (this.GameInstance == null)
                return;

            try
            {
                TtEngine.Instance?.TickableManager.RemoveTickable(this.GameInstance);
                this.GameInstance.BeginDestroy();
                var wr = new WeakReference(this.GameInstance);
                this.GameInstance.Dispose();
            }
            finally
            {
                this.GameInstance = null;

                TtEngine.Instance.PlayMode = EPlayMode.Editor;
            }
        }
        #region Multi Game Instance
        public virtual async Thread.Async.TtTask<bool> StartMultiPlayInEditor(TtSlateApplication application, RName main, int index)
        {
            if (this.GameInstance == null || index<0 ||index >=MultiGameInstances.Length)
                return false;

            if (MultiGameInstances[index] != null)
                return false;

            var gameInstance = new GamePlay.TtGameInstance();
            gameInstance.WorldViewportSlate.Title = $"Game:{main.Name}[{index}]";
            gameInstance.WorldViewportSlate.MultiGameIndex = index;

            gameInstance.McObject.Name = main;
            var ret = await gameInstance.BeginPlay();
            if (ret == false)
            {
                Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Error, $"{main}[{index}] BeginPlay failed!");
                return false;
            }

            TtEngine.Instance.InputSystem.Mouse.ShowCursor = true;
            var esc = IControl.Create<UKey>(new UKey.UKeyData() { Keycode = Bricks.Input.Keycode.KEY_ESCAPE });
            esc.TriggerPress += (ITriggerControl sender) =>
            {
                TtEngine.Instance.InputSystem.Mouse.ShowCursor = true;
                EndMultiPlayInEditor(index);
            };

            var outOfMouse = IControl.Create<UKey>(new UKey.UKeyData() { Keycode = Bricks.Input.Keycode.KEY_F1 });
            outOfMouse.TriggerPress += (ITriggerControl sender) =>
            {
                TtEngine.Instance.InputSystem.Mouse.ShowCursor = !TtEngine.Instance.InputSystem.Mouse.ShowCursor;
            };

            TtEngine.Instance.TickableManager.AddTickable(gameInstance);
            MultiGameInstances[index] = gameInstance;
            return true;
        }
        public void EndMultiPlayInEditor(int index)
        {
            if (this.GameInstance == null)
                return;

            if (index < 0 || index >= MultiGameInstances.Length)
                return;

            if (MultiGameInstances[index]== null)
                return;

            try
            {
                TtEngine.Instance?.TickableManager.RemoveTickable(MultiGameInstances[index]);
                MultiGameInstances[index].BeginDestroy();
                MultiGameInstances[index].Dispose();
                if (MultiGameInstances[index].WorldViewportSlate!=null)
                    MultiGameInstances[index].WorldViewportSlate.MultiGameIndex = -1;
            }
            finally
            {
                MultiGameInstances[index] = null;
            }
        }
        #endregion
    }
}