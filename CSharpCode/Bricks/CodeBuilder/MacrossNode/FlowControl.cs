using System;
using System.Collections.Generic;
using System.ComponentModel;
using EngineNS.Bricks.NodeGraph;

namespace EngineNS.Bricks.CodeBuilder.MacrossNode
{
    [ContextMenu("Sequence", "FlowControl\\Sequence", UMacross.MacrossEditorKeyword, ShaderNode.UMaterialGraph.MaterialEditorKeyword)]
    public partial class SequenceNode : UNodeBase, IBeforeExecNode
    {
        int mSequenceCount = 0;
        [Rtti.Meta]
        public int SequenceCount 
        {
            get => mSequenceCount;
            set
            {
                mSequenceCount = value;
                for(int i=0; i < mSequenceCount; i++)
                    AddSequencePin();

                OnPositionChanged();
            }
        }
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
        public PinOut FirstPin { get; set; } = new PinOut();
        public List<PinOut> Sequences = new List<PinOut>();

        void AddSequencePin()
        {
            var aPin = new PinOut();
            aPin.Name = $"Pin{Sequences.Count + 1}";
            aPin.LinkDesc = MacrossStyles.Instance.NewExecPinDesc();
            Sequences.Add(aPin);
            AddPinOut(aPin);
        }
        public override void OnShowPinMenu(NodePin pin)
        {
            if(ImGuiAPI.MenuItem("AddPin", null, false, true))
            {
                AddSequencePin();
                mSequenceCount = Sequences.Count;
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
        public override void BuildStatements(ref BuildCodeStatementsData data)
        {
            var firstLinker = data.NodeGraph.GetFirstLinker(FirstPin);
            if (firstLinker != null)
                firstLinker.InPin.HostNode.BuildStatements(ref data);
            for(int i=0; i<Sequences.Count; i++)
            {
                var linker = data.NodeGraph.GetFirstLinker(Sequences[i]);
                if (linker != null)
                    linker.InPin.HostNode.BuildStatements(ref data);
            }
        }
    }
    [ContextMenu("if", "FlowControl\\If", UMacross.MacrossEditorKeyword, ShaderNode.UMaterialGraph.MaterialEditorKeyword)]
    public partial class IfNode : UNodeBase, IBeforeExecNode, IAfterExecNode
    {
        public PinIn BeforeExec { get; set; } = new PinIn();
        public PinOut AfterExec { get; set; } = new PinOut();

        int mConditionCount = 1;
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
        public override void BuildStatements(ref BuildCodeStatementsData data)
        {
            var ifStatement = new UIfStatement();
            data.CurrentStatements.Add(ifStatement);
            for(int i=0; i<ConditionResultPairs.Count; i++)
            {
                UExpressionBase condition;
                if (data.NodeGraph.PinHasLinker(ConditionResultPairs[i].Key))
                    condition = data.NodeGraph.GetOppositePinExpression(ConditionResultPairs[i].Key, ref data);
                else
                    condition = new UPrimitiveExpression(true);
                var node = data.NodeGraph.GetOppositePinNode(ConditionResultPairs[i].Value);
                var trueStatement = new UExecuteSequenceStatement();
                if(node != null)
                {
                    var trueData = new BuildCodeStatementsData();
                    data.CopyTo(ref trueData);
                    trueData.CurrentStatements = trueStatement.Sequence;
                    node.BuildStatements(ref trueData);
                }
                if(i == 0)
                {
                    ifStatement.Condition = condition;
                    ifStatement.TrueStatement = trueStatement;
                }
                else
                {
                    var elseIfStatement = new UIfStatement()
                    {
                        Condition = condition,
                        TrueStatement = trueStatement,
                    };
                    ifStatement.ElseIfs.Add(elseIfStatement);
                }
            }
            var falseStatement = new UExecuteSequenceStatement();
            ifStatement.FalseStatement = falseStatement;
            var falseNode = data.NodeGraph.GetOppositePinNode(FalsePin);
            if(falseNode != null)
            {
                var falseData = new BuildCodeStatementsData();
                data.CopyTo(ref falseData);
                falseData.CurrentStatements = falseStatement.Sequence;
                falseNode.BuildStatements(ref falseData);
            }

            var nextNode = data.NodeGraph.GetOppositePinNode(AfterExec);
            if (nextNode != null)
                nextNode.BuildStatements(ref data);
        }
    }
    [ContextMenu("return", "FlowControl\\Return", UMacross.MacrossEditorKeyword, ShaderNode.UMaterialGraph.MaterialEditorKeyword)]
    public partial class ReturnNode : UNodeBase, IBeforeExecNode
    {
        public PinIn BeforeExec { get; set; } = new PinIn();

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
            if(data.MethodDec.ReturnValue != null)
            {
                var retPin = AddPinIn(new PinIn()
                {
                    Name = data.MethodDec.ReturnValue.VariableName
                });
            }
            for(int i=0; i<data.MethodDec.Arguments.Count; i++)
            {
                var argDec = data.MethodDec.Arguments[i];
                if(argDec.OperationType == EMethodArgumentAttribute.Ref ||
                   argDec.OperationType == EMethodArgumentAttribute.Out)
                {
                    var pin = AddPinIn(new PinIn()
                    {
                        Name = argDec.VariableName
                    });
                }
            }
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
        public override void OnMouseStayPin(NodePin stayPin)
        {
            //if (ReturnValuePin != stayPin)
            //    return;

            //EGui.Controls.CtrlUtility.DrawHelper(ReturnType);
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
        public override void BuildStatements(ref BuildCodeStatementsData data)
        {
            for(int i=0; i<Inputs.Count; i++)
            {
                if (Inputs[i] == BeforeExec)
                    continue;

                var exp = data.NodeGraph.GetOppositePinExpression(Inputs[i], ref data);
                var st = new UAssignOperatorStatement()
                {
                    From = exp,
                    To = new UVariableReferenceExpression(Inputs[i].Name)
                };
                data.CurrentStatements.Add(st);
            }
            data.CurrentStatements.Add(new UReturnStatement());
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
    }
    [ContextMenu("forloop", "FlowControl\\For", UMacross.MacrossEditorKeyword, ShaderNode.UMaterialGraph.MaterialEditorKeyword)]
    public partial class ForLoopNode : UNodeBase, IBeforeExecNode, IAfterExecNode
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

        public override void BuildStatements(ref BuildCodeStatementsData data)
        {
            var forStatement = new UForLoopStatement()
            {
                LoopIndexName = mLoopIdxName,
            };
            data.CurrentStatements.Add(forStatement);
            var beginIdxExp = data.NodeGraph.GetOppositePinExpression(BeginIdxPin, ref data);
            if (beginIdxExp != null)
                forStatement.BeginExpression = beginIdxExp;
            else
                forStatement.BeginExpression = new UPrimitiveExpression(BeginIdx);
            var endIdxExp = data.NodeGraph.GetOppositePinExpression(EndIdxPin, ref data);
            if (endIdxExp != null)
                forStatement.EndExpression = endIdxExp;
            else
                forStatement.EndExpression = new UPrimitiveExpression(EndIdx);
            var stepExp = data.NodeGraph.GetOppositePinExpression(StepPin, ref data);
            if (stepExp != null)
                forStatement.StepExpression = stepExp;
            else
                forStatement.StepExpression = new UPrimitiveExpression(StepIdx);

            var bodyNode = data.NodeGraph.GetOppositePinNode(LoopBodyPin);
            if(bodyNode != null)
            {
                var bodyStatements = new UExecuteSequenceStatement();
                forStatement.LoopBody = bodyStatements;
                var loopData = new BuildCodeStatementsData();
                data.CopyTo(ref loopData);
                loopData.CurrentStatements = bodyStatements.Sequence;
                bodyNode.BuildStatements(ref loopData);
            }

            var nextNode = data.NodeGraph.GetOppositePinNode(AfterExec);
            if (nextNode != null)
                nextNode.BuildStatements(ref data);
        }
        public override UExpressionBase GetExpression(NodePin pin, ref BuildCodeStatementsData data)
        {
            if (pin == null || pin == IndexPin)
                return new UVariableReferenceExpression(mLoopIdxName);
            return null;
        }
    }
    [ContextMenu("whileloop", "FlowControl\\While", UMacross.MacrossEditorKeyword, ShaderNode.UMaterialGraph.MaterialEditorKeyword)]
    public partial class WhileNode : UNodeBase, IBeforeExecNode, IAfterExecNode
    {
        public PinIn ConditionPin;
        public PinOut LoopBodyPin;
        public PinIn BeforeExec { get; set; } = new PinIn();
        public PinOut AfterExec { get; set; } = new PinOut();

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

        public override void BuildStatements(ref BuildCodeStatementsData data)
        {
            var whileStatement = new UWhileLoopStatement();
            data.CurrentStatements.Add(whileStatement);
            if (data.NodeGraph.PinHasLinker(ConditionPin))
                whileStatement.Condition = data.NodeGraph.GetOppositePinExpression(ConditionPin, ref data);
            else
                whileStatement.Condition = new UPrimitiveExpression(false);

            var bodyNode = data.NodeGraph.GetOppositePinNode(LoopBodyPin);
            if(bodyNode != null)
            {
                var bodyStatement = new UExecuteSequenceStatement();
                whileStatement.LoopBody = bodyStatement;
                var loopData = new BuildCodeStatementsData();
                data.CopyTo(ref loopData);
                loopData.CurrentStatements = bodyStatement.Sequence;
                bodyNode.BuildStatements(ref loopData);
            }

            var nextNode = data.NodeGraph.GetOppositePinNode(AfterExec);
            if (nextNode != null)
                nextNode.BuildStatements(ref data);
        }
    }
    [ContextMenu("continue", "FlowControl\\Continue", UMacross.MacrossEditorKeyword, ShaderNode.UMaterialGraph.MaterialEditorKeyword)]
    public partial class ContinueNode : UNodeBase, IBeforeExecNode
    {
        public PinIn BeforeExec { get; set; } = new PinIn();
        public ContinueNode()
        {
            Name = "Continue";
            BeforeExec.Name = " >>";
            BeforeExec.LinkDesc = MacrossStyles.Instance.NewExecPinDesc();
            AddPinIn(BeforeExec);
        }
        public override void BuildStatements(ref BuildCodeStatementsData data)
        {
            data.CurrentStatements.Add(new UContinueStatement());
        }
    }
    [ContextMenu("break", "FlowControl\\Break", UMacross.MacrossEditorKeyword, ShaderNode.UMaterialGraph.MaterialEditorKeyword)]
    public partial class BreakNode : UNodeBase, IBeforeExecNode
    {
        public PinIn BeforeExec { get; set; } = new PinIn();
        public BreakNode()
        {
            Name = "Break";
            BeforeExec.Name = " >>";
            BeforeExec.LinkDesc = MacrossStyles.Instance.NewExecPinDesc();
            AddPinIn(BeforeExec);
        }
        public override void BuildStatements(ref BuildCodeStatementsData data)
        {
            data.CurrentStatements.Add(new UBreakStatement());
        }
    }
}
