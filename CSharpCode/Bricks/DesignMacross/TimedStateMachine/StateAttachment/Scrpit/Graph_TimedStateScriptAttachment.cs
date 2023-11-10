using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Editor;
using EngineNS.DesignMacross.Base.Render;
using EngineNS.DesignMacross.Design;
using EngineNS.Rtti;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.DesignMacross.TimedStateMachine.StateAttachment.Scrpit
{
    [ImGuiElementRender(typeof(TtGraph_TimedStateScriptAttachmentRender))]
    public class TtGraph_TimedStateScriptAttachment : TtGraph, IContextMeunable
    {
        public TtTimedStateScriptAttachmentClassDescription ScriptAttachmentClassDescription { get => Description as TtTimedStateScriptAttachmentClassDescription; }
        public TtCommandHistory CommandHistory { get; set; } = new TtCommandHistory();
   
        public TtGraph_TimedStateScriptAttachment(IDescription description) : base(description)
        {

        }
        #region IContextMeunable

        public override void ConstructContextMenu(ref FGraphElementRenderingContext context, TtPopupMenu PopupMenu)
        {
            PopupMenu.Reset();
        }
        #endregion IContextMeunable
        public void Construct()
        {
            //Elements.Clear();
            //{
            //    var graphElementAttribute = GraphElementAttribute.GetAttributeWithSpecificClassType<IGraphElement>(ScriptAttachmentClassDescription.OnBeginDesc.GetType());
            //    var instance = UTypeDescManager.CreateInstance(graphElementAttribute.ClassType, new object[] { ScriptAttachmentClassDescription.OnBeginDesc }) as IGraphElement;
            //    instance.Parent = this;
            //    instance.Description = ScriptAttachmentClassDescription.OnBeginDesc;
            //    instance.Construct();
            //    Elements.Add(instance);
            //}
        }

        public override void ConstructElements(ref FGraphRenderingContext context)
        {
            
        }
    }
    public class TtGraph_TimedStateScriptAttachmentRender : IGraphRender
    {
        TtPopupMenu PopupMenu { get; set; } = new TtPopupMenu("Graph_TimedStateScriptAttachmentRender");
        public void Draw(IRenderableElement renderableElement, ref FGraphRenderingContext context)
        {
            var graph = renderableElement as TtGraph_TimedStateScriptAttachment;
            if (graph == null)
                return;

            if (ImGuiAPI.BeginChild(graph.Name + "_SubGraph", in Vector2.Zero, false, ImGuiWindowFlags_.ImGuiWindowFlags_NoMove | ImGuiWindowFlags_.ImGuiWindowFlags_NoScrollbar | ImGuiWindowFlags_.ImGuiWindowFlags_NoScrollWithMouse))
            {
                var cmd = ImGuiAPI.GetWindowDrawList();

                Vector2 sz = ImGuiAPI.GetWindowContentRegionMax() - ImGuiAPI.GetWindowContentRegionMin();
                var winPos = ImGuiAPI.GetWindowPos();
                // initialize
                graph.Size = new SizeF(sz.X, sz.Y);
                graph.ViewPort.Location = winPos;
                graph.ViewPort.Size = new SizeF(sz.X, sz.Y);
                graph.Camera.Size = new SizeF(sz.X, sz.Y);

                context.CommandHistory = graph.CommandHistory;
                context.ViewPort = graph.ViewPort;
                context.Camera = graph.Camera;
                //

                FGraphElementRenderingContext elementRenderingContext = default;
                elementRenderingContext.Camera = context.Camera;
                elementRenderingContext.ViewPort = context.ViewPort;
                elementRenderingContext.CommandHistory = graph.CommandHistory;
                elementRenderingContext.EditorInteroperation = context.EditorInteroperation;

                TtGraphElement_GridLine grid = new TtGraphElement_GridLine();
                grid.Size = new SizeF(sz.X, sz.Y);
                var gridRender = TtElementRenderDevice.CreateGraphElementRender(grid);
                if (gridRender != null)
                    gridRender.Draw(grid, ref elementRenderingContext);

                if (graph is IContextMeunable meunableGraph)
                {
                    meunableGraph.SetContextMenuableId(PopupMenu);
                    if (ImGuiAPI.IsMouseClicked(ImGuiMouseButton_.ImGuiMouseButton_Right, false)
                        && context.ViewPort.IsInViewport(ImGuiAPI.GetMousePos()))
                    {
                        var pos = context.ViewPortInverseTransform(ImGuiAPI.GetMousePos());
                        if (graph.HitCheck(pos))
                        {
                            ImGuiAPI.CloseCurrentPopup();
                            meunableGraph.ConstructContextMenu(ref elementRenderingContext, PopupMenu);
                            PopupMenu.OpenPopup();
                        }
                    }

                    PopupMenu.Draw(ref elementRenderingContext);
                }

                foreach (var element in graph.Elements)
                {
                    var elementRender = TtElementRenderDevice.CreateGraphElementRender(element);
                    if (elementRender != null)
                    {
                        if (element is ILayoutable layoutable)
                        {
                            var size = layoutable.Measuring(new SizeF());
                            layoutable.Arranging(new Rect(element.Location, size));
                        }
                        elementRender.Draw(element, ref elementRenderingContext);
                    }
                }
                TtMouseEventProcesser.Instance.Processing(graph, ref elementRenderingContext);
            }
            ImGuiAPI.EndChild();
        }
    }
}
