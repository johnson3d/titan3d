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
    public class Tick
    {
        public double Pos
        {
            get => TickRect.Margin.Left;
            set { var margin = TickRect.Margin; margin.Left = value; TickRect.Margin = margin; }
        }
        public double Height
        {
            get => TickRect.Height;
            set => TickRect.Height = value;
        }
        public double Width
        {
            get => TickRect.Width;
            set => TickRect.Width = value;
        }
        public Brush Fill
        {
            get => TickRect.Fill;
            set => TickRect.Fill = value;
        }
        public Rectangle TickRect = new Rectangle();
        public Tick()
        {
            TickRect.IsHitTestVisible = false;
        }

    }

    public class TickBarScaleEventArgs
    {
        public double DeltaScale;
        public double Percent;
        public TickBarScaleEventArgs(double deltaScale, double percent)
        {
            DeltaScale = deltaScale;
            Percent = percent;
        }
    }
    public class TickBarClickEventArgs
    {
        //relative to tickBar
        public Point MousePoint;
        public TickBarClickEventArgs(Point mousePoint)
        {
            MousePoint = mousePoint;
        }
    }
    /// <summary>
    /// Interaction logic for CustomSlider.xaml
    /// </summary>
    public partial class CustomSlider : UserControl
    {
        #region Property
        public bool ThumbDragable
        {
            get { return (bool)GetValue(ThumbDragableProperty); }
            set { SetValue(ThumbDragableProperty, value); }
        }
        public static DependencyProperty ThumbDragableProperty = DependencyProperty.Register("ThumbDragable", typeof(bool), typeof(CustomSlider), new PropertyMetadata(true));
        public double ThumbWidth
        {
            get { return (double)GetValue(ThumbWidthProperty); }
            set { SetValue(ThumbWidthProperty, value); }
        }
        public static DependencyProperty ThumbWidthProperty = DependencyProperty.Register("ThumbWidth", typeof(double), typeof(CustomSlider), new PropertyMetadata(8.0));
        public double ReservedSpace
        {
            get { return (double)GetValue(ReservedSpaceProperty); }
            set { SetValue(ReservedSpaceProperty, value); }
        }
        public static DependencyProperty ReservedSpaceProperty = DependencyProperty.Register("ReservedSpace", typeof(double), typeof(CustomSlider), new PropertyMetadata(4.0, new PropertyChangedCallback(ReservedSpacePropertyChanged)));
        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }
        public static DependencyProperty MinimumProperty = DependencyProperty.Register("Minimum", typeof(double), typeof(CustomSlider), new PropertyMetadata(0.0, new PropertyChangedCallback(MinimumPropertyChanged)));
        protected double mMaximum = 100.0;
        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }
        public static DependencyProperty MaximumProperty = DependencyProperty.Register("Maximum", typeof(double), typeof(CustomSlider), new PropertyMetadata(100.0, new PropertyChangedCallback(MaximumPropertyChanged)));
        public double TickFrequency
        {
            get { return (double)GetValue(TickFrequencyProperty); }
            set { SetValue(TickFrequencyProperty, value); }
        }
        public static DependencyProperty TickFrequencyProperty = DependencyProperty.Register("TickFrequency", typeof(double), typeof(CustomSlider), new PropertyMetadata(1.0, new PropertyChangedCallback(TickFrequencyPropertyChanged)));
        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
        public static DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(double), typeof(CustomSlider), new PropertyMetadata(0.0, new PropertyChangedCallback(ValuePropertyChanged)));
        public double LargeChange
        {
            get { return (double)GetValue(LargeChangeProperty); }
            set { SetValue(LargeChangeProperty, value); }
        }
        public static DependencyProperty LargeChangeProperty = DependencyProperty.Register("LargeChange", typeof(double), typeof(CustomSlider), new PropertyMetadata(0.0, new PropertyChangedCallback(LargeChangePropertyChanged)));
        public double SmallChange
        {
            get { return (double)GetValue(SmallProperty); }
            set { SetValue(SmallProperty, value); }
        }
        public static DependencyProperty SmallProperty = DependencyProperty.Register("SmallChange", typeof(double), typeof(CustomSlider), new PropertyMetadata(0.0, new PropertyChangedCallback(SmallChangePropertyChanged)));

        public static void TickFrequencyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var tickObj = d as CustomSlider;
            tickObj.OnTickFrequencyChanged((double)e.OldValue, (double)e.NewValue);
        }
        public static void ValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var tickObj = d as CustomSlider;
            tickObj.OnValueChanged((double)e.OldValue, (double)e.NewValue);
        }
        public static void MinimumPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var tickObj = d as CustomSlider;
            tickObj.OnMinimumChanged((double)e.OldValue, (double)e.NewValue);
        }
        public static void MaximumPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var tickObj = d as CustomSlider;
            tickObj.OnMaximumChanged((double)e.OldValue, (double)e.NewValue);
        }
        public static void LargeChangePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var tickObj = d as CustomSlider;
            tickObj.OnLargeChangeChanged((double)e.OldValue, (double)e.NewValue);
        }
        public static void SmallChangePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var tickObj = d as CustomSlider;
            tickObj.OnSmallChangeChanged((double)e.OldValue, (double)e.NewValue);
        }
        public static void ReservedSpacePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var tickObj = d as CustomSlider;
            tickObj.OnReservedSpaceChanged((double)e.OldValue, (double)e.NewValue);
        }

        #endregion
        public event EventHandler OnTicksCanvasLButtonDown;
        public event EventHandler OnTickCreated;
        public event EventHandler<TickBarScaleEventArgs> OnTickBarScaling;
        public event EventHandler<TickBarClickEventArgs> OnTickBarRightButtonClick;
        public CustomSlider()
        {
            InitializeComponent();
        }
        void OnMaximumChanged(double oldMaximum, double newMaximum)
        {
            CreateTicks();
        }
        void OnMinimumChanged(double oldMinimum, double newMinimum)
        {
            //CreateTicks();
        }
        void OnValueChanged(double oldValue, double newValue)
        {
            MoveThumbByPlay(newValue);
        }
        void OnLargeChangeChanged(double oldValue, double newValue)
        {

        }
        void OnTickFrequencyChanged(double oldValue, double newValue)
        {

        }
        void OnSmallChangeChanged(double oldValue, double newValue)
        {

        }
        void OnReservedSpaceChanged(double oldValue, double newValue)
        {

        }
        protected double TickBarScaleWidth
        {
            get { return LastTickPos - FirstTickPos; }
        }
        public double TicksScaleInterval
        {
            get => TickBarScaleWidth / TicksCount;
        }
        public double FirstTickPos
        {
            get
            {
                if (mTicksList.Count > 0)
                {
                    return mTicksList[0].Pos;
                }
                return 0.0;
            }
        }
        public double LastTickPos
        {
            get
            {
                if (mTicksList.Count > 0)
                {
                    return mTicksList[mTicksList.Count-1].Pos;
                }
                return 0.0;
            }
        }
        protected double TickBarRealWidth
        {
            get { return TicksCanvas.ActualWidth - 2 * ReservedSpace; }
        }
        protected double TicksCount
        {
            get { return Math.Max(1,(Maximum - Minimum)); }
        }

        protected double TicksInterval
        {
            get => TickBarRealWidth / TicksCount;
        }


        protected List<Tick> mTicksList = new List<Tick>();
        void CreateTicks()
        {
            TicksCanvas.Children.Clear();
            mTicksList.Clear();
            for (int i = 0; i < TicksCount + 1; ++i)
            {
                var rect = new Tick();
                mTicksList.Add(rect);
                TicksCanvas.Children.Add(rect.TickRect);
            }
            ResetTicksPosition();
            OnTickCreated?.Invoke(this, new EventArgs());
        }
        public void ResetTicksPosition()
        {
            var height = this.ActualHeight;
            for (int i = 0; i < TicksCount + 1; ++i)
            {
                if (i >= mTicksList.Count)
                    return;
                var rect = mTicksList[i];
                rect.Fill = this.Foreground;
                rect.Height = height;
                rect.Width = 1;
                rect.Pos = TicksInterval * i + ReservedSpace;
                rect.TickRect.Visibility = Visibility.Visible;
            }
        }
        void MoveThumb(double pointRelativeTicksCanvas)
        {
            var margin = Thumb.Margin;
            var point = pointRelativeTicksCanvas;
            double xPos = 0;
            if (point < FirstTickPos)
                xPos = FirstTickPos;
            else if (point > LastTickPos)
                xPos = LastTickPos;
            else
                xPos = point;
            margin.Left = xPos - Thumb.ActualWidth * 0.5f;
            Thumb.Margin = margin;
            var progressValue = (xPos - FirstTickPos) / TicksScaleInterval;
            SetValue(ValueProperty, progressValue);
        }
        void MoveThumbByPlay(double frameValue)
        {
            var xPos = TicksScaleInterval * frameValue - Thumb.Width * 0.5 + FirstTickPos;
            var margin = Thumb.Margin;
            margin.Left = xPos;
            Thumb.Margin = margin;
        }
        public void AddElementToTickBar(UIElement element)
        {
            TicksCanvas.Children.Add(element);
        }
        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ResetElements();
            
        }
        void ResetElements()
        {
            ResetTicksPosition();
            MoveThumbByPlay(Value);
            RestCustomItems();
        }
        protected virtual void RestCustomItems()
        {

        }
        bool isThumbButtonDown = false;
        private void Thumb_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!ThumbDragable)
                return;
            isThumbButtonDown = true;
            Mouse.Capture(sender as FrameworkElement);
        }
        private void Thumb_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isThumbButtonDown = false;
            Mouse.Capture(null);
        }
        private void Thumb_MouseMove(object sender, MouseEventArgs e)
        {
            if (isThumbButtonDown)
            {
                MoveThumb(e.GetPosition(TicksCanvas).X);
            }
        }
        bool isCanvasLButtonDown = false;
        private void TicksCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!ThumbDragable)
                return;
            OnTicksCanvasLButtonDown?.Invoke(this, new EventArgs());
            isCanvasLButtonDown = true;
            MoveThumb(e.GetPosition(TicksCanvas).X);
            Mouse.Capture(sender as FrameworkElement);
        }
        private void TicksCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isCanvasLButtonDown = false;

            Mouse.Capture(null);
        }
        private void TicksCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (isCanvasLButtonDown)
            {
                MoveThumb(e.GetPosition(TicksCanvas).X);
            }
        }
        private void TicksCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
            var deltaScale = e.Delta / 1200.0;
            var percent = e.GetPosition(TicksCanvas).X / TicksCanvas.ActualWidth;
            TickBarScale(deltaScale, percent);
            OnTickBarScaling?.Invoke(this, new TickBarScaleEventArgs(deltaScale, percent));
        }
        private void Thumb_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
            var deltaScale = e.Delta / 1200.0;
            var percent = e.GetPosition(TicksCanvas).X / TicksCanvas.ActualWidth;
            TickBarScale(deltaScale, percent);
            OnTickBarScaling?.Invoke(this, new TickBarScaleEventArgs(deltaScale, percent));
        }
        public void TickBarScale(double deltaScale, double percent)
        {
            TicksScale(deltaScale, percent);
            var value = (double)GetValue(ValueProperty);
            MoveThumb(TicksScaleInterval * value + FirstTickPos);
            ScaleCustomItem(deltaScale, percent);
        }
        void TicksScale(double deltaScale, double percent)
        {
            var mouseLoc = percent * TicksCanvas.ActualWidth;
            if (mouseLoc < 0 || mouseLoc > TickBarRealWidth + ReservedSpace)
                return;
            bool leftReached = false;
            bool rigthReached = false;
            var newLeftPos = mouseLoc - (mouseLoc - FirstTickPos) * (1 + deltaScale);
            var newRightPos = mouseLoc + (LastTickPos - mouseLoc) * (1 + deltaScale);
            if (newLeftPos > ReservedSpace && newRightPos < TickBarRealWidth + ReservedSpace)
            {
                leftReached = true;
                rigthReached = true;
                for (int i = 0; i < mTicksList.Count; ++i)
                {
                    mTicksList[i].Pos = ReservedSpace + i * TicksInterval;
                }
            }
            else if (newLeftPos > ReservedSpace)
            {
                leftReached = true;
                var tempPos = ReservedSpace - FirstTickPos;
                for (int i = 0; i < mTicksList.Count; ++i)
                {
                    mTicksList[i].Pos += tempPos;
                }
                for (int i = 0; i < mTicksList.Count; ++i)
                {
                    mTicksList[i].Pos = ReservedSpace + (mTicksList[i].Pos - ReservedSpace) * (1 + deltaScale);

                }
            }
            else if (newRightPos < TickBarRealWidth + ReservedSpace)
            {
                rigthReached = true;
                var tempPos = TickBarRealWidth + ReservedSpace - mTicksList[mTicksList.Count - 1].Pos;
                for (int i = 0; i < mTicksList.Count; ++i)
                {
                    mTicksList[i].Pos += tempPos;
                }
                var rightPos = TickBarRealWidth + ReservedSpace;
                for (int i = 0; i < mTicksList.Count; ++i)
                {
                    mTicksList[i].Pos = rightPos - (rightPos - mTicksList[i].Pos) * (1 + deltaScale);
                }
            }
            else
            {
                for (int i = 0; i < mTicksList.Count; ++i)
                {
                    if (mTicksList[i].Pos < mouseLoc)
                    {
                        mTicksList[i].Pos = mouseLoc - (mouseLoc - mTicksList[i].Pos) * (1 + deltaScale);
                    }
                    else
                    {
                        mTicksList[i].Pos = mouseLoc + (mTicksList[i].Pos - mouseLoc) * (1 + deltaScale);
                    }
                }
            }
            //check
            if (leftReached)
            {
                //check right
                if (TickBarRealWidth + ReservedSpace - mTicksList[mTicksList.Count - 1].Pos > 0)
                {
                    for (int i = 0; i < mTicksList.Count; ++i)
                    {
                        mTicksList[i].Pos = ReservedSpace + i * TicksInterval;
                    }
                }
            }
            if (rigthReached)
            {
                //check left
                if (ReservedSpace - FirstTickPos < 0)
                {
                    for (int i = 0; i < mTicksList.Count; ++i)
                    {
                        mTicksList[i].Pos = ReservedSpace + i * TicksInterval;
                    }
                }
            }

            //check tick visiable
            for (int i = 0; i < mTicksList.Count; ++i)
            {
                if(mTicksList[i].Pos > TickBarRealWidth + ReservedSpace)
                    mTicksList[i].TickRect.Visibility = Visibility.Hidden;
                else
                    mTicksList[i].TickRect.Visibility = Visibility.Visible;
            }


        }
        public virtual void ScaleCustomItem(double deltaScale, double percent)
        {

        }
        private void TicksCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            OnTickBarRightButtonClick?.Invoke(this, new TickBarClickEventArgs(e.GetPosition(TicksCanvas)));
        }

        private void Thumb_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            OnTickBarRightButtonClick?.Invoke(this, new TickBarClickEventArgs(e.GetPosition(TicksCanvas)));
        }
        bool IsTicksInitialized = false;
        private void userControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!IsTicksInitialized)
            {
                CreateTicks();
                IsTicksInitialized = true;
            }
        }
    }
}
