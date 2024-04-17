using EngineNS.UI.Controls;
using EngineNS.UI.Controls.Containers;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace EngineNS.UI
{
    internal class VisualTreeHelper
    {
        [Flags]
        public enum EFlag
        {
            None                                    = 0,
            PassContentsPresenter                   = 1<<0,
            PassCollapsedContentsPresenterChildren  = 1<<1,
        }
        static bool HasFlag(EFlag flag, EFlag checker)
        {
            return ((flag & checker) == checker);
        }
        public static int GetChildrenCount(TtContainer container, EFlag flags = EFlag.PassContentsPresenter | EFlag.PassCollapsedContentsPresenterChildren)
        {
            int count = 0;
            var children = container.Children.mChildren;
            if (children == null)
                return 0;
            if (HasFlag(flags, EFlag.PassContentsPresenter) && container.ChildIsContentsPresenter)
            {
                for(int i=0; i< children.Count; i++)
                {
                    var cp = children[i] as TtContentsPresenter;
                    if (cp == null)
                        count++;
                    else if(!HasFlag(flags, EFlag.PassCollapsedContentsPresenterChildren) || cp.Visibility != Visibility.Collapsed)
                        count += cp.Children.mChildren.Count;
                }
            }
            else if(children != null)
            {
                count = children.Count;
            }
            return count;
        }
        public static TtUIElement GetChild(TtContainer container, int index, EFlag flags = EFlag.PassContentsPresenter | EFlag.PassCollapsedContentsPresenterChildren)
        {
            var children = container.Children.mChildren;
            if (children == null)
                return null;
            if (HasFlag(flags, EFlag.PassContentsPresenter) && container.ChildIsContentsPresenter)
            {
                var count = children.Count;
                var realIndex = 0;
                for(int i=0; i<count; i++)
                {
                    var cp = children[i] as TtContentsPresenter;
                    if (cp == null)
                    {
                        if (realIndex == index)
                            return children[i];
                        realIndex++;
                    }
                    else if(!HasFlag(flags, EFlag.PassCollapsedContentsPresenterChildren) || cp.Visibility != Visibility.Collapsed)
                    {
                        var cpChildren = cp.Children.mChildren;
                        if((index - realIndex) < cpChildren.Count)
                        {
                            return cpChildren[index - realIndex];
                        }
                        realIndex += cpChildren.Count;
                    }
                }
            }
            else
            {
                if (index < 0 || index >= children.Count)
                    return null;
                return children[index];
            }
            return null;
        }
        static TtUIElement GetChildFromChildren(string name, List<TtUIElement> children, bool findInChildren, EFlag flags)
        {
            for(int i=0; i<children.Count; i++)
            {
                if (children[i].Name == name)
                    return children[i];
                if(findInChildren)
                {
                    var cct = children[i] as TtContainer;
                    if(cct != null)
                    {
                        var result = VisualTreeHelper.GetChild(cct, name, findInChildren, flags);
                        if (result != null)
                            return result;
                    }
                }
            }
            return null;
        }
        public static TtUIElement GetChild(TtContainer container, string name, bool findInChildren = true, EFlag flags = EFlag.PassContentsPresenter | EFlag.PassCollapsedContentsPresenterChildren)
        {
            var children = container.Children.mChildren;
            if (children == null)
                return null;
            if (HasFlag(flags, EFlag.PassContentsPresenter) && container.ChildIsContentsPresenter)
            {
                for(int i=0; i<children.Count; i++)
                {
                    var cp = children[i] as TtContentsPresenter;
                    if(cp == null)
                    {
                        if (children[i].Name == name)
                            return children[i];
                        if(findInChildren)
                        {
                            var cct = children[i] as TtContainer;
                            if (cct != null)
                            {
                                var result = VisualTreeHelper.GetChild(cct, name, findInChildren, flags);
                                if (result != null)
                                    return result;
                            }
                        }
                    }
                    else if (!HasFlag(flags, EFlag.PassCollapsedContentsPresenterChildren) || cp.Visibility != Visibility.Collapsed)
                    {
                        var cChildren = container.ChildContentsPresenter.Children.mChildren;
                        return GetChildFromChildren(name, cChildren, findInChildren, flags);
                    }
                }
            }
            else
            {
                return GetChildFromChildren(name, children, findInChildren, flags);
            }
            return null;
        }
        public static TtContainer GetParent(TtUIElement element, EFlag flags = EFlag.PassContentsPresenter | EFlag.PassCollapsedContentsPresenterChildren)
        {
            if(HasFlag(flags, EFlag.PassContentsPresenter))
            {
                if (element.mVisualParent is TtContentsPresenter)
                    return element.mVisualParent.mVisualParent;
            }

            return element.mVisualParent;
        }
    }
}
