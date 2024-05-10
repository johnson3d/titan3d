using System;
using System.Collections.Generic;
using EngineNS.Bricks.NodeGraph;
using System.ComponentModel;

namespace EngineNS.Bricks.CodeBuilder.ShaderNode.Var
{
    [ContextMenu("f4,float4", "Data\\float4@_serial@", UMaterialGraph.MaterialEditorKeyword)]
    public class VarDimF4 : VarNode
    {
        public override Rtti.UTypeDesc GetOutPinType(PinOut pin)
        {
            if (pin == OutXYZW)
                return Rtti.UTypeDesc.TypeOf(typeof(Vector4));
            else if (pin == OutXYZ)
                return Rtti.UTypeDesc.TypeOf(typeof(Vector3));
            else if (pin == OutX ||
                pin == OutY ||
                pin == OutZ ||
                pin == OutW)
                return Rtti.UTypeDesc.TypeOf(typeof(float));
            return null;
        }
        public override bool CanLinkFrom(PinIn iPin, UNodeBase OutNode, PinOut oPin)
        {
            if (base.CanLinkFrom(iPin, OutNode, oPin) == false)
                return false;

            var nodeExpr = OutNode as UNodeBase;
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
        //public override void PreGenExpr()
        //{
        //    Executed = false;
        //}
        //bool Executed = false;
        //public override IExpression GetVarExpr(UMaterialGraph funGraph, ICodeGen cGen, PinOut oPin, bool bTakeResult)
        //{
        //    DefineVar Var = new DefineVar();
        //    Var.IsLocalVar = !IsUniform;
        //    Var.VarName = this.Name;
        //    Var.DefType = Type2HLSLType(VarType.SystemType, IsHalfPrecision);

        //    Var.InitValue = GetDefaultValue();

        //    if (Var.IsLocalVar)
        //    {
        //        funGraph.ShaderEditor.MaterialOutput.Function.AddLocalVar(Var);
        //    }
        //    else
        //    {
        //        funGraph.ShaderEditor.MaterialOutput.UniformVars.Add(Var);
        //    }

        //    if (Executed)
        //    {
        //        return new OpUseDefinedVar(Var);
        //    }
        //    Executed = true;

        //    var seq = new ExecuteSequence();
        //    var linkXYZW = funGraph.FindInLinkerSingle(InXYZW);
        //    if (linkXYZW != null)
        //    {
        //        var exprNode = linkXYZW.OutNode as IBaseNode;
        //        var xyExpr = exprNode.GetExpr(funGraph, cGen, linkXYZW.OutPin, true) as OpExpress;
        //        var setExpr = new AssignOp();
        //        setExpr.Left = new OpUseDefinedVar(Var);
        //        setExpr.Right = xyExpr;

        //        seq.Lines.Add(setExpr);
        //    }
        //    var linkX = funGraph.FindInLinkerSingle(InX);
        //    if (linkX != null)
        //    {
        //        var exprNode = linkX.OutNode as IBaseNode;
        //        var xExpr = exprNode.GetExpr(funGraph, cGen, linkX.OutPin, true) as OpExpress;
        //        var setExpr = new AssignOp();
        //        var member = new BinocularOp(EBinocularOp.GetMember);
        //        member.Left = new OpUseDefinedVar(Var);
        //        member.Right = new HardCodeOp() { Code = "x" };
        //        setExpr.Left = member;
        //        setExpr.Right = xExpr;

        //        seq.Lines.Add(setExpr);
        //    }
        //    var linkY = funGraph.FindInLinkerSingle(InY);
        //    if (linkY != null)
        //    {
        //        var exprNode = linkY.OutNode as IBaseNode;
        //        var yExpr = exprNode.GetExpr(funGraph, cGen, linkY.OutPin, true) as OpExpress;
        //        var setExpr = new AssignOp();
        //        var member = new BinocularOp(EBinocularOp.GetMember);
        //        member.Left = new OpUseDefinedVar(Var);
        //        member.Right = new HardCodeOp() { Code = "y" };
        //        setExpr.Left = member;
        //        setExpr.Right = yExpr;

        //        seq.Lines.Add(setExpr);
        //    }
        //    var linkZ = funGraph.FindInLinkerSingle(InZ);
        //    if (linkZ != null)
        //    {
        //        var exprNode = linkZ.OutNode as IBaseNode;
        //        var yExpr = exprNode.GetExpr(funGraph, cGen, linkZ.OutPin, true) as OpExpress;
        //        var setExpr = new AssignOp();
        //        var member = new BinocularOp(EBinocularOp.GetMember);
        //        member.Left = new OpUseDefinedVar(Var);
        //        member.Right = new HardCodeOp() { Code = "z" };
        //        setExpr.Left = member;
        //        setExpr.Right = yExpr;

        //        seq.Lines.Add(setExpr);
        //    }
        //    var linkW = funGraph.FindInLinkerSingle(InW);
        //    if (linkW != null)
        //    {
        //        var exprNode = linkW.OutNode as IBaseNode;
        //        var yExpr = exprNode.GetExpr(funGraph, cGen, linkW.OutPin, true) as OpExpress;
        //        var setExpr = new AssignOp();
        //        var member = new BinocularOp(EBinocularOp.GetMember);
        //        member.Left = new OpUseDefinedVar(Var);
        //        member.Right = new HardCodeOp() { Code = "w" };
        //        setExpr.Left = member;
        //        setExpr.Right = yExpr;

        //        seq.Lines.Add(setExpr);
        //    }
        //    return new OpExecuteAndUseDefinedVar(seq, Var);
        //}
        //public override IExpression GetExpr(UMaterialGraph funGraph, ICodeGen cGen, PinOut oPin, bool bTakeResult)
        //{
        //    var expr = GetVarExpr(funGraph, cGen, oPin, bTakeResult);
        //    if (oPin == OutXYZW)
        //    {
        //        var dotOp = new BinocularOp(EBinocularOp.GetMember);
        //        dotOp.Left = expr as OpExpress;
        //        var member = new HardCodeOp();
        //        member.Code = "xyzw";
        //        dotOp.Right = member;
        //        return dotOp;
        //    }
        //    else if (oPin == OutX)
        //    {
        //        var dotOp = new BinocularOp(EBinocularOp.GetMember);
        //        dotOp.Left = expr as OpExpress;
        //        var member = new HardCodeOp();
        //        member.Code = "x";
        //        dotOp.Right = member;
        //        return dotOp;
        //    }
        //    else if (oPin == OutY)
        //    {
        //        var dotOp = new BinocularOp(EBinocularOp.GetMember);
        //        dotOp.Left = expr as OpExpress;
        //        var member = new HardCodeOp();
        //        member.Code = "y";
        //        dotOp.Right = member;
        //        return dotOp;
        //    }
        //    else if (oPin == OutZ)
        //    {
        //        var dotOp = new BinocularOp(EBinocularOp.GetMember);
        //        dotOp.Left = expr as OpExpress;
        //        var member = new HardCodeOp();
        //        member.Code = "z";
        //        dotOp.Right = member;
        //        return dotOp;
        //    }
        //    else if (oPin == OutW)
        //    {
        //        var dotOp = new BinocularOp(EBinocularOp.GetMember);
        //        dotOp.Left = expr as OpExpress;
        //        var member = new HardCodeOp();
        //        member.Code = "w";
        //        dotOp.Right = member;
        //        return dotOp;
        //    }
        //    return null;
        //}
        public override void BuildStatements(NodePin pin, ref BuildCodeStatementsData data)
        {
            base.BuildStatements(pin, ref data);
            if(data.NodeGraph.PinHasLinker(InXYZW))
            {
                var pinNode = data.NodeGraph.GetOppositePinNode(InXYZW);
                var opPin = data.NodeGraph.GetOppositePin(InXYZW);
                pinNode.BuildStatements(opPin, ref data);

                var assignStatment = new UAssignOperatorStatement()
                {
                    From = pinNode.GetExpression(opPin, ref data),
                    To = new UVariableReferenceExpression(Name),
                };
                if(!data.CurrentStatements.Contains(assignStatment))
                    data.CurrentStatements.Add(assignStatment);
            }
            if (data.NodeGraph.PinHasLinker(InX))
            {
                var pinNode = data.NodeGraph.GetOppositePinNode(InX);
                var opPin = data.NodeGraph.GetOppositePin(InX);
                pinNode.BuildStatements(opPin, ref data);

                var assignStatment = new UAssignOperatorStatement()
                {
                    From = pinNode.GetExpression(opPin, ref data),
                    To = new UVariableReferenceExpression("x", new UVariableReferenceExpression(Name)),
                };
                if(!data.CurrentStatements.Contains(assignStatment))
                    data.CurrentStatements.Add(assignStatment);
            }
            if (data.NodeGraph.PinHasLinker(InY))
            {
                var pinNode = data.NodeGraph.GetOppositePinNode(InY);
                var opPin = data.NodeGraph.GetOppositePin(InY);
                pinNode.BuildStatements(opPin, ref data);

                var assignStatment = new UAssignOperatorStatement()
                {
                    From = pinNode.GetExpression(opPin, ref data),
                    To = new UVariableReferenceExpression("y", new UVariableReferenceExpression(Name)),
                };
                if(!data.CurrentStatements.Contains(assignStatment))
                    data.CurrentStatements.Add(assignStatment);
            }
            if (data.NodeGraph.PinHasLinker(InZ))
            {
                var pinNode = data.NodeGraph.GetOppositePinNode(InZ);
                var opPin = data.NodeGraph.GetOppositePin(InZ);
                pinNode.BuildStatements(opPin, ref data);

                var assignStatment = new UAssignOperatorStatement()
                {
                    From = pinNode.GetExpression(opPin, ref data),
                    To = new UVariableReferenceExpression("z", new UVariableReferenceExpression(Name)),
                };
                if(!data.CurrentStatements.Contains(assignStatment))
                    data.CurrentStatements.Add(assignStatment);
            }
            if (data.NodeGraph.PinHasLinker(InW))
            {
                var pinNode = data.NodeGraph.GetOppositePinNode(InW);
                var opPin = data.NodeGraph.GetOppositePin(InW);
                pinNode.BuildStatements(opPin, ref data);

                data.CurrentStatements.Add(new UAssignOperatorStatement()
                {
                    From = pinNode.GetExpression(opPin, ref data),
                    To = new UVariableReferenceExpression(Name),
                });
            }

            if (!data.MethodDec.HasLocalVariable(Name))
            {
                var val = new UVariableDeclaration()
                {
                    VariableType = new UTypeReference(Type2HLSLType(VarType, IsHalfPrecision)),
                    VariableName = Name,
                    InitValue = new UPrimitiveExpression(Value),
                };
                if (IsUniform)
                {
                    var graph = data.NodeGraph as UMaterialGraph;
                    graph.ShaderEditor.MaterialOutput.UniformVars.Add(val);
                }
                else
                    data.MethodDec.AddLocalVar(val);
            }
        }
        public override UExpressionBase GetExpression(NodePin pin, ref BuildCodeStatementsData data)
        {
            if (pin == OutXYZW)
                return new UVariableReferenceExpression(Name);
            else if (pin == OutXYZ)
                return new UVariableReferenceExpression("xyz", new UVariableReferenceExpression(Name));
            else if (pin == OutX)
                return new UVariableReferenceExpression("x", new UVariableReferenceExpression(Name));
            else if (pin == OutY)
                return new UVariableReferenceExpression("y", new UVariableReferenceExpression(Name));
            else if (pin == OutZ)
                return new UVariableReferenceExpression("z", new UVariableReferenceExpression(Name));
            else if (pin == OutW)
                return new UVariableReferenceExpression("w", new UVariableReferenceExpression(Name));
            return null;
        }
        protected Vector4 mValue = Vector4.Zero;
        [Rtti.Meta]
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
            VarType = Rtti.UTypeDescGetter<Vector4>.TypeDesc;

            //Name = $"{Value}";

            InXYZW.Name = "in ";
            InXYZW.LinkDesc = UShaderEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinIn(InXYZW);
            InX.Name = "x ";
            InX.LinkDesc = UShaderEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinIn(InX);
            InY.Name = "y ";
            InY.LinkDesc = UShaderEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinIn(InY);
            InZ.Name = "z ";
            InZ.LinkDesc = UShaderEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinIn(InZ);
            InW.Name = "w ";
            InW.LinkDesc = UShaderEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinIn(InW);

            OutXYZW.Name = " xyzw";
            OutXYZW.LinkDesc = UShaderEditorStyles.Instance.NewInOutPinDesc();
            OutXYZW.MultiLinks = true;
            this.AddPinOut(OutXYZW);
            OutXYZ.Name = " xyz";
            OutXYZ.LinkDesc = UShaderEditorStyles.Instance.NewInOutPinDesc();
            OutXYZ.MultiLinks = true;
            this.AddPinOut(OutXYZ);
            OutX.Name = " x";
            OutX.LinkDesc = UShaderEditorStyles.Instance.NewInOutPinDesc();
            OutX.MultiLinks = true;
            this.AddPinOut(OutX);
            OutY.Name = " y";
            OutY.LinkDesc = UShaderEditorStyles.Instance.NewInOutPinDesc();
            OutY.MultiLinks = true;
            this.AddPinOut(OutY);
            OutZ.Name = " z";
            OutZ.LinkDesc = UShaderEditorStyles.Instance.NewInOutPinDesc();
            OutZ.MultiLinks = true;
            this.AddPinOut(OutZ);
            OutW.Name = " w";
            OutW.LinkDesc = UShaderEditorStyles.Instance.NewInOutPinDesc();
            OutW.MultiLinks = true;
            this.AddPinOut(OutW);
        }
    }
    [ContextMenu("color4", "Data\\color4@_serial@", UMaterialGraph.MaterialEditorKeyword)]
    public class VarColor4 : VarDimF4
    {
        public VarColor4()
        {
            PrevSize = new Vector2(100, 100);
            OutXYZ.Name = " xyz";
            OutXYZ.LinkDesc = UShaderEditorStyles.Instance.NewInOutPinDesc();
            OutXYZ.MultiLinks = true;
            this.AddPinOut(OutXYZ);
        }
        [EGui.Controls.PropertyGrid.Color4PickerEditor()]
        public Vector4 Color { get => mValue; set => mValue = value; }
        public override Rtti.UTypeDesc GetOutPinType(PinOut pin)
        {
            if (pin == OutXYZW)
                return Rtti.UTypeDesc.TypeOf(typeof(Vector4));
            else if (pin == OutXYZ)
                return Rtti.UTypeDesc.TypeOf(typeof(Vector3));
            else if (pin == OutX ||
                pin == OutY ||
                pin == OutZ ||
                pin == OutW)
                return Rtti.UTypeDesc.TypeOf(typeof(float));
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
        public override UExpressionBase GetExpression(NodePin pin, ref BuildCodeStatementsData data)
        {
            if (pin == OutXYZ)
                return new UVariableReferenceExpression("xyz", new UVariableReferenceExpression(Name));
            else
                return base.GetExpression(pin, ref data);
        }
    }

    [ContextMenu("i4,int4", "Data\\int4@_serial@", UMaterialGraph.MaterialEditorKeyword)]
    public class VarDimI4 : VarNode
    {
        public override Rtti.UTypeDesc GetOutPinType(PinOut pin)
        {
            if (pin == OutXYZW)
                return Rtti.UTypeDesc.TypeOf(typeof(Vector4i));
            else if (pin == OutXYZ)
                return Rtti.UTypeDesc.TypeOf(typeof(Vector3i));
            else if (pin == OutX ||
                pin == OutY ||
                pin == OutZ ||
                pin == OutW)
                return Rtti.UTypeDesc.TypeOf(typeof(int));
            return null;
        }
        public override bool CanLinkFrom(PinIn iPin, UNodeBase OutNode, PinOut oPin)
        {
            if (base.CanLinkFrom(iPin, OutNode, oPin) == false)
                return false;

            var nodeExpr = OutNode as UNodeBase;
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

                var assignStatment = new UAssignOperatorStatement()
                {
                    From = pinNode.GetExpression(opPin, ref data),
                    To = new UVariableReferenceExpression(Name),
                };
                if (!data.CurrentStatements.Contains(assignStatment))
                    data.CurrentStatements.Add(assignStatment);
            }
            if (data.NodeGraph.PinHasLinker(InX))
            {
                var pinNode = data.NodeGraph.GetOppositePinNode(InX);
                var opPin = data.NodeGraph.GetOppositePin(InX);
                pinNode.BuildStatements(opPin, ref data);

                var assignStatment = new UAssignOperatorStatement()
                {
                    From = pinNode.GetExpression(opPin, ref data),
                    To = new UVariableReferenceExpression("x", new UVariableReferenceExpression(Name)),
                };
                if (!data.CurrentStatements.Contains(assignStatment))
                    data.CurrentStatements.Add(assignStatment);
            }
            if (data.NodeGraph.PinHasLinker(InY))
            {
                var pinNode = data.NodeGraph.GetOppositePinNode(InY);
                var opPin = data.NodeGraph.GetOppositePin(InY);
                pinNode.BuildStatements(opPin, ref data);

                var assignStatment = new UAssignOperatorStatement()
                {
                    From = pinNode.GetExpression(opPin, ref data),
                    To = new UVariableReferenceExpression("y", new UVariableReferenceExpression(Name)),
                };
                if (!data.CurrentStatements.Contains(assignStatment))
                    data.CurrentStatements.Add(assignStatment);
            }
            if (data.NodeGraph.PinHasLinker(InZ))
            {
                var pinNode = data.NodeGraph.GetOppositePinNode(InZ);
                var opPin = data.NodeGraph.GetOppositePin(InZ);
                pinNode.BuildStatements(opPin, ref data);

                var assignStatment = new UAssignOperatorStatement()
                {
                    From = pinNode.GetExpression(opPin, ref data),
                    To = new UVariableReferenceExpression("z", new UVariableReferenceExpression(Name)),
                };
                if (!data.CurrentStatements.Contains(assignStatment))
                    data.CurrentStatements.Add(assignStatment);
            }
            if (data.NodeGraph.PinHasLinker(InW))
            {
                var pinNode = data.NodeGraph.GetOppositePinNode(InW);
                var opPin = data.NodeGraph.GetOppositePin(InW);
                pinNode.BuildStatements(opPin, ref data);

                data.CurrentStatements.Add(new UAssignOperatorStatement()
                {
                    From = pinNode.GetExpression(opPin, ref data),
                    To = new UVariableReferenceExpression(Name),
                });
            }

            if (!data.MethodDec.HasLocalVariable(Name))
            {
                var val = new UVariableDeclaration()
                {
                    VariableType = new UTypeReference(Type2HLSLType(VarType, IsHalfPrecision)),
                    VariableName = Name,
                    InitValue = new UPrimitiveExpression(Value),
                };
                if (IsUniform)
                {
                    var graph = data.NodeGraph as UMaterialGraph;
                    graph.ShaderEditor.MaterialOutput.UniformVars.Add(val);
                }
                else
                    data.MethodDec.AddLocalVar(val);
            }
        }
        public override UExpressionBase GetExpression(NodePin pin, ref BuildCodeStatementsData data)
        {
            if (pin == OutXYZW)
                return new UVariableReferenceExpression(Name);
            else if (pin == OutXYZ)
                return new UVariableReferenceExpression("xyz", new UVariableReferenceExpression(Name));
            else if (pin == OutX)
                return new UVariableReferenceExpression("x", new UVariableReferenceExpression(Name));
            else if (pin == OutY)
                return new UVariableReferenceExpression("y", new UVariableReferenceExpression(Name));
            else if (pin == OutZ)
                return new UVariableReferenceExpression("z", new UVariableReferenceExpression(Name));
            else if (pin == OutW)
                return new UVariableReferenceExpression("w", new UVariableReferenceExpression(Name));
            return null;
        }
        protected Vector4i mValue = Vector4i.Zero;
        [Rtti.Meta]
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
            VarType = Rtti.UTypeDescGetter<Vector4i>.TypeDesc;

            //Name = $"{Value}";

            InXYZW.Name = "in ";
            InXYZW.LinkDesc = UShaderEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinIn(InXYZW);
            InX.Name = "x ";
            InX.LinkDesc = UShaderEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinIn(InX);
            InY.Name = "y ";
            InY.LinkDesc = UShaderEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinIn(InY);
            InZ.Name = "z ";
            InZ.LinkDesc = UShaderEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinIn(InZ);
            InW.Name = "w ";
            InW.LinkDesc = UShaderEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinIn(InW);

            OutXYZW.Name = " xyzw";
            OutXYZW.LinkDesc = UShaderEditorStyles.Instance.NewInOutPinDesc();
            OutXYZW.MultiLinks = true;
            this.AddPinOut(OutXYZW);
            OutXYZ.Name = " xyz";
            OutXYZ.LinkDesc = UShaderEditorStyles.Instance.NewInOutPinDesc();
            OutXYZ.MultiLinks = true;
            this.AddPinOut(OutXYZ);
            OutX.Name = " x";
            OutX.LinkDesc = UShaderEditorStyles.Instance.NewInOutPinDesc();
            OutX.MultiLinks = true;
            this.AddPinOut(OutX);
            OutY.Name = " y";
            OutY.LinkDesc = UShaderEditorStyles.Instance.NewInOutPinDesc();
            OutY.MultiLinks = true;
            this.AddPinOut(OutY);
            OutZ.Name = " z";
            OutZ.LinkDesc = UShaderEditorStyles.Instance.NewInOutPinDesc();
            OutZ.MultiLinks = true;
            this.AddPinOut(OutZ);
            OutW.Name = " w";
            OutW.LinkDesc = UShaderEditorStyles.Instance.NewInOutPinDesc();
            OutW.MultiLinks = true;
            this.AddPinOut(OutW);
        }
    }
}
