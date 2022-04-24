using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.NodeGraph
{
    public class LinkDesc
    {
        public EGui.UUvAnim Icon { get; set; } = new EGui.UUvAnim();
        public uint LineColor { get; set; } = 0xFFFF0000; // 0xFF00FFFF
        public float LineThinkness { get; set; } = 3.0f;
        public float ExtPadding { get; set; } = 10;
        
        public List<string> CanLinks { get; } = new List<string>();
    }
    public class NodePin
    {
        public string Name { get; set; }
        public Guid NodeId 
        { 
            get
            {
                return HostNode.NodeId;
            }
        }
        public UNodeBase HostNode { get; set; }
        public LinkDesc Link { get; set; }

        public Vector2 Position { get; set; }
        public Vector2 Size { get; set; }
        public bool MultiLinks { get; set; }
        public object Tag { get; set; }
        public bool IsHit(float x, float y)
        {
            if (x<Position.X || y<Position.Y || x >= Position.X + Size.X || y >= Position.Y + Size.Y)
            {
                return false;
            }
            return true;
        }
        public bool HasLinker()
        {
            if (HostNode == null || HostNode.ParentGraph == null)
                return false;
            //return true;
            return HostNode.ParentGraph.PinHasLinker(this);
        }
    }
    public class PinIn : NodePin
    {
        public EGui.Controls.NodeGraph.EditableValue EditValue { get; set; } = null;
    }
    public class PinOut : NodePin
    {
    }


    public class UNodeBase : IO.ISerializer
    {
        public virtual void OnPreRead(object tagObject, object hostObject, bool fromXml)
        {
            var graph = hostObject as UNodeGraph;
            if (graph != null)
                ParentGraph = graph;
        }
        public virtual void OnPropertyRead(object root, System.Reflection.PropertyInfo prop, bool fromXml) { }
        [Rtti.Meta]
        public virtual string Name { get; set; }
        [Rtti.Meta]
        public Guid NodeId { get; set; }
        public bool Selected { get; set; }
        internal Vector2 mPosition;
        [Rtti.Meta]
        public Vector2 Position 
        { 
            get => mPosition;
            set
            {
                mPosition = value;
                OnPositionChanged();
            }
        }
        public Vector2 Size { get; set; }
        public Vector2 PrevSize { get; set; }
        public EGui.UUvAnim Icon { get; set; } = new EGui.UUvAnim();
        public float TitleHeight { get; set; }
        public UNodeGraph ParentGraph { get; set; }
        public uint BackColor { get; set; }
        public uint TitleColor { get; set; }
        public List<PinIn> Inputs { get; } = new List<PinIn>();
        public List<PinOut> Outputs { get; } = new List<PinOut>();

        public UNodeBase()
        {
            Selected = false;
            ParentGraph = null;
            TitleHeight = 0;
            BackColor = 0xFF808080;
            TitleColor = 0xFF800000;
            Position.SetValue(0, 0);
            Size.SetValue(0, 0);
            PrevSize.SetValue(0, 0);
            NodeId = Guid.NewGuid();
        }
        #region Pos&Hit
        protected void SetIfBigger(ref float OldValue, float NewValue)
        {
            if (NewValue > OldValue)
            {
                OldValue = NewValue;
            }
        }
        public static EngineNS.Vector2 CalcTextSize(string text)
        {
            unsafe
            {
                if (ImGuiAPI.GetFont().IsValidPointer == false)
                {
                    EngineNS.Vector2 tmp;
                    tmp.X = text.Length * 18;
                    tmp.Y = 18;
                    return tmp;
                }
            }            
            return ImGuiAPI.CalcTextSize(text, false, -1.0f);
        }
        public virtual void OnPositionChanged()
        {
            var styles = EGui.Controls.NodeGraph.NodeGraphStyles.DefaultStyles;
            float fNodeW = 0;
            float fNodeH = 0;
            float lineWidth = 0;
            float lineHeight = 0;
            lineWidth += styles.IconOffset.X;
            lineWidth += Icon.Size.X;
            var nameSize = CalcTextSize(Name);
            lineWidth += nameSize.X;
            SetIfBigger(ref fNodeW, lineWidth);
            SetIfBigger(ref lineHeight, styles.IconOffset.Y * 2 + Icon.Size.Y);
            SetIfBigger(ref lineHeight, nameSize.Y);
            TitleHeight = lineHeight;
            fNodeH += TitleHeight;

            var lines = Math.Max(Inputs.Count, Outputs.Count);
            for (int i = 0; i < lines; i++)
            {
                lineWidth = 0;
                lineHeight = 0;
                float extPadding = 0;
                if (i < Inputs.Count)
                {
                    var inIcon = styles.PinInStyle.Image;
                    if (Inputs[i].Link != null)
                    {
                        if (Inputs[i].Link.Icon != null)
                        {
                            inIcon = Inputs[i].Link.Icon;
                        }
                    }

                    lineWidth += styles.PinInStyle.Offset;
                    Inputs[i].Position = Position + new Vector2(lineWidth, fNodeH);
                    Inputs[i].Size = inIcon.Size;

                    lineWidth += inIcon.Size.X;
                    lineWidth += styles.PinSpacing;
                    nameSize = CalcTextSize(Inputs[i].Name);

                    //lineWidth += styles.MinSpaceInOut;

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
                        if (Outputs[i].Link.Icon != null)
                        {
                            inIcon = Outputs[i].Link.Icon;
                        }
                    }
                    lineWidth += styles.PinOutStyle.Offset;
                    Outputs[i].Position = new Vector2(styles.PinOutStyle.Offset, Position.Y + fNodeH);
                    Outputs[i].Size = inIcon.Size;

                    lineWidth += inIcon.Size.X;
                    lineWidth += styles.PinSpacing;
                    nameSize = CalcTextSize(Outputs[i].Name);
                    //lineWidth += styles.MinSpaceInOut;

                    SetIfBigger(ref lineHeight, inIcon.Size.Y);
                    SetIfBigger(ref lineHeight, nameSize.Y);

                    if (Outputs[i].Link != null)
                    {
                        SetIfBigger(ref extPadding, Outputs[i].Link.ExtPadding);
                    }
                }

                SetIfBigger(ref fNodeW, lineWidth);
                fNodeH += (lineHeight + styles.PinPadding + extPadding);
            }
            Size = new Vector2(fNodeW + PrevSize.X, fNodeH);
            if (fNodeH - TitleHeight < PrevSize.Y)
            {
                Size = new Vector2(Size.X, TitleHeight + PrevSize.Y);
            }
            for (int i = 0; i < Outputs.Count; i++)
            {
                float oldValue = Outputs[i].Position.X;
                Outputs[i].Position = new Vector2(Position.X + Size.X - oldValue - Outputs[i].Size.X, Outputs[i].Position.Y);
            }
        }
        public bool IsHit(float x, float y)
        {
            if (x<Position.X ||
                y<Position.Y ||
                x >= Position.X + Size.X ||
                y >= Position.Y + Size.Y )
            {
                return false;
            }
            return true;
        }
        public bool IsHitRect(in Vector2 start, in Vector2 end)
        {
            if (end.X < Position.X || end.Y < Position.Y ||
                start.X >= Position.X + Size.X || start.Y >= Position.Y + Size.Y)
            {
                return false;
            }
            return true;
        }
        public NodePin IsHitPin(float x, float y)
        {
            foreach (var i in Inputs)
            {
                if (i.IsHit(x, y))
                    return i;
            }
            foreach (var i in Outputs)
            {
                if (i.IsHit(x, y))
                    return i;
            }
            return null;
        }
        public bool IsHitTitle(float x, float y)
        {
            if (x<Position.X || y<Position.Y || x >= Position.X + Size.X || y >= Position.Y + TitleHeight)
            {
                return false;
            }
            return true;
        }
        #endregion
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
        public virtual bool CanLinkTo(PinOut oPin, UNodeBase InNode, PinIn iPin)
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
        public virtual bool CanLinkFrom(PinIn iPin, UNodeBase OutNode, PinOut oPin)
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
        public PinIn AddPinIn(PinIn pin)
        {
            pin.HostNode = this;
            foreach (var i in Inputs)
            {
                if (i == pin)
                    return pin;
            }
            Inputs.Add(pin);
            return pin;
        }
        public void RemovePinIn(PinIn pin)
        {
            pin.HostNode = null;
            for (int i = 0; i < Inputs.Count; i++)
            {
                if (Inputs[i] == pin)
                {
                    Inputs.RemoveAt(i);
                    return;
                }
            }
        }
        public PinOut AddPinOut(PinOut pin)
        {
            pin.HostNode = this;
            foreach (var i in Outputs)
            {
                if (i == pin)
                    return pin;
            }
            Outputs.Add(pin);
            return pin;
        }
        public void RemovePinOut(PinOut pin)
        {
            pin.HostNode = null;
            for (int i = 0; i < Outputs.Count; i++)
            {
                if (Outputs[i] == pin)
                {
                    Outputs.RemoveAt(i);
                    return;
                }
            }
        }
        #region override
        public virtual void OnPreviewDraw(in Vector2 prevStart, in Vector2 prevEnd, ImDrawList cmdlist)
        {

        }
        public virtual void OnAfterDraw(EngineNS.EGui.Controls.NodeGraph.NodeGraphStyles styles, ImDrawList cmdlist)
        {

        }
        public virtual void OnShowPinMenu(NodePin pin)
        {

        }
        public virtual void OnRemoveLinker(UPinLinker linker)
        {

        }
        public virtual void OnLoadLinker(UPinLinker linker)
        {

        }
        public virtual void OnLinkedTo(PinOut oPin, UNodeBase InNode, PinIn iPin)
        {
        }
        public virtual void OnLinkedFrom(PinIn iPin, UNodeBase OutNode, PinOut oPin)
        {
        }
        public virtual void OnDoubleClick() 
        {
        }
        public virtual void OnLButtonClicked(NodePin hitPin)
        {
        }
        public virtual void OnMouseStayPin(NodePin stayPin)
        {

        }
        #endregion
        public delegate bool FOnNodeVisit(NodePin iPin, UPinLinker linker);
        public bool TourNodeTree(FOnNodeVisit visit)
        {
            foreach (var i in Outputs)
            {
                var linker = ParentGraph.FindOutLinkerSingle(i);
                if (visit(i, linker) == false)
                    return false;
                if (linker != null)
                {
                    if (linker.InNode.TourNodeTree(visit) == false)
                        return false;
                }
            }
            return true;
        }
        public bool InvTourNodeTree(FOnNodeVisit visit)
        {
            foreach(var i in Inputs)
            {
                var linker = ParentGraph.FindInLinkerSingle(i);
                if (visit(i, linker) == false)
                    return false;
                if (linker != null)
                {
                    if (linker.OutNode.InvTourNodeTree(visit) == false)
                        return false;
                }
            }
            return true;
        }
    }
}
