using EngineNS.EGui.Controls.NodeGraph;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.CodeBuilder.ShaderNode
{
    public partial class IBaseNode : EGui.Controls.NodeGraph.NodeBase
    {
        internal UMaterialGraph Graph;
        public float PreviewWidth = 0;
        public override void OnPreRead(object tagObject, object hostObject, bool fromXml)
        {
            base.OnPreRead(tagObject, hostObject, fromXml);
            var editor = tagObject as UShaderEditor;
            if (editor != null)
            {
                Graph = editor.MaterialGraph;
            }
        }
        public virtual void OnMaterialEditorGenCode(Bricks.CodeBuilder.HLSL.UHLSLGen gen, Graphics.Pipeline.Shader.UMaterial material)
        {

        }
        public virtual string GetTitleName()
        {
            return Name;
        }
        public unsafe override void OnDraw(EGui.Controls.NodeGraph.NodeGraphStyles styles)
        {
            this.Size = Measure(styles);
            float fScale = ParentGraph.ScaleFactor;
            var ScaledNodeSize = this.Size * fScale;

            var cmdlist = new ImDrawList(ImGuiAPI.GetWindowDrawList());
            ImGuiAPI.SetWindowFontScale(fScale);

            var nameSize = ImGuiAPI.CalcTextSize(GetTitleName(), false, -1.0f);

            var start = DrawPosition;
            var nodeMin = start;
            var nodeMax = nodeMin + new Vector2(ScaledNodeSize.X, nameSize.Y);
            TitleImage.OnDraw(ref cmdlist, in nodeMin, in nodeMax, 0);
            cmdlist.AddText(in nodeMin, Icon.Color, GetTitleName(), null);

            nodeMax = nodeMin + ScaledNodeSize;
            nodeMin.Y += nameSize.Y;
            Background.OnDraw(ref cmdlist, in nodeMin, in nodeMax, 0);

            {
                var prevStart = nodeMin;
                prevStart.X += (this.Size.X - PreviewWidth) * fScale / 2;
                var prevEnd = prevStart + new Vector2(PreviewWidth * fScale);

                OnPreviewDraw(ref prevStart, ref prevEnd);
            }

            var rectMin = new Vector2(nodeMin.X, nodeMin.Y);
            var lines = Math.Max(Inputs.Count, Outputs.Count);
            for (int i = 0; i < lines; i++)
            {
                rectMin = nodeMin;
                float lineHeight = 0;
                float extPadding = 0;
                if (i < Inputs.Count)
                {
                    var inIcon = styles.PinInStyle.Image;
                    if (Inputs[i].Link != null)
                    {
                        inIcon = Inputs[i].Link.Icon;
                    }

                    var rectMax = rectMin + inIcon.Size * fScale;
                    inIcon.OnDraw(ref cmdlist, in rectMin, in rectMax, 0);
                    Inputs[i].Offset = (rectMin - start) / ParentGraph.ScaleFactor;

                    nameSize = ImGuiAPI.CalcTextSize(Inputs[i].Name, false, -1.0f);
                    rectMin.X -= nameSize.X;
                    cmdlist.AddText(in rectMin, Icon.Color, Inputs[i].Name, null);

                    SetIfBigger(ref lineHeight, inIcon.Size.Y * fScale);
                    if (Inputs[i].Link != null)
                    {
                        SetIfBigger(ref extPadding, Inputs[i].Link.ExtPadding * fScale);
                    }
                }
                if (i < Outputs.Count)
                {
                    var inIcon = styles.PinInStyle.Image;
                    if (Outputs[i].Link != null)
                    {
                        inIcon = Outputs[i].Link.Icon;
                    }

                    rectMin.X = nodeMax.X - inIcon.Size.X * fScale;
                    var rectMax = rectMin + inIcon.Size * fScale;
                    inIcon.OnDraw(ref cmdlist, in rectMin, in rectMax, 0);
                    Outputs[i].Offset = (rectMin - start) / ParentGraph.ScaleFactor;

                    rectMin.X = rectMax.X;
                    cmdlist.AddText(in rectMin, Icon.Color, Outputs[i].Name, null);

                    SetIfBigger(ref lineHeight, inIcon.Size.Y * fScale);
                    if (Outputs[i].Link != null)
                    {
                        SetIfBigger(ref extPadding, Outputs[i].Link.ExtPadding * fScale);
                    }
                }
                nodeMin.Y += (lineHeight + styles.PinPadding * fScale + extPadding);
            }

            if (Selected)
            {
                cmdlist.AddRect(in start, in nodeMax, styles.SelectedColor, 0, ImDrawFlags_.ImDrawFlags_RoundCornersAll, 2);
            }
            ImGuiAPI.SetWindowFontScale(1.0f);
        }
        public virtual void OnPreviewDraw(ref Vector2 prevStart, ref Vector2 prevEnd)
        {

        }
        public Vector2 Measure(EGui.Controls.NodeGraph.NodeGraphStyles styles)
        {
            Vector2 size = new Vector2(0);
            var nameSize = ImGuiAPI.CalcTextSize(Name, false, -1.0f);

            size.Y += nameSize.Y;
            SetIfBigger(ref size.X, nameSize.X);
            var lines = Math.Max(Inputs.Count, Outputs.Count);
            for (int i = 0; i < lines; i++)
            {
                float lineWidth = 0;
                float lineHeight = 0;
                float extPadding = 0;
                if (i < Inputs.Count)
                {
                    var inIcon = styles.PinInStyle.Image;
                    if (Inputs[i].Link != null)
                    {
                        inIcon = Inputs[i].Link.Icon;
                    }
                    lineWidth += styles.PinInStyle.Offset;
                    lineWidth += inIcon.Size.X;
                    lineWidth += styles.PinSpacing;

                    nameSize = ImGuiAPI.CalcTextSize(Inputs[i].Name, false, -1.0f);
                    SetIfBigger(ref lineHeight, inIcon.Size.Y);
                    SetIfBigger(ref lineHeight, nameSize.Y);

                    if (Inputs[i].Link != null)
                    {
                        SetIfBigger(ref extPadding, Inputs[i].Link.ExtPadding);
                    }
                }
                if (i < Outputs.Count)
                {
                    var inIcon = styles.PinOutStyle.Image;
                    if (Outputs[i].Link != null)
                    {
                        inIcon = Outputs[i].Link.Icon;
                    }
                    lineWidth += styles.PinOutStyle.Offset;
                    lineWidth += inIcon.Size.X;
                    lineWidth += styles.PinSpacing;

                    nameSize = ImGuiAPI.CalcTextSize(Outputs[i].Name, false, -1.0f);
                    SetIfBigger(ref lineHeight, inIcon.Size.Y);
                    SetIfBigger(ref lineHeight, nameSize.Y);

                    if (Outputs[i].Link != null)
                    {
                        SetIfBigger(ref extPadding, Outputs[i].Link.ExtPadding);
                    }
                }
                SetIfBigger(ref size.X, lineWidth);
                size.Y += (lineHeight + styles.PinPadding + extPadding);
            }

            size.X += PreviewWidth;
            SetIfBigger(ref size.Y, PreviewWidth + nameSize.Y);
            return size;
        }

        public virtual System.Type GetOutPinType(EGui.Controls.NodeGraph.PinOut pin)
        {
            return null;
        }
        public override void OnLinkedFrom(PinIn iPin, NodeBase OutNode, PinOut oPin)
        {
            var funcGraph = ParentGraph as UMaterialGraph;
            if (funcGraph == null || oPin.Link == null || iPin.Link == null)
            {
                return;
            }
            if (iPin.Link.CanLinks.Contains("Value"))
            {
                funcGraph.RemoveLinkedInExcept(iPin, OutNode, oPin.Name);
            }
        }
        public override void OnLClicked(NodePin clickedPin)
        {
            //if (HasError)
            //{
            //    if (Graph != null)
            //    {
            //        Graph.ShaderEditor.NodePropGrid.SingleTarget = CodeExcept;
            //    }
            //    return;
            //}

            Graph.ShaderEditor.NodePropGrid.HideInheritDeclareType = null;

            if (GetPropertyEditObject() == null)
            {
                if (Graph != null)
                {
                    Graph.ShaderEditor.NodePropGrid.Target = null;
                }
                return;
            }

            Graph.ShaderEditor.NodePropGrid.HideInheritDeclareType = Rtti.UTypeDescGetter<IBaseNode>.TypeDesc;
            Graph.ShaderEditor.NodePropGrid.Target = GetPropertyEditObject();
        }
        protected virtual object GetPropertyEditObject()
        {
            return this;
        }
        public virtual void PreGenExpr()
        {

        }
        public virtual IExpression GetExpr(UMaterialGraph funGraph, ICodeGen cGen, PinOut oPin, bool bTakeResult)
        {
            return null;
        }
    }
}
