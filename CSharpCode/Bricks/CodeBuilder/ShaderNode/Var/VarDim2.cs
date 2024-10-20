using System;
using System.Collections.Generic;
using EngineNS.Bricks.NodeGraph;
using System.ComponentModel;

namespace EngineNS.Bricks.CodeBuilder.ShaderNode.Var
{
    [ContextMenu("f2,float2", "Data\\float2@_serial@", TtMaterialGraph.MaterialEditorKeyword)]
    public class VarDimF2 : VarNode
    {
        public override Rtti.TtTypeDesc GetOutPinType(PinOut pin)
        {
            if (pin == OutXY)
                return Rtti.TtTypeDesc.TypeOf(typeof(Vector2));
            else if (pin == OutX ||
                pin == OutY)
                return Rtti.TtTypeDesc.TypeOf(typeof(float));
            return null;
        }
        public override bool CanLinkFrom(PinIn iPin, TtNodeBase OutNode, PinOut oPin)
        {
            if (base.CanLinkFrom(iPin, OutNode, oPin) == false)
                return false;

            var nodeExpr = OutNode as TtNodeBase;
            var type = nodeExpr.GetOutPinType(oPin);
            if (type == null)
                return false;

            if (iPin == InXY)
            {
                if (!type.IsEqual(typeof(Vector2)))
                    return false;
            }
            else if (iPin == InX ||
                iPin == InY)
            {
                if (!type.IsEqual(typeof(float)))
                    return false;
            }
            return true;
        }
        protected override string GetDefaultValue()
        {
            if (IsHalfPrecision)
                return $"half2({Value.X}, {Value.Y})";
            else
                return $"float2({Value.X}, {Value.Y})";
        }
        public override void BuildStatements(NodePin pin, ref BuildCodeStatementsData data)
        {
            base.BuildStatements(pin, ref data);
            if (data.NodeGraph.PinHasLinker(InXY))
            {
                var pinNode = data.NodeGraph.GetOppositePinNode(InXY);
                var opPin = data.NodeGraph.GetOppositePin(InXY);
                pinNode.BuildStatements(opPin, ref data);

                var assignStatement = new TtAssignOperatorStatement()
                {
                    From = pinNode.GetExpression(opPin, ref data),
                    To = new TtVariableReferenceExpression(Name),
                };
                if(!data.CurrentStatements.Contains(assignStatement))
                    data.CurrentStatements.Add(assignStatement);
            }
            if(data.NodeGraph.PinHasLinker(InX))
            {
                var pinNode = data.NodeGraph.GetOppositePinNode(InX);
                var opPin = data.NodeGraph.GetOppositePin(InX);
                pinNode.BuildStatements(opPin, ref data);

                var assignStatement = new TtAssignOperatorStatement()
                {
                    From = pinNode.GetExpression(opPin, ref data),
                    To = new TtVariableReferenceExpression("x", new TtVariableReferenceExpression(Name)),
                };
                if(!data.CurrentStatements.Contains(assignStatement))
                    data.CurrentStatements.Add(assignStatement);
            }
            if (data.NodeGraph.PinHasLinker(InY))
            {
                var pinNode = data.NodeGraph.GetOppositePinNode(InY);
                var opPin = data.NodeGraph.GetOppositePin(InY);
                pinNode.BuildStatements(opPin, ref data);

                var assignStatement = new TtAssignOperatorStatement()
                {
                    From = pinNode.GetExpression(opPin, ref data),
                    To = new TtVariableReferenceExpression("y", new TtVariableReferenceExpression(Name)),
                };
                if(!data.CurrentStatements.Contains(assignStatement))
                    data.CurrentStatements.Add(assignStatement);
            }

            if (!data.MethodDec.HasLocalVariable(Name))
            {
                var val = new TtVariableDeclaration()
                {
                    VariableType = new TtTypeReference(Type2HLSLType(VarType, IsHalfPrecision)),
                    VariableName = Name,
                    InitValue = new TtPrimitiveExpression(Value),
                };
                OnAddLocalVar(val, ref data);
            }
        }
        public override TtExpressionBase GetExpression(NodePin pin, ref BuildCodeStatementsData data)
        {
            if (pin == OutXY)
                return new TtVariableReferenceExpression(Name);
            else if (pin == OutX)
                return new TtVariableReferenceExpression("x", new TtVariableReferenceExpression(Name));
            else if (pin == OutY)
                return new TtVariableReferenceExpression("y", new TtVariableReferenceExpression(Name));
            return null;
        }

        [Rtti.Meta]
        [Category("Option")]
        public Vector2 Value { get; set; } = Vector2.Zero;
        [Browsable(false)]
        public PinIn InXY { get; set; } = new PinIn();
        [Browsable(false)]
        public PinIn InX { get; set; } = new PinIn();
        [Browsable(false)]
        public PinIn InY { get; set; } = new PinIn();
        [Browsable(false)]
        public PinOut OutXY { get; set; } = new PinOut();
        [Browsable(false)]
        public PinOut OutX { get; set; } = new PinOut();
        [Browsable(false)]
        public PinOut OutY { get; set; } = new PinOut();
        public VarDimF2()
        {
            VarType = Rtti.TtTypeDescGetter<Vector2>.TypeDesc;

            //Name = $"{Value}";

            InXY.Name = "in ";
            InXY.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinIn(InXY);
            InX.Name = "x ";
            InX.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinIn(InX);
            InY.Name = "y ";
            InY.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinIn(InY);

            OutXY.Name = " xy";
            OutXY.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            OutXY.MultiLinks = true;
            this.AddPinOut(OutXY);
            OutX.Name = " x";
            OutX.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            OutX.MultiLinks = true;
            this.AddPinOut(OutX);
            OutY.Name = " y";
            OutY.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            OutY.MultiLinks = true;
            this.AddPinOut(OutY);
        }
    }

    [ContextMenu("i2,int2", "Data\\int2@_serial@", TtMaterialGraph.MaterialEditorKeyword)]
    public class VarDimI2 : VarNode
    {
        public override Rtti.TtTypeDesc GetOutPinType(PinOut pin)
        {
            if (pin == OutXY)
                return Rtti.TtTypeDesc.TypeOf(typeof(Vector2i));
            else if (pin == OutX ||
                pin == OutY)
                return Rtti.TtTypeDesc.TypeOf(typeof(int));
            return null;
        }
        public override bool CanLinkFrom(PinIn iPin, TtNodeBase OutNode, PinOut oPin)
        {
            if (base.CanLinkFrom(iPin, OutNode, oPin) == false)
                return false;

            var nodeExpr = OutNode as TtNodeBase;
            var type = nodeExpr.GetOutPinType(oPin);
            if (type == null)
                return false;

            if (iPin == InXY)
            {
                if (!type.IsEqual(typeof(Vector2i)))
                    return false;
            }
            else if (iPin == InX ||
                iPin == InY)
            {
                if (!type.IsEqual(typeof(int)))
                    return false;
            }
            return true;
        }
        protected override string GetDefaultValue()
        {
            if (IsHalfPrecision)
                return $"min16int2({Value.X}, {Value.Y})";
            else
                return $"int2({Value.X}, {Value.Y})";
        }
        public override void BuildStatements(NodePin pin, ref BuildCodeStatementsData data)
        {
            base.BuildStatements(pin, ref data);
            if (data.NodeGraph.PinHasLinker(InXY))
            {
                var pinNode = data.NodeGraph.GetOppositePinNode(InXY);
                var opPin = data.NodeGraph.GetOppositePin(InXY);
                pinNode.BuildStatements(opPin, ref data);

                var assignStatement = new TtAssignOperatorStatement()
                {
                    From = pinNode.GetExpression(opPin, ref data),
                    To = new TtVariableReferenceExpression(Name),
                };
                if (!data.CurrentStatements.Contains(assignStatement))
                    data.CurrentStatements.Add(assignStatement);
            }
            if (data.NodeGraph.PinHasLinker(InX))
            {
                var pinNode = data.NodeGraph.GetOppositePinNode(InX);
                var opPin = data.NodeGraph.GetOppositePin(InX);
                pinNode.BuildStatements(opPin, ref data);

                var assignStatement = new TtAssignOperatorStatement()
                {
                    From = pinNode.GetExpression(opPin, ref data),
                    To = new TtVariableReferenceExpression("x", new TtVariableReferenceExpression(Name)),
                };
                if (!data.CurrentStatements.Contains(assignStatement))
                    data.CurrentStatements.Add(assignStatement);
            }
            if (data.NodeGraph.PinHasLinker(InY))
            {
                var pinNode = data.NodeGraph.GetOppositePinNode(InY);
                var opPin = data.NodeGraph.GetOppositePin(InY);
                pinNode.BuildStatements(opPin, ref data);

                var assignStatement = new TtAssignOperatorStatement()
                {
                    From = pinNode.GetExpression(opPin, ref data),
                    To = new TtVariableReferenceExpression("y", new TtVariableReferenceExpression(Name)),
                };
                if (!data.CurrentStatements.Contains(assignStatement))
                    data.CurrentStatements.Add(assignStatement);
            }

            if (!data.MethodDec.HasLocalVariable(Name))
            {
                var val = new TtVariableDeclaration()
                {
                    VariableType = new TtTypeReference(Type2HLSLType(VarType, IsHalfPrecision)),
                    VariableName = Name,
                    InitValue = new TtPrimitiveExpression(Value),
                };
                OnAddLocalVar(val, ref data);
            }
        }
        public override TtExpressionBase GetExpression(NodePin pin, ref BuildCodeStatementsData data)
        {
            if (pin == OutXY)
                return new TtVariableReferenceExpression(Name);
            else if (pin == OutX)
                return new TtVariableReferenceExpression("x", new TtVariableReferenceExpression(Name));
            else if (pin == OutY)
                return new TtVariableReferenceExpression("y", new TtVariableReferenceExpression(Name));
            return null;
        }

        [Rtti.Meta]
        [Category("Option")]
        public Vector2i Value { get; set; } = Vector2i.Zero;
        [Browsable(false)]
        public PinIn InXY { get; set; } = new PinIn();
        [Browsable(false)]
        public PinIn InX { get; set; } = new PinIn();
        [Browsable(false)]
        public PinIn InY { get; set; } = new PinIn();
        [Browsable(false)]
        public PinOut OutXY { get; set; } = new PinOut();
        [Browsable(false)]
        public PinOut OutX { get; set; } = new PinOut();
        [Browsable(false)]
        public PinOut OutY { get; set; } = new PinOut();
        public VarDimI2()
        {
            VarType = Rtti.TtTypeDescGetter<Vector2i>.TypeDesc;

            //Name = $"{Value}";

            InXY.Name = "in ";
            InXY.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinIn(InXY);
            InX.Name = "x ";
            InX.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinIn(InX);
            InY.Name = "y ";
            InY.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinIn(InY);

            OutXY.Name = " xy";
            OutXY.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            OutXY.MultiLinks = true;
            this.AddPinOut(OutXY);
            OutX.Name = " x";
            OutX.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            OutX.MultiLinks = true;
            this.AddPinOut(OutX);
            OutY.Name = " y";
            OutY.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            OutY.MultiLinks = true;
            this.AddPinOut(OutY);
        }
    }
}
