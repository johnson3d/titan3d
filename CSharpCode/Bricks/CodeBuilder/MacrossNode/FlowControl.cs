using System;
using System.Collections.Generic;
using System.ComponentModel;
using EngineNS.Bricks.NodeGraph;
using EngineNS.EGui.Controls;

namespace EngineNS.Bricks.CodeBuilder.MacrossNode
{
    [ContextMenu("Sequence", "FlowControl\\Sequence", TtMacross.MacrossEditorKeyword, ShaderNode.UMaterialGraph.MaterialEditorKeyword)]
    public partial class SequenceNode : UNodeBase, IBeforeExecNode, IBreakableNode
    {
        [Rtti.Meta]
        public int SequenceCount 
        {
            get => Outputs.Count;
            set
            {
                int nSaveCount = Outputs.Count;
                if (nSaveCount == value)
                {
                    return;
                }
                else if (nSaveCount < value)
                {
                    for (int i = 0; i < value - nSaveCount; i++)
                    {
                        AddSequencePin();
                    }
                }
                else
                {
                    for (int i = 0; i < nSaveCount - value; i++)
                    {
                        RemoveSequecePin();
                    }
                }
                OnPositionChanged();
            }
        }
        [Browsable(false)]
        public PinIn BeforeExec { get; set; } = new PinIn();

        public SequenceNode()
        {
            Name = "Sequence";
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF0fEF30;
            TitleColor = MacrossStyles.Instance.FlowControlTitleColor;
            BackColor = MacrossStyles.Instance.BGColor;

            FirstPin.Name = "Pin0";
            FirstPin.LinkDesc = MacrossStyles.Instance.NewExecPinDesc();

            BeforeExec.Name = " >>";
            BeforeExec.LinkDesc = MacrossStyles.Instance.NewExecPinDesc();
            AddPinIn(BeforeExec);
            AddPinOut(FirstPin);
        }
        [Browsable(false)]
        public PinOut FirstPin { get; set; } = new PinOut();
        [Browsable(false)]
        public string BreakerName => "breaker_sequence_" + (uint)NodeId.GetHashCode();

        EBreakerState mBreakerState = EBreakerState.Hidden;
        [Browsable(false)]
        public EBreakerState BreakerState
        {
            get => mBreakerState;
            set
            {
                mBreakerState = value;
                Macross.TtMacrossDebugger.Instance.SetBreakEnable(BreakerName, (value == EBreakerState.Enable));
            }
        }

        public List<PinOut> Sequences = new List<PinOut>();

        void AddSequencePin()
        {
            var aPin = new PinOut();
            aPin.Name = $"Pin{Sequences.Count + 1}";
            aPin.LinkDesc = MacrossStyles.Instance.NewExecPinDesc();
            Sequences.Add(aPin);
            AddPinOut(aPin);
        }
        void RemoveSequecePin()
        {
            var t = Sequences[Sequences.Count - 1];
            Sequences.RemoveAt(Sequences.Count - 1);
            RemovePinOut(t);
        }
        public override void OnShowPinMenu(NodePin pin)
        {
            if(ImGuiAPI.MenuItem("AddPin", null, false, true))
            {
                AddSequencePin();
                SequenceCount = Sequences.Count;
                OnPositionChanged();
            }
            if(pin != FirstPin && pin != BeforeExec)
            {
                if (ImGuiAPI.MenuItem($"DeletePin", null, false, true))
                {
                    var addedPin = pin as PinOut;
                    if (Sequences.Contains(addedPin))
                    {
                        ParentGraph.RemoveLinkedOut(addedPin);
                        RemovePinOut(addedPin);
                        Sequences.Remove(addedPin);

                        //ParentGraph.mMenuShowPin = null;
                        //ParentGraph.mMenuType = EGui.Controls.NodeGraph.NodeGraph.EMenuType.None;

                        for (int i = 0; i < Sequences.Count; i++)
                        {
                            Sequences[i].Name = $"Pin{i}";
                        }
                    }
                }
            }
        }
        public override void BuildStatements(NodePin pin, ref BuildCodeStatementsData data)
        {
            var firstLinker = data.NodeGraph.GetFirstLinker(FirstPin);
            if (firstLinker != null)
                firstLinker.InPin.HostNode.BuildStatements(firstLinker.InPin, ref data);
            for(int i=0; i<Sequences.Count; i++)
            {
                var linker = data.NodeGraph.GetFirstLinker(Sequences[i]);
                if (linker != null)
                    linker.InPin.HostNode.BuildStatements(linker.InPin, ref data);
            }
        }

        public void LightDebuggerLine()
        {
            var linker = ParentGraph.GetFirstLinker(BeforeExec);
            if (linker != null)
            {
                linker.InDebuggerLine = true;
                var node = ParentGraph.GetOppositePinNode(BeforeExec) as IBeforeExecNode;
                if (node != null)
                    node.LightDebuggerLine();
            }
        }

        public void UnLightDebuggerLine()
        {
            var linker = ParentGraph.GetFirstLinker(BeforeExec);
            if (linker != null)
            {
                linker.InDebuggerLine = false;
                var node = ParentGraph.GetOppositePinNode(BeforeExec) as IBeforeExecNode;
                if (node != null)
                    node.UnLightDebuggerLine();
            }
        }

        public void AddMenuItems(TtMenuItem parentItem)
        {
            parentItem.AddMenuSeparator("BREAKPOINTS");
            parentItem.AddMenuItem("Add Breakpoint", null,
                (TtMenuItem item, object sender) =>
                {
                    BreakerState = EBreakerState.Enable;
                });
            parentItem.AddMenuItem("Remove Breakpoint", null,
                (TtMenuItem item, object sender) =>
                {
                    BreakerState = EBreakerState.Hidden;
                });
        }
    }
    [ContextMenu("if", "FlowControl\\If", TtMacross.MacrossEditorKeyword, ShaderNode.UMaterialGraph.MaterialEditorKeyword)]
    public partial class IfNode : UNodeBase, IBeforeExecNode, IAfterExecNode, IBreakableNode
    {
        [Browsable(false)]
        public PinIn BeforeExec { get; set; } = new PinIn();
        [Browsable(false)]
        public PinOut AfterExec { get; set; } = new PinOut();
        [Browsable(false)]
        public string BreakerName => "breaker_if_" + (uint)NodeId.GetHashCode();
        EBreakerState mBreakerState = EBreakerState.Hidden;
        [Browsable(false)]
        public EBreakerState BreakerState
        {
            get => mBreakerState;
            set
            {
                mBreakerState = value;
                Macross.TtMacrossDebugger.Instance.SetBreakEnable(BreakerName, (value == EBreakerState.Enable));
            }
        }
        public void AddMenuItems(TtMenuItem parentItem)
        {
            parentItem.AddMenuSeparator("BREAKPOINTS");
            parentItem.AddMenuItem("Add Breakpoint", null,
                (TtMenuItem item, object sender) =>
                {
                    BreakerState = EBreakerState.Enable;
                });
            parentItem.AddMenuItem("Remove Breakpoint", null,
                (TtMenuItem item, object sender) =>
                {
                    BreakerState = EBreakerState.Hidden;
                });
        }

        int mConditionCount = 1;
        [Browsable(false)]
        [Rtti.Meta]
        public int ConditionCount
        {
            get => mConditionCount;
            set
            {
                mConditionCount = value;
                for(int i=1; i<mConditionCount; i++)
                    AddConditionResultPair();

                OnPositionChanged();
            }
        }

        public IfNode()
        {
            Name = "If";
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF0fEF30;
            TitleColor = MacrossStyles.Instance.FlowControlTitleColor;
            BackColor = MacrossStyles.Instance.BGColor;

            BeforeExec.Name = " >>";
            AfterExec.Name = ">> ";
            BeforeExec.LinkDesc = MacrossStyles.Instance.NewExecPinDesc();
            AfterExec.LinkDesc = MacrossStyles.Instance.NewExecPinDesc();
            AddPinIn(BeforeExec);
            AddPinOut(AfterExec);

            //ConditionPin.Name = "bool";
            //ConditionPin.Link = MacrossStyles.Instance.NewInOutPinDesc();
            //ConditionPin.Link.CanLinks.Clear();
            //ConditionPin.Link.CanLinks.Add("Value");

            //TruePin.Name = "True";
            //TruePin.Link = MacrossStyles.Instance.NewExecPinDesc();
            AddConditionResultPair();

            FalsePin.Name = "False";
            FalsePin.LinkDesc = MacrossStyles.Instance.NewExecPinDesc();

            //AddPinIn(ConditionPin);
            //AddPinOut(TruePin);
            AddPinOut(FalsePin);
        }
        void AddConditionResultPair()
        {
            RemovePinOut(FalsePin);

            var pinIn = AddPinIn(new PinIn()
            {
                Name = "Condition" + ConditionResultPairs.Count,
            });
            var pinOut = AddPinOut(new PinOut()
            {
                Name = "True" + ConditionResultPairs.Count,
            });
            pinOut.LinkDesc = MacrossStyles.Instance.NewExecPinDesc();
            ConditionResultPairs.Add(new KeyValuePair<PinIn, PinOut>(pinIn, pinOut));

            AddPinOut(FalsePin);
        }
        //public PinIn ConditionPin { get; set; } = new PinIn();
        //public PinOut TruePin { get; set; } = new PinOut();
        public List<KeyValuePair<PinIn, PinOut>> ConditionResultPairs = new List<KeyValuePair<PinIn, PinOut>>();
        [Browsable(false)]
        public PinOut FalsePin { get; set; } = new PinOut();
        public override bool CanLinkFrom(PinIn iPin, UNodeBase OutNode, PinOut oPin)
        {
            if (base.CanLinkFrom(iPin, OutNode, oPin) == false)
                return false;

            for(int i=0; i<ConditionResultPairs.Count; i++)
            {
                if (iPin == ConditionResultPairs[i].Key)
                {
                    var nodeExpr = OutNode as UNodeBase;
                    var type = nodeExpr.GetOutPinType(oPin);
                    if (type == null)
                        return false;
                    return type.IsEqual(typeof(bool));
                }
            }
            return true;
        }

        public override void OnShowPinMenu(NodePin pin)
        {
            if(ImGuiAPI.MenuItem("AddCondition", null, false, true))
            {
                AddConditionResultPair();
                mConditionCount = ConditionResultPairs.Count;
                OnPositionChanged();
            }
            if(pin != FalsePin && pin != BeforeExec && pin != AfterExec && pin != ConditionResultPairs[0].Key)
            {
                if(ImGuiAPI.MenuItem("DeletePin", null, false, true))
                {
                    for(int i=1; i<ConditionResultPairs.Count; i++)
                    {
                        if(pin == ConditionResultPairs[i].Key)
                        {
                            ParentGraph.RemoveLinkedOut(ConditionResultPairs[i].Value);
                            ParentGraph.RemoveLinkedIn(ConditionResultPairs[i].Key);
                            RemovePinIn(ConditionResultPairs[i].Key);
                            RemovePinOut(ConditionResultPairs[i].Value);
                            ConditionResultPairs.RemoveAt(i);
                            break;
                        }
                    }
                }
            }
        }

        public override void OnMouseStayPin(NodePin stayPin, UNodeGraph graph)
        {
            for (int i = 0; i < ConditionResultPairs.Count; i++)
            {
                if(stayPin == ConditionResultPairs[i].Key)
                {
                    EGui.Controls.CtrlUtility.DrawHelper("bool " + GetRuntimeValueString(stayPin.Name + "_" + (uint)NodeId.GetHashCode()));
                }
            }
        }

        public override Rtti.TtTypeDesc GetInPinType(PinIn pin)
        {
            for(int i=0; i< ConditionResultPairs.Count; i++)
            {
                if (pin == ConditionResultPairs[i].Key)
                    return Rtti.TtTypeDesc.TypeOf(typeof(bool));
            }
            return null;
        }

        //public override IExpression GetExpr(UMacrossMethodGraph funGraph, ICodeGen cGen, bool bTakeResult)
        //{
        //    var ifOp = new IfOp();
        //    var links = new List<UPinLinker>();
        //    funGraph.FindInLinker(ConditionPin, links);
        //    if (links.Count != 1)
        //    {
        //        throw new GraphException(this, ConditionPin, $"Condition link error : {links.Count}");
        //    }
        //    var condiNode = links[0].OutNode as UNodeExpr;
        //    var condiExpr = condiNode.GetExpr(funGraph, cGen, true) as OpExpress;
        //    {
        //        var condiType = condiExpr.GetType();
        //        bool checkOk = false;
        //        var cmpOp = condiExpr as BinocularOp;
        //        if (cmpOp != null && cmpOp.IsCmpOp())
        //        {
        //            checkOk = true;
        //        }
        //        if (condiType.IsSubclassOf(typeof(BoolOp)))
        //        {
        //            checkOk = true;
        //        }
        //        if(checkOk == false)
        //        {
        //            throw new GraphException(this, ConditionPin, $"Condition must be bool expression");
        //        }
        //    }
        //    ifOp.Condition = condiExpr;

        //    links.Clear();
        //    funGraph.FindOutLinker(TruePin, links);
        //    if (links.Count == 1)
        //    {
        //        var trueNode = links[0].InNode as UNodeExpr;
        //        var sequence = new ExecuteSequence();
        //        var trueExpr = trueNode.GetExpr(funGraph, cGen, false);
        //        if (trueExpr == null)
        //        {
        //            throw new GraphException(this, TruePin, $"True expression must be ExecuteSequence");
        //        }
        //        else
        //        {
        //            sequence.PushExpr(trueExpr);
        //        }
        //        ifOp.TrueExpr = sequence;
        //    }

        //    links.Clear();
        //    funGraph.FindOutLinker(FalsePin, links);
        //    if (links.Count == 1)
        //    {
        //        var falseNode = links[0].InNode as UNodeExpr;
        //        var sequence = new ExecuteSequence();                
        //        var falseExpr = falseNode.GetExpr(funGraph, cGen, false);
        //        if (falseExpr == null)
        //        {
        //            throw new GraphException(this, FalsePin, $"False expression must be ExecuteSequence");
        //        }
        //        else
        //        {
        //            sequence.PushExpr(falseExpr);
        //        }
        //        ifOp.ElseExpr = sequence;
        //    }

        //    ifOp.NextExpr = this.GetNextExpr(funGraph, cGen);
        //    return ifOp;
        //}
        public override void BuildStatements(NodePin pin, ref BuildCodeStatementsData data)
        {
            var nodeId = (uint)NodeId.GetHashCode();
            var ifStatement = new TtIfStatement();
            for(int i=0; i<ConditionResultPairs.Count; i++)
            {
                TtExpressionBase condition;
                if (data.NodeGraph.PinHasLinker(ConditionResultPairs[i].Key))
                    condition = data.NodeGraph.GetOppositePinExpression(ConditionResultPairs[i].Key, ref data);
                else
                    condition = new TtPrimitiveExpression(true);
                var opPin = data.NodeGraph.GetOppositePin(ConditionResultPairs[i].Value);
                var node = data.NodeGraph.GetOppositePinNode(ConditionResultPairs[i].Value);
                var trueStatement = new TtExecuteSequenceStatement();
                if(node != null)
                {
                    var trueData = new BuildCodeStatementsData();
                    data.CopyTo(ref trueData);
                    trueData.CurrentStatements = trueStatement.Sequence;
                    node.BuildStatements(opPin, ref trueData);
                }
                if(i == 0)
                {
                    ifStatement.Condition = condition;
                    ifStatement.TrueStatement = trueStatement;
                }
                else
                {
                    var elseIfStatement = new TtIfStatement()
                    {
                        Condition = condition,
                        TrueStatement = trueStatement,
                    };
                    ifStatement.ElseIfs.Add(elseIfStatement);
                }

                data.CurrentStatements.Add(new TtDebuggerSetWatchVariable()
                {
                    VariableType = new TtTypeReference(typeof(bool)),
                    VariableName = ConditionResultPairs[i].Key.Name + "_" + nodeId,
                    VariableValue = condition,
                });
            }

            AddDebugBreakerStatement(BreakerName, ref data);

            data.CurrentStatements.Add(ifStatement);
            var falseStatement = new TtExecuteSequenceStatement();
            ifStatement.FalseStatement = falseStatement;
            var falseOpPin = data.NodeGraph.GetOppositePin(FalsePin);
            var falseNode = data.NodeGraph.GetOppositePinNode(FalsePin);
            if(falseNode != null)
            {
                var falseData = new BuildCodeStatementsData();
                data.CopyTo(ref falseData);
                falseData.CurrentStatements = falseStatement.Sequence;
                falseNode.BuildStatements(falseOpPin, ref falseData);
            }

            var nextOpPin = data.NodeGraph.GetOppositePin(AfterExec);
            var nextNode = data.NodeGraph.GetOppositePinNode(AfterExec);
            if (nextNode != null)
                nextNode.BuildStatements(nextOpPin, ref data);
        }

        public void LightDebuggerLine()
        {
            var linker = ParentGraph.GetFirstLinker(BeforeExec);
            if (linker != null)
            {
                linker.InDebuggerLine = true;
                var node = ParentGraph.GetOppositePinNode(BeforeExec) as IBeforeExecNode;
                if (node != null)
                    node.LightDebuggerLine();
            }
        }

        public void UnLightDebuggerLine()
        {
            var linker = ParentGraph.GetFirstLinker(BeforeExec);
            if (linker != null)
            {
                linker.InDebuggerLine = false;
                var node = ParentGraph.GetOppositePinNode(BeforeExec) as IBeforeExecNode;
                if (node != null)
                    node.UnLightDebuggerLine();
            }
        }
    }
    [ContextMenu("return", "FlowControl\\Return", TtMacross.MacrossEditorKeyword, ShaderNode.UMaterialGraph.MaterialEditorKeyword)]
    public partial class ReturnNode : UNodeBase, IBeforeExecNode, IBreakableNode
    {
        [Browsable(false)]
        public PinIn BeforeExec { get; set; } = new PinIn();
        [Browsable(false)]
        public string BreakerName => "breaker_return_" + (uint)NodeId.GetHashCode();
        EBreakerState mBreakerState = EBreakerState.Hidden;
        [Browsable(false)]
        public EBreakerState BreakerState
        {
            get => mBreakerState;
            set
            {
                mBreakerState = value;
                Macross.TtMacrossDebugger.Instance.SetBreakEnable(BreakerName, (value == EBreakerState.Enable));
            }
        }
        public void AddMenuItems(TtMenuItem parentItem)
        {
            parentItem.AddMenuSeparator("BREAKPOINTS");
            parentItem.AddMenuItem("Add Breakpoint", null,
                (TtMenuItem item, object sender) =>
                {
                    BreakerState = EBreakerState.Enable;
                });
            parentItem.AddMenuItem("Remove Breakpoint", null,
                (TtMenuItem item, object sender) =>
                {
                    BreakerState = EBreakerState.Hidden;
                });
        }

        public static ReturnNode NewReturnNode(UMacrossMethodGraph funGraph)
        {
            var result = new ReturnNode();
            result.Initialize(funGraph);
            return result;
        }
        public ReturnNode()
        {
            Name = "Return";
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF0fEF30;
            TitleColor = MacrossStyles.Instance.FlowControlTitleColor;
            BackColor = MacrossStyles.Instance.BGColor;

            BeforeExec.Name = " >>";
            BeforeExec.LinkDesc = MacrossStyles.Instance.NewExecPinDesc();
            AddPinIn(BeforeExec);
        }
        public void Initialize(UMacrossMethodGraph methodGraph)
        {
            if (methodGraph.MethodDatas.Count != 1)
                return;
            // 图中只有一个函数时才能有返回值
            var data = methodGraph.MethodDatas[0];
            UpdateMethodDefine(data.MethodDec);
            //ReturnType = methodGraph.Function.ReturnType;
            //var retType = Rtti.UTypeDescManager.Instance.GetTypeDescFromFullName(methodGraph.Function.ReturnType);
            //if (retType != null)
            //{
            //    if (retType.SystemType == typeof(void))
            //        return;
            //}
            //if (ReturnValuePin != null)
            //{
            //    RemovePinIn(ReturnValuePin);
            //}

            //ReturnValuePin = new PinIn();

            //ReturnValuePin.Name = "Result";
            //ReturnValuePin.Link = MacrossStyles.Instance.NewInOutPinDesc();
            //ReturnValuePin.Link.CanLinks.Add("Value");

            //AddPinIn(ReturnValuePin);
        }
        public List<PinIn> Arguments = new List<PinIn>();
        List<PinIn> mTemplateArguments = new List<PinIn>();
        public void UpdateMethodDefine(TtMethodDeclaration methodDec)
        {
            mTemplateArguments.Clear();
            if (methodDec.ReturnValue != null)
            {
                PinIn retPin = null;
                if(Arguments.Count > 0)
                {
                    var defType = Arguments[0].Tag as TtTypeReference;
                    if(Inputs[0].Name == methodDec.ReturnValue.VariableName &&
                       defType == methodDec.ReturnValue.VariableType)
                    {
                        retPin = Arguments[0];
                        Arguments.Remove(retPin);
                        Inputs.Remove(retPin);
                    }
                }
                if(retPin == null)
                {
                    retPin = new PinIn()
                    {
                        Name = methodDec.ReturnValue.VariableName,
                        Tag = methodDec.ReturnValue.VariableType,
                    };
                }
                mTemplateArguments.Add(retPin);
            }

            for (int i = 0; i < methodDec.Arguments.Count; i++)
            {
                var argDec = methodDec.Arguments[i];
                if (argDec.OperationType == EMethodArgumentAttribute.Ref ||
                   argDec.OperationType == EMethodArgumentAttribute.Out)
                {
                    PinIn argPin = null;
                    for(int argIdx= Arguments.Count - 1; argIdx >= 0; argIdx--)
                    {
                        var defType = Arguments[argIdx].Tag as TtTypeReference;
                        if(Arguments[argIdx].Name == argDec.VariableName && defType == argDec.VariableType)
                        {
                            argPin = Arguments[argIdx];
                            Arguments.RemoveAt(argIdx);
                            Inputs.Remove(argPin);
                            break;
                        }
                    }
                    if(argPin == null)
                    {
                        argPin = new PinIn()
                        {
                            Name = argDec.VariableName,
                            Tag = argDec.VariableType,
                        };
                    }
                    mTemplateArguments.Add(argPin);
                }
            }

            var methodGraph = ParentGraph as UMacrossMethodGraph;
            if(methodGraph != null)
            {
                foreach (var i in Arguments)
                {
                    methodGraph.RemoveLinkedIn(i);
                    RemovePinIn(i);
                }
                Arguments.Clear();
            }

            for(int i=0; i<mTemplateArguments.Count; i++)
            {
                Arguments.Add(mTemplateArguments[i]);
                AddPinIn(mTemplateArguments[i]);
            }

            OnPositionChanged();
        }
        public override void OnPreRead(object tagObject, object hostObject, bool fromXml)
        {
            base.OnPreRead(tagObject, hostObject, fromXml);

            var funGraph = this.ParentGraph as UMacrossMethodGraph;
            if (funGraph == null)
                return;
            Initialize(funGraph);
        }
        //[Rtti.Meta]
        //public string ReturnType
        //{
        //    get;
        //    set;
        //}
        //public PinIn ReturnValuePin { get; set; } = null;
        public override void OnMouseStayPin(NodePin stayPin, UNodeGraph graph)
        {
            //if (ReturnValuePin != stayPin)
            //    return;

            //EGui.Controls.CtrlUtility.DrawHelper(ReturnType);
            for(int i=0; i<Inputs.Count; i++)
            {
                if (Inputs[i] == BeforeExec)
                    continue;

                var valueString = GetRuntimeValueString(Inputs[i].Name + "_" + (uint)NodeId.GetHashCode());
                var typeString = (Inputs[i].Tag as TtTypeReference).TypeFullName;
                EGui.Controls.CtrlUtility.DrawHelper($"{valueString}({typeString})");
            }
        }
        public override bool CanLinkFrom(PinIn iPin, UNodeBase OutNode, PinOut oPin)
        {
            if (base.CanLinkFrom(iPin, OutNode, oPin) == false)
                return false;

            if (iPin == BeforeExec)
                return true;

            var funGraph = this.ParentGraph as UMacrossMethodGraph;
            if (funGraph == null)
                return false;

            var nodeExpr = OutNode as UNodeBase;
            if (nodeExpr == null)
                return false;

            //var retType = Rtti.UTypeDescManager.Instance.GetTypeDescFromFullName(funGraph.Function.ReturnType);
            //if (retType != null)
            //{
            //    if (retType.SystemType == typeof(void))
            //        return false;

            //    var type = nodeExpr.GetOutPinType(oPin);
            //    if (ICodeGen.CanConvert(type, retType.SystemType))
            //    {
            //        return true;
            //    }
            //}
            //return false;
            return true;
        }
        public override void BuildStatements(NodePin pin, ref BuildCodeStatementsData data)
        {
            for(int i=0; i<Inputs.Count; i++)
            {
                if (Inputs[i] == BeforeExec)
                    continue;

                var exp = data.NodeGraph.GetOppositePinExpression(Inputs[i], ref data);
                if(exp == null)
                {
                    var typeRef = Inputs[i].Tag as TtTypeReference;
                    if(typeRef != null)
                        exp = new TtDefaultValueExpression(typeRef);
                }
                var st = new TtAssignOperatorStatement()
                {
                    From = exp,
                    To = new TtVariableReferenceExpression(Inputs[i].Name)
                };
                data.CurrentStatements.Add(st);

                data.CurrentStatements.Add(new TtDebuggerSetWatchVariable()
                {
                    VariableType = Inputs[i].Tag as TtTypeReference,
                    VariableName = Inputs[i].Name + "_" + (uint)NodeId.GetHashCode(),
                    VariableValue = new TtVariableReferenceExpression(Inputs[i].Name)
                });
            }
            AddDebugBreakerStatement(BreakerName, ref data);
            data.CurrentStatements.Add(new TtReturnStatement());
        }
        //public override IExpression GetExpr(UMacrossMethodGraph funGraph, ICodeGen cGen, bool bTakeResult)
        //{
        //    var retType = Rtti.UTypeDescManager.Instance.GetTypeDescFromFullName(funGraph.Function.ReturnType);
        //    if (retType == null)
        //        throw new GraphException(this, ReturnValuePin, "ReturnType is null");

        //    var expr = new ReturnOp();
        //    if (retType.SystemType != typeof(void))
        //    {
        //        var links = new List<UPinLinker>();
        //        funGraph.FindInLinker(ReturnValuePin, links);
        //        if (links.Count != 1)
        //            throw new GraphException(this, ReturnValuePin, "Please link Result pin");
        //        var returnValueNode = links[0].OutNode as UNodeExpr;
        //        var resultExpr = returnValueNode.GetExpr(funGraph, cGen, true) as OpExpress;
        //        var resultType = returnValueNode.GetOutPinType(links[0].OutPin);
        //        if (resultType == retType.SystemType)
        //        {
        //            expr.ReturnExpr = resultExpr;
        //        }
        //        else
        //        {
        //            var cvtExpr = new ConvertTypeOp();
        //            cvtExpr.TargetType = cGen.GetTypeString(retType.SystemType);
        //            cvtExpr.ObjExpr = resultExpr;
        //            if (retType.SystemType.IsValueType == false && retType.SystemType.IsEnum == false)
        //                cvtExpr.UseAs = true;
        //            else
        //                cvtExpr.UseAs = false;
        //            expr.ReturnExpr = cvtExpr;
        //        }
        //        if (expr.ReturnExpr == null)
        //            throw new GraphException(this, ReturnValuePin, "Result must link a OpExpression");
        //    }

        //    return expr;
        //}

        public void LightDebuggerLine()
        {
            var linker = ParentGraph.GetFirstLinker(BeforeExec);
            if (linker != null)
            {
                linker.InDebuggerLine = true;
                var node = ParentGraph.GetOppositePinNode(BeforeExec) as IBeforeExecNode;
                if (node != null)
                    node.LightDebuggerLine();
            }
        }

        public void UnLightDebuggerLine()
        {
            var linker = ParentGraph.GetFirstLinker(BeforeExec);
            if (linker != null)
            {
                linker.InDebuggerLine = false;
                var node = ParentGraph.GetOppositePinNode(BeforeExec) as IBeforeExecNode;
                if (node != null)
                    node.UnLightDebuggerLine();
            }
        }
        public override object GetPropertyEditObject()
        {
            return ParentGraph;
        }
    }
    [ContextMenu("forloop", "FlowControl\\For", TtMacross.MacrossEditorKeyword, ShaderNode.UMaterialGraph.MaterialEditorKeyword)]
    public partial class ForLoopNode : UNodeBase, IBeforeExecNode, IAfterExecNode, IBreakableNode
    {
        [Browsable(false)]
        public PinIn BeginIdxPin;
        [Browsable(false)]
        public PinIn EndIdxPin;
        [Browsable(false)]
        public PinIn StepPin;
        [Browsable(false)]
        public PinOut IndexPin;
        [Browsable(false)]
        public PinOut LoopBodyPin;

        [Rtti.Meta]
        public Int64 BeginIdx { get; set; } = 0;
        [Rtti.Meta]
        public Int64 EndIdx { get; set; } = 1;
        [Rtti.Meta]
        public Int64 StepIdx { get; set; } = 1;

        static string mLoopIdxName = "loopIndex";
        [Browsable(false)]
        public PinIn BeforeExec { get; set; } = new PinIn();
        [Browsable(false)]
        public PinOut AfterExec { get; set; } = new PinOut();
        [Browsable(false)]
        public string BreakerName => "breaker_for_" + (uint)NodeId.GetHashCode();

        EBreakerState mBreakerState = EBreakerState.Hidden;
        [Browsable(false)]
        public EBreakerState BreakerState
        {
            get => mBreakerState;
            set
            {
                mBreakerState = value;
                Macross.TtMacrossDebugger.Instance.SetBreakEnable(BreakerName, (value == EBreakerState.Enable));
            }
        }
        public void AddMenuItems(TtMenuItem parentItem)
        {
            parentItem.AddMenuSeparator("BREAKPOINTS");
            parentItem.AddMenuItem("Add Breakpoint", null,
                (TtMenuItem item, object sender) =>
                {
                    BreakerState = EBreakerState.Enable;
                });
            parentItem.AddMenuItem("Remove Breakpoint", null,
                (TtMenuItem item, object sender) =>
                {
                    BreakerState = EBreakerState.Hidden;
                });
        }
        public ForLoopNode()
        {
            BeginIdx = 0;
            EndIdx = 100;
            StepIdx = 1;

            Name = "For";
            TitleColor = MacrossStyles.Instance.FlowControlTitleColor;
            BackColor = MacrossStyles.Instance.BGColor;

            BeforeExec.Name = " >>";
            AfterExec.Name = ">> ";
            BeforeExec.LinkDesc = MacrossStyles.Instance.NewExecPinDesc();
            AfterExec.LinkDesc = MacrossStyles.Instance.NewExecPinDesc();
            AddPinIn(BeforeExec);
            AddPinOut(AfterExec);
            BeginIdxPin = AddPinIn(new PinIn() { Name = "BeginIdx" });
            EndIdxPin = AddPinIn(new PinIn() { Name = "EndIdx" });
            StepPin = AddPinIn(new PinIn() { Name = "Step" });

            IndexPin = AddPinOut(new PinOut() { Name = "Index" });
            LoopBodyPin = AddPinOut(new PinOut() { Name = "LoopBody" });
            LoopBodyPin.LinkDesc = MacrossStyles.Instance.NewExecPinDesc();
        }

        public override object GetPropertyEditObject()
        {
            return this;
        }

        public override void OnMouseStayPin(NodePin stayPin, UNodeGraph graph)
        {
            var nodeId = (uint)NodeId.GetHashCode();
            if(stayPin == BeginIdxPin)
                EGui.Controls.CtrlUtility.DrawHelper(GetRuntimeValueString(stayPin.Name + "_" + nodeId));
            else if(stayPin == EndIdxPin)
                EGui.Controls.CtrlUtility.DrawHelper(GetRuntimeValueString(stayPin.Name + "_" + nodeId));
            else if(stayPin == StepPin)
                EGui.Controls.CtrlUtility.DrawHelper(GetRuntimeValueString(stayPin.Name + "_" + nodeId));
            else if(stayPin == IndexPin)
                EGui.Controls.CtrlUtility.DrawHelper(GetRuntimeValueString(mLoopIdxName + "_" + nodeId));
        }

        public override void BuildStatements(NodePin pin, ref BuildCodeStatementsData data)
        {
            var nodeId = (uint)NodeId.GetHashCode();
            var forStatement = new TtForLoopStatement()
            {
                LoopIndexName = mLoopIdxName,
            };
            var beginIdxExp = data.NodeGraph.GetOppositePinExpression(BeginIdxPin, ref data);
            if (beginIdxExp != null)
                forStatement.BeginExpression = beginIdxExp;
            else
                forStatement.BeginExpression = new TtPrimitiveExpression(BeginIdx);
            data.CurrentStatements.Add(new TtDebuggerSetWatchVariable()
            {
                VariableType = new TtTypeReference(typeof(UInt64)),
                VariableName = BeginIdxPin.Name + "_" + nodeId,
                VariableValue = forStatement.BeginExpression,
            });

            var endIdxExp = data.NodeGraph.GetOppositePinExpression(EndIdxPin, ref data);
            if (endIdxExp != null)
                forStatement.EndExpression = endIdxExp;
            else
                forStatement.EndExpression = new TtPrimitiveExpression(EndIdx);
            data.CurrentStatements.Add(new TtDebuggerSetWatchVariable()
            {
                VariableType = new TtTypeReference(typeof(UInt64)),
                VariableName = EndIdxPin.Name + "_" + nodeId,
                VariableValue = forStatement.EndExpression,
            });

            var stepExp = data.NodeGraph.GetOppositePinExpression(StepPin, ref data);
            if (stepExp != null)
                forStatement.StepExpression = stepExp;
            else
                forStatement.StepExpression = new TtPrimitiveExpression(StepIdx);
            data.CurrentStatements.Add(new TtDebuggerSetWatchVariable()
            {
                VariableType = new TtTypeReference(typeof(UInt64)),
                VariableName = StepPin.Name + "_" + nodeId,
                VariableValue = forStatement.StepExpression,
            });

            AddDebugBreakerStatement(BreakerName, ref data);
            data.CurrentStatements.Add(forStatement);
            var bodyNodePin = data.NodeGraph.GetOppositePin(LoopBodyPin);
            var bodyNode = data.NodeGraph.GetOppositePinNode(LoopBodyPin);
            if(bodyNode != null)
            {
                var bodyStatements = new TtExecuteSequenceStatement();
                bodyStatements.Sequence.Add(new TtDebuggerSetWatchVariable()
                {
                    VariableType = new TtTypeReference(typeof(UInt64)),
                    VariableName = mLoopIdxName + "_" + nodeId,
                    VariableValue = new TtVariableReferenceExpression(mLoopIdxName),
                });
                forStatement.LoopBody = bodyStatements;
                var loopData = new BuildCodeStatementsData();
                data.CopyTo(ref loopData);
                loopData.CurrentStatements = bodyStatements.Sequence;
                bodyNode.BuildStatements(bodyNodePin, ref loopData);
            }

            var nextNodePin = data.NodeGraph.GetOppositePin(AfterExec);
            var nextNode = data.NodeGraph.GetOppositePinNode(AfterExec);
            if (nextNode != null)
                nextNode.BuildStatements(nextNodePin, ref data);
        }
        public override TtExpressionBase GetExpression(NodePin pin, ref BuildCodeStatementsData data)
        {
            if (pin == null || pin == IndexPin)
                return new TtVariableReferenceExpression(mLoopIdxName);
            return null;
        }

        public void LightDebuggerLine()
        {
            var linker = ParentGraph.GetFirstLinker(BeforeExec);
            if (linker != null)
            {
                linker.InDebuggerLine = true;
                var node = ParentGraph.GetOppositePinNode(BeforeExec) as IBeforeExecNode;
                if (node != null)
                    node.LightDebuggerLine();
            }
        }

        public void UnLightDebuggerLine()
        {
            var linker = ParentGraph.GetFirstLinker(BeforeExec);
            if (linker != null)
            {
                linker.InDebuggerLine = false;
                var node = ParentGraph.GetOppositePinNode(BeforeExec) as IBeforeExecNode;
                if (node != null)
                    node.UnLightDebuggerLine();
            }
        }
    }
    [ContextMenu("whileloop", "FlowControl\\While", TtMacross.MacrossEditorKeyword, ShaderNode.UMaterialGraph.MaterialEditorKeyword)]
    public partial class WhileNode : UNodeBase, IBeforeExecNode, IAfterExecNode, IBreakableNode
    {
        public PinIn ConditionPin;
        public PinOut LoopBodyPin;
        [Browsable(false)]
        public PinIn BeforeExec { get; set; } = new PinIn();
        [Browsable(false)]
        public PinOut AfterExec { get; set; } = new PinOut();
        [Browsable(false)]
        public string BreakerName => "breaker_while_" + (uint)NodeId.GetHashCode();

        EBreakerState mBreakerState = EBreakerState.Hidden;
        [Browsable(false)]
        public EBreakerState BreakerState
        {
            get => mBreakerState;
            set
            {
                mBreakerState = value;
                Macross.TtMacrossDebugger.Instance.SetBreakEnable(BreakerName, (value == EBreakerState.Enable));
            }
        }
        public void AddMenuItems(TtMenuItem parentItem)
        {
            parentItem.AddMenuSeparator("BREAKPOINTS");
            parentItem.AddMenuItem("Add Breakpoint", null,
                (TtMenuItem item, object sender) =>
                {
                    BreakerState = EBreakerState.Enable;
                });
            parentItem.AddMenuItem("Remove Breakpoint", null,
                (TtMenuItem item, object sender) =>
                {
                    BreakerState = EBreakerState.Hidden;
                });
        }
        public WhileNode()
        {
            Name = "While";
            TitleColor = MacrossStyles.Instance.FlowControlTitleColor;
            BackColor = MacrossStyles.Instance.BGColor;

            BeforeExec.Name = " >>";
            AfterExec.Name = ">> ";
            BeforeExec.LinkDesc = MacrossStyles.Instance.NewExecPinDesc();
            AfterExec.LinkDesc = MacrossStyles.Instance.NewExecPinDesc();
            AddPinIn(BeforeExec);
            AddPinOut(AfterExec);
            ConditionPin = AddPinIn(new PinIn() { Name = "Condition" });
            ConditionPin.LinkDesc = MacrossStyles.Instance.NewInOutPinDesc();
            ConditionPin.LinkDesc.CanLinks.Add("Value");

            LoopBodyPin = AddPinOut(new PinOut() { Name = "LoopBody" });
            LoopBodyPin.LinkDesc = MacrossStyles.Instance.NewExecPinDesc();
        }

        public override void BuildStatements(NodePin pin, ref BuildCodeStatementsData data)
        {
            var whileStatement = new TtWhileLoopStatement();
            if (data.NodeGraph.PinHasLinker(ConditionPin))
                whileStatement.Condition = data.NodeGraph.GetOppositePinExpression(ConditionPin, ref data);
            else
                whileStatement.Condition = new TtPrimitiveExpression(false);

            data.CurrentStatements.Add(new TtDebuggerSetWatchVariable()
            {
                VariableType = new TtTypeReference(typeof(bool)),
                VariableName = ConditionPin.Name + "_" + (uint)NodeId.GetHashCode(),
                VariableValue = whileStatement.Condition,
            });
            AddDebugBreakerStatement(BreakerName, ref data);
            data.CurrentStatements.Add(whileStatement);
            var bodyNodePin = data.NodeGraph.GetOppositePin(LoopBodyPin);
            var bodyNode = data.NodeGraph.GetOppositePinNode(LoopBodyPin);
            if(bodyNode != null)
            {
                var bodyStatement = new TtExecuteSequenceStatement();
                whileStatement.LoopBody = bodyStatement;
                var loopData = new BuildCodeStatementsData();
                data.CopyTo(ref loopData);
                loopData.CurrentStatements = bodyStatement.Sequence;
                bodyNode.BuildStatements(bodyNodePin, ref loopData);
            }

            var nextNodePin = data.NodeGraph.GetOppositePin(AfterExec);
            var nextNode = data.NodeGraph.GetOppositePinNode(AfterExec);
            if (nextNode != null)
                nextNode.BuildStatements(nextNodePin, ref data);
        }

        public override void OnMouseStayPin(NodePin stayPin, UNodeGraph graph)
        {
            if(stayPin == ConditionPin)
            {
                EGui.Controls.CtrlUtility.DrawHelper(GetRuntimeValueString(ConditionPin.Name + "_" + (uint)NodeId.GetHashCode()));
            }
        }

        public void LightDebuggerLine()
        {
            var linker = ParentGraph.GetFirstLinker(BeforeExec);
            if (linker != null)
            {
                linker.InDebuggerLine = true;
                var node = ParentGraph.GetOppositePinNode(BeforeExec) as IBeforeExecNode;
                if (node != null)
                    node.LightDebuggerLine();
            }
        }

        public void UnLightDebuggerLine()
        {
            var linker = ParentGraph.GetFirstLinker(BeforeExec);
            if (linker != null)
            {
                linker.InDebuggerLine = false;
                var node = ParentGraph.GetOppositePinNode(BeforeExec) as IBeforeExecNode;
                if (node != null)
                    node.UnLightDebuggerLine();
            }
        }
    }
    [ContextMenu("continue", "FlowControl\\Continue", TtMacross.MacrossEditorKeyword, ShaderNode.UMaterialGraph.MaterialEditorKeyword)]
    public partial class ContinueNode : UNodeBase, IBeforeExecNode, IBreakableNode
    {
        [Browsable(false)]
        public PinIn BeforeExec { get; set; } = new PinIn();
        [Browsable(false)]
        public string BreakerName => "breaker_continue_" + (uint)NodeId.GetHashCode();
        EBreakerState mBreakerState = EBreakerState.Hidden;
        [Browsable(false)]
        public EBreakerState BreakerState
        {
            get => mBreakerState;
            set
            {
                mBreakerState = value;
                Macross.TtMacrossDebugger.Instance.SetBreakEnable(BreakerName, (value == EBreakerState.Enable));
            }
        }
        public void AddMenuItems(TtMenuItem parentItem)
        {
            parentItem.AddMenuSeparator("BREAKPOINTS");
            parentItem.AddMenuItem("Add Breakpoint", null,
                (TtMenuItem item, object sender) =>
                {
                    BreakerState = EBreakerState.Enable;
                });
            parentItem.AddMenuItem("Remove Breakpoint", null,
                (TtMenuItem item, object sender) =>
                {
                    BreakerState = EBreakerState.Hidden;
                });
        }
        public ContinueNode()
        {
            Name = "Continue";
            BeforeExec.Name = " >>";
            BeforeExec.LinkDesc = MacrossStyles.Instance.NewExecPinDesc();
            AddPinIn(BeforeExec);
        }
        public override void BuildStatements(NodePin pin, ref BuildCodeStatementsData data)
        {
            AddDebugBreakerStatement(BreakerName, ref data);
            data.CurrentStatements.Add(new TtContinueStatement());
        }

        public void LightDebuggerLine()
        {
            var linker = ParentGraph.GetFirstLinker(BeforeExec);
            if (linker != null)
            {
                linker.InDebuggerLine = true;
                var node = ParentGraph.GetOppositePinNode(BeforeExec) as IBeforeExecNode;
                if (node != null)
                    node.LightDebuggerLine();
            }
        }

        public void UnLightDebuggerLine()
        {
            var linker = ParentGraph.GetFirstLinker(BeforeExec);
            if (linker != null)
            {
                linker.InDebuggerLine = false;
                var node = ParentGraph.GetOppositePinNode(BeforeExec) as IBeforeExecNode;
                if (node != null)
                    node.UnLightDebuggerLine();
            }
        }

        public override object GetPropertyEditObject()
        {
            return null;
        }
    }
    [ContextMenu("break", "FlowControl\\Break", TtMacross.MacrossEditorKeyword, ShaderNode.UMaterialGraph.MaterialEditorKeyword)]
    public partial class BreakNode : UNodeBase, IBeforeExecNode, IBreakableNode
    {
        [Browsable(false)]
        public PinIn BeforeExec { get; set; } = new PinIn();
        [Browsable(false)]
        public string BreakerName => "breaker_break_" + (uint)NodeId.GetHashCode();
        EBreakerState mBreakerState = EBreakerState.Hidden;
        [Browsable(false)]
        public EBreakerState BreakerState
        {
            get => mBreakerState;
            set
            {
                mBreakerState = value;
                Macross.TtMacrossDebugger.Instance.SetBreakEnable(BreakerName, (value == EBreakerState.Enable));
            }
        }
        public void AddMenuItems(TtMenuItem parentItem)
        {
            parentItem.AddMenuSeparator("BREAKPOINTS");
            parentItem.AddMenuItem("Add Breakpoint", null,
                (TtMenuItem item, object sender) =>
                {
                    BreakerState = EBreakerState.Enable;
                });
            parentItem.AddMenuItem("Remove Breakpoint", null,
                (TtMenuItem item, object sender) =>
                {
                    BreakerState = EBreakerState.Hidden;
                });
        }
        public BreakNode()
        {
            Name = "Break";
            BeforeExec.Name = " >>";
            BeforeExec.LinkDesc = MacrossStyles.Instance.NewExecPinDesc();
            AddPinIn(BeforeExec);
        }
        public override void BuildStatements(NodePin pin, ref BuildCodeStatementsData data)
        {
            data.CurrentStatements.Add(new TtBreakStatement());
        }

        public void LightDebuggerLine()
        {
            var linker = ParentGraph.GetFirstLinker(BeforeExec);
            if (linker != null)
            {
                linker.InDebuggerLine = true;
                var node = ParentGraph.GetOppositePinNode(BeforeExec) as IBeforeExecNode;
                if (node != null)
                    node.LightDebuggerLine();
            }
        }

        public void UnLightDebuggerLine()
        {
            var linker = ParentGraph.GetFirstLinker(BeforeExec);
            if (linker != null)
            {
                linker.InDebuggerLine = false;
                var node = ParentGraph.GetOppositePinNode(BeforeExec) as IBeforeExecNode;
                if (node != null)
                    node.UnLightDebuggerLine();
            }
        }

        public override object GetPropertyEditObject()
        {
            return null;
        }
    }
}
