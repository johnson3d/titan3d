using EditorCommon.DragDrop;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for AnimationBlendSpaceNodeControl.xaml
    /// </summary>
    public partial class AnimationBlendSpaceNodeControl : UserControl, INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion
        public event EventHandler OnSelected;
        public event EventHandler OnDelete;
        bool mIsSelected = false;
        public bool IsSelected
        {
            get => mIsSelected;
            set
            {
                mIsSelected = value;
                if (mIsSelected)
                {
                    Rect.Fill = new SolidColorBrush(Color.FromRgb(0, 255, 0));
                }
                else
                {
                    if (mIsValided)
                        Rect.Fill = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                    else
                        Rect.Fill = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                }
            }
        }
        bool mIsValided = true;
        public bool IsValided
        {
            get => mIsValided;
            set
            {
                mIsValided = value;
                if (mIsValided)
                {
                    if (mIsSelected)
                        Rect.Fill = new SolidColorBrush(Color.FromRgb(0, 255, 0));
                    else
                        Rect.Fill = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                }
                else
                {
                    Rect.Fill = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                }
            }
        }
        BlendSpaceEditorNode mHostNode = null;
        public BlendSpaceEditorNode HostNode
        {
            get => mHostNode;
            set
            {
                mHostNode = value;
                ProGrid.Instance = value;
            }
        }
        string mContentName = "";
        bool mRightSide = true;
        public AnimationBlendSpaceNodeControl()
        {
            InitializeComponent();
        }

        public string ContentName
        {
            get => mContentName;
            set
            {
                mContentName = value;
                if (mRightSide)
                {
                    LeftName.Text = "";
                    RightName.Text = mContentName;
                }
                if (!mRightSide)
                {
                    LeftName.Text = mContentName;
                    RightName.Text = "";
                }
                OnPropertyChanged("ContentName");
            }
        }
        public bool RightSide
        {
            get => mRightSide;
            set
            {
                mRightSide = value;
                if (mRightSide)
                {
                    LeftName.Text = "";
                    RightName.Text = ContentName;
                }
                if (!mRightSide)
                {
                    LeftName.Text = ContentName;
                    RightName.Text = "";
                }
            }
        }
        bool mHideContent = false;
        public bool HideContent
        {
            get => mHideContent;
            set
            {
                mHideContent = value;
                if (mHideContent)
                {
                    LeftName.Visibility = Visibility.Hidden;
                    RightName.Visibility = Visibility.Hidden;
                }
                else
                {
                    LeftName.Visibility = Visibility.Visible;
                    RightName.Visibility = Visibility.Visible;
                }
            }
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UserControlTranslateTransform.X = -this.ActualWidth / 2.0f;
            UserControlTranslateTransform.Y = -this.ActualHeight / 2.0f;
        }

        private void Rect_DragEnter(object sender, DragEventArgs e)
        {

        }

        private void Rect_DragLeave(object sender, DragEventArgs e)
        {

        }

        private void Rect_DragOver(object sender, DragEventArgs e)
        {

        }

        private void Rect_Drop(object sender, DragEventArgs e)
        {

        }

        bool IsLButtonDown = false;
        Point mMouseLeftButtonDownPointInTreeView;
        Int64 mMouseButtonDownTime = 0;
        //bool IsDrag = false;
        private void Rect_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                mMouseButtonDownTime = EngineNS.CEngine.Instance.EngineTime;
                IsLButtonDown = true;
                var elm = sender as FrameworkElement;
                mMouseLeftButtonDownPointInTreeView = e.GetPosition(elm);
                Mouse.Capture(elm);
            }
        }

        private void Rect_MouseMove(object sender, MouseEventArgs e)
        {
            if (IsLButtonDown)
            {
                //IsDrag = true;
                var drags = new IDragAbleObject[1];
                drags[0] = HostNode;
                EditorCommon.DragDrop.DragDropManager.Instance.StartDrag(EditorCommon.Program.ResourcItemDragType, drags);
                IsLButtonDown = false;
                Mouse.Capture(null);
            }
        }

        private void Rect_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (IsLButtonDown)
            {
                IsLButtonDown = false;
                OnSelected?.Invoke(HostNode, new EventArgs());
            }
            Mouse.Capture(null);
        }

        private void Rect_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!IsSelected && IsValided)
                Rect.Fill = new SolidColorBrush(Color.FromRgb(255, 160, 0));
        }

        private void Rect_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!IsSelected && IsValided)
                Rect.Fill = new SolidColorBrush(Color.FromRgb(255, 255, 255));
        }

        private void Rect_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            //MenuPopup.IsOpen = true;
        }
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            OnDelete?.Invoke(HostNode, new EventArgs());
            ContexMenu.IsOpen = false;
        }

        private void Rect_LostFocus(object sender, RoutedEventArgs e)
        {
        }
    }
}
