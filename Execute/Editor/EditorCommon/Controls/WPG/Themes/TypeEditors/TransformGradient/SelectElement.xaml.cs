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
using EngineNS.GamePlay.Component;

namespace WPG.Themes.TypeEditors.TransformGradient
{
    public interface IRotationAngle
    { 
        EngineNS.Vector3 YawPitchRoll
        {
            get;
            set;
        }

    }
    [EngineNS.Rtti.MetaClass]
    public class Data : EngineNS.IO.Serializer.Serializer, EngineNS.GamePlay.Component.IPlaceable, IRotationAngle
    {
        EngineNS.GamePlay.Component.GPlacementComponent mPlacement;
        [System.ComponentModel.DisplayName("数值")]
        //[EngineNS.Rtti.MetaData]
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public EngineNS.GamePlay.Component.GPlacementComponent Placement
        {
            get
            {
                return mPlacement;
            }
            set
            {
                mPlacement = value;
                mPlacement.PlaceableHost = this;
            }
        }

        public EngineNS.Vector3 YawPitchRoll
        {
            get;
            set;
        } = EngineNS.Vector3.Zero;

        public Data()
        {
            EngineNS.GamePlay.Component.GPlacementComponent.GPlacementComponentInitializer mPlacementData = new EngineNS.GamePlay.Component.GPlacementComponent.GPlacementComponentInitializer();
            Placement = new EngineNS.GamePlay.Component.GPlacementComponent();
            var text = Placement.SetInitializer(EngineNS.CEngine.Instance.RenderContext, new EngineNS.GamePlay.Actor.GActor(), null, mPlacementData as EngineNS.GamePlay.Component.GComponent.GComponentInitializer);
            //Placement.SetMatrix(ref sys.Matrix);
        }

        public Data Duplicate()
        {
            Data data = new Data();
            data.YawPitchRoll = YawPitchRoll;
            var mat = Placement.WorldMatrix;
            data.Placement.SetMatrix(ref mat);

            return data;
        }

        public TransformGradient Host;
        public  void OnPlacementChanged(GPlacementComponent placement)
        {
            //throw new NotImplementedException();
            if (Host != null && Host.DataCollect != null)
            {
                var datas = Host.DataCollect as DataCollect;
                datas.OnChangeValue?.Invoke();
            }
        }

        public void OnPlacementChangedUninfluencePhysics(GPlacementComponent placement)
        {
            //throw new NotImplementedException();
        }
    }
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

        public TransformGradient Host;
        public bool IsDrag = false;
        public bool IsPress = false;
        DataHelper _DataHelper;
        public void SetDataHelper(DataHelper data)
        {
            _DataHelper = data;
            if (_DataHelper != null && _DataHelper.value != null)
            {
                _DataHelper.value.Host = Host;
            }
        }
        float mOffset = 0.0f;
        public float Offset
        {
            get
            {
                return _DataHelper.offset;
            }
            set
            {
                _DataHelper.offset = value;
                if (Host != null && Host.DataCollect != null)
                {
                    var datas = Host.DataCollect as DataCollect;
                    datas.OnChangeValue?.Invoke();
                }
            }
        }

        public Data Data
        {
            get
            {
                return _DataHelper.value;
            }
            set
            {
                _DataHelper.value = value;
                _DataHelper.value.Host = Host;
                if (Host != null && Host.DataCollect != null)
                {
                    var datas = Host.DataCollect as DataCollect;
                    datas.OnChangeValue?.Invoke();
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

            if (Data != null)
            {
                this.ToolTip = $"Offset:{ Offset * 100}\n Location: {Data.Placement.Location.X},{Data.Placement.Location.Y},{Data.Placement.Location.Z}\nRotation: {Data.Placement.Rotation.X},{Data.Placement.Rotation.Y},{Data.Placement.Rotation.Z},{Data.Placement.Rotation.W}\nScale: {Data.Placement.Scale.X},{Data.Placement.Scale.Y},{Data.Placement.Scale.Z}";
            }
            
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
