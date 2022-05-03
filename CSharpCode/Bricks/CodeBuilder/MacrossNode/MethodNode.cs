using System;
using System.Collections.Generic;
using EngineNS.Bricks.NodeGraph;

namespace EngineNS.Bricks.CodeBuilder.MacrossNode
{
    public partial class MethodNode : UNodeBase, EGui.Controls.NodeGraph.EditableValue.IValueEditNotify
    {
        public PinOut Result = null;
        public PinIn Self = null;
        public struct PinData
        {
            public PinIn PinIn;
            public PinOut PinOut;
            public EMethodArgumentAttribute OpType;
        }
        public List<PinData> Arguments = new List<PinData>();
        private Rtti.UClassMeta.MethodMeta Method
        {
            get
            {
                var segs = mMethodMeta.Split('#');
                if (segs.Length != 2)
                    return null;
                var kls = Rtti.UClassMetaManager.Instance.GetMeta(segs[0]);
                if (kls != null)
                {
                    return kls.GetMethod(segs[1]);
                }
                return null;
            }
        }
        private Rtti.UClassMeta HostClass
        {
            get
            {
                var segs = mMethodMeta.Split('#');
                if (segs.Length != 2)
                    return null;
                return Rtti.UClassMetaManager.Instance.GetMeta(segs[0]);
            }
        }
        public UMethodDeclaration MethodDesc;
        public string mMethodMeta;
        [Rtti.Meta(Order = 0)]
        public string MethodMeta
        {
            get
            {
                return mMethodMeta;
                //var kls = Rtti.UTypeDescManager.Instance.GetTypeStringFromType(Method.Method.DeclaringType);
                //return $"{kls}#{Method.GetMethodDeclareString()}";
            }
            set
            {
                mMethodMeta = value;
                if (string.IsNullOrEmpty(value))
                    return;
                if(value[0] == '@')
                {
                    var mtd = GetMethodMeta(value.TrimStart('@'));
                    if (mtd != null)
                        Initialize(mtd);
                }
                else
                {
                    var mtd = GetMethodMeta(value);
                    if (mtd != null)
                        Initialize(mtd);
                }
            }
        }
        Rtti.UClassMeta.MethodMeta GetMethodMeta(string metaStr)
        {
            var segs = metaStr.Split('#');
            if (segs.Length != 2)
                return null;
            var kls = Rtti.UClassMetaManager.Instance.GetMeta(segs[0]);
            if (kls != null)
            {
                var mtd = kls.GetMethod(segs[1]);
                return mtd;
            }
            return null;
        }
        [Rtti.Meta]
        public bool SelfMethod { get; set; } = false;
        public class TSaveData : IO.BaseSerializer
        {
            [Rtti.Meta]
            public Dictionary<string, string> DefaultArguments { get; } = new Dictionary<string, string>();
        }
        [Rtti.Meta(Order = 1)]
        public TSaveData SaveData
        {
            get
            {
                var tmp = new TSaveData();
                foreach(var i in Arguments)
                {
                    if (i.PinIn == null)
                        continue;
                    var pin = i.PinIn;
                    if (pin.EditValue == null)
                        continue;

                    tmp.DefaultArguments[pin.Name] = pin.EditValue.Value.ToString();
                }
                return tmp;
            }
            set
            {
                foreach (var i in value.DefaultArguments)
                {
                    for (int j = 0; j < Arguments.Count; j++)
                    {
                        if(Arguments[j].PinIn != null)
                        {
                            var pin = Arguments[j].PinIn;
                            if (pin.EditValue == null)
                                continue;
                            if (pin.Name == i.Key)
                            {
                                pin.EditValue.Value = Support.TConvert.ToObject(Method.GetParameter(j).ParameterType, i.Value);
                                OnValueChanged(pin.EditValue);
                            }
                        }
                    }
                }
            }
        }
        public static MethodNode NewMethodNode(Rtti.UClassMeta.MethodMeta m)
        {
            var result = new MethodNode();
            result.Initialize(m);
            return result;
        }
        public static MethodNode NewMethodNode(UMethodDeclaration methodDef)
        {
            var result = new MethodNode();
            result.Initialize(methodDef);
            return result;
        }
        public PinIn BeforeExec { get; set; } = new PinIn();
        public PinOut AfterExec { get; set; } = new PinOut();
        public MethodNode()
        {
            BeforeExec.Name = " >>";
            AfterExec.Name = ">> ";
            BeforeExec.Link = MacrossStyles.Instance.NewExecPinDesc();
            AfterExec.Link = MacrossStyles.Instance.NewExecPinDesc();
            AddPinIn(BeforeExec);
            AddPinOut(AfterExec);

            Icon = MacrossStyles.Instance.FunctionIcon;
            TitleColor = MacrossStyles.Instance.FunctionTitleColor;
            BackColor = MacrossStyles.Instance.FunctionBGColor;
        }
        public void OnValueChanged(EGui.Controls.NodeGraph.EditableValue ev)
        {
            if (ev.ValueType.FullName == "System.Type")
            {
                var pin = ev.Tag as PinIn;
                if (pin == null)
                    return;
                var arg = Method.FindParameter(pin.Name);
                if (arg.Meta != null && arg.Meta.FilterType != null)
                {
                    if ((arg.Meta.ConvertOutArguments & Rtti.MetaParameterAttribute.EArgumentFilter.R) != 0)
                    {
                        Result.Tag = Rtti.UTypeDesc.TypeOf((Type)ev.Value);
                    }
                }
            }
        }
        private void Initialize(Rtti.UClassMeta.MethodMeta m)
        {
            //Method = m;
            if (string.IsNullOrEmpty(mMethodMeta))
            {
                mMethodMeta = Rtti.UTypeDesc.TypeStr(m.DeclaringType) + "#" + m.GetMethodDeclareString();
            }
            var method = Method;
            Name = method.MethodName;

            if (method.IsStatic == false)
            {
                Self = new PinIn();
                Self.Link = MacrossStyles.Instance.NewInOutPinDesc();
                Self.Link.CanLinks.Add("Value");
                Self.Name = "Self";
                AddPinIn(Self);
            }

            if (!method.ReturnType.IsEqual(typeof(void)))
            {
                Result = new PinOut();
                Result.Link = MacrossStyles.Instance.NewInOutPinDesc();
                Result.Link.CanLinks.Add("Value");
                Result.Name = "Result";
                AddPinOut(Result);
            }

            Arguments.Clear();
            foreach (var i in method.Parameters)
            {
                var pinData = new PinData();
                pinData.OpType = EMethodArgumentAttribute.Default;
                if (i.IsOut)
                    pinData.OpType = EMethodArgumentAttribute.Out;
                else if (i.IsIn)
                    pinData.OpType = EMethodArgumentAttribute.In;
                else if (i.IsRef)
                    pinData.OpType = EMethodArgumentAttribute.Ref;
                
                if (!i.IsOut)
                {
                    var pin = new PinIn();
                    pin.Link = MacrossStyles.Instance.NewInOutPinDesc();
                    pin.Link.CanLinks.Add("Value");
                    pin.Name = i.Name;
                    pin.Tag = i.ParameterType;
                    var ev = EGui.Controls.NodeGraph.EditableValue.CreateEditableValue(this, i.ParameterType, pin);
                    if (ev != null)
                    {
                        ev.ControlWidth = 80;
                        pin.EditValue = ev;
                        if (i.Meta != null && i.ParameterType.IsEqual(typeof(System.Type)))
                        {
                            var typeEV = ev as EGui.Controls.NodeGraph.TypeSelectorEValue;
                            if (typeEV != null)
                            {
                                typeEV.Selector.BaseType = Rtti.UTypeDesc.TypeOf(i.Meta.FilterType);
                                if (typeEV.Selector.SelectedType == null)
                                    typeEV.Selector.SelectedType = Rtti.UTypeDesc.TypeOf(i.Meta.FilterType);
                                if (ev.Value == null)
                                {
                                    ev.Value = i.Meta.FilterType;
                                    OnValueChanged(typeEV);
                                }
                            }
                        }
                    }
                    AddPinIn(pin);
                    pinData.PinIn = pin;
                }
                if (i.IsOut || i.IsRef)
                {
                    var pinOut = new PinOut();
                    pinOut.Link = MacrossStyles.Instance.NewInOutPinDesc();
                    pinOut.Link.CanLinks.Add("Value");
                    pinOut.Name = i.Name;
                    pinOut.Tag = i.ParameterType;
                    AddPinOut(pinOut);
                    pinData.PinOut = pinOut;
                }
                Arguments.Add(pinData);

                if (i.Meta != null && i.Meta.FilterType != null && i.Meta.ConvertOutArguments != 0)
                {
                    if (Result != null && (i.Meta.ConvertOutArguments & Rtti.MetaParameterAttribute.EArgumentFilter.R) != 0)
                    {
                        if (Result.Tag != null)
                        {
                            Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "Meta", $"{method.MethodName} ParamMeta Error");
                        }
                        Result.Tag = i.Meta.FilterType;
                    }
                }
            }
        }
        public static string GetMethodMeta(UMethodDeclaration m)
        {
            var methodMeta = "@";
            if(m.HostClass != null)
            {
                methodMeta += (m.HostClass.Namespace != null) ? (m.HostClass.Namespace.Namespace + ".") : "" +
                               m.HostClass.ClassName;
            }
            methodMeta += "#" + m.GetKeyword();
            return methodMeta;
        }
        private void Initialize(UMethodDeclaration m)
        {
            if (string.IsNullOrEmpty(mMethodMeta))
            {
                mMethodMeta = GetMethodMeta(m);
            }
            Name = m.MethodName;
            MethodDesc = m;

            if(m.ReturnValue != null)
            {
                Result = new PinOut();
                Result.Link = MacrossStyles.Instance.NewInOutPinDesc();
                Result.Link.CanLinks.Add("Value");
                Result.Name = "Result";
                AddPinOut(Result);
            }

            Arguments.Clear();
            foreach(var i in m.Arguments)
            {
                var pinData = new PinData();
                pinData.OpType = i.OperationType;

                if(i.OperationType != EMethodArgumentAttribute.Out)
                {
                    var pin = new PinIn();
                    pin.Link = MacrossStyles.Instance.NewInOutPinDesc();
                    pin.Link.CanLinks.Add("Value");
                    pin.Name = i.VariableName;
                    pin.Tag = i.VariableType.TypeDesc;
                    var ev = EGui.Controls.NodeGraph.EditableValue.CreateEditableValue(this, i.VariableType.TypeDesc, pin);
                    if (ev != null)
                    {
                        ev.ControlWidth = 80;
                        pin.EditValue = ev;
                        if (i.Meta != null && i.VariableType.IsEqual(typeof(System.Type)))
                        {
                            var typeEV = ev as EGui.Controls.NodeGraph.TypeSelectorEValue;
                            if (typeEV != null)
                            {
                                typeEV.Selector.BaseType = Rtti.UTypeDesc.TypeOf(i.Meta.FilterType);
                                if (typeEV.Selector.SelectedType == null)
                                    typeEV.Selector.SelectedType = Rtti.UTypeDesc.TypeOf(i.Meta.FilterType);
                                if (ev.Value == null)
                                {
                                    ev.Value = i.Meta.FilterType;
                                    OnValueChanged(typeEV);
                                }
                            }
                        }
                    }
                    AddPinIn(pin);
                    pinData.PinIn = pin;
                }
                if(i.OperationType == EMethodArgumentAttribute.Out || i.OperationType == EMethodArgumentAttribute.Ref)
                {
                    var pinOut = new PinOut();
                    pinOut.Link = MacrossStyles.Instance.NewInOutPinDesc();
                    pinOut.Link.CanLinks.Add("Value");
                    pinOut.Name = i.VariableName;
                    pinOut.Tag = i.VariableType.TypeDesc;
                    AddPinOut(pinOut);
                    pinData.PinOut = pinOut;
                }
                Arguments.Add(pinData);

                if (i.Meta != null && i.Meta.FilterType != null && i.Meta.ConvertOutArguments != 0)
                {
                    if (Result != null && (i.Meta.ConvertOutArguments & Rtti.MetaParameterAttribute.EArgumentFilter.R) != 0)
                    {
                        if (Result.Tag != null)
                        {
                            Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "Meta", $"{m.MethodName} ParamMeta Error");
                        }
                        Result.Tag = i.Meta.FilterType;
                    }
                }
            }
        }
        public override void OnMouseStayPin(NodePin pin)
        {
            if (pin == Self)
            {
                EGui.Controls.CtrlUtility.DrawHelper($"{Method.DeclaringType.FullName}"); 
                return;
            }
            if (pin == Result)
            {
                if (Result.Tag != null)
                {
                    var cvtType = Result.Tag as Rtti.UTypeDesc;
                    if (cvtType != null)
                    {
                        EGui.Controls.CtrlUtility.DrawHelper($"{cvtType.FullName}");
                        return;
                    }
                }
                EGui.Controls.CtrlUtility.DrawHelper($"{Method.ReturnType.FullName}");
                return;
            }
            for (int i = 0; i < Arguments.Count; i++)
            {
                if (pin == Arguments[i].PinIn)
                {
                    var inPin = pin as PinIn;
                    var paramMeta = GetInPinParamMeta(inPin);
                    if (paramMeta != null)
                    {
                        EGui.Controls.CtrlUtility.DrawHelper($"{paramMeta.ParameterType.FullName}");
                    }
                    return;
                }
                else if(pin == Arguments[i].PinOut)
                {
                    var paramMeta = Method.FindParameter(pin.Name);
                    if (paramMeta != null)
                    {
                        EGui.Controls.CtrlUtility.DrawHelper($"{paramMeta.ParameterType.FullName}");
                    }
                    return;
                }
            }
        }
        //public override IExpression GetExpr(UMacrossMethodGraph funGraph, ICodeGen cGen, bool bTakeResult)
        //{
        //    ConvertTypeOp cvtExpr = null;
        //    DefineVar retVar = null;

        //    var mth_ret_temp_name = $"tmp_r_{Method.Method.Name}_{this.NodeId.GetHashCode().ToString().Replace("-", "_")}";
        //    if (Method.Method.ReturnType != typeof(void))
        //    {
        //        if (bTakeResult)
        //        {
        //            return new OpUseVar(mth_ret_temp_name, false);
        //        }
        //        retVar = new DefineVar();
        //        retVar.IsLocalVar = true;
        //        retVar.DefType = Method.Method.ReturnType.FullName;
        //        retVar.VarName = mth_ret_temp_name;
        //        retVar.InitValue = cGen.GetDefaultValue(Method.Method.ReturnType);

        //        if (Result != null && Result.Tag != null && ((Result.Tag as System.Type) != Method.Method.ReturnType))
        //        {
        //            var cvtTargetType = (Result.Tag as System.Type);
        //            retVar.DefType = cvtTargetType.FullName;
        //            cvtExpr = new ConvertTypeOp();
        //            cvtExpr.TargetType = retVar.DefType;
        //        }
        //    }
        //    if (bTakeResult)
        //    {
        //        throw new GraphException(this, Self, "Use return value with void function");
        //    }

        //    var callExpr = GetExpr_Impl(funGraph, cGen) as CallOp;

        //    if (retVar != null)
        //    {
        //        funGraph.Function.AddLocalVar(retVar);
        //        callExpr.FunReturnLocalVar = retVar.VarName;
        //    }
        //    if (cvtExpr != null)
        //    {
        //        callExpr.ConvertType = cvtExpr;
        //    }

        //    callExpr.NextExpr = this.GetNextExpr(funGraph, cGen);
        //    return callExpr;
        //}
        //private IExpression GetExpr_Impl(UMacrossMethodGraph funGraph, ICodeGen cGen)
        //{
        //    CallOp CallExpr = new CallOp();
        //    var links = new List<UPinLinker>();
        //    if (Self != null)
        //    {
        //        CallExpr.IsStatic = false;
        //        funGraph.FindInLinker(Self, links);
        //        if (links.Count == 0)
        //        {
        //            if(SelfMethod)
        //            {
        //                CallExpr.Host = new ThisVar();
        //                CallExpr.Name = Method.Method.Name;
        //            }
        //            else
        //            {
        //                CallExpr.Host = new NewObjectOp() { Type = Method.Method.DeclaringType.FullName };
        //                CallExpr.Name = Method.Method.Name;
        //            }
        //        }
        //        else if (links.Count == 1)
        //        {
        //            var selfNode = links[0].OutNode as UNodeExpr;
        //            var selfExpr = selfNode.GetExpr(funGraph, cGen, true) as OpExpress;
        //            CallExpr.Host = selfExpr;
        //            CallExpr.Name = Method.Method.Name;
        //        }
        //        else
        //        {
        //            throw new GraphException(this, Self, "Please Self pin");
        //        }
        //    }
        //    else
        //    {
        //        //这里要处理Static名字获取
        //        //CallExpr.Host = selfExpr;
        //        CallExpr.IsStatic = true;
        //        CallExpr.Host = new HardCodeOp() { Code = Method.Method.DeclaringType.FullName };
        //        CallExpr.Name = Method.Method.Name;
        //    }

        //    for (int i = 0; i < Arguments.Count; i++)
        //    {
        //        links.Clear();
        //        links = new List<UPinLinker>();
        //        funGraph.FindInLinker(Arguments[i], links);
        //        OpExpress argExpr = null;
        //        if (links.Count == 1)
        //        {
        //            var argNode = links[0].OutNode as UNodeExpr;
        //            argExpr = argNode.GetExpr(funGraph, cGen, true) as OpExpress;
        //        }
        //        else if (links.Count == 0)
        //        {
        //            var paramInfo = Method.GetParameter(i).ParamInfo;

        //            var refType = VariableReferenceOp.eReferenceType.None;
        //            if (paramInfo.IsIn)
        //            {
        //                refType = VariableReferenceOp.eReferenceType.In;
        //            }
        //            else if(paramInfo.IsOut)
        //            {
        //                refType = VariableReferenceOp.eReferenceType.Out;
        //            }
        //            else if(paramInfo.ParameterType.IsByRef)
        //            {
        //                refType = VariableReferenceOp.eReferenceType.Ref;
        //            }

        //            if(refType == VariableReferenceOp.eReferenceType.None)
        //            {
        //                var newOp = new NewObjectOp();
        //                argExpr = newOp;
        //                newOp.Type = cGen.GetTypeString(paramInfo.ParameterType.GetElementType());
        //                if (Arguments[i].EditValue != null)
        //                {
        //                    if (Arguments[i].EditValue.Value is System.Type)
        //                        newOp.InitValue = ((System.Type)Arguments[i].EditValue.Value).FullName;
        //                    else
        //                        newOp.InitValue = Arguments[i].EditValue.Value?.ToString();
        //                }
        //                else if (paramInfo.ParameterType.IsValueType == false)
        //                {
        //                    newOp.InitValue = "null";
        //                }
        //                else
        //                {

        //                }
        //            }
        //            else
        //            {
        //                var refOp = new VariableReferenceOp();
        //                var paramTempName = $"tmp_v_{paramInfo.Name}_{Method.Method.Name}_{(uint)this.NodeId.GetHashCode()}";
        //                refOp.VariableName = paramTempName;
        //                refOp.ReferenceType = refType;
        //                var defineVar = new DefineVar()
        //                {
        //                    IsLocalVar = true,
        //                    DefType = cGen.GetTypeString(paramInfo.ParameterType.GetElementType()),
        //                    VarName = paramTempName
        //                };
        //                funGraph.Function.LocalVars.Add(defineVar);
        //                argExpr = refOp;
        //                if (Arguments[i].EditValue != null)
        //                {
        //                    if (Arguments[i].EditValue.Value is System.Type)
        //                        defineVar.InitValue = ((System.Type)Arguments[i].EditValue.Value).FullName;
        //                    else
        //                        defineVar.InitValue = Arguments[i].EditValue.Value?.ToString();
        //                }
        //                else
        //                {
        //                    defineVar.InitValue = cGen.GetDefaultValue(Method.Parameters[i].ParamInfo.ParameterType.GetElementType());
        //                }
        //            }
        //        }
        //        else
        //        {
        //            throw new GraphException(this, Self, $"Arg error:{Arguments[i].Name}");
        //        }
        //        CallExpr.Arguments.Add(argExpr);
        //    }

        //    return CallExpr;
        //}
        string GetReturnValueName()
        {
            return $"tmp_r_{Method.MethodName}_{(uint)NodeId.GetHashCode()}";
        }
        string GetParamValueName(string paramName)
        {
            return $"v_{paramName}_{Method.MethodName}_{(uint)NodeId.GetHashCode()}";
        }

        void GenArgumentCodes(int argIdx, ref BuildCodeStatementsData data, out UExpressionBase exp, 
            List<UStatementBase> beforeStatements = null, 
            List<UStatementBase> afterStatements = null)
        {
            var pinData = Arguments[argIdx];
            switch (pinData.OpType)
            {
                case EMethodArgumentAttribute.Out:
                    {
                        var outPin = pinData.PinOut;
                        var paramName = GetParamValueName(outPin.Name);
                        exp = new UVariableReferenceExpression(paramName);
                        if(beforeStatements != null)
                        {
                            var varDec = new UVariableDeclaration()
                            {
                                VariableType = new UTypeReference(outPin.Tag as Rtti.UTypeDesc),
                                VariableName = paramName,
                            };
                            beforeStatements.Add(varDec);
                        }
                    }
                    break;
                case EMethodArgumentAttribute.Ref:
                case EMethodArgumentAttribute.In:
                    {
                        var inPin = pinData.PinIn;
                        if(data.NodeGraph.PinHasLinker(inPin))
                        {
                            var paramName = GetParamValueName(inPin.Name);
                            var texp = data.NodeGraph.GetOppositePinExpression(inPin, ref data);
                            if(texp is UVariableReferenceExpression)
                            { 
                                if(((UVariableReferenceExpression)texp).IsProperty)
                                {
                                    exp = new UVariableReferenceExpression(paramName);
                                    if(beforeStatements != null)
                                    {
                                        var varDec = new UVariableDeclaration()
                                        {
                                            VariableType = new UTypeReference(inPin.Tag as Rtti.UTypeDesc),
                                            VariableName = paramName,
                                            InitValue = texp,
                                        };
                                        beforeStatements.Add(varDec);
                                    }
                                    if(afterStatements != null && pinData.OpType == EMethodArgumentAttribute.Ref)
                                    {
                                        var assign = new UAssignOperatorStatement()
                                        {
                                            From = new UVariableReferenceExpression(paramName),
                                            To = texp,
                                        };
                                        afterStatements.Add(assign);
                                    }
                                }
                                else
                                    exp = texp;
                            }
                            else
                            {
                                exp = new UVariableReferenceExpression(paramName);
                                if(beforeStatements != null)
                                {
                                    var varDec = new UVariableDeclaration()
                                    {
                                        VariableType = new UTypeReference(inPin.Tag as Rtti.UTypeDesc),
                                        VariableName = paramName,
                                        InitValue = texp,
                                    };
                                    beforeStatements.Add(varDec);
                                }
                            }
                        }
                        else
                            exp = GetNoneLinkedParameterExp(inPin, argIdx, ref data);
                    }
                    break;
                default:
                    {
                        var inPin = pinData.PinIn;
                        if (data.NodeGraph.PinHasLinker(inPin))
                            exp = data.NodeGraph.GetOppositePinExpression(inPin, ref data);
                        else
                            exp = GetNoneLinkedParameterExp(inPin, argIdx, ref data);
                    }
                    break;
            }
        }
        protected virtual UExpressionBase GetNoneLinkedParameterExp(PinIn pin, int argIdx, ref BuildCodeStatementsData data)
        {
            return new UPrimitiveExpression(pin.EditValue.ValueType, pin.EditValue.Value);
        }
        public override void BuildStatements(ref BuildCodeStatementsData data)
        {
            if (MethodDesc != null)
                BuildStatementsWithMethodDec(ref data);
            else
                BuildStatementsWithMethodMeta(ref data);
        }
        private void BuildStatementsWithMethodDec(ref BuildCodeStatementsData data)
        {
            var methodInvokeExp = new UMethodInvokeStatement()
            {
                MethodName = MethodDesc.MethodName,
            };
            if (Self != null)
            {
                if (data.NodeGraph.PinHasLinker(Self))
                    methodInvokeExp.Host = data.NodeGraph.GetOppositePinExpression(Self, ref data);
            }
            else
            {
                // method is static
                if (HostClass != null)
                    methodInvokeExp.Host = new UClassReferenceExpression() { Class = HostClass.ClassType };
            }

            if (MethodDesc.ReturnValue != null)
            {
                var retValName = GetReturnValueName();
                methodInvokeExp.ReturnValue = new UVariableDeclaration()
                {
                    VariableType = MethodDesc.ReturnValue.VariableType,
                    VariableName = retValName,
                    InitValue = new UDefaultValueExpression(MethodDesc.ReturnValue.VariableType),
                };
                if (!data.MethodDec.HasLocalVariable(retValName))
                    data.MethodDec.AddLocalVar(methodInvokeExp.ReturnValue);
            }

            List<UStatementBase> beforeSt = new List<UStatementBase>();
            List<UStatementBase> afterSt = new List<UStatementBase>();
            for (int i = 0; i < Arguments.Count; i++)
            {
                var arg = new UMethodInvokeArgumentExpression()
                {
                    OperationType = Arguments[i].OpType,
                };
                GenArgumentCodes(i, ref data, out arg.Expression, beforeSt, afterSt);
                methodInvokeExp.Arguments.Add(arg);
            }
            data.CurrentStatements.AddRange(beforeSt);
            data.CurrentStatements.Add(methodInvokeExp);
            data.CurrentStatements.AddRange(afterSt);

            var nextNode = data.NodeGraph.GetOppositePinNode(AfterExec);
            if (nextNode != null)
                nextNode.BuildStatements(ref data);
        }
        private void BuildStatementsWithMethodMeta(ref BuildCodeStatementsData data)
        {
            var method = Method;
            var methodInvokeExp = new UMethodInvokeStatement()
            {
                MethodName = method.MethodName,
            };
            if(Self != null)
            {
                if(data.NodeGraph.PinHasLinker(Self))
                    methodInvokeExp.Host = data.NodeGraph.GetOppositePinExpression(Self, ref data);
            }
            else
            {
                // method is static
                if(HostClass != null)
                    methodInvokeExp.Host = new UClassReferenceExpression() { Class = HostClass.ClassType };
            }

            if(method.HasReturnValue())
            {
                var retValName = GetReturnValueName();
                methodInvokeExp.ReturnValue = new UVariableDeclaration()
                {
                    VariableType = new UTypeReference(method.ReturnType),
                    VariableName = retValName,
                    InitValue = new UDefaultValueExpression(method.ReturnType),
                };
                if(!data.MethodDec.HasLocalVariable(retValName))
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
                GenArgumentCodes(i, ref data, out arg.Expression, beforeSt, afterSt);
                methodInvokeExp.Arguments.Add(arg);
            }
            data.CurrentStatements.AddRange(beforeSt);
            data.CurrentStatements.Add(methodInvokeExp);
            data.CurrentStatements.AddRange(afterSt);

            var nextNode = data.NodeGraph.GetOppositePinNode(AfterExec);
            if (nextNode != null)
                nextNode.BuildStatements(ref data);
        }
        public override UExpressionBase GetExpression(NodePin pin, ref BuildCodeStatementsData data)
        {
            if(pin == Result)
            {
                return new UVariableReferenceExpression(GetReturnValueName());
            }
            else
            {
                for (int i = 0; i < Arguments.Count; i++)
                {
                    if (pin == Arguments[i].PinIn || 
                        pin == Arguments[i].PinOut)
                    {
                        UExpressionBase retVal;
                        GenArgumentCodes(i, ref data, out retVal);
                        return retVal;
                    }
                }
            }
            return null;
        }
        public Rtti.UClassMeta.MethodMeta.ParamMeta GetInPinParamMeta(PinIn pin)
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
            var method = Method;
            if (pin == Result)
            {
                if (Result.Tag != null)
                {
                    var cvtType = Result.Tag as Rtti.UTypeDesc;
                    if (cvtType != null)
                        return cvtType;
                }
                return method.ReturnType;
            }
            foreach(var i in Arguments)
            {
                if (pin == i.PinOut)
                {
                    foreach(var j in method.Parameters)
                    {
                        if (j.Name == i.PinOut.Name && j.IsOut)
                            return j.ParameterType;
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

            if (iPin == Self)
            {
                var testType = nodeExpr.GetOutPinType(oPin);
                return UCodeGeneratorBase.CanConvert(testType, Method.DeclaringType);
            }
            for (int i = 0; i < Arguments.Count; i++)
            {
                if (iPin == Arguments[i].PinIn)
                {
                    var testType = nodeExpr.GetOutPinType(oPin);
                    return UCodeGeneratorBase.CanConvert(testType, Method.GetParameter(i).ParameterType);
                }
            }
            return true;
        }
    }
    public class MethodSelector
    {
        public class MethodSelectorStyle
        {
            public static MethodSelectorStyle Instance = new MethodSelectorStyle();
            public uint NameSpaceColor = 0xFFFFFFFF;
            public uint ClassColor = 0xFFFF80FF;
            public uint MemberColor = 0xFF80FF00;
            public uint FieldColor = 0xFF806F40;
            public uint MethodColor = 0xFFFF4080;
            public uint SubClassColor = 0xFF5340FF;
        }
        public MethodSelectorStyle Styles = MethodSelectorStyle.Instance;
        public Rtti.UClassMeta.MethodMeta mSltMethod;
        public Rtti.UClassMeta.FieldMeta mSltField;
        public Rtti.UClassMeta.PropertyMeta mSltMember;
        public unsafe void OnDraw(Vector2 pos)
        {
            var pivot = new Vector2(0, 0);
            var size = new Vector2(300, 500);
            ImGuiAPI.SetNextWindowPos(in pos, ImGuiCond_.ImGuiCond_None, in pivot);
            ImGuiAPI.SetNextWindowSize(in size, ImGuiCond_.ImGuiCond_None);
            if (ImGuiAPI.BeginPopup("MethodSelector", ImGuiWindowFlags_.ImGuiWindowFlags_NoMove))
            {
                OnDrawTree();

                ImGuiAPI.EndPopup();
            }
        }
        public unsafe void OnDrawTree(string filterText = null)
        {
            //var buffer = BigStackBuffer.CreateInstance(256);
            //buffer.SetText(mFilterText);
            //ImGuiAPI.InputText("##in", buffer.GetBuffer(), (uint)buffer.GetSize(), ImGuiInputTextFlags_.ImGuiInputTextFlags_None, null, (void*)0);
            //mFilterText = buffer.AsText();
            //buffer.DestroyMe();
            ImGuiAPI.Separator();

            DrawNSTree(filterText, Rtti.UClassMetaManager.Instance.TreeManager.RootNS);
        }
        public unsafe void DrawNSTree(string filterText, Rtti.NameSpace ns)
        {
            bool bTestFilter = string.IsNullOrEmpty(filterText) == false;
            if (bTestFilter && ns.IsContain(filterText) == false)
            {
                return;
            }
            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Text, Styles.NameSpaceColor);
            var bShow = ImGuiAPI.TreeNode(ns.Name);
            ImGuiAPI.PopStyleColor(1);
            if (bShow)
            {
                foreach(var i in ns.ChildrenNS)
                {
                    DrawNSTree(filterText, i);
                }
                ImGuiTreeNodeFlags_ flags = ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Leaf | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_NoTreePushOnOpen | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Bullet;
                foreach (var i in ns.Types)
                {
                    ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Text, Styles.ClassColor);
                    bShow = ImGuiAPI.TreeNode(i.ClassType.Name);
                    ImGuiAPI.PopStyleColor(1);
                    if (bShow)
                    {
                        foreach(var j in i.CurrentVersion.Propertys)
                        {
                            if (bTestFilter && j.PropertyName.Contains(filterText) == false)
                                continue;
                            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Text, Styles.MemberColor);
                            ImGuiAPI.TreeNodeEx(j.PropertyName, flags);
                            if (ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left))
                            {
                                mSltMember = j;
                            }
                            ImGuiAPI.PopStyleColor(1);
                            if (ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None))
                                EGui.Controls.CtrlUtility.DrawHelper(j.ToString());
                        }                        
                        foreach (var j in i.Fields)
                        {
                            if (bTestFilter && j.Field.Name.Contains(filterText) == false)
                                continue;
                            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Text, Styles.FieldColor);
                            ImGuiAPI.TreeNodeEx(j.Field.Name, flags);
                            if (ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left))
                            {
                                mSltField = j;
                            }
                            ImGuiAPI.PopStyleColor(1);
                            if (ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None))
                                EGui.Controls.CtrlUtility.DrawHelper(j.ToString());
                        }
                        foreach (var j in i.Methods)
                        {
                            if (bTestFilter && j.MethodName.Contains(filterText) == false)
                                continue;
                            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Text, Styles.MethodColor);
                            ImGuiAPI.TreeNodeEx(j.MethodName, flags);
                            if (ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left))
                            {
                                mSltMethod = j;
                            }
                            ImGuiAPI.PopStyleColor(1);
                            if (ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None))
                                EGui.Controls.CtrlUtility.DrawHelper(j.ToString());
                        }
                        ImGuiAPI.TreePop();
                    }   
                }
                ImGuiAPI.TreePop();
            }
        }
    }

    public class MacrossSelector
    {
        public Rtti.UClassMeta KlsMeta;
        public Rtti.UClassMeta.MethodMeta mSltMethod;
        public Rtti.UClassMeta.FieldMeta mSltField;
        public Rtti.UClassMeta.PropertyMeta mSltMember;
        public Rtti.UClassMeta mSltSubClass;
        public unsafe void OnDraw(Vector2 pos)
        {
            var Styles = MethodSelector.MethodSelectorStyle.Instance;
            ImGuiTreeNodeFlags_ flags = ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Leaf | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_NoTreePushOnOpen | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Bullet;
            //列出DownCast List
            if (ImGuiAPI.TreeNode("Cast"))
            {
                if (KlsMeta != null)
                {
                    var kls = KlsMeta.SubClasses;
                    foreach (var j in kls)
                    {
                        ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Text, Styles.SubClassColor);
                        ImGuiAPI.TreeNodeEx(j.ClassType.Name, flags);
                        if (ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left))
                        {
                            mSltSubClass = j;
                        }
                        if (ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None))
                            EGui.Controls.CtrlUtility.DrawHelper(j.ClassType.FullName);
                        ImGuiAPI.PopStyleColor(1);
                    }
                }
                ImGuiAPI.TreePop();
            }

            //列出所有属性
            if (ImGuiAPI.TreeNode("Properties"))
            {
                if (KlsMeta != null)
                {
                    foreach (var j in KlsMeta.CurrentVersion.Propertys)
                    {
                        ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Text, Styles.MemberColor);
                        ImGuiAPI.TreeNodeEx(j.PropertyName, flags);
                        if (ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left))
                        {
                            mSltMember = j;
                        }
                        ImGuiAPI.PopStyleColor(1);
                        if (ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None))
                            EGui.Controls.CtrlUtility.DrawHelper(j.ToString());
                    }
                }
                ImGuiAPI.TreePop();
            }

            //所有field
            if (ImGuiAPI.TreeNode("Fields"))
            {
                if (KlsMeta != null)
                {
                    foreach (var j in KlsMeta.Fields)
                    {
                        ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Text, Styles.FieldColor);
                        ImGuiAPI.TreeNodeEx(j.Field.Name, flags);
                        if (ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left))
                        {
                            mSltField = j;
                        }
                        ImGuiAPI.PopStyleColor(1);
                        if (ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None))
                            EGui.Controls.CtrlUtility.DrawHelper(j.ToString());
                    }
                }
                ImGuiAPI.TreePop();
            }            

            //method
            if (ImGuiAPI.TreeNode("Methods"))
            {
                if (KlsMeta != null)
                {
                    foreach (var j in KlsMeta.Methods)
                    {
                        ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Text, Styles.MethodColor);
                        ImGuiAPI.TreeNodeEx(j.MethodName, flags);
                        if (ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left))
                        {
                            mSltMethod = j;
                        }
                        ImGuiAPI.PopStyleColor(1);
                        if (ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None))
                            EGui.Controls.CtrlUtility.DrawHelper(j.ToString());
                    }
                }
                ImGuiAPI.TreePop();
            }
        }
    }
}