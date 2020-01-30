using System.Windows;
using System.Windows.Controls;

namespace EditorCommon.Controls.ResourceBrowser
{
    /// <summary>
    /// Interaction logic for WaitingProgress.xaml
    /// </summary>
    public partial class WaitingProgress : UserControl
    {
        private System.Windows.Media.Animation.Storyboard story;

        public WaitingProgress()
        {
            InitializeComponent();

            this.story = (base.Resources["waiting"] as System.Windows.Media.Animation.Storyboard);
            this.story.Begin(this.image, true);
        }

        private void Image_Loaded_1(object sender, RoutedEventArgs e)
        {

        }

        public void Stop()
        {
            this.story.Pause(this.image);
            base.Visibility = System.Windows.Visibility.Collapsed;
        }
    }
}
