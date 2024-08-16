using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
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

            Position = new Vector2(100, 100);

            Name = "Output";

            var fields = typeof(Graphics.Pipeline.Shader.MTL_OUTPUT).GetFields();
            foreach (var i in fields)
            {
                var attr = i.GetCustomAttribute<EngineNS.Editor.ShaderCompiler.TtShaderDefineAttribute>(false);
                if (attr == null)
                    continue;
                var pin = new PinIn();
                System.Diagnostics.Debug.Assert(attr.ShaderName.StartsWith("m"));
                pin.Name = attr.ShaderName.Substring(1) + " ";
                pin.LinkDesc = UShaderEditorStyles.Instance.NewInOutPinDesc();
                this.AddPinIn(pin);

                FieldPins.Add(pin);
            }   

            //Albedo.Name = "Albedo ";
            //Albedo.LinkDesc = UShaderEditorStyles.Instance.NewInOutPinDesc();            
            //this.AddPinIn(Albedo);

            //Emissive.Name = "Emissive ";
            //Emissive.LinkDesc = UShaderEditorStyles.Instance.NewInOutPinDesc();
            //this.AddPinIn(Emissive);

            //Normal.Name = "Normal ";
            //Normal.LinkDesc = UShaderEditorStyles.Instance.NewInOutPinDesc();
            //this.AddPinIn(Normal);

            //Metallic.Name = "Metallic ";
            //Metallic.LinkDesc = UShaderEditorStyles.Instance.NewInOutPinDesc();
            //this.AddPinIn(Metallic);

            //Rough.Name = "Rough ";
            //Rough.LinkDesc = UShaderEditorStyles.Instance.NewInOutPinDesc();
            //this.AddPinIn(Rough);

            //Alpha.Name = "Alpha ";
            //Alpha.LinkDesc = UShaderEditorStyles.Instance.NewInOutPinDesc();
            //this.AddPinIn(Alpha);

            //AlphaTest.Name = "AlphaTest ";
            //AlphaTest.LinkDesc = UShaderEditorStyles.Instance.NewInOutPinDesc();
            //this.AddPinIn(AlphaTest);

            //VertexOffset.Name = "VertexOffset ";
            //VertexOffset.LinkDesc = UShaderEditorStyles.Instance.NewInOutPinDesc();
            //this.AddPinIn(VertexOffset);

            //AO.Name = "AO";
            //AO.LinkDesc = UShaderEditorStyles.Instance.NewInOutPinDesc();
            //this.AddPinIn(AO); 
        }
        [Browsable(false)]
        public List<PinIn> FieldPins { get; } = new List<PinIn>();
        
        //[Browsable(false)]
        //public PinIn Albedo { get; set; } = new PinIn();

        //[Browsable(false)]
        //public PinIn Emissive { get; set; } = new PinIn();
        //[Browsable(false)]
        //public PinIn Normal { get; set; } = new PinIn();
        //[Browsable(false)]
        //public PinIn Metallic { get; set; } = new PinIn();
        //[Browsable(false)]
        //public PinIn Rough { get; set; } = new PinIn();
        //[Browsable(false)]
        //public PinIn Alpha { get; set; } = new PinIn();
        //[Browsable(false)]
        //public PinIn AlphaTest { get; set; } = new PinIn();
        //[Browsable(false)]
        //public PinIn VertexOffset { get; set; } = new PinIn();
        //[Browsable(false)]
        //public PinIn AO { get; set; } = new PinIn();
        public override bool CanLinkFrom(PinIn iPin, UNodeBase OutNode, PinOut oPin)
        {
            if (base.CanLinkFrom(iPin, OutNode, oPin) == false)
                return false;

            var nodeExpr = OutNode as UNodeBase;
            var type = nodeExpr.GetOutPinType(oPin);

            if (iPin == FindPinIn("Albedo") ||
                iPin == FindPinIn("Emissive") ||
                iPin == FindPinIn("Normal") ||
                iPin == FindPinIn("VertexOffset"))
            {
                if (!type.IsEqual(typeof(Vector3)))
                    return false;
            }
            else if (iPin == FindPinIn("Metallic") ||
                iPin == FindPinIn("Rough") ||
                iPin == FindPinIn("AO") ||
                iPin == FindPinIn("Alpha") ||
                iPin == FindPinIn("AlphaTest"))
            {
                if (!type.IsEqual(typeof(float)))
                    return false;
            }

            return true;
        }

        public List<EngineNS.NxRHI.EVertexStreamType> GetVSNeedStreams()
        {
            var result = new List<EngineNS.NxRHI.EVertexStreamType>();

            foreach (var i in this.ParentGraph.Nodes)
            {
                if (i.GetType() == typeof(Bricks.CodeBuilder.ShaderNode.Control.SampleLevel2DNode) ||
                    i.GetType() == typeof(Bricks.CodeBuilder.ShaderNode.Control.Sample2DNode) ||
                    i.GetType() == typeof(Bricks.CodeBuilder.ShaderNode.Control.SampleArrayLevel2DNode) ||
                    i.GetType() == typeof(Bricks.CodeBuilder.ShaderNode.Control.SampleArray2DNode))
                {
                    if (result.Contains(EngineNS.NxRHI.EVertexStreamType.VST_UV) == false)
                        result.Add(EngineNS.NxRHI.EVertexStreamType.VST_UV);
                }
                else
                {
                    if (i.Name.StartsWith("input."))
                    {
                        var name = i.Name.Substring("input.".Length);
                        var t = Graphics.Pipeline.Shader.VS_INPUT.NameToInputStream(name);
                        if (t != EngineNS.NxRHI.EVertexStreamType.VST_Number)
                        {
                            if (result.Contains(t) == false)
                                result.Add(t);
                        }
                    }
                }
            }

            return result;
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
                        var t = Graphics.Pipeline.Shader.PS_INPUT.NameToInput(name);
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

            foreach (var i in FieldPins)
            {
                if (i.HasLinker())
                {
                    var linker = graph.FindInLinkerSingle(i);
                    var opPin = graph.GetOppositePin(i);
                    var pinNode = graph.GetOppositePinNode(i);
                    pinNode.BuildStatements(opPin, ref data);
                    var exp = graph.GetOppositePinExpression(i, ref data);
                    var assign = new UAssignOperatorStatement()
                    {
                        From = exp,
                        To = new UVariableReferenceExpression("m" + i.Name, new UVariableReferenceExpression("mtl")),
                    };
                    PSFunction.MethodBody.Sequence.Add(assign);
                }
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
        }
    }
}
