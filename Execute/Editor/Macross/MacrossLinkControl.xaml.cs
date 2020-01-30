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

namespace Macross
{

    /// <summary>
    /// Interaction logic for MacrossLinkControl.xaml
    /// </summary>
    public partial class MacrossLinkControl : MacrossLinkControlBase
    {

        public MacrossLinkControl()
        {
            InitializeComponent();

            NodesCtrlAssist = NodesCtrlAssistCtrl;
            MacrossOpPanel = MacrossOpPanelCtrl;
            mPG = PG;

            NodesCtrlAssist.HostControl = this;
            NodesCtrlAssist.LinkedCategoryItemName = MacrossPanel.MainGraphName;
            MacrossOpPanel.HostControl = this;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            UISlider.Control = this;
        }
    }
}
