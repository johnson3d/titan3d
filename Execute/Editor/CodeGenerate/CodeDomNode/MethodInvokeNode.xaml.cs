using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using CodeGenerateSystem.Base;
using CodeGenerateSystem.Controls;

namespace CodeDomNode
{
    public partial class MethodInvokeNode : CodeGenerateSystem.Base.IDebugableNode
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

        Visibility mAsyncIconVisibility = Visibility.Collapsed;
        public Visibility AsyncIconVisibility
        {
            get => mAsyncIconVisibility;
            set
            {
                mAsyncIconVisibility = value;
                OnPropertyChanged("AsyncIconVisibility");
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
            return (MethodLink_Pre.Visibility == Visibility.Visible);
        }

        public Visibility TargetPinVisible
        {
            get { return (Visibility)GetValue(TargetPinVisibleProperty); }
            set { SetValue(TargetPinVisibleProperty, value); }
        }
        public static readonly DependencyProperty TargetPinVisibleProperty = DependencyProperty.Register("TargetPinVisible", typeof(Visibility), typeof(MethodInvokeNode), new FrameworkPropertyMetadata(Visibility.Visible));

        partial void InitConstruction()
        {
            this.InitializeComponent();
            mChildNodeContainer = stackPanel_InputParams;

            mCtrlMethodLink_Target = MethodLink_Target;
            mCtrlMethodLink_Pre = MethodLink_Pre;
            mCtrlMethodLink_Next = MethodLink_Next;
            mCtrlreturnLink = returnLink;
            mInputParamsPanel = stackPanel_InputParams;
            mOutputParamsPanel = stackPanel_OutputParams;

            var param = CSParam as MethodNodeConstructionParams;
            switch(param.MethodInfo.HostType)
            {
                //case MethodInfoAssist.enHostType.Instance:
                case MethodInfoAssist.enHostType.Static:
                    TargetPinVisible = Visibility.Collapsed;
                    break;
                case MethodInfoAssist.enHostType.This:
                case MethodInfoAssist.enHostType.Base:
                    TargetThisFlag.Visibility = Visibility.Visible;
                    break;
                default:
                    TargetThisFlag.Visibility = Visibility.Collapsed;
                    break;
            }

            if ((param.MethodInfo.ReturnType.BaseType == typeof(System.Threading.Tasks.Task)))
            {
                AsyncIconVisibility = Visibility.Visible;
            }
            SetBinding(ToolTipProperty, new Binding("ClassInstanceName") { Source = this });
            SetUpLinkElement(MethodLink_Pre);
        }
        protected override void CollectionErrorMsg()
        {
            var param = CSParam as MethodNodeConstructionParams;

            if (param.MethodInfo.ParentClassType == null)
            {
                HasError = true;
                ErrorDescription = "找不到该函数所属类型";
                return;
            }
            if(param.MethodInfo.ReturnType == null)
            {
                HasError = true;
                ErrorDescription = "找不到该函数的返回值类型";
                return;
            }
            Type[] paramTypes = new Type[param.MethodInfo.Params.Count];
            for (int i = 0; i < paramTypes.Length; i++)
            {
                switch(param.MethodInfo.Params[i].FieldDirection)
                {
                    case FieldDirection.In:
                        if (param.MethodInfo.Params[i].IsParamsArray)
                            throw new InvalidOperationException("未实现");
                        else
                            paramTypes[i] = param.MethodInfo.Params[i].ParameterType;
                        break;
                    case FieldDirection.Out:
                    case FieldDirection.Ref:
                        if (param.MethodInfo.Params[i].IsParamsArray)
                            throw new InvalidOperationException("未实现");
                        else
                            paramTypes[i] = param.MethodInfo.Params[i].ParameterType.MakeByRefType();
                        break;
                }
                if (paramTypes[i] == null)
                {
                    HasError = true;
                    ErrorDescription = $"找不到参数{param.MethodInfo.Params[i].ParamName}的类型";
                    return;
                }
            }
            var method = param.MethodInfo.ParentClassType.GetMethod(param.MethodInfo.MethodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null, paramTypes, null);
            if(method == null)
            {
                HasError = true;
                ErrorDescription = $"在类型{EngineNS.Rtti.RttiHelper.GetAppTypeString(param.MethodInfo.ParentClassType)}找不到符合当前参数的函数{param.MethodInfo.MethodName}";
                return;
            }
            switch (param.MethodInfo.HostType)
            {
                case MethodInfoAssist.enHostType.Target:
                    if (!mCtrlMethodLink_Target.HasLink)
                    {
                        HasError = true;
                        ErrorDescription = "Target未链接";
                    }
                    break;
            }

            foreach(var child in mChildNodes)
            {
                if(!child.CheckError())
                {
                    HasError = true;
                    ErrorDescription = child.ErrorDescription;
                    break;
                }
            }
        }
        partial void SetErrorShowToolTip(string toolTip)
        {
            ErrorDescription = toolTip;
        }
        partial void SetReturn_WPF(Type returnType)
        {
            if (returnType == typeof(void))
            {
                returnLink.Visibility = System.Windows.Visibility.Collapsed;
            }
            else if(returnType != null)
            {
                if(returnType.BaseType == typeof(System.Threading.Tasks.Task))
                {
                    var retType = returnType.GetGenericArguments()[0];
                    returnLink.NameString = retType.Name;
                }
                else
                    returnLink.NameString = returnType.Name;
            }
        }

        void OnParamLinkAddLinkInfo_WPF(CodeGenerateSystem.Base.LinkInfo info)
        {
            TargetThisFlag.Visibility = Visibility.Collapsed;
        }
        void OnParamLinkDelLinkInfo_WPF(CodeGenerateSystem.Base.LinkInfo info)
        {
            TargetThisFlag.Visibility = Visibility.Visible;
        }
        //partial void InitializeLinkLine()
        //{
        //    if (ParentDrawCanvas == null)
        //        return;

        //    BindingOperations.ClearBinding(this.mParentLinkPath, Path.VisibilityProperty);
        //    BindingOperations.SetBinding(this.mParentLinkPath, Path.VisibilityProperty, new Binding("Visibility") { Source = this });
        //    mParentLinkPath.Stroke = Brushes.LightGray;
        //    mParentLinkPath.StrokeDashArray = new DoubleCollection(new double[] { 2, 4 });
        //    //m_ParentLinkPath.StrokeThickness = 3;
        //    mParentLinkPathFig.Segments.Add(mParentLinkBezierSeg);
        //    PathFigureCollection pfc = new PathFigureCollection();
        //    pfc.Add(mParentLinkPathFig);
        //    PathGeometry pg = new PathGeometry();
        //    pg.Figures = pfc;
        //    mParentLinkPath.Data = pg;
        //    ParentDrawCanvas.Children.Add(mParentLinkPath);
        //}

        //public override void UpdateLink()
        //{
        //    base.UpdateLink();

        //    if (mHostUsefulMemberData == null || mHostUsefulMemberData.LinkObject == null)
        //        return;

        //    mParentLinkPathFig.StartPoint = mHostUsefulMemberData.LinkObject.LinkElement.TranslatePoint(mHostUsefulMemberData.LinkObject.LinkElementOffset, ParentDrawCanvas);

        //    // 如果这个节点隐藏，就获取打包它的节点的坐标。
        //    mParentLinkBezierSeg.Point3 = GetPositionInContainer();

        //    double delta = System.Math.Max(System.Math.Abs(mParentLinkBezierSeg.Point3.X - mParentLinkPathFig.StartPoint.X) / 2, 25);
        //    delta = System.Math.Min(150, delta);

        //    switch (mHostUsefulMemberData.LinkObject.BezierType)
        //    {
        //        case CodeGenerateSystem.Base.enBezierType.Left:
        //            mParentLinkBezierSeg.Point1 = new System.Windows.Point(mParentLinkPathFig.StartPoint.X - delta, mParentLinkPathFig.StartPoint.Y);
        //            break;
        //        case CodeGenerateSystem.Base.enBezierType.Right:
        //            mParentLinkBezierSeg.Point1 = new System.Windows.Point(mParentLinkPathFig.StartPoint.X + delta, mParentLinkPathFig.StartPoint.Y);
        //            break;
        //        case CodeGenerateSystem.Base.enBezierType.Top:
        //            mParentLinkBezierSeg.Point1 = new System.Windows.Point(mParentLinkPathFig.StartPoint.X, mParentLinkPathFig.StartPoint.Y - delta);
        //            break;
        //        case CodeGenerateSystem.Base.enBezierType.Bottom:
        //            mParentLinkBezierSeg.Point1 = new System.Windows.Point(mParentLinkPathFig.StartPoint.X, mParentLinkPathFig.StartPoint.Y + delta);
        //            break;
        //    }

        //    mParentLinkBezierSeg.Point2 = new System.Windows.Point(mParentLinkBezierSeg.Point3.X, mParentLinkBezierSeg.Point3.Y - delta);
        //}

        public override object GetShowPropertyObject()
        {
            return mTemplateClassInstance;
        }

        class ParamTypeLinkData
        {
            public MethodInvokeParameterControl TagCtrl;
        }
        Dictionary<string, List<ParamTypeLinkData>> mParamTypeLinkDic = new Dictionary<string, List<ParamTypeLinkData>>();
        partial void BuildParamTypeLinkDic()
        {
            mParamTypeLinkDic.Clear();

            var param = CSParam as MethodInvokeNode.MethodNodeConstructionParams;
            if(param.MethodInfo.ReturnTypeLinkIndex >= 0 && param.MethodInfo.ReturnTypeLinkIndex < param.MethodInfo.Params.Count)
            {
                var data = new ParamTypeLinkData();
                var paramInfo = param.MethodInfo.Params[param.MethodInfo.ReturnTypeLinkIndex];
                var list = new List<ParamTypeLinkData>();
                list.Add(data);
                mParamTypeLinkDic[paramInfo.ParamName] = list;
            }

            foreach (var child in mChildNodes)
            {
                var methodParamNode = child as MethodInvokeParameterControl;
                if (methodParamNode == null)
                    continue;
                var methodParamNodeParam = methodParamNode.CSParam as MethodInvokeParameterControl.MethodInvokeParameterConstructionParams;

                if (methodParamNodeParam.ParamInfo.TypeLinkIndex < 0 || methodParamNodeParam.ParamInfo.TypeLinkIndex >= param.MethodInfo.Params.Count)
                    continue;
                var linkEdParam = param.MethodInfo.Params[methodParamNodeParam.ParamInfo.TypeLinkIndex];

                // 只有type类型的参数才能做type改变链接
                if (linkEdParam.ParameterType != typeof(Type))
                    continue;
                var data = new ParamTypeLinkData();
                data.TagCtrl = methodParamNode;
                List<ParamTypeLinkData> list;
                if (!mParamTypeLinkDic.TryGetValue(linkEdParam.ParamName, out list))
                {
                    list = new List<ParamTypeLinkData>();
                    mParamTypeLinkDic[linkEdParam.ParamName] = list;
                }
                list.Add(data);
            }
        }
        partial void InitTemplateClass_WPF(List<CodeGenerateSystem.Base.CustomPropertyInfo> propertys)
        {
            if(propertys != null)
            {
                foreach (var pro in propertys)
                {
                    var property = mTemplateClassInstance.GetType().GetProperty(pro.PropertyName);
                    if (property == null)
                        continue;
                    property.SetValue(mTemplateClassInstance, pro.CurrentValue);
                }
            }

            if (mTemplateClassInstance != null)
            {
                var classType = mTemplateClassInstance.GetType();
                foreach (var childNode in mChildNodes)
                {
                    var metCtrl = childNode as MethodInvokeParameterControl;
                    if (metCtrl == null || metCtrl.IsConstructOut)
                        continue;

                    var ctrlParam = metCtrl.CSParam as MethodInvokeParameterControl.MethodInvokeParameterConstructionParams;
                    BindingOperations.ClearBinding(metCtrl.DefaultValueTextBlock, TextBlock.TextProperty);
                    if(classType.GetProperty(ctrlParam.ParamInfo.ParamName) != null)
                        metCtrl.DefaultValueTextBlock.SetBinding(TextBlock.TextProperty, new Binding(ctrlParam.ParamInfo.ParamName) { Source = mTemplateClassInstance });
                }

                mTemplateClassInstance.OnPropertyChangedAction = (propertyName, newVal, oldVal) =>
                {
                    mParamDeclarationStatementsDic.Clear();
                    mParamDeclarationInitStatementsDic.Clear();

                    if(newVal is Type)
                    {
                        var newType = (Type)newVal;
                        List<ParamTypeLinkData> datas;
                        if (mParamTypeLinkDic.TryGetValue(propertyName, out datas))
                        {
                            foreach(var data in datas)
                            {
                                if (data.TagCtrl == null)
                                {
                                    var param = CSParam as MethodInvokeNode.MethodNodeConstructionParams;
                                    if (param.MethodInfo.ReturnType.BaseType == typeof(System.Threading.Tasks.Task))
                                    {
                                        param.MethodInfo.ReturnDisplayType = typeof(System.Threading.Tasks.Task<>).MakeGenericType(new Type[] { newType });
                                    }
                                    else
                                        param.MethodInfo.ReturnDisplayType = newType;

                                    mReturnType = param.MethodInfo.ReturnDisplayType;
                                    SetReturn_WPF(mReturnType);

                                    CollectLinkPinInfo(param, "CtrlreturnLink", newType, enBezierType.Right, enLinkOpType.Start, true);
                                    var pinDesc = GetLinkPinDesc(param, "CtrlreturnLink");
                                    mCtrlreturnLink.LinkType = pinDesc.PinType;
                                    mCtrlreturnLink.ClassType = pinDesc.ClassType;
                                }
                                else
                                {
                                    var param = data.TagCtrl.CSParam as MethodInvokeParameterControl.MethodInvokeParameterConstructionParams;
                                    data.TagCtrl.UpdateParamType(newType, true);
                                }
                            }
                        }
                    }
                };
            }
        }

        public override BaseNodeControl Duplicate(DuplicateParam param)
        {
            var copyedNode = base.Duplicate(param) as MethodInvokeNode;
            CodeGenerateSystem.Base.PropertyClassGenerator.CloneClassInstanceProperties(mTemplateClassInstance, copyedNode.mTemplateClassInstance);
            return copyedNode;
        }
    }
    /// <summary>
    /// ////////////////////////////////////////////////////////////////////////////
    /// </summary>

    // 带params标识的参数对象
    public partial class ParamParameterControl
    {
        partial void InitConstruction()
        {
            Resources = new ResourceDictionary()
            {
                Source = new Uri("pack://application:,,,/CodeGenerateSystem;component/Themes/Generic.xaml", UriKind.Absolute),
            };

            StackPanel mainPanel = new StackPanel()
            {
                Margin = new System.Windows.Thickness(0, 5, 0, 0),
            };
            AddChild(mainPanel);

            mParamsPanel = new StackPanel();
            mainPanel.Children.Add(mParamsPanel);

            var button = new Button()
            {
                Content = " 增加参数 ",
                HorizontalAlignment = HorizontalAlignment.Left,
                Style = this.TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "ButtonStyle_Default")) as System.Windows.Style,
                Margin = new System.Windows.Thickness(8, 2, 2, 2),
            };
            button.Click += (sender, e) =>
            {
                // 添加参数操作
                var paramInfo = GetParameterInfoString();
                AddParam(paramInfo, true);
            };
            mainPanel.Children.Add(button);
        }

        partial void SetDeleteButton(MethodInvokeParameterControl childCtrl, CodeGenerateSystem.Base.LinkPinControl paramEllip)
        {
            var childCtrlParamStackPanel = LogicalTreeHelper.FindLogicalNode(childCtrl, "ParamStackPanel") as StackPanel;
            if (childCtrlParamStackPanel != null)
            {
                var button = new Button()
                {
                    Content = new Image()
                    {
                        Source = new System.Windows.Media.Imaging.BitmapImage(new System.Uri("pack://application:,,,/ResourceLibrary;component/Icon/del.png", UriKind.Absolute)),
                    },
                    Style = this.TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "ButtonStyle_Default")) as System.Windows.Style,
                    Margin = new System.Windows.Thickness(8, 3, 2, 3),
                    Width = 16,
                    Height = 16,
                    ToolTip = "删除当前参数",
                };
                childCtrlParamStackPanel.Children.Add(button);
                button.Click += (sender, e) =>
                {
                    // 删除参数操作
                    DelChildNode(childCtrl);
                    mParamEllipses.Remove(paramEllip);

                    // 内部对象重新命名
                    int i = 1;
                    foreach (MethodInvokeParameterControl child in mParamsPanel.Children)
                    {
                        var linkCtrl = LogicalTreeHelper.FindLogicalNode(child, "ParamPin") as LinkPinControl;
                        var newName = mParamName + i;
                        var param = child.CSParam as MethodInvokeParameterControl.MethodInvokeParameterConstructionParams;
                        if (param.ParamInfo.ParamName != newName)
                        {
                            child.CSParam.ConstructParam = child.CSParam.ConstructParam.Replace(param.ParamInfo.ParamName, newName);
                            param.ParamInfo.ParamName = newName;
                            linkCtrl.NameString = param.ParamInfo.ParamName;
                        }
                        i++;
                    }

                    RecreateMethodInvokeTemplateClass();
                };
            }
        }

        

        partial void RecreateMethodInvokeTemplateClass()
        {
            if (ParentNode != null)
            {
                var pNode = ParentNode as MethodInvokeNode;
                if (pNode != null)
                {
                    var cpInfos = new List<CodeGenerateSystem.Base.CustomPropertyInfo>(pNode.StandardParamPropertyInfos);
                    cpInfos.AddRange(GetCPInfos());
                    pNode.CreateTemplateClass(cpInfos);
                }
            }
        }
    }

    //public partial class DelegateParameterControl
    //{
    //    StackPanel mMainPanel;

    //    partial void InitConstruction()
    //    {
    //        Resources = new ResourceDictionary()
    //        {
    //            Source = new Uri("pack://application:,,,/CodeGenerateSystem;component/Themes/Generic.xaml", UriKind.Absolute),
    //        };

    //        HorizontalAlignment = HorizontalAlignment.Stretch;

    //        var param = CSParam as DelegateParameterConstructionParams;
    //        switch (param.ConstructType)
    //        {
    //            case MethodInvokeNode.enParamConstructType.MethodInvoke:
    //                break;
    //            default:
    //                throw new InvalidOperationException();
    //        }

    //        Border bd = new Border()
    //        {
    //            BorderThickness = new System.Windows.Thickness(0,1,0,1),
    //            BorderBrush = Brushes.Gray,
    //            HorizontalAlignment = HorizontalAlignment.Stretch,
    //        };
    //        AddChild(bd);

    //        mMainPanel = new StackPanel()
    //        {
    //            HorizontalAlignment = HorizontalAlignment.Right,
    //        };
    //        //Grid grid = new Grid();
    //        bd.Child = mMainPanel;
            
    //        var stackPanel = new StackPanel()
    //        {
    //            Orientation = Orientation.Horizontal,
    //            HorizontalAlignment = HorizontalAlignment.Right,
    //            Margin = new System.Windows.Thickness(0),
    //        };

    //        // 代理函数体
    //        TextBlock textBlockType = new TextBlock()
    //        {
    //            Text = mParamFlag + (string.IsNullOrEmpty(mParamFlag) ? "" : " ") + mParamType.Name,
    //            Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(160, 160, 160)),
    //            Margin = new System.Windows.Thickness(2),
    //            VerticalAlignment = VerticalAlignment.Center,
    //        };
    //        stackPanel.Children.Add(textBlockType);
    //        mParamSquare = new CodeGenerateSystem.Controls.LinkOutControl()
    //        {
    //            Margin = new System.Windows.Thickness(8),
    //            NameString = mParamName,
    //            BackBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0x82, 0x82, 0xD8)),
    //            HorizontalAlignment = HorizontalAlignment.Right,
    //            Direction = CodeGenerateSystem.Base.enBezierType.Right,
    //            PinType = LinkControl.enPinType.Exec,
    //        };
    //        stackPanel.Children.Add(mParamSquare);

    //        mMainPanel.Children.Add(stackPanel);

    //        //             var method = mParamType.GetMethod("Invoke");
    //        //             if (method == null)
    //        //                 return;            
    //        //             foreach (var p in method.GetParameters())
    //        //             {                
    //        //                 var par = p.Name + ":" + EngineNS.Rtti.RttiHelper.GetTypeSaveString(p.ParameterType);
    //        //                 var pc = new ParameterControl(ParentDrawCanvas, par);
    //        //                 pc.ParamEllipse.BackBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(252, 212, 11));
    //        //                 pc.ParamEllipse.Direction = CodeGenerateSystem.Base.enBezierType.Left;                
    //        //                 AddChildNode(pc, panel);
    //        //             }
    //    }

    //    partial void InitMethodParam(MethodParamInfoAssist meParam)
    //    {
    //        var stackPanel_Param = new StackPanel()
    //        {
    //            Orientation = Orientation.Horizontal,
    //            HorizontalAlignment = HorizontalAlignment.Right,
    //            Margin = new System.Windows.Thickness(2),
    //        };

    //        var strFlag = "";
    //        if (meParam.IsOut)
    //        {
    //            strFlag = ":Out";
    //        }
    //        else if (meParam.ParameterType.IsByRef)
    //        {
    //            strFlag = ":Ref";
    //        }

    //        // 代理函数体
    //        TextBlock textBlockType_Param = new TextBlock()
    //        {
    //            Text = meParam.FieldDirection + " " + EngineNS.Rtti.RttiHelper.GetAppTypeString(meParam.ParameterType),
    //            Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(160, 160, 160)),
    //            Margin = new System.Windows.Thickness(2)
    //        };
    //        stackPanel_Param.Children.Add(textBlockType_Param);
    //        TextBlock label_Param = new TextBlock()
    //        {
    //            Text = meParam.ParamName,
    //            Foreground = Brushes.White,
    //            Margin = new System.Windows.Thickness(2)
    //        };
    //        stackPanel_Param.Children.Add(label_Param);
    //        var squre = new CodeGenerateSystem.Controls.LinkOutControl()
    //        {
    //            Margin = new System.Windows.Thickness(0, 0, -23, 0),
    //            Width = 13,
    //            Height = 13,
    //            BackBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0xF3, 0x92, 0xF3)),
    //            HorizontalAlignment = HorizontalAlignment.Right,
    //            Direction = CodeGenerateSystem.Base.enBezierType.Right,
    //        };
    //        stackPanel_Param.Children.Add(squre);
    //        mMainPanel.Children.Add(stackPanel_Param);
    //    }

    //    //public List<CodeGenerateSystem.Base.UsefulMemberHostData> GetUsefulMembers()
    //    //{
    //    //    var retValue = new List<CodeGenerateSystem.Base.UsefulMemberHostData>();

    //    //    if (mMethodParamDatas != null)
    //    //    {
    //    //        foreach(var data in mMethodParamDatas)
    //    //        {
    //    //            var memberData = new CodeGenerateSystem.Base.UsefulMemberHostData()
    //    //            {
    //    //                ClassTypeFullName = data.ParamType.FullName,
    //    //                HostControl = this,
    //    //                LinkObject = data.LinkObjectInfo,
    //    //                MemberHostName = data.ParamName,
    //    //            };
    //    //            retValue.Add(memberData);
    //    //        }
    //    //    }
    //    //    return retValue;
    //    //}
    //    //public List<CodeGenerateSystem.Base.UsefulMemberHostData> GetUsefulMembers(CodeGenerateSystem.Base.LinkControl linkCtrl)
    //    //{
    //    //    var retValue = new List<CodeGenerateSystem.Base.UsefulMemberHostData>();

    //    //    if (mMethodParamDatas != null)
    //    //    {
    //    //        foreach (var data in mMethodParamDatas)
    //    //        {
    //    //            if(data.Control == linkCtrl)
    //    //            {
    //    //                var memberData = new CodeGenerateSystem.Base.UsefulMemberHostData()
    //    //                {
    //    //                    ClassTypeFullName = data.ParamType.FullName,
    //    //                    HostControl = this,
    //    //                    LinkObject = data.LinkObjectInfo,
    //    //                    MemberHostName = data.ParamName,
    //    //                };
    //    //                retValue.Add(memberData);
    //    //            }
    //    //        }
    //    //    }
    //    //    return retValue;
    //    //}
    //}

}
