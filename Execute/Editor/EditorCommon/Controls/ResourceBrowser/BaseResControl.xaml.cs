using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EditorCommon.Controls.ResourceBrowser
{
    /// <summary>
    /// UserControl1.xaml 的交互逻辑
    /// </summary>
    public partial class BaseResControl : UserControl
    {
        public ImageSource Icon
        {
            get { return (ImageSource)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(ImageSource), typeof(BaseResControl), new FrameworkPropertyMetadata(null));

        public Brush ResourceBrush
        {
            get { return (Brush)GetValue(ResourceBrushProperty); }
            set { SetValue(ResourceBrushProperty, value); }
        }
        public static readonly DependencyProperty ResourceBrushProperty =
            DependencyProperty.Register("ResourceBrush", typeof(Brush), typeof(BaseResControl), new FrameworkPropertyMetadata(null));
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(BaseResControl), new FrameworkPropertyMetadata(""));

        public BaseResControl()
        {
            InitializeComponent();
        }
    }
}
