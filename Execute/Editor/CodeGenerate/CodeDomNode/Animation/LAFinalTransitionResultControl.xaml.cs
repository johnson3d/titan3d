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

namespace CodeDomNode.Animation
{
    /// <summary>
    /// Interaction logic for FinalTransitionResultControl.xaml
    /// </summary>
    public partial class LAFinalTransitionResultControl
    {
        partial void InitConstruction()
        {
            InitializeComponent();
            mCtrlValueInputHandle = ValueInputHandle;
        }
    }
}
