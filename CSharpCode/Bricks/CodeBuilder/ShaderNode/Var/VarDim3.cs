using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.CodeBuilder.ShaderNode.Var
{
    public class VarDimF3 : VarNode
    {
        public override string GetTitleName()
        {
            return $"{Name}=({Value})";
        }
        public override System.Type GetOutPinType(EGui.Controls.NodeGraph.PinOut pin)
        {
            if (pin == OutXYZ)
                return typeof(Vector3);
            else if (pin == OutX ||
                pin == OutY ||
                pin == OutZ)
                return typeof(float);
            return null;
        }
        public override bool CanLinkFrom(EGui.Controls.NodeGraph.PinIn iPin, EGui.Controls.NodeGraph.NodeBase OutNode, EGui.Controls.NodeGraph.PinOut oPin)
        {
            if (base.CanLinkFrom(iPin, OutNode, oPin) == false)
                return false;

            var nodeExpr = OutNode as IBaseNode;
            var type = nodeExpr.GetOutPinType(oPin);

            if (iPin == InXYZ)
            {
                if (type != typeof(Vector3))
                    return false;
            }
            else if (iPin == InX ||
                iPin == InY ||
                iPin == InZ)
            {
                if (type != typeof(float))
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
            Executed = true;
            var seq = new ExecuteSequence();
            var linkXYZ = funGraph.FindInLinkerSingle(InXYZ);
            if (linkXYZ != null)
            {
                var exprNode = linkXYZ.OutNode as IBaseNode;
                var xyExpr = exprNode.GetExpr(funGraph, cGen, linkXYZ.Out, true) as OpExpress;
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
            var linkZ = funGraph.FindInLinkerSingle(InZ);
            if (linkZ != null)
            {
                var exprNode = linkZ.OutNode as IBaseNode;
                var yExpr = exprNode.GetExpr(funGraph, cGen, linkZ.Out, true) as OpExpress;
                var setExpr = new AssignOp();
                var member = new BinocularOp(EBinocularOp.GetMember);
                member.Left = new OpUseDefinedVar(Var);
                member.Right = new HardCodeOp() { Code = "z" };
                setExpr.Left = member;
                setExpr.Right = yExpr;

                seq.Lines.Add(setExpr);
            }
            return new OpExecuteAndUseDefinedVar(seq, Var);
        }
        public override IExpression GetExpr(UMaterialGraph funGraph, ICodeGen cGen, EGui.Controls.NodeGraph.PinOut oPin, bool bTakeResult)
        {
            var expr = GetVarExpr(funGraph, cGen, oPin, bTakeResult);
            if (oPin == OutXYZ)
            {
                var dotOp = new BinocularOp(EBinocularOp.GetMember);
                dotOp.Left = expr as OpExpress;
                var member = new HardCodeOp();
                member.Code = "xyz";
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
            else if (oPin == OutZ)
            {
                var dotOp = new BinocularOp(EBinocularOp.GetMember);
                dotOp.Left = expr as OpExpress;
                var member = new HardCodeOp();
                member.Code = "z";
                dotOp.Right = member;
                return dotOp;
            }
            return null;
        }
        protected Vector3 mValue = Vector3.Zero;
        [Rtti.Meta]
        public Vector3 Value { get => mValue; set => mValue = value; }
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public EGui.Controls.NodeGraph.PinIn InXYZ { get; set; } = new EGui.Controls.NodeGraph.PinIn();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public EGui.Controls.NodeGraph.PinIn InX { get; set; } = new EGui.Controls.NodeGraph.PinIn();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public EGui.Controls.NodeGraph.PinIn InY { get; set; } = new EGui.Controls.NodeGraph.PinIn();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public EGui.Controls.NodeGraph.PinIn InZ { get; set; } = new EGui.Controls.NodeGraph.PinIn();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public EGui.Controls.NodeGraph.PinOut OutXYZ { get; set; } = new EGui.Controls.NodeGraph.PinOut();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public EGui.Controls.NodeGraph.PinOut OutX { get; set; } = new EGui.Controls.NodeGraph.PinOut();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public EGui.Controls.NodeGraph.PinOut OutY { get; set; } = new EGui.Controls.NodeGraph.PinOut();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public EGui.Controls.NodeGraph.PinOut OutZ { get; set; } = new EGui.Controls.NodeGraph.PinOut();
        public VarDimF3()
        {
            VarType = Rtti.UTypeDescGetter<Vector3>.TypeDesc;

            //Name = $"{Value}";

            InXYZ.Name = "in ";
            InXYZ.Link = UShaderEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinIn(InXYZ);
            InX.Name = "x ";
            InX.Link = UShaderEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinIn(InX);
            InY.Name = "y ";
            InY.Link = UShaderEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinIn(InY);
            InZ.Name = "z ";
            InZ.Link = UShaderEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinIn(InZ);

            OutXYZ.Name = " xyz";
            OutXYZ.Link = UShaderEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinOut(OutXYZ);
            OutX.Name = " x";
            OutX.Link = UShaderEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinOut(OutX);
            OutY.Name = " y";
            OutY.Link = UShaderEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinOut(OutY);
            OutZ.Name = " z";
            OutZ.Link = UShaderEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinOut(OutZ);
        }
    }
    public class VarColor3 : VarDimF3
    {
        public VarColor3()
        {
            PreviewWidth = 100;
        }
        [EGui.Controls.PropertyGrid.Color3PickerEditor()]
        public Vector3 Color { get => mValue; set => mValue = value; }
    }
}
