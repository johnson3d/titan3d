using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WPG.Themes.TypeEditors
{
    /// <summary>
    /// Interaction logic for Angle360EditorControl.xaml
    /// </summary>
    public partial class Angle360EditorControl : UserControl
    {
        // 角度值单位为度
        public float Angle
        {
            get { return (float)GetValue(AngleProperty); }
            set { SetValue(AngleProperty, value); }
        }

        public static readonly DependencyProperty AngleProperty =
            DependencyProperty.Register("Angle", typeof(float), typeof(Angle360EditorControl), new FrameworkPropertyMetadata(0.0f, new PropertyChangedCallback(OnAngleChanged)));
        public static void OnAngleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as Angle360EditorControl;

            //float newValue = (float)e.NewValue;
            //ctrl.Line_Angle_Rot.Angle = newValue;
        }

        public Angle360EditorControl()
        {
            InitializeComponent();
        }

        private void CalculateAngleFromPos(System.Windows.Point pos)
        {
            var centerPos = new System.Windows.Point(Ellipse_BG.ActualWidth * 0.5, Ellipse_BG.ActualHeight * 0.5);
            var lengthA = Math.Sqrt((pos.X - centerPos.X) * (pos.X - centerPos.X) + (pos.Y - centerPos.Y) * (pos.Y - centerPos.Y));
            var lengthB = pos.Y - centerPos.Y;
            if (pos.X > centerPos.X)
                Angle = 180 - (float)(System.Math.Acos(lengthB / lengthA) / System.Math.PI * 180);
            else
                Angle = 180 + (float)(System.Math.Acos(lengthB / lengthA) / System.Math.PI * 180);
        }

        private void Ellipse_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(Ellipse_BG);
            CalculateAngleFromPos(pos);

            Mouse.Capture(Ellipse_BG);
        }

        private void Ellipse_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var pos = e.GetPosition(Ellipse_BG);
                CalculateAngleFromPos(pos);
            }
        }

        private void Ellipse_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
        }
    }
}
