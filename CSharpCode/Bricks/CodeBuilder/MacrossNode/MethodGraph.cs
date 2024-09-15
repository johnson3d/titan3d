using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Threading.Tasks;
using EngineNS.Bricks.NodeGraph;
using EngineNS.EGui.Controls;
using EngineNS.EGui.Controls.PropertyGrid;
using Mono.CompilerServices.SymbolWriter;
using NPOI.SS.UserModel;

namespace EngineNS.Bricks.CodeBuilder.MacrossNode
{
    public interface IMacrossMethodHolder : IGraphEditor
    {
        public MemberVar DraggingMember { get; set; }
        public bool IsDraggingMember { get; set; }
        public MethodLocalVar DraggingLocalVar { get; set; }
        public bool IsDraggingLocalVar { get; set; }
        public TtClassDeclaration DefClass { get; }
        public UCSharpCodeGenerator CSCodeGen { get; }
        public UHLSLCodeGenerator HlslCodeGen { get; }
        public TtCodeGeneratorBase CodeGen { get; }
        public List<UMacrossMethodGraph> Methods { get; }
        public EGui.Controls.PropertyGrid.PropertyGrid PGMember { get; set; }
        public void RemoveMethod(UMacrossMethodGraph method);
        public void SetConfigUnionNode(NodeGraph.IUnionNode node);
    }
    public partial class UMethodStartNode : UNodeBase, IAfterExecNode
    {
        public override string Label 
        {
            get
            {
                for(int i=0; i<MethodGraph.MethodDatas.Count; i++)
                {
                    if(MethodGraph.MethodDatas[i].StartNode == this)
                    {
                        base.Label = MethodGraph.MethodDatas[i].MethodDec.DisplayName;
                        return base.Label;
                    }
                }

                return base.Label;
            }
            set => base.Label = value; 
        }

        public PinOut AfterExec { get; set; } = new PinOut();
        string mMethodDecKeyword;
        [Rtti.Meta]
        public string MethodDecKeyword 
        {
            get => mMethodDecKeyword;
            set
            {
                mMethodDecKeyword = value;
                TtMethodDeclaration method = null;
                for (int i = 0; i < MethodGraph.MethodDatas.Count; i++)
                {
                    if (MethodGraph.MethodDatas[i].MethodDec.GetKeyword() == value)
                    {
                        MethodGraph.MethodDatas[i].StartNode = this;
                        method = MethodGraph.MethodDatas[i].MethodDec;
                        break;
                    }
                }
                if(method == null)
                {
                    HasError = true;
                    CodeExcept = new GraphException(this, null, $"Can not find method with {value}");
                }
                else
                    Initialize(MethodGraph, method);
            }
        }

        public static UMethodStartNode NewStartNode(UMacrossMethodGraph graph, TtMethodDeclaration methodDec)
        {
            var result = new UMethodStartNode();
            result.MethodGraph = graph;
            result.Initialize(graph, methodDec);
            return result;
        }
        public UMethodStartNode()
        {
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF40FF40;
            TitleColor = 0xFF804020;
            BackColor = 0x80808080;

            AfterExec.Name = ">> ";
            AfterExec.LinkDesc = MacrossStyles.Instance.NewExecPinDesc();
            Position = new Vector2(100, 100);
        }
        private void Initialize(UMacrossMethodGraph graph, TtMethodDeclaration methodDec)
        {
            MethodGraph = graph;
            mMethodDecKeyword = methodDec.GetKeyword();

            AddPinOut(AfterExec);
            UpdateMethodDefine(methodDec);
            OnPositionChanged();
        }
        public override void OnPreRead(object tagObject, object hostObject, bool fromXml)
        {
            base.OnPreRead(tagObject, hostObject, fromXml);
            var graph = hostObject as UMacrossMethodGraph;
            if (graph == null)
                return;

            MethodGraph = graph;
        }
        public UMacrossMethodGraph MethodGraph;
        public List<PinOut> Arguments = new List<PinOut>();
        List<PinOut> mTemplateArguments = new List<PinOut>();
        public void UpdateMethodDefine(TtMethodDeclaration methodDec)
        {
            mMethodDecKeyword = methodDec.GetKeyword();

            for (int i = 0; i < methodDec.Arguments.Count; i++)
            {
                if(methodDec.Arguments[i].VariableName == null)
                {
                    methodDec.Arguments[i].VariableName = $"arg{i}";
                }
            }
            Name = methodDec.MethodName;

            var ignoreOut = !methodDec.IsOverride;
            mTemplateArguments.Clear();
            foreach (var i in methodDec.Arguments)
            {
                if (ignoreOut && i.OperationType == EMethodArgumentAttribute.Out)
                    continue;
                PinOut argPin = null;
                foreach(var j in Arguments)
                {
                    var defType = j.Tag as TtTypeReference;
                    if (j.Name == i.VariableName && defType == i.VariableType)
                    {
                        argPin = j;
                        Arguments.Remove(j);
                        // todo: 标记为丢失参数
                        Outputs.Remove(j);//非常危险的操作，这里有把握才能操作
                        break;
                    }
                }
                if (argPin == null)
                {
                    argPin = new PinOut();
                    argPin.MultiLinks = true;
                    argPin.Tag = i.VariableType;
                    argPin.Name = i.VariableName;
                }
                mTemplateArguments.Add(argPin);
            }

            foreach (var i in Arguments)
            {
                MethodGraph.RemoveLinkedOut(i);
                RemovePinOut(i);
            }
            Arguments.Clear();

            foreach (var i in mTemplateArguments)
            {
                Arguments.Add(i);
                AddPinOut(i);
            }

            if (this.ParentGraph != null)
            {
                foreach (var i in this.ParentGraph.Nodes)
                {
                    var retNode = i as ReturnNode;
                    if (retNode == null)
                        continue;

                    retNode.Initialize(MethodGraph);
                    //if (retNode.ReturnValuePin != null && retNode.ReturnType != MethodGraph.Function.ReturnType)
                    //{
                    //    if (retNode.ReturnValuePin.HasLinker())
                    //    {
                    //        //FuncGraph.RemoveLinkedIn(retNode.ReturnValuePin);
                    //        retNode.HasError = true;
                    //    }
                    //    else
                    //    {
                    //        retNode.ReturnType = MethodGraph.Function.ReturnType;
                    //        retNode.Initialize(this.ParentGraph as UMacrossMethodGraph);
                    //    }
                    //}
                }
            }

            OnPositionChanged();
        }
        public override void BuildStatements(NodePin pin, ref BuildCodeStatementsData data)
        {
            for(int i=0; i<data.MethodDec.Arguments.Count; i++)
            {
                if(data.MethodDec.Arguments[i].OperationType == EMethodArgumentAttribute.Out)
                {
                    var assignOp = new TtAssignOperatorStatement();
                    assignOp.To = new TtVariableReferenceExpression() { VariableName = data.MethodDec.Arguments[i].VariableName };
                    assignOp.From = new TtDefaultValueExpression() { Type = data.MethodDec.Arguments[i].VariableType };
                    data.CurrentStatements.Add(assignOp);
                }
            }

            var links = new List<UPinLinker>();
            data.NodeGraph.FindOutLinker(AfterExec, links);
            foreach (var i in links)
            {
                var nextNode = i.InNode;
                var nextPin = i.InPin;
                nextNode.BuildStatements(nextPin, ref data);
                //funGraph.Function.Body.PushExpr(nextNode.GetExpr(funGraph, cGen, false));
            }
        }

        public override CodeBuilder.TtExpressionBase GetExpression(NodePin pin, ref BuildCodeStatementsData data)
        {
            for(int i=0; i<Arguments.Count; i++)
            {
                if(pin == Arguments[i])
                {
                    return new TtVariableReferenceExpression(Arguments[i].Name);
                }
            }
            return null;
        }
        public override Rtti.TtTypeDesc GetOutPinType(PinOut pin)
        {
            for (int i = 0; i < Arguments.Count; i++)
            {
                if (pin == Arguments[i])
                {
                    var argType = (TtTypeReference)(Arguments[i].Tag);
                    var typeDesc = argType.TypeDesc;
                    if (typeDesc != null)
                        return typeDesc;
                }
            }
            return null;
        }
        public override void OnMouseStayPin(NodePin stayPin, UNodeGraph graph)
        {
            for(int i=0; i<Arguments.Count; i++)
            {
                if(stayPin == Arguments[i])
                {
                    string typeFullName = "";
                    var argType = (TtTypeReference)(Arguments[i].Tag);
                    var typeDesc = argType.TypeDesc;
                    if (typeDesc != null)
                        typeFullName = typeDesc.FullName;

                    string valueString = GetRuntimeValueString(stayPin.Name);
                    EGui.Controls.CtrlUtility.DrawHelper($"{valueString}({typeFullName})");
                }
            }
        }

        public override object GetPropertyEditObject()
        {
            return MethodGraph;
        }
    }
    public class MethodData : IO.ISerializer
    {
        public UMethodStartNode StartNode;
        [Rtti.Meta]
        public TtMethodDeclaration MethodDec { get; set; }
        public Rtti.TtClassMeta.TtMethodMeta Method;
        [Rtti.Meta]
        public bool IsDelegate { get; set; } = false;

        public string GetMethodName()
        {
            return MethodDec.MethodName;
        }

        //public static MethodData CreateFromMethod(UMacrossMethodGraph graph, System.Reflection.MethodInfo method)
        //{
        //    MethodData methodData = new MethodData();
        //    methodData.MethodDec = new UMethodDeclaration()
        //    {
        //        IsOverride = true,
        //        MethodName = method.Name,
        //    };

        //    foreach (var param in method.GetParameters())
        //    {
        //        var argDec = new UMethodArgumentDeclaration()
        //        {
        //            VariableType = new UTypeReference(param.ParameterType),
        //            VariableName = param.Name,
        //            OperationType = EMethodArgumentAttribute.Default,
        //        };
        //        if (param.IsOut)
        //            argDec.OperationType = EMethodArgumentAttribute.Out;
        //        else if (param.IsIn)
        //            argDec.OperationType = EMethodArgumentAttribute.In;
        //        else if (param.ParameterType.IsByRef)
        //            argDec.OperationType = EMethodArgumentAttribute.Ref;
        //        methodData.MethodDec.Arguments.Add(argDec);
        //    }

        //    if(method.ReturnType != typeof(void))
        //    {
        //        methodData.MethodDec.ReturnValue = new UVariableDeclaration()
        //        {
        //            VariableType = new UTypeReference(method.ReturnType),
        //            VariableName = method.Name + "_ReturnValue",
        //        };
        //    }

        //    methodData.StartNode = UMethodStartNode.NewStartNode(graph, methodData.MethodDec);
        //    return methodData;
        //}
        public static MethodData CreateFromMethod(UMacrossMethodGraph graph, TtMethodDeclaration method)
        {
            MethodData methodData = new MethodData();
            methodData.MethodDec = method;
            methodData.StartNode = UMethodStartNode.NewStartNode(graph, method);
            return methodData;
        }
        public override string ToString()
        {
            return (MethodDec.IsOverride ? "[O]" : "") + MethodDec.MethodName;
        }

        public string DisplayName
        {
            get
            {
                return (MethodDec.IsOverride ? "[O]" : "") + MethodDec.DisplayName;
            }
        }

        public void OnPreRead(object tagObject, object hostObject, bool fromXml)
        {
        }

        public void OnPropertyRead(object tagObject, System.Reflection.PropertyInfo prop, bool fromXml)
        {
        }
    }
    public partial class UMacrossMethodGraph : UNodeGraph, IPropertyCustomization
    {
        [Rtti.Meta, Category("Option")]
        public List<TtVariableDeclaration> LocalVars { get; set; } = new List<TtVariableDeclaration>();
        public TtVariableDeclaration FindLocalVar(string name)
        {
            foreach(var i in LocalVars)
            {
                if (i.VariableName == name)
                    return i;
            }
            return null;
        }
        [Rtti.Meta, Category("Option")]
        public string CustumCode { get; set; } = null;
        [Rtti.Meta, Category("Option")]
        public bool IsUseCustumCode { get; set; } = false;
        bool mInputsDirty = true;
        List<TtMethodArgumentDeclaration> mInputs = new List<TtMethodArgumentDeclaration>();
        [InputsOperationCallback, Category("Params")]
        public List<TtMethodArgumentDeclaration> Inputs
        {
            get
            {
                if(mInputsDirty)
                {
                    mInputs.Clear();
                    for (int i = 0; i < MethodDatas[0].MethodDec.Arguments.Count; i++)
                    {
                        var arg = MethodDatas[0].MethodDec.Arguments[i];
                        switch(arg.OperationType)
                        {
                            case EMethodArgumentAttribute.Default:
                            case EMethodArgumentAttribute.In:
                            case EMethodArgumentAttribute.Ref:
                                mInputs.Add(arg);
                                break;
                        }
                        arg.GetErrorStringAction = GetVariableErrorString;
                    }
                    mInputsDirty = false;
                }

                return mInputs;
            }
            set
            {
                mInputs = value;
                MethodDatas[0].StartNode.UpdateMethodDefine(MethodDatas[0].MethodDec);
            }
        }
        class InputsOperationCallbackAttribute : PGListOperationCallbackAttribute
        {
            void PreInsertOperation(int index, object value, UMacrossMethodGraph graph)
            {
                var name = "InValue" + index;
                var methodDec = graph.MethodDatas[0].MethodDec;
                var sameName = false;
                do
                {
                    sameName = false;
                    for(int i=0; i < methodDec.Arguments.Count; i++)
                    {
                        if (methodDec.Arguments[i].VariableName == name)
                        {
                            name += "1";
                            sameName = true;
                            break;
                        }
                    }
                }
                while (sameName);
                var arg = value as TtMethodArgumentDeclaration;
                arg.VariableName = name;
                arg.OperationVisible = false;
                arg.OperationType = EMethodArgumentAttribute.Default;
            }
            public override void OnPreInsert(int index, object value, object objInstance)
            {
                var enumrableInterface = objInstance.GetType().GetInterface(typeof(IEnumerable).FullName, false);
                if(enumrableInterface != null)
                {
                    foreach(var ins in (IEnumerable)objInstance)
                    {
                        var graph = ins as UMacrossMethodGraph;
                        PreInsertOperation(index, value, graph);
                    }
                }
                else
                {
                    var graph = objInstance as UMacrossMethodGraph;
                    PreInsertOperation(index, value, graph);
                }
            }
            void AfterInsertOperation(int index, object value, UMacrossMethodGraph graph)
            {
                var arg = value as TtMethodArgumentDeclaration;
                var methodDec = graph.MethodDatas[0].MethodDec;
                if (index >= methodDec.Arguments.Count)
                    methodDec.Arguments.Add(arg);
                else
                    methodDec.Arguments.Insert(index, arg);
            }
            public override void OnAfterInsert(int index, object value, object objInstance)
            {
                var enumrableInterface = objInstance.GetType().GetInterface(typeof(IEnumerable).FullName, false);
                if(enumrableInterface != null)
                {
                    foreach(var ins in (IEnumerable)objInstance)
                    {
                        if (ins == null)
                            continue;

                        var graph = ins as UMacrossMethodGraph;
                        AfterInsertOperation(index, value, graph);
                    }
                }
                else
                {
                    var graph = objInstance as UMacrossMethodGraph;
                    AfterInsertOperation(index, value, graph);
                }
            }
            void AfterRemoveOperation(int index, UMacrossMethodGraph graph)
            {
                var methodDec = graph.MethodDatas[0].MethodDec;
                methodDec.Arguments.RemoveAt(index);
            }
            public override void OnAfterRemoveAt(int index, object objInstance)
            {
                var enumrableInterface = objInstance.GetType().GetInterface(typeof(IEnumerable).FullName, false);
                if(enumrableInterface != null)
                {
                    foreach(var ins in (IEnumerable)objInstance)
                    {
                        if (ins == null)
                            continue;

                        var graph = ins as UMacrossMethodGraph;
                        AfterRemoveOperation(index, graph);
                    }
                }
                else
                {
                    var graph = objInstance as UMacrossMethodGraph;
                    AfterRemoveOperation(index, graph);
                }
            }

            TtMethodArgumentDeclaration mOldDec = new TtMethodArgumentDeclaration();
            void PreValueChanged(int index, UMacrossMethodGraph graph, TtMethodArgumentDeclaration value)
            {
                mOldDec.VariableName = value.VariableName;
            }
            public override void OnPreValueChanged(int index, object value, object objInstance)
            {
                var enumrableInterface = objInstance.GetType().GetInterface(typeof(IEnumerable).FullName, false);
                if (enumrableInterface != null)
                {
                    foreach (var ins in (IEnumerable)objInstance)
                    {
                        if (ins == null)
                            continue;

                        var graph = ins as UMacrossMethodGraph;
                        PreValueChanged(index, graph, value as TtMethodArgumentDeclaration);
                    }
                }
                else
                {
                    var graph = objInstance as UMacrossMethodGraph;
                    PreValueChanged(index, graph, value as TtMethodArgumentDeclaration);
                }
            }
            void AfterValueChanged(int index, UMacrossMethodGraph graph, TtMethodArgumentDeclaration value)
            {
                if(value.VariableName != mOldDec.VariableName)
                {
                    for(int i=0; i<graph.MethodDatas[0].StartNode.Arguments.Count; i++)
                    {
                        var pin = graph.MethodDatas[0].StartNode.Arguments[i];
                        if (pin.Name == mOldDec.VariableName)
                        {
                            pin.Name = value.VariableName;
                            break;
                        }
                    }
                }
            }
            public override void OnAfterValueChanged(int index, object value, object objInstance)
            {
                var enumrableInterface = objInstance.GetType().GetInterface(typeof(IEnumerable).FullName, false);
                if (enumrableInterface != null)
                {
                    foreach (var ins in (IEnumerable)objInstance)
                    {
                        if (ins == null)
                            continue;

                        var graph = ins as UMacrossMethodGraph;
                        AfterValueChanged(index, graph, value as TtMethodArgumentDeclaration);
                    }
                }
                else
                {
                    var graph = objInstance as UMacrossMethodGraph;
                    AfterValueChanged(index, graph, value as TtMethodArgumentDeclaration);
                }
            }
        }

        bool mOutputsDirty = true;
        List<TtMethodArgumentDeclaration> mOutputs = new List<TtMethodArgumentDeclaration>();
        [OutputsOperationCallback, Category("Params")]
        public List<TtMethodArgumentDeclaration> Outputs
        {
            get
            {
                if(mOutputsDirty)
                {
                    mOutputs.Clear();
                    for (int i = 0; i < MethodDatas[0].MethodDec.Arguments.Count; i++)
                    {
                        var arg = MethodDatas[0].MethodDec.Arguments[i];
                        switch (arg.OperationType)
                        {
                            case EMethodArgumentAttribute.Ref:
                            case EMethodArgumentAttribute.Out:
                                mOutputs.Add(arg);
                                break;
                        }
                        arg.GetErrorStringAction = GetVariableErrorString;
                    }
                    mOutputsDirty = false;
                }
                return mOutputs;
            }
            set
            {
                mOutputs = value;
                bool hasReturnNode = false;
                for (int i = 0; i < Nodes.Count; i++)
                {
                    if (Nodes[i] is ReturnNode)
                    {
                        var retNode = Nodes[i] as ReturnNode;
                        retNode.UpdateMethodDefine(MethodDatas[0].MethodDec);
                        hasReturnNode = true;
                    }
                }
                if(!hasReturnNode)
                {
                    var node = new ReturnNode();
                    SetDefaultActionForNode(node);
                    this.AddNode(node);
                    node.UpdateMethodDefine(MethodDatas[0].MethodDec);
                    node.Position = MethodDatas[0].StartNode.Position + new Vector2(300, 0);

                }
            }
        }
        class OutputsOperationCallbackAttribute : PGListOperationCallbackAttribute
        {
            void PreInsertOperation(int index, object value, UMacrossMethodGraph graph)
            {
                var name = "OutValue" + index;
                var methodDec = graph.MethodDatas[0].MethodDec;
                var sameName = false;
                do
                {
                    sameName = false;
                    for (int i = 0; i < methodDec.Arguments.Count; i++)
                    {
                        if (methodDec.Arguments[i].VariableName == name)
                        {
                            name += "1";
                            sameName = true;
                            break;
                        }
                    }
                }
                while (sameName);
                var arg = value as TtMethodArgumentDeclaration;
                arg.VariableName = name;
                arg.OperationVisible = false;
                arg.OperationType = EMethodArgumentAttribute.Out;
            }
            public override void OnPreInsert(int index, object value, object objInstance)
            {
                var enumrableInterface = objInstance.GetType().GetInterface(typeof(IEnumerable).FullName, false);
                if (enumrableInterface != null)
                {
                    foreach (var ins in (IEnumerable)objInstance)
                    {
                        var graph = ins as UMacrossMethodGraph;
                        PreInsertOperation(index, value, graph);
                    }
                }
                else
                {
                    var graph = objInstance as UMacrossMethodGraph;
                    PreInsertOperation(index, value, graph);
                }
            }
            void AfterInsertOperation(int index, object value, UMacrossMethodGraph graph)
            {
                var arg = value as TtMethodArgumentDeclaration;
                var methodDec = graph.MethodDatas[0].MethodDec;
                var realIdx = graph.Inputs.Count + index;
                if (realIdx >= methodDec.Arguments.Count)
                    methodDec.Arguments.Add(arg);
                else
                    methodDec.Arguments.Insert(realIdx, arg);
            }
            public override void OnAfterInsert(int index, object value, object objInstance)
            {
                var enumrableInterface = objInstance.GetType().GetInterface(typeof(IEnumerable).FullName, false);
                if (enumrableInterface != null)
                {
                    foreach (var ins in (IEnumerable)objInstance)
                    {
                        if (ins == null)
                            continue;

                        var graph = ins as UMacrossMethodGraph;
                        AfterInsertOperation(index, value, graph);
                    }
                }
                else
                {
                    var graph = objInstance as UMacrossMethodGraph;
                    AfterInsertOperation(index, value, graph);
                }
            }
            void AfterRemoveOperation(int index, UMacrossMethodGraph graph)
            {
                var methodDec = graph.MethodDatas[0].MethodDec;
                methodDec.Arguments.RemoveAt(index + graph.Inputs.Count);
            }
            public override void OnAfterRemoveAt(int index, object objInstance)
            {
                var enumrableInterface = objInstance.GetType().GetInterface(typeof(IEnumerable).FullName, false);
                if (enumrableInterface != null)
                {
                    foreach (var ins in (IEnumerable)objInstance)
                    {
                        if (ins == null)
                            continue;

                        var graph = ins as UMacrossMethodGraph;
                        AfterRemoveOperation(index, graph);
                    }
                }
                else
                {
                    var graph = objInstance as UMacrossMethodGraph;
                    AfterRemoveOperation(index, graph);
                }
            }
            TtMethodArgumentDeclaration mOldDec = new TtMethodArgumentDeclaration();
            void PreValueChanged(int index, UMacrossMethodGraph graph, TtMethodArgumentDeclaration value)
            {
                mOldDec.VariableName = value.VariableName;
            }
            public override void OnPreValueChanged(int index, object value, object objInstance)
            {
                var enumrableInterface = objInstance.GetType().GetInterface(typeof(IEnumerable).FullName, false);
                if (enumrableInterface != null)
                {
                    foreach (var ins in (IEnumerable)objInstance)
                    {
                        if (ins == null)
                            continue;

                        var graph = ins as UMacrossMethodGraph;
                        PreValueChanged(index, graph, value as TtMethodArgumentDeclaration);
                    }
                }
                else
                {
                    var graph = objInstance as UMacrossMethodGraph;
                    PreValueChanged(index, graph, value as TtMethodArgumentDeclaration);
                }
            }
            void AfterValueChanged(int index, UMacrossMethodGraph graph, TtMethodArgumentDeclaration value)
            {
                if (value.VariableName != mOldDec.VariableName)
                {
                    for (int i = 0; i < graph.Nodes.Count; i++)
                    {
                        if (graph.Nodes[i] is ReturnNode)
                        {
                            var retNode = graph.Nodes[i] as ReturnNode;
                            for (int argIdx = 0; argIdx < retNode.Arguments.Count; argIdx++)
                            {
                                var pin = retNode.Arguments[argIdx];
                                if (pin.Name == mOldDec.VariableName)
                                {
                                    pin.Name = value.VariableName;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            public override void OnAfterValueChanged(int index, object value, object objInstance)
            {
                var enumrableInterface = objInstance.GetType().GetInterface(typeof(IEnumerable).FullName, false);
                if (enumrableInterface != null)
                {
                    foreach (var ins in (IEnumerable)objInstance)
                    {
                        if (ins == null)
                            continue;

                        var graph = ins as UMacrossMethodGraph;
                        AfterValueChanged(index, graph, value as TtMethodArgumentDeclaration);
                    }
                }
                else
                {
                    var graph = objInstance as UMacrossMethodGraph;
                    AfterValueChanged(index, graph, value as TtMethodArgumentDeclaration);
                }
            }
        }

        string GetVariableErrorString(in PGCustomValueEditorAttribute.EditorInfo info, TtMethodArgumentDeclaration dec, object newValue)
        {
            var newName = (string)newValue;
            for(int i=0; i<Inputs.Count; i++)
            {
                if ((Inputs[i].VariableName == newName) && (Inputs[i] != dec))
                    return "Same name with input " + i;
            }
            for(int i=0; i<Outputs.Count; i++)
            {
                if ((Outputs[i].VariableName == newName) && (Outputs[i] != dec))
                    return "Same name with output " + i;
            }
            return null;
        }
        public static UMacrossMethodGraph NewGraph(IMacrossMethodHolder kls, TtMethodDeclaration method = null)
        {
            var result = new UMacrossMethodGraph();
            result.MacrossEditor = kls;
            result.Initialize();
            //result.FunctionName = funName;
            //if (result.Function == null)
            //    return null;
            if (method != null)
            {
                var methodData = MethodData.CreateFromMethod(result, method);
                result.MethodDatas.Add(methodData);
                result.AddNode(methodData.StartNode);
                result.GraphName = method.MethodName;
            }
            return result;
        }
        public override void OnPreRead(object tagObject, object hostObject, bool fromXml)
        {
            var klsGraph = tagObject as UMacrossEditor;
            if (klsGraph == null)
                return;

            this.MacrossEditor = klsGraph;
        }
        public override void OnPropertyRead(object tagObject, System.Reflection.PropertyInfo prop, bool fromXml)
        {
            var klsGraph = tagObject as UMacrossEditor;
            if (klsGraph == null)
                return;
        }
        public Bricks.NodeGraph.TtGraphRenderer GraphRenderer = new NodeGraph.TtGraphRenderer();
        //[Obsolete]
        //public void BuildCodeExpr(ICodeGen cGen)
        //{
        //    Function.LocalVars.Clear();
        //    Function.Body.Lines.Clear();
        //    foreach(UNodeExpr i in this.Nodes)
        //    {
        //        i.HasError = false;
        //        i.CodeExcept = null;
        //    }
        //    StartNode.BuildExpr(this, cGen);
        //}
        public void BuildExpression(TtClassDeclaration classDesc)
        {
            for(int i=0; i<Nodes.Count; i++)
            {
                (Nodes[i]).ResetErrors();
            }
            for(int i=0; i<MethodDatas.Count; i++)
            {
                MethodDatas[i].MethodDec.MethodBody.Sequence.Clear();
                MethodDatas[i].MethodDec.LocalVariables.Clear();
                MethodDatas[i].MethodDec.LocalVariables.AddRange(this.LocalVars);
                BuildCodeStatementsData data = new BuildCodeStatementsData()
                {
                    ClassDec = classDesc,
                    MethodDec = MethodDatas[i].MethodDec,
                    CodeGen = MacrossEditor.CodeGen,
                    NodeGraph = this,
                    CurrentStatements = MethodDatas[i].MethodDec.MethodBody.Sequence,
                };
                MethodDatas[i].StartNode.BuildStatements(null, ref data);
            }
        }
        //[Rtti.Meta]
        //public string FunctionName
        //{
        //    get { return Function.GetFunctionDeclType(); }
        //}
        //[Rtti.Meta]
        //public Guid StartNodeId
        //{
        //    get { return StartNode.NodeId; }
        //    set
        //    {
        //        StartNode = this.FindNode(value) as UMethodStartNode;
        //        if (StartNode == null)
        //        {
        //            StartNode = UMethodStartNode.NewStartNode(this);
        //            AddNode(StartNode);
        //        }
        //    }
        //}
        //private UMethodStartNode StartNode;
        [Browsable(false)]
        public IMacrossMethodHolder MacrossEditor
        {
            get => (IMacrossMethodHolder)Editor;
            protected set
            {
                Editor = value;
            }
        }
        [GraphName]
        public string Name
        {
            get => ToString();
            set
            {
                if(MethodDatas.Count == 1)
                {
                    var methodDec = MethodDatas[0].MethodDec;
                    if (!methodDec.IsOverride)
                        methodDec.MethodName = value;
                }
                GraphName = value;
            }
        }

        public string DisplayName
        {
            get
            {
                if (MethodDatas.Count == 1)
                    return MethodDatas[0].DisplayName;
                return GraphName;
            }
        }

        class GraphNameAttribute : EGui.Controls.PropertyGrid.PGCustomValueEditorAttribute
        {
            protected override async Task<bool> Initialize_Override()
            {
                return await base.Initialize_Override();
            }
            protected override void Cleanup_Override()
            {
                base.Cleanup_Override();
            }
            public override bool OnDraw(in EditorInfo info, out object newValue)
            {
                bool isReadonly = true;
                var enumrableInterface = info.ObjectInstance.GetType().GetInterface(typeof(IEnumerable).FullName, false);
                if(enumrableInterface != null)
                {
                    foreach(var objIns in (IEnumerable)info.ObjectInstance)
                    {
                        if (objIns == null)
                            continue;

                        var graph = objIns as UMacrossMethodGraph;
                        if (graph != null && graph.MethodDatas.Count == 1)
                        {
                            if (!graph.MethodDatas[0].MethodDec.IsOverride)
                            {
                                isReadonly = false;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    var graph = info.ObjectInstance as UMacrossMethodGraph;
                    if (graph != null && graph.MethodDatas.Count == 1)
                    {
                        if (!graph.MethodDatas[0].MethodDec.IsOverride)
                            isReadonly = false;
                    }
                }
                EditorInfo tempInfo = new EditorInfo();
                info.CopyTo(ref tempInfo);
                tempInfo.Readonly = isReadonly;
                return StringEditor.OnDraw(this, in info, out newValue);
            }
        }

        public override string ToString()
        {
            if (MethodDatas.Count == 1)
                return MethodDatas[0].ToString();
            return GraphName;
        }
        public bool VisibleInClassGraphTables = false;
        //[Rtti.Meta]
        //public DefineFunction Function { get; set; }
        [Rtti.Meta]
        [Browsable(false)]
        public List<MethodData> MethodDatas
        {
            get;
            set;
        } = new List<MethodData>();

        public bool IsDelegateGraph()
        {
            for (int i = 0; i < MethodDatas.Count; i++)
            {
                if (MethodDatas[i].IsDelegate)
                    return true;
            }
            return false;
        }

        string[] GetContextPath(Rtti.TtTypeDesc type, string name)
        {
            var typeStr = Rtti.TtTypeDescManager.Instance.GetTypeStringFromType(type, false);
            typeStr = typeStr.Replace("EngineNS.", "").Replace("Bricks.", "");
            var idx = typeStr.LastIndexOf('@');
            if(idx >= 0)
                typeStr = typeStr.Substring(0, idx);
            typeStr += "." + name;
            return typeStr.Split('.');
        }
        public override void UpdateCanvasMenus()
        {
            var editor = this.Editor as UMacrossEditor;

            CanvasMenus.SubMenuItems.Clear();
            CanvasMenus.Text = "Canvas";
            foreach (var service in Rtti.TtTypeDescManager.Instance.Services.Values)
            {
                foreach (var typeDesc in service.Types.Values)
                {
                    {
                        var atts = typeDesc.GetCustomAttributes(typeof(ContextMenuAttribute), true);
                        if (atts.Length > 0)
                        {
                            var parentMenu = CanvasMenus;
                            var att = atts[0] as ContextMenuAttribute;
                            if (!att.HasKeyString(UMacross.MacrossEditorKeyword))
                                continue;
                            for (var menuIdx = 0; menuIdx < att.MenuPaths.Length; menuIdx++)
                            {
                                var menuStr = att.MenuPaths[menuIdx];
                                var menuName = GetMenuName(menuStr);
                                if (menuIdx < att.MenuPaths.Length - 1)
                                    parentMenu = parentMenu.AddMenuItem(menuName, null, null);
                                else
                                {
                                    parentMenu.AddMenuItem(menuName, att.FilterStrings, null,
                                        (TtMenuItem item, object sender) =>
                                        {
                                            var node = Rtti.TtTypeDescManager.CreateInstance(typeDesc) as UNodeBase;
                                            var nodeName = GetSerialFinalString(menuStr, GenSerialId());
                                            if (nodeName != null)
                                                node.Name = nodeName;
                                            node.UserData = MacrossEditor;
                                            node.Position = PopMenuPosition;
                                            SetDefaultActionForNode(node);
                                            this.AddNode(node);
                                        });
                                }
                            }
                        }
                    }
                }
            }
            foreach(var metaData in Rtti.TtClassMetaManager.Instance.Metas)
            {
                // create
                if(metaData.Value.MetaAttribute != null && !metaData.Value.MetaAttribute.IsNoMacrossCreate)
                {

                }
                // static method
                foreach(var methodMeta in metaData.Value.Methods)
                {
                    if (!methodMeta.IsStatic)
                        continue;
                    if (methodMeta.Meta.IsNoMacrossUseable)
                        continue;

                    if (this.IsMetaFilter(methodMeta.Meta) == false)
                        continue;

                    string[] path = methodMeta.Meta.MacrossDisplayPath;
                    if(path == null)
                        path = GetContextPath(methodMeta.DeclaringType, methodMeta.MethodName);
                    var parentMenu = CanvasMenus;
                    for(var menuIdx = 0; menuIdx < path.Length; menuIdx++)
                    {
                        var menuStr = path[menuIdx];
                        var menuName = GetMenuName(menuStr);
                        if (menuIdx < path.Length - 1)
                            parentMenu = parentMenu.AddMenuItem(menuName, null, null);
                        else
                        {
                            parentMenu.AddMenuItem(menuName, methodMeta.MethodName, null,
                                (TtMenuItem item, object sender) =>
                                {
                                    var node = MethodNode.NewMethodNode(methodMeta);
                                    node.Position = PopMenuPosition;
                                    SetDefaultActionForNode(node);
                                    this.AddNode(node);
                                });
                        }
                    }
                }
                // static fields
                foreach(var fieldMeta in metaData.Value.Fields)
                {
                    if (!fieldMeta.IsStatic)
                        continue;
                    if(!fieldMeta.IsPublic)
                        continue;
                    if (fieldMeta.Meta.IsNoMacrossUseable)
                        continue;
                    if (this.IsMetaFilter(fieldMeta.Meta) == false)
                        continue;
                    var path = fieldMeta.Meta.MacrossDisplayPath;
                    if (path == null)
                        path = GetContextPath(fieldMeta.DeclaringType, fieldMeta.FieldName);
                    var parentMenu = CanvasMenus;
                    for (var menuIdx = 0; menuIdx < path.Length; menuIdx++)
                    {
                        var menuStr = path[menuIdx];
                        var menuName = GetMenuName(menuStr);
                        if (menuIdx < path.Length - 1)
                            parentMenu = parentMenu.AddMenuItem(menuName, null, null);
                        else
                        {
                            parentMenu.AddMenuItem("get " + menuName, fieldMeta.FieldName, null,
                                (TtMenuItem item, object sender) =>
                                {
                                    var node = ClassFieldVar.NewClassMemberVar(fieldMeta, true);
                                    node.Position = PopMenuPosition;
                                    SetDefaultActionForNode(node);
                                    this.AddNode(node);
                                });
                            parentMenu.AddMenuItem("set " + menuName, fieldMeta.FieldName, null,
                                (TtMenuItem item, object sender) =>
                                {
                                    var node = ClassFieldVar.NewClassMemberVar(fieldMeta, false);
                                    node.Position = PopMenuPosition;
                                    SetDefaultActionForNode(node);
                                    this.AddNode(node);
                                });
                        }
                    }
                }
                // static properties
                foreach(var proMeta in metaData.Value.Properties)
                {
                    if (!proMeta.IsGetStatic && !proMeta.IsSetStatic)
                        continue;
                    if (!proMeta.IsGetPublic && !proMeta.IsSetPublic)
                        continue;
                    if (proMeta.Meta.IsNoMacrossUseable)
                        continue;
                    if (this.IsMetaFilter(proMeta.Meta) == false)
                        continue;
                    string[] path = proMeta.Meta.MacrossDisplayPath;
                    if (path == null)
                        path = GetContextPath(proMeta.HostType, proMeta.PropertyName);
                    var parentMenu = CanvasMenus;
                    for(var menuIdx = 0; menuIdx < path.Length; menuIdx++)
                    {
                        var menuStr = path[menuIdx];
                        var menuName = GetMenuName(menuStr);
                        if (menuIdx < path.Length - 1)
                            parentMenu = parentMenu.AddMenuItem(menuName, null, null);
                        else
                        {
                            if(proMeta.IsGetStatic && proMeta.IsGetPublic)
                            {
                                parentMenu.AddMenuItem("get " + menuName, proMeta.PropertyName, null,
                                    (TtMenuItem item, object sender) =>
                                    {
                                        var node = ClassPropertyVar.NewClassProperty(proMeta, true);
                                        node.Label = metaData.Value.ClassMetaName;
                                        node.UserData = MacrossEditor;
                                        node.Position = PopMenuPosition;
                                        SetDefaultActionForNode(node);
                                        this.AddNode(node);
                                    });
                            }
                            if(proMeta.IsSetStatic && proMeta.IsSetPublic && !proMeta.Meta.IsMacrossReadOnly)
                            {
                                parentMenu.AddMenuItem("set " + menuName, proMeta.PropertyName, null,
                                    (TtMenuItem item, object sender) =>
                                    {
                                        var node = ClassPropertyVar.NewClassProperty(proMeta, false);
                                        node.Label = metaData.Value.ClassMetaName;
                                        node.UserData = MacrossEditor;
                                        node.Position = PopMenuPosition;
                                        SetDefaultActionForNode(node);
                                        this.AddNode(node);
                                    });
                            }
                        }
                    }
                }
            }
            var Datas = CanvasMenus.AddMenuItem("Data", null, null);
            {
                Datas.AddMenuItem("TypeConverter", null,
                    (TtMenuItem item, object sender) =>
                    {
                        var type = Rtti.TtClassMetaManager.Instance.GetMeta(Rtti.TtTypeDesc.TypeStr(typeof(object)));
                        var node = TypeConverterVar.NewTypeConverterVar(type, type);
                        node.Name = $"lAnyVar_{GenSerialId()}";
                        node.UserData = MacrossEditor;
                        node.Position = PopMenuPosition;
                        SetDefaultActionForNode(node);
                        this.AddNode(node);
                    });
            }
            var flowControls = CanvasMenus.AddMenuItem("FlowControl", null, null);
            {
                flowControls.AddMenuItem("Return", null,
                    (TtMenuItem item, object sender) =>
                    {
                        var node = ReturnNode.NewReturnNode(this);
                        node.UserData = MacrossEditor;
                        node.Position = PopMenuPosition;
                        SetDefaultActionForNode(node);
                        this.AddNode(node);
                    });
            }

            var selfMenu = CanvasMenus.AddMenuItem("Self", null, null);
            for (int i = 0; i < MacrossEditor.DefClass.SupperClassNames.Count; i++)
            {
                var classMeta = Rtti.TtClassMetaManager.Instance.GetMetaFromFullName(MacrossEditor.DefClass.SupperClassNames[i]);
                if (classMeta == null)
                    continue;

                UpdateMenuWithClassMeta(classMeta, selfMenu);
            }

            var selfNode = CanvasMenus.AddMenuItem("Self", null, null);
            {
                var selfMeta = Rtti.TtClassMetaManager.Instance.GetMetaFromFullName(MacrossEditor.DefClass.GetFullName());
                if (selfMeta != null)
                {
                    UpdateMenuWithClassMeta(selfMeta, selfMenu);
                }
            }
            //var selfNode = CanvasMenus.AddMenuItem("Self", null, null);
            //{
            //    for(int i=0; i<MacrossEditor.DefClass.Methods.Count; i++)
            //    {
            //        var methodDef = MacrossEditor.DefClass.Methods[i];
            //        if (methodDef.IsOverride)
            //            continue;

            //        selfNode.AddMenuItem(methodDef.MethodName, null,
            //            (UMenuItem item, object sender) =>
            //            {
            //                var node = MethodNode.NewMethodNode(methodDef);
            //                node.UserData = MacrossEditor;
            //                node.Position = PopMenuPosition;
            //                SetDefaultActionForNode(node);
            //                this.AddNode(node);
            //            });
            //    }
            //}
        }

        public override void UpdateNodeMenus()
        {
            base.UpdateNodeMenus();
            int start = -1;
            int i = 0;
            for(i=0; i<NodeMenus.SubMenuItems.Count; i++)
            {
                var menu = NodeMenus.SubMenuItems[i];
                if (start >= 0)
                {
                    if (menu.IsSeparator)
                    {
                        start = i;
                        break;
                    }
                }
                if(menu.Text == "ORGANIZATION" && menu.IsSeparator)
                {
                    start = i;
                }
            }
            //var collapseToMethodAction = new Action<UMenuItem, object>((item, sender) =>
            //{
            //    var nodeList = new List<UNodeBase>(SelectedNodes.Count);
            //    for (int i = 0; i < SelectedNodes.Count; i++)
            //    {
            //        nodeList.Add(SelectedNodes[i].Node);
            //    }
            //    CollapseNodes(nodeList);
            //});
            //if(start < 0 || i > NodeMenus.SubMenuItems.Count)
            //{
            //    NodeMenus.InsertMenuItem(start, "Collapse to Method", null, collapseToMethodAction)
            //}
        }

        public override void CollapseNodes(List<UNodeBase> nodeList)
        {
            var node = IUnionNode.CreateUnionNode<UnionNode, UnionPinDefine, EndPointNode>(this, nodeList);
            node.Name = "Collapse Node";
            ((UMacrossMethodGraph)(node.ContentGraph)).MacrossEditor = MacrossEditor;
            DeleteSelectedNodes();
        }
        public override TtGraphRenderer GetGraphRenderer()
        {
            return GraphRenderer;
        }
        public bool IsMetaFilter(Rtti.MetaAttribute attr)
        {
            var editor = this.Editor as UMacrossEditor;
            if (editor != null && editor.IsGenShader && attr.ShaderName == null)
                return false;
            return true;
        }
        private void UpdateMenuWithClassMeta(Rtti.TtClassMeta classMeta, TtMenuItem menu)
        {
            for (int proIdx = 0; proIdx < classMeta.Properties.Count; proIdx++)
            {
                var pro = classMeta.Properties[proIdx];
                var metaAttr = pro.PropInfo.GetCustomAttribute<Rtti.MetaAttribute>(false);
                if (IsMetaFilter(metaAttr) == false)
                    continue;
                string[] menuPath = null;
                string filterStr = pro.PropertyName;
                var proInfo = pro.PropInfo;
                if (proInfo != null)
                {
                    var atts = proInfo.GetCustomAttributes(typeof(ContextMenuAttribute), false);
                    if (atts.Length > 0)
                    {
                        var att = atts[0] as ContextMenuAttribute;
                        menuPath = att.MenuPaths;
                        filterStr = att.FilterStrings;
                    }
                }
                if (menuPath == null)
                {
                    menuPath = new string[] { pro.PropertyName };
                }
                var parentMenu = menu;
                for (var menuIdx = 0; menuIdx < menuPath.Length; menuIdx++)
                {
                    var menuStr = menuPath[menuIdx];
                    var menuName = GetMenuName(menuStr);
                    if (menuIdx < menuPath.Length - 1)
                        parentMenu = parentMenu.AddMenuItem(menuName, null, null);
                    else
                    {
                        if(proInfo.CanRead)
                        {
                            parentMenu.AddMenuItem("Get " + menuName, filterStr, null,
                                (TtMenuItem item, object sender) =>
                                {
                                    var node = ClassPropertyVar.NewClassProperty(pro, true);
                                    var nodeName = GetSerialFinalString(menuStr, GenSerialId());
                                    if (nodeName != null)
                                        node.Name = nodeName;
                                    node.UserData = MacrossEditor;
                                    node.Position = PopMenuPosition;
                                    SetDefaultActionForNode(node);
                                    this.AddNode(node);

                                    if (LinkingOp.StartPin != null && Rtti.TtTypeDesc.CanCast(LinkingOp.StartPin.GetType(), typeof(PinOut)))
                                    {
                                        var outPin = LinkingOp.StartPin as PinOut;
                                        AddLink(outPin, node.Self, true);
                                    }
                                });
                        }
                        if(proInfo.CanWrite && !pro.Meta.IsMacrossReadOnly)
                        {
                            parentMenu.AddMenuItem("Set " + menuName, filterStr, null,
                                (TtMenuItem item, object sender) =>
                                {
                                    var node = ClassPropertyVar.NewClassProperty(pro, false);
                                    var nodeName = GetSerialFinalString(menuStr, GenSerialId());
                                    if (nodeName != null)
                                        node.Name = nodeName;
                                    node.UserData = MacrossEditor;
                                    node.Position = PopMenuPosition;
                                    SetDefaultActionForNode(node);
                                    this.AddNode(node);

                                    if (LinkingOp.StartPin != null && Rtti.TtTypeDesc.CanCast(LinkingOp.StartPin.GetType(), typeof(PinOut)))
                                    {
                                        var outPin = LinkingOp.StartPin as PinOut;
                                        AddLink(outPin, node.Self, true);
                                        var afterNode = outPin.HostNode as IAfterExecNode;
                                        if (afterNode != null && afterNode.AfterExec.HostNode == afterNode)
                                            AddLink(afterNode.AfterExec, node.BeforeExec, true);
                                    }
                                });
                        }
                    }
                }
            }
            for (int fieldIdx = 0; fieldIdx < classMeta.Fields.Count; fieldIdx++)
            {
                var field = classMeta.Fields[fieldIdx];
                string[] menuPath = null;
                var fieldInfo = field.GetFieldInfo();
                var metaAttr = fieldInfo.GetCustomAttribute<Rtti.MetaAttribute>(false);
                if (IsMetaFilter(metaAttr) == false)
                    continue;
                string filterStr = fieldInfo.Name;
                var atts = fieldInfo.GetCustomAttributes(typeof(ContextMenuAttribute), false);
                if (atts.Length > 0)
                {
                    var att = atts[0] as ContextMenuAttribute;
                    menuPath = att.MenuPaths;
                    filterStr = att.FilterStrings;
                }
                if (menuPath == null)
                    menuPath = new string[] { fieldInfo.Name };
                var parentMenu = menu;
                for (var menuIdx = 0; menuIdx < menuPath.Length; menuIdx++)
                {
                    var menuStr = menuPath[menuIdx];
                    var menuName = GetMenuName(menuStr);
                    if (menuIdx < menuPath.Length - 1)
                        parentMenu = parentMenu.AddMenuItem(menuName, null, null);
                    else
                    {
                        parentMenu.AddMenuItem("Get " + menuName, filterStr, null,
                            (TtMenuItem item, object sender) =>
                            {
                                var node = ClassFieldVar.NewClassMemberVar(field, true);
                                var nodeName = GetSerialFinalString(menuStr, GenSerialId());
                                if (nodeName != null)
                                    node.Name = nodeName;
                                node.UserData = MacrossEditor;
                                node.Position = PopMenuPosition;
                                SetDefaultActionForNode(node);
                                this.AddNode(node);

                                if (LinkingOp.StartPin != null && Rtti.TtTypeDesc.CanCast(LinkingOp.StartPin.GetType(), typeof(PinOut)))
                                {
                                    var outPin = LinkingOp.StartPin as PinOut;
                                    AddLink(outPin, node.Self, true);
                                }
                            });
                        parentMenu.AddMenuItem("Set " + menuName, filterStr, null,
                            (TtMenuItem item, object sender) =>
                            {
                                var node = ClassFieldVar.NewClassMemberVar(field, false);
                                var nodeName = GetSerialFinalString(menuStr, GenSerialId());
                                if (nodeName != null)
                                    node.Name = nodeName;
                                node.UserData = MacrossEditor;
                                node.Position = PopMenuPosition;
                                SetDefaultActionForNode(node);
                                this.AddNode(node);

                                if (LinkingOp.StartPin != null && Rtti.TtTypeDesc.CanCast(LinkingOp.StartPin.GetType(), typeof(PinOut)))
                                {
                                    var outPin = LinkingOp.StartPin as PinOut;
                                    AddLink(outPin, node.Self, true);
                                    var afterNode = outPin.HostNode as IAfterExecNode;
                                    if (afterNode != null && afterNode.AfterExec.HostNode == afterNode)
                                        AddLink(afterNode.AfterExec, node.BeforeExec, true);
                                }
                            });
                    }
                }
            }
            for (int methodIdx = 0; methodIdx < classMeta.Methods.Count; methodIdx++)
            {
                var method = classMeta.Methods[methodIdx];
                var metaAttr = method.GetFirstCustomAttribute<Rtti.MetaAttribute>(false);
                if (IsMetaFilter(metaAttr) == false)
                    continue;
                string[] menuPath = null;
                string filterStr = method.MethodName;
                var atts = method.GetCustomAttributes(typeof(ContextMenuAttribute), false);
                if (atts.Length > 0)
                {
                    var att = atts[0] as ContextMenuAttribute;
                    menuPath = att.MenuPaths;
                    filterStr = att.FilterStrings;
                }
                if (menuPath == null)
                    menuPath = new string[] { method.MethodName };
                var parentMenu = menu;
                for (var menuIdx = 0; menuIdx < menuPath.Length; menuIdx++)
                {
                    var menuStr = menuPath[menuIdx];
                    var menuName = GetMenuName(menuStr);
                    if (menuIdx < menuPath.Length - 1)
                        parentMenu = parentMenu.AddMenuItem(menuName, null, null);
                    else
                    {
                        parentMenu.AddMenuItem(menuName, filterStr, null,
                            (TtMenuItem item, object sender) =>
                            {
                                var node = MethodNode.NewMethodNode(method);
                                node.SetSelfMethod();
                                var nodeName = GetSerialFinalString(menuStr, GenSerialId());
                                if (nodeName != null)
                                    node.Name = nodeName;
                                node.UserData = MacrossEditor;
                                node.Position = PopMenuPosition;
                                SetDefaultActionForNode(node);
                                this.AddNode(node);

                                if(LinkingOp.StartPin != null && Rtti.TtTypeDesc.CanCast(LinkingOp.StartPin.GetType(), typeof(PinOut)))
                                {
                                    var outPin = LinkingOp.StartPin as PinOut;
                                    AddLink(outPin, node.Self, true);
                                    var afterNode = outPin.HostNode as IAfterExecNode;
                                    if (afterNode != null && afterNode.AfterExec.HostNode == afterNode)
                                        AddLink(afterNode.AfterExec, node.BeforeExec, true);
                                }
                            });
                    }
                }
            }
        }

        public override void UpdatePinLinkMenu()
        {
            ObjectMenus.SubMenuItems.Clear();
            ObjectMenus.Text = "Object";

            var type = PopMenuPressObject as Rtti.TtTypeDesc;
            if (type == null)
                return;
            var typeFullName = type.FullName;
            if (type.IsRefType)
            {
                typeFullName = type.FullName.Substring(0, type.FullName.Length - 1);
            }
            var classMeta = Rtti.TtClassMetaManager.Instance.GetMetaFromFullName(typeFullName);
            if (classMeta != null)
            {
                UpdateMenuWithClassMeta(classMeta, ObjectMenus);
                var editor = this.Editor as UMacrossEditor;
                if (editor != null && editor.IsGenShader == false)
                {
                    // only down cast here
                    for (int i = 0; i < classMeta.SubClasses.Count; i++)
                    {
                        var subClass = classMeta.SubClasses[i];
                        if (subClass != null)
                        {
                            var clsTypeName = subClass.ClassType.FullName;
                            ObjectMenus.AddMenuItem($"Cast to {clsTypeName}", clsTypeName, null,
                                (TtMenuItem item, object sender) =>
                                {
                                    var node = TypeConverterVar.NewTypeConverterVar(classMeta, subClass);
                                    node.UserData = MacrossEditor;
                                    node.Position = PopMenuPosition;
                                    SetDefaultActionForNode(node);
                                    this.AddNode(node);

                                    if (LinkingOp.StartPin != null && Rtti.TtTypeDesc.CanCast(LinkingOp.StartPin.GetType(), typeof(PinOut)))
                                    {
                                        var outPin = LinkingOp.StartPin as PinOut;
                                        AddLink(outPin, node.Left, true);
                                    }
                                });
                        }
                    }
                }
            }
        }

        public override void SetDefaultActionForNode(UNodeBase node)
        {
            node.OnLinkedToAction = NodeOnLinkedTo;
            node.OnLinkedFromAction = NodeOnLinkedFrom;
            node.OnLButtonClickedAction = NodeOnLButtonClicked;
            node.OnPreReadAction = NodeOnPreRead;
        }

        //Bricks.CodeBuilder.MacrossNode.MethodSelector mMethodSelector = new Bricks.CodeBuilder.MacrossNode.MethodSelector();
        MacrossSelector KlassSelector = new MacrossSelector();
        public override void OnAfterDrawMenu(UNodeGraphStyles styles)
        {
            //mMethodSelector.mSltMember = null;
            //mMethodSelector.mSltField = null;
            //mMethodSelector.mSltMethod = null;
            //mMethodSelector.OnDrawTree();
            //if (mMethodSelector.mSltMember != null)
            //{
            //    CurMenuType = EGraphMenu.None;
            //}
            //else if (mMethodSelector.mSltField != null)
            //{
            //    CurMenuType = EGraphMenu.None;
            //}
            //else if (mMethodSelector.mSltMethod != null)
            //{
            //    CurMenuType = EGraphMenu.None;
            //    var node = MethodNode.NewMethodNode(mMethodSelector.mSltMethod);
            //    node.UserData = MacrossEditor;
            //    node.Position = PopMenuPosition;
            //    SetDefaultActionForNode(node);
            //    this.AddNode(node);
            //}
        }
        public override void OnDrawAfter(Bricks.NodeGraph.TtGraphRenderer renderer, UNodeGraphStyles styles, ImDrawList cmdlist)
        {
            var mousePt = ImGuiAPI.GetMousePos() - ImGuiAPI.GetWindowPos();
            if (mousePt.X < 0 || mousePt.Y < 0)
                return;
            var winSize = ImGuiAPI.GetWindowSize();
            if (mousePt.X > winSize.X || mousePt.Y > winSize.Y)
                return;

            if (MacrossEditor != null && MacrossEditor.IsDraggingMember && MacrossEditor.DraggingMember != null)
            {
                MacrossEditor.DraggingMember.ParentGraph = this;
                var screenPt = this.ToScreenPos(mousePt.X, mousePt.Y);
                MacrossEditor.DraggingMember.Position = this.ViewportRateToCanvas(in screenPt);
                //MacrossEditor.DraggingMember.Position = this.View2WorldSpace(ref mousePt);
                //MacrossEditor.DraggingMember.OnDraw(styles);
                renderer.DrawNode(cmdlist, MacrossEditor.DraggingMember);

                if (ImGuiAPI.IsMouseDown(ImGuiMouseButton_.ImGuiMouseButton_Left) == false)
                {
                    SetDefaultActionForNode(MacrossEditor.DraggingMember);
                    this.AddNode(MacrossEditor.DraggingMember);
                    MacrossEditor.IsDraggingMember = false;
                    MacrossEditor.DraggingMember = null;
                }
            }

            if (MacrossEditor != null && MacrossEditor.IsDraggingLocalVar && MacrossEditor.DraggingLocalVar != null)
            {
                MacrossEditor.DraggingLocalVar.ParentGraph = this;
                var screenPt = this.ToScreenPos(mousePt.X, mousePt.Y);
                MacrossEditor.DraggingLocalVar.Position = this.ViewportRateToCanvas(in screenPt);
                //MacrossEditor.DraggingMember.Position = this.View2WorldSpace(ref mousePt);
                //MacrossEditor.DraggingMember.OnDraw(styles);
                renderer.DrawNode(cmdlist, MacrossEditor.DraggingLocalVar);

                if (ImGuiAPI.IsMouseDown(ImGuiMouseButton_.ImGuiMouseButton_Left) == false)
                {
                    SetDefaultActionForNode(MacrossEditor.DraggingLocalVar);
                    this.AddNode(MacrossEditor.DraggingLocalVar);
                    MacrossEditor.IsDraggingLocalVar = false;
                    MacrossEditor.DraggingLocalVar = null;
                }
            }

            //if (Function.IsFunctionDefineChanged)
            //{
            //    StartNode.UpdateMethodDefine();
            //    Function.IsFunctionDefineChanged = false;
            //}
        }
        public override void OnLButtonClicked()
        {
            //MacrossEditor.NodePropGrid.SingleTarget = null;
        }
        bool PopKlassSelector = false;
        public override bool OnLinkingUp(ULinkingLine linking, UNodeBase pressNode)
        {
            if (linking.StartPin == null)
            {
                return true;
            }
            var nodeExpr = linking.StartPin.HostNode;
            if (nodeExpr == null)
                return true;

            if (linking.StartPin != null && pressNode == null)
            {
                var oPin = linking.StartPin as PinOut;
                if (oPin != null)
                {
                    var type = nodeExpr.GetOutPinType(oPin);
                    if (type != null)
                    {
                        KlassSelector.KlsMeta = Rtti.TtClassMetaManager.Instance.GetMeta(type.TypeString);

                        PopKlassSelector = true;
                        LinkingOp.IsBlocking = true;
                        return false;
                    }
                }
            }
            return true;
        }
        public override unsafe void OnBeforeDrawMenu(UNodeGraphStyles styles)
        {
            base.OnBeforeDrawMenu(styles);
            if (PopKlassSelector)
            {
                var vPos = ImGuiAPI.GetMousePos();
                if (ImGuiAPI.BeginPopupContextWindow(null, ImGuiPopupFlags_.ImGuiPopupFlags_None))  
                {
                    KlassSelector.mSltMethod = null;
                    KlassSelector.mSltField = null;
                    KlassSelector.mSltMember = null;
                    KlassSelector.mSltSubClass = null;
                    KlassSelector.OnDraw(vPos);
                    if (KlassSelector.mSltSubClass != null)
                    {
                        var oPin = LinkingOp.StartPin as PinOut;
                        if (oPin != null)
                        {
                            var oNode = oPin.HostNode;
                            var type = oNode.GetOutPinType(oPin);
                            if (type != null)
                            {
                                var srcType = Rtti.TtClassMetaManager.Instance.GetMeta(Rtti.TtTypeDesc.TypeStr(type));
                                if (srcType != null)
                                {
                                    var node = TypeConverterVar.NewTypeConverterVar(srcType, KlassSelector.mSltSubClass);
                                    if (node != null)
                                    {
                                        node.UserData = MacrossEditor;
                                        node.Position = this.PopMenuPosition;
                                        SetDefaultActionForNode(node);
                                        this.AddNode(node);

                                        this.AddLink(oPin.HostNode, oPin.Name, node, node.Left.Name);
                                    }
                                }
                            }
                        }

                        LinkingOp.Reset();
                        PopKlassSelector = false;
                    }
                    else if (KlassSelector.mSltMember != null)
                    {
                        var node = ClassPropertyVar.NewClassProperty(KlassSelector.mSltMember, true);
                        node.UserData = MacrossEditor;
                        node.Position = PopMenuPosition;
                        SetDefaultActionForNode(node);
                        this.AddNode(node);

                        var oPin = LinkingOp.StartPin as PinOut;
                        if (oPin != null)
                        {
                            this.AddLink(oPin.HostNode, oPin.Name, node, node.Self.Name);
                        }

                        LinkingOp.Reset();
                        PopKlassSelector = false;
                    }
                    else if (KlassSelector.mSltField != null)
                    {
                        var node = ClassFieldVar.NewClassMemberVar(KlassSelector.mSltField, true);
                        node.UserData = MacrossEditor;
                        node.Position = PopMenuPosition;
                        SetDefaultActionForNode(node);
                        this.AddNode(node);

                        var oPin = LinkingOp.StartPin as PinOut;
                        if (oPin != null)
                        {
                            this.AddLink(oPin.HostNode, oPin.Name, node, node.Self.Name);
                        }

                        LinkingOp.Reset();
                        PopKlassSelector = false;
                    }
                    else if (KlassSelector.mSltMethod != null)
                    {
                        var node = MethodNode.NewMethodNode(KlassSelector.mSltMethod);
                        node.UserData = MacrossEditor;
                        node.Position = PopMenuPosition;
                        SetDefaultActionForNode(node);
                        this.AddNode(node);

                        var oPin = LinkingOp.StartPin as PinOut;
                        if (oPin != null)
                        {
                            if (KlassSelector.mSltMethod.IsStatic == false)
                            {
                                this.AddLink(oPin.HostNode, oPin.Name, node, node.Self.Name);
                            }
                        }
                        
                        LinkingOp.Reset();
                        PopKlassSelector = false;
                    }
                    ImGuiAPI.EndPopup();
                }
                else
                {
                    if(CurMenuType != EGraphMenu.Object)
                        LinkingOp.StartPin = null;
                    LinkingOp.HoverPin = null;
                    LinkingOp.IsBlocking = false;
                    PopKlassSelector = false;
                }
            }
            //else
            //{
            //    base.OnBeforeDrawMenu(styles);
            //}
        }
        private void NodeOnLinkedTo(UNodeBase node, PinOut oPin, UNodeBase InNode, PinIn iPin)
        {
            var funcGraph = ParentGraph as UMacrossMethodGraph;
            if (funcGraph == null || oPin.LinkDesc == null || iPin.LinkDesc == null)
            {
                return;
            }

            if (oPin.LinkDesc.CanLinks.Contains("Exec"))// || oPin.Link.CanLinks.Contains("Bool"))
            {
                funcGraph.RemoveLinkedOutExcept(oPin, InNode, iPin.Name);
                //funcGraph.AddLink(this, oPin.Name, InNode, iPin.Name, false);
            }
        }
        private void NodeOnLinkedFrom(UNodeBase node, PinIn iPin, UNodeBase OutNode, PinOut oPin)
        {
            var funcGraph = ParentGraph as UMacrossMethodGraph;
            if (funcGraph == null || oPin.LinkDesc == null || iPin.LinkDesc == null)
            {
                return;
            }
            if (iPin.LinkDesc.CanLinks.Contains("Value"))
            {
                funcGraph.RemoveLinkedInExcept(iPin, OutNode, oPin.Name);
                //funcGraph.AddLink(OutNode, oPin.Name, this, iPin.Name, false);
            }
        }
        private void NodeOnLButtonClicked(UNodeBase node, NodePin clickedPin)
        {
            //var editor = node.UserData as UMacrossEditor;
            //if (node.HasError)
            //{
            //    if (editor != null)
            //    {
            //        editor.PGMember.Target = node.CodeExcept;
            //    }
            //    return;
            //}
            //if (node.GetPropertyEditObject() == null)
            //{
            //    if (editor != null)
            //        editor.PGMember.Target = null;
            //    return;
            //}

            //if (editor != null)
            //    editor.PGMember.Target = node.GetPropertyEditObject();
        }
        public void UpdateSelectPG()
        {
            if (SelectedNodesDirty == false)
                return;
            if (MacrossEditor == null)
                return;
            SelectedNodesDirty = false;
            var list = new List<object>(SelectedNodes.Count);
            for(int i=0; i<SelectedNodes.Count; i++)
            {
                var node = SelectedNodes[i].Node;
                if (node.HasError)
                    list.Add(node.CodeExcept);
                var obj = node.GetPropertyEditObject();
                list.Add(obj);
            }
            MacrossEditor.PGMember.Target = list;
        }
        private void NodeOnPreRead(UNodeBase node, object tagObject, object hostObject, bool fromXml)
        {
            var klsGraph = tagObject as UMacrossEditor;
            if (klsGraph == null)
                return;

            node.UserData = klsGraph;
        }

        [Browsable(false)]
        public bool IsPropertyVisibleDirty { get; set; } = false;
        public void GetProperties(ref CustomPropertyDescriptorCollection collection, bool parentIsValueType)
        {
            var pros = TypeDescriptor.GetProperties(this);
            var thisType = Rtti.TtTypeDesc.TypeOf(this.GetType());
            foreach (PropertyDescriptor prop in pros)
            {
                var proDesc = EGui.Controls.PropertyGrid.PropertyCollection.PropertyDescPool.QueryObjectSync();
                proDesc.InitValue(this, thisType, prop, parentIsValueType);
                if (!proDesc.IsBrowsable)
                    continue;
                switch (proDesc.Name)
                {
                    case "GraphName":
                        break;
                    case "Inputs":
                    case "Outputs":
                        {
                            if (MethodDatas.Count > 0 && !MethodDatas[0].MethodDec.IsOverride)
                                collection.Add(proDesc);
                        }
                        break;
                    default:
                        collection.Add(proDesc);
                        break;
                }
            }
            IsPropertyVisibleDirty = false;
        }

        public object GetPropertyValue(string propertyName)
        {
            return PropertyCustomizationHelper<UMacrossMethodGraph>.GetPropertyValue(this, propertyName);
        }

        public void SetPropertyValue(string propertyName, object value)
        {
            PropertyCustomizationHelper<UMacrossMethodGraph>.SetPropertyValue(this, propertyName, value);
        }

        public override void SetConfigUnionNode(IUnionNode node)
        {
            this.MacrossEditor.SetConfigUnionNode(node);
        }
    }
}

