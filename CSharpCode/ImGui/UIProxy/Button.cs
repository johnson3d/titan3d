using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.EGui.UIProxy
{
    public class ImageButtonProxy : IUIProxyBase
    {
        public RName ImageFile
        {
            get => mImage.ImageFile;
            set => mImage.ImageFile = value;
        }
        Vector2 mSize = new Vector2(32, 32);
        public Vector2 Size
        {
            get => mSize;
            set => mSize = value;
        }
        public Vector2 UVMin
        {
            get => mImage.UVMin;
            set => mImage.UVMin = value;
        }
        public Vector2 UVMax
        {
            get => mImage.UVMax;
            set => mImage.UVMax = value;
        }
        public Vector2 ImageSize
        {
            get => mImage.ImageSize;
            set => mImage.ImageSize = value;
        }
        public Action Action;
        public UInt32 ImageColor = 0xFFBFBFBF;
        public UInt32 ImageHoveredColor = 0xFFFFFFFF;
        public UInt32 ImageActiveColor = 0xFFFFFFFF;
        public bool ShowBG = false;

        //System.Threading.Tasks.Task<RHI.CShaderResourceView> mTask;
        //IntPtr mImagePtr;

        ImageProxy mImage = new ImageProxy();

        public void Cleanup()
        {
            mImage?.Dispose();
        }
        public async System.Threading.Tasks.Task<bool> Initialize()
        {
            await mImage.Initialize();
            return true;
        }
        public unsafe bool OnDraw(ref ImDrawList drawList, ref Support.UAnyPointer drawData)
        {
            bool retValue = false;
            var rectStart = ImGuiAPI.GetCursorScreenPos();

            var id = ImGuiAPI.GetID("#Button");
            var style = ImGuiAPI.GetStyle();
            var size = ImGuiAPI.CalcItemSize(ref mSize, ImageSize.X + style->FramePadding.X * 2.0f, ImageSize.Y + style->FramePadding.Y * 2.0f);
            var rectEnd = rectStart + size;

            ImGuiAPI.ItemSize(ref size, style->FramePadding.Y);
            if (!ImGuiAPI.ItemAdd(ref rectStart, ref rectEnd, id, 0))
                return false;

            bool hovered = false, held = false;
            var pressed = ImGuiAPI.ButtonBehavior(ref rectStart, ref rectEnd, id, ref hovered, ref held, ImGuiButtonFlags_.ImGuiButtonFlags_MouseButtonLeft);
            if (pressed)
                mImage.Color = ImageActiveColor;
            else if (hovered)
                mImage.Color = ImageHoveredColor;
            else
                mImage.Color = ImageColor;
            if(ShowBG)
            {
                var col = ImGuiAPI.ColorConvertFloat4ToU32(&style->Colors[(int)((held && hovered) ? ImGuiCol_.ImGuiCol_ButtonActive : hovered ? ImGuiCol_.ImGuiCol_ButtonHovered : ImGuiCol_.ImGuiCol_Button)]);
                ImGuiAPI.RenderFrame(ref rectStart, ref rectEnd, col, true, style->FrameRounding);
            }

            if (pressed)
            {
                Action?.Invoke();
                retValue = true;
            }

            var imgPos = rectStart + (size - mImage.ImageSize) * 0.5f;
            mImage.OnDraw(ref drawList, ref imgPos);

            return retValue;
        }
    }
}
