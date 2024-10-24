﻿using System;
using System.Collections.Generic;
using EngineNS.Bricks.NodeGraph;
using System.ComponentModel;

namespace EngineNS.Bricks.CodeBuilder.ShaderNode.Var
{
    [ContextMenu("f4,float4", "Data\\float4@_serial@", TtMaterialGraph.MaterialEditorKeyword)]
    public class VarDimF4 : VarNode
    {
        public override Rtti.TtTypeDesc GetOutPinType(PinOut pin)
        {
            if (pin == OutXYZW)
                return Rtti.TtTypeDesc.TypeOf(typeof(Vector4));
            else if (pin == OutXYZ)
                return Rtti.TtTypeDesc.TypeOf(typeof(Vector3));
            else if (pin == OutX ||
                pin == OutY ||
                pin == OutZ ||
                pin == OutW)
                return Rtti.TtTypeDesc.TypeOf(typeof(float));
            return null;
        }
        public override bool CanLinkFrom(PinIn iPin, TtNodeBase OutNode, PinOut oPin)
        {
            if (base.CanLinkFrom(iPin, OutNode, oPin) == false)
                return false;

            var nodeExpr = OutNode as TtNodeBase;
            var type = nodeExpr.GetOutPinType(oPin);

            if (iPin == InXYZW)
            {
                if (!type.IsEqual(typeof(Vector4)))
                    return false;
            }
            else if (iPin == InX ||
                iPin == InY ||
                iPin == InZ ||
                iPin == InW)
            {
                if (!type.IsEqual(typeof(float)))
                    return false;
            }
            return true;
        }
        protected override string GetDefaultValue()
        {
            if (IsHalfPrecision)
                return $"half4({Value.X},{Value.Y},{Value.Z},{Value.W})";
            else
                return $"float4({Value.X},{Value.Y},{Value.Z},{Value.W})";
        }
        public override void BuildStatements(NodePin pin, ref BuildCodeStatementsData data)
        {
            base.BuildStatements(pin, ref data);
            if(data.NodeGraph.PinHasLinker(InXYZW))
            {
                var pinNode = data.NodeGraph.GetOppositePinNode(InXYZW);
                var opPin = data.NodeGraph.GetOppositePin(InXYZW);
                pinNode.BuildStatements(opPin, ref data);

                var assignStatment = new TtAssignOperatorStatement()
                {
                    From = pinNode.GetExpression(opPin, ref data),
                    To = new TtVariableReferenceExpression(Name),
                };
                if(!data.CurrentStatements.Contains(assignStatment))
                    data.CurrentStatements.Add(assignStatment);
            }
            if (data.NodeGraph.PinHasLinker(InX))
            {
                var pinNode = data.NodeGraph.GetOppositePinNode(InX);
                var opPin = data.NodeGraph.GetOppositePin(InX);
                pinNode.BuildStatements(opPin, ref data);

                var assignStatment = new TtAssignOperatorStatement()
                {
                    From = pinNode.GetExpression(opPin, ref data),
                    To = new TtVariableReferenceExpression("x", new TtVariableReferenceExpression(Name)),
                };
                if(!data.CurrentStatements.Contains(assignStatment))
                    data.CurrentStatements.Add(assignStatment);
            }
            if (data.NodeGraph.PinHasLinker(InY))
            {
                var pinNode = data.NodeGraph.GetOppositePinNode(InY);
                var opPin = data.NodeGraph.GetOppositePin(InY);
                pinNode.BuildStatements(opPin, ref data);

                var assignStatment = new TtAssignOperatorStatement()
                {
                    From = pinNode.GetExpression(opPin, ref data),
                    To = new TtVariableReferenceExpression("y", new TtVariableReferenceExpression(Name)),
                };
                if(!data.CurrentStatements.Contains(assignStatment))
                    data.CurrentStatements.Add(assignStatment);
            }
            if (data.NodeGraph.PinHasLinker(InZ))
            {
                var pinNode = data.NodeGraph.GetOppositePinNode(InZ);
                var opPin = data.NodeGraph.GetOppositePin(InZ);
                pinNode.BuildStatements(opPin, ref data);

                var assignStatment = new TtAssignOperatorStatement()
                {
                    From = pinNode.GetExpression(opPin, ref data),
                    To = new TtVariableReferenceExpression("z", new TtVariableReferenceExpression(Name)),
                };
                if(!data.CurrentStatements.Contains(assignStatment))
                    data.CurrentStatements.Add(assignStatment);
            }
            if (data.NodeGraph.PinHasLinker(InW))
            {
                var pinNode = data.NodeGraph.GetOppositePinNode(InW);
                var opPin = data.NodeGraph.GetOppositePin(InW);
                pinNode.BuildStatements(opPin, ref data);

                data.CurrentStatements.Add(new TtAssignOperatorStatement()
                {
                    From = pinNode.GetExpression(opPin, ref data),
                    To = new TtVariableReferenceExpression(Name),
                });
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
            if (pin == OutXYZW)
                return new TtVariableReferenceExpression(Name);
            else if (pin == OutXYZ)
                return new TtVariableReferenceExpression("xyz", new TtVariableReferenceExpression(Name));
            else if (pin == OutX)
                return new TtVariableReferenceExpression("x", new TtVariableReferenceExpression(Name));
            else if (pin == OutY)
                return new TtVariableReferenceExpression("y", new TtVariableReferenceExpression(Name));
            else if (pin == OutZ)
                return new TtVariableReferenceExpression("z", new TtVariableReferenceExpression(Name));
            else if (pin == OutW)
                return new TtVariableReferenceExpression("w", new TtVariableReferenceExpression(Name));
            return null;
        }
        protected Vector4 mValue = Vector4.Zero;
        [Rtti.Meta]
        [Category("Option")]
        public Vector4 Value { get => mValue; set => mValue = value; }
        [Browsable(false)]
        public PinIn InXYZW { get; set; } = new PinIn();
        [Browsable(false)]
        public PinIn InX { get; set; } = new PinIn();
        [Browsable(false)]
        public PinIn InY { get; set; } = new PinIn();
        [Browsable(false)]
        public PinIn InZ { get; set; } = new PinIn();
        [Browsable(false)]
        public PinIn InW { get; set; } = new PinIn();
        [Browsable(false)]
        public PinOut OutXYZW { get; set; } = new PinOut();
        [Browsable(false)]
        public PinOut OutXYZ { get; set; } = new PinOut();
        [Browsable(false)]
        public PinOut OutX { get; set; } = new PinOut();
        [Browsable(false)]
        public PinOut OutY { get; set; } = new PinOut();
        [Browsable(false)]
        public PinOut OutZ { get; set; } = new PinOut();
        [Browsable(false)]
        public PinOut OutW { get; set; } = new PinOut();
        public VarDimF4()
        {
            VarType = Rtti.TtTypeDescGetter<Vector4>.TypeDesc;

            //Name = $"{Value}";

            InXYZW.Name = "in ";
            InXYZW.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinIn(InXYZW);
            InX.Name = "x ";
            InX.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinIn(InX);
            InY.Name = "y ";
            InY.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinIn(InY);
            InZ.Name = "z ";
            InZ.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinIn(InZ);
            InW.Name = "w ";
            InW.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinIn(InW);

            OutXYZW.Name = " xyzw";
            OutXYZW.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            OutXYZW.MultiLinks = true;
            this.AddPinOut(OutXYZW);
            OutXYZ.Name = " xyz";
            OutXYZ.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            OutXYZ.MultiLinks = true;
            this.AddPinOut(OutXYZ);
            OutX.Name = " x";
            OutX.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            OutX.MultiLinks = true;
            this.AddPinOut(OutX);
            OutY.Name = " y";
            OutY.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            OutY.MultiLinks = true;
            this.AddPinOut(OutY);
            OutZ.Name = " z";
            OutZ.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            OutZ.MultiLinks = true;
            this.AddPinOut(OutZ);
            OutW.Name = " w";
            OutW.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            OutW.MultiLinks = true;
            this.AddPinOut(OutW);
        }
    }
    [ContextMenu("color4", "Data\\color4@_serial@", TtMaterialGraph.MaterialEditorKeyword)]
    public class VarColor4 : VarDimF4
    {
        public VarColor4()
        {
            PrevSize = new Vector2(100, 100);
            OutXYZ.Name = " xyz";
            OutXYZ.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            OutXYZ.MultiLinks = true;
            this.AddPinOut(OutXYZ);
        }
        [EGui.Controls.PropertyGrid.Color4PickerEditor()]
        [Category("Option")]
        public Vector4 Color { get => mValue; set => mValue = value; }
        public override Rtti.TtTypeDesc GetOutPinType(PinOut pin)
        {
            if (pin == OutXYZW)
                return Rtti.TtTypeDesc.TypeOf(typeof(Vector4));
            else if (pin == OutXYZ)
                return Rtti.TtTypeDesc.TypeOf(typeof(Vector3));
            else if (pin == OutX ||
                pin == OutY ||
                pin == OutZ ||
                pin == OutW)
                return Rtti.TtTypeDesc.TypeOf(typeof(float));
            return null;
        }
        public unsafe override void OnPreviewDraw(in Vector2 prevStart, in Vector2 prevEnd, ImDrawList cmdlist)
        {
            var color = new Color4f(mValue.W, mValue.X, mValue.Y, mValue.Z);
            fixed (Vector2* p_prevStart = &prevStart)
            fixed (Vector2* p_prevEnd = &prevEnd)
            {
                cmdlist.AddRectFilled(p_prevStart, p_prevEnd, color.ToBgr(), 0, ImDrawFlags_.ImDrawFlags_RoundCornersAll);
            }
        }
        public override void OnLButtonClicked(NodePin clickedPin)
        {
            base.OnLButtonClicked(clickedPin);

            //Graph.ShaderEditor.NodePropGrid.HideInheritDeclareType = Rtti.UTypeDescGetter<VarDimF4>.TypeDesc;
        }
        public override TtExpressionBase GetExpression(NodePin pin, ref BuildCodeStatementsData data)
        {
            if (pin == OutXYZ)
                return new TtVariableReferenceExpression("xyz", new TtVariableReferenceExpression(Name));
            else
                return base.GetExpression(pin, ref data);
        }
    }

    [ContextMenu("i4,int4", "Data\\int4@_serial@", TtMaterialGraph.MaterialEditorKeyword)]
    public class VarDimI4 : VarNode
    {
        public override Rtti.TtTypeDesc GetOutPinType(PinOut pin)
        {
            if (pin == OutXYZW)
                return Rtti.TtTypeDesc.TypeOf(typeof(Vector4i));
            else if (pin == OutXYZ)
                return Rtti.TtTypeDesc.TypeOf(typeof(Vector3i));
            else if (pin == OutX ||
                pin == OutY ||
                pin == OutZ ||
                pin == OutW)
                return Rtti.TtTypeDesc.TypeOf(typeof(int));
            return null;
        }
        public override bool CanLinkFrom(PinIn iPin, TtNodeBase OutNode, PinOut oPin)
        {
            if (base.CanLinkFrom(iPin, OutNode, oPin) == false)
                return false;

            var nodeExpr = OutNode as TtNodeBase;
            var type = nodeExpr.GetOutPinType(oPin);

            if (iPin == InXYZW)
            {
                if (!type.IsEqual(typeof(Vector4)))
                    return false;
            }
            else if (iPin == InX ||
                iPin == InY ||
                iPin == InZ ||
                iPin == InW)
            {
                if (!type.IsEqual(typeof(float)))
                    return false;
            }
            return true;
        }
        protected override string GetDefaultValue()
        {
            if (IsHalfPrecision)
                return $"half4({Value.X},{Value.Y},{Value.Z},{Value.W})";
            else
                return $"float4({Value.X},{Value.Y},{Value.Z},{Value.W})";
        }
        public override void BuildStatements(NodePin pin, ref BuildCodeStatementsData data)
        {
            base.BuildStatements(pin, ref data);
            if (data.NodeGraph.PinHasLinker(InXYZW))
            {
                var pinNode = data.NodeGraph.GetOppositePinNode(InXYZW);
                var opPin = data.NodeGraph.GetOppositePin(InXYZW);
                pinNode.BuildStatements(opPin, ref data);

                var assignStatment = new TtAssignOperatorStatement()
                {
                    From = pinNode.GetExpression(opPin, ref data),
                    To = new TtVariableReferenceExpression(Name),
                };
                if (!data.CurrentStatements.Contains(assignStatment))
                    data.CurrentStatements.Add(assignStatment);
            }
            if (data.NodeGraph.PinHasLinker(InX))
            {
                var pinNode = data.NodeGraph.GetOppositePinNode(InX);
                var opPin = data.NodeGraph.GetOppositePin(InX);
                pinNode.BuildStatements(opPin, ref data);

                var assignStatment = new TtAssignOperatorStatement()
                {
                    From = pinNode.GetExpression(opPin, ref data),
                    To = new TtVariableReferenceExpression("x", new TtVariableReferenceExpression(Name)),
                };
                if (!data.CurrentStatements.Contains(assignStatment))
                    data.CurrentStatements.Add(assignStatment);
            }
            if (data.NodeGraph.PinHasLinker(InY))
            {
                var pinNode = data.NodeGraph.GetOppositePinNode(InY);
                var opPin = data.NodeGraph.GetOppositePin(InY);
                pinNode.BuildStatements(opPin, ref data);

                var assignStatment = new TtAssignOperatorStatement()
                {
                    From = pinNode.GetExpression(opPin, ref data),
                    To = new TtVariableReferenceExpression("y", new TtVariableReferenceExpression(Name)),
                };
                if (!data.CurrentStatements.Contains(assignStatment))
                    data.CurrentStatements.Add(assignStatment);
            }
            if (data.NodeGraph.PinHasLinker(InZ))
            {
                var pinNode = data.NodeGraph.GetOppositePinNode(InZ);
                var opPin = data.NodeGraph.GetOppositePin(InZ);
                pinNode.BuildStatements(opPin, ref data);

                var assignStatment = new TtAssignOperatorStatement()
                {
                    From = pinNode.GetExpression(opPin, ref data),
                    To = new TtVariableReferenceExpression("z", new TtVariableReferenceExpression(Name)),
                };
                if (!data.CurrentStatements.Contains(assignStatment))
                    data.CurrentStatements.Add(assignStatment);
            }
            if (data.NodeGraph.PinHasLinker(InW))
            {
                var pinNode = data.NodeGraph.GetOppositePinNode(InW);
                var opPin = data.NodeGraph.GetOppositePin(InW);
                pinNode.BuildStatements(opPin, ref data);

                data.CurrentStatements.Add(new TtAssignOperatorStatement()
                {
                    From = pinNode.GetExpression(opPin, ref data),
                    To = new TtVariableReferenceExpression(Name),
                });
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
            if (pin == OutXYZW)
                return new TtVariableReferenceExpression(Name);
            else if (pin == OutXYZ)
                return new TtVariableReferenceExpression("xyz", new TtVariableReferenceExpression(Name));
            else if (pin == OutX)
                return new TtVariableReferenceExpression("x", new TtVariableReferenceExpression(Name));
            else if (pin == OutY)
                return new TtVariableReferenceExpression("y", new TtVariableReferenceExpression(Name));
            else if (pin == OutZ)
                return new TtVariableReferenceExpression("z", new TtVariableReferenceExpression(Name));
            else if (pin == OutW)
                return new TtVariableReferenceExpression("w", new TtVariableReferenceExpression(Name));
            return null;
        }
        protected Vector4i mValue = Vector4i.Zero;
        [Rtti.Meta]
        [Category("Option")]
        public Vector4i Value { get => mValue; set => mValue = value; }
        [Browsable(false)]
        public PinIn InXYZW { get; set; } = new PinIn();
        [Browsable(false)]
        public PinIn InX { get; set; } = new PinIn();
        [Browsable(false)]
        public PinIn InY { get; set; } = new PinIn();
        [Browsable(false)]
        public PinIn InZ { get; set; } = new PinIn();
        [Browsable(false)]
        public PinIn InW { get; set; } = new PinIn();
        [Browsable(false)]
        public PinOut OutXYZW { get; set; } = new PinOut();
        [Browsable(false)]
        public PinOut OutXYZ { get; set; } = new PinOut();
        [Browsable(false)]
        public PinOut OutX { get; set; } = new PinOut();
        [Browsable(false)]
        public PinOut OutY { get; set; } = new PinOut();
        [Browsable(false)]
        public PinOut OutZ { get; set; } = new PinOut();
        [Browsable(false)]
        public PinOut OutW { get; set; } = new PinOut();
        public VarDimI4()
        {
            VarType = Rtti.TtTypeDescGetter<Vector4i>.TypeDesc;

            //Name = $"{Value}";

            InXYZW.Name = "in ";
            InXYZW.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinIn(InXYZW);
            InX.Name = "x ";
            InX.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinIn(InX);
            InY.Name = "y ";
            InY.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinIn(InY);
            InZ.Name = "z ";
            InZ.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinIn(InZ);
            InW.Name = "w ";
            InW.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinIn(InW);

            OutXYZW.Name = " xyzw";
            OutXYZW.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            OutXYZW.MultiLinks = true;
            this.AddPinOut(OutXYZW);
            OutXYZ.Name = " xyz";
            OutXYZ.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            OutXYZ.MultiLinks = true;
            this.AddPinOut(OutXYZ);
            OutX.Name = " x";
            OutX.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            OutX.MultiLinks = true;
            this.AddPinOut(OutX);
            OutY.Name = " y";
            OutY.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            OutY.MultiLinks = true;
            this.AddPinOut(OutY);
            OutZ.Name = " z";
            OutZ.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            OutZ.MultiLinks = true;
            this.AddPinOut(OutZ);
            OutW.Name = " w";
            OutW.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            OutW.MultiLinks = true;
            this.AddPinOut(OutW);
        }
    }
}
