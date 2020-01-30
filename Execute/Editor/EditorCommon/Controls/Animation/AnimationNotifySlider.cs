using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EditorCommon.Controls.Animation
{
    public class AnimNotifySliderEventArgs
    {
        public AnimNotifyNodeControl Ctrl;
        public Point MousePoint;
        public AnimNotifySliderEventArgs(AnimNotifyNodeControl ctrl ,Point point = default(Point))
        {
            Ctrl = ctrl;
            
        }
    }
    public class AnimationNotifySlider : CustomSlider
    {
        public event EventHandler<AnimNotifySliderEventArgs> OnNotifyCtrlPickUp;
        public event EventHandler<AnimNotifySliderEventArgs> OnNotifyCtrlDropDown;
        public event EventHandler<AnimNotifySliderEventArgs> OnNotifyCtrlRightButtonDown;
        public event EventHandler<AnimNotifySliderEventArgs> OnNotifyCtrlLeftButtonDown;
        public Int64 AnimationDuration = 0;
        List<AnimNotifyNodeControl> mAnimNodifyNodes = new List<AnimNotifyNodeControl>();
        public void AddNofity(AnimNotifyNodeControl notify)
        {
            mAnimNodifyNodes.Add(notify);
            TicksCanvas.Children.Add(notify);
            notify.LocationFrame = (notify.Pos - mTicksList[0].Pos) / TicksScaleInterval;
            notify.CGfxNotify.NotifyTime = (long)(notify.LocationFrame / Maximum * AnimationDuration);
            notify.Height = 30;
            notify.AnimNotifyMouseLeftButtonDown += Notify_AnimNotifyMouseLeftButtonDown;
            notify.AnimNotifyMouseLeftButtonUp += Notify_AnimNotifyMouseLeftButtonUp;
            notify.AnimNotifyMouseMove += Notify_AnimNotifyMouseMove;
            notify.AnimNotifyMouseRightButtonDown += Notify_AnimNotifyMouseRightButtonDown;
            notify.AnimNotifyMouseWheel += Notify_AnimNotifyMouseWheel;
        }
   
        public void RemoveNotify(AnimNotifyNodeControl notify)
        {
            mAnimNodifyNodes.Remove(notify);
            TicksCanvas.Children.Remove(notify);
            notify.AnimNotifyMouseLeftButtonDown -= Notify_AnimNotifyMouseLeftButtonDown;
            notify.AnimNotifyMouseLeftButtonUp -= Notify_AnimNotifyMouseLeftButtonUp;
            notify.AnimNotifyMouseMove -= Notify_AnimNotifyMouseMove;
            notify.AnimNotifyMouseRightButtonDown -= Notify_AnimNotifyMouseRightButtonDown;
            notify.AnimNotifyMouseWheel -= Notify_AnimNotifyMouseWheel;
        }
        public bool CheckMouseIn(Point point)
        {
            if(point.X >0 && point.Y >0 && point.X<TicksCanvas.ActualWidth && point.Y < TicksCanvas.ActualHeight)
            {
                return true;
            }
            return false;
        }
        protected override void RestCustomItems()
        {
            ScaleCustomItem(0, 0);
        }
        public override void ScaleCustomItem(double deltaScale, double percent)
        {
            foreach(var notifyNode in mAnimNodifyNodes)
            {
                notifyNode.Pos = TicksScaleInterval * notifyNode.LocationFrame - notifyNode.NodeWidth*0.5 + mTicksList[0].Pos;
            }
        }
        public void MoveNotifyNode(AnimNotifyNodeControl notifyNode, double delta)
        {
            notifyNode.Pos += delta;
            notifyNode.LocationFrame = (notifyNode.Pos - mTicksList[0].Pos) / TicksScaleInterval;
            notifyNode.CGfxNotify.NotifyTime = (long)(notifyNode.LocationFrame / Maximum * AnimationDuration);
        }
        #region NotifyEvent
        private void Notify_AnimNotifyMouseLeftButtonDown(object notify, object sender, MouseButtonEventArgs e)
        {
            var notifyNode = notify as AnimNotifyNodeControl;
            notifyNode.IsLButtonDown = true;
            lastMousePoint = e.GetPosition(TicksCanvas);
            Mouse.Capture(sender as FrameworkElement);
            OnNotifyCtrlLeftButtonDown?.Invoke(this, new AnimNotifySliderEventArgs(notifyNode));
            OnNotifyCtrlPickUp?.Invoke(this, new AnimNotifySliderEventArgs(notifyNode));
        }
        static Point lastMousePoint;
        private void Notify_AnimNotifyMouseMove(object notify, object sender, MouseEventArgs e)
        {
            var notifyNode = notify as AnimNotifyNodeControl;
            if (notifyNode.IsLButtonDown)
            {
                MoveNotifyNode(notifyNode, e.GetPosition(TicksCanvas).X - lastMousePoint.X);
                lastMousePoint = e.GetPosition(TicksCanvas);
            }
        }
        private void Notify_AnimNotifyMouseLeftButtonUp(object notify, object sender, MouseButtonEventArgs e)
        {
            var notifyNode = notify as AnimNotifyNodeControl;
            notifyNode.IsLButtonDown = false;
            Mouse.Capture(null);
            OnNotifyCtrlDropDown?.Invoke(this, new AnimNotifySliderEventArgs(notifyNode));
        }
        private void Notify_AnimNotifyMouseWheel(object notify, object sender, MouseWheelEventArgs e)
        {

        }
        private void Notify_AnimNotifyMouseRightButtonDown(object notify, object sender, MouseButtonEventArgs e)
        {
            var notifyNode = notify as AnimNotifyNodeControl;
            OnNotifyCtrlRightButtonDown?.Invoke(this, new AnimNotifySliderEventArgs(notifyNode,e.GetPosition(TicksCanvas)));
            e.Handled = true;
        }
        #endregion
    }
}
