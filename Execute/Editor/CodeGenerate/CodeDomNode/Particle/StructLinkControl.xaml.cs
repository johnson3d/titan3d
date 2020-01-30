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

namespace CodeDomNode.Particle
{
    /// <summary>
    /// Interaction logic for StructLinkControl.xaml
    /// </summary>
    public partial class StructLinkControl: LinkPinControl
    {
        Brush DefaultBrushes = Brushes.Gray;
        public StructLinkControl()
        {
            InitializeComponent();
            //DefaultBrushes = BGBorder.Background;
            BGBorder.Background = DefaultBrushes;
            this.MouseRightButtonDown += (sender, e) =>
            {
                e.Handled = true;
            };
        }
       
        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //e.Handled = true;
        }

        private void UserControl_MouseLeftUp(object sender, MouseButtonEventArgs e)
        {
            if (HostNodesContainer == null)
            {
                //HostNodesContainer?.PreviewLinkCurve?.Visibility = Visibility.Collapsed;
                return;
            }

            if (HostNodesContainer?.PreviewLinkCurve?.Visibility == Visibility.Visible)
            {
                if (CodeGenerateSystem.Base.LinkInfo.CanLinkWith(HostNodesContainer.StartLinkObj, this) && ModuleLinkInfo.CanLinkWith2(HostNodesContainer.StartLinkObj, this))
                {

                }
                else
                {
                    //HostNodesContainer?.PreviewLinkCurve?.Visibility = Visibility.Collapsed;
                }
            }
        }

        
        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            if (HostNodesContainer == null)
            {
                BGBorder.Background = Brushes.White;
                return;
            }

            if (HostNodesContainer?.PreviewLinkCurve?.Visibility == Visibility.Visible)
            {
                if (CodeGenerateSystem.Base.LinkInfo.CanLinkWith(HostNodesContainer.StartLinkObj, this) &&
                    ModuleLinkInfo.CanLinkWith2(HostNodesContainer.StartLinkObj, this))
                {
                    BGBorder.Background = Brushes.Green;
                }
                else
                {
                    BGBorder.Background = Brushes.Red;
                }
            }
            else
            {
                BGBorder.Background = Brushes.White;
            }
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            BGBorder.Background = DefaultBrushes;
        }

        //public override LinkInfo GetNewLinkInfo(Canvas drawCanvas, LinkPinControl startObj, LinkPinControl endObj)
        //{
        //    return new ModuleLinkInfo(drawCanvas, startObj, endObj);
        //}
    }
}
