using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Controls;
using CodeGenerateSystem.Base;
using EngineNS.IO;

namespace CodeDomNode
{
    [CodeGenerateSystem.CustomConstructionParams(typeof(MethodOverrideConstructParam))]
    public partial class MethodOverride : CodeGenerateSystem.Base.BaseNodeControl, CodeGenerateSystem.Base.IMethodGenerator
    {
        [EngineNS.Rtti.MetaClass]
        public class MethodOverrideConstructParam : CodeGenerateSystem.Base.ConstructionParams
        {
            [EngineNS.Rtti.MetaData]
            public MethodInfoAssist MethodInfo { get; set; }
            public MethodOverrideConstructParam()
            {
            }
            public override EditorCommon.CodeGenerateSystem.INodeConstructionParams Duplicate()
            {
                var retVal = base.Duplicate() as MethodOverrideConstructParam;
                retVal.MethodInfo = MethodInfo;
                return retVal;
            }
            public override bool Equals(object obj)
            {
                if (!base.Equals(obj))
                    return false;
                var param = obj as MethodOverrideConstructParam;
                if (param == null)
                    return false;
                if (MethodInfo == param.MethodInfo)
                    return true;
                return false;
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

        CodeGenerateSystem.Base.LinkPinControl mCtrlMethodPin_Next = new CodeGenerateSystem.Base.LinkPinControl();
        StackPanel mParamsPanel = null;
        public Type MethodReturnType
        {
            get;
            protected set;
        }

        partial void InitConstruction();
        public MethodOverride(CodeGenerateSystem.Base.ConstructionParams csParam)
            : base(csParam)
        {
            InitConstruction();

            IsOnlyReturnValue = true;
            AddLinkPinInfo("CtrlMethodPin_Next", mCtrlMethodPin_Next, null);
            //if (string.IsNullOrEmpty(csParam.ConstructParam))
            //    return;

            try
            {
                var param = csParam as MethodOverrideConstructParam;
                NodeName = param.MethodInfo.MethodName;
                ShowNodeName = param.MethodInfo.ShowMethodName;
                //mMethodInfo = Program.GetMethodInfoFromParam(CSParam.ConstructParam);
                //if(mMethodInfo != null)
                //{
                //    SetParameters(mMethodInfo.GetParameters(), StandardParamPropertyInfos);
                //    MethodReturnType = mMethodInfo.ReturnType;
                //}
                //else
                //{
                //    // 连线函数不存在
                //    HasError = true;
                //    ErrorDescription = $"函数不存在！";

                //    if(!string.IsNullOrEmpty(splits[3]))
                //    {
                //        var paramSplits = splits[3].Split('/');
                //        SetParameters(paramSplits, CSParam.CSType);
                //    }
                //    if(!string.IsNullOrEmpty(splits[4]))
                //    {
                //        var retType = EngineNS.Rtti.RttiHelper.GetTypeFromSaveString(splits[4]);
                //        MethodReturnType = retType;
                //    }
                //}
                SetParameters(param.MethodInfo.Params);
                MethodReturnType = param.MethodInfo.ReturnType;
            }
            catch(System.Exception e)
            {
                HasError = true;
                ErrorDescription = $"连线函数节点异常！：Name={NodeName}, Param={csParam.ConstructParam}\r\n{e.ToString()}";
            }
        }

        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            CollectLinkPinInfo(smParam, "CtrlMethodPin_Next", CodeGenerateSystem.Base.enLinkType.Method, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, false);
        }

        protected void SetParameters(List<MethodParamInfoAssist> paramInfos)
        {
            if (paramInfos == null)
                return;

            mParamsPanel?.Children.Clear();
            foreach(var paramInfo in paramInfos)
            {
                CodeGenerateSystem.Base.BaseNodeControl pc = null;
                if(paramInfo.ParameterType.IsSubclassOf(typeof(System.Delegate)))
                {
                    var csParam = new MethodInvoke_DelegateControl.MethodInvoke_DelegateControlConstructionParams()
                    {
                        CSType = mCSParam.CSType,
                        HostNodesContainer = mCSParam.HostNodesContainer,
                        ConstructParam = this.NodeName,
                        ConstructType = MethodInvokeNode.enParamConstructType.MethodOverride,
                        ParamInfo = paramInfo,
                    };
                    pc = new MethodInvoke_DelegateControl(csParam);
                }
                else
                {
                    if(paramInfo.IsParamsArray)
                    {
                        var csParam = new ParamParameterControl.ParamParameterConstructionParams()
                        {
                            CSType = mCSParam.CSType,
                            HostNodesContainer = mCSParam.HostNodesContainer,
                            ConstructParam = "",
                            ConstructType = MethodInvokeNode.enParamConstructType.MethodOverride,
                            ParamInfo = paramInfo,
                        };
                        pc = new ParamParameterControl(csParam);
                    }
                    else
                    {
                        var csParam = new MethodInvokeParameterControl.MethodInvokeParameterConstructionParams()
                        {
                            CSType = mCSParam.CSType,
                            HostNodesContainer = mCSParam.HostNodesContainer,
                            ConstructParam = "",
                            ConstructType = MethodInvokeNode.enParamConstructType.MethodOverride,
                            ParamInfo = paramInfo,
                        };
                        pc = new MethodInvokeParameterControl(csParam);
                    }
                }

                //pc.SetToolTip(EngineNS.Rtti.AttributeHelper.GetCustomAttributePropertyValue(paramInfo, typeof(DescriptionAttribute).FullName, "Description", false));
                AddChildNode(pc, mParamsPanel);
            }
        }
        protected void SetParameters(string[] paramInfos, EngineNS.ECSType csType)
        {
            foreach(var info in paramInfos)
            {
                var tempSplits = info.Split(':');
                if(tempSplits.Length > 2)
                {
                    switch(tempSplits[2])
                    {
                        case "Ref":
                        case "Out":
                            tempSplits[1] += "&";
                            break;
                    }
                }
                CodeGenerateSystem.Base.BaseNodeControl pc = null;
                var type = EngineNS.Rtti.RttiHelper.GetTypeFromSaveString(tempSplits[1]);
                if(type != null && type.IsSubclassOf(typeof(System.Delegate)))
                {
                    var csParam = new MethodInvoke_DelegateControl.MethodInvoke_DelegateControlConstructionParams()
                    {
                        CSType = csType,
                        HostNodesContainer = this.HostNodesContainer,
                        ConstructParam = info + ":" + this.NodeName,
                        ConstructType = MethodInvokeNode.enParamConstructType.MethodOverride,
                    };
                    pc = new MethodInvoke_DelegateControl(csParam);
                }
                else
                {
                    var csParam = new MethodInvokeParameterControl.MethodInvokeParameterConstructionParams()
                    {
                        CSType = csType,
                        HostNodesContainer = this.HostNodesContainer,
                        ConstructParam = info,
                        ConstructType = MethodInvokeNode.enParamConstructType.MethodOverride,
                    };
                    pc = new MethodInvokeParameterControl(csParam);
                }

                AddChildNode(pc, mParamsPanel);
            }
        }

        #region GenerateCode

        public override async System.Threading.Tasks.Task GCode_CodeDom_GenerateCode(CodeTypeDeclaration codeClass, LinkPinControl element, GenerateCodeContext_Class context)
        {
            await GCode_CodeDom_GenerateMethodCode(codeClass, element, context, null);
        }
        public async System.Threading.Tasks.Task GCode_CodeDom_GenerateMethodCode(CodeTypeDeclaration codeClass, LinkPinControl element, GenerateCodeContext_Class context, MethodGenerateData data)
        {
            var csParam = CSParam as MethodOverrideConstructParam;

            Type[] paramTypes = new Type[csParam.MethodInfo.Params.Count];
            for (int i = 0; i < paramTypes.Length; i++)
            {
                switch (csParam.MethodInfo.Params[i].FieldDirection)
                {
                    case FieldDirection.In:
                        if (csParam.MethodInfo.Params[i].IsParamsArray)
                            throw new InvalidOperationException("未实现");
                        else
                            paramTypes[i] = csParam.MethodInfo.Params[i].ParameterType;
                        break;
                    case FieldDirection.Out:
                    case FieldDirection.Ref:
                        if (csParam.MethodInfo.Params[i].IsParamsArray)
                            throw new InvalidOperationException("未实现");
                        else
                            paramTypes[i] = csParam.MethodInfo.Params[i].ParameterType.MakeByRefType();
                        break;
                }
            }
            EngineNS.Editor.MacrossMemberAttribute.enMacrossType macrossType = EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Overrideable;
            if(csParam.MethodInfo.IsFromMacross)
            {
                macrossType |= EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable;
            }
            else
            {
                var methodInfo = csParam.MethodInfo.ParentClassType.GetMethod(csParam.MethodInfo.MethodName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static, null, paramTypes, null);
                var atts = methodInfo.GetCustomAttributes(typeof(EngineNS.Editor.MacrossMemberAttribute), false);
                if (atts.Length > 0)
                {
                    var macrossMemberAtt = atts[0] as EngineNS.Editor.MacrossMemberAttribute;
                    macrossType = macrossMemberAtt.MacrossType;
                }
            }

            if (element == null || element == mCtrlMethodPin_Next)
            {
                var methodCode = new CodeGenerateSystem.CodeDom.CodeMemberMethod();
                methodCode.Attributes = MemberAttributes.Override;
                //if (mMethodInfo != null)
                //{
                if (csParam.MethodInfo.IsFamily)
                    methodCode.Attributes |= MemberAttributes.Family;
                if (csParam.MethodInfo.IsFamilyAndAssembly)
                    methodCode.Attributes |= MemberAttributes.FamilyAndAssembly;
                if (csParam.MethodInfo.IsFamilyOrAssembly)
                    methodCode.Attributes |= MemberAttributes.FamilyOrAssembly;
                if (csParam.MethodInfo.IsPublic)
                    methodCode.Attributes |= MemberAttributes.Public;
                //}
                //else
                //    methodCode.Attributes |= MemberAttributes.Public;
                methodCode.Name = NodeName;

                var mcType = EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Unknow;
                if (csParam.MethodInfo.MC_Callable)
                    mcType = mcType | EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable;
                if (csParam.MethodInfo.MC_Overrideable)
                    mcType = mcType | EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Overrideable;
                if (mcType != EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Unknow)
                    methodCode.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(typeof(EngineNS.Editor.MacrossMemberAttribute)), new CodeAttributeArgument(new CodePrimitiveExpression(mcType))));

                if (data != null)
                {
                    foreach (var localParam in data.LocalParams)
                    {
                        var defVal = CodeGenerateSystem.Program.GetDefaultValueFromType(localParam.ParamType);
                        var initExp = Program.GetValueCode(methodCode.Statements, localParam.ParamType, defVal);
                        methodCode.Statements.Add(new CodeVariableDeclarationStatement(localParam.ParamType, localParam.ParamName, initExp));
                    }
                }

                string paramPreStr = "temp___";
                //bool needUnsafeFlag = false;
                string catchParamName = "(";
                foreach (var paramNode in mChildNodes)
                {
                    var paramExp = new System.CodeDom.CodeParameterDeclarationExpression();
                    if (paramNode is MethodInvokeParameterControl)
                    {
                        var pm = paramNode as MethodInvokeParameterControl;
                        var pmParam = pm.CSParam as MethodInvokeParameterControl.MethodInvokeParameterConstructionParams;
                        paramExp.Direction = pm.ParamFlag;
                        if(pmParam.ParamInfo.ParameterDisplayType != null)
                        {
                            paramExp.Name = paramPreStr + pmParam.ParamInfo.ParamName;
                            paramExp.Type = new CodeTypeReference(pmParam.ParamInfo.ParameterType);
                        }
                        else
                        {
                            paramExp.Name = pmParam.ParamInfo.ParamName;
                            paramExp.Type = new System.CodeDom.CodeTypeReference(pm.ParamType);
                        }

                        //if (pm.ParamType.IsPointer)
                        //    needUnsafeFlag = true;
                    }
                    else if (paramNode is ParamParameterControl)
                    {
                        var pm = paramNode as ParamParameterControl;
                        paramExp.Name = pm.ParamName;
                        paramExp.Type = new System.CodeDom.CodeTypeReference(pm.ParamType);

                        //if (pm.ParamType.IsPointer)
                        //    needUnsafeFlag = true;
                    }
                    else if (paramNode is MethodInvoke_DelegateControl)
                    {
                        var pm = paramNode as MethodInvoke_DelegateControl;
                        paramExp.Name = pm.ParamName;
                        paramExp.Type = new System.CodeDom.CodeTypeReference(pm.ParamType);
                    }

                    methodCode.Parameters.Add(paramExp);
                    catchParamName += paramExp.Type + " " + paramExp.Name + ",";
                }
                // 所有函数全部unsafe
                //if (needUnsafeFlag)
                {
                    //var typeName = MethodReturnType.FullName;
                    methodCode.ReturnType = new CodeTypeReference(MethodReturnType);
                    if(MethodReturnType == typeof(System.Threading.Tasks.Task) || MethodReturnType.BaseType == typeof(System.Threading.Tasks.Task))
                    {
                        methodCode.IsAsync = true;
                    }
                    else
                    {
                        if (EngineNS.Editor.MacrossMemberAttribute.HasType(macrossType, EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Unsafe))
                            methodCode.IsUnsafe = true;
                    }
                }
                //else
                //    methodCode.ReturnType = new CodeTypeReference(MethodReturnType);

                catchParamName = catchParamName.TrimEnd(',');
                catchParamName += ")";

                var tryCatchExp = new System.CodeDom.CodeTryCatchFinallyStatement();
                tryCatchExp.TryStatements.Add(new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(new CodeVariableReferenceExpression(context.ScopFieldName), "Begin", new CodeExpression[0]));
                var exName = "ex_" + EngineNS.Editor.Assist.GetValuedGUIDString(Id);
                var cah = new System.CodeDom.CodeCatchClause(exName);
                cah.Statements.Add(new System.CodeDom.CodeExpressionStatement(
                                            new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(
                                                new System.CodeDom.CodeSnippetExpression("EngineNS.Profiler.Log"), "WriteException",
                                                new System.CodeDom.CodeVariableReferenceExpression(exName),
                                                new CodePrimitiveExpression("Macross异常"))));
                tryCatchExp.CatchClauses.Add(cah);
                tryCatchExp.FinallyStatements.Add(new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(new CodeVariableReferenceExpression(context.ScopFieldName), "End", new CodeExpression[0]));

                string paramComment = "";
                // 设置out参数默认值
                foreach (var param in csParam.MethodInfo.Params)
                {
                    if(param.ParameterDisplayType != null)
                    {
                        if (param.FieldDirection == FieldDirection.Out)
                        {
                            methodCode.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(paramPreStr + param.ParamName),
                                                                              new CodePrimitiveExpression(CodeGenerateSystem.Program.GetDefaultValueFromType(param.ParameterType))));
                        }
                        methodCode.Statements.Add(new CodeVariableDeclarationStatement(param.ParameterDisplayType, param.ParamName, new CodeGenerateSystem.CodeDom.CodeCastExpression(param.ParameterDisplayType, new CodeVariableReferenceExpression(paramPreStr + param.ParamName))));
                    }
                    else
                    {
                        if(param.FieldDirection == FieldDirection.Out)
                        {
                            methodCode.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(param.ParamName),
                                                                              new CodePrimitiveExpression(CodeGenerateSystem.Program.GetDefaultValueFromType(param.ParameterType))));
                        }
                    }

                    paramComment += param.FieldDirection + "," + param.ParameterType.FullName + "|";
                }
                paramComment = paramComment.TrimEnd('|');

                methodCode.Statements.Add(tryCatchExp);
                foreach(var param in csParam.MethodInfo.Params)
                {
                    // ref或out，需要将displayType造成的临时变量再赋给原函数参数
                    if((param.FieldDirection == FieldDirection.Out || param.FieldDirection == FieldDirection.Ref) && param.ParameterDisplayType != null)
                    {
                        methodCode.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(paramPreStr + param.ParamName), new CodeGenerateSystem.CodeDom.CodeCastExpression(param.ParameterType, new CodeVariableReferenceExpression(param.ParamName))));
                    }
                }

                if (csParam.MethodInfo.ReturnType != typeof(void) && csParam.MethodInfo.ReturnType != typeof(System.Threading.Tasks.Task))
                {
                    var retVal = CodeGenerateSystem.Program.GetDefaultValueFromType(csParam.MethodInfo.ReturnType);
                    methodCode.Statements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(retVal)));
                }


                if (csParam.MethodInfo.IsFromMacross)
                {
                    codeClass.Members.Add(new CodeSnippetTypeMember($"// OverrideStart {csParam.MethodInfo.FuncId.ToString()} {NodeName} {paramComment}"));
                    codeClass.Members.Add(new CodeSnippetTypeMember("#pragma warning disable 1998"));
                    codeClass.Members.Add(methodCode);
                    codeClass.Members.Add(new CodeSnippetTypeMember("#pragma warning restore 1998"));
                    codeClass.Members.Add(new CodeSnippetTypeMember($"// OverrideEnd {csParam.MethodInfo.FuncId.ToString()} {NodeName}"));
                }
                else
                {
                    codeClass.Members.Add(new CodeSnippetTypeMember($"// OverrideStart {NodeName} {paramComment}"));
                    codeClass.Members.Add(new CodeSnippetTypeMember("#pragma warning disable 1998"));
                    codeClass.Members.Add(methodCode);
                    codeClass.Members.Add(new CodeSnippetTypeMember("#pragma warning restore 1998"));
                    codeClass.Members.Add(new CodeSnippetTypeMember($"// OverrideEnd {NodeName}"));
                }

                var methodContext = new CodeGenerateSystem.Base.GenerateCodeContext_Method(context, methodCode);
                // 收集用于调试的数据的代码
                var debugCodes = CodeDomNode.BreakPoint.BeginMacrossDebugCodeStatments(tryCatchExp.TryStatements);
                foreach (var paramNode in mChildNodes)
                {
                    if (paramNode is MethodInvokeParameterControl)
                    {
                        var paramCtrl = paramNode as MethodInvokeParameterControl;
                        CodeDomNode.BreakPoint.GetGatherDataValueCodeStatement(debugCodes, paramCtrl.ParamPin.GetLinkPinKeyName(), paramCtrl.GCode_CodeDom_GetValue(paramCtrl.ParamPin, methodContext), paramCtrl.GCode_GetTypeString(paramCtrl.ParamPin, methodContext), methodContext);
                    }
                    else if (paramNode is ParamParameterControl)
                    {
                        throw new InvalidOperationException();
                    }
                }
                // 断点
                var breakCondStatement = CodeDomNode.BreakPoint.BreakCodeStatement(codeClass, debugCodes, HostNodesContainer.GUID, Id);
                // 设置数据
                foreach(var paramNode in mChildNodes)
                {
                    if(paramNode is MethodInvokeParameterControl)
                    {
                        var paramCtrl = paramNode as MethodInvokeParameterControl;
                        CodeDomNode.BreakPoint.GetSetDataValueCodeStatement(breakCondStatement.TrueStatements, paramCtrl.ParamPin.GetLinkPinKeyName(), paramCtrl.GCode_CodeDom_GetValue(paramCtrl.ParamPin, methodContext), paramCtrl.GCode_GetType(paramCtrl.ParamPin, methodContext));
                    }
                    else if(paramNode is ParamParameterControl)
                    {
                        throw new InvalidOperationException();
                    }
                }
                CodeDomNode.BreakPoint.EndMacrossDebugCodeStatements(tryCatchExp.TryStatements, debugCodes);

                if (mCtrlMethodPin_Next.HasLink)
                {
                    methodContext.ReturnValueType = MethodReturnType;
                    await mCtrlMethodPin_Next.GetLinkedObject(0, false).GCode_CodeDom_GenerateCode(codeClass, tryCatchExp.TryStatements, mCtrlMethodPin_Next.GetLinkedPinControl(0, false), methodContext);
                }
            }
        }

        public override bool Pin_UseOrigionParamName(LinkPinControl linkElement)
        {
            return true;
        }

        #endregion
    }
}
