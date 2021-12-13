using EngineNS.Graphics.Pipeline.Shader;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.CodeBuilder.ShaderNode.Var
{
    public class SamplerState : VarNode
    {
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public EGui.Controls.NodeGraph.PinOut OutSampler { get; set; } = new EGui.Controls.NodeGraph.PinOut();
        public SamplerState()
        {
            VarType = Rtti.UTypeDescGetter<SamplerState>.TypeDesc;
            PreviewWidth = 0;

            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF40FF40;
            TitleImage.Color = 0xFF804020;
            Background.Color = 0x80808080;

            OutSampler.Name = "sampler";
            OutSampler.Link = UShaderEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinOut(OutSampler);

            mDesc.SetDefault();
        }
        public override void OnMaterialEditorGenCode(Bricks.CodeBuilder.HLSL.UHLSLGen gen, UMaterial Material)
        {
            var tmp = new Graphics.Pipeline.Shader.UMaterial.NameSamplerStateDescPair();
            tmp.Name = this.Name;
            var sampNode = this;
            tmp.Value = sampNode.Desc;
            Material.UsedSamplerStates.Add(tmp);
        }
        public override System.Type GetOutPinType(EGui.Controls.NodeGraph.PinOut pin)
        {
            return VarType.SystemType;
        }
        ISamplerStateDesc mDesc;
        public ISamplerStateDesc Desc { get => mDesc; set => mDesc = value; }
        public override IExpression GetExpr(UMaterialGraph funGraph, ICodeGen cGen, EGui.Controls.NodeGraph.PinOut oPin, bool bTakeResult)
        {
            var Var = new OpUseVar(this.Name, false);
            return Var;
        }
    }
}
