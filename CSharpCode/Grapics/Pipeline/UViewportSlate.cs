using System;
using System.Collections.Generic;
using SDL2;

namespace EngineNS.Graphics.Pipeline
{
    public class UViewportSlate : IEventProcessor, Editor.IRootForm
    {
        public UViewportSlate(bool regRoot)
        {
            if (regRoot)
                Editor.UMainEditorApplication.RegRootForm(this);
        }
        public string Title { get; set; } = "Game";
        public bool Visible { get; set; } = true;
        public uint DockId { get; set; }
        public Vector2 ViewportPos { get; private set; }
        public Vector2 WindowPos { get; private set; }
        public Vector2 ClientMin { get; private set; }
        public Vector2 ClientMax { get; private set; }
        public Vector2 ClientSize
        {
            get
            {
                return ClientMax - ClientMin;
            }
        }
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(ReadOnly = true, UserDraw = false)]
        public Graphics.Pipeline.IRenderPolicy RenderPolicy { get; protected set; }
        public Vector2 Window2Viewport(Vector2 pos)
        {//pos为真实窗口的坐标，返回ViewportSlate坐标
            Vector2 tmp;
            tmp.X = pos.X - (int)(WindowPos.X + ClientMin.X - ViewportPos.X);
            tmp.Y = pos.Y - (int)(WindowPos.Y + ClientMin.Y - ViewportPos.Y);
            return tmp;
        }
        private bool mClientChanged = false;
        private bool mSizeChanged = false;
        public bool IsValidClientArea()
        {
            return (ClientSize.X > 1 && ClientSize.Y > 1);
        }
        public bool IsFocused { get; private set; }
        public bool IsDrawing { get; private set; }

        private Graphics.Pipeline.UPresentWindow mPresentWindow;
        public bool ShowCloseButton = false;
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;
        public virtual unsafe void OnDraw()
        {
            ImGuiAPI.SetNextWindowDockID(DockId, DockCond);
            var sz = new Vector2(100, 100);
            //ImGuiAPI.SetNextWindowSize(ref sz, ImGuiCond_.ImGuiCond_FirstUseEver);
            IsDrawing = false;
            bool bShow;
            if (ShowCloseButton)
            {
                var visible = Visible;
                bShow = ImGuiAPI.Begin(Title, ref visible, ImGuiWindowFlags_.ImGuiWindowFlags_NoBackground);
            }
            else
            {
                bShow = ImGuiAPI.Begin(Title, (bool*)0, ImGuiWindowFlags_.ImGuiWindowFlags_NoBackground);
            }
            if (ImGuiAPI.IsWindowDocked())
            {
                DockId = ImGuiAPI.GetWindowDockID();
            }
            if (bShow)
            {
                sz = ImGuiAPI.GetWindowSize();
                var imViewport = ImGuiAPI.GetWindowViewport();
                if ((IntPtr)imViewport->PlatformUserData != IntPtr.Zero)
                {
                    var gcHandle = System.Runtime.InteropServices.GCHandle.FromIntPtr((IntPtr)imViewport->PlatformUserData);
                    var myWindow = gcHandle.Target as Graphics.Pipeline.UPresentWindow;
                    if (myWindow != mPresentWindow)
                    {
                        mPresentWindow?.UnregEventProcessor(this);
                        mPresentWindow = myWindow;
                        mPresentWindow.RegEventProcessor(this);
                    }
                }
                IsDrawing = true;
                IsFocused = ImGuiAPI.IsWindowFocused(ImGuiFocusedFlags_.ImGuiFocusedFlags_None);
                var pos = ImGuiAPI.GetWindowPos();
                ViewportPos = ImGuiAPI.GetWindowViewport()->Pos;
                if (pos != WindowPos)
                {
                    mClientChanged = true;
                    WindowPos = pos;
                }
                var min = ImGuiAPI.GetWindowContentRegionMin();
                var max = ImGuiAPI.GetWindowContentRegionMax();
                sz = max - min;
                if (sz.X == 0 || sz.Y == 0)
                {
                    sz.X++;
                    sz.Y++;
                }
                ImGuiAPI.InvisibleButton("ViewportClient", &sz, ImGuiButtonFlags_.ImGuiButtonFlags_None);
                if (min != ClientMin || max != ClientMax)
                {
                    mClientChanged = true;

                    var saved = ClientSize;
                    ClientMin = min;
                    ClientMax = max;

                    if (Math.Abs(ClientSize.X - saved.X) > 0.1f || Math.Abs(ClientSize.Y - saved.Y) > 0.1f)
                    {
                        mSizeChanged = true;
                    }
                }
                var showTexture = GetShowTexture();
                if (showTexture != IntPtr.Zero)
                {
                    var drawlist = new ImDrawList(ImGuiAPI.GetWindowDrawList());
                    var uv1 = new Vector2(0, 0);
                    var uv2 = new Vector2(1, 1);
                    unsafe
                    {
                        min = min + pos;
                        max = max + pos;
                        drawlist.AddImage(showTexture.ToPointer(), ref min, ref max, ref uv1, ref uv2, 0xFFFFFFFF);
                    }
                }
            }
            else
            {
                IsFocused = false;
                mPresentWindow?.UnregEventProcessor(this);
                mPresentWindow = null;
            }
            ImGuiAPI.End();
            if (mClientChanged && IsValidClientArea())
            {
                OnClientChanged(mSizeChanged);
                mClientChanged = false;
                mSizeChanged = false;
            }
        }
        protected virtual void OnClientChanged(bool bSizeChanged)
        {

        }
        public unsafe virtual bool OnEvent(ref SDL.SDL_Event e)
        {
            if (e.type == SDL.SDL_EventType.SDL_MOUSEBUTTONUP)
            {
                var edtorPolicy = this.RenderPolicy as Graphics.Pipeline.Mobile.UMobileEditorFSPolicy;
                if (edtorPolicy != null)
                {
                    var pos = Window2Viewport(new Vector2((float)e.motion.x, (float)e.motion.y));
                    var hitObj = edtorPolicy.GetHitproxy((uint)pos.X, (uint)pos.Y);
                    OnHitproxySelected(hitObj);
                }
            }
            return true;
        }
        protected virtual IntPtr GetShowTexture()
        {
            return IntPtr.Zero;
        }
        protected virtual void OnHitproxySelected(Graphics.Pipeline.IProxiable proxy)
        {
            var edtorPolicy = this.RenderPolicy as Graphics.Pipeline.Mobile.UMobileEditorFSPolicy;
            if (edtorPolicy != null)
            {
                if (proxy == null)
                {
                    edtorPolicy.PickedProxiableManager.ClearSelected();
                }
                else
                {
                    edtorPolicy.PickedProxiableManager.Selected(proxy);
                }
            }
            if (proxy == null)
            {
                var mainEditor = UEngine.Instance.GfxDevice.MainWindow as Editor.UMainEditorApplication;
                mainEditor.WorldViewportSlate.ShowBoundVolumes(false, null);
                return;
            }
            var node = proxy as GamePlay.Scene.UNode;
            if (node != null)
            {
                var mainEditor = UEngine.Instance.GfxDevice.MainWindow as Editor.UMainEditorApplication;
                mainEditor.WorldViewportSlate.ShowBoundVolumes(true, node);
            }
        }
    }
}
