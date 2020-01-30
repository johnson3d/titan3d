using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CodeDomNode
{
    public partial class MethodInvokeParameterControl
    {
        System.Windows.Controls.StackPanel mParamPanel;
        public System.Windows.Controls.StackPanel ParamPanel
        {
            get { return mParamPanel; }
        }
        TextBlock mDefaultValueTextBlock;
        public TextBlock DefaultValueTextBlock
        {
            get { return mDefaultValueTextBlock; }
        }

        public bool IsConstructOut
        {
            get
            {
                var param = CSParam as MethodInvokeParameterConstructionParams;
                switch (param.ConstructType)
                {
                    case MethodInvokeNode.enParamConstructType.MethodOverride:
                    case MethodInvokeNode.enParamConstructType.MethodCustom:
                    case MethodInvokeNode.enParamConstructType.MethodCustomInvoke_Out:
                    case MethodInvokeNode.enParamConstructType.MethodInvoke_Out:
                        return true;
                    default:
                        return false;
                }
            }
        }

        TextBlock mTextBlockType;
        string mGenericTypeName;
        Button mRemoveButton;
        public Button RemoveButton
        {
            get => mRemoveButton;
        }

        partial void InitConstruction()
        {
            Resources = new ResourceDictionary()
            {
                Source = new Uri("pack://application:,,,/CodeGenerateSystem;component/Themes/Generic.xaml", UriKind.Absolute),
            };

            var param = CSParam as MethodInvokeParameterConstructionParams;

            Grid grid = new Grid()
            {
                VerticalAlignment = VerticalAlignment.Center,
            };
            AddChild(grid);

            if (mIsGenericType)
            {
                if (ParamType.BaseType == typeof(System.Threading.Tasks.Task))
                {
                    mGenericTypeName = EngineNS.Rtti.RttiHelper.GetAppTypeString(ParamType.GetGenericArguments()[0]);
                }
                else
                    mGenericTypeName = ParamType.Name;
                //throw new InvalidOperationException("没实现");
            }

            var paramType = param.ParamInfo.ParameterType;
            if (param.ParamInfo.ParameterDisplayType != null)
                paramType = param.ParamInfo.ParameterDisplayType;
            switch (param.ConstructType)
            {
                case MethodInvokeNode.enParamConstructType.MethodInvoke:
                case MethodInvokeNode.enParamConstructType.Return:
                case MethodInvokeNode.enParamConstructType.ReturnCustom:
                case MethodInvokeNode.enParamConstructType.Delegate:
                case MethodInvokeNode.enParamConstructType.MethodCustomInvoke_In:
                    {
                        grid.HorizontalAlignment = HorizontalAlignment.Left;
                        //grid.Margin = new Thickness(0, 0, 0, 8);
                        grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Auto) });
                        grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

                        mParamPin = new CodeGenerateSystem.Controls.LinkInControl()
                        {
                            Name = "ParamPin",
                            Margin = new System.Windows.Thickness(8, 2, 8, 2),
                            //BackBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(243, 146, 243)),
                            HorizontalAlignment = HorizontalAlignment.Center,
                            Direction = CodeGenerateSystem.Base.enBezierType.Left,
                            //NameString = mParamName,
                        };
                        mParamPin.SetBinding(CodeGenerateSystem.Controls.LinkInControl.NameStringProperty, new Binding("UIDisplayParamName") { Source = param.ParamInfo });
                        if (paramType.GetInterface(typeof(System.Collections.IEnumerable).FullName) != null &&
                            paramType != typeof(string))
                            mParamPin.PinType = CodeGenerateSystem.Base.LinkPinControl.enPinType.Array;
                        grid.Children.Add(mParamPin);

                        mParamPanel = new StackPanel()
                        {
                            Orientation = Orientation.Horizontal,
                            Name = "ParamStackPanel",
                            VerticalAlignment = VerticalAlignment.Center,
                        };
                        Grid.SetColumn(mParamPanel, 1);
                        grid.Children.Add(mParamPanel);
                        mTextBlockType = new TextBlock()
                        {
                            Text = (mIsGenericType ? mGenericTypeName : paramType.Name) + " " + mParamFlag.ToString(),
                            Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(160, 160, 160)),//TryFindResource("DefaultValueBrush") as Brush
                            Margin = new System.Windows.Thickness(2),
                            VerticalAlignment = VerticalAlignment.Center,
                        };
                        mParamPanel.Children.Add(mTextBlockType);

                        mDefaultValueTextBlock = new TextBlock()
                        {
                            Name = "DefaultValue",
                            Foreground = Brushes.GreenYellow,
                            Margin = new System.Windows.Thickness(2, 3, 2, 3),
                        };
                        mParamPanel.Children.Add(mDefaultValueTextBlock);
                        if (param.ConstructType == MethodInvokeNode.enParamConstructType.Delegate)
                        {
                            mRemoveButton = new Button()
                            {
                                Content = new Image()
                                {
                                    Source = new BitmapImage(new System.Uri("pack://application:,,,/ResourceLibrary;component/Icons/Icons/icon_Edit_Delete_40x.png", UriKind.Absolute)),
                                },
                                Style = TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "ButtonStyle_Default")) as Style,
                                Margin = new Thickness(3),
                                Width = 16,
                                Height = 16,
                                ToolTip = "删除",
                            };
                            mParamPanel.Children.Add(mRemoveButton);

                            mDefaultValueTextBlock.Visibility = Visibility.Hidden;
                        }
                    }
                    break;
                case MethodInvokeNode.enParamConstructType.MethodOverride:
                case MethodInvokeNode.enParamConstructType.MethodCustom:
                case MethodInvokeNode.enParamConstructType.MethodCustomInvoke_Out:
                case MethodInvokeNode.enParamConstructType.MethodInvoke_Out:
                    {
                        grid.HorizontalAlignment = HorizontalAlignment.Right;
                        grid.Margin = new Thickness(8, 0, 0, 0);
                        grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Auto) });
                        grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

                        mParamPanel = new StackPanel()
                        {
                            Orientation = Orientation.Horizontal,
                            Name = "ParamStackPanel",
                            VerticalAlignment = VerticalAlignment.Center,
                        };
                        grid.Children.Add(mParamPanel);
                        mTextBlockType = new TextBlock()
                        {
                            Text = mParamFlag.ToString() + " " + (mIsGenericType ? mGenericTypeName : paramType.Name),
                            Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(160, 160, 160)),
                            Margin = new System.Windows.Thickness(2)
                        };
                        mParamPanel.Children.Add(mTextBlockType);

                        mParamPin = new CodeGenerateSystem.Controls.LinkOutControl()
                        {
                            Name = "ParamPin",
                            Margin = new System.Windows.Thickness(8, 2, 8, 2),
                            //BackBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(243, 146, 243)),
                            HorizontalAlignment = HorizontalAlignment.Center,
                            Direction = CodeGenerateSystem.Base.enBezierType.Right,
                            //NameString = mParamName,
                        };
                        mParamPin.SetBinding(CodeGenerateSystem.Controls.LinkInControl.NameStringProperty, new Binding("UIDisplayParamName") { Source = param.ParamInfo });
                        Grid.SetColumn(mParamPin, 1);
                        grid.Children.Add(mParamPin);
                    }
                    break;
                default:
                    throw new InvalidOperationException();
            }

        }

        partial void ResetParamInfo_WPF(CustomMethodInfo.FunctionParam param, System.CodeDom.FieldDirection dir)
        {
            mParamPin.SetBinding(CodeGenerateSystem.Controls.LinkInControl.NameStringProperty, new Binding("ParamName") { Source = param });
        }

        partial void UpdateParamType_WPF(Type newType)
        {
            var param = CSParam as MethodInvokeParameterConstructionParams;
            switch (param.ConstructType)
            {
                case MethodInvokeNode.enParamConstructType.MethodInvoke:
                case MethodInvokeNode.enParamConstructType.Return:
                case MethodInvokeNode.enParamConstructType.ReturnCustom:
                case MethodInvokeNode.enParamConstructType.MethodCustomInvoke_In:
                    {
                        mTextBlockType.Text = (mIsGenericType ? mGenericTypeName : param.ParamInfo.ParameterType.Name) + " " + mParamFlag.ToString();
                    }
                    break;
                case MethodInvokeNode.enParamConstructType.MethodCustom:
                case MethodInvokeNode.enParamConstructType.MethodOverride:
                case MethodInvokeNode.enParamConstructType.MethodCustomInvoke_Out:
                case MethodInvokeNode.enParamConstructType.MethodInvoke_Out:
                    {
                        if (param.ParamInfo.ParameterDisplayType != null)
                            mTextBlockType.Text = mParamFlag.ToString() + " " + (mIsGenericType ? mGenericTypeName : param.ParamInfo.ParameterDisplayType.Name);
                        else
                            mTextBlockType.Text = mParamFlag.ToString() + " " + (mIsGenericType ? mGenericTypeName : param.ParamInfo.ParameterType.Name);
                    }
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }

        partial void OnParamLinkAddLinkInfo_WPF(CodeGenerateSystem.Base.LinkInfo info)
        {
            var param = CSParam as MethodInvokeParameterConstructionParams;
            switch (param.ConstructType)
            {
                case MethodInvokeNode.enParamConstructType.Delegate:
                    {
                        mTextBlockType.Text = (mIsGenericType ? mGenericTypeName : param.ParamInfo.ParameterType.Name) + " " + mParamFlag.ToString();

                        var delegateNode = ParentNode as MethodInvoke_DelegateControl;
                        var noUse = delegateNode.AddGraphInParam(this, info);
                    }
                    break;
                case MethodInvokeNode.enParamConstructType.ReturnCustom:
                    {
                        if (mDefaultValueTextBlock != null)
                        {
                            mDefaultValueTextBlock.Visibility = Visibility.Visible;
                        }
                        //var retCtrl = ParentNode as ReturnCustom;
                        //if (param.ParamInfo.ParameterType.IsGenericParameter)
                        //{
                        //    // XXX
                        //}
                    }
                    break;
                default:
                    if (mDefaultValueTextBlock != null)
                    {
                        mDefaultValueTextBlock.Visibility = Visibility.Hidden;
                    }
                    break;
            }
        }
        partial void OnParamLinkDelLinkInfo_WPF(CodeGenerateSystem.Base.LinkInfo info)
        {
            var param = CSParam as MethodInvokeParameterConstructionParams;
            switch (param.ConstructType)
            {
                case MethodInvokeNode.enParamConstructType.Delegate:
                    {
                        mTextBlockType.Text = (mIsGenericType ? mGenericTypeName : param.ParamInfo.ParameterType.Name) + " " + mParamFlag.ToString();

                        var delegateNode = ParentNode as MethodInvoke_DelegateControl;
                        var noUse = delegateNode.RemoveGraphInParam(this);
                    }
                    break;
                default:
                    if (mDefaultValueTextBlock != null)
                    {
                        mDefaultValueTextBlock.Visibility = Visibility.Visible;
                    }
                    break;
            }
        }

        //public bool IsParamLinkRight(ref string strMsg)
        //{
        //    //EngineNS.ECSType csType = EngineNS.ECSType.All;
        //    //if (HostNodesContainer != null)
        //    //    csType = HostNodesContainer.mCSType;
        //    var linkOI = GetLinkPinInfo(mParamPin);
        //    if (linkOI.HasLink)
        //    {
        //        Type parType = EngineNS.Rtti.RttiHelper.GetTypeFromSaveString(linkOI.GetLinkObject(0, true).GCode_GetValueType(linkOI.GetLinkElement(0, true)));
        //        if (parType != ParamType)
        //        {
        //            strMsg = "函数参数类型与连接的类型不匹配\r\n函数参数类型：" + ParamType.ToString() + "\r\n连接参数类型" + parType.ToString();
        //            return false;
        //        }
        //    }
        //    else
        //    {
        //        var param = CSParam as MethodInvokeParameterConstructionParams;
        //        strMsg = "函数没有设置参数 " + param.ParamInfo.ParamName;
        //        return false;
        //    }

        //    return true;
        //}

        //public override void Save(CSUtility.Support.XmlNode xmlNode)
        //{
        //    xmlNode.AddAttrib("Params", m_methodInfoToSave);

        //    base.Save(xmlNode);
        //}

        public override string GetNodeDescriptionString()
        {
            var param = CSParam as MethodInvokeParameterConstructionParams;
            return ParentNode.NodeName + "." + param.ParamInfo.ParamName;
        }

        public override void RefreshFromLink(CodeGenerateSystem.Base.LinkPinControl pin, int linkIndex)
        {
            var param = CSParam as MethodInvokeParameterConstructionParams;
            if (param.ParamInfo.ParameterType == typeof(object) && param.ParamInfo.ParameterDisplayType == null)
            {
                var type = pin.GetLinkedObject(0, true).GCode_GetType(pin.GetLinkedPinControl(0, true), null);
                if (type != typeof(object))
                {
                    param.ParamInfo.ParameterType = type;
                    // XXX
                }
            }
        }
    }

}
