using System;
using System.Collections.Generic;
using EngineNS.Bricks.NodeGraph;
using System.ComponentModel;

namespace EngineNS.Bricks.CodeBuilder.ShaderNode.Var
{
    [ContextMenu("f3,float3", "Data\\float3@_serial@", TtMaterialGraph.MaterialEditorKeyword)]
    public class VarDimF3 : VarNode
    {
        public override Rtti.TtTypeDesc GetOutPinType(PinOut pin)
        {
            if (pin == OutXYZ)
                return Rtti.TtTypeDesc.TypeOf(typeof(Vector3));
            else if (pin == OutX ||
                pin == OutY ||
                pin == OutZ)
                return Rtti.TtTypeDesc.TypeOf(typeof(float));
            return null;
        }
        public override bool CanLinkFrom(PinIn iPin, TtNodeBase OutNode, PinOut oPin)
        {
            if (base.CanLinkFrom(iPin, OutNode, oPin) == false)
                return false;

            var nodeExpr = OutNode as TtNodeBase;
            var type = nodeExpr.GetOutPinType(oPin);

            if (iPin == InXYZ)
            {
                if (!type.IsEqual(typeof(Vector3)))
                    return false;
            }
            else if (iPin == InX ||
                iPin == InY ||
                iPin == InZ)
            {
                if (!type.IsEqual(typeof(float)))
                    return false;
            }
            return true;
        }
        protected override string GetDefaultValue()
        {
            if (IsHalfPrecision)
                return $"half3({Value.X}, {Value.Y}, {Value.Z})";
            else
                return $"float3({Value.X}, {Value.Y}, {Value.Z})";
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
        //    var linkXYZ = funGraph.FindInLinkerSingle(InXYZ);
        //    if (linkXYZ != null)
        //    {
        //        var exprNode = linkXYZ.OutNode as IBaseNode;
        //        var xyExpr = exprNode.GetExpr(funGraph, cGen, linkXYZ.OutPin, true) as OpExpress;
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
        //    return new OpExecuteAndUseDefinedVar(seq, Var);
        //}
        //public override IExpression GetExpr(UMaterialGraph funGraph, ICodeGen cGen, PinOut oPin, bool bTakeResult)
        //{
        //    var expr = GetVarExpr(funGraph, cGen, oPin, bTakeResult);
        //    if (oPin == OutXYZ)
        //    {
        //        var dotOp = new BinocularOp(EBinocularOp.GetMember);
        //        dotOp.Left = expr as OpExpress;
        //        var member = new HardCodeOp();
        //        member.Code = "xyz";
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
        //    return null;
        //}
        public override void BuildStatements(NodePin pin, ref BuildCodeStatementsData data)
        {
            base.BuildStatements(pin, ref data);
            if (data.NodeGraph.PinHasLinker(InXYZ))
            {
                var pinNode = data.NodeGraph.GetOppositePinNode(InXYZ);
                var opPin = data.NodeGraph.GetOppositePin(InXYZ);
                pinNode.BuildStatements(opPin, ref data);

                var assignStatement = new TtAssignOperatorStatement()
                {
                    From = pinNode.GetExpression(opPin, ref data),
                    To = new TtVariableReferenceExpression(Name),
                };
                if(!data.CurrentStatements.Contains(assignStatement))
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
                if(!data.CurrentStatements.Contains(assignStatement))
                    data.CurrentStatements.Add(assignStatement);
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
            if (pin == OutXYZ)
                return new TtVariableReferenceExpression(Name);
            else if (pin == OutX)
                return new TtVariableReferenceExpression("x", new TtVariableReferenceExpression(Name));
            else if (pin == OutY)
                return new TtVariableReferenceExpression("y", new TtVariableReferenceExpression(Name));
            else if (pin == OutZ)
                return new TtVariableReferenceExpression("z", new TtVariableReferenceExpression(Name));
            return null;
        }

        protected Vector3 mValue = Vector3.Zero;
        [Rtti.Meta]
        public Vector3 Value { get => mValue; set => mValue = value; }
        [Browsable(false)]
        public PinIn InXYZ { get; set; } = new PinIn();
        [Browsable(false)]
        public PinIn InX { get; set; } = new PinIn();
        [Browsable(false)]
        public PinIn InY { get; set; } = new PinIn();
        [Browsable(false)]
        public PinIn InZ { get; set; } = new PinIn();
        [Browsable(false)]
        public PinOut OutXYZ { get; set; } = new PinOut();
        [Browsable(false)]
        public PinOut OutX { get; set; } = new PinOut();
        [Browsable(false)]
        public PinOut OutY { get; set; } = new PinOut();
        [Browsable(false)]
        public PinOut OutZ { get; set; } = new PinOut();
        public VarDimF3()
        {
            VarType = Rtti.TtTypeDescGetter<Vector3>.TypeDesc;

            //Name = $"{Value}";

            InXYZ.Name = "in ";
            InXYZ.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinIn(InXYZ);
            InX.Name = "x ";
            InX.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinIn(InX);
            InY.Name = "y ";
            InY.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinIn(InY);
            InZ.Name = "z ";
            InZ.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinIn(InZ);

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
        }
    }
    [ContextMenu("color3", "Data\\Color3@_serial@", TtMaterialGraph.MaterialEditorKeyword)]
    public class VarColor3 : VarDimF3
    {
        public VarColor3()
        {
            PrevSize = new Vector2(100, 100);
        }
        [EGui.Controls.PropertyGrid.Color3PickerEditor()]
        public Vector3 Color { get => mValue; set => mValue = value; }
    }

    [ContextMenu("i3,int3", "Data\\int3@_serial@", TtMaterialGraph.MaterialEditorKeyword)]
    public class VarDimI3 : VarNode
    {
        public override Rtti.TtTypeDesc GetOutPinType(PinOut pin)
        {
            if (pin == OutXYZ)
                return Rtti.TtTypeDesc.TypeOf(typeof(Vector3i));
            else if (pin == OutX ||
                pin == OutY ||
                pin == OutZ)
                return Rtti.TtTypeDesc.TypeOf(typeof(int));
            return null;
        }
        public override bool CanLinkFrom(PinIn iPin, TtNodeBase OutNode, PinOut oPin)
        {
            if (base.CanLinkFrom(iPin, OutNode, oPin) == false)
                return false;

            var nodeExpr = OutNode as TtNodeBase;
            var type = nodeExpr.GetOutPinType(oPin);

            if (iPin == InXYZ)
            {
                if (!type.IsEqual(typeof(Vector3)))
                    return false;
            }
            else if (iPin == InX ||
                iPin == InY ||
                iPin == InZ)
            {
                if (!type.IsEqual(typeof(float)))
                    return false;
            }
            return true;
        }
        protected override string GetDefaultValue()
        {
            if (IsHalfPrecision)
                return $"min16int3({Value.X}, {Value.Y}, {Value.Z})";
            else
                return $"int3({Value.X}, {Value.Y}, {Value.Z})";
        }
        public override void BuildStatements(NodePin pin, ref BuildCodeStatementsData data)
        {
            base.BuildStatements(pin, ref data);
            if (data.NodeGraph.PinHasLinker(InXYZ))
            {
                var pinNode = data.NodeGraph.GetOppositePinNode(InXYZ);
                var opPin = data.NodeGraph.GetOppositePin(InXYZ);
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
            if (pin == OutXYZ)
                return new TtVariableReferenceExpression(Name);
            else if (pin == OutX)
                return new TtVariableReferenceExpression("x", new TtVariableReferenceExpression(Name));
            else if (pin == OutY)
                return new TtVariableReferenceExpression("y", new TtVariableReferenceExpression(Name));
            else if (pin == OutZ)
                return new TtVariableReferenceExpression("z", new TtVariableReferenceExpression(Name));
            return null;
        }

        protected Vector3i mValue = Vector3i.Zero;
        [Rtti.Meta]
        public Vector3i Value { get => mValue; set => mValue = value; }
        [Browsable(false)]
        public PinIn InXYZ { get; set; } = new PinIn();
        [Browsable(false)]
        public PinIn InX { get; set; } = new PinIn();
        [Browsable(false)]
        public PinIn InY { get; set; } = new PinIn();
        [Browsable(false)]
        public PinIn InZ { get; set; } = new PinIn();
        [Browsable(false)]
        public PinOut OutXYZ { get; set; } = new PinOut();
        [Browsable(false)]
        public PinOut OutX { get; set; } = new PinOut();
        [Browsable(false)]
        public PinOut OutY { get; set; } = new PinOut();
        [Browsable(false)]
        public PinOut OutZ { get; set; } = new PinOut();
        public VarDimI3()
        {
            VarType = Rtti.TtTypeDescGetter<Vector3i>.TypeDesc;

            //Name = $"{Value}";

            InXYZ.Name = "in ";
            InXYZ.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinIn(InXYZ);
            InX.Name = "x ";
            InX.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinIn(InX);
            InY.Name = "y ";
            InY.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinIn(InY);
            InZ.Name = "z ";
            InZ.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinIn(InZ);

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
        }
    }
}
