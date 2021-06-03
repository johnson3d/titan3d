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
        }
        public override System.Type GetOutPinType(EGui.Controls.NodeGraph.PinOut pin)
        {
            return VarType.SystemType;
        }
    }
}
