using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.NodeGraph
{
    public class UGraphRenderer : Graphics.Pipeline.IGuiModule
    {
        public UGraphRenderer()
        {
            var styles = UNodeGraphStyles.DefaultStyles;

            if (UEngine.Instance.UIManager[styles.NodeBodyImg] == null)
                UEngine.Instance.UIManager[styles.NodeBodyImg] = new EGui.UIProxy.BoxImageProxy(RName.GetRName(styles.NodeBodyImg, RName.ERNameType.Engine), new Thickness(16.0f / 64.0f, 25.0f / 64.0f, 16.0f / 64.0f, 16.0f / 64.0f));
            if (UEngine.Instance.UIManager[styles.NodeTitleImg] == null)
                UEngine.Instance.UIManager[styles.NodeTitleImg] = new EGui.UIProxy.BoxImageProxy(RName.GetRName(styles.NodeTitleImg, RName.ERNameType.Engine), new Thickness(12.0f / 64.0f));
            if (UEngine.Instance.UIManager[styles.NodeColorSpillImg] == null)
                UEngine.Instance.UIManager[styles.NodeColorSpillImg] = new EGui.UIProxy.BoxImageProxy(RName.GetRName(styles.NodeColorSpillImg, RName.ERNameType.Engine), new Thickness(8.0f / 64.0f, 3.0f / 32.0f, 0, 0));
            if (UEngine.Instance.UIManager[styles.NodeTitleHighlightImg] == null)
                UEngine.Instance.UIManager[styles.NodeTitleHighlightImg] = new EGui.UIProxy.BoxImageProxy(RName.GetRName(styles.NodeTitleHighlightImg, RName.ERNameType.Engine), new Thickness(16.0f / 64.0f, 1.0f, 16.0f / 64.0f, 0.0f));
            if (UEngine.Instance.UIManager[styles.NodeShadowImg] == null)
                UEngine.Instance.UIManager[styles.NodeShadowImg] = new EGui.UIProxy.BoxImageProxy(RName.GetRName(styles.NodeShadowImg, RName.ERNameType.Engine), new Thickness(18.0f / 64.0f));
            if (UEngine.Instance.UIManager[styles.NodeShadowSelectedImg] == null)
                UEngine.Instance.UIManager[styles.NodeShadowSelectedImg] = new EGui.UIProxy.BoxImageProxy(RName.GetRName(styles.NodeShadowSelectedImg, RName.ERNameType.Engine), new Thickness(18.0f / 64.0f));

            if (UEngine.Instance.UIManager[styles.PinConnectedVarImg] == null)
                UEngine.Instance.UIManager[styles.PinConnectedVarImg] = new EGui.UIProxy.ImageProxy(RName.GetRName(styles.PinConnectedVarImg, RName.ERNameType.Engine));
            if (UEngine.Instance.UIManager[styles.PinDisconnectedVarImg] == null)
                UEngine.Instance.UIManager[styles.PinDisconnectedVarImg] = new EGui.UIProxy.ImageProxy(RName.GetRName(styles.PinDisconnectedVarImg, RName.ERNameType.Engine));
            if (UEngine.Instance.UIManager[styles.PinConnectedExecImg] == null)
                UEngine.Instance.UIManager[styles.PinConnectedExecImg] = new EGui.UIProxy.ImageProxy(RName.GetRName(styles.PinConnectedExecImg, RName.ERNameType.Engine));
            if (UEngine.Instance.UIManager[styles.PinDisconnectedExecImg] == null)
                UEngine.Instance.UIManager[styles.PinDisconnectedExecImg] = new EGui.UIProxy.ImageProxy(RName.GetRName(styles.PinDisconnectedExecImg, RName.ERNameType.Engine));
            if (UEngine.Instance.UIManager[styles.PinHoverCueImg] == null)
                UEngine.Instance.UIManager[styles.PinHoverCueImg] = new EGui.UIProxy.ImageProxy(RName.GetRName(styles.PinHoverCueImg, RName.ERNameType.Engine));
            
            if (UEngine.Instance.UIManager[styles.BreakpointNodeImg] == null)
                UEngine.Instance.UIManager[styles.BreakpointNodeImg] = new EGui.UIProxy.ImageProxy(RName.GetRName(styles.BreakpointNodeImg, RName.ERNameType.Engine));
        }

        UNodeGraph mGraph = null;
        public UNodeGraph Graph => mGraph;
        List<UNodeGraph> mGraphInherit { get; } = new List<UNodeGraph>();
        Vector2 DrawOffset;
        public void SetGraph(UNodeGraph graph)
        {
            if(mGraph != null)
                mGraph.ResetButtonPress();
            mGraph = graph;
            // Update inherit
            mGraphInherit.Clear();
            if (graph != null)
            {
                mGraphInherit.Add(graph);
                var parent = graph.ParentGraph;
                while (parent != null)
                {
                    mGraphInherit.Insert(0, parent);
                    parent = parent.ParentGraph;
                }
            }
        }
        protected Vector2 GraphSize;
        public void OnDraw()
        {
            if (mGraph == null)
                return;
            if (mGraph.AssetName != null)
            {
                if (ImGuiAPI.Button("Snap"))
                {
                    var presentWindow = ImGuiAPI.GetWindowViewportData();
                    if (presentWindow != null)
                    {
                        var ameta = UEngine.Instance.AssetMetaManager.GetAssetMeta(mGraph.AssetName);
                        Editor.USnapshot.Save(mGraph.AssetName, ameta, presentWindow.SwapChain.mCoreObject.GetBackBuffer(0), 
                            (uint)DrawOffset.X, (uint)DrawOffset.Y, (uint)GraphSize.X, (uint)GraphSize.Y);
                    }
                }
                ImGuiAPI.SameLine(0, -1);
            }
            for (int i = 0; i < mGraphInherit.Count; i++)
            {
                if (i != 0)
                {
                    ImGuiAPI.SameLine(0, -1);
                }
                if (ImGuiAPI.Button(mGraphInherit[i].GraphName))
                {
                    //mGraph.ChangeGraph(mGraphInherit[i]);
                    SetGraph(mGraphInherit[i]);
                }
                if (i < mGraphInherit.Count - 1)
                {
                    ImGuiAPI.SameLine(0, -1);
                    ImGuiAPI.Text("/");
                }
            }
            if (ImGuiAPI.BeginChild("Graph", in Vector2.Zero, false, ImGuiWindowFlags_.ImGuiWindowFlags_NoMove | ImGuiWindowFlags_.ImGuiWindowFlags_NoScrollbar | ImGuiWindowFlags_.ImGuiWindowFlags_NoScrollWithMouse))
            {
                var vpMin = ImGuiAPI.GetWindowContentRegionMin();
                var vpMax = ImGuiAPI.GetWindowContentRegionMax();

                Vector2 sz = new Vector2(-1,-1);
                //ImGuiAPI.InvisibleButton("ClientContent", sz, ImGuiButtonFlags_.ImGuiButtonFlags_None);
                sz.X = vpMax.X - vpMin.X;
                sz.Y = vpMax.Y - vpMin.Y;
                GraphSize = sz;

                var winPos = ImGuiAPI.GetWindowPos();
                DrawOffset.SetValue(winPos.X + vpMin.X, winPos.Y + vpMin.Y);
                var pt = ImGuiAPI.GetMousePos();
                var delta = pt - DrawOffset;
                var screenPt = mGraph.ToScreenPos(delta.X, delta.Y);
                if (delta.X >= 0 && delta.X <= sz.X && delta.Y >= 0 && delta.Y <= sz.Y)
                {
                    ProcessKeyboard();
                    ProcessMouse(in screenPt, in sz);
                }

                var cmd = ImGuiAPI.GetWindowDrawList();
                if (mGraph.PhysicalSizeVP.X != sz.X || mGraph.PhysicalSizeVP.Y != sz.Y)
                {
                    mGraph.SetPhysicalSizeVP(sz.X, sz.Y);
                }


                // draw grid
                var styles = UNodeGraphStyles.DefaultStyles;
                cmd.AddRectFilled(DrawOffset, DrawOffset + sz, styles.GridBackgroundColor, 0, ImDrawFlags_.ImDrawFlags_None);
                var step = styles.GridStep / mGraph.ScaleVP;
                var hCount = (int)(sz.X / step);
                var vCount = (int)(sz.Y / step);
                var gridStart = CanvasToDraw(new Vector2(0, 0));// - DrawOffset;
                var offSet = (winPos - gridStart) / step;
                for (int i=(int)offSet.X; i<hCount + offSet.X; i++)
                {
                    cmd.AddLine(
                        new Vector2(gridStart.X + i * step, winPos.Y),
                        new Vector2(gridStart.X + i * step, winPos.Y + sz.Y),
                        (i % 8 == 0) ? styles.GridSplitLineColor : styles.GridNormalLineColor, 1.0f);
                }
                for (int i = (int)offSet.Y; i < vCount + offSet.Y; i++)
                {
                    cmd.AddLine(
                        new Vector2(winPos.X, gridStart.Y + i * step),
                        new Vector2(winPos.X + sz.X, gridStart.Y + i * step),
                        (i % 8 == 0) ? styles.GridSplitLineColor : styles.GridNormalLineColor, 1.0f);
                }

                for (int i = 0; i < mGraph.Linkers.Count;)
                {
                    var cur = mGraph.Linkers[i];
                    if (cur.InPin == null || cur.OutPin == null || cur.InPin.HostNode == null || cur.OutPin.HostNode == null)
                    {
                        mGraph.Linkers.RemoveAt(i);
                        break;
                    }
                    DrawLinker(cmd, cur);
                    i++;
                }

                foreach (var i in mGraph.Nodes)
                {
                    if (mGraph.IsInViewport(i))
                    {
                        DrawNode(cmd, i);
                    }
                }

                if (mGraph.LinkingOp.StartPin != null)
                {
                    var mPos = ImGuiAPI.GetMousePos();
                }

                // draw mouse drag rect
                if (mGraph.MultiSelectionMode)
                {
                    var min = mGraph.CanvasToViewport(Vector2.Minimize(mGraph.PressPosition, mGraph.DragPosition)) + DrawOffset;
                    var max = mGraph.CanvasToViewport(Vector2.Maximize(mGraph.PressPosition, mGraph.DragPosition)) + DrawOffset;
                    var size = max - min;
                    if (size.X > MathHelper.Epsilon || size.Y > MathHelper.Epsilon)
                    {
                        cmd.AddRect(min, max, 0xFF00FF00, 0.0f, ImDrawFlags_.ImDrawFlags_None, 2);
                    }
                }

                mGraph.OnDrawAfter(this, styles, cmd);
                DrawPopMenu();
            }
            ImGuiAPI.EndChild();
        }

        void ProcessKeyboard()
        {
            if (ImGuiAPI.IsWindowFocused(ImGuiFocusedFlags_.ImGuiFocusedFlags_ChildWindows | ImGuiFocusedFlags_.ImGuiFocusedFlags_RootWindow) == false)
                return;

            if(UEngine.Instance.InputSystem.IsKeyDown(Input.Keycode.KEY_DELETE))
            {
                foreach(var node in mGraph.SelectedNodes)
                {
                    mGraph.RemoveNode(node.Node);
                }
                mGraph.ClearSelected();
            }
            if(UEngine.Instance.InputSystem.IsKeyDown(Input.Keycode.KEY_TAB))
            {
                mGraph.CurMenuType = UNodeGraph.EGraphMenu.Canvas;
                mGraph.CanvasMenus.SetIsExpanded(false, true);
                mGraph.CanvasMenuFilterStr = "";
            }
        }

        unsafe void ProcessMouse(in Vector2 screenPt, in Vector2 rectSize)
        {
            //if (ImGuiAPI.IsWindowFocused(ImGuiFocusedFlags_.ImGuiFocusedFlags_ChildWindows | ImGuiFocusedFlags_.ImGuiFocusedFlags_RootWindow) == false)
            //    return;
            if (mGraph.CurMenuType != UNodeGraph.EGraphMenu.None)
                return;
            if (ImGuiAPI.IsWindowHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_ChildWindows))
            {
                mGraph.Zoom(screenPt, ImGuiAPI.GetIO().MouseWheel);
            }
            mGraph.PressDrag(in screenPt);

            var clickPos = Vector2.Zero;
            if (ImGuiAPI.IsMouseDown(ImGuiMouseButton_.ImGuiMouseButton_Left) || ImGuiAPI.IsMouseReleased(ImGuiMouseButton_.ImGuiMouseButton_Left))
                clickPos = ImGuiAPI.GetIO().MouseClickedPos[(int)ImGuiMouseButton_.ImGuiMouseButton_Left];
            if (ImGuiAPI.IsMouseDown(ImGuiMouseButton_.ImGuiMouseButton_Right) || ImGuiAPI.IsMouseReleased(ImGuiMouseButton_.ImGuiMouseButton_Right))
                clickPos = ImGuiAPI.GetIO().MouseClickedPos[(int)ImGuiMouseButton_.ImGuiMouseButton_Right];
            if (ImGuiAPI.IsMouseDown(ImGuiMouseButton_.ImGuiMouseButton_Middle) || ImGuiAPI.IsMouseReleased(ImGuiMouseButton_.ImGuiMouseButton_Middle))
                clickPos = ImGuiAPI.GetIO().MouseClickedPos[(int)ImGuiMouseButton_.ImGuiMouseButton_Middle];
            var delta = clickPos - DrawOffset;
            if (delta.X >= 0 && delta.X <= rectSize.X && delta.Y >= 0 && delta.Y <= rectSize.Y)
            {
                if (mGraph.ButtonPress[(int)UNodeGraph.EMouseButton.Middle] == false)
                {
                    if (ImGuiAPI.IsMouseDown(ImGuiMouseButton_.ImGuiMouseButton_Middle))
                    {
                        mGraph.MiddlePress(in screenPt);
                    }
                }
                else
                {
                    if (!ImGuiAPI.IsMouseDown(ImGuiMouseButton_.ImGuiMouseButton_Middle))
                    {
                        mGraph.MiddleRelease(in screenPt);
                    }
                }
                if (ImGuiAPI.IsMouseDoubleClicked(ImGuiMouseButton_.ImGuiMouseButton_Middle))
                {
                    mGraph.MiddleDoubleClicked(in screenPt);
                }

                if (mGraph.ButtonPress[(int)UNodeGraph.EMouseButton.Left] == false)
                {
                    if (ImGuiAPI.IsMouseDown(ImGuiMouseButton_.ImGuiMouseButton_Left))
                    {
                        mGraph.LeftPress(in screenPt);
                    }
                }
                else
                {
                    if (!ImGuiAPI.IsMouseDown(ImGuiMouseButton_.ImGuiMouseButton_Left))
                    {
                        mGraph.LeftRelease(in screenPt);
                    }
                }
                if (ImGuiAPI.IsMouseDoubleClicked(ImGuiMouseButton_.ImGuiMouseButton_Left))
                {
                    mGraph.LeftDoubleClicked(in screenPt);
                }

                if (mGraph.ButtonPress[(int)UNodeGraph.EMouseButton.Right] == false)
                {
                    if (ImGuiAPI.IsMouseDown(ImGuiMouseButton_.ImGuiMouseButton_Right))
                    {
                        mGraph.RightPress(in screenPt);
                    }
                }
                else
                {
                    if (!ImGuiAPI.IsMouseDown(ImGuiMouseButton_.ImGuiMouseButton_Right))
                    {
                        mGraph.RightRelease(in screenPt);
                    }
                }
                if (ImGuiAPI.IsMouseDoubleClicked(ImGuiMouseButton_.ImGuiMouseButton_Right))
                {
                    mGraph.RightDoubleClicked(screenPt);
                }
            }
        }
        public void DrawImage(ImDrawList cmdlist, EGui.UUvAnim icon, in Vector2 rcMin, in Vector2 rcMax)
        {
            icon.OnDraw(cmdlist, rcMin, rcMax, 0);
        }
        public Vector2 CanvasToDraw(in Vector2 pos)
        {
            return mGraph.CanvasToViewport(in pos) + DrawOffset;
        }

        public string BreakerName = "";
        public void DrawNode(ImDrawList cmdlist, UNodeBase node)
        {
            if(node.LayoutDirty)
            {
                node.UpdateLayout();
                node.LayoutDirty = false;
            }
            var styles = UNodeGraphStyles.DefaultStyles;
            var nodeStart = CanvasToDraw(node.Position);
            var nodeEnd = CanvasToDraw(node.Position + node.Size);

            if (node.ParentGraph != null)
                ImGuiAPI.SetWindowFontScale(1.0f / node.ParentGraph.ScaleVP);


            var shadowExt = new Vector2(12, 12);
            //cmdlist.AddRectFilled(in nodeStart, in nodeEnd, node.BackColor, 0, 0);
            if (node.Selected)
            {
                var selImg = UEngine.Instance.UIManager[styles.NodeShadowSelectedImg] as EGui.UIProxy.ImageProxy;
                if(selImg != null)
                    selImg.OnDraw(cmdlist, nodeStart - shadowExt, nodeEnd + shadowExt);
            }
            else
            {
                var shadowImg = UEngine.Instance.UIManager[styles.NodeShadowImg] as EGui.UIProxy.ImageProxy;
                if(shadowImg != null)
                    shadowImg.OnDraw(cmdlist, nodeStart - shadowExt, nodeEnd + shadowExt);
            }
            var nodeBodyImg = UEngine.Instance.UIManager[styles.NodeBodyImg] as EGui.UIProxy.ImageProxy;
            if(nodeBodyImg != null)
                nodeBodyImg.OnDraw(cmdlist, nodeStart, nodeEnd);

            var endTitle = mGraph.CanvasToViewport(node.Position + new Vector2(node.Size.X, node.TitleHeight)) + DrawOffset;
            //cmdlist.AddRectFilled(in nodeStart, in endTitle, node.TitleColor, 0, 0);
            uint col = 0xFFFFFFFF;
            uint errorCol = 0xFF0000FF;
            {//DrawTitle
                var titleImg = UEngine.Instance.UIManager[styles.NodeTitleImg] as EGui.UIProxy.ImageProxy;
                if(titleImg != null)
                    titleImg.OnDraw(cmdlist, nodeStart, endTitle);
                var colorSpillImg = UEngine.Instance.UIManager[styles.NodeColorSpillImg] as EGui.UIProxy.ImageProxy;
                if(colorSpillImg != null)
                    colorSpillImg.OnDraw(cmdlist, nodeStart, endTitle, node.TitleColor);
                var titleHighlightImg = UEngine.Instance.UIManager[styles.NodeTitleHighlightImg] as EGui.UIProxy.ImageProxy;
                if (titleHighlightImg != null)
                    titleHighlightImg.OnDraw(cmdlist, nodeStart, endTitle);
                //var curStart = node.Position;
                //{//Draw Node Icon

                    //    curStart.X += styles.IconOffset.X;
                    //    var drawStart = CanvasToDraw(curStart);
                    //    var drawEnd = CanvasToDraw(curStart + node.Icon.Size);
                    //    DrawImage(cmdlist, node.Icon, in drawStart, in drawEnd);

                    //    curStart.X += node.Icon.Size.X;
                    //}

                //Draw Node Name
                {
                    var drawStart = CanvasToDraw(node.Position + styles.TitlePadding);
                    if (node.HasError)
                        cmdlist.AddText(in drawStart, errorCol, node.Name, null);
                    else
                        cmdlist.AddText(in drawStart, col, node.Name, null);
                }
            }

            {//Draw Preview
                Vector2 prevStart = node.PrevPos;
                //prevStart.X = node.Position.X + (node.Size.X - node.PrevSize.X) * 0.5f;
                //prevStart.Y = node.Position.Y + node.TitleHeight;
                var start1 = CanvasToDraw(in prevStart);
                var end1 = CanvasToDraw(prevStart + node.PrevSize);
                node.OnPreviewDraw(in start1, in end1, cmdlist);
            }

            if (mGraph.LinkingOp.HoverPin != null)
            {
                var min = mGraph.LinkingOp.HoverPin.Position;
                Vector2 max;
                max = min + mGraph.LinkingOp.HoverPin.Size;
                min = mGraph.CanvasToViewport(min) + DrawOffset;
                max = mGraph.CanvasToViewport(max) + DrawOffset;
                cmdlist.AddRect(in min, in max, styles.HighLightColor, 0, ImDrawFlags_.ImDrawFlags_RoundCornersAll, 2);
            }

            string lastGroup = null;
            ImGuiAPI.PushID($"{node.NodeId.ToString()}");
            Vector2 groupStart = Vector2.Zero, groupEnd = Vector2.Zero;
            for(int i=0; i<node.Inputs.Count; i++)
            {
                var inPin = node.Inputs[i];

                var start = CanvasToDraw(inPin.HotPosition);
                var end = CanvasToDraw(inPin.HotPosition + inPin.HotSize);
                var pinEnd = CanvasToDraw(inPin.Position + inPin.Size);
                groupEnd = new Vector2(Math.Max(pinEnd.X, groupEnd.X), Math.Max(pinEnd.Y, groupEnd.Y));
                if (!string.IsNullOrEmpty(inPin.GroupName) && (lastGroup != inPin.GroupName))
                {
                    if (!string.IsNullOrEmpty(lastGroup))
                    {
                        cmdlist.AddLine(groupStart, new Vector2(groupEnd.X, groupStart.Y), 0xFFFFFFFF, 1);
                        cmdlist.AddLine(new Vector2(groupStart.X, groupEnd.Y), groupEnd, 0xFFFFFFFF, 1);
                        //cmdlist.AddRect(groupStart, groupEnd, 0xFF0000FF, 0, ImDrawFlags_.ImDrawFlags_None, 1);
                    }
                    groupStart = CanvasToDraw(inPin.Position);
                    groupEnd = pinEnd;
                    lastGroup = inPin.GroupName;
                }
                if(i == (node.Inputs.Count - 1))
                {
                    if (!string.IsNullOrEmpty(lastGroup))
                    {
                        cmdlist.AddLine(groupStart, new Vector2(groupEnd.X, groupStart.Y), 0xFFFFFFFF, 1);
                        cmdlist.AddLine(new Vector2(groupStart.X, groupEnd.Y), groupEnd, 0xFFFFFFFF, 1);
                        //cmdlist.AddRect(groupStart, groupEnd, 0xFF0000FF, 0, ImDrawFlags_.ImDrawFlags_None, 1);
                    }
                }
                if(inPin.ShowIcon)
                {
                    if(mGraph.PinHasLinker(inPin))
                    {
                        if (inPin.LinkDesc != null && inPin.LinkDesc.Icon != null)
                            DrawImage(cmdlist, inPin.LinkDesc.Icon, start, end);
                        else
                            DrawImage(cmdlist, styles.PinInStyle.Image, start, end);
                    }
                    else
                    {
                        if (inPin.LinkDesc != null && inPin.LinkDesc.DisconnectIcon != null)
                            DrawImage(cmdlist, inPin.LinkDesc.DisconnectIcon, start, end);
                        else
                            DrawImage(cmdlist, styles.PinInStyle.DisconnectImage, start, end);
                    }
                }
                if(!string.IsNullOrEmpty(inPin.Name) && inPin.ShowName)
                {
                    start = CanvasToDraw(inPin.NamePosition);
                    if(node.CodeExcept != null && node.CodeExcept.ErrorPin == inPin)
                        cmdlist.AddText(start, errorCol, inPin.Name, null);
                    else
                        cmdlist.AddText(start, col, inPin.Name, null);
                }
				if (inPin.EditValue != null)
                {
                    var pos = CanvasToDraw(inPin.EditValuePosition) - ImGuiAPI.GetWindowPos();
                    ImGuiAPI.SetCursorPos(pos);
                    inPin.EditValue.OnDraw(node, inPin, styles, 1/mGraph.ScaleVP);
                }
            }
            ImGuiAPI.PopID();

            foreach (var i in node.Outputs)
            {
                var start = CanvasToDraw(i.HotPosition);
                var end = CanvasToDraw(i.HotPosition + i.HotSize);
                if(i.ShowIcon)
                {
                    if (mGraph.PinHasLinker(i))
                    {
                        if (i.LinkDesc != null && i.LinkDesc.Icon != null)
                            DrawImage(cmdlist, i.LinkDesc.Icon, start, end);
                        else
                            DrawImage(cmdlist, styles.PinInStyle.Image, start, end);
                    }
                    else
                    {
                        if (i.LinkDesc != null && i.LinkDesc.DisconnectIcon != null)
                            DrawImage(cmdlist, i.LinkDesc.DisconnectIcon, start, end);
                        else
                            DrawImage(cmdlist, styles.PinInStyle.DisconnectImage, start, end);
                    }
                }
                if (!string.IsNullOrEmpty(i.Name) && i.ShowName)
                {
                    start = CanvasToDraw(i.NamePosition);
                    if(node.CodeExcept != null && node.CodeExcept.ErrorPin == i)
                        cmdlist.AddText(start, errorCol, i.Name, null);
                    else
                        cmdlist.AddText(start, col, i.Name, null);
                }
            }

            if (mGraph.LinkingOp.StartPin != null)
            {
                DrawLinkingOp(cmdlist);
            }

            // breaker point
            if(node is IBreakableNode)
            {
                var breakableNode = node as IBreakableNode;
                int circleSegments = 36;
                uint circleBG = 0xFF0000FF;
                uint circleBorder = 0xFF000000;
                float borderThickness = 2 / mGraph.ScaleVP;
                float circleRadius = 12 / mGraph.ScaleVP;
                switch(breakableNode.BreakerState)
                {
                    case EBreakerState.Enable:
                        cmdlist.AddCircleFilled(in nodeStart, circleRadius, circleBG, circleSegments);
                        cmdlist.AddCircle(in nodeStart, circleRadius, circleBorder, circleSegments, borderThickness);
                        break;
                    case EBreakerState.Disable:
                        cmdlist.AddCircle(in nodeStart, circleRadius - 2, circleBG, circleSegments, borderThickness + 1);
                        cmdlist.AddCircle(in nodeStart, circleRadius - 4, circleBorder, circleSegments, 1);
                        cmdlist.AddCircle(in nodeStart, circleRadius, circleBorder, circleSegments, 1);
                        break;
                    case EBreakerState.Hidden:
                        break;
                }

                if(BreakerName == breakableNode.BreakerName)
                {
                    var breakpointImg = UEngine.Instance.UIManager[styles.BreakpointNodeImg] as EGui.UIProxy.ImageProxy;
                    if (breakpointImg != null)
                    {
                        var imgSize = new Vector2(50.0f, 50.0f) / mGraph.ScaleVP;
                        var posStart = nodeStart + new Vector2((nodeEnd.X - nodeStart.X - imgSize.X) * 0.5f, -imgSize.Y);
                        var posEnd = posStart + imgSize;
                        breakpointImg.OnDraw(cmdlist, posStart, posEnd, 0xFFFFFFFF);
                    }

                    var execNode = node as IBeforeExecNode;
                    if(execNode != null)
                    {
                        execNode.LightDebuggerLine();
                    }
                }

                Vector2 circleStart = nodeStart - new Vector2(circleRadius, circleRadius);
                Vector2 circleEnd = nodeStart + new Vector2(circleRadius, circleRadius);
                if(ImGuiAPI.IsMouseClicked(ImGuiMouseButton_.ImGuiMouseButton_Left, false) && ImGuiAPI.IsMouseHoveringRect(circleStart, circleEnd, true))
                {
                    switch(breakableNode.BreakerState)
                    {
                        case EBreakerState.Enable:
                            breakableNode.BreakerState = EBreakerState.Disable;
                            break;
                        case EBreakerState.Disable:
                            breakableNode.BreakerState = EBreakerState.Enable;
                            break;
                    }
                }
            }

            node.OnAfterDraw(styles, cmdlist);
            ImGuiAPI.SetWindowFontScale(1.0f);
        }
        public void DrawLinker(ImDrawList cmdlist, UPinLinker linker)
        {
            var p1_v = linker.OutPin.HotPosition + linker.OutPin.HotSize * 0.5f;
            var p4_v = linker.InPin.HotPosition + linker.InPin.HotSize * 0.5f;
            var p1 = new Vector2(p1_v.X, p1_v.Y);
            var p4 = new Vector2(p4_v.X, p4_v.Y);
            var delta = p4_v - p1_v;
            //auto delta = ImVec2(delta_v.GetX(), delta_v.GetY());
            delta.X = Math.Abs(delta.X);

            var p2 = new Vector2(p1.X + delta.X * 0.5f, p1.Y);
            var p3 = new Vector2(p4.X - delta.X * 0.5f, p4.Y);

            var styles = UNodeGraphStyles.DefaultStyles;
            int num_segs = (int)(delta.Length() / styles.BezierPixelPerSegement + 1);

            var lineColor = styles.LinkerColor;
            if (linker.InDebuggerLine)
                lineColor = 0xFF0000FF;
            else if (linker.OutPin.LinkDesc != null)
                lineColor = linker.OutPin.LinkDesc.LineColor;
            else if (linker.InPin.LinkDesc != null)
                lineColor = linker.InPin.LinkDesc.LineColor;

            var thinkness = styles.LinkerThinkness;
            if (linker.OutPin.LinkDesc != null)
                thinkness = linker.OutPin.LinkDesc.LineThinkness;
            else if (linker.InPin.LinkDesc != null)
                thinkness = linker.InPin.LinkDesc.LineThinkness;

            p1 = mGraph.CanvasToViewport(in p1);
            p2 = mGraph.CanvasToViewport(in p2);
            p3 = mGraph.CanvasToViewport(in p3);
            p4 = mGraph.CanvasToViewport(in p4);
            p1 += DrawOffset;
            p2 += DrawOffset;
            p3 += DrawOffset;
            p4 += DrawOffset;
            cmdlist.AddBezierCubic(in p1, in p2, in p3, in p4, lineColor, thinkness, num_segs);
        }
        public void DrawLinkingOp(ImDrawList cmdlist)
        {
            var styles = UNodeGraphStyles.DefaultStyles;
            var LinkingOp = mGraph.LinkingOp;
            var p4 = mGraph.LinkingOp.BlockingEnd;
            Vector2 p1;
            float ControlLength = 0;
            if (LinkingOp.StartPin.GetType() == typeof(PinIn))
            {
                p1 = LinkingOp.StartPin.HotPosition + LinkingOp.StartPin.HotSize * 0.5f;
                var delta = p4 - p1;
                delta.X = Math.Abs(delta.X);
                ControlLength = delta.X * 0.5f;
            }
            else
            {
                p1 = LinkingOp.StartPin.HotPosition + LinkingOp.StartPin.HotSize * 0.5f;
                var delta = p4 - p1;
                delta.X = Math.Abs(delta.X);
                ControlLength = delta.X * (-0.5f);
            }
            var p2 = new Vector2(p1.X - ControlLength, p1.Y);
            var p3 = new Vector2(p4.X + ControlLength, p4.Y);

            p1 = mGraph.CanvasToViewport(in p1);
            p2 = mGraph.CanvasToViewport(in p2);
            p3 = mGraph.CanvasToViewport(in p3);
            p4 = mGraph.CanvasToViewport(in p4);
            p1 += DrawOffset;
            p2 += DrawOffset;
            p3 += DrawOffset;
            p4 += DrawOffset;
            cmdlist.AddBezierCubic(in p1, in p2, in p3, in p4, styles.LinkerColor, 3, 30);
        }
        bool mCanvasMenuFilterFocused = false;
        List<Rect> mMouseInvalidAreas = new List<Rect>();
        Vector2 mMenuWinSize = Vector2.Zero;
        public void DrawPopMenu()
        {
            if (mGraph.CurMenuType == UNodeGraph.EGraphMenu.None)
                return;

            var styles = UNodeGraphStyles.DefaultStyles;
            if(ImGuiAPI.IsMouseDown(ImGuiMouseButton_.ImGuiMouseButton_Left) ||
               ImGuiAPI.IsMouseDown(ImGuiMouseButton_.ImGuiMouseButton_Right))
            {
                var pos = ImGuiAPI.GetMousePos();
                bool contain = false;
                for(int i=0; i<mMouseInvalidAreas.Count; i++)
                {
                    contain |= mMouseInvalidAreas[i].Contains(pos);
                }
                if(!contain)
                {
                    mGraph.CurMenuType = UNodeGraph.EGraphMenu.None;
                    return;
                }
            }
            //if (ImGuiAPI.BeginPopupContextWindow(null, ImGuiPopupFlags_.ImGuiPopupFlags_MouseButtonLeft) ||
            //    ImGuiAPI.BeginPopupContextWindow(null, ImGuiPopupFlags_.ImGuiPopupFlags_MouseButtonRight))
            ImGuiAPI.OpenPopup("GraphContextMenu", ImGuiPopupFlags_.ImGuiPopupFlags_None);
            if(ImGuiAPI.BeginPopup("GraphContextMenu",
                ImGuiWindowFlags_.ImGuiWindowFlags_AlwaysAutoResize |
                ImGuiWindowFlags_.ImGuiWindowFlags_NoTitleBar | 
                ImGuiWindowFlags_.ImGuiWindowFlags_NoSavedSettings | 
                ImGuiWindowFlags_.ImGuiWindowFlags_NoScrollbar |
                ImGuiWindowFlags_.ImGuiWindowFlags_NoNav))
            {
                mGraph.OnBeforeDrawMenu(styles);

                if(mGraph.FirstSetCurMenuType)
                {
                    mSelectQuickMenuIdx = 0;
                }

                switch (mGraph.CurMenuType)
                {
                    case UNodeGraph.EGraphMenu.Canvas:
                        {
                            if(mMouseInvalidAreas.Count != 2)
                            {
                                mMouseInvalidAreas.Clear();
                                mMouseInvalidAreas.Add(Rect.DefaultRect);
                                mMouseInvalidAreas.Add(Rect.DefaultRect);
                            }
                            {
                                var pos = ImGuiAPI.GetWindowPos();
                                var size = ImGuiAPI.GetWindowSize();
                                mMouseInvalidAreas[0] = new Rect(pos.X, pos.Y, size.X, size.Y);

                                var width = ImGuiAPI.GetWindowContentRegionWidth();//ImGuiAPI.GetWindowWidth();
                                var drawList = ImGuiAPI.GetWindowDrawList();
                                if(mGraph.FirstSetCurMenuType)
                                    ImGuiAPI.SetKeyboardFocusHere(0);

                                EGui.UIProxy.SearchBarProxy.OnDraw(ref mCanvasMenuFilterFocused, in drawList, "search item", ref mGraph.CanvasMenuFilterStr, width);
                                Vector2 wsize = new Vector2(200, 400);
                                var id = ImGuiAPI.GetID("GraphContextMenu");
                                if(ImGuiAPI.BeginChild(id, wsize, false, 
                                    ImGuiWindowFlags_.ImGuiWindowFlags_NoTitleBar | 
                                    ImGuiWindowFlags_.ImGuiWindowFlags_NoSavedSettings))
                                {
                                    if (mGraph.CanvasMenuDirty)
                                    {
                                        mGraph.UpdateCanvasMenus();
                                        mGraph.CanvasMenuDirty = false;
                                    }
                                    mCurrentQuickMenuIdx = 0;
                                    for(var childIdx = 0; childIdx < mGraph.CanvasMenus.SubMenuItems.Count; childIdx++)
                                        DrawMenu(mGraph.CanvasMenus.SubMenuItems[childIdx], mGraph.CanvasMenuFilterStr.ToLower(), ref mMenuWinSize);
                                }
                                ImGuiAPI.EndChild();
                            }
                        }
                        break;
                    case UNodeGraph.EGraphMenu.Node:
                        if (mMouseInvalidAreas.Count != 1)
                        {
                            mMouseInvalidAreas.Clear();
                            mMouseInvalidAreas.Add(Rect.DefaultRect);
                        }
                        {
                            var pos = ImGuiAPI.GetWindowPos();
                            var size = ImGuiAPI.GetWindowSize();
                            mMouseInvalidAreas[0] = new Rect(pos.X, pos.Y, size.X, size.Y);

                            if (mGraph.NodeMenuDirty)
                            {
                                mGraph.UpdateNodeMenus();
                                var node = mGraph.PopMenuPressObject as IBreakableNode;
                                if(node != null)
                                {
                                    node.AddMenuItems(mGraph.NodeMenus);
                                }
                                mGraph.NodeMenuDirty = false;
                            }
                            for (var childIdx = 0; childIdx < mGraph.NodeMenus.SubMenuItems.Count; childIdx++)
                                DrawMenu(mGraph.NodeMenus.SubMenuItems[childIdx], "".ToLower(), ref mMenuWinSize);
                        }
                        break;
                    case UNodeGraph.EGraphMenu.Pin:
                        {
                            if (mMouseInvalidAreas.Count != 1)
                            {
                                mMouseInvalidAreas.Clear();
                                mMouseInvalidAreas.Add(Rect.DefaultRect);
                            }
                            {
                                var pos = ImGuiAPI.GetWindowPos();
                                var size = ImGuiAPI.GetWindowSize();
                                mMouseInvalidAreas[0] = new Rect(pos.X, pos.Y, size.X, size.Y);

                               if (mGraph.PinMenuDirty)
                                {
                                    mGraph.UpdatePinMenus();
                                    mGraph.PinMenuDirty = false;
                                }
                                for (var childIdx = 0; childIdx < mGraph.PinMenus.SubMenuItems.Count; childIdx++)
                                    DrawMenu(mGraph.PinMenus.SubMenuItems[childIdx], "".ToLower(), ref mMenuWinSize);
                                var pressPin = mGraph.PopMenuPressObject as NodePin;
                                if (pressPin != null)
                                {
                                    pressPin.HostNode.OnShowPinMenu(pressPin);
                                }
                            }
                        }
                        break;
                    case UNodeGraph.EGraphMenu.Object:
                        {
                            if (mMouseInvalidAreas.Count != 2)
                            {
                                mMouseInvalidAreas.Clear();
                                mMouseInvalidAreas.Add(Rect.DefaultRect);
                                mMouseInvalidAreas.Add(Rect.DefaultRect);
                            }
                            {
                                var pos = ImGuiAPI.GetWindowPos();
                                var size = ImGuiAPI.GetWindowSize();
                                mMouseInvalidAreas[0] = new Rect(pos.X, pos.Y, size.X, size.Y);

                                if (mGraph.PinLinkMenuDirty)
                                {
                                    mGraph.UpdatePinLinkMenu();
                                    mGraph.PinLinkMenuDirty = false;
                                }
                                var width = ImGuiAPI.GetColumnWidth(0);
                                var drawList = ImGuiAPI.GetWindowDrawList();
                                if (mGraph.FirstSetCurMenuType)
                                    ImGuiAPI.SetKeyboardFocusHere(0);

                                EGui.UIProxy.SearchBarProxy.OnDraw(ref mCanvasMenuFilterFocused, in drawList, "search item", ref mGraph.CanvasMenuFilterStr, width);
                                for (var childIdx = 0; childIdx < mGraph.ObjectMenus.SubMenuItems.Count; childIdx++)
                                    DrawMenu(mGraph.ObjectMenus.SubMenuItems[childIdx], mGraph.CanvasMenuFilterStr.ToLower(), ref mMenuWinSize);
                            }
                        }
                        break;
                    default:
                        break;
                };
                mGraph.OnAfterDrawMenu(styles);

                if (ImGuiAPI.IsKeyPressed((int)ImGuiKey_.ImGuiKey_UpArrow, true))
                {
                    mSelectQuickMenuIdx--;
                    if (mSelectQuickMenuIdx < 0)
                        mSelectQuickMenuIdx = 0;
                }
                if (ImGuiAPI.IsKeyPressed((int)ImGuiKey_.ImGuiKey_DownArrow, true))
                {
                    mSelectQuickMenuIdx++;
                    if (mSelectQuickMenuIdx >= mCurrentQuickMenuIdx)
                        mSelectQuickMenuIdx = mCurrentQuickMenuIdx - 1;
                }

                mGraph.FirstSetCurMenuType = false;
            }
            ImGuiAPI.EndPopup();
            //else
            //{
            //    mGraph.CurMenuType = UNodeGraph.EGraphMenu.None;
            //}
        }
        int mSelectQuickMenuIdx = 0;
        int mCurrentQuickMenuIdx = 0;
        public void DrawMenu(UMenuItem item, string filter, ref Vector2 maxSize)
        {
            if (!item.FilterCheck(filter))
                return;

            if (item.OnMenuDraw != null)
            {
                item.OnMenuDraw(item, this);
                return;
            }
            
            if (item.SubMenuItems.Count == 0)
            {
                if(!string.IsNullOrEmpty(item.Text))
                {
                    var flag = ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Leaf | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_NoTreePushOnOpen;
                    if (mSelectQuickMenuIdx == mCurrentQuickMenuIdx && !string.IsNullOrEmpty(filter))
                    {
                        flag |= ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Selected;
                        if(ImGuiAPI.IsKeyPressed((int)ImGuiKey_.ImGuiKey_Enter, false))
                        {
                            item.Action(item, mGraph.PopMenuPressObject);
                            ImGuiAPI.CloseCurrentPopup();
                            mGraph.CurMenuType = UNodeGraph.EGraphMenu.None;
                            mGraph.LinkingOp.StartPin = null;
                        }
                    }
                    ImGuiAPI.TreeNodeEx(item.Text, flag);
                    var size = ImGuiAPI.GetItemRectSize();
                    maxSize.X = Math.Max(size.X, maxSize.X);
                    maxSize.Y += size.Y;
                    if (ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left))
                    {
                        if (item.Action != null)
                        {
                            item.Action(item, mGraph.PopMenuPressObject);
                            ImGuiAPI.CloseCurrentPopup();
                            mGraph.CurMenuType = UNodeGraph.EGraphMenu.None;
                            mGraph.LinkingOp.StartPin = null;
                        }
                    }
                    mCurrentQuickMenuIdx++;
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(filter))
                    item.IsExpanded = true;
                ImGuiAPI.SetNextItemOpen(item.IsExpanded, ImGuiCond_.ImGuiCond_None);
                if (ImGuiAPI.TreeNode(item.Text))
                {
                    var size = ImGuiAPI.GetItemRectSize();
                    maxSize.X = Math.Max(size.X, maxSize.X);
                    maxSize.Y += size.Y;
                    item.IsExpanded = true;
                    for (int menuIdx = 0; menuIdx < item.SubMenuItems.Count; menuIdx++)
                    {
                        DrawMenu(item.SubMenuItems[menuIdx], filter, ref maxSize);
                    }
                    ImGuiAPI.TreePop();
                }
                else
                    item.IsExpanded = false;
            }
        }
    }
}
