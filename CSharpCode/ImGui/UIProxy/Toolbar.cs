using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.EGui.UIProxy
{
    public interface IToolbarItem : IUIProxyBase
    {
        public float NextItemOffset { get; }
        public float NextItemSpacing { get; }
    }

    public class Toolbar : IUIProxyBase
    {
        public List<IToolbarItem> ToolbarItems = new List<IToolbarItem>();
        public float ToolbarHeight
        {
            get => StyleConfig.Instance.ToolbarHeight;
        }

        public void Cleanup()
        {
            for(int i=0; i<ToolbarItems.Count; i++)
            {
                ToolbarItems[i].Cleanup();
            }
        }
        public async System.Threading.Tasks.Task<bool> Initialize()
        {
            await EngineNS.Thread.TtAsyncDummyClass.DummyFunc();
            return true;
        }

        public void AddToolbarItems(params IToolbarItem[] items)
        {
            ToolbarItems.AddRange(items);
        }
        public void RemoveToolbarItem(IToolbarItem item)
        {
            ToolbarItems.Remove(item);
        }

        public bool OnDraw(in ImDrawList drawList, in Support.TtAnyPointer drawData)
        {
            var cursorPos = ImGuiAPI.GetCursorScreenPos();
            var windowWidth = ImGuiAPI.GetWindowWidth();
            var rectMax = cursorPos + new Vector2(windowWidth, EGui.UIProxy.StyleConfig.Instance.ToolbarHeight);
            drawList.AddRectFilled(in cursorPos, in rectMax, EGui.UIProxy.StyleConfig.Instance.ToolbarBG, 0.0f, ImDrawFlags_.ImDrawFlags_None);
            //float itemOffset = 0;
            //float itemSpacing = -1;
            ImGuiAPI.BeginGroup();
            for (int i = 0; i < ToolbarItems.Count; ++i)
            {
                //ImGuiAPI.SameLine(itemOffset, itemSpacing);
                //ImGuiAPI.SameLine(0, -1);
                ToolbarItems[i].OnDraw(in drawList, in drawData);
                //itemOffset = ToolbarItems[i].NextItemOffset;
                //itemSpacing = ToolbarItems[i].NextItemSpacing;
            }
            ImGuiAPI.SetCursorScreenPos(in rectMax);
            ImGuiAPI.EndGroup();
            return true;
        }

        public static Vector2 RectMax;
        public static unsafe void BeginToolbar(in ImDrawList drawList, bool drawSplitline = true)
        {
            var style = ImGuiAPI.GetStyle();
            var cursorPos = ImGuiAPI.GetCursorScreenPos();
            var windowWidth = ImGuiAPI.GetWindowContentRegionWidth();
            RectMax = cursorPos + new Vector2(windowWidth, EGui.UIProxy.StyleConfig.Instance.ToolbarHeight);
            drawList.AddRectFilled(in cursorPos, in RectMax, EGui.UIProxy.StyleConfig.Instance.ToolbarBG, 0.0f, ImDrawFlags_.ImDrawFlags_None);
            if (drawSplitline)
            {
                var thickness = 1;
                drawList.AddLine(new Vector2(cursorPos.X, RectMax.Y - thickness), new Vector2(RectMax.X, RectMax.Y - thickness), EGui.UIProxy.StyleConfig.Instance.WindowBackground, thickness);
            }
            ImGuiAPI.BeginGroup();
        }
        public static void EndToolbar()
        {
            ImGuiAPI.SetCursorScreenPos(in RectMax);
            ImGuiAPI.EndGroup();
        }
    }

    public class ToolbarIconButtonProxy : IToolbarItem
    {
        public string Name = "Unkonw";
        public ImageProxy Icon = null;
        public Action Action = null;

        public float NextItemOffset => 0;
        public float NextItemSpacing => -1;

        public bool IsMouseDown = false;
        public bool IsMouseHover = false;
        public bool IsDisable = false;

        public void Cleanup()
        {
            Icon?.Dispose();
            Icon = null;
        }
        public async System.Threading.Tasks.Task<bool> Initialize()
        {
            await EngineNS.Thread.TtAsyncDummyClass.DummyFunc();
            return true;
        }

        public bool OnDraw(in ImDrawList drawList, in Support.TtAnyPointer drawData)
        {
            if(DrawButton(drawList, ref IsMouseDown, ref IsMouseHover, Icon, Name, IsDisable, NextItemOffset, NextItemSpacing))
            {
                Action?.Invoke();
                return true;
            }
            return false;
        }
        public unsafe static bool DrawButton(in ImDrawList drawList, 
                                  ref bool isMouseDown, 
                                  ref bool isMouseHover,
                                  ImageProxy icon,
                                  string name,
                                  bool disable = false,
                                  float parentHeight = -1,
                                  float itemOffset = 0,
                                  float itemSpacing = -1
            )
        {
            ImGuiAPI.SameLine(itemOffset, itemSpacing);

            if (parentHeight < 0)
                parentHeight = StyleConfig.Instance.ToolbarHeight;

            bool retValue = false;
            var cursorScrPos = ImGuiAPI.GetCursorScreenPos();
            ImGuiAPI.BeginGroup(); 
            var tempScrPos = cursorScrPos;
            var clickDelta = 0.0f;
            if(isMouseDown)
            {
                clickDelta = 2.0f * ImGuiAPI.GetWindowDpiScale();
            }

            Vector2 hitRectMin = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 hitRectMax = new Vector2(float.MinValue, float.MinValue);
            if(icon != null)
            {
                tempScrPos.Y = cursorScrPos.Y + (parentHeight - icon.ImageSize.Y) * 0.5f + clickDelta;
                hitRectMin.X = cursorScrPos.X;
                hitRectMin.Y = tempScrPos.Y;
                icon.OnDraw(in drawList, in tempScrPos);

                tempScrPos.X = cursorScrPos.X + icon.ImageSize.X + StyleConfig.Instance.ToolbarButtonIconTextSpacing;
                hitRectMax.X = tempScrPos.X;
                hitRectMax.Y = tempScrPos.Y + icon.ImageSize.Y;
            }

            if(!string.IsNullOrEmpty(name))
            {
                var textSize = ImGuiAPI.CalcTextSize(name, false, -1);
                tempScrPos.Y = cursorScrPos.Y + (parentHeight - textSize.Y) * 0.5f + clickDelta;
                hitRectMin.X = System.Math.Min(hitRectMin.X, tempScrPos.X);
                hitRectMin.Y = System.Math.Min(hitRectMin.Y, tempScrPos.Y);
                hitRectMax.X = System.Math.Max(hitRectMax.X, tempScrPos.X + textSize.X);
                hitRectMax.Y = System.Math.Max(hitRectMax.Y, tempScrPos.Y + textSize.Y);
            }
            ImGuiAPI.ItemAdd(cursorScrPos, tempScrPos, 0, (int)ImGuiItemFlags_.ImGuiItemFlags_None);
            ImGuiAPI.SetCursorScreenPos(in tempScrPos);
            Vector4 color = ImGuiAPI.ColorConvertU32ToFloat4(StyleConfig.Instance.ToolbarButtonTextColor);
            if(disable)
                color = ImGuiAPI.ColorConvertU32ToFloat4(StyleConfig.Instance.ToolbarButtonTextColor_Disable);
            else if (isMouseDown)
                color = ImGuiAPI.ColorConvertU32ToFloat4(StyleConfig.Instance.ToolbarButtonTextColor_Press);
            else if(isMouseHover)
                color = ImGuiAPI.ColorConvertU32ToFloat4(StyleConfig.Instance.ToolbarButtonTextColor_Hover);

            if (!string.IsNullOrEmpty(name))
            {
                ImGuiAPI.TextColored(in color, name);
            }
            if (icon != null)
                icon.Color = ImGuiAPI.ColorConvertFloat4ToU32(in color);

            ImGuiAPI.EndGroup();
            if (ImGuiAPI.IsMouseClicked(ImGuiMouseButton_.ImGuiMouseButton_Left, false) && isMouseHover)
            {
                retValue = true;
            }
            isMouseHover = ImGuiAPI.IsMouseHoveringRect(in hitRectMin, in hitRectMax, true);
            isMouseDown = ImGuiAPI.IsMouseDown(ImGuiMouseButton_.ImGuiMouseButton_Left) && isMouseHover;

            return retValue && !disable;
        }
        public static bool DrawCheckBox(in ImDrawList drawList,
                                  ImageProxy icon,
                                  string name, ref bool value, bool readOnly = false,
                                  bool disable = false,
                                  float parentHeight = -1,
                                  float itemOffset = 0,
                                  float itemSpacing = -1)
        {
            ImGuiAPI.SameLine(itemOffset, itemSpacing);

            if (parentHeight < 0)
                parentHeight = StyleConfig.Instance.ToolbarHeight;

            bool retValue = false;
            var cursorScrPos = ImGuiAPI.GetCursorScreenPos();
            ImGuiAPI.BeginGroup();
            var tempScrPos = cursorScrPos;
            var clickDelta = 0.0f;
            
            Vector2 hitRectMin = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 hitRectMax = new Vector2(float.MinValue, float.MinValue);
            if (icon != null)
            {
                tempScrPos.Y = cursorScrPos.Y + (parentHeight - icon.ImageSize.Y) * 0.5f + clickDelta;
                hitRectMin.X = cursorScrPos.X;
                hitRectMin.Y = tempScrPos.Y;
                icon.OnDraw(in drawList, in tempScrPos);

                tempScrPos.X = cursorScrPos.X + icon.ImageSize.X + StyleConfig.Instance.ToolbarButtonIconTextSpacing;
                hitRectMax.X = tempScrPos.X;
                hitRectMax.Y = tempScrPos.Y + icon.ImageSize.Y;
            }

            if (!string.IsNullOrEmpty(name))
            {
                var textSize = ImGuiAPI.CalcTextSize(name, false, -1);
                tempScrPos.Y = cursorScrPos.Y + (parentHeight - textSize.Y) * 0.5f + clickDelta;
                hitRectMin.X = System.Math.Min(hitRectMin.X, tempScrPos.X);
                hitRectMin.Y = System.Math.Min(hitRectMin.Y, tempScrPos.Y);
                hitRectMax.X = System.Math.Max(hitRectMax.X, tempScrPos.X + textSize.X);
                hitRectMax.Y = System.Math.Max(hitRectMax.Y, tempScrPos.Y + textSize.Y);
            }
            ImGuiAPI.SetCursorScreenPos(in tempScrPos);

            Vector4 color = ImGuiAPI.ColorConvertU32ToFloat4(StyleConfig.Instance.ToolbarButtonTextColor);

            retValue = EGui.UIProxy.CheckBox.DrawCheckBox(name, ref value, readOnly);

            if (icon != null)
                icon.Color = ImGuiAPI.ColorConvertFloat4ToU32(in color);

            ImGuiAPI.EndGroup();
            
            return retValue && !disable;
        }
    }

    public class ToolbarSeparator : IToolbarItem
    {
        public float NextItemOffset => 0;
        public float NextItemSpacing => StyleConfig.Instance.ToolbarSeparatorThickness + StyleConfig.Instance.ItemSpacing.X * 2;

        public void Cleanup() { }
        public async System.Threading.Tasks.Task<bool> Initialize()
        {
            await EngineNS.Thread.TtAsyncDummyClass.DummyFunc();
            return true;
        }

        public bool OnDraw(in ImDrawList drawList, in Support.TtAnyPointer drawData)
        {
            return DrawSeparator(drawList, drawData, NextItemOffset, NextItemSpacing);
        }
        public static bool DrawSeparator(in ImDrawList drawList, float itemOffset = 0, float itemSpacing = -1)
        {
            return DrawSeparator(in drawList, in Support.TtAnyPointer.Default, itemOffset, itemSpacing);
        }
        public static bool DrawSeparator(in ImDrawList drawList, in Support.TtAnyPointer drawData, float itemOffset = 0, float itemSpacing = -1)
        {
            ImGuiAPI.SameLine(itemOffset, itemSpacing);

            var cursorScrPos = ImGuiAPI.GetCursorScreenPos();
            cursorScrPos.X += StyleConfig.Instance.ToolbarSeparatorThickness * 0.5f;
            var maxPos = cursorScrPos + new Vector2(0, StyleConfig.Instance.ToolbarHeight);
            drawList.AddLine(in cursorScrPos, in maxPos, StyleConfig.Instance.SeparatorColor, StyleConfig.Instance.ToolbarSeparatorThickness);
            return true;
        }
    }
}
