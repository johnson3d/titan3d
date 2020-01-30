using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Text;
using CodeGenerateSystem.Base;

namespace CodeDomNode
{
    [CodeGenerateSystem.CustomConstructionParams(typeof(MethodInvokeParameterConstructionParams))]
    public partial class MethodInvokeParameterControl : CodeGenerateSystem.Base.BaseNodeControl
    {
        [EngineNS.Rtti.MetaClass]
        public class MethodInvokeParameterConstructionParams : CodeGenerateSystem.Base.ConstructionParams
        {
            [EngineNS.Rtti.MetaData]
            public CodeDomNode.MethodInvokeNode.enParamConstructType ConstructType
            {
                get;
                set;
            } = CodeDomNode.MethodInvokeNode.enParamConstructType.MethodInvoke;
            [EngineNS.Rtti.MetaData]
            public CodeDomNode.MethodParamInfoAssist ParamInfo
            {
                get;
                set;
            }
            public MethodInvokeParameterConstructionParams()
            {
            }

            public override EditorCommon.CodeGenerateSystem.INodeConstructionParams Duplicate()
            {
                var retVal = base.Duplicate() as MethodInvokeParameterConstructionParams;
                retVal.ConstructType = ConstructType;
                retVal.ParamInfo = ParamInfo;
                return retVal;
            }

            public override bool Equals(object obj)
            {
                if (!base.Equals(obj))
                    return false;
                var param = obj as MethodInvokeParameterConstructionParams;
                if (param == null)
                    return false;
                if ((ConstructType == param.ConstructType) &&
                    (ParamInfo == param.ParamInfo))
                    return true;
                return false;
            }

            public override int GetHashCode()
            {
                return (base.GetHashCodeString() + ConstructType.ToString() + ParamInfo.ToString()).GetHashCode();
            }

            //public override void Write(XndNode xndNode)
            //{
            //    var att = xndNode.AddAttrib("ConstructionParams");
            //    att.Version = 0;
            //    att.BeginWrite();
            //    att.Write(ConstructParam);
            //    byte consType = (byte)ConstructType;
            //    att.Write(consType);
            //    att.WriteMetaObject(ParamInfo);
            //    att.EndWrite();
            //}
            public override void Read(EngineNS.IO.XndNode xndNode)
            {
                var att = xndNode.FindAttrib("ConstructionParams");
                if (att != null)
                {
                    att.BeginRead();
                    switch (att.Version)
                    {
                        case 0:
                            att.Read(out mConstructParam);
                            byte consType;
                            att.Read(out consType);
                            ConstructType = (CodeDomNode.MethodInvokeNode.enParamConstructType)consType;
                            if (ParamInfo == null)
                                ParamInfo = new CodeDomNode.MethodParamInfoAssist();
                            att.ReadMetaObject(ParamInfo);
                            break;
                        case 1:
                            att.ReadMetaObject(this);
                            break;
                    }
                    att.EndRead();
                }
            }
        }

        CodeGenerateSystem.Base.LinkPinControl mParamPin = new CodeGenerateSystem.Base.LinkPinControl();
        public CodeGenerateSystem.Base.LinkPinControl ParamPin
        {
            get { return mParamPin; }
        }


        public Type ParamType
        {
            get
            {
                var param = (MethodInvokeParameterConstructionParams)CSParam;
                if (param.ParamInfo.ParameterDisplayType != null)
                    return param.ParamInfo.ParameterDisplayType;
                else
                    return param.ParamInfo.ParameterType;
            }
        }
        System.CodeDom.FieldDirection mParamFlag = FieldDirection.In;
        public FieldDirection ParamFlag
        {
            get { return mParamFlag; }
        }
        bool mIsGenericType
        {
            get => ((MethodInvokeParameterConstructionParams)CSParam).ParamInfo.ParameterType.IsGenericType;
        }

        partial void InitConstruction();
        public MethodInvokeParameterControl(CodeGenerateSystem.Base.ConstructionParams csParam)
            : base(csParam)
        {
            //EngineNS.ECSType csType = EngineNS.ECSType.All;
            //if (HostNodesContainer != null)
            //    csType = HostNodesContainer.mCSType;
            NodeType = enNodeType.ChildNode;

            var param = csParam as MethodInvokeParameterConstructionParams;
            switch (param.ConstructType)
            {
                case CodeDomNode.MethodInvokeNode.enParamConstructType.MethodInvoke:
                case CodeDomNode.MethodInvokeNode.enParamConstructType.Return:
                case MethodInvokeNode.enParamConstructType.ReturnCustom:
                    break;
                case CodeDomNode.MethodInvokeNode.enParamConstructType.MethodOverride:
                case CodeDomNode.MethodInvokeNode.enParamConstructType.MethodCustom:
                case CodeDomNode.MethodInvokeNode.enParamConstructType.Delegate:
                case MethodInvokeNode.enParamConstructType.MethodCustomInvoke_In:
                case MethodInvokeNode.enParamConstructType.MethodCustomInvoke_Out:
                case MethodInvokeNode.enParamConstructType.MethodInvoke_Out:
                    IsOnlyReturnValue = true;
                    break;
                default:
                    throw new InvalidOperationException();
            }

            mParamFlag = param.ParamInfo.FieldDirection;

            InitConstruction();
            var linkInfo = AddLinkPinInfo("ParamPin", mParamPin);
            linkInfo.OnAddLinkInfo += OnParamLinkAddLinkInfo;
            linkInfo.OnDelLinkInfo += OnParamLinkDelLinkInfo;
        }
        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            var param = smParam as MethodInvokeParameterConstructionParams;

            var paramType = param.ParamInfo.ParameterType;
            if (param.ParamInfo.ParameterDisplayType != null)
                paramType = param.ParamInfo.ParameterDisplayType;
            bool isGenericType = paramType.IsGenericType;
            switch (param.ConstructType)
            {
                case CodeDomNode.MethodInvokeNode.enParamConstructType.MethodInvoke:
                case CodeDomNode.MethodInvokeNode.enParamConstructType.Return:
                case CodeDomNode.MethodInvokeNode.enParamConstructType.ReturnCustom:
                case CodeDomNode.MethodInvokeNode.enParamConstructType.Delegate:
                case MethodInvokeNode.enParamConstructType.MethodCustomInvoke_In:
                    if (isGenericType)
                    {
                        if (paramType.BaseType == typeof(System.Threading.Tasks.Task))
                        {
                            var genericType = paramType.GetGenericArguments()[0];
                            CollectLinkPinInfo(smParam, "ParamPin", genericType, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, true);
                        }
                        else
                            CollectLinkPinInfo(smParam, "ParamPin", paramType, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
                    }
                    else
                        CollectLinkPinInfo(smParam, "ParamPin", paramType, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
                    break;
                case CodeDomNode.MethodInvokeNode.enParamConstructType.MethodOverride:
                case CodeDomNode.MethodInvokeNode.enParamConstructType.MethodCustom:
                case MethodInvokeNode.enParamConstructType.MethodCustomInvoke_Out:
                case MethodInvokeNode.enParamConstructType.MethodInvoke_Out:
                    if (isGenericType)
                    {
                        if (paramType.BaseType == typeof(System.Threading.Tasks.Task))
                        {
                            var genericType = paramType.GetGenericArguments()[0];
                            CollectLinkPinInfo(smParam, "ParamPin", genericType, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, true);
                        }
                        else
                            CollectLinkPinInfo(smParam, "ParamPin", paramType, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, true);
                    }
                    else
                        CollectLinkPinInfo(smParam, "ParamPin", paramType, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, true);
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }

        partial void ResetParamInfo_WPF(CodeDomNode.CustomMethodInfo.FunctionParam param, FieldDirection dir);
        public void ResetParamInfo(CodeDomNode.CustomMethodInfo.FunctionParam param, FieldDirection dir)
        {
            var info = param.CreateParamInfoAssist(dir);
            param.OnParamTypeChanged -= UpdateParamType;
            param.OnParamTypeChanged += UpdateParamType;
            var csParam = CSParam as MethodInvokeParameterConstructionParams;
            csParam.ParamInfo = info;
            ResetParamInfo_WPF(param, dir);
        }

        public Action<MethodInvokeParameterControl, CodeDomNode.VariableType> OnUpdateParamTypeAction;
        partial void UpdateParamType_WPF(Type newType);

        public void UpdateParamType(Type type, bool onlyChangeDisplayType)
        {
            var param = CSParam as MethodInvokeParameterConstructionParams;
            if (onlyChangeDisplayType)
                param.ParamInfo.ParameterDisplayType = type;
            else
                param.ParamInfo.ParameterType = type;
            var paramType = param.ParamInfo.ParameterType;
            if (param.ParamInfo.ParameterDisplayType != null)
                paramType = param.ParamInfo.ParameterDisplayType;
            switch (param.ConstructType)
            {
                case CodeDomNode.MethodInvokeNode.enParamConstructType.MethodInvoke:
                case CodeDomNode.MethodInvokeNode.enParamConstructType.Return:
                case CodeDomNode.MethodInvokeNode.enParamConstructType.ReturnCustom:
                case CodeDomNode.MethodInvokeNode.enParamConstructType.Delegate:
                case MethodInvokeNode.enParamConstructType.MethodCustomInvoke_In:
                    if (paramType.IsGenericType)
                        CollectLinkPinInfo(param, "ParamPin", CodeGenerateSystem.Base.enLinkType.All, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false, true);
                    else
                        CollectLinkPinInfo(param, "ParamPin", paramType, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false, true);
                    break;
                case CodeDomNode.MethodInvokeNode.enParamConstructType.MethodOverride:
                case CodeDomNode.MethodInvokeNode.enParamConstructType.MethodCustom:
                case MethodInvokeNode.enParamConstructType.MethodCustomInvoke_Out:
                case MethodInvokeNode.enParamConstructType.MethodInvoke_Out:
                    if (paramType.IsGenericType)
                        CollectLinkPinInfo(param, "ParamPin", CodeGenerateSystem.Base.enLinkType.All, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, true, true);
                    else
                        CollectLinkPinInfo(param, "ParamPin", paramType, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, true, true);
                    break;
                default:
                    throw new InvalidOperationException();
            }
            var pinDesc = GetLinkPinDesc(param, "ParamPin");
            ParamPin.LinkType = pinDesc.PinType;
            ParamPin.ClassType = pinDesc.ClassType;

            UpdateParamType_WPF(type);
        }
        public void UpdateParamType(CodeDomNode.VariableType newType)
        {
            if (ParamPin.HasLink)
            {
                ParamPin.Clear();
            }

            var desc = GetLinkPinDesc(CSParam, "ParamPin");
            var type = newType.GetActualType();
            UpdateParamType(type, false);
            OnUpdateParamTypeAction?.Invoke(this, newType);
        }

        partial void OnParamLinkAddLinkInfo_WPF(CodeGenerateSystem.Base.LinkInfo info);
        void OnParamLinkAddLinkInfo(CodeGenerateSystem.Base.LinkInfo info)
        {
            if (!HostNodesContainer.IsLoading)
            {
                base.AfterLink();
                var param = CSParam as MethodInvokeParameterConstructionParams;
                switch (param.ConstructType)
                {
                    case CodeDomNode.MethodInvokeNode.enParamConstructType.Delegate:
                        {
                            // 检查类型
                            if (param.ParamInfo.ParameterType == typeof(object))
                            {
                                var type = info.m_linkFromObjectInfo.HostNodeControl.GCode_GetType(info.m_linkFromObjectInfo, null);
                                if (type != typeof(object))
                                {
                                    param.ParamInfo.ParameterType = type;
                                }
                                ParamPin.ClassType = EngineNS.Rtti.RttiHelper.GetAppTypeString(type);
                                ParamPin.LinkType = CodeGenerateSystem.Base.LinkPinControl.GetLinkTypeFromCommonType(type);
                            }
                            param.ParamInfo.ParamName = info.m_linkFromObjectInfo.HostNodeControl.GCode_GetValueName(info.m_linkFromObjectInfo, null);
                        }
                        break;
                }
            }
            OnParamLinkAddLinkInfo_WPF(info);
        }
        partial void OnParamLinkDelLinkInfo_WPF(CodeGenerateSystem.Base.LinkInfo info);
        void OnParamLinkDelLinkInfo(CodeGenerateSystem.Base.LinkInfo info)
        {
            if (!HostNodesContainer.IsLoading)
            {
                base.BreakLink();
                var param = CSParam as MethodInvokeParameterConstructionParams;
                switch (param.ConstructType)
                {
                    case MethodInvokeNode.enParamConstructType.Delegate:
                        {
                            param.ParamInfo.ParameterType = typeof(object);
                            param.ParamInfo.ParamName = "";
                            ParamPin.ClassType = "System.Object";
                            ParamPin.LinkType = CodeGenerateSystem.Base.enLinkType.All;
                        }
                        break;
                }
            }
            OnParamLinkDelLinkInfo_WPF(info);
        }

        #region 代码生成

        public CodeExpression TempParamName;
        // 有displayType的out等参数需要类型一致才能进行函数调用，这里缓存与函数参数类型一致的参数
        public string TempDisplayWithTypeParamName;

        public bool IsUseLinkFromParamName()
        {
            if (!mParamPin.HasLink)
                return false;
            if (mParamPin.LinkOpType == CodeGenerateSystem.Base.enLinkOpType.Start)
                return false;
            var linkObj = mParamPin.GetLinkedObject(0);
            if (linkObj == null)
                return false;
            return linkObj.Pin_UseOrigionParamName(mParamPin.GetLinkedPinControl(0));
        }
        public override bool Pin_UseOrigionParamName(LinkPinControl linkElement)
        {
            if(ParentNode is MethodOverride || ParentNode is MethodCustom)
            {
                switch (ParamFlag)
                {
                    case FieldDirection.Out:
                    case FieldDirection.Ref:
                        return true;
                    default:
                        return false;
                }
            }
            return false;
        }
        public bool HasLink()
        {
            if (mParamPin == null)
                return false;
            return mParamPin.HasLink;
        }

        public bool NeedCopyParam()
        {
            if (!HasLink())
                return false;
            if (IsUseLinkFromParamName())
                return false;
            var param = CSParam as MethodInvokeParameterConstructionParams;
            if (param.ConstructType == MethodInvokeNode.enParamConstructType.MethodInvoke_Out)
                return false;
            switch (ParamFlag)
            {
                case FieldDirection.Ref:
                case FieldDirection.Out:
                    return true;
            }
            return false;
            //return true;
        }
        public CodeExpression GetLinkFromParamName(CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            if (!mParamPin.HasLink)
                return null;
            return mParamPin.GetLinkedObject(0, true).GCode_CodeDom_GetValue(mParamPin.GetLinkedPinControl(0, true), context);
        }

        //System.CodeDom.CodeStatement mVarDec;
        System.CodeDom.CodeStatement mTemplateClassStatement;
        void GenerateCode(Type paramType, System.CodeDom.CodeTypeDeclaration codeClass, System.CodeDom.CodeStatementCollection codeStatementCollection, CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            var param = CSParam as MethodInvokeParameterConstructionParams;
            var paramCodeName = "param_" + EngineNS.Editor.Assist.GetValuedGUIDString(ParentNode.Id) + "_" + param.ParamInfo.ParamName;

            if (paramType.IsPointer)
            {
                if (!context.ClassContext.PtrInvokeParamNames.Contains(paramCodeName))
                    context.ClassContext.PtrInvokeParamNames.Add(paramCodeName);
            }

            if (mTemplateClassStatement == null || !codeStatementCollection.Contains(mTemplateClassStatement))
            {
                if(ParentNode is CodeDomNode.MethodInvokeNode)
                {
                    var node = ParentNode as CodeDomNode.MethodInvokeNode;
                    if (node.TemplateClassInstance != null && (mParamFlag == FieldDirection.Ref || mParamFlag == FieldDirection.Out))
                    {
                        var proInfo = node.TemplateClassInstance.GetType().GetProperty(param.ParamInfo.ParamName);
                        object classValue;
                        if (proInfo == null)
                            classValue = CodeGenerateSystem.Program.GetDefaultValueFromType(paramType);
                        else
                            classValue = proInfo.GetValue(node.TemplateClassInstance);

                        mTemplateClassStatement = Program.GenerateAssignCode(codeStatementCollection, new CodeVariableReferenceExpression(paramCodeName), paramType, classValue);
                    }
                }
                else if(ParentNode is CodeDomNode.MethodCustomInvoke)
                {
                    var node = ParentNode as CodeDomNode.MethodCustomInvoke;
                    if(mParamFlag == FieldDirection.Out)
                    {
                        mTemplateClassStatement = new CodeVariableDeclarationStatement(paramType, paramCodeName);
                    }
                }
                else if(ParentNode is CodeDomNode.ReturnCustom)
                {
                    var node = ParentNode as CodeDomNode.ReturnCustom;
                    if(node.TemplateClassInstance != null)
                    {
                        var proInfo = node.TemplateClassInstance.GetType().GetProperty(param.ParamInfo.ParamName);
                        object classValue;
                        if (proInfo == null)
                            classValue = CodeGenerateSystem.Program.GetDefaultValueFromType(paramType);
                        else
                            classValue = proInfo.GetValue(node.TemplateClassInstance);
                        mTemplateClassStatement = Program.GenerateAssignCode(codeStatementCollection, new CodeVariableReferenceExpression(paramCodeName), paramType, classValue);
                    }
                }
            }
        }
        public override async System.Threading.Tasks.Task GCode_CodeDom_GenerateCode(System.CodeDom.CodeTypeDeclaration codeClass, System.CodeDom.CodeStatementCollection codeStatementCollection, CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            var param = CSParam as MethodInvokeParameterConstructionParams;
            if (mParamPin.HasLink)
            {
                if (!mParamPin.GetLinkedObject(0, true).IsOnlyReturnValue)
                    await mParamPin.GetLinkedObject(0, true).GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, mParamPin.GetLinkedPinControl(0, true), context);
            }
            else if (mIsGenericType)
            {
                if (ParamType.BaseType == typeof(System.Threading.Tasks.Task))
                {
                    var genericType = ParamType.GetGenericArguments()[0];
                    GenerateCode(genericType, codeClass, codeStatementCollection, element, context);
                }
                else
                {
                    GenerateCode(ParamType, codeClass, codeStatementCollection, element, context);
                    //throw new InvalidOperationException("未实现");
                }
            }
            else if(param.ParamInfo.ParameterType.IsGenericParameter)
            {
                var finalType = mParamPin.GetLinkedObject(0).GCode_GetType(mParamPin.GetLinkedPinControl(0), context);
                GenerateCode(finalType, codeClass, codeStatementCollection, element, context);
            }
            else
            {
                GenerateCode(ParamType, codeClass, codeStatementCollection, element, context);
            }
        }

        public override System.CodeDom.CodeExpression GCode_CodeDom_GetValue(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context, CodeGenerateSystem.Base.GenerateCodeContext_PreNode preNodeContext = null)
        {
            var param = CSParam as MethodInvokeParameterConstructionParams;
            if (mParamPin.LinkOpType == CodeGenerateSystem.Base.enLinkOpType.Start)
            {
                var name = GCode_GetValueName(element, context);

                if (preNodeContext?.NeedDereferencePoint == true && ParamType.IsPointer)
                    return new CodeVariableReferenceExpression($"(*{name})");
                else
                    return new CodeVariableReferenceExpression(name);
            }
            else
            {
                if (mParamPin.HasLink)
                {
                    return mParamPin.GetLinkedObject(0, true).GCode_CodeDom_GetValue(mParamPin.GetLinkedPinControl(0, true), context);
                    //System.CodeDom.FieldDirection fd = System.CodeDom.FieldDirection.In;
                    //switch (mParamFlag)
                    //{
                    //    case "Ref":
                    //        fd = System.CodeDom.FieldDirection.Ref;
                    //        break;
                    //    case "Out":
                    //        fd = System.CodeDom.FieldDirection.Out;
                    //        break;
                    //    default:
                    //        fd = System.CodeDom.FieldDirection.In;
                    //        break;
                    //}
                    //return new System.CodeDom.CodeDirectionExpression(fd, mParamPin.GetLinkObject(0, true).GCode_CodeDom_GetValue(mParamPin.GetLinkElement(0, true)));
                }
                else
                {
                    if (ParentNode is CodeDomNode.MethodInvokeNode ||
                        ParentNode is CodeDomNode.MethodCustomInvoke ||
                        ParentNode is CodeDomNode.Return ||
                        ParentNode is CodeDomNode.ReturnCustom)
                    {
                        var paramName = "param_" + EngineNS.Editor.Assist.GetValuedGUIDString(ParentNode.Id) + "_" + param.ParamInfo.ParamName;
                        return new CodeVariableReferenceExpression(paramName);
                    }
                    else if (ParentNode is CodeDomNode.MethodInvoke_DelegateControl)
                    {
                        throw new InvalidOperationException();
                    }
                    else
                        throw new InvalidOperationException();
                }
            }
        }

        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            if (ParamType.BaseType == typeof(System.Threading.Tasks.Task))
            {
                var genericType = ParamType.GetGenericArguments()[0];
                return EngineNS.Rtti.RttiHelper.GetAppTypeString(genericType);
            }
            else
            {
                return EngineNS.Rtti.RttiHelper.GetAppTypeString(ParamType);
            }
        }

        public override Type GCode_GetType(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            if (ParamType == typeof(System.Threading.Tasks.Task))
            {
                var genericType = ParamType.GetGenericArguments()[0];
                return genericType;
            }
            else if (ParamType.IsGenericParameter)
            {
                throw new InvalidOperationException("函数参数泛型处理");
            }
            else
            {
                return ParamType;
            }
        }
        public override string GCode_GetValueName(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            var param = CSParam as MethodInvokeParameterConstructionParams;
            if (mParamPin.LinkOpType == CodeGenerateSystem.Base.enLinkOpType.Start)
            {
                switch (param.ConstructType)
                {
                    case MethodInvokeNode.enParamConstructType.MethodInvoke_Out:
                        break;
                    case MethodInvokeNode.enParamConstructType.MethodCustomInvoke_Out:
                        break;
                    default:
                        return param.ParamInfo.ParamName;
                }
            }
            return "param_" + EngineNS.Editor.Assist.GetValuedGUIDString(ParentNode.Id) + "_" + param.ParamInfo.ParamName;
        }

        #endregion
    }
}
