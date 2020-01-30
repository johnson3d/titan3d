using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.ComponentModel;
using EngineNS.Bricks.Animation;

namespace EditorCommon.Controls.TimeLine
{
    /// <summary>
    /// Interaction logic for TimeLineTrackItem.xaml
    /// </summary>
    public partial class TimeLineTrackItem : UserControl, INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        public delegate void Delegate_OnUpdateTimeLinkTrackItemActiveShow(TimeLineTrackItem item);
        public Delegate_OnUpdateTimeLinkTrackItemActiveShow OnUpdateTimeLineTrackItemActiveShow;

        public delegate void Delegate_RemoveTimeLineTrackItem(TimeLineTrackItem item);
        public Delegate_RemoveTimeLineTrackItem OnRemoveTimeLineTrackItem;

        public delegate void Delegate_OnSelected(TimeLineTrackItem item);
        public Delegate_OnSelected OnSelected;

        public delegate void Delegate_OnUnSelected(TimeLineTrackItem item);        
        public delegate void Delegate_OnCreateTimeLinkTrackItem(TimeLineTrackItem item);

        bool mIsSelected = false;
        public bool IsSelected
        {
            get { return mIsSelected; }
            set
            {
                mIsSelected = value;

                if (mIsSelected)
                {
                    if (CanModifyLength && IsModifyLength)
                    {
                        Border_LengthKerframe.BorderBrush = FindResource("SelectedColor") as Brush;
                    }
                    else
                    {
                        Ellipse_PointKeyframe.Fill = FindResource("SelectedColor") as Brush;
                    }                    
                    if (OnSelected != null)
                        OnSelected(this);
                }
                else
                {
                    if (CanModifyLength && IsModifyLength)
                    {
                        Border_LengthKerframe.BorderBrush = FindResource("NormalColor") as Brush;
                    }
                    else
                    {
                        Ellipse_PointKeyframe.Fill = FindResource("NormalColor") as Brush;
                    }                    
                }

                OnPropertyChanged("IsSelected");
            }
        }

        Int64 mFrameStart = 0;
        public Int64 FrameStart
        {
            get { return mFrameStart; }
            set
            {
                mFrameStart = value;

                if (mKeyFrameItem != null)
                {
                    mKeyFrameItem.KeyFrameMilliTimeStart = (Int64)System.Math.Round(mFrameStart * 1000.0 / mHostTimeLineTrack.HostControl.FPS);
                }                

                OnPropertyChanged("FrameStart");
            }
        }

        Int64 mFrameEnd = 0;
        public Int64 FrameEnd
        {
            get { return mFrameEnd; }
            set
            {
                mFrameEnd = value;

                if (mKeyFrameItem != null)
                {
                    mKeyFrameItem.KeyFrameMilliTimeEnd = (Int64)System.Math.Round(mFrameEnd * 1000.0 / mHostTimeLineTrack.HostControl.FPS);
                }

                OnPropertyChanged("FrameEnd");
            }
        }        

        bool mIsActive = false;
        public bool IsActive
        {
            get { return mIsActive; }
            set
            {
                mIsActive = value;

                if (mTimeLineItemProCtrl != null)
                {
                    mTimeLineItemProCtrl.IsActive = value;
                }

                if (OnUpdateTimeLineTrackItemActiveShow != null)
                    OnUpdateTimeLineTrackItemActiveShow(this);

                OnPropertyChanged("IsActive");
            }
        }

        bool mCanModifyLength = true;
        public bool CanModifyLength
        {
            get { return mCanModifyLength; }
            set
            {
                mCanModifyLength = value;                

                if (value)
                {                                    
                    //var notiyPoint = mKeyFrameItem as EditorCommon.Controls.Animation.NotifyPoint;
                    //if (notiyPoint != null)
                    //{
                    //    if(mHostTimeLineTrack.HostControl.TotalFrame >= 1)
                    //    {
                    //        IsModifyLength = notiyPoint.IsModifyLength;
                    //    }
                    //    else
                    //    {
                    //        CanModifyLength = false;
                    //    }                        
                    //}
                }
                else
                {
                    Border_LengthKerframe.Visibility = System.Windows.Visibility.Collapsed;
                    Ellipse_PointKeyframe.Visibility = System.Windows.Visibility.Visible;
                }
            }
        }

        TimeLineTrackItemPropertyControl_Base mTimeLineItemProCtrl = null;
        public TimeLineTrackItemPropertyControl_Base TimeLineItemProCtrl
        {
            get { return mTimeLineItemProCtrl; }
        }

        TimeLineTrack mHostTimeLineTrack;
        public TimeLineTrack HostTimeLineTrack
        {
            get { return mHostTimeLineTrack; }
        }

        TimeLineKeyFrameObjectInterface mKeyFrameItem;
        public TimeLineKeyFrameObjectInterface KeyFrameItem
        {
            get { return mKeyFrameItem; }
        }

        public TimeLineTrackItem(TimeLineTrack hostTimeLineTrack, TimeLineKeyFrameObjectInterface keyFrameItem)
        {
            InitializeComponent();

            mHostTimeLineTrack = hostTimeLineTrack;
            mKeyFrameItem = keyFrameItem;

//             var ecType = Type.GetType(keyFrameItem.GetTimeLineKeyFrameObjectEditorControlType());
//             if (ecType != null)
//             {
//                 mTimeLineItemProCtrl = System.Activator.CreateInstance(ecType) as TimeLineTrackItemPropertyControl_Base;
//                 mTimeLineItemProCtrl.HostTimeLineTrackItem = this;
//                 mTimeLineItemProCtrl.PropertyInstance = keyFrameItem;
//                 //Grid_ControlContainer.Children.Add(mTimeLineItemProCtrl);
//             }

            //ProGrid.Instance = keyFrameItem;

            FrameStart = (Int64)System.Math.Round(mKeyFrameItem.KeyFrameMilliTimeStart * 0.001 * hostTimeLineTrack.HostControl.FPS);
            FrameEnd = (Int64)System.Math.Round(mKeyFrameItem.KeyFrameMilliTimeEnd * 0.001 * hostTimeLineTrack.HostControl.FPS);
            CanModifyLength = mKeyFrameItem.CanModityLength();
            ToolTip = keyFrameItem.UpdateToolTip();
        }

        public void UpdateTrackActive(Int64 curFrame)
        {
            if (curFrame >= FrameStart && curFrame <= FrameEnd)
                IsActive = true;
            else
                IsActive = false;
        }

#region 鼠标拖动操作

        Point mMouseLeftButtonDown;
        private void UserControl_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {           
            IsSelected = true;

            Mouse.Capture(sender as UIElement);
            mMouseLeftButtonDown = e.GetPosition(sender as UIElement);
        }

        private void UserControl_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
        }

        private void UserControl_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var frameLength = FrameEnd - FrameStart;
                var posDelta = e.GetPosition(sender as UIElement).X - mMouseLeftButtonDown.X;

                var frameDelta = (Int64)System.Math.Round(posDelta / mHostTimeLineTrack.ActualWidth * mHostTimeLineTrack.HostControl.TotalFrame);
                if (FrameEnd + frameDelta > mHostTimeLineTrack.HostControl.TotalFrame)
                {
                    frameDelta = mHostTimeLineTrack.HostControl.TotalFrame - FrameEnd;
                }
                else if (FrameStart + frameDelta < 0)
                {
                    frameDelta = -FrameStart;
                }

                if (mHostTimeLineTrack.MovedFrameStartIsLegitimate(FrameStart, frameDelta))
                {
                    FrameStart += frameDelta;
                    FrameEnd = FrameStart + frameLength;
                    
                    mHostTimeLineTrack.UpdateTrackShow();
                }
            }
        }

        private void Rectangle_StartSet_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {            
            IsSelected = true;

            Mouse.Capture(sender as UIElement);
            mMouseLeftButtonDown = e.GetPosition(sender as UIElement);
            e.Handled = true;
        }

        private void Rectangle_StartSet_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
            e.Handled = true;
        }

        private void Rectangle_StartSet_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var point = e.GetPosition(sender as UIElement);

                var delta = point.X - mMouseLeftButtonDown.X;
                var start = FrameStart + (Int64)(delta / mHostTimeLineTrack.ActualWidth * mHostTimeLineTrack.HostControl.TotalFrame);
                if (start >= 0 && start < FrameEnd)
                    FrameStart = (Int64)start;

                mHostTimeLineTrack.UpdateTrackShow();
            }
            e.Handled = true;
        }

        private void Rectangle_EndSet_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {            
            IsSelected = true;

            Mouse.Capture(sender as UIElement);
            mMouseLeftButtonDown = e.GetPosition(sender as UIElement);
            e.Handled = true;
        }

        private void Rectangle_EndSet_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
            e.Handled = true;
        }

        private void Rectangle_EndSet_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var point = e.GetPosition(sender as UIElement);

                var delta = point.X - mMouseLeftButtonDown.X;
                var end = FrameEnd + (Int64)(delta / mHostTimeLineTrack.ActualWidth * mHostTimeLineTrack.HostControl.TotalFrame);
                if (end > FrameStart && end <= mHostTimeLineTrack.HostControl.TotalFrame)
                    FrameEnd = (Int64)end;

                mHostTimeLineTrack.UpdateTrackShow();
            }
            e.Handled = true;
        }

        private void UserControl_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            //Popup_Propertys.IsOpen = true;
        }

        private void UserControl_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            //Popup_Propertys.IsOpen = false;
        }

        private void Button_RemoveKeyframe_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Popup_Propertys.IsOpen = false;
            if (OnRemoveTimeLineTrackItem != null)
                OnRemoveTimeLineTrackItem(this);
        }

        private void UserControl_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Popup_Propertys.IsOpen = true;

            //var notifyPoint = mKeyFrameItem as EngineNS.Graphics.Mesh.Animation.NotifyPoint;
            //var notifyPoint = null;
            //if (notifyPoint == null)
            //    return;

            //if (mCanModifyLength)
            //{
            //    op_btn.Visibility = Visibility.Visible;
            //    if (notifyPoint.IsModifyLength)
            //    {
            //        op_btn.Content = "单一关键帧";
            //    }
            //    else
            //    {
            //        op_btn.Content = "可变长关键帧";
            //    }
            //}
            //else
            //{
            //    op_btn.Visibility = Visibility.Collapsed;
            //}            
        }

        bool mIsModifyLength;
        public bool IsModifyLength
        {
            get { return mIsModifyLength; }
            set
            {
                mIsModifyLength = value;                
                if (value)
                {
                    Border_LengthKerframe.Visibility = System.Windows.Visibility.Visible;
                    Ellipse_PointKeyframe.Visibility = System.Windows.Visibility.Collapsed;
                }
                else
                {
                    Border_LengthKerframe.Visibility = System.Windows.Visibility.Collapsed;
                    Ellipse_PointKeyframe.Visibility = System.Windows.Visibility.Visible;

                    FrameEnd = FrameStart;
                }

                //var notiyPoint = mKeyFrameItem as CSUtility.Animation.NotifyPoint;
                //if (notiyPoint != null)
                //{
                //    notiyPoint.IsModifyLength = value;
                //}
                IsSelected = IsSelected;
                mHostTimeLineTrack.UpdateTrackShow();
            }
        }
        #endregion

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            IsModifyLength = !IsModifyLength;
        }
    }
}
