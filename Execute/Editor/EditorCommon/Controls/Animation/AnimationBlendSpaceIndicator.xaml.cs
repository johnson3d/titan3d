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

namespace EditorCommon.Controls.Animation
{
    /// <summary>
    /// Interaction logic for AnimationBlendSpaceIndicator.xaml
    /// </summary>
    public partial class AnimationBlendSpaceIndicator : UserControl
    {
        BlendSpaceIndicator mHostBlendSpaceIndicator;
        public BlendSpaceIndicator HostBlendSpaceIndicator
        {
            get => mHostBlendSpaceIndicator;
            set => mHostBlendSpaceIndicator = value;
        }
        public AnimationBlendSpaceIndicator()
        {
            InitializeComponent();
        }

        private void CurrentPos_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            CurrentPos_TranslateTransform.X = -CurrentPos.ActualWidth / 2.0f;
            CurrentPos_TranslateTransform.Y = -CurrentPos.ActualHeight / 2.0f;
        }

        private void CurrentPos_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            mHostBlendSpaceIndicator.MouseLeftButtonDown(sender, e);
        }

        private void CurrentPos_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            mHostBlendSpaceIndicator.MouseLeftButtonUp(sender, e);
        }

        private void CurrentPos_MouseEnter(object sender, MouseEventArgs e)
        {
            mHostBlendSpaceIndicator.MouseEnter(sender, e);
        }

        private void CurrentPos_MouseLeave(object sender, MouseEventArgs e)
        {
            mHostBlendSpaceIndicator.MouseLeave(sender, e);
        }
    }
}
