using System.Windows;
using System.Windows.Interactivity;
using System.Windows.Controls;
using System.Windows.Input;

namespace UVAnimEditor.Behavior
{
    public class DragMoveScaleInCanvasBehavior : Behavior<FrameworkElement>
    {
        bool m_bDraging = false;
        Point m_leftBtnDownPt;
        Point m_scaleCenterPoint;

        bool m_LeftButton = true;
        public bool LeftButton
        {
            get { return m_LeftButton; }
            set { m_LeftButton = value; }
        }

        bool m_bScale = true;
        public bool CanScale
        {
            get { return m_bScale; }
            set { m_bScale = value; }
        }
        protected override void OnAttached()
        {
            base.OnAttached();

            this.AssociatedObject.MouseDown += AssociatedObject_MouseDown;
            //this.AssociatedObject.MouseLeftButtonDown += AssociatedObject_MouseLeftButtonDown;
            this.AssociatedObject.MouseMove += AssociateObject_MouseMove;
            this.AssociatedObject.MouseUp += AssociatedObject_MouseUp;
            //this.AssociatedObject.MouseLeftButtonUp += AssociatedObject_MouseLeftButtonUp;
            this.AssociatedObject.MouseWheel += AssociatedObject_MouseWheel;
        }

        void AssociatedObject_MouseUp(object sender, MouseButtonEventArgs e)
        {
            m_bDraging = false;
            Mouse.Capture(null);            
        }

        void AssociatedObject_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (LeftButton && e.LeftButton == MouseButtonState.Pressed ||
                !LeftButton && e.RightButton == MouseButtonState.Pressed)
            {
                m_bDraging = true;
                m_leftBtnDownPt = e.GetPosition(AssociatedObject);
                Mouse.Capture(AssociatedObject);
            }
        }

        void AssociatedObject_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!CanScale)
                return;

            var delta = 1 + e.Delta * 0.001;

            if (delta < 0)
                delta = 1;

            var oldWidth = AssociatedObject.Width;
            var oldHeight = AssociatedObject.Height;
            AssociatedObject.Width *= delta;
            AssociatedObject.Height *= delta;

            var left = Canvas.GetLeft(AssociatedObject);
            left -= (AssociatedObject.Width - oldWidth) * m_scaleCenterPoint.X;
            Canvas.SetLeft(AssociatedObject, left);

            var top = Canvas.GetTop(AssociatedObject);
            top -= (AssociatedObject.Height - oldHeight) * m_scaleCenterPoint.Y;
            Canvas.SetTop(AssociatedObject, top);
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            this.AssociatedObject.MouseDown -= AssociatedObject_MouseDown;
            //this.AssociatedObject.MouseLeftButtonDown -= AssociatedObject_MouseLeftButtonDown;
            this.AssociatedObject.MouseMove -= AssociateObject_MouseMove;
            this.AssociatedObject.MouseUp -= AssociatedObject_MouseUp;
            //this.AssociatedObject.MouseLeftButtonUp -= AssociatedObject_MouseLeftButtonUp;
            this.AssociatedObject.MouseWheel -= AssociatedObject_MouseWheel;
        }

        //private void AssociatedObject_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    m_bDraging = true;
        //    m_leftBtnDownPt = e.GetPosition(AssociatedObject);
        //    Mouse.Capture(AssociatedObject);
        //}

        private void AssociateObject_MouseMove(object sender, MouseEventArgs e)
        {
            var point = e.GetPosition(AssociatedObject);
            m_scaleCenterPoint.X = point.X / AssociatedObject.Width;
            m_scaleCenterPoint.Y = point.Y / AssociatedObject.Height;

            if (m_bDraging == true)
            {
                point = e.GetPosition(AssociatedObject);
                var delta = point - m_leftBtnDownPt;
                var left = Canvas.GetLeft(AssociatedObject);
                Canvas.SetLeft(AssociatedObject, left + delta.X);
                var top = Canvas.GetTop(AssociatedObject);
                Canvas.SetTop(AssociatedObject, top + delta.Y);
            }
        }

        //private void AssociatedObject_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        //{
        //    m_bDraging = false;
        //    Mouse.Capture(null);
        //}
    }
}
