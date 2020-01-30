using System.Windows;
using System.Windows.Controls;

namespace WPG.Themes.TypeEditors
{
    /// <summary>
    /// Interaction logic for SystemDrawingPointEditor.xaml
    /// </summary>
    public partial class SystemDrawingPointEditor : UserControl
    {
        public System.Drawing.Point Point
        {
            get { return (System.Drawing.Point)GetValue(PointProperty); }
            set { SetValue(PointProperty, value); }
        }
        public static readonly DependencyProperty PointProperty =
            DependencyProperty.Register("Point", typeof(System.Drawing.Point), typeof(SystemDrawingPointEditor),
            new FrameworkPropertyMetadata(System.Drawing.Point.Empty, new PropertyChangedCallback(OnPointChanged)));

        public static void OnPointChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SystemDrawingPointEditor control = d as SystemDrawingPointEditor;

            System.Drawing.Point newValue = (System.Drawing.Point)e.NewValue;

            if(control.X != newValue.X)
                control.X = newValue.X;
            if(control.Y != newValue.Y)
                control.Y = newValue.Y;
        }

        public int X
        {
            get { return (int)GetValue(XProperty); }
            set { SetValue(XProperty, value); }
        }
        public static readonly DependencyProperty XProperty =
            DependencyProperty.Register("X", typeof(int), typeof(SystemDrawingPointEditor),
            new FrameworkPropertyMetadata(0, new PropertyChangedCallback(OnXChanged)));

        public static void OnXChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SystemDrawingPointEditor control = d as SystemDrawingPointEditor;

            int newValue = (int)e.NewValue;

            if (newValue != control.Point.X)
                control.Point = new System.Drawing.Point(newValue, control.Y);
        }

        public int Y
        {
            get { return (int)GetValue(YProperty); }
            set { SetValue(YProperty, value); }
        }
        public static readonly DependencyProperty YProperty =
            DependencyProperty.Register("Y", typeof(int), typeof(SystemDrawingPointEditor),
            new FrameworkPropertyMetadata(0, new PropertyChangedCallback(OnYChanged)));

        public static void OnYChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SystemDrawingPointEditor control = d as SystemDrawingPointEditor;

            int newValue = (int)e.NewValue;

            if (newValue != control.Point.Y)
                control.Point = new System.Drawing.Point(control.X, newValue);
        }

        public SystemDrawingPointEditor()
        {
            InitializeComponent();
        }
    }
}
