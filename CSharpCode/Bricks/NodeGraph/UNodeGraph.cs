using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.NodeGraph
{
    public class UNodeGraph : IO.ISerializer
    {
        public virtual void OnPreRead(object tagObject, object hostObject, bool fromXml) { }
        public virtual void OnPropertyRead(object root, System.Reflection.PropertyInfo prop, bool fromXml) { }
        public virtual void Initialize()
        {
            UpdateCanvasMenus();
            UpdateNodeMenus();
            UpdatePinMenus();
        }
        [Rtti.Meta]
        public string GraphName { get; set; } = "NodeGraph";
        [Rtti.Meta]
        public List<UNodeBase> Nodes { get; } = new List<UNodeBase>();
        [Rtti.Meta(Order = 1)]
        public List<UPinLinker> Linkers { get; } = new List<UPinLinker>();
        
        public ULinkingLine LinkingOp = new ULinkingLine();
        public class FSelNodeState
        {
            public UNodeBase Node;
            public Vector2 MoveOffset;
        }
        public List<FSelNodeState> SelectedNodes { get; } = new List<FSelNodeState>();

        public Dictionary<UNodeBase, UNodeGraph> SubGraphs;
        public UNodeGraph ParentGraph;

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
        }
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
        public void AddLink(PinOut oPin, PinIn iPin, bool bCallLinked)
        {
            foreach (var i in Linkers)
            {
                if (i.InPin == iPin && i.OutPin == oPin)
                    return;
            }
            if (oPin.HostNode.CanLinkTo(oPin, iPin.HostNode, iPin) &&
                    iPin.HostNode.CanLinkFrom(iPin, oPin.HostNode, oPin))
            {
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
        }
        public void RemoveSelected(UNodeBase node)
        {
            node.Selected = false;
            for (int i = 0; i < SelectedNodes.Count; i++)
            {
                if (SelectedNodes[i].Node == node)
                {
                    SelectedNodes.RemoveAt(i);
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
        protected Vector2 PressPosition;
        protected Vector2 DragPosition;

        protected Vector2 PositionVP;
        protected Vector2 SizeVP;
        //Vector2 MoveVPOffset;

        public Vector2 PhysicalSizeVP;
        public float ScaleVP = 1.0f;

        protected bool IsZooming;
        protected Vector2 ZoomCenter;
        protected bool IsMovingSelNodes;

        public enum EGraphMenu
        {
            None,
            Canvas,
            Node,
            Pin,
        };

        public EGraphMenu CurMenuType;
        public object PopMenuPressObject;
        public Vector2 PopMenuPosition;
        public UMenuItem CanvasMenus = new UMenuItem();
        public UMenuItem NodeMenus = new UMenuItem();
        public UMenuItem PinMenus = new UMenuItem();

        public virtual void UpdateCanvasMenus()
        {
            CanvasMenus.SubMenuItems.Clear();
            CanvasMenus.Text = "Canvas";
        }
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
                    }
                });
        }

        public virtual bool IsKeydown(EKey key)
        {
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
            else if (ScaleVP < 0.3f)
            {
                ScaleVP = 0.5f;
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

        public void LeftPress(in Vector2 screenPos)
        {
            ButtonPress[(int)EMouseButton.Left] = true;
            PressPosition = ViewportRateToCanvas(in screenPos);

            var hit = HitObject(PressPosition.X, PressPosition.Y);
            if (hit == null)
            {
                return;
            }

            if (SelectedNodes.Count == 0)
            {
                var pKls = hit.GetType();
                if (Rtti.UTypeDesc.CanCast(pKls, typeof(UNodeBase)))
                {
                    var node = hit as UNodeBase;
                    if (node.IsHitTitle(PressPosition.X, PressPosition.Y))
                    {
                        AddSelected(node);
                        IsMovingSelNodes = true;
                        foreach (var i in SelectedNodes)
                        {
                            i.MoveOffset = PressPosition - i.Node.Position;
                        }
                    }
                }
                else if (Rtti.UTypeDesc.CanCast(pKls, typeof(NodePin)))
                {
                    LinkingOp.StartPin = hit as NodePin;
                    LinkingOp.HoverPin = hit as NodePin;
                    LinkingOp.BlockingEnd = PressPosition;
                }
            }
            else
            {
                var pKls = hit.GetType();
                if (Rtti.UTypeDesc.CanCast(pKls, typeof(UNodeBase)))
                {
                    var node = hit as UNodeBase;
                    if (node.Selected && node.IsHitTitle(PressPosition.X, PressPosition.Y))
                    {
                        IsMovingSelNodes = true;
                        foreach (var i in SelectedNodes)
                        {
                            i.MoveOffset = PressPosition - i.Node.Position;
                        }
                    }
                }
            }
        }

        public void LeftRelease(in Vector2 screenPos)
        {
            DragPosition = ViewportRateToCanvas(in screenPos);
            //if (ButtonPress[EMouseButton::Left])
            UNodeBase pressNode = null;
            if (CurMenuType == EGraphMenu.None)
            {
                if (this.IsKeydown(EKey.Ctl))
                {
                    List<UNodeBase> sels = new List<UNodeBase>();
                    GetSelectNodes(PressPosition, DragPosition, sels);
                    foreach (var i in sels)
                    {
                        AddSelected(i);
                    }
                }
                else if (this.IsKeydown(EKey.Shift))
                {
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
                    foreach (var i in sels)
                    {
                        RemoveSelected(i);
                    }
                }
                else
                {
                    if (!IsMovingSelNodes)
                    {
                        if (LinkingOp.StartPin != null)
                        {
                            var hit = HitObject(DragPosition.X, DragPosition.Y);
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
                                else
                                {
                                    pressNode = hit as UNodeBase;
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
                            foreach (var i in sels)
                            {
                                AddSelected(i);
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
                        }
                        else
                        {
                            var hitNode = hit as UNodeBase;
                            if (hitNode != null)
                            {
                                hitNode.OnLButtonClicked(null);
                            }
                            else
                                OnLButtonClicked();
                        }
                    }
                }
            }
            ButtonPress[(int)EMouseButton.Left] = false;
            IsMovingSelNodes = false;
            if (OnLinkingUp(LinkingOp, pressNode))
            {
                LinkingOp.StartPin = null;
            }
            //LinkingOp.StartPin = null;
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
                if (pKls == typeof(PinIn ) || pKls == typeof(PinOut))
                {
                    CurMenuType = EGraphMenu.Pin;
                }
                else if (Rtti.UTypeDesc.CanCast(pKls, typeof(UNodeBase)))
                {
                    CurMenuType = EGraphMenu.Node;
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
        public void PressDrag(in Vector2 screenPos)
        {
            DragPosition = ViewportRateToCanvas(in screenPos);
            var hit = HitObject(DragPosition.X, DragPosition.Y);
            if (hit != null && Rtti.UTypeDesc.CanCast(hit.GetType(), typeof(NodePin)))
            {
                LinkingOp.HoverPin = hit as NodePin;
                if (LinkingOp.HoverPin != null)
                {
                    LinkingOp.HoverPin.HostNode.OnMouseStayPin(LinkingOp.HoverPin);
                }
            }

            if (ButtonPress[(int)EMouseButton.Left] == false &&
                ButtonPress[(int)EMouseButton.Right] == false &&
                ButtonPress[(int)EMouseButton.Middle] == false)
            {
                return;
            }
            else if (ButtonPress[(int)EMouseButton.Left])
            {
                if (IsMovingSelNodes)
                {
                    foreach (var i in SelectedNodes)
                    {
                        i.Node.Position = DragPosition - i.MoveOffset;
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
        }
        public void RightDoubleClicked(in Vector2 screenPos)
        {

        }
        public void MiddleDoubleClicked(in Vector2 screenPos)
        {

        }
        #endregion

        #region override
        public virtual void OnDrawAfter(Bricks.NodeGraph.UGraphRenderer renderer, EGui.Controls.NodeGraph.NodeGraphStyles styles, ImDrawList cmdlist)
        {

        }
        public virtual unsafe void OnBeforeDrawMenu(EngineNS.EGui.Controls.NodeGraph.NodeGraphStyles styles)
        {

        }
        public virtual unsafe void OnAfterDrawMenu(EngineNS.EGui.Controls.NodeGraph.NodeGraphStyles styles)
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
    }
}
