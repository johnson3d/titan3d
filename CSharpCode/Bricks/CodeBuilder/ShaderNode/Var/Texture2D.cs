using EngineNS.Graphics.Pipeline.Shader;
using System;
using System.Collections.Generic;
using EngineNS.Bricks.NodeGraph;

namespace EngineNS.Bricks.CodeBuilder.ShaderNode.Var
{
    public class Texture2D : VarNode
    {
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinOut OutTex { get; set; } = new PinOut();
        [Rtti.Meta]
        [RName.PGRName(FilterExts = RHI.CShaderResourceView.AssetExt)]
        [System.ComponentModel.Browsable(false)]
        public RName AssetName
        {
            get
            {
                if (TextureSRV == null)
                    return null;
                return TextureSRV.AssetName;
            }
            set
            {
                System.Action exec = async () =>
                {
                    TextureSRV = await UEngine.Instance.GfxDevice.TextureManager.GetTexture(value);
                };
                exec();
            }
        }
        private RHI.CShaderResourceView TextureSRV;
        public Texture2D()
        {
            VarType = Rtti.UTypeDescGetter<Texture2D>.TypeDesc;
            PrevSize = new Vector2(100, 100);

            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF40FF40;
            TitleColor = 0xFF804020;
            BackColor = 0x80808080;

            OutTex.Name = "texture";
            OutTex.Link = UShaderEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinOut(OutTex);
        }
        ~Texture2D()
        {
            
        }
        public override void OnMaterialEditorGenCode(Bricks.CodeBuilder.HLSL.UHLSLGen gen, UMaterial Material)
        {
            var tmp = new Graphics.Pipeline.Shader.UMaterial.NameRNamePair();
            tmp.Name = this.Name;
            var texNode = this;
            tmp.Value = texNode.AssetName;
            Material.UsedRSView.Add(tmp);
        }
        public override System.Type GetOutPinType(PinOut pin)
        {
            return VarType.SystemType;
        }
        public unsafe override void OnPreviewDraw(in Vector2 prevStart, in Vector2 prevEnd, ImDrawList cmdlist)
        {
            if (TextureSRV == null)
                return;

            var uv0 = new Vector2(0, 0);
            var uv1 = new Vector2(1, 1);
            unsafe
            {
                cmdlist.AddImage(TextureSRV.GetTextureHandle().ToPointer(), in prevStart, in prevEnd, in uv0, in uv1, 0xFFFFFFFF);
            }
        }
        public override void OnLButtonClicked(NodePin clickedPin)
        {
            base.OnLButtonClicked(clickedPin);

            Graph.ShaderEditor.NodePropGrid.HideInheritDeclareType = Rtti.UTypeDescGetter<VarNode>.TypeDesc;
        }
        public override IExpression GetExpr(UMaterialGraph funGraph, ICodeGen cGen, PinOut oPin, bool bTakeResult)
        {
            var Var = new OpUseVar(this.Name, false);
            return Var;
        }
    }
    public class Texture2DArray : VarNode
    {
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinOut OutTex { get; set; } = new PinOut();
        public Texture2DArray()
        {
            VarType = Rtti.UTypeDescGetter<Texture2DArray>.TypeDesc;
            PrevSize = new Vector2(100, 100);

            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF40FF40;
            TitleColor = 0xFF804020;
            BackColor = 0x80808080;

            OutTex.Name = "texture";
            OutTex.Link = UShaderEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinOut(OutTex);
        }
        public override System.Type GetOutPinType(PinOut pin)
        {
            return VarType.SystemType;
        }
    }
}
