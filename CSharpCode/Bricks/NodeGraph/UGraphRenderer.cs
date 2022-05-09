using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.NodeGraph
{
    public class UGraphRenderer : Graphics.Pipeline.IGuiModule
    {
        UNodeGraph mGraph = null;
        List<UNodeGraph> mGraphInherit { get; } = new List<UNodeGraph>();
        Vector2 DrawOffset;
        public void SetGraph(UNodeGraph graph)
        {
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
        public void OnDraw()
        {
            if (mGraph == null)
                return;
            for (int i = 0; i < mGraphInherit.Count; i++)
            {
                if (i != 0)
                {
                    ImGuiAPI.SameLine(0, -1);
                }
                if (ImGuiAPI.Button(mGraphInherit[i].GraphName))
                {
                    mGraph.ChangeGraph(mGraphInherit[i]);
                }
                if (i < mGraphInherit.Count - 1)
                {
                    ImGuiAPI.SameLine(0, -1);
                    ImGuiAPI.Text("/");
                }
            }
            if (ImGuiAPI.BeginChild("Graph", in Vector2.Zero, false, ImGuiWindowFlags_.ImGuiWindowFlags_NoMove))
            {
                var vpMin = ImGuiAPI.GetWindowContentRegionMin();
                var vpMax = ImGuiAPI.GetWindowContentRegionMax();

                Vector2 sz = new Vector2(-1,-1);
                //ImGuiAPI.InvisibleButton("ClientContent", sz, ImGuiButtonFlags_.ImGuiButtonFlags_None);
                sz.X = vpMax.X - vpMin.X;
                sz.Y = vpMax.Y - vpMin.Y;

                var winPos = ImGuiAPI.GetWindowPos();
                DrawOffset.SetValue(winPos.X + vpMin.X, winPos.Y + vpMin.Y);
                var pt = ImGuiAPI.GetMousePos();
                var screenPt = mGraph.ToScreenPos(pt.X - DrawOffset.X, pt.Y - DrawOffset.Y);
                ProcessMouse(in screenPt);

                var cmd = ImGuiAPI.GetWindowDrawList();
                if (mGraph.PhysicalSizeVP.X != sz.X || mGraph.PhysicalSizeVP.Y != sz.Y)
                {
                    mGraph.SetPhysicalSizeVP(sz.X, sz.Y);
                }
                foreach (var i in mGraph.Nodes)
                {
                    if (mGraph.IsInViewport(i))
                    {
                        DrawNode(cmd, i);
                    }
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

                if (mGraph.LinkingOp.StartPin != null)
                {
                    var mPos = ImGuiAPI.GetMousePos();
                }

                var styles = EGui.Controls.NodeGraph.NodeGraphStyles.DefaultStyles;
                mGraph.OnDrawAfter(this, styles, cmd);
                DrawPopMenu();
            }
            ImGuiAPI.EndChild();
        }

        void ProcessMouse(in Vector2 screenPt)
        {
            if (ImGuiAPI.IsWindowFocused(ImGuiFocusedFlags_.ImGuiFocusedFlags_ChildWindows) == false)
                return;
            if (ImGuiAPI.IsWindowHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_ChildWindows))
            {
                mGraph.Zoom(screenPt, ImGuiAPI.GetIO().MouseWheel);
            }
            mGraph.PressDrag(in screenPt);

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
        public void DrawImage(ImDrawList cmdlist, EGui.UUvAnim icon, in Vector2 rcMin, in Vector2 rcMax)
        {
            cmdlist.AddRectFilled(in rcMin, in rcMax, icon.Color, 0, ImDrawFlags_.ImDrawFlags_RoundCornersAll);
        }
        public Vector2 CanvasToDraw(in Vector2 pos)
        {
            return mGraph.CanvasToViewport(in pos) + DrawOffset;
        }
        public void DrawNode(ImDrawList cmdlist, UNodeBase node)
        {
            var styles = EGui.Controls.NodeGraph.NodeGraphStyles.DefaultStyles;
            var start = CanvasToDraw(node.Position);
            var end = CanvasToDraw(node.Position + node.Size);

            ImGuiAPI.SetWindowFontScale(1.0f / node.ParentGraph.ScaleVP);

            cmdlist.AddRectFilled(in start, in end, node.BackColor, 0, 0);
            var endTitle = mGraph.CanvasToViewport(node.Position + new Vector2(node.Size.X, node.TitleHeight)) + DrawOffset;
            cmdlist.AddRectFilled(in start, in endTitle, node.TitleColor, 0, 0);

            {//DrawTitle
                var curStart = node.Position;
                {//Draw Node Icon

                    curStart.X += styles.IconOffset.X;
                    var drawStart = CanvasToDraw(curStart);
                    var drawEnd = CanvasToDraw(curStart + node.Icon.Size);
                    DrawImage(cmdlist, node.Icon, in drawStart, in drawEnd);

                    curStart.X += node.Icon.Size.X;
                }

                //Draw Node Name
                {
                    var drawStart = CanvasToDraw(curStart);
                    cmdlist.AddText(in drawStart, 0xFFFFFFFF, node.Name, null);
                }
            }

            {//Draw Preview
                Vector2 prevStart;
                prevStart.X = node.Position.X + (node.Size.X - node.PrevSize.X) * 0.5f;
                prevStart.Y = node.Position.Y + node.TitleHeight;
                var start1 = CanvasToDraw(in prevStart);
                var end1 = CanvasToDraw(prevStart + node.PrevSize);
                node.OnPreviewDraw(in start1, in end1, cmdlist);
            }

            if (node.Selected)
            {//Draw Selected Rect
                cmdlist.AddRect(in start, in end, 0xFFFF00FF, 3, ImDrawFlags_.ImDrawFlags_None, 1.0f);
            }

            var nodeStart = CanvasToDraw(node.Position);
            var nodeEnd = CanvasToDraw(node.Position + node.Size);
            foreach (var i in node.Inputs)
            {
                start = CanvasToDraw(i.Position);
                end = CanvasToDraw(i.Position + i.Size);

                var nameSize = UNodeBase.CalcTextSize(i.Name);
                var textPos = new Vector2(nodeStart.X - nameSize.X - styles.PinPadding - styles.PinInStyle.TextOffset, start.Y);
                if(!string.IsNullOrEmpty(i.Name))
                    cmdlist.AddText(textPos, 0xFFFFFFFF, i.Name, null);
                if (i.Link != null && i.Link.Icon != null)
                {
                    DrawImage(cmdlist, i.Link.Icon, start, end);
                }
                else
                {
                    DrawImage(cmdlist, styles.PinInStyle.Image, start, end);
                }
            }

            foreach (var i in node.Outputs)
            {
                start = CanvasToDraw(i.Position);
                end = CanvasToDraw(i.Position + i.Size);

                var textPos = new Vector2(nodeEnd.X + styles.PinPadding + styles.PinInStyle.TextOffset, start.Y);
                if (!string.IsNullOrEmpty(i.Name))
                    cmdlist.AddText(textPos, 0xFFFFFFFF, i.Name, null);
                if (i.Link != null && i.Link.Icon != null)
                {
                    DrawImage(cmdlist, i.Link.Icon, start, end);
                }
                else
                {
                    DrawImage(cmdlist, styles.PinInStyle.Image, start, end);
                }
            }

            if (mGraph.LinkingOp.StartPin != null)
            {
                DrawLinkingOp(cmdlist);
            }

            node.OnAfterDraw(styles, cmdlist);
            ImGuiAPI.SetWindowFontScale(1.0f);
        }
        public void DrawLinker(ImDrawList cmdlist, UPinLinker linker)
        {
            var p1_v = linker.OutPin.Position + linker.OutPin.Size * 0.5f;
            var p4_v = linker.InPin.Position + linker.InPin.Size * 0.5f;
            var p1 = new Vector2(p1_v.X, p1_v.Y);
            var p4 = new Vector2(p4_v.X, p4_v.Y);
            var delta = p4_v - p1_v;
            //auto delta = ImVec2(delta_v.GetX(), delta_v.GetY());
            delta.X = Math.Abs(delta.X);

            var p2 = new Vector2(p1.X + delta.X * 0.5f, p1.Y);
            var p3 = new Vector2(p4.X - delta.X * 0.5f, p4.Y);

            var styles = EGui.Controls.NodeGraph.NodeGraphStyles.DefaultStyles;
            int num_segs = (int)(delta.Length() / styles.BezierPixelPerSegement + 1);

            var lineColor = styles.LinkerColor;
            if (linker.OutPin.Link != null)
                lineColor = linker.OutPin.Link.LineColor;
            else if (linker.InPin.Link != null)
                lineColor = linker.InPin.Link.LineColor;

            var thinkness = styles.LinkerThinkness;
            if (linker.OutPin.Link != null)
                thinkness = linker.OutPin.Link.LineThinkness;
            else if (linker.InPin.Link != null)
                thinkness = linker.InPin.Link.LineThinkness;

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
            var styles = EGui.Controls.NodeGraph.NodeGraphStyles.DefaultStyles;
            var LinkingOp = mGraph.LinkingOp;
            var p4 = mGraph.LinkingOp.BlockingEnd;
            Vector2 p1;
            float ControlLength = 0;
            if (LinkingOp.StartPin.GetType() == typeof(PinIn))
            {
                p1 = LinkingOp.StartPin.Position + LinkingOp.StartPin.Size * 0.5f;
                var delta = p4 - p1;
                delta.X = Math.Abs(delta.X);
                ControlLength = delta.X * 0.5f;
            }
            else
            {
                p1 = LinkingOp.StartPin.Position + LinkingOp.StartPin.Size * 0.5f;
                var delta = p4 - p1;
                delta.X = Math.Abs(delta.X);
                ControlLength = delta.X * (-0.5f);
            }
            var p2 = new Vector2(p1.X - ControlLength, p1.Y);
            var p3 = new Vector2(p4.X + ControlLength, p4.Y);

            if (LinkingOp.HoverPin != null)
            {
                var min = LinkingOp.HoverPin.Position;
                Vector2 max;
                max = min + LinkingOp.HoverPin.Size;
                min += DrawOffset;
                max += DrawOffset;
                cmdlist.AddRect(in min, in max, styles.HighLightColor, 0, ImDrawFlags_.ImDrawFlags_RoundCornersAll, 2);
            }

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
        string mCanvasMenuFilterStr = "";
        public void DrawPopMenu()
        {
            if (mGraph.CurMenuType == UNodeGraph.EGraphMenu.None)
                return;
            var styles = EGui.Controls.NodeGraph.NodeGraphStyles.DefaultStyles;
            
            if (ImGuiAPI.BeginPopupContextWindow(null, ImGuiPopupFlags_.ImGuiPopupFlags_MouseButtonRight))
            {
                switch (mGraph.CurMenuType)
                {
                    case UNodeGraph.EGraphMenu.Canvas:
                        {
                            mGraph.OnBeforeDrawMenu(styles);
                            var width = ImGuiAPI.GetColumnWidth(0);
                            var drawList = ImGuiAPI.GetWindowDrawList();
                            EGui.UIProxy.SearchBarProxy.OnDraw(ref mCanvasMenuFilterFocused, in drawList, "search item", ref mCanvasMenuFilterStr, width);
                            if(mGraph.CanvasMenuDirty)
                                mGraph.UpdateCanvasMenus();
                            for(var childIdx = 0; childIdx < mGraph.CanvasMenus.SubMenuItems.Count; childIdx++)
                                DrawMenu(mGraph.CanvasMenus.SubMenuItems[childIdx], mCanvasMenuFilterStr.ToLower());
                            mGraph.OnAfterDrawMenu(styles);
                        }
                        break;
                    case UNodeGraph.EGraphMenu.Node:
                        for(var childIdx = 0; childIdx < mGraph.NodeMenus.SubMenuItems.Count; childIdx++)
                            DrawMenu(mGraph.NodeMenus.SubMenuItems[childIdx], mCanvasMenuFilterStr.ToLower());
                        break;
                    case UNodeGraph.EGraphMenu.Pin:
                        {
                            for(var childIdx = 0; childIdx < mGraph.PinMenus.SubMenuItems.Count; childIdx++)
                                DrawMenu(mGraph.PinMenus.SubMenuItems[childIdx], mCanvasMenuFilterStr.ToLower());
                            var pressPin = mGraph.PopMenuPressObject as NodePin;
                            if (pressPin != null)
                            {
                                pressPin.HostNode.OnShowPinMenu(pressPin);
                            }
                        }
                        break;
                    default:
                        break;
                };
                ImGuiAPI.EndPopup();
            }
            else
            {
                mGraph.CurMenuType = UNodeGraph.EGraphMenu.None;
            }
        }
        public void DrawMenu(UMenuItem item, string filter = "")
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
                    ImGuiAPI.TreeNodeEx(item.Text, ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Leaf | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_NoTreePushOnOpen);
                    if (ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left))
                    {
                        if (item.Action != null)
                        {
                            item.Action(item, mGraph.PopMenuPressObject);
                            ImGuiAPI.CloseCurrentPopup();
                        }
                    }
                }
            }
            else
            {
                if(ImGuiAPI.TreeNode(item.Text))
                {
                    for(int menuIdx = 0; menuIdx < item.SubMenuItems.Count; menuIdx++)
                    {
                        DrawMenu(item.SubMenuItems[menuIdx], filter);
                    }
                    ImGuiAPI.TreePop();
                }
            }
        }
    }
}
