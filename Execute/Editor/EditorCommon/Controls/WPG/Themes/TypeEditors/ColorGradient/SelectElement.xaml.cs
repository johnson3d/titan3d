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

namespace WPG.Themes.TypeEditors.ColorGradient
{
    /// <summary>
    /// SelectElement.xaml 的交互逻辑
    /// </summary>
    public partial class SelectElement : UserControl
    {
        public enum USETYPE
        {
            COLOR = 0,
            ALPHA = 1,
        }
        public USETYPE UseType;
        public SelectElement(USETYPE type)
        {
            InitializeComponent();
            UseType = type;
            var color = new Color();
            color.A = 255;
            color.R = 255;
            color.G = 255;
            color.B = 255;
            GradientStopValue = new GradientStop(color, 1.0f);
        }

        bool mSelectd = false;
        public bool Selectd
        {
            get
            {
                return mSelectd;
            }
            set
            {
                mSelectd = value;
                if (value == false)
                {
                    Color color = new Color();
                    color.A = 255;
                    color.R = 255;
                    color.G = 255;
                    color.B = 255;
                    var BorderBrush = new LinearGradientBrush(color, color, new Point(0, 0), new Point(0, 1));
                    UIElement.Fill = BorderBrush;
                }
            }
        }

        public ColorGradient Host;
        public bool IsDrag = false;
        public bool IsPress = false;
        public GradientStop GradientStopValue;

        public void SetGradientStopColor(Color color)
        {
            GradientStopValue.Color = color;
            if (_DataHelper != null)
            {
                _DataHelper.value = GetColor4();
            }
        }

        public EngineNS.Color4 GetColor4()
        {
            EngineNS.Color4 color4;
            color4.Alpha = (float)GradientStopValue.Color.A / 255f;
            color4.Red = (float)GradientStopValue.Color.R / 255f;
            color4.Green = (float)GradientStopValue.Color.G / 255f;
            color4.Blue = (float)GradientStopValue.Color.B / 255f;

            return color4;
        }
        float mOffset = 0.0f;
        public float Offset
        {
            get => mOffset;
            set
            {
                mOffset = value;
                GradientStopValue.Offset = value;

                if (_DataHelper != null)
                {
                    _DataHelper.offset = value;
                }

                if (Host != null && Host.DataCollect != null)
                {
                    var datas = Host.DataCollect as DataCollect;
                    datas.OnChangeValue?.Invoke();
                }
            }
        }

        float RemoveDistance = 60;
        double RemoveY = 0;

        DataHelper _DataHelper;
        public void SetDataHelper(DataHelper data)
        {
            _DataHelper = data;
        }
        private void UIElement_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Host.SetSelectElement(this);
            IsDrag = true;
            Mouse.Capture(Host);

            e.Handled = true;

            Point point = e.GetPosition(this);
            RemoveY = point.Y;

            IsPress = true;
        }

        private void UIElement_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            IsDrag = false;
            e.Handled = true;
            Mouse.Capture(null);
        }

        public void UIElement_MouseMove(object sender, MouseEventArgs e)
        {
            if (Host == null)
                return;
            
            Point point = e.GetPosition(this);
            if (point.Y - RemoveY > RemoveDistance)
            {
                Mouse.Capture(null);
                Host.RemoveElement(this, true);
            }

        }

        private void UIElement_MouseEnter(object sender, MouseEventArgs e)
        {
            Color color = new Color();
            color.A = 255;
            color.R = 251;
            color.G = 232;
            color.B = 8;
            var BorderBrush = new LinearGradientBrush(color, color, new Point(0, 0), new Point(0, 1));
            UIElement.Fill = BorderBrush;
            this.ToolTip = $"Localtion:{ Offset * 100}\nRGBA : {GradientStopValue.Color.R},{GradientStopValue.Color.G},{GradientStopValue.Color.B},{GradientStopValue.Color.A}";
        }

        private void UIElement_MouseLeave(object sender, MouseEventArgs e)
        {
            if (Selectd == false)
            {
                Color color = new Color();
                color.A = 255;
                color.R = 255;
                color.G = 255;
                color.B = 255;
                var BorderBrush = new LinearGradientBrush(color, color, new Point(0, 0), new Point(0, 1));
                UIElement.Fill = BorderBrush;
            }
        }
    }
}
