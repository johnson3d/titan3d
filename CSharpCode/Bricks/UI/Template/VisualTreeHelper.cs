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
            var children = container.Children.mChildren;
            if (passContentsPresenter && container.ChildIsContentsPresenter)
                children = container.Parent.mLogicContentsPresenter.Children.mChildren;
            if (children == null)
                return 0;
            return children.Count;
        }
        public static TtUIElement GetChild(TtContainer container, int index, bool passContentsPresenter = true)
        {
            var children = container.Children.mChildren;
            if (passContentsPresenter && container.ChildIsContentsPresenter)
                children = container.Parent.mLogicContentsPresenter.Children.mChildren;
            if (children == null)
                return null;
            if (index < 0 || index >= children.Count)
                return null;
            return children[index];
        }
        public static TtUIElement GetChild(TtContainer container, string name, bool findInChildren = true, bool passContentsPresenter = true)
        {
            var children = container.Children.mChildren;
            if (passContentsPresenter && container.ChildIsContentsPresenter)
                children = container.Parent.mLogicContentsPresenter.Children.mChildren;
            if (children == null)
                return null;
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
