using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using CodeGenerateSystem.Base;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace CodeDomNode
{
    [CodeGenerateSystem.ShowInNodeList("逻辑/switch", "条件选择节点，根据条件选择执行的脚本")]
    public partial class SwitchNode : CodeGenerateSystem.Base.IDebugableNode
    {
        bool mBreaked = false;
        public bool Breaked
        {
            get { return mBreaked; }
            set
            {
                if (mBreaked == value)
                    return;
                mBreaked = value;
                EngineNS.CEngine.Instance.EventPoster.RunOn(() =>
                {
                    BreakedPinShow = mBreaked;
                    ChangeParentLogicLinkLine(mBreaked);
                    return true;
                }, EngineNS.Thread.Async.EAsyncTarget.Editor);
            }
        }
        public void ChangeParentLogicLinkLine(bool change)
        {
            ChangeParentLogicLinkLine(change, MethodLink_Pre);
        }
        public override void Tick(long elapsedMillisecond)
        {
            TickDebugLine(elapsedMillisecond, MethodLink_Pre);
        }
        public bool CanBreak()
        {
            return true;
        }

        partial void InitConstruction()
        {
            InitializeComponent();

            mCtrlMethodLink_Pre = MethodLink_Pre;
            mCtrlMethodLink_Next = MethodLink_Next;
            mCtrlSwitchItemPin = SwitchItemPin;
            mCtrlMethodLink_Default = MethodLinkPin_Default;

            mChildNodeContainer = CaseStack;
            SetUpLinkElement(MethodLink_Pre);

            AddCasePin();
        }

        private void Button_AddCase_Click(object sender, RoutedEventArgs e)
        {
            AddCasePin();
        }

        void AddCasePin()
        {
            var param = CodeGenerateSystem.Base.BaseNodeControl.CreateConstructionParam(typeof(CaseControl));
            param.CSType = CSParam.CSType;
            param.HostNodesContainer = this.HostNodesContainer;
            var caseCtrl = new CaseControl(param);
            caseCtrl.OrigionType = OrigionType;
            AddChildNode(caseCtrl, CaseStack);
        }

        protected override void CollectionErrorMsg()
        {
            if(!mCtrlSwitchItemPin.HasLink)
            {
                HasError = true;
                ErrorDescription = "target没有连接";
                return;
            }
            if(OrigionType.IsPrimitive)
            {
                int i = 0;
                string[] strArray = new string[mChildNodes.Count];
                foreach (var child in mChildNodes)
                {
                    var cc = child as CaseControl;
                    if (cc.CasePin.HasLink)
                        continue;

                    if(string.IsNullOrEmpty(cc.CaseValueStr))
                    {
                        HasError = true;
                        ErrorDescription = $"第{i+1}个case参数没有设置";
                        return;
                    }

                    if((cc.CasePin.LinkType & enLinkType.IntegerTypeValue) != 0)
                    {
                        // 判断只有数字
                        Int64 val;
                        if(!Int64.TryParse(cc.CaseValueStr, out val))
                        {
                            HasError = true;
                            ErrorDescription = $"第{i+1}个case参数不能转换为数字";
                            return;
                        }
                    }
                    else if((cc.CasePin.LinkType & enLinkType.FloatTypeValue) != 0)
                    {
                        // 判断只有.和数字
                        double val;
                        if(!double.TryParse(cc.CaseValueStr, out val))
                        {
                            HasError = true;
                            ErrorDescription = $"第{i+1}个case参数不能转换为数字";
                            return;
                        }
                    }
                    else if(cc.CasePin.LinkType != enLinkType.String)
                    {
                        HasError = true;
                        ErrorDescription = $"第{i+1}个case没有连接";
                        return;
                    }
                    strArray[i] = cc.CaseValueStr;

                    i++;
                }
                for(i=0; i<strArray.Length-1; i++)
                {
                    for(int j=i+1; j<strArray.Length; j++)
                    {
                        if(strArray[i] == strArray[j])
                        {
                            HasError = true;
                            ErrorDescription = $"第{i+1}个case与{j+1}个case相同";
                            return;
                        }
                    }
                }
            }
            else if(OrigionType.IsEnum)
            {
                for(int i=0; i<mChildNodes.Count; i++)
                {
                    var cc = mChildNodes[i] as CaseControl;
                    if(!cc.CasePin.HasLink)
                    {
                        if(string.IsNullOrEmpty(cc.CaseValueStr))
                        {
                            HasError = true;
                            ErrorDescription = $"第{i+1}个case没有设置值";
                            return;
                        }

                        for(int j=i+1; j<mChildNodes.Count; j++)
                        {
                            var ccb = mChildNodes[j] as CaseControl;
                            if(!ccb.CasePin.HasLink)
                            {
                                if(cc.CaseValueStr == ccb.CaseValueStr)
                                {
                                    HasError = true;
                                    ErrorDescription = $"第{i+1}个case和第{j+1}个case值相同";
                                    return;
                                }
                            }
                        }
                    }
                }
            }
            else if(OrigionType != typeof(string))
            {
                int i = 1;
                foreach(var child in mChildNodes)
                {
                    var cc = child as CaseControl;
                    if(!cc.CasePin.HasLink)
                    {
                        HasError = true;
                        ErrorDescription = $"第{i}个case没有连接";
                        return;
                    }
                    i++;
                }
            }
        }

        public override BaseNodeControl Duplicate(DuplicateParam param)
        {
            var copyedNode = base.Duplicate(param) as SwitchNode;
            copyedNode.OrigionType = OrigionType;
            copyedNode.ClearChildNode();
            foreach(var child in mChildNodes)
            {
                var copyedChild = child.Duplicate(param);
                copyedNode.AddChildNode(copyedChild, copyedNode.CaseStack);
            }
            return copyedNode;
        }
    }

    public partial class CaseControl
    {
        Grid mValueGrid;

        partial void OnOrigionTypeChanged_WPF()
        {
            if (mValueGrid == null)
                return;

            mValueGrid.Children.Clear();
            if(mOrigionType.IsEnum)
            {
                var cb = new ComboBox()
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    Style = TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "ComboBoxStyle_Default")) as Style,
                };
                mValueGrid.Children.Add(cb);
                foreach(var val in System.Enum.GetNames(mOrigionType))
                {
                    cb.Items.Add(val);
                }
                cb.SelectedItem = mCaseValueStr;
                cb.SelectionChanged += (object sender, SelectionChangedEventArgs e)=>
                {
                    mCaseValueStr = e.AddedItems[0].ToString();
                };
            }
            else if(mOrigionType.IsPrimitive || (mOrigionType == typeof(string)))
            {
                var tb = new TextBox()
                {
                    MinWidth = 80,
                    VerticalAlignment = VerticalAlignment.Center,
                    Style = TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "TextBoxStyle_Default")) as Style,
                };
                mValueGrid.Children.Add(tb);
                tb.SetBinding(TextBox.TextProperty, new Binding("CaseValueStr") { Source = this, Mode=BindingMode.TwoWay, UpdateSourceTrigger=UpdateSourceTrigger.PropertyChanged });
            }
        }

        partial void InitConstruction()
        {
            Resources = new ResourceDictionary()
            {
                Source = new Uri("pack://application:,,,/CodeGenerateSystem;component/Themes/Generic.xaml", UriKind.Absolute),
            };

            NodeType = enNodeType.ChildNode;

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1.0f, GridUnitType.Auto) });
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1.0f, GridUnitType.Auto) });
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1.0f, GridUnitType.Star) });
            AddChild(grid);

            mCasePin = new CodeGenerateSystem.Controls.LinkInControl()
            {
                Margin = new Thickness(8, 3, 3, 3),
                NameString = "case",
                HorizontalAlignment = HorizontalAlignment.Left,
                Direction = CodeGenerateSystem.Base.enBezierType.Left,
            };
            mCasePin.OnCollectionContextMenus = (CodeGenerateSystem.Base.LinkPinControl linkControl) =>
            {
                mCasePin.AddContextMenuItem("移除节点", "switch", (obj, arg) =>
                {
                    RemoveFromParent();
                });
            };
            grid.Children.Add(mCasePin);
            Grid.SetColumn(mCasePin, 0);

            mValueGrid = new Grid();
            grid.Children.Add(mValueGrid);
            Grid.SetColumn(mValueGrid, 1);

            mCaseMethod = new CodeGenerateSystem.Controls.LinkOutControl()
            {
                Margin = new Thickness(8,3,8,3),
                HorizontalAlignment = HorizontalAlignment.Right,
                Direction = CodeGenerateSystem.Base.enBezierType.Right,
                PinType = CodeGenerateSystem.Base.LinkPinControl.enPinType.Exec,
            };
            grid.Children.Add(mCaseMethod);
            Grid.SetColumn(mCaseMethod, 2);
        }

        public override bool CanLink(LinkPinControl selfLinkPin, LinkPinControl otherLinkPin)
        {
            if(selfLinkPin == mCasePin)
            {
                var switchNode = ParentNode as SwitchNode;
                if(!switchNode.SwitchItemPin.HasLink)
                    return false;

                if (mOrigionType == typeof(string))
                {
                    return otherLinkPin.LinkType == enLinkType.String;
                }
                else if(mOrigionType != typeof(string) && mOrigionType.IsValueType && mOrigionType.IsPrimitive)
                {
                    return (otherLinkPin.LinkType & enLinkType.NumbericalValue) == otherLinkPin.LinkType;
                }

                var type = EngineNS.Rtti.RttiHelper.GetTypeFromTypeFullName(otherLinkPin.ClassType);
                if (type == null)
                    return false;
                if (type == mOrigionType)
                    return true;
                if (type.IsSubclassOf(mOrigionType))
                    return true;

                return false;
            }

            return true;
        }

        public override BaseNodeControl Duplicate(DuplicateParam param)
        {
            var copyedNode = base.Duplicate(param) as CaseControl;
            copyedNode.CaseValueStr = CaseValueStr;
            copyedNode.OrigionType = OrigionType;
            return copyedNode;
        }
    }
}
