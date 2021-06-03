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
            get => StyleConfig.ToolbarHeight;
        }

        public void AddToolbarItems(params IToolbarItem[] items)
        {
            ToolbarItems.AddRange(items);
        }
        public void RemoveToolbarItem(IToolbarItem item)
        {
            ToolbarItems.Remove(item);
        }

        public void OnDraw(ref ImDrawList drawList)
        {
            var cursorPos = ImGuiAPI.GetCursorScreenPos();
            var windowWidth = ImGuiAPI.GetWindowWidth();
            var rectMax = cursorPos + new Vector2(windowWidth, EGui.UIProxy.StyleConfig.ToolbarHeight);
            drawList.AddRectFilled(ref cursorPos, ref rectMax, ImGuiAPI.ColorConvertFloat4ToU32(ref EGui.UIProxy.StyleConfig.ToolbarBG), 0.0f, ImDrawFlags_.ImDrawFlags_None);
            float itemOffset = 0;
            float itemSpacing = -1;
            ImGuiAPI.BeginGroup();
            for (int i = 0; i < ToolbarItems.Count; ++i)
            {
                ImGuiAPI.SameLine(itemOffset, itemSpacing);
                //ImGuiAPI.SameLine(0, -1);
                ToolbarItems[i].OnDraw(ref drawList);
                itemOffset = ToolbarItems[i].NextItemOffset;
                itemSpacing = ToolbarItems[i].NextItemSpacing;
            }
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

        bool isMouseDown = false;
        bool isMouseHover = false;

        public void OnDraw(ref ImDrawList drawList)
        {
            ImGuiAPI.BeginGroup(); 
            var cursorScrPos = ImGuiAPI.GetCursorScreenPos();
            var tempScrPos = cursorScrPos;
            var clickDelta = 0.0f;
            if(isMouseDown)
            {
                clickDelta = 2.0f * ImGuiAPI.GetWindowDpiScale();
            }

            Vector2 hitRectMin = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 hitRectMax = new Vector2(float.MinValue, float.MinValue); ;
            if(Icon != null)
            {
                tempScrPos.Y = cursorScrPos.Y + (StyleConfig.ToolbarHeight - Icon.ImageSize.Y) * 0.5f + clickDelta;
                hitRectMin.X = cursorScrPos.X;
                hitRectMin.Y = tempScrPos.Y;
                Icon.OnDraw(ref drawList, ref tempScrPos);

                tempScrPos.X = cursorScrPos.X + Icon.ImageSize.X + StyleConfig.ToolbarButtonIconTextSpacing;
                hitRectMax.X = tempScrPos.X;
                hitRectMax.Y = tempScrPos.Y + Icon.ImageSize.Y;
            }

            var textSize = ImGuiAPI.CalcTextSize(Name, false, -1);
            tempScrPos.Y = cursorScrPos.Y + (StyleConfig.ToolbarHeight - textSize.Y) * 0.5f + clickDelta;
            ImGuiAPI.SetCursorScreenPos(ref tempScrPos);
            hitRectMin.X = System.Math.Min(hitRectMin.X, tempScrPos.X);
            hitRectMin.Y = System.Math.Min(hitRectMin.Y, tempScrPos.Y);
            hitRectMax.X = System.Math.Max(hitRectMax.X, tempScrPos.X + textSize.X);
            hitRectMax.Y = System.Math.Max(hitRectMax.Y, tempScrPos.Y + textSize.Y);
            if(isMouseDown)
            {
                ImGuiAPI.TextColored(ref StyleConfig.ToolbarButtonTextColor_Press, Name);
                if(Icon != null)
                    Icon.Color = ImGuiAPI.ColorConvertFloat4ToU32(ref StyleConfig.ToolbarButtonTextColor_Press);
            }
            else if(isMouseHover)
            {
                ImGuiAPI.TextColored(ref StyleConfig.ToolbarButtonTextColor_Hover, Name);
                if(Icon != null)
                    Icon.Color = ImGuiAPI.ColorConvertFloat4ToU32(ref StyleConfig.ToolbarButtonTextColor_Hover);
            }
            else
            {
                ImGuiAPI.TextColored(ref StyleConfig.ToolbarButtonTextColor, Name);
                if (Icon != null)
                    Icon.Color = ImGuiAPI.ColorConvertFloat4ToU32(ref StyleConfig.ToolbarButtonTextColor);
            }
            ImGuiAPI.EndGroup();
            if (ImGuiAPI.IsMouseDown(ImGuiMouseButton_.ImGuiMouseButton_Left) && isMouseHover)
            {
                isMouseDown = true;
                Action?.Invoke();
            }
            else
                isMouseDown = false;

            if (ImGuiAPI.IsMouseHoveringRect(ref hitRectMin, ref hitRectMax, true))
                isMouseHover = true;
            else
                isMouseHover = false;
        }
    }

    public class ToolbarSeparator : IToolbarItem
    {
        public float NextItemOffset => 0;
        public float NextItemSpacing => StyleConfig.ToolbarSeparatorThickness + StyleConfig.ItemSpacing.X * 2;

        public void OnDraw(ref ImDrawList drawList)
        {
            var cursorScrPos = ImGuiAPI.GetCursorScreenPos();
            cursorScrPos.X += StyleConfig.ToolbarSeparatorThickness * 0.5f;
            var maxPos = cursorScrPos + new Vector2(0, StyleConfig.ToolbarHeight);
            drawList.AddLine(ref cursorScrPos, ref maxPos, StyleConfig.SeparatorColor, StyleConfig.ToolbarSeparatorThickness);
        }
    }
}
