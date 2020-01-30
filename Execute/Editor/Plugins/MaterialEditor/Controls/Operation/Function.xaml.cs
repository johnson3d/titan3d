using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using CodeGenerateSystem.Base;

namespace MaterialEditor.Controls.Operation
{
    [EngineNS.Rtti.MetaClass]
    public class FunctionConstructionParams : NodeControlConstructionBase { }
    /// <summary>
    /// Interaction logic for Function.xaml
    /// </summary>
    [CodeGenerateSystem.CustomConstructionParams(typeof(FunctionConstructionParams))]
    public partial class Function : BaseNodeControl
    {
        string mStrFuncName = "";

        private FrameworkElement mReturnValueHandle;
        private class stParamData
        {
            public int mPos;                       // 第几个参数
            public string mStrName;                // 参数名称
            public string mStrType;                // 参数类型
            public string mStrAttribute;           // 参数inout、out、return等说明
            public CodeGenerateSystem.Base.LinkPinControl mInElement;
            public CodeGenerateSystem.Base.LinkPinControl mOutElement;
        }
        private Dictionary<FrameworkElement, stParamData> mOutValueDataDictionary = new Dictionary<FrameworkElement, stParamData>();
        private List<stParamData> mInParamDataList = new List<stParamData>();
        private List<stParamData> mOnlyOutParamDataList = new List<stParamData>();

        public void UpdateParam(System.Xml.XmlDocument xmlDocNew, string csParamPre)
        {
            var tempElements = xmlDocNew.GetElementsByTagName("Param");
            if (tempElements.Count > 0)
            {
                var cpInfos = new List<CodeGenerateSystem.Base.CustomPropertyInfo>();

                var paramInElm = tempElements[0];
                foreach (System.Xml.XmlElement node in paramInElm.ChildNodes)
                {
                    var typeStr = node.GetAttribute("Type");
                    var nameStr = node.GetAttribute("Name");
                    var strAttr = node.GetAttribute("Attribute");

                    foreach(var data in mOutValueDataDictionary.Values)
                    {
                        if(data.mStrName == nameStr)
                        {
                            data.mStrType = typeStr;
                            data.mStrAttribute = strAttr;
                        }
                    }
                    foreach(var data in mInParamDataList)
                    {
                        if (data.mStrName == nameStr)
                        {
                            data.mStrType = typeStr;
                            data.mStrAttribute = strAttr;
                        }
                    }
                    foreach(var data in mOnlyOutParamDataList)
                    {
                        if (data.mStrName == nameStr)
                        {
                            data.mStrType = typeStr;
                            data.mStrAttribute = strAttr;
                        }
                    }

                    // float替换为half
                    var oldTypeStr = typeStr.Replace("half", "float");
                    var newStr = typeStr + "_" + nameStr;
                    var oldStr = oldTypeStr + "_" + nameStr;

                    LinkPinControl ctrl;
                    if(mLinkPinInfoDic_Name.TryGetValue(oldStr, out ctrl))
                    {
                        mLinkPinInfoDic_Name.Remove(oldStr);
                        ctrl.PinName = newStr;
                        mLinkPinInfoDic_Name.Add(newStr, ctrl);
                    }

                }

            }
            CSParam.ConstructParam = csParamPre + "|" + xmlDocNew.OuterXml;
        }

        // 临时类，用于选中后显示参数属性
        CodeGenerateSystem.Base.GeneratorClassBase mTemplateClassInstance = null;
        public CodeGenerateSystem.Base.GeneratorClassBase TemplateClassInstance
        {
            get { return mTemplateClassInstance; }
        }

        public Function(CodeGenerateSystem.Base.ConstructionParams smParam)
            : base(smParam)
        {
            InitializeComponent();

            var splits = CSParam.ConstructParam.Split('|');

            var include = splits[0];
            if (include.Contains(":"))
            {
                include = EngineNS.CEngine.Instance.FileManager._GetRelativePathFromAbsPath(include, EngineNS.CEngine.Instance.FileManager.Bin);
            }
            include = include.Replace("\\", "/");
            if (include.Contains("bin/"))
            {
                var nIdx = include.IndexOf("bin/");
                include = include.Substring(nIdx + 4);
            }

            System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
            xmlDoc.LoadXml(splits[1]);

            mStrFuncName = xmlDoc.DocumentElement.GetAttribute("Name");
            NodeName = mStrFuncName;
            var tempElements = xmlDoc.GetElementsByTagName("Param");
            if (tempElements.Count > 0)
            {
                var cpInfos = new List<CodeGenerateSystem.Base.CustomPropertyInfo>();

                var paramInElm = tempElements[0];
                int nIdx = 0;
                foreach (System.Xml.XmlElement node in paramInElm.ChildNodes)
                {
                    var typeStr = node.GetAttribute("Type");
                    var nameStr = node.GetAttribute("Name");
                    var strAttr = node.GetAttribute("Attribute");

                    switch (strAttr)
                    {
                        case "out":
                        case "return":
                            break;
                        default:
                            {
                                var cpInfo = CodeGenerateSystem.Base.CustomPropertyInfo.GetFromParamInfo(Program.GetTypeFromValueType(typeStr), nameStr, new Attribute[] { new EngineNS.Rtti.MetaDataAttribute() });
                                if (cpInfo != null)
                                    cpInfos.Add(cpInfo);
                            }
                            break;
                    }

                    AddLink(nIdx, typeStr, nameStr, strAttr);
                    nIdx++;
                }

                mTemplateClassInstance = CodeGenerateSystem.Base.PropertyClassGenerator.CreateClassInstanceFromCustomPropertys(cpInfos, this, $"{this.GetType().FullName}.PropertyClass_{mStrFuncName}", false);
                foreach (var property in mTemplateClassInstance.GetType().GetProperties())
                {
                    property.SetValue(mTemplateClassInstance, CodeGenerateSystem.Program.GetDefaultValueFromType(property.PropertyType));
                }
                if (HostNodesContainer != null && HostNodesContainer.HostControl != null)
                    mTemplateClassInstance.EnableUndoRedo(HostNodesContainer.HostControl.UndoRedoKey, mStrFuncName);
            }

            this.UpdateLayout();
        }

        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            var splits = smParam.ConstructParam.Split('|');

            var include = splits[0];
            if (include.Contains(":"))
            {
                include = EngineNS.CEngine.Instance.FileManager._GetRelativePathFromAbsPath(include, EngineNS.CEngine.Instance.FileManager.Bin);
            }
            include = include.Replace("\\", "/");
            if (include.Contains("bin/"))
            {
                var nIdx = include.IndexOf("bin/");
                include = include.Substring(nIdx + 4);
            }

            System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
            xmlDoc.LoadXml(splits[1]);

            var tempElements = xmlDoc.GetElementsByTagName("Param");
            if (tempElements.Count > 0)
            {
                var cpInfos = new List<CodeGenerateSystem.Base.CustomPropertyInfo>();

                var paramInElm = tempElements[0];
                foreach (System.Xml.XmlElement node in paramInElm.ChildNodes)
                {
                    var strType = node.GetAttribute("Type");
                    var nameStr = node.GetAttribute("Name");
                    var strAttr = node.GetAttribute("Attribute");

                    if(string.IsNullOrEmpty(strAttr))
                    {
                        var linkType = LinkPinControl.GetLinkTypeFromTypeString(strType);
                        switch(linkType)
                        {
                            case enLinkType.Int32:
                            case enLinkType.Int64:
                            case enLinkType.Single:
                            case enLinkType.Double:
                            case enLinkType.Byte:
                            case enLinkType.SByte:
                            case enLinkType.Int16:
                            case enLinkType.UInt16:
                            case enLinkType.UInt32:
                            case enLinkType.UInt64:
                            case enLinkType.Float1:
                                linkType = enLinkType.NumbericalValue | enLinkType.Float1 | enLinkType.Float2 | enLinkType.Float3 | enLinkType.Float4;
                                break;
                            case enLinkType.Float2:
                                linkType = enLinkType.Float2 | enLinkType.Float3 | enLinkType.Float4;
                                break;
                            case enLinkType.Float3:
                                linkType = enLinkType.Float3 | enLinkType.Float4;
                                break;
                        }
                        CollectLinkPinInfo(smParam, strType + "_" + nameStr, linkType, enBezierType.Left, enLinkOpType.End, false);
                    }
                    else if(strAttr == "out")
                    {
                        CollectLinkPinInfo(smParam, strType + "_" + nameStr, LinkPinControl.GetLinkTypeFromTypeString(strType), enBezierType.Right, enLinkOpType.Start, true);
                    }
                    else if(strAttr == "inout")
                    {
                        var linkType = LinkPinControl.GetLinkTypeFromTypeString(strType);
                        switch (linkType)
                        {
                            case enLinkType.Int32:
                            case enLinkType.Int64:
                            case enLinkType.Single:
                            case enLinkType.Double:
                            case enLinkType.Byte:
                            case enLinkType.SByte:
                            case enLinkType.Int16:
                            case enLinkType.UInt16:
                            case enLinkType.UInt32:
                            case enLinkType.UInt64:
                            case enLinkType.Float1:
                                linkType = enLinkType.Float1 | enLinkType.Float2 | enLinkType.Float3 | enLinkType.Float4;
                                break;
                            case enLinkType.Float2:
                                linkType = enLinkType.Float2 | enLinkType.Float3 | enLinkType.Float4;
                                break;
                            case enLinkType.Float3:
                                linkType = enLinkType.Float3 | enLinkType.Float4;
                                break;
                        }
                        CollectLinkPinInfo(smParam, strType + "_" + nameStr + "_in", linkType, enBezierType.Left, enLinkOpType.End, false);
                        CollectLinkPinInfo(smParam, strType + "_" + nameStr + "_out", linkType, enBezierType.Right, enLinkOpType.Start, true);
                    }
                    else if (strAttr == "return")
                    {
                        CollectLinkPinInfo(smParam, strType + "_" + nameStr, LinkPinControl.GetLinkTypeFromTypeString(strType), enBezierType.Right, enLinkOpType.Start, true);
                    }
                }
            }
        }

        public override object GetShowPropertyObject()
        {
            return mTemplateClassInstance;
        }
        public override BaseNodeControl Duplicate(DuplicateParam param)
        {
            var copyedNode = base.Duplicate(param) as Function;
            CodeGenerateSystem.Base.PropertyClassGenerator.CloneClassInstanceProperties(mTemplateClassInstance, copyedNode.mTemplateClassInstance);
            return copyedNode;
        }
        public override void Save(EngineNS.IO.XndNode xndNode, bool newGuid)
        {
            if (mTemplateClassInstance != null)
            {
                var att = xndNode.AddAttrib("_funcDefParam");
                att.Version = 1;
                att.BeginWrite();
                CodeGenerateSystem.Base.PropertyClassGenerator.SaveClassInstanceProperties(mTemplateClassInstance, att);
                att.EndWrite();
            }
            base.Save(xndNode, newGuid);
        }

        public override async System.Threading.Tasks.Task Load(EngineNS.IO.XndNode xndNode)
        {
            await base.Load(xndNode);

            if (mTemplateClassInstance != null)
            {
                var att = xndNode.FindAttrib("_funcDefParam");
                att.BeginRead();
                switch(att.Version)
                {
                    case 0:
                        att.ReadMetaObject(mTemplateClassInstance);
                        break;
                    case 1:
                        CodeGenerateSystem.Base.PropertyClassGenerator.LoadClassInstanceProperties(mTemplateClassInstance, att);
                        break;
                }
                att.EndRead();
            }
        }

        private void AddLink(int nIdx, string strType, string strName, string strAttribute)
        {
            // half在显示时使用float
            var showType = strType.Replace("half", "float");

            //var splits = strAttribute.Split('|');
            if (string.IsNullOrEmpty(strAttribute))
            {
                var ellipse = new CodeGenerateSystem.Controls.LinkInControl()
                {
                    Margin = new System.Windows.Thickness(8),
                    BackBrush = Program.GetBrushFromValueType(strType, this),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Direction = enBezierType.Left,
                    NameString = strName + "(" + showType + ")",
                };
                AddLinkPinInfo(strType + "_" + strName, ellipse, null);
                StackPanel_InValue.Children.Add(ellipse);

                stParamData parData = new stParamData()
                {
                    mPos = nIdx,
                    mStrName = strName,
                    mStrType = strType,
                    mStrAttribute = "",
                    mInElement = ellipse
                };
                mInParamDataList.Add(parData);
            }
            else if (strAttribute == "out")
            {
                var rect = new CodeGenerateSystem.Controls.LinkOutControl()
                {
                    Margin = new System.Windows.Thickness(8),
                    BackBrush = Program.GetBrushFromValueType(strType, this),
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Direction = enBezierType.Right,
                    NameString = strName + "(" + showType + ")",
                };
                AddLinkPinInfo(strType + "_" + strName, rect, rect.BackBrush);
                StackPanel_OutValue.Children.Add(rect);

                stParamData ovData = new stParamData()
                {
                    mPos = nIdx,
                    mStrName = strName,
                    mStrType = strType,
                    mStrAttribute = strAttribute,
                    mOutElement = rect
                };

                mInParamDataList.Add(ovData);
                mOnlyOutParamDataList.Add(ovData);
                mOutValueDataDictionary[rect] = ovData;
            }
            else if(strAttribute == "inout")
            {
                var ellipse = new CodeGenerateSystem.Controls.LinkInControl()
                {
                    Margin = new System.Windows.Thickness(8),
                    BackBrush = Program.GetBrushFromValueType(strType, this),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Direction = enBezierType.Left,
                    NameString = strName + "(" + showType + ")",
                };
                AddLinkPinInfo(strType + "_" + strName + "_in", ellipse, null);
                StackPanel_InValue.Children.Add(ellipse);


                var rect = new CodeGenerateSystem.Controls.LinkOutControl()
                {
                    Margin = new System.Windows.Thickness(8),
                    BackBrush = Program.GetBrushFromValueType(strType, this),
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Direction = enBezierType.Right,
                    NameString = strName + "(" + showType + ")",
                };
                Grid.SetColumn(rect, 1);
                AddLinkPinInfo(strType + "_" + strName + "_out", rect, rect.BackBrush);
                StackPanel_OutValue.Children.Add(rect);

                stParamData parData = new stParamData()
                {
                    mPos = nIdx,
                    mStrName = strName, 
                    mStrType = strType, 
                    mStrAttribute = strAttribute, 
                    mInElement = ellipse,
                    mOutElement = rect,
                };
                mInParamDataList.Add(parData);

                stParamData ovData = new stParamData()
                {
                    mPos = nIdx,
                    mStrName = strName,
                    mStrType = strType,
                    mStrAttribute = strAttribute,
                    mInElement = ellipse,
                    mOutElement = rect
                };
                mOutValueDataDictionary[rect] = ovData;
            }
            else if (strAttribute == "return")
            {
                var rect = new CodeGenerateSystem.Controls.LinkOutControl()
                {
                    Margin = new System.Windows.Thickness(8),
                    BackBrush = Program.GetBrushFromValueType(strType, this),
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Direction = enBezierType.Right,
                    NameString = "Return(" + showType + ")",
                };
                Grid.SetColumn(rect, 1);
                AddLinkPinInfo(strType + "_" + strName, rect, rect.BackBrush);
                mReturnValueHandle = rect;
                StackPanel_OutValue.Children.Add(rect);

                stParamData ovData = new stParamData()
                {
                    mPos = nIdx,
                    mStrName = strName,
                    mStrType = strType,
                    mStrAttribute = strAttribute,
                    mOutElement = rect
                };

                mOutValueDataDictionary[rect] = ovData;
            }
        }

        protected override void CollectionErrorMsg()
        {
            foreach (var pardata in mInParamDataList)
            {
                if (pardata.mInElement == null)
                    continue;
                var lOI = pardata.mInElement;
                if (!lOI.HasLink)
                {
                    if(mTemplateClassInstance != null)
                    {
                        if(mTemplateClassInstance.GetType().GetProperty(pardata.mStrName) == null)
                        {
                            HasError = true;
                            ErrorDescription = "未连接参数!";
                        }
                    }
                    else
                    {
                        HasError = true;
                        ErrorDescription = "未连接参数!";
                    }
                }
            }
        }

        public override void GCode_GenerateCode(ref string strDefinitionSegment, ref string strSegment, int nLayer, CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            string strTab = GCode_GetTabString(nLayer);
            string strIdt = "";

            // 所有的out和inout需要转成对应的类型传参然后再赋值回来
            foreach(var pardata in mInParamDataList)
            {
                if (!pardata.mStrType.Contains("half"))
                    continue;

                if(pardata.mStrAttribute == "out")
                {
                    //strIdt += $"{strTab}{pardata.mStrType} {pardata.mStrName}_{EngineNS.Editor.Assist.GetValuedGUIDString(Id)};\r\n";
                }
                else if(pardata.mStrAttribute == "inout")
                {
                    strIdt += $"{strTab}{pardata.mStrName}_{EngineNS.Editor.Assist.GetValuedGUIDString(Id)}";
                    var lOI = pardata.mInElement;
                    if(lOI.HasLink)
                    {
                        lOI.GetLinkedObject(0, true).GCode_GenerateCode(ref strDefinitionSegment, ref strSegment, nLayer, lOI.GetLinkedPinControl(0, true), context);
                        var linkVarName = lOI.GetLinkedObject(0, true).GCode_GetValueName(lOI.GetLinkedPinControl(0, true), context);
                        var linkType = lOI.GetLinkType(0, true);
                        switch (pardata.mStrType)
                        {
                            case "float":
                            case "float1":
                            case "half":
                            case "half1":
                                if (linkType != enLinkType.Float1)
                                    linkVarName += ".x";
                                break;
                            case "float2":
                            case "half2":
                                if (linkType != enLinkType.Float2)
                                    linkVarName += ".xy";
                                break;
                            case "float3":
                            case "half3":
                                if (linkType != enLinkType.Float3)
                                    linkVarName += ".xyz";
                                break;
                        }
                        strIdt += $" = ({pardata.mStrType}){linkVarName};\r\n";
                    }
                    else
                    {
                        // 写入默认值
                        if(mTemplateClassInstance != null)
                        {
                            var property = mTemplateClassInstance.GetType().GetProperty(pardata.mStrName);
                            if(property != null)
                            {
                                var propertyValue = property.GetValue(mTemplateClassInstance);
                                var value = Program.GetTypeValueString(pardata.mStrType, propertyValue);
                                strIdt += $" = {value};\r\n";
                            }
                        }
                    }
                }
            };

            strIdt += strTab + mStrFuncName + "(";
            foreach (var pardata in mInParamDataList)
            {
                if (pardata.mStrAttribute == "inout" || pardata.mStrAttribute == "out")
                {
                    if (pardata.mStrType.Contains("half"))
                        strIdt += $"{pardata.mStrName}_{EngineNS.Editor.Assist.GetValuedGUIDString(Id)},";
                    else
                        strIdt += GCode_GetValueName(pardata.mOutElement, context) + ",";
                }
                else if(pardata.mStrAttribute == "in" || string.IsNullOrEmpty(pardata.mStrAttribute))
                {
                    var lOI = pardata.mInElement;
                    if (lOI.HasLink)
                    {
                        lOI.GetLinkedObject(0, true).GCode_GenerateCode(ref strDefinitionSegment, ref strSegment, nLayer, lOI.GetLinkedPinControl(0, true), context);
                        var linkVarName = lOI.GetLinkedObject(0, true).GCode_GetValueName(lOI.GetLinkedPinControl(0, true), context);
                        var linkType = lOI.GetLinkType(0, true);
                        switch (pardata.mStrType)
                        {
                            case "float":
                            case "float1":
                            case "half":
                            case "half1":
                                if (linkType != enLinkType.Float1)
                                    linkVarName += ".x";
                                break;
                            case "float2":
                            case "half2":
                                if (linkType != enLinkType.Float2)
                                    linkVarName += ".xy";
                                break;
                            case "float3":
                            case "half3":
                                if (linkType != enLinkType.Float3)
                                    linkVarName += ".xyz";
                                break;
                        }
                        strIdt += $"({pardata.mStrType}){linkVarName},";
                    }
                    else
                    {
                        // 写入默认值
                        if(mTemplateClassInstance != null)
                        {
                            var property = mTemplateClassInstance.GetType().GetProperty(pardata.mStrName);
                            if(property != null)
                            {
                                var propertyValue = property.GetValue(mTemplateClassInstance);
                                var value = Program.GetTypeValueString(pardata.mStrType, propertyValue);
                                strIdt += $"{value},";
                            }
                        }
                    }
                }
            }


            // 去掉最后一个","
            strIdt = strIdt.Remove(strIdt.Length - 1);
            strIdt += ");\r\n";

            foreach(var pardata in mOutValueDataDictionary.Values)
            {
                if (!pardata.mStrType.Contains("half"))
                    continue;

                var tagType = pardata.mStrType.Replace("half", "float");
                // 赋值
                switch(pardata.mStrAttribute)
                {
                    case "out":
                    case "inout":
                        {
                            strIdt += $"{strTab}{GCode_GetValueName(pardata.mOutElement, context)} = ({tagType}){pardata.mStrName}_{EngineNS.Editor.Assist.GetValuedGUIDString(Id)};\r\n";
                        }
                        break;
                }
            }

            if (mReturnValueHandle != null)
            {
                var pardata = mOutValueDataDictionary[mReturnValueHandle];
                if(pardata.mStrType.Contains("half"))
                {
                    var finalType = pardata.mStrType.Replace("half", "float");
                    strIdt = $"{finalType} {GCode_GetValueName(null, context)} = ({finalType}){strIdt}";
                }
                else
                    strIdt = pardata.mStrType + " " + GCode_GetValueName(null, context) + " = " + strIdt;
            }

            //strIdt = strTab + strIdt;

            // 函数输出参数放入声明段，重复的不再添加
            foreach (var pardata in mOutValueDataDictionary.Values)
            {
                if(pardata.mStrAttribute == "out" || pardata.mStrAttribute == "inout")
                {
                    var finalType = pardata.mStrType.Replace("half", "float");
                    var strValueIdt = finalType + " " + GCode_GetValueName(pardata.mOutElement, context) + " = " + Program.GetInitialNewString(finalType) + ";\r\n";
                    if (!strDefinitionSegment.Contains(strValueIdt))
                        strDefinitionSegment += "    " + strValueIdt;

                    var tempIdt = $"{strTab}{pardata.mStrType} {pardata.mStrName}_{EngineNS.Editor.Assist.GetValuedGUIDString(Id)};\r\n";
                    if (!strDefinitionSegment.Contains(tempIdt))
                        strDefinitionSegment += "    " + tempIdt;
                }
            }

            // 判断该段代码是否调用过，同样的参数调用过则不再调用该函数
            //if (!strSegment.Contains(strIdt))
            if (!Program.IsSegmentContainString(strSegment.Length - 1, strSegment, strIdt))
            {                
                strSegment += strIdt;
            }

            base.GCode_GenerateCode(ref strDefinitionSegment, ref strSegment, nLayer, element, context);
        }

        public override string GCode_GetValueName(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            if (element == null || element == mReturnValueHandle)
            {
                return "FuncRetValue_" + EngineNS.Editor.Assist.GetValuedGUIDString(Id);
            }
            else
            {
                stParamData valData;
                if (mOutValueDataDictionary.TryGetValue(element, out valData))
                {
                    if (valData.mStrAttribute == "out")
                    {
                        return "FuncTempValue_" + EngineNS.Editor.Assist.GetValuedGUIDString(Id) + "_" + valData.mPos;
                    }
                    else
                    {
                        var lOI = valData.mInElement;
                        if (lOI.HasLink)
                        {
                            return lOI.GetLinkedObject(0, true).GCode_GetValueName(lOI.GetLinkedPinControl(0, true), context);
                        }
                        else
                            return "FuncTempValue_" + EngineNS.Editor.Assist.GetValuedGUIDString(Id) + "_" + valData.mPos;
                    }
                }
            }

            return base.GCode_GetValueName(element, context);
        }

        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            stParamData valData;
            if (mOutValueDataDictionary.TryGetValue(element, out valData))
                return valData.mStrType;

            return base.GCode_GetTypeString(element, context);
        }
    }
}
