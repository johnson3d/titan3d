using System;
using System.Collections.Generic;
using System.Text;
using ImGuiNET;

namespace EngineNS.GuiWrapper
{
    public class GuiContext
    {
        public GuiContext()
        {
            mContext = ImGui.CreateContext();
        }
        ~GuiContext()
        {
            if (mContext != IntPtr.Zero)
            {
                ImGui.DestroyContext(mContext);
                mContext = IntPtr.Zero;
            }
        }
        IntPtr mContext;
        public IntPtr Context
        {
            get { return mContext; }
        }
        public void SetFont(RName fontname)
        {
            var fonts = ImGui.GetIO().Fonts;
            fonts.AddFontDefault();

            FontTexId = GuiManager.Instance.GetFontMaterial(fontname, this).Ptr;
            fonts.SetTexID(FontTexId);
        }
        public IntPtr FontTexId
        {
            get;
            private set;
        }
        public void TickLogic(float deltaSeconds)
        {
            ImGui.SetCurrentContext(mContext);
            SetPerFrameImGuiData(deltaSeconds);
            ImGui.NewFrame();
        }
        public Vector2 ScaleFactor
        {
            get;
            set;
        }
        private void SetPerFrameImGuiData(float deltaSeconds)
        {
            ImGuiIOPtr io = ImGui.GetIO();
            //io.DisplaySize = new Vector2(
            //    _windowWidth / _scaleFactor.X,
            //    _windowHeight / _scaleFactor.Y);
            //io.DisplayFramebufferScale = new System.Numerics.Vector2(ScaleFactor.X, ScaleFactor.Y);
            io.DeltaTime = deltaSeconds; // DeltaTime is in seconds.
        }
    }
}
