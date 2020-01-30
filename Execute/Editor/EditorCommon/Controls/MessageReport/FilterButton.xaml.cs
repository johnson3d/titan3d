using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace EditorCommon.Controls.MessageReport
{
    /// <summary>
    /// FilterButton.xaml 的交互逻辑
    /// </summary>
    public partial class FilterButton : UserControl
    {
        public UInt64 Count
        {
            get { return (UInt64)GetValue(CountProperty); }
            set { SetValue(CountProperty, value); }
        }

        public static readonly DependencyProperty CountProperty =
            DependencyProperty.Register("Count", typeof(UInt64), typeof(FilterButton),
                                    new FrameworkPropertyMetadata(0, new PropertyChangedCallback(OnCountChanged)));
        public static void OnCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        public string ShowName
        {
            get { return (string)GetValue(ShowNameProperty); }
            set { SetValue(ShowNameProperty, value); }
        }
        public static readonly DependencyProperty ShowNameProperty =
            DependencyProperty.Register("ShowName", typeof(string), typeof(FilterButton), new FrameworkPropertyMetadata(""));

        public ImageSource Icon
        {
            get{ return (ImageSource)GetValue(IconProperty); }
            set{ SetValue(IconProperty, value); }
        }
        public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(ImageSource), typeof(FilterButton), new FrameworkPropertyMetadata(null));


        EngineNS.Profiler.ELogTag mLogTag;
        MessageReport mHostCtrl;

        public FilterButton(MessageReport hostCtrl, EngineNS.Profiler.ELogTag logTag)
        {
            InitializeComponent();

            mHostCtrl = hostCtrl;
            mLogTag = logTag;
            var valName = mLogTag.ToString();
            var field = typeof(EngineNS.Profiler.ELogTag).GetField(valName);
            if(field != null)
            {
                ///ResourceLibrary;component/Icon/output_error.png
                var attrs = field.GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (attrs.Length > 0)
                    ShowName = ((DescriptionAttribute)attrs[0]).Description;
            }
            Icon = (hostCtrl.TryFindResource(valName + "_Image") as Image).Source;
        }
    }
}
