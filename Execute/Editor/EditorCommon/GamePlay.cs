using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EditorCommon
{
    public class GamePlay : INotifyPropertyChanged, EngineNS.Editor.IEditorInstanceObject
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion
        public static GamePlay Instance
        {
            get
            {
                var name = typeof(GamePlay).FullName;
                if (EngineNS.CEngine.Instance.GameEditorInstance[name] == null)
                    EngineNS.CEngine.Instance.GameEditorInstance[name] = new GamePlay();
                return EngineNS.CEngine.Instance.GameEditorInstance[name];
            }
        }

        private GamePlay()
        {

        }
        public void FinalCleanup()
        {

        }

        public string GameDllAbsFileName;
        GameForm mGameForm;
        public Type GameInstanceType
        {
            get
            {
                //var assm = EngineNS.Rtti.RttiHelper.GetAssemblyFromDllFileName(EngineNS.ECSType.Client, GameDllAbsFileName, "", true, false);
                var assm = EngineNS.Rtti.RttiHelper.GetAssemblyFromDllFileName(EngineNS.ECSType.Client, GameDllAbsFileName);
                return assm.Assembly.GetType("Game.CGameInstance");
            }
        }
        //internal List<EditorCommon.Controls.Debugger.PlayAndDebugToolbar> PADToolbars = new List<Controls.Debugger.PlayAndDebugToolbar>();

        public bool IsInPIEMode
        {
            get => mGameForm != null;
            set
            {
                if (IsInPIEMode)
                {
                    EngineNS.CEngine.Instance.GameEditorInstance.EnableTick = false;
                }
                else
                {
                    EngineNS.CEngine.Instance.GameEditorInstance.EnableTick = true;
                }
                //foreach(var toolbar in PADToolbars)
                //{
                //    toolbar.IsInPIEMode = value;
                //}
                OnPropertyChanged("IsInPIEMode");
            }
        }
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            EngineNS.Input.Device.Keyboard.KeyboardInputEventArgs kea = new EngineNS.Input.Device.Keyboard.KeyboardInputEventArgs();
            kea.DeviceType = EngineNS.Input.Device.DeviceType.Keyboard;
            EngineNS.Input.Device.Keyboard.KeyboardEventArgs ke = new EngineNS.Input.Device.Keyboard.KeyboardEventArgs();
            ke.KeyState = EngineNS.Input.Device.Keyboard.KeyState.Press;
            ke.Alt = e.Alt;
            ke.Control = e.Control;
            ke.Shift = e.Shift;
            ke.KeyCode = (EngineNS.Input.Device.Keyboard.Keys)e.KeyCode;
            ke.KeyValue = e.KeyValue;
            kea.KeyboardEvent = ke;
            EngineNS.CEngine.Instance.InputServerInstance.OnInputEvnet(kea);
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            EngineNS.Input.Device.Keyboard.KeyboardInputEventArgs kea = new EngineNS.Input.Device.Keyboard.KeyboardInputEventArgs();
            kea.DeviceType = EngineNS.Input.Device.DeviceType.Keyboard;
            EngineNS.Input.Device.Keyboard.KeyboardEventArgs ke = new EngineNS.Input.Device.Keyboard.KeyboardEventArgs();
            ke.KeyState = EngineNS.Input.Device.Keyboard.KeyState.Release;
            ke.Alt = e.Alt;
            ke.Control = e.Control;
            ke.Shift = e.Shift;
            ke.KeyCode = (EngineNS.Input.Device.Keyboard.Keys)e.KeyCode;
            ke.KeyValue = e.KeyValue;
            kea.KeyboardEvent = ke;
            EngineNS.CEngine.Instance.InputServerInstance.OnInputEvnet(kea);
        }
        public async Task ShowPlayInWindow()
        {

            if (mGameForm != null)
                return;
            if (GameInstanceType == null)
                throw new InvalidOperationException("GameInstanceType没有设置!");

            var screenPos = new ResourceLibrary.Win32.POINT();
            ResourceLibrary.Win32.GetCursorPos(ref screenPos);
            var currentScreen = System.Windows.Forms.Screen.FromPoint(new System.Drawing.Point(screenPos.X, screenPos.Y));

            // 保存当前场景，用于PIE使用
            if (EngineNS.CEngine.Instance.GameEditorInstance.World.DefaultScene != null)
            {
                var xnd = EngineNS.IO.XndHolder.NewXNDHolder();
                await EngineNS.CEngine.Instance.GameEditorInstance.World.DefaultScene.Save2Xnd(xnd.Node);
                var dir = EngineNS.CEngine.Instance.FileManager.EditorContent + "tempscene/";
                if (!EngineNS.CEngine.Instance.FileManager.DirectoryExists(dir))
                    EngineNS.CEngine.Instance.FileManager.CreateDirectory(dir);
                EngineNS.IO.XndHolder.SaveXND(dir + "scene.map", xnd);
                // 保存当前场景的寻路信息
                var NavMesh = EngineNS.CEngine.Instance.GameEditorInstance.World.DefaultScene.NavMesh;
                if (NavMesh != null)
                    NavMesh.SaveNavMesh(dir + "/navmesh.dat");
            }

            mGameForm = new GameForm();
            EngineNS.CIPlatform.Instance.GameWindowHwnd = mGameForm.Handle;
            mGameForm.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            mGameForm.Width = (int)(currentScreen.WorkingArea.Width * 0.7f);
            mGameForm.Height = (int)(currentScreen.WorkingArea.Height * 0.7f);
            mGameForm.Left = (int)((currentScreen.WorkingArea.Width - mGameForm.Width) * 0.5f) + currentScreen.WorkingArea.Left;
            mGameForm.Top = (int)((currentScreen.WorkingArea.Height - mGameForm.Height) * 0.5f) + currentScreen.WorkingArea.Top;
            mGameForm.Text = "PIEWindow";
            mGameForm.Load += (object loadSender, EventArgs loadE) =>
            {
                var desc = new EngineNS.GamePlay.GGameInstanceDesc();
                desc.SceneName = desc.DefaultMapName;
                Action action = async () =>
                {
                    EngineNS.CEngine.Instance.McEngineGetter?.Get(false)?.OnStartGame(EngineNS.CEngine.Instance);
                    EngineNS.CIPlatform.Instance.PlayMode = EngineNS.CIPlatform.enPlayMode.PlayerInEditor;
                    await EngineNS.CEngine.Instance.StartGame(GameInstanceType, mGameForm.Handle,
                        (UInt32)mGameForm.ClientSize.Width, (UInt32)mGameForm.ClientSize.Height, desc, null,
                        EngineNS.CEngine.Instance.Desc.GameMacross); //EngineNS.RName.GetRName("Macross/mygame.macross"));
                };
                EngineNS.Editor.Runner.RunnerManager.Instance.ActiveAllEnabledBreaks();
                action();
            };
            mGameForm.Show();
            mGameForm.KeyDown += new System.Windows.Forms.KeyEventHandler(Form1_KeyDown);
            mGameForm.KeyUp += new System.Windows.Forms.KeyEventHandler(Form1_KeyUp);


            // 通知各编辑器绑定对象进入PIE模式
            IsInPIEMode = true;
            // 激活所有断点
            mGameForm.FormClosed += async (object closeSender, System.Windows.Forms.FormClosedEventArgs closeE) =>
            {
                EngineNS.Editor.Runner.RunnerManager.Instance.DeactiveAllEnabledBreaks();
                IsInPIEMode = false;
                EngineNS.Editor.Runner.RunnerManager.Instance.Resume();

                await EngineNS.CEngine.Instance.StopGame();
                EngineNS.CEngine.Instance.EngineTimeScale = 1.0f;
                EngineNS.CIPlatform.Instance.GameWindowHwnd = IntPtr.Zero;
                mGameForm = null;
                EngineNS.CIPlatform.Instance.PlayMode = EngineNS.CIPlatform.enPlayMode.Editor;
                IsInPIEMode = false;
            };
            unsafe
            {
                mGameForm.SizeChanged += (object sizeChangedSender, EventArgs sizeChangedE) =>
                {
                    var clientSize = mGameForm.ClientSize;
                    if (clientSize.Width == 0 || clientSize.Height == 0)
                        return;
                    if (EngineNS.CEngine.Instance.GameInstance != null)
                    {
                        EngineNS.CEngine.Instance.GameInstance.OnWindowsSizeChanged((UInt32)clientSize.Width, (UInt32)clientSize.Height);
                    }
                };
                mGameForm.MouseDown += (object mouseSender, System.Windows.Forms.MouseEventArgs mouseE) =>
                {
                    var mouseEventArgs = new EngineNS.Input.Device.Mouse.MouseInputEventArgs();
                    mouseEventArgs.DeviceType = EngineNS.Input.Device.DeviceType.Mouse;
                    var mouseEvent = new EngineNS.Input.Device.Mouse.MouseEventArgs();
                    mouseEvent.Button = (EngineNS.Input.Device.Mouse.MouseButtons)mouseE.Button;
                    mouseEvent.State = EngineNS.Input.Device.Mouse.ButtonState.Down;
                    mouseEvent.X = mouseE.X;
                    mouseEvent.Y = mouseE.Y;
                    mouseEvent.OffsetX = mouseE.X - mGameForm.PrevMouseX;
                    mouseEvent.OffsetY = mouseE.Y - mGameForm.PrevMouseY;
                    mGameForm.PrevMouseX = mouseE.X;
                    mGameForm.PrevMouseY = mouseE.Y;
                    mouseEvent.Delta = mouseE.Delta;
                    mouseEventArgs.MouseEvent = mouseEvent;
                    EngineNS.CEngine.Instance.InputServerInstance.OnInputEvnet(mouseEventArgs);
                    return;
                    //if (mouseE.Button == System.Windows.Forms.MouseButtons.Left)
                    //{
                    //    var ae = new EngineNS.Input.ActionEvent();
                    //    ae.ViewWidth = mGameForm.Width;
                    //    ae.ViewHeight = mGameForm.Height;
                    //    ae.X = mouseE.X;
                    //    ae.Y = mouseE.Y;
                    //    ae.PointerNumber = 1;
                    //    ae.Flags = (uint)EngineNS.Input.MouseEventArgs.MouseButtons.Left;
                    //    ae.MotionType = EngineNS.Input.EMotionType.Down;
                    //    EngineNS.CEngine.Instance.GameInstance.OnActionEvent(ref ae);
                    //}
                    //else if (mouseE.Button == System.Windows.Forms.MouseButtons.Middle)
                    //{
                    //    var ae = new EngineNS.Input.ActionEvent();
                    //    ae.ViewWidth = mGameForm.Width;
                    //    ae.ViewHeight = mGameForm.Height;
                    //    ae.X = mouseE.X;
                    //    ae.Y = mouseE.Y;
                    //    ae.PointerNumber = 3;
                    //    for (int i = 0; i < ae.PointerNumber; i++)
                    //    {
                    //        ae.PointX[i] = mouseE.X;
                    //        ae.PointY[i] = mouseE.Y;
                    //    }
                    //    ae.Flags = (uint)EngineNS.Input.MouseEventArgs.MouseButtons.Middle;
                    //    ae.MotionType = EngineNS.Input.EMotionType.Down;
                    //    EngineNS.CEngine.Instance.GameInstance.OnActionEvent(ref ae);
                    //}
                    //else if (mouseE.Button == System.Windows.Forms.MouseButtons.Right)
                    //{
                    //    var ae = new EngineNS.Input.ActionEvent();
                    //    ae.ViewWidth = mGameForm.Width;
                    //    ae.ViewHeight = mGameForm.Height;
                    //    ae.X = mouseE.X;
                    //    ae.Y = mouseE.Y;
                    //    ae.PointerNumber = 2;
                    //    ae.PointX[0] = mouseE.X;
                    //    ae.PointY[0] = mouseE.Y;
                    //    ae.Flags = (uint)EngineNS.Input.MouseEventArgs.MouseButtons.Right;
                    //    ae.MotionType = EngineNS.Input.EMotionType.Down;
                    //    EngineNS.CEngine.Instance.GameInstance.OnActionEvent(ref ae);
                    //}
                };
                mGameForm.MouseUp += (object mouseSender, System.Windows.Forms.MouseEventArgs mouseE) =>
                {
                    var mouseEventArgs = new EngineNS.Input.Device.Mouse.MouseInputEventArgs();
                    mouseEventArgs.DeviceType = EngineNS.Input.Device.DeviceType.Mouse;
                    var mouseEvent = new EngineNS.Input.Device.Mouse.MouseEventArgs();
                    mouseEvent.Button = (EngineNS.Input.Device.Mouse.MouseButtons)mouseE.Button;
                    mouseEvent.State = EngineNS.Input.Device.Mouse.ButtonState.Up;
                    mouseEvent.X = mouseE.X;
                    mouseEvent.Y = mouseE.Y;
                    mouseEvent.OffsetX = mouseE.X - mGameForm.PrevMouseX;
                    mouseEvent.OffsetY = mouseE.Y - mGameForm.PrevMouseY;
                    mGameForm.PrevMouseX = mouseE.X;
                    mGameForm.PrevMouseY = mouseE.Y;
                    mouseEvent.Delta = mouseE.Delta;
                    mouseEventArgs.MouseEvent = mouseEvent;
                    EngineNS.CEngine.Instance.InputServerInstance.OnInputEvnet(mouseEventArgs);
                };
                //mGameForm.MouseMove += MGameForm_MouseMove;
                mGameForm.MouseMove += (object mouseSender, System.Windows.Forms.MouseEventArgs mouseE) =>
                {
                    ResourceLibrary.Win32.POINT point = new ResourceLibrary.Win32.POINT();
                    ResourceLibrary.Win32.GetCursorPos(ref point);
                    var mousePoint = mGameForm.PointToClient(new System.Drawing.Point(point.X, point.Y));
                    var mouseEventArgs = new EngineNS.Input.Device.Mouse.MouseInputEventArgs();
                    mouseEventArgs.DeviceType = EngineNS.Input.Device.DeviceType.Mouse;
                    var mouseEvent = new EngineNS.Input.Device.Mouse.MouseEventArgs();
                    mouseEvent.Button = (EngineNS.Input.Device.Mouse.MouseButtons)mouseE.Button;
                    mouseEvent.State = EngineNS.Input.Device.Mouse.ButtonState.Move;
                    mouseEvent.X = mousePoint.X;
                    mouseEvent.Y = mousePoint.Y;
                    mouseEvent.OffsetX = mouseE.X - mGameForm.PrevMouseX;
                    mouseEvent.OffsetY = mouseE.Y - mGameForm.PrevMouseY;
                    mGameForm.PrevMouseX = mouseE.X;
                    mGameForm.PrevMouseY = mouseE.Y;
                    mouseEvent.Delta = mouseE.Delta;
                    mouseEventArgs.MouseEvent = mouseEvent;
                    EngineNS.CEngine.Instance.InputServerInstance.OnInputEvnet(mouseEventArgs);
                };
                mGameForm.MouseWheel += (object mouseSender, System.Windows.Forms.MouseEventArgs mouseE) =>
                {
                    var mouseEventArgs = new EngineNS.Input.Device.Mouse.MouseInputEventArgs();
                    mouseEventArgs.DeviceType = EngineNS.Input.Device.DeviceType.Mouse;
                    var mouseEvent = new EngineNS.Input.Device.Mouse.MouseEventArgs();
                    mouseEvent.Button = (EngineNS.Input.Device.Mouse.MouseButtons)mouseE.Button;
                    mouseEvent.State = EngineNS.Input.Device.Mouse.ButtonState.WheelScroll;
                    mouseEvent.X = mouseE.X;
                    mouseEvent.Y = mouseE.Y;
                    mouseEvent.Delta = mouseE.Delta;
                    mouseEventArgs.MouseEvent = mouseEvent;
                    EngineNS.CEngine.Instance.InputServerInstance.OnInputEvnet(mouseEventArgs);
                };
            }

        }

        public void StopPlayInWindow()
        {
            mGameForm?.Close();
        }
    }
}
