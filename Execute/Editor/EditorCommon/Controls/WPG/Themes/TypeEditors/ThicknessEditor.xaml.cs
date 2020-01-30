using System.Windows;
using System.Windows.Controls;

namespace WPG.Themes.TypeEditors
{
    /// <summary>
    /// Interaction logic for ThicknessEditor.xaml
    /// </summary>
    public partial class ThicknessEditor : UserControl
    {
        public EngineNS.Thickness ControlMargin
        {
            get { return (EngineNS.Thickness)GetValue(ControlMarginProperty); }
            set
            {
                SetValue(ControlMarginProperty, value);
            }
        }

        public static readonly DependencyProperty ControlMarginProperty =
            DependencyProperty.Register("ControlMargin", typeof(EngineNS.Thickness), typeof(ThicknessEditor),
                                                        new FrameworkPropertyMetadata(new EngineNS.Thickness(0), new PropertyChangedCallback(OnControlMarginChanged)
                                        ));

        public static void OnControlMarginChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var newValue = (EngineNS.Thickness)e.NewValue;
            ThicknessEditor ctrl = d as ThicknessEditor;
            if (ctrl.ControlMarginLeft != newValue.Left)
                ctrl.ControlMarginLeft = newValue.Left;
            if (ctrl.ControlMarginRight != newValue.Right)
                ctrl.ControlMarginRight = newValue.Right;
            if (ctrl.ControlMarginTop != newValue.Top)
                ctrl.ControlMarginTop = newValue.Top;
            if (ctrl.ControlMarginBottom != newValue.Bottom)
                ctrl.ControlMarginBottom = newValue.Bottom;
        }

        public double ControlMarginLeft
        {
            get { return (double)GetValue(ControlMarginLeftProperty); }
            set
            {
                SetValue(ControlMarginLeftProperty, value);
            }
        }

        public static readonly DependencyProperty ControlMarginLeftProperty =
            DependencyProperty.Register("ControlMarginLeft", typeof(double), typeof(ThicknessEditor),
                                                        new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(OnControlMarginValueChanged)));

        public double ControlMarginRight
        {
            get { return (double)GetValue(ControlMarginRightProperty); }
            set
            {
                SetValue(ControlMarginRightProperty, value);
            }
        }

        public static readonly DependencyProperty ControlMarginRightProperty =
            DependencyProperty.Register("ControlMarginRight", typeof(double), typeof(ThicknessEditor),
                                                        new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(OnControlMarginValueChanged)));

        public double ControlMarginTop
        {
            get { return (double)GetValue(ControlMarginTopProperty); }
            set
            {
                SetValue(ControlMarginTopProperty, value);
            }
        }

        public static readonly DependencyProperty ControlMarginTopProperty =
            DependencyProperty.Register("ControlMarginTop", typeof(double), typeof(ThicknessEditor),
                                                        new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(OnControlMarginValueChanged)));

        //public static void OnControlMarginTopChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        //{
        //    //Thickness newValue = (Thickness)e.NewValue;
        //    //ThicknessEditor ctrl = d as ThicknessEditor;
        //    //if (ctrl.ControlMargin != newValue)
        //    //    ctrl.ControlMargin = newValue;
        //}

        public double ControlMarginBottom
        {
            get { return (double)GetValue(ControlMarginBottomProperty); }
            set
            {
                SetValue(ControlMarginBottomProperty, value);
            }
        }

        public static readonly DependencyProperty ControlMarginBottomProperty =
            DependencyProperty.Register("ControlMarginBottom", typeof(double), typeof(ThicknessEditor),
                                                        new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(OnControlMarginValueChanged)));

        public static void OnControlMarginValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ThicknessEditor ctrl = d as ThicknessEditor;
            switch(e.Property.Name)
            {
                case "ControlMarginLeft":
                    {
                        if(ctrl.ControlMarginLeft != ctrl.ControlMargin.Left)
                            ctrl.ControlMargin = new EngineNS.Thickness(ctrl.ControlMarginLeft, ctrl.ControlMarginTop, ctrl.ControlMarginRight, ctrl.ControlMarginBottom);
                    }
                    break;

                case "ControlMarginTop":
                    {
                        if (ctrl.ControlMarginTop != ctrl.ControlMargin.Top)
                            ctrl.ControlMargin = new EngineNS.Thickness(ctrl.ControlMarginLeft, ctrl.ControlMarginTop, ctrl.ControlMarginRight, ctrl.ControlMarginBottom);
                    }
                    break;

                case "ControlMarginRight":
                    {
                        if(ctrl.ControlMarginRight != ctrl.ControlMargin.Right)
                            ctrl.ControlMargin = new EngineNS.Thickness(ctrl.ControlMarginLeft, ctrl.ControlMarginTop, ctrl.ControlMarginRight, ctrl.ControlMarginBottom);
                    }
                    break;

                case "ControlMarginBottom":
                    {
                        if(ctrl.ControlMarginBottom != ctrl.ControlMargin.Bottom)
                            ctrl.ControlMargin = new EngineNS.Thickness(ctrl.ControlMarginLeft, ctrl.ControlMarginTop, ctrl.ControlMarginRight, ctrl.ControlMarginBottom);
                    }
                    break;
            }
            //Thickness newValue = new Thickness(ctrl.ControlMarginLeft, ctrl.ControlMarginTop, ctrl.ControlMarginRight, ctrl.ControlMarginBottom);
            //if (ctrl.ControlMargin != newValue)
            //    ctrl.ControlMargin = newValue;
        }

        public ThicknessEditor()
        {
            InitializeComponent();

            //BindingOperations.SetBinding(this, ControlMarginLeftProperty, new Binding("Left") { Source = this.ControlMargin, Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
        }
    }
}
