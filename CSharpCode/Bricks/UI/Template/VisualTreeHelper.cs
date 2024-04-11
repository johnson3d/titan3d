using EngineNS.UI.Controls;
using EngineNS.UI.Controls.Containers;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.UI
{
    internal class VisualTreeHelper
    {
        public static int GetChildrenCount(TtContainer container, bool passContentsPresenter = true)
        {
            int count = 0;
            var children = container.Children.mChildren;
            if (children == null)
                return 0;
            if (passContentsPresenter && container.ChildIsContentsPresenter)
            {
                for(int i=0; i< children.Count; i++)
                {
                    var cp = children[i] as TtContentsPresenter;
                    if (cp == null)
                        count++;
                    else
                        count += cp.Children.mChildren.Count;
                }
            }
            else if(children != null)
            {
                count = children.Count;
            }
            return count;
        }
        public static TtUIElement GetChild(TtContainer container, int index, bool passContentsPresenter = true)
        {
            var children = container.Children.mChildren;
            if (children == null)
                return null;
            if (passContentsPresenter && container.ChildIsContentsPresenter)
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
                    else
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
        static TtUIElement GetChildFromChildren(string name, List<TtUIElement> children, bool findInChildren, bool passContentsPresenter)
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
                        var result = VisualTreeHelper.GetChild(cct, name, passContentsPresenter);
                        if (result != null)
                            return result;
                    }
                }
            }
            return null;
        }
        public static TtUIElement GetChild(TtContainer container, string name, bool findInChildren = true, bool passContentsPresenter = true)
        {
            var children = container.Children.mChildren;
            if (children == null)
                return null;
            if (passContentsPresenter && container.ChildIsContentsPresenter)
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
                                var result = VisualTreeHelper.GetChild(cct, name, passContentsPresenter);
                                if (result != null)
                                    return result;
                            }
                        }
                    }
                    else
                    {
                        var cChildren = container.ChildContentsPresenter.Children.mChildren;
                        return GetChildFromChildren(name, cChildren, findInChildren, passContentsPresenter);
                    }
                }
            }
            else
            {
                return GetChildFromChildren(name, children, findInChildren, passContentsPresenter);
            }
            return null;
        }
        public static TtContainer GetParent(TtUIElement element, bool passContentsPresenter = true)
        {
            if(passContentsPresenter)
            {
                if (element.mVisualParent is TtContentsPresenter)
                    return element.mVisualParent.mVisualParent;
            }

            return element.mVisualParent;
        }
    }
}
