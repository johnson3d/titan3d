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
namespace CodeDomNode.Particle
{
    public partial class TitlePanel : UserControl, INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        public CodeGenerateSystem.Base.BaseNodeControl Host;
        string mNodeName = "NodeName";
        public string NodeName
        {
            get { return (string)GetValue(NodeNameProperty); }
            set { SetValue(NodeNameProperty, value); }
        }

        public static readonly DependencyProperty NodeNameProperty = DependencyProperty.Register("NodeName", typeof(string), typeof(TitlePanel), new FrameworkPropertyMetadata(null));
        public TitlePanel()
        {
            InitializeComponent();
        }

        private void UIMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TitleLabel.Visibility = System.Windows.Visibility.Collapsed;
            TitleBox.Visibility = System.Windows.Visibility.Visible;
            TitleBox.Text = Host.NodeName;
            TitleBox.Focus();
            e.Handled = true;
        }

        private void UILostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            TitleLabel.Visibility = System.Windows.Visibility.Visible;
            TitleBox.Visibility = System.Windows.Visibility.Collapsed;
            Host.NodeName = TitleBox.Text;
        }
    }
}
