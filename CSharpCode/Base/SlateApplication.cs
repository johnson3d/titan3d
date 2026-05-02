using EngineNS.Graphics.Pipeline;
using SDL;
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
        Thread.Async.TtTask<bool> Initialize();
    }
    public class TtRootFormManager
    {
        internal static TtRootFormManager Insance = new TtRootFormManager();
        private TtRootFormManager()
        {

        }
        #region RootForms
        private List<WeakReference<IRootForm>> AppendForms { get; } = new List<WeakReference<IRootForm>>();
        private List<WeakReference<IRootForm>> RootForms { get; } = new List<WeakReference<IRootForm>>();
        public void TourRootForms(Action<IRootForm> action)
        {
            foreach(var i in RootForms)
            {
                IRootForm form;
                if (!i.TryGetTarget(out form))
                    continue;
                action?.Invoke(form);
            }
        }

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
                        RootForms.Remove(i);
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

    public partial class TtSlateApplication
    {
        public TtPresentWindow NativeWindow;
        //public virtual EGui.Slate.UWorldViewportSlate GetWorldViewportSlate() { return null; }

        public IntPtr mImGuiContext;
        public EGui.TtImDrawDataRHI mDrawData = new EGui.TtImDrawDataRHI();
        
        public virtual async Thread.Async.TtTask<bool> InitializeApplication(NxRHI.TtGpuDevice rc, RName rpName)
        {
            await Thread.TtAsyncDummyClass.DummyFunc();

            NativeWindow.InitSwapChain(rc);
            unsafe
            {
                mDrawData.InitializeGraphics(NativeWindow.GetSwapchainFormat(), NativeWindow.GetSwapchainDSFormat());

                mImGuiContext = (IntPtr)ImGuiAPI.CreateContext(new ImFontAtlas((void*)0));
                ImGuiAPI.SetCurrentContext(mImGuiContext.ToPointer());
                TtEngine.Instance.GfxDevice.SlateRenderer.RecreateFontDeviceTexture();

                var io = ImGuiAPI.GetIO();
                var cachePath = TtEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.Cache);
                var imgui = TtEngine.Instance.Config.ImGuiIniPath;
                ImGuiAPI.SetIniFilename(IO.TtFileManager.CombinePath(cachePath, imgui));

                ImGuiConfigFlags_ configFlags = ImGuiConfigFlags_.ImGuiConfigFlags_None;
                //configFlags |= ImGuiConfigFlags_.ImGuiConfigFlags_DpiEnableScaleViewports;
                configFlags |= ImGuiConfigFlags_.ImGuiConfigFlags_DpiEnableScaleFonts;
                configFlags |= ImGuiConfigFlags_.ImGuiConfigFlags_NavEnableKeyboard;       // Enable Keyboard Controls
                //configFlags |= ImGuiConfigFlags_NavEnableGamepad;      // Enable Gamepad Controls
                configFlags |= ImGuiConfigFlags_.ImGuiConfigFlags_DockingEnable;           // Enable Docking
                if (TtEngine.Instance.Config.SupportMultWindows)
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

                ImGui_Init_SDL(ImGuiAPI.GetIO(), NativeWindow.Window);

                SetPerFrameImGuiData(1f / 60f);
            }
            return true;
        }
        public virtual void Cleanup()
        {
            mDrawData.Dispose();

            ImGuiData.Dispose();

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

            TtDockWindowSDL.ImGui_ImplSDL3_UpdateMouseData(ImGuiAPI.GetIO());
            TtDockWindowSDL.ImGui_ImplSDL3_UpdateMouseCursor(ImGuiAPI.GetIO());
            
            //EGui.UDockWindowSDL.ImGui_ImplSDL2_UpdateMonitors();

            ImGuiAPI.NewFrame();
        }
        #endregion

        [ThreadStatic]
        private static Profiler.TimeScope mScopeOnDrawUI;
        private static Profiler.TimeScope ScopeOnDrawUI
        {
            get
            {
                if (mScopeOnDrawUI == null)
                    mScopeOnDrawUI = new Profiler.TimeScope(typeof(TtSlateApplication), nameof(OnDrawUI));
                return mScopeOnDrawUI;
            }
        }

        [ThreadStatic]
        private static Profiler.TimeScope mScopeImGuiRender;
        private static Profiler.TimeScope ScopeImGuiRender
        {
            get
            {
                if (mScopeImGuiRender == null)
                    mScopeImGuiRender = new Profiler.TimeScope(typeof(TtSlateApplication), "RenderImData");
                return mScopeImGuiRender;
            }
        }
        
        public unsafe virtual void OnDrawSlate()
        {
            if (mImGuiContext == IntPtr.Zero)
                return;

            using (new Profiler.TimeScopeHelper(ScopeOnDrawUI))
            {
                ImGuiAPI.SetCurrentContext(mImGuiContext.ToPointer());

                Update((TtEngine.Instance.ElapseTickCountMS) * 0.001f);
                OnDrawUI();
                TtEngine.Instance.OnDrawTopMost();
            }

            TtEngine.Instance.InputSystem.ClearFilesDrop();
            ImGuiAPI.Render();

            using (new Profiler.TimeScopeHelper(ScopeImGuiRender))
            {
                if (TtEngine.Instance.Config.SupportMultWindows == false)
                {
                    var draw_data = ImGuiAPI.GetDrawData();
                    EGui.TtImDrawDataRHI.RenderImDrawData(ref *draw_data, NativeWindow, mDrawData);
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
            //var visible = true;
            //Vector2 sz = new Vector2(300, 600);
            //ImGuiAPI.SetNextWindowSize(in sz, ImGuiCond_.ImGuiCond_None);
            //if (ImGuiAPI.Begin("Slate", &visible, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            //{

            //}
            //ImGuiAPI.End();
            TtEngine.RootFormManager.DrawRootForms();
        }
    }

    public class TtSlateAppBase : TtSlateApplication, ITickable
    {
        public int GetTickOrder()
        {
            return -2;
        }
        public override void Cleanup()
        {
            TtEngine.Instance.TickableManager.RemoveTickable(this);
            base.Cleanup();
        }
        public override async Thread.Async.TtTask<bool> InitializeApplication(NxRHI.TtGpuDevice rc, RName rpName)
        {
            await base.InitializeApplication(rc, rpName);

            TtEngine.Instance.TickableManager.AddTickable(this);
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
                    TtEngine.Instance.PostQuitMessage();
                }
                return;
            }
            //Vector2 sz = new Vector2(300, 600);
            //ImGuiAPI.SetNextWindowSize(in sz, ImGuiCond_.ImGuiCond_FirstUseEver);
            //if (ImGuiAPI.Begin("Slate", ref Visible, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            //{

            //}
            //ImGuiAPI.End();

            TtEngine.RootFormManager.DrawRootForms();
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

    partial class TtEngine
    {
        public static TtRootFormManager RootFormManager
        {
            get
            {
                return TtRootFormManager.Insance;
            }
        }
        private List<IRootForm> TopMostForms { get; } = new List<IRootForm>();
        public void ShowTopMost(IRootForm form)
        {
            if (TopMostForms.Contains(form))
                return;
            form.Visible = true;
            TopMostForms.Add(form);
        }
        public void OnDrawTopMost()
        {
            for (int i = 0; i < TopMostForms.Count; i++)
            {
                if (TopMostForms[i].Visible)
                {
                    TopMostForms[i].OnDraw();
                }
                else
                {
                    TopMostForms.RemoveAt(i);
                    i--;
                }
            }
        }
        public bool IsBlockOperation = false;
        public string StopOperationInfo;
        public Action<ImDrawList, Vector2, Vector2> StopOperationDrawAction;
        public void BlockOperation(string info, Action<ImDrawList, Vector2, Vector2> drawAction = null)
        {
            IsBlockOperation = true;
            StopOperationInfo = info;
            StopOperationDrawAction = drawAction;
            //mStopOperateCover.Info = info;
            //mStopOperateCover.DrawAction = drawAction;
            //ShowTopMost(mStopOperateCover);
        }
        public void ResumeOperation()
        {
            IsBlockOperation = false;
            StopOperationInfo = "";
            StopOperationDrawAction = null;
            //mStopOperateCover.Visible = false;
            //mStopOperateCover.Info = null;
            //mStopOperateCover.DrawAction = null;
        }
    }
}
