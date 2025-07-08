using EngineNS.Graphics.Pipeline;
using System;
using System.Collections.Generic;
using EngineNS.GamePlay.Scene;
using EngineNS.Thread.Async;
using EngineNS.GamePlay;
using System.ComponentModel;

namespace EngineNS.Bricks.AdvanceShadow
{
    public partial class TtQTree
    {
        public struct FStats
        {
            public int Node;
            public int Tile;
        }
        public void DrawQTree(TtAdvanceShadowMapNode graphNode, ImDrawList cmdlist, in Vector2 drawSize, in Vector2 DrawOffset, ref FStats stats)
        {
            DrawQTree(graphNode, Root, cmdlist, in drawSize, in DrawOffset, ref stats);
        }
        private void DrawQTree(TtAdvanceShadowMapNode graphNode, TtQNode node, ImDrawList cmdlist, in Vector2 drawSize, in Vector2 DrawOffset, ref FStats stats)
        {
            var size = Root.AABB.GetSize();
            var offsetMin = node.AABB.Minimum - Root.AABB.Minimum;
            var offsetMax = node.AABB.Maximum - Root.AABB.Minimum;
            var min = new Vector2((float)(offsetMin.X / size.X), (float)(offsetMin.Y / size.Y)) * drawSize + DrawOffset;
            var max = new Vector2((float)(offsetMax.X / size.X), (float)(offsetMax.Y / size.Y)) * drawSize + DrawOffset;

            if (node.NodeType == TtQNode.ENodeType.Node && node.Child00 != null)
            {
                var level = (byte)(node.DeepLevel * 255 / MaxDeepLevel);
                var color = new Color4b(level, level, level, 255);
                cmdlist.AddRect(in min, in max, color.ToAbgr(), 0.0f, ImDrawFlags_.ImDrawFlags_None, 1.0f);
                stats.Node++;

                DrawQTree(graphNode, node.Child00, cmdlist, in drawSize, in DrawOffset, ref stats);
                DrawQTree(graphNode, node.Child01, cmdlist, in drawSize, in DrawOffset, ref stats);
                DrawQTree(graphNode, node.Child10, cmdlist, in drawSize, in DrawOffset, ref stats);
                DrawQTree(graphNode, node.Child11, cmdlist, in drawSize, in DrawOffset, ref stats);
            }
            else if (node.NodeType == TtQNode.ENodeType.DeathNode)
            {
                return;
            }
            else
            {
                if (node.ShadowObjects.Count > 0 && graphNode.mDebuggerSRViews != null && node.Leaf.PageIndex < graphNode.mDebuggerSRViews.Length)
                {
                    var srv = graphNode.mDebuggerSRViews[node.Leaf.PageIndex];
                    ImTextureRef imTextureRef = new ImTextureRef();
                    imTextureRef.m__TexID = (ulong)srv.GetTextureHandle();
                    cmdlist.AddImage(imTextureRef, in min, in max, in Vector2.Zero, in Vector2.One, 0xffffffff);
                }
                var level = (byte)(node.DeepLevel * 255 / MaxDeepLevel);
                var color = new Color4b(level, level, level, 255);
                cmdlist.AddRect(in min, in max, color.ToAbgr(), 0.0f, ImDrawFlags_.ImDrawFlags_None, 1.0f);
                //cmdlist.AddText(in min, Color4b.White.ToAbgr(), node.PageIndex.ToString(), null);
                stats.Node++;
                stats.Tile += 1;
                if (ImGuiAPI.IsMouseDown(ImGuiMouseButton_.ImGuiMouseButton_Left))
                {
                    //var clickPos = ImGuiAPI.GetMousePos() - DrawOffset;
                    //if (new BoundingBox2D(min, max).Contains(clickPos) == ContainmentType.Contains)
                    //{
                    //    int xx = node.PageIndex;
                    //}
                }
            }
        }
    }

    [EGui.Controls.PropertyGrid.PGCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    public class TtQTreeVisualDebugger : IRootForm
    {
        public bool Visible { get; set; } = true;
        public uint DockId { get; set; }
        public ImGuiWindowClass DockKeyClass { get; }
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;
        public TtQTreeVisualDebugger()
        {

        }
        public unsafe void Dispose()
        {

        }
        public async Thread.Async.TtTask<bool> Initialize()
        {
            await EngineNS.Thread.TtAsyncDummyClass.DummyFunc();
            return true;
        }
        public TtAdvanceShadowNode mAdanceShadowNode;
        public TtCpuCullingNode mCullingNode;
        [Category("Debug")]
        public TtQTree QTree
        {
            get => mAdanceShadowNode.mShadowMapTree;
        }
        float ScaleFactor = 1.0f;
        Vector2 Offset = Vector2.Zero;
        public void OnDraw()
        {
            var result = EGui.UIProxy.DockProxy.BeginMainForm("Advance Shadow Debugger", this, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (result)
            {
                if (mAdanceShadowNode.mRenderGraphNode != null)
                {
                    var winPos = ImGuiAPI.GetWindowPos();
                    var vpMin = ImGuiAPI.GetWindowContentRegionMin();
                    var vpMax = ImGuiAPI.GetWindowContentRegionMax();
                    var DrawOffset = new Vector2();
                    DrawOffset.SetValue(winPos.X + vpMin.X, winPos.Y + vpMin.Y);
                    DrawOffset += Offset;

                    var cmdlist = ImGuiAPI.GetWindowDrawList();
                    var size = ImGuiAPI.GetWindowSize();
                    float side = MathF.Min(size.X, size.Y);
                    var stats = new TtQTree.FStats();
                    stats.Node = 0;
                    stats.Tile = 0;
                    mAdanceShadowNode.mShadowMapTree.DrawQTree(mAdanceShadowNode.mRenderGraphNode, cmdlist, new Vector2(side, side) * ScaleFactor, in DrawOffset, ref stats);

                    if (mCullingNode != null)
                    {
                        var cameral = mCullingNode.VisParameter.CullCamera;
                        var p = new DVector2(cameral.GetPosition().X, cameral.GetPosition().Z);
                        var pos = (p - mAdanceShadowNode.mShadowMapTree.Root.AABB.Minimum).AsSingleVector();
                        var t = mAdanceShadowNode.mShadowMapTree.Root.AABB.GetSize();
                        pos.X = (float)(pos.X * side / t.X);
                        pos.Y = (float)(pos.Y * side / t.Y);
                        pos += DrawOffset;
                        cmdlist.AddCircle(in pos, 5, Color4b.Red.ToAbgr(), 10, 1);
                    }
                }
            }
            EGui.UIProxy.DockProxy.EndMainForm(result);
        }
    }
}
