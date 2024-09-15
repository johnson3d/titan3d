using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Editor.GraphPanel;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.Rtti;

namespace EngineNS.DesignMacross.Editor
{
    public class TtGraphEditPanel
    {
        public bool AlreadyNavigated = true;
        public TtNavigableGraphsPanel ActiveGraphNavigatedPanel { get; set; } = null;

        public Dictionary<IDescription, TtNavigableGraphsPanel> OpenedNavigableGraphsPanels = new();
        [Rtti.Meta]
        public List<IGraph> Graphs { get; set; } = new List<IGraph>();
        public void EditGraph(IDescription description)
        {
            if (OpenedNavigableGraphsPanels.ContainsKey(description))
            {
                ActiveGraphNavigatedPanel = OpenedNavigableGraphsPanels[description];
            }
            else
            {
                var graphAttribute = GraphAttribute.GetAttributeWithSpecificClassType<IGraph>(description.GetType());
                System.Diagnostics.Debug.Assert(graphAttribute != null);
                var graph = TtTypeDescManager.CreateInstance(graphAttribute.ClassType, new object[] { description }) as IGraph;
                var navigableGraphsPanel = new TtNavigableGraphsPanel(graph);
                OpenedNavigableGraphsPanels.Add(description, navigableGraphsPanel);
                ActiveGraphNavigatedPanel = navigableGraphsPanel;
            }
            AlreadyNavigated = false;
        }
        public void Draw(FDesignMacrossEditorRenderingContext context)
        {
            TtGraphPanelRender render = new TtGraphPanelRender();
            render.Draw(this, context);
        }
    }
    public struct TtGraphPanelRender
    {
        public void Draw(TtGraphEditPanel graphEditPanel, FDesignMacrossEditorRenderingContext context)
        {
            //Macross.UMacrossBreak mBreakerStore = null;
            var vMin = ImGuiAPI.GetWindowContentRegionMin();
            var vMax = ImGuiAPI.GetWindowContentRegionMax();
            IDescription PanelKeyWillBeRemoved = null;
            if (ImGuiAPI.BeginTabBar("GraphTab", ImGuiTabBarFlags_.ImGuiTabBarFlags_None))
            {
                var itMax = ImGuiAPI.GetItemRectSize();
                vMin.Y += itMax.Y;
                var sz = vMax - vMin;
                foreach (var ui in graphEditPanel.OpenedNavigableGraphsPanels)
                {

                    var flag = ImGuiTabItemFlags_.ImGuiTabItemFlags_None;
                    if(!graphEditPanel.AlreadyNavigated)
                    {
                        if (graphEditPanel.ActiveGraphNavigatedPanel == ui.Value)
                        {
                            flag |= ImGuiTabItemFlags_.ImGuiTabItemFlags_SetSelected;
                        }
                    }
                    
                    bool showTab = true;
                    if (ImGuiAPI.BeginTabItem(ui.Key.Name, ref showTab, flag))
                    {
                        if(graphEditPanel.AlreadyNavigated)
                        {
                            graphEditPanel.ActiveGraphNavigatedPanel = ui.Value;
                        }
                        else
                        {
                            if(graphEditPanel.ActiveGraphNavigatedPanel == ui.Value)
                            {
                                graphEditPanel.AlreadyNavigated = true;
                            }
                        }
                        var graphContext = new FGraphRenderingContext();
                        graphContext.CommandHistory = context.CommandHistory;
                        graphContext.EditorInteroperation = context.EditorInteroperation;
                        graphContext.GraphElementStyleManager = context.GraphElementStyleManager;
                        graphContext.DescriptionsElement = context.DescriptionsElement;
                        graphContext.DesignedClassDescription = context.DesignedClassDescription;
                        var render = new TtNavigableGraphsPanelRender();
                        render.Draw(ui.Value, context);
                        ImGuiAPI.EndTabItem();
                    }
                    if(!showTab)
                    {
                        PanelKeyWillBeRemoved = ui.Key;
                    }
                }
                ImGuiAPI.EndTabBar();
            }
            if(PanelKeyWillBeRemoved != null)
            {
                graphEditPanel.OpenedNavigableGraphsPanels.Remove(PanelKeyWillBeRemoved);
            }
        }
    }
}
