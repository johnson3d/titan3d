using EngineNS.Graphics.Pipeline.Shader;
using System;
using System.Collections.Generic;
using EngineNS.Bricks.NodeGraph;

namespace EngineNS.Bricks.CodeBuilder.ShaderNode.Var
{
    public class SamplerState : VarNode
    {
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinOut OutSampler { get; set; } = new PinOut();
        public SamplerState()
        {
            VarType = Rtti.UTypeDescGetter<SamplerState>.TypeDesc;
            
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF40FF40;
            TitleColor = 0xFF804020;
            BackColor = 0x80808080;

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
        public override System.Type GetOutPinType(PinOut pin)
        {
            return VarType.SystemType;
        }
        ISamplerStateDesc mDesc;
        public ISamplerStateDesc Desc { get => mDesc; set => mDesc = value; }
        public override IExpression GetExpr(UMaterialGraph funGraph, ICodeGen cGen, PinOut oPin, bool bTakeResult)
        {
            var Var = new OpUseVar(this.Name, false);
            return Var;
        }
    }
}
