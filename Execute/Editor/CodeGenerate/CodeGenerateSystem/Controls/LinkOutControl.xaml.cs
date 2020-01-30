using System.Windows.Input;
using System.Windows.Media.Animation;
using CodeGenerateSystem.Base;
using System.Windows;

namespace CodeGenerateSystem.Controls
{
    /// <summary>
    /// LinkOutControl.xaml 的交互逻辑
    /// </summary>
    public partial class LinkOutControl : Base.LinkPinControl
    {
        public LinkOutControl()
        {
            InitializeComponent();
            UpdateImageShow();
        }

        protected override void SelectedOperation(bool selected)
        {
            var storyboard = this.FindResource("Storyboard_Selected") as Storyboard;
            if (selected)
                storyboard?.Begin();
            else
                storyboard?.Stop();
        }
        
        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.RightButton == MouseButtonState.Pressed)
            {
                if(EnableSelected)
                {
//                     this.Selected = true;
//                     if (HostNodesContainer != null)
//                         HostNodesContainer.SelectedLinkControl = this;

                    e.Handled = true;
                }
            }
        }

        private void UserControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if(this.Selected)
                e.Handled = true;
        }

        private void userControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateShow();
        }

        public override double GetPinWidth()
        {
            return Img.Width;
        }
        public override double GetPinHeight()
        {
            return Img.Height;
        }
    }
}
