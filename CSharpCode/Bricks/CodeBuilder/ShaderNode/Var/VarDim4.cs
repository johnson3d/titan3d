using EngineNS.EGui.Controls.NodeGraph;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.CodeBuilder.ShaderNode.Var
{
    public class VarDimF4 : VarNode
    {
        public override string GetTitleName()
        {
            return $"{Name}=({Value})";
        }
        public override System.Type GetOutPinType(EGui.Controls.NodeGraph.PinOut pin)
        {
            if (pin == OutXYZW)
                return typeof(Vector4);
            else if (pin == OutX ||
                pin == OutY ||
                pin == OutZ ||
                pin == OutW)
                return typeof(float);
            return null;
        }
        public override bool CanLinkFrom(EGui.Controls.NodeGraph.PinIn iPin, EGui.Controls.NodeGraph.NodeBase OutNode, EGui.Controls.NodeGraph.PinOut oPin)
        {
            if (base.CanLinkFrom(iPin, OutNode, oPin) == false)
                return false;

            var nodeExpr = OutNode as IBaseNode;
            var type = nodeExpr.GetOutPinType(oPin);

            if (iPin == InXYZW)
            {
                if (type != typeof(Vector4))
                    return false;
            }
            else if (iPin == InX ||
                iPin == InY ||
                iPin == InZ ||
                iPin == InW)
            {
                if (type != typeof(float))
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
            var linkXYZW = funGraph.FindInLinkerSingle(InXYZW);
            if (linkXYZW != null)
            {
                var exprNode = linkXYZW.OutNode as IBaseNode;
                var xyExpr = exprNode.GetExpr(funGraph, cGen, linkXYZW.Out, true) as OpExpress;
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
            var linkW = funGraph.FindInLinkerSingle(InW);
            if (linkW != null)
            {
                var exprNode = linkW.OutNode as IBaseNode;
                var yExpr = exprNode.GetExpr(funGraph, cGen, linkW.Out, true) as OpExpress;
                var setExpr = new AssignOp();
                var member = new BinocularOp(EBinocularOp.GetMember);
                member.Left = new OpUseDefinedVar(Var);
                member.Right = new HardCodeOp() { Code = "w" };
                setExpr.Left = member;
                setExpr.Right = yExpr;

                seq.Lines.Add(setExpr);
            }
            return new OpExecuteAndUseDefinedVar(seq, Var);
        }
        public override IExpression GetExpr(UMaterialGraph funGraph, ICodeGen cGen, EGui.Controls.NodeGraph.PinOut oPin, bool bTakeResult)
        {
            var expr = GetVarExpr(funGraph, cGen, oPin, bTakeResult);
            if (oPin == OutXYZW)
            {
                var dotOp = new BinocularOp(EBinocularOp.GetMember);
                dotOp.Left = expr as OpExpress;
                var member = new HardCodeOp();
                member.Code = "xyzw";
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
            else if (oPin == OutW)
            {
                var dotOp = new BinocularOp(EBinocularOp.GetMember);
                dotOp.Left = expr as OpExpress;
                var member = new HardCodeOp();
                member.Code = "w";
                dotOp.Right = member;
                return dotOp;
            }
            return null;
        }
        protected Vector4 mValue = Vector4.Zero;
        [Rtti.Meta]
        public Vector4 Value { get => mValue; set => mValue = value; }
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public EGui.Controls.NodeGraph.PinIn InXYZW { get; set; } = new EGui.Controls.NodeGraph.PinIn();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public EGui.Controls.NodeGraph.PinIn InX { get; set; } = new EGui.Controls.NodeGraph.PinIn();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public EGui.Controls.NodeGraph.PinIn InY { get; set; } = new EGui.Controls.NodeGraph.PinIn();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public EGui.Controls.NodeGraph.PinIn InZ { get; set; } = new EGui.Controls.NodeGraph.PinIn();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public EGui.Controls.NodeGraph.PinIn InW { get; set; } = new EGui.Controls.NodeGraph.PinIn();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public EGui.Controls.NodeGraph.PinOut OutXYZW { get; set; } = new EGui.Controls.NodeGraph.PinOut();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public EGui.Controls.NodeGraph.PinOut OutX { get; set; } = new EGui.Controls.NodeGraph.PinOut();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public EGui.Controls.NodeGraph.PinOut OutY { get; set; } = new EGui.Controls.NodeGraph.PinOut();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public EGui.Controls.NodeGraph.PinOut OutZ { get; set; } = new EGui.Controls.NodeGraph.PinOut();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public EGui.Controls.NodeGraph.PinOut OutW { get; set; } = new EGui.Controls.NodeGraph.PinOut();
        public VarDimF4()
        {
            VarType = Rtti.UTypeDescGetter<Vector4>.TypeDesc;

            //Name = $"{Value}";

            InXYZW.Name = "in ";
            InXYZW.Link = UShaderEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinIn(InXYZW);
            InX.Name = "x ";
            InX.Link = UShaderEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinIn(InX);
            InY.Name = "y ";
            InY.Link = UShaderEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinIn(InY);
            InZ.Name = "z ";
            InZ.Link = UShaderEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinIn(InZ);
            InW.Name = "w ";
            InW.Link = UShaderEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinIn(InW);

            OutXYZW.Name = " xyzw";
            OutXYZW.Link = UShaderEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinOut(OutXYZW);
            OutX.Name = " x";
            OutX.Link = UShaderEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinOut(OutX);
            OutY.Name = " y";
            OutY.Link = UShaderEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinOut(OutY);
            OutZ.Name = " z";
            OutZ.Link = UShaderEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinOut(OutZ);
            OutW.Name = " w";
            OutW.Link = UShaderEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinOut(OutW);
        }
    }
    public class VarColor4 : VarDimF4
    {
        public VarColor4()
        {
            PreviewWidth = 100;
            OutXYZ.Name = " xyz";
            OutXYZ.Link = UShaderEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinOut(OutXYZ);
        }
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public EGui.Controls.NodeGraph.PinOut OutXYZ { get; set; } = new EGui.Controls.NodeGraph.PinOut();
        [EGui.Controls.PropertyGrid.Color4PickerEditor()]
        public Vector4 Color { get => mValue; set => mValue = value; }
        public override System.Type GetOutPinType(EGui.Controls.NodeGraph.PinOut pin)
        {
            if (pin == OutXYZW)
                return typeof(Vector4);
            else if (pin == OutXYZ)
                return typeof(Vector3);
            else if (pin == OutX ||
                pin == OutY ||
                pin == OutZ ||
                pin == OutW)
                return typeof(float);
            return null;
        }
        public unsafe override void OnPreviewDraw(ref Vector2 prevStart, ref Vector2 prevEnd)
        {
            var cmdlist = new ImDrawList(ImGuiAPI.GetWindowDrawList());

            var color = new Color4(mValue.W, mValue.X, mValue.Y, mValue.Z);
            fixed (Vector2* p_prevStart = &prevStart)
            fixed (Vector2* p_prevEnd = &prevEnd)
            {
                cmdlist.AddRectFilled(p_prevStart, p_prevEnd, color.ToBgr(), 0, ImDrawFlags_.ImDrawFlags_RoundCornersAll);
            }
        }
        public override void OnLClicked(NodePin clickedPin)
        {
            base.OnLClicked(clickedPin);

            //Graph.ShaderEditor.NodePropGrid.HideInheritDeclareType = Rtti.UTypeDescGetter<VarDimF4>.TypeDesc;
        }
        public override IExpression GetExpr(UMaterialGraph funGraph, ICodeGen cGen, EGui.Controls.NodeGraph.PinOut oPin, bool bTakeResult)
        {
            var expr = GetVarExpr(funGraph, cGen, oPin, bTakeResult);
            if (oPin == OutXYZW)
            {
                var dotOp = new BinocularOp(EBinocularOp.GetMember);
                dotOp.Left = expr as OpExpress;
                var member = new HardCodeOp();
                member.Code = "xyzw";
                dotOp.Right = member;
                return dotOp;
            }
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
            else if (oPin == OutW)
            {
                var dotOp = new BinocularOp(EBinocularOp.GetMember);
                dotOp.Left = expr as OpExpress;
                var member = new HardCodeOp();
                member.Code = "w";
                dotOp.Right = member;
                return dotOp;
            }
            return null;
        }
    }
}
