using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.EGui.UIProxy
{
    public class MenuItemProxy : IUIProxyBase
    {
        public string MenuName = "Unknow";
        public string Shortcut = null;
        public ImageProxy Icon = null;
        public bool IsTopMenuItem = false;
        public bool Selected = false;
        public bool Enable = true;
        public bool Opened = false;
        public bool Hovered = false;
        public Action Action;

        public List<IUIProxyBase> SubMenus;

        public void Cleanup()
        {
            Icon?.Dispose();
            for(int i=0; i<SubMenus.Count; i++)
            {
                SubMenus[i].Cleanup();
            }
        }
        public async System.Threading.Tasks.Task<bool> Initialize()
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            return true;
        }

        public bool OnDraw(ref ImDrawList drawList)
        {
            bool retValue = false;
            //ImGuiAPI.BeginGroup();
            bool colorPushed = false;
            if (this.Opened)
            {
                ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Text, EGui.UIProxy.StyleConfig.Instance.TextSelectedColor);
                colorPushed = true;
            }
            if (this.Hovered)
                ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Text, EGui.UIProxy.StyleConfig.Instance.TextHoveredColor);
            if (this.IsTopMenuItem)
            {
                ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Header, StyleConfig.Instance.MenuHeaderColor);
                ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FramePadding, ref StyleConfig.Instance.TopMenuFramePadding);
            }
            else
            {
                ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FramePadding, ref StyleConfig.Instance.MenuItemFramePadding);
                ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_ItemSpacing, ref StyleConfig.Instance.MenuItemSpacing);
                ImGuiAPI.Indent(StyleConfig.Instance.MenuItemIndent);
            }
            if(Icon != null)
            {
                Icon.OnDraw(ref drawList);
                var posX = ImGuiAPI.GetCursorPosX();
                posX += Icon.ImageSize.X + StyleConfig.Instance.ItemSpacing.X;
                ImGuiAPI.SetCursorPosX(posX);

            }
            if (this.SubMenus != null && this.SubMenus.Count > 0)
                this.Opened = ImGuiAPI.BeginMenu(this.MenuName, this.Enable);
            else
                this.Opened = ImGuiAPI.MenuItem(this.MenuName, this.Shortcut, this.Selected, this.Enable);
            //ImGuiAPI.EndGroup();
            if(this.IsTopMenuItem)
            {
                ImGuiAPI.PopStyleColor(1);
                ImGuiAPI.PopStyleVar(1);
            }
            else
            {
                ImGuiAPI.PopStyleVar(2);
                ImGuiAPI.Unindent(StyleConfig.Instance.MenuItemIndent);
            }
            if (this.Hovered)
                ImGuiAPI.PopStyleColor(1);
            this.Hovered = ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None);
            if (this.Opened)
            {
                if(!(this.SubMenus != null && this.SubMenus.Count > 0))
                {
                    Action?.Invoke();
                    retValue = true;
                }
                if (colorPushed)
                {
                    ImGuiAPI.PopStyleColor(1);
                    colorPushed = false;
                }

                if (this.SubMenus != null)
                {
                    var subMenuDraweList = ImGuiAPI.GetWindowDrawList();
                    for (int i = 0; i < this.SubMenus.Count; i++)
                    {
                        this.SubMenus[i].OnDraw(ref subMenuDraweList);
                    }
                }

                if(this.SubMenus != null && this.SubMenus.Count > 0)
                    ImGuiAPI.EndMenu();
            }
            else
            {
                if (colorPushed)
                {
                    ImGuiAPI.PopStyleColor(1);
                    colorPushed = false;
                }
            }
            return retValue;
        }
    }

    public class NamedMenuSeparator : IUIProxyBase
    {
        public string Name = "Unknow";
        public float Thickness = 1.0f;

        public void Cleanup()
        {

        }
        public async System.Threading.Tasks.Task<bool> Initialize()
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            return true;
        }

        public bool OnDraw(ref ImDrawList drawList)
        {
            ImGuiAPI.BeginGroup();
            UEngine.Instance.GfxDevice.SlateRenderer.PushFont((int)Slate.UBaseRenderer.enFont.Font_Bold_13px);
            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Text, StyleConfig.Instance.NamedMenuSeparatorColor);
            ImGuiAPI.Text(Name);
            ImGuiAPI.PopStyleColor(1);
            ImGuiAPI.SameLine(0, -1);
            var cursorPos = ImGuiAPI.GetCursorScreenPos();
            var winWidth = ImGuiAPI.GetWindowWidth();
            var itemSize = ImGuiAPI.GetItemRectSize();
            var start = new Vector2(cursorPos.X, cursorPos.Y + itemSize.Y * 0.5f);
            var end = new Vector2(start.X + winWidth - itemSize.X - StyleConfig.Instance.WindowPadding.X * 2 - StyleConfig.Instance.ItemSpacing.X, start.Y);
            drawList.AddLine(ref start, ref end, StyleConfig.Instance.NamedMenuSeparatorColor, Thickness);
            UEngine.Instance.GfxDevice.SlateRenderer.PopFont();
            ImGuiAPI.EndGroup();
            return true;
        }
    }
}
