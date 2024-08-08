using System;
using System.Collections.Generic;
using EngineNS.Bricks.NodeGraph;
using MathNet.Numerics.LinearAlgebra.Factorization;

namespace EngineNS.Bricks.CodeBuilder.ShaderNode.Control
{
    //[Obsolete]
    public class UserCallNodeAttribute : Attribute
    {
        public Type CallNodeType;
    }
    public class CallNode : UNodeBase
    {
        public PinOut Result = null;
        public struct PinData
        {
            public PinIn PinIn;
            public PinOut PinOut;
            public EMethodArgumentAttribute OpType;
        }
        //public List<PinIn> Arguments = new List<PinIn>();
        //public List<PinOut> OutArguments = new List<PinOut>();
        public List<PinData> Arguments = new List<PinData>();
        public Rtti.UClassMeta.TtMethodMeta Method;
        [Rtti.Meta]
        public string MethodDeclString
        {
            get
            {
                if (Method == null)
                    return null;
                return Method.GetMethodDeclareString(true);
            }
            set
            {
                //var meta = Rtti.UClassMetaManager.Instance.GetMetaFromFullName(typeof(HLSLMethod).FullName);
                //Method = meta.GetMethod(value);

                Method = UEngine.Instance.HLSLMethodManager.GetMethodByDeclString(value);
                if (Method == null)
                {
                    var name = Rtti.UClassMeta.GetNameByDeclstring(value);
                    Method = UEngine.Instance.HLSLMethodManager.GetMethod(name);
                }
                this.Initialize(Method);
            }
        }
        public static CallNode NewMethodNode(Rtti.UClassMeta.TtMethodMeta m)
        {
            CallNode result = null;
            if (m.MethodName == "Sample2D")
            {
                result = new Sample2DNode();
            }
            else
            {
                result = new CallNode();
            }
            result.Initialize(m);
            return result;
        }
        public CallNode()
        {
            Icon = UShaderEditorStyles.Instance.FunctionIcon;
            TitleColor = UShaderEditorStyles.Instance.FunctionTitleColor;
            BackColor = UShaderEditorStyles.Instance.FunctionBGColor;
        }
        internal void Initialize(Rtti.UClassMeta.TtMethodMeta m)
        {
            Method = m;
            Name = m.MethodName;

            if (!m.ReturnType.IsEqual(typeof(void)))
            {
                Result = new PinOut();
                Result.LinkDesc = UShaderEditorStyles.Instance.NewInOutPinDesc();
                Result.Name = "Result";
                Result.MultiLinks = true;
                AddPinOut(Result);
            }

            Arguments.Clear();
            foreach(var i in m.Parameters)
            {
                var pinData = new PinData();
                pinData.OpType = EMethodArgumentAttribute.Default;
                if (i.IsOut)
                    pinData.OpType = EMethodArgumentAttribute.Out;
                else if (i.IsIn)
                    pinData.OpType = EMethodArgumentAttribute.In;
                else if (i.IsRef)
                    pinData.OpType = EMethodArgumentAttribute.Ref;

                if(!i.IsOut)
                {
                    var pin = new PinIn();
                    pin.LinkDesc = UShaderEditorStyles.Instance.NewInOutPinDesc();
                    pin.LinkDesc.CanLinks.Add("Value");
                    pin.Name = i.Name;
                    pin.Tag = i.ParameterType;
                    AddPinIn(pin);
                    pinData.PinIn = pin;
                }
                if(i.IsOut || i.IsRef)
                {
                    var pinOut = new PinOut();
                    pinOut.LinkDesc = UShaderEditorStyles.Instance.NewInOutPinDesc();
                    pinOut.LinkDesc.CanLinks.Add("Value");
                    pinOut.Name = i.Name;
                    pinOut.Tag = i.ParameterType;
                    pinOut.MultiLinks = true;
                    AddPinOut(pinOut);
                    pinData.PinOut = pinOut;
                }
                Arguments.Add(pinData);
            }
        }
        public override void OnMouseStayPin(NodePin pin, UNodeGraph graph)
        {
            if (pin == Result)
            {
                if (Result.Tag != null)
                {
                    var cvtType = Result.Tag as System.Type;
                    if (cvtType != null)
                    {
                        EGui.Controls.CtrlUtility.DrawHelper($"{cvtType.FullName}");
                        return;
                    }
                }
                EGui.Controls.CtrlUtility.DrawHelper($"{Method.ReturnType.FullName}");
                return;
            }
            var method = Method;
            for(int i=0; i<Arguments.Count; i++)
            {
                if(pin == Arguments[i].PinIn)
                {
                    var inPin = pin as PinIn;
                    var paramMeta = GetInPinParamMeta(inPin);
                    if(paramMeta != null)
                    {
                        EGui.Controls.CtrlUtility.DrawHelper($"{paramMeta.ParameterType.FullName}");
                    }
                    return;
                }
                if(pin == Arguments[i].PinOut)
                {
                    var paramMeta = method.FindParameter(pin.Name);
                    if(paramMeta != null)
                    {
                        EGui.Controls.CtrlUtility.DrawHelper($"{paramMeta.ParameterType.FullName}");
                    }
                    return;
                }
            }
            //for (int i = 0; i < Arguments.Count; i++)
            //{
            //    if (pin == Arguments[i])
            //    {
            //        var inPin = pin as PinIn;
            //        var paramMeta = GetInPinParamMeta(inPin);
            //        if (paramMeta != null)
            //        {
            //            EGui.Controls.CtrlUtility.DrawHelper($"{paramMeta.ParameterType.FullName}");
            //        }
            //        return;
            //    }
            //}
            //for (int i = 0; i < OutArguments.Count; i++)
            //{
            //    if (pin == OutArguments[i])
            //    {
            //        var paramMeta = Method.FindParameter(pin.Name);
            //        if (paramMeta != null)
            //        {
            //            EGui.Controls.CtrlUtility.DrawHelper($"{paramMeta.ParameterType.FullName}");
            //        }
            //        return;
            //    }
            //}
        }
        public Rtti.UClassMeta.TtMethodMeta.TtParamMeta GetInPinParamMeta(PinIn pin)
        {
            for (int i = 0; i < Arguments.Count; i++)
            {
                if (pin == Arguments[i].PinIn)
                {
                    return Method.GetParameter(i);
                }
            }
            return null;
        }
        public override Rtti.UTypeDesc GetOutPinType(PinOut pin)
        {
            if (pin == null)
                return null;

            if (pin == Result)
            {
                if (Result.Tag != null)
                {
                    var cvtType = Result.Tag as Rtti.UTypeDesc;
                    if (cvtType != null)
                        return cvtType;
                }
                return Method.ReturnType;
            }
            foreach (var i in Arguments)
            {
                if (pin == i.PinOut)
                {
                    foreach (var j in Method.Parameters)
                    {
                        if (j.Name == i.PinOut.Name && j.IsOut)
                        {
                            return j.ParameterType;
                        }
                    }
                }
            }
            return null;
        }
        public override bool CanLinkFrom(PinIn iPin, UNodeBase OutNode, PinOut oPin)
        {
            if (base.CanLinkFrom(iPin, OutNode, oPin) == false)
                return false;

            var nodeExpr = OutNode as UNodeBase;
            if (nodeExpr == null)
                return true;

            for (int i = 0; i < Arguments.Count; i++)
            {
                if (iPin == Arguments[i].PinIn)
                {
                    var testType = nodeExpr.GetOutPinType(oPin);
                    return CodeBuilder.UCodeGeneratorBase.CanConvert(testType, Method.GetParameter(i).ParameterType);
                }
            }
            return true;
        }
        //public override void PreGenExpr()
        //{
        //    Executed = false;
        //}
        //bool Executed = false;
        //[Obsolete]
        //public override IExpression GetExpr(UMaterialGraph funGraph, ICodeGen cGen, PinOut oPin, bool bTakeResult)
        //{
        //    if (Executed)
        //    {
        //        if (oPin == Result)
        //        {
        //            var mth_ret_temp_name = $"tmp_r_{Method.MethodName}_{(uint)this.NodeId.GetHashCode()}";
        //            return new OpUseVar(mth_ret_temp_name, false);
        //        }
        //        else
        //        {
        //            var parameters = Method.GetParameters();
        //            for (int i = 0; i < parameters.Length; i++)
        //            {
        //                if (parameters[i].Name == oPin.Name)
        //                {
        //                    System.Diagnostics.Debug.Assert(parameters[i].IsOut);
        //                    var mth_outarg_temp_name = $"tmp_o_{parameters[i].Name}_{Method.MethodName}_{(uint)this.NodeId.GetHashCode()}";
        //                    return new OpUseVar(mth_outarg_temp_name, false);
        //                }
        //            }
        //        }
        //        System.Diagnostics.Debug.Assert(false);
        //    }
        //    Executed = true;

        //    ConvertTypeOp cvtExpr = null;
        //    DefineVar retVar = null;

        //    if (!Method.ReturnType.IsEqual(typeof(void)))
        //    {
        //        var mth_ret_temp_name = $"tmp_r_{Method.MethodName}_{(uint)this.NodeId.GetHashCode()}";
        //        retVar = new DefineVar();
        //        retVar.IsLocalVar = true;
        //        retVar.DefType = cGen.GetTypeString(Method.ReturnType);
        //        retVar.VarName = mth_ret_temp_name;
        //        retVar.InitValue = cGen.GetDefaultValue(Method.ReturnType);
        //        funGraph.ShaderEditor.MaterialOutput.Function.AddLocalVar(retVar);

        //        if (Result != null && Result.Tag != null && ((Result.Tag as Rtti.UTypeDesc) != Method.ReturnType))
        //        {
        //            var cvtTargetType = (Result.Tag as System.Type);
        //            retVar.DefType = cvtTargetType.FullName;
        //            cvtExpr = new ConvertTypeOp();
        //            cvtExpr.TargetType = retVar.DefType;
        //        }
        //    }
            
        //    var callExpr = GetExpr_Impl(funGraph, cGen) as CallOp;

        //    if (retVar != null)
        //    {
        //        callExpr.FunReturnLocalVar = retVar.VarName;
        //    }
        //    if (cvtExpr != null)
        //    {
        //        callExpr.ConvertType = cvtExpr;
        //    }

        //    if (oPin == Result)
        //    {
        //        var mth_ret_temp_name = $"tmp_r_{Method.MethodName}_{(uint)this.NodeId.GetHashCode()}";
        //        callExpr.FunOutLocalVar = mth_ret_temp_name;
        //    }
        //    else
        //    {
        //        var parameters = Method.GetParameters();
        //        for (int i = 0; i < parameters.Length; i++)
        //        {
        //            if (parameters[i].Name == oPin.Name)
        //            {
        //                System.Diagnostics.Debug.Assert(parameters[i].IsOut);
        //                var mth_outarg_temp_name = $"tmp_o_{parameters[i].Name}_{Method.MethodName}_{(uint)this.NodeId.GetHashCode()}";
        //                callExpr.FunOutLocalVar = mth_outarg_temp_name;
        //            }
        //        }
        //    }

        //    return callExpr;
        //}
        //private IExpression GetExpr_Impl(UMaterialGraph funGraph, ICodeGen cGen)
        //{
        //    CallOp CallExpr = new CallOp();
        //    var links = new List<UPinLinker>();
            
        //    {
        //        //这里要处理Static名字获取
        //        //CallExpr.Host = selfExpr;
        //        CallExpr.IsStatic = true;
        //        CallExpr.Host = new HardCodeOp() { Code = "" };
        //        CallExpr.Name = Method.MethodName;
        //    }

        //    for (int i = 0; i < Arguments.Count; i++)
        //    {
        //        if (Method.Parameters[i].IsOut)
        //        {
        //            var mth_outarg_temp_name = $"tmp_o_{Method.Parameters[i].Name}_{Method.MethodName}_{(uint)this.NodeId.GetHashCode()}";
        //            var retVar = new DefineVar();
        //            retVar.IsLocalVar = true;
        //            retVar.DefType = cGen.GetTypeString(Method.Parameters[i].ParameterType);
        //            retVar.VarName = mth_outarg_temp_name;
        //            retVar.InitValue = cGen.GetDefaultValue(Method.Parameters[i].ParameterType);

        //            funGraph.ShaderEditor.MaterialOutput.Function.AddLocalVar(retVar);
        //            CallExpr.Arguments.Add(new OpUseDefinedVar(retVar));
        //            continue;
        //        }

        //        links.Clear();
        //        links = new List<UPinLinker>();
        //        funGraph.FindInLinker(Arguments[i], links);
        //        OpExpress argExpr = null;
        //        if (links.Count == 1)
        //        {
        //            var argNode = links[0].OutNode as IBaseNode;
        //            argExpr = argNode.GetExpr(funGraph, cGen, links[0].OutPin, true) as OpExpress;
        //            if (argExpr == null)
        //                throw new GraphException(this, Arguments[i], $"argExpr = null:{Arguments[i].Name}");
        //        }
        //        else if (links.Count == 0)
        //        {
        //            argExpr = OnNoneLinkedParameter(funGraph, cGen, i);
        //        }
        //        else
        //        {
        //            throw new GraphException(this, Arguments[i], $"Arg error:{Arguments[i].Name}");
        //        }
        //        CallExpr.Arguments.Add(argExpr);
        //    }

        //    return CallExpr;
        //}
        //protected virtual OpExpress OnNoneLinkedParameter(UMaterialGraph funGraph, ICodeGen cGen, int i)
        //{
        //    var mth_arg_temp_name = $"t_{Method.Parameters[i].Name}_{Method.MethodName}_{(uint)this.NodeId.GetHashCode()}";
        //    var retVar = new DefineVar();
        //    retVar.IsLocalVar = true;
        //    retVar.DefType = cGen.GetTypeString(Method.Parameters[i].ParameterType);
        //    retVar.VarName = mth_arg_temp_name;
        //    retVar.InitValue = cGen.GetDefaultValue(Method.Parameters[i].ParameterType);
        //    funGraph.ShaderEditor.MaterialOutput.Function.AddLocalVar(retVar);

        //    return new OpUseDefinedVar(retVar);
        //}

        string GetReturnValueName()
        {
            return $"tmp_r_{Method.MethodName}_{(uint)NodeId.GetHashCode()}";
        }
        string GetParamValueName(string paramName)
        {
            return $"tmp_o_{paramName}_{Method.MethodName}_{(uint)NodeId.GetHashCode()}";
        }

        protected virtual UExpressionBase GetNoneLinkedParameterExp(PinIn pin, int argIdx, ref BuildCodeStatementsData data)
        {
            var paramName = GetParamValueName(pin.Name);
            if (!data.MethodDec.HasLocalVariable(paramName))
            {
                var arg = Method.FindParameter(pin.Name);
                var type = pin.Tag as Rtti.UTypeDesc;
                var varDec = new UVariableDeclaration()
                {
                    VariableType = new UTypeReference(type),
                    VariableName = paramName,
                    InitValue = (type.IsPrimitive && arg.DefaultValue != null && arg.DefaultValue.GetType()!=typeof(System.DBNull)) ? new UPrimitiveExpression(type, arg.DefaultValue) : new UDefaultValueExpression(type),
                };
                data.MethodDec.AddLocalVar(varDec);
            }
            var retVal = new UVariableReferenceExpression(paramName);
            return retVal;
        }

        void GenArgumentCodes(int argIdx, ref BuildCodeStatementsData data, out UExpressionBase exp,
            List<UStatementBase> beforeStatements = null,
            List<UStatementBase> afterStatements = null)
        {
            var pinData = Arguments[argIdx];
            switch(pinData.OpType)
            {
                case EMethodArgumentAttribute.Out:
                    {
                        var outPin = pinData.PinOut;
                        var paramName = GetParamValueName(outPin.Name);
                        exp = new UVariableReferenceExpression(paramName);

                        if(!data.MethodDec.HasLocalVariable(paramName))
                        {
                            var type = outPin.Tag as Rtti.UTypeDesc;
                            var varDec = new UVariableDeclaration()
                            {
                                VariableType = new UTypeReference(type),
                                VariableName = paramName,
                                InitValue = new UDefaultValueExpression(type),
                            };
                            data.MethodDec.AddLocalVar(varDec);
                        }
                    }
                    break;
                case EMethodArgumentAttribute.Ref:
                case EMethodArgumentAttribute.In:
                    {
                        var inPin = pinData.PinIn;
                        if(data.NodeGraph.PinHasLinker(inPin))
                        {
                            var opPin = data.NodeGraph.GetOppositePin(inPin);
                            var opNode = data.NodeGraph.GetOppositePinNode(inPin);
                            opNode.BuildStatements(opPin, ref data);
                            exp = data.NodeGraph.GetOppositePinExpression(inPin, ref data);
                        }
                        else
                        {
                            var paramName = GetParamValueName(inPin.Name);
                            exp = GetNoneLinkedParameterExp(inPin, argIdx, ref data);
                        }
                    }
                    break;
                default:
                    {
                        var inPin = pinData.PinIn;
                        if (data.NodeGraph.PinHasLinker(inPin))
                        {
                            var opPin = data.NodeGraph.GetOppositePin(inPin);
                            var opNode = data.NodeGraph.GetOppositePinNode(inPin);
                            opNode.BuildStatements(opPin, ref data);
                            exp = data.NodeGraph.GetOppositePinExpression(inPin, ref data);
                        }
                        else
                            exp = GetNoneLinkedParameterExp(inPin, argIdx, ref data);
                    }
                    break;
            }
        }

        public override void BuildStatements(NodePin pin, ref BuildCodeStatementsData data)
        {
            var method = Method;
            var incAttr = Method.DeclaringType.GetCustomAttribute<EngineNS.Bricks.CodeBuilder.ShaderNode.Control.TtHLSLProviderAttribute>(false);
            if (incAttr != null && incAttr.Include != null)
            {
                data.ClassDec.PushPreInclude(incAttr.Include);
            }

            var methodInvokeExp = new UMethodInvokeStatement()
            {
                MethodName = method.MethodName,
                Method = method,
            };

            if(method.HasReturnValue())
            {
                var retValName = GetReturnValueName();
                methodInvokeExp.ReturnValue = new UVariableDeclaration()
                {
                    VariableType = new UTypeReference(method.ReturnType),
                    VariableName = retValName,
                    InitValue = new UDefaultValueExpression(method.ReturnType),
                };
                if (!data.MethodDec.HasLocalVariable(retValName))
                    data.MethodDec.AddLocalVar(methodInvokeExp.ReturnValue);
            }

            List<UStatementBase> beforeSt = new List<UStatementBase>();
            List<UStatementBase> afterSt = new List<UStatementBase>();
            for (int i=0; i<Arguments.Count; i++)
            {
                var arg = new UMethodInvokeArgumentExpression()
                {
                    OperationType = Arguments[i].OpType,
                };
                UExpressionBase exp;
                GenArgumentCodes(i, ref data, out exp, beforeSt, afterSt);
                arg.Expression = exp;
                methodInvokeExp.Arguments.Add(arg);
            }

            if (data.CurrentStatements.Contains(methodInvokeExp))
                return;

            data.CurrentStatements.AddRange(beforeSt);
            data.CurrentStatements.Add(methodInvokeExp);
            data.CurrentStatements.AddRange(afterSt);
        }
        public override CodeBuilder.UExpressionBase GetExpression(NodePin pin, ref BuildCodeStatementsData data)
        {
            var method = Method;
            if (pin == Result)
            {
                return new UVariableReferenceExpression(GetReturnValueName());
            }
            else
            {
                var parameters = method.GetParameters();
                for (int i = 0; i < parameters.Length; i++)
                {
                    if (parameters[i].Name == pin.Name)
                    {
                        System.Diagnostics.Debug.Assert(parameters[i].IsOut);
                        var mth_outarg_temp_name = GetParamValueName(parameters[i].Name);// $"tmp_o_{parameters[i].Name}_{method.MethodName}_{(uint)this.NodeId.GetHashCode()}";
                        return new UVariableReferenceExpression(mth_outarg_temp_name);
                    }
                }
            }
            System.Diagnostics.Debug.Assert(false);
            return null;
        }
    }
}
