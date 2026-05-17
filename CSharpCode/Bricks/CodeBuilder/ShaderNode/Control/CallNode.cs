using System;
using System.Collections.Generic;
using EngineNS.Bricks.NodeGraph;
using EngineNS.Graphics.Pipeline.Shader;

namespace EngineNS.Bricks.CodeBuilder.ShaderNode.Control
{
    public class UserCallNodeAttribute : Attribute
    {
        public Type CallNodeType;
    }
    public class CallNode : TtNodeBase
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
        public Rtti.TtClassMeta.TtMethodMeta Method;
        [Rtti.Meta("")]
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

                Method = TtEngine.Instance.MaterialMethodManager.GetMethodByDeclString(value);
                if (Method == null)
                {
                    var name = Rtti.TtClassMeta.GetNameByDeclstring(value);
                    Method = TtEngine.Instance.MaterialMethodManager.GetMethod(name);
                }
                if (Method != null)
                    this.Initialize(Method);
            }
        }
        public static CallNode NewMethodNode(Rtti.TtClassMeta.TtMethodMeta m)
        {
            CallNode result = null;
            //if (m.MethodName == "Sample2D")
            //{
            //    result = new Sample2DNode();
            //}
            //else if (m.MethodName == "Sample2DBias")
            //{
            //    result = new Sample2DBiasNode();
            //}
            //else
            {
                result = new CallNode();
            }
            result.Initialize(m);
            return result;
        }
        public CallNode()
        {
            Icon = TtMaterialEditorStyles.Instance.FunctionIcon;
            TitleColor = TtMaterialEditorStyles.Instance.FunctionTitleColor;
            BackColor = TtMaterialEditorStyles.Instance.FunctionBGColor;
        }
        internal void Initialize(Rtti.TtClassMeta.TtMethodMeta m)
        {
            Method = m;
            Name = m.MethodName;

            if (!m.ReturnType.IsEqual(typeof(void)))
            {
                Result = new PinOut();
                Result.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
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
                    pin.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
                    pin.LinkDesc.CanLinks.Add("Value");
                    pin.Name = i.Name;
                    pin.Tag = i.ParameterType;
                    AddPinIn(pin);
                    pinData.PinIn = pin;
                }
                if(i.IsOut || i.IsRef)
                {
                    var pinOut = new PinOut();
                    pinOut.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
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
        public override void OnMouseStayPin(NodePin pin, TtNodeGraph graph)
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
        }
        public Rtti.TtClassMeta.TtMethodMeta.TtParamMeta GetInPinParamMeta(PinIn pin)
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
        public override Rtti.TtTypeDesc GetOutPinType(PinOut pin)
        {
            if (pin == null)
                return null;

            if (pin == Result)
            {
                if (Result.Tag != null)
                {
                    var cvtType = Result.Tag as Rtti.TtTypeDesc;
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
        public override bool CanLinkFrom(PinIn iPin, TtNodeBase OutNode, PinOut oPin)
        {
            if (base.CanLinkFrom(iPin, OutNode, oPin) == false)
                return false;

            var nodeExpr = OutNode as TtNodeBase;
            if (nodeExpr == null)
                return true;

            for (int i = 0; i < Arguments.Count; i++)
            {
                if (iPin == Arguments[i].PinIn)
                {
                    var testType = nodeExpr.GetOutPinType(oPin);
                    return CodeBuilder.TtCodeGeneratorBase.CanConvert(testType, Method.GetParameter(i).ParameterType);
                }
            }
            return true;
        }
        protected string GetReturnValueName()
        {
            return $"tmp_r_{Method.MethodName}_{(uint)NodeId.GetHashCode()}";
        }
        protected string GetParamValueName(string paramName)
        {
            return $"tmp_o_{paramName}_{Method.MethodName}_{(uint)NodeId.GetHashCode()}";
        }
        protected virtual TtExpressionBase GetNoneLinkedParameterExp(PinIn pin, int argIdx, ref BuildCodeStatementsData data)
        {
            var paramName = GetParamValueName(pin.Name);
            if (!data.MethodDec.HasLocalVariable(paramName))
            {
                var arg = Method.FindParameter(pin.Name);
                var type = pin.Tag as Rtti.TtTypeDesc;
                bool isPrimitive = type.IsPrimitive;
                if (type.SystemType == typeof(Vector3) ||
                    type.SystemType == typeof(Vector4) ||
                    type.SystemType == typeof(Vector2))
                {
                    isPrimitive = true;
                }
                var varDec = new TtVariableDeclaration()
                {
                    VariableType = new TtTypeReference(type),
                    VariableName = paramName,
                    InitValue = (isPrimitive && arg.DefaultValue != null && arg.DefaultValue.GetType()!=typeof(System.DBNull)) ? new TtPrimitiveExpression(type, arg.DefaultValue) : new TtDefaultValueExpression(type),
                };
                data.MethodDec.AddLocalVar(varDec);
            }
            var retVal = new TtVariableReferenceExpression(paramName);
            return retVal;
        }

        protected void GenArgumentCodes(int argIdx, ref BuildCodeStatementsData data, out TtExpressionBase exp,
            List<TtStatementBase> beforeStatements = null,
            List<TtStatementBase> afterStatements = null)
        {
            var pinData = Arguments[argIdx];
            switch(pinData.OpType)
            {
                case EMethodArgumentAttribute.Out:
                    {
                        var outPin = pinData.PinOut;
                        var paramName = GetParamValueName(outPin.Name);
                        exp = new TtVariableReferenceExpression(paramName);

                        if(!data.MethodDec.HasLocalVariable(paramName))
                        {
                            var type = outPin.Tag as Rtti.TtTypeDesc;
                            var varDec = new TtVariableDeclaration()
                            {
                                VariableType = new TtTypeReference(type),
                                VariableName = paramName,
                                InitValue = new TtDefaultValueExpression(type),
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
                            if(opPin == null)
                            {
                                this.HasError = true;
                                CodeExcept = new GraphException(this, opPin, "null");
                                exp = GetNoneLinkedParameterExp(inPin, argIdx, ref data);
                            }
                            else
                            {
                                opNode.BuildStatements(opPin, ref data);
                                exp = data.NodeGraph.GetOppositePinExpression(inPin, ref data);
                            }
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
            var incAttr = Method.GetFirstCustomAttribute<EngineNS.Bricks.CodeBuilder.ShaderNode.Control.TtMaterialShaderAttribute>(false);
            if (incAttr != null && incAttr.Include != null)
            {
                data.ClassDec.PushPreInclude(incAttr.Include);
            }

            var methodInvokeExp = new TtMethodInvokeStatement()
            {
                MethodName = method.MethodName,
                Method = method,
            };

            if(method.HasReturnValue())
            {
                var retValName = GetReturnValueName();
                methodInvokeExp.ReturnValue = new TtVariableDeclaration()
                {
                    VariableType = new TtTypeReference(method.ReturnType),
                    VariableName = retValName,
                    InitValue = new TtDefaultValueExpression(method.ReturnType),
                };
                if (!data.MethodDec.HasLocalVariable(retValName))
                    data.MethodDec.AddLocalVar(methodInvokeExp.ReturnValue);
            }

            List<TtStatementBase> beforeSt = new List<TtStatementBase>();
            List<TtStatementBase> afterSt = new List<TtStatementBase>();
            for (int i=0; i<Arguments.Count; i++)
            {
                var arg = new TtMethodInvokeArgumentExpression()
                {
                    OperationType = Arguments[i].OpType,
                };
                TtExpressionBase exp;
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
        public override CodeBuilder.TtExpressionBase GetExpression(NodePin pin, ref BuildCodeStatementsData data)
        {
            var method = Method;
            if (pin == Result)
            {
                return new TtVariableReferenceExpression(GetReturnValueName());
            }
            else
            {
                var parameters = method.GetParameters();
                for (int i = 0; i < parameters.Count; i++)
                {
                    if (parameters[i].Name == pin.Name)
                    {
                        System.Diagnostics.Debug.Assert(parameters[i].IsOut);
                        var mth_outarg_temp_name = GetParamValueName(parameters[i].Name);// $"tmp_o_{parameters[i].Name}_{method.MethodName}_{(uint)this.NodeId.GetHashCode()}";
                        return new TtVariableReferenceExpression(mth_outarg_temp_name);
                    }
                }
            }
            System.Diagnostics.Debug.Assert(false);
            return null;
        }
    }
    
    public class TtCallMaterialFunctionNode : CallNode
    {
        RName mFunctionName;
        [Rtti.Meta("",Order = 1)]
        [RName.PGRName(FilterExts = TtMaterialFunction.AssetExt)]
        public RName FunctionName 
        {
            get 
            {
                return mFunctionName;
            }
            set
            {
                if (mFunctionName == value)
                    return;
                mFunctionName = value;

                MaterialFunction = value.GetAsset<Graphics.Pipeline.Shader.TtMaterialFunction>().GetResultUntilCompleted();
                this.Initialize(MaterialFunction.MethodMeta);
                this.Name = MaterialFunction.CallNodeName;
            }
        }

        public TtMaterialFunction MaterialFunction { get; private set; }
        public override void BuildStatements(NodePin pin, ref BuildCodeStatementsData data)
        {
            var method = Method;
            var incAttr = Method.GetFirstCustomAttribute<EngineNS.Bricks.CodeBuilder.ShaderNode.Control.TtMaterialShaderAttribute>(false);
            if (incAttr != null && incAttr.Include != null)
            {
                data.ClassDec.PushPreInclude(incAttr.Include);
            }

            var methodInvokeExp = new TtMethodInvokeStatement()
            {
                MethodName = method.MethodName,
                Method = method,
            };

            if (method.HasReturnValue())
            {
                var retValName = GetReturnValueName();
                methodInvokeExp.ReturnValue = new TtVariableDeclaration()
                {
                    VariableType = new TtTypeReference(method.ReturnType),
                    VariableName = retValName,
                    InitValue = new TtDefaultValueExpression(method.ReturnType),
                };
                if (!data.MethodDec.HasLocalVariable(retValName))
                    data.MethodDec.AddLocalVar(methodInvokeExp.ReturnValue);
            }

            List<TtStatementBase> beforeSt = new List<TtStatementBase>();
            List<TtStatementBase> afterSt = new List<TtStatementBase>();
            {
                var inputExpr = new TtMethodInvokeArgumentExpression();
                var varRef = new TtVariableReferenceExpression("input");
                inputExpr.Expression = varRef;
                methodInvokeExp.Arguments.Add(inputExpr);
            }
            for (int i = 1; i < Arguments.Count; i++)
            {
                var arg = new TtMethodInvokeArgumentExpression()
                {
                    OperationType = Arguments[i].OpType,
                };
                TtExpressionBase exp;
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
    }
}
