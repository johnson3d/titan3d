using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using EngineNS.IO;

namespace CodeDomNode
{
    [CodeGenerateSystem.CustomConstructionParams(typeof(ReturnCustomConstructParam))]
    public partial class ReturnCustom : CodeGenerateSystem.Base.BaseNodeControl, CodeDomNode.CustomMethodInfo.IFunctionOutParamOperation, CustomMethodInfo.IFunctionResetOperationNode
    {
        [EngineNS.Rtti.MetaClass]
        public class ReturnCustomConstructParam : CodeGenerateSystem.Base.ConstructionParams
        {
            [EngineNS.Rtti.MetaData]
            public CustomMethodInfo MethodInfo { get; set; }

            public enum enShowPropertyType
            {
                MethodInfo,
                ReturnValue,
                None,
            }
            [EngineNS.Rtti.MetaData]
            public enShowPropertyType ShowPropertyType
            {
                get;
                set;
            } = enShowPropertyType.MethodInfo;

            public ReturnCustomConstructParam()
            {
            }
            public override EditorCommon.CodeGenerateSystem.INodeConstructionParams Duplicate()
            {
                var retVal = base.Duplicate() as ReturnCustomConstructParam;
                retVal.MethodInfo = MethodInfo;
                retVal.ShowPropertyType = ShowPropertyType;
                return retVal;
            }
            public override bool Equals(object obj)
            {
                var param = obj as ReturnCustomConstructParam;
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

        // 临时类，用于选中后显示参数属性
        CodeGenerateSystem.Base.GeneratorClassBase mTemplateClassInstance = null;
        public CodeGenerateSystem.Base.GeneratorClassBase TemplateClassInstance
        {
            get { return mTemplateClassInstance; }
        }

        CodeGenerateSystem.Base.LinkPinControl mCtrlMethodLink = new CodeGenerateSystem.Base.LinkPinControl();
        partial void InitConstruction();
        public ReturnCustom(CodeGenerateSystem.Base.ConstructionParams csParam)
            : base(csParam)
        {
            InitConstruction();
            NodeName = "Return";

            var param = csParam as ReturnCustomConstructParam;

            SetParameters(param.MethodInfo);

            AddLinkPinInfo("CtrlMethodLink", mCtrlMethodLink, null);
        }
        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            CollectLinkPinInfo(smParam, "CtrlMethodLink", CodeGenerateSystem.Base.enLinkType.Method, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, true);
        }

        public void CreateTemplateClass(List<CodeGenerateSystem.Base.CustomPropertyInfo> cpInfos)
        {
            var csParam = CSParam as ReturnCustomConstructParam;
            mTemplateClassInstance = CodeGenerateSystem.Base.PropertyClassGenerator.CreateClassInstanceFromCustomPropertys(cpInfos, this, $"CustomReturn_{csParam.MethodInfo.MethodName}_{Guid.NewGuid().ToString().Replace("-","_")}", false);
            InitTemplateClass_WPF(cpInfos);
        }
        partial void InitTemplateClass_WPF(List<CodeGenerateSystem.Base.CustomPropertyInfo> propertys);

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
            att.EndWrite();
            base.Save(xndNode, newGuid);
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

                foreach(var child in mChildNodes)
                {
                    var childParameterCtrl = child as MethodInvokeParameterControl;
                    if(childParameterCtrl != null)
                    {
                        childParameterCtrl.OnUpdateParamTypeAction = OnUpdateChildParamType;
                    }
                }
            }
            catch(System.Exception e)
            {
                HasError = true;
                ErrorDescription = e.ToString();
            }

            InitTemplateClass_WPF(null);
        }

        public override bool CheckCanDelete()
        {
            if (!base.CheckCanDelete())
                return false;

            HostNodesContainer.OnBeferDeleteNodeControl -= ReturnCustom_HostNodesContainer_OnBeferDeleteNodeControl;
            HostNodesContainer.OnDeletedNodeControl -= ReturnCustom_HostNodesContainer_OnDeletedNodeControl;
            int retCount = 0;
            for(int i=0; i<HostNodesContainer.CtrlNodeList.Count; i++)
            {
                var node = HostNodesContainer.CtrlNodeList[i];
                if (node is ReturnCustom)
                    retCount++;
            }
            if(retCount == 1)
            {
                if (EditorCommon.MessageBox.Show("删除后将删除所有返回参数，是否继续？", EditorCommon.MessageBox.enMessageBoxButton.YesNo) != EditorCommon.MessageBox.enMessageBoxResult.Yes)
                    return false;
                HostNodesContainer.OnBeferDeleteNodeControl += ReturnCustom_HostNodesContainer_OnBeferDeleteNodeControl;
                HostNodesContainer.OnDeletedNodeControl += ReturnCustom_HostNodesContainer_OnDeletedNodeControl;
            }

            return true;
        }

        private void ReturnCustom_HostNodesContainer_OnBeferDeleteNodeControl(CodeGenerateSystem.Base.BaseNodeControl node)
        {
            var param = CSParam as ReturnCustomConstructParam;
            for (int i = 0; i < param.MethodInfo.OutParams.Count; i++)
            {
                var otParam = param.MethodInfo.OutParams[i];
                var chd = mChildNodes[i] as MethodInvokeParameterControl;
                otParam.OnParamTypeChanged -= chd.UpdateParamType;
            }
        }

        private void ReturnCustom_HostNodesContainer_OnDeletedNodeControl(CodeGenerateSystem.Base.BaseNodeControl node)
        {
            // 清除OutParams
            var param = CSParam as ReturnCustomConstructParam;
            param.MethodInfo.OutParams.Clear();
            param.MethodInfo.RemoveOutParamOperation(this);
            //param.MethodInfo.OnAddedOutParam -= ((ReturnCustom)node).ReturnCustom_AddParam;
            //param.MethodInfo.OnRemovedOutParam -= ((ReturnCustom)node).ReturnCustom_RemoveParam;
        }

        StackPanel mParamsPanel = null;
        // 这里不做参数在PropertyGrid中的显示设置，防止类似Ref参数被无意设置
        void SetParameters(CustomMethodInfo methodInfo)
        {
            var param = CSParam as ReturnCustomConstructParam;
            //param.MethodInfo.OnAddedOutParam -= ReturnCustom_AddParam;
            //param.MethodInfo.OnAddedOutParam += ReturnCustom_AddParam;
            //param.MethodInfo.OnRemovedOutParam -= ReturnCustom_RemoveParam;
            //param.MethodInfo.OnRemovedOutParam += ReturnCustom_RemoveParam;
            //param.MethodInfo.AddOutParamOperation(this);

            param.MethodInfo.AddOutParamOperation(this);

            var cpInfos = new List<CodeGenerateSystem.Base.CustomPropertyInfo>(methodInfo.OutParams.Count);
            mParamsPanel?.Children.Clear();
            foreach (var methodParam in methodInfo.OutParams)
            {
                var paramType = methodParam.ParamType.GetActualType();
                if(Program.IsTypeValidInPropertyGridShow(paramType))
                {
                    var infoAssist = new MethodParamInfoAssist()
                    {
                        FieldDirection = System.CodeDom.FieldDirection.Out,
                        IsParamsArray = false,
                        ParameterType = paramType,
                        ParamName = methodParam.ParamName,
                    };
                    var cpInfo = Program.GetFromParamInfo(infoAssist);
                    cpInfos.Add(cpInfo);
                }

                //ReturnCustom_AddParam(methodParam);
                var noUse = OnAddedOutParam(methodParam);
            }

            if(param.ShowPropertyType == ReturnCustomConstructParam.enShowPropertyType.ReturnValue)
                CreateTemplateClass(cpInfos);
        }

        //void ReturnCustom_AddParam(CustomMethodInfo.FunctionParam funcParam)
        public async Task OnAddedOutParam(CodeDomNode.CustomMethodInfo.FunctionParam funcParam)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            var csParam = CSParam as ReturnCustomConstructParam;
            MethodParamInfoAssist retValue;
            if (funcParam.ParamType.Type.IsByRef)
            {
                var typefullname = funcParam.ParamType.Type.FullName.Substring(0, funcParam.ParamType.Type.FullName.Length - 1);
                var type = funcParam.ParamType.Type.Assembly.GetType(typefullname);
                //funcParam.ParamType = new VariableType(type, funcParam.ParamType.CSType);
                retValue = new MethodParamInfoAssist();
                retValue.FieldDirection = System.CodeDom.FieldDirection.Out;
                retValue.IsParamsArray = false;
                retValue.ParameterType = type;
                retValue.ParamName = funcParam.ParamName;
                BindingOperations.SetBinding(this, CodeDomNode.CustomMethodInfo.FunctionParam.ParamNameProperty, new Binding("ParamName") { Source = retValue, Mode = BindingMode.TwoWay });
            }
            else
            {
                retValue = funcParam.CreateParamInfoAssist(System.CodeDom.FieldDirection.Out);
            }
            
            var pm = new MethodInvokeParameterControl.MethodInvokeParameterConstructionParams()
            {
                CSType = CSParam.CSType,
                HostNodesContainer = CSParam.HostNodesContainer,
                ConstructParam = "",
                ConstructType = MethodInvokeNode.enParamConstructType.ReturnCustom,
                ParamInfo = retValue,
            };
            var pc = new MethodInvokeParameterControl(pm);
            funcParam.OnParamTypeChanged -= pc.UpdateParamType;
            funcParam.OnParamTypeChanged += pc.UpdateParamType;
            pc.OnUpdateParamTypeAction = OnUpdateChildParamType;
            AddChildNode(pc, mParamsPanel);
        }
        public async Task OnInsertOutParam(int index, CodeDomNode.CustomMethodInfo.FunctionParam funcParam)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            var csParam = CSParam as ReturnCustomConstructParam;
            var pm = new MethodInvokeParameterControl.MethodInvokeParameterConstructionParams()
            {
                CSType = CSParam.CSType,
                HostNodesContainer = CSParam.HostNodesContainer,
                ConstructParam = "",
                ConstructType = MethodInvokeNode.enParamConstructType.ReturnCustom,
                ParamInfo = funcParam.CreateParamInfoAssist(System.CodeDom.FieldDirection.Out),
            };
            var pc = new MethodInvokeParameterControl(pm);
            funcParam.OnParamTypeChanged -= pc.UpdateParamType;
            funcParam.OnParamTypeChanged += pc.UpdateParamType;
            pc.OnUpdateParamTypeAction = OnUpdateChildParamType;
            InsertChildNode(index, pc, mParamsPanel);
        }
        //void ReturnCustom_RemoveParam(int index, CustomMethodInfo.FunctionParam funcParam)
        public async Task OnRemovedOutParam(int index, CodeDomNode.CustomMethodInfo.FunctionParam param)
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

        public CustomMethodInfo GetMethodInfo()
        {
            var pm = CSParam as ReturnCustomConstructParam;
            return pm.MethodInfo;
        }

        partial void ResetMethodInfo_WPF(CodeDomNode.CustomMethodInfo methodInfo);
        public async Task ResetMethodInfo(CodeDomNode.CustomMethodInfo methodInfo)
        {
            var pp = mCSParam as ReturnCustomConstructParam;
            pp.MethodInfo.RemoveOutParamOperation(this);

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
                        var pm = mChildNodes[i] as MethodInvokeParameterControl;
                        pm.ResetParamInfo(methodInfo.OutParams[i], System.CodeDom.FieldDirection.Out);
                    }
                    else
                    {
                        await OnRemovedOutParam(i, pp.MethodInfo.OutParams[i]);
                        await OnInsertOutParam(i, methodInfo.OutParams[i]);
                    }
                }
            }
            // 删除多余的参数
            for(int i=methodInfo.OutParams.Count; i<pp.MethodInfo.OutParams.Count; i++)
            {
                await OnRemovedOutParam(i, pp.MethodInfo.OutParams[i]);
            }

            pp.MethodInfo = methodInfo;
            ResetMethodInfo_WPF(methodInfo);

            methodInfo.AddOutParamOperation(this);
        }
        void OnUpdateChildParamType(MethodInvokeParameterControl ctrl, CodeDomNode.VariableType newType)
        {
            // 类型改变，重新创建TemplateClass;
            var param = CSParam as ReturnCustomConstructParam;
            if(param.ShowPropertyType == ReturnCustomConstructParam.enShowPropertyType.ReturnValue)
            {
                var cpInfos = new List<CodeGenerateSystem.Base.CustomPropertyInfo>(param.MethodInfo.OutParams.Count);
                foreach (var methodParam in param.MethodInfo.OutParams)
                {
                    var paramType = methodParam.ParamType.GetActualType();
                    if (Program.IsTypeValidInPropertyGridShow(paramType))
                    {
                        var infoAssist = new MethodParamInfoAssist()
                        {
                            FieldDirection = System.CodeDom.FieldDirection.Out,
                            IsParamsArray = false,
                            ParameterType = paramType,
                            ParamName = methodParam.ParamName,
                        };
                        var cpInfo = Program.GetFromParamInfo(infoAssist);
                        cpInfos.Add(cpInfo);
                    }
                }
                var tempClass = mTemplateClassInstance;
                if(tempClass != null)
                {
                    var oldTempClassType = tempClass.GetType();
                    CreateTemplateClass(cpInfos);
                    foreach (var pro in mTemplateClassInstance.GetType().GetProperties())
                    {
                        var oldPro = oldTempClassType.GetProperty(pro.Name);
                        if (oldPro==null || oldPro.PropertyType != pro.PropertyType)
                            continue;

                        pro.SetValue(mTemplateClassInstance, oldPro.GetValue(tempClass));
                    }
                }
            }
        }

        public override async System.Threading.Tasks.Task GCode_CodeDom_GenerateCode(System.CodeDom.CodeTypeDeclaration codeClass, System.CodeDom.CodeStatementCollection codeStatementCollection, CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            MethodInvokeParameterControl retCtrl = null;
            Dictionary<CodeGenerateSystem.Base.BaseNodeControl, System.CodeDom.CodeExpression> paramCodeExps = new Dictionary<CodeGenerateSystem.Base.BaseNodeControl, System.CodeDom.CodeExpression>();
            var rcParam = CSParam as ReturnCustomConstructParam;
            if(rcParam.MethodInfo.IsAsync)
            {
                // 异步不能有out，只能有一个返回值
                if (mChildNodes.Count == 1)
                    retCtrl = mChildNodes[0] as MethodInvokeParameterControl;
            }
            else
            {
                foreach (var paramNode in mChildNodes)
                {
                    if (paramNode is MethodInvokeParameterControl)
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
                        else if (mTemplateClassInstance != null)
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
            }

            if (retCtrl != null)
            {
                if (retCtrl.HasLink())
                    await retCtrl.GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, retCtrl.ParamPin, context);

                System.CodeDom.CodeExpression retExp = null;
                if (retCtrl.HasLink())
                {
                    var retStatement = new System.CodeDom.CodeMethodReturnStatement();
                    retExp = retCtrl.GCode_CodeDom_GetValue(retCtrl.ParamPin, context);
                    paramCodeExps[retCtrl] = retExp;
                }
                else if (mTemplateClassInstance != null)
                {
                    var param = retCtrl.CSParam as MethodInvokeParameterControl.MethodInvokeParameterConstructionParams;
                    var proInfo = mTemplateClassInstance.GetType().GetProperty(param.ParamInfo.ParamName);
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
                foreach (var paramNode in mChildNodes)
                {
                    if (paramNode is MethodInvokeParameterControl)
                    {
                        var pm = paramNode as MethodInvokeParameterControl;
                        System.CodeDom.CodeExpression exp;
                        if (paramCodeExps.TryGetValue(paramNode, out exp))
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

                codeStatementCollection.Add(new System.CodeDom.CodeMethodReturnStatement(retExp));
            }
            else
            {
                #region Debug
                var debugCodes = CodeDomNode.BreakPoint.BeginMacrossDebugCodeStatments(codeStatementCollection);
                var breakCondStatement = CodeDomNode.BreakPoint.BreakCodeStatement(codeClass, debugCodes, HostNodesContainer.GUID, Id);
                CodeDomNode.BreakPoint.EndMacrossDebugCodeStatements(codeStatementCollection, debugCodes);
                #endregion
                codeStatementCollection.Add(new System.CodeDom.CodeMethodReturnStatement());
            }
        }
    }
}
