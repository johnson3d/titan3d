using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Reflection;

namespace AloneGame
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        public bool CreateDebugLayer = true;
        [DllImport("user32.dll", EntryPoint = "SendMessage")]
        private static extern int SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);
        private void Form1_Load(object sender, EventArgs e)
        {
            AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            System.Action action = async () =>
            {
#if FinalRelease
                var assm = EngineNS.Rtti.VAssemblyManager.Instance.LoadAssembly("Batman/Game.Windows.dll", EngineNS.ECSType.Client, false, false);
#else
                EngineNS.Rtti.VAssembly assm = null;
                var files = System.IO.Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "Game.Windows*.dll", System.IO.SearchOption.TopDirectoryOnly);
                if (files.Length > 0)
                {
                    assm = EngineNS.Rtti.VAssemblyManager.Instance.LoadAssembly(files[0], EngineNS.ECSType.Client, false, false);
                }
                else
                    return;
#endif
                EngineNS.Rtti.RttiHelper.RegisterAnalyseAssembly("Game", assm);

                var types = assm.GetTypes();

                System.Type GameInstanceType = null;
                foreach (var i in types)
                {
                    if(i.FullName == "Game.CGameEngine")
                    {
                        EngineNS.CEngine.Instance = System.Activator.CreateInstance(i) as EngineNS.CEngine;
                    }
                    if (i.FullName == "Game.CGameInstance")
                        GameInstanceType = i;
                }
                //EngineNS.CEngine.Instance = new Game.CGameEngine();
                EngineNS.CEngine.mGenerateShaderForMobilePlatform = true;
                EngineNS.CEngine.Instance.PreInitEngine();

                var pak = new EngineNS.IO.CPakFile();
                var mountPoint = EngineNS.CEngine.Instance.FileManager.ProjectRoot;

                var pakFile = mountPoint + "../a.tpak";
                if(pak.LoadPak(pakFile))
                {
                    pak.MountPak(mountPoint, 100);
                }

                EngineNS.CEngine.Instance.InitEngine("Game", null);
                EngineNS.CEngine.Instance.Interval = 20;

                EngineNS.CIPlatform.Instance.GameWindowHwnd = this.Handle;
                Application.Idle += (object sender1, EventArgs e1) =>
                {
                    EngineNS.CEngine.Instance.MainTick();
                };

                this.ClientSizeChanged += (object sender2, EventArgs e2) =>
                {
                    EngineNS.CEngine.Instance.GameInstance?.OnWindowsSizeChanged((UInt32)this.ClientSize.Width, (UInt32)this.ClientSize.Height);
                };

                var rhiType = EngineNS.ERHIType.RHT_OGL;
                if (EngineNS.IO.FileManager.UseCooked == "windows")
                    rhiType = EngineNS.ERHIType.RHT_D3D11;
                await EngineNS.CEngine.Instance.InitSystem(this.Handle, (UInt32)this.Width, (UInt32)this.Height, rhiType, CreateDebugLayer);

                MobileGPUSimulator.Instance.SimulateGPU(MobileGPUSimulator.Instance.Gpu);
                //EngineNS.CEngine.Instance.MetaClassManager.CheckNewMetaClass();

                var desc = new EngineNS.GamePlay.GGameInstanceDesc();
                desc.SceneName = desc.DefaultMapName;
                await EngineNS.CEngine.Instance.OnEngineInited();

                EngineNS.CEngine.Instance.McEngineGetter?.Get()?.OnStartGame(EngineNS.CEngine.Instance);
                await EngineNS.CEngine.Instance.StartGame(GameInstanceType, this.Handle,
                    (UInt32)this.ClientSize.Width, (UInt32)this.ClientSize.Height, desc, null,
                    EngineNS.CEngine.Instance.Desc.GameMacross);

                this.Text = $"TitanGame({rhiType})";
            };
            action();
        }

        Dictionary<string, Assembly> mAssemblies = new Dictionary<string, Assembly>();
        private System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            Assembly assembly;
            mAssemblies.TryGetValue(args.Name, out assembly);
            return assembly;
        }

        private void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            var assembly = args.LoadedAssembly;
            mAssemblies[assembly.FullName] = assembly;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            //SendMessage(this.Handle, (int)EngineNS.CIPlatform.WindowMessage.WM_QUIT, 0, 0);
            EngineNS.CIPlatform.Instance.GameWindowHwnd = IntPtr.Zero;
            EngineNS.CIPlatform.Instance.RunWinForm = false;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (EngineNS.CEngine.Instance.GameInstance == null)
                return;
            e.Cancel = true;
            System.Action action = async () =>
            {
                await EngineNS.CEngine.Instance.StopGame();
                EngineNS.CEngine.Instance.Cleanup();
                this.Close();
            };
            action();
        }
        int PrevMouseX;
        int PrevMouseY;
        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            //ResourceLibrary.Win32.POINT point = new ResourceLibrary.Win32.POINT();
            //ResourceLibrary.Win32.GetCursorPos(ref point);
            //var mousePoint = mGameForm.PointToClient(new System.Drawing.Point(point.X, point.Y));
            var mouseEventArgs = new EngineNS.Input.Device.Mouse.MouseInputEventArgs();
            mouseEventArgs.DeviceType = EngineNS.Input.Device.DeviceType.Mouse;
            var mouseEvent = new EngineNS.Input.Device.Mouse.MouseEventArgs();
            mouseEvent.Button = (EngineNS.Input.Device.Mouse.MouseButtons)e.Button;
            mouseEvent.State = EngineNS.Input.Device.Mouse.ButtonState.Move;
            mouseEvent.X = e.X;
            mouseEvent.Y = e.Y;
            mouseEvent.OffsetX = e.X - PrevMouseX;
            mouseEvent.OffsetY = e.Y - PrevMouseY;
            PrevMouseX = e.X;
            PrevMouseY = e.Y;
            mouseEvent.Delta = e.Delta;
            mouseEventArgs.MouseEvent = mouseEvent;
            EngineNS.CEngine.Instance?.InputServerInstance?.OnInputEvnet(mouseEventArgs);
        }
        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            var mouseEventArgs = new EngineNS.Input.Device.Mouse.MouseInputEventArgs();
            mouseEventArgs.DeviceType = EngineNS.Input.Device.DeviceType.Mouse;
            var mouseEvent = new EngineNS.Input.Device.Mouse.MouseEventArgs();
            mouseEvent.Button = (EngineNS.Input.Device.Mouse.MouseButtons)e.Button;
            mouseEvent.State = EngineNS.Input.Device.Mouse.ButtonState.Down;
            mouseEvent.X = e.X;
            mouseEvent.Y = e.Y;
            mouseEvent.OffsetX = e.X - PrevMouseX;
            mouseEvent.OffsetY = e.Y - PrevMouseY;
            PrevMouseX = e.X;
            PrevMouseY = e.Y;
            mouseEvent.Delta = e.Delta;
            mouseEventArgs.MouseEvent = mouseEvent;
            EngineNS.CEngine.Instance.InputServerInstance?.OnInputEvnet(mouseEventArgs);
        }
        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            var mouseEventArgs = new EngineNS.Input.Device.Mouse.MouseInputEventArgs();
            mouseEventArgs.DeviceType = EngineNS.Input.Device.DeviceType.Mouse;
            var mouseEvent = new EngineNS.Input.Device.Mouse.MouseEventArgs();
            mouseEvent.Button = (EngineNS.Input.Device.Mouse.MouseButtons)e.Button;
            mouseEvent.State = EngineNS.Input.Device.Mouse.ButtonState.Up;
            mouseEvent.X = e.X;
            mouseEvent.Y = e.Y;
            mouseEvent.OffsetX = e.X - PrevMouseX;
            mouseEvent.OffsetY = e.Y - PrevMouseY;
            PrevMouseX = e.X;
            PrevMouseY = e.Y;
            mouseEvent.Delta = e.Delta;
            mouseEventArgs.MouseEvent = mouseEvent;
            EngineNS.CEngine.Instance.InputServerInstance?.OnInputEvnet(mouseEventArgs);
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            var noUsed = OnResize();
        }
        protected override bool ProcessKeyMessage(ref Message m)
        {
            var message = new EngineNS.Input.Device.Keyboard.WindowsMessage();
            message.HWnd = m.HWnd;
            message.Msg = m.Msg;
            message.WParam = m.WParam;
            message.LParam = m.LParam;
            message.Result = m.Result;
            var keyboardE = EngineNS.Input.Device.Keyboard.ProcessWindowsKeyMessage(ref message);
            KeyEventArgs e = null;
            if(keyboardE.KeyCode != EngineNS.Input.Device.Keyboard.Keys.None)
            {
                e = new KeyEventArgs((Keys)keyboardE.KeyCode);
                if (e.KeyData != Keys.None)
                {
                    if (m.Msg == EngineNS.Input.Device.Keyboard.WM_KEYDOWN)
                        OnKeyDown(e);
                    else
                        OnKeyUp(e);
                    return true;
                }
            }
            return base.ProcessKeyMessage(ref m);
        }
        private async System.Threading.Tasks.Task OnResize()
        {
            if (EngineNS.CEngine.Instance == null)
                return;
            await EngineNS.CEngine.Instance.AwaitEngineInited();
            if (EngineNS.CEngine.Instance.GameInstance != null)
            {
                EngineNS.CEngine.Instance.GameInstance.OnWindowsSizeChanged((UInt32)this.ClientSize.Width, (UInt32)this.ClientSize.Height);
                var policy = EngineNS.CEngine.Instance.GameInstance.RenderPolicy as EngineNS.Graphics.RenderPolicy.CGfxRP_GameMobile;
                if (policy != null)
                {
                    policy.OnResize(EngineNS.CEngine.Instance.RenderContext, policy.SwapChain, (UInt32)this.Width, (UInt32)this.Height);
                }
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
            EngineNS.CEngine.Instance.InputServerInstance?.OnInputEvnet(kea);
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
            EngineNS.CEngine.Instance?.InputServerInstance?.OnInputEvnet(kea);

            if(e.Shift && e.Alt && e.Control && e.KeyCode == Keys.P)
            {
                EngineNS.CEngine.Instance.OnPause();
                EngineNS.CEngine.Instance.OnResume(this.Handle);
                //EngineNS.CEngine.Instance.GameInstance?.OnPause();
            }
        }

        private void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {

        }
    }
}
