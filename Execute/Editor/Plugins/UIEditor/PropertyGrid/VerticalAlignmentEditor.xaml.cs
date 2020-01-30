using System.Windows;
using System.Windows.Controls;

namespace UIEditor.PropertyGrid
{
    /// <summary>
    /// Interaction logic for VerticalAlignmentEditor.xaml
    /// </summary>
    public partial class VerticalAlignmentEditor : UserControl
    {
        public object BindInstance
        {
            get { return (object)GetValue(BindInstanceProperty); }
            set { SetValue(BindInstanceProperty, value); }
        }
        public static readonly DependencyProperty BindInstanceProperty =
                            DependencyProperty.Register("BindInstance", typeof(object), typeof(VerticalAlignmentEditor), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnBindInstanceChanged)));

        public static void OnBindInstanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //VerticalAlignmentEditor control = d as VerticalAlignmentEditor;
            //UISystem.WinBase win = e.NewValue as UISystem.WinBase;
            //if (win == null)
            //    return;

            //if (win.LockedVerticals.Count > 0)
            //{
            //    control.TopEnabled = false;
            //    control.CenterEnabled = false;
            //    control.BottomEnabled = false;
            //    control.StretchEnabled = false;

            //    foreach (var lockAlignment in win.LockedVerticals)
            //    {
            //        switch (lockAlignment)
            //        {
            //            case EngineNS.UISystem.VerticalAlignment.Top:
            //                control.TopEnabled = true;
            //                break;
            //            case EngineNS.UISystem.VerticalAlignment.Center:
            //                control.CenterEnabled = true;
            //                break;
            //            case EngineNS.UISystem.VerticalAlignment.Bottom:
            //                control.BottomEnabled = true;
            //                break;
            //            case EngineNS.UISystem.VerticalAlignment.Stretch:
            //                control.StretchEnabled = true;
            //                break;
            //        }
            //    }
            //}
        }

        public EngineNS.UISystem.VerticalAlignment VerticalAlign
        {
            get { return (EngineNS.UISystem.VerticalAlignment)GetValue(VerticalAlignProperty); }
            set
            {
                SetValue(VerticalAlignProperty, value);
            }
        }

        public static readonly DependencyProperty VerticalAlignProperty =
            DependencyProperty.Register("VerticalAlign", typeof(EngineNS.UISystem.VerticalAlignment), typeof(VerticalAlignmentEditor),
                                                        new FrameworkPropertyMetadata(EngineNS.UISystem.VerticalAlignment.Top, new PropertyChangedCallback(OnVerticalAlignChanged)
                                        ));

        public static void OnVerticalAlignChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            VerticalAlignmentEditor control = d as VerticalAlignmentEditor;

            EngineNS.UISystem.VerticalAlignment newValue = (EngineNS.UISystem.VerticalAlignment)e.NewValue;
            switch (newValue)
            {
                case EngineNS.UISystem.VerticalAlignment.Top:
                    control.Top = true;
                    break;
                case EngineNS.UISystem.VerticalAlignment.Center:
                    control.Center = true;
                    break;
                case EngineNS.UISystem.VerticalAlignment.Bottom:
                    control.Bottom = true;
                    break;
                case EngineNS.UISystem.VerticalAlignment.Stretch:
                    control.Stretch = true;
                    break;
            }
        }

        public bool Top
        {
            get { return (bool)GetValue(TopProperty); }
            set
            {
                SetValue(TopProperty, value);
            }
        }
        public static readonly DependencyProperty TopProperty =
            DependencyProperty.Register("Top", typeof(bool), typeof(VerticalAlignmentEditor),
                                                    new FrameworkPropertyMetadata(true, new PropertyChangedCallback(OnTopChanged)));
        public static void OnTopChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            VerticalAlignmentEditor control = d as VerticalAlignmentEditor;

            bool newValue = (bool)e.NewValue;
            if (newValue)
            {
                //control.Center = false;
                //control.Bottom = false;
                //control.Stretch = false;
                control.VerticalAlign = EngineNS.UISystem.VerticalAlignment.Top;
            }
        }

        public bool TopEnabled
        {
            get { return (bool)GetValue(TopEnabledProperty); }
            set { SetValue(TopEnabledProperty, value); }
        }
        public static readonly DependencyProperty TopEnabledProperty = DependencyProperty.Register("TopEnabled", typeof(bool), typeof(VerticalAlignmentEditor), new UIPropertyMetadata(true));

        public bool Center
        {
            get { return (bool)GetValue(CenterProperty); }
            set
            {
                SetValue(CenterProperty, value);
            }
        }
        public static readonly DependencyProperty CenterProperty =
            DependencyProperty.Register("Center", typeof(bool), typeof(VerticalAlignmentEditor),
                                                    new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnCenterChanged)));
        public static void OnCenterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            VerticalAlignmentEditor control = d as VerticalAlignmentEditor;

            bool newValue = (bool)e.NewValue;
            if (newValue)
            {
                //control.Top = false;
                //control.Bottom = false;
                //control.Stretch = false;
                control.VerticalAlign = EngineNS.UISystem.VerticalAlignment.Center;
            }
        }

        public bool CenterEnabled
        {
            get { return (bool)GetValue(CenterEnabledProperty); }
            set { SetValue(CenterEnabledProperty, value); }
        }
        public static readonly DependencyProperty CenterEnabledProperty = DependencyProperty.Register("CenterEnabled", typeof(bool), typeof(VerticalAlignmentEditor), new UIPropertyMetadata(true));

        public bool Bottom
        {
            get { return (bool)GetValue(BottomProperty); }
            set
            {
                SetValue(BottomProperty, value);
            }
        }
        public static readonly DependencyProperty BottomProperty =
            DependencyProperty.Register("Bottom", typeof(bool), typeof(VerticalAlignmentEditor),
                                                    new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnBottomChanged)));
        public static void OnBottomChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            VerticalAlignmentEditor control = d as VerticalAlignmentEditor;

            bool newValue = (bool)e.NewValue;
            if (newValue)
            {
                //control.Top = false;
                //control.Center = false;
                //control.Stretch = false;
                control.VerticalAlign = EngineNS.UISystem.VerticalAlignment.Bottom;
            }
        }

        public bool BottomEnabled
        {
            get { return (bool)GetValue(BottomEnabledProperty); }
            set { SetValue(BottomEnabledProperty, value); }
        }
        public static readonly DependencyProperty BottomEnabledProperty = DependencyProperty.Register("BottomEnabled", typeof(bool), typeof(VerticalAlignmentEditor), new UIPropertyMetadata(true));

        public bool Stretch
        {
            get { return (bool)GetValue(StretchProperty); }
            set
            {
                SetValue(StretchProperty, value);
            }
        }
        public static readonly DependencyProperty StretchProperty =
            DependencyProperty.Register("Stretch", typeof(bool), typeof(VerticalAlignmentEditor),
                                                    new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnStretchChanged)));
        public static void OnStretchChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            VerticalAlignmentEditor control = d as VerticalAlignmentEditor;

            bool newValue = (bool)e.NewValue;
            if (newValue)
            {
                //control.Top = false;
                //control.Center = false;
                //control.Bottom = false;
                control.VerticalAlign = EngineNS.UISystem.VerticalAlignment.Stretch;
            }
        }

        public bool StretchEnabled
        {
            get { return (bool)GetValue(StretchEnabledProperty); }
            set { SetValue(StretchEnabledProperty, value); }
        }
        public static readonly DependencyProperty StretchEnabledProperty = DependencyProperty.Register("StretchEnabled", typeof(bool), typeof(VerticalAlignmentEditor), new UIPropertyMetadata(true));

        public VerticalAlignmentEditor()
        {
            InitializeComponent();
        }
    }
}
