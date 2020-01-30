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

namespace WPG.Themes.TypeEditors.TimerLine
{
    /// <summary>
    /// SelectElement.xaml 的交互逻辑
    /// </summary>
    public partial class SelectElement : UserControl
    {
        public SelectElement()
        {
            InitializeComponent();
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

        public TimeLinePanel Host;
        public bool IsDrag = false;
        public bool IsPress = false;
        float mOffset = 0.0f;
        DataHelper _DataHelper;
        public void SetDataHelper(DataHelper data)
        {
            _DataHelper = data;
        }

        public float Offset
        {
            get => mOffset;
            set
            {
                mOffset = value;
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

        EngineNS.Vector4 mValua = new EngineNS.Vector4(0, 0, 0, 0);
        EngineNS.Vector4 mValua2 = new EngineNS.Vector4(0, 0, 0, 0);

        public EngineNS.Vector4 Value
        {
            get => mValua;
        }

        public float X
        {
            get
            {
                return mValua.X;
            }
            set
            {
                mValua.X = value;
                if (_DataHelper != null)
                {
                    _DataHelper.value = mValua;
                }

                if (Host != null && Host.DataCollect != null)
                {
                    var datas = Host.DataCollect as DataCollect;

                    datas.OnChangeValue?.Invoke();
                }
            }
        }

        public float Y
        {
            get
            {
                return mValua.Y;
            }
            set
            {
                mValua.Y = value;
                if (_DataHelper != null)
                {
                    _DataHelper.value = mValua;
                }

                if (Host != null && Host.DataCollect != null)
                {
                    var datas = Host.DataCollect as DataCollect;

                    datas.OnChangeValue?.Invoke();
                }
            }
        }

        public float Z
        {
            get
            {
                return mValua.Z;
            }
            set
            {
                mValua.Z = value;
                if (_DataHelper != null)
                {
                    _DataHelper.value = mValua;
                }

                if (Host != null && Host.DataCollect != null)
                {
                    var datas = Host.DataCollect as DataCollect;

                    datas.OnChangeValue?.Invoke();
                }
            }
        }

        public float W
        {
            get
            {
                return mValua.W;
            }
            set
            {
                mValua.W = value;
                if (_DataHelper != null)
                {
                    _DataHelper.value = mValua;
                }

                if (Host != null && Host.DataCollect != null)
                {
                    var datas = Host.DataCollect as DataCollect;

                    datas.OnChangeValue?.Invoke();
                }
            }
        }

        public EngineNS.Vector4 Value2
        {
            get => mValua2;
        }

        public float X2
        {
            get
            {
                return mValua2.X;
            }
            set
            {
                mValua2.X = value;
                if (_DataHelper != null)
                {
                    _DataHelper.value2 = mValua2;
                }
            }
        }

        public float Y2
        {
            get
            {
                return mValua2.Y;
            }
            set
            {
                mValua2.Y = value;
                if (_DataHelper != null)
                {
                    _DataHelper.value2 = mValua2;
                }
            }
        }

        public float Z2
        {
            get
            {
                return mValua2.Z;
            }
            set
            {
                mValua2.Z = value;
                if (_DataHelper != null)
                {
                    _DataHelper.value2 = mValua2;
                }
            }
        }

        public float W2
        {
            get
            {
                return mValua2.W;
            }
            set
            {
                mValua2.W = value;
                if (_DataHelper != null)
                {
                    _DataHelper.value2 = mValua2;
                }
            }
        }

        float RemoveDistance = 60;
        double RemoveY = 0;
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

            this.ToolTip = $"Localtion:{ Offset * 100}\nX : {mValua.X} Y : {mValua.Y} Z : {mValua.Z} W : {mValua.W}\nX2 : {mValua2.X} Y2 : {mValua2.Y} Z2 : {mValua2.Z} W2 : {mValua2.W}";
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
