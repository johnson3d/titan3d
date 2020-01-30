using System.Windows;
using System.Windows.Controls;

namespace UIEditor.PropertyGrid
{
    /// <summary>
    /// Interaction logic for HorizontalAlignmentEditor.xaml
    /// </summary>
    public partial class HorizontalAlignmentEditor : UserControl
    {
        public object BindInstance
        {
            get { return (object)GetValue(BindInstanceProperty); }
            set { SetValue(BindInstanceProperty, value); }
        }
        public static readonly DependencyProperty BindInstanceProperty =
                            DependencyProperty.Register("BindInstance", typeof(object), typeof(HorizontalAlignmentEditor), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnBindInstanceChanged)));

        public static void OnBindInstanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //HorizontalAlignmentEditor control = d as HorizontalAlignmentEditor;
            //var win = e.NewValue as UISystem.WinBase;
            //if (win == null)
            //    return;

            //if (win.LockedHorizontals.Count > 0)
            //{
            //    control.LeftEnabled = false;
            //    control.CenterEnabled = false;
            //    control.RightEnabled = false;
            //    control.StretchEnabled = false;

            //    foreach (var lockAlignment in win.LockedHorizontals)
            //    {
            //        switch (lockAlignment)
            //        {
            //            case EngineNS.UISystem.HorizontalAlignment.Left:
            //                control.LeftEnabled = true;
            //                break;
            //            case EngineNS.UISystem.HorizontalAlignment.Center:
            //                control.CenterEnabled = true;
            //                break;
            //            case EngineNS.UISystem.HorizontalAlignment.Right:
            //                control.RightEnabled = true;
            //                break;
            //            case EngineNS.UISystem.HorizontalAlignment.Stretch:
            //                control.StretchEnabled = true;
            //                break;
            //        }
            //    }
            //}
        }

        public EngineNS.UISystem.HorizontalAlignment HorizontalAlign
        {
            get { return (EngineNS.UISystem.HorizontalAlignment)GetValue(HorizontalAlignProperty); }
            set
            {
                SetValue(HorizontalAlignProperty, value);
            }
        }

        public static readonly DependencyProperty HorizontalAlignProperty =
            DependencyProperty.Register("HorizontalAlign", typeof(EngineNS.UISystem.HorizontalAlignment), typeof(HorizontalAlignmentEditor),
                                                        new FrameworkPropertyMetadata(EngineNS.UISystem.HorizontalAlignment.Left, new PropertyChangedCallback(OnHorizontalAlignChanged)));

        public static void OnHorizontalAlignChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            HorizontalAlignmentEditor control = d as HorizontalAlignmentEditor;

            EngineNS.UISystem.HorizontalAlignment newValue = (EngineNS.UISystem.HorizontalAlignment)e.NewValue;
            switch (newValue)
            {
                case EngineNS.UISystem.HorizontalAlignment.Left:
                    control.Left = true;
                    break;
                case EngineNS.UISystem.HorizontalAlignment.Right:
                    control.Right = true;
                    break;
                case EngineNS.UISystem.HorizontalAlignment.Center:
                    control.Center = true;
                    break;
                case EngineNS.UISystem.HorizontalAlignment.Stretch:
                    control.Stretch = true;
                    break;
            }
        }

        public bool Left
        {
            get { return (bool)GetValue(LeftProperty); }
            set
            {
                SetValue(LeftProperty, value);
            }
        }
        public static readonly DependencyProperty LeftProperty =
            DependencyProperty.Register("Left", typeof(bool), typeof(HorizontalAlignmentEditor),
                                                    new FrameworkPropertyMetadata(true, new PropertyChangedCallback(OnLeftChanged)));
        public static void OnLeftChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            HorizontalAlignmentEditor control = d as HorizontalAlignmentEditor;

            bool newValue = (bool)e.NewValue;
            if (newValue)
            {
                //control.Center = false;
                //control.Right = false;
                //control.Stretch = false;
                control.HorizontalAlign = EngineNS.UISystem.HorizontalAlignment.Left;
            }
        }

        public bool LeftEnabled
        {
            get { return (bool)GetValue(LeftEnabledProperty); }
            set { SetValue(LeftEnabledProperty, value); }
        }
        public static readonly DependencyProperty LeftEnabledProperty = DependencyProperty.Register("LeftEnabled", typeof(bool), typeof(HorizontalAlignmentEditor), new UIPropertyMetadata(true));

        public bool Center
        {
            get { return (bool)GetValue(CenterProperty); }
            set
            {
                SetValue(CenterProperty, value);
            }
        }
        public static readonly DependencyProperty CenterProperty =
            DependencyProperty.Register("Center", typeof(bool), typeof(HorizontalAlignmentEditor),
                                                    new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnCenterChanged)));
        public static void OnCenterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            HorizontalAlignmentEditor control = d as HorizontalAlignmentEditor;

            bool newValue = (bool)e.NewValue;
            if (newValue)
            {
                //control.Left = false;
                //control.Right = false;
                //control.Stretch = false;
                control.HorizontalAlign = EngineNS.UISystem.HorizontalAlignment.Center;
            }
        }

        public bool CenterEnabled
        {
            get { return (bool)GetValue(CenterEnabledProperty); }
            set { SetValue(CenterEnabledProperty, value); }
        }
        public static readonly DependencyProperty CenterEnabledProperty = DependencyProperty.Register("CenterEnabled", typeof(bool), typeof(HorizontalAlignmentEditor), new UIPropertyMetadata(true));

        public bool Right
        {
            get { return (bool)GetValue(RightProperty); }
            set
            {
                SetValue(RightProperty, value);
            }
        }
        public static readonly DependencyProperty RightProperty =
            DependencyProperty.Register("Right", typeof(bool), typeof(HorizontalAlignmentEditor),
                                                    new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnRightChanged)));
        public static void OnRightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            HorizontalAlignmentEditor control = d as HorizontalAlignmentEditor;

            bool newValue = (bool)e.NewValue;
            if (newValue)
            {
                //control.Left = false;
                //control.Center = false;
                //control.Stretch = false;
                control.HorizontalAlign = EngineNS.UISystem.HorizontalAlignment.Right;
            }
        }

        public bool RightEnabled
        {
            get { return (bool)GetValue(RightEnabledProperty); }
            set { SetValue(RightEnabledProperty, value); }
        }
        public static readonly DependencyProperty RightEnabledProperty = DependencyProperty.Register("RightEnabled", typeof(bool), typeof(HorizontalAlignmentEditor), new UIPropertyMetadata(true));

        public bool Stretch
        {
            get { return (bool)GetValue(StretchProperty); }
            set
            {
                SetValue(StretchProperty, value);
            }
        }
        public static readonly DependencyProperty StretchProperty =
            DependencyProperty.Register("Stretch", typeof(bool), typeof(HorizontalAlignmentEditor),
                                                    new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnStretchChanged)));
        public static void OnStretchChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            HorizontalAlignmentEditor control = d as HorizontalAlignmentEditor;

            bool newValue = (bool)e.NewValue;
            if (newValue)
            {
                //control.Left = false;
                //control.Center = false;
                //control.Right = false;
                control.HorizontalAlign = EngineNS.UISystem.HorizontalAlignment.Stretch;
            }
        }

        public bool StretchEnabled
        {
            get { return (bool)GetValue(StretchEnabledProperty); }
            set { SetValue(StretchEnabledProperty, value); }
        }
        public static readonly DependencyProperty StretchEnabledProperty = DependencyProperty.Register("StretchEnabled", typeof(bool), typeof(HorizontalAlignmentEditor), new UIPropertyMetadata(true));

        public HorizontalAlignmentEditor()
        {
            InitializeComponent();
        }
    }
}
