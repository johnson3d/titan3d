using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

namespace CodeDomNode
{
    public partial class MethodInvokeUnionNode : CodeGenerateSystem.Base.BaseNodeControl, CodeGenerateSystem.Base.GeneratorClass
    {
        public override CodeGenerateSystem.Base.LinkObjInfo GetLinkObjInfo(Guid guid)
        {
            var retLinkObj = base.GetLinkObjInfo(guid);
            if (retLinkObj != null)
            {
                return retLinkObj;
            }

            foreach (var ctrlNode in PackageNodeList)
            {
                retLinkObj = ctrlNode.GetLinkObjInfo(guid);
                if (retLinkObj != null)
                    return retLinkObj;
            }
            return retLinkObj;
        }
        partial void ShowMethodLost_WPF(bool show);
        bool mShowMethodLost = false;
        public bool ShowMethodLost
        {
            get { return mShowMethodLost; }
            set
            {
                mShowMethodLost = value;
                ShowMethodLost_WPF(mShowMethodLost);
            }
        }

        bool mAutoGenericIsNullCode = true;
        public bool AutoGenericIsNullCode
        {
            get { return mAutoGenericIsNullCode; }
            set
            {
                mAutoGenericIsNullCode = value;
                OnPropertyChanged("AutoGenericIsNullCode");
            }
        }

        // 临时类，用于选中后显示参数属性
        CodeGenerateSystem.Base.GeneratorClassBase mTemplateClassInstance = null;
        public CodeGenerateSystem.Base.GeneratorClassBase TemplateClassInstance
        {
            get { return mTemplateClassInstance; }
        }
        List<CodeGenerateSystem.Base.CustomPropertyInfo> mStandardParamPropertyInfos = new List<CodeGenerateSystem.Base.CustomPropertyInfo>();
        public List<CodeGenerateSystem.Base.CustomPropertyInfo> StandardParamPropertyInfos
        {
            get { return mStandardParamPropertyInfos; }
        }
        // 类实例名称
        string mClassInstanceName;
        public string ClassInstanceName
        {
            get { return mClassInstanceName; }
            set
            {
                mClassInstanceName = value;
                OnPropertyChanged("ClassInstanceName");
            }
        }
        CodeGenerateSystem.Base.UsefulMemberHostData mHostUsefulMemberData = new CodeGenerateSystem.Base.UsefulMemberHostData();
        System.Reflection.MethodInfo mMethodInfo = null;

        public delegate CodeGenerateSystem.Base.BaseNodeControl DelegateAddNewChildNode(Type nodeType, string Params, double x, double y, Canvas drawNode);
        public static DelegateAddNewChildNode AddNodeControl;

        // 打包的节点列表
        public List<CodeGenerateSystem.Base.BaseNodeControl> PackageNodeList = new List<CodeGenerateSystem.Base.BaseNodeControl>();
        string mUnionNodeNameText = "组合节点";
        public string UnionNodeNameText
        {
            get { return mUnionNodeNameText; }
            set
            {
                mUnionNodeNameText = value;
                OnPropertyChanged("UnionNodeNameText");
            }
        }
        List<string> mConstructionErrors = new List<string>();

        partial void OnDeleteNode_WPF();
        partial void InitConstruction();
        public MethodInvokeUnionNode(CodeGenerateSystem.Base.ConstructionParams csParam, string methodParam)
            : base(csParam, methodParam)
        {
            InitConstruction();

            try
            {
                if (methodParam != "")
                {
                    var UnionParamSplits = methodParam.Split(';');
                    if (UnionParamSplits.Length < 2)
                        return;

                    UnionNodeNameText = UnionParamSplits[0];
                    NodeName = UnionNodeNameText;

                    var splits = UnionParamSplits[1].Split('^');

                    // 输入参数
                    if (splits[0] != "")
                    {
                        var param = splits[0].Split('/');
                        for (int j = 0; j < param.Length; j++)
                        {
                            AddLinkInNode(param[j]);
                        }
                    }

                    // 输出参数
                    if (splits[1] != "")
                    {
                        var param = splits[1].Split('/');
                        for (int j = 0; j < param.Length; j++)
                        {
                            AddLinkOutNode(param[j]);
                        }
                    }

                    // 函数连接输入参数
                    if (splits[2] != "")
                    {
                        var param = splits[2].Split('/');
                        for (int j = 0; j < param.Length; j++)
                        {
                            AddLinkInMethodNode(param[j]);
                        }
                    }

                    // 函数连接输出参数
                    if (splits[3] != "")
                    {
                        var param = splits[3].Split('/');
                        for (int j = 0; j < param.Length; j++)
                        {
                            AddLinkOutMethodNode(param[j]);
                        }
                    }
                }

                OnDeleteNode += (node) =>
                {
                    OnDeleteNode_WPF();
                    mHostUsefulMemberData.HostControl?.RemoveMoveableChildNode(this);
                };
            }
            catch (System.Exception e)
            {
                ShowMethodLost = true;
                var errorStr = $"连线函数节点异常！：Name={NodeName}, Param={methodParam}\r\n{e.ToString()}";
                mConstructionErrors.Add(errorStr);
                System.Diagnostics.Debug.WriteLine(errorStr);
            }
        }
        protected override void AddChildNode(CodeGenerateSystem.Base.BaseNodeControl node, Panel container)
        {
            if (node is CodeDomNode.MethodInUnionControl)
            {
                base.AddChildNodeNoChanageContainer(node, mInputMethodParamsPanel);
            }
            else if (node is CodeDomNode.MethodOutUnionControl)
            {
                base.AddChildNodeNoChanageContainer(node, mOutputMethodParamsPanel);
            }
            else
            {
                base.AddChildNode(node, container);
            }
        }

        StackPanel mInputParamsPanel = null;
        StackPanel mInputMethodParamsPanel = null;
        StackPanel mOutputMethodParamsPanel = null;
        public CodeGenerateSystem.Base.BaseNodeControl AddLinkInNode(string param)
        {
            var csParam = new CodeGenerateSystem.Base.ConstructionParams()
            {
                CSType = mCSParam.CSType,
                HostNodesContainer = mCSParam.HostNodesContainer,
            };
            var pc = new MethodInvokeParameterUnionControl(csParam, param);
            AddChildNode(pc, mInputParamsPanel);
            return pc;
        }

        public CodeGenerateSystem.Base.BaseNodeControl AddLinkOutNode(string param)
        {
            var csParam = new CodeGenerateSystem.Base.ConstructionParams()
            {
                CSType = mCSParam.CSType,
                HostNodesContainer = mCSParam.HostNodesContainer,
            };
            var pc = new MethodInvokeResultUnionControl(csParam, param);
            AddChildNode(pc, mInputParamsPanel);
            return pc;
        }

        public CodeGenerateSystem.Base.BaseNodeControl AddLinkInMethodNode(string param)
        {
            var csParam = new CodeGenerateSystem.Base.ConstructionParams()
            {
                CSType = mCSParam.CSType,
                HostNodesContainer = mCSParam.HostNodesContainer,
            };
            var pc = new MethodInUnionControl(csParam, param);
            AddChildNode(pc, mInputMethodParamsPanel);
            return pc;
        }

        public CodeGenerateSystem.Base.BaseNodeControl AddLinkOutMethodNode(string param)
        {
            var csParam = new CodeGenerateSystem.Base.ConstructionParams()
            {
                CSType = mCSParam.CSType,
                HostNodesContainer = mCSParam.HostNodesContainer,
            };
            var pc = new MethodOutUnionControl(csParam, param);
            AddChildNode(pc, mOutputMethodParamsPanel);
            return pc;
        }
        public override void Save(EngineNS.IO.XmlNode xmlNode, bool newGuid, EngineNS.IO.XmlHolder holder)
        {
            StrParams = UnionNodeNameText;

            var listParam = new List<string>();
            var listResult = new List<string>();
            var listIn = new List<string>();
            var listOut = new List<string>();
            foreach (var childNode in GetChildNodes())
            {
                foreach (var linkObj in childNode.LinkObjList.Values)
                {
                    var param = childNode.GetLinkObjParamInfo(linkObj);
                    if (childNode is MethodInvokeParameterUnionControl)
                    {
                        listParam.Add(param);
                    }
                    else if (childNode is MethodInvokeResultUnionControl)
                    {
                        listResult.Add(param);
                    }
                    else if (childNode is MethodInUnionControl)
                    {
                        listIn.Add(param);
                    }
                    else if (childNode is MethodOutUnionControl)
                    {
                        listOut.Add(param);
                    }
                    else
                    {
                        // 不可能
                    }
                }
            }

            StrParams += ";";
            foreach (var str in listParam)
            {
                StrParams += str + "/";
            }
            // 删除最后一个"/"
            if (listParam.Count != 0)
            {
                StrParams = StrParams.Remove(StrParams.Length - 1);
            }

            StrParams += "^";
            foreach (var str in listResult)
            {
                StrParams += str + "/";
            }
            // 删除最后一个"/"
            if (listResult.Count != 0)
            {
                StrParams = StrParams.Remove(StrParams.Length - 1);
            }

            StrParams += "^";
            foreach (var str in listIn)
            {
                StrParams += str + "/";
            }
            // 删除最后一个"/"
            if (listIn.Count != 0)
            {
                StrParams = StrParams.Remove(StrParams.Length - 1);
            }

            StrParams += "^";
            foreach (var str in listOut)
            {
                StrParams += str + "/";
            }
            // 删除最后一个"/"
            if (listOut.Count != 0)
            {
                StrParams = StrParams.Remove(StrParams.Length - 1);
            }

            var combinationNodes = xmlNode.AddNode("combinationNodes", "", holder);
            foreach (var node in PackageNodeList)
            {
                EngineNS.IO.XmlNode childXml = combinationNodes.AddNode(node.GetType().Name, node.GetType().FullName, holder);
                node.Save(childXml, newGuid, holder);
                node.SaveLinks(childXml, false, holder);
            }
            base.Save(xmlNode, newGuid, holder);
        }

        partial void ClearChildNode_WPF(CodeGenerateSystem.Base.BaseNodeControl child);
        protected override void ClearChildNode()
        {
            foreach (var child in mChildNodes)
            {
                child.OnStartLink -= new CodeGenerateSystem.Base.Delegate_StartLink(StartLink);
                child.OnEndLink -= new CodeGenerateSystem.Base.Delegate_EndLink(EndLink);
                ClearChildNode_WPF(child);
            }
            mChildNodes.Clear();
        }
        partial void SetErrorShow_WPF(object showValue);
        public override void Load(EngineNS.IO.XmlNode xmlNode, double deltaX, double deltaY)
        {
            try
            {
                base.Load(xmlNode, deltaX, deltaY);

                var childNode = xmlNode.FindNode("combinationNodes");
                foreach (var node in childNode.GetNodes())
                {
                    Type ctrlType = EngineNS.Rtti.RttiHelper.GetTypeFromSaveString(node.FindAttrib("Type").Value);//  Program.GetType(xmlNode.FindAttrib("Type").Value);
                    if (ctrlType == null)
                        continue;
                    //Type nodeType = Type.GetType(element.GetAttribute("Type"));

                    //string strParam = element.GetAttribute("Params");
                    var paramAtt = node.FindAttrib("Params");
                    string strParam = null;
                    if (paramAtt != null)
                        strParam = paramAtt.Value;

                    //double x = System.Convert.ToDouble(element.GetAttribute("X"));
                    //double y = System.Convert.ToDouble(element.GetAttribute("Y"));
                    double x = System.Convert.ToDouble(node.FindAttrib("X").Value);
                    double y = System.Convert.ToDouble(node.FindAttrib("Y").Value);

                    CodeGenerateSystem.Base.BaseNodeControl nodeControl = HostNodesContainer.AddNodeControl(ctrlType, strParam, x, y, false);
                    nodeControl.Load(node);
                    PackageNodeList.Add(nodeControl);
                    nodeControl.PackagedNode = this;
                }
            }
            catch (System.Exception e)
            {
                ShowMethodLost = true;
                SetErrorShow_WPF("节点读取失败！");
                System.Diagnostics.Debug.WriteLine($"节点读取失败！：Name={NodeName}\r\n{e.ToString()}");
            }
        }

        public override void LoadLinks(EngineNS.IO.XmlNode xmlNode, List<string> includeIdList = null)
        {
            var childNode = xmlNode.FindNode("combinationNodes");
            foreach (var cXmlNode in childNode.GetNodes())
            {
                string strGUID = cXmlNode.FindAttrib("ID").Value;
                Guid tagGUID = Guid.Parse(strGUID);
                foreach (var child in PackageNodeList)
                {
                    if (child.Id == tagGUID)
                    {
                        child.LoadLinks(cXmlNode, includeIdList);
                        // 刷新未加到Container中的节点连线 Important
                        child.UpdateNodeAndLink();
                    }
                }
            }
            base.LoadLinks(xmlNode);
        }

        #region 代码生成

        public override string GCode_GetValueName(CodeGenerateSystem.Base.LinkControl element)
        {
            return "MethodReturnValue_" + EngineNS.Editor.Assist.GetValuedGUIDString(Id);
        }
        public override string GCode_GetValueType(CodeGenerateSystem.Base.LinkControl element)
        {
            return string.Empty;
        }

        public override Type GCode_GetType(CodeGenerateSystem.Base.LinkControl element)
        {
            return null;
        }

        Dictionary<string, System.CodeDom.CodeVariableDeclarationStatement> mParamDeclarationStatementsDic = new Dictionary<string, CodeVariableDeclarationStatement>();
        public override void GCode_CodeDom_GenerateCode(CodeTypeDeclaration codeClass, CodeStatementCollection codeStatementCollection, CodeGenerateSystem.Base.LinkControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            if (mMethodInfo == null)
            {
                base.GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, element, context);
                return;
            }

            // 保存节点Id和节点名字
            Editor.Runner.RunnerManager.Instance.LastRunnedNodeInfo = EngineNS.Editor.Assist.GetValuedGUIDString(this.Id) + this.NodeName;
            Editor.Runner.RunnerManager.Instance.CurrentId = this.Id;

            if (MemberParentNode != null && !MemberParentNode.IsOnlyReturnValue)
            {
                var tempContext = context.Copy();
                tempContext.GenerateNext = false;
                MemberParentNode?.GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, mHostUsefulMemberData.LinkObject?.LinkElement, tempContext);
            }


            //if (element == MethodLink_Pre)
            //{
            //    if (codeStatementCollection.Contains(mMethodInvokeStatment))
            //        return;

            //    // 参数
            //    foreach (var paramNode in mChildNodes)
            //    {
            //        if (paramNode is MethodInvokeParameterControl)
            //        {
            //            var paramCtrl = paramNode as MethodInvokeParameterUnionControl;
            //            paramCtrl.GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, null, context);
            //        }
            //    }
            //}
        }

        public override CodeExpression GCode_CodeDom_GetValue(CodeGenerateSystem.Base.LinkControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            if (mMethodInfo == null)
                return base.GCode_CodeDom_GetValue(element, context);

            return base.GCode_CodeDom_GetValue(element, context);
        }
        #endregion
    }
    //////////////////////////////////////////////////////////////////////////////////
    public partial class MethodInvokeParameterUnionControl : CodeGenerateSystem.Base.BaseNodeControl
    {
        public CodeGenerateSystem.Base.LinkControl mParamEllipse = new CodeGenerateSystem.Base.LinkControl();
        public CodeGenerateSystem.Base.LinkControl ParamEllipse
        {
            get { return mParamEllipse; }
        }
        Type mParamType;
        public Type ParamType
        {
            get { return mParamType; }
        }

        CodeGenerateSystem.Base.enLinkType mParamELinkType = CodeGenerateSystem.Base.enLinkType.All;
        string mParamValue = "";

        string mParamName;
        public string ParamName
        {
            get { return mParamName; }
            set { mParamName = value; }
        }
        string mParamFlag = "";
        public string ParamFlag
        {
            get { return mParamFlag; }
        }
        bool mIsGenericType = false;
        partial void InitConstruction();
        public MethodInvokeParameterUnionControl(CodeGenerateSystem.Base.ConstructionParams csParam, string param)
            : base(csParam, param)
        {
            InitConstruction();

            string[] splits = param.Split(':');
            mParamValue = splits[0];
            mParamName = splits[1];

            if (splits[2].StartsWith("@"))
            {
                string[] splitsType = splits[2].Split('#');

                string[] linkTypeSplits = splitsType[1].Split(',');
                foreach (var linkType in linkTypeSplits)
                {
                    var str = linkType.Replace(" ", "");
                    if (mParamELinkType == CodeGenerateSystem.Base.enLinkType.All)
                    {
                        mParamELinkType = CodeGenerateSystem.Base.LinkObjInfo.GetLinkTypeFromTypeString(str);
                    }
                    else
                    {
                        mParamELinkType |= CodeGenerateSystem.Base.LinkObjInfo.GetLinkTypeFromTypeString(str);
                    }
                }

                mParamType = EngineNS.Rtti.RttiHelper.GetTypeFromTypeFullName(splitsType[0].Remove(0, 1), csParam.CSType);
            }
            else
            {
                string[] linkTypeSplits = splits[2].Split(',');
                foreach (var linkType in linkTypeSplits)
                {
                    var str = linkType.Replace(" ", "");
                    if (mParamELinkType == CodeGenerateSystem.Base.enLinkType.All)
                    {
                        mParamELinkType = CodeGenerateSystem.Base.LinkObjInfo.GetLinkTypeFromTypeString(str);
                    }
                    else
                    {
                        mParamELinkType |= CodeGenerateSystem.Base.LinkObjInfo.GetLinkTypeFromTypeString(str);
                    }
                }
            }
            AddLinkObject(mParamELinkType, mParamEllipse, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, null, true);
        }
        public override string GetLinkObjParamInfo(CodeGenerateSystem.Base.LinkObjInfo linkObj)
        {
            string returnStr = "";

            if (linkObj.LinkElement == mParamEllipse)
            {
                returnStr += ":" + mParamName + ":" + "@" + mParamType + "#" + linkObj.LinkType.ToString();
            }
            else
            {
                returnStr += "::";
            }
            return returnStr;
        }
        public override void GCode_CodeDom_GenerateCode(System.CodeDom.CodeTypeDeclaration codeClass, System.CodeDom.CodeStatementCollection codeStatementCollection, CodeGenerateSystem.Base.LinkControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            var linkOI = GetLinkObjInfo(mParamEllipse);
            if (linkOI.HasLink)
            {
                int i = 0;
                for (; i < linkOI.LinkInfos.Count; i++)
                {
                    var linkInfo = linkOI.LinkInfos[i];
                    if (mbFromObject)
                    {
                        if (linkInfo.m_linkToObjectInfo == linkOI)
                        {
                            break;
                        }
                    }
                    else
                    {
                        if (linkInfo.m_linkFromObjectInfo == linkOI)
                        {
                            break;
                        }
                    }
                }

                if (!linkOI.GetLinkObject(i, mbFromObject).IsOnlyReturnValue)
                    linkOI.GetLinkObject(i, mbFromObject).GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, linkOI.GetLinkElement(i, mbFromObject), context);
            }
        }

        public override System.CodeDom.CodeExpression GCode_CodeDom_GetValue(CodeGenerateSystem.Base.LinkControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            var linkOI = GetLinkObjInfo(mParamEllipse);
            if (linkOI.HasLink)
            {
                int i = 0;
                for (; i < linkOI.LinkInfos.Count; i++)
                {
                    var linkInfo = linkOI.LinkInfos[i];
                    if (mbFromObject)
                    {
                        if (linkInfo.m_linkToObjectInfo == linkOI)
                        {
                            break;
                        }
                    }
                    else
                    {
                        if (linkInfo.m_linkFromObjectInfo == linkOI)
                        {
                            break;
                        }
                    }
                }
                return linkOI.GetLinkObject(i, mbFromObject).GCode_CodeDom_GetValue(linkOI.GetLinkElement(i, mbFromObject), context);
            }
            return null;
        }

        public override string GCode_GetValueType(CodeGenerateSystem.Base.LinkControl element)
        {
            var linkOI = GetLinkObjInfo(mParamEllipse);
            if (linkOI.HasLink)
            {
                int i = 0;
                for (; i < linkOI.LinkInfos.Count; i++)
                {
                    var linkInfo = linkOI.LinkInfos[i];
                    if (mbFromObject)
                    {
                        if (linkInfo.m_linkToObjectInfo == linkOI)
                        {
                            break;
                        }
                    }
                    else
                    {
                        if (linkInfo.m_linkFromObjectInfo == linkOI)
                        {
                            break;
                        }
                    }
                }
                return linkOI.GetLinkObject(i, mbFromObject).GCode_GetValueType(linkOI.GetLinkElement(i, mbFromObject));
            }
            else if (!mIsGenericType)
            {
                return ParamType?.FullName;
            }

            return base.GCode_GetValueType(element);
        }

        public override Type GCode_GetType(CodeGenerateSystem.Base.LinkControl element)
        {
            var linkOI = GetLinkObjInfo(mParamEllipse);
            if (linkOI.HasLink)
            {
                int i = 0;
                for (; i < linkOI.LinkInfos.Count; i++)
                {
                    var linkInfo = linkOI.LinkInfos[i];
                    if (mbFromObject)
                    {
                        if (linkInfo.m_linkToObjectInfo == linkOI)
                        {
                            break;
                        }
                    }
                    else
                    {
                        if (linkInfo.m_linkFromObjectInfo == linkOI)
                        {
                            break;
                        }
                    }
                }
                return linkOI.GetLinkObject(i, mbFromObject).GCode_GetType(linkOI.GetLinkElement(i, mbFromObject));
            }
            else if (!mIsGenericType)
            {
                return ParamType;
            }

            return base.GCode_GetType(element);
        }
    }
    ////////////////////////////////////////////////////////////////////////////////////////////
    public partial class MethodInvokeResultUnionControl : CodeGenerateSystem.Base.BaseNodeControl
    {
        public CodeGenerateSystem.Base.LinkControl mResultMethod = new CodeGenerateSystem.Base.LinkControl();
        public CodeGenerateSystem.Base.LinkControl ResultMethod
        {
            get { return mResultMethod; }
        }
        Type mParamType;
        public Type ParamType
        {
            get { return mParamType; }
        }

        CodeGenerateSystem.Base.enLinkType mParamELinkType = CodeGenerateSystem.Base.enLinkType.All;
        string mParamValue = "";

        string mParamName;
        public string ParamName
        {
            get { return mParamName; }
            set { mParamName = value; }
        }
        string mParamFlag = "";
        public string ParamFlag
        {
            get { return mParamFlag; }
        }
        bool mIsGenericType = false;
        partial void InitConstruction();
        public MethodInvokeResultUnionControl(CodeGenerateSystem.Base.ConstructionParams csParam, string param)
            : base(csParam, param)
        {
            InitConstruction();
            string[] splits = param.Split(':');
            mParamValue = splits[0];
            mParamName = splits[1];
            if (splits[2].StartsWith("@"))
            {
                string[] splitsType = splits[2].Split('#');

                string[] linkTypeSplits = splitsType[1].Split(',');
                foreach (var linkType in linkTypeSplits)
                {
                    var str = linkType.Replace(" ", "");
                    if (mParamELinkType == CodeGenerateSystem.Base.enLinkType.All)
                    {
                        mParamELinkType = CodeGenerateSystem.Base.LinkObjInfo.GetLinkTypeFromTypeString(str);
                    }
                    else
                    {
                        mParamELinkType |= CodeGenerateSystem.Base.LinkObjInfo.GetLinkTypeFromTypeString(str);
                    }
                }

                mParamType = EngineNS.Rtti.RttiHelper.GetTypeFromTypeFullName(splitsType[0].Remove(0, 1), csParam.CSType);
            }
            else
            {
                string[] linkTypeSplits = splits[2].Split(',');
                foreach (var linkType in linkTypeSplits)
                {
                    var str = linkType.Replace(" ", "");
                    if (mParamELinkType == CodeGenerateSystem.Base.enLinkType.All)
                    {
                        mParamELinkType = CodeGenerateSystem.Base.LinkObjInfo.GetLinkTypeFromTypeString(str);
                    }
                    else
                    {
                        mParamELinkType |= CodeGenerateSystem.Base.LinkObjInfo.GetLinkTypeFromTypeString(str);
                    }
                }
            }
            AddLinkObject(mParamELinkType, mResultMethod, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, null, true);
        }
        public override string GetLinkObjParamInfo(CodeGenerateSystem.Base.LinkObjInfo linkObj)
        {
            string returnStr = "";

            if (linkObj.LinkElement == mResultMethod)
            {
                returnStr += ":" + mParamName + ":" + "@" + mParamType + "#" + linkObj.LinkType.ToString();
            }
            else
            {
                returnStr += "::";
            }
            return returnStr;
        }
        public override void GCode_CodeDom_GenerateCode(System.CodeDom.CodeTypeDeclaration codeClass, System.CodeDom.CodeStatementCollection codeStatementCollection, CodeGenerateSystem.Base.LinkControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            var linkOI = GetLinkObjInfo(mResultMethod);
            if (linkOI.HasLink)
            {
                int i = 0;
                for (; i < linkOI.LinkInfos.Count; i++)
                {
                    var linkInfo = linkOI.LinkInfos[i];
                    if (mbFromObject)
                    {
                        if (linkInfo.m_linkToObjectInfo == linkOI)
                        {
                            break;
                        }
                    }
                    else
                    {
                        if (linkInfo.m_linkFromObjectInfo == linkOI)
                        {
                            break;
                        }
                    }
                }
                if (!linkOI.GetLinkObject(i, mbFromObject).IsOnlyReturnValue)
                    linkOI.GetLinkObject(i, mbFromObject).GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, linkOI.GetLinkElement(i, mbFromObject), context);
            }
        }

        public override System.CodeDom.CodeExpression GCode_CodeDom_GetValue(CodeGenerateSystem.Base.LinkControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            var linkOI = GetLinkObjInfo(mResultMethod);
            if (linkOI.HasLink)
            {
                int i = 0;
                for (; i < linkOI.LinkInfos.Count; i++)
                {
                    var linkInfo = linkOI.LinkInfos[i];
                    if (mbFromObject)
                    {
                        if (linkInfo.m_linkToObjectInfo == linkOI)
                        {
                            break;
                        }
                    }
                    else
                    {
                        if (linkInfo.m_linkFromObjectInfo == linkOI)
                        {
                            break;
                        }
                    }
                }
                return linkOI.GetLinkObject(i, mbFromObject).GCode_CodeDom_GetValue(linkOI.GetLinkElement(i, mbFromObject), context);
            }
            return null;
        }

        public override string GCode_GetValueType(CodeGenerateSystem.Base.LinkControl element)
        {
            var linkOI = GetLinkObjInfo(mResultMethod);
            if (linkOI.HasLink)
            {
                int i = 0;
                for (; i < linkOI.LinkInfos.Count; i++)
                {
                    var linkInfo = linkOI.LinkInfos[i];
                    if (mbFromObject)
                    {
                        if (linkInfo.m_linkToObjectInfo == linkOI)
                        {
                            break;
                        }
                    }
                    else
                    {
                        if (linkInfo.m_linkFromObjectInfo == linkOI)
                        {
                            break;
                        }
                    }
                }
                return linkOI.GetLinkObject(i, mbFromObject).GCode_GetValueType(linkOI.GetLinkElement(i, mbFromObject));
            }
            else if (!mIsGenericType)
            {
                return ParamType?.FullName;
            }

            return base.GCode_GetValueType(element);
        }

        public override Type GCode_GetType(CodeGenerateSystem.Base.LinkControl element)
        {
            var linkOI = GetLinkObjInfo(mResultMethod);
            if (linkOI.HasLink)
            {
                int i = 0;
                for (; i < linkOI.LinkInfos.Count; i++)
                {
                    var linkInfo = linkOI.LinkInfos[i];
                    if (mbFromObject)
                    {
                        if (linkInfo.m_linkToObjectInfo == linkOI)
                        {
                            break;
                        }
                    }
                    else
                    {
                        if (linkInfo.m_linkFromObjectInfo == linkOI)
                        {
                            break;
                        }
                    }
                }
                return linkOI.GetLinkObject(i, mbFromObject).GCode_GetType(linkOI.GetLinkElement(i, mbFromObject));
            }
            else if (!mIsGenericType)
            {
                return ParamType;
            }

            return base.GCode_GetType(element);
        }
    }
    //////////////////////////////////////////////////////////////////////////////////////
    public partial class MethodInUnionControl : CodeGenerateSystem.Base.BaseNodeControl
    {
        public CodeGenerateSystem.Base.LinkControl mParamsMethod = new CodeGenerateSystem.Base.LinkControl();
        public CodeGenerateSystem.Base.LinkControl ParamsMethod
        {
            get { return mParamsMethod; }
        }
        //////Type mParamType;
        //////public Type ParamType
        //////{
        //////    get { return mParamType; }
        //////}
        string mParamName;
        public string ParamName
        {
            get { return mParamName; }
            set { mParamName = value; }
        }
        string mParamFlag = "";
        public string ParamFlag
        {
            get { return mParamFlag; }
        }
        //////bool mIsGenericType = false;
        partial void InitConstruction();
        public MethodInUnionControl(CodeGenerateSystem.Base.ConstructionParams csParam, string param)
            : base(csParam, param)
        {
            InitConstruction();
            AddLinkObject(CodeGenerateSystem.Base.enLinkType.Method, mParamsMethod, CodeGenerateSystem.Base.enBezierType.Top, CodeGenerateSystem.Base.enLinkOpType.End, null, true);
        }
        public override void GCode_CodeDom_GenerateCode(System.CodeDom.CodeTypeDeclaration codeClass, System.CodeDom.CodeStatementCollection codeStatementCollection, CodeGenerateSystem.Base.LinkControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            var linkOI = GetLinkObjInfo(mParamsMethod);
            if (linkOI.HasLink)
            {
                int i = 0;
                for (; i < linkOI.LinkInfos.Count; i++)
                {
                    var linkInfo = linkOI.LinkInfos[i];
                    if (linkInfo.m_linkFromObjectInfo == linkOI)
                    {
                        break;
                    }
                }
                if (!linkOI.GetLinkObject(i, false).IsOnlyReturnValue)
                    linkOI.GetLinkObject(i, false).GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, linkOI.GetLinkElement(i, false), context);
            }
        }

        public override System.CodeDom.CodeExpression GCode_CodeDom_GetValue(CodeGenerateSystem.Base.LinkControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            var linkOI = GetLinkObjInfo(mParamsMethod);
            if (linkOI.HasLink)
            {
                int i = 0;
                for (; i < linkOI.LinkInfos.Count; i++)
                {
                    var linkInfo = linkOI.LinkInfos[i];
                    if (linkInfo.m_linkFromObjectInfo == linkOI)
                    {
                        break;
                    }
                }
                return linkOI.GetLinkObject(i, false).GCode_CodeDom_GetValue(linkOI.GetLinkElement(i, false), context);
            }
            return null;
        }

        public override string GCode_GetValueType(CodeGenerateSystem.Base.LinkControl element)
        {
            var linkOI = GetLinkObjInfo(mParamsMethod);
            if (linkOI.HasLink)
            {
                int i = 0;
                for (; i < linkOI.LinkInfos.Count; i++)
                {
                    var linkInfo = linkOI.LinkInfos[i];
                    if (linkInfo.m_linkFromObjectInfo == linkOI)
                    {
                        break;
                    }
                }
                return linkOI.GetLinkObject(i, false).GCode_GetValueType(linkOI.GetLinkElement(i, false));
            }
            //////else if (!mIsGenericType)
            //////{
            //////    return ParamType?.FullName;
            //////}

            return base.GCode_GetValueType(element);
        }

        public override Type GCode_GetType(CodeGenerateSystem.Base.LinkControl element)
        {
            var linkOI = GetLinkObjInfo(mParamsMethod);
            if (linkOI.HasLink)
            {
                int i = 0;
                for (; i < linkOI.LinkInfos.Count; i++)
                {
                    var linkInfo = linkOI.LinkInfos[i];
                    if (linkInfo.m_linkFromObjectInfo == linkOI)
                    {
                        break;
                    }
                }
                return linkOI.GetLinkObject(i, false).GCode_GetType(linkOI.GetLinkElement(i, false));
            }
            //////else if (!mIsGenericType)
            //////{
            //////    return ParamType;
            //////}

            return base.GCode_GetType(element);
        }
    }
    //////////////////////////////////////////////////////////////////////////////////
    public partial class MethodOutUnionControl : CodeGenerateSystem.Base.BaseNodeControl
    {
        public CodeGenerateSystem.Base.LinkControl mResultMethod;
        public CodeGenerateSystem.Base.LinkControl ResultMethod
        {
            get { return mResultMethod; }
        }
        //////Type mParamType;
        //////public Type ParamType
        //////{
        //////    get { return mParamType; }
        //////}
        string mParamName;
        public string ParamName
        {
            get { return mParamName; }
            set { mParamName = value; }
        }
        string mParamFlag = "";
        public string ParamFlag
        {
            get { return mParamFlag; }
        }
        //////bool mIsGenericType = false;
        partial void InitConstruction();
        public MethodOutUnionControl(CodeGenerateSystem.Base.ConstructionParams csParam, string param)
            : base(csParam, param)
        {
            InitConstruction();
            AddLinkObject(CodeGenerateSystem.Base.enLinkType.Method, mResultMethod, CodeGenerateSystem.Base.enBezierType.Bottom, CodeGenerateSystem.Base.enLinkOpType.Start, null, true);
        }
        public override void GCode_CodeDom_GenerateCode(System.CodeDom.CodeTypeDeclaration codeClass, System.CodeDom.CodeStatementCollection codeStatementCollection, CodeGenerateSystem.Base.LinkControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            var linkOI = GetLinkObjInfo(mResultMethod);
            if (linkOI.HasLink)
            {
                int i = 0;
                for (; i < linkOI.LinkInfos.Count; i++)
                {
                    var linkInfo = linkOI.LinkInfos[i];
                    if (linkInfo.m_linkFromObjectInfo == linkOI)
                    {
                        break;
                    }
                }
                if (!linkOI.GetLinkObject(i, false).IsOnlyReturnValue)
                    linkOI.GetLinkObject(i, false).GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, linkOI.GetLinkElement(i, false), context);
            }
        }

        public override System.CodeDom.CodeExpression GCode_CodeDom_GetValue(CodeGenerateSystem.Base.LinkControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            var linkOI = GetLinkObjInfo(mResultMethod);
            if (linkOI.HasLink)
            {
                int i = 0;
                for (; i < linkOI.LinkInfos.Count; i++)
                {
                    var linkInfo = linkOI.LinkInfos[i];
                    if (linkInfo.m_linkFromObjectInfo == linkOI)
                    {
                        break;
                    }
                }
                return linkOI.GetLinkObject(i, false).GCode_CodeDom_GetValue(linkOI.GetLinkElement(i, false), context);
            }
            return null;
        }

        public override string GCode_GetValueType(CodeGenerateSystem.Base.LinkControl element)
        {
            var linkOI = GetLinkObjInfo(mResultMethod);
            if (linkOI.HasLink)
            {
                int i = 0;
                for (; i < linkOI.LinkInfos.Count; i++)
                {
                    var linkInfo = linkOI.LinkInfos[i];
                    if (linkInfo.m_linkFromObjectInfo == linkOI)
                    {
                        break;
                    }
                }
                return linkOI.GetLinkObject(i, false).GCode_GetValueType(linkOI.GetLinkElement(i, false));
            }
            //////else if (!mIsGenericType)
            //////{
            //////    return ParamType?.FullName;
            //////}

            return base.GCode_GetValueType(element);
        }

        public override Type GCode_GetType(CodeGenerateSystem.Base.LinkControl element)
        {
            var linkOI = GetLinkObjInfo(mResultMethod);
            if (linkOI.HasLink)
            {
                int i = 0;
                for (; i < linkOI.LinkInfos.Count; i++)
                {
                    var linkInfo = linkOI.LinkInfos[i];
                    if (linkInfo.m_linkFromObjectInfo == linkOI)
                    {
                        break;
                    }
                }
                return linkOI.GetLinkObject(i, false).GCode_GetType(linkOI.GetLinkElement(i, false));
            }
            //////else if (!mIsGenericType)
            //////{
            //////    return ParamType;
            //////}

            return base.GCode_GetType(element);
        }
    }
}
