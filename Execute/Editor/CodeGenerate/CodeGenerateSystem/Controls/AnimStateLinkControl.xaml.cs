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
using CodeGenerateSystem.Base;
using EngineNS.IO;

namespace CodeGenerateSystem.Controls
{
    public class StateTransitionPair
    {
        public Guid From = Guid.Empty;
        public Guid To = Guid.Empty;
        public List<Guid> Transitions = new List<Guid>();
    }
    /// <summary>
    /// Interaction logic for AnimStateLinkControl.xaml
    /// </summary>
    public partial class AnimStateLinkControl : Base.LinkPinControl
    {
        public AnimStateLinkControl()
        {
            InitializeComponent();
        }
       

        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void UserControl_MouseUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            
            //BGBorder.Background = Brushes.White;
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            //BGBorder.Background = Brushes.
        }
    }
}
