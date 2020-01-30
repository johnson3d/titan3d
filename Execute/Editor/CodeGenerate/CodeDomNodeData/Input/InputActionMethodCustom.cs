using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using CodeGenerateSystem.Base;
using EngineNS.IO;
using EngineNS.IO.Serializer;
using EngineNS.Rtti;

namespace CodeDomNode
{
    public enum InputActionType
    {
        Action = 0,
        Axis ,
    }
    [CodeGenerateSystem.CustomConstructionParams(typeof(InputActionMethodCustomConstructParam))]
    public partial class InputActionMethodCustom : CodeGenerateSystem.Base.BaseNodeControl, CodeDomNode.CustomMethodInfo.IFunctionInParamOperation, CustomMethodInfo.IFunctionResetOperationNode, IMethodGenerator
    {
        [EngineNS.Rtti.MetaClass]
        public class InputActionMethodCustomConstructParam : CodeGenerateSystem.Base.ConstructionParams
        {
            [EngineNS.Rtti.MetaData]
            public bool IsShowProperty = true;
            CustomMethodInfo mMethodInfo;
            [EngineNS.Rtti.MetaData]
            public CustomMethodInfo MethodInfo
            {
                get => mMethodInfo;
                set
                {
                    if (mMethodInfo == value)
                        return;
                    //if(mMethodInfo != null)
                    //{
                    //    mMethodInfo.OnAddedInParam = null;
                    //    mMethodInfo.OnRemovedInParam = null;
                    //}

                    mMethodInfo = value;
                }
            }
            [EngineNS.Rtti.MetaData]
            public string MethodName
            {
                get;
                set;
            }
            [EngineNS.Rtti.MetaData]
            public InputActionType InputActionType
            {
                get;
                set;
            }

            public InputActionMethodCustomConstructParam()
            {
            }
            public override EditorCommon.CodeGenerateSystem.INodeConstructionParams Duplicate()
            {
                var retVal = base.Duplicate() as InputActionMethodCustomConstructParam;
                retVal.MethodInfo = MethodInfo;
                retVal.IsShowProperty = IsShowProperty;
                retVal.MethodName = MethodName;
                retVal.InputActionType = InputActionType;
                return retVal;
            }
            public override bool Equals(object obj)
            {
                if (!base.Equals(obj))
                    return false;
                var param = obj as InputActionMethodCustomConstructParam;
                if (param == null)
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
                                MethodInfo = new CustomMethodInfo();
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

        partial void InitConstruction();
        public InputActionMethodCustom(CodeGenerateSystem.Base.ConstructionParams csParam)
            : base(csParam)
        {
            InitConstruction();

            IsOnlyReturnValue = true;
            AddLinkPinInfo("CtrlMethodPin_Next", mCtrlMethodPin_Next, null);

            var param = csParam as InputActionMethodCustomConstructParam;
            //param.MethodInfo.OnAddedInParam -= AddParam;
            //param.MethodInfo.OnAddedInParam += AddParam;
            //param.MethodInfo.OnRemovedInParam -= RemoveParam;
            //param.MethodInfo.OnRemovedInParam += RemoveParam;
            param.MethodInfo.AddInParamOperation(this);

            mParamsPanel?.Children.Clear();
            foreach(var methodParam in param.MethodInfo.InParams)
            {
                // AddParam(methodParam);
                var noUse = OnAddedInParam(methodParam);
            }
        }

        //public void AddParam(CustomMethodInfo.FunctionParam funcParam)
        public async Task OnAddedInParam(CodeDomNode.CustomMethodInfo.FunctionParam funcParam)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            var csParam = CSParam as InputActionMethodCustomConstructParam;
            var pm = new MethodInvokeParameterControl.MethodInvokeParameterConstructionParams()
            {
                CSType = csParam.CSType,
                HostNodesContainer = csParam.HostNodesContainer,
                ConstructType = MethodInvokeNode.enParamConstructType.MethodCustom,
                ParamInfo = funcParam.CreateParamInfoAssist(System.CodeDom.FieldDirection.In),
            };
            var paramCtrl = new MethodInvokeParameterControl(pm);
            funcParam.OnParamTypeChanged -= paramCtrl.UpdateParamType;
            funcParam.OnParamTypeChanged += paramCtrl.UpdateParamType;
            AddChildNode(paramCtrl, mParamsPanel);
        }
        public async Task OnInsertInParam(int index, CodeDomNode.CustomMethodInfo.FunctionParam funcParam)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            var csParam = CSParam as InputActionMethodCustomConstructParam;
            var pm = new MethodInvokeParameterControl.MethodInvokeParameterConstructionParams()
            {
                CSType = csParam.CSType,
                HostNodesContainer = csParam.HostNodesContainer,
                ConstructType = MethodInvokeNode.enParamConstructType.MethodCustom,
                ParamInfo = funcParam.CreateParamInfoAssist(System.CodeDom.FieldDirection.In),
            };
            var paramCtrl = new MethodInvokeParameterControl(pm);
            funcParam.OnParamTypeChanged -= paramCtrl.UpdateParamType;
            funcParam.OnParamTypeChanged += paramCtrl.UpdateParamType;
            InsertChildNode(index, paramCtrl, mParamsPanel);
        }
        //void RemoveParam(int index, CustomMethodInfo.FunctionParam funcParam)
        public async Task OnRemovedInParam(int index, CodeDomNode.CustomMethodInfo.FunctionParam param)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            var node = mChildNodes[index] as MethodInvokeParameterControl;
            if(node != null)
            {
                if(node.ParamPin.HasLink)
                {
                    var pinInfo = node.ParamPin;
                    pinInfo.Clear();
                }

                param.OnParamTypeChanged -= node.UpdateParamType;
                RemoveChildNodeByIndex(index);
            }
        }

        public CustomMethodInfo GetMethodInfo()
        {
            var pm = CSParam as InputActionMethodCustomConstructParam;
            return pm.MethodInfo;
        }

        partial void ResetMethodInfo_WPF(CodeDomNode.CustomMethodInfo methodInfo);
        public async Task ResetMethodInfo(CodeDomNode.CustomMethodInfo methodInfo)
        {
            // 加载完成后处理
            var pp = mCSParam as InputActionMethodCustomConstructParam;
            pp.MethodInfo.RemoveInParamOperation(this);

            // 比较新旧MethodInfo参数，对参数进行增删
            for(int i=0; i<methodInfo.InParams.Count; i++)
            {
                if(i >= pp.MethodInfo.InParams.Count)
                {
                    await OnAddedInParam(methodInfo.InParams[i]);
                }
                else
                {
                    if (pp.MethodInfo.InParams[i].IsEqual(methodInfo.InParams[i]))
                    {
                        var pm = mChildNodes[i] as MethodInvokeParameterControl;
                        pm.ResetParamInfo(methodInfo.InParams[i], FieldDirection.In);
                    }
                    else
                    {
                        await OnRemovedInParam(i, pp.MethodInfo.InParams[i]);
                        await OnInsertInParam(i, methodInfo.InParams[i]);
                    }
                }
            }
            // 删除多余的参数
            for(int i=methodInfo.InParams.Count; i<pp.MethodInfo.InParams.Count; i++)
            {
                await OnRemovedInParam(i, pp.MethodInfo.InParams[i]);
            }

            pp.MethodInfo = methodInfo;
            ResetMethodInfo_WPF(methodInfo);

            methodInfo.AddInParamOperation(this);
        }

        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            CollectLinkPinInfo(smParam, "CtrlMethodPin_Next", CodeGenerateSystem.Base.enLinkType.Method, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, false);
        }

        #region GenerateCode

        public override async System.Threading.Tasks.Task GCode_CodeDom_GenerateCode(CodeTypeDeclaration codeClass, CodeStatementCollection codeStatementCollection, LinkPinControl element, GenerateCodeContext_Method context)
        {
            // 通过代理生成的函数
            var csParam = CSParam as InputActionMethodCustomConstructParam;

            var methodCode = new CodeMemberMethod();
            methodCode.Attributes = MemberAttributes.Final | MemberAttributes.Private;
            methodCode.Name = context.DelegateMethodName;
            List<string> genericParamNames = new List<string>();
            var catchparamName = "(";
            foreach(var paramNode in mChildNodes)
            {
                var paramExp = new CodeParameterDeclarationExpression();
                var pm = paramNode as MethodInvokeParameterControl;
                var pmParam = pm.CSParam as MethodInvokeParameterControl.MethodInvokeParameterConstructionParams;
                paramExp.Direction = pm.ParamFlag;
                paramExp.Name = pmParam.ParamInfo.ParamName;
                var paramType = pm.ParamType;
                paramExp.Type = new CodeTypeReference(paramType);
                if (paramType.IsGenericParameter && !genericParamNames.Contains(paramType.Name))
                {
                    genericParamNames.Add(paramType.Name);
                    methodCode.TypeParameters.Add(new CodeTypeParameter(paramType.Name));
                }

                methodCode.Parameters.Add(paramExp);
                catchparamName += paramExp.Type + " " + paramExp.Name + ",";
            }
            bool hasReturn = false;
            foreach(var param in csParam.MethodInfo.OutParams)
            {
                if(param.ParamName == "Return")
                {
                    var paramType = param.ParamType.GetActualType();
                    if (csParam.MethodInfo.IsAsync)
                        methodCode.ReturnType = new CodeTypeReference($"async System.Threading.Tasks.Task<{EngineNS.Rtti.RttiHelper.GetAppTypeString(paramType)}>");
                    else
                        methodCode.ReturnType = new CodeTypeReference(paramType);
                    if(paramType.IsGenericParameter && !genericParamNames.Contains(paramType.Name))
                    {
                        genericParamNames.Add(paramType.Name);
                        methodCode.TypeParameters.Add(new CodeTypeParameter(paramType.Name));
                    }
                    context.ReturnValueType = paramType;
                    hasReturn = true;
                }
                else
                {
                    var paramExp = new CodeParameterDeclarationExpression();
                    paramExp.Direction = FieldDirection.Out;
                    paramExp.Name = param.ParamName;
                    var paramType = param.ParamType.GetActualType();
                    paramExp.Type = new CodeTypeReference(paramType);
                    if (paramType.IsGenericParameter && !genericParamNames.Contains(paramType.Name))
                    {
                        genericParamNames.Add(paramType.Name);
                        methodCode.TypeParameters.Add(new CodeTypeParameter(paramType.Name));
                    }

                    methodCode.Parameters.Add(paramExp);
                    catchparamName += paramExp.Type + " " + paramExp.Name + ",";
                }
            }

            if(!hasReturn)
            {
                if (csParam.MethodInfo.IsAsync)
                    methodCode.ReturnType = new CodeTypeReference($"async System.Threading.Tasks.Task");
                else
                    methodCode.ReturnType = new CodeTypeReference(typeof(void));
            }

            catchparamName = catchparamName.TrimEnd(',');
            catchparamName += ")";

            var tryCatchExp = new CodeTryCatchFinallyStatement();
            var exName = "ex_" + EngineNS.Editor.Assist.GetValuedGUIDString(Id);
            var cah = new CodeCatchClause(exName);
            cah.Statements.Add(new CodeExpressionStatement(
                                        new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(
                                            new CodeSnippetExpression("EngineNS.Profiler.Log"), "WriteException",
                                            new CodeVariableReferenceExpression(exName),
                                            new CodePrimitiveExpression("Macross异常"))));
            tryCatchExp.CatchClauses.Add(cah);
            methodCode.Statements.Add(tryCatchExp);

            foreach(var param in csParam.MethodInfo.OutParams)
            {
                if (param.ParamName == "Return")
                    continue;

                methodCode.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(param.ParamName),
                                                  new CodePrimitiveExpression(CodeGenerateSystem.Program.GetDefaultValueFromType(param.ParamType.GetActualType()))));
            }
            codeClass.Members.Add(methodCode);

            #region Debug
            // 收集用于调试的数据的代码
            var debugCodes = CodeDomNode.BreakPoint.BeginMacrossDebugCodeStatments(tryCatchExp.TryStatements);
            foreach (var param in mChildNodes)
            {
                var pm = param as MethodInvokeParameterControl;
                CodeDomNode.BreakPoint.GetGatherDataValueCodeStatement(debugCodes, pm.ParamPin.GetLinkPinKeyName(), pm.GCode_CodeDom_GetValue(pm.ParamPin, context), pm.GCode_GetTypeString(pm.ParamPin, context), context);
            }
            // 断点
            var breakCondStatement = CodeDomNode.BreakPoint.BreakCodeStatement(codeClass, debugCodes, HostNodesContainer.GUID, Id);
            // 设置数据
            foreach (var param in mChildNodes)
            {
                var pm = param as MethodInvokeParameterControl;
                CodeDomNode.BreakPoint.GetSetDataValueCodeStatement(breakCondStatement.TrueStatements, pm.ParamPin.GetLinkPinKeyName(), pm.GCode_CodeDom_GetValue(pm.ParamPin, context), pm.GCode_GetType(pm.ParamPin, context));
            }
            CodeDomNode.BreakPoint.EndMacrossDebugCodeStatements(tryCatchExp.TryStatements, debugCodes);
            #endregion

            if(mCtrlMethodPin_Next.HasLink)
            {
                await mCtrlMethodPin_Next.GetLinkedObject(0, false).GCode_CodeDom_GenerateCode(codeClass, tryCatchExp.TryStatements, mCtrlMethodPin_Next.GetLinkedPinControl(0, false), context);
            }

            if(hasReturn)
            {
                if (context.ReturnValueType.IsGenericParameter)
                    methodCode.Statements.Add(new CodeMethodReturnStatement(new CodeSnippetExpression($"default({context.ReturnValueType.Name})")));
                else
                    methodCode.Statements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(CodeGenerateSystem.Program.GetDefaultValueFromType(context.ReturnValueType))));
            }
        }
        public override async System.Threading.Tasks.Task GCode_CodeDom_GenerateCode(CodeTypeDeclaration codeClass, LinkPinControl element, GenerateCodeContext_Class context)
        {
            await GCode_CodeDom_GenerateMethodCode(codeClass, element, context, null);
        }
        public async System.Threading.Tasks.Task GCode_CodeDom_GenerateMethodCode(CodeTypeDeclaration codeClass, LinkPinControl element, GenerateCodeContext_Class context, MethodGenerateData data)
        {
            // 正常的自定义函数
            var csParam = CSParam as InputActionMethodCustomConstructParam;

            var methodCode = new CodeMemberMethod();
            if (csParam.MethodInfo.OverrideAble)
                methodCode.Attributes = MemberAttributes.Public;
            else
                methodCode.Attributes = MemberAttributes.Final | MemberAttributes.Public;

            methodCode.Name = NodeName;
            var catchParamName = "(";
            foreach(var paramNode in mChildNodes)
            {
                var paramExp = new System.CodeDom.CodeParameterDeclarationExpression();
                var pm = paramNode as MethodInvokeParameterControl;
                var pmParam = pm.CSParam as MethodInvokeParameterControl.MethodInvokeParameterConstructionParams;
                paramExp.Direction = pm.ParamFlag;
                paramExp.Name = pmParam.ParamInfo.ParamName;
                paramExp.Type = new CodeTypeReference(pm.ParamType);

                methodCode.Parameters.Add(paramExp);
                catchParamName += paramExp.Type + " " + paramExp.Name + ",";
            }
            foreach(var param in csParam.MethodInfo.OutParams)
            {
                var paramExp = new CodeParameterDeclarationExpression();
                paramExp.Direction = FieldDirection.Out;
                paramExp.Name = param.ParamName;
                paramExp.Type = new CodeTypeReference(param.ParamType.GetActualType());

                methodCode.Parameters.Add(paramExp);
                catchParamName += paramExp.Type + " " + paramExp.Name + ",";
            }

            {
                // unsafe
                //methodCode.ReturnType = new CodeTypeReference($"unsafe void");
                if (csParam.MethodInfo.IsAsync)
                    methodCode.ReturnType = new CodeTypeReference($"async System.Threading.Tasks.Task");
                else
                    methodCode.ReturnType = new CodeTypeReference(typeof(void));
            }

            if (data != null)
            {
                foreach (var localParam in data.LocalParams)
                {
                    var defVal = CodeGenerateSystem.Program.GetDefaultValueFromType(localParam.ParamType);
                    var initExp = Program.GetValueCode(methodCode.Statements, localParam.ParamType, defVal);
                    methodCode.Statements.Add(new CodeVariableDeclarationStatement(localParam.ParamType, localParam.ParamName, initExp));
                }
            }

            catchParamName = catchParamName.TrimEnd(',');
            catchParamName += ")";

            var tryCatchExp = new CodeTryCatchFinallyStatement();
            var exName = "ex_" + EngineNS.Editor.Assist.GetValuedGUIDString(Id);
            var cah = new CodeCatchClause(exName);
            cah.Statements.Add(new CodeExpressionStatement(
                                        new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(
                                            new CodeSnippetExpression("EngineNS.Profiler.Log"), "WriteException",
                                            new CodeVariableReferenceExpression(exName),
                                            new CodePrimitiveExpression("Macross异常"))));
            tryCatchExp.CatchClauses.Add(cah);
            methodCode.Statements.Add(tryCatchExp);

           
            foreach (var param in csParam.MethodInfo.OutParams)
            {
                methodCode.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(param.ParamName),
                                                                  new CodePrimitiveExpression(CodeGenerateSystem.Program.GetDefaultValueFromType(param.ParamType.GetActualType()))));
            }
            codeClass.Members.Add(methodCode);

            #region Debug
            var methodContext = new CodeGenerateSystem.Base.GenerateCodeContext_Method(context, methodCode);
            if(csParam.MethodInfo.IsAsync)
                methodContext.ReturnValueType = typeof(System.Threading.Tasks.Task);
            // 收集用于调试的数据的代码
            var debugCodes = CodeDomNode.BreakPoint.BeginMacrossDebugCodeStatments(tryCatchExp.TryStatements);
            foreach(var param in mChildNodes)
            {
                var pm = param as MethodInvokeParameterControl;
                CodeDomNode.BreakPoint.GetGatherDataValueCodeStatement(debugCodes, pm.ParamPin.GetLinkPinKeyName(), pm.GCode_CodeDom_GetValue(pm.ParamPin, methodContext), pm.GCode_GetTypeString(pm.ParamPin, methodContext), methodContext);
            }
            // 断点
            var breakCondStatement = CodeDomNode.BreakPoint.BreakCodeStatement(codeClass, debugCodes, HostNodesContainer.GUID, Id);
            // 设置数据
            foreach(var param in mChildNodes)
            {
                var pm = param as MethodInvokeParameterControl;
                CodeDomNode.BreakPoint.GetSetDataValueCodeStatement(breakCondStatement.TrueStatements, pm.ParamPin.GetLinkPinKeyName(), pm.GCode_CodeDom_GetValue(pm.ParamPin, methodContext), pm.GCode_GetType(pm.ParamPin, methodContext));
            }
            CodeDomNode.BreakPoint.EndMacrossDebugCodeStatements(tryCatchExp.TryStatements, debugCodes);
            #endregion

            if (mCtrlMethodPin_Next.HasLink)
            {
                if (csParam.MethodInfo.IsAsync)
                    methodContext.ReturnValueType = typeof(System.Threading.Tasks.Task);
                else
                    methodContext.ReturnValueType = typeof(void);
                await mCtrlMethodPin_Next.GetLinkedObject(0, false).GCode_CodeDom_GenerateCode(codeClass, tryCatchExp.TryStatements, mCtrlMethodPin_Next.GetLinkedPinControl(0, false), methodContext);
            }
        }

        public override bool Pin_UseOrigionParamName(LinkPinControl linkElement)
        {
            return true;
        }

        #endregion
    }
}
