using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using CodeGenerateSystem.Base;
using EngineNS.IO;

namespace CodeDomNode
{
    [CodeGenerateSystem.CustomConstructionParams(typeof(ReturnConstructParam))]
    public partial class Return : CodeGenerateSystem.Base.BaseNodeControl
    {
        [EngineNS.Rtti.MetaClass]
        public class ReturnConstructParam : CodeGenerateSystem.Base.ConstructionParams
        {
            [EngineNS.Rtti.MetaData]
            public MethodInfoAssist MethodInfo { get; set; }
            public ReturnConstructParam()
            {
            }
            public override EditorCommon.CodeGenerateSystem.INodeConstructionParams Duplicate()
            {
                var retVal = base.Duplicate() as ReturnConstructParam;
                retVal.MethodInfo = MethodInfo;
                return retVal;
            }
            public override bool Equals(object obj)
            {
                var param = obj as ReturnConstructParam;
                if (param == null)
                    return false;
                if (!base.Equals(obj))
                    return false;
                if (MethodInfo != param.MethodInfo)
                    return false;
                return true;
            }
            public override int GetHashCode()
            {
                return (base.GetHashCodeString() + MethodInfo.ToString()).GetHashCode();
            }
            //public override void Write(EngineNS.IO.XndNode xndNode)
            //{
            //    var att = xndNode.AddAttrib("ConstructionParams");
            //    att.Version = 0;
            //    att.BeginWrite();
            //    att.Write(ConstructParam);
            //    att.WriteMetaObject(MethodInfo);
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
                            if (MethodInfo == null)
                                MethodInfo = new MethodInfoAssist();
                            att.ReadMetaObject(MethodInfo);
                            break;
                        case 1:
                            att.ReadMetaObject(this);
                            break;
                    }
                    att.EndRead();
                }
            }
        }

        CodeGenerateSystem.Base.LinkPinControl mCtrlMethodLink = new CodeGenerateSystem.Base.LinkPinControl();

        // 临时类，用于选中后显示参数属性
        CodeGenerateSystem.Base.GeneratorClassBase mTemplateClassInstance = null;
        public CodeGenerateSystem.Base.GeneratorClassBase TemplateClassInstance
        {
            get { return mTemplateClassInstance; }
        }

        partial void InitConstruction();
        public Return(CodeGenerateSystem.Base.ConstructionParams csParam)
            : base(csParam)
        {
            InitConstruction();
            NodeName = "Return";

            var param = csParam as ReturnConstructParam;
            SetParameters(param.MethodInfo);

            AddLinkPinInfo("CtrlMethodLink", mCtrlMethodLink, null);
        }
        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            CollectLinkPinInfo(smParam, "CtrlMethodLink", CodeGenerateSystem.Base.enLinkType.Method, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, true);
        }

        StackPanel mParamsPanel = null;
        // 这里不做参数在PropertyGrid中的显示设置，防止类似Ref参数被无意设置
        protected void SetParameters(MethodInfoAssist methodInfo)
        {
            var cpInfos = new List<CustomPropertyInfo>();
            mParamsPanel?.Children.Clear();
            var param = CSParam as ReturnConstructParam;
            // return
            if((param.MethodInfo.ReturnType != typeof(void)) && (param.MethodInfo.ReturnType != typeof(System.Threading.Tasks.Task)))
            {
                var csParam = new MethodInvokeParameterControl.MethodInvokeParameterConstructionParams()
                {
                    CSType = CSParam.CSType,
                    HostNodesContainer = CSParam.HostNodesContainer,
                    ConstructParam = "",
                    ConstructType = MethodInvokeNode.enParamConstructType.Return,
                    ParamInfo = new MethodParamInfoAssist()
                    {
                        FieldDirection = System.CodeDom.FieldDirection.Out,
                        IsParamsArray = false,
                        ParameterType = param.MethodInfo.ReturnType,
                        ParamName = "Return",
                    },
                };
                var ctrl = new MethodInvokeParameterControl(csParam);
                AddChildNode(ctrl, mParamsPanel);

                if(Program.IsTypeValidInPropertyGridShow(param.MethodInfo.ReturnType))
                {
                    var cpInfo = Program.GetFromParamInfo(csParam.ParamInfo);
                    cpInfos.Add(cpInfo);
                }
            }

            foreach(var methodParam in methodInfo.Params)
            {
                if (methodParam.FieldDirection != System.CodeDom.FieldDirection.Out)
                    continue;

                CodeGenerateSystem.Base.BaseNodeControl pc = null;
                if(methodParam.ParameterType.IsSubclassOf(typeof(System.Delegate)))
                {
                    var csParam = new MethodInvoke_DelegateControl.MethodInvoke_DelegateControlConstructionParams()
                    {
                        CSType = CSParam.CSType,
                        HostNodesContainer = CSParam.HostNodesContainer,
                        ConstructParam = this.NodeName,
                        ConstructType = MethodInvokeNode.enParamConstructType.Return,
                        ParamInfo = methodParam,
                    };
                    pc = new MethodInvoke_DelegateControl(csParam);
                }
                else
                {
                    if(methodParam.IsParamsArray)
                    {
                        var csParam = new ParamParameterControl.ParamParameterConstructionParams()
                        {
                            CSType = CSParam.CSType,
                            HostNodesContainer = CSParam.HostNodesContainer,
                            ConstructParam = "",
                            ConstructType = MethodInvokeNode.enParamConstructType.Return,
                            ParamInfo = methodParam,
                        };
                        pc = new ParamParameterControl(csParam);
                    }
                    else
                    {
                        var csParam = new MethodInvokeParameterControl.MethodInvokeParameterConstructionParams()
                        {
                            CSType = CSParam.CSType,
                            HostNodesContainer = CSParam.HostNodesContainer,
                            ConstructParam = "",
                            ConstructType = MethodInvokeNode.enParamConstructType.Return,
                            ParamInfo = methodParam,
                        };
                        pc = new MethodInvokeParameterControl(csParam);

                        if(Program.IsTypeValidInPropertyGridShow(methodParam.ParameterType))
                        {
                            var cpInfo = Program.GetFromParamInfo(methodParam);
                            cpInfos.Add(cpInfo);
                        }
                    }
                }
                AddChildNode(pc, mParamsPanel);
            }

            CreateTemplateClass(cpInfos);
        }
        public void CreateTemplateClass(List<CodeGenerateSystem.Base.CustomPropertyInfo> cpInfos)
        {
            var csParam = CSParam as ReturnConstructParam;
            mTemplateClassInstance = CodeGenerateSystem.Base.PropertyClassGenerator.CreateClassInstanceFromCustomPropertys(cpInfos, this, $"Return_{csParam.MethodInfo.ParentClassType.FullName}.{csParam.MethodInfo.MethodName}", false);
            InitTemplateClass_WPF(cpInfos);
        }
        partial void InitTemplateClass_WPF(List<CodeGenerateSystem.Base.CustomPropertyInfo> propertys);

        public override async System.Threading.Tasks.Task GCode_CodeDom_GenerateCode(System.CodeDom.CodeTypeDeclaration codeClass, System.CodeDom.CodeStatementCollection codeStatementCollection, CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            MethodInvokeParameterControl retCtrl = null;
            Dictionary<BaseNodeControl, System.CodeDom.CodeExpression> paramCodeExps = new Dictionary<BaseNodeControl, System.CodeDom.CodeExpression>();
            foreach(var paramNode in mChildNodes)
            {
                if(paramNode is MethodInvokeParameterControl)
                {
                    var pm = paramNode as MethodInvokeParameterControl;
                    var param = pm.CSParam as MethodInvokeParameterControl.MethodInvokeParameterConstructionParams;
                    if (param.ParamInfo.ParamName == "Return")
                    {
                        retCtrl = pm;
                        continue;
                    }
                    if (pm.HasLink())
                    {
                        await pm.GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, pm.ParamPin, context);
                        var exp = new CodeGenerateSystem.CodeDom.CodeCastExpression(param.ParamInfo.ParameterType, pm.GCode_CodeDom_GetValue(pm.ParamPin, context));
                        paramCodeExps[paramNode] = exp;
                        codeStatementCollection.Add(new System.CodeDom.CodeAssignStatement(
                                                                new System.CodeDom.CodeVariableReferenceExpression(param.ParamInfo.ParamName),
                                                                exp));
                    }
                    else if(mTemplateClassInstance != null)
                    {
                        var proInfo = mTemplateClassInstance.GetType().GetProperty(param.ParamInfo.ParamName);
                        object proValue;
                        if (proInfo == null)
                            proValue = CodeGenerateSystem.Program.GetDefaultValueFromType(param.ParamInfo.ParameterType);
                        else
                            proValue = proInfo.GetValue(mTemplateClassInstance);
                        var valueExp = Program.GetValueCode(codeStatementCollection, param.ParamInfo.ParameterType, proValue);
                        paramCodeExps[paramNode] = valueExp;
                        codeStatementCollection.Add(new System.CodeDom.CodeAssignStatement(new System.CodeDom.CodeVariableReferenceExpression(param.ParamInfo.ParamName), valueExp));
                    }
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }

            if(retCtrl != null)
            {
                if (retCtrl.HasLink())
                    await retCtrl.GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, retCtrl.ParamPin, context);

                System.CodeDom.CodeExpression retExp = null;
                if(retCtrl.HasLink())
                {
                    var retStatement = new System.CodeDom.CodeMethodReturnStatement();
                    retExp = retCtrl.GCode_CodeDom_GetValue(retCtrl.ParamPin, context);
                    paramCodeExps[retCtrl] = retExp;
                }
                else if (mTemplateClassInstance != null)
                {
                    var param = retCtrl.CSParam as MethodInvokeParameterControl.MethodInvokeParameterConstructionParams;
                    var proInfo = mTemplateClassInstance.GetType().GetProperty("Return");
                    object proValue;
                    if (proInfo == null)
                        proValue = CodeGenerateSystem.Program.GetDefaultValueFromType(param.ParamInfo.ParameterType);
                    else
                        proValue = proInfo.GetValue(mTemplateClassInstance);
                    retExp = Program.GetValueCode(codeStatementCollection, param.ParamInfo.ParameterType, proValue);
                    paramCodeExps[retCtrl] = retExp;
                }

                #region Debug
                var debugCodes = CodeDomNode.BreakPoint.BeginMacrossDebugCodeStatments(codeStatementCollection);
                foreach(var paramNode in mChildNodes)
                {
                    if(paramNode is MethodInvokeParameterControl)
                    {
                        var pm = paramNode as MethodInvokeParameterControl;
                        System.CodeDom.CodeExpression exp;
                        if(paramCodeExps.TryGetValue(paramNode, out exp))
                            CodeDomNode.BreakPoint.GetGatherDataValueCodeStatement(debugCodes, pm.ParamPin.GetLinkPinKeyName(), exp, pm.GCode_GetTypeString(pm.ParamPin, context), context);
                    }
                    else
                    {
                        throw new InvalidOperationException("未实现");
                    }
                }
                var breakCondStatement = CodeDomNode.BreakPoint.BreakCodeStatement(codeClass, debugCodes, HostNodesContainer.GUID, Id);
                CodeDomNode.BreakPoint.EndMacrossDebugCodeStatements(codeStatementCollection, debugCodes);
                #endregion

                var returnParam = CSParam as ReturnConstructParam;
                foreach(var param in returnParam.MethodInfo.Params)
                {
                    // ref或out，需要将displayType造成的临时变量再赋给原函数参数
                    if ((param.FieldDirection == FieldDirection.Out || param.FieldDirection == FieldDirection.Ref) && param.ParameterDisplayType != null)
                    {
                        codeStatementCollection.Add(new CodeAssignStatement(new CodeVariableReferenceExpression("temp___" + param.ParamName), new CodeGenerateSystem.CodeDom.CodeCastExpression(param.ParameterType, new CodeVariableReferenceExpression(param.ParamName))));
                    }
                }
                if (returnParam.MethodInfo.ReturnType.BaseType == typeof(System.Threading.Tasks.Task))
                {
                    var genericType = returnParam.MethodInfo.ReturnType.GetGenericArguments()[0];
                    codeStatementCollection.Add(new System.CodeDom.CodeMethodReturnStatement(new CodeGenerateSystem.CodeDom.CodeCastExpression(genericType, retExp)));
                }
                else
                    codeStatementCollection.Add(new System.CodeDom.CodeMethodReturnStatement(new CodeGenerateSystem.CodeDom.CodeCastExpression(returnParam.MethodInfo.ReturnType, retExp)));
            }
            else
            {
                #region Debug
                var debugCodes = CodeDomNode.BreakPoint.BeginMacrossDebugCodeStatments(codeStatementCollection);
                var breakCondStatement = CodeDomNode.BreakPoint.BreakCodeStatement(codeClass, debugCodes, HostNodesContainer.GUID, Id);
                CodeDomNode.BreakPoint.EndMacrossDebugCodeStatements(codeStatementCollection, debugCodes);
                #endregion

                var returnParam = CSParam as ReturnConstructParam;
                foreach(var param in returnParam.MethodInfo.Params)
                {
                    // ref或out，需要将displayType造成的临时变量再赋给原函数参数
                    if ((param.FieldDirection == FieldDirection.Out || param.FieldDirection == FieldDirection.Ref) && param.ParameterDisplayType != null)
                    {
                        codeStatementCollection.Add(new CodeAssignStatement(new CodeVariableReferenceExpression("temp___" + param.ParamName), new CodeGenerateSystem.CodeDom.CodeCastExpression(param.ParameterType, new CodeVariableReferenceExpression(param.ParamName))));
                    }
                }
                codeStatementCollection.Add(new System.CodeDom.CodeMethodReturnStatement());
            }
        }

        public override void Save(XndNode xndNode, bool newGuid)
        {
            var att = xndNode.AddAttrib("defaultParam");
            att.Version = 1;
            att.BeginWrite();
            if (mTemplateClassInstance != null)
            {
                att.Write((Byte)1);
                CodeGenerateSystem.Base.PropertyClassGenerator.SaveClassInstanceProperties(mTemplateClassInstance, att);
            }
            else
                att.Write((Byte)0);
            att.EndWrite(); base.Save(xndNode, newGuid);
        }
        public override async System.Threading.Tasks.Task Load(XndNode xndNode)
        {
            try
            {
                await base.Load(xndNode);
                var att = xndNode.FindAttrib("defaultParam");
                if (att != null)
                {
                    att.BeginRead();
                    switch(att.Version)
                    {
                        case 0:
                            {
                                Byte result;
                                att.Read(out result);
                                if (result == 1)
                                    att.ReadMetaObject(mTemplateClassInstance);
                            }
                            break;
                        case 1:
                            {
                                Byte result;
                                att.Read(out result);
                                if (result == 1)
                                    CodeGenerateSystem.Base.PropertyClassGenerator.LoadClassInstanceProperties(mTemplateClassInstance, att);
                            }
                            break;
                    }
                    att.EndRead();
                }
            }
            catch (System.Exception e)
            {
                HasError = true;
                ErrorDescription = e.ToString();
            }

            InitTemplateClass_WPF(null);
        }
    }
}
