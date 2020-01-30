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
    /// Interaction logic for NotifyEditeControl.xaml
    /// </summary>
    public partial class NotifyEditeControl : UserControl
    {
        public event EventHandler<TickBarScaleEventArgs> OnTickBarScaling;
        public NotifyEditeControl()
        {
            InitializeComponent();
            AnimSlider.OnTickBarScaling += AnimSlider_OnTickBarScaling;
            AnimSlider.OnTickBarRightButtonClick += AnimSlider_OnTickBarRightButtonClick;
        }
        Point rightClickMousePoint;
        private void AnimSlider_OnTickBarRightButtonClick(object sender, TickBarClickEventArgs e)
        {
            rightClickMousePoint = e.MousePoint;
            AnimSlider.TickBarContexMenu.Items.Clear();
            CreateItmeToContextMenu("Play Sound", PlaySound_Click);
            CreateItmeToContextMenu("Play Effect", PlayEffect_Click);
            CreateItmeToContextMenu("New Notify", NewNotify_Click);
            AnimSlider.TickBarContexMenu.PlacementTarget = sender as Canvas;
            AnimSlider.TickBarContexMenu.IsOpen = true;
        }
        void CreateItmeToContextMenu(string itemName,RoutedEventHandler handler)
        {
            MenuItem item = new MenuItem();
            item.Header = itemName;
            item.Foreground = Brushes.White;
            item.Click += handler;
            AnimSlider.TickBarContexMenu.Visibility = Visibility.Visible;
            AnimSlider.TickBarContexMenu.Items.Add(item);
        }
        private void PlaySound_Click(object sender, RoutedEventArgs e)
        {
            //var pos = System.Windows.Forms.Control.MousePosition;
            //var transform = PresentationSource.FromVisual(this).CompositionTarget.TransformFromDevice;
            //var mousePos = transform.Transform(new Point(pos.X, pos.Y));
            //var win = new NotifyCreateWindow();
            //win.Left = mousePos.X;
            //win.Top = mousePos.Y;
            //if (win.ShowDialog() == false)
            //    return;
            //var notifyName = win.NotifyTextBox.Text;
            //AnimNotifyNodeControl node = new AnimNotifyNodeControl();
            //node.Height = AnimSlider.ActualHeight;
            //node.NodeName = notifyName;
            //node.Pos = rightClickMousePoint.X - node.NodeWidth * 0.5;
            //AnimSlider.AddNofity(node);
        }
        private void PlayEffect_Click(object sender, RoutedEventArgs e)
        {
            //var pos = System.Windows.Forms.Control.MousePosition;
            //var transform = PresentationSource.FromVisual(this).CompositionTarget.TransformFromDevice;
            //var mousePos = transform.Transform(new Point(pos.X, pos.Y));
            //var win = new NotifyCreateWindow();
            //win.Left = mousePos.X;
            //win.Top = mousePos.Y;
            //if (win.ShowDialog() == false)
            //    return;
            //var notifyName = win.NotifyTextBox.Text;
            //AnimNotifyNodeControl node = new AnimNotifyNodeControl();
            //node.Height = AnimSlider.ActualHeight;
            //node.NodeName = notifyName;
            //node.Pos = rightClickMousePoint.X - node.NodeWidth * 0.5;
            //AnimSlider.AddNofity(node);
        }
        private void NewNotify_Click(object sender, RoutedEventArgs e)
        {
            var pos = System.Windows.Forms.Control.MousePosition;
            var transform = PresentationSource.FromVisual(this).CompositionTarget.TransformFromDevice;
            var mousePos = transform.Transform(new Point(pos.X,pos.Y));
            var win = new NotifyCreateWindow();
            win.Left = mousePos.X;
            win.Top = mousePos.Y;
            if (win.ShowDialog() == false)
                return;
            var notifyName = win.NotifyTextBox.Text;
            AnimNotifyNodeControl node = new AnimNotifyNodeControl();
            node.Height = AnimSlider.ActualHeight;
            node.NodeName = notifyName;
            node.Pos = rightClickMousePoint.X - node.NodeWidth*0.5;
            AnimSlider.AddNofity(node);
        }
        private void AnimSlider_OnTickBarScaling(object sender, TickBarScaleEventArgs e)
        {
            OnTickBarScaling?.Invoke(this, e);
        }
        public void TickBarScale(double deltaScale, double percent)
        {
            AnimSlider.TickBarScale(deltaScale, percent);
        }
        public double Value
        {
            get { return (float)AnimSlider.Value; }
            set
            {
                EngineNS.CEngine.Instance.EventPoster.RunOn(()=> 
                {
                    AnimSlider.Value = (double)value;
                    return true;
                }, EngineNS.Thread.Async.EAsyncTarget.Editor);
                
            }
        }
    }
}
