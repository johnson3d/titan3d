using EngineNS.Graphics.Pipeline.Shader;
using System;
using System.Collections.Generic;
using EngineNS.Bricks.NodeGraph;
using System.ComponentModel;

namespace EngineNS.Bricks.CodeBuilder.ShaderNode.Var
{
    [ContextMenu("texture2d", "Data\\Texture2D@_serial@", TtMaterialGraph.MaterialEditorKeyword)]
    public class Texture2D : VarNode
    {
        [Browsable(false)]
        public PinOut OutTex { get; set; } = new PinOut();
        RName mAssetName;
        [Rtti.Meta]
        [RName.PGRName(FilterExts = NxRHI.TtSrView.AssetExt)]
        [Category("Option")]
        public RName AssetName
        {
            get
            {
                if (TextureSRV == null)
                    return mAssetName;
                return TextureSRV.AssetName;
            }
            set
            {
                mAssetName = value;
                System.Action exec = async () =>
                {
                    TextureSRV = await TtEngine.Instance.GfxDevice.TextureManager.GetTexture(value);
                    mSlateEffect = await TtEngine.Instance.GfxDevice.EffectManager.GetEffect(
                        await TtEngine.Instance.ShadingEnvManager.GetShadingEnv<EngineNS.Editor.Forms.USlateTextureViewerShading>(),
                        TtEngine.Instance.GfxDevice.MaterialManager.ScreenMaterial, new Graphics.Mesh.TtMdfStaticMesh());
                };
                exec();
            }
        }
        public Texture2D()
        {
            VarType = Rtti.TtTypeDescGetter<Texture2D>.TypeDesc;
            PrevSize = new Vector2(100, 100);

            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF40FF40;
            TitleColor = 0xFF804020;
            BackColor = 0x80808080;

            OutTex.Name = "texture";
            OutTex.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            OutTex.MultiLinks = true;
            this.AddPinOut(OutTex);
        }
        ~Texture2D()
        {
            
        }
        public override Rtti.TtTypeDesc GetOutPinType(PinOut pin)
        {
            return VarType;
        }
        TtEffect mSlateEffect;
        EngineNS.Editor.Forms.TtTextureViewerCmdParams CmdParameters = null;
        NxRHI.FSamplerDesc mSampler;
        [Rtti.Meta]
        [Category("Option")]
        public NxRHI.FSamplerDesc Sampler { get => mSampler; set => mSampler = value; }
        private NxRHI.TtSrView TextureSRV;
        public static unsafe void PreviewDraw(ref Editor.Forms.TtTextureViewerCmdParams CmdParameters, TtEffect mSlateEffect, NxRHI.TtSrView TextureSRV, 
            in Vector2 prevStart, in Vector2 prevEnd, ImDrawList cmdlist)
        {
            if (TextureSRV == null || mSlateEffect == null)
                return;

            if (CmdParameters == null)
            {
                var rc = TtEngine.Instance.GfxDevice.RenderContext;

                var iptDesc = new NxRHI.TtInputLayoutDesc();
                unsafe
                {
                    iptDesc.mCoreObject.AddElement("POSITION", 0, EPixelFormat.PXF_R32G32_FLOAT, 0, 0, 0, 0);
                    iptDesc.mCoreObject.AddElement("TEXCOORD", 0, EPixelFormat.PXF_R32G32_FLOAT, 0, (uint)sizeof(Vector2), 0, 0);
                    iptDesc.mCoreObject.AddElement("COLOR", 0, EPixelFormat.PXF_R8G8B8A8_UNORM, 0, (uint)sizeof(Vector2) * 2, 0, 0);
                    //iptDesc.SetShaderDesc(SlateEffect.GraphicsEffect);
                }
                iptDesc.mCoreObject.SetShaderDesc(mSlateEffect.DescVS.mCoreObject);
                var InputLayout = rc.CreateInputLayout(iptDesc); //TtEngine.Instance.GfxDevice.InputLayoutManager.GetPipelineState(rc, iptDesc);
                mSlateEffect.ShaderEffect.mCoreObject.BindInputLayout(InputLayout.mCoreObject);

                var cmdParams = EGui.TtImDrawCmdParameters.CreateInstance<EngineNS.Editor.Forms.TtTextureViewerCmdParams>();
                var cbBinder = mSlateEffect.ShaderEffect.FindBinder("ProjectionMatrixBuffer");
                cmdParams.CBuffer = rc.CreateCBV(cbBinder);
                cmdParams.Drawcall.BindShaderEffect(mSlateEffect);
                cmdParams.Drawcall.BindCBuffer(cbBinder.mCoreObject, cmdParams.CBuffer);
                cmdParams.Drawcall.BindSRV(TtNameTable.FontTexture, TextureSRV);
                cmdParams.Drawcall.BindSampler(TtNameTable.Samp_FontTexture, TtEngine.Instance.GfxDevice.SamplerStateManager.PointState);

                cmdParams.IsNormalMap = 0;
                if (TextureSRV.PicDesc.Format == EPixelFormat.PXF_BC5_UNORM || TextureSRV.PicDesc.Format == EPixelFormat.PXF_BC5_TYPELESS || TextureSRV.PicDesc.Format == EPixelFormat.PXF_BC5_SNORM)
                    cmdParams.IsNormalMap = 1;

                CmdParameters = cmdParams;
            }

            var uv0 = new Vector2(0, 0);
            var uv1 = new Vector2(1, 1);
            cmdlist.AddImage((ulong)CmdParameters.GetHandle(), in prevStart, in prevEnd, in uv0, in uv1, 0xFFFFFFFF);
        }
        public unsafe override void OnPreviewDraw(in Vector2 prevStart, in Vector2 prevEnd, ImDrawList cmdlist)
        {
            if (TextureSRV == null || mSlateEffect == null)
                return;

            PreviewDraw(ref CmdParameters, mSlateEffect, TextureSRV, in prevStart, in prevEnd, cmdlist);
        }
        public override void OnLButtonClicked(NodePin clickedPin)
        {
            base.OnLButtonClicked(clickedPin);
            //var graph = UserData as TtMaterialGraphBase;
            //if (graph != null)
            //{
            //    graph.ShaderEditor.NodePropGrid.HideInheritDeclareType = Rtti.TtTypeDescGetter<VarNode>.TypeDesc;
            //}
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
                material.UsedSrView.Add(tmp);
            }
        }
        public override TtExpressionBase GetExpression(NodePin pin, ref BuildCodeStatementsData data)
        {
            return new TtVariableReferenceExpression(Name);
        }
    }
    [ContextMenu("texture2darray", "Data\\Texture2DArray@_serial@", TtMaterialGraph.MaterialEditorKeyword)]
    public class Texture2DArray : VarNode
    {
        [Browsable(false)]
        public PinOut OutTex { get; set; } = new PinOut();
        public Texture2DArray()
        {
            VarType = Rtti.TtTypeDescGetter<Texture2DArray>.TypeDesc;
            PrevSize = new Vector2(100, 100);

            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF40FF40;
            TitleColor = 0xFF804020;
            BackColor = 0x80808080;

            OutTex.Name = "texture";
            OutTex.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            OutTex.MultiLinks = true;
            this.AddPinOut(OutTex);
        }
        public override Rtti.TtTypeDesc GetOutPinType(PinOut pin)
        {
            return VarType;
        }
    }
}
