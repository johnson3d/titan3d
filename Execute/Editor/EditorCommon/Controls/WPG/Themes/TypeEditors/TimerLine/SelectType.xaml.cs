using ResourceLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
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
    /// SelectType.xaml 的交互逻辑
    /// </summary>
    public partial class SelectType : UserControl
    {
        public SelectType()
        {
            InitializeComponent();
        }

        public string TypeStr
        {
            get;
            set;
        }

        //private void Button_Click_Cancel(object sender, RoutedEventArgs e)
        //{
        //    TypeStr = "";
        //}

        public delegate void OKEventDelegate(string TypeStr);
        public OKEventDelegate OKEvent;
        private void Button_Click_OK(object sender, RoutedEventArgs e)
        {
            if (UIList.SelectedItem == null)
            {
                TypeStr = "";
                OKEvent?.Invoke(TypeStr);
                return;
            }
            var textblock = UIList.SelectedItem as TextBlock;
            TypeStr = textblock.Text.ToString();
            OKEvent?.Invoke(TypeStr);
        }
    }
}
