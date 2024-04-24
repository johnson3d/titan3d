using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.EGui.Controls
{
    public class TtMenuItem
    {
        public string Text;
        public string TextForFilter;
        public object UserData;
        public delegate void FMenuAction(TtMenuItem item, object sender);
        public FMenuAction Action;
        public delegate void FOnMenuDraw(TtMenuItem item, object sender);
        public FOnMenuDraw OnMenuDraw;
        public Func<TtMenuItem, object, bool> BeforeMenuDraw;
        public List<TtMenuItem> SubMenuItems { get; } = new List<TtMenuItem>();
        public TtMenuItem Parent = null;
        public bool IsSeparator = false;

        public EGui.UIProxy.MenuItemProxy.MenuState MenuState = new EGui.UIProxy.MenuItemProxy.MenuState();

        public TtMenuItem()
        {
            MenuState.Reset();
        }
        public void SetIsExpanded(bool value, bool withChildren)
        {
            MenuState.Opened = value;
            if(withChildren)
            {
                for(int i=0; i<SubMenuItems.Count; i++)
                {
                    SubMenuItems[i].SetIsExpanded(value, withChildren);
                }
            }
        }

        public TtMenuItem FindMenuItem(string path)
        {
            var segs = path.Split("/");
            return FindMenuItem(segs, 0);
        }
        private TtMenuItem FindMenuItem(string[] path, int curIndex)
        {
            if (path.Length == 0)
                return this;
            var cur = path[curIndex];            
            foreach (var i in SubMenuItems)
            {
                if (cur == i.Text)
                {
                    return i.FindMenuItem(path, ++curIndex);
                }
            }
            return null;
        }
        public TtMenuItem AddMenuSeparator(string text)
        {
            var tmp = new TtMenuItem();
            tmp.Text = text;
            tmp.IsSeparator = true;
            SubMenuItems.Add(tmp);
            return tmp;
        }
        public TtMenuItem InsertMenuItem(int index, string text, object userData, FMenuAction action, Func<TtMenuItem, object, bool> beforeAction = null)
        {
            return InsertMenuItem(index, text, null, userData, action, beforeAction);
        }
        public TtMenuItem InsertMenuItem(int index, string text, string filter, object userData, FMenuAction action, Func<TtMenuItem, object, bool> beforeAction = null)
        {
            if (index < 0)
                return null;
            if(index >= SubMenuItems.Count)
                return AddMenuItem(text, filter, userData, action, beforeAction);
            else
            {
                foreach (var i in SubMenuItems)
                {
                    if (i.Text == text)
                    {
                        i.UserData = userData;
                        i.Action = action;
                        i.BeforeMenuDraw = beforeAction;
                        return i;
                    }
                }
                var tmp = new TtMenuItem();
                tmp.Text = text;
                tmp.TextForFilter = string.IsNullOrEmpty(filter) ? text.ToLower() : filter.ToLower();
                tmp.UserData = userData;
                tmp.Action = action;
                tmp.BeforeMenuDraw = beforeAction;
                tmp.Parent = this;
                SubMenuItems.Insert(index, tmp);
                return tmp;
            }
        }
        public TtMenuItem AddMenuItem(string text, object userData, FMenuAction action, Func<TtMenuItem, object, bool> beforeAction = null)
        {
            return AddMenuItem(text, null, userData, action, beforeAction);
        }
        public TtMenuItem AddMenuItem(string text, string filter, object userData, FMenuAction action, Func<TtMenuItem, object, bool> beforeAction = null)
        {
            foreach (var i in SubMenuItems)
            {
                if (i.Text == text)
                {
                    i.UserData = userData;
                    i.Action = action;
                    i.BeforeMenuDraw = beforeAction;
                    return i;
                }
            }
            var tmp = new TtMenuItem();
            tmp.Text = text;
            tmp.TextForFilter = string.IsNullOrEmpty(filter) ? text.ToLower() : filter.ToLower();
            tmp.UserData = userData;
            tmp.Action = action;
            tmp.BeforeMenuDraw = beforeAction;
            tmp.Parent = this;
            SubMenuItems.Add(tmp);
            return tmp;
        }
        public TtMenuItem AddMenuDraw(string text, object userData, FOnMenuDraw action)
        {
            return AddMenuDraw(text, null, userData, action);
        }
        public TtMenuItem AddMenuDraw(string text, string filter, object userData, FOnMenuDraw action)
        {
            foreach (var i in SubMenuItems)
            {
                if (i.Text == text)
                {
                    i.UserData = userData;
                    i.OnMenuDraw = action;
                    return i;
                }
            }
            var tmp = new TtMenuItem();
            tmp.Text = text;
            tmp.TextForFilter = filter.ToLower();
            tmp.UserData = userData;
            tmp.OnMenuDraw = action;
            tmp.Parent = this;
            SubMenuItems.Add(tmp);
            return tmp;
        }
        public void RemoveMenuItem(string path)
        {
            var item = FindMenuItem(path);
            RemoveMenuItem(item);
        }
        public void RemoveMenuItem(TtMenuItem item)
        {
            if (item == null)
                return;
            if (item.Parent == null)
                return;
            foreach (var i in item.Parent.SubMenuItems)
            {
                if (i.Text == item.Text)
                {
                    item.Parent.SubMenuItems.Remove(i);
                    break;
                }
            }
            if (item.Parent.SubMenuItems.Count == 0)
            {
                RemoveMenuItem(item.Parent);
            }
        }
        string mFilterStore;
        bool mLastCheckResult;
        public bool FilterCheck(string filter)
        {
            if (string.IsNullOrEmpty(filter))
                return true;

            if (mFilterStore == filter)
                return mLastCheckResult;

            var finalFilter = filter.Replace(" ", "");

            bool checkResult;
            if (SubMenuItems.Count > 0)
            {
                bool result = false;
                for(int i=0; i<SubMenuItems.Count; i++)
                {
                    result = result || SubMenuItems[i].FilterCheck(filter);
                }
                checkResult = result;
            }
            else
                checkResult = TextForFilter.Contains(finalFilter);

            mFilterStore = finalFilter;
            mLastCheckResult = checkResult;
            return checkResult;
        }

        public enum EMenuStyle
        {
            TreeList,
            Menu,
        }
        public static void Draw(
            TtMenuItem item, 
            object drawSender,
            object actionSender,
            string filter, 
            in ImDrawList cmdList,
            ref int selectQuickMenuIdx,
            ref int currentQuickMenuIdx,
            Action<TtMenuItem> postActionWhenMenuActive,
            EMenuStyle style = EMenuStyle.TreeList)
        {
            if (!item.FilterCheck(filter))
                return;

            if (item.OnMenuDraw != null)
            {
                item.OnMenuDraw(item, drawSender);
                return;
            }
            if (item.BeforeMenuDraw != null)
            {
                if (item.BeforeMenuDraw(item, drawSender) == false)
                    return;
            }
            if (item.SubMenuItems.Count == 0)
            {
                if (item.IsSeparator)
                {
                    EGui.UIProxy.NamedMenuSeparator.OnDraw(item.Text, in cmdList, EGui.UIProxy.StyleConfig.Instance.NamedMenuSeparatorThickness);
                }
                else if (!string.IsNullOrEmpty(item.Text))
                {
                    var flag = ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Leaf | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_NoTreePushOnOpen;
                    if (selectQuickMenuIdx == currentQuickMenuIdx && !string.IsNullOrEmpty(filter))
                    {
                        flag |= ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Selected;
                        if (ImGuiAPI.IsKeyPressed(ImGuiKey.ImGuiKey_Enter, false))
                        {
                            if (item.Action != null)
                            {
                                item.Action(item, actionSender);
                                ImGuiAPI.CloseCurrentPopup();
                                postActionWhenMenuActive?.Invoke(item);
                            }
                        }
                    }
                    bool clicked = false;
                    switch (style)
                    {
                        case EMenuStyle.TreeList:
                            ImGuiAPI.TreeNodeEx(item.Text, flag);
                            clicked = ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left);
                            break;
                        case EMenuStyle.Menu:
                            clicked = EGui.UIProxy.MenuItemProxy.MenuItem(item.Text, "", false, null, in cmdList, Support.UAnyPointer.Default, ref item.MenuState);
                            break;
                    }
                    if (clicked)
                    {
                        if (item.Action != null)
                        {
                            item.Action(item, actionSender);
                            ImGuiAPI.CloseCurrentPopup();
                            postActionWhenMenuActive?.Invoke(item);
                        }
                    }
                    currentQuickMenuIdx++;
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(filter))
                    item.MenuState.Opened = true;
                switch (style)
                {
                    case EMenuStyle.TreeList:
                        ImGuiAPI.SetNextItemOpen(item.MenuState.Opened, ImGuiCond_.ImGuiCond_None);
                        if (ImGuiAPI.TreeNode(item.Text))
                        {
                            item.MenuState.Opened = true;
                            for (int menuIdx = 0; menuIdx < item.SubMenuItems.Count; menuIdx++)
                            {
                                Draw(item.SubMenuItems[menuIdx], drawSender, actionSender, filter, cmdList, ref selectQuickMenuIdx, ref currentQuickMenuIdx, postActionWhenMenuActive, style);
                            }
                            ImGuiAPI.TreePop();
                        }
                        else
                            item.MenuState.Opened = false;
                        break;
                    case EMenuStyle.Menu:
                        if (EGui.UIProxy.MenuItemProxy.BeginMenuItem(item.Text, "", null, cmdList, Support.UAnyPointer.Default, ref item.MenuState))
                        {
                            for (int menuIdx = 0; menuIdx < item.SubMenuItems.Count; menuIdx++)
                            {
                                Draw(item.SubMenuItems[menuIdx], drawSender, actionSender, filter, cmdList, ref selectQuickMenuIdx, ref currentQuickMenuIdx, postActionWhenMenuActive, style);
                            }
                            EGui.UIProxy.MenuItemProxy.EndMenuItem();
                        }
                        break;
                }

            }
        }
    }
}
