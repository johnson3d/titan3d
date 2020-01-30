using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace ResourceLibrary.Controls.Menu
{
    public class MenuAssist : DependencyObject
    {
        public static ImageSource GetIcon(DependencyObject obj)
        {
            return (ImageSource)obj.GetValue(IconProperty);
        }
        public static void SetIcon(DependencyObject obj, ImageSource value)
        {
            obj.SetValue(IconProperty, value);
        }

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.RegisterAttached("Icon", typeof(ImageSource), typeof(MenuAssist), new UIPropertyMetadata(null));

        public static bool GetHasOffset(DependencyObject obj)
        {
            return (bool)obj.GetValue(HasOffsetProperty);
        }
        public static void SetHasOffset(DependencyObject obj, bool value)
        {
            obj.SetValue(HasOffsetProperty, value);
        }
        public static readonly DependencyProperty HasOffsetProperty =
            DependencyProperty.RegisterAttached("HasOffset", typeof(bool), typeof(MenuAssist), new UIPropertyMetadata(true));

        public static bool GetHighlightAble(DependencyObject obj)
        {
            return (bool)obj.GetValue(HighlightAbleProperty);
        }
        public static void SetHighlightAble(DependencyObject obj, bool value)
        {
            obj.SetValue(HighlightAbleProperty, value);
        }
        public static readonly DependencyProperty HighlightAbleProperty =
            DependencyProperty.RegisterAttached("HighlightAble", typeof(bool), typeof(MenuAssist), new UIPropertyMetadata(true));

        public static bool GetNotCloseMenuOnClick(DependencyObject obj)
        {
            return (bool)obj.GetValue(NotCloseMenuOnClickProperty);
        }
        public static void SetNotCloseMenuOnClick(DependencyObject obj, bool value)
        {
            obj.SetValue(NotCloseMenuOnClickProperty, value);
        }
        public static readonly DependencyProperty NotCloseMenuOnClickProperty =
            DependencyProperty.RegisterAttached("NotCloseMenuOnClick", typeof(bool), typeof(MenuAssist), new UIPropertyMetadata(true));
    }
}
