using EngineNS.Bricks.NodeGraph;
using EngineNS.EGui.Controls;
using EngineNS.Rtti;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.Bricks.CodeBuilder.MacrossNode
{
    [ContextMenu("create,creator,new", "Create instance", UMacross.MacrossEditorKeyword)]
    public partial class CreatorNode : UNodeBase, IBeforeExecNode, IAfterExecNode, IBreakableNode, UEditableValue.IValueEditNotify
    {
        public PinIn TypePin;
        public PinOut OutPin;
        [Browsable(false)]
        public PinIn BeforeExec { get; set; } = new PinIn();
        [Browsable(false)]
        public PinOut AfterExec { get; set; } = new PinOut();

        public string BreakerName => "breaker_creator_" + (uint)NodeId.GetHashCode();

        EBreakerState mBreakerState = EBreakerState.Hidden;
        [Browsable(false)]
        public EBreakerState BreakerState
        {
            get => mBreakerState;
            set
            {
                mBreakerState = value;
                Macross.UMacrossDebugger.Instance.SetBreakEnable(BreakerName, (value == EBreakerState.Enable));
            }
        }

        public string VariableName => "new_" + (uint)NodeId.GetHashCode();
        Rtti.UTypeDesc mTargetType;
        [Rtti.Meta(Order = 0)]
        public Rtti.UTypeDesc TargetType
        {
            get => mTargetType;
            set
            {
                mTargetType = value;
            }
        }

        public CreatorNode()
        {
            Name = "Create";
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
            TypePin = AddPinIn(new PinIn() { Name = "Type" });
            TypePin.LinkDesc = MacrossStyles.Instance.NewInOutPinDesc();
            TypePin.LinkDesc.CanLinks.Add("Value");
            var ev = UEditableValue.CreateEditableValue(this, Rtti.UTypeDesc.TypeOf(typeof(System.Type)), TypePin) as UTypeSelectorEValue;
            foreach(var meta in Rtti.TtClassMetaManager.Instance.Metas.Values)
            {
                if (meta.MetaAttribute == null)
                    continue;
                if (meta.MetaAttribute.IsNoMacrossUseable)
                    continue;
                if (meta.MetaAttribute.IsNoMacrossCreate)
                    continue;
                ev.Selector.AddShowType(meta.ClassType);
            }
            TypePin.EditValue = ev;

            OutPin = AddPinOut(new PinOut() { Name = "Instance" });
            OutPin.LinkDesc = MacrossStyles.Instance.NewInOutPinDesc();
        }

        public override UExpressionBase GetExpression(NodePin pin, ref BuildCodeStatementsData data)
        {
            if (pin == null || pin == OutPin)
                return new UVariableReferenceExpression(VariableName);
            return null;
        }

        public override void BuildStatements(NodePin pin, ref BuildCodeStatementsData data)
        {
            if (pin != null && pin != OutPin)
                return;
            UTypeDesc resultType = TargetType;
            if(TypePin.HasLinker())
            {
                resultType = data.NodeGraph.GetOppositePinType(TypePin);
            }
            var varDecStatement = new UVariableDeclaration()
            {
                VariableName = VariableName,
                VariableType = new UTypeReference(resultType),
                InitValue = new UCreateObjectExpression(data.CodeGen.GetTypeString(resultType)),
            };
            data.CurrentStatements.Add(new UDebuggerSetWatchVariable()
            {
                VariableType = new UTypeReference(typeof(Type)),
                VariableName = TypePin.Name + "_" + (uint)NodeId.GetHashCode(),
                VariableValue = new UVariableReferenceExpression(VariableName),
            });
            AddDebugBreakerStatement(BreakerName, ref data);
            data.CurrentStatements.Add(varDecStatement);

            var nextNodePin = data.NodeGraph.GetOppositePin(AfterExec);
            var nextNode = data.NodeGraph.GetOppositePinNode(AfterExec);
            if (nextNode != null)
                nextNode.BuildStatements(nextNodePin, ref data);
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

        public void OnValueChanged(UEditableValue ev)
        {
            if(ev.ValueType.FullName == "System.Type")
            {
                var pin = ev.Tag as PinIn;
                if (pin == null)
                    return;
                mTargetType = (Rtti.UTypeDesc)ev.Value;
                OutPin.Tag = mTargetType;
            }
        }

        public override void OnMouseStayPin(NodePin stayPin, UNodeGraph graph)
        {
            if(mTargetType == null)
                EGui.Controls.CtrlUtility.DrawHelper("null");
            else if(stayPin == TypePin)
            {
                if(TypePin.HasLinker())
                {
                    var type = graph.GetOppositePinType(TypePin);
                    if(type != null)
                        EGui.Controls.CtrlUtility.DrawHelper(type.FullName);
                }
                else if(mTargetType != null)
                    EGui.Controls.CtrlUtility.DrawHelper(mTargetType.FullName);
            }
            else if(stayPin == OutPin)
            {
                string valueStr = GetRuntimeValueString(VariableName);
                string typeString = mTargetType.FullName;
                EGui.Controls.CtrlUtility.DrawHelper($"{valueStr}({typeString})");
            }
        }
        public override UTypeDesc GetInPinType(PinIn pin)
        {
            if(pin == TypePin)
                return mTargetType;
            return null;
        }
        public override UTypeDesc GetOutPinType(PinOut pin)
        {
            if(pin == OutPin)
                return mTargetType;
            return null;
        }
    }
}
