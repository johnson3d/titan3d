using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.GamePlay
{
    public class UGameViewportSlate : EngineNS.EGui.Slate.UWorldViewportSlate, Graphics.Pipeline.IRootForm
    {
        public UGameViewportSlate(bool regRoot)
        {
            if (regRoot)
                Editor.UMainEditorApplication.RegRootForm(this);
            CameraController = new Editor.Controller.EditorCameraController();
        }
        CNativeString mNstrTitle = new CNativeString();
        public override string Title 
        {
            get => base.Title;
            set
            {
                base.Title = value;
                mNstrTitle.SetText(value);
            } 
        }
        protected override void OnVieportClosed()
        {
            UEngine.Instance.EndPlayInEditor();
        }
        public bool IsSetViewportPos = false;
        public Vector2 GameViewportPos;
        public Vector2 GameViewportSize;
        //public static bool bMessageBox = false;
        public override unsafe void OnDraw()
        {
            ImGuiAPI.SetNextWindowDockID(DockId, DockCond);
            if (IsSetViewportPos)
            {
                ImGuiAPI.SetNextWindowPos(ref GameViewportPos, ImGuiCond_.ImGuiCond_Always, ref Vector2.mZero);
                ImGuiAPI.SetNextWindowSize(ref GameViewportSize, ImGuiCond_.ImGuiCond_Always);
            }
            IsDrawing = false;
            //ClrLogger.SetMessageBox(bMessageBox);
            //CoreSDK.Print2Console2("aaa", true);
            bool bShow = ImGuiAPI.Begin(Title, ref mVisible, ImGuiWindowFlags_.ImGuiWindowFlags_NoBackground);
            if (ImGuiAPI.IsWindowDocked())
            {
                DockId = ImGuiAPI.GetWindowDockID();
            }
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
            if (mVisible == false)
            {
                OnVieportClosed();
            }
        }
    }
}
