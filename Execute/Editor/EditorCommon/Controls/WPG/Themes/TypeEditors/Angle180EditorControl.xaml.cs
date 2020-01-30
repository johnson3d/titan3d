using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WPG.Themes.TypeEditors
{
    /// <summary>
    /// Interaction logic for Angle180EditorControl.xaml
    /// </summary>
    public partial class Angle180EditorControl : UserControl
    {
        // 角度值单位为度
        public float Angle
        {
            get { return (float)GetValue(AngleProperty); }
            set { SetValue(AngleProperty, value); }
        }

        public static readonly DependencyProperty AngleProperty =
            DependencyProperty.Register("Angle", typeof(float), typeof(Angle180EditorControl), new FrameworkPropertyMetadata(0.0f, new PropertyChangedCallback(OnAngleChanged)));
        public static void OnAngleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as Angle180EditorControl;

            float newValue = (float)e.NewValue;
            ctrl.Arc_Angle.EndAngle = 90 - newValue;
            //ctrl.Line_Angle_Rot.Angle = newValue;
        }

        public Angle180EditorControl()
        {
            InitializeComponent();
        }

        private void CalculateAngleFromPos(System.Windows.Point pos)
        {
            var centerPos = new System.Windows.Point(Arc_BG.ActualWidth * 0.5, Arc_BG.ActualHeight * 0.5);
            var lengthA = Math.Sqrt((pos.X - centerPos.X) * (pos.X - centerPos.X) + (pos.Y - centerPos.Y) * (pos.Y - centerPos.Y));
            var lengthB = pos.Y - centerPos.Y;
            float tempAngle = 0;
            if (pos.X > centerPos.X)
                tempAngle = -90 + (float)(System.Math.Acos(lengthB / lengthA) / System.Math.PI * 180);
            else
                tempAngle = 270 - (float)(System.Math.Acos(lengthB / lengthA) / System.Math.PI * 180);

            if (tempAngle < 0)
                tempAngle = 0;
            else if (tempAngle > 180)
                tempAngle = 180;
            Angle = tempAngle;
        }

        private void Arc_BG_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(Arc_BG);
            CalculateAngleFromPos(pos);

            Mouse.Capture(Arc_BG);
        }

        private void Arc_BG_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var pos = e.GetPosition(Arc_BG);
                CalculateAngleFromPos(pos);
            }
        }

        private void Arc_BG_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
        }
    }
}
