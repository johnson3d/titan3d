using EngineNS.Bricks.NodeGraph;
using EngineNS.Rtti;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.CodeBuilder.MacrossNode
{
    [ContextMenu("istype,checktype", "Utilitites\\IsType", TtMacross.MacrossEditorKeyword)]
    public partial class IsTypeNode : TtNodeBase, UEditableValue.IValueEditNotify
    {
        public PinIn InPin { get; set; } = new PinIn();
        public PinIn TypePin { get; set; } = new PinIn();
        public PinOut OutPin { get; set; }

        [Rtti.Meta(Order = 0)]
        public Rtti.TtTypeDesc TargetType
        {
            get;
            set;
        }

        public IsTypeNode()
        {
            Initialize();
        }
        protected virtual void Initialize()
        {
            Name = "IsType";
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF0fEF30;
            TitleColor = MacrossStyles.Instance.FlowControlTitleColor;
            BackColor = MacrossStyles.Instance.BGColor;

            InPin.Name = "Value";
            InPin.LinkDesc = MacrossStyles.Instance.NewInOutPinDesc();
            InPin.LinkDesc.CanLinks.Add("Value");
            AddPinIn(InPin);

            TypePin.Name = "Type";
            TypePin.LinkDesc = MacrossStyles.Instance.NewInOutPinDesc();
            TypePin.LinkDesc.CanLinks.Add("Value");
            var ev = UEditableValue.CreateEditableValue(this, Rtti.TtTypeDesc.TypeOf(typeof(System.Type)), TypePin) as UTypeSelectorEValue;
            foreach (var service in Rtti.TtTypeDescManager.Instance.Services.Values)
            {
                foreach (var typeDesc in service.Types.Values)
                {
                    ev.Selector.AddShowType(typeDesc);
                }
            }
            TypePin.EditValue = ev;
            AddPinIn(TypePin);

            OutPin = new PinOut();
            OutPin.Name = "Result";
            OutPin.LinkDesc = MacrossStyles.Instance.NewInOutPinDesc();
            OutPin.LinkDesc.CanLinks.Add("Value");
            AddPinOut(OutPin);
        }
        public override void OnMouseStayPin(NodePin stayPin, TtNodeGraph graph)
        {
            if(stayPin == InPin)
            {
                if(InPin.HasLinker())
                {
                    var type = graph.GetOppositePinType(InPin);
                    if (type != null)
                        EGui.Controls.CtrlUtility.DrawHelper(type.FullName);
                }
            }
            else if(stayPin == TypePin)
            {
                if(TypePin.HasLinker())
                {
                    var type = graph.GetOppositePinType(TypePin);
                    if(type != null)
                        EGui.Controls.CtrlUtility.DrawHelper(type.FullName);
                }
                else if(TargetType != null)
                    EGui.Controls.CtrlUtility.DrawHelper(TargetType.FullName);
            }
            else if(stayPin == OutPin)
            {
                EGui.Controls.CtrlUtility.DrawHelper("bool");
            }
        }

        public void OnValueChanged(UEditableValue ev)
        {
            if(ev.ValueType.FullName == "System.Type")
            {
                var pin = ev.Tag as PinIn;
                if (pin == null)
                    return;
                TargetType = (Rtti.TtTypeDesc)ev.Value;
                if(OutPin != null)
                    OutPin.Tag = TargetType;
            }
        }
        public override TtExpressionBase GetExpression(NodePin pin, ref BuildCodeStatementsData data)
        {
            if(pin == null || pin == OutPin)
            {
                if(!InPin.HasLinker())
                {
                    return new TtPrimitiveExpression(false);
                }
                var leftExp = data.NodeGraph.GetOppositePinExpression(InPin, ref data);
                TtTypeDesc finalType = TargetType;
                if (TypePin.HasLinker())
                    finalType = data.NodeGraph.GetOppositePinType(TypePin);
                var rightExp = new TtPrimitiveExpression(finalType, false);

                return new TtBinaryOperatorExpression()
                {
                    Left = leftExp,
                    Right = rightExp,
                    Operation = TtBinaryOperatorExpression.EBinaryOperation.Is,
                };
            }
            return null;
        }
        public override TtTypeDesc GetOutPinType(PinOut pin)
        {
            if(pin == OutPin)
                return TtTypeDesc.TypeOf(typeof(bool));
            return null;
        }
    }
    [ContextMenu("istype,checktype", "Utilitites\\IsTypeEx", TtMacross.MacrossEditorKeyword)]
    public partial class IsTypeNodeEx : IsTypeNode, IBeforeExecNode
    {
        public PinIn BeforeExec { get; set; } = new PinIn();
        public PinOut TrueExec;
        public PinOut FalseExec;

        protected override void Initialize()
        {
            Name = "IsType";
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF0fEF30;
            TitleColor = MacrossStyles.Instance.FlowControlTitleColor;
            BackColor = MacrossStyles.Instance.BGColor;

            BeforeExec.Name = " >>";
            BeforeExec.LinkDesc = MacrossStyles.Instance.NewExecPinDesc();
            AddPinIn(BeforeExec);

            InPin.Name = "Value";
            InPin.LinkDesc = MacrossStyles.Instance.NewInOutPinDesc();
            InPin.LinkDesc.CanLinks.Add("Value");
            AddPinIn(InPin);

            TypePin.Name = "Type";
            TypePin.LinkDesc = MacrossStyles.Instance.NewInOutPinDesc();
            TypePin.LinkDesc.CanLinks.Add("Value");
            var ev = UEditableValue.CreateEditableValue(this, Rtti.TtTypeDesc.TypeOf(typeof(System.Type)), TypePin) as UTypeSelectorEValue;
            foreach (var service in Rtti.TtTypeDescManager.Instance.Services.Values)
            {
                foreach (var typeDesc in service.Types.Values)
                {
                    ev.Selector.AddShowType(typeDesc);
                }
            }
            TypePin.EditValue = ev;
            AddPinIn(TypePin);

            TrueExec = new PinOut();
            TrueExec.Name = "True";
            TrueExec.LinkDesc = MacrossStyles.Instance.NewExecPinDesc();
            AddPinOut(TrueExec);

            FalseExec = new PinOut();
            FalseExec.Name = "False";
            FalseExec.LinkDesc = MacrossStyles.Instance.NewExecPinDesc();
            AddPinOut(FalseExec);
        }

        public void LightDebuggerLine()
        {
            base.LightDebuggerLine(BeforeExec);
        }

        public void UnLightDebuggerLine()
        {
            base.UnLightDebuggerLine(BeforeExec);
        }

        public override void BuildStatements(NodePin pin, ref BuildCodeStatementsData data)
        {
            var ifStatement = new TtIfStatement();
            ifStatement.Condition = GetExpression(null, ref data);
            data.CurrentStatements.Add(ifStatement);
            var trueStatement = new TtExecuteSequenceStatement();
            ifStatement.TrueStatement = trueStatement;
            if(TrueExec.HasLinker())
            {
                var trueOpPin = data.NodeGraph.GetOppositePin(TrueExec);
                var trueOpNode = data.NodeGraph.GetOppositePinNode(TrueExec);
                if(trueOpNode != null)
                {
                    var trueData = new BuildCodeStatementsData();
                    data.CopyTo(ref trueData);
                    trueData.CurrentStatements = trueStatement.Sequence;
                    trueOpNode.BuildStatements(trueOpPin, ref trueData);
                }
            }
            if(FalseExec.HasLinker())
            {
                var falseOpPin = data.NodeGraph.GetOppositePin(FalseExec);
                var falseOpNode = data.NodeGraph.GetOppositePinNode(FalseExec);
                if (falseOpNode != null)
                {
                    var falseStatement = new TtExecuteSequenceStatement();
                    ifStatement.FalseStatement = falseStatement;
                    var falseData = new BuildCodeStatementsData();
                    data.CopyTo(ref falseData);
                    falseData.CurrentStatements = falseStatement.Sequence;
                    falseOpNode.BuildStatements(falseOpPin, ref falseData);
                }
            }
        }
    }

    [ContextMenu("isnull,isvalid,valid", "Utilitites\\IsValid", TtMacross.MacrossEditorKeyword)]
    public partial class IsValidNode : TtNodeBase
    {
        public PinIn InPin { get; set; } = new PinIn();
        public PinOut OutPin { get; set; }

        public IsValidNode()
        {
            Initialize();
        }
        protected virtual void Initialize()
        {
            Name = "IsValid";
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF0fEF30;
            TitleColor = MacrossStyles.Instance.FlowControlTitleColor;
            BackColor = MacrossStyles.Instance.BGColor;

            InPin.Name = "Value";
            InPin.LinkDesc = MacrossStyles.Instance.NewInOutPinDesc();
            InPin.LinkDesc.CanLinks.Add("Value");
            AddPinIn(InPin);

            OutPin = new PinOut();
            OutPin.Name = "Result";
            OutPin.LinkDesc = MacrossStyles.Instance.NewInOutPinDesc();
            OutPin.LinkDesc.CanLinks.Add("Value");
            AddPinOut(OutPin);
        }
        public override void OnMouseStayPin(NodePin stayPin, TtNodeGraph graph)
        {
            if(stayPin == InPin)
            {
                if (InPin.HasLinker())
                {
                    var type = graph.GetOppositePinType(InPin);
                    if (type != null)
                        EGui.Controls.CtrlUtility.DrawHelper(type.FullName);
                }
            }
            else if(stayPin == OutPin)
                EGui.Controls.CtrlUtility.DrawHelper("bool");
        }

        public override TtExpressionBase GetExpression(NodePin pin, ref BuildCodeStatementsData data)
        {
            if(pin == null || pin == OutPin)
            {
                if (!InPin.HasLinker())
                    return new TtPrimitiveExpression(false);
                var inType = data.NodeGraph.GetOppositePinType(InPin);
                if(inType.IsValueType)
                {
                    return new TtPrimitiveExpression(true);
                }
                else
                {
                    var leftExp = data.NodeGraph.GetOppositePinExpression(InPin, ref data);
                    var rightExp = new TtNullValueExpression();
                    return new TtBinaryOperatorExpression()
                    {
                        Left = leftExp,
                        Right = rightExp,
                        Operation = TtBinaryOperatorExpression.EBinaryOperation.NotEquality,
                    };
                }
            }
            return null;
        }
        public override TtTypeDesc GetOutPinType(PinOut pin)
        {
            if (pin == OutPin)
                return TtTypeDesc.TypeOf(typeof(bool));
            return null;
        }
    }

    [ContextMenu("isnull,isvalid,valid", "Utilitites\\IsValidEx", TtMacross.MacrossEditorKeyword)]
    public partial class IsValidNodeEx : IsValidNode, IBeforeExecNode
    {
        public PinIn BeforeExec { get; set; } = new PinIn();
        public PinOut TrueExec;
        public PinOut FalseExec;

        protected override void Initialize()
        {
            Name = "IsValid";
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF0fEF30;
            TitleColor = MacrossStyles.Instance.FlowControlTitleColor;
            BackColor = MacrossStyles.Instance.BGColor;

            BeforeExec.Name = " >>";
            BeforeExec.LinkDesc = MacrossStyles.Instance.NewExecPinDesc();
            AddPinIn(BeforeExec);

            InPin.Name = "Value";
            InPin.LinkDesc = MacrossStyles.Instance.NewInOutPinDesc();
            InPin.LinkDesc.CanLinks.Add("Value");
            AddPinIn(InPin);

            TrueExec = new PinOut();
            TrueExec.Name = "True";
            TrueExec.LinkDesc = MacrossStyles.Instance.NewExecPinDesc();
            AddPinOut(TrueExec);

            FalseExec = new PinOut();
            FalseExec.Name = "False";
            FalseExec.LinkDesc = MacrossStyles.Instance.NewExecPinDesc();
            AddPinOut(FalseExec);
        }

        public void LightDebuggerLine()
        {
            base.LightDebuggerLine(BeforeExec);
        }

        public void UnLightDebuggerLine()
        {
            base.UnLightDebuggerLine(BeforeExec);
        }

        public override void BuildStatements(NodePin pin, ref BuildCodeStatementsData data)
        {
            var ifStatement = new TtIfStatement();
            ifStatement.Condition = GetExpression(null, ref data);
            data.CurrentStatements.Add(ifStatement);
            var trueStatement = new TtExecuteSequenceStatement();
            ifStatement.TrueStatement = trueStatement;
            if(TrueExec.HasLinker())
            {
                var trueOpPin = data.NodeGraph.GetOppositePin(TrueExec);
                var trueOpNode = data.NodeGraph.GetOppositePinNode(TrueExec);
                if(trueOpNode != null)
                {
                    var trueData = new BuildCodeStatementsData();
                    data.CopyTo(ref trueData);
                    trueData.CurrentStatements = trueStatement.Sequence;
                    trueOpNode.BuildStatements(trueOpPin, ref trueData);
                }
            }
            if(FalseExec.HasLinker())
            {
                var falseOpPin = data.NodeGraph.GetOppositePin(FalseExec);
                var falseOpNode = data.NodeGraph.GetOppositePinNode(FalseExec);
                if(falseOpNode != null)
                {
                    var falseStatement = new TtExecuteSequenceStatement();
                    ifStatement.FalseStatement = falseStatement;
                    var falseData = new BuildCodeStatementsData();
                    data.CopyTo(ref falseData);
                    falseData.CurrentStatements = falseStatement.Sequence;
                    falseOpNode.BuildStatements(falseOpPin, ref falseData);
                }
            }
        }
    }
}
