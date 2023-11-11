using EngineNS.EGui.Controls.PropertyGrid;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.Bricks.NodeGraph
{
    public class GraphException : Exception, EGui.Controls.PropertyGrid.IPropertyCustomization
    {
        public UNodeBase ErrorNode;
        public NodePin ErrorPin;
        public string ErrorPinName
        {
            get
            {
                if (ErrorPin != null)
                    return ErrorPin.Name;
                return "";
            }
        }
        public string ErrorInfo { get; set; }
        public GraphException(UNodeBase node, NodePin pin, string info,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
        {
            ErrorNode = node;
            ErrorPin = pin;
            ErrorInfo = $"{sourceFilePath}:{sourceLineNumber}->{memberName}->{info}";
        }

        [Browsable(false)]
        public bool IsPropertyVisibleDirty { get; set; } = false;
        public void GetProperties(ref CustomPropertyDescriptorCollection collection, bool parentIsValueType)
        {
            if(ErrorPin != null)
            {
                var pinNameProDesc = EGui.Controls.PropertyGrid.PropertyCollection.PropertyDescPool.QueryObjectSync();
                pinNameProDesc.Name = "ErrorPinName";
                pinNameProDesc.PropertyType = Rtti.UTypeDesc.TypeOf(typeof(string));
                pinNameProDesc.IsReadonly = true;
                collection.Add(pinNameProDesc);
            }

            var infoProDesc = EGui.Controls.PropertyGrid.PropertyCollection.PropertyDescPool.QueryObjectSync();
            infoProDesc.Name = "ErrorInfo";
            infoProDesc.PropertyType = Rtti.UTypeDesc.TypeOf(typeof(string));
            infoProDesc.IsReadonly = true;
            collection.Add(infoProDesc);
        }

        public object GetPropertyValue(string propertyName)
        {
            switch(propertyName)
            {
                case "ErrorPinName":
                    return ErrorPinName;
                case "ErrorInfo":
                    return ErrorInfo;
            }
            return null;
        }

        public void SetPropertyValue(string propertyName, object value)
        {
        }
    }

    public class LinkDesc
    {
        public EGui.UUvAnim Icon { get; set; } = new EGui.UUvAnim();
        public EGui.UUvAnim DisconnectIcon { get; set; } = new EGui.UUvAnim();
        public uint LineColor { get; set; } = 0xFFFF0000; // 0xFF00FFFF
        public float LineThinkness { get; set; } = 3.0f;
        public float ExtPadding { get; set; } = 10;
        
        public List<string> CanLinks { get; } = new List<string>();

        public void SetColor(uint color, bool withIcon = true)
        {
            LineColor = color;
            Icon.Color = color;
            DisconnectIcon.Color = color;
        }
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
        public LinkDesc LinkDesc { get; set; }
        
        public Vector2 Position { get; set; }
        public Vector2 Size { get; set; }
        public bool MultiLinks { get; set; }
        public object Tag { get; set; }

        public Vector2 HotPosition;
        public Vector2 HotSize;
        public Vector2 NamePosition;
        public Vector2 EditValuePosition;

        public string GroupName;
        public bool ShowIcon = true;
        public bool ShowName = true;

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
        public virtual void CopyTo(NodePin pin)
        {
            CopyTo(this, pin);
        }
        public static void CopyTo(NodePin src, NodePin tag)
        {
            tag.Name = src.Name;
            tag.LinkDesc = src.LinkDesc;
            tag.Position = src.Position;
            tag.Size = src.Size;
            tag.MultiLinks = src.MultiLinks;
            tag.Tag = src.Tag;
            tag.GroupName = src.GroupName;
            tag.ShowIcon = src.ShowIcon;
            tag.ShowName = src.ShowName;
        }
    }
    public class PinIn : NodePin
    {
        [Rtti.Meta]
        public UEditableValue EditValue { get; set; } = null;

        public override void CopyTo(NodePin pin)
        {
            var pinIn = pin as PinIn;
            if (pinIn == null)
                return;

            base.CopyTo(pinIn);
            pinIn.EditValue = EditValue;
        }
    }
    public class PinOut : NodePin
    {
    }

    public interface IBeforeExecNode
    {
        public PinIn BeforeExec { get; set; }
        public void LightDebuggerLine();
        public void UnLightDebuggerLine();
    }
    public interface IAfterExecNode
    {
        public PinOut AfterExec { get; set; }
    }

    public enum EBreakerState
    {
        Hidden,
        Enable,
        Disable,
    }
    public interface IBreakableNode
    {
        [Browsable(false)]
        public string BreakerName { get; }
        [Browsable(false)]
        public EBreakerState BreakerState { get; set; }
        public void AddMenuItems(UMenuItem parentItem);
    }

    public interface INodeWithContextMenu
    {
        public UMenuItem ContextMenu { get; set; }
    }
    public class UNodeBase : IO.ISerializer
    {
        public bool LayoutDirty = true;

        public bool HasError = false;
        public GraphException CodeExcept;
        public void ResetErrors()
        {
            HasError = false;
            CodeExcept = null;
        }

        public Action<UNodeBase, object, object, bool> OnPreReadAction;
        public virtual void OnPreRead(object tagObject, object hostObject, bool fromXml)
        {
            var graph = hostObject as UNodeGraph;
            if (graph != null)
                ParentGraph = graph;
            OnPreReadAction?.Invoke(this, tagObject, hostObject, fromXml);
        }
        public virtual void OnPropertyRead(object root, System.Reflection.PropertyInfo prop, bool fromXml) { }
        string mName = "NoName";
        [Rtti.Meta]
        public virtual string Name 
        {
            get => mName;
            set
            {
                if (mName == "NoName")
                    Label = value;
                mName = value;
                LayoutDirty = true;
            }
        }
        [Rtti.Meta]
        [System.ComponentModel.Browsable(false)]
        public virtual string Label { get; set; } = "NoName";
        [Rtti.Meta]
        [System.ComponentModel.Browsable(false)]
        public Guid NodeId { get; set; }
        public string NodeType
        {
            get
            {
                return this.GetType().Name;
            }
        }
        [System.ComponentModel.Browsable(false)]
        public bool Selected { get; set; }
        internal Vector2 mPosition;
        [Rtti.Meta]
        [System.ComponentModel.Browsable(false)]
        public Vector2 Position 
        { 
            get => mPosition;
            set
            {
                mPosition = value;
                OnPositionChanged();
            }
        }
        [System.ComponentModel.Browsable(false)]
        public Vector2 Size { get; set; }
        public Vector2 PrevPos;
        [System.ComponentModel.Browsable(false)]
        public Vector2 PrevSize { get; set; }
        [System.ComponentModel.Browsable(false)]
        public EGui.UUvAnim Icon { get; set; } = new EGui.UUvAnim();
        [System.ComponentModel.Browsable(false)]
        public float TitleHeight { get; set; }
        [System.ComponentModel.Browsable(false)]
        public UNodeGraph ParentGraph { get; set; }
        [System.ComponentModel.Browsable(false)]
        public uint BackColor { get; set; }
        [System.ComponentModel.Browsable(false)]
        public uint TitleColor { get; set; }
        [System.ComponentModel.Browsable(false)]
        public List<PinIn> Inputs { get; } = new List<PinIn>();
        [System.ComponentModel.Browsable(false)]
        public List<PinOut> Outputs { get; } = new List<PinOut>();

        public object UserData;
        public class UInputEditableValueInfo : IO.BaseSerializer
        {
            [Rtti.Meta]
            public string PinName { get; set; }
            [Rtti.Meta]
            public object Value 
            { 
                get; 
                set; 
            }
        }

        [Rtti.Meta(Order = 1), Browsable(false)]
        public List<UInputEditableValueInfo> InputEditableValues
        {
            get
            {
                var result = new List<UInputEditableValueInfo>();
                foreach(var i in Inputs)
                {
                    if (i.EditValue != null)
                    {
                        var t = new UInputEditableValueInfo();
                        t.PinName = i.Name;
                        t.Value = i.EditValue.Value;
                        result.Add(t);
                    }
                }
                return result;
            }
            set
            {
                foreach (var i in value)
                {
                    var pin = this.FindPinIn(i.PinName);
                    if (pin != null)
                    {
                        if (pin.EditValue != null)
                        {
                            pin.EditValue.Value = i.Value;
                        }
                    }
                }
            }
        }

        public UNodeBase()
        {
            Icon.Size = new Vector2(25, 25);
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
                if(ImGuiAPI.GetCurrentContext()==IntPtr.Zero.ToPointer())
                {
                    return Vector2.One;
                }
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
            LayoutDirty = true;
        }
        public virtual void UpdateLayout()
        {
            var styles = UNodeGraphStyles.DefaultStyles;
            float fNodeW = 0;
            float fNodeH = 0;
            float lineWidth = 0;
            float lineHeight = 0;
            lineWidth += styles.IconOffset.X;
            lineWidth += Icon.Size.X;
            var nameSize = CalcTextSize(Name);
            if(Name != Label && !string.IsNullOrEmpty(Label))
            {
                var tempSize = CalcTextSize(Label);
                nameSize.Y += tempSize.Y + styles.TitleTextOffset;
                nameSize.X = System.Math.Max(tempSize.X, nameSize.X);
            }
            lineWidth += nameSize.X;
            lineHeight = nameSize.Y + styles.TitlePadding.Y * 2;
            SetIfBigger(ref fNodeW, lineWidth);
            SetIfBigger(ref lineHeight, styles.IconOffset.Y * 2 + Icon.Size.Y);
            TitleHeight = lineHeight;
            fNodeH += TitleHeight;
            fNodeH += styles.TitlePadding.Y;
            PrevPos.Y = Position.Y + fNodeH;
            float maxInputSizeX = 0;
            var lines = Math.Max(Inputs.Count, Outputs.Count);
            for (int i = 0; i < lines; i++)
            {
                lineWidth = 0;
                lineHeight = 0;
                float extPadding = 0;
                if (i < Inputs.Count)
                {
                    var inIcon = styles.PinInStyle.Image;
                    if (Inputs[i].LinkDesc != null)
                    {
                        if (Inputs[i].LinkDesc.Icon != null)
                        {
                            inIcon = Inputs[i].LinkDesc.Icon;
                        }
                    }

                    var offset = styles.PinInStyle.Offset + styles.PinPadding;
                    lineWidth += offset;
                    Inputs[i].Position = Position + new Vector2(offset, fNodeH);
                    Inputs[i].Size = inIcon.Size;

                    nameSize = CalcTextSize(Inputs[i].Name);
                    var inputSize = Inputs[i].Size;
                    SetIfBigger(ref inputSize.Y, nameSize.Y);
                    inputSize.X += styles.PinSpacing;
                    inputSize.X += nameSize.X;
                    Inputs[i].Size = inputSize;
                    lineWidth += inputSize.X;

                    //lineWidth += styles.MinSpaceInOut;

                    SetIfBigger(ref lineHeight, Inputs[i].Size.Y);

                    if (Inputs[i].LinkDesc != null)
                    {
                        SetIfBigger(ref extPadding, Inputs[i].LinkDesc.ExtPadding);
                    }
                    if(Inputs[i].EditValue != null)
                    {
                        var editWidth = Inputs[i].EditValue.ControlWidth + styles.PinSpacing;
                        lineWidth += editWidth;
                        inputSize.X += editWidth;
                        var editHeight = Inputs[i].EditValue.ControlHeight;
                        SetIfBigger(ref inputSize.Y, editHeight);
                        SetIfBigger(ref lineHeight, inputSize.Y);
                        Inputs[i].Size = inputSize;
                    }

                    Inputs[i].HotPosition = new Vector2(
                        Inputs[i].Position.X,
                        Inputs[i].Position.Y + (Inputs[i].Size.Y - inIcon.Size.Y) * 0.5f + 2.0f);
                    Inputs[i].HotSize = inIcon.Size;
                    Inputs[i].NamePosition = new Vector2(
                        Inputs[i].HotPosition.X + Inputs[i].HotSize.X + styles.PinSpacing,
                        Inputs[i].Position.Y + (Inputs[i].Size.Y - nameSize.Y) * 0.5f);

                    Inputs[i].EditValuePosition = Inputs[i].NamePosition + new Vector2(styles.PinSpacing + nameSize.X, 0);
                }
                SetIfBigger(ref maxInputSizeX, lineWidth);
                if (i < Outputs.Count)
                {
                    var inIcon = styles.PinOutStyle.Image;
                    if (Outputs[i].LinkDesc != null)
                    {
                        if (Outputs[i].LinkDesc.Icon != null)
                        {
                            inIcon = Outputs[i].LinkDesc.Icon;
                        }
                    }
                    lineWidth += styles.PinOutStyle.Offset + styles.PinPadding;
                    Outputs[i].Position = new Vector2(styles.PinOutStyle.Offset + styles.PinPadding, Position.Y + fNodeH);
                    Outputs[i].Size = inIcon.Size;
                    Outputs[i].HotSize = inIcon.Size;

                    nameSize = CalcTextSize(Outputs[i].Name);
                    var outputSize = Outputs[i].Size;
                    SetIfBigger(ref outputSize.Y, nameSize.Y);
                    outputSize.X += styles.PinSpacing;
                    outputSize.X += nameSize.X;
                    Outputs[i].Size = outputSize;
                    lineWidth += outputSize.X;
                    //lineWidth += styles.MinSpaceInOut;

                    SetIfBigger(ref lineHeight, Outputs[i].Size.Y);

                    if (Outputs[i].LinkDesc != null)
                    {
                        SetIfBigger(ref extPadding, Outputs[i].LinkDesc.ExtPadding);
                    }
                }

                SetIfBigger(ref fNodeW, lineWidth);
                fNodeH += (lineHeight + styles.PinPadding + extPadding);
            }
            PrevPos.X = Position.X + maxInputSizeX + styles.PinSpacing;
            var doubleSpacing = styles.PinSpacing * 2;
            Size = new Vector2(fNodeW + PrevSize.X + styles.MinSpaceInOut + doubleSpacing, fNodeH);
            if ((fNodeH - TitleHeight) < (PrevSize.Y + doubleSpacing))
            {
                Size = new Vector2(Size.X, TitleHeight + PrevSize.Y + doubleSpacing);
            }
            for (int i = 0; i < Outputs.Count; i++)
            {
                float oldValue = Outputs[i].Position.X;
                Outputs[i].Position = new Vector2(Position.X + Size.X - oldValue - Outputs[i].Size.X, Outputs[i].Position.Y);

                Outputs[i].HotPosition = new Vector2(
                    Outputs[i].Position.X + Outputs[i].Size.X - Outputs[i].HotSize.X,
                    Outputs[i].Position.Y + (Outputs[i].Size.Y - Outputs[i].HotSize.Y) * 0.5f + 2.0f);

                nameSize = CalcTextSize(Outputs[i].Name);
                Outputs[i].NamePosition = new Vector2(
                    Outputs[i].Position.X,
                    Outputs[i].Position.Y + (Outputs[i].Size.Y - nameSize.Y) * 0.5f);
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
            if (oPin.LinkDesc != null && iPin.LinkDesc != null)
            {
                foreach (var i in oPin.LinkDesc.CanLinks)
                {
                    foreach (var j in iPin.LinkDesc.CanLinks)
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
            if (oPin.LinkDesc != null && iPin.LinkDesc != null)
            {
                foreach (var i in oPin.LinkDesc.CanLinks)
                {
                    foreach (var j in iPin.LinkDesc.CanLinks)
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
        public virtual PinIn AddPinIn(PinIn pin)
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
        public virtual PinIn InsertPinIn(int idx, PinIn pin)
        {
            if(idx < 0 || idx >= Inputs.Count)
                return AddPinIn(pin);

            pin.HostNode = this;
            foreach (var i in Inputs)
            {
                if (i == pin)
                    return pin;
            }
            Inputs.Insert(idx, pin);
            return pin;
        }
        public virtual void RemovePinIn(PinIn pin)
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
        public virtual PinOut AddPinOut(PinOut pin)
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
        public virtual void RemovePinOut(PinOut pin)
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
        public delegate void Deleage_OnPreviewDraw(UNodeBase node, in Vector2 prevStart, in Vector2 prevEnd, ImDrawList cmdlist);
        public Deleage_OnPreviewDraw OnPreviewDrawAction;
        public virtual void OnPreviewDraw(in Vector2 prevStart, in Vector2 prevEnd, ImDrawList cmdlist)
        {
            OnPreviewDrawAction?.Invoke(this, in prevStart, in prevEnd, cmdlist);
        }
        public Action<UNodeBase, UNodeGraphStyles, ImDrawList> OnAfterDrawAction;
        public virtual void OnAfterDraw(UNodeGraphStyles styles, ImDrawList cmdlist)
        {
            OnAfterDrawAction?.Invoke(this, styles, cmdlist);
        }
        public Action<UNodeBase, NodePin> OnShowPinMenuAction;
        public virtual void OnShowPinMenu(NodePin pin)
        {
            OnShowPinMenuAction?.Invoke(this, pin);
        }
        public Action<UNodeBase, UPinLinker> OnRemoveLinkerAction;
        public virtual void OnRemoveLinker(UPinLinker linker)
        {
            OnRemoveLinkerAction?.Invoke(this, linker);
        }
        public Action<UNodeBase, UPinLinker> OnLoadLinkerAction;
        public virtual void OnLoadLinker(UPinLinker linker)
        {
            OnLoadLinkerAction?.Invoke(this, linker);
        }
        public Action<UNodeBase, PinOut, UNodeBase, PinIn> OnLinkedToAction;
        public virtual void OnLinkedTo(PinOut oPin, UNodeBase InNode, PinIn iPin)
        {
            OnLinkedToAction?.Invoke(this, oPin, InNode, iPin);
        }
        public Action<UNodeBase, PinIn, UNodeBase, PinOut> OnLinkedFromAction;
        public virtual void OnLinkedFrom(PinIn iPin, UNodeBase OutNode, PinOut oPin)
        {
            OnLinkedFromAction?.Invoke(this, iPin, OutNode, oPin);
        }
        public Action<UNodeBase> OnDoubleClickAction;
        public virtual void OnDoubleClick() 
        {
            OnDoubleClickAction?.Invoke(this);
        }
        public Action<UNodeBase, NodePin> OnOnDoubleClickedPinAction;
        public virtual void OnDoubleClickedPin(NodePin hitPin)
        {
            OnOnDoubleClickedPinAction?.Invoke(this, hitPin);
        }
        public Action<UNodeBase, NodePin> OnLButtonClickedAction;
        public virtual void OnLButtonClicked(NodePin hitPin)
        {
            OnLButtonClickedAction?.Invoke(this, hitPin);
        }
        public Func<UNodeBase, object> GetPropertyEditObjectAction;
        public virtual object GetPropertyEditObject()
        {
            if (GetPropertyEditObjectAction != null)
                return GetPropertyEditObjectAction(this);
            return this;
        }
        public Action<UNodeBase, NodePin> OnMouseStayPinAction;
        public virtual void OnMouseStayPin(NodePin stayPin)
        {
            OnMouseStayPinAction?.Invoke(this, stayPin);
        }
        public Func<PinOut, Rtti.UTypeDesc> GetOutPinTypeAction;
        public virtual Rtti.UTypeDesc GetOutPinType(PinOut pin)
        {
            if (GetOutPinTypeAction != null)
                return GetOutPinTypeAction(pin);
            return null;
        }
        public Func<PinIn, Rtti.UTypeDesc> GetInPinTypeAction;
        public virtual Rtti.UTypeDesc GetInPinType(PinIn pin)
        {
            if (GetInPinTypeAction != null)
                return GetInPinTypeAction(pin);
            return null;
        }
        public Rtti.UTypeDesc GetPinType<T>(T pin)
        {
            if (typeof(T) == typeof(PinIn))
                return GetInPinType(pin as PinIn);
            else
                return GetOutPinType(pin as PinOut);
        }
        public virtual void BuildStatements(NodePin pin, ref BuildCodeStatementsData data) 
        {
            throw new InvalidOperationException("Invalid build statements");
        }
        public virtual CodeBuilder.UExpressionBase GetExpression(NodePin pin, ref BuildCodeStatementsData data) 
        {
            throw new NotImplementedException("Invalid get expression");
        }
        public Action<UNodeBase> OnRemoveNodeAction;
        public virtual void OnRemoveNode()
        {
            OnRemoveNodeAction?.Invoke(this);
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

        public static void AddDebugBreakerStatement(string breakName, ref BuildCodeStatementsData data)
        {
            var breakType = Rtti.UTypeDesc.TypeOf(typeof(EngineNS.Macross.UMacrossBreak));
            var breakDef = new CodeBuilder.UVariableDeclaration()
            {
                VariableType = new CodeBuilder.UTypeReference(breakType),
                VariableName = breakName,
                InitValue = new CodeBuilder.UCreateObjectExpression(data.CodeGen.GetTypeString(breakType), new CodeBuilder.UPrimitiveExpression(breakName))
            };
            if (!data.ClassDec.PreDefineVariables.Contains(breakDef))
                data.ClassDec.PreDefineVariables.Add(breakDef);
            data.CurrentStatements.Add(new CodeBuilder.UDebuggerTryBreak(breakName));
        }
        public static string GetRuntimeValueString(string name)
        {
            if (Macross.UMacrossDebugger.Instance.CurrrentBreak != null && Macross.UMacrossDebugger.Instance.CurrrentBreak.BreakStackFrame != null)
            {
                if (Macross.UMacrossDebugger.Instance.CurrrentBreak.BreakStackFrame.HasWatchVariable(name))
                {
                    var obj = Macross.UMacrossDebugger.Instance.CurrrentBreak.BreakStackFrame.GetWatchVariable(name);
                    return (obj == null) ? "null" : obj.ToString() + " ";
                }
            }
            return "";
        }
        public virtual bool CopyTo(UNodeBase target, bool withId = false)
        {
            if (target.GetType() != this.GetType())
                return false;

            var id = target.NodeId;
            var type = Rtti.UTypeDesc.TypeOf(this.GetType());
            var meta = Rtti.TtClassMetaManager.Instance.GetMeta(type);
            meta.CopyObjectMetaField(target, this);
            if(!withId)
                target.NodeId = id;
            return true;
        }
    }
}
