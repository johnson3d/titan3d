using EngineNS.Bricks.CodeBuilder.MacrossNode;
using EngineNS.EGui.Controls;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.NodeGraph
{
    public class TtGraphRenderer : IGuiModule
    {
        public TtGraphRenderer()
        {
            var styles = UNodeGraphStyles.DefaultStyles;

            if (TtEngine.Instance.UIProxyManager[styles.NodeBodyImg] == null)
                TtEngine.Instance.UIProxyManager[styles.NodeBodyImg] = new EGui.UIProxy.BoxImageProxy(RName.GetRName(styles.NodeBodyImg, RName.ERNameType.Engine), new Thickness(16.0f / 64.0f, 25.0f / 64.0f, 16.0f / 64.0f, 16.0f / 64.0f));
            if (TtEngine.Instance.UIProxyManager[styles.NodeTitleImg] == null)
                TtEngine.Instance.UIProxyManager[styles.NodeTitleImg] = new EGui.UIProxy.BoxImageProxy(RName.GetRName(styles.NodeTitleImg, RName.ERNameType.Engine), new Thickness(12.0f / 64.0f));
            if (TtEngine.Instance.UIProxyManager[styles.NodeColorSpillImg] == null)
                TtEngine.Instance.UIProxyManager[styles.NodeColorSpillImg] = new EGui.UIProxy.BoxImageProxy(RName.GetRName(styles.NodeColorSpillImg, RName.ERNameType.Engine), new Thickness(8.0f / 64.0f, 3.0f / 32.0f, 0, 0));
            if (TtEngine.Instance.UIProxyManager[styles.NodeTitleHighlightImg] == null)
                TtEngine.Instance.UIProxyManager[styles.NodeTitleHighlightImg] = new EGui.UIProxy.BoxImageProxy(RName.GetRName(styles.NodeTitleHighlightImg, RName.ERNameType.Engine), new Thickness(16.0f / 64.0f, 1.0f, 16.0f / 64.0f, 0.0f));
            if (TtEngine.Instance.UIProxyManager[styles.NodeShadowImg] == null)
                TtEngine.Instance.UIProxyManager[styles.NodeShadowImg] = new EGui.UIProxy.BoxImageProxy(RName.GetRName(styles.NodeShadowImg, RName.ERNameType.Engine), new Thickness(18.0f / 64.0f));
            if (TtEngine.Instance.UIProxyManager[styles.NodeShadowSelectedImg] == null)
                TtEngine.Instance.UIProxyManager[styles.NodeShadowSelectedImg] = new EGui.UIProxy.BoxImageProxy(RName.GetRName(styles.NodeShadowSelectedImg, RName.ERNameType.Engine), new Thickness(18.0f / 64.0f));

            if (TtEngine.Instance.UIProxyManager[styles.PinConnectedVarImg] == null)
                TtEngine.Instance.UIProxyManager[styles.PinConnectedVarImg] = new EGui.UIProxy.ImageProxy(RName.GetRName(styles.PinConnectedVarImg, RName.ERNameType.Engine));
            if (TtEngine.Instance.UIProxyManager[styles.PinDisconnectedVarImg] == null)
                TtEngine.Instance.UIProxyManager[styles.PinDisconnectedVarImg] = new EGui.UIProxy.ImageProxy(RName.GetRName(styles.PinDisconnectedVarImg, RName.ERNameType.Engine));
            if (TtEngine.Instance.UIProxyManager[styles.PinConnectedExecImg] == null)
                TtEngine.Instance.UIProxyManager[styles.PinConnectedExecImg] = new EGui.UIProxy.ImageProxy(RName.GetRName(styles.PinConnectedExecImg, RName.ERNameType.Engine));
            if (TtEngine.Instance.UIProxyManager[styles.PinDisconnectedExecImg] == null)
                TtEngine.Instance.UIProxyManager[styles.PinDisconnectedExecImg] = new EGui.UIProxy.ImageProxy(RName.GetRName(styles.PinDisconnectedExecImg, RName.ERNameType.Engine));
            if (TtEngine.Instance.UIProxyManager[styles.PinHoverCueImg] == null)
                TtEngine.Instance.UIProxyManager[styles.PinHoverCueImg] = new EGui.UIProxy.ImageProxy(RName.GetRName(styles.PinHoverCueImg, RName.ERNameType.Engine));
            
            if (TtEngine.Instance.UIProxyManager[styles.BreakpointNodeImg] == null)
                TtEngine.Instance.UIProxyManager[styles.BreakpointNodeImg] = new EGui.UIProxy.ImageProxy(RName.GetRName(styles.BreakpointNodeImg, RName.ERNameType.Engine));
        }

        UNodeGraph mGraph = null;
        public UNodeGraph Graph => mGraph;
        List<UNodeGraph> mGraphInherit { get; } = new List<UNodeGraph>();
        Vector2 DrawOffset;
        public void SetGraph(UNodeGraph graph)
        {
            if(mGraph != null)
            {
                mGraph.ResetButtonPress();
                mGraph.ClearSelected();
            }
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
        public bool DrawInherit = true;
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
                        var ameta = TtEngine.Instance.AssetMetaManager.GetAssetMeta(mGraph.AssetName);
                        Editor.USnapshot.Save(mGraph.AssetName, ameta, presentWindow.SwapChain.mCoreObject.GetBackBuffer(0), 
                            (uint)DrawOffset.X, (uint)DrawOffset.Y, (uint)GraphSize.X, (uint)GraphSize.Y);
                    }
                }
                ImGuiAPI.SameLine(0, -1);
            }
            if(DrawInherit)
            {
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
            }
            if (ImGuiAPI.BeginChild("Graph", in Vector2.Zero, ImGuiChildFlags_.ImGuiChildFlags_None, ImGuiWindowFlags_.ImGuiWindowFlags_NoMove | ImGuiWindowFlags_.ImGuiWindowFlags_NoScrollbar | ImGuiWindowFlags_.ImGuiWindowFlags_NoScrollWithMouse))
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
                // ProcessKeyboard/ProcessMouse may be change mGraph, so store graph to make sure operation the same graph
                var tempGraph = mGraph;
                var screenPt = tempGraph.ToScreenPos(delta.X, delta.Y);
                ProcessKeyboard(in screenPt, tempGraph);
                ProcessMouse(in screenPt, in sz, tempGraph);

                var cmd = ImGuiAPI.GetWindowDrawList();
                if (tempGraph.PhysicalSizeVP.X != sz.X || tempGraph.PhysicalSizeVP.Y != sz.Y)
                {
                    tempGraph.SetPhysicalSizeVP(sz.X, sz.Y);
                }


                // draw grid
                var styles = UNodeGraphStyles.DefaultStyles;
                cmd.AddRectFilled(DrawOffset, DrawOffset + sz, styles.GridBackgroundColor, 0, ImDrawFlags_.ImDrawFlags_None);
                var step = styles.GridStep / tempGraph.ScaleVP;
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

                for (int i = 0; i < tempGraph.Linkers.Count;)
                {
                    var cur = tempGraph.Linkers[i];
                    if (cur.InPin == null || cur.OutPin == null || cur.InPin.HostNode == null || cur.OutPin.HostNode == null)
                    {
                        tempGraph.Linkers.RemoveAt(i);
                        break;
                    }
                    DrawLinker(cmd, cur);
                    i++;
                }

                foreach (var i in tempGraph.Nodes)
                {
                    if (tempGraph.IsInViewport(i))
                    {
                        DrawNode(cmd, i);
                    }
                }

                //if (tempGraph.LinkingOp.StartPin != null)
                //{
                //    var mPos = ImGuiAPI.GetMousePos();
                //}

                // draw mouse drag rect
                if (tempGraph.MultiSelectionMode)
                {
                    var min = tempGraph.CanvasToViewport(Vector2.Minimize(tempGraph.PressPosition, tempGraph.DragPosition)) + DrawOffset;
                    var max = tempGraph.CanvasToViewport(Vector2.Maximize(tempGraph.PressPosition, tempGraph.DragPosition)) + DrawOffset;
                    var size = max - min;
                    if (size.X > MathHelper.Epsilon || size.Y > MathHelper.Epsilon)
                    {
                        cmd.AddRect(min, max, 0xFF00FF00, 0.0f, ImDrawFlags_.ImDrawFlags_None, 2);
                    }
                }

                tempGraph.OnDrawAfter(this, styles, cmd);
                DrawPopMenu();
            }
            ImGuiAPI.EndChild();
        }

        void ProcessKeyboard(in Vector2 screenPt, UNodeGraph graph)
        {
            if (!ImGuiAPI.IsWindowHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_ChildWindows))
                return;
            //if (ImGuiAPI.IsWindowFocused(ImGuiFocusedFlags_.ImGuiFocusedFlags_ChildWindows | ImGuiFocusedFlags_.ImGuiFocusedFlags_RootWindow) == false)
            //    return;

            if (TtEngine.Instance.InputSystem.IsKeyPressed(Input.Keycode.KEY_DELETE))
            {
                foreach(var node in graph.SelectedNodes)
                {
                    graph.RemoveNode(node.Node);
                }
                graph.ClearSelected();
            }
            if (TtEngine.Instance.InputSystem.IsKeyPressed(Input.Keycode.KEY_TAB))
            {
                graph.PopMenuPosition = graph.ViewportRateToCanvas(screenPt);
                graph.CurMenuType = UNodeGraph.EGraphMenu.Canvas;
            }
            if(graph.IsKeydown(UNodeGraph.EKey.Ctl))
            {
                if (TtEngine.Instance.InputSystem.IsKeyPressed(Input.Keycode.KEY_c))
                {
                    graph.Copy();
                }
                if(TtEngine.Instance.InputSystem.IsKeyPressed(Input.Keycode.KEY_v))
                {
                    graph.Paste(screenPt);
                }
            }
        }

        internal bool mIsLeftMouseFocusOnGraph = false;
        internal bool mIsRightMouseFocusOnGraph = false;
        internal bool mIsMiddleMouseFocusOnGraph = false;
        unsafe void ProcessMouse(in Vector2 screenPt, in Vector2 rectSize, UNodeGraph graph)
        {
            //if (ImGuiAPI.IsWindowFocused(ImGuiFocusedFlags_.ImGuiFocusedFlags_ChildWindows | ImGuiFocusedFlags_.ImGuiFocusedFlags_RootWindow) == false)
            //    return;
            if (graph.CurMenuType != UNodeGraph.EGraphMenu.None)
                return;
            bool isHovered = false;
            if (ImGuiAPI.IsWindowHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_ChildWindows))
            {
                graph.Zoom(screenPt, ImGuiAPI.GetIO().MouseWheel);
                isHovered = true;
            }
            graph.PressDrag(in screenPt);

            var clickPos = Vector2.Zero;
            if (ImGuiAPI.IsMouseDown(ImGuiMouseButton_.ImGuiMouseButton_Left) || ImGuiAPI.IsMouseReleased(ImGuiMouseButton_.ImGuiMouseButton_Left))
                clickPos = ImGuiAPI.GetIO().MouseClickedPos[(int)ImGuiMouseButton_.ImGuiMouseButton_Left];
            if (ImGuiAPI.IsMouseDown(ImGuiMouseButton_.ImGuiMouseButton_Right) || ImGuiAPI.IsMouseReleased(ImGuiMouseButton_.ImGuiMouseButton_Right))
                clickPos = ImGuiAPI.GetIO().MouseClickedPos[(int)ImGuiMouseButton_.ImGuiMouseButton_Right];
            if (ImGuiAPI.IsMouseDown(ImGuiMouseButton_.ImGuiMouseButton_Middle) || ImGuiAPI.IsMouseReleased(ImGuiMouseButton_.ImGuiMouseButton_Middle))
                clickPos = ImGuiAPI.GetIO().MouseClickedPos[(int)ImGuiMouseButton_.ImGuiMouseButton_Middle];
            var delta = clickPos - DrawOffset;
            if (delta.X >= 0 && delta.X <= rectSize.X && delta.Y >= 0 && delta.Y <= rectSize.Y && isHovered)
            {
                if (graph.ButtonPress[(int)UNodeGraph.EMouseButton.Middle] == false)
                {
                    if (ImGuiAPI.IsMouseDown(ImGuiMouseButton_.ImGuiMouseButton_Middle))
                    {
                        graph.MiddlePress(in screenPt);
                        mIsMiddleMouseFocusOnGraph = true;
                    }
                }
                if (ImGuiAPI.IsMouseDoubleClicked(ImGuiMouseButton_.ImGuiMouseButton_Middle))
                {
                    graph.MiddleDoubleClicked(in screenPt);
                }

                if (graph.ButtonPress[(int)UNodeGraph.EMouseButton.Left] == false)
                {
                    if (ImGuiAPI.IsMouseDown(ImGuiMouseButton_.ImGuiMouseButton_Left))
                    {
                        graph.LeftPress(in screenPt);
                        mIsLeftMouseFocusOnGraph = true;
                    }
                }
                if (ImGuiAPI.IsMouseDoubleClicked(ImGuiMouseButton_.ImGuiMouseButton_Left))
                {
                    graph.LeftRelease(in screenPt);
                    graph.LeftDoubleClicked(in screenPt);
                }

                if (graph.ButtonPress[(int)UNodeGraph.EMouseButton.Right] == false)
                {
                    if (ImGuiAPI.IsMouseDown(ImGuiMouseButton_.ImGuiMouseButton_Right))
                    {
                        graph.RightPress(in screenPt);
                        mIsRightMouseFocusOnGraph = true;
                    }
                }                
                if (ImGuiAPI.IsMouseDoubleClicked(ImGuiMouseButton_.ImGuiMouseButton_Right))
                {
                    graph.RightRelease(in screenPt);
                    graph.RightDoubleClicked(screenPt);
                }
            }
            if(graph.ButtonPress[(int)UNodeGraph.EMouseButton.Middle] == true)
            {
                if (!ImGuiAPI.IsMouseDown(ImGuiMouseButton_.ImGuiMouseButton_Middle))
                {
                    graph.MiddleRelease(in screenPt);
                    mIsMiddleMouseFocusOnGraph = false;
                }
            }
            if (graph.ButtonPress[(int)UNodeGraph.EMouseButton.Left] == true)
            {
                if (!ImGuiAPI.IsMouseDown(ImGuiMouseButton_.ImGuiMouseButton_Left))
                {
                    graph.LeftRelease(in screenPt);
                    mIsLeftMouseFocusOnGraph = false;
                }
            }
            if (graph.ButtonPress[(int)UNodeGraph.EMouseButton.Right] == true)
            {
                if (!ImGuiAPI.IsMouseDown(ImGuiMouseButton_.ImGuiMouseButton_Right))
                {
                    graph.RightRelease(in screenPt);
                    mIsRightMouseFocusOnGraph = false;
                }
            }
        }
        public void DrawImage(ImDrawList cmdlist, EGui.TtUVAnim icon, in Vector2 rcMin, in Vector2 rcMax)
        {
            icon.OnDraw(cmdlist, rcMin, rcMax, 0);
        }
        public Vector2 CanvasToDraw(in Vector2 pos)
        {
            return mGraph.CanvasToViewport(in pos) + DrawOffset;
        }

        public string BreakerName = "";
        public unsafe void DrawNode(ImDrawList cmdlist, UNodeBase node)
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

            var font = ImGuiAPI.GetDrawListFont(cmdlist);

            var shadowExt = new Vector2(12, 12);
            //cmdlist.AddRectFilled(in nodeStart, in nodeEnd, node.BackColor, 0, 0);
            if (node.Selected)
            {
                var selImg = TtEngine.Instance.UIProxyManager[styles.NodeShadowSelectedImg] as EGui.UIProxy.ImageProxy;
                if(selImg != null)
                    selImg.OnDraw(cmdlist, nodeStart - shadowExt, nodeEnd + shadowExt);
            }
            else
            {
                var shadowImg = TtEngine.Instance.UIProxyManager[styles.NodeShadowImg] as EGui.UIProxy.ImageProxy;
                if(shadowImg != null)
                    shadowImg.OnDraw(cmdlist, nodeStart - shadowExt, nodeEnd + shadowExt);
            }
            var nodeBodyImg = TtEngine.Instance.UIProxyManager[styles.NodeBodyImg] as EGui.UIProxy.ImageProxy;
            if(nodeBodyImg != null)
                nodeBodyImg.OnDraw(cmdlist, nodeStart, nodeEnd);

            var endTitle = mGraph.CanvasToViewport(node.Position + new Vector2(node.Size.X, node.TitleHeight)) + DrawOffset;
            //cmdlist.AddRectFilled(in nodeStart, in endTitle, node.TitleColor, 0, 0);
            {//DrawTitle
                var titleImg = TtEngine.Instance.UIProxyManager[styles.NodeTitleImg] as EGui.UIProxy.ImageProxy;
                if(titleImg != null)
                    titleImg.OnDraw(cmdlist, nodeStart, endTitle);
                var colorSpillImg = TtEngine.Instance.UIProxyManager[styles.NodeColorSpillImg] as EGui.UIProxy.ImageProxy;
                if(colorSpillImg != null)
                    colorSpillImg.OnDraw(cmdlist, nodeStart, endTitle, node.TitleColor);
                var titleHighlightImg = TtEngine.Instance.UIProxyManager[styles.NodeTitleHighlightImg] as EGui.UIProxy.ImageProxy;
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
                var drawStart = CanvasToDraw(node.Position + styles.TitlePadding);
                if (node.Name != node.Label && !string.IsNullOrEmpty(node.Label))
                {
                    cmdlist.AddText(font, font.FontSize / mGraph.ScaleVP, &drawStart, styles.TitleTextDarkColor, node.Label, null, 0.0f, null);
                    var textSize = ImGuiAPI.CalcTextSize(node.Label, false, 0.0f);
                    drawStart.Y += textSize.Y + styles.TitleTextOffset;
                }
                if (node.HasError)
                    cmdlist.AddText(font, font.FontSize / mGraph.ScaleVP, &drawStart, styles.TitleTextErrorColor, node.Name != null ? node.Name : "none", null, 0.0f, null);
                else
                    cmdlist.AddText(font, font.FontSize / mGraph.ScaleVP, &drawStart, styles.TitleTextColor, node.Name != null ? node.Name : "none", null, 0.0f, null);
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
                        cmdlist.AddText(font, font.FontSize / mGraph.ScaleVP, &start, styles.TitleTextErrorColor, inPin.Name, null, 0.0f, null);
                    else
                        cmdlist.AddText(font, font.FontSize / mGraph.ScaleVP, &start, styles.TitleTextColor, inPin.Name, null, 0.0f, null);
                }
				if (inPin.EditValue != null)
                {
                    var pos = CanvasToDraw(inPin.EditValuePosition) - ImGuiAPI.GetWindowPos();
                    ImGuiAPI.SetCursorPos(pos);
                    inPin.EditValue.OnDraw(node, inPin, styles, 1/mGraph.ScaleVP, false);
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
                        cmdlist.AddText(font, font.FontSize / mGraph.ScaleVP, &start, styles.TitleTextErrorColor, i.Name, null, 0.0f, null);
                    else
                        cmdlist.AddText(font, font.FontSize / mGraph.ScaleVP, &start, styles.TitleTextColor, i.Name, null, 0.0f, null);
                }
            }

            if (mGraph.LinkingOp.IsDraging)
            {
                DrawLinkingOp(cmdlist);
            }
            DrawPreorderLink(cmdlist);

            // breaker point
            if (node is IBreakableNode)
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
                    var breakpointImg = TtEngine.Instance.UIProxyManager[styles.BreakpointNodeImg] as EGui.UIProxy.ImageProxy;
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
            var styles = UNodeGraphStyles.DefaultStyles;
            var p1_v = linker.OutPin.HotPosition + linker.OutPin.HotSize * 0.5f;
            var p4_v = linker.InPin.HotPosition + linker.InPin.HotSize * 0.5f;
            var p1 = new Vector2(p1_v.X, p1_v.Y);
            var p4 = new Vector2(p4_v.X, p4_v.Y);
            var delta = p4_v - p1_v;
            //auto delta = ImVec2(delta_v.GetX(), delta_v.GetY());
            var ctDelta = Math.Min(styles.LinkerMaxDelta, Math.Max(styles.LinkerMinDelta, Math.Max(Math.Abs(delta.X), Math.Abs(delta.Y)) * 0.5f));

            var p2 = new Vector2(p1.X + ctDelta, p1.Y);
            var p3 = new Vector2(p4.X - ctDelta, p4.Y);

            int num_segs = (int)(delta.Length() / styles.BezierPixelPerSegement + 1);

            float thicknessDelta = 1.5f;
            var lineColor = styles.LinkerColor;
            var thinkness = styles.LinkerThinkness;
            if (linker.InDebuggerLine)
                lineColor = MacrossStyles.Instance.DebugLineColor;
            else if (linker.OutPin.LinkDesc != null)
            {
                if(mGraph.LinkingOp.HoverPin != null)
                {
                    if (mGraph.LinkingOp.HoverPin == linker.OutPin || mGraph.LinkingOp.HoverPin == linker.InPin)
                    {
                        lineColor = LinkDesc.GetHighlight(linker.OutPin.LinkDesc.LineColor);
                        thinkness = linker.OutPin.LinkDesc.LineThinkness * thicknessDelta;
                    }
                    else
                    {
                        lineColor = LinkDesc.GetLowLight(linker.OutPin.LinkDesc.LineColor);
                        thinkness = linker.OutPin.LinkDesc.LineThinkness * thicknessDelta;
                    }
                }
                else
                {
                    lineColor = linker.OutPin.LinkDesc.LineColor;
                    thinkness = linker.OutPin.LinkDesc.LineThinkness;
                }
            }
            else if (linker.InPin.LinkDesc != null)
            {
                if(mGraph.LinkingOp.HoverPin != null)
                {
                    if (mGraph.LinkingOp.HoverPin == linker.OutPin || mGraph.LinkingOp.HoverPin == linker.InPin)
                    {
                        lineColor = LinkDesc.GetHighlight(linker.InPin.LinkDesc.LineColor);
                        thinkness = linker.InPin.LinkDesc.LineThinkness * thicknessDelta;
                    }
                    else
                    {
                        lineColor = LinkDesc.GetLowLight(linker.InPin.LinkDesc.LineColor);
                        thinkness = linker.InPin.LinkDesc.LineThinkness * thicknessDelta;
                    }
                }
                else
                {
                    lineColor = linker.InPin.LinkDesc.LineColor;
                    thinkness = linker.InPin.LinkDesc.LineThinkness;
                }
            }

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
            if(mGraph.LinkingOp.HoverPin != null)
            {
                p4 = mGraph.LinkingOp.HoverPin.HotPosition + mGraph.LinkingOp.HoverPin.HotSize * 0.5f;
            }
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
        void DrawPreorderLink(ImDrawList cmdlist)
        {
            if (mGraph.PreOrderLinker == null)
                return;

            var styles = UNodeGraphStyles.DefaultStyles;
            if(mGraph.PreOrderPinIn != null)
            {
                var ptA = mGraph.PreOrderPinIn.HotPosition + mGraph.PreOrderPinIn.HotSize * 0.5f;
                var ptB = mGraph.PreOrderLinker.OutPin.HotPosition + mGraph.PreOrderLinker.OutPin.HotSize * 0.5f;
                ptA = mGraph.CanvasToViewport(ptA) + DrawOffset;
                ptB = mGraph.CanvasToViewport(ptB) + DrawOffset;
                cmdlist.AddLine(ptA, ptB, styles.PreOrderLinkerColor, 1);
            }
            if(mGraph.PreOrderPinOut != null)
            {
                var ptA = mGraph.PreOrderLinker.InPin.HotPosition + mGraph.PreOrderLinker.InPin.HotSize * 0.5f;
                var ptB = mGraph.PreOrderPinOut.HotPosition + mGraph.PreOrderPinOut.HotSize * 0.5f;
                ptA = mGraph.CanvasToViewport(ptA) + DrawOffset;
                ptB = mGraph.CanvasToViewport(ptB) + DrawOffset;
                cmdlist.AddLine(ptA, ptB, styles.PreOrderLinkerColor, 1);
            }
        }
        bool mCanvasMenuFilterFocused = false;
        List<Rect> mMouseInvalidAreas = new List<Rect>();
        public string CanvasMenuFilterStr = "";

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
                    mGraph.LinkingOp.StartPin = null;
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
                                {
                                    ImGuiAPI.SetKeyboardFocusHere(0);
                                    mGraph.CanvasMenus.SetIsExpanded(false, true);
                                    CanvasMenuFilterStr = "";
                                }

                                EGui.UIProxy.SearchBarProxy.OnDraw(ref mCanvasMenuFilterFocused, in drawList, "search item", ref CanvasMenuFilterStr, width);
                                Vector2 wsize = new Vector2(200, 400);
                                var id = ImGuiAPI.GetID("GraphContextMenu");
                                if(ImGuiAPI.BeginChild(id, wsize, ImGuiChildFlags_.ImGuiChildFlags_None, 
                                    ImGuiWindowFlags_.ImGuiWindowFlags_NoTitleBar | 
                                    ImGuiWindowFlags_.ImGuiWindowFlags_NoSavedSettings |
                                    ImGuiWindowFlags_.ImGuiWindowFlags_HorizontalScrollbar))
                                {
                                    if (mGraph.CanvasMenuDirty)
                                    {
                                        mGraph.UpdateCanvasMenus();
                                        mGraph.CanvasMenuDirty = false;
                                    }
                                    var cmdList = ImGuiAPI.GetWindowDrawList();
                                    mCurrentQuickMenuIdx = 0;
                                    for(var childIdx = 0; childIdx < mGraph.CanvasMenus.SubMenuItems.Count; childIdx++)
                                        TtMenuItem.Draw(mGraph.CanvasMenus.SubMenuItems[childIdx], this, mGraph.PopMenuPressObject, CanvasMenuFilterStr.ToLower(), in cmdList, ref mSelectQuickMenuIdx, ref mCurrentQuickMenuIdx, MenuPostAction);
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
                            var cmdList = ImGuiAPI.GetWindowDrawList();
                            for (var childIdx = 0; childIdx < mGraph.NodeMenus.SubMenuItems.Count; childIdx++)
                                TtMenuItem.Draw(mGraph.NodeMenus.SubMenuItems[childIdx], this, mGraph.PopMenuPressObject, "".ToLower(), in cmdList, ref mSelectQuickMenuIdx, ref mCurrentQuickMenuIdx, MenuPostAction, TtMenuItem.EMenuStyle.Menu);
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
                                var cmdList = ImGuiAPI.GetWindowDrawList();
                                for (var childIdx = 0; childIdx < mGraph.PinMenus.SubMenuItems.Count; childIdx++)
                                    TtMenuItem.Draw(mGraph.PinMenus.SubMenuItems[childIdx], this, mGraph.PopMenuPressObject, "".ToLower(), in cmdList, ref mSelectQuickMenuIdx, ref mCurrentQuickMenuIdx, MenuPostAction, TtMenuItem.EMenuStyle.Menu);
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
                                {
                                    ImGuiAPI.SetKeyboardFocusHere(0);
                                    CanvasMenuFilterStr = "";
                                    mGraph.ObjectMenus.SetIsExpanded(false, true);
                                }

                                //var cmdList = ImGuiAPI.GetWindowDrawList();
                                EGui.UIProxy.SearchBarProxy.OnDraw(ref mCanvasMenuFilterFocused, in drawList, "search item", ref CanvasMenuFilterStr, width);
                                for (var childIdx = 0; childIdx < mGraph.ObjectMenus.SubMenuItems.Count; childIdx++)
                                    TtMenuItem.Draw(mGraph.ObjectMenus.SubMenuItems[childIdx], this, mGraph.PopMenuPressObject, CanvasMenuFilterStr.ToLower(), in drawList, ref mSelectQuickMenuIdx, ref mCurrentQuickMenuIdx, MenuPostAction);
                            }
                        }
                        break;
                    default:
                        break;
                };
                mGraph.OnAfterDrawMenu(styles);

                if (ImGuiAPI.IsKeyPressed(ImGuiKey.ImGuiKey_UpArrow, true))
                {
                    mSelectQuickMenuIdx--;
                    if (mSelectQuickMenuIdx < 0)
                        mSelectQuickMenuIdx = 0;
                }
                if (ImGuiAPI.IsKeyPressed(ImGuiKey.ImGuiKey_DownArrow, true))
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
        void MenuPostAction(TtMenuItem menuItem)
        {
            mGraph.CurMenuType = UNodeGraph.EGraphMenu.None;
            mGraph.LinkingOp.Reset();
        }
    }
}
