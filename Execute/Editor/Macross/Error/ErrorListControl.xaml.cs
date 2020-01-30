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

namespace Macross.Error
{
    public class ErrorListItem : DependencyObject
    {
        public ErrorListItem()
        {

        }
    }

    /// <summary>
    /// Interaction logic for ErrorListControl.xaml
    /// </summary>
    public partial class ErrorListControl : UserControl
    {
        public ErrorListControl()
        {
            InitializeComponent();
        }
    }
}
