using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// ColorGradient.xaml 的交互逻辑
    /// </summary>
    public partial class ColorGradient : UserControl, INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        public ColorGradient()
        {
            InitializeComponent();
        }
        List<SelectElement> ColorSelectElements = new List<SelectElement>();

        public object DataCollect
        {
            get { return GetValue(DataCollectProperty); }
            set { SetValue(DataCollectProperty, value); }
        }

        public static readonly DependencyProperty DataCollectProperty =
            DependencyProperty.Register("DataCollect", typeof(object), typeof(ColorGradient),
                                    new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnObjectChanged)));


        public static void OnObjectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ColorGradient panel = d as ColorGradient;

            DataCollect collect = e.NewValue as DataCollect;
            panel.SetDataCollect(collect);
        }

        public void SetDataCollect(DataCollect datacollect)
        {
            UIColorGrid.Children.Clear();
            GradientStopColors.Clear();

            for (int i = 0; i < datacollect.Datas.Count; i++)
            {
                var data = datacollect.Datas[i];
                var element = AddData(data.offset, data.value, this.Width);
                datacollect.BindElement(element, data);
            }
        }

        public SelectElement AddData(float offset, EngineNS.Color4 value, double width)
        {
            SelectElement element = new SelectElement(0);
            element.Offset = offset;
            Color color = new Color();
            color.A = (Byte)(value.Alpha * 255);
            color.R = (Byte)(value.Red * 255);
            color.G = (Byte)(value.Green * 255);
            color.B = (Byte)(value.Blue * 255);

            element.GradientStopValue = new GradientStop(color, offset);

            ResetElemetSize(element, width);

            AddElement(element);
            return element;
        }

        public void GetDataCollect(DataCollect datacollect)
        {
            datacollect.Datas.Clear();
            if (TempGradientStopColors != null && TempGradientStopColors.Count > 0)
            {
                for (int i = 0; i < TempGradientStopColors.Count; i++)
                {
                    var data = new DataHelper();

                    EngineNS.Color4 color4;
                    color4.Alpha = (float)TempGradientStopColors[i].Color.A / 255f;
                    color4.Red = (float)TempGradientStopColors[i].Color.R / 255f;
                    color4.Green = (float)TempGradientStopColors[i].Color.G / 255f;
                    color4.Blue = (float)TempGradientStopColors[i].Color.B / 255f;
                    data.value = color4;
                    data.offset = (float)TempGradientStopColors[i].Offset;
                    datacollect.Datas.Add(data);
                }
            }
            {
                SelectElement[] elements = GetElements();
                for (int i = 0; i < elements.Length; i++)
                {
                    var data = new DataHelper();
                    var element = elements[i];
                    data.value = element.GetColor4();
                    data.offset = element.Offset;
                    datacollect.Datas.Add(data);
                }
            }
            
        }

        Color mColorPicker;
        public Color ColorPicker
        {
            get => mColorPicker;
            set
            {
                mColorPicker = value;
                if (SelectElementColor != null)
                {
                    SelectElementColor.SetGradientStopColor(value);
                    //UIColor.Background = new LinearGradientBrush(GradientStopColors);
                    if (DataCollect != null)
                    {
                        var datas = DataCollect as DataCollect;
                        datas.OnChangeValue?.Invoke();
                    }
                }
                OnPropertyChanged("ColorPicker");
            }
        }
        public void SetSelectElement(SelectElement element)
        {
            if (SelectElementColor != null)
            {
                SelectElementColor.Selectd = false;
            }

            SelectElementColor = element;
            if (element != null)
            {
                element.Selectd = true;
                UIColorPicker.EditColor = element.GradientStopValue.Color;
                UILocation.Text = Math.Ceiling(element.GradientStopValue.Offset * 100).ToString();
            }
        }

        public SelectElement SelectElementColor;
        public void SetSelectElementX(double x)
        {
            if (SelectElementColor == null)
                return;

            if (SelectElementColor.IsDrag == false)
                return;

            //var needx = x - SelectElementColor.ActualWidth * 0.5f;
            var needx = Math.Min(this.ActualWidth - SelectElementColor.ActualWidth * 0.5f, Math.Max(-SelectElementColor.ActualWidth * 0.5f, x - SelectElementColor.ActualWidth * 0.5f));
            var oldMargin = SelectElementColor.Margin;
            SelectElementColor.Margin = new Thickness(needx, oldMargin.Top, this.ActualWidth - needx - 20, oldMargin.Bottom);
            SelectElementColor.Offset = Math.Min(1, Math.Max(0, (float)(x / this.ActualWidth)));

            UILocation.Text = Math.Ceiling(SelectElementColor.GradientStopValue.Offset * 100).ToString();
        }
        public void SetElementX(SelectElement element, double x)
        {
            if (element == null)
                return;

            var needx = Math.Min(this.ActualWidth, Math.Max(0, x - element.ActualWidth * 0.5f));
            var oldMargin = element.Margin;
            element.Margin = new Thickness(needx, oldMargin.Top, this.ActualWidth - needx - 20, oldMargin.Bottom);
            element.Offset = (float)(x / this.ActualWidth);
        }

        public void AddElement(SelectElement element, bool need = false)
        {
            element.Host = this;
            UIColorGrid.Children.Add(element);
           
            GradientStopColors.Add(element.GradientStopValue);
            if (need && DataCollect != null)
            {
                var datas = DataCollect as DataCollect;
                datas.AddElement(element);
            }
        }

        public void AddDefaultElement()
        {
            SelectElement element = new SelectElement(SelectElement.USETYPE.COLOR);

            AddElement(element);
        }

        public SelectElement[] GetElements()
        {
            List<SelectElement> elements = new List<SelectElement>();
            for (int i = 0; i < UIColorGrid.Children.Count; i++)
            {
                var element = UIColorGrid.Children[i] as SelectElement;
                if (element != null)
                {
                    elements.Add(element);
                }
            }
            return elements.ToArray();
        }

        private void UIColorSelect_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point point = UIColor.TranslatePoint(new Point(0, 0), UIColorGrid);
            var y = point.Y;

            SelectElement element = new SelectElement(SelectElement.USETYPE.COLOR);

            point = e.GetPosition(UIColorGrid);
            var x = point.X - element.ActualWidth * 0.5f;
            element.Margin = new Thickness(x, -1, this.ActualWidth - x - 20, 10);//double left, double top, double right, double bottom
            element.Offset = (float)(point.X / this.ActualWidth);
            SetSelectElement(element);
            AddElement(element, true);
        }


        public void MouseMoveForElement(MouseEventArgs e)
        {
            if (SelectElementColor.IsPress)
            {
                SelectElementColor.IsPress = false;
                return;
            }

            if (SelectElementColor == null || SelectElementColor.IsDrag == false)
                return;

            Point point = e.GetPosition(UIColorGrid);
            SetSelectElementX(point.X);
     
        }

        private GradientStopCollection GradientStopColors = new GradientStopCollection();
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
           
            //Color color = new Color();
            //color.A = 255;
            //color.R = 255;
            //color.G = 255;
            //color.B = 255;
            //GradientStopColors.Add(new GradientStop(color, 0.0f));
            //GradientStopColors.Add(new GradientStop(color, 1.0f));
            var BorderBrush = new LinearGradientBrush(GradientStopColors);
            BorderBrush.EndPoint = new Point(1, 0);
            UIColor.Background = BorderBrush;
            BindingOperations.SetBinding(UIColorPicker, SystemColorPicker.EditColorProperty, new Binding("ColorPicker") { Source = this, Mode = BindingMode.TwoWay });

            if (TempGradientStopColors != null && TempGradientStopColors.Count > 0)
            {
                SetDatas(TempGradientStopColors, 0);
            }

        }

        public GradientStopCollection GetGradientStopColors()
        {
            //if (TempGradientStopColors != null && TempGradientStopColors.Count > 0)
            //    return TempGradientStopColors;
            return GradientStopColors;
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            foreach (var obj in UIColorGrid.Children)
            {
                var element = obj as SelectElement;
                if (element != null)
                {
                    ResetElemetSize(element, e.NewSize.Width);
                }
            }
        }

        public void RemoveElement(SelectElement element, bool need = false)
        {
            if (element != null)
            {
                GradientStopColors.Remove(element.GradientStopValue);
                UIColorGrid.Children.Remove(element);

                if (need && DataCollect != null)
                {
                    var datas = DataCollect as DataCollect;
                    datas.RemoveElement(element);
                }
            }
            SetSelectElement(null);
        }
        private void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                if (SelectElementColor != null)
                {
                    RemoveElement(SelectElementColor, true);
                }
            }
        }

        private void UILocation_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SelectElementColor == null)
                return;

            e.Handled = true;  
            try
            {
                int value = Convert.ToInt32(UILocation.Text);
                SelectElementColor.Offset = Math.Abs(Math.Min(1.0f, (float)value / 100.0f));
                ResetElemetSize(SelectElementColor);

            }
            catch
            {
                //UILocation.Text = Math.Ceiling(SelectElementValue.GradientStopValue.Offset * 100).ToString();
            }
        }

        GradientStopCollection TempGradientStopColors;
        double TempBaseWidth;
        public void SetPreDatas(GradientStopCollection datas, double width)
        {
            if (this.IsLoaded == false)
            {
                TempGradientStopColors = datas;
                TempBaseWidth = width;
            }
            else
            {
                SetDatas(datas, width);
            }
            
        }

        public void ResetElemetSize(SelectElement element, double width = 0)
        {
            if (width == 0 || double.IsNaN(width))
            {
                width = this.RenderSize.Width;
            }
            var x = (width * element.Offset) - 10;
            element.Margin = new Thickness(x, -1, width - x - 20, 10);//double left, double top, double right, double bottom
        }
        public void SetDatas(GradientStopCollection datas, double width)
        {
            GradientStopColors.Clear();
            for (int i = 0; i < datas.Count; i++)
            {
                SelectElement element = new SelectElement(SelectElement.USETYPE.COLOR);
                element.GradientStopValue = datas[i];
                element.Offset = (float)element.GradientStopValue.Offset;
                ResetElemetSize(element, width);
                AddElement(element);
            }

            TempGradientStopColors = null;

        }

        private void UserControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (SelectElementColor != null && SelectElementColor.IsDrag)
            {
                MouseMoveForElement(e);
                SelectElementColor.UIElement_MouseMove(sender, e);

                if (DataCollect != null)
                {
                    var datas = DataCollect as DataCollect;
                    datas.OnChangeValue?.Invoke();
                }
            }
           
           
        }

        private void UserControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (SelectElementColor != null)
            {
                SelectElementColor.IsDrag = false;
            }

            Mouse.Capture(null);
        }
    }

    [EngineNS.Rtti.MetaClass]
    public class DataHelper : EngineNS.IO.Serializer.Serializer
    {
        [EngineNS.Rtti.MetaData]
        public float offset
        {
            get;
            set;
        }

        [EngineNS.Rtti.MetaData]
        public EngineNS.Color4 value
        {
            get;
            set;
        }

        public DataHelper Duplicate()
        {
            DataHelper data = new DataHelper();
            data.offset = offset;
            data.value = value;
            return data;
        }
    }

    public class DataCollect
    {
        public List<DataHelper> Datas = new List<DataHelper>();
        public Dictionary<SelectElement, DataHelper> DatasHelper = new Dictionary<SelectElement, DataHelper>();
        public delegate void OnChangeValueDelegate();
        public OnChangeValueDelegate OnChangeValue;

        public void AddElement(SelectElement element)
        {
            var data = new DataHelper();

            data.value = element.GetColor4();

            data.offset = element.Offset;
            Datas.Add(data);
            BindElement(element, data);

            OnChangeValue?.Invoke();
        }

        public void RemoveElement(SelectElement element)
        {
            DataHelper data;
            if (DatasHelper.TryGetValue(element, out data) == false)
                return;

            Datas.Remove(data);
            DatasHelper.Remove(element);
            BindElement(element, null);
            OnChangeValue?.Invoke();
        }

        public void BindElement(SelectElement element, DataHelper data)
        {
            DatasHelper.Add(element, data);
            element.SetDataHelper(data);
        }

        public DataCollect Duplicate()
        {
            DataCollect collect = new DataCollect();
            for (int i = 0; i < Datas.Count; i++)
            {
                collect.Datas.Add(Datas[i].Duplicate());
            }
            return collect;
        }
    }
}
