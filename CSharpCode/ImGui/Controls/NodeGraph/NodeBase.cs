using System;
using System.Collections.Generic;
using System.Text;
using EngineNS;

namespace EngineNS.EGui.Controls.NodeGraph
{
    public class EditableValue
    {
        public interface IValueEditNotify
        {
            void OnValueChanged(EditableValue ev);
        }
        public object Tag;
        protected IValueEditNotify mNotify;
        public EditableValue(IValueEditNotify notify)
        {
            mNotify = notify;
            LabelName = "EVL_" + System.Threading.Interlocked.Add(ref GID_EditableValue, 1).ToString();
        }
        public static EditableValue CreateEditableValue(IValueEditNotify notify, System.Type type, object tag)
        {
            if (type == typeof(SByte) ||
                type == typeof(Int16) ||
                type == typeof(Int32) ||
                type == typeof(byte) ||
                type == typeof(UInt16) ||
                type == typeof(UInt32) ||
                type == typeof(UInt64) ||
                type == typeof(Int64) ||
                type == typeof(Int64) ||
                type == typeof(float) ||
                type == typeof(double) ||
                type == typeof(string) ||
                type == typeof(Vector2) ||
                type == typeof(Vector3) ||
                type == typeof(Vector4))
            {
                var result = new EditableValue(notify);
                result.ValueType = Rtti.UTypeDesc.TypeOf(type);
                result.Tag = tag;
                return result;
            }
            else if(type == typeof(System.Type))
            {
                var result = new TypeSelectorEValue(notify);
                result.ValueType = Rtti.UTypeDesc.TypeOf(type);
                result.Selector.CtrlId = result.LabelName;
                result.Tag = tag;
                return result;
            }
            return null;
        }
        private string LabelName;
        public Rtti.UTypeDesc ValueType { get; set; }
        public object Value { get; set; }
        public float ControlWidth { get; set; } = 80;
        private static int GID_EditableValue = 0;
        public virtual unsafe void OnDraw(NodeBase node, int index, NodeGraphStyles styles, float fScale)
        {
            //LabelName目前每次构造时确保id唯一，也许以后可以找到类似
            //GetObjectHandleAddress这样的方法替换
            if (ValueType.SystemType == typeof(SByte) ||
                ValueType.SystemType == typeof(Int16) ||
                ValueType.SystemType == typeof(Int32))
            {
                ImGuiAPI.PushItemWidth(ControlWidth * fScale);
                var v = System.Convert.ToInt32(Value);
                var saved = v;
                ImGuiAPI.InputInt($"##{LabelName}", ref v, -1, 100, ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
                if (saved != v)
                {
                    Value = v;
                    mNotify?.OnValueChanged(this);
                }
                ImGuiAPI.PopItemWidth();
            }
            else if (ValueType.SystemType == typeof(byte) ||
                ValueType.SystemType == typeof(UInt16) ||
                ValueType.SystemType == typeof(UInt32) ||
                ValueType.SystemType == typeof(UInt64) ||
                ValueType.SystemType == typeof(Int64))
            {
                unsafe
                {
                    ImGuiAPI.PushItemWidth(ControlWidth * fScale);
                    var oldStr = Value.ToString();
                    bool nameChanged = ImGuiAPI.InputText($"##{LabelName}", ref oldStr);
                    if (nameChanged)
                    {
                        Value = EngineNS.Support.TConvert.ToObject(ValueType.SystemType, oldStr);
                        mNotify?.OnValueChanged(this);
                    }
                    ImGuiAPI.PopItemWidth();
                }
            }
            else if (ValueType.SystemType == typeof(float))
            {
                ImGuiAPI.PushItemWidth(ControlWidth * fScale);
                var v = System.Convert.ToSingle(Value);
                var saved = v;
                ImGuiAPI.InputFloat($"##{LabelName}", ref v, 0.1f, 10.0f, "%.6f", ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
                if (saved != v)
                {
                    Value = v;
                    mNotify?.OnValueChanged(this);
                }
                ImGuiAPI.PopItemWidth();
            }
            else if (ValueType.SystemType == typeof(double))
            {
                ImGuiAPI.PushItemWidth(ControlWidth * fScale);
                var v = System.Convert.ToDouble(Value);
                var saved = v;
                ImGuiAPI.InputDouble($"##{LabelName}", ref v, 0.1, 10.0, "%.6f", ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
                if (saved != v)
                {
                    Value = v;
                    mNotify?.OnValueChanged(this);
                }
                ImGuiAPI.PopItemWidth();
            }
            else if (ValueType.SystemType == typeof(Vector2))
            {
                ImGuiAPI.PushItemWidth(ControlWidth * fScale);
                var v = (Vector2)(Value);
                var saved = v;
                ImGuiAPI.InputFloat2($"##{LabelName}", (float*)&v, "%.6f", ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
                if (saved != v)
                {
                    Value = v;
                    mNotify?.OnValueChanged(this);
                }
                ImGuiAPI.PopItemWidth();
            }
            else if (ValueType.SystemType == typeof(Vector3))
            {
                ImGuiAPI.PushItemWidth(ControlWidth * fScale);
                var v = (Vector2)(Value);
                var saved = v;
                ImGuiAPI.InputFloat3($"##{LabelName}", (float*)&v, "%.6f", ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
                if (saved != v)
                {
                    Value = v;
                    mNotify?.OnValueChanged(this);
                }
                ImGuiAPI.PopItemWidth();
            }
            else if (ValueType.SystemType == typeof(Vector4))
            {
                ImGuiAPI.PushItemWidth(ControlWidth * fScale);
                var v = (Vector2)(Value);
                var saved = v;
                ImGuiAPI.InputFloat4($"##{LabelName}", (float*)&v, "%.6f", ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
                if (saved != v)
                {
                    Value = v;
                    mNotify?.OnValueChanged(this);
                }
                ImGuiAPI.PopItemWidth();
            }
            else if (ValueType.SystemType == typeof(string))
            {
                unsafe
                {
                    ImGuiAPI.PushItemWidth(ControlWidth * fScale);
                    var oldStr = Value.ToString();
                    bool nameChanged = ImGuiAPI.InputText($"##{LabelName}", ref oldStr);
                    if (nameChanged)
                    {
                        Value = EngineNS.Support.TConvert.ToObject(ValueType.SystemType, oldStr);
                        mNotify?.OnValueChanged(this);
                    }
                    ImGuiAPI.PopItemWidth();
                }
            }
        }
    }
    public class TypeSelectorEValue : EditableValue
    {
        public TypeSelectorEValue(EGui.Controls.NodeGraph.EditableValue.IValueEditNotify notify)
            : base(notify)
        {
            ControlWidth = 100;
        }
        public EGui.Controls.TypeSelector Selector
        {
            get;
        } = new EGui.Controls.TypeSelector();
        public override void OnDraw(EGui.Controls.NodeGraph.NodeBase node, int index, EGui.Controls.NodeGraph.NodeGraphStyles styles, float fScale)
        {
            Selector.SelectedType = this.Value as Rtti.UTypeDesc;
            var saved = Selector.SelectedType;
            Selector.OnDraw(ControlWidth * fScale, 8);
            if(saved != Selector.SelectedType)
            {
                this.Value = Selector.SelectedType;
                mNotify?.OnValueChanged(this);
            }
        }
        public void OnValueChanged(EGui.Controls.NodeGraph.EditableValue ev)
        {

        }
    }
    public class NodePin
    {
        public class LinkDesc
        {
            public UUvAnim Icon { get; set; } = new UUvAnim();
            public float ExtPadding { get; set; } = 10;
            public uint LineColor { get; set; } = 0xFFFF0000;//0xFF00FFFF
            public float LineThinkness { get; set; } = 3;
            public List<string> CanLinks { get; } = new List<string>();
        }
        public LinkDesc Link { get; set; }
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
        public Vector2 GetIconSize(NodeGraphStyles styles, float scale)
        {
            if (Link != null)
            {
                return Link.Icon.Size * scale;
            }
            else
            {
                if (this.GetType() == typeof(PinIn))
                    return styles.PinInStyle.Image.Size * scale;
                else
                    return styles.PinOutStyle.Image.Size * scale;
            }
        }
        public virtual bool HasLinker()
        {
            return HostNode.ParentGraph.PinHasLinker(this);
        }
    }
    public class PinIn : NodePin
    {
        public EditableValue EditValue { get; set; } = null;
    }
    public class PinOut : NodePin
    {
    }
    public partial class NodeBase : IO.ISerializer
    {
        public virtual void OnPreRead(object tagObject, object hostObject, bool fromXml)
        {
            var graph = hostObject as NodeGraph;
            if (graph != null)
                ParentGraph = graph;
        }
        public virtual void OnPropertyRead(object root, System.Reflection.PropertyInfo prop, bool fromXml) { }
        public NodeBase()
        {
            Icon.Color = 0xFF00FF00;
            TitleImage.Color = 0xFF402020;
            Background.Color = 0x80808080;
        }
        [Rtti.Meta]
        public Guid NodeId
        {
            get;
            set;
        } = Guid.NewGuid();
        public NodeGraph ParentGraph { get; set; }
        [Rtti.Meta]
        public virtual string Name
        {
            get;
            set;
        }
        public bool Visible { get; set; } = true;
        public bool Selected { get; set; }
        internal Vector2 OffsetOfDragPoint { get; set; }
        Vector2 mPosition;
        [Rtti.Meta]
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
        public UUvAnim Icon { get; set; } = new UUvAnim();
        public UUvAnim TitleImage { get; set; } = new UUvAnim();
        public UUvAnim Background { get; set; } = new UUvAnim();
        public List<PinIn> Inputs
        {
            get;
        } = new List<PinIn>();
        public List<PinOut> Outputs
        {
            get;
        } = new List<PinOut>();
        public void AddPinIn(PinIn pin)
        {
            pin.HostNode = this;
            if (Inputs.Contains(pin))
                return;
            Inputs.Add(pin);
        }
        public void RemovePinIn(PinIn pin)
        {
            pin.HostNode = null;
            Inputs.Remove(pin);
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
        public void RemovePinOut(PinOut pin)
        {
            pin.HostNode = null;
            Outputs.Remove(pin);
        }
        public NodePin PointInPin(ref Vector2 posInView, NodeGraphStyles styles)
        {
            foreach (var i in Inputs)
            {
                var rcMin = i.DrawPosition;
                var rcMax = rcMin + i.GetIconSize(styles, ParentGraph.ScaleFactor);
                if (ImGuiAPI.PointInRect(ref posInView, ref rcMin, ref rcMax))
                    return i;
            }
            foreach (var i in Outputs)
            {
                var rcMin = i.DrawPosition;
                var rcMax = rcMin + i.GetIconSize(styles, ParentGraph.ScaleFactor);
                if (ImGuiAPI.PointInRect(ref posInView, ref rcMin, ref rcMax))
                    return i;
            }
            return null;
        }
        public unsafe virtual void OnDraw(NodeGraphStyles styles = null)
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
            var nameSize = ImGuiAPI.CalcTextSize(Name, false, -1.0f);
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
                            float extPadding = 0;
                            
                            if (i < Inputs.Count)
                            {
                                var inIcon = styles.PinInStyle.Image;
                                if (Inputs[i].Link!=null)
                                {
                                    inIcon = Inputs[i].Link.Icon;
                                }
                                lineWidth += styles.PinInStyle.Offset;
                                lineWidth += inIcon.Size.X;
                                lineWidth += styles.PinSpacing;
                                nameSize = ImGuiAPI.CalcTextSize(Inputs[i].Name, false, -1.0f);
                                lineWidth += nameSize.X;
                                if (Inputs[i].EditValue != null)
                                {
                                    lineWidth += Inputs[i].EditValue.ControlWidth;
                                }
                                lineWidth += styles.MinSpaceInOut;

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
                                lineWidth += nameSize.X;
                                lineWidth += styles.MinSpaceInOut;

                                SetIfBigger(ref lineHeight, inIcon.Size.Y);
                                SetIfBigger(ref lineHeight, nameSize.Y);

                                if (Outputs[i].Link != null)
                                {
                                    SetIfBigger(ref extPadding, Outputs[i].Link.ExtPadding);
                                }
                            }

                            SetIfBigger(ref NodeSize.X, lineWidth);
                            NodeSize.Y += (lineHeight + styles.PinPadding + extPadding);
                        }
                    }
                    break;
                case NodeGraphStyles.EFlowMode.Vertical:
                    break;
            }

            var ScaledNodeSize = NodeSize * fScale;
            this.Size = NodeSize;
            ImGuiAPI.SetNextWindowSize(in ScaledNodeSize, ImGuiCond_.ImGuiCond_None);
            var drawPosition = DrawPosition;
            //if(Visible)
            {
                var cmdlist = new ImDrawList(ImGuiAPI.GetWindowDrawList());
                ImGuiAPI.SetWindowFontScale(fScale);
                var NodeMax = drawPosition + ScaledNodeSize;
                float lineY = drawPosition.Y;
                
                lineHeight = 0;
                SetIfBigger(ref lineHeight, (styles.IconOffset.Y * 2 + Icon.Size.Y) * fScale);
                SetIfBigger(ref lineHeight, nameSize.Y * fScale);
                {
                    var rectTitleMin = drawPosition;
                    var rectTitleMax = rectTitleMin + new Vector2(ScaledNodeSize.X, lineHeight);
                    TitleImage.OnDraw(ref cmdlist, in rectTitleMin, in rectTitleMax, 0);

                    rectTitleMin.Y += lineHeight;
                    rectTitleMax = NodeMax;
                    Background.OnDraw(ref cmdlist, in rectTitleMin, in rectTitleMax, 0);
                }

                var rectMin = new Vector2(drawPosition.X + styles.IconOffset.X * fScale, lineY + styles.IconOffset.Y * fScale);
                var rectMax = rectMin + Icon.Size * fScale;
                Icon.OnDraw(ref cmdlist, in rectMin, in rectMax, 0);
                rectMin.X = rectMax.X;
                rectMin.Y -= styles.IconOffset.Y * fScale;
                nameSize = ImGuiAPI.CalcTextSize(Name, false, -1.0f);
                cmdlist.AddText(in rectMin, styles.TitleTextColor, Name, null);

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
                                float extPadding = 0;
                                if (i < Inputs.Count)
                                {
                                    var inIcon = styles.PinInStyle.Image;
                                    if (Inputs[i].Link != null)
                                    {
                                        inIcon = Inputs[i].Link.Icon;
                                    }
                                    rectMin.X = drawPosition.X + styles.PinInStyle.Offset * fScale;
                                    rectMax = rectMin + inIcon.Size * fScale;
                                    inIcon.OnDraw(ref cmdlist, in rectMin, in rectMax, 0);
                                    Inputs[i].Offset = (rectMin - drawPosition) / ParentGraph.ScaleFactor;
                                    nameSize = ImGuiAPI.CalcTextSize(Inputs[i].Name, false, -1.0f);
                                    rectMin.X = rectMax.X;
                                    cmdlist.AddText(in rectMin, styles.PinTextColor, Inputs[i].Name, null);

                                    if (Inputs[i].EditValue != null)
                                    {
                                        var ctrlPos = new Vector2(rectMax.X + nameSize.X + styles.PinSpacing, rectMin.Y);
                                        ctrlPos -= ImGuiAPI.GetWindowPos();
                                        ImGuiAPI.SetCursorPos(in ctrlPos);
                                        Inputs[i].EditValue.OnDraw(this, i, styles, fScale);
                                    }

                                    SetIfBigger(ref lineHeight, inIcon.Size.Y * fScale);
                                    SetIfBigger(ref lineHeight, nameSize.Y);

                                    if (Inputs[i].Link != null)
                                    {
                                        SetIfBigger(ref extPadding, Inputs[i].Link.ExtPadding * fScale);
                                    }
                                }
                                if (i < Outputs.Count)
                                {
                                    var inIcon = styles.PinOutStyle.Image;
                                    if (Outputs[i].Link != null)
                                    {
                                        inIcon = Outputs[i].Link.Icon;
                                    }
                                    rectMin.X = NodeMax.X - styles.PinOutStyle.Offset * fScale - inIcon.Size.X * fScale;
                                    rectMax = rectMin + inIcon.Size * fScale;
                                    inIcon.OnDraw(ref cmdlist, in rectMin, in rectMax, 0);
                                    Outputs[i].Offset = (rectMin - drawPosition)/ParentGraph.ScaleFactor;
                                    nameSize = ImGuiAPI.CalcTextSize(Outputs[i].Name, false, -1.0f);
                                    rectMin.X -= nameSize.X;
                                    cmdlist.AddText(in rectMin, 0xFFFFFF00, Outputs[i].Name, null);

                                    SetIfBigger(ref lineHeight, inIcon.Size.Y * fScale);
                                    SetIfBigger(ref lineHeight, nameSize.Y);

                                    if (Outputs[i].Link != null)
                                    {
                                        SetIfBigger(ref extPadding, Outputs[i].Link.ExtPadding * fScale);
                                    }
                                }

                                lineY += (lineHeight + styles.PinPadding * fScale + extPadding);
                            }
                        }
                        break;
                    case NodeGraphStyles.EFlowMode.Vertical:
                        break;
                }

                OnAfterDraw(styles, ref cmdlist);

                if (Selected)
                {
                    cmdlist.AddRect(in drawPosition, in NodeMax, styles.SelectedColor, 0, ImDrawFlags_.ImDrawFlags_RoundCornersAll, 2);
                }
                ImGuiAPI.SetWindowFontScale(1.0f);
            }
        }
        protected virtual void OnAfterDraw(NodeGraphStyles styles, ref ImDrawList cmdlist)
        {

        }
        protected void SetIfBigger(ref float OldValue, float NewValue)
        {
            if (NewValue > OldValue)
            {
                OldValue = NewValue;
            }
        }

        public virtual bool CanLinkTo(PinOut oPin, NodeBase InNode, PinIn iPin)
        {
            if (InNode == this)
                return false;
            if (oPin.Link != null && iPin.Link != null)
            {
                foreach (var i in oPin.Link.CanLinks)
                {
                    foreach (var j in iPin.Link.CanLinks)
                    {
                        if (i == j)
                            return true;
                    }
                }
                return false;
            }
            else
            {
                return true;
            }
        }
        public virtual bool CanLinkFrom(PinIn iPin, NodeBase OutNode, PinOut oPin)
        {
            if (OutNode == this)
                return false;
            if (oPin.Link != null && iPin.Link != null)
            {
                foreach (var i in oPin.Link.CanLinks)
                {
                    foreach (var j in iPin.Link.CanLinks)
                    {
                        if (i == j)
                            return true;
                    }
                }
                return false;
            }
            else
            {
                return true;
            }
        }

        public virtual void OnLinkedTo(PinOut oPin, NodeBase InNode, PinIn iPin)
        {

        }
        public virtual void OnLinkedFrom(PinIn iPin, NodeBase OutNode, PinOut oPin)
        {

        }
        public virtual void OnRemoveLinker(PinLinker linker)
        {

        }
        public virtual void OnLButtonClicked(NodePin clickedPin)
        {

        }
        public virtual void OnMouseStayPin(NodePin stayPin)
        {

        }
        public virtual void OnLoadLinker(PinLinker linker)
        {

        }
        public virtual void OnShowPinMenu(NodePin pin)
        {

        }
    }
}
