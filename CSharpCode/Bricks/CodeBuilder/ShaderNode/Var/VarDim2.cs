using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.CodeBuilder.ShaderNode.Var
{
    public class VarDimF2 : VarNode
    {
        public override string GetTitleName()
        {
            return $"{Name}=({Value})";
        }
        public override System.Type GetOutPinType(EGui.Controls.NodeGraph.PinOut pin)
        {
            if (pin == OutXY)
                return typeof(Vector2);
            else if (pin == OutX ||
                pin == OutY)
                return typeof(float);
            return null;
        }
        public override bool CanLinkFrom(EGui.Controls.NodeGraph.PinIn iPin, EGui.Controls.NodeGraph.NodeBase OutNode, EGui.Controls.NodeGraph.PinOut oPin)
        {
            if (base.CanLinkFrom(iPin, OutNode, oPin) == false)
                return false;

            var nodeExpr = OutNode as IBaseNode;
            var type = nodeExpr.GetOutPinType(oPin);

            if (iPin == InXY)
            {
                if (type != typeof(Vector2))
                    return false;
            }
            else if (iPin == InX ||
                iPin == InY)
            {
                if (type != typeof(float))
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
        public override void PreGenExpr()
        {
            Executed = false;
        }
        bool Executed = false;
        public override IExpression GetVarExpr(UMaterialGraph funGraph, ICodeGen cGen, EGui.Controls.NodeGraph.PinOut oPin, bool bTakeResult)
        {
            DefineVar Var = new DefineVar();
            Var.IsLocalVar = !IsUniform;
            Var.VarName = this.Name;
            Var.DefType = Type2HLSLType(VarType.SystemType, IsHalfPrecision);

            Var.InitValue = GetDefaultValue();

            if (Var.IsLocalVar)
            {
                funGraph.ShaderEditor.MaterialOutput.Function.AddLocalVar(Var);
            }
            else
            {
                funGraph.ShaderEditor.MaterialOutput.UniformVars.Add(Var);
            }
            if (Executed)
            {
                return new OpUseDefinedVar(Var);
            }
            var seq = new ExecuteSequence();
            var linkXY = funGraph.FindInLinkerSingle(InXY);
            if (linkXY != null)
            {
                var exprNode = linkXY.OutNode as IBaseNode;
                var xyExpr = exprNode.GetExpr(funGraph, cGen, linkXY.Out, true) as OpExpress;
                var setExpr = new AssignOp();
                setExpr.Left = new OpUseDefinedVar(Var);
                setExpr.Right = xyExpr;

                seq.Lines.Add(setExpr);
            }
            var linkX = funGraph.FindInLinkerSingle(InX);
            if (linkX != null)
            {
                var exprNode = linkX.OutNode as IBaseNode;
                var xExpr = exprNode.GetExpr(funGraph, cGen, linkX.Out, true) as OpExpress;
                var setExpr = new AssignOp();
                var member = new BinocularOp(EBinocularOp.GetMember);
                member.Left = new OpUseDefinedVar(Var);
                member.Right = new HardCodeOp() { Code = "x" };
                setExpr.Left = member;
                setExpr.Right = xExpr;

                seq.Lines.Add(setExpr);
            }
            var linkY = funGraph.FindInLinkerSingle(InY);
            if (linkY != null)
            {
                var exprNode = linkY.OutNode as IBaseNode;
                var yExpr = exprNode.GetExpr(funGraph, cGen, linkY.Out, true) as OpExpress;
                var setExpr = new AssignOp();
                var member = new BinocularOp(EBinocularOp.GetMember);
                member.Left = new OpUseDefinedVar(Var);
                member.Right = new HardCodeOp() { Code = "y" };
                setExpr.Left = member;
                setExpr.Right = yExpr;

                seq.Lines.Add(setExpr);
            }
            return new OpExecuteAndUseDefinedVar(seq, Var);
        }
        public override IExpression GetExpr(UMaterialGraph funGraph, ICodeGen cGen, EGui.Controls.NodeGraph.PinOut oPin, bool bTakeResult)
        {
            var expr = GetVarExpr(funGraph, cGen, oPin, bTakeResult);
            if (oPin == OutXY)
            {
                var dotOp = new BinocularOp(EBinocularOp.GetMember);
                dotOp.Left = expr as OpExpress;
                var member = new HardCodeOp();
                member.Code = "xy";
                dotOp.Right = member;
                return dotOp;
            }
            else if (oPin == OutX)
            {
                var dotOp = new BinocularOp(EBinocularOp.GetMember);
                dotOp.Left = expr as OpExpress;
                var member = new HardCodeOp();
                member.Code = "x";
                dotOp.Right = member;
                return dotOp;
            }
            else if (oPin == OutY)
            {
                var dotOp = new BinocularOp(EBinocularOp.GetMember);
                dotOp.Left = expr as OpExpress;
                var member = new HardCodeOp();
                member.Code = "y";
                dotOp.Right = member;
                return dotOp;
            }
            return null;
        }
        [Rtti.Meta]
        public Vector2 Value { get; set; } = Vector2.Zero;
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public EGui.Controls.NodeGraph.PinIn InXY { get; set; } = new EGui.Controls.NodeGraph.PinIn();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public EGui.Controls.NodeGraph.PinIn InX { get; set; } = new EGui.Controls.NodeGraph.PinIn();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public EGui.Controls.NodeGraph.PinIn InY { get; set; } = new EGui.Controls.NodeGraph.PinIn();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public EGui.Controls.NodeGraph.PinOut OutXY { get; set; } = new EGui.Controls.NodeGraph.PinOut();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public EGui.Controls.NodeGraph.PinOut OutX { get; set; } = new EGui.Controls.NodeGraph.PinOut();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public EGui.Controls.NodeGraph.PinOut OutY { get; set; } = new EGui.Controls.NodeGraph.PinOut();
        public VarDimF2()
        {
            VarType = Rtti.UTypeDescGetter<Vector2>.TypeDesc;

            //Name = $"{Value}";

            InXY.Name = "in ";
            InXY.Link = UShaderEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinIn(InXY);
            InX.Name = "x ";
            InX.Link = UShaderEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinIn(InX);
            InY.Name = "y ";
            InY.Link = UShaderEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinIn(InY);

            OutXY.Name = " xy";
            OutXY.Link = UShaderEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinOut(OutXY);
            OutX.Name = " x";
            OutX.Link = UShaderEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinOut(OutX);
            OutY.Name = " y";
            OutY.Link = UShaderEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinOut(OutY);
        }
    }
}
