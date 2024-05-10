using System;
using System.Collections.Generic;
using EngineNS.Bricks.NodeGraph;
using System.ComponentModel;

namespace EngineNS.Bricks.CodeBuilder.ShaderNode.Var
{
    [ContextMenu("f2,float2", "Data\\float2@_serial@", UMaterialGraph.MaterialEditorKeyword)]
    public class VarDimF2 : VarNode
    {
        public override Rtti.UTypeDesc GetOutPinType(PinOut pin)
        {
            if (pin == OutXY)
                return Rtti.UTypeDesc.TypeOf(typeof(Vector2));
            else if (pin == OutX ||
                pin == OutY)
                return Rtti.UTypeDesc.TypeOf(typeof(float));
            return null;
        }
        public override bool CanLinkFrom(PinIn iPin, UNodeBase OutNode, PinOut oPin)
        {
            if (base.CanLinkFrom(iPin, OutNode, oPin) == false)
                return false;

            var nodeExpr = OutNode as UNodeBase;
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
        //    var seq = new ExecuteSequence();
        //    var linkXY = funGraph.FindInLinkerSingle(InXY);
        //    if (linkXY != null)
        //    {
        //        var exprNode = linkXY.OutNode as IBaseNode;
        //        var xyExpr = exprNode.GetExpr(funGraph, cGen, linkXY.OutPin, true) as OpExpress;
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
        //    return new OpExecuteAndUseDefinedVar(seq, Var);
        //}
        //public override IExpression GetExpr(UMaterialGraph funGraph, ICodeGen cGen, PinOut oPin, bool bTakeResult)
        //{
        //    var expr = GetVarExpr(funGraph, cGen, oPin, bTakeResult);
        //    if (oPin == OutXY)
        //    {
        //        var dotOp = new BinocularOp(EBinocularOp.GetMember);
        //        dotOp.Left = expr as OpExpress;
        //        var member = new HardCodeOp();
        //        member.Code = "xy";
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
        //    return null;
        //}

        public override void BuildStatements(NodePin pin, ref BuildCodeStatementsData data)
        {
            base.BuildStatements(pin, ref data);
            if (data.NodeGraph.PinHasLinker(InXY))
            {
                var pinNode = data.NodeGraph.GetOppositePinNode(InXY);
                var opPin = data.NodeGraph.GetOppositePin(InXY);
                pinNode.BuildStatements(opPin, ref data);

                var assignStatement = new UAssignOperatorStatement()
                {
                    From = pinNode.GetExpression(opPin, ref data),
                    To = new UVariableReferenceExpression(Name),
                };
                if(!data.CurrentStatements.Contains(assignStatement))
                    data.CurrentStatements.Add(assignStatement);
            }
            if(data.NodeGraph.PinHasLinker(InX))
            {
                var pinNode = data.NodeGraph.GetOppositePinNode(InX);
                var opPin = data.NodeGraph.GetOppositePin(InX);
                pinNode.BuildStatements(opPin, ref data);

                var assignStatement = new UAssignOperatorStatement()
                {
                    From = pinNode.GetExpression(opPin, ref data),
                    To = new UVariableReferenceExpression("x", new UVariableReferenceExpression(Name)),
                };
                if(!data.CurrentStatements.Contains(assignStatement))
                    data.CurrentStatements.Add(assignStatement);
            }
            if (data.NodeGraph.PinHasLinker(InY))
            {
                var pinNode = data.NodeGraph.GetOppositePinNode(InY);
                var opPin = data.NodeGraph.GetOppositePin(InY);
                pinNode.BuildStatements(opPin, ref data);

                var assignStatement = new UAssignOperatorStatement()
                {
                    From = pinNode.GetExpression(opPin, ref data),
                    To = new UVariableReferenceExpression("y", new UVariableReferenceExpression(Name)),
                };
                if(!data.CurrentStatements.Contains(assignStatement))
                    data.CurrentStatements.Add(assignStatement);
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
            if (pin == OutXY)
                return new UVariableReferenceExpression(Name);
            else if (pin == OutX)
                return new UVariableReferenceExpression("x", new UVariableReferenceExpression(Name));
            else if (pin == OutY)
                return new UVariableReferenceExpression("y", new UVariableReferenceExpression(Name));
            return null;
        }

        [Rtti.Meta]
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
            VarType = Rtti.UTypeDescGetter<Vector2>.TypeDesc;

            //Name = $"{Value}";

            InXY.Name = "in ";
            InXY.LinkDesc = UShaderEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinIn(InXY);
            InX.Name = "x ";
            InX.LinkDesc = UShaderEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinIn(InX);
            InY.Name = "y ";
            InY.LinkDesc = UShaderEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinIn(InY);

            OutXY.Name = " xy";
            OutXY.LinkDesc = UShaderEditorStyles.Instance.NewInOutPinDesc();
            OutXY.MultiLinks = true;
            this.AddPinOut(OutXY);
            OutX.Name = " x";
            OutX.LinkDesc = UShaderEditorStyles.Instance.NewInOutPinDesc();
            OutX.MultiLinks = true;
            this.AddPinOut(OutX);
            OutY.Name = " y";
            OutY.LinkDesc = UShaderEditorStyles.Instance.NewInOutPinDesc();
            OutY.MultiLinks = true;
            this.AddPinOut(OutY);
        }
    }

    [ContextMenu("i2,int2", "Data\\int2@_serial@", UMaterialGraph.MaterialEditorKeyword)]
    public class VarDimI2 : VarNode
    {
        public override Rtti.UTypeDesc GetOutPinType(PinOut pin)
        {
            if (pin == OutXY)
                return Rtti.UTypeDesc.TypeOf(typeof(Vector2i));
            else if (pin == OutX ||
                pin == OutY)
                return Rtti.UTypeDesc.TypeOf(typeof(int));
            return null;
        }
        public override bool CanLinkFrom(PinIn iPin, UNodeBase OutNode, PinOut oPin)
        {
            if (base.CanLinkFrom(iPin, OutNode, oPin) == false)
                return false;

            var nodeExpr = OutNode as UNodeBase;
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

                var assignStatement = new UAssignOperatorStatement()
                {
                    From = pinNode.GetExpression(opPin, ref data),
                    To = new UVariableReferenceExpression(Name),
                };
                if (!data.CurrentStatements.Contains(assignStatement))
                    data.CurrentStatements.Add(assignStatement);
            }
            if (data.NodeGraph.PinHasLinker(InX))
            {
                var pinNode = data.NodeGraph.GetOppositePinNode(InX);
                var opPin = data.NodeGraph.GetOppositePin(InX);
                pinNode.BuildStatements(opPin, ref data);

                var assignStatement = new UAssignOperatorStatement()
                {
                    From = pinNode.GetExpression(opPin, ref data),
                    To = new UVariableReferenceExpression("x", new UVariableReferenceExpression(Name)),
                };
                if (!data.CurrentStatements.Contains(assignStatement))
                    data.CurrentStatements.Add(assignStatement);
            }
            if (data.NodeGraph.PinHasLinker(InY))
            {
                var pinNode = data.NodeGraph.GetOppositePinNode(InY);
                var opPin = data.NodeGraph.GetOppositePin(InY);
                pinNode.BuildStatements(opPin, ref data);

                var assignStatement = new UAssignOperatorStatement()
                {
                    From = pinNode.GetExpression(opPin, ref data),
                    To = new UVariableReferenceExpression("y", new UVariableReferenceExpression(Name)),
                };
                if (!data.CurrentStatements.Contains(assignStatement))
                    data.CurrentStatements.Add(assignStatement);
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
            if (pin == OutXY)
                return new UVariableReferenceExpression(Name);
            else if (pin == OutX)
                return new UVariableReferenceExpression("x", new UVariableReferenceExpression(Name));
            else if (pin == OutY)
                return new UVariableReferenceExpression("y", new UVariableReferenceExpression(Name));
            return null;
        }

        [Rtti.Meta]
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
            VarType = Rtti.UTypeDescGetter<Vector2i>.TypeDesc;

            //Name = $"{Value}";

            InXY.Name = "in ";
            InXY.LinkDesc = UShaderEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinIn(InXY);
            InX.Name = "x ";
            InX.LinkDesc = UShaderEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinIn(InX);
            InY.Name = "y ";
            InY.LinkDesc = UShaderEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinIn(InY);

            OutXY.Name = " xy";
            OutXY.LinkDesc = UShaderEditorStyles.Instance.NewInOutPinDesc();
            OutXY.MultiLinks = true;
            this.AddPinOut(OutXY);
            OutX.Name = " x";
            OutX.LinkDesc = UShaderEditorStyles.Instance.NewInOutPinDesc();
            OutX.MultiLinks = true;
            this.AddPinOut(OutX);
            OutY.Name = " y";
            OutY.LinkDesc = UShaderEditorStyles.Instance.NewInOutPinDesc();
            OutY.MultiLinks = true;
            this.AddPinOut(OutY);
        }
    }
}
