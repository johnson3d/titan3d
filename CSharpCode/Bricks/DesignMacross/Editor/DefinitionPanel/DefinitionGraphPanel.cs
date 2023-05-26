using EngineNS.Bricks.CodeBuilder;
using EngineNS.Bricks.NodeGraph;
using EngineNS.DesignMacross.Description;
using EngineNS.DesignMacross.Editor.DeclarationPanel;
using EngineNS.DesignMacross.Graph;
using EngineNS.DesignMacross.Graph;
using EngineNS.DesignMacross.Outline;
using EngineNS.DesignMacross.Render;
using EngineNS.Rtti;
using Microsoft.CodeAnalysis.Differencing;
using SixLabors.Fonts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace EngineNS.DesignMacross.Editor
{
    public class TtDefinitionGraphPanel
    {
        public IGraph CurrentDefinitionGraph = null;

        public Dictionary<IDescription, IGraph> DefinitionGraphs = new Dictionary<IDescription, IGraph>();
        [Rtti.Meta]
        public List<IGraph> Graphs { get; set; } = new List<IGraph>();
        public void EditDefinitionGraph(IDescription description)
        {
            if (DefinitionGraphs.ContainsKey(description))
            {
                CurrentDefinitionGraph = DefinitionGraphs[description];
            }
            else
            {
                var att = description.GetType().GetCustomAttribute(typeof(GraphElementAttribute), false);
                if (att != null)
                {
                    if (att is GraphElementAttribute graphElementAttr)
                    {
                        var graph = UTypeDescManager.CreateInstance(graphElementAttr.ClassType, new object[] { description }) as IGraph;
                        graph.Construct();
                        DefinitionGraphs.Add(description, graph);
                        CurrentDefinitionGraph = graph;
                    }
                }
            }
        }
        public void Draw(FDesignMacrossEditorRenderingContext context)
        {
            TtDefinitionGraphPanelRender render = new TtDefinitionGraphPanelRender();
            render.Draw(this, context);
        }
    }
    //public class TtDefinitionGraphPanelRender
    public struct TtDefinitionGraphPanelRender
    {
        public void Draw(TtDefinitionGraphPanel definitionGraphPanel, FDesignMacrossEditorRenderingContext context)
        {
            Macross.UMacrossBreak mBreakerStore = null;
            var vMin = ImGuiAPI.GetWindowContentRegionMin();
            var vMax = ImGuiAPI.GetWindowContentRegionMin();
            if (ImGuiAPI.BeginTabBar("GraphTab", ImGuiTabBarFlags_.ImGuiTabBarFlags_None))
            {
                var itMax = ImGuiAPI.GetItemRectSize();
                vMin.Y += itMax.Y;
                var sz = vMax - vMin;
                bool breakerChanged = false;
                if (mBreakerStore != Macross.UMacrossDebugger.Instance.CurrrentBreak)
                {
                    mBreakerStore = Macross.UMacrossDebugger.Instance.CurrrentBreak;
                    breakerChanged = true;
                }
                foreach (var ui in definitionGraphPanel.DefinitionGraphs)
                {
                    if (breakerChanged)
                    {
                        //for (int linkerIdx = 0; linkerIdx < graph.Linkers.Count; linkerIdx++)
                        //{
                        //    graph.Linkers[linkerIdx].InDebuggerLine = false;
                        //}
                        //if (mBreakerStore != null)
                        //    graph.GraphRenderer.BreakerName = mBreakerStore.BreakName;
                        //else
                        //    graph.GraphRenderer.BreakerName = "";
                    }
                    var flag = ImGuiTabItemFlags_.ImGuiTabItemFlags_None;
                    bool visibleGraph = definitionGraphPanel.CurrentDefinitionGraph == ui.Value;
                    if (visibleGraph)
                    {
                        flag |= ImGuiTabItemFlags_.ImGuiTabItemFlags_SetSelected;
                    }
                    bool showTab = true;
                    if (ImGuiAPI.BeginTabItem(ui.Key.Name, ref showTab, flag))
                    {
                        definitionGraphPanel.CurrentDefinitionGraph = ui.Value;
                        var graphContext = new FGraphRenderingContext();
                        graphContext.CommandHistory = context.CommandHistory;
                        graphContext.EditorInteroperation = context.EditorInteroperation;
                        var graphRender = TtElementRenderDevice.CreateGraphRender(ui.Value);
                        if (graphRender != null)
                        {
                            if (ui.Value is ILayoutable layoutable)
                            {
                                var desireSize = layoutable.Measuring(new SizeF());
                                layoutable.Arranging(new Rect(Vector2.Zero, desireSize));
                            }
                            graphRender.Draw(ui.Value, ref graphContext);
                        }
                        ImGuiAPI.EndTabItem();
                    }
                }
                ImGuiAPI.EndTabBar();
            }
        }
    }
}
