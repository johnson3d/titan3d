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

namespace WPG.Themes.TypeEditors.TransformGradient
{
    /// <summary>
    /// ColorGradient.xaml 的交互逻辑
    /// </summary>
    public partial class TransformGradient : UserControl, INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        public TransformGradient()
        {
            InitializeComponent();
        }

        public object DataCollect
        {
            get { return GetValue(DataCollectProperty); }
            set { SetValue(DataCollectProperty, value); }
        }

        public static readonly DependencyProperty DataCollectProperty =
            DependencyProperty.Register("DataCollect", typeof(object), typeof(TransformGradient),
                                    new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnObjectChanged)));


        public static void OnObjectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TransformGradient gradient = d as TransformGradient;

            DataCollect collect = e.NewValue as DataCollect;
            gradient.SetDataCollect(collect);
        }


        public void SetDataCollect(DataCollect datacollect)
        {
            UIColorGrid.Children.Clear();

            for (int i = 0; i < datacollect.Datas.Count; i++)
            {
                var data = datacollect.Datas[i];
                SelectElement element = new SelectElement();
                datacollect.BindElement(element, data);
                ResetElemetSize(element, this.Width);

                AddElement(element);
                data.value.Host = this;
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
                data.value = element.Data;
                data.value.Host = this;
                data.offset = element.Offset;
                datacollect.Datas.Add(data);
            }
        }

        List<SelectElement> SelectElements = new List<SelectElement>();
      
        public void SetSelectElement(SelectElement element)
        {
            if (SelectElementValue != null)
            {
                SelectElementValue.Selectd = false;
            }

            SelectElementValue = element;
            if (element != null)
            {
                element.Selectd = false;
                PG.Instance = element.Data;
                UILocation.Text = Math.Ceiling(element.Offset * 100).ToString();
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

            var needx = Math.Min(this.ActualWidth, Math.Max(0, x - element.ActualWidth * 0.5f));
            var oldMargin = element.Margin;
            element.Margin = new Thickness(needx, oldMargin.Top, this.ActualWidth - needx - 20, oldMargin.Bottom);
            element.Offset = (float)(x / this.ActualWidth);
        }

        public void AddElement(SelectElement element, bool need = false)
        {
            if (need && DataCollect != null)
            {
                var datas = DataCollect as DataCollect;
                datas.AddElement(element);
            }
            element.Host = this;
            UIColorGrid.Children.Add(element);
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

        public void AddDefaultElement()
        {
            SelectElement element = new SelectElement();

            AddElement(element);
        }



        private void UIColorSelect_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point point = UILocationGrid.TranslatePoint(new Point(0, 0), UIColorGrid);
            var y = point.Y;

            SelectElement element = new SelectElement();
            AddElement(element, true);
            point = e.GetPosition(UIColorGrid);
            var x = point.X - element.ActualWidth * 0.5f;
            element.Margin = new Thickness(x, -1, this.ActualWidth - x - 20, 10);//double left, double top, double right, double bottom
            element.Offset = (float)(point.X / this.ActualWidth);
            SetSelectElement(element);
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

        public GradientStopCollection GradientStopColors = new GradientStopCollection();
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
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

        double TempBaseWidth;
        public void SetPreDatas(GradientStopCollection datas, double width)
        {
            if (this.IsLoaded == false)
            {
                //TempGradientStopColors = datas;
                TempBaseWidth = width;
            }
            else
            {
            }
            
        }

        public void ResetElemetSize(SelectElement element, double width = 0)
        {
            if (width == 0 || double.IsNaN(width))
            {
                width = this.RenderSize.Width;
            }
            var x = (width * element.Offset) -  10;
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
        public Data value
        {
            get;
            set;
        }

        public DataHelper()
        {
            value = new Data();
        }

        public DataHelper Duplicate()
        {
            DataHelper data = new DataHelper();
            data.offset = offset;
            data.value = value.Duplicate();

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

        public void Save(EngineNS.IO.XndNode xndNode, bool newGuid)
        {
            //TODO..
            var att = xndNode.AddAttrib("TransformDataCollect");
            att.Version = 0;
            att.BeginWrite();
            att.Write(Datas.Count);
            for (int i = 0; i < Datas.Count; i++)
            {
                att.Write(Datas[i].offset);
            }

            att.EndWrite();

            for (int i = 0; i < Datas.Count; i++)
            {
                var subatt = xndNode.AddAttrib("TransformDataCollect_Angle_" + i);
                {
                    subatt.Version = 0;
                    subatt.BeginWrite();
                    subatt.Write(Datas[i].value.YawPitchRoll.X);
                    subatt.Write(Datas[i].value.YawPitchRoll.Y);
                    subatt.Write(Datas[i].value.YawPitchRoll.Z);
                    subatt.EndWrite();
                      
                }
                var node = xndNode.AddNode("TransformDataCollect_Placement_" + i, 0, 0);
                Datas[i].value.Placement.Save2Xnd(node);
            }
        }

        public async System.Threading.Tasks.Task Load(EngineNS.IO.XndNode xndNode)
        {
            //Todo..
            var att = xndNode.FindAttrib("TransformDataCollect");
            att.BeginRead();

            int count = 0;
            att.Read(out count);
            for (int i = 0; i < count; i++)
            {
                var data = new DataHelper();
                float offset;
                att.Read(out offset);
                data.offset = offset;
                Datas.Add(data);
            }
            att.EndRead();

            for (int i = 0; i < Datas.Count; i++)
            {
                var subatt = xndNode.FindAttrib("TransformDataCollect_Angle_" + i);
                if(subatt != null)
                {
                    if (subatt.Version == 0)
                    {
                        float Yaw, Pitch, Roll;
                        subatt.BeginRead();
                        subatt.Read(out Yaw);
                        subatt.Read(out Pitch);
                        subatt.Read(out Roll);
                        subatt.EndRead();
                        Datas[i].value.YawPitchRoll = new EngineNS.Vector3(Yaw, Pitch, Roll);
                    }

                }
                var node = xndNode.FindNode("TransformDataCollect_Placement_" + i);

                await Datas[i].value.Placement.LoadXnd(EngineNS.CEngine.Instance.RenderContext, Datas[i].value.Placement.Host, Datas[i].value.Placement.HostContainer, node);
            }
        }
    }
}
