using EngineNS.Graphics.Pipeline.Shader;
using System;
using System.Collections.Generic;
using EngineNS.Bricks.NodeGraph;
using System.ComponentModel;

namespace EngineNS.Bricks.CodeBuilder.ShaderNode.Var
{
    [ContextMenu("texture2d", "Data\\Texture2D@_serial@", UMaterialGraph.MaterialEditorKeyword)]
    public class Texture2D : VarNode
    {
        [Browsable(false)]
        public PinOut OutTex { get; set; } = new PinOut();
        [Rtti.Meta]
        [RName.PGRName(FilterExts = NxRHI.USrView.AssetExt)]
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
                    TextureSRV = await TtEngine.Instance.GfxDevice.TextureManager.GetTexture(value);
                };
                exec();
            }
        }
        private NxRHI.USrView TextureSRV;
        public Texture2D()
        {
            VarType = Rtti.UTypeDescGetter<Texture2D>.TypeDesc;
            PrevSize = new Vector2(100, 100);

            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF40FF40;
            TitleColor = 0xFF804020;
            BackColor = 0x80808080;

            OutTex.Name = "texture";
            OutTex.LinkDesc = UShaderEditorStyles.Instance.NewInOutPinDesc();
            OutTex.MultiLinks = true;
            this.AddPinOut(OutTex);
        }
        ~Texture2D()
        {
            
        }
        //public override void OnMaterialEditorGenCode(UMaterial Material)
        //{
        //    var tmp = new Graphics.Pipeline.Shader.UMaterial.NameRNamePair();
        //    tmp.Name = this.Name;
        //    var texNode = this;
        //    tmp.Value = texNode.AssetName;
        //    Material.UsedRSView.Add(tmp);
        //}
        public override Rtti.UTypeDesc GetOutPinType(PinOut pin)
        {
            return VarType;
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
            var graph = UserData as UMaterialGraph;
            graph.ShaderEditor.NodePropGrid.HideInheritDeclareType = Rtti.UTypeDescGetter<VarNode>.TypeDesc;
        }
        //public override IExpression GetExpr(UMaterialGraph funGraph, ICodeGen cGen, PinOut oPin, bool bTakeResult)
        //{
        //    var Var = new OpUseVar(this.Name, false);
        //    return Var;
        //}

        public override void BuildStatements(NodePin pin, ref BuildCodeStatementsData data)
        {
            var material = data.UserData as TtMaterial;
            if(material.FindSRV(this.Name) == null)
            {
                var tmp = new Graphics.Pipeline.Shader.TtMaterial.NameRNamePair();
                tmp.Name = this.Name;
                var texNode = this;
                tmp.Value = texNode.AssetName;
                material.UsedRSView.Add(tmp);
            }
        }
        public override UExpressionBase GetExpression(NodePin pin, ref BuildCodeStatementsData data)
        {
            return new UVariableReferenceExpression(Name);
        }
    }
    [ContextMenu("texture2darray", "Data\\Texture2DArray@_serial@", UMaterialGraph.MaterialEditorKeyword)]
    public class Texture2DArray : VarNode
    {
        [Browsable(false)]
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
            OutTex.LinkDesc = UShaderEditorStyles.Instance.NewInOutPinDesc();
            OutTex.MultiLinks = true;
            this.AddPinOut(OutTex);
        }
        public override Rtti.UTypeDesc GetOutPinType(PinOut pin)
        {
            return VarType;
        }
    }
}
