using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using CodeGenerateSystem.Base;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace CodeDomNode
{
    public class ListProcessCommon
    {
        public static void OnValueTypeChanged(ref FrameworkElement valueControl, StackPanel parentPanel, Type valueType, ref object value, string valueName, BaseNodeControl node)
        {
            if (valueControl != null)
                parentPanel.Children.Remove(valueControl);
            if (valueType == typeof(bool))
            {
                var ctrl = new CheckBox()
                {
                    VerticalAlignment = VerticalAlignment.Center,
                };
                if (value == null)
                    value = false;
                parentPanel.Children.Add(ctrl);
                ctrl.SetBinding(CheckBox.IsCheckedProperty, new Binding(valueName) { Source = node, Converter = new EditorCommon.Converter.Bool2String() });
                valueControl = ctrl;
            }
            else if (valueType.IsEnum)
            {
                var ctrl = new WPG.Themes.TypeEditors.EnumEditor()
                {
                    VerticalAlignment = VerticalAlignment.Center,
                };
                if (value == null)
                    value = System.Enum.GetValues(valueType).GetValue(0);
                parentPanel.Children.Add(ctrl);
                ctrl.SetBinding(WPG.Themes.TypeEditors.EnumEditor.EnumObjectProperty, new Binding(valueName) { Source = node });
                valueControl = ctrl;
            }
            else if ((valueType == typeof(string)))
            {
                var ctrl = new TextBox()
                {
                    Style = node.TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "TextBoxStyle_Default")) as Style,
                    VerticalAlignment = VerticalAlignment.Center,
                    MinWidth = 30,
                };
                if (value == null)
                    value = "";
                parentPanel.Children.Add(ctrl);
                ctrl.SetBinding(TextBox.TextProperty, new Binding(valueName) { Source = node, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
                valueControl = ctrl;
            }
            else if (valueType.IsValueType && valueType.IsPrimitive)
            {
                var ctrl = new EditorCommon.NumericTypeEditor()
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    MinWidth = 30,
                };
                if (value == null)
                    value = 0;
                ctrl.NumericObject = value;
                parentPanel.Children.Add(ctrl);
                ctrl.SetBinding(EditorCommon.NumericTypeEditor.NumericObjectProperty, new Binding(valueName) { Source = node });
                valueControl = ctrl;
            }
        }
    }

    [CodeGenerateSystem.ShowInNodeList("List/MakeList", "")]
    public sealed partial class MakeList
    {
        public System.Windows.Media.Brush ArrayTypeBrush
        {
            get { return (System.Windows.Media.Brush)GetValue(ArrayTypeBrushProperty); }
            set { SetValue(ArrayTypeBrushProperty, value); }
        }
        public static readonly DependencyProperty ArrayTypeBrushProperty = DependencyProperty.Register("ArrayTypeBrush", typeof(System.Windows.Media.Brush), typeof(MakeList), new UIPropertyMetadata(System.Windows.Media.Brushes.Gray));

        partial void InitConstruction()
        {
            InitializeComponent();
            mArrayOutPin = ArrayOut;
            AddElementPin();
        }

        public override bool CanLink(LinkPinControl linkPin, LinkPinControl pinInfo)
        {
            if ((pinInfo.LinkType & enLinkType.Enumerable) != enLinkType.Enumerable)
                return false;

            // todo: 类型判断
            //pinInfo.ClassType

            return true;
        }

        partial void OnOrigionTypeChanged_WPF()
        {
            if(mArrayOutPin.HasLink)
            {
                //var linkNode = linkInfo.GetLinkObject(0, false);
                //var linkElement = linkInfo.GetLinkElement(0, false);
                //var type = linkNode.GCode_GetType(linkElement);
                //if (type == null || type == typeof(object))
                //    linkNode.RefreshFromLink(linkElement);
                //else if(type.IsGenericType)
                //{
                //    if (type.GetGenericArguments()[0] == typeof(object))
                //        linkNode.RefreshFromLink(linkElement);
                //}
                for(int i=0; i< mArrayOutPin.GetLinkInfosCount(); i++)
                    mArrayOutPin.GetLinkedObject(i, false).RefreshFromLink(mArrayOutPin.GetLinkedPinControl(i, false), i);
            }
        }
        
        int ElementHasLinkCount()
        {
            int retValue = 0;
            foreach (var child in mChildNodes)
            {
                var elm = child as MakeListElement;
                if (elm != null)
                {
                    if (elm.HasLink())
                        retValue++;
                }
            }

            return retValue;
        }

        void AddElementPin()
        {
            var param = CodeGenerateSystem.Base.BaseNodeControl.CreateConstructionParam(typeof(MakeListElement));
            param.CSType = CSParam.CSType;
            param.HostNodesContainer = this.HostNodesContainer;
            var elm = new MakeListElement(param);
            elm.ElementIdx = mChildNodes.Count;
            elm.OrigionType = OrigionType;
            AddChildNode(elm, StackPanel_ElementPins);
        }
        private void Button_AddPin_Click(object sender, RoutedEventArgs e)
        {
            AddElementPin();
        }

        public void UpdateElementIndexes()
        {
            for(int i=0; i<mChildNodes.Count; i++)
            {
                var elm = mChildNodes[i] as MakeListElement;
                if (elm != null)
                    elm.ElementIdx = i;
            }
        }

        protected override void CollectionErrorMsg()
        {
            if (!ArrayOut.HasLink)
                return;

            var type1 = GetElementTypeFromArrayLink();
            var type2 = GetElementTypeFromElementChildren();
            if( type1 != type2 &&
               !type1.IsSubclassOf(type2) && 
               !type2.IsSubclassOf(type1))
            {
                HasError = true;
                ErrorDescription = "类型不匹配";
            }

            foreach(var child in mChildNodes)
            {
                var elm = child as MakeListElement;
                if (elm == null)
                    continue;

                try
                {
                    var val = System.Convert.ChangeType(elm.ElementValue, elm.OrigionType);
                }
                catch(System.Exception)
                {
                    HasError = true;
                    ErrorDescription = $"无效的类型,{elm.ElementValue}不能转换为类型{EngineNS.Rtti.RttiHelper.GetAppTypeString(elm.OrigionType)}";
                }
            }
        }

        public override void RefreshFromLink(LinkPinControl pin, int linkIndex)
        {
            if(OrigionType == typeof(object))
            {
                if(pin == mArrayOutPin)
                {
                    OrigionType = GetElementTypeFromArrayLink();
                }
                else
                {
                    OrigionType = GetElementTypeFromElementChildren();
                }
            }
        }

        public override BaseNodeControl Duplicate(DuplicateParam param)
        {
            var copyedNode = base.Duplicate(param) as MakeList;
            copyedNode.OrigionType = OrigionType;
            copyedNode.ClearChildNode();
            foreach(var child in mChildNodes)
            {
                var copyedChild = child.Duplicate(param);
                copyedNode.AddChildNode(copyedChild, copyedNode.StackPanel_ElementPins);
            }
            return copyedNode;
        }
    }

    public partial class MakeListElement
    {
        public System.Windows.Media.Brush ArrayTypeBrush
        {
            get { return (System.Windows.Media.Brush)GetValue(ArrayTypeBrushProperty); }
            set { SetValue(ArrayTypeBrushProperty, value); }
        }
        public static readonly DependencyProperty ArrayTypeBrushProperty = DependencyProperty.Register("ArrayTypeBrush", typeof(System.Windows.Media.Brush), typeof(MakeListElement), new UIPropertyMetadata(System.Windows.Media.Brushes.Gray));

        StackPanel mPanel;
        FrameworkElement mValueControl;

        partial void InitConstruction()
        {
            Resources = new ResourceDictionary()
            {
                Source = new Uri("pack://application:,,,/CodeGenerateSystem;component/Themes/Generic.xaml", UriKind.Absolute),
            };

            mPanel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 8, 8),
            };
            AddChild(mPanel);
            mElemPin = new CodeGenerateSystem.Controls.LinkInControl()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                Direction = CodeGenerateSystem.Base.enBezierType.Left,
            };
            mPanel.Children.Add(mElemPin);
            mElemPin.NameString = "[0]";
            mElemPin.SetBinding(CodeGenerateSystem.Controls.LinkInControl.BackBrushProperty, new Binding("ArrayTypeBrush") { Source = this });
            mElemPin.OnCollectionContextMenus = (CodeGenerateSystem.Base.LinkPinControl linkControl) =>
            {
                mElemPin.AddContextMenuItem("移除节点", "MakeList", (obj, arg) =>
                {
                    RemoveFromParent();
                    ((MakeList)ParentNode).UpdateElementIndexes();
                });
            };

        }

        public override bool CanLink(LinkPinControl linkPin, LinkPinControl pinInfo)
        {
            if(mOrigionType == typeof(string))
            {
                return pinInfo.LinkType == enLinkType.String;
            }
            else if(mOrigionType != typeof(string) && mOrigionType.IsValueType && mOrigionType.IsPrimitive)
            {
                return (pinInfo.LinkType & enLinkType.NumbericalValue) == pinInfo.LinkType;
            }

            var type = EngineNS.Rtti.RttiHelper.GetTypeFromTypeFullName(pinInfo.ClassType);
            if (type == null)
                return false;
            if (type == mOrigionType)
                return true;
            if (type.IsSubclassOf(mOrigionType))
                return true;

            return true;
        }

        partial void OnOrigionTypeChanged()
        {
            ListProcessCommon.OnValueTypeChanged(ref mValueControl, mPanel, mOrigionType, ref mElementValue, "ElementValue", this);
        }

        public System.Windows.Media.Brush GetLinkTargetBrush()
        {
            if (mElemPin != null && mElemPin.HasLink)
                return mElemPin.GetLinkedPinControl(0, true).BackBrush;
            else
                return System.Windows.Media.Brushes.Gray;
        }

        public override BaseNodeControl Duplicate(DuplicateParam param)
        {
            var copyedNode = base.Duplicate(param) as MakeListElement;
            copyedNode.ElementIdx = ElementIdx;
            copyedNode.mElementValueStr = mElementValueStr;
            copyedNode.ElementValue = ElementValue;
            copyedNode.OrigionType = OrigionType;
            return copyedNode;
        }
    }
}