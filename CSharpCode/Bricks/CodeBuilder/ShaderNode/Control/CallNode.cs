using System;
using System.Collections.Generic;
using EngineNS.Bricks.NodeGraph;

namespace EngineNS.Bricks.CodeBuilder.ShaderNode.Control
{
    public class UserCallNodeAttribute : Attribute
    {
        public Type CallNodeType;
    }
    public class CallNode : IBaseNode
    {
        public PinOut Result = null;
        public List<PinIn> Arguments = new List<PinIn>();
        public List<PinOut> OutArguments = new List<PinOut>();
        public Rtti.UClassMeta.MethodMeta Method;
        [Rtti.Meta]
        public string MethodDeclString
        {
            get
            {
                if (Method == null)
                    return null;
                return Method.GetMethodDeclareString();
            }
            set
            {
                var meta = Rtti.UClassMetaManager.Instance.GetMetaFromFullName(typeof(HLSLMethod).FullName);
                Method = meta.GetMethod(value);
                this.Initialize(Method);
            }
        }
        public static CallNode NewMethodNode(Rtti.UClassMeta.MethodMeta m)
        {
            var result = new CallNode();
            result.Initialize(m);
            return result;
        }
        public CallNode()
        {
            Icon = UShaderEditorStyles.Instance.FunctionIcon;
            TitleColor = UShaderEditorStyles.Instance.FunctionTitleColor;
            BackColor = UShaderEditorStyles.Instance.FunctionBGColor;
        }
        internal void Initialize(Rtti.UClassMeta.MethodMeta m)
        {
            Method = m;
            Name = Method.Method.Name;

            if (Method.Method.ReturnType != typeof(void))
            {
                Result = new PinOut();
                Result.Link = UShaderEditorStyles.Instance.NewInOutPinDesc();
                Result.Name = "Result";
                AddPinOut(Result);
            }

            Arguments.Clear();
            OutArguments.Clear();
            foreach (var i in Method.Parameters)
            {
                var pin = new PinIn();
                pin.Link = UShaderEditorStyles.Instance.NewInOutPinDesc();
                pin.Link.CanLinks.Add("Value");
                pin.Name = i.ParamInfo.Name;

                Arguments.Add(pin);
                AddPinIn(pin);
                if (i.ParamInfo.IsOut)
                {
                    var pinOut = new PinOut();
                    pinOut.Link = UShaderEditorStyles.Instance.NewInOutPinDesc();
                    pin.Link.CanLinks.Add("Value");
                    pinOut.Name = i.ParamInfo.Name;
                    OutArguments.Add(pinOut);
                    AddPinOut(pinOut);
                }
                //else if (i.ParamInfo.ParameterType.IsByRef)
                //{
                //    Arguments.Add(pin);
                //    AddPinIn(pin);

                //    var pinOut = new EGui.Controls.NodeGraph.PinOut();
                //    pinOut.Link = UShaderEditorStyles.Instance.NewInOutPinDesc();
                //    pin.Link.CanLinks.Add("Value");
                //    pinOut.Name = i.ParamInfo.Name;
                //    OutArguments.Add(pinOut);
                //    AddPinOut(pinOut);
                //}
                //else
                //{
                //    Arguments.Add(pin);
                //    AddPinIn(pin);
                //}
            }
        }
        public override void OnMouseStayPin(NodePin pin)
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
                EGui.Controls.CtrlUtility.DrawHelper($"{Method.Method.ReturnType.FullName}");
                return;
            }
            for (int i = 0; i < Arguments.Count; i++)
            {
                if (pin == Arguments[i])
                {
                    var inPin = pin as PinIn;
                    var paramMeta = GetInPinParamMeta(inPin);
                    if (paramMeta != null)
                    {
                        EGui.Controls.CtrlUtility.DrawHelper($"{paramMeta.ParamInfo.ParameterType.FullName}");
                    }
                    return;
                }
            }
            for (int i = 0; i < OutArguments.Count; i++)
            {
                if (pin == OutArguments[i])
                {
                    var paramMeta = Method.FindParameter(pin.Name);
                    if (paramMeta != null)
                    {
                        EGui.Controls.CtrlUtility.DrawHelper($"{paramMeta.ParamInfo.ParameterType.FullName}");
                    }
                    return;
                }
            }
        }
        public Rtti.UClassMeta.MethodMeta.ParamMeta GetInPinParamMeta(PinIn pin)
        {
            for (int i = 0; i < Arguments.Count; i++)
            {
                if (pin == Arguments[i])
                {
                    return Method.GetParameter(i);
                }
            }
            return null;
        }
        public override System.Type GetOutPinType(PinOut pin)
        {
            if (pin == Result)
            {
                if (Result.Tag != null)
                {
                    var cvtType = Result.Tag as System.Type;
                    if (cvtType != null)
                        return cvtType;
                }
                return Method.Method.ReturnType;
            }
            foreach (var i in OutArguments)
            {
                if (pin == i)
                {
                    foreach (var j in Method.Parameters)
                    {
                        if (j.ParamInfo.Name == i.Name && j.ParamInfo.IsOut)
                        {
                            return j.ParamInfo.ParameterType.GetElementType();
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

            var nodeExpr = OutNode as IBaseNode;
            if (nodeExpr == null)
                return true;

            for (int i = 0; i < Arguments.Count; i++)
            {
                if (iPin == Arguments[i])
                {
                    var testType = nodeExpr.GetOutPinType(oPin);
                    return ICodeGen.CanConvert(testType, Method.GetParameter(i).ParamInfo.ParameterType);
                }
            }
            return true;
        }
        public override void PreGenExpr()
        {
            Executed = false;
        }
        bool Executed = false;
        public override IExpression GetExpr(UMaterialGraph funGraph, ICodeGen cGen, PinOut oPin, bool bTakeResult)
        {
            if (Executed)
            {
                if (oPin == Result)
                {
                    var mth_ret_temp_name = $"tmp_r_{Method.Method.Name}_{(uint)this.NodeId.GetHashCode()}";
                    return new OpUseVar(mth_ret_temp_name, false);
                }
                else
                {
                    var parameters = Method.Method.GetParameters();
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        if (parameters[i].Name == oPin.Name)
                        {
                            System.Diagnostics.Debug.Assert(parameters[i].IsOut);
                            var mth_outarg_temp_name = $"tmp_o_{parameters[i].Name}_{Method.Method.Name}_{(uint)this.NodeId.GetHashCode()}";
                            return new OpUseVar(mth_outarg_temp_name, false);
                        }
                    }
                }
                System.Diagnostics.Debug.Assert(false);
            }
            Executed = true;

            ConvertTypeOp cvtExpr = null;
            DefineVar retVar = null;

            if (Method.Method.ReturnType != typeof(void))
            {
                var mth_ret_temp_name = $"tmp_r_{Method.Method.Name}_{(uint)this.NodeId.GetHashCode()}";
                retVar = new DefineVar();
                retVar.IsLocalVar = true;
                retVar.DefType = cGen.GetTypeString(Method.Method.ReturnType);
                retVar.VarName = mth_ret_temp_name;
                retVar.InitValue = cGen.GetDefaultValue(Method.Method.ReturnType);
                funGraph.ShaderEditor.MaterialOutput.Function.AddLocalVar(retVar);

                if (Result != null && Result.Tag != null && ((Result.Tag as System.Type) != Method.Method.ReturnType))
                {
                    var cvtTargetType = (Result.Tag as System.Type);
                    retVar.DefType = cvtTargetType.FullName;
                    cvtExpr = new ConvertTypeOp();
                    cvtExpr.TargetType = retVar.DefType;
                }
            }
            
            var callExpr = GetExpr_Impl(funGraph, cGen) as CallOp;

            if (retVar != null)
            {
                callExpr.FunReturnLocalVar = retVar.VarName;
            }
            if (cvtExpr != null)
            {
                callExpr.ConvertType = cvtExpr;
            }

            if (oPin == Result)
            {
                var mth_ret_temp_name = $"tmp_r_{Method.Method.Name}_{(uint)this.NodeId.GetHashCode()}";
                callExpr.FunOutLocalVar = mth_ret_temp_name;
            }
            else
            {
                var parameters = Method.Method.GetParameters();
                for (int i = 0; i < parameters.Length; i++)
                {
                    if (parameters[i].Name == oPin.Name)
                    {
                        System.Diagnostics.Debug.Assert(parameters[i].IsOut);
                        var mth_outarg_temp_name = $"tmp_o_{parameters[i].Name}_{Method.Method.Name}_{(uint)this.NodeId.GetHashCode()}";
                        callExpr.FunOutLocalVar = mth_outarg_temp_name;
                    }
                }
            }

            return callExpr;
        }
        private IExpression GetExpr_Impl(UMaterialGraph funGraph, ICodeGen cGen)
        {
            CallOp CallExpr = new CallOp();
            var links = new List<UPinLinker>();
            
            {
                //这里要处理Static名字获取
                //CallExpr.Host = selfExpr;
                CallExpr.IsStatic = true;
                CallExpr.Host = new HardCodeOp() { Code = "" };
                CallExpr.Name = Method.Method.Name;
            }

            for (int i = 0; i < Arguments.Count; i++)
            {
                if (Method.Parameters[i].ParamInfo.IsOut)
                {
                    var mth_outarg_temp_name = $"tmp_o_{Method.Parameters[i].ParamInfo.Name}_{Method.Method.Name}_{(uint)this.NodeId.GetHashCode()}";
                    var retVar = new DefineVar();
                    retVar.IsLocalVar = true;
                    retVar.DefType = cGen.GetTypeString(Method.Parameters[i].ParamInfo.ParameterType.GetElementType());
                    retVar.VarName = mth_outarg_temp_name;
                    retVar.InitValue = cGen.GetDefaultValue(Method.Parameters[i].ParamInfo.ParameterType.GetElementType());

                    funGraph.ShaderEditor.MaterialOutput.Function.AddLocalVar(retVar);
                    CallExpr.Arguments.Add(new OpUseDefinedVar(retVar));
                    continue;
                }

                links.Clear();
                links = new List<UPinLinker>();
                funGraph.FindInLinker(Arguments[i], links);
                OpExpress argExpr = null;
                if (links.Count == 1)
                {
                    var argNode = links[0].OutNode as IBaseNode;
                    argExpr = argNode.GetExpr(funGraph, cGen, links[0].OutPin, true) as OpExpress;
                    if (argExpr == null)
                        throw new GraphException(this, Arguments[i], $"argExpr = null:{Arguments[i].Name}");
                }
                else if (links.Count == 0)
                {
                    argExpr = OnNoneLinkedParameter(funGraph, cGen, i);
                }
                else
                {
                    throw new GraphException(this, Arguments[i], $"Arg error:{Arguments[i].Name}");
                }
                CallExpr.Arguments.Add(argExpr);
            }

            return CallExpr;
        }
        protected virtual OpExpress OnNoneLinkedParameter(UMaterialGraph funGraph, ICodeGen cGen, int i)
        {
            var mth_arg_temp_name = $"t_{Method.Parameters[i].ParamInfo.Name}_{Method.Method.Name}_{(uint)this.NodeId.GetHashCode()}";
            var retVar = new DefineVar();
            retVar.IsLocalVar = true;
            retVar.DefType = cGen.GetTypeString(Method.Parameters[i].ParamInfo.ParameterType);
            retVar.VarName = mth_arg_temp_name;
            retVar.InitValue = cGen.GetDefaultValue(Method.Parameters[i].ParamInfo.ParameterType);
            funGraph.ShaderEditor.MaterialOutput.Function.AddLocalVar(retVar);

            return new OpUseDefinedVar(retVar);
        }
    }
}
