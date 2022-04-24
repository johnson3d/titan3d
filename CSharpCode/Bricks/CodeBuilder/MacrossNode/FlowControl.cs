using System;
using System.Collections.Generic;
using EngineNS.Bricks.NodeGraph;

namespace EngineNS.Bricks.CodeBuilder.MacrossNode
{
    public partial class SequenceNode : INodeExpr
    {
        public SequenceNode()
        {
            Name = "Sequence";
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF0fEF30;
            TitleColor = MacrossStyles.Instance.FlowControlTitleColor;
            BackColor = MacrossStyles.Instance.BGColor;

            AddPin.Name = "AddPin";
            AddPin.Link = MacrossStyles.Instance.NewExecPinDesc();
            AddPin.Link.CanLinks.Clear();

            AddPinIn(BeforeExec);
            AddPinOut(AddPin);
        }
        public PinOut AddPin { get; set; } = new PinOut();
        public List<PinOut> Sequences = new List<PinOut>();
        public override void OnLButtonClicked(NodePin clickedPin)
        {
            if (clickedPin == AddPin)
            {
                var aPin = new PinOut();
                aPin.Name = $"Pin{Sequences.Count}";
                aPin.Link = MacrossStyles.Instance.NewExecPinDesc();
                Sequences.Add(aPin);
                AddPinOut(aPin);
            }
        }
        public override void OnShowPinMenu(NodePin pin)
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
    public partial class IfNode : INodeExpr
    {
        public IfNode()
        {
            Name = "If";
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF0fEF30;
            TitleColor = MacrossStyles.Instance.FlowControlTitleColor;
            BackColor = MacrossStyles.Instance.BGColor;

            ConditionPin.Name = "bool";
            ConditionPin.Link = MacrossStyles.Instance.NewInOutPinDesc();
            ConditionPin.Link.CanLinks.Clear();
            ConditionPin.Link.CanLinks.Add("Value");

            TruePin.Name = "True";
            TruePin.Link = MacrossStyles.Instance.NewExecPinDesc();

            FalsePin.Name = "False";
            FalsePin.Link = MacrossStyles.Instance.NewExecPinDesc();

            AddPinIn(BeforeExec);
            AddPinOut(AfterExec);

            AddPinIn(ConditionPin);
            AddPinOut(TruePin);
            AddPinOut(FalsePin);
        }
        public PinIn ConditionPin { get; set; } = new PinIn();
        public PinOut TruePin { get; set; } = new PinOut();
        public PinOut FalsePin { get; set; } = new PinOut();
        public override bool CanLinkFrom(PinIn iPin, UNodeBase OutNode, PinOut oPin)
        {
            if (base.CanLinkFrom(iPin, OutNode, oPin) == false)
                return false;

            if (iPin == ConditionPin)
            {
                var nodeExpr = OutNode as INodeExpr;
                var type = nodeExpr.GetOutPinType(oPin);
                if (type == null)
                    return false;
                return type == typeof(bool);
            }
            return true;
        }
        public override IExpression GetExpr(UMacrossFunctionGraph funGraph, ICodeGen cGen, bool bTakeResult)
        {
            var ifOp = new IfOp();
            var links = new List<UPinLinker>();
            funGraph.FindInLinker(ConditionPin, links);
            if (links.Count != 1)
            {
                throw new GraphException(this, ConditionPin, $"Condition link error : {links.Count}");
            }
            var condiNode = links[0].OutNode as INodeExpr;
            var condiExpr = condiNode.GetExpr(funGraph, cGen, true) as OpExpress;
            {
                var condiType = condiExpr.GetType();
                bool checkOk = false;
                var cmpOp = condiExpr as BinocularOp;
                if (cmpOp != null && cmpOp.IsCmpOp())
                {
                    checkOk = true;
                }
                if (condiType.IsSubclassOf(typeof(BoolOp)))
                {
                    checkOk = true;
                }
                if(checkOk == false)
                {
                    throw new GraphException(this, ConditionPin, $"Condition must be bool expression");
                }
            }
            ifOp.Condition = condiExpr;

            links.Clear();
            funGraph.FindOutLinker(TruePin, links);
            if (links.Count == 1)
            {
                var trueNode = links[0].InNode as INodeExpr;
                var sequence = new ExecuteSequence();
                var trueExpr = trueNode.GetExpr(funGraph, cGen, false);
                if (trueExpr == null)
                {
                    throw new GraphException(this, TruePin, $"True expression must be ExecuteSequence");
                }
                else
                {
                    sequence.PushExpr(trueExpr);
                }
                ifOp.TrueExpr = sequence;
            }

            links.Clear();
            funGraph.FindOutLinker(FalsePin, links);
            if (links.Count == 1)
            {
                var falseNode = links[0].InNode as INodeExpr;
                var sequence = new ExecuteSequence();                
                var falseExpr = falseNode.GetExpr(funGraph, cGen, false);
                if (falseExpr == null)
                {
                    throw new GraphException(this, FalsePin, $"False expression must be ExecuteSequence");
                }
                else
                {
                    sequence.PushExpr(falseExpr);
                }
                ifOp.ElseExpr = sequence;
            }

            ifOp.NextExpr = this.GetNextExpr(funGraph, cGen);
            return ifOp;
        }
    }
    public partial class ReturnNode : INodeExpr
    {
        public static ReturnNode NewReturnNode(UMacrossFunctionGraph funGraph)
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

            AddPinIn(BeforeExec);
        }
        public void Initialize(UMacrossFunctionGraph funGraph)
        {
            ReturnType = funGraph.Function.ReturnType;
            var retType = Rtti.UTypeDescManager.Instance.GetTypeDescFromFullName(funGraph.Function.ReturnType);
            if (retType != null)
            {
                if (retType.SystemType == typeof(void))
                    return;
            }
            if (ReturnValuePin != null)
            {
                RemovePinIn(ReturnValuePin);
            }

            ReturnValuePin = new PinIn();

            ReturnValuePin.Name = "Result";
            ReturnValuePin.Link = MacrossStyles.Instance.NewInOutPinDesc();
            ReturnValuePin.Link.CanLinks.Add("Value");

            AddPinIn(ReturnValuePin);
        }
        public override void OnPreRead(object tagObject, object hostObject, bool fromXml)
        {
            base.OnPreRead(tagObject, hostObject, fromXml);

            var funGraph = this.ParentGraph as UMacrossFunctionGraph;
            if (funGraph == null)
                return;
            Initialize(funGraph);
        }
        [Rtti.Meta]
        public string ReturnType
        {
            get;
            set;
        }
        public PinIn ReturnValuePin { get; set; } = null;
        public override void OnMouseStayPin(NodePin stayPin)
        {
            if (ReturnValuePin != stayPin)
                return;

            EGui.Controls.CtrlUtility.DrawHelper(ReturnType);
        }
        public override bool CanLinkFrom(PinIn iPin, UNodeBase OutNode, PinOut oPin)
        {
            if (base.CanLinkFrom(iPin, OutNode, oPin) == false)
                return false;

            if (iPin == BeforeExec)
                return true;

            var funGraph = this.ParentGraph as UMacrossFunctionGraph;
            if (funGraph == null)
                return false;

            var nodeExpr = OutNode as INodeExpr;
            if (nodeExpr == null)
                return false;

            var retType = Rtti.UTypeDescManager.Instance.GetTypeDescFromFullName(funGraph.Function.ReturnType);
            if (retType != null)
            {
                if (retType.SystemType == typeof(void))
                    return false;

                var type = nodeExpr.GetOutPinType(oPin);
                if (ICodeGen.CanConvert(type, retType.SystemType))
                {
                    return true;
                }
            }
            return false;
        }

        public override IExpression GetExpr(UMacrossFunctionGraph funGraph, ICodeGen cGen, bool bTakeResult)
        {
            var retType = Rtti.UTypeDescManager.Instance.GetTypeDescFromFullName(funGraph.Function.ReturnType);
            if (retType == null)
                throw new GraphException(this, ReturnValuePin, "ReturnType is null");

            var expr = new ReturnOp();
            if (retType.SystemType != typeof(void))
            {
                var links = new List<UPinLinker>();
                funGraph.FindInLinker(ReturnValuePin, links);
                if (links.Count != 1)
                    throw new GraphException(this, ReturnValuePin, "Please link Result pin");
                var returnValueNode = links[0].OutNode as INodeExpr;
                var resultExpr = returnValueNode.GetExpr(funGraph, cGen, true) as OpExpress;
                var resultType = returnValueNode.GetOutPinType(links[0].OutPin);
                if (resultType == retType.SystemType)
                {
                    expr.ReturnExpr = resultExpr;
                }
                else
                {
                    var cvtExpr = new ConvertTypeOp();
                    cvtExpr.TargetType = cGen.GetTypeString(retType.SystemType);
                    cvtExpr.ObjExpr = resultExpr;
                    if (retType.SystemType.IsValueType == false && retType.SystemType.IsEnum == false)
                        cvtExpr.UseAs = true;
                    else
                        cvtExpr.UseAs = false;
                    expr.ReturnExpr = cvtExpr;
                }
                if (expr.ReturnExpr == null)
                    throw new GraphException(this, ReturnValuePin, "Result must link a OpExpression");
            }
            
            return expr;
        }
    }
}
