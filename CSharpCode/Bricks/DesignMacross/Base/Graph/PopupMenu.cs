using EngineNS.Bricks.CodeBuilder;
using EngineNS.Bricks.NodeGraph;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.Rtti;
using SixLabors.Fonts;
using System;
using System.Collections.Generic;
using System.Text;
using static EngineNS.Bricks.NodeGraph.UMenuItem;

namespace EngineNS.DesignMacross.Base.Graph
{
    public class TtPopupMenu
    {
        public string StringId;
        public UMenuItem Menu = new UMenuItem();
        TtPopupMenuRender PopupMenuRender = new TtPopupMenuRender();
        public Vector2 PopedPosition { get; set; } = Vector2.Zero;
        public TtPopupMenu(string id)
        {
            StringId = id;
        }
        public void OpenPopup()
        {
            PopupMenuRender.OpenPopup(this);
        }
        public void Draw(ref FGraphElementRenderingContext context)
        {
            PopupMenuRender.Draw(this, context);
        }
        public void Reset()
        {
            Menu.SubMenuItems.Clear();
            //for now just new a new one
            // menuItem could be a struct 
            Menu = new UMenuItem();
        }
    }

    public class TtPopupMenuRender
    {
        public void OpenPopup(TtPopupMenu popupMenu)
        {
            ImGuiAPI.OpenPopup(popupMenu.StringId, ImGuiPopupFlags_.ImGuiPopupFlags_None);
        }
        int mSelectQuickMenuIdx = 0;
        int mCurrentQuickMenuIdx = 0;
        bool mCanvasMenuFilterFocused = false;
        public string CanvasMenuFilterStr = "";
        public void Draw(TtPopupMenu popupMenu, FGraphElementRenderingContext context)
        {
            if (ImGuiAPI.BeginPopup(popupMenu.StringId,
                ImGuiWindowFlags_.ImGuiWindowFlags_AlwaysAutoResize |
                ImGuiWindowFlags_.ImGuiWindowFlags_NoTitleBar |
                ImGuiWindowFlags_.ImGuiWindowFlags_NoSavedSettings |
                ImGuiWindowFlags_.ImGuiWindowFlags_NoScrollbar |
                ImGuiWindowFlags_.ImGuiWindowFlags_NoNav))
            {
                {
                    popupMenu.PopedPosition = context.ViewPortInverseTransform(ImGuiAPI.GetMousePos());
                    var size = ImGuiAPI.GetWindowSize();


                    var width = ImGuiAPI.GetWindowContentRegionWidth();//ImGuiAPI.GetWindowWidth();
                    var drawList = ImGuiAPI.GetWindowDrawList();

                    EGui.UIProxy.SearchBarProxy.OnDraw(ref mCanvasMenuFilterFocused, in drawList, "search item", ref CanvasMenuFilterStr, width);
                    Vector2 wsize = new Vector2(200, 400);
                    var id = ImGuiAPI.GetID(popupMenu.StringId);
                    if (ImGuiAPI.BeginChild(id, wsize, false,
                        ImGuiWindowFlags_.ImGuiWindowFlags_NoTitleBar |
                        ImGuiWindowFlags_.ImGuiWindowFlags_NoSavedSettings))
                    {
                        var cmdList = ImGuiAPI.GetWindowDrawList();
                        for (var childIdx = 0; childIdx < popupMenu.Menu.SubMenuItems.Count; childIdx++)
                            DrawMenu(popupMenu, popupMenu.Menu.SubMenuItems[childIdx], CanvasMenuFilterStr.ToLower(), cmdList);
                    }
                    ImGuiAPI.EndChild();
                }
                ImGuiAPI.EndPopup();
            }
        }
        enum eMenuStyle
        {
            TreeList,
            Menu,
        }
        void DrawMenu(TtPopupMenu popupMenu, UMenuItem item, string filter, ImDrawList cmdList, eMenuStyle style = eMenuStyle.TreeList)
        {
            if (!item.FilterCheck(filter))
                return;

            if (item.OnMenuDraw != null)
            {
                item.OnMenuDraw(item, this);
                return;
            }
            if (item.BeforeMenuDraw != null)
            {
                if (item.BeforeMenuDraw(item, this) == false)
                    return;
            }
            if (item.SubMenuItems.Count == 0)
            {
                if (item.IsSeparator)
                {
                    EGui.UIProxy.NamedMenuSeparator.OnDraw(item.Text, cmdList, EGui.UIProxy.StyleConfig.Instance.NamedMenuSeparatorThickness);
                }
                else if (!string.IsNullOrEmpty(item.Text))
                {
                    var flag = ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Leaf | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_NoTreePushOnOpen;
                    if (mSelectQuickMenuIdx == mCurrentQuickMenuIdx && !string.IsNullOrEmpty(filter))
                    {
                        flag |= ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Selected;
                        if (ImGuiAPI.IsKeyPressed(ImGuiKey.ImGuiKey_Enter, false))
                        {
                            if (item.Action != null)
                            {
                                item.Action(item, this);
                                ImGuiAPI.CloseCurrentPopup();
                            }
                        }
                    }
                    bool clicked = false;
                    switch (style)
                    {
                        case eMenuStyle.TreeList:
                            ImGuiAPI.TreeNodeEx(item.Text, flag);
                            clicked = ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left);
                            break;
                        case eMenuStyle.Menu:
                            clicked = EGui.UIProxy.MenuItemProxy.MenuItem(item.Text, "", false, null, cmdList, Support.UAnyPointer.Default, ref item.MenuState);
                            break;
                    }
                    if (clicked)
                    {
                        if (item.Action != null)
                        {
                            item.Action(item, popupMenu);
                            ImGuiAPI.CloseCurrentPopup();
                        }
                    }
                    mCurrentQuickMenuIdx++;
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(filter))
                    item.MenuState.Opened = true;
                switch (style)
                {
                    case eMenuStyle.TreeList:
                        ImGuiAPI.SetNextItemOpen(item.MenuState.Opened, ImGuiCond_.ImGuiCond_None);
                        if (ImGuiAPI.TreeNode(item.Text))
                        {
                            item.MenuState.Opened = true;
                            for (int menuIdx = 0; menuIdx < item.SubMenuItems.Count; menuIdx++)
                            {
                                DrawMenu(popupMenu, item.SubMenuItems[menuIdx], filter, cmdList, style);
                            }
                            ImGuiAPI.TreePop();
                        }
                        else
                        {
                            item.MenuState.Opened = false;
                        }
                        break;
                    case eMenuStyle.Menu:
                        if (EGui.UIProxy.MenuItemProxy.BeginMenuItem(item.Text, "", null, cmdList, Support.UAnyPointer.Default, ref item.MenuState))
                        {
                            for (int menuIdx = 0; menuIdx < item.SubMenuItems.Count; menuIdx++)
                            {
                                DrawMenu(popupMenu, item.SubMenuItems[menuIdx], filter, cmdList, style);
                            }
                            EGui.UIProxy.MenuItemProxy.EndMenuItem();
                        }
                        break;
                }

            }
        }
    }

    public class TtMenuUtil
    {

        public static void ConstructMenuItem(UMenuItem menuToAdded, UTypeDesc typeDesc, string[] menuPaths, string filterStrings, FMenuAction action, Func<UMenuItem, object, bool> beforeAction = null)
        {
            var parentMenu = menuToAdded;
            for (var menuIdx = 0; menuIdx < menuPaths.Length; menuIdx++)
            {
                var menuStr = menuPaths[menuIdx];
                var menuName = GetMenuName(menuStr);
                if (menuIdx < menuPaths.Length - 1)
                    parentMenu = parentMenu.AddMenuItem(menuName, null, null);
                else
                {
                    parentMenu.AddMenuItem(menuName, filterStrings, null, action, beforeAction);
                }
            }

        }
        protected static string GetMenuName(in string menuString)
        {
            var idx = menuString.IndexOf('@');
            if (idx >= 0)
            {
                var idxEnd = menuString.IndexOf('@', idx + 1);
                return menuString.Remove(idx, idxEnd - idx + 1);
            }
            return menuString;
        }
    }
}
