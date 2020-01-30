using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public enum PlayingState
    {
        Forward = 0,
        Backward,
        Pause,
    }

    public delegate void PlayingEventHander(object sender, RoutedEventArgs e);
    /// <summary>
    /// Interaction logic for AnimationTimeLineControl.xaml
    /// </summary>
    public partial class AnimationPlayingControl : UserControl, INotifyPropertyChanged, EngineNS.ITickInfo
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
            if (mPlayingState != PlayingState.Pause)
            {
                float playRate = 1;
                float delta = EngineNS.CEngine.Instance.EngineElapseTimeSecond;
                if (mPlayingState == PlayingState.Backward)
                    playRate = -1;
                CurrentTime += delta * playRate;
                if (CurrentTime < 0)
                {
                    if (mLoop)
                    {
                        CurrentTime = Duration;
                    }
                    else
                    {
                        CurrentTime = 0;
                        PlayingState = PlayingState.Pause;
                    }
                }
                else if (CurrentTime > Duration)
                {
                    if (mLoop)
                    {
                        CurrentTime = 0;
                    }
                    else
                    {
                        CurrentTime = Duration;
                        PlayingState = PlayingState.Pause;
                    }
                }
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

        public event PlayingEventHander OnForwardPlay;
        public event PlayingEventHander OnBackwardPlay;
        public event PlayingEventHander OnPause;
        public event PlayingEventHander OnForwardNext;
        public event PlayingEventHander OnForwardEnd;
        public event PlayingEventHander OnBackwardNext;
        public event PlayingEventHander OnBackwardEnd;
        public event PlayingEventHander OnLoopCheck;
        public event PlayingEventHander OnRecordStart;
        public event PlayingEventHander OnRecordEnd;
        public event EventHandler<TickBarScaleEventArgs> OnTickBarScaling;


        Visibility mCollapsedTitle = Visibility.Visible;
        public Visibility CollapsedTitle
        {
            get => mCollapsedTitle;
            set
            {
                mCollapsedTitle = value;
                OnPropertyChanged("CollapsedTitle");
            }
        }
        //刻度
        string mName = "TestName";
        public string AnimationName
        {
            get { return mName; }
            set
            {
                mName = value;
                OnPropertyChanged("AnimationName");
            }
        }

        //刻度
        int mTickFrequency = 1;
        public int TickFrequency
        {
            get { return mTickFrequency; }
            set
            {
                mTickFrequency = value;
                OnPropertyChanged("TickFrequency");
            }
        }
        float mDuration = 0;
        //单位 秒
        public float Duration
        {
            get { return mDuration; }
            set
            {
                if (value == 0)
                    return;
                float scale = 1;
                if (mDuration != 0)
                    scale = value / mDuration;
                mDuration = value;
                CurrentTime = CurrentTime * scale;
                OnPropertyChanged("Duration");
            }
        }

        float mCurrentTime = 0;
        public float CurrentTime
        {
            get { return mCurrentTime; }
            set
            {
                if (mCurrentTime == value)
                    return;
                mCurrentTime = value;
                PlayPercent = mCurrentTime / Duration;
                if (TotalFrame == 0)
                    CurrentFrame = 0;
                else
                    CurrentFrame = PlayPercent * TotalFrame;
                OnPropertyChanged("CurrentTime");
            }
        }

        // 单位：帧
        float mTotalFrame = 30;
        public float TotalFrame
        {
            get { return mTotalFrame; }
            set
            {
                mTotalFrame = value;
                //ViewFrameStart = 0;
                //ViewFrameEnd = mTotalFrame;
                //UpdateShowNumbers();

                //UpdateSliderItemAndFrameShow();

                OnPropertyChanged("TotalFrame");
            }
        }

        float mCurrentFrame = 0;
        public float CurrentFrame
        {
            get { return mCurrentFrame; }
            set
            {
                mCurrentFrame = value;
                mCurrentTime = Duration * (value / mTotalFrame);
                PlayPercent = mCurrentTime / Duration;
                OnPropertyChanged("CurrentFrame");
                OnPropertyChanged("CurrentTime");
            }
        }
        //fram/second
        //float mFps = 30.0f;

        bool mLoop = true;

        float mPlayPercent = 0;
        public float PlayPercent
        {
            get { return mPlayPercent; }
            set
            {
                if (float.IsNaN(value))
                    return;
                mPlayPercent = value;
                OnPropertyChanged("PlayPercent");
            }
        }

        PlayingState mPlayingState = PlayingState.Pause;

        public PlayingState PlayingState
        {
            get { return mPlayingState; }
            set
            {
                mPlayingState = value;
                CheckButtonState(value);
            }
        }

        public AnimationPlayingControl()
        {
            InitializeComponent();
            AnimSlider.OnTickBarScaling += AnimSlider_OnTickBarScaling;
        }

        private void AnimSlider_OnTickBarScaling(object sender, TickBarScaleEventArgs e)
        {
            OnTickBarScaling?.Invoke(this, e);
        }
        public void TickBarScale(double deltaScale, double percent)
        {
            AnimSlider.TickBarScale(deltaScale, percent);
        }
        private void CheckButtonState(PlayingState playState)
        {
            if (playState == PlayingState.Forward)
            {
                Backward_TGButton.IsChecked = false;
            }
            if (playState == PlayingState.Backward)
            {
                Forward_TGButton.IsChecked = false;
            }
            if (playState == PlayingState.Pause)
            {
                Backward_TGButton.IsChecked = false;
                Forward_TGButton.IsChecked = false;
            }
        }
        private void Forward_TGButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as ToggleButton;
            if (button != null)
            {
                if (button.IsChecked == true)
                {
                    PlayingState = PlayingState.Forward;
                    OnForwardPlay?.Invoke(this, e);
                }
                else
                {
                    PlayingState = PlayingState.Pause;
                    OnPause?.Invoke(this, e);

                }
            }
        }

        private void Forward_Step_Button_Click(object sender, RoutedEventArgs e)
        {
            PlayingState = PlayingState.Pause;
            var frame = Math.Truncate(CurrentFrame + 1);
            if (mTotalFrame != 0)
            {
                CurrentFrame = (float)EngineNS.MathHelper.Clamp(frame, 0, mTotalFrame);
            }
            OnForwardNext?.Invoke(this, e);
        }

        private void Forward_End_Button_Click(object sender, RoutedEventArgs e)
        {
            PlayingState = PlayingState.Pause;
            CurrentFrame = mTotalFrame;
            OnForwardEnd?.Invoke(this, e);
        }

        private void Loop_Button_Click(object sender, RoutedEventArgs e)
        {
            OnLoopCheck?.Invoke(this, e);
        }

        private void Record_TGButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as ToggleButton;
            if (button != null)
            {
                if (button.IsChecked == true)
                    OnRecordStart?.Invoke(this, e);
                else
                    OnRecordEnd?.Invoke(this, e);
            }
        }

        private void Backward_TGButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as ToggleButton;
            if (button != null)
            {
                if (button.IsChecked == true)
                {
                    PlayingState = PlayingState.Backward;
                    OnBackwardPlay?.Invoke(this, e);
                }
                else
                {
                    PlayingState = PlayingState.Pause;
                    OnPause?.Invoke(this, e);
                }
            }
        }

        private void Backward_Step_Button_Click(object sender, RoutedEventArgs e)
        {
            PlayingState = PlayingState.Pause;
            var frame = Math.Ceiling(CurrentFrame - 1);
            if (mTotalFrame != 0)
                CurrentFrame = (float)EngineNS.MathHelper.Clamp(frame, 0, mTotalFrame);
            OnBackwardNext?.Invoke(this, e);
        }

        private void Backward_End_Button_Click(object sender, RoutedEventArgs e)
        {
            PlayingState = PlayingState.Pause;
            CurrentFrame = 0;
            OnBackwardEnd?.Invoke(this, e);
        }

        private void AnimSlider_OnTicksCanvasLButtonDown(object sender, EventArgs e)
        {
            PlayingState = PlayingState.Pause;
        }
    }
}
