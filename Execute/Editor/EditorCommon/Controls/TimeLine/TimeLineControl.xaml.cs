using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.ComponentModel;
using EngineNS.Bricks.Animation;

namespace EditorCommon.Controls.TimeLine
{
    /// <summary>
    /// Interaction logic for TimeLineControl.xaml
    /// </summary>
    public partial class TimeLineControl : UserControl, INotifyPropertyChanged, EngineNS.ITickInfo
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        #region TickInfo

        public bool EnableTick { get; set; }
        public EngineNS.Profiler.TimeScope GetLogicTimeScope()
        {
            return null;
        }
        public void BeforeFrame()
        {

        }
        public void TickLogic()
        {
            if (Playing)
            {
                var milliTime = EngineNS.CEngine.Instance.EngineElapseTime;
                mCurrentMillisecondTime += milliTime - mLastMilliTickTime;
                mLastMilliTickTime = milliTime;

                var oldFrame = CurrentFrame;
                var frame = (Int64)System.Math.Ceiling(mCurrentMillisecondTime * 0.001 * FPS);

                if (frame > TotalFrame)
                {
                    if (PlayLoop)
                    {
                        if (TotalFrame == 0)
                        {
                            CurrentFrame = 0;
                            mCurrentMillisecondTime = 0;
                        }
                        else
                        {
                            CurrentFrame = frame - (frame / TotalFrame * TotalFrame);//frame - TotalFrame;
                            mCurrentMillisecondTime = (Int64)((CurrentFrame * 1.0 / TotalFrame) / TotalMilliTime);
                            //mCurrentMillisecondTime = mCurrentMillisecondTime - (mCurrentMillisecondTime / TotalMilliTime * TotalMilliTime);//mCurrentMillisecondTime - TotalMilliTime;
                        }
                    }
                    else
                    {
                        CurrentFrame = 0;
                        mCurrentMillisecondTime = 0;
                        Playing = false;
                    }
                }
                else
                    CurrentFrame = frame;

                ////////////////////////////////////////////////////
                //if(frame < oldFrame)
                //{
                //    System.Diagnostics.Debug.WriteLine("===============================================");
                //}
                //System.Diagnostics.Debug.WriteLine($"old:{oldFrame},new:{frame}");
                /////////////////////////////////////////////////////
            }
        }
        public void TickRender()
        {

        }
        async System.Threading.Tasks.Task DirtyProcess(bool force = false, bool needGenerateCode = true)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
        }
        async System.Threading.Tasks.Task DirtyProcessAsync(bool force = false, bool needGenerateCode = true)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
        }
        //bool mNeedDirtyProcess = false;
        public void TickSync()
        {
            var noUse = DirtyProcess();
        }
        #endregion

        public TimeLineTrackItem.Delegate_OnUpdateTimeLinkTrackItemActiveShow OnUpdateTimeLinkTrackItemActiveShow;
        public TimeLineTrackItem.Delegate_RemoveTimeLineTrackItem OnRemoveTimeLineTrackItem;
        public TimeLineTrackItem.Delegate_OnSelected OnTimeLineTrackItemSelected;
        public TimeLineTrackItem.Delegate_OnUnSelected OnTimeLineTrackItemUnSelected;
        public TimeLineTrackItem.Delegate_OnCreateTimeLinkTrackItem OnCreateTimeLineTrackItem;

        public delegate void Delegate_OnAddFrame(TimeLineKeyFrameObjectInterface keyObj);
        public Delegate_OnAddFrame OnAddFrame;
        public delegate void Delegate_OnSelectedTimeLineObject(TimeLineObjectInterface obj);
        public Delegate_OnSelectedTimeLineObject OnSelectedTimeLineObject;
        public delegate void Delegate_OnCurrentFrameChanged(Int64 currentFrame);
        public Delegate_OnCurrentFrameChanged OnCurrentFrameChanged;
        //public delegate CSUtility.Animation.TimeLineKeyFrameObjectInterface Delegate_OnAddFrame(Int64 startMilliTime);
        //public Delegate_OnAddFrame OnAddKeyFrame;

        TimeLineConfigWindow mConfigWindow;

        bool mPlayLoop = false;
        public bool PlayLoop
        {
            get { return mPlayLoop; }
            set
            {
                mPlayLoop = value;
                OnPropertyChanged("PlayLoop");
            }
        }

        Int64 mFrequency = 5;
        public Int64 Frequency
        {
            get { return mFrequency; }
            set
            {
                mFrequency = value;

                UpdateShowNumbers();

                OnPropertyChanged("Frequency");
            }
        }

        Int64 mFPS = 30;
        public Int64 FPS
        {
            get { return mFPS; }
            set
            {
                mFPS = value;
                OnPropertyChanged("FPS");
            }
        }

        // 单位：毫秒
        Int64 mTotalMilliTime = (Int64)(100 / 30.0f * 1000);
        public Int64 TotalMilliTime
        {
            get { return mTotalMilliTime; }
            set
            {
                mTotalMilliTime = value;
                TotalFrame = (Int64)System.Math.Round(mTotalMilliTime / 1000.0 * FPS);

                OnPropertyChanged("TotalMilliTime");
            }
        }

        // 单位：帧
        Int64 mTotalFrame = 100;
        public Int64 TotalFrame
        {
            get { return mTotalFrame; }
            set
            {
                mTotalFrame = value;
                ViewFrameStart = 0;
                ViewFrameEnd = mTotalFrame;
                UpdateShowNumbers();

                UpdateSliderItemAndFrameShow();

                OnPropertyChanged("TotalFrame");
            }
        }

        Int64 mViewFrameStart = 0;
        public Int64 ViewFrameStart
        {
            get { return mViewFrameStart; }
            set
            {
                if (value >= ViewFrameEnd - 5)
                    return;

                mViewFrameStart = value;
                if (mViewFrameStart < 0)
                    mViewFrameStart = 0;

                UpdateSliderItemAndFrameShow();

                OnPropertyChanged("ViewFrameStart");
            }
        }

        Int64 mViewFrameEnd = 0;
        public Int64 ViewFrameEnd
        {
            get { return mViewFrameEnd; }
            set
            {
                if (value <= ViewFrameStart + 5)
                    return;

                mViewFrameEnd = value;

                UpdateSliderItemAndFrameShow();

                OnPropertyChanged("ViewFrameEnd");
            }
        }

        public Int64 CurrentFrame
        {
            get { return (Int64)GetValue(CurrentFrameProperty); }
            set
            {
                SetValue(CurrentFrameProperty, value);
            }
        }

        public static readonly DependencyProperty CurrentFrameProperty =
            DependencyProperty.Register("CurrentFrame", typeof(Int64), typeof(TimeLineControl),
                                                        new FrameworkPropertyMetadata((Int64)0, new PropertyChangedCallback(_OnCurrentFrameChanged)
                                        ));

        public static void _OnCurrentFrameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as TimeLineControl;

            Int64 frame = (Int64)e.NewValue;
            if (frame < 0)
                frame = 0;
            double left = 0;
            if (ctrl.TotalFrame != 0)
                left = frame * ctrl.Canvas_Draw.ActualWidth / ctrl.TotalFrame;
            Canvas.SetLeft(ctrl.TimeLineShow, left);

            // 更新各节点的Active状态
            foreach (TimeLineTrack track in ctrl.StackPanel_Tracks.Children)
            {
                track.UpdateTrackItemActive(frame);
            }

            // 根据当前帧来设置显示区域
            if (frame < ctrl.ViewFrameStart)
            {
                ctrl.ViewFrameEnd = ctrl.ViewFrameEnd - ctrl.ViewFrameStart + frame;
                ctrl.ViewFrameStart = frame;
            }
            else if (frame > ctrl.ViewFrameEnd)
            {
                ctrl.ViewFrameStart = frame - (ctrl.ViewFrameEnd - ctrl.ViewFrameStart);
                ctrl.ViewFrameEnd = frame;
            }

            if (ctrl.OnCurrentFrameChanged != null)
                ctrl.OnCurrentFrameChanged(frame);
        }

        public bool Playing
        {
            get { return (bool)GetValue(PlayingProperty); }
            set
            {
                SetValue(PlayingProperty, value);
            }
        }
        public static readonly DependencyProperty PlayingProperty =
            DependencyProperty.Register("Playing", typeof(bool), typeof(TimeLineControl),
                                                        new FrameworkPropertyMetadata(false, new PropertyChangedCallback(_OnPlayingChanged)
                                        ));

        public static void _OnPlayingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as TimeLineControl;

            ctrl.mCurrentMillisecondTime = (Int64)(ctrl.CurrentFrame * 1.0 / ctrl.FPS * 1000);
            ctrl.mLastMilliTickTime = EngineNS.CEngine.Instance.EngineElapseTime;
        }

        public TimeLineControl()
        {
            InitializeComponent();


            mConfigWindow = new TimeLineConfigWindow(this);

            //MaxTime = 100;
            //for (int i = 0; i < MaxTime / mFrequency; ++i)
            //{
            //    TextBlock textBlock = new TextBlock()
            //    {
            //        Foreground = new SolidColorBrush(Color.FromArgb(255,145,145,145)),// Brushes.White,
            //        FontSize = 9,
            //        Padding = new Thickness(0),
            //        HorizontalAlignment = HorizontalAlignment.Left,
            //        Text = (i * mFrequency).ToString()
            //    };
            //    TimeLineNumShow.Children.Add(textBlock);
            //}


            ViewFrameStart = 0;
            ViewFrameEnd = TotalFrame;
        }
        ~TimeLineControl()
        {
        }

        public Int64 GetFrameMillisecondTime(Int64 frame)
        {
            //return (Int64)System.Math.Round(frame * 1000.0 / FPS); 
            //插件导出时没有做四舍五入，所以这里统一也不做四舍五入
            return (Int64)(frame * 1000.0 / FPS);
        }

        public void Initialize()
        {
            CurrentFrame = 0;
            mCurrentMillisecondTime = 0;
            //ListBox_Items.Items.Clear();
            //StackPanel_Tracks.Children.Clear();
        }

        public void Cleanup()
        {
            CurrentFrame = 0;
            mCurrentMillisecondTime = 0;
            ListBox_Items.Items.Clear();
            StackPanel_Tracks.Children.Clear();
        }

        //bool mInitialized = false;
        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            //if (!mInitialized)
            //{
            //    var scrollView = VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(ListBox_Items, 0), 0) as ScrollViewer;
            //    var binding = new Binding("VerticalOffset") { Source = scrollView };
            //    var offsetChangeListener = DependencyProperty.RegisterAttached("TimeLineControlListenerVerticalOffset", typeof(object), typeof(UserControl), new PropertyMetadata(OnVerticalScrollChanged));
            //    scrollView.SetBinding(offsetChangeListener, binding);

            //    scrollView.verticalof
            //    BindingOperations.ClearBinding(scrollView, ScrollViewer.VerticalOffsetProperty);
            //    BindingOperations.SetBinding(scrollView, ScrollViewer.VerticalOffsetProperty, new Binding("Value") { Source = ScrollBar_Vertical, Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });

            //    UpdateShowNumbers();
            //    UpdateSliderItemAndFrameShow();

            //    mInitialized = true;
            //}

            //EditorCommon.TickInfo.Instance.AddTickInfo(this);

            UpdateShowNumbers();
        }

        private void UserControl_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
           // EditorCommon.TickInfo.Instance.RemoveTickInfo(this);
        }

        private void UpdateShowNumbers()
        {
            Grid_Numbers.Children.Clear();
            Grid_Numbers.ColumnDefinitions.Clear();

            var numberCount = TotalFrame / Frequency;
            for (int i = 0; i < numberCount; i++)
            {
                ColumnDefinition column = new ColumnDefinition();
                column.Width = new System.Windows.GridLength(1, GridUnitType.Star);
                Grid_Numbers.ColumnDefinitions.Add(column);

                TextBlock textBlock = new TextBlock()
                {
                    HorizontalAlignment = HorizontalAlignment.Left,
                    FontSize = 10,
                    Foreground = new SolidColorBrush(Color.FromArgb(255, 145, 145, 145)),
                    Text = (i * Frequency).ToString(),
                    Margin = new Thickness(1)
                };
                Grid.SetColumn(textBlock, i);

                Grid_Numbers.Children.Add(textBlock);
            }
        }

        public void UpdateSliderItemAndFrameShow()
        {
            // 根据当前的FrameStart和FrameEnd来设置SliderItem和FrameShow
            var containerWidth = Grid_SliderContainer.ActualWidth;

            if (TotalFrame == 0)
            {
                SliderItem.Margin = new Thickness(0);
                Canvas_Draw.Width = Canvas_FrameView.ActualWidth;
                Canvas.SetLeft(Canvas_Draw, 0);
            }
            else
            {
                var left = ViewFrameStart * containerWidth / TotalFrame;
                var right = (TotalFrame - ViewFrameEnd) * containerWidth / TotalFrame;
                SliderItem.Margin = new Thickness(left, SliderItem.Margin.Top, right, SliderItem.Margin.Bottom);

                var frameViewWidth = Canvas_FrameView.ActualWidth;
                Canvas_Draw.Width = frameViewWidth * containerWidth / (containerWidth - left - right);
                Canvas.SetLeft(Canvas_Draw, -ViewFrameStart * Canvas_Draw.Width / TotalFrame);
            }
        }

        private void Button_TimelineConfig_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            mConfigWindow.ShowDialog();
        }

        private void Button_PlayBackup_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            CurrentFrame = 0;
        }

        private void Button_PlayForward_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            CurrentFrame = TotalFrame;
        }

        private void Grid_SliderContainer_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            UpdateSliderItemAndFrameShow();
        }

        private void SliderItem_MouseMove(object sender, MouseEventArgs e)
        {
            //if (e.LeftButton == MouseButtonState.Pressed && EditTimeValue.Visibility != Visibility.Visible)
            //{
            //    var point = e.GetPosition(SliderItem);
            //    var left = SliderItem.Margin.Left + point.X - mMouseLeftButtonDownPt.X;
            //    if (left < 0)
            //        left = 0;
            //    else if ((left + SliderItem.ActualWidth) > TimeLineGrid.ActualWidth)
            //    {
            //        left = TimeLineGrid.ActualWidth - SliderItem.ActualWidth;
            //    }
            //    SliderItem.Margin = new System.Windows.Thickness(left, SliderItem.Margin.Top, SliderItem.Margin.Right, SliderItem.Margin.Bottom);

            //    var Time = left / (TimeLineGrid.ActualWidth - SliderItem.ActualWidth) * MaxTime;
            //    TimeNumShow.Content = Time.ToString("0.000");
            //    var timeLineShowLeft = (TimeLineGrid.ActualWidth - TickBar_SmallTick.Margin.Left - TickBar_SmallTick.Margin.Right) / MaxTime * Time + TickBar_SmallTick.Margin.Left - TimeLineShow.ActualWidth * 0.5;
            //    TimeLineShow.Margin = new System.Windows.Thickness(timeLineShowLeft, TimeLineShow.Margin.Top, TimeLineShow.Margin.Right, TimeLineShow.Margin.Bottom);
            //}
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var point = e.GetPosition(SliderItem);

                var delta = point.X - mMouseLeftButtonDownPt.X;
                var frameDelta = (Int64)(delta / Grid_SliderContainer.ActualWidth * TotalFrame);
                var start = ViewFrameStart + frameDelta;
                var end = ViewFrameEnd + frameDelta;
                if (frameDelta > 0)
                {
                    if (end > TotalFrame)
                    {
                        ViewFrameStart = TotalFrame - (ViewFrameEnd - ViewFrameStart);
                        ViewFrameEnd = TotalFrame;
                    }
                    else
                    {
                        ViewFrameStart = (Int64)start;
                        ViewFrameEnd = (Int64)end;
                    }
                }
                else
                {
                    if (start < 0)
                    {
                        ViewFrameEnd = ViewFrameEnd - ViewFrameStart;
                        ViewFrameStart = 0;
                    }
                    else
                    {
                        ViewFrameStart = (Int64)start;
                        ViewFrameEnd = (Int64)end;
                    }
                }
            }
        }

        Point mMouseLeftButtonDownPt;
        private void SliderItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //if(e.ClickCount == 2)
            //{
            //    TimeNumShow.Visibility = Visibility.Hidden;
            //    EditTimeValue.Visibility = Visibility.Visible;
            //    OKButton.Visibility = Visibility.Visible;
            //    EditTimeValue.Text = TimeNumShow.Content.ToString();
            //}
            //else if(EditTimeValue.Visibility != Visibility.Visible)
            //{
            mMouseLeftButtonDownPt = e.GetPosition(SliderItem);
            Mouse.Capture(SliderItem, CaptureMode.Element);
            //}
        }

        private void SliderItem_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
        }

        private void Rectangle_SliderLeft_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            mMouseLeftButtonDownPt = e.GetPosition(Rectangle_SliderLeft);
            Mouse.Capture(Rectangle_SliderLeft, CaptureMode.Element);
            e.Handled = true;
        }

        private void Rectangle_SliderLeft_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
            e.Handled = true;
        }

        private void Rectangle_SliderLeft_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var point = e.GetPosition(Rectangle_SliderLeft);

                var delta = point.X - mMouseLeftButtonDownPt.X;
                var start = ViewFrameStart + (Int64)(delta / Grid_SliderContainer.ActualWidth * TotalFrame);
                if (start >= 0 && start < ViewFrameEnd)
                    ViewFrameStart = (Int64)start;
            }

            e.Handled = true;
        }

        private void Rectangle_SliderRight_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            mMouseLeftButtonDownPt = e.GetPosition(Rectangle_SliderRight);
            Mouse.Capture(Rectangle_SliderRight, CaptureMode.Element);
            e.Handled = true;
        }

        private void Rectangle_SliderRight_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
            e.Handled = true;
        }

        private void Rectangle_SliderRight_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var point = e.GetPosition(Rectangle_SliderRight);

                var delta = point.X - mMouseLeftButtonDownPt.X;
                var end = ViewFrameEnd + (Int64)(delta / Grid_SliderContainer.ActualWidth * TotalFrame);
                if (end > ViewFrameStart && end <= TotalFrame)
                    ViewFrameEnd = (Int64)end;
            }

            e.Handled = true;
        }

        private void TimeLine_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Mouse.Capture(sender as UIElement);
            CurrentFrame = (Int64)(e.GetPosition(Canvas_Draw).X / Canvas_Draw.ActualWidth * TotalFrame);
        }

        private void TimeLine_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
        }

        private void TimeLine_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (Playing)
                    Playing = false;

                var frame = (Int64)System.Math.Round(e.GetPosition(Canvas_Draw).X / Canvas_Draw.ActualWidth * TotalFrame);
                if (frame < 0)
                    CurrentFrame = 0;
                else if (frame > TotalFrame)
                    CurrentFrame = TotalFrame;
                else
                    CurrentFrame = frame;
            }
        }

        Int64 mCurrentMillisecondTime = 0;
        Int64 mLastMilliTickTime = 0;
        private void Button_AddFrame_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ListBox_Items.SelectedIndex >= 0)
            {
                var selectedTrack = StackPanel_Tracks.Children[ListBox_Items.SelectedIndex] as TimeLineTrack;
                Int64 frameMilliTime = (Int64)System.Math.Round(CurrentFrame * 1000.0 / FPS);

                foreach (var frame in selectedTrack.ListItem.TLObject.GetKeyFrames())
                {
                    if (frame.KeyFrameMilliTimeStart == frameMilliTime)
                    {
                        // 已存在此位置的关键帧，不需要再添加
                        return;
                    }
                }

                var kObj = selectedTrack.ListItem.TLObject.AddKeyFrameObject(frameMilliTime, frameMilliTime, "NewFrame");
                selectedTrack.AddTrackItem(kObj);
                selectedTrack.UpdateTrackShow();

                if (OnAddFrame != null)
                    OnAddFrame(kObj);
            }
            else
            {
                EditorCommon.MessageBox.Show("没有选择的Notify项！", "提示");
            }
        }

        public Int64 GetCurrentFrameTime()
        {
            return (Int64)System.Math.Round(CurrentFrame * 1000.0 / FPS);
        }

        public TimeLineTrack GetTrack(int index)
        {
            if (index < 0 || index >= StackPanel_Tracks.Children.Count)
                return null;

            return StackPanel_Tracks.Children[index] as TimeLineTrack;
        }

        public ItemCollection GetAllTimeLineTracks()
        {
            return ListBox_Items.Items;
        }

        private void Button_DelFrame_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // 在此处添加事件处理程序实现。
        }

        private void _OnRemoveTimeLineTrackItem(TimeLineTrackItem item)
        {
            item.HostTimeLineTrack.ListItem.TLObject.RemoveKeyFrameObject(item.KeyFrameItem);

            OnRemoveTimeLineTrackItem?.Invoke(item);
        }

        private void _OnUpdateTimeLinkTrackItemActiveShow(TimeLineTrackItem item)
        {
            if (OnUpdateTimeLinkTrackItemActiveShow != null)
                OnUpdateTimeLinkTrackItemActiveShow(item);
        }

        TimeLineTrackItem mSelectedItem;
        private void _OnTimeLineTrackItemSelected(TimeLineTrackItem item)
        {
            if (mSelectedItem == item)
                return;

            if (mSelectedItem != null)
            {
                mSelectedItem.IsSelected = false;
                OnTimeLineTrackItemUnSelected?.Invoke(mSelectedItem);
            }

            if (OnTimeLineTrackItemSelected != null)
                OnTimeLineTrackItemSelected(item);

            mSelectedItem = item;                     
        }

        private void _OnCreateTimeLineTrackItem(TimeLineTrackItem item)
        {
            OnCreateTimeLineTrackItem?.Invoke(item);
        }

        public void AddTimeLineObject(TimeLineObjectInterface tlObj)
        {
            Dispatcher.Invoke(()=> 
            {
                TimeLineListItem item = new TimeLineListItem(tlObj);
                ListBox_Items.Items.Add(item);

                TimeLineTrack tItem = new TimeLineTrack(this, item);
                tItem.OnRemoveTimeLineTrackItem = new TimeLineTrackItem.Delegate_RemoveTimeLineTrackItem(_OnRemoveTimeLineTrackItem);
                tItem.OnUpdateTimeLinkTrackItemActiveShow = new TimeLineTrackItem.Delegate_OnUpdateTimeLinkTrackItemActiveShow(_OnUpdateTimeLinkTrackItemActiveShow);
                tItem.OnTimeLineTrackItemSelected = new TimeLineTrackItem.Delegate_OnSelected(_OnTimeLineTrackItemSelected);
                BindingOperations.SetBinding(tItem, TimeLineTrackItem.WidthProperty, new Binding("ActualWidth") { Source = StackPanel_Tracks });
                StackPanel_Tracks.Children.Add(tItem);
            });
        }

        public TimeLineObjectInterface GetSelectedTimeLineObject()
        {
            var item = ListBox_Items.SelectedItem as TimeLineListItem;

            if (item == null)
                return null;

            return item.TLObject;
        }

        public void SelectedTimeLineObject(int idx)
        {
            if (idx < ListBox_Items.Items.Count)
                ListBox_Items.SelectedIndex = idx;
        }

        public TimeLineObjectInterface RemoveSelectedTimeLineObject()
        {
            var item = ListBox_Items.SelectedItem as TimeLineListItem;

            if (item == null)
                return null;

            StackPanel_Tracks.Children.RemoveAt(ListBox_Items.SelectedIndex);
            ListBox_Items.Items.Remove(item);

            return item.TLObject;
        }

        public void RemoveAllTimeLineObject()
        {
            ListBox_Items.Items.Clear();
            StackPanel_Tracks.Children.Clear();
        }

        private void StackPanel_Tracks_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            UpdateScrollBarShow();
        }

        private void ScrollBar_Vertical_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            UpdateScrollBarShow();
        }

        private void UpdateScrollBarShow()
        {
            ScrollBar_Vertical.Maximum = StackPanel_Tracks.ActualHeight - ScrollBar_Vertical.ActualHeight;
        }

        private void ScrollBar_Vertical_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            Canvas.SetTop(StackPanel_Tracks, 20 - e.NewValue);
            var scrollView = VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(ListBox_Items, 0), 0) as ScrollViewer;
            if (scrollView != null)
            {
                scrollView.ScrollToVerticalOffset(e.NewValue);
            }
        }

        public void OnVerticalScrollChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var newValue = (double)e.NewValue;

            if (ScrollBar_Vertical.Value == newValue)
                return;

            ScrollBar_Vertical.Value = newValue;
        }

        private void ListBox_Items_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var item = ListBox_Items.SelectedItem as TimeLineListItem;
            if (item != null && OnSelectedTimeLineObject != null)
            {
                OnSelectedTimeLineObject(item.TLObject);
            }
        }

        public void UpdateTimeLineTrackShow()
        {
            //  UI动画编辑器中有多个时间轨迹,故做修改使更新所有时间轨迹

            for (int i = 0; i < StackPanel_Tracks.Children.Count; ++i)
            {
                var selectedTrack = StackPanel_Tracks.Children[i] as TimeLineTrack;

                if (selectedTrack != null)
                    selectedTrack.UpdateTrackShow();
            }

            //  以下是原来的代码
            //var selectedTrack = StackPanel_Tracks.Children[ListBox_Items.SelectedIndex] as TimeLineTrack;
            //
            //if (selectedTrack != null)
            //    selectedTrack.UpdateTrackShow();
        }

        public void RemoveAllTracks()
        {
            ListBox_Items.Items.Clear();
            StackPanel_Tracks.Children.Clear();
        }
    }
}
