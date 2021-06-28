using System;
using System.Collections.Generic;
using SDL2;

namespace EngineNS.Graphics.Pipeline
{
    public class UViewportSlate : IEventProcessor
    {
        public virtual string Title { get; set; } = "Game";
        protected bool mVisible = true;
        public bool Visible { get => mVisible; set => mVisible = value; }
        public uint DockId { get; set; }
        public Vector2 ViewportPos { get; protected set; }
        public Vector2 WindowPos { get; protected set; }
        public Vector2 ClientMin { get; protected set; }
        public Vector2 ClientMax { get; protected set; }
        public Vector2 ClientSize
        {
            get
            {
                return ClientMax - ClientMin;
            }
        }
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(ReadOnly = true, UserDraw = false)]
        public Graphics.Pipeline.IRenderPolicy RenderPolicy { get; set; }
        public Vector2 Window2Viewport(Vector2 pos)
        {//pos为真实窗口的坐标，返回ViewportSlate坐标
            Vector2 tmp;
            tmp.X = pos.X - (int)(WindowPos.X + ClientMin.X - ViewportPos.X);
            tmp.Y = pos.Y - (int)(WindowPos.Y + ClientMin.Y - ViewportPos.Y);
            return tmp;
        }
        protected bool mClientChanged = false;
        protected bool mSizeChanged = false;
        public bool IsValidClientArea()
        {
            return (ClientSize.X > 1 && ClientSize.Y > 1);
        }
        public bool IsFocused { get; protected set; }
        public bool IsDrawing { get; protected set; }

        protected Graphics.Pipeline.UPresentWindow mPresentWindow;
        public enum EVieportType
        {
            Window,
            WindowWithClose,            
            ChildWindow,
        }
        public EVieportType VieportType { get; set; } = EVieportType.Window;
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;
        public virtual unsafe void OnDraw()
        {
            ImGuiAPI.SetNextWindowDockID(DockId, DockCond);
            var sz = new Vector2(-1);
            //ImGuiAPI.SetNextWindowSize(ref sz, ImGuiCond_.ImGuiCond_FirstUseEver);
            IsDrawing = false;
            bool bShow = false;
            switch(VieportType)
            {
                case EVieportType.Window:
                    bShow = ImGuiAPI.Begin(Title, ref mVisible, ImGuiWindowFlags_.ImGuiWindowFlags_NoBackground);
                    break;
                case EVieportType.WindowWithClose:
                    bShow = ImGuiAPI.Begin(Title, (bool*)0, ImGuiWindowFlags_.ImGuiWindowFlags_NoBackground);
                    break;
                case EVieportType.ChildWindow:
                    bShow = ImGuiAPI.BeginChild(Title, ref sz, false, ImGuiWindowFlags_.ImGuiWindowFlags_NoBackground);
                    break;
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
            switch (VieportType)
            {
                case EVieportType.Window:
                case EVieportType.WindowWithClose:
                    ImGuiAPI.End();
                    break;
                case EVieportType.ChildWindow:
                    ImGuiAPI.EndChild();
                    break;
            }
            
            if (mClientChanged && IsValidClientArea())
            {
                OnClientChanged(mSizeChanged);
                mClientChanged = false;
                mSizeChanged = false;
            }
            if (mVisible == false)
            {
                OnVieportClosed();
            }
        }
        protected virtual void OnVieportClosed()
        {

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
        }
    }
}
