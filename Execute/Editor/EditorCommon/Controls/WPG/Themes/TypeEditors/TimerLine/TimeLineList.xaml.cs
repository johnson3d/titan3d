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

namespace WPG.Themes.TypeEditors.TimerLine
{
    /// <summary>
    /// TimeLineList.xaml 的交互逻辑
    /// </summary>
    public partial class TimeLineList : UserControl
    {
        public TimeLineList()
        {
            InitializeComponent();
        }


        public delegate void AddDataControlDelegate(double height);
        public event AddDataControlDelegate AddDataControl;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //var createwindow = new SelectType();
            //createwindow.ShowDialog();
            //if (string.IsNullOrEmpty(createwindow.TypeStr) == false)
            //{
            //    var TimeLine = new TimeLinePanel();
            //    TimeLine.TypeStr = createwindow.TypeStr;
            //    UIStack.Children.Add(TimeLine);

            //    AddDataControl?.Invoke(130);
            //}
           
        }
    }
}
