using System;
using System.Collections.Generic;
using System.Text;
using EngineNS;

namespace CSharpCode.Controls.NodeGraph
{
    public class EditableValue
    {
        public EditableValue()
        {
            LabelName = "EVL_" + System.Threading.Interlocked.Add(ref GID_EditableValue, 1).ToString();
        }
        private string LabelName;
        public Type ValueType { get; set; }
        public object Value { get; set; }
        public float ControlWidth { get; set; } = 30;
        private static int GID_EditableValue = 0;
        public virtual void OnDraw(NodeBase node, int index, NodeGraphStyles styles, float fScale)
        {
            //LabelName目前每次构造时确保id唯一，也许以后可以找到类似
            //GetObjectHandleAddress这样的方法替换
            if (ValueType==typeof(int))
            {
                ImGuiAPI.PushItemWidth(ControlWidth * fScale);
                var v = System.Convert.ToInt32(Value);
                ImGuiAPI.InputInt($"##{LabelName}", ref v, -1, 100, ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
                Value = v;
                ImGuiAPI.PopItemWidth();
            }
            else if (ValueType == typeof(float))
            {
                ImGuiAPI.PushItemWidth(ControlWidth * fScale);
                var v = System.Convert.ToSingle(Value);
                ImGuiAPI.InputFloat($"##{LabelName}", ref v, 0.1f, 10.0f, "%.3f", ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
                Value = v;
                ImGuiAPI.PopItemWidth();
            }
        }
    }

    public class NodePin
    {
        public string Name { get; set; }
        public NodeBase HostNode { get; set; }
        public Vector2 Offset { get; set; }
        public Vector2 DrawPosition
        {
            get
            {
                return HostNode.DrawPosition + Offset * HostNode.ParentGraph.ScaleFactor;
            }
        }
        public object Tag { get; set; }
    }
    public class PinIn : NodePin
    {
        public EditableValue EditValue { get; set; } = null;
    }
    public class PinOut : NodePin
    {
        
    }
    public class NodeBase
    {
        public NodeBase()
        {
            Icon.Color = 0xFF00FF00;
            TitleImage.Color = 0xFF402020;
            Background.Color = 0x80808080;
        }
        public Guid NodeId
        {
            get;
        } = Guid.NewGuid();
        public NodeGraph ParentGraph { get; set; }
        public string Name
        {
            get;
            set;
        }
        public bool Visible { get; set; } = true;
        public bool Selected { get; set; }
        internal Vector2 OffsetOfDragPoint { get; set; }
        Vector2 mPosition;
        public Vector2 Position
        {
            get => mPosition;
            set => mPosition = value;
        }
        public Vector2 Size { get; set; }
        public Vector2 DrawPosition
        {
            get { return ParentGraph.WindowPos + (Position - ParentGraph.ViewPortPosition) * ParentGraph.ScaleFactor; }
        }
        public Vector2 DrawSize
        {
            get { return Size * ParentGraph.ScaleFactor; }
        }
        public UVAnim Icon { get; set; } = new UVAnim();
        public UVAnim TitleImage { get; set; } = new UVAnim();
        public UVAnim Background { get; set; } = new UVAnim();
        public List<PinIn> Inputs
        {
            get;
        } = new List<PinIn>();
        public void AddPinIn(PinIn pin)
        {
            pin.HostNode = this;
            if (Inputs.Contains(pin))
                return;
            Inputs.Add(pin);
        }
        public PinIn FindPinIn(string name)
        {
            foreach(var i in Inputs)
            {
                if (i.Name == name)
                    return i;
            }
            return null;
        }
        public List<PinOut> Outputs
        {
            get;
        } = new List<PinOut>();
        public PinOut FindPinOut(string name)
        {
            foreach (var i in Outputs)
            {
                if (i.Name == name)
                    return i;
            }
            return null;
        }
        public void AddPinOut(PinOut pin)
        {
            pin.HostNode = this;
            if (Outputs.Contains(pin))
                return;
            Outputs.Add(pin);
        }
        public NodePin PointInPin(ref Vector2 posInView, NodeGraphStyles styles)
        {
            foreach (var i in Inputs)
            {
                var rcMin = i.DrawPosition;
                var rcMax = rcMin + styles.PinInStyle.Image.Size * ParentGraph.ScaleFactor;
                if (ImGuiAPI.PointInRect(ref posInView, ref rcMin, ref rcMax))
                    return i;
            }
            foreach (var i in Outputs)
            {
                var rcMin = i.DrawPosition;
                var rcMax = rcMin + styles.PinInStyle.Image.Size * ParentGraph.ScaleFactor;
                if (ImGuiAPI.PointInRect(ref posInView, ref rcMin, ref rcMax))
                    return i;
            }
            return null;
        }
        public virtual void OnDraw(NodeGraphStyles styles = null)
        {
            if (styles == null)
            {
                styles = NodeGraphStyles.DefaultStyles;
            }
            float fScale = ParentGraph.ScaleFactor;
            float lineWidth = 0;
            float lineHeight = 0;
            Vector2 NodeSize = new Vector2(0, 0);
            lineWidth += styles.IconOffset.X;
            lineWidth += Icon.Size.X;
            var nameSize = ImGuiAPI.CalcTextSize(Name, CppBool.FromBoolean(false), -1.0f);
            lineWidth += nameSize.X;

            SetIfBigger(ref NodeSize.X, lineWidth);

            SetIfBigger(ref lineHeight, styles.IconOffset.Y * 2 + Icon.Size.Y);
            SetIfBigger(ref lineHeight, nameSize.Y);
            NodeSize.Y += lineHeight;

            switch (styles.FlowMode)
            {
                case NodeGraphStyles.EFlowMode.Horizon:
                    {
                        var lines = Math.Max(Inputs.Count, Outputs.Count);
                        for (int i = 0; i < lines; i++)
                        {
                            lineWidth = 0;
                            lineHeight = 0;
                            if (i < Inputs.Count)
                            {
                                lineWidth += styles.PinInStyle.Offset;
                                lineWidth += styles.PinInStyle.Image.Size.X;
                                nameSize = ImGuiAPI.CalcTextSize(Inputs[i].Name, CppBool.FromBoolean(false), -1.0f);
                                lineWidth += nameSize.X;
                                if(Inputs[i].EditValue!=null)
                                {
                                    lineWidth += Inputs[i].EditValue.ControlWidth;
                                }
                                lineWidth += styles.MinSpaceInOut;

                                SetIfBigger(ref lineHeight, styles.PinInStyle.Image.Size.Y);
                                SetIfBigger(ref lineHeight, nameSize.Y);
                            }
                            if (i < Outputs.Count)
                            {
                                lineWidth += styles.PinOutStyle.Offset;
                                lineWidth += styles.PinOutStyle.Image.Size.X;
                                nameSize = ImGuiAPI.CalcTextSize(Outputs[i].Name, CppBool.FromBoolean(false), -1.0f);
                                lineWidth += nameSize.X;
                                lineWidth += styles.MinSpaceInOut;

                                SetIfBigger(ref lineHeight, styles.PinOutStyle.Image.Size.Y);
                                SetIfBigger(ref lineHeight, nameSize.Y);
                            }

                            SetIfBigger(ref NodeSize.X, lineWidth);
                            NodeSize.Y += (lineHeight + styles.PinPadding);
                        }
                    }
                    break;
                case NodeGraphStyles.EFlowMode.Vertical:
                    break;
            }

            var ScaledNodeSize = NodeSize * fScale;
            this.Size = NodeSize;
            ImGuiAPI.SetNextWindowSize(ref ScaledNodeSize, ImGuiCond_.ImGuiCond_None);
            CppBool bBorder = CppBool.FromBoolean(true);
            var drawPosition = DrawPosition;
            //if(Visible)
            {
                ImGuiAPI.SetWindowFontScale(fScale);
                var NodeMax = drawPosition + ScaledNodeSize;
                float lineY = drawPosition.Y;
                var cmdlist = new ImDrawList_PtrType(ImGuiAPI.GetWindowDrawList().NativePointer);

                lineHeight = 0;
                SetIfBigger(ref lineHeight, (styles.IconOffset.Y * 2 + Icon.Size.Y) * fScale);
                SetIfBigger(ref lineHeight, nameSize.Y * fScale);
                {
                    var rectTitleMin = drawPosition;
                    var rectTitleMax = rectTitleMin + new Vector2(ScaledNodeSize.X, lineHeight);
                    TitleImage.OnDraw(ref cmdlist, ref rectTitleMin, ref rectTitleMax);

                    rectTitleMin.Y += lineHeight;
                    rectTitleMax = NodeMax;
                    Background.OnDraw(ref cmdlist, ref rectTitleMin, ref rectTitleMax);
                }

                var rectMin = new Vector2(drawPosition.X + styles.IconOffset.X * fScale, lineY + styles.IconOffset.Y * fScale);
                var rectMax = rectMin + Icon.Size * fScale;
                Icon.OnDraw(ref cmdlist, ref rectMin, ref rectMax);
                rectMin.X = rectMax.X;
                rectMin.Y -= styles.IconOffset.Y * fScale;
                nameSize = ImGuiAPI.CalcTextSize(Name, CppBool.FromBoolean(false), -1.0f);
                cmdlist.AddText(ref rectMin, styles.TitleTextColor, Name, null);

                lineY += lineHeight;

                switch (styles.FlowMode)
                {
                    case NodeGraphStyles.EFlowMode.Horizon:
                        {
                            var lines = Math.Max(Inputs.Count, Outputs.Count);

                            for (int i = 0; i < lines; i++)
                            {
                                rectMin.Y = lineY;
                                lineHeight = 0;
                                if (i < Inputs.Count)
                                {
                                    rectMin.X = drawPosition.X + styles.PinInStyle.Offset * fScale;
                                    rectMax = rectMin + styles.PinInStyle.Image.Size * fScale;
                                    styles.PinInStyle.Image.OnDraw(ref cmdlist, ref rectMin, ref rectMax);
                                    Inputs[i].Offset = (rectMin - drawPosition) / ParentGraph.ScaleFactor;
                                    nameSize = ImGuiAPI.CalcTextSize(Inputs[i].Name, CppBool.FromBoolean(false), -1.0f);
                                    rectMin.X = rectMax.X;
                                    cmdlist.AddText(ref rectMin, styles.PinTextColor, Inputs[i].Name, null);

                                    if (Inputs[i].EditValue != null)
                                    {
                                        var ctrlPos = new Vector2(rectMax.X + nameSize.X, rectMin.Y);
                                        ctrlPos -= ImGuiAPI.GetWindowPos();
                                        ImGuiAPI.SetCursorPos(ref ctrlPos);
                                        Inputs[i].EditValue.OnDraw(this, i, styles, fScale);
                                    }

                                    SetIfBigger(ref lineHeight, styles.PinInStyle.Image.Size.Y * fScale);
                                    SetIfBigger(ref lineHeight, nameSize.Y);
                                }
                                if (i < Outputs.Count)
                                {
                                    rectMin.X = NodeMax.X - styles.PinOutStyle.Offset * fScale - styles.PinOutStyle.Image.Size.X * fScale;
                                    rectMax = rectMin + styles.PinOutStyle.Image.Size * fScale;
                                    styles.PinOutStyle.Image.OnDraw(ref cmdlist, ref rectMin, ref rectMax);
                                    Outputs[i].Offset = (rectMin - drawPosition)/ParentGraph.ScaleFactor;
                                    nameSize = ImGuiAPI.CalcTextSize(Outputs[i].Name, CppBool.FromBoolean(false), -1.0f);
                                    rectMin.X -= nameSize.X;
                                    cmdlist.AddText(ref rectMin, 0xFFFFFF00, Outputs[i].Name, null);

                                    SetIfBigger(ref lineHeight, styles.PinOutStyle.Image.Size.Y * fScale);
                                    SetIfBigger(ref lineHeight, nameSize.Y);
                                }

                                lineY += (lineHeight + styles.PinPadding);
                            }
                        }
                        break;
                    case NodeGraphStyles.EFlowMode.Vertical:
                        break;
                }

                if(Selected)
                {
                    cmdlist.AddRect(ref drawPosition, ref NodeMax, styles.SelectedColor, 0, ImDrawCornerFlags_.ImDrawCornerFlags_All, 2);
                }
                ImGuiAPI.SetWindowFontScale(1.0f);
            }
        }
        private void SetIfBigger(ref float OldValue, float NewValue)
        {
            if(NewValue > OldValue)
            {
                OldValue = NewValue;
            }
        }
    }
}
