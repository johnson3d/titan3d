using System;
using System.Collections.Generic;
using System.Text;


namespace EngineNS.EGui.UIProxy
{
    using MenuItemAction = Action<MenuItemProxy, Support.UAnyPointer>;

    public class MenuItemProxy : IUIProxyBase
    {
        public static MenuItemProxy FindSubMenu(MenuItemProxy item, string[] nameTree)
        {
            var cur = item;
            for (int i = 0; i < nameTree.Length; i++)
            {
                bool find = false;
                foreach (var j in cur.SubMenus)
                {
                    var t = j as MenuItemProxy;
                    if (t == null)
                        continue;
                    if (t.MenuName == nameTree[i])
                    {
                        cur = t;
                        find = true;
                        break;
                    }
                }
                if (find == false)
                {
                    return null;
                }
            }
            return cur;
        }
        public string MenuName = "Unknow";
        public string Shortcut = null;
        public bool IsTopMenuItem = false;
        public bool Selected = false;
        public ImageProxy Icon = null;
        public bool Visible = true;

        public struct MenuState
        {
            public bool Enable;
            public bool Opened;
            public bool Hovered;
            public bool HasIndent;

            public void Reset()
            {
                Enable = true;
                Opened = false;
                Hovered = false;
                HasIndent = true;
            }
        }
        public MenuState State = new MenuState();
        public MenuItemAction Action;

        public List<IUIProxyBase> SubMenus = new List<IUIProxyBase>();

        public MenuItemProxy()
        {
            State.Reset();
        }

        public void Cleanup()
        {
            Icon?.Dispose();
            CleanupSubMenus();
        }
        public void CleanupSubMenus()
        {
            if (SubMenus == null)
                return;
            for (int i = 0; i < SubMenus.Count; i++)
            {
                SubMenus[i].Cleanup();
            }
        }
        public async System.Threading.Tasks.Task<bool> Initialize()
        {
            await EngineNS.Thread.TtAsyncDummyClass.DummyFunc();
            return true;
        }

        public bool OnDraw(in ImDrawList drawList, in Support.UAnyPointer drawData)
        {
            if (!Visible)
                return false;

            bool retValue = false;
            //ImGuiAPI.BeginGroup();
            bool colorPushed = false;
            if (State.Opened)
            {
                ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Text, EGui.UIProxy.StyleConfig.Instance.TextSelectedColor);
                colorPushed = true;
            }
            if (State.Hovered)
                ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Text, EGui.UIProxy.StyleConfig.Instance.TextHoveredColor);
            if (IsTopMenuItem)
            {
                ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FramePadding, in StyleConfig.Instance.TopMenuFramePadding);
            }
            else
            {
                ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FramePadding, in StyleConfig.Instance.MenuItemFramePadding);
                ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_ItemSpacing, in StyleConfig.Instance.MenuItemSpacing);
                if(State.HasIndent)
                    ImGuiAPI.Indent(StyleConfig.Instance.MenuItemIndent);
            }
            if (Icon != null)
            {
                Icon.OnDraw(in drawList, in drawData);
                var posX = ImGuiAPI.GetCursorPosX();
                posX += Icon.ImageSize.X + StyleConfig.Instance.ItemSpacing.X;
                ImGuiAPI.SetCursorPosX(posX);

            }
                ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Header, StyleConfig.Instance.MenuHeaderColor);
            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_HeaderHovered, UIProxy.StyleConfig.Instance.MenuHeaderColor);
            if (this.SubMenus != null && this.SubMenus.Count > 0)
                State.Opened = ImGuiAPI.BeginMenu(MenuName, State.Enable);
            else
                State.Opened = ImGuiAPI.MenuItem(MenuName, Shortcut, Selected, State.Enable);
            ImGuiAPI.PopStyleColor(2);
            //ImGuiAPI.EndGroup();
            if(IsTopMenuItem)
            {
                ImGuiAPI.PopStyleVar(1);
            }
            else
            {
                ImGuiAPI.PopStyleVar(2);
                if(State.HasIndent)
                    ImGuiAPI.Unindent(StyleConfig.Instance.MenuItemIndent);
            }
            if (State.Hovered)
                ImGuiAPI.PopStyleColor(1);
            State.Hovered = ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None);
            if (State.Opened)
            {
                if(!(this.SubMenus != null && this.SubMenus.Count > 0))
                {
                    Action?.Invoke(this, drawData);
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
                        this.SubMenus[i].OnDraw(in subMenuDraweList, in drawData);
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

        public static bool MenuItem(
            string menuName,
            string shortcut,
            bool selected,
            ImageProxy icon, 
            in ImDrawList drawList, 
            in Support.UAnyPointer drawData,
            ref MenuState state)
        {
            bool colorPushed = false;
            if (state.Opened)
            {
                ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Text, EGui.UIProxy.StyleConfig.Instance.TextSelectedColor);
                colorPushed = true;
            }
            if(state.Hovered)
                ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Text, EGui.UIProxy.StyleConfig.Instance.TextHoveredColor);

            ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FramePadding, in StyleConfig.Instance.MenuItemFramePadding);
            ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_ItemSpacing, in StyleConfig.Instance.MenuItemSpacing);
            if(state.HasIndent)
                ImGuiAPI.Indent(StyleConfig.Instance.MenuItemIndent);

            if(icon != null)
            {
                icon.OnDraw(in drawList, in drawData);
                var posX = ImGuiAPI.GetCursorPosX();
                posX += icon.ImageSize.X + StyleConfig.Instance.ItemSpacing.X;
                ImGuiAPI.SetCursorPosX(posX);
            }

            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_HeaderHovered, UIProxy.StyleConfig.Instance.TVHeaderActive);
            state.Opened = ImGuiAPI.MenuItem(menuName, shortcut, selected, state.Enable);
            ImGuiAPI.PopStyleColor(1);
            ImGuiAPI.PopStyleVar(2);
            if(state.HasIndent)
                ImGuiAPI.Unindent(StyleConfig.Instance.MenuItemIndent);

            if (state.Hovered)
                ImGuiAPI.PopStyleColor(1);
            state.Hovered = ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None);

            bool retValue = false;
            if (state.Opened)
            {
                retValue = true;

                if (colorPushed)
                {
                    ImGuiAPI.PopStyleColor(1);
                    colorPushed = false;
                }

                //if (this.SubMenus != null)
                //{
                //    var subMenuDraweList = ImGuiAPI.GetWindowDrawList();
                //    for (int i = 0; i < this.SubMenus.Count; i++)
                //    {
                //        this.SubMenus[i].OnDraw(ref subMenuDraweList, ref drawData);
                //    }
                //}

                //if (this.SubMenus != null && this.SubMenus.Count > 0)
                //    ImGuiAPI.EndMenu();
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

        public static bool BeginMenuItem(
            string menuName,
            string shortcut,
            ImageProxy icon,
            in ImDrawList drawList,
            in Support.UAnyPointer drawData,
            ref MenuState state,
            bool isTopMenuItem = false
            )
        {
            bool colorPushed = false;
            if (state.Opened)
            {
                ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Text, EGui.UIProxy.StyleConfig.Instance.TextSelectedColor);
                colorPushed = true;
            }
            if(state.Hovered)
                ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Text, EGui.UIProxy.StyleConfig.Instance.TextHoveredColor);
            if (isTopMenuItem)
            {
                ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Header, StyleConfig.Instance.MenuHeaderColor);
                ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FramePadding, in StyleConfig.Instance.TopMenuFramePadding);
            }
            else
            {
                ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FramePadding, in StyleConfig.Instance.MenuItemFramePadding);
                ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_ItemSpacing, in StyleConfig.Instance.MenuItemSpacing);
                if(state.HasIndent)
                    ImGuiAPI.Indent(StyleConfig.Instance.MenuItemIndent);
            }
            if (icon != null)
            {
                icon.OnDraw(in drawList, in drawData);
                var posX = ImGuiAPI.GetCursorPosX();
                posX += icon.ImageSize.X + StyleConfig.Instance.ItemSpacing.X;
                ImGuiAPI.SetCursorPosX(posX);
            }
            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_HeaderHovered, UIProxy.StyleConfig.Instance.TVHeaderActive);
            state.Opened = ImGuiAPI.BeginMenu(menuName, state.Enable);
            ImGuiAPI.PopStyleColor(1);
            if(isTopMenuItem)
            {
                ImGuiAPI.PopStyleColor(1);
                ImGuiAPI.PopStyleVar(1);
            }
            else
            {
                ImGuiAPI.PopStyleVar(2);
                if(state.HasIndent)
                    ImGuiAPI.Unindent(StyleConfig.Instance.MenuItemIndent);
            }
            if (state.Hovered)
                ImGuiAPI.PopStyleColor(1);
            state.Hovered = ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None);

            if(colorPushed)
            {
                ImGuiAPI.PopStyleColor(1);
                colorPushed = false;
            }

            return state.Opened;
        }
        public static void EndMenuItem()
        {
            ImGuiAPI.EndMenu();
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
            await EngineNS.Thread.TtAsyncDummyClass.DummyFunc();
            return true;
        }

        public bool OnDraw(in ImDrawList drawList, in Support.UAnyPointer drawData)
        {
            return OnDraw(Name, drawList, Thickness);
        }

        public static bool OnDraw(string name, in ImDrawList drawList, float thickness)
        {
            ImGuiAPI.BeginGroup();
            UEngine.Instance.GfxDevice.SlateRenderer.PushFont((int)Slate.UBaseRenderer.enFont.Font_Bold_13px);
            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Text, StyleConfig.Instance.NamedMenuSeparatorColor);
            ImGuiAPI.Text(name);
            ImGuiAPI.PopStyleColor(1);
            ImGuiAPI.SameLine(0, -1);
            var cursorPos = ImGuiAPI.GetCursorScreenPos();
            var winWidth = ImGuiAPI.GetWindowWidth();
            var itemSize = ImGuiAPI.GetItemRectSize();
            var start = new Vector2(cursorPos.X, cursorPos.Y + itemSize.Y * 0.5f);
            var end = new Vector2(start.X + winWidth - itemSize.X - StyleConfig.Instance.WindowPadding.X * 2 - StyleConfig.Instance.ItemSpacing.X, start.Y);
            drawList.AddLine(in start, in end, StyleConfig.Instance.NamedMenuSeparatorColor, thickness);
            UEngine.Instance.GfxDevice.SlateRenderer.PopFont();
            ImGuiAPI.EndGroup();
            return true;
        }
    }
}
