using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Render;
using EngineNS.Rtti;

namespace EngineNS.DesignMacross.Editor.GraphPanel
{
    public class TtNavigableGraphsPanel
    {
        public TtNavigableGraphsPanel(IGraph graph)
        {
            MainGraph = graph;
            Navigation.Push(graph);
        }
        //the graph that opened from outline
        public IGraph MainGraph { get; } = null;
        public Dictionary<IDescription, IGraph> SubGraphs { get; set; } = new();
        public Stack<IGraph> Navigation { get; set; } = new();
        public void OpenSubGraph(IDescription description)
        {
            if (SubGraphs.TryGetValue(description, out var subGraph))
            {
                Navigation.Push(subGraph);
            }
            else
            {
                var graphAttribute = GraphAttribute.GetAttributeWithSpecificClassType<IGraph>(description.GetType());
                var graph = TtTypeDescManager.CreateInstance(graphAttribute.ClassType, new object[] { description }) as IGraph;
                SubGraphs.Add(description, graph);
                Navigation.Push(graph);
            }
        }
    }

    public struct TtNavigableGraphsPanelRender
    {
        public void Draw(TtNavigableGraphsPanel navigableGraphsPanel, FDesignMacrossEditorRenderingContext context)
        {
            if (ImGuiAPI.Button("<"))
            {
                if (navigableGraphsPanel.Navigation.Count > 1)
                {
                    navigableGraphsPanel.Navigation.Pop();
                }
            }
            ImGuiAPI.SameLine(0, 5);

            foreach (var graph in navigableGraphsPanel.Navigation.ToArray().Reverse())
            {
                ImGuiAPI.SameLine(0, -1);
                ImGuiAPI.Text("/");
                ImGuiAPI.SameLine(0, -1);
                if (ImGuiAPI.Button(graph.Name))
                {
                    while(navigableGraphsPanel.Navigation.Peek().Name != graph.Name)
                    {
                        navigableGraphsPanel.Navigation.Pop();
                    }
                }
            }

            var graphContext = new FGraphRenderingContext();
            graphContext.CommandHistory = context.CommandHistory;
            graphContext.EditorInteroperation = context.EditorInteroperation;
            graphContext.GraphElementStyleManager = context.GraphElementStyleManager;
            graphContext.DescriptionsElement = context.DescriptionsElement;
            graphContext.DesignedClassDescription = context.DesignedClassDescription;
            var currentRenderingGraph = navigableGraphsPanel.Navigation.Peek();
            currentRenderingGraph.ConstructElements(ref graphContext);
            currentRenderingGraph.AfterConstructElements(ref graphContext);
            var graphRender = TtElementRenderDevice.CreateGraphRender(currentRenderingGraph);
            if (graphRender != null)
            {
                if (currentRenderingGraph is ILayoutable layoutable)
                {
                    var desireSize = layoutable.Measuring(new SizeF());
                    layoutable.Arranging(new Rect(Vector2.Zero, desireSize));
                }
                graphRender.Draw(currentRenderingGraph, ref graphContext);
            }
        }
    }
}
