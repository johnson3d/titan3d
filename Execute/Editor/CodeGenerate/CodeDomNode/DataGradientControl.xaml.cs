using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using CodeGenerateSystem.Base;
using CodeGenerateSystem.Controls;
using WPG.Themes.TypeEditors.TimerLine;

namespace CodeDomNode
{
    [CodeGenerateSystem.ShowInNodeList("数值/Float数据(DataGradientControl)", "Float--Float4")]
    public sealed partial class DataGradientControl
    { 
        partial void InitConstruction()
        {
            this.InitializeComponent();

            mChildNodeContainer = StackPanel_ElementPins;
            var param = CSParam as DataGradientControlConstructParam;
            this.Width = param.Width;
            SetChildrenWidth(this.Width);
            SetChildrenHeight(param.Height);
            mCtrlValueIn = ValueIn;

        }

        public void SetChildrenWidth(double width)
        {
            width = width - 112;
            //StackPanel_ElementPins.Width = width;
            UITimeLineWidth.Width = new GridLength(width);
            foreach (var node in mChildNodes)
            {
                var gradientnode = node as DataGradientElement;
                if (gradientnode != null)
                {
                    gradientnode.RenderWidth = width;
                }
            }
        }

        public void SetChildrenHeight(double height)
        {
            this.Height = height;
            UITimeLineHeight.Height = new GridLength(height);
        }

        protected override void OnSizeChanged(double width, double height)
        {
            var param = CSParam as DataGradientControlConstructParam;
            param.Width = width;
            //UITimeLineWidth.Width = new GridLength(width - 112);
            SetChildrenWidth(width);
        }

        
        public void AddDataControl(double height)
        {
            UITimeLineHeight.Height = new GridLength(height + UITimeLineHeight.Height.Value);
            this.Height += height;
            var param = CSParam as DataGradientControlConstructParam;
            param.Height = this.Height;
        }

        private void BaseNodeControl_Loaded(object sender, RoutedEventArgs e)
        {
            //UITimeLine.AddDataControl -= AddDataControl;
            //UITimeLine.AddDataControl += AddDataControl;
            //UITimeLine.
            UITimeLine.OKEvent -= AddPanel;
            UITimeLine.OKEvent += AddPanel;
        }

        void AddElementData(string TypeStr)
        {
            var param = CreateConstructionParam(typeof(DataGradientElement)) as DataGradientElementConstructParam;
            param.CSType = CSParam.CSType;
            param.HostNodesContainer = this.HostNodesContainer;
            param.TypeStr = TypeStr;
            var elm = new DataGradientElement(param);
            elm.TypeStr = TypeStr;
            elm.ElementIdx = mChildNodes.Count;
            AddChildNode(elm, StackPanel_ElementPins);
            SetChildrenWidth(this.Width);
        }

        public void AddPanel(string typestr)
        {
            if (string.IsNullOrEmpty(typestr) == false)
            {
                AddElementData(typestr);
                AddDataControl(130);
            }
            Popup_Edit.IsOpen = false;
        }

        private void Button_AddPin_Click(object sender, RoutedEventArgs e)
        {
            //var createwindow = new SelectType();
            //createwindow.ShowDialog();
            //if (string.IsNullOrEmpty(createwindow.TypeStr) == false)
            //{
            //    AddElementData(createwindow.TypeStr);
            //    AddDataControl(130);
            //}
            Popup_Edit.IsOpen = !Popup_Edit.IsOpen;
           
        }

        public void UpdateElementIndexes()
        {
            for (int i = 0; i < mChildNodes.Count; i++)
            {
                var elm = mChildNodes[i] as DataGradientElement;
                if (elm != null)
                    elm.ElementIdx = i;
            }
        }

        public override BaseNodeControl Duplicate(DuplicateParam param)
        {
            var copyedNode = base.Duplicate(param) as DataGradientControl;
            for(int i=0; i<mChildNodes.Count; i++)
            {
                var elm = mChildNodes[i] as DataGradientElement;
                var copyedElm = elm.Duplicate(param) as DataGradientElement;
                copyedNode.AddChildNode(copyedElm, copyedNode.StackPanel_ElementPins);
                copyedNode.SetChildrenWidth(copyedNode.Width);
                copyedNode.AddDataControl(130);
            }
            return copyedNode;
        }
    }

    public partial class DataGradientElement
    {
        StackPanel mPanel;
        TimeLinePanel mTimeLine;
        public TimeLinePanel TimeLine
        {
            get => mTimeLine;
        }
        public string TypeStr
        {
            get => mTimeLine.TypeStr;
            set
            {
                mTimeLine.TypeStr = value;
                SetValueType(value);
            }
        }

        public double RenderWidth
        {
            get
            {
                var param = CSParam as DataGradientElementConstructParam;
                return param.RenderWidth;
            }
            set
            {
                var param = CSParam as DataGradientElementConstructParam;
                param.RenderWidth = value;

                this.Width = value;
                mPanel.Width = value;
                mTimeLine.Width = value - 40;
            }
        }
        partial void SetValueType(string typestr);
        partial void InitConstruction()
        {
            Resources = new ResourceDictionary()
            {
                Source = new Uri("pack://application:,,,/CodeGenerateSystem;component/Themes/Generic.xaml", UriKind.Absolute),
            };

            mPanel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                //Margin = new Thickness(0, 0, 8, 8),
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };

            AddChild(mPanel);
            mElemPin = new CodeGenerateSystem.Controls.LinkOutControl()
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                Direction = CodeGenerateSystem.Base.enBezierType.Right,
                PinType = CodeGenerateSystem.Base.LinkPinControl.enPinType.Normal,
                Margin = new Thickness(0, 0, 8, 8),
            };
            
            mElemPin.NameString = "[0]";
            mElemPin.OnCollectionContextMenus = (CodeGenerateSystem.Base.LinkPinControl linkControl) =>
            {
                mElemPin.AddContextMenuItem("移除节点", "Sequence", (obj, arg) =>
                {
                    RemoveFromParent();
                });
            };

            mTimeLine = new TimeLinePanel()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };

            mPanel.Children.Add(mTimeLine);
            mPanel.Children.Add(mElemPin);
        }

        partial void GetGradientDatas()
        {
            GradientDatas.Clear();

            var elements = mTimeLine.GetElements();

            for (int i = 0; i < elements.Length; i++)
            {
                GradientData data = new GradientData();
                data.Offset = elements[i].Offset;
                data.Value = elements[i].Value;
                GradientDatas.Add(data);
            }

            GradientDatas.Sort((a, b) =>
            {
                if (b.Offset > a.Offset)
                    return 1;
                if (b.Offset == a.Offset)
                    return 0;
                else
                    return -1;
            });
        }

        partial void SetGradientDatas(List<GradientData> datas)
        {
            if (datas == null)
                return;

            for (int i = 0; i < datas.Count; i++)
            {
                var element = mTimeLine.AddData(datas[i].Offset, datas[i].Value, RenderWidth);
            }

            //mTimeLine.InitLines(RenderWidth);
        }

  //      protected override void OnSizeChanged(double width, double height)
    //    {
     //       CallSizeChanged(width, height);
      //  }

        public void CallSizeChanged(double width, double height)
        {
            RenderWidth = width;
        }

        public override BaseNodeControl Duplicate(DuplicateParam param)
        {
            var copyedNode = base.Duplicate(param) as DataGradientElement;
            GetGradientDatas();
            copyedNode.GradientDatas = new List<GradientData>(GradientDatas);
            copyedNode.SetGradientDatas(copyedNode.GradientDatas);
            return copyedNode;
        }
    }
}
