﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.EGui.UIProxy
{
    public class ComboBox : IUIProxyBase
    {
        public string Name;
        public string PreviewValue;
        public ImGuiComboFlags_ Flags = ImGuiComboFlags_.ImGuiComboFlags_None;
        public ImGuiWindowFlags_ WinFlags = 
            ImGuiWindowFlags_.ImGuiWindowFlags_AlwaysAutoResize |
            ImGuiWindowFlags_.ImGuiWindowFlags_Popup |
            ImGuiWindowFlags_.ImGuiWindowFlags_NoTitleBar |
            ImGuiWindowFlags_.ImGuiWindowFlags_NoResize |
            ImGuiWindowFlags_.ImGuiWindowFlags_NoSavedSettings |
            ImGuiWindowFlags_.ImGuiWindowFlags_NoMove;
        public float Width = -1;

        public delegate void Delegate_ComboOpenAction(in Support.TtAnyPointer data);
        public Delegate_ComboOpenAction ComboOpenAction;

        //ImageProxy mImage;
        public void Cleanup()
        {
            //mImage?.Cleanup();
            //mImage = null;
        }
        public async Task<bool> Initialize()
        {
            //mImage = new ImageProxy()
            //{
            //    ImageFile = RName.GetRName("icons/icons.srv", RName.ERNameType.Engine),
            //    ImageSize = new Vector2(16, 16),
            //    UVMin = new Vector2(543.0f / 1024, 3.0f / 1024),
            //    UVMax = new Vector2(559.0f / 1024, 19.0f / 1024),
            //};
            //return await mImage.Initialize();
            await Thread.TtAsyncDummyClass.DummyFunc();
            return true;
        }
        public unsafe bool OnDraw(in ImDrawList drawList, in Support.TtAnyPointer drawData)
        {
            //if (mImage == null)
            //    return false;
            var style = ImGuiAPI.GetStyle();
            ImGuiAPI.SetNextItemWidth(Width);

            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Header, EGui.UIProxy.StyleConfig.Instance.PopupColor);
            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_PopupBg, EGui.UIProxy.StyleConfig.Instance.PopupColor);
            ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_WindowPadding, in EGui.UIProxy.StyleConfig.Instance.PopupWindowsPadding);
            ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_ItemSpacing, in EGui.UIProxy.StyleConfig.Instance.PopupItemSpacing);
            ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_PopupBorderSize, EGui.UIProxy.StyleConfig.Instance.PopupBordersize);

            var cursorPos = ImGuiAPI.GetCursorScreenPos();
            var endPos = cursorPos;
            if(!string.IsNullOrEmpty(Name))
            {
                var nameSize = ImGuiAPI.CalcTextSize(Name, true, -1);
                endPos = cursorPos + new Vector2((Width < 0 ? 0 : Width) + ((nameSize.X > 0.0f) ? (style->ItemInnerSpacing.X + nameSize.X) : 0.0f), nameSize.Y + style->FramePadding.Y * 2.0f);
            }
            var hovered = ImGuiAPI.IsMouseHoveringRectInCurrentWindow(in cursorPos, in endPos, true);
            if (hovered)
                ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Border, EGui.UIProxy.StyleConfig.Instance.PGItemBorderHoveredColor);
            var comboOpen = ImGuiAPI.BeginCombo(Name, PreviewValue, Flags, WinFlags);
            //var comboOpen = ImGuiAPI.BeginCombo(Name, PreviewValue, Flags);
            if(hovered)
                ImGuiAPI.PopStyleColor(1);
            //var itemSize = ImGuiAPI.GetItemRectSize();
            //var pos = cursorPos + new Vector2(itemSize.X - mImage.ImageSize.X - style->FramePadding.X, style->FramePadding.Y * 0.5f);// cursorPos + new Vector2(0, fontSize * 0.134f * 0.5f);// + new Vector2(itemRectMax.X - mImage.ImageSize.X, fontSize * 0.134f * 0.5f);
            //mImage.OnDraw(ref drawList, ref pos);

            if(comboOpen)
            {
                ComboOpenAction?.Invoke(in drawData);
                ImGuiAPI.EndCombo();
            }

            ImGuiAPI.PopStyleColor(2);
            ImGuiAPI.PopStyleVar(3);

            return true;
        }

        public static unsafe bool BeginCombo(string name, string previewValue, float width = -1, ImGuiComboFlags_ flags = ImGuiComboFlags_.ImGuiComboFlags_None,
            ImGuiWindowFlags_ winFlags =
            ImGuiWindowFlags_.ImGuiWindowFlags_AlwaysAutoResize |
            ImGuiWindowFlags_.ImGuiWindowFlags_Popup |
            ImGuiWindowFlags_.ImGuiWindowFlags_NoTitleBar |
            ImGuiWindowFlags_.ImGuiWindowFlags_NoResize |
            ImGuiWindowFlags_.ImGuiWindowFlags_NoSavedSettings |
            ImGuiWindowFlags_.ImGuiWindowFlags_NoMove)
        {
            var style = ImGuiAPI.GetStyle();
            ImGuiAPI.SetNextItemWidth(width);

            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Header, EGui.UIProxy.StyleConfig.Instance.PopupColor);
            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_PopupBg, EGui.UIProxy.StyleConfig.Instance.PopupColor);
            ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_WindowPadding, in EGui.UIProxy.StyleConfig.Instance.PopupWindowsPadding);
            ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_ItemSpacing, in EGui.UIProxy.StyleConfig.Instance.PopupItemSpacing);
            ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_PopupBorderSize, EGui.UIProxy.StyleConfig.Instance.PopupBordersize);

            var cursorPos = ImGuiAPI.GetCursorScreenPos();
            var endPos = cursorPos;
            if (!string.IsNullOrEmpty(name))
            {
                var nameSize = ImGuiAPI.CalcTextSize(name, true, -1);
                endPos = cursorPos + new Vector2((width < 0 ? 0 : width) + ((nameSize.X > 0.0f) ? (style->ItemInnerSpacing.X + nameSize.X) : 0.0f), nameSize.Y + style->FramePadding.Y * 2.0f);
            }
            var hovered = ImGuiAPI.IsMouseHoveringRectInCurrentWindow(in cursorPos, in endPos, true);
            if (hovered)
                ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Border, EGui.UIProxy.StyleConfig.Instance.PGItemBorderHoveredColor);
            var comboOpen = ImGuiAPI.BeginCombo(name, previewValue, flags, winFlags);
            //var comboOpen = ImGuiAPI.BeginCombo(name, previewValue, flags);
            if (hovered)
                ImGuiAPI.PopStyleColor(1);

            ImGuiAPI.PopStyleColor(2);
            ImGuiAPI.PopStyleVar(3);

            return comboOpen;
        }
        public static unsafe void EndCombo()
        {
            ImGuiAPI.EndCombo();
        }
    }
}
