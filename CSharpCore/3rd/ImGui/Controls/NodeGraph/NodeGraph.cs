using System;
using System.Collections.Generic;
using System.Text;
using EngineNS;

namespace CSharpCode.Controls.NodeGraph
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
        public float PinPadding { get; set; } = 3;
        public Vector2 IconOffset { get; set; } = new Vector2(2, 2);
        public float MinSpaceInOut { get; set; } = 5.0f;
        public UInt32 TitleTextColor { get; set; } = 0xFFFFFFFF;
        public UInt32 PinTextColor { get; set; } = 0xFFFFFFFF;
        public UInt32 SelectedColor { get; set; } = 0xFFFF00FF;
        public UInt32 LinkerColor { get; set; } = 0xFF00FFFF;
        public UInt32 HighLightColor { get; set; } = 0xFF0000FF;
        public float BezierPixelPerSegement { get; set; } = 10.0f;
    }
    public class NodeGraph
    {
        private CppBool mVisible = CppBool.FromBoolean(true);
        public bool Visible
        {
            get
            {
                return mVisible;
            }
            set
            {
                mVisible = CppBool.FromBoolean(value);
            }
        }
        public string GraphName { get; set; } = "NodeGraph";
        public List<NodeBase> Nodes
        {
            get;
        } = new List<NodeBase>();
        public NodeBase FindNode(Guid id)
        {
            foreach(var i in Nodes)
            {
                if (i.NodeId == id)
                    return i;
            }
            return null;
        }
        public List<PinLinker> Linkers
        {
            get;
        } = new List<PinLinker>();
        private LinkingLine LinkingOp = new LinkingLine();
        public void AddLink(NodeBase OutNode, string OutPin, NodeBase InNode, string InPin)
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
                result.InPin.Name = InPin;
                result.InPin.NodeId = InNode.NodeId;
                result.InPin.NodePin = iPin;
            }
            if (oPin != null)
            {
                result.OutPin.Name = OutPin;
                result.OutPin.NodeId = OutNode.NodeId;
                result.OutPin.NodePin = oPin;
            }
            foreach(var i in Linkers)
            {
                if (i.InPin.NodePin == result.InPin.NodePin &&
                    i.OutPin.NodePin == result.OutPin.NodePin)
                    return;
            }
            Linkers.Add(result);
        }
        public void RemoveLink(PinOut oPin, PinIn iPin )
        {
            for (int i = 0; i < Linkers.Count; i++)
            {
                if (Linkers[i].InPin.NodePin == iPin &&
                    Linkers[i].OutPin.NodePin == oPin)
                {
                    Linkers[i].InPin.NodePin = null;
                    Linkers[i].OutPin.NodePin = null;
                    Linkers.RemoveAt(i);
                    return;
                }
            }
        }
        public void RemoveLinkedIn(PinIn pin)
        {
            for (int i = 0; i < Linkers.Count; i++)
            {
                if (Linkers[i].InPin.NodePin == pin)
                {
                    Linkers[i].InPin.NodePin = null;
                    Linkers[i].OutPin.NodePin = null;
                    Linkers.RemoveAt(i);
                    i--;
                }
            }
        }
        public void RemoveLinkedOut(PinOut pin)
        {
            for (int i = 0; i < Linkers.Count; i++)
            {
                if (Linkers[i].OutPin.NodePin == pin)
                {
                    Linkers[i].InPin.NodePin = null;
                    Linkers[i].OutPin.NodePin = null;
                    Linkers.RemoveAt(i);
                    i--;
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
            Nodes.Add(node);
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
        List<System.Action> PostExecutes = new List<Action>();
        public virtual void OnDraw(NodeGraphStyles styles = null)
        {
            if (Visible == false)
                return;
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
                fixed(CppBool* pVisible = &mVisible)
                {
                    if (ImGuiAPI.Begin(GraphName, (CppBool*)0/*pVisible*/, ImGuiWindowFlags_.ImGuiWindowFlags_NoScrollbar))
                    {
                        CheckNodeVisible();
                        ProcEventState(styles);
                        var clientSize = ImGuiAPI.GetWindowContentRegionMax() - ImGuiAPI.GetWindowContentRegionMin();
                        if (ImGuiAPI.BeginChild("ContentWindow", ref clientSize, CppBool.FromBoolean(false), ImGuiWindowFlags_.ImGuiWindowFlags_NoMove))
                        {
                            var cmdlist = new ImDrawList_PtrType(ImGuiAPI.GetWindowDrawList().NativePointer);
                            foreach (var i in Nodes)
                            {
                                if (i.Visible)
                                    i.OnDraw(styles);
                            }
                            if (mMenuShowPin != null)
                            {
                                var min = mMenuShowPin.DrawPosition;
                                Vector2 max;
                                if (mMenuShowPin.GetType() == typeof(PinIn))
                                    max = min + styles.PinInStyle.Image.Size * ScaleFactor;
                                else
                                    max = min + styles.PinOutStyle.Image.Size * ScaleFactor;
                                cmdlist.AddRect(ref min, ref max, styles.HighLightColor, 0, ImDrawCornerFlags_.ImDrawCornerFlags_All, 2);
                            }
                            foreach (var i in Linkers)
                            {
                                i.OnDraw(this, styles);
                            }

                            if (LinkingOp.StartPin != null)
                            {
                                var mPos = ImGuiAPI.GetMousePos();
                                LinkingOp.OnDraw(this, ref mPos, styles);
                            }
                            OnDrawMenu(styles);
                        }
                        ImGuiAPI.EndChild();
                    }

                    ImGuiAPI.End();
                }
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
            var mousePosVP = ImGuiAPI.GetMousePos() - WindowPos;
            var mousePos = View2WorldSpace(ref mousePosVP);
            if (ImGuiAPI.IsWindowFocused(ImGuiFocusedFlags_.ImGuiFocusedFlags_ChildWindows) && ImGuiAPI.IO.MouseWheel != 0)
            {
                var min = ImGuiAPI.GetWindowContentRegionMin();
                var max = ImGuiAPI.GetWindowContentRegionMax();
                if (ImGuiAPI.PointInRect(ref mousePosVP, ref min, ref max))
                {
                    var center = ImGuiAPI.GetMousePos() - WindowPos;

                    var OldPos = View2WorldSpace(ref center);

                    ScaleFactor += ImGuiAPI.IO.MouseWheel * 0.05f;
                    //ScaleFactor = System.Math.Clamp(ScaleFactor, 0.3f, 2.0f);
                    ScaleFactor = System.Math.Max(ScaleFactor, 0.3f);
                    ScaleFactor = System.Math.Min(ScaleFactor, 2.0f);

                    var NewPos = View2WorldSpace(ref center);

                    ViewPortPosition = ViewPortPosition - (NewPos - OldPos);
                }
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
            if (ImGuiAPI.IO.KeyCtrl == false)
            {
                var vPos = ImGuiAPI.GetMousePos();
                if (IsLeftDown == false && pressNode != null)
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
                if (LinkingOp.StartPin != null)
                {
                    var vPos = ImGuiAPI.GetMousePos();
                    var pin = pressNode.PointInPin(ref vPos, styles);
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
            LinkingOp.StartPin = null;
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
        enum EMenuType
        {
            None,
            PinMenu,
            NodeMenu,
            GraphMenu,
        }
        EMenuType mMenuType = EMenuType.None;
        NodePin mMenuShowPin;
        NodeBase mMenuShowNode;
        private void OnDrawMenu(NodeGraphStyles styles)
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
            if (ImGuiAPI.BeginPopupContextWindow(null, ImGuiPopupFlags_.ImGuiPopupFlags_MouseButtonRight))
            {
                if (ImGuiAPI.BeginMenu("Delete Linkers", CppBool.FromBoolean(true)))
                {
                    var itemSize = new Vector2(-1, 0);
                    if (isPinIn)
                    {
                        for (var i = 0; i < Linkers.Count; i++)
                        {
                            var cur = Linkers[i];
                            if (cur.InPin.NodePin == pin)
                            {
                                if (ImGuiAPI.MenuItem($"From {cur.OutPin.NodePin.HostNode.Name}->{cur.OutPin.Name}", null, CppBool.FromBoolean(false), CppBool.FromBoolean(true)))
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
                                if (ImGuiAPI.MenuItem($"To {cur.InPin.NodePin.HostNode.Name}->{cur.InPin.Name}", null, CppBool.FromBoolean(false), CppBool.FromBoolean(true)))
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
            if (ImGuiAPI.BeginPopupContextWindow(null, ImGuiPopupFlags_.ImGuiPopupFlags_MouseButtonRight))
            {
                var btSize = new Vector2(-1, 0);
                if (ImGuiAPI.Button("Delete", ref btSize))
                {
                    RemoveNode(node);
                    mMenuType = EMenuType.None;
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
        private bool ShowGraphMenu(NodeGraphStyles styles)
        {
            if (ImGuiAPI.BeginPopupContextWindow(null, ImGuiPopupFlags_.ImGuiPopupFlags_MouseButtonRight))
            {
                var posMenu = ImGuiAPI.GetWindowPos();
                var btSize = new Vector2(-1, 0);
                if (ImGuiAPI.BeginMenu("Add Node", CppBool.FromBoolean(true)))
                {
                    if (ImGuiAPI.MenuItem($"Node1", null, CppBool.FromBoolean(false), CppBool.FromBoolean(true)))
                    {
                        var TestNode1 = new CSharpCode.Controls.NodeGraph.NodeBase();
                        TestNode1.Icon.Size = new Vector2(25, 25);
                        TestNode1.Icon.Color = 0xFF00FF00;
                        TestNode1.TitleImage.Color = 0xFF204020;
                        TestNode1.Background.Color = 0x80808080;
                        TestNode1.Position = View2WorldSpace(ref posMenu);
                        TestNode1.Name = "TestGraphNode";
                        var pin_in = new CSharpCode.Controls.NodeGraph.PinIn();
                        pin_in.Name = "Pin0";
                        pin_in.EditValue = new CSharpCode.Controls.NodeGraph.EditableValue();
                        pin_in.EditValue.ValueType = typeof(int);
                        pin_in.EditValue.Value = (int)3;
                        pin_in.EditValue.ControlWidth = 100;
                        TestNode1.AddPinIn(pin_in);
                        pin_in = new CSharpCode.Controls.NodeGraph.PinIn();
                        pin_in.Name = "Pin1";
                        TestNode1.AddPinIn(pin_in);
                        pin_in = new CSharpCode.Controls.NodeGraph.PinIn();
                        pin_in.Name = "Pin2";
                        TestNode1.AddPinIn(pin_in);

                        var pin_out = new CSharpCode.Controls.NodeGraph.PinOut();
                        pin_out.Name = "Out0";
                        TestNode1.AddPinOut(pin_out);
                        pin_out = new CSharpCode.Controls.NodeGraph.PinOut();
                        pin_out.Name = "Out1";
                        TestNode1.AddPinOut(pin_out);

                        this.AddNode(TestNode1);
                    }
                    if (ImGuiAPI.MenuItem($"Node2", null, CppBool.FromBoolean(false), CppBool.FromBoolean(true)))
                    {
                        var TestNode2 = new CSharpCode.Controls.NodeGraph.NodeBase();
                        TestNode2.Icon.Size = new Vector2(25, 25);
                        TestNode2.Icon.Color = 0xFFFFFF00;
                        TestNode2.TitleImage.Color = 0xFF402020;
                        TestNode2.Background.Color = 0x80808080;
                        
                        TestNode2.Position = View2WorldSpace(ref posMenu);
                        TestNode2.Name = "TestGraphNode2";
                        var pin_in = new CSharpCode.Controls.NodeGraph.PinIn();
                        pin_in.Name = "Pin0";
                        TestNode2.AddPinIn(pin_in);
                        pin_in = new CSharpCode.Controls.NodeGraph.PinIn();
                        pin_in.Name = "Pin1";
                        TestNode2.AddPinIn(pin_in);

                        var pin_out = new CSharpCode.Controls.NodeGraph.PinOut();
                        pin_out.Name = "Out0";
                        TestNode2.AddPinOut(pin_out);

                        this.AddNode(TestNode2);
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
    }
}
