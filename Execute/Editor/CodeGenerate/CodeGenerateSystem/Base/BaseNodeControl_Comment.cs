using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Controls;

namespace CodeGenerateSystem.Base
{
    public partial class BaseNodeControl
    {
        public void RegisterLeftButtonEvent(FrameworkElement element)
        {
            element.MouseLeftButtonDown += BorderCtrl_MouseLeftButtonDown;
            element.MouseLeftButtonUp += BorderCtrl_MouseLeftButtonUp;
            element.MouseMove += Left_MouseMove;
        }

        public void RegisterRightButtonEvent(FrameworkElement element)
        {
            element.MouseLeftButtonDown += BorderCtrl_MouseLeftButtonDown;
            element.MouseLeftButtonUp += BorderCtrl_MouseLeftButtonUp;
            element.MouseMove += Right_MouseMove;
        }

        void OnApplyTemplate_Comment()
        {
            var left = Template.FindName("PART_BORDER_LEFT", this) as Rectangle;
            if(left != null)
            {
                left.MouseLeftButtonDown += BorderCtrl_MouseLeftButtonDown;
                left.MouseLeftButtonUp += BorderCtrl_MouseLeftButtonUp;
                left.MouseMove += Left_MouseMove;
            }
            var right = Template.FindName("PART_BORDER_Right", this) as Rectangle;
            if (right != null)
            {
                right.MouseLeftButtonDown += BorderCtrl_MouseLeftButtonDown;
                right.MouseLeftButtonUp += BorderCtrl_MouseLeftButtonUp;
                right.MouseMove += Right_MouseMove;
            }
            var top = Template.FindName("PART_BORDER_TOP", this) as Rectangle;
            if(top != null)
            {
                top.MouseLeftButtonDown += BorderCtrl_MouseLeftButtonDown;
                top.MouseLeftButtonUp += BorderCtrl_MouseLeftButtonUp;
                top.MouseMove += Top_MouseMove;
            }
            var bottom = Template.FindName("PART_BORDER_BOTTOM", this) as Rectangle;
            if(bottom != null)
            {
                bottom.MouseLeftButtonDown += BorderCtrl_MouseLeftButtonDown;
                bottom.MouseLeftButtonUp += BorderCtrl_MouseLeftButtonUp;
                bottom.MouseMove += Bottom_MouseMove;
            }
            var lt = Template.FindName("PART_BORDER_LT", this) as Rectangle;
            if(lt != null)
            {
                lt.MouseLeftButtonDown += BorderCtrl_MouseLeftButtonDown;
                lt.MouseLeftButtonUp += BorderCtrl_MouseLeftButtonUp;
                lt.MouseMove += Lt_MouseMove;
            }
            var lb = Template.FindName("PART_BORDER_LB", this) as Rectangle;
            if(lb != null)
            {
                lb.MouseLeftButtonDown += BorderCtrl_MouseLeftButtonDown;
                lb.MouseLeftButtonUp += BorderCtrl_MouseLeftButtonUp;
                lb.MouseMove += Lb_MouseMove;
            }
            var rt = Template.FindName("PART_BORDER_RT", this) as Rectangle;
            if(rt != null)
            {
                rt.MouseLeftButtonDown += BorderCtrl_MouseLeftButtonDown;
                rt.MouseLeftButtonUp += BorderCtrl_MouseLeftButtonUp;
                rt.MouseMove += Rt_MouseMove;
            }
            var rb = Template.FindName("PART_BORDER_RB", this) as Rectangle;
            if(rb != null)
            {
                rb.MouseLeftButtonDown += BorderCtrl_MouseLeftButtonDown;
                rb.MouseLeftButtonUp += BorderCtrl_MouseLeftButtonUp;
                rb.MouseMove += Rb_MouseMove;
            }
        }

        protected virtual void OnSizeChanged(double width, double height) { }
        private void Rb_MouseMove(object sender, MouseEventArgs e)
        {
            var elm = sender as System.Windows.IInputElement;
            if (Mouse.Captured == elm)
            {
                var newPos = e.GetPosition(elm);
                var delta = newPos - mMouseLeftButtonDownPos;
                var deltaWidth = this.Width + delta.X;
                if (deltaWidth < this.MinWidth)
                {
                    deltaWidth = this.MinWidth;
                    delta.X = this.Width - deltaWidth;
                }
                var deltaHeight = this.Height + delta.Y;
                if (deltaHeight < this.MinHeight)
                {
                    deltaHeight = this.MinHeight;
                    delta.Y = this.Height - deltaHeight;
                }

                this.Width = deltaWidth;
                this.Height = deltaHeight;

                OnSizeChanged(Width, Height);
            }
            e.Handled = true;
        }

        private void Rt_MouseMove(object sender, MouseEventArgs e)
        {
            var elm = sender as System.Windows.IInputElement;
            if (Mouse.Captured == elm)
            {
                var newPos = e.GetPosition(elm);
                var delta = newPos - mMouseLeftButtonDownPos;
                var deltaWidth = this.Width + delta.X;
                if (deltaWidth < this.MinWidth)
                {
                    deltaWidth = this.MinWidth;
                    delta.X = this.Width - deltaWidth;
                }
                var deltaHeight = this.Height - delta.Y;
                if (deltaHeight < this.MinHeight)
                {
                    deltaHeight = this.MinHeight;
                    delta.Y = this.Height - deltaHeight;
                }

                this.Width = deltaWidth;
                var top = Canvas.GetTop(this);
                Canvas.SetTop(this, top + delta.Y);
                this.Height = deltaHeight;

                OnSizeChanged(Width, Height);
            }
            e.Handled = true;
        }

        private void Lb_MouseMove(object sender, MouseEventArgs e)
        {
            var elm = sender as System.Windows.IInputElement;
            if (Mouse.Captured == elm)
            {
                var newPos = e.GetPosition(elm);
                var delta = newPos - mMouseLeftButtonDownPos;
                var deltaWidth = this.Width - delta.X;
                if (deltaWidth < this.MinWidth)
                {
                    deltaWidth = this.MinWidth;
                    delta.X = this.Width - deltaWidth;
                }
                var deltaHeight = this.Height + delta.Y;
                if (deltaHeight < this.MinHeight)
                {
                    deltaHeight = this.MinHeight;
                    delta.Y = this.Height - deltaHeight;
                }

                var left = Canvas.GetLeft(this);
                Canvas.SetLeft(this, left + delta.X);
                this.Width = deltaWidth;
                this.Height = deltaHeight;

                OnSizeChanged(Width, Height);
            }
            e.Handled = true;
        }

        private void Lt_MouseMove(object sender, MouseEventArgs e)
        {
            var elm = sender as System.Windows.IInputElement;
            if (Mouse.Captured == elm)
            {
                var newPos = e.GetPosition(elm);
                var delta = newPos - mMouseLeftButtonDownPos;
                var deltaWidth = this.Width - delta.X;
                if (deltaWidth < this.MinWidth)
                {
                    deltaWidth = this.MinWidth;
                    delta.X = this.Width - deltaWidth;
                }
                var deltaHeight = this.Height - delta.Y;
                if(deltaHeight < this.MinHeight)
                {
                    deltaHeight = this.MinHeight;
                    delta.Y = this.Height - deltaHeight;
                }

                var left = Canvas.GetLeft(this);
                Canvas.SetLeft(this, left + delta.X);
                this.Width = deltaWidth;

                var top = Canvas.GetTop(this);
                Canvas.SetTop(this, top + delta.Y);
                this.Height = deltaHeight;

                OnSizeChanged(Width, Height);
            }
            e.Handled = true;
        }

        private void Bottom_MouseMove(object sender, MouseEventArgs e)
        {
            var elm = sender as System.Windows.IInputElement;
            if (Mouse.Captured == elm)
            {
                var newPos = e.GetPosition(elm);
                var deltaY = newPos.Y - mMouseLeftButtonDownPos.Y;
                var deltaHeight = this.Height + deltaY;
                if (deltaHeight < this.MinHeight)
                {
                    deltaHeight = this.MinHeight;
                    deltaY = this.Height - deltaHeight;
                }

                this.Height = deltaHeight;

                OnSizeChanged(Width, Height);
            }
            e.Handled = true;
        }

        private void Right_MouseMove(object sender, MouseEventArgs e)
        {
            var elm = sender as System.Windows.IInputElement;
            if (Mouse.Captured == elm)
            {
                var newPos = e.GetPosition(elm);
                var deltaX = newPos.X - mMouseLeftButtonDownPos.X;
                var deltaWidth = this.Width + deltaX;
                if (deltaWidth < this.MinWidth)
                {
                    deltaWidth = this.MinWidth;
                    deltaX = this.Width - deltaWidth;
                }

                this.Width = deltaWidth;

                OnSizeChanged(Width, Height);
            }
            e.Handled = true;
        }

        private void Top_MouseMove(object sender, MouseEventArgs e)
        {
            var elm = sender as System.Windows.IInputElement;
            if (Mouse.Captured == elm)
            {
                var newPos = e.GetPosition(elm);
                var deltaY = newPos.Y - mMouseLeftButtonDownPos.Y;
                var deltaHeight = this.Height - deltaY;
                if (deltaHeight < this.MinHeight)
                {
                    deltaHeight = this.MinHeight;
                    deltaY = this.Height - deltaHeight;
                }

                var top = Canvas.GetTop(this);
                Canvas.SetTop(this, top + deltaY);
                this.Height = deltaHeight;

                OnSizeChanged(Width, Height);
            }
            e.Handled = true;
        }

        private void Left_MouseMove(object sender, MouseEventArgs e)
        {
            var elm = sender as System.Windows.IInputElement;
            if (Mouse.Captured == elm)
            {
                var newPos = e.GetPosition(elm);
                var deltaX = newPos.X - mMouseLeftButtonDownPos.X;
                var deltaWidth = this.Width - deltaX;
                if(deltaWidth < this.MinWidth)
                {
                    deltaWidth = this.MinWidth;
                    deltaX = this.Width - deltaWidth;
                }

                var left = Canvas.GetLeft(this);
                Canvas.SetLeft(this, left + deltaX);
                this.Width = deltaWidth;

                OnSizeChanged(Width, Height);
            }
            e.Handled = true;
        }

        Point mMouseLeftButtonDownPos;
        private void BorderCtrl_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
            e.Handled = true;
        }

        private void BorderCtrl_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var elm = sender as System.Windows.IInputElement;
            mMouseLeftButtonDownPos = e.GetPosition(elm);
            Mouse.Capture(elm);
            e.Handled = true;
        }
    }
}
