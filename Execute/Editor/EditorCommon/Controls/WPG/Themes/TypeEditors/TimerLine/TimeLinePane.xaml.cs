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

namespace WPG.Themes.TypeEditors.TimerLine
{
    /// <summary>
    /// ColorGradient.xaml 的交互逻辑
    /// </summary>
    public partial class TimeLinePanel : UserControl, INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        public TimeLinePanel()
        {
            InitializeComponent();
        }

        public object DataCollect
        {
            get { return GetValue(DataCollectProperty); }
            set { SetValue(DataCollectProperty, value); }
        }

        public static readonly DependencyProperty DataCollectProperty =
            DependencyProperty.Register("DataCollect", typeof(object), typeof(TimeLinePanel),
                                    new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnObjectChanged)));

    
        public static void OnObjectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TimeLinePanel panel = d as TimeLinePanel;
           
            DataCollect collect = e.NewValue as DataCollect;
            panel.SetDataCollect(collect);
        }

        public void SetDataCollect(DataCollect datacollect)
        {
            UIColorGrid.Children.Clear();

            TypeStr = datacollect.TypeStr;
            for (int i = 0; i < datacollect.Datas.Count; i++)
            {
                var data = datacollect.Datas[i];
                var element = AddData(data.offset, data.value, data.value2, this.Width);
                datacollect.BindElement(element, data);
            }
        }

        public void GetDataCollect(DataCollect datacollect)
        {
            datacollect.Datas.Clear();
            SelectElement[] elements = GetElements();
            for (int i = 0; i < elements.Length; i++)
            {
                var data = new DataHelper();
                var element = elements[i];
                data.value = new EngineNS.Vector4(element.X, element.Y, element.Z, element.W);
                data.value2 = new EngineNS.Vector4(element.X2, element.Y2, element.Z2, element.W2);
                data.offset = element.Offset;
                datacollect.Datas.Add(data);
            }


            datacollect.TypeStr = TypeStr;
        }

        string mTypeStr = "Float";
        public string TypeStr
        {
            get => mTypeStr;
            set
            {
                mTypeStr = value;
                if (mTypeStr == "Float4")
                {
                    UIYText.Visibility = Visibility.Visible;
                    UIYValue.Visibility = Visibility.Visible;

                    UIZText.Visibility = Visibility.Visible;
                    UIZValue.Visibility = Visibility.Visible;

                    UIWText.Visibility = Visibility.Visible;
                    UIWValue.Visibility = Visibility.Visible;
                    //if (DataCollect != null)
                    //{
                    //    UIXText2.Visibility = Visibility.Visible;
                    //    UIXValue2.Visibility = Visibility.Visible;

                    //    UIYText2.Visibility = Visibility.Visible;
                    //    UIYValue2.Visibility = Visibility.Visible;

                    //    UIZText2.Visibility = Visibility.Visible;
                    //    UIZValue2.Visibility = Visibility.Visible;

                    //    UIWText2.Visibility = Visibility.Visible;
                    //    UIWValue2.Visibility = Visibility.Visible;
                    //}
                }
                else if (mTypeStr == "Float3")
                {

                    UIYText.Visibility = Visibility.Visible;
                    UIYValue.Visibility = Visibility.Visible;

                    UIZText.Visibility = Visibility.Visible;
                    UIZValue.Visibility = Visibility.Visible;

                    //if (DataCollect != null)
                    //{
                    //    UIXText2.Visibility = Visibility.Visible;
                    //    UIXValue2.Visibility = Visibility.Visible;

                    //    UIYText2.Visibility = Visibility.Visible;
                    //    UIYValue2.Visibility = Visibility.Visible;

                    //    UIZText2.Visibility = Visibility.Visible;
                    //    UIZValue2.Visibility = Visibility.Visible;

                    //}
                }
                else if (mTypeStr == "Float2")
                {
                    UIYText.Visibility = Visibility.Visible;
                    UIYValue.Visibility = Visibility.Visible;
                    //if (DataCollect != null)
                    //{
                    //    UIXText2.Visibility = Visibility.Visible;
                    //    UIXValue2.Visibility = Visibility.Visible;

                    //    UIYText2.Visibility = Visibility.Visible;
                    //    UIYValue2.Visibility = Visibility.Visible;

                    //}
                }
                else if (mTypeStr == "Float")
                {
                    //if (DataCollect != null)
                    //{
                    //    UIXText2.Visibility = Visibility.Visible;
                    //    UIXValue2.Visibility = Visibility.Visible;
                    //}
                  
                }
            }
        }


        List<SelectElement> ColorSelectElements = new List<SelectElement>();


        //public void InitLines(double basewidth = 0)
        //{
        //    if (basewidth == 0)
        //    {
        //        basewidth = this.RenderSize.Width;
        //    }

        //    var data = new PathGeometry();
        //    data.Figures.Clear();
        //    for (int i = 0; i <= 100; i++)
        //    {
        //        PathFigure figure = new PathFigure();
        //        UIPathInfo.Children.Clear();
               
        //        LineSegment ls1 = new LineSegment(new Point(basewidth * i / 100f, 0), true);
        //        figure.Segments.Add(ls1);
        //        if (i % 10 == 0)
        //        {
        //            var w = basewidth * i / 100f;
        //            LineSegment ls2 = new LineSegment(new Point(w, 15), true);
        //            figure.Segments.Add(ls2);

        //            TextBlock textblock = new TextBlock();
        //            textblock.Text = i.ToString();
        //            textblock.Margin = new Thickness(w, 15, basewidth - w - 8, 0);
        //            UIPathInfo.Children.Add(textblock);
        //        }
        //        else
        //        {
        //            LineSegment ls2 = new LineSegment(new Point(basewidth * i / 100f, 10), true);

        //            figure.Segments.Add(ls2);
        //        }

        //        UIPathInfo.Children.Add(UIPathLine);
        //        data.Figures.Add(figure);
        //    }
        //    UIPathLine.Data = data;
            
        //}

        public void SetSelectElement(SelectElement element)
        {
            if (SelectElementValue != null)
            {
                SelectElementValue.Selectd = false;
            }
            SelectElementValue = element;
            if (element != null)
            {
                element.Selectd = true;
                UILocation.Text = Math.Ceiling(element.Offset * 100).ToString();
                UIXValue.Text = element.X.ToString();
                UIYValue.Text = element.Y.ToString();
                UIZValue.Text = element.Z.ToString();
                UIWValue.Text = element.W.ToString();
            }
        }

        public SelectElement SelectElementValue;
        public void SetSelectElementX(double x)
        {
            if (SelectElementValue == null)
                return;

            if (SelectElementValue.IsDrag == false)
                return;

            //var needx = x - SelectElementValue.ActualWidth * 0.5f;
            var needx = Math.Min(this.ActualWidth - SelectElementValue.ActualWidth * 0.5f, Math.Max(-SelectElementValue.ActualWidth * 0.5f, x - SelectElementValue.ActualWidth * 0.5f));
            var oldMargin = SelectElementValue.Margin;
            SelectElementValue.Margin = new Thickness(needx, oldMargin.Top, this.ActualWidth - needx - 20, oldMargin.Bottom);
            SelectElementValue.Offset = Math.Min(1, Math.Max(0, (float)(x / this.ActualWidth)));

            UILocation.Text = Math.Ceiling(SelectElementValue.Offset * 100).ToString();
        }
        public void SetElementX(SelectElement element, double x)
        {
            if (element == null)
                return;

            //var needx = x - element.ActualWidth * 0.5f;
            var needx = Math.Min(this.ActualWidth, Math.Max(0, x - element.ActualWidth * 0.5f));
            var oldMargin = element.Margin;
            element.Margin = new Thickness(needx, oldMargin.Top, this.ActualWidth - needx - 20, oldMargin.Bottom);
            element.Offset = (float)(x / this.ActualWidth);
        }

        public void AddElement(SelectElement element, bool need = false)
        {
            element.Host = this;
            UIColorGrid.Children.Add(element);
            if (need && DataCollect != null)
            {
                var datas = DataCollect as DataCollect;
                datas.AddElement(element);
            }
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
            Point point = UIColorGrid.TranslatePoint(new Point(0, 0), UIColorGrid);
            var y = point.Y;

            SelectElement element = new SelectElement();

            point = e.GetPosition(UIColorGrid);
            var x = point.X - element.ActualWidth * 0.5f;
            element.Margin = new Thickness(x, -1, this.ActualWidth - x - 20, 10);//double left, double top, double right, double bottom
            element.Offset = (float)(point.X / this.ActualWidth);
            SetSelectElement(element);
            AddElement(element, true);
        }


        public void MouseMoveForElement(MouseEventArgs e)
        {
            if (SelectElementValue.IsPress)
            {
                SelectElementValue.IsPress = false;
                return;
            }

            if (SelectElementValue == null || SelectElementValue.IsDrag == false)
                return;

            Point point = e.GetPosition(UIColorGrid);
            SetSelectElementX(point.X);

        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //InitLines(350);
            if (DataCollect != null)
            {
                SetDataCollect((DataCollect as DataCollect));
            }
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

            //InitLines();
        }

        public void RemoveElement(SelectElement element, bool need = false)
        {
            if (element != null)
            {
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
                if (SelectElementValue != null)
                {
                    RemoveElement(SelectElementValue, true);
                }
            }
        }

        private void UILocation_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SelectElementValue == null)
                return;

            e.Handled = true;
            try
            {
                int value = Convert.ToInt32(UILocation.Text);
                SelectElementValue.Offset = Math.Abs(Math.Min(1.0f, (float)value / 100.0f));
                ResetElemetSize(SelectElementValue);

            }
            catch
            {
                //UILocation.Text = Math.Ceiling(SelectElementValue.GradientStopValue.Offset * 100).ToString();
            }
        }

        public void ResetElemetSize(SelectElement element, double width = 0)
        {
            if (double.IsNaN(width) || width == 0)
            {
                width = this.RenderSize.Width;
            }
            var x = (width * element.Offset) - 10;
            element.Margin = new Thickness(x, -1, width - x - 20, 10);//double left, double top, double right, double bottom
        }

        private void UserControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (SelectElementValue != null && SelectElementValue.IsDrag)
            {
                MouseMoveForElement(e);
                SelectElementValue.UIElement_MouseMove(sender, e);

                if (DataCollect != null)
                {
                    var datas = DataCollect as DataCollect;
           
                     datas.OnChangeValue?.Invoke();

                }
            }


        }

        private void UserControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (SelectElementValue != null)
            {
                SelectElementValue.IsDrag = false;
            }

            Mouse.Capture(null);
        }

        private void UIXValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SelectElementValue == null)
                return;

            try
            {
                var value = Convert.ToSingle(UIXValue.Text);
                SelectElementValue.X = value;
            }
            catch(Exception exc)
            {
                //TODO..
            }

            e.Handled = true;
        }

        private void UIYValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SelectElementValue == null)
                return;

            try
            {
                var value = Convert.ToSingle(UIYValue.Text);
                SelectElementValue.Y = value;
            }
            catch (Exception exc)
            {
                //TODO..
            }

            e.Handled = true;
        }

        private void UIZValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SelectElementValue == null)
                return;

            try
            {
                var value = Convert.ToSingle(UIZValue.Text);
                SelectElementValue.Z = value;
            }
            catch (Exception exc)
            {
                //TODO..
            }

            e.Handled = true;
        }

        private void UIWValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SelectElementValue == null)
                return;

            try
            {
                var value = Convert.ToSingle(UIWValue.Text);
                SelectElementValue.W = value;
            }
            catch (Exception exc)
            {
                //TODO..
            }

            e.Handled = true;
        }

        public SelectElement AddData(float offset, EngineNS.Vector4 value, double width)
        {
            SelectElement element = new SelectElement();
            element.Offset = offset;
            element.X = value.X;
            element.Y = value.Y;
            element.Z = value.Z;
            element.W = value.W;
            ResetElemetSize(element, width);
            
            AddElement(element);
            return element;
        }

        public SelectElement AddData(float offset, EngineNS.Vector4 value, EngineNS.Vector4 value2, double width)
        {
            SelectElement element = new SelectElement();
            element.Offset = offset;
            element.X = value.X;
            element.Y = value.Y;
            element.Z = value.Z;
            element.W = value.W;

            element.X2 = value2.X;
            element.Y2 = value2.Y;
            element.Z2 = value2.Z;
            element.W2 = value2.W;

            ResetElemetSize(element, width);

            AddElement(element);
            return element;
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
        public EngineNS.Vector4 value
        {
            get;
            set;
        }

        [EngineNS.Rtti.MetaData]
        public EngineNS.Vector4 value2
        {
            get;
            set;
        }

        public DataHelper Duplicate()
        {

            DataHelper data = new DataHelper();
            data.offset = offset;
            data.value = value;
            data.value2 = value2;
            return data;
        }
}

    public class DataCollect
    {
        public List<DataHelper> Datas = new List<DataHelper>();
        public Dictionary< SelectElement,DataHelper> DatasHelper = new Dictionary<SelectElement, DataHelper>();
        public string TypeStr = "Float";
        public delegate void OnChangeValueDelegate();
        public OnChangeValueDelegate OnChangeValue;

        public void AddElement(SelectElement element)
        {
            var data = new DataHelper();
            data.value = new EngineNS.Vector4(element.X, element.Y, element.Z, element.W);
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
            collect.TypeStr = TypeStr;
            for (int i = 0; i < Datas.Count; i++)
            {
                collect.Datas.Add(Datas[i].Duplicate());
            }
            return collect;
        }
    }
}
