using EngineNS.EGui.Controls.NodeGraph;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.CodeBuilder.ShaderNode.Var
{
    public class Texture2D : VarNode
    {
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public EGui.Controls.NodeGraph.PinOut OutTex { get; set; } = new EGui.Controls.NodeGraph.PinOut();
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
                if (SnapshotPtr != IntPtr.Zero)
                {
                    var handle = System.Runtime.InteropServices.GCHandle.FromIntPtr(SnapshotPtr);
                    handle.Free();
                    SnapshotPtr = IntPtr.Zero;
                }
                System.Action exec = async () =>
                {
                    TextureSRV = await UEngine.Instance.GfxDevice.TextureManager.GetTexture(value);
                };
                exec();
            }
        }
        private RHI.CShaderResourceView TextureSRV;
        IntPtr SnapshotPtr;
        public Texture2D()
        {
            VarType = Rtti.UTypeDescGetter<Texture2D>.TypeDesc;
            PreviewWidth = 100;

            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF40FF40;
            TitleImage.Color = 0xFF804020;
            Background.Color = 0x80808080;

            OutTex.Name = "texture";
            OutTex.Link = UShaderEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinOut(OutTex);
        }
        ~Texture2D()
        {
            if (SnapshotPtr != IntPtr.Zero)
            {
                var handle = System.Runtime.InteropServices.GCHandle.FromIntPtr(SnapshotPtr);
                handle.Free();
                SnapshotPtr = IntPtr.Zero;
            }
        }
        public override System.Type GetOutPinType(EGui.Controls.NodeGraph.PinOut pin)
        {
            return VarType.SystemType;
        }
        public unsafe override void OnPreviewDraw(ref Vector2 prevStart, ref Vector2 prevEnd)
        {
            if (TextureSRV == null)
                return;

            var cmdlist = new ImDrawList(ImGuiAPI.GetWindowDrawList());

            if (SnapshotPtr == IntPtr.Zero)
            {
                SnapshotPtr = System.Runtime.InteropServices.GCHandle.ToIntPtr(System.Runtime.InteropServices.GCHandle.Alloc(TextureSRV));
            }

            var uv0 = new Vector2(0, 0);
            var uv1 = new Vector2(1, 1);
            unsafe
            {
                cmdlist.AddImage(SnapshotPtr.ToPointer(), in prevStart, in prevEnd, in uv0, in uv1, 0xFFFFFFFF);
            }
        }
        public override void OnLClicked(NodePin clickedPin)
        {
            base.OnLClicked(clickedPin);

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
        public EGui.Controls.NodeGraph.PinOut OutTex { get; set; } = new EGui.Controls.NodeGraph.PinOut();
        public Texture2DArray()
        {
            VarType = Rtti.UTypeDescGetter<Texture2DArray>.TypeDesc;
            PreviewWidth = 100;

            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF40FF40;
            TitleImage.Color = 0xFF804020;
            Background.Color = 0x80808080;

            OutTex.Name = "texture";
            OutTex.Link = UShaderEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinOut(OutTex);
        }
        public override System.Type GetOutPinType(EGui.Controls.NodeGraph.PinOut pin)
        {
            return VarType.SystemType;
        }
    }
}
