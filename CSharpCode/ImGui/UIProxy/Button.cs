using EngineNS.Graphics.Pipeline;
using EngineNS.Support;
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

        //System.Threading.Tasks.Task<NxRHI.USrView> mTask;
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
        public unsafe bool OnDraw(in ImDrawList drawList, in Support.UAnyPointer drawData)
        {
            bool retValue = false;
            var rectStart = ImGuiAPI.GetCursorScreenPos();

            var id = ImGuiAPI.GetID("#Button");
            var style = ImGuiAPI.GetStyle();
            var size = ImGuiAPI.CalcItemSize(ref mSize, ImageSize.X + style->FramePadding.X * 2.0f, ImageSize.Y + style->FramePadding.Y * 2.0f);
            var rectEnd = rectStart + size;

            ImGuiAPI.ItemSize(in size, 0);
            if (!ImGuiAPI.ItemAdd(in rectStart, in rectEnd, id, 0))
                return false;

            bool hovered = false, held = false;
            var pressed = ImGuiAPI.ButtonBehavior(in rectStart, in rectEnd, id, ref hovered, ref held, ImGuiButtonFlags_.ImGuiButtonFlags_MouseButtonLeft);
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
            mImage.OnDraw(in drawList, in imgPos);

            return retValue;
        }
    }

    public class ImageToggleButtonProxy : IUIProxyBase
    {
        RName mNormal;
        public RName Normal
        {
            get => mNormal;
            set
            {
                mNormal = value;
                if (value == null)
                    mNormalUVAnimTask = null;
                else
                    mNormalUVAnimTask = UEngine.Instance.GfxDevice.UvAnimManager.GetUVAnim(value);
            }
        }
        RName mChecked;
        public RName Checked
        {
            get => mChecked;
            set
            {
                mChecked = value;
                if (value == null)
                    mCheckedUVAnimTask = null;
                else
                    mCheckedUVAnimTask = UEngine.Instance.GfxDevice.UvAnimManager.GetUVAnim(value);
            }
        }

        public bool IsChecked = false;
        public string Name;
        public Vector2 Size = new Vector2(32, 32);
        public Action Action;

        Thread.Async.TtTask<TtUVAnim>? mNormalUVAnimTask;
        Thread.Async.TtTask<TtUVAnim>? mCheckedUVAnimTask;
        public EGui.TtUVAnim NormalUVAnim
        {
            get
            {
                if (mNormalUVAnimTask == null)
                    return null;
                if (mNormalUVAnimTask.Value.IsCompleted == false)
                    return null;
                return mNormalUVAnimTask.Value.Result;
            }
        }
        public EGui.TtUVAnim CheckedUVAnim
        {
            get
            {
                if (mCheckedUVAnimTask == null)
                    return null;
                if (mCheckedUVAnimTask.Value.IsCompleted == false)
                    return null;
                return mCheckedUVAnimTask.Value.Result;
            }
        }

        public void Cleanup()
        {

        }

        public async Task<bool> Initialize()
        {
            return true;
        }

        public bool IsReadToDraw()
        {
            if (mNormalUVAnimTask != null)
            {
                if (mNormalUVAnimTask.Value.IsCompleted == false)
                    return false;
                if (mNormalUVAnimTask.Value.Result.IsReadyToDraw() == false)
                    return false;
            }
            if (mCheckedUVAnimTask != null)
            {
                if (mCheckedUVAnimTask.Value.IsCompleted == false)
                    return false;
                if (mCheckedUVAnimTask.Value.Result.IsReadyToDraw() == false)
                    return false;
            }
            return true;
        }

        public unsafe bool OnDraw(in ImDrawList drawList, in UAnyPointer drawData)
        {
            if (!IsReadToDraw())
                return false;

            bool retValue = false;
            var rectStart = ImGuiAPI.GetCursorScreenPos();

            var id = ImGuiAPI.GetID(Name);
            var style = ImGuiAPI.GetStyle();
            var size = ImGuiAPI.CalcItemSize(ref Size, Size.X + style->FramePadding.X * 2.0f, Size.Y + style->FramePadding.Y * 2.0f);
            var rectEnd = rectStart + size;

            ImGuiAPI.ItemSize(in size, 0);
            if (!ImGuiAPI.ItemAdd(in rectStart, in rectEnd, id, 0))
                return false;

            bool hovered = false, held = false;
            var pressed = ImGuiAPI.ButtonBehavior(in rectStart, in rectEnd, id, ref hovered, ref held, ImGuiButtonFlags_.ImGuiButtonFlags_MouseButtonLeft);

            if (pressed)
            {
                IsChecked = !IsChecked;
                Action?.Invoke();
                retValue = true;
            }

            var imgPos = rectStart + (size - Size) * 0.5f;
            var imgEnd = imgPos + Size;
            if(IsChecked)
            {
                if(mCheckedUVAnimTask != null)
                {
                    mCheckedUVAnimTask.Value.Result.OnDraw(in drawList, in imgPos, in imgEnd, 0);
                }
            }
            else
            {
                if(mNormalUVAnimTask != null)
                {
                    mNormalUVAnimTask.Value.Result.OnDraw(in drawList, in imgPos, in imgEnd, 0);
                }
            }

            return retValue;
        }
    }

    public class CustomButton
    {
        public unsafe static bool ToolButton(string label, in Vector2 size_arg, string idStr = null)
        {
            var rectStart = ImGuiAPI.GetCursorScreenPos();
            var label_size = ImGuiAPI.CalcTextSize(label, false, 0);
            uint id;
            if (string.IsNullOrEmpty(idStr))
                id = ImGuiAPI.GetID("#Button" + label);
            else
                id = ImGuiAPI.GetID(idStr);
            var style = ImGuiAPI.GetStyle();
            Vector2 size = size_arg;
            size = ImGuiAPI.CalcItemSize(ref size, label_size.X + style->FramePadding.X * 2.0f, label_size.Y + style->FramePadding.Y * 2.0f);
            var rectEnd = rectStart + size;

            ImGuiAPI.ItemSize(in size, 0);
            if (!ImGuiAPI.ItemAdd(in rectStart, in rectEnd, id, 0))
                return false;

            bool hovered = false, held = false;
            var pressed = ImGuiAPI.ButtonBehavior(in rectStart, in rectEnd, id, ref hovered, ref held, ImGuiButtonFlags_.ImGuiButtonFlags_MouseButtonLeft);
            var color = StyleConfig.Instance.ToolButtonTextColor;
            if (pressed)
                color = StyleConfig.Instance.ToolButtonTextColor_Press;
            else if (hovered)
                color = StyleConfig.Instance.ToolButtonTextColor_Hover;

            var drawList = ImGuiAPI.GetWindowDrawList();
            var pos = rectStart + (size - label_size) * 0.5f;
            drawList.AddText(in pos, color, label, null);

            return pressed;
        }
        public unsafe static bool ToolButton(string label, in Vector2 size_arg, uint hightLightColor, string idStr = null)
        {
            var rectStart = ImGuiAPI.GetCursorScreenPos();
            var label_size = ImGuiAPI.CalcTextSize(label, false, 0);
            uint id;
            if (string.IsNullOrEmpty(idStr))
                id = ImGuiAPI.GetID("#Button" + label);
            else
                id = ImGuiAPI.GetID(idStr);
            var style = ImGuiAPI.GetStyle();
            Vector2 size = size_arg;
            size = ImGuiAPI.CalcItemSize(ref size, label_size.X + style->FramePadding.X * 2.0f, label_size.Y + style->FramePadding.Y * 2.0f);
            var rectEnd = rectStart + size;

            ImGuiAPI.ItemSize(in size, 0);
            if (!ImGuiAPI.ItemAdd(in rectStart, in rectEnd, id, 0))
                return false;

            bool hovered = false, held = false;
            var pressed = ImGuiAPI.ButtonBehavior(in rectStart, in rectEnd, id, ref hovered, ref held, ImGuiButtonFlags_.ImGuiButtonFlags_MouseButtonLeft);
            var color = StyleConfig.Instance.ToolButtonTextColor;
            if (pressed)
                color = hightLightColor;
            else if (hovered)
                color = hightLightColor;

            var drawList = ImGuiAPI.GetWindowDrawList();
            var pos = rectStart + (size - label_size) * 0.5f;
            drawList.AddText(in pos, color, label, null);

            return pressed;
        }
        public unsafe static bool ToolButton(
            string label, 
            in Vector2 size_arg, 
            uint textColor,
            uint textPressedColor,
            uint textHoveredColor,
            uint backgroundColor, 
            uint backgroundPressedColor, 
            uint backgroundHoveredColor,
            float backgroundRounding = 5,
            ImDrawFlags_ backgroundDrawFlags = ImDrawFlags_.ImDrawFlags_None,
            string idStr = null)
        {
            var rectStart = ImGuiAPI.GetCursorScreenPos();
            var label_size = ImGuiAPI.CalcTextSize(label, false, 0);
            uint id;
            if (string.IsNullOrEmpty(idStr))
                id = ImGuiAPI.GetID("#Button" + label);
            else
                id = ImGuiAPI.GetID(idStr);
            var style = ImGuiAPI.GetStyle();
            Vector2 size = size_arg;
            size = ImGuiAPI.CalcItemSize(ref size, label_size.X + style->FramePadding.X * 2.0f, label_size.Y + style->FramePadding.Y * 2.0f);
            var rectEnd = rectStart + size;

            ImGuiAPI.ItemSize(in size, 0);
            if (!ImGuiAPI.ItemAdd(in rectStart, in rectEnd, id, 0))
                return false;

            bool hovered = false, held = false;
            var pressed = ImGuiAPI.ButtonBehavior(in rectStart, in rectEnd, id, ref hovered, ref held, ImGuiButtonFlags_.ImGuiButtonFlags_MouseButtonLeft);
            var color = textColor;
            var bgColor = backgroundColor;
            if (pressed)
            {
                color = textPressedColor;
                bgColor = backgroundPressedColor;
            }
            else if (hovered)
            {
                color = textHoveredColor;
                bgColor = backgroundHoveredColor;
            }

            var drawList = ImGuiAPI.GetWindowDrawList();
            drawList.AddRectFilled(rectStart, rectEnd, bgColor, backgroundRounding, backgroundDrawFlags);
            var pos = rectStart + (size - label_size) * 0.5f;
            drawList.AddText(in pos, color, label, null);

            return pressed;
        }
        public unsafe static bool ToggleButton(string label, in Vector2 size_arg, ref bool toggle, string idStr = null)
        {
            var rectStart = ImGuiAPI.GetCursorScreenPos();
            var label_size = ImGuiAPI.CalcTextSize(label, false, 0);
            uint id;
            if (string.IsNullOrEmpty(idStr))
                id = ImGuiAPI.GetID("#Button" + label);
            else
                id = ImGuiAPI.GetID(idStr);
            var style = ImGuiAPI.GetStyle();
            Vector2 size = size_arg;
            size = ImGuiAPI.CalcItemSize(ref size, label_size.X + style->FramePadding.X * 2.0f, label_size.Y + style->FramePadding.Y * 2.0f);
            var rectEnd = rectStart + size;

            ImGuiAPI.ItemSize(in size, 0);
            if (!ImGuiAPI.ItemAdd(in rectStart, in rectEnd, id, 0))
                return false;

            bool hovered = false, held = false;
            var pressed = ImGuiAPI.ButtonBehavior(in rectStart, in rectEnd, id, ref hovered, ref held, ImGuiButtonFlags_.ImGuiButtonFlags_MouseButtonLeft);
            if(pressed)
                toggle = !toggle;
            var color = StyleConfig.Instance.ToolButtonTextColor;
            if (toggle)
                color = StyleConfig.Instance.ToolButtonTextColor_Press;
            else if (hovered)
                color = StyleConfig.Instance.ToolButtonTextColor_Hover;

            var drawList = ImGuiAPI.GetWindowDrawList();
            var pos = rectStart + (size - label_size) * 0.5f;
            drawList.AddText(in pos, color, label, null);

            return pressed;
        }
    }
}
