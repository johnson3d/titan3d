using System;
using System.Collections.Generic;
using System.Text;
using EngineNS;

namespace EngineNS.EGui.Controls.NodeGraph
{
    public class NodeGraphStyles
    {
        public static NodeGraphStyles DefaultStyles = new NodeGraphStyles();
        public NodeGraphStyles()
        {
            PinInStyle.Image.Size = new Vector2(20, 20);
            PinOutStyle.Image.Size = new Vector2(20, 20);
        }
        public enum EFlowMode
        {
            Horizon,
            Vertical,
        }
        public EFlowMode FlowMode { get; set; } = EFlowMode.Horizon;
        public class PinStyle
        {
            public UVAnim Image { get; set; } = new UVAnim();
            public float Offset { get; set; } = 3;
        }
        public PinStyle PinInStyle { get; set; } = new PinStyle();
        public PinStyle PinOutStyle { get; set; } = new PinStyle();
        public float PinSpacing { get; set; } = 10;
        public float PinPadding { get; set; } = 3;
        public Vector2 IconOffset { get; set; } = new Vector2(2, 2);
        public float MinSpaceInOut { get; set; } = 5.0f;
        public UInt32 TitleTextColor { get; set; } = 0xFFFFFFFF;
        public UInt32 PinTextColor { get; set; } = 0xFFFFFFFF;
        public UInt32 SelectedColor { get; set; } = 0xFFFF00FF;
        public UInt32 LinkerColor { get; set; } = 0xFF00FFFF;
        public float LinkerThinkness { get; set; } = 3;
        public UInt32 HighLightColor { get; set; } = 0xFF0000FF;
        public float BezierPixelPerSegement { get; set; } = 10.0f;
    }
    public class NodeGraph : IO.ISerializer
    {
        public virtual void OnPreRead(object tagObject, object hostObject, bool fromXml) { }
        public virtual void OnPropertyRead(object root, System.Reflection.PropertyInfo prop, bool fromXml) { }
        public void ResetGraph()
        {
            Nodes.Clear();
            Linkers.Clear();
            this.SelectedNodes.Clear();
            this.ScaleFactor = 1.0f;
            this.PostExecutes.Clear();
            this.mMenuType = EMenuType.None;
            this.mMenuShowNode = null;
            this.mMenuShowPin = null;
            this.LinkingOp.HoverPin = null;
            this.LinkingOp.StartPin = null;
            this.LinkingOp.IsBlocking = false;
        }
        [Rtti.Meta]
        public string GraphName { get; set; } = "NodeGraph";
        [Rtti.Meta]
        public List<NodeBase> Nodes
        {
            get;
        } = new List<NodeBase>();
        public NodeBase FindNode(Guid id)
        {
            foreach (var i in Nodes)
            {
                if (i.NodeId == id)
                    return i;
            }
            return null;
        }
        [Rtti.Meta(Order = 1)]
        public List<PinLinker> Linkers
        {
            get;
        } = new List<PinLinker>();
        [Rtti.Meta(Order = 100)]
        public bool ReadSignal
        {
            get => true;
        }
        protected LinkingLine LinkingOp = new LinkingLine();
        public void AddLink(NodeBase OutNode, string OutPin, NodeBase InNode, string InPin, bool bCallLinked = true)
        {
            PinOut oPin = null;
            if (OutNode != null)
            {
                oPin = OutNode.FindPinOut(OutPin);
            }
            PinIn iPin = null;
            if(InNode!=null)
            {
                iPin = InNode.FindPinIn(InPin);
            }
            var result = new PinLinker();
            if (iPin != null)
            {
                result.InPin.NodeId = InNode.NodeId;
                result.InPin.NodePin = iPin;
            }
            if (oPin != null)
            {
                result.OutPin.NodeId = OutNode.NodeId;
                result.OutPin.NodePin = oPin;
            }
            foreach(var i in Linkers)
            {
                if (i.InPin.NodePin == result.InPin.NodePin &&
                    i.OutPin.NodePin == result.OutPin.NodePin)
                    return;
            }
            if (OutNode.CanLinkTo(result.Out, InNode, result.In) && InNode.CanLinkFrom(result.In, OutNode, result.Out))
            {
                Linkers.Add(result);
                if (bCallLinked)
                {
                    OutNode.OnLinkedTo(result.Out, InNode, result.In);
                    InNode.OnLinkedFrom(result.In, OutNode, result.Out);
                }
            }
        }
        public void RemoveLink(PinOut oPin, PinIn iPin )
        {
            for (int i = 0; i < Linkers.Count; i++)
            {
                if (Linkers[i].InPin.NodePin == iPin &&
                    Linkers[i].OutPin.NodePin == oPin)
                {
                    var saved = Linkers[i];
                    _RemoveLinker(i);
                    saved.InPin.NodePin = null;
                    saved.OutPin.NodePin = null;
                    return;
                }
            }
        }
        private void _RemoveLinker(int index)
        {
            var linker = Linkers[index];
            linker.In.HostNode.OnRemoveLinker(linker);
            linker.Out.HostNode.OnRemoveLinker(linker);
            Linkers.RemoveAt(index);
        }
        public void RemoveLinkedIn(PinIn pin)
        {
            for (int i = 0; i < Linkers.Count; i++)
            {
                if (Linkers[i].InPin.NodePin == pin)
                {
                    var saved = Linkers[i];
                    _RemoveLinker(i);
                    i--;
                    saved.InPin.NodePin = null;
                    saved.OutPin.NodePin = null;                    
                }
            }
        }
        public void RemoveLinkedInExcept(PinIn pin, NodeBase OutNode, string OutPin)
        {
            for (int i = 0; i < Linkers.Count; i++)
            {
                if (Linkers[i].InPin.NodePin == pin)
                {
                    if (Linkers[i].Out.HostNode == OutNode && Linkers[i].Out.Name == OutPin)
                        continue;
                    var saved = Linkers[i];
                    _RemoveLinker(i);
                    i--;
                    saved.InPin.NodePin = null;
                    saved.OutPin.NodePin = null;
                }
            }
        }
        public void RemoveLinkedOut(PinOut pin)
        {
            for (int i = 0; i < Linkers.Count; i++)
            {
                if (Linkers[i].OutPin.NodePin == pin)
                {
                    var saved = Linkers[i];
                    _RemoveLinker(i);
                    i--;
                    saved.InPin.NodePin = null;
                    saved.OutPin.NodePin = null;
                }
            }
        }
        public void RemoveLinkedOutExcept(PinOut pin, NodeBase InNode, string InPin)
        {
            for (int i = 0; i < Linkers.Count; i++)
            {
                if (Linkers[i].OutPin.NodePin == pin)
                {
                    if (Linkers[i].In.HostNode == InNode && Linkers[i].In.Name == InPin)
                        continue;

                    var saved = Linkers[i];
                    _RemoveLinker(i);
                    i--;
                    saved.InPin.NodePin = null;
                    saved.OutPin.NodePin = null;
                }
            }
        }
        public void FindOutLinker(PinOut pin, List<PinLinker> linkers)
        {
            linkers.Clear();
            foreach (var i in Linkers)
            {
                if (i.OutPin.NodePin == pin)
                {
                    linkers.Add(i);
                }
            }
        }
        public bool PinHasLinker(NodePin pin)
        {
            foreach (var i in Linkers)
            {
                if (i.InPin.NodePin == pin || i.OutPin.NodePin == pin)
                {
                    return true;
                }
            }
            return false;
        }
        public PinLinker FindInLinkerSingle(PinIn pin)
        {
            foreach (var i in Linkers)
            {
                if (i.InPin.NodePin == pin)
                {
                    return i;
                }
            }
            return null;
        }
        public void FindInLinker(PinIn pin, List<PinLinker> linkers)
        {
            linkers.Clear();
            foreach (var i in Linkers)
            {
                if (i.InPin.NodePin == pin)
                {
                    linkers.Add(i);
                }
            }
        }
        public void AddNode(NodeBase node)
        {
            PostExecutes.Add(() =>
            {
                AddNode_Impl(node);
            });
        }
        public void AddNode_Impl(NodeBase node)
        {
            node.ParentGraph = this;
            if (Nodes.Contains(node) == false)
            {
                Nodes.Add(node);
            }
        }
        public void RemoveNode(NodeBase node)
        {
            PostExecutes.Add(() =>
            {
                RemoveNode_Impl(node);
            });
        }
        private void RemoveNode_Impl(NodeBase node)
        {
            foreach(var i in node.Inputs)
            {
                RemoveLinkedIn(i);
            }
            foreach (var i in node.Outputs)
            {
                RemoveLinkedOut(i);
            }
            Nodes.Remove(node);
            node.ParentGraph = null;
        }
        public List<NodeBase> SelectedNodes
        {
            get;
        } = new List<NodeBase>();
        public void SelecteNode(NodeBase node)
        {
            node.Selected = true;
            if (SelectedNodes.Contains(node))
                return;
            SelectedNodes.Add(node);
        }
        public void UnSelecteNode(NodeBase node)
        {
            node.Selected = false;
            if (SelectedNodes.Contains(node)==false)
                return;            
            SelectedNodes.Remove(node);
        }
        public void UnSelectAllNodes()
        {
            foreach(var i in SelectedNodes)
            {
                i.Selected = false;
            }
            SelectedNodes.Clear();
        }
        private void UpdateSelectedDrageOffset(ref Vector2 pos)
        {
            foreach (var i in SelectedNodes)
            {
                i.OffsetOfDragPoint = i.Position - pos;
            }
        }
        private void UpdateSelectedPosition(ref Vector2 pos)
        {
            foreach (var i in SelectedNodes)
            {
                i.Position = pos + i.OffsetOfDragPoint;
            }
        }
        private bool IntersectRect(ref Vector2 r1Min, ref Vector2 r1Max, ref Vector2 r2Min, ref Vector2 r2Max)
        {
            if (r1Max.X < r2Min.X ||
                r1Max.Y < r2Min.Y ||
                r1Min.X > r2Max.X ||
                r1Min.Y > r2Max.Y)
                return false;
            return true;
        }
        private NodeBase PointInNodes(ref Vector2 pt, bool testSelected = true)
        {
            foreach (var i in Nodes)
            {
                if(testSelected == false)
                {
                    if (i.Selected)
                        continue;
                }
                var rcMin = i.Position;
                var rcMax = rcMin + i.Size;
                if (ImGuiAPI.PointInRect(ref pt, ref rcMin, ref rcMax))
                {
                    return i;
                }
            }
            return null;
        }
        private NodeBase PointInSelectNodes(ref Vector2 pt)
        {
            foreach(var i in SelectedNodes)
            {
                var rcMin = i.Position;
                var rcMax = rcMin + i.Size;
                if (ImGuiAPI.PointInRect(ref pt, ref rcMin, ref rcMax))
                {
                    return i;
                }
            }
            return null;
        }
        private void CheckNodeVisible()
        {
            //这里后面可以做一个dirty的优化
            //只有发生了node移动，创建，Viewport移动，放缩等操作后才做一次
            var max = ImGuiAPI.GetWindowContentRegionMax() + WindowPos;
            var min = ImGuiAPI.GetWindowContentRegionMin() + WindowPos;
            foreach (var i in Nodes)
            {
                var rcMin = i.DrawPosition;
                var rcMax = rcMin + i.DrawSize;
                if (IntersectRect(ref rcMin, ref rcMax, ref min, ref max ))
                {
                    i.Visible = true;
                }
                else
                {
                    i.Visible = false;
                }
            }
        }
        public List<System.Action> PostExecutes = new List<Action>();
        public virtual void OnDrawAfter(NodeGraphStyles styles = null)
        {

        }
        public Vector2 GraphViewSize;
        //private bool mMouseHoverContentWindow = false;
        public virtual void OnDraw(NodeGraphStyles styles = null, bool bNewForm = true)
        {
            if (styles == null)
            {
                styles = NodeGraphStyles.DefaultStyles;
            }
            foreach (var i in PostExecutes)
            {
                i();
            }
            PostExecutes.Clear();
            unsafe
            {
                if (bNewForm)
                {
                    bool vis = true;
                    ImGuiAPI.Begin(GraphName, &vis, ImGuiWindowFlags_.ImGuiWindowFlags_NoScrollbar);
                }

                CheckNodeVisible();
                ProcEventState(styles);
                GraphViewSize = ImGuiAPI.GetWindowContentRegionMax() - ImGuiAPI.GetWindowContentRegionMin();
                if (ImGuiAPI.BeginChild("ContentWindow", ref GraphViewSize, false, 
                    ImGuiWindowFlags_.ImGuiWindowFlags_NoMove | ImGuiWindowFlags_.ImGuiWindowFlags_NoScrollbar))
                {
                    var cmdlist = new ImDrawList(ImGuiAPI.GetWindowDrawList());
                    foreach (var i in Nodes)
                    {
                        if (i.Visible)
                            i.OnDraw(styles);
                    }
                    if (mMenuShowPin != null)
                    {
                        var min = mMenuShowPin.DrawPosition;
                        Vector2 max;
                        max = min + mMenuShowPin.GetIconSize(styles, ScaleFactor);
                        cmdlist.AddRect(ref min, ref max, styles.HighLightColor, 0, ImDrawFlags_.ImDrawFlags_RoundCornersAll, 2);
                    }
                    foreach (var i in Linkers)
                    {
                        if(i.InPin==null || i.OutPin==null ||
                            i.InNode == null || i.OutNode == null)
                        {
                            Linkers.Remove(i);
                            break;
                        }
                        i.OnDraw(this, styles);
                    }

                    if (LinkingOp.StartPin != null)
                    {
                        var mPos = ImGuiAPI.GetMousePos();
                        LinkingOp.OnDraw(this, ref mPos, styles);
                    }
                    OnDrawMenu(styles);

                    OnDrawAfter(styles);
                }
                ImGuiAPI.EndChild();

                if (bNewForm)
                    ImGuiAPI.End();
            }
        }
        public Vector2 WindowPos
        {
            get;set;
        }
        Vector2 mViewPortPosition;
        public Vector2 ViewPortPosition
        {
            get => mViewPortPosition; set => mViewPortPosition = value;
        }
        public float ScaleFactor
        {
            get;
            protected set;
        } = 1.0f;
        private Vector2 ViewPortDragPos;
        private Vector2 WorldDragPos;
        private bool IsMiddleDown = false;
        private bool IsLeftDown = false;
        private bool IsRightDown = false;
        private bool DraggingNodes = false;
        private bool DraggingViewPort = false;
        public Vector2 View2WorldSpace(ref Vector2 pos)
        {
            return View2WorldSpace(ref pos, ref mViewPortPosition);
        }
        private Vector2 View2WorldSpace(ref Vector2 pos, ref Vector2 vpPos)
        {
            return vpPos + pos / ScaleFactor;
        }
        private void ProcEventState(NodeGraphStyles styles)
        {
            WindowPos = ImGuiAPI.GetWindowPos();
            var vPos = ImGuiAPI.GetMousePos();
            var mousePosVP = vPos - WindowPos;
            var mousePos = View2WorldSpace(ref mousePosVP);
            bool IsMouseHoverGraph = ImGuiAPI.IsWindowFocused(ImGuiFocusedFlags_.ImGuiFocusedFlags_ChildWindows) &&
                !ImGuiAPI.IsAnyItemHovered();
            bool IsMouseOnClient = false;
            if (IsMouseHoverGraph)
            {
                var min = ImGuiAPI.GetWindowContentRegionMin();
                var max = ImGuiAPI.GetWindowContentRegionMax();
                if (ImGuiAPI.PointInRect(ref mousePosVP, ref min, ref max))
                {
                    IsMouseOnClient = true;
                }
            }

            if (IsMouseOnClient)
            {
                var pressNode = PointInNodes(ref mousePos);
                if (pressNode != null)
                {
                    var hoverPin = pressNode.PointInPin(ref vPos, styles);
                    if (hoverPin != null)
                    {
                        pressNode.OnMouseStayPin(hoverPin);
                    }
                }
            }
            if (IsMouseOnClient && ImGuiAPI.GetIO().MouseWheel != 0)
            {
                var center = ImGuiAPI.GetMousePos() - WindowPos;

                var OldPos = View2WorldSpace(ref center);

                ScaleFactor += ImGuiAPI.GetIO().MouseWheel * 0.05f;
                //ScaleFactor = System.Math.Clamp(ScaleFactor, 0.3f, 2.0f);
                ScaleFactor = System.Math.Max(ScaleFactor, 0.3f);
                ScaleFactor = System.Math.Min(ScaleFactor, 2.0f);

                var NewPos = View2WorldSpace(ref center);

                ViewPortPosition = ViewPortPosition - (NewPos - OldPos);
            }
            else if (ImGuiAPI.IsMouseDown(ImGuiMouseButton_.ImGuiMouseButton_Middle))
            {
                ProcMButtonDown(styles);
            }
            else if (ImGuiAPI.IsMouseDown(ImGuiMouseButton_.ImGuiMouseButton_Right))
            {
                ProcRButtonDown(styles);
            }
            else if (ImGuiAPI.IsMouseDown(ImGuiMouseButton_.ImGuiMouseButton_Left))
            {
                ProcLButtonDown(styles);
            }

            if (ImGuiAPI.IsMouseDown(ImGuiMouseButton_.ImGuiMouseButton_Middle) == false)
            {
                if (IsMiddleDown)
                {
                    ProcMButtonUp(styles);
                }
                IsMiddleDown = false;
            }
            if (ImGuiAPI.IsMouseDown(ImGuiMouseButton_.ImGuiMouseButton_Left) == false)
            {
                if (IsLeftDown)
                {
                    ProcLButtonUp(styles);
                }
                IsLeftDown = false;
            }
            if (ImGuiAPI.IsMouseDown(ImGuiMouseButton_.ImGuiMouseButton_Right)==false)
            {
                if(IsRightDown)
                {
                    ProcRButtonUp(styles);
                }
                IsRightDown = false;
            }
        }
        private void ProcMButtonDown(NodeGraphStyles styles)
        {
            var mousePosVP = ImGuiAPI.GetMousePos() - WindowPos;
            var mousePos = View2WorldSpace(ref mousePosVP);
            var pressNode = PointInNodes(ref mousePos);

            if (DraggingViewPort == false)
            {
                ViewPortDragPos = mousePos;
                WorldDragPos = ViewPortPosition;
            }
            else
            {
                var v0 = View2WorldSpace(ref mousePosVP, ref WorldDragPos);
                ViewPortPosition = WorldDragPos - (v0 - ViewPortDragPos);
            }
            DraggingViewPort = true;

            IsMiddleDown = true;
        }
        private void ProcMButtonUp(NodeGraphStyles styles)
        {
            DraggingViewPort = false;
            IsMiddleDown = false;
        }
        private void ProcLButtonDown(NodeGraphStyles styles)
        {
            var mousePosVP = ImGuiAPI.GetMousePos() - WindowPos;
            var mousePos = View2WorldSpace(ref mousePosVP);
            var pressNode = PointInNodes(ref mousePos);

            if (DraggingNodes)
            {
                UpdateSelectedPosition(ref mousePos);
            }
            if (ImGuiAPI.GetIO().KeyCtrl == false)
            {
                var vPos = ImGuiAPI.GetMousePos();
                if (IsLeftDown == false)
                {
                    if (pressNode != null)
                    {
                        var clickedPin = pressNode.PointInPin(ref vPos, styles);
                        if (clickedPin != null)
                        {
                            //UnSelectAllNodes();
                            //拉出新的线来
                            LinkingOp.StartPin = clickedPin;
                            LinkingOp.HoverPin = clickedPin;
                        }
                        else if (pressNode.Selected)
                        {
                            if (DraggingNodes == false)
                            {
                                UpdateSelectedDrageOffset(ref mousePos);
                            }
                            DraggingNodes = true;
                        }
                        else
                        {
                            UnSelectAllNodes();
                            SelecteNode(pressNode);
                            UpdateSelectedDrageOffset(ref mousePos);
                            DraggingNodes = true;
                        }
                        pressNode.OnLClicked(clickedPin);
                    }
                    else
                    {
                        this.OnLClicked();
                    }
                }
                else
                {
                    if (LinkingOp.StartPin != null && pressNode != null)
                    {
                        LinkingOp.HoverPin = pressNode.PointInPin(ref vPos, styles);
                    }
                    else
                    {
                        LinkingOp.HoverPin = null;
                    }
                    if (DraggingNodes == false)
                    {
                        UnSelectAllNodes();
                        pressNode = PointInNodes(ref mousePos, false);
                        if (pressNode != null)
                        {
                            SelecteNode(pressNode);
                        }
                    }
                }
            }
            else
            {
                if (IsLeftDown == false)
                {
                    if (pressNode != null)
                    {
                        if (pressNode.Selected)
                            UnSelecteNode(pressNode);
                        else
                            SelecteNode(pressNode);
                    }
                }
            }

            IsLeftDown = true;
        }
        private void ProcLButtonUp(NodeGraphStyles styles)
        {
            var mousePosVP = ImGuiAPI.GetMousePos() - WindowPos;
            var mousePos = View2WorldSpace(ref mousePosVP);
            var pressNode = PointInNodes(ref mousePos, true);
            if (pressNode == null)
            {
                if (DraggingNodes == false)
                {
                    UnSelectAllNodes();
                }
            }
            else
            {
                var vPos = ImGuiAPI.GetMousePos();
                var pin = pressNode.PointInPin(ref vPos, styles);
                //if (pin != null)
                //{
                //}
                if (LinkingOp.StartPin != null)
                {   
                    pressNode.PointInPin(ref vPos, styles);
                    if (pin != null)
                    {
                        if (LinkingOp.StartPin.GetType() == typeof(PinOut))
                        {
                            if (pin.GetType() == typeof(PinIn))
                            {
                                AddLink(LinkingOp.StartPin.HostNode, LinkingOp.StartPin.Name, pin.HostNode, pin.Name);
                            }
                        }
                        else if (LinkingOp.StartPin.GetType() == typeof(PinIn))
                        {
                            if (pin.GetType() == typeof(PinOut))
                            {
                                AddLink(pin.HostNode, pin.Name, LinkingOp.StartPin.HostNode, LinkingOp.StartPin.Name);
                            }
                        }
                    }
                }
            }
            if (OnLinkingUp(LinkingOp, pressNode))
            {
                LinkingOp.StartPin = null;
            }
            DraggingNodes = false;
            IsLeftDown = false;
        }
        private void ProcRButtonDown(NodeGraphStyles styles)
        {
            IsRightDown = true;
        }
        private void ProcRButtonUp(NodeGraphStyles styles)
        {
            var mouseInWindow = ImGuiAPI.GetMousePos();
            var mousePosVP = mouseInWindow - WindowPos;
            var mousePos = View2WorldSpace(ref mousePosVP);
            var pressNode = PointInNodes(ref mousePos);
            if (pressNode != null)
            {
                var pin = pressNode.PointInPin(ref mouseInWindow, styles);
                if (pin != null)
                {
                    mMenuType = EMenuType.PinMenu;
                    mMenuShowPin = pin;
                }
                else
                {
                    mMenuType = EMenuType.NodeMenu;
                    mMenuShowNode = pressNode;
                    mMenuShowPin = null;
                }
            }
            else
            {
                mMenuType = EMenuType.GraphMenu;
                mMenuShowNode = null;
                mMenuShowPin = null;
            }
            IsRightDown = true;
        }
        public enum EMenuType
        {
            None,
            PinMenu,
            NodeMenu,
            GraphMenu,
            
        }
        public EMenuType mMenuType = EMenuType.None;
        public NodePin mMenuShowPin;
        public NodeBase mMenuShowNode;
        protected virtual void OnDrawMenu(NodeGraphStyles styles)
        {
            bool isShow = false;
            switch(mMenuType)
            {
                case EMenuType.PinMenu:
                    isShow = ShowPinMenu(styles, mMenuShowPin);
                    break;
                case EMenuType.NodeMenu:
                    isShow = ShowNodeMenu(styles, mMenuShowNode);
                    break;
                case EMenuType.GraphMenu:
                    isShow = ShowGraphMenu(styles);
                    break;
                default:
                    break;
            }
            if (isShow == false)
            {
                mMenuShowPin = null;
                mMenuType = EMenuType.None;
            }
        }
        private bool ShowPinMenu(NodeGraphStyles styles, NodePin pin)
        {
            if (pin == null)
                return false;
            bool isPinIn = pin.GetType() == typeof(PinIn);
            Vector2 GraphMenuSize = new Vector2(-1, -1);
            ImGuiAPI.SetNextWindowSize(ref GraphMenuSize, ImGuiCond_.ImGuiCond_None);
            if (ImGuiAPI.BeginPopupContextWindow(null, ImGuiPopupFlags_.ImGuiPopupFlags_MouseButtonRight))
            {
                if (ImGuiAPI.BeginMenu("Delete Linkers", true))
                {
                    var itemSize = new Vector2(-1, 0);
                    if (isPinIn)
                    {
                        for (var i = 0; i < Linkers.Count; i++)
                        {
                            var cur = Linkers[i];
                            if (cur.InPin.NodePin == pin)
                            {
                                if (ImGuiAPI.MenuItem($"From {cur.OutPin.NodePin.HostNode.Name}->{cur.OutPin.Name}", null, false, true))
                                {
                                    RemoveLink(cur.Out, cur.In);
                                    mMenuType = EMenuType.None;
                                    i--;
                                }
                            }
                        }
                    }
                    else
                    {   
                        for(var i = 0; i<Linkers.Count; i++)
                        {
                            var cur = Linkers[i];
                            if (cur.OutPin.NodePin == pin)
                            {
                                if (ImGuiAPI.MenuItem($"To {cur.InPin.NodePin.HostNode.Name}->{cur.InPin.Name}", null, false, true))
                                {
                                    RemoveLink(cur.Out, cur.In);
                                    mMenuType = EMenuType.None;
                                    i--;
                                }
                            }
                        }
                    }
                    ImGuiAPI.EndMenu();
                }
                var btSize = new Vector2(-1, 0);
                if (ImGuiAPI.Button("Break All", ref btSize))
                {
                    if (isPinIn)
                    {
                        RemoveLinkedIn(pin as PinIn);
                    }
                    else
                    {
                        RemoveLinkedOut(pin as PinOut);
                    }
                    mMenuType = EMenuType.None;
                }
                if (ImGuiAPI.Button("Close Menu", ref btSize))
                {
                    mMenuType = EMenuType.None;
                }

                pin.HostNode.OnShowPinMenu(pin);

                ImGuiAPI.EndPopup();
                return true;
            }
            else
            {
                return false;
            }
        }
        private bool ShowNodeMenu(NodeGraphStyles styles, NodeBase node)
        {
            if (node == null)
                return false;
            Vector2 GraphMenuSize = new Vector2(-1, -1);
            ImGuiAPI.SetNextWindowSize(ref GraphMenuSize, ImGuiCond_.ImGuiCond_None);
            if (ImGuiAPI.BeginPopupContextWindow(null, ImGuiPopupFlags_.ImGuiPopupFlags_MouseButtonRight))
            {
                var btSize = new Vector2(-1, 0);
                if (ImGuiAPI.BeginMenu("Node Operation", true))
                {
                    if (ImGuiAPI.MenuItem($"Delete", null, false, true))
                    {
                        RemoveNode(node);
                        mMenuType = EMenuType.None;
                    }
                    ImGuiAPI.EndMenu();
                }
                if (ImGuiAPI.Button("Close Menu", ref btSize))
                {
                    mMenuType = EMenuType.None;
                }
                ImGuiAPI.EndPopup();
                return true;
            }
            else
            {
                return false;
            }
        }
        protected virtual bool ShowGraphMenu(NodeGraphStyles styles)
        {
            Vector2 GraphMenuSize = new Vector2(-1, -1);
            ImGuiAPI.SetNextWindowSize(ref GraphMenuSize, ImGuiCond_.ImGuiCond_None);
            if (ImGuiAPI.BeginPopupContextWindow(null, ImGuiPopupFlags_.ImGuiPopupFlags_MouseButtonRight))
            {
                var posMenu = ImGuiAPI.GetWindowPos();
                var btSize = new Vector2(-1, 0);
                if (ImGuiAPI.BeginMenu("Add Node", true))
                {
                    ShowAddNode(posMenu);
                    ImGuiAPI.EndMenu();
                }
                if (ImGuiAPI.Button("Close Menu", ref btSize))
                {
                    mMenuType = EMenuType.None;
                }

                OnAppendGraphMenuContent(posMenu);
                
                ImGuiAPI.EndPopup();
                return true;
            }
            else
            {
                return false;
            }
        }
        protected virtual void ShowAddNode(Vector2 posMenu)
        {
            if (ImGuiAPI.MenuItem($"Node1", null, false, true))
            {
                var TestNode1 = new EngineNS.EGui.Controls.NodeGraph.NodeBase();
                TestNode1.Icon.Size = new Vector2(25, 25);
                TestNode1.Icon.Color = 0xFF00FF00;
                TestNode1.TitleImage.Color = 0xFF204020;
                TestNode1.Background.Color = 0x80808080;
                TestNode1.Position = View2WorldSpace(ref posMenu);
                TestNode1.Name = "TestGraphNode";
                var pin_in = new EngineNS.EGui.Controls.NodeGraph.PinIn();
                pin_in.Name = "Pin0";
                pin_in.EditValue = new EngineNS.EGui.Controls.NodeGraph.EditableValue(null);
                pin_in.EditValue.ValueType = Rtti.UTypeDescGetter<int>.TypeDesc;
                pin_in.EditValue.Value = (int)3;
                pin_in.EditValue.ControlWidth = 100;
                TestNode1.AddPinIn(pin_in);
                pin_in = new EngineNS.EGui.Controls.NodeGraph.PinIn();
                pin_in.Name = "Pin1";
                TestNode1.AddPinIn(pin_in);
                pin_in = new EngineNS.EGui.Controls.NodeGraph.PinIn();
                pin_in.Name = "Pin2";
                TestNode1.AddPinIn(pin_in);

                var pin_out = new EngineNS.EGui.Controls.NodeGraph.PinOut();
                pin_out.Name = "Out0";
                TestNode1.AddPinOut(pin_out);
                pin_out = new EngineNS.EGui.Controls.NodeGraph.PinOut();
                pin_out.Name = "Out1";
                TestNode1.AddPinOut(pin_out);

                this.AddNode(TestNode1);
            }
            if (ImGuiAPI.MenuItem($"Node2", null, false, true))
            {
                var TestNode2 = new EngineNS.EGui.Controls.NodeGraph.NodeBase();
                TestNode2.Icon.Size = new Vector2(25, 25);
                TestNode2.Icon.Color = 0xFFFFFF00;
                TestNode2.TitleImage.Color = 0xFF402020;
                TestNode2.Background.Color = 0x80808080;

                TestNode2.Position = View2WorldSpace(ref posMenu);
                TestNode2.Name = "TestGraphNode2";
                var pin_in = new EngineNS.EGui.Controls.NodeGraph.PinIn();
                pin_in.Name = "Pin0";
                TestNode2.AddPinIn(pin_in);
                pin_in = new EngineNS.EGui.Controls.NodeGraph.PinIn();
                pin_in.Name = "Pin1";
                TestNode2.AddPinIn(pin_in);

                var pin_out = new EngineNS.EGui.Controls.NodeGraph.PinOut();
                pin_out.Name = "Out0";
                TestNode2.AddPinOut(pin_out);

                this.AddNode(TestNode2);
            }
        }
        protected virtual void OnAppendGraphMenuContent(Vector2 posMenu)
        {

        }
        protected virtual void OnLClicked()
        {

        }
        protected virtual bool OnLinkingUp(LinkingLine linking, NodeBase pressNode)
        {
            return true;
        }
    }
}
