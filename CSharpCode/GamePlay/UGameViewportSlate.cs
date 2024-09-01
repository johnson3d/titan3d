using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.GamePlay
{
    public class TtGameViewportSlate : EngineNS.EGui.Slate.TtWorldViewportSlate, IRootForm
    {
        public TtGameViewportSlate(bool regRoot)
        {
            if (regRoot)
                TtEngine.RootFormManager.RegRootForm(this);
            CameraController = new Editor.Controller.EditorCameraController();
        }
        TtNativeString mNstrTitle = new TtNativeString();
        public override string Title 
        {
            get => base.Title;
            set
            {
                base.Title = value;
                mNstrTitle.SetText(value);
            } 
        }
        protected override void OnViewportClosed()
        {
            mPresentWindow?.UnregEventProcessor(this);
            TtEngine.Instance.EndPlayInEditor();
        }
        public bool IsSetViewportPos = false;
        public Vector2 GameViewportPos = new Vector2(0,0);
        public Vector2 GameViewportSize = new Vector2(800, 600);
        //public static bool bMessageBox = false;
        public override unsafe void OnDraw()
        {
            //if (IsSetViewportPos)
            {
                ImGuiAPI.SetNextWindowPos(in GameViewportPos, ImGuiCond_.ImGuiCond_FirstUseEver, in Vector2.Zero);
                ImGuiAPI.SetNextWindowSize(in GameViewportSize, ImGuiCond_.ImGuiCond_FirstUseEver);
            }
            IsDrawing = false;
            //ClrLogger.SetMessageBox(bMessageBox);
            //CoreSDK.Print2Console2("aaa", true);
            bool bShow = EGui.UIProxy.DockProxy.BeginMainForm(Title, this, ImGuiWindowFlags_.ImGuiWindowFlags_NoBackground);
            if (bShow)
            {
                var sz = ImGuiAPI.GetWindowSize();
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
                        drawlist.AddImage(showTexture.ToPointer(), in min, in max, in uv1, in uv2, 0x01FFFFFF);// 0xFFFFFFFF);
                    }
                }
            }
            else
            {
                IsFocused = false;
                mPresentWindow?.UnregEventProcessor(this);
                mPresentWindow = null;
            }
            EGui.UIProxy.DockProxy.EndMainForm(bShow);
            if (mClientChanged && IsValidClientArea())
            {
                OnClientChanged(mSizeChanged);
                mClientChanged = false;
                mSizeChanged = false;
            }
            if (mVisible == false)
            {
                OnViewportClosed();
            }
        }
    }
}
