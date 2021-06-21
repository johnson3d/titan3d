using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SDL2;

namespace EngineNS.Editor
{
    public class UMainEditorApplication : Graphics.Pipeline.USlateApplication, ITickable
    {
        public UAssetEditorManager AssetEditorManager { get; } = new UAssetEditorManager();
        public static List<IRootForm> RootForms { get; } = new List<IRootForm>();
        public static void RegRootForm(IRootForm form)
        {
            if (RootForms.Contains(form))
                return;
            RootForms.Add(form);
        }
        public static void UnregRootForm(IRootForm form)
        {
            RootForms.Remove(form);
        }
        public UMainEditorApplication()
        {
            mCpuProfiler = new Editor.Forms.UCpuProfiler();            
            mMetaViewer = new Editor.MetaViewEditor();            
            mMainInspector = new Forms.UInspector();
            WorldViewportSlate = new UWorldViewportSlate(true);            
            mWorldOutliner = new Editor.Forms.UWorldOutliner();
            
            mWorldOutliner.TestUWorldOutliner(this);
        }
        private bool IsVisible = true;
        public Editor.Forms.UWorldOutliner mWorldOutliner;
        public Editor.Forms.UCpuProfiler mCpuProfiler;
        public Editor.Forms.UInspector mMainInspector;
        public Editor.MetaViewEditor mMetaViewer = null;

        public UWorldViewportSlate WorldViewportSlate = null;
        public EGui.Controls.ContentBrowser ContentBrowser = new EGui.Controls.ContentBrowser();
        public override void Cleanup()
        {
            RootForms.Clear();
            UEngine.Instance?.TickableManager.RemoveTickable(this);
            base.Cleanup();
        }
        public override async System.Threading.Tasks.Task<bool> InitializeApplication(RHI.CRenderContext rc, Type rpType)
        {
            await base.InitializeApplication(rc, rpType);

            await ContentBrowser.Initialize();

            var RenderPolicy = Rtti.UTypeDescManager.CreateInstance(rpType) as Graphics.Pipeline.IRenderPolicy;
            await WorldViewportSlate.Initialize(this, RenderPolicy, 0, 1);

            mMainInspector.PropertyGrid.PGName = "MainInspector";
            mMainInspector.PropertyGrid.Target = EGui.UIProxy.StyleConfig.Instance;// WorldViewportSlate;

            UEngine.Instance.TickableManager.AddTickable(this);

            /////////////////////////////////
            mWinTest.Initialized();
            /////////////////////////////////

            return true;
        }

        #region DrawGui
        bool _showDemoWindow = true;
        public float LeftWidth = 0;
        public float CenterWidth = 0;
        public float RightWidth = 0;

        ////////////////////////
        UIWindowsTest mWinTest = new UIWindowsTest();
        ////////////////////////

        public uint LeftDockId { get; private set; } = 0;
        public uint CenterDockId { get; private set; } = 0;
        public uint RightDockId { get; private set; } = 0;

        public UInt32 ActiveViewportId = UInt32.MaxValue;
        protected unsafe override void OnDrawUI()
        {
            {
                var count = ImGuiAPI.PlatformIO_Viewports_Size(ImGuiAPI.GetPlatformIO());
                for (int i = 0; i < count; i++)
                {
                    var pViewport = ImGuiAPI.PlatformIO_Viewports_Get(ImGuiAPI.GetPlatformIO(), i);
                    var nativeWindow = pViewport->m_PlatformHandle;
                    var flags = SDL.SDL_GetWindowFlags((IntPtr)nativeWindow);
                    if ((flags & (uint)SDL.SDL_WindowFlags.SDL_WINDOW_INPUT_FOCUS) != 0)
                    {
                        ActiveViewportId = pViewport->ID;
                    }
                }
            }
            if (IsVisible == false)
            {
                AssetEditorManager.CloseAll();
                var num = ImGuiAPI.PlatformIO_Viewports_Size(ImGuiAPI.GetPlatformIO());
                if (num == 1)
                {//只剩下被特意隐藏的主Viewport了
                    UEngine.Instance.PostQuitMessage();
                }
                return;
            }

            if (LeftDockId == 0)
            {
                LeftDockId = ImGuiAPI.GetID("LeftDocker");
                CenterDockId = ImGuiAPI.GetID("CenterDocker");
                RightDockId = ImGuiAPI.GetID("RightDocker");

                mCpuProfiler.DockId = CenterDockId;
                mMetaViewer.DockId = RightDockId;
                WorldViewportSlate.DockId = CenterDockId;
                mWorldOutliner.DockId = LeftDockId;
                mMainInspector.DockId = RightDockId;
                ContentBrowser.DockId = CenterDockId;
            }

            var io = ImGuiAPI.GetIO();
            if ((io.ConfigFlags & ImGuiConfigFlags_.ImGuiConfigFlags_DockingEnable) != 0)
            {
                ImGuiAPI.DockSpaceOverViewport(ImGuiAPI.GetMainViewport(), ImGuiDockNodeFlags_.ImGuiDockNodeFlags_None, null);
                
                //var dockspace_id = ImGuiAPI.GetID("MyDockSpace");
                //ImGuiDockNodeFlags_ dockspace_flags = ImGuiDockNodeFlags_.ImGuiDockNodeFlags_None;
                //var winClass = new ImGuiWindowClass();
                //winClass.UnsafeCallConstructor();
                //var sz = new Vector2(0.0f, 0.0f);                
                //ImGuiAPI.DockSpace(dockspace_id, ref sz, dockspace_flags, ref winClass);
                //winClass.UnsafeCallDestructor();
            }
            try
            {
                var mainPos = new Vector2(0);
                ImGuiAPI.SetNextWindowPos(ref mainPos, ImGuiCond_.ImGuiCond_FirstUseEver, ref mainPos);
                var wsz = new Vector2(1290, 800);
                ImGuiAPI.SetNextWindowSize(ref wsz, ImGuiCond_.ImGuiCond_FirstUseEver);
                if (ImGuiAPI.Begin("T3D CoreEditor", ref IsVisible, //ImGuiWindowFlags_.ImGuiWindowFlags_NoMove |
                    //ImGuiWindowFlags_.ImGuiWindowFlags_NoResize |
                    ImGuiWindowFlags_.ImGuiWindowFlags_NoCollapse |
                    ImGuiWindowFlags_.ImGuiWindowFlags_MenuBar))
                {
                    wsz = ImGuiAPI.GetWindowSize();
                    DrawMainMenu();
                    DrawToolBar();

                    ImGuiAPI.Separator();
                    ImGuiAPI.Columns(3, null, true);
                    if (LeftWidth == 0)
                    {
                        ImGuiAPI.SetColumnWidth(0, wsz.X * 0.15f);
                    }
                    var min = ImGuiAPI.GetWindowContentRegionMin();
                    var max = ImGuiAPI.GetWindowContentRegionMin();

                    DrawLeft(ref min, ref max);
                    LeftWidth = ImGuiAPI.GetColumnWidth(0);
                    ImGuiAPI.NextColumn();

                    if (RightWidth == 0)
                    {
                        CenterWidth = ImGuiAPI.GetColumnWidth(1);
                        RightWidth = ImGuiAPI.GetColumnWidth(2);
                        ImGuiAPI.SetColumnWidth(1, wsz.X * 0.70f);
                        //if (CenterWidth + RightWidth > 200)
                        //{
                        //    ImGuiAPI.SetColumnWidth(1, CenterWidth + RightWidth - 200);
                        //}
                    }
                    DrawCenter(ref min, ref max);
                    CenterWidth = ImGuiAPI.GetColumnWidth(1);
                    ImGuiAPI.NextColumn();

                    DrawRight(ref min, ref max);
                    RightWidth = ImGuiAPI.GetColumnWidth(2);
                    ImGuiAPI.NextColumn();

                    ImGuiAPI.Columns(1, null, true);
                }
                ImGuiAPI.End();

                WorldViewportSlate.DockId = CenterDockId;

                foreach(var i in RootForms)
                {
                    if (i.Visible == false)
                        continue;
                    i.OnDraw();
                }

                AssetEditorManager.OnDraw();

                mWinTest.OnDraw();
            }
            catch
            {

            }

            if (_showDemoWindow)
            {
                // Normally user code doesn't need/want to call this because positions are saved in .ini file anyway.
                // Here we just want to make the demo initial state a bit more friendly!
                var pos = new Vector2(650, 20);
                var pivot = new Vector2(0, 0);
                ImGuiAPI.SetNextWindowPos(ref pos, ImGuiCond_.ImGuiCond_FirstUseEver, ref pivot);
                ImGuiAPI.ShowDemoWindow(ref _showDemoWindow);
            }
        }
        private void DrawMainMenu()
        {
            if (ImGuiAPI.BeginMenuBar())
            {
                if (ImGuiAPI.BeginMenu("File", true))
                {
                    if (ImGuiAPI.MenuItem("Load", null, false, true))
                    {
                        
                    }
                    if (ImGuiAPI.MenuItem("Save", null, false, true))
                    {
                        
                    }
                    ImGuiAPI.EndMenu();
                }
                if (ImGuiAPI.BeginMenu("Windows", true))
                {
                    var check = mWorldOutliner.Visible;
                    ImGuiAPI.Checkbox("##mWorldOutliner", ref check);
                    ImGuiAPI.SameLine(0, -1);
                    if ( ImGuiAPI.MenuItem("WorldOutliner", null, false, true))
                    {
                        mWorldOutliner.Visible = !mWorldOutliner.Visible;
                    }
                    check = mCpuProfiler.Visible;
                    ImGuiAPI.Checkbox("##mCpuProfiler", ref check);
                    ImGuiAPI.SameLine(0, -1);
                    if (ImGuiAPI.MenuItem("CpuProfiler", null, false, true))
                    {
                        mCpuProfiler.Visible = !mCpuProfiler.Visible;
                    }
                    check = mMainInspector.Visible;
                    ImGuiAPI.Checkbox("##mMainInspector", ref check);
                    ImGuiAPI.SameLine(0, -1);
                    if (ImGuiAPI.MenuItem("MainInspector", null, false, true))
                    {
                        mMainInspector.Visible = !mMainInspector.Visible;
                    }
                    check = mMetaViewer.Visible;
                    ImGuiAPI.Checkbox("##mMetaViewer", ref check);
                    ImGuiAPI.SameLine(0, -1);
                    if (ImGuiAPI.MenuItem("MetaViewer", null, false, true))
                    {
                        mMetaViewer.Visible = !mMetaViewer.Visible;
                    }
                    ImGuiAPI.EndMenu();
                }
                ImGuiAPI.EndMenuBar();
            }
        }
        private void DrawToolBar()
        {
            var btSize = new Vector2(64, 64);
            ImGuiAPI.Button("New", ref btSize);
            ImGuiAPI.SameLine(0, -1);
            ImGuiAPI.Button("Save", ref btSize);
        }
        protected unsafe void DrawLeft(ref Vector2 min, ref Vector2 max)
        {
            var size = new Vector2(-1, -1);
            if (ImGuiAPI.BeginChild("LeftWindow", ref size, false, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                ImGuiDockNodeFlags_ dockspace_flags = ImGuiDockNodeFlags_.ImGuiDockNodeFlags_None;
                var winClass = new ImGuiWindowClass();
                winClass.UnsafeCallConstructor();
                var sz = new Vector2(0.0f, 0.0f);
                ImGuiAPI.DockSpace(LeftDockId, ref sz, dockspace_flags, ref winClass);
                winClass.UnsafeCallDestructor();
            }
            ImGuiAPI.EndChild();
        }
        protected unsafe void DrawCenter(ref Vector2 min, ref Vector2 max)
        {
            var size = new Vector2(-1, -1);
            if (ImGuiAPI.BeginChild("CenterWindow", ref size, false, 
                ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                ImGuiDockNodeFlags_ dockspace_flags = ImGuiDockNodeFlags_.ImGuiDockNodeFlags_None;
                var sz = new Vector2(0.0f, 0.0f);
                //var winClass = new ImGuiWindowClass();
                //winClass.UnsafeCallConstructor();
                //ImGuiAPI.DockSpace(CenterDockId, ref sz, dockspace_flags, ref winClass);
                //winClass.UnsafeCallDestructor();

                ImGuiAPI.DockSpace(CenterDockId, &sz, dockspace_flags, (ImGuiWindowClass*)0);
            }
            ImGuiAPI.EndChild();
        }
        protected unsafe void DrawRight(ref Vector2 min, ref Vector2 max)
        {
            var size = new Vector2(-1, -1);
            if (ImGuiAPI.BeginChild("RightWindow", ref size, false, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                ImGuiDockNodeFlags_ dockspace_flags = ImGuiDockNodeFlags_.ImGuiDockNodeFlags_None;
                var winClass = new ImGuiWindowClass();
                winClass.UnsafeCallConstructor();
                var sz = new Vector2(0.0f, 0.0f);
                ImGuiAPI.DockSpace(RightDockId, ref sz, dockspace_flags, ref winClass);
                winClass.UnsafeCallDestructor();
            }
            ImGuiAPI.EndChild();
        }
        #endregion

        #region Tick
        public void TickLogic(int ellapse)
        {
            WorldViewportSlate.TickLogic(ellapse);
        }
        public void TickRender(int ellapse)
        {
            WorldViewportSlate.TickRender(ellapse);
        }
        public void TickSync(int ellapse)
        {
            WorldViewportSlate.TickSync(ellapse);

            OnDrawSlate();
        }
        #endregion
    }

    public interface IRootForm
    {
        bool Visible { get; set; }
        void OnDraw();
        uint DockId { get; set; }
        ImGuiCond_ DockCond { get; set; }
    }
}
