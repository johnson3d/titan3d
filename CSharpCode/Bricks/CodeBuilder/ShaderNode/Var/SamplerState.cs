using EngineNS.Graphics.Pipeline.Shader;
using System;
using System.Collections.Generic;
using EngineNS.Bricks.NodeGraph;
using System.ComponentModel;

namespace EngineNS.Bricks.CodeBuilder.ShaderNode.Var
{
    [ContextMenu("sampler", "Data\\Sampler@_serial@", UMaterialGraph.MaterialEditorKeyword)]
    public class SamplerState : VarNode
    {
        [Browsable(false)]
        public PinOut OutSampler { get; set; } = new PinOut();
        public SamplerState()
        {
            VarType = Rtti.UTypeDescGetter<SamplerState>.TypeDesc;
            
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF40FF40;
            TitleColor = 0xFF804020;
            BackColor = 0x80808080;

            OutSampler.Name = "sampler";
            OutSampler.LinkDesc = UShaderEditorStyles.Instance.NewInOutPinDesc();
            OutSampler.MultiLinks = true;
            this.AddPinOut(OutSampler);

            mDesc.SetDefault();
        }
        //public override void OnMaterialEditorGenCode(UMaterial Material)
        //{
        //    var tmp = new Graphics.Pipeline.Shader.UMaterial.NameSamplerStateDescPair();
        //    tmp.Name = this.Name;
        //    var sampNode = this;
        //    tmp.Value = sampNode.Desc;
        //    Material.UsedSamplerStates.Add(tmp);
        //}
        public override Rtti.UTypeDesc GetOutPinType(PinOut pin)
        {
            return VarType;
        }
        NxRHI.FSamplerDesc mDesc;
        [Rtti.Meta]
        public NxRHI.FSamplerDesc Desc { get => mDesc; set => mDesc = value; }
        //public override IExpression GetExpr(UMaterialGraph funGraph, ICodeGen cGen, PinOut oPin, bool bTakeResult)
        //{
        //    var Var = new OpUseVar(this.Name, false);
        //    return Var;
        //}

        public override void BuildStatements(NodePin pin, ref BuildCodeStatementsData data)
        {
            var material = data.UserData as TtMaterial;
            if(material.FindSampler(this.Name) == null)
            {
                var tmp = new Graphics.Pipeline.Shader.TtMaterial.NameSamplerStateDescPair();
                tmp.Name = this.Name;
                var sampNode = this;
                tmp.Value = sampNode.Desc;
                material.UsedSamplerStates.Add(tmp);
            }
        }
        public override UExpressionBase GetExpression(NodePin pin, ref BuildCodeStatementsData data)
        {
            return new UVariableReferenceExpression(Name);
        }
    }
}
