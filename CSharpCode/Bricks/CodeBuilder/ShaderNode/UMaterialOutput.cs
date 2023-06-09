using System;
using System.Collections.Generic;
using EngineNS.Bricks.NodeGraph;
using NPOI.POIFS.Crypt.Dsig;

namespace EngineNS.Bricks.CodeBuilder.ShaderNode
{
    public partial class UMaterialOutput : UNodeBase
    {
        public static UMaterialOutput NewNode(UMaterialGraph graph)
        {
            var result = new UMaterialOutput();
            result.UserData = graph;
            return result;
        }
        public UMaterialOutput()
        {
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF40FF40;
            TitleColor = 0xFF804020;
            BackColor = 0x80808080;

            Name = "Output";

            Position = new Vector2(100, 100);

            Albedo.Name = "Albedo ";
            Albedo.LinkDesc = UShaderEditorStyles.Instance.NewInOutPinDesc();            
            this.AddPinIn(Albedo);

            Emissive.Name = "Emissive ";
            Emissive.LinkDesc = UShaderEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinIn(Emissive);

            Normal.Name = "Normal ";
            Normal.LinkDesc = UShaderEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinIn(Normal);

            Metallic.Name = "Metallic ";
            Metallic.LinkDesc = UShaderEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinIn(Metallic);

            Rough.Name = "Rough ";
            Rough.LinkDesc = UShaderEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinIn(Rough);

            Alpha.Name = "Alpha ";
            Alpha.LinkDesc = UShaderEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinIn(Alpha);

            AlphaTest.Name = "AlphaTest ";
            AlphaTest.LinkDesc = UShaderEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinIn(AlphaTest);

            VertexOffset.Name = "VertexOffset ";
            VertexOffset.LinkDesc = UShaderEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinIn(VertexOffset);
        }
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinIn Albedo { get; set; } = new PinIn();

        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinIn Emissive { get; set; } = new PinIn();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinIn Normal { get; set; } = new PinIn();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinIn Metallic { get; set; } = new PinIn();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinIn Rough { get; set; } = new PinIn();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinIn Alpha { get; set; } = new PinIn();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinIn AlphaTest { get; set; } = new PinIn();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinIn VertexOffset { get; set; } = new PinIn();
        public override bool CanLinkFrom(PinIn iPin, UNodeBase OutNode, PinOut oPin)
        {
            if (base.CanLinkFrom(iPin, OutNode, oPin) == false)
                return false;

            var nodeExpr = OutNode as UNodeBase;
            var type = nodeExpr.GetOutPinType(oPin);

            if (iPin == Albedo ||
                iPin == Emissive ||
                iPin == Normal ||
                iPin == VertexOffset)
            {
                if (!type.IsEqual(typeof(Vector3)))
                    return false;
            }
            else if (iPin == Metallic ||
                iPin == Rough ||
                iPin == Alpha ||
                iPin == AlphaTest)
            {
                if (!type.IsEqual(typeof(float)))
                    return false;
            }

            return true;
        }
        public List<Graphics.Pipeline.Shader.EPixelShaderInput> GetPSNeedInputs()
        {
            var result = new List<Graphics.Pipeline.Shader.EPixelShaderInput>();
            foreach (var i in this.ParentGraph.Nodes)
            {
                if (i.GetType() == typeof(Bricks.CodeBuilder.ShaderNode.Control.SampleLevel2DNode) ||
                    i.GetType() == typeof(Bricks.CodeBuilder.ShaderNode.Control.Sample2DNode) ||
                    i.GetType() == typeof(Bricks.CodeBuilder.ShaderNode.Control.SampleArrayLevel2DNode) ||
                    i.GetType() == typeof(Bricks.CodeBuilder.ShaderNode.Control.SampleArray2DNode))
                {
                    if (result.Contains(Graphics.Pipeline.Shader.EPixelShaderInput.PST_UV) == false)
                        result.Add(Graphics.Pipeline.Shader.EPixelShaderInput.PST_UV);
                }
                else
                {
                    if (i.Name.StartsWith("input."))
                    {
                        var name = i.Name.Substring("input.".Length);
                        var t = Graphics.Pipeline.Shader.UMaterial.PSInput.NameToInput(name);
                        if (t != Graphics.Pipeline.Shader.EPixelShaderInput.PST_Number)
                        {
                            if (result.Contains(t) == false)
                                result.Add(t);
                        }
                    }
                }
            }
            return result;
        }

        public UMethodDeclaration PSFunction { get; } = new UMethodDeclaration();
        public UMethodDeclaration VSFunction { get; } = new UMethodDeclaration();
        public List<UVariableDeclaration> UniformVars { get; } = new List<UVariableDeclaration>();
        public override void BuildStatements(NodePin pin, ref BuildCodeStatementsData data)
        {
            var graph = data.NodeGraph as UMaterialGraph;

            PSFunction.MethodName = "DO_PS_MATERIAL_IMPL";
            PSFunction.Arguments.Clear();
            PSFunction.Arguments.Add(
                new UMethodArgumentDeclaration()
                {
                    OperationType = EMethodArgumentAttribute.In,
                    VariableType = new UTypeReference("PS_INPUT"),
                    VariableName = "input",
                });
            PSFunction.Arguments.Add(
                new UMethodArgumentDeclaration()
                {
                    OperationType = EMethodArgumentAttribute.Ref,
                    VariableType = new UTypeReference("MTL_OUTPUT"),
                    VariableName = "mtl",
                });
            PSFunction.LocalVariables.Clear();
            UniformVars.Clear();
            PSFunction.MethodBody.Sequence.Clear();
            data.CurrentStatements = PSFunction.MethodBody.Sequence;
            data.MethodDec = PSFunction;
            if (Albedo.HasLinker())
            {
                var linker = graph.FindInLinkerSingle(Albedo);
                var opPin = graph.GetOppositePin(Albedo);
                var pinNode = graph.GetOppositePinNode(Albedo);
                pinNode.BuildStatements(opPin, ref data);
                var exp = graph.GetOppositePinExpression(Albedo, ref data);
                var assign = new UAssignOperatorStatement()
                {
                    From = exp,
                    To = new UVariableReferenceExpression("mAlbedo", new UVariableReferenceExpression("mtl")),
                };
                PSFunction.MethodBody.Sequence.Add(assign);
            }
            if(Emissive.HasLinker())
            {
                var linker = graph.FindInLinkerSingle(Emissive);
                var opPin = graph.GetOppositePin(Emissive);
                var pinNode = graph.GetOppositePinNode(Emissive);
                pinNode.BuildStatements(opPin, ref data);
                var exp = graph.GetOppositePinExpression(Emissive, ref data);
                var assign = new UAssignOperatorStatement()
                {
                    From = exp,
                    To = new UVariableReferenceExpression("mEmissive", new UVariableReferenceExpression("mtl")),
                };
                PSFunction.MethodBody.Sequence.Add(assign);
            }
            if (Normal.HasLinker())
            {
                var linker = graph.FindInLinkerSingle(Normal);
                var pinNode = graph.GetOppositePinNode(Normal);
                var opPin = graph.GetOppositePin(Normal);
                pinNode.BuildStatements(opPin, ref data);
                var exp = graph.GetOppositePinExpression(Normal, ref data);
                var assign = new UAssignOperatorStatement()
                {
                    From = exp,
                    To = new UVariableReferenceExpression("mNormal", new UVariableReferenceExpression("mtl")),
                };
                PSFunction.MethodBody.Sequence.Add(assign);
            }
            if (Metallic.HasLinker())
            {
                var linker = graph.FindInLinkerSingle(Metallic);
                var pinNode = graph.GetOppositePinNode(Metallic);
                var opPin = graph.GetOppositePin(Metallic);
                pinNode.BuildStatements(opPin, ref data);
                var exp = graph.GetOppositePinExpression(Metallic, ref data);
                var assign = new UAssignOperatorStatement()
                {
                    From = exp,
                    To = new UVariableReferenceExpression("mMetallic", new UVariableReferenceExpression("mtl")),
                };
                PSFunction.MethodBody.Sequence.Add(assign);
            }
            if (Rough.HasLinker())
            {
                var linker = graph.FindInLinkerSingle(Rough);
                var pinNode = graph.GetOppositePinNode(Rough);
                var opPin = graph.GetOppositePin(Rough);
                pinNode.BuildStatements(opPin, ref data);
                var exp = graph.GetOppositePinExpression(Rough, ref data);
                var assign = new UAssignOperatorStatement()
                {
                    From = exp,
                    To = new UVariableReferenceExpression("mRough", new UVariableReferenceExpression("mtl")),
                };
                PSFunction.MethodBody.Sequence.Add(assign);
            }
            if (Alpha.HasLinker())
            {
                var linker = graph.FindInLinkerSingle(Alpha);
                var pinNode = graph.GetOppositePinNode(Alpha);
                var opPin = graph.GetOppositePin(Alpha);
                pinNode.BuildStatements(opPin, ref data);
                var exp = graph.GetOppositePinExpression(Alpha, ref data);
                var assign = new UAssignOperatorStatement()
                {
                    From = exp,
                    To = new UVariableReferenceExpression("mAlpha", new UVariableReferenceExpression("mtl")),
                };
                PSFunction.MethodBody.Sequence.Add(assign);
            }
            if (AlphaTest.HasLinker())
            {
                var linker = graph.FindInLinkerSingle(AlphaTest);
                var pinNode = graph.GetOppositePinNode(AlphaTest);
                var opPin = graph.GetOppositePin(AlphaTest);
                pinNode.BuildStatements(opPin, ref data);
                var exp = graph.GetOppositePinExpression(AlphaTest, ref data);
                var assign = new UAssignOperatorStatement()
                {
                    From = exp,
                    To = new UVariableReferenceExpression("mAlphaTest", new UVariableReferenceExpression("mtl")),
                };
                PSFunction.MethodBody.Sequence.Add(assign);
            }

            VSFunction.MethodName = "DO_VS_MATERIAL_IMPL";
            VSFunction.Arguments.Clear();
            VSFunction.Arguments.Add(
                new UMethodArgumentDeclaration()
                {
                    OperationType = EMethodArgumentAttribute.In,
                    VariableType = new UTypeReference("PS_INPUT"),
                    VariableName = "input",
                });
            VSFunction.Arguments.Add(
                new UMethodArgumentDeclaration()
                {
                    OperationType = EMethodArgumentAttribute.Ref,
                    VariableType = new UTypeReference("MTL_OUTPUT"),
                    VariableName = "mtl",
                });
            VSFunction.LocalVariables.Clear();
            VSFunction.MethodBody.Sequence.Clear();
            data.CurrentStatements = VSFunction.MethodBody.Sequence;
            data.MethodDec = VSFunction;
            if (VertexOffset.HasLinker())
            {
                var opPin = graph.GetOppositePin(VertexOffset);
                var pinNode = graph.GetOppositePinNode(VertexOffset);
                pinNode.BuildStatements(opPin, ref data);
                var exp = graph.GetOppositePinExpression(VertexOffset, ref data);
                var assign = new UAssignOperatorStatement()
                {
                    From = exp,
                    To = new UVariableReferenceExpression("mVertexOffset", new UVariableReferenceExpression("mtl")),
                };
                VSFunction.MethodBody.Sequence.Add(assign);
            }
        }
    }
}
