using EngineNS.EGui.Controls.PropertyGrid;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;

namespace EngineNS.Bricks.NodeGraph
{
    public struct BuildCodeStatementsData
    {
        public CodeBuilder.UNamespaceDeclaration NsDec;
        public CodeBuilder.UClassDeclaration ClassDec;
        public CodeBuilder.UMethodDeclaration MethodDec;
        public CodeBuilder.UCodeGeneratorBase CodeGen;
        public UNodeGraph NodeGraph;
        public List<CodeBuilder.UStatementBase> CurrentStatements;
        public object UserData;
        
        public void CopyTo(ref BuildCodeStatementsData data)
        {
            data.NsDec = NsDec;
            data.ClassDec = ClassDec;
            data.MethodDec = MethodDec;
            data.NodeGraph = NodeGraph;
            data.CodeGen = CodeGen;
        }
    }

    public partial class UNodeGraph : IO.ISerializer
    {
        public virtual void OnPreRead(object tagObject, object hostObject, bool fromXml) { }
        public virtual void OnPropertyRead(object root, System.Reflection.PropertyInfo prop, bool fromXml) { }
        public virtual void Initialize()
        {
            UpdateCanvasMenus();
            UpdateNodeMenus();
            UpdatePinMenus();
        }
        public virtual void SetDefaultActionForNode(UNodeBase node) { }
        [Rtti.Meta]
        public RName AssetName { get; set; }
        [Rtti.Meta]
        public string GraphName { get; set; } = "NodeGraph";
        [Rtti.Meta]
        public List<UNodeBase> Nodes { get; } = new List<UNodeBase>();
        [Rtti.Meta(Order = 1)]
        public List<UPinLinker> Linkers { get; } = new List<UPinLinker>();
        
        public ULinkingLine LinkingOp { get; } = new ULinkingLine();
        public class FSelNodeState : EGui.Controls.PropertyGrid.IPropertyCustomization
        {
            public UNodeBase Node;
            public Vector2 MoveOffset;

            public void GetProperties(ref CustomPropertyDescriptorCollection collection, bool parentIsValueType)
            {
                if (Node == null)
                    return;
                var pros = TypeDescriptor.GetProperties(Node);
                collection.InitValue(Node, Rtti.UTypeDesc.TypeOf(Node.GetType()), pros, parentIsValueType);
            }

            public object GetPropertyValue(string propertyName)
            {
                if (Node == null)
                    return null;
                var pro = Node.GetType().GetProperty(propertyName);
                if (pro == null)
                    return null;
                return pro.GetValue(Node);
            }

            public void SetPropertyValue(string propertyName, object value)
            {
                if (Node == null)
                    return;
                var pro = Node.GetType().GetProperty(propertyName);
                if (pro == null)
                    return;
                pro.SetValue(Node, value);
            }
        }
        public bool SelectedNodesDirty = false;
        public List<FSelNodeState> SelectedNodes { get; } = new List<FSelNodeState>();

        public Dictionary<UNodeBase, UNodeGraph> SubGraphs;
        public UNodeGraph ParentGraph { get; set; }

        public delegate void FOnChangeGraph(UNodeGraph graph);
        public FOnChangeGraph OnChangeGraph;
        public void ChangeGraph(UNodeGraph graph)
        {
            if (OnChangeGraph != null)
            {
                OnChangeGraph(graph);
            }
        }

        #region DataOp
        public void ResetGraph()
        {
            Nodes.Clear();
            Linkers.Clear();
            PreOrderLinker = null;
            PreOrderPinIn = null;
            PreOrderPinOut = null;
        }
        [Rtti.Meta]
        public UNodeBase FindFirstNode(string name)
        {
            foreach (var i in Nodes)
            {
                if (i.Name == name)
                    return i;
            }
            return null;
        }
        public UNodeBase FindNode(in Guid id)
        {
            foreach (var i in Nodes)
            {
                if (i.NodeId == id)
                    return i;
            }
            return null;
        }
        public void AddLink(UNodeBase OutNode, string OutPin,
            UNodeBase InNode, string InPin, bool bCallLinked = true)
        {
            PinOut oPin = null;
            if (OutNode != null)
            {
                oPin = OutNode.FindPinOut(OutPin);
            }
            PinIn iPin = null;
            if (InNode != null)
            {
                iPin = InNode.FindPinIn(InPin);
            }
            AddLink(oPin, iPin, bCallLinked);
        }
        public bool CanLink(PinOut oPin, PinIn iPin)
        {
            if (oPin == null || iPin == null)
                return false;
            return (oPin.HostNode.CanLinkTo(oPin, iPin.HostNode, iPin) &&
                    iPin.HostNode.CanLinkFrom(iPin, oPin.HostNode, oPin));
        }
        public void AddLink(PinOut oPin, PinIn iPin, bool bCallLinked)
        {
            foreach (var i in Linkers)
            {
                if (i.InPin == iPin && i.OutPin == oPin)
                    return;
            }
            if (CanLink(oPin, iPin))
            {
                if(!iPin.MultiLinks)
                    RemoveLink(iPin);
                var result = new UPinLinker();
                if (iPin != null)
                {
                    result.InPin = iPin;
                }
                if (oPin != null)
                {
                    result.OutPin = oPin;
                }
                Linkers.Add(result);
                if (bCallLinked)
                {
                    oPin.HostNode.OnLinkedTo(oPin, iPin.HostNode, iPin);
                    iPin.HostNode.OnLinkedFrom(iPin, oPin.HostNode, oPin);
                }
            }
        }
        public void RemoveLink(NodePin pin)
        {
            for (int i = 0; i < Linkers.Count; i++)
            {
                var linker = Linkers[i];
                if (linker.InPin == pin || linker.OutPin == pin)
                {
                    linker.InPin.HostNode.OnRemoveLinker(linker);
                    linker.OutPin.HostNode.OnRemoveLinker(linker);
                    linker.InPin = null;
                    linker.OutPin = null;
                    Linkers.RemoveAt(i);
                    i--;
                }
            }
        }
        public void RemoveLink(PinOut oPin, PinIn iPin)
        {
            for (int i = 0; i < Linkers.Count; i++)
            {
                if (Linkers[i].InPin == iPin && Linkers[i].OutPin == oPin)
                {
                    var saved = Linkers[i];
                    _RemoveLinker(i);
                    saved.InPin = null;
                    saved.OutPin = null;
                    return;
                }
            }
        }
        private void _RemoveLinker(int index)
        {
            var linker = Linkers[index];
            linker.InPin.HostNode.OnRemoveLinker(linker);
            linker.OutPin.HostNode.OnRemoveLinker(linker);
            Linkers.RemoveAt(index);
        }
        public void RemoveLinkedIn(PinIn pin)
        {
            for (int i = 0; i < Linkers.Count; i++)
            {
                if (Linkers[i].InPin == pin)
                {
                    var saved = Linkers[i];
                    _RemoveLinker(i);
                    i--;
                    saved.InPin = null;
                    saved.OutPin = null;
                }
            }
        }
        public void RemoveLinkedInExcept(PinIn pin, UNodeBase OutNode, string OutPin)
        {
            for (int i = 0; i < Linkers.Count; i++)
            {
                if (Linkers[i].InPin == pin)
                {
                    if (Linkers[i].OutPin.HostNode == OutNode && Linkers[i].OutPin.Name == OutPin)
                        continue;
                    var saved = Linkers[i];
                    _RemoveLinker(i);
                    i--;
                    saved.InPin = null;
                    saved.OutPin = null;
                }
            }
        }
        public void RemoveLinkedOut(PinOut pin)
        {
            for (int i = 0; i < Linkers.Count; i++)
            {
                if (Linkers[i].OutPin == pin)
                {
                    var saved = Linkers[i];
                    _RemoveLinker(i);
                    i--;
                    saved.InPin = null;
                    saved.OutPin = null;
                }
            }
        }
        public void RemoveLinkedOutExcept(PinOut pin, UNodeBase InNode, string InPin)
        {
            for (int i = 0; i < Linkers.Count; i++)
            {
                if (Linkers[i].OutPin == pin)
                {
                    if (Linkers[i].InPin.HostNode == InNode && Linkers[i].InPin.Name == InPin)
                        continue;

                    var saved = Linkers[i];
                    _RemoveLinker(i);
                    i--;
                    saved.InPin = null;
                    saved.OutPin = null;
                }
            }
        }
        public int GetNumOfOutLinker(PinOut pin)
        {
            int num = 0;
            foreach (var i in Linkers)
            {
                if (i.OutPin == pin)
                {
                    num++;
                }
            }
            return num;
        }
        public void FindOutLinker(PinOut pin, List<UPinLinker> linkers)
        {
            linkers.Clear();
            foreach (var i in Linkers)
            {
                if (i.OutPin == pin)
                {
                    linkers.Add(i);
                }
            }
        }
        public UPinLinker GetFirstLinker(PinOut pin)
        {
            foreach (var i in Linkers)
            {
                if (i.OutPin == pin)
                {
                    return i;
                }
            }
            return null;
        }
        public bool PinHasLinker(NodePin pin)
        {
            foreach (var i in Linkers)
            {
                if (i.InPin == pin || i.OutPin == pin)
                {
                    return true;
                }
            }
            return false;
        }
        public UPinLinker FindInLinkerSingle(PinIn pin)
        {
            foreach (var i in Linkers)
            {
                if (i.InPin == pin)
                {
                    return i;
                }
            }
            return null;
        }
        public UPinLinker FindOutLinkerSingle(PinOut pin)
        {
            foreach (var i in Linkers)
            {
                if (i.OutPin == pin)
                {
                    return i;
                }
            }
            return null;
        }
        public void FindInLinker(PinIn pin, List<UPinLinker> linkers)
        {
            linkers.Clear();
            foreach (var i in Linkers)
            {
                if (i.InPin == pin)
                {
                    linkers.Add(i);
                }
            }
        }
        public UPinLinker GetFirstLinker(PinIn pin)
        {
            foreach (var i in Linkers)
            {
                if (i.InPin == pin)
                {
                    return i;
                }
            }
            return null;
        }
        public UNodeBase GetOppositePinNode(PinIn pin)
        {
            var linker = GetFirstLinker(pin);
            if (linker == null)
            {
                return null;
            }
            return linker.OutPin.HostNode;
        }
        public UNodeBase GetOppositePinNode(PinOut pin)
        {
            var linker = GetFirstLinker(pin);
            if (linker == null)
            {
                return null;
            }
            return linker.InPin.HostNode;
        }
        public CodeBuilder.UExpressionBase GetOppositePinExpression(PinIn pin, ref BuildCodeStatementsData data)
        {
            var linker = GetFirstLinker(pin);
            if (linker == null)
                return null;
            return linker.OutPin.HostNode.GetExpression(linker.OutPin, ref data);
        }
        public CodeBuilder.UExpressionBase GetOppositePinExpression(PinOut pin, ref BuildCodeStatementsData data)
        {
            var linker = GetFirstLinker(pin);
            if (linker == null)
                return null;
            return linker.InPin.HostNode.GetExpression(linker.InPin, ref data);
        }
        public PinOut GetOppositePin(PinIn pin)
        {
            var linker = GetFirstLinker(pin);
            if (linker == null)
                return null;
            return linker.OutPin;
        }
        public PinIn GetOppositePin(PinOut pin)
        {
            var linker = GetFirstLinker(pin);
            if (linker == null)
                return null;
            return linker.InPin;
        }
        public Rtti.UTypeDesc GetOppositePinType(PinIn pin)
        {
            var linker = GetFirstLinker(pin);
            if (linker == null)
                return null;
            return linker.OutPin.HostNode.GetOutPinType(linker.OutPin);
        }
        public Rtti.UTypeDesc GetOppositePinType(PinOut pin)
        {
            var linker = GetFirstLinker(pin);
            if (linker == null)
                return null;
            return linker.InPin.HostNode.GetInPinType(linker.InPin);
        }

        public UNodeBase AddNode(UNodeBase node)
        {
            node.ParentGraph = this;
            foreach (var i in Nodes)
            {
                if (i == node)
                {
                    return i;
                }
            }
            Nodes.Add(node);
            return node;
        }
        public void RemoveNode(UNodeBase node)
        {
            node.OnRemoveNode();
            foreach (var i in node.Inputs)
            {
                RemoveLinkedIn(i);
            }
            foreach (var i in node.Outputs)
            {
                RemoveLinkedOut(i);
            }
            for (int i = 0; i < Nodes.Count; i++)
            {
                if (Nodes[i] == node)
                {
                    Nodes.RemoveAt(i);
                    break;
                }
            }
            node.ParentGraph = null;
        }

        public bool IsInViewport(UNodeBase node)
        {
            if (node.Position.X > PositionVP.X + SizeVP.X ||
                node.Position.Y > PositionVP.Y + SizeVP.Y ||
                node.Position.X + node.Size.X < PositionVP.X ||
                node.Position.Y + node.Size.Y < PositionVP.Y)
            {
                return false;
            }
            return true;
        }
        public void AddSelected(UNodeBase node)
        {
            node.Selected = true;
            FSelNodeState state = new FSelNodeState();
            state.Node = node;
            SelectedNodes.Add(state);
            SelectedNodesDirty = true;
        }
        public void RemoveSelected(UNodeBase node)
        {
            node.Selected = false;
            for (int i = 0; i < SelectedNodes.Count; i++)
            {
                if (SelectedNodes[i].Node == node)
                {
                    SelectedNodes.RemoveAt(i);
                    SelectedNodesDirty = true;
                    i--;
                }
            }
        }
        public void ClearSelected()
        {
            foreach (var i in SelectedNodes)
            {
                i.Node.Selected = false;
            }
            SelectedNodes.Clear();
            SelectedNodesDirty = true;
        }
        #endregion

        #region Event
        public enum EKey
        {
            Alt,
            Ctl,
            Shift,
        }
        public enum EMouseButton : int
        {
            Left,
            Right,
            Middle,
            Number,
        };
        public bool[] ButtonPress = new bool[(int)EMouseButton.Number];
        public Vector2 PressPosition;
        public Vector2 DragPosition;

        protected Vector2 PositionVP;
        protected Vector2 SizeVP;
        //Vector2 MoveVPOffset;

        public Vector2 PhysicalSizeVP;
        public float ScaleVP = 1.0f;

        protected bool IsZooming;
        protected Vector2 ZoomCenter;
        protected bool mIsMovingSelNodes;
        public bool IsMovingSelNodes => mIsMovingSelNodes;

        public enum EGraphMenu
        {
            None,
            Canvas,
            Node,
            Pin,
            Object,
        };

        EGraphMenu mCurMenuType;
        public bool FirstSetCurMenuType = false;
        public EGraphMenu CurMenuType
        {
            get => mCurMenuType;
            set
            {
                if(mCurMenuType != value)
                    FirstSetCurMenuType = true;
                mCurMenuType = value;
            }
        }
        public object PopMenuPressObject;
        public Vector2 PopMenuPosition;
        public UMenuItem CanvasMenus = new UMenuItem();
        public UMenuItem NodeMenus = new UMenuItem();
        public UMenuItem PinMenus = new UMenuItem();
        public UMenuItem ObjectMenus = new UMenuItem();

        public bool CanvasMenuDirty = true;
        public virtual void UpdateCanvasMenus()
        {
            CanvasMenus.SubMenuItems.Clear();
            CanvasMenus.Text = "Canvas";
        }
        public bool NodeMenuDirty = false;
        public virtual void UpdateNodeMenus()
        {
            NodeMenus.SubMenuItems.Clear();
            NodeMenus.Text = "Node";
            NodeMenus.AddMenuItem(
                "Delete Node", null,
                (UMenuItem item, object sender) =>
                {
                    var pNode = sender as UNodeBase;
                    if (pNode == null)
                        return;

                    this.RemoveNode(pNode);
                });            
            NodeMenus.AddMenuItem(
                "Delete Selected", null,
                (UMenuItem item, object sender) =>
                {
                    foreach (var i in SelectedNodes)
                    {
                        this.RemoveNode(i.Node);
                    }
                    this.ClearSelected();
                });
        }
        public bool PinMenuDirty = false;
        public virtual void UpdatePinMenus()
        {
            PinMenus.SubMenuItems.Clear();
            PinMenus.Text = "Pin";
            PinMenus.AddMenuItem(
                "Break All", null,
                (UMenuItem item, object sender) =>
                {
                    if (PopMenuPressObject != null &&
                        Rtti.UTypeDesc.CanCast(PopMenuPressObject.GetType(), typeof(NodePin)))
                    {
                        this.RemoveLink(this.PopMenuPressObject as NodePin);
                        PopMenuPressObject = null;
                    }
                });
        }
        public bool PinLinkMenuDirty = false;
        public virtual void UpdatePinLinkMenu()
        {
            //PopMenuPressObject
        }

        public virtual bool IsKeydown(EKey key)
        {
            switch(key)
            {
                case EKey.Ctl:
                    return UEngine.Instance.InputSystem.IsKeyDown(Input.Keycode.KEY_LCTRL) || UEngine.Instance.InputSystem.IsKeyDown(Input.Keycode.KEY_RCTRL);
                case EKey.Shift:
                    return UEngine.Instance.InputSystem.IsKeyDown(Input.Keycode.KEY_LSHIFT) || UEngine.Instance.InputSystem.IsKeyDown(Input.Keycode.KEY_RSHIFT);
                case EKey.Alt:
                    return UEngine.Instance.InputSystem.IsKeyDown(Input.Keycode.KEY_LALT) || UEngine.Instance.InputSystem.IsKeyDown(Input.Keycode.KEY_RALT);
            }

            return false;
        }
        public object HitObject(float x, float y)
        {
            foreach (var i in Nodes)
            {
                if (i.IsHit(x, y))
                {
                    var pin = i.IsHitPin(x, y);
                    if (pin != null)
                        return pin;
                    return i;
                }
            }
            return null;
        }
        public Vector2 ToScreenPos(float x, float y)
        {
            return new Vector2(x / PhysicalSizeVP.X, y / PhysicalSizeVP.Y);
        }
        public void SetPhysicalSizeVP(float x, float y)
        {
            PhysicalSizeVP = new Vector2(x, y);
            SizeVP = PhysicalSizeVP * ScaleVP;
        }
        public void SetScaleVP(float scale)
        {
            ScaleVP = scale;
            if (ScaleVP > 2.0f)
            {
                ScaleVP = 2.0f;
            }
            else if (ScaleVP < 0.2f)
            {
                ScaleVP = 0.2f;
            }
            SizeVP = PhysicalSizeVP * ScaleVP;
        }
        public Vector2 CanvasToViewportRate(in Vector2 pos)
        {
            var t = pos - PositionVP;
            t.X /= SizeVP.X;
            t.Y /= SizeVP.Y;
            return t;
        }
        public Vector2 ViewportRateToCanvas(in Vector2 pos)
        {
            Vector2 t;
            t.X = pos.X * SizeVP.X;
            t.Y = pos.Y * SizeVP.Y;
            return t + PositionVP;
        }
        public Vector2 CanvasToViewport(in Vector2 pos)
        {
            var rate = CanvasToViewportRate(in pos);
            Vector2 t;
            t.X = PhysicalSizeVP.X * rate.X;
            t.Y = PhysicalSizeVP.Y * rate.Y;
            return t;
        }
        public void GetSelectNodes(in Vector2 start, in Vector2 end, List<UNodeBase> OutNodes)
        {
            foreach (var i in Nodes)
            {
                if (i.IsHitRect(start, end))
                {
                    OutNodes.Add(i);
                }
            }
        }

        public void ResetButtonPress()
        {
            for(int i = 0; i < ButtonPress.Length; i++)
            {
                ButtonPress[i] = false;
            }
        }
        public bool MultiSelectionMode = false;
        public void LeftPress(in Vector2 screenPos)
        {
            ButtonPress[(int)EMouseButton.Left] = true;
            PressPosition = ViewportRateToCanvas(in screenPos);

            var hit = HitObject(PressPosition.X, PressPosition.Y);
            MultiSelectionMode = false;
            if (hit == null)
            {
                MultiSelectionMode = true;
                return;
            }

            var pKls = hit.GetType();
            if (Rtti.UTypeDesc.CanCast(pKls, typeof(UNodeBase)))
            {
                var node = hit as UNodeBase;
                if (node.Selected)
                {
                    if(this.IsKeydown(EKey.Ctl))
                        RemoveSelected(node);
                }
                else
                {
                    if (!IsKeydown(EKey.Ctl) && !IsKeydown(EKey.Shift))
                        ClearSelected();
                    AddSelected(node);
                }
                if(node.IsHitTitle(PressPosition.X, PressPosition.Y))
                {
                    mIsMovingSelNodes = true;
                    foreach (var i in SelectedNodes)
                    {
                        i.MoveOffset = PressPosition - i.Node.Position;
                    }
                }
            }
            else if (Rtti.UTypeDesc.CanCast(pKls, typeof(NodePin)))
            {
                var pin = hit as NodePin;
                LinkingOp.StartPin = pin;
                LinkingOp.HoverPin = pin;
                LinkingOp.BlockingEnd = PressPosition;
                PopMenuPressObject = pin.HostNode.GetPinType(pin);
                PinLinkMenuDirty = true;
            }
        }

        public void LeftRelease(in Vector2 screenPos)
        {
            DragPosition = ViewportRateToCanvas(in screenPos);
            //if (ButtonPress[EMouseButton::Left])
            UNodeBase pressNode = null;
            if (CurMenuType == EGraphMenu.None)
            {
                if (!mIsMovingSelNodes)
                {
                    if (LinkingOp.StartPin != null)
                    {
                        var hit = LinkingOp.HoverPin; //HitObject(DragPosition.X, DragPosition.Y);
                        if (hit != null)
                        {
                            var pKls = hit.GetType();
                            if (Rtti.UTypeDesc.CanCast(pKls,typeof(PinIn)) &&
                                Rtti.UTypeDesc.CanCast(LinkingOp.StartPin.GetType(),typeof(PinOut)))
                            {
                                var outPin = LinkingOp.StartPin as PinOut;
                                var inPin = hit as PinIn;
                                AddLink(outPin, inPin, true);
                                pressNode = inPin.HostNode;
                            }
                            else if (
                                Rtti.UTypeDesc.CanCast(pKls,typeof(PinOut)) &&
                                Rtti.UTypeDesc.CanCast(LinkingOp.StartPin.GetType(),typeof(PinIn)))
                            {
                                var outPin = hit as PinOut;
                                var inPin = LinkingOp.StartPin as PinIn;
                                AddLink(outPin, inPin, true);
                                pressNode = outPin.HostNode;
                            }
                            //else
                            //{
                            //    pressNode = hit as UNodeBase;
                            //}
                        }
                        else
                        {
                            // 打开该pin关联的菜单
                            PopMenuPosition = ViewportRateToCanvas(in screenPos);
                            if(PopMenuPressObject != null && PopMenuPressObject != this)
                            {
                                CurMenuType = EGraphMenu.Object;
                            }
                        }
                    }
                    else
                    {
                        var hit = HitObject(DragPosition.X, DragPosition.Y);
                        var hitPin = hit as NodePin;
                        if (hitPin != null)
                        {
                            hitPin.HostNode.OnLButtonClicked(hitPin);
                            pressNode = hitPin.HostNode;
                        }
                        else
                        {
                            var hitNode = hit as UNodeBase;
                            if (hitNode != null)
                            {
                                hitNode.OnLButtonClicked(null);
                                pressNode = hitNode;
                            }
                            else
                                OnLButtonClicked();
                        }

                        if(MultiSelectionMode)
                        {
                            if(!this.IsKeydown(EKey.Shift) && !this.IsKeydown(EKey.Ctl))
                                ClearSelected();
                            var start = PressPosition;
                            var end = DragPosition;
                            if (start.X > end.X)
                            {
                                var save = end.X;
                                end.X = start.X;
                                start.X = save;
                            }
                            if (start.Y > end.Y)
                            {
                                var save = end.Y;
                                end.Y = start.Y;
                                start.Y = save;
                            }
                            List<UNodeBase> sels = new List<UNodeBase>();
                            GetSelectNodes(start, end, sels);
                            if(this.IsKeydown(EKey.Ctl))
                            {
                                foreach (var i in sels)
                                {
                                    if (i.Selected)
                                        RemoveSelected(i);
                                    else
                                        AddSelected(i);
                                }
                            }
                            else
                            {
                                foreach (var i in sels)
                                {
                                    AddSelected(i);
                                }
                            }
                        }

                    }
                }
                else
                {
                    if(PreOrderLinker != null)
                    {
                        AddLink(PreOrderLinker.OutPin, PreOrderPinIn, true);
                        AddLink(PreOrderPinOut, PreOrderLinker.InPin, true);
                        // todo: rearrange
                    }
                    else
                    {
                        var hit = HitObject(DragPosition.X, DragPosition.Y);
                        var hitPin = hit as NodePin;
                        if (hitPin != null)
                            hitPin.HostNode.OnLButtonClicked(hitPin);
                        else
                        {
                            var hitNode = hit as UNodeBase;
                            if (hitNode != null)
                                hitNode.OnLButtonClicked(null);
                            else
                                OnLButtonClicked();
                        }
                    }
                }
            }
            ButtonPress[(int)EMouseButton.Left] = false;
            mIsMovingSelNodes = false;
            mLastDragDirection = Vector2.Zero;
            mShakeTime = 0;
            if (OnLinkingUp(LinkingOp, pressNode))
            {
                LinkingOp.StartPin = null;
            }
            //LinkingOp.StartPin = null;
            MultiSelectionMode = false;
            PreOrderLinker = null;
            PreOrderPinIn = null;
            PreOrderPinOut = null;
            mCheckCopyDrag = true;
        }

        public void RightPress(in Vector2 screenPos)
        {
            ButtonPress[(int)EMouseButton.Right] = true;
            PressPosition = ViewportRateToCanvas(in screenPos);
            var hit = HitObject(PressPosition.X, PressPosition.Y);
            if (hit == null)
                return;
        }
        public void RightRelease(in Vector2 screenPos)
        {
            PopMenuPosition = ViewportRateToCanvas(in screenPos);
            PopMenuPressObject = HitObject(PopMenuPosition.X, PopMenuPosition.Y);
            if (PopMenuPressObject != null)
            {
                var pKls = PopMenuPressObject.GetType();
                if (pKls == typeof(PinIn) || pKls == typeof(PinOut))
                {
                    CurMenuType = EGraphMenu.Pin;
                    PinMenuDirty = true;
                }
                else if (Rtti.UTypeDesc.CanCast(pKls, typeof(UNodeBase)))
                {
                    CurMenuType = EGraphMenu.Node;
                    NodeMenuDirty = true;
                }
            }
            else
            {
                CurMenuType = EGraphMenu.Canvas;
                PopMenuPressObject = this;
            }
            ButtonPress[(int)EMouseButton.Right] = false;
        }
        public void MiddlePress(in Vector2 screenPos)
        {
            ButtonPress[(int)EMouseButton.Middle] = true;
            PressPosition = ViewportRateToCanvas(in screenPos);
        }
        public void MiddleRelease(in Vector2 screenPos)
        {
            var pos = ViewportRateToCanvas(in screenPos);
            var hit = HitObject(pos.X, pos.Y);
            if (hit != null)
            {

            }

            ButtonPress[(int)EMouseButton.Middle] = false;
        }
        Vector2 mLastDragPosition;
        Vector2 mLastDragDirection;
        int mShakeTime = 0;
        float mShakeElapsedSecond = 0;
        bool mCheckCopyDrag = true;
        public bool CheckMouseShake()
        {
            var dir = DragPosition - mLastDragPosition;
            dir.Normalize();
            if (mLastDragDirection == Vector2.Zero)
            {
                mShakeElapsedSecond = 0.0f;
                mLastDragDirection = dir;
                return false;
            }

            mShakeElapsedSecond += EngineNS.UEngine.Instance.ElapsedSecond;
            var dotVal = Vector2.Dot(dir, mLastDragDirection);
            if(dotVal < 0)
            {
                mLastDragDirection = dir;
                if (mShakeElapsedSecond < 0.3f)
                {
                    mShakeElapsedSecond = 0.0f;
                    mShakeTime++;
                    if(mShakeTime >= 6)
                    {
                        mShakeTime = 0;
                        return true;
                    }
                }
                else
                    mShakeTime = 0;
            }

            return false;
        }
        public void PressDrag(in Vector2 screenPos)
        {
            mLastDragPosition = DragPosition;
            DragPosition = ViewportRateToCanvas(in screenPos);
            LinkingOp.HoverPin = null;
            object hit = null;
            if(LinkingOp.StartPin != null)
            {
                float minLen = 50;
                for (int i = 0; i < Nodes.Count; i++)
                {
                    if (Nodes[i] == LinkingOp.StartPin.HostNode)
                        continue;

                    if (Vector2.Intersects(Nodes[i].Position, Nodes[i].Position + Nodes[i].Size, DragPosition, 50))
                    {
                        if (LinkingOp.StartPin is PinIn)
                        {
                            for (int pinId = 0; pinId < Nodes[i].Outputs.Count; pinId++)
                            {
                                var pin = Nodes[i].Outputs[pinId];
                                var len = (DragPosition - (pin.HotPosition + pin.HotSize * 0.5f)).Length();
                                if (len < minLen && CanLink(pin, LinkingOp.StartPin as PinIn))
                                {
                                    hit = pin;
                                    minLen = len;
                                }
                            }
                        }
                        else
                        {
                            for (int pinId = 0; pinId < Nodes[i].Inputs.Count; pinId++)
                            {
                                var pin = Nodes[i].Inputs[pinId];
                                var len = (DragPosition - (pin.HotPosition + pin.HotSize * 0.5f)).Length();
                                if (len < minLen && CanLink(LinkingOp.StartPin as PinOut, pin))
                                {
                                    hit = pin;
                                    minLen = len;
                                }
                            }
                        }
                    }
                }
            }
            else
                hit = HitObject(DragPosition.X, DragPosition.Y);
            if (hit != null)
            {
                if(Rtti.UTypeDesc.CanCast(hit.GetType(), typeof(NodePin)))
                {
                    LinkingOp.HoverPin = hit as NodePin;
                    if (LinkingOp.HoverPin != null)
                    {
                        LinkingOp.HoverPin.HostNode.OnMouseStayPin(LinkingOp.HoverPin);
                    }
                }
                else if(Rtti.UTypeDesc.CanCast(hit.GetType(), typeof(UNodeBase)))
                {
                    var node = hit as UNodeBase;
                    if(node.HasError && node.CodeExcept != null)
                    {
                        EGui.Controls.CtrlUtility.DrawHelper(node.CodeExcept.ErrorInfo);
                    }
                }
            }

            if (ButtonPress[(int)EMouseButton.Left])
            {
                if (mIsMovingSelNodes)
                {
                    if (ImGuiAPI.IsMouseDragging(ImGuiMouseButton_.ImGuiMouseButton_Left, -1))
                    {
                        if(IsKeydown(EKey.Shift) && mCheckCopyDrag)
                        {
                            mCheckCopyDrag = false;
                            // copy selected nodes
                            List<FSelNodeState> copyedNodes = new List<FSelNodeState>(SelectedNodes.Count);
                            foreach (var i in SelectedNodes)
                            {
                                var type = Rtti.UTypeDesc.TypeOf(i.Node.GetType());
                                var node = Rtti.UTypeDescManager.CreateInstance(type) as UNodeBase;
                                i.Node.CopyTo(node);
                                node.Name = i.Node.Name + "_copy";
                                node.UserData = i.Node.UserData;
                                node.Position = i.Node.Position;
                                SetDefaultActionForNode(node);
                                this.AddNode(node);
                                i.Node.Selected = false;
                                node.Selected = true;
                                copyedNodes.Add(new FSelNodeState()
                                {
                                    Node = node,
                                    MoveOffset = i.MoveOffset,
                                });
                            }
                            SelectedNodes.Clear();
                            SelectedNodes.AddRange(copyedNodes.ToArray());
                            SelectedNodesDirty = true;
                        }
                        foreach (var i in SelectedNodes)
                        {
                            i.Node.Position = DragPosition - i.MoveOffset;
                        }
                    }
                    CheckNodeIntersectLink(DragPosition);
                    if(CheckMouseShake())
                    {
                        if(SelectedNodes.Count == 1)
                        {
                            var node = SelectedNodes[0].Node;
                            var inLinkers = new List<UPinLinker>(node.Inputs.Count);
                            for (int pI = 0; pI < node.Inputs.Count; pI++)
                            {
                                var inPin = node.Inputs[pI];
                                for (int i = 0; i < Linkers.Count; i++)
                                {
                                    if (Linkers[i].InPin == inPin)
                                    {
                                        inLinkers.Add(Linkers[i]);
                                    }
                                }
                            }
                            var outLinkers = new List<UPinLinker>(node.Outputs.Count);
                            for(int pI = 0; pI < node.Outputs.Count; pI++)
                            {
                                var outPin = node.Outputs[pI];
                                for(int i=0; i<Linkers.Count; i++)
                                {
                                    if(Linkers[i].OutPin == outPin)
                                    {
                                        outLinkers.Add(Linkers[i]);
                                    }
                                }
                            }
                            unsafe
                            {
                                var used = stackalloc bool[outLinkers.Count];
                                for (int i = 0; i < inLinkers.Count; i++)
                                {
                                    for (int j = 0; j < outLinkers.Count; j++)
                                    {
                                        if (used[j])
                                            continue;

                                        if (CanLink(inLinkers[i].OutPin, outLinkers[j].InPin))
                                        {
                                            used[j] = true;
                                            AddLink(inLinkers[i].OutPin, outLinkers[j].InPin, true);
                                            break;
                                        }
                                    }
                                    inLinkers[i].InPin?.HostNode.OnRemoveLinker(inLinkers[i]);
                                    inLinkers[i].OutPin?.HostNode.OnRemoveLinker(inLinkers[i]);
                                    inLinkers[i].InPin = null;
                                    inLinkers[i].OutPin = null;
                                    Linkers.Remove(inLinkers[i]);
                                }
                                for(int i=0; i<outLinkers.Count; i++)
                                {
                                    outLinkers[i].InPin?.HostNode.OnRemoveLinker(outLinkers[i]);
                                    outLinkers[i].OutPin?.HostNode.OnRemoveLinker(outLinkers[i]);
                                    outLinkers[i].InPin = null;
                                    outLinkers[i].OutPin = null;
                                    Linkers.Remove(outLinkers[i]);
                                }
                            }
                        }
                        else
                        {
                            foreach(var node in SelectedNodes)
                            {
                                foreach (var iPin in node.Node.Inputs)
                                {
                                    RemoveLinkedIn(iPin);
                                }
                                foreach (var oPin in node.Node.Outputs)
                                {
                                    RemoveLinkedOut(oPin);
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (LinkingOp.StartPin != null && LinkingOp.IsBlocking == false)
                    {
                        LinkingOp.BlockingEnd = DragPosition;
                    }
                }
            }
            else if (ButtonPress[(int)EMouseButton.Middle])
            {
                Vector2 t;
                t.X = screenPos.X * SizeVP.X;
                t.Y = screenPos.Y * SizeVP.Y;
                var NewPositionVP = PressPosition - t;
                PositionVP = NewPositionVP;
            }
        }

        public UPinLinker PreOrderLinker;
        public PinIn PreOrderPinIn;
        public PinOut PreOrderPinOut;
        void CheckNodeIntersectLink(in Vector2 dragPosition)
        {
            PreOrderLinker = null;
            PreOrderPinIn = null;
            PreOrderPinOut = null;
            if (SelectedNodes.Count != 1)
                return;
            var selNode = SelectedNodes[0].Node;
            float minDis = 20.0f;
            for (int i = 0; i < Linkers.Count; i++)
            {
                var linker = Linkers[i];
                var pointA = linker.InPin.HotPosition + linker.InPin.HotSize * 0.5f;
                var pointB = linker.OutPin.HotPosition + linker.OutPin.HotSize * 0.5f;
                var dis = PointF.DistanceToLine(dragPosition.X, dragPosition.Y, pointA.X, pointA.Y, pointB.X, pointB.Y);
                if(dis < minDis)
                {
                    for(int pinIdx = 0; pinIdx < selNode.Inputs.Count; pinIdx++)
                    {
                        var pin = selNode.Inputs[pinIdx];
                        if (linker.InPin == pin)
                            continue;
                        if(CanLink(linker.OutPin, pin))
                        {
                            PreOrderLinker = linker;
                            PreOrderPinIn = pin;
                            minDis = dis;
                            break;
                        }
                    }
                    for(int pinIdx = 0; pinIdx < selNode.Outputs.Count; pinIdx++)
                    {
                        var pin = selNode.Outputs[pinIdx];
                        if (linker.OutPin == pin)
                            continue;
                        if(CanLink(pin, linker.InPin))
                        {
                            PreOrderLinker = linker;
                            PreOrderPinOut = pin;
                            minDis = dis;
                            break;
                        }
                    }
                }
            }
        }
        public float WheelSpeed { get; set; } = 0.1f;
        public void Zoom(in Vector2 screenPos, float delta)
        {
            if (delta == 0)
            {
                IsZooming = false;
                return;
            }
            if (IsZooming == false)
            {
                ZoomCenter = ViewportRateToCanvas(in screenPos);
            }
            IsZooming = true;

            var scale = ScaleVP + delta * WheelSpeed;
            SetScaleVP(scale);

            //保证鼠标压着的缩放中心在缩放后，依然保持在视口中的位置，建立方程，反解NewPositionVP
            //(ZoomCenter - NewPositionVP) / SizeVP = screenPos;
            Vector2 t;
            t.X = screenPos.X * SizeVP.X;
            t.Y = screenPos.Y * SizeVP.Y;
            var NewPositionVP = ZoomCenter - t;

            PositionVP = NewPositionVP;
        }
        public void LeftDoubleClicked(in Vector2 screenPos)
        {
            PressPosition = ViewportRateToCanvas(in screenPos);
            var hit = HitObject(PressPosition.X, PressPosition.Y);
            if (hit == null)
            {
                return;
            }

            var pKls = hit.GetType();
            if (Rtti.UTypeDesc.CanCast(pKls, typeof(UNodeBase)))
            {
                var node = hit as UNodeBase;
                node.OnDoubleClick();
            }
            else if(Rtti.UTypeDesc.CanCast(pKls, typeof(NodePin)))
            {
                var pin = hit as NodePin;
                if(pin != null)
                    pin.HostNode.OnDoubleClickedPin(pin);
            }
        }
        public void RightDoubleClicked(in Vector2 screenPos)
        {

        }
        public void MiddleDoubleClicked(in Vector2 screenPos)
        {

        }
        #endregion

        #region override
        public virtual void OnDrawAfter(Bricks.NodeGraph.UGraphRenderer renderer, UNodeGraphStyles styles, ImDrawList cmdlist)
        {

        }
        public virtual unsafe void OnBeforeDrawMenu(UNodeGraphStyles styles)
        {

        }
        public virtual unsafe void OnAfterDrawMenu(UNodeGraphStyles styles)
        {

        }
        public virtual bool OnLinkingUp(ULinkingLine linking, UNodeBase pressNode)
        {
            return true;
        }

        public virtual void OnLButtonClicked()
        {

        }
        #endregion

        uint CurSerialId = uint.MaxValue;
        public uint GenSerialId()
        {
            if (CurSerialId == uint.MaxValue)
            {
                CurSerialId = 0;
                for (int i = 0; i < Nodes.Count; i++)
                {
                    var atts = Nodes[i].GetType().GetCustomAttributes(typeof(CodeBuilder.ContextMenuAttribute), false);
                    if (atts == null || atts.Length <= 0)
                        continue;

                    var att = atts[0] as CodeBuilder.ContextMenuAttribute;
                    if (att.MenuPaths.Length <= 0)
                        continue;

                    var menuString = att.MenuPaths[att.MenuPaths.Length - 1];
                    var idx = menuString.IndexOf('@');
                    if (idx >= 0)
                    {
                        var idxEnd = menuString.IndexOf('@', idx + 1);
                        var subStr = menuString.Substring(idx + 1, idxEnd - idx - 1);
                        subStr = subStr.Replace("serial", "");
                        var tempStr = menuString.Remove(idx, idxEnd - idx + 1);
                        tempStr = tempStr.Insert(idx, subStr);
                        tempStr = Nodes[i].Name.Replace(tempStr, "");
                        try
                        {
                            var id = System.Convert.ToUInt32(tempStr);
                            if (id > CurSerialId)
                                CurSerialId = id;
                        }
                        catch (System.Exception)
                        {

                        }
                    }
                }
            }
            return ++CurSerialId;
        }
        protected static string GetMenuName(in string menuString)
        {
            var idx = menuString.IndexOf('@');
            if (idx >= 0)
            {
                var idxEnd = menuString.IndexOf('@', idx + 1);
                return menuString.Remove(idx, idxEnd - idx + 1);
            }
            return menuString;
        }
        protected static string GetSerialFinalString(in string menuString, uint serialIdx)
        {
            var idx = menuString.IndexOf('@');
            if (idx >= 0)
            {
                var idxEnd = menuString.IndexOf('@', idx + 1);
                var subStr = menuString.Substring(idx + 1, idxEnd - idx - 1);
                subStr = subStr.Replace("serial", serialIdx.ToString());
                var menuName = menuString.Remove(idx, idxEnd - idx + 1);
                return menuName.Insert(idx, subStr);
            }
            return null;
        }
    }
}
