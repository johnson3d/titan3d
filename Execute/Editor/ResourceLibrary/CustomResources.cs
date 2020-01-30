using System.Windows;

namespace ResourceLibrary
{
    /// <summary>
    ///
    /// </summary>
    public class CustomResources
    {
        public static ComponentResourceKey ToolBarToggleButtonStyle_Default
        {
            get
            {
                return new ComponentResourceKey(typeof(CustomResources), "ToolBarToggleButtonStyle_Default");
            }
        }

        public static ComponentResourceKey ToggleButtonStyle
        {
            get
            {
                return new ComponentResourceKey(typeof(CustomResources), "ToggleButtonStyle");
            }
        }

        public static ComponentResourceKey RadioToggleButtonStyle
        { 
            get
            {
                return new ComponentResourceKey(typeof(CustomResources), "RadioToggleButtonStyle");
            }
        }

        public static ComponentResourceKey TreeViewItemStyle
        {
            get
            {
                return new ComponentResourceKey(typeof(CustomResources), "TreeViewItemStyle_Default");
            }
        }

        public static ComponentResourceKey TabItemStyle
        {
            get
            {
                return new ComponentResourceKey(typeof(CustomResources), "TabItemStyle_Default");
            }
        }

        public static ComponentResourceKey ButtonStyle
        {
            get
            {
                return new ComponentResourceKey(typeof(CustomResources), "ButtonStyle_Default");
            }
        }

        public static ComponentResourceKey MenuItemStyle
        {
            get
            {
                return new ComponentResourceKey(typeof(CustomResources), "MenuItem_Default");
            }
        }        
    }

    //public class WindowAttachedPropertys : DependencyObject
    //{
    //    // 标题栏
    //    public static bool GetShowTitle(DependencyObject obj)
    //    {
    //        return (bool)obj.GetValue(ShowTitleProperty);
    //    }
    //    public static void SetShowTitle(DependencyObject obj, bool value)
    //    {
    //        obj.SetValue(ShowTitleProperty, value);
    //    }
    //    public static readonly DependencyProperty ShowTitleProperty = DependencyProperty.RegisterAttached("ShowTitle", typeof(bool), typeof(WindowAttachedPropertys), new PropertyMetadata(true));

    //    // 横向填充按钮
    //    public static bool GetShowHSizeButton(DependencyObject obj)
    //    {
    //        return (bool)obj.GetValue(ShowHSizeButtonProperty);
    //    }
    //    public static void SetShowHSizeButton(DependencyObject obj, bool value)
    //    {
    //        obj.SetValue(ShowHSizeButtonProperty, value);
    //    }
    //    public static readonly DependencyProperty ShowHSizeButtonProperty = DependencyProperty.RegisterAttached("ShowHSizeButton", typeof(bool), typeof(WindowAttachedPropertys), new PropertyMetadata(true));

    //    // 纵向填充按钮
    //    public static bool GetShowVSizeButton(DependencyObject obj)
    //    {
    //        return (bool)obj.GetValue(ShowVSizeButtonProperty);
    //    }
    //    public static void SetShowVSizeButton(DependencyObject obj, bool value)
    //    {
    //        obj.SetValue(ShowVSizeButtonProperty, value);
    //    }
    //    public static readonly DependencyProperty ShowVSizeButtonProperty = DependencyProperty.RegisterAttached("ShowVSizeButton", typeof(bool), typeof(WindowAttachedPropertys), new PropertyMetadata(true));

    //    // 最小化按钮
    //    public static bool GetShowMinButton(DependencyObject obj)
    //    {
    //        return (bool)obj.GetValue(ShowMinButtonProperty);
    //    }
    //    public static void SetShowMinButton(DependencyObject obj, bool value)
    //    {
    //        obj.SetValue(ShowMinButtonProperty, value);
    //    }
    //    public static readonly DependencyProperty ShowMinButtonProperty = DependencyProperty.RegisterAttached("ShowVSizeButton", typeof(bool), typeof(WindowAttachedPropertys), new PropertyMetadata(true));

    //}
}