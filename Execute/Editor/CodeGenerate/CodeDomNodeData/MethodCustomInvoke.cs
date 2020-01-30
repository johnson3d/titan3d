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
    [CodeGenerateSystem.CustomConstructionParams(typeof(MethodCustomInvokeConstructParam))]
    public partial class MethodCustomInvoke : CodeGenerateSystem.Base.BaseNodeControl, CustomMethodInfo.IFunctionInParamOperation, CustomMethodInfo.IFunctionOutParamOperation, CustomMethodInfo.IFunctionResetOperationNode, IRNameContainer
    {
        #region IRNameContainer
        public void CollectRefRNames(List<EngineNS.RName> rNames)
        {
            var tcType = mTemplateClassInstance.GetType();
            foreach (var cNode in mChildNodes)
            {
                var cCtrl = cNode as CodeDomNode.MethodInvokeParameterControl;
                if (cCtrl == null)
                    continue;

                if (cCtrl.ParamType == typeof(EngineNS.RName))
                {
                    var param = cCtrl.CSParam as MethodInvokeParameterControl.MethodInvokeParameterConstructionParams;
                    var property = tcType.GetProperty(param.ParamInfo.ParamName);
                    var value = property.GetValue(mTemplateClassInstance) as EngineNS.RName;
                    if(!rNames.Contains(value))
                        rNames.Add(value);
                }
                else if (cCtrl.ParamType.GetInterface(typeof(EngineNS.Macross.IMacrossType).FullName) != null)
                {

                    EngineNS.RName ValueRName = EngineNS.RName.GetRName(cCtrl.ParamType.FullName + ".macross");//EngineNS.Macross.MacrossFactory.Instance.GetMacrossRName(cCtrl.ParamType); 
                    if (ValueRName != EngineNS.RName.EmptyName)
                    {
                        if (!rNames.Contains(ValueRName))
                            rNames.Add(ValueRName);
                    }
                   
                }

            }
        }
        #endregion

        [EngineNS.Rtti.MetaClass]
        public class MethodCustomInvokeConstructParam : CodeGenerateSystem.Base.ConstructionParams
        {
            CustomMethodInfo mMethodInfo;
            [EngineNS.Rtti.MetaData]
            public CustomMethodInfo MethodInfo
            {
                get => mMethodInfo;
                set
                {
                    if (mMethodInfo == value)
                        return;

                    mMethodInfo = value;
                }
            }

            public override EditorCommon.CodeGenerateSystem.INodeConstructionParams Duplicate()
            {
                var retVal = base.Duplicate() as MethodCustomInvokeConstructParam;
                retVal.MethodInfo = MethodInfo;
                return retVal;
            }
            public override bool Equals(object obj)
            {
                if (!base.Equals(obj))
                    return false;
                var param = obj as MethodCustomInvokeConstructParam;
                if (param == null)
                    return false;
                if (MethodInfo != param.MethodInfo)
                    return false;
                return true;
            }
            public override int GetHashCode()
            {
                var retVal = (base.GetHashCodeString() + MethodInfo.ToString()).GetHashCode();
                return retVal;
            }
        }

        CodeGenerateSystem.Base.LinkPinControl mCtrlMethodPin_Pre = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mCtrlMethodPin_Next = new CodeGenerateSystem.Base.LinkPinControl();
        StackPanel mParamsPanel = null; // 输入参数
        StackPanel mValuesPanel = null; // 输出参数

        protected override Panel GetChildNodeContainer(BaseNodeControl childNode)
        {
            var ctrl = childNode as MethodInvokeParameterControl;
            if(ctrl != null)
            {
                switch(ctrl.ParamFlag)
                {
                    case FieldDirection.In:
                        return mParamsPanel;
                    case FieldDirection.Out:
                        return mValuesPanel;
                }
            }
            return null;
        }

        public async Task OnAddedInParam(CustomMethodInfo.FunctionParam param)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            var csParam = CSParam as MethodCustomInvokeConstructParam;
            var pm = new MethodInvokeParameterControl.MethodInvokeParameterConstructionParams()
            {
                CSType = csParam.CSType,
                HostNodesContainer = csParam.HostNodesContainer,
                ConstructType = MethodInvokeNode.enParamConstructType.MethodCustomInvoke_In,
                ParamInfo = param.CreateParamInfoAssist(System.CodeDom.FieldDirection.In),
            };
            var paramCtrl = new MethodInvokeParameterControl(pm);
            param.OnParamTypeChanged -= paramCtrl.UpdateParamType;
            param.OnParamTypeChanged += paramCtrl.UpdateParamType;
            param.OnParamTypeChanged -= UpdateInParamType;
            param.OnParamTypeChanged += UpdateInParamType;

            int i = 0;
            for(i=0; i< mChildNodes.Count; i++)
            {
                var tempParam = mChildNodes[i].CSParam as MethodInvokeParameterControl.MethodInvokeParameterConstructionParams;
                if (tempParam.ParamInfo.FieldDirection == FieldDirection.Out)
                    break;
            }
            if(i >= mChildNodes.Count)
                AddChildNodeNoChanageContainer(paramCtrl, mParamsPanel);
            else
                InsertChildNodeNoChangeContainer(i, paramCtrl, mParamsPanel);

            CreateTemplateClass(param.HostMethodInfo);
        }
        public async Task OnInsertInParam(int index, CustomMethodInfo.FunctionParam param)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            var csParam = CSParam as MethodCustomInvokeConstructParam;
            var pm = new MethodInvokeParameterControl.MethodInvokeParameterConstructionParams()
            {
                CSType = csParam.CSType,
                HostNodesContainer = csParam.HostNodesContainer,
                ConstructType = MethodInvokeNode.enParamConstructType.MethodCustomInvoke_In,
                ParamInfo = param.CreateParamInfoAssist(FieldDirection.In),
            };
            var paramCtrl = new MethodInvokeParameterControl(pm);
            param.OnParamTypeChanged -= paramCtrl.UpdateParamType;
            param.OnParamTypeChanged += paramCtrl.UpdateParamType;
            param.OnParamTypeChanged -= UpdateInParamType;
            param.OnParamTypeChanged += UpdateInParamType;
            InsertChildNodeNoChangeContainer(index, paramCtrl, mParamsPanel);

            CreateTemplateClass(param.HostMethodInfo);
        }
        public async Task OnRemovedInParam(int index, CustomMethodInfo.FunctionParam param)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            var node = mChildNodes[index] as MethodInvokeParameterControl;
            if (node != null)
            {
                if (node.ParamPin.HasLink)
                {
                    var pinInfo = node.ParamPin;
                    pinInfo.Clear();
                }

                param.OnParamTypeChanged -= node.UpdateParamType;
                param.OnParamTypeChanged -= UpdateInParamType;
                RemoveChildNodeByIndex(index);
            }

            CreateTemplateClass(param.HostMethodInfo);
        }
        public async Task OnAddedOutParam(CustomMethodInfo.FunctionParam param)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            var pm = new MethodInvokeParameterControl.MethodInvokeParameterConstructionParams()
            {
                CSType = CSParam.CSType,
                HostNodesContainer = CSParam.HostNodesContainer,
                ConstructParam = "",
                ConstructType = MethodInvokeNode.enParamConstructType.MethodCustomInvoke_Out,
                ParamInfo = param.CreateParamInfoAssist(System.CodeDom.FieldDirection.Out),
            };
            var pc = new MethodInvokeParameterControl(pm);
            param.OnParamTypeChanged -= pc.UpdateParamType;
            param.OnParamTypeChanged += pc.UpdateParamType;
            AddChildNodeNoChanageContainer(pc, mValuesPanel);
        }
        public async Task OnInsertOutParam(int index, CustomMethodInfo.FunctionParam param)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            var pm = new MethodInvokeParameterControl.MethodInvokeParameterConstructionParams()
            {
                CSType = CSParam.CSType,
                HostNodesContainer = CSParam.HostNodesContainer,
                ConstructParam = "",
                ConstructType = MethodInvokeNode.enParamConstructType.MethodCustomInvoke_Out,
                ParamInfo = param.CreateParamInfoAssist(System.CodeDom.FieldDirection.Out),
            };
            var pc = new MethodInvokeParameterControl(pm);
            param.OnParamTypeChanged -= pc.UpdateParamType;
            param.OnParamTypeChanged += pc.UpdateParamType;
            InsertChildNodeNoChangeContainer(index, pc, mValuesPanel);
        }
        public async Task OnRemovedOutParam(int index, CustomMethodInfo.FunctionParam param)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            var node = mChildNodes[index] as MethodInvokeParameterControl;
            if (node != null)
            {
                if (node.ParamPin.HasLink)
                {
                    var pinInfo = node.ParamPin;
                    pinInfo.Clear();
                }

                param.OnParamTypeChanged -= node.UpdateParamType;
                RemoveChildNodeByIndex(index);
            }
        }

        public void UpdateInParamType(CodeDomNode.VariableType newType)
        {
            var param = CSParam as MethodCustomInvokeConstructParam;
            CreateTemplateClass(param.MethodInfo);
        }

        CodeGenerateSystem.Base.GeneratorClassBase mTemplateClassInstance = null;
        public CodeGenerateSystem.Base.GeneratorClassBase TemplateClassInstance
        {
            get { return mTemplateClassInstance; }
        }

        partial void InitConstruction();
        public MethodCustomInvoke(CodeGenerateSystem.Base.ConstructionParams csParam)
            : base(csParam)
        {
            InitConstruction();

            AddLinkPinInfo("CtrlMethodPin_Pre", mCtrlMethodPin_Pre);
            AddLinkPinInfo("CtrlMethodPin_Next", mCtrlMethodPin_Next);

            var param = csParam as MethodCustomInvokeConstructParam;
            param.MethodInfo.AddInParamOperation(this);
            param.MethodInfo.AddOutParamOperation(this);

            mDoNotCreateTemplateClass = true;
            mParamsPanel?.Children.Clear();
            var noUse = SetParams(param.MethodInfo);
            mDoNotCreateTemplateClass = false;
            CreateTemplateClass(param.MethodInfo);
        }
        async Task SetParams(CustomMethodInfo methodInfo)
        {
            foreach (var methodParam in methodInfo.InParams)
                await OnAddedInParam(methodParam);
            foreach (var methodParam in methodInfo.OutParams)
                await OnAddedOutParam(methodParam);
        }
        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            CollectLinkPinInfo(smParam, "CtrlMethodPin_Pre", CodeGenerateSystem.Base.enLinkType.Method, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, true);
            CollectLinkPinInfo(smParam, "CtrlMethodPin_Next", CodeGenerateSystem.Base.enLinkType.Method, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, false);
        }

        bool mDoNotCreateTemplateClass = false;
        public void CreateTemplateClass(CustomMethodInfo methodInfo)
        {
            if (mDoNotCreateTemplateClass)
                return;
            var csParam = CSParam as MethodCustomInvokeConstructParam;
            var oldTemplateClassIns = mTemplateClassInstance;

            var cpInfos = new List<CodeGenerateSystem.Base.CustomPropertyInfo>();
            var className = csParam.MethodInfo.MethodName;
            foreach (var methodParam in methodInfo.InParams)
            {
                var cpInfo = new CodeGenerateSystem.Base.CustomPropertyInfo();
                cpInfo.PropertyName = methodParam.ParamName;
                var paramType = methodParam.ParamType.GetActualType();
                cpInfo.PropertyType = paramType;
                //cpInfo.PropertyAttributes.Add() asdasd
                if (methodParam.Attributes.Count > 0)
                {
                    for (int i = 0; i < methodParam.Attributes.Count; i++)
                    {
                        cpInfo.PropertyAttributes.Add(methodParam.Attributes[i]);
                    }
                }
                
                cpInfos.Add(cpInfo);

                className += "_" + paramType.ToString().Replace("+", "_");
            }
            mTemplateClassInstance = CodeGenerateSystem.Base.PropertyClassGenerator.CreateClassInstanceFromCustomPropertys(cpInfos, this, className, false);
            InitTemplateClass_WPF(cpInfos);
            if(oldTemplateClassIns != null)
            {
                var newTemCType = mTemplateClassInstance.GetType();
                var pros = oldTemplateClassIns.GetType().GetProperties();
                foreach (var pro in pros)
                {
                    var val = pro.GetValue(oldTemplateClassIns);
                    var newPro = newTemCType.GetProperty(pro.Name);
                    if (newPro != null && newPro.PropertyType == pro.PropertyType)
                        newPro.SetValue(mTemplateClassInstance, val);
                }
            }
        }
        partial void InitTemplateClass_WPF(List<CodeGenerateSystem.Base.CustomPropertyInfo> propertys);

        public override void Save(XndNode xndNode, bool newGuid)
        {
            var proValAtt = xndNode.AddAttrib("ProValData");
            proValAtt.BeginWrite();
            CodeGenerateSystem.Base.PropertyClassGenerator.SaveClassInstanceProperties(mTemplateClassInstance, proValAtt);
            proValAtt.EndWrite();

            base.Save(xndNode, newGuid);
        }
        public override async System.Threading.Tasks.Task Load(XndNode xndNode)
        {
            await base.Load(xndNode);

            var proValAtt = xndNode.FindAttrib("ProValData");
            if(proValAtt != null)
            {
                proValAtt.BeginRead();
                PropertyClassGenerator.LoadClassInstanceProperties(mTemplateClassInstance, proValAtt);
                proValAtt.EndRead();
            }
        }

        public CustomMethodInfo GetMethodInfo()
        {
            var pm = CSParam as MethodCustomInvokeConstructParam;
            return pm.MethodInfo;
        }
        partial void ResetMethodInfo_WPF(CustomMethodInfo methodInfo);
        public async Task ResetMethodInfo(CustomMethodInfo methodInfo)
        {
            mDoNotCreateTemplateClass = true;
            // 加载完成后处理
            var pp = mCSParam as MethodCustomInvokeConstructParam;
            pp.MethodInfo.RemoveInParamOperation(this);

            int outParamStart = pp.MethodInfo.InParams.Count;

            // Out
            // 比较新旧MethodInfo参数，对参数进行增删
            for(int i=0; i<methodInfo.OutParams.Count; i++)
            {
                if(i >= pp.MethodInfo.OutParams.Count)
                {
                    await OnAddedOutParam(methodInfo.OutParams[i]);
                }
                else
                {
                    if(pp.MethodInfo.OutParams[i].IsEqual(methodInfo.OutParams[i]))
                    {
                        var pm = mChildNodes[i + outParamStart] as MethodInvokeParameterControl;
                        pm.ResetParamInfo(methodInfo.OutParams[i], FieldDirection.Out);
                    }
                    else
                    {
                        await OnRemovedOutParam(i + outParamStart, pp.MethodInfo.OutParams[i]);
                        await OnInsertOutParam(i + outParamStart, methodInfo.OutParams[i]);
                    }
                }
            }
            // 删除多余的参数
            for(int i=methodInfo.OutParams.Count; i<pp.MethodInfo.OutParams.Count; i++)
            {
                await OnRemovedOutParam(i + outParamStart, pp.MethodInfo.OutParams[i]);
            }

            // In
            // 比较新旧MethodInfo参数，对参数进行增删
            for (int i=0; i<methodInfo.InParams.Count; i++)
            {
                if(i >= pp.MethodInfo.InParams.Count)
                {
                    var tempIdx = outParamStart + i;
                    if (tempIdx >= mChildNodes.Count)
                        await OnAddedInParam(methodInfo.InParams[i]);
                    else
                        await OnInsertInParam(tempIdx, methodInfo.InParams[i]);
                }
                else
                {
                    if(pp.MethodInfo.InParams[i].IsEqual(methodInfo.InParams[i]))
                    {
                        var param = methodInfo.InParams[i];
                        var pm = mChildNodes[i] as MethodInvokeParameterControl;
                        pm.ResetParamInfo(param, FieldDirection.In);
                        param.OnParamTypeChanged -= UpdateInParamType;
                        param.OnParamTypeChanged += UpdateInParamType;
                    }
                    else
                    {
                        await OnRemovedInParam(i, pp.MethodInfo.InParams[i]);
                        if (i >= mChildNodes.Count)
                            await OnAddedInParam(methodInfo.InParams[i]);
                        else
                            await OnInsertInParam(i, methodInfo.InParams[i]);
                    }
                }
            }

            pp.MethodInfo = methodInfo;
            ResetMethodInfo_WPF(methodInfo);

            methodInfo.AddInParamOperation(this);
            methodInfo.AddOutParamOperation(this);

            mDoNotCreateTemplateClass = false;
            CreateTemplateClass(methodInfo);
        }

        System.CodeDom.CodeVariableDeclarationStatement mVariableDeclarationStatement = null;
        Dictionary<string, System.CodeDom.CodeVariableDeclarationStatement> mParamDeclarationStatementsDic = new Dictionary<string, CodeVariableDeclarationStatement>();
        Dictionary<string, System.CodeDom.CodeStatementCollection> mParamDeclarationInitStatementsDic = new Dictionary<string, CodeStatementCollection>();
        public override async Task GCode_CodeDom_GenerateCode(CodeTypeDeclaration codeClass, CodeStatementCollection codeStatementCollection, LinkPinControl element, GenerateCodeContext_Method context)
        {
            var consParam = CSParam as MethodCustomInvokeConstructParam;
            if (element == mCtrlMethodPin_Pre)
            {
                MethodInvokeParameterControl retCtrl = null;
                string retValName = "retVal_" + EngineNS.Editor.Assist.GetValuedGUIDString(this.Id);

                int tempParamNamesCount = 0;
                foreach (var paramNode in mChildNodes)
                {
                    if (paramNode is MethodInvokeParameterControl)
                    {
                        var invokeParam = paramNode as MethodInvokeParameterControl;
                        if(invokeParam.ParamFlag == FieldDirection.Out && consParam.MethodInfo.IsAsync)
                        {
                            retCtrl = invokeParam;
                        }
                        else
                            tempParamNamesCount++;
                    }
                }
                if (retCtrl != null)
                    retValName = retCtrl.GCode_GetValueName(null, context);
                var tempParamNames = new CodeExpression[tempParamNamesCount];

                int i = 0;
                if(tempParamNames.Length > 0)
                {
                    foreach (var paramNode in mChildNodes)
                    {
                        if (paramNode is MethodInvokeParameterControl)
                        {
                            var paramCtrl = paramNode as MethodInvokeParameterControl;
                
                            if (paramCtrl.ParamFlag == FieldDirection.Out && consParam.MethodInfo.IsAsync)
                                continue;
                
                            await paramCtrl.GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, paramCtrl.ParamPin, context);
                            var paramCtrlCSParam = paramCtrl.CSParam as MethodInvokeParameterControl.MethodInvokeParameterConstructionParams;
                
                            if (paramCtrl.IsUseLinkFromParamName())
                            {
                                tempParamNames[i] = paramCtrl.GetLinkFromParamName(context);
                            }
                            else
                            {
                                var tempParamName = paramCtrl.GCode_GetValueName(null, context);
                
                                CodeVariableDeclarationStatement varDec;
                                var initStatement = new CodeStatementCollection();
                                if (!mParamDeclarationStatementsDic.TryGetValue(tempParamName, out varDec))
                                {
                                    if (paramCtrl.HasLink())
                                        varDec = new CodeVariableDeclarationStatement(paramCtrl.ParamType, tempParamName, CodeGenerateSystem.Program.GetDefaultValueExpressionFromType(paramCtrl.ParamType));
                                    else
                                    {
                                        var proInfo = mTemplateClassInstance.GetType().GetProperty(paramCtrlCSParam.ParamInfo.ParamName);
                                        object proValue;
                                        if (proInfo == null)
                                            proValue = CodeGenerateSystem.Program.GetDefaultValueFromType(paramCtrl.ParamType);
                                        else
                                            proValue = proInfo.GetValue(mTemplateClassInstance);
                                        var initExp = Program.GetValueCode(initStatement, paramCtrl.ParamType, proValue);
                                        varDec = new CodeVariableDeclarationStatement(paramCtrl.ParamType, tempParamName, initExp);
                                    }
                                    mParamDeclarationStatementsDic[tempParamName] = varDec;
                                    mParamDeclarationInitStatementsDic[tempParamName] = initStatement;
                                }
                                else
                                {
                                    mParamDeclarationInitStatementsDic.TryGetValue(tempParamName, out initStatement);
                                }
                                if (!context.Method.Statements.Contains(varDec))
                                {
                                    context.Method.Statements.Insert(0, varDec);
                                    for (int stIdx = initStatement.Count - 1; stIdx >= 0; stIdx--)
                                    {
                                        context.Method.Statements.Insert(0, initStatement[stIdx]);
                                    }
                                }
                
                                tempParamNames[i] = new CodeVariableReferenceExpression(tempParamName);
                                if (paramCtrl.HasLink())
                                {
                                    var state = new CodeAssignStatement(tempParamNames[i], new CodeGenerateSystem.CodeDom.CodeCastExpression(paramCtrl.ParamType, paramCtrl.GCode_CodeDom_GetValue(null, context)));
                                    codeStatementCollection.Add(state);
                                }
                            }
                            i++;
                            //////switch(paramCtrlCSParam.ConstructType)
                            //////{
                            //////    case MethodInvokeNode.enParamConstructType.MethodCustomInvoke_In:
                            //////        {
                            //////            codeStatementCollection.Add(new System.CodeDom.CodeAssignStatement(
                            //////                                                            new System.CodeDom.CodeVariableReferenceExpression(paramCtrlCSParam.ParamInfo.ParamName),
                            //////                                                            paramCtrl.GCode_CodeDom_GetValue(paramCtrl.ParamPin, context)));
                            //////        }
                            //////        break;
                            //////    case MethodInvokeNode.enParamConstructType.MethodCustomInvoke_Out:
                            //////        {
                
                            //////        }
                            //////        break;
                            //////}
                        }
                    }
                }

                var debugCodes = BreakPoint.BeginMacrossDebugCodeStatments(codeStatementCollection);
                i = 0;
                if(tempParamNames.Length > 0)
                {
                    foreach (var paramNode in mChildNodes)
                    {
                        if (paramNode is MethodInvokeParameterControl)
                        {
                            var paramCtrl = paramNode as MethodInvokeParameterControl;
                            if (paramCtrl.ParamFlag == FieldDirection.Out && consParam.MethodInfo.IsAsync)
                                continue;

                            string typeStr;

                            if (paramCtrl.IsUseLinkFromParamName())
                                typeStr = paramCtrl.ParamPin.GetLinkedObject(0).GCode_GetTypeString(paramCtrl.ParamPin.GetLinkedPinControl(0), context);
                            else
                                typeStr = paramCtrl.GCode_GetTypeString(paramCtrl.ParamPin, context);
                            var ctrlCSParam = paramCtrl.CSParam as MethodInvokeParameterControl.MethodInvokeParameterConstructionParams;
                            BreakPoint.GetGatherDataValueCodeStatement(debugCodes, paramCtrl.ParamPin.GetLinkPinKeyName(), tempParamNames[i], typeStr, context);
                            i++;
                        }
                    }
                }
                // 调试用代码
                var breakCondStatement = CodeDomNode.BreakPoint.BreakCodeStatement(codeClass, debugCodes, HostNodesContainer.GUID, Id);
                System.CodeDom.CodeExpression[] expColls = new System.CodeDom.CodeExpression[tempParamNamesCount];
                // 设置数据代码
                int expIdx = 0;
                int paramNameIdx = 0;
                if(tempParamNames.Length > 0)
                {
                    foreach (var paramNode in mChildNodes)
                    {
                        if (paramNode is MethodInvokeParameterControl)
                        {
                            var paramCtrl = paramNode as MethodInvokeParameterControl;
                            if (paramCtrl.ParamFlag == FieldDirection.Out && consParam.MethodInfo.IsAsync)
                                continue;
                            var ctrlCSParam = paramCtrl.CSParam as MethodInvokeParameterControl.MethodInvokeParameterConstructionParams;
                            Type type;
                            if (paramCtrl.IsUseLinkFromParamName())
                                type = paramCtrl.ParamPin.GetLinkedObject(0).GCode_GetType(paramCtrl.ParamPin.GetLinkedPinControl(0), context);
                            else
                                type = paramCtrl.GCode_GetType(paramCtrl.ParamPin, context);
                            CodeDomNode.BreakPoint.GetSetDataValueCodeStatement(breakCondStatement.TrueStatements, paramCtrl.ParamPin.GetLinkPinKeyName(), tempParamNames[paramNameIdx], type);

                            expColls[expIdx] = new System.CodeDom.CodeDirectionExpression(paramCtrl.ParamFlag, tempParamNames[paramNameIdx]);
                            paramNameIdx++;
                            expIdx++;
                        }
                    }
                }
                CodeDomNode.BreakPoint.EndMacrossDebugCodeStatements(codeStatementCollection, debugCodes);

                // 函数调用
                var methodRef = new System.CodeDom.CodeMethodReferenceExpression();
                bool needAwait = false;
                if (consParam.MethodInfo.IsAsync)
                    needAwait = context.IsReturnTypeIsTask();
                if(needAwait)
                {
                    methodRef.TargetObject = new CodeVariableReferenceExpression("await this");
                }
                else
                {
                    methodRef.TargetObject = new CodeThisReferenceExpression();
                }
                methodRef.MethodName = consParam.MethodInfo.MethodName;
                var methodInvokeExp = new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(methodRef, expColls);
                if (retCtrl == null)
                {
                    codeStatementCollection.Add(new CodeExpressionStatement(methodInvokeExp));
                }
                else
                {
                    bool exist = false;
                    foreach (var statement in context.Method.Statements)
                    {
                        if (statement is CodeVariableDeclarationStatement)
                        {
                            var state = statement as CodeVariableDeclarationStatement;
                            if (state != null && mVariableDeclarationStatement != null && state.Name == mVariableDeclarationStatement.Name)
                            {
                                exist = true;
                                break;
                            }
                        }
                    }


                    if (retCtrl.HasLink())
                    {
                        if (!exist)
                        {
                            mVariableDeclarationStatement = new CodeVariableDeclarationStatement(retCtrl.GCode_GetType(null, context), retValName);
                            codeStatementCollection.Add(mVariableDeclarationStatement);
                        }

                        if (needAwait)
                        {
                            codeStatementCollection.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(retValName), methodInvokeExp));
                        }
                        else
                        {
                            var tempTaskValueName = "task_" + EngineNS.Editor.Assist.GetValuedGUIDString(Guid.NewGuid());
                            codeStatementCollection.Add(new CodeVariableDeclarationStatement($"System.Threading.Tasks.Task<{EngineNS.Rtti.RttiHelper.GetAppTypeString(retCtrl.GCode_GetType(null, context))}>", tempTaskValueName, methodInvokeExp));
                            var conditionStatement = new CodeConditionStatement();
                            conditionStatement.Condition = new CodeBinaryOperatorExpression(new CodePropertyReferenceExpression(
                                                                                                        new CodeVariableReferenceExpression(tempTaskValueName), "IsCompleted"),
                                                                                                        CodeBinaryOperatorType.ValueEquality,
                                                                                                        new CodePrimitiveExpression(true));
                            conditionStatement.TrueStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(retValName),
                                                                                          new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(tempTaskValueName), "Result")));
                            conditionStatement.FalseStatements.Add(new CodeThrowExceptionStatement(new CodeObjectCreateExpression(typeof(System.InvalidProgramException), new CodePrimitiveExpression($"异步函数 {consParam.MethodInfo.MethodName} 操作未执行完成!"))));
                            codeStatementCollection.Add(conditionStatement);
                        }
                    }
                    else
                    {
                        codeStatementCollection.Add(new CodeVariableDeclarationStatement("var", retValName, methodInvokeExp));
                    }
                }

                paramNameIdx = 0;
                if(tempParamNames.Length > 0)
                {
                    foreach (var paramNode in mChildNodes)
                    {
                        if (paramNode is MethodInvokeParameterControl)
                        {
                            var paramCtrl = paramNode as MethodInvokeParameterControl;
                            if (paramCtrl.ParamFlag == FieldDirection.Out && consParam.MethodInfo.IsAsync)
                                continue;
                            if (paramCtrl.NeedCopyParam())
                            {
                                codeStatementCollection.Add(new CodeAssignStatement(paramCtrl.GCode_CodeDom_GetValue(null, context), tempParamNames[paramNameIdx]));
                            }
                            paramNameIdx++;
                        }
                    }
                }

                // 返回值获取
                if (retCtrl != null && retCtrl.HasLink())
                {
                    debugCodes = CodeDomNode.BreakPoint.BeginMacrossDebugCodeStatments(codeStatementCollection);
                    CodeDomNode.BreakPoint.GetGatherDataValueCodeStatement(debugCodes, retCtrl.ParamPin.GetLinkPinKeyName(),
                                                                            new CodeVariableReferenceExpression(retValName),
                                                                            retCtrl.GCode_GetTypeString(null, context), context);
                    CodeDomNode.BreakPoint.EndMacrossDebugCodeStatements(codeStatementCollection, debugCodes);
                }

                if(context.GenerateNext)
                {
                    if (mCtrlMethodPin_Next.HasLink)
                        await mCtrlMethodPin_Next.GetLinkedObject(0, false).GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, mCtrlMethodPin_Next.GetLinkedPinControl(0, false), context);
                }
            }
        }
    }
}
