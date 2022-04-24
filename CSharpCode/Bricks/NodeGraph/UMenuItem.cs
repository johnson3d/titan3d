using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.NodeGraph
{
    public class UMenuItem
    {
        public string Text;
        public object UserData;
        public delegate void FMenuAction(UMenuItem item, object sender);
        public FMenuAction Action;
        public delegate void FOnMenuDraw(UMenuItem item, object sender);
        public FOnMenuDraw OnMenuDraw;
        public List<UMenuItem> SubMenuItems { get; } = new List<UMenuItem>();
        public UMenuItem Parent = null;

        public UMenuItem FindMenuItem(string path)
        {
            var segs = path.Split("/");
            return FindMenuItem(segs, 0);
        }
        private UMenuItem FindMenuItem(string[] path, int curIndex)
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
        public UMenuItem AddMenuItem(string text, object userData, FMenuAction action)
        {
            foreach (var i in SubMenuItems)
            {
                if (i.Text == text)
                {
                    i.UserData = userData;
                    i.Action = action;
                    return i;
                }
            }
            var tmp = new UMenuItem();
            tmp.Text = text;
            tmp.UserData = userData;
            tmp.Action = action;
            tmp.Parent = this;
            SubMenuItems.Add(tmp);
            return tmp;
        }
        public UMenuItem AddMenuDraw(string text, object userData, FOnMenuDraw action)
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
            var tmp = new UMenuItem();
            tmp.Text = text;
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
        public void RemoveMenuItem(UMenuItem item)
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
    }
}
