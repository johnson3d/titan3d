using EngineNS;
using EngineNS.Profiler;
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

namespace EditorCommon.Controls
{
    /// <summary>
    /// ResourceIconControl.xaml 的交互逻辑
    /// </summary>
    public partial class ResourceIconControl : UserControl
    {
        public ImageSource ResIcon
        {
            get { return (ImageSource)GetValue(ResIconProperty); }
            set { SetValue(ResIconProperty, value); }
        }
        public static readonly DependencyProperty ResIconProperty = DependencyProperty.Register("ResIcon", typeof(ImageSource), typeof(ResourceIconControl), new UIPropertyMetadata(null));
        public ImageSource Snapshot
        {
            get { return (ImageSource)GetValue(SnapshotProperty); }
            set { SetValue(SnapshotProperty, value); }
        }
        public static readonly DependencyProperty SnapshotProperty = DependencyProperty.Register("Snapshot", typeof(ImageSource), typeof(ResourceIconControl), new UIPropertyMetadata(null, OnSnapshotPropertyChanged));

        private static void OnSnapshotPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as ResourceIconControl;
            var newValue = (ImageSource)e.NewValue;
            if (newValue == null)
            {
                ctrl.PART_Icon.Visibility = Visibility.Visible;
                ctrl.PART_SmallIcon.Visibility = Visibility.Collapsed;
            }
            else
            {
                ctrl.PART_Icon.Visibility = Visibility.Collapsed;
                ctrl.PART_SmallIcon.Visibility = Visibility.Visible;
            }
        }

        public Brush ResourceBrush
        {
            get { return (Brush)GetValue(ResourceBrushProperty); }
            set { SetValue(ResourceBrushProperty, value); }
        }
        public static readonly DependencyProperty ResourceBrushProperty = DependencyProperty.Register("ResourceBrush", typeof(Brush), typeof(ResourceIconControl), new UIPropertyMetadata(null, ResourceBrushPropertyChanged));

        private static void ResourceBrushPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as ResourceIconControl;
            var newValue = (Brush)e.NewValue;
            if (newValue != null && newValue != Brushes.Transparent)
            {
                if (ctrl.HighLight)
                    ctrl.BGImage = ctrl.TryFindResource("HighLightBG") as ImageSource;
                else
                    ctrl.BGImage = ctrl.TryFindResource("NormalBG") as ImageSource;

                ctrl.bottomBar.Height = new GridLength(1, GridUnitType.Star);
                ctrl.bottomBar.MinHeight = 5;
            }
            else
            {
                ctrl.BGImage = null;

                ctrl.bottomBar.Height = new GridLength(0, GridUnitType.Pixel);
                ctrl.bottomBar.MinHeight = 0;
            }
        }

        public ImageSource BGImage
        {
            get { return (ImageSource)GetValue(BGImageProperty); }
            set { SetValue(BGImageProperty, value); }
        }
        public static readonly DependencyProperty BGImageProperty = DependencyProperty.Register("BGImage", typeof(ImageSource), typeof(ResourceIconControl), new UIPropertyMetadata(null));
        public bool HighLight
        {
            get { return (bool)GetValue(HighLightProperty); }
            set { SetValue(HighLightProperty, value); }
        }
        public static readonly DependencyProperty HighLightProperty = DependencyProperty.Register("HighLight", typeof(bool), typeof(ResourceIconControl), new UIPropertyMetadata(false, OnHighLightPropertyChangedChanged));

        private static void OnHighLightPropertyChangedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as ResourceIconControl;
            var newValue = (bool)e.NewValue;

            ImageSource bg;
            if (newValue)
                bg = ctrl.TryFindResource("HighLightBG") as ImageSource;
            else
                bg = ctrl.TryFindResource("NormalBG") as ImageSource;
        }
        public EngineNS.RName ResourceRName
        {
            get { return (EngineNS.RName)GetValue(ResourceRNameProperty); }
            set { SetValue(ResourceRNameProperty, value); }
        }
        public static readonly DependencyProperty ResourceRNameProperty = DependencyProperty.Register("ResourceRName", typeof(EngineNS.RName), typeof(ResourceIconControl), new UIPropertyMetadata(EngineNS.RName.EmptyName));

        public ResourceIconControl()
        {
            InitializeComponent();
            if(ResourceBrush != null)
                BGImage = TryFindResource("NormalBG") as ImageSource;
        }

        public class SnapshotAnimTick : ITickInfo
        {
            public bool EnableTick
            {
                get;
                set;
            } = true;

            public void BeforeFrame()
            {
                
            }

            public TimeScope GetLogicTimeScope()
            {
                return null;
            }

            public void TickLogic()
            {
                
            }

            public void TickRender()
            {
                
            }
            public ResourceIconControl CurrIcon;
            public long EnterTime;
            public ImageSource[] IconAnim;
            private System.Threading.Tasks.Task<ImageSource[]> IconAnimTask;
            long mPrevTick;
            int PlayFrame = 0;
            public void TickSync()
            {
                if (CurrIcon == null)
                    return;

                var now = EngineNS.Support.Time.GetTickCount();
                if(now - EnterTime < 1000)
                {
                    return;
                }
                if(now - mPrevTick < 100)
                {
                    return;
                }
                mPrevTick = now;
                if (IconAnimTask == null)
                {
                    IconAnimTask = ImageInit.GetImage(CurrIcon.ResourceRName.Address + ".snap");
                }
                else
                {
                    if (IconAnimTask.IsCompleted)
                    {
                        IconAnim = IconAnimTask.Result;
                        IconAnimTask = null;
                    }
                }
                if (IconAnim != null)
                {
                    PlayFrame++;
                    if (PlayFrame >= IconAnim.Length)
                    {
                        PlayFrame = 0;
                    }
                    CurrIcon.Snapshot = IconAnim[PlayFrame];
                }
            }
        }

        static SnapshotAnimTick mSnapTick = null;
        private void PART_Snapshot_MouseEnter(object sender, MouseEventArgs e)
        {
            //if(ResourceRName.GetExtension() != "material" && 
            //    ResourceRName.GetExtension() != "instmtl")
            //{
            //    return;
            //}
            if (mSnapTick == null)
            {
                mSnapTick = new SnapshotAnimTick();
                EngineNS.CEngine.Instance.TickManager.AddTickInfo(mSnapTick);
            }
            mSnapTick.CurrIcon = this;
            mSnapTick.EnterTime = EngineNS.Support.Time.GetTickCount();
        }

        private void PART_Snapshot_MouseLeave(object sender, MouseEventArgs e)
        {
            if (mSnapTick!=null && mSnapTick.CurrIcon == this)
            {
                mSnapTick.CurrIcon = null;
                mSnapTick.IconAnim = null;
            }
        }
    }
}
