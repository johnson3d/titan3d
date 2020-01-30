using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using CodeGenerateSystem.Base;
using CodeGenerateSystem.Controls;
using CSUtility.Support;

namespace CodeDomNode
{
    public partial class MethodInvokeUnionNode : CodeGenerateSystem.Base.UsefulMember, CodeGenerateSystem.Base.IDebugableNode
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
                if (mBreaked)
                {
                    Border_Main.BorderBrush = Brushes.Red;
                }
                else
                {
                    Border_Main.BorderBrush = null;
                }

                ChangeParentLogicLinkLine(mBreaked);
            }
        }

        partial void ShowMethodLost_WPF(bool show)
        {
            if (show)
            {
                Border_Main.BorderBrush = Brushes.Red;
                ErrorShow.Visibility = Visibility.Visible;
            }
            else
            {
                Border_Main.BorderBrush = null;
                ErrorShow.Visibility = Visibility.Collapsed;
            }
        }

        public void ChangeParentLogicLinkLine(bool change)
        {
            //var linkObj = GetLinkObjInfo(MethodLink_Pre);
            //ChangeLinkLine(change, linkObj,
            //    CodeGenerateSystem.Program.DebugLineDashArray,
            //    CodeGenerateSystem.Program.DebugLineThickness,
            //    CodeGenerateSystem.Program.DebugLineColor);
            //if (change)
            //    this.HostNodesContainer.AddTickNode(this);
            //else
            //    this.HostNodesContainer.RemoveTickNode(this);

            //var obj = linkObj.GetLinkObject(0, true) as CodeGenerateSystem.Base.IDebugableNode;
            //if (obj != null)
            //    obj.ChangeParentLogicLinkLine(change);
        }
        public override void Tick(long elapsedMillisecond)
        {
            //var linkObj = GetLinkObjInfo(MethodLink_Pre);
            //if (linkObj.HasLink)
            //    linkObj.LinkInfos[0].Offset += elapsedMillisecond * CodeGenerateSystem.Program.DebugLineOffsetSpeed;
        }

        public List<CodeGenerateSystem.Base.UsefulMemberHostData> GetUsefulMembers()
        {
            List<CodeGenerateSystem.Base.UsefulMemberHostData> retValue = new List<CodeGenerateSystem.Base.UsefulMemberHostData>();
            foreach (var paramNode in mChildNodes)
            {
                if (paramNode is DelegateParameterControl)
                {
                    retValue.AddRange(((DelegateParameterControl)paramNode).GetUsefulMembers());
                }
            }

            return retValue;
        }

        public List<CodeGenerateSystem.Base.UsefulMemberHostData> GetUsefulMembers(CodeGenerateSystem.Base.LinkControl linkCtrl)
        {
            List<CodeGenerateSystem.Base.UsefulMemberHostData> retValue = new List<CodeGenerateSystem.Base.UsefulMemberHostData>();
            foreach (var paramNode in mChildNodes)
            {
                if (paramNode is DelegateParameterControl)
                {
                    retValue.AddRange(((DelegateParameterControl)paramNode).GetUsefulMembers(linkCtrl));
                }
            }

            return retValue;
        }

        ////////////public static string GetParamFromParamInfo(System.Reflection.ParameterInfo pInfo)
        ////////////{
        ////////////    var parameterTypeString = CSUtility.Program.GetTypeSaveString(pInfo.ParameterType);
        ////////////    string strFlag = ":";
        ////////////    if (pInfo.IsOut)
        ////////////    {
        ////////////        strFlag = ":Out";
        ////////////        parameterTypeString = parameterTypeString.Remove(parameterTypeString.Length - 1);
        ////////////    }
        ////////////    else if (pInfo.ParameterType.IsByRef)
        ////////////    {
        ////////////        strFlag = ":Ref";
        ////////////        parameterTypeString = parameterTypeString.Remove(parameterTypeString.Length - 1);
        ////////////    }
        ////////////    else
        ////////////    {
        ////////////        var atts = pInfo.GetCustomAttributes(typeof(System.ParamArrayAttribute), false);
        ////////////        if (atts.Length > 0)
        ////////////        {
        ////////////            strFlag = ":Params";
        ////////////        }
        ////////////    }

        ////////////    return pInfo.Name + ":" + parameterTypeString + strFlag;
        ////////////}
        ////////////public static string GetParamFromMethodInfo(System.Reflection.MethodInfo info, string path)
        ////////////{
        ////////////    string strRet = path + "," + CSUtility.Program.GetTypeSaveString(info.ReflectedType) + "," + info.Name + ",";

        ////////////    System.Reflection.ParameterInfo[] parInfos = info.GetParameters();
        ////////////    if (parInfos.Length > 0)
        ////////////    {
        ////////////        foreach (var pInfo in parInfos)
        ////////////        {
        ////////////            strRet += GetParamFromParamInfo(pInfo) + "/";
        ////////////        }
        ////////////        strRet = strRet.Remove(strRet.Length - 1);   // 去除最后一个"/"
        ////////////    }

        ////////////    strRet += "," + CSUtility.Program.GetTypeSaveString(info.ReturnType);

        ////////////    return strRet;
        ////////////}

        ////////////static Dictionary<CSUtility.Helper.enCSType, Dictionary<string, System.Reflection.MethodInfo>> mMethodInfoWithParamKeyDic = new Dictionary<CSUtility.Helper.enCSType, Dictionary<string, System.Reflection.MethodInfo>>();

        ////////////public static System.Reflection.MethodInfo GetMethodInfoFromParam(string param)
        ////////////{
        ////////////    try
        ////////////    {
        ////////////        if (string.IsNullOrEmpty(param))
        ////////////            return null;


        ////////////        bool isGenericMethodDefinition = false;
        ////////////        var splits = param.Split(',');
        ////////////        CSUtility.Helper.enCSType csType = CSUtility.Helper.enCSType.All;
        ////////////        if (splits.Length > 5)
        ////////////            csType = (CSUtility.Helper.enCSType)CSUtility.Support.IHelper.EnumTryParse(typeof(CSUtility.Helper.enCSType), splits[5]);

        ////////////        Dictionary<string, System.Reflection.MethodInfo> methodDic = null;
        ////////////        if (mMethodInfoWithParamKeyDic.TryGetValue(csType, out methodDic))
        ////////////        {
        ////////////            System.Reflection.MethodInfo retInfo = null;
        ////////////            if (methodDic.TryGetValue(param, out retInfo))
        ////////////                return retInfo;
        ////////////        }
        ////////////        else
        ////////////        {
        ////////////            methodDic = new Dictionary<string, System.Reflection.MethodInfo>();
        ////////////            mMethodInfoWithParamKeyDic[csType] = methodDic;
        ////////////        }

        ////////////        if (splits.Length > 6)
        ////////////            isGenericMethodDefinition = System.Convert.ToBoolean(splits[6]);
        ////////////        var path = splits[0];
        ////////////        var classType = CSUtility.Program.GetTypeFromSaveString(splits[1], csType);
        ////////////        if (classType == null)
        ////////////            return null;
        ////////////        var methodName = splits[2];

        ////////////        if (isGenericMethodDefinition)
        ////////////        {
        ////////////            var methods = classType.GetMethods();
        ////////////            foreach (var method in methods)
        ////////////            {
        ////////////                var methodParam = GetParamFromMethodInfo(method, path) + "," + csType.ToString() + "," + method.IsGenericMethodDefinition;
        ////////////                if (string.Equals(methodParam, param))
        ////////////                {
        ////////////                    methodDic[param] = method;
        ////////////                    return method;
        ////////////                }
        ////////////            }
        ////////////        }
        ////////////        if (!string.IsNullOrEmpty(splits[3]))
        ////////////        {
        ////////////            var paramSplits = splits[3].Split('/');
        ////////////            Type[] paramTypes = new Type[paramSplits.Length];
        ////////////            for (int i = 0; i < paramSplits.Length; i++)
        ////////////            {
        ////////////                var tempSplits = paramSplits[i].Split(':');
        ////////////                if (tempSplits.Length > 2)
        ////////////                {
        ////////////                    switch (tempSplits[2])
        ////////////                    {
        ////////////                        case "Ref":
        ////////////                        case "Out":
        ////////////                            tempSplits[1] += "&";
        ////////////                            break;
        ////////////                    }
        ////////////                }
        ////////////                paramTypes[i] = CSUtility.Program.GetTypeFromSaveString(tempSplits[1], csType);
        ////////////            }

        ////////////            var retValue = classType.GetMethod(methodName, paramTypes);
        ////////////            methodDic[param] = retValue;
        ////////////            return retValue;
        ////////////        }
        ////////////        else
        ////////////        {
        ////////////            var retValue = classType.GetMethod(methodName, new Type[0]);
        ////////////            methodDic[param] = retValue;
        ////////////            return retValue;
        ////////////        }
        ////////////    }
        ////////////    catch (System.Exception e)
        ////////////    {
        ////////////        EditorCommon.MessageReport.Instance.ReportMessage(EditorCommon.MessageReport.enMessageType.Error, "逻辑图：" + e.ToString());
        ////////////    }

        ////////////    return null;
        ////////////}

        partial void InitConstruction()
        {
            this.InitializeComponent();

            SetDragObject(RectangleTitle);
            mNodeContainer = stackPanel_InputParams;
            mInputParamsPanel = stackPanel_InputParams;
            mInputMethodParamsPanel = stackPanel_InputMethodParams;
            mOutputMethodParamsPanel = stackPanel_OutputMethodParams;
            BP.Initialize(this);
        }

        partial void OnDeleteNode_WPF()
        {
            ParentDrawCanvas.Children.Remove(mParentLinkPath);
        }
        partial void ClearChildNode_WPF(CodeGenerateSystem.Base.BaseNodeControl child)
        {
            if (mNodeContainer != null)
                mNodeContainer.Children.Remove(child);
            if (stackPanel_OutputMethodParams != null)
                stackPanel_OutputMethodParams.Children.Remove(child);
            if (stackPanel_InputMethodParams != null)
                stackPanel_InputMethodParams.Children.Remove(child);
        }
        partial void SetErrorShow_WPF(object showValue)
        {
            ErrorShow.ToolTip = showValue;
        }
        //public override string GetLinkObjParamInfo(LinkObjInfo linkObj)
        //{
        //    string returnStr = "";

        //    //if (linkObj.LinkElement == mResultMethod)
        //    //{
        //    //    returnStr += ":" + mParamName + ":" + "@" + mParamType.ToString() + "#" + linkObj.m_linkType.ToString();
        //    //}
        //    //else
        //    //{
        //    //    returnStr += "::";
        //    //}
        //    return returnStr;
        //}

        public override void CollectConstructionErrors()
        {
            foreach (var error in mConstructionErrors)
            {
                AddErrorMsg(CodeGenerateSystem.Controls.ErrorReportControl.ReportType.Error, error);
            }
        }

        private void InitializeLinkLine()
        {
            if (ParentDrawCanvas == null)
                return;

            mHostUsefulMemberData.HostControl?.AddMoveableChildNode(this);

            BindingOperations.ClearBinding(this.mParentLinkPath, Path.VisibilityProperty);
            BindingOperations.SetBinding(this.mParentLinkPath, Path.VisibilityProperty, new Binding("Visibility") { Source = this });
            mParentLinkPath.Stroke = Brushes.LightGray;
            mParentLinkPath.StrokeDashArray = new DoubleCollection(new double[] { 2, 4 });
            //m_ParentLinkPath.StrokeThickness = 3;
            mParentLinkPathFig.Segments.Add(mParentLinkBezierSeg);
            PathFigureCollection pfc = new PathFigureCollection();
            pfc.Add(mParentLinkPathFig);
            PathGeometry pg = new PathGeometry();
            pg.Figures = pfc;
            mParentLinkPath.Data = pg;
            ParentDrawCanvas.Children.Add(mParentLinkPath);
        }

        public override void UpdateLink()
        {
            base.UpdateLink();

            foreach(var childNode in PackageNodeList)
            {
                childNode.UpdateLink();
            }
        }

        public override BaseNodeControl FindChildNode(Guid id)
        {
            var node = base.FindChildNode(id);

            if(node == null)
            {
                foreach (var childNode in PackageNodeList)
                {
                    if (childNode.Id == id)
                        node = childNode;
                }
            }
            return node;
        }

        public override void InitializeUsefulLinkDatas()
        {
            foreach(var childNode in PackageNodeList)
            {
                childNode.InitializeUsefulLinkDatas();
            }
        }

        protected override void CollectionErrorMsg()
        {
            //AddErrorMsg(returnLink, CodeGenerateSystem.Controls.ErrorReportControl.ReportType.Error, "错误测试");
        }

        public override object GetShowPropertyObject()
        {
            return mTemplateClassInstance;
        }

        internal void CreateTemplateClass(List<CodeGenerateSystem.Base.CustomPropertyInfo> propertys)
        {
            var tempClassStore = mTemplateClassInstance;

            var classType = CodeGenerateSystem.Base.PropertyClassGenerator.CreateTypeFromCustomPropertys(propertys);
            mTemplateClassInstance = System.Activator.CreateInstance(classType) as CodeGenerateSystem.Base.GeneratorClassBase;
            var field = mTemplateClassInstance.GetType().GetField("HostNode");
            if (field != null)
                field.SetValue(mTemplateClassInstance, this);
            foreach (var property in mTemplateClassInstance.GetType().GetProperties())
            {
                if (tempClassStore != null)
                {
                    var oldPro = tempClassStore.GetType().GetProperty(property.Name);
                    if (oldPro != null)
                        property.SetValue(mTemplateClassInstance, oldPro.GetValue(tempClassStore));
                    else
                        property.SetValue(mTemplateClassInstance, CodeGenerateSystem.Program.GetDefaultValueFromType(property.PropertyType));
                }
                else
                    property.SetValue(mTemplateClassInstance, CodeGenerateSystem.Program.GetDefaultValueFromType(property.PropertyType));
            }
        }
    }
    ////////////////////////////////////////////////////////////////////////////////////////
    public partial class MethodInvokeParameterUnionControl
    {
        partial void InitConstruction()
        {
            Grid grid = new Grid();
            AddChild(grid);
                        
            mParamEllipse = new CodeGenerateSystem.Controls.LinkInControl()
            {
                Name = "ParamEllipse",
                Margin = new System.Windows.Thickness(-15, 0, 0, 0),
                Width = 10,
                Height = 10,
                BackBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(243, 146, 243)),
                HorizontalAlignment = HorizontalAlignment.Left,
                Direction = CodeGenerateSystem.Base.enBezierType.Left,
            };
            grid.Children.Add(mParamEllipse);

            var stackPanel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Name = "ParamStackPanel",
            };
            grid.Children.Add(stackPanel);

            TextBlock label = new TextBlock()
            {
                Name = "ParamNameTextBlock",
                Text = mParamName,
                Foreground = Brushes.White,
                Margin = new System.Windows.Thickness(2, 3, 2, 3)
            };
            stackPanel.Children.Add(label);

            TextBlock textBlockType = new TextBlock()
            {
                Text = mParamFlag + (string.IsNullOrEmpty(mParamFlag) ? "" : " ") + mParamType,
                Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(160, 160, 160)),
                Margin = new System.Windows.Thickness(2)
            };
            stackPanel.Children.Add(textBlockType);
        }
        
        public bool IsParamLinkRight(ref string strMsg)
        {
            CSUtility.Helper.enCSType csType = CSUtility.Helper.enCSType.All;
            if (HostNodesContainer != null)
                csType = HostNodesContainer.mCSType;
            var linkOI = GetLinkObjInfo(mParamEllipse);
            if (linkOI.HasLink)
            {
                Type parType = CSUtility.Program.GetTypeFromSaveString(linkOI.GetLinkObject(0, true).GCode_GetValueType(linkOI.GetLinkElement(0, true)), csType);
                if (parType != mParamType)
                {
                    strMsg = "函数参数类型与连接的类型不匹配\r\n函数参数类型：" + mParamType.ToString() + "\r\n连接参数类型" + parType.ToString();
                    return false;
                }
            }
            else
            {
                strMsg = "函数没有设置参数 " + mParamName;
                return false;
            }

            return true;
        }
    }
    ///////////////////////////////////////////////////////////////////////////////
    public partial class MethodInvokeResultUnionControl
    {
        partial void InitConstruction()
        {
            Visibility = Visibility.Visible;
            Grid grid = new Grid();
            AddChild(grid);

            mResultMethod = new CodeGenerateSystem.Controls.LinkOutControl()
            {
                Margin = new System.Windows.Thickness(0, 0, -15, 0),
                Width = 10,
                Height = 10,
                BackBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(243, 146, 243)),
                HorizontalAlignment = HorizontalAlignment.Right,
                Direction = CodeGenerateSystem.Base.enBezierType.Right,
            };
            grid.Children.Add(mResultMethod);

            var stackPanel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Name = "ParamStackPanel",
                HorizontalAlignment = HorizontalAlignment.Right,
            };
            grid.Children.Add(stackPanel);

            TextBlock label = new TextBlock()
            {
                Name = "ParamNameTextBlock",
                Text = mParamName,
                Foreground = Brushes.White,
                Margin = new System.Windows.Thickness(2, 3, 2, 3),
            };
            stackPanel.Children.Add(label);

            TextBlock textBlockType = new TextBlock()
            {
                Text = mParamFlag + (string.IsNullOrEmpty(mParamFlag) ? "" : " ") + mParamType,
                Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(160, 160, 160)),
                Margin = new System.Windows.Thickness(2)
            };
            stackPanel.Children.Add(textBlockType);
        }

        public bool IsParamLinkRight(ref string strMsg)
        {
            CSUtility.Helper.enCSType csType = CSUtility.Helper.enCSType.All;
            if (HostNodesContainer != null)
                csType = HostNodesContainer.mCSType;
            var linkOI = GetLinkObjInfo(mResultMethod);
            if (linkOI.HasLink)
            {
                Type parType = CSUtility.Program.GetTypeFromSaveString(linkOI.GetLinkObject(0, true).GCode_GetValueType(linkOI.GetLinkElement(0, true)), csType);
                if (parType != mParamType)
                {
                    strMsg = "函数参数类型与连接的类型不匹配\r\n函数参数类型：" + mParamType.ToString() + "\r\n连接参数类型" + parType.ToString();
                    return false;
                }
            }
            else
            {
                strMsg = "函数没有设置参数 " + mParamName;
                return false;
            }

            return true;
        }

        //public override void Save(CSUtility.Support.XmlNode xmlNode)
        //{
        //    xmlNode.AddAttrib("Params", m_methodInfoToSave);

        //    base.Save(xmlNode);
        //}
    }

    public partial class MethodInUnionControl
    {
        partial void InitConstruction()
        {
            Visibility = Visibility.Visible;
            Grid grid = new Grid();
            AddChild(grid);

            mParamsMethod = new CodeGenerateSystem.Controls.LinkInControl()
            {
                Margin = new System.Windows.Thickness(0, 0, 0, 0),
                Width = 10,
                Height = 10,
                BackBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(130, 130, 216)),
                HorizontalAlignment = HorizontalAlignment.Right,
                Direction = CodeGenerateSystem.Base.enBezierType.Top,
            };

            grid.Children.Add(mParamsMethod);
        }

        //////public bool IsParamLinkRight(ref string strMsg)
        //////{
        //////    CSUtility.Helper.enCSType csType = CSUtility.Helper.enCSType.All;
        //////    if (HostNodesContainer != null)
        //////        csType = HostNodesContainer.mCSType;
        //////    var linkOI = GetLinkObjInfo(mParamsMethod);
        //////    if (linkOI.HasLink)
        //////    {
        //////        Type parType = CSUtility.Program.GetTypeFromSaveString(linkOI.GetLinkObject(0, true).GCode_GetValueType(linkOI.GetLinkElement(0, true)), csType);
        //////        if (parType != mParamType)
        //////        {
        //////            strMsg = "函数参数类型与连接的类型不匹配\r\n函数参数类型：" + mParamType.ToString() + "\r\n连接参数类型" + parType.ToString();
        //////            return false;
        //////        }
        //////    }
        //////    else
        //////    {
        //////        strMsg = "函数没有设置参数 " + mParamName;
        //////        return false;
        //////    }

        //////    return true;
        //////}
    }
    ///////////////////////////////////////////////////////////////////////////
    public partial class MethodOutUnionControl
    {
        partial void InitConstruction()
        {
            Visibility = Visibility.Visible;
            Grid grid = new Grid();
            AddChild(grid);

            mResultMethod = new CodeGenerateSystem.Controls.LinkOutControl()
            {
                Margin = new System.Windows.Thickness(0, 0, 0, 0),
                Width = 10,
                Height = 10,
                BackBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(130, 130, 216)),
                HorizontalAlignment = HorizontalAlignment.Right,
                Direction = CodeGenerateSystem.Base.enBezierType.Bottom,
            };

            grid.Children.Add(mResultMethod);
        }

        //////public bool IsParamLinkRight(ref string strMsg)
        //////{
        //////    CSUtility.Helper.enCSType csType = CSUtility.Helper.enCSType.All;
        //////    if (HostNodesContainer != null)
        //////        csType = HostNodesContainer.mCSType;
        //////    var linkOI = GetLinkObjInfo(mResultMethod);
        //////    if (linkOI.HasLink)
        //////    {
        //////        Type parType = CSUtility.Program.GetTypeFromSaveString(linkOI.GetLinkObject(0, true).GCode_GetValueType(linkOI.GetLinkElement(0, true)), csType);
        //////        if (parType != mParamType)
        //////        {
        //////            strMsg = "函数参数类型与连接的类型不匹配\r\n函数参数类型：" + mParamType.ToString() + "\r\n连接参数类型" + parType.ToString();
        //////            return false;
        //////        }
        //////    }
        //////    else
        //////    {
        //////        strMsg = "函数没有设置参数 " + mParamName;
        //////        return false;
        //////    }

        //////    return true;
        //////}
    }
}
