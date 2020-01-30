using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CodeGenerateSystem.Controls
{
    public sealed partial class CommentNode
    {
        partial void SetComment_WPF(string cmt)
        {
            if (string.IsNullOrEmpty(cmt))
            {
                TextBlock_Comment.Visibility = Visibility.Collapsed;
                TextBlock_Tips.Visibility = Visibility.Visible;
            }
            else
            {
                TextBlock_Comment.Visibility = Visibility.Visible;
                TextBlock_Tips.Visibility = Visibility.Collapsed;
            }
        }

        partial void InitConstruction()
        {
            this.InitializeComponent();

            Canvas.SetZIndex(this, -99);
        }

        partial void Save_WPF(EngineNS.IO.XndNode xndNode)
        {
            var att = xndNode.AddAttrib("_commentNodeData_WPF");
            att.Version = 0;
            att.BeginWrite();
            att.Write((double)this.ActualWidth);
            att.Write((double)this.ActualHeight);
            att.EndWrite();
        }
        partial void Load_WPF(EngineNS.IO.XndNode xndNode)
        {
            var att = xndNode.FindAttrib("_commentNodeData_WPF");
            if(att != null)
            {
                switch (att.Version)
                {
                    case 0:
                        {
                            double width, height;
                            att.Read(out width);
                            att.Read(out height);
                            this.Width = width;
                            this.Height = height;
                        }
                        break;
                }
            }
        }
        public override double GetHeight(bool withChildren = true)
        {
            return this.ActualHeight + Grid_Title.ActualHeight;
        }

        private void Grid_Title_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            Grid_Title.Margin = new System.Windows.Thickness(0, -e.NewSize.Height, 0, 0);
        }

        bool mDrag = false;
        System.Windows.Point mDragStartPosition = new System.Windows.Point();
        private void Rect_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed)
            {
                var element = sender as FrameworkElement;
                Mouse.Capture(element);
                mDragStartPosition = e.GetPosition(element);
                mDrag = true;
                e.Handled = true;
            }
        }

        private void Rect_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
            e.Handled = true;
            mDrag = false;
        }

        private void Rect_LT_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed && mDrag)
            {
                var pos = e.GetPosition(Rect_LT);
                var deltaX = pos.X - mDragStartPosition.X;
                var left = Canvas.GetLeft(this);
                Canvas.SetLeft(this, left + deltaX);
                this.Width = this.ActualWidth - deltaX;
                var deltaY = pos.Y - mDragStartPosition.Y;
                var top = Canvas.GetTop(this);
                Canvas.SetTop(this, top + deltaY);
                this.Height = this.ActualHeight - deltaY;
            }
        }

        private void Rect_L_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && mDrag)
            {
                var pos = e.GetPosition(Rect_L);
                var deltaX = pos.X - mDragStartPosition.X;
                var left = Canvas.GetLeft(this);
                Canvas.SetLeft(this, left + deltaX);
                this.Width = this.ActualWidth - deltaX;
            }
        }

        private void Rect_LB_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed && mDrag)
            {
                var pos = e.GetPosition(Rect_LB);
                var deltaX = pos.X - mDragStartPosition.X;
                var left = Canvas.GetLeft(this);
                Canvas.SetLeft(this, left + deltaX);
                this.Width = this.ActualWidth - deltaX;
                var deltaY = pos.Y - mDragStartPosition.Y;
                this.Height = this.ActualHeight + deltaY;
            }
        }

        private void Rect_B_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed && mDrag)
            {
                var pos = e.GetPosition(Rect_B);
                var deltaY = pos.Y - mDragStartPosition.Y;
                this.Height = this.ActualHeight + deltaY;
            }
        }

        private void Rect_RB_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed && mDrag)
            {
                var pos = e.GetPosition(Rect_RB);
                var deltaX = pos.X - mDragStartPosition.X;
                this.Width = this.ActualWidth + deltaX;
                var deltaY = pos.Y - mDragStartPosition.Y;
                this.Height = this.ActualHeight + deltaY;
            }
        }

        private void Rect_R_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed && mDrag)
            {
                var pos = e.GetPosition(Rect_R);
                var deltaX = pos.X - mDragStartPosition.X;
                this.Width = this.ActualWidth + deltaX;
            }
        }

        private void Rect_RT_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && mDrag)
            {
                var pos = e.GetPosition(Rect_RT);
                var deltaX = pos.X - mDragStartPosition.X;
                this.Width = this.ActualWidth + deltaX;
                var deltaY = pos.Y - mDragStartPosition.Y;
                var top = Canvas.GetTop(this);
                Canvas.SetTop(this, top + deltaY);
                this.Height = this.ActualHeight - deltaY;
            }
        }

        private void Rect_T_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed && mDrag)
            {
                var pos = e.GetPosition(Rect_T);
                var deltaY = pos.Y - mDragStartPosition.Y;
                var top = Canvas.GetTop(this);
                Canvas.SetTop(this, top + deltaY);
                this.Height = this.ActualHeight - deltaY;
            }
        }

        private void Rectangle_Title_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
            {
                // 双击标题编写注释
                if(TextBox_Comment.Visibility != Visibility.Visible)
                {
                    TextBox_Comment.Visibility = Visibility.Visible;
                    TextBlock_Comment.Visibility = Visibility.Hidden;
                    TextBlock_Tips.Visibility = Visibility.Collapsed;
                    Keyboard.Focus(TextBox_Comment);
                    TextBox_Comment.SelectAll();
                    mOldComment = Comment;
                }
            }
        }

        private void TextBox_Comment_KeyDown(object sender, KeyEventArgs e)
        {
            switch(e.Key)
            {
                case Key.Enter:
                    {
                        TextBox_Comment.Visibility = Visibility.Hidden;
                        TextBlock_Comment.Visibility = Visibility.Visible;
                        if (string.IsNullOrEmpty(Comment))
                            TextBlock_Tips.Visibility = Visibility.Visible;
                    }
                    break;
                case Key.Escape:
                    {
                        TextBox_Comment.Visibility = Visibility.Hidden;
                        TextBlock_Comment.Visibility = Visibility.Visible;
                        Comment = mOldComment;
                        if (string.IsNullOrEmpty(Comment))
                            TextBlock_Tips.Visibility = Visibility.Visible;
                    }
                    break;
            }
        }

        private void TextBox_Comment_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            TextBox_Comment.Visibility = Visibility.Hidden;
            TextBlock_Comment.Visibility = Visibility.Visible;
            if (string.IsNullOrEmpty(Comment))
                TextBlock_Tips.Visibility = Visibility.Visible;
        }

        List<Base.BaseNodeControl> mContainNodes = new List<Base.BaseNodeControl>();
        protected override void StartDrag(UIElement dragObj, MouseButtonEventArgs e)
        {
            base.StartDrag(dragObj, e);

            var start = Border_Container.TranslatePoint(new System.Windows.Point(0, 0), ParentDrawCanvas);
            var end = Border_Container.TranslatePoint(new System.Windows.Point(Border_Container.ActualWidth, Border_Container.ActualHeight), ParentDrawCanvas);

            foreach(var node in this.HostNodesContainer.CtrlNodeList)
            {
                var loc = node.GetLocation();
                var right = node.GetWidth(false) + loc.X;
                var bottom = node.GetHeight(false) + loc.Y;

                if(loc.X > start.X && loc.Y > start.Y && right < end.X && bottom < end.Y)
                {
                    mContainNodes.Add(node);
                    node.CalculateDeltaPt(e);
                }
            }
        }

        protected override void DragMove(MouseEventArgs e)
        {
            base.DragMove(e);

            foreach(var node in mContainNodes)
            {
                var pt = e.GetPosition(ParentDrawCanvas);
                node.MoveWithPt(pt);
            }
        }

        protected override void EndDrag()
        {
            base.EndDrag();

            mContainNodes.Clear();
        }
    }
}
