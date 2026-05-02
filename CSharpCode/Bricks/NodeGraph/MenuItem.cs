using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.NodeGraph
{
    /*public class TtMenuItem
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

        public enum eMenuStyle
        {
            TreeList,
            Menu,
        }
        public static void Draw(TtMenuItem item, string filter, ImDrawList cmdList, eMenuStyle style = eMenuStyle.TreeList)
        {

        }
    }*/
}
