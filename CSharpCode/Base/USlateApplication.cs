using EngineNS.Graphics.Pipeline;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS
{
    public interface IGuiModule
    {
        void OnDraw();
    }
    public interface IRootForm : IGuiModule, IDisposable
    {
        bool Visible { get; set; }
        uint DockId { get; set; }
        ImGuiWindowClass DockKeyClass { get; }
        ImGuiCond_ DockCond { get; set; }
        Task<bool> Initialize();
    }
    public class URootFormManager
    {
        internal static URootFormManager Insance = new URootFormManager();
        private URootFormManager()
        {

        }
        #region RootForms
        private List<WeakReference<IRootForm>> AppendForms { get; } = new List<WeakReference<IRootForm>>();
        private List<WeakReference<IRootForm>> RootForms { get; } = new List<WeakReference<IRootForm>>();
        public void RegRootForm(IRootForm form)
        {
            foreach (var i in AppendForms)
            {
                IRootForm rf;
                if (i.TryGetTarget(out rf))
                {
                    if (rf == form)
                        return;
                }
            }
            foreach (var i in RootForms)
            {
                IRootForm rf;
                if (i.TryGetTarget(out rf))
                {
                    if (rf == form)
                        return;
                }
            }
            AppendForms.Add(new WeakReference<IRootForm>(form));
        }
        public void UnregRootForm(IRootForm form)
        {
            foreach (var i in AppendForms)
            {
                IRootForm rf;
                if (i.TryGetTarget(out rf))
                {
                    if (rf == form)
                    {
                        AppendForms.Remove(i);
                        break;
                    }
                }
            }
            foreach (var i in RootForms)
            {
                IRootForm rf;
                if (i.TryGetTarget(out rf))
                {
                    if (rf == form)
                    {
                        AppendForms.Remove(i);
                        break;
                    }
                }
            }
        }
        public void DrawRootForms()
        {
            if (AppendForms.Count > 0)
            {
                RootForms.AddRange(AppendForms);
                AppendForms.Clear();
            }

            for (int i = 0; i < RootForms.Count; i++)
            {
                IRootForm rf;
                if (RootForms[i].TryGetTarget(out rf))
                {
                    if (rf.Visible == false)
                        continue;
                    rf.OnDraw();
                }
                else
                {
                    RootForms.RemoveAt(i);
                    i--;
                }
            }
        }
        public void ClearRootForms()
        {
            for (int i = 0; i < AppendForms.Count; i++)
            {
                IRootForm rf;
                if (AppendForms[i].TryGetTarget(out rf))
                {
                    rf.Dispose();
                }
            }
            AppendForms.Clear();
            for (int i = 0; i < RootForms.Count; i++)
            {
                IRootForm rf;
                if (RootForms[i].TryGetTarget(out rf))
                {
                    rf.Dispose();
                }
            }
            RootForms.Clear();
        }
        #endregion
    }

    public partial class USlateApplication
    {
        public UPresentWindow NativeWindow;
        //public virtual EGui.Slate.UWorldViewportSlate GetWorldViewportSlate() { return null; }

        public IntPtr mImGuiContext;
        public EGui.UImDrawDataRHI mDrawData = new EGui.UImDrawDataRHI();

        bool[] MousePressed = new bool[3] { false, false, false };
        IntPtr[] MouseCursors = new IntPtr[(int)ImGuiMouseCursor_.ImGuiMouseCursor_COUNT];
        partial void LoadMouseCursors();
        partial void FreeMouseCursors();
        partial void MapKeys(ImGuiIO io);
        partial void ImGui_Init(ImGuiIO io, IntPtr window);
        partial void ImGui_UpdateMousePosAndButtons(ImGuiIO io);
        partial void ImGui_UpdateMouseCursor(ImGuiIO io);

        public virtual async Task<bool> InitializeApplication(NxRHI.UGpuDevice rc, RName rpName)
        {
            await Thread.TtAsyncDummyClass.DummyFunc();

            NativeWindow.InitSwapChain(rc);
            unsafe
            {
                mDrawData.InitializeGraphics(NativeWindow.GetSwapchainFormat(), NativeWindow.GetSwapchainDSFormat());

                mImGuiContext = (IntPtr)ImGuiAPI.CreateContext(new ImFontAtlas((void*)0));
                ImGuiAPI.SetCurrentContext(mImGuiContext.ToPointer());
                UEngine.Instance.GfxDevice.SlateRenderer.RecreateFontDeviceTexture();

                var io = ImGuiAPI.GetIO();

                ImGuiConfigFlags_ configFlags = ImGuiConfigFlags_.ImGuiConfigFlags_None;
                configFlags |= ImGuiConfigFlags_.ImGuiConfigFlags_NavEnableKeyboard;       // Enable Keyboard Controls
                //configFlags |= ImGuiConfigFlags_NavEnableGamepad;      // Enable Gamepad Controls
                configFlags |= ImGuiConfigFlags_.ImGuiConfigFlags_DockingEnable;           // Enable Docking
                if (UEngine.Instance.Config.SupportMultWindows)
                    configFlags |= ImGuiConfigFlags_.ImGuiConfigFlags_ViewportsEnable;         // Enable Multi-Viewport / Platform Windows
                //io.ConfigViewportsNoAutoMerge = true;
                //io.ConfigViewportsNoTaskBarIcon = true;
                io.ConfigFlags = configFlags;

                ImGuiAPI.StyleColorsDark((ImGuiStyle*)0);

                var style = ImGuiAPI.GetStyle();
                if ((io.ConfigFlags & ImGuiConfigFlags_.ImGuiConfigFlags_ViewportsEnable) != 0)
                {
                    style->WindowRounding = 0.0f;
                    style->Colors[(int)ImGuiCol_.ImGuiCol_WindowBg].W = 1.0f;
                }

                ImGui_Init(ImGuiAPI.GetIO(), NativeWindow.Window);

                SetPerFrameImGuiData(1f / 60f);
            }
            return true;
        }
        public virtual void Cleanup()
        {
            mDrawData.Dispose();

            FreeMouseCursors();

            MouseCursors = new IntPtr[(int)ImGuiMouseCursor_.ImGuiMouseCursor_COUNT];

            NativeWindow?.Cleanup();
            NativeWindow = null;
            //unsafe
            //{
            //    ImGuiViewport* main_viewport = ImGuiAPI.GetMainViewport();
            //    if ((IntPtr)main_viewport->m_PlatformUserData != IntPtr.Zero)
            //    {
            //        var gcHandle = System.Runtime.InteropServices.GCHandle.FromIntPtr((IntPtr)main_viewport->PlatformUserData);
            //        System.Diagnostics.Debug.Assert(gcHandle.Target == this);

            //        main_viewport->PlatformUserData = IntPtr.Zero.ToPointer();
            //        main_viewport->PlatformHandle = IntPtr.Zero.ToPointer();

            //        gcHandle.Free();
            //    }
            //}
        }

        #region ImGUI & SDL

        bool g_MouseCanUseGlobalState = false;


        public virtual void OnResize(float x, float y)
        {
            NativeWindow.OnResize(x, y);
        }
        private void SetPerFrameImGuiData(float deltaSeconds)
        {
            var io = ImGuiAPI.GetIO();

            var sz = NativeWindow.GetWindowSize();
            int w = (int)sz.X;
            int h = (int)sz.Y;

            if (NativeWindow.IsMinimized)
            {
                w = h = 0;
            }

            io.DisplaySize = new Vector2((float)w, (float)h);
            if (w > 0 && h > 0)
            {
                io.DisplayFramebufferScale = new Vector2(1, 1);
            }
            else
            {
                io.DisplayFramebufferScale = new Vector2(1, 1);
            }
            io.DeltaTime = deltaSeconds; // DeltaTime is in seconds.
        }
        private void Update(float deltaSeconds)
        {
            SetPerFrameImGuiData(deltaSeconds);

            ImGui_UpdateMousePosAndButtons(ImGuiAPI.GetIO());
            ImGui_UpdateMouseCursor(ImGuiAPI.GetIO());

            ImGuiAPI.NewFrame();
        }
        #endregion

        [ThreadStatic]
        private static Profiler.TimeScope ScopeOnDrawUI = Profiler.TimeScopeManager.GetTimeScope(typeof(USlateApplication), nameof(OnDrawUI));
        [ThreadStatic]
        private static Profiler.TimeScope ScopeImGuiRender = Profiler.TimeScopeManager.GetTimeScope(typeof(ImGuiAPI), "Render");
        public unsafe virtual void OnDrawSlate()
        {
            if (mImGuiContext == IntPtr.Zero)
                return;

            using (new Profiler.TimeScopeHelper(ScopeOnDrawUI))
            {
                ImGuiAPI.SetCurrentContext(mImGuiContext.ToPointer());

                Update((UEngine.Instance.ElapseTickCountMS) * 0.001f);
                OnDrawUI();
            }

            UEngine.Instance.InputSystem.ClearFilesDrop();
            ImGuiAPI.Render();

            using (new Profiler.TimeScopeHelper(ScopeImGuiRender))
            {
                if (UEngine.Instance.Config.SupportMultWindows == false)
                {
                    var draw_data = ImGuiAPI.GetDrawData();
                    EGui.UImDrawDataRHI.RenderImDrawData(ref *draw_data, NativeWindow, mDrawData);
                    NativeWindow.SwapChain.Present(0, 0);
                }

                // Update and Render additional Platform Windows
                var io = ImGuiAPI.GetIO();
                if ((io.ConfigFlags & ImGuiConfigFlags_.ImGuiConfigFlags_ViewportsEnable) != 0)
                {
                    ImGuiAPI.UpdatePlatformWindows();
                    ImGuiAPI.RenderPlatformWindowsDefault((void*)0, (void*)0);
                }
            }
        }
        protected unsafe virtual void OnDrawUI()
        {
            var visible = true;
            Vector2 sz = new Vector2(300, 600);
            ImGuiAPI.SetNextWindowSize(in sz, ImGuiCond_.ImGuiCond_None);
            if (ImGuiAPI.Begin("Slate", &visible, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {

            }
            ImGuiAPI.End();
        }
    }

    public class USlateAppBase : USlateApplication, ITickable
    {
        public int GetTickOrder()
        {
            return -2;
        }
        public override void Cleanup()
        {
            UEngine.Instance.TickableManager.RemoveTickable(this);
            base.Cleanup();
        }
        public override async System.Threading.Tasks.Task<bool> InitializeApplication(NxRHI.UGpuDevice rc, RName rpName)
        {
            await base.InitializeApplication(rc, rpName);

            UEngine.Instance.TickableManager.AddTickable(this);
            return true;
        }
        protected bool Visible = true;
        protected unsafe override void OnDrawUI()
        {
            if (Visible == false)
            {
                var num = ImGuiAPI.PlatformIO_Viewports_Size(ImGuiAPI.GetPlatformIO());
                if (num == 1)
                {//只剩下被特意隐藏的主Viewport了
                    UEngine.Instance.PostQuitMessage();
                }
                return;
            }
            Vector2 sz = new Vector2(300, 600);
            ImGuiAPI.SetNextWindowSize(in sz, ImGuiCond_.ImGuiCond_FirstUseEver);
            if (ImGuiAPI.Begin("Slate", ref Visible, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {

            }
            ImGuiAPI.End();

            UEngine.RootFormManager.DrawRootForms();
        }
        #region Tick
        public virtual void TickLogic(float ellapse)
        {

        }
        public virtual void TickRender(float ellapse)
        {

        }
        public void TickBeginFrame(float ellapse)
        {
            
        }
        public virtual void TickSync(float ellapse)
        {
            //OnDrawSlate();
        }
        #endregion
    }

    partial class UEngine
    {
        public static URootFormManager RootFormManager
        {
            get
            {
                return URootFormManager.Insance;
            }
        }
    }
}
