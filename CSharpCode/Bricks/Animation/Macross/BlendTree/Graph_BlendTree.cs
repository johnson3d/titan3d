using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Render;
using EngineNS.DesignMacross.Editor;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation.Macross.BlendTree
{
    [ImGuiElementRender(typeof(TtGraph_BlendTreeRender))]
    public class TtGraph_BlendTree : TtGraph
    {
        public TtGraph_BlendTree(IDescription description) : base(description)
        {
            Description = description;

        }

        public override void ConstructElements(ref FGraphRenderingContext context)
        {
            Elements.Clear();
            FGraphElementRenderingContext elementRenderingContext = default;
            elementRenderingContext.Camera = context.Camera;
            elementRenderingContext.ViewPort = context.ViewPort;
            elementRenderingContext.CommandHistory = context.CommandHistory;
            elementRenderingContext.EditorInteroperation = context.EditorInteroperation;
            elementRenderingContext.GraphElementStyleManager = context.GraphElementStyleManager;
            elementRenderingContext.DescriptionsElement = context.DescriptionsElement;

            
            foreach (var element in Elements)
            {
                element.ConstructElements(ref elementRenderingContext);
            }
        }
        public override void AfterConstructElements(ref FGraphRenderingContext context)
        {

        }

        #region IContextMeunable

        public override void ConstructContextMenu(ref FGraphElementRenderingContext context, TtPopupMenu popupMenu)
        {
            popupMenu.bHasSearchBox = true;
            var cmdHistory = context.CommandHistory;
            var graphElementStyleManager = context.GraphElementStyleManager;
            //for now just put here, util we have the init method
           

        }
        public string GetValidNodeName(string name)
        {
            int index = 0;
            var newName = name;
            while (Elements.Find((element) =>
            {
                return element.Name == newName;
            }) != null)
            {
                index++;
                newName = name + "_" + index;
            }
            return newName;
        }
        #endregion IContextMeunable
    }
    public class TtGraph_BlendTreeRender : IGraphRender
    {
        public void Draw(IRenderableElement renderableElement, ref FGraphRenderingContext context)
        {
            var graph = renderableElement as TtGraph_BlendTree;
            if (graph == null)
                return;

            if (ImGuiAPI.BeginChild(graph.Name + "_Graph", in Vector2.Zero, false, ImGuiWindowFlags_.ImGuiWindowFlags_NoMove | ImGuiWindowFlags_.ImGuiWindowFlags_NoScrollbar | ImGuiWindowFlags_.ImGuiWindowFlags_NoScrollWithMouse))
            {
                var cmd = ImGuiAPI.GetWindowDrawList();

                Vector2 sz = ImGuiAPI.GetWindowContentRegionMax() - ImGuiAPI.GetWindowContentRegionMin();
                var winPos = ImGuiAPI.GetWindowPos();
                // initialize
                graph.Size = new SizeF(sz.X, sz.Y);
                graph.ViewPort.Location = winPos;
                graph.ViewPort.Size = new SizeF(sz.X, sz.Y);
                graph.Camera.Size = new SizeF(sz.X, sz.Y);

                graph.CommandHistory = context.CommandHistory;
                context.ViewPort = graph.ViewPort;
                context.Camera = graph.Camera;
                //

                FGraphElementRenderingContext elementRenderingContext = default;
                elementRenderingContext.Camera = context.Camera;
                elementRenderingContext.ViewPort = context.ViewPort;
                elementRenderingContext.CommandHistory = graph.CommandHistory;
                elementRenderingContext.EditorInteroperation = context.EditorInteroperation;
                elementRenderingContext.GraphElementStyleManager = context.GraphElementStyleManager;
                elementRenderingContext.DescriptionsElement = context.DescriptionsElement;
                elementRenderingContext.DesignedGraph = graph;

                TtGraphElement_GridLine grid = new TtGraphElement_GridLine();
                grid.Size = new SizeF(sz.X, sz.Y);
                var gridRender = TtElementRenderDevice.CreateGraphElementRender(grid);
                if (gridRender != null)
                    gridRender.Draw(grid, ref elementRenderingContext);

                foreach (var element in graph.Elements)
                {
                    if (element is ILayoutable layoutable)
                    {
                        var size = layoutable.Measuring(new SizeF());
                        layoutable.Arranging(new Rect(element.Location, size));
                    }
                }
                foreach (var element in graph.Elements)
                {
                    var elementRender = TtElementRenderDevice.CreateGraphElementRender(element);
                    if (elementRender != null)
                    {
                        elementRender.Draw(element, ref elementRenderingContext);
                    }
                }
                TtMouseEventProcesser.Instance.Processing(graph, ref elementRenderingContext);
                TtContextMenuHandler.Instance.HandleContextMenu(TtMouseEventProcesser.Instance.LastElement, ref elementRenderingContext);
            }
            ImGuiAPI.EndChild();
        }
    }
}
