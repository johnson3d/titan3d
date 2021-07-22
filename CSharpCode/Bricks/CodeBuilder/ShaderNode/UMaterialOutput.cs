using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.CodeBuilder.ShaderNode
{
    public partial class UMaterialOutput : IBaseNode
    {
        public static UMaterialOutput NewNode(UMaterialGraph graph)
        {
            var result = new UMaterialOutput();
            result.Graph = graph;
            return result;
        }
        public UMaterialOutput()
        {
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF40FF40;
            TitleImage.Color = 0xFF804020;
            Background.Color = 0x80808080;

            Name = "Output";

            Position = new Vector2(100, 100);

            Albedo.Name = "Albedo ";
            Albedo.Link = UShaderEditorStyles.Instance.NewInOutPinDesc();            
            this.AddPinIn(Albedo);

            Emissive.Name = "Emissive ";
            Emissive.Link = UShaderEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinIn(Emissive);

            Normal.Name = "Normal ";
            Normal.Link = UShaderEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinIn(Normal);

            Metallic.Name = "Metallic ";
            Metallic.Link = UShaderEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinIn(Metallic);

            Rough.Name = "Rough ";
            Rough.Link = UShaderEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinIn(Rough);

            Alpha.Name = "Alpha ";
            Alpha.Link = UShaderEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinIn(Alpha);

            AlphaTest.Name = "AlphaTest ";
            AlphaTest.Link = UShaderEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinIn(AlphaTest);
        }
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public EGui.Controls.NodeGraph.PinIn Albedo { get; set; } = new EGui.Controls.NodeGraph.PinIn();

        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public EGui.Controls.NodeGraph.PinIn Emissive { get; set; } = new EGui.Controls.NodeGraph.PinIn();[EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public EGui.Controls.NodeGraph.PinIn Normal { get; set; } = new EGui.Controls.NodeGraph.PinIn();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public EGui.Controls.NodeGraph.PinIn Metallic { get; set; } = new EGui.Controls.NodeGraph.PinIn();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public EGui.Controls.NodeGraph.PinIn Rough { get; set; } = new EGui.Controls.NodeGraph.PinIn();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public EGui.Controls.NodeGraph.PinIn Alpha { get; set; } = new EGui.Controls.NodeGraph.PinIn();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public EGui.Controls.NodeGraph.PinIn AlphaTest { get; set; } = new EGui.Controls.NodeGraph.PinIn();
        public override bool CanLinkFrom(EGui.Controls.NodeGraph.PinIn iPin, EGui.Controls.NodeGraph.NodeBase OutNode, EGui.Controls.NodeGraph.PinOut oPin)
        {
            if (base.CanLinkFrom(iPin, OutNode, oPin) == false)
                return false;

            var nodeExpr = OutNode as IBaseNode;
            var type = nodeExpr.GetOutPinType(oPin);

            if (iPin == Albedo ||
                iPin == Emissive ||
                iPin == Normal )
            {
                if (type != typeof(Vector3))
                    return false;
            }
            else if (iPin == Metallic ||
                iPin == Rough ||
                iPin == Alpha ||
                iPin == AlphaTest)
            {
                if (type != typeof(float))
                    return false;
            }

            return true;
        }

        public override IExpression GetExpr(UMaterialGraph funGraph, ICodeGen cGen, EGui.Controls.NodeGraph.PinOut oPin, bool bTakeResult)
        {
            Function.Name = "DO_PS_MATERIAL_IMPL";
            Function.ReturnType = typeof(void).FullName;
            Function.Arguments.Clear();
            Function.Arguments.Add(new DefineVar() { DefType = "in PS_INPUT", VarName = "input" });
            Function.Arguments.Add(new DefineVar() { DefType = "inout MTL_OUTPUT", VarName = "mtl" });
            Function.LocalVars.Clear();
            UniformVars.Clear();
            Function.Body.Lines.Clear();
            if (Albedo.HasLinker())
            {
                var linker = funGraph.FindInLinkerSingle(Albedo);
                var exprNode = linker.OutNode as IBaseNode;
                var expr = exprNode.GetExpr(funGraph, cGen, linker.Out, false) as OpExpress;
                var assignOp = new AssignOp();
                var setExpr = new HardCodeOp();
                setExpr.Code = "mtl.mAlbedo";
                assignOp.Left = setExpr;
                assignOp.Right = expr;
                Function.Body.PushExpr(assignOp);
            }
            if (Emissive.HasLinker())
            {
                var linker = funGraph.FindInLinkerSingle(Emissive);
                var exprNode = linker.OutNode as IBaseNode;
                var expr = exprNode.GetExpr(funGraph, cGen, linker.Out, false) as OpExpress;
                var assignOp = new AssignOp();
                var setExpr = new HardCodeOp();
                setExpr.Code = "mtl.mEmissive";
                assignOp.Left = setExpr;
                assignOp.Right = expr;
                Function.Body.PushExpr(assignOp);
            }
            if (Normal.HasLinker())
            {
                var linker = funGraph.FindInLinkerSingle(Normal);
                var exprNode = linker.OutNode as IBaseNode;
                var expr = exprNode.GetExpr(funGraph, cGen, linker.Out, false) as OpExpress;
                var assignOp = new AssignOp();
                var setExpr = new HardCodeOp();
                setExpr.Code = "mtl.mNormal";
                assignOp.Left = setExpr;
                assignOp.Right = expr;
                Function.Body.PushExpr(assignOp);
            }
            if (Metallic.HasLinker())
            {
                var linker = funGraph.FindInLinkerSingle(Metallic);
                var exprNode = linker.OutNode as IBaseNode;
                var expr = exprNode.GetExpr(funGraph, cGen, linker.Out, false) as OpExpress;
                var assignOp = new AssignOp();
                var setExpr = new HardCodeOp();
                setExpr.Code = "mtl.mMetallic";
                assignOp.Left = setExpr;
                assignOp.Right = expr;
                Function.Body.PushExpr(assignOp);
            }
            if (Rough.HasLinker())
            {
                var linker = funGraph.FindInLinkerSingle(Rough);
                var exprNode = linker.OutNode as IBaseNode;
                var expr = exprNode.GetExpr(funGraph, cGen, linker.Out, false) as OpExpress;
                var assignOp = new AssignOp();
                var setExpr = new HardCodeOp();
                setExpr.Code = "mtl.mRough";
                assignOp.Left = setExpr;
                assignOp.Right = expr;
                Function.Body.PushExpr(assignOp);
            }
            if (Alpha.HasLinker())
            {
                var linker = funGraph.FindInLinkerSingle(Alpha);
                var exprNode = linker.OutNode as IBaseNode;
                var expr = exprNode.GetExpr(funGraph, cGen, linker.Out, false) as OpExpress;
                var assignOp = new AssignOp();
                var setExpr = new HardCodeOp();
                setExpr.Code = "mtl.mAlpha";
                assignOp.Left = setExpr;
                assignOp.Right = expr;
                Function.Body.PushExpr(assignOp);
            }
            if (AlphaTest.HasLinker())
            {
                var linker = funGraph.FindInLinkerSingle(AlphaTest);
                var exprNode = linker.OutNode as IBaseNode;
                var expr = exprNode.GetExpr(funGraph, cGen, linker.Out, false) as OpExpress;
                var assignOp = new AssignOp();
                var setExpr = new HardCodeOp();
                setExpr.Code = "mtl.mAlphaTest";
                assignOp.Left = setExpr;
                assignOp.Right = expr;
                Function.Body.PushExpr(assignOp);
            }

            return Function;
        }

        public DefineFunction Function { get; } = new DefineFunction();
        public List<DefineVar> UniformVars { get; } = new List<DefineVar>();
    }
}
