using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Editor;
using EngineNS.DesignMacross.Base.Render;
using EngineNS.DesignMacross.Design;
using EngineNS.Rtti;
using System;
using System.Collections.Generic;
using System.Text;
using EngineNS.DesignMacross.TimedStateMachine.StateTransition;

namespace EngineNS.DesignMacross.TimedStateMachine.StateAttachment.Scrpit
{
    public class TtGraph_AttachmentMethod : TtGraph_Method
    {
        public TtGraph_AttachmentMethod(IDescription description) : base(description)
        {

        }
        int VariableCount = 0;
        int MethodCount = 0;
        public override void ConstructElements(ref FGraphRenderingContext context)
        {
            var classDesc = MethodDescription.Parent as TtTimedStateAttachmentClassDescription;
            if (MethodDescription.MethodGraph == null)
            {
                var macrossHolder = new TtTransitionMacrossHolder();
                FClassBuildContext classBuildContext = new() { MainClassDescription = classDesc };
                var classDec = TtDescriptionASTBuildUtil.BuildDefaultPartForClassDeclaration(classDesc, ref classBuildContext);
                macrossHolder.DefClass = classDec;
                MethodDescription.MethodGraph = TtDesignMacrossMethodGraph.CreateGraph(macrossHolder, TtDescriptionASTBuildUtil.BuildDefaultPartForMethodDeclaration(classDesc.TickMethodDescription, ref classBuildContext));
                MethodDescription.MethodGraph.GraphName = MethodDescription.Name;
            }
            if (classDesc.Variables.Count != VariableCount)
            {
                VariableCount = classDesc.Variables.Count;
                MethodDescription.MethodGraph.CanvasMenuDirty = true;
            }
            if (classDesc.Methods.Count != MethodCount)
            {
                MethodCount = classDesc.Methods.Count;
                MethodDescription.MethodGraph.CanvasMenuDirty = true;
            }
            if (MethodDescription.MethodGraph.MacrossEditor == null)
            {
                var macrossHolder = new TtTransitionMacrossHolder();
                FClassBuildContext classBuildContext = new() { MainClassDescription = classDesc };
                var classDec = TtDescriptionASTBuildUtil.BuildDefaultPartForClassDeclaration(classDesc, ref classBuildContext);
                macrossHolder.DefClass = classDec;
                MethodDescription.MethodGraph.SetMacrossEditor(macrossHolder);
            }
        }
    }
    [ImGuiElementRender(typeof(TtGraph_TimedStateScriptAttachmentRender))]
    public class TtGraph_TimedStateScriptAttachment : TtGraph, IContextMeunable
    {
        public TtTimedStateScriptAttachmentClassDescription ScriptAttachmentClassDescription { get => Description as TtTimedStateScriptAttachmentClassDescription; }
        public TtGraph_AttachmentMethod Graph_AttachmentMethod = null;
        public TtCommandHistory CommandHistory { get; set; } = new TtCommandHistory();
   
        public TtGraph_TimedStateScriptAttachment(IDescription description) : base(description)
        {

        }
        //internal int VariableCount = 0;
        //internal int MethodCount = 0;
        public override void ConstructElements(ref FGraphRenderingContext context)
        {
            if (Graph_AttachmentMethod == null)
            {
                Graph_AttachmentMethod = new TtGraph_AttachmentMethod(ScriptAttachmentClassDescription.TickMethodDescription);
            }
            Graph_AttachmentMethod.ConstructElements(ref context);
        }
        #region IContextMeunable

        public override void ConstructContextMenu(ref FGraphElementRenderingContext context, TtPopupMenu PopupMenu)
        {
        }
        #endregion IContextMeunable

    }
    public class TtGraph_TimedStateScriptAttachmentRender : IGraphRender
    {
        TtGraph_MethodRender Graph_MethodRender = new TtGraph_MethodRender();
        TtPopupMenu PopupMenu { get; set; } = new TtPopupMenu("Graph_TimedStateScriptAttachmentRender");
        public void Draw(IRenderableElement renderableElement, ref FGraphRenderingContext context)
        {
            var graph = renderableElement as TtGraph_TimedStateScriptAttachment;
            if (graph == null)
                return;

            
            Graph_MethodRender.Draw(graph.Graph_AttachmentMethod, ref context);

            return;

            //if (ImGuiAPI.BeginChild(graph.Name + "_SubGraph", in Vector2.Zero, false, ImGuiWindowFlags_.ImGuiWindowFlags_NoMove | ImGuiWindowFlags_.ImGuiWindowFlags_NoScrollbar | ImGuiWindowFlags_.ImGuiWindowFlags_NoScrollWithMouse))
            //{
            //    var cmd = ImGuiAPI.GetWindowDrawList();

            //    Vector2 sz = ImGuiAPI.GetWindowContentRegionMax() - ImGuiAPI.GetWindowContentRegionMin();
            //    var winPos = ImGuiAPI.GetWindowPos();
            //    // initialize
            //    graph.Size = new SizeF(sz.X, sz.Y);
            //    graph.ViewPort.Location = winPos;
            //    graph.ViewPort.Size = new SizeF(sz.X, sz.Y);
            //    graph.Camera.Size = new SizeF(sz.X, sz.Y);

            //    context.CommandHistory = graph.CommandHistory;
            //    context.ViewPort = graph.ViewPort;
            //    context.Camera = graph.Camera;
            //    //

            //    FGraphElementRenderingContext elementRenderingContext = default;
            //    elementRenderingContext.Camera = context.Camera;
            //    elementRenderingContext.ViewPort = context.ViewPort;
            //    elementRenderingContext.CommandHistory = graph.CommandHistory;
            //    elementRenderingContext.EditorInteroperation = context.EditorInteroperation;

            //    TtGraphElement_GridLine grid = new TtGraphElement_GridLine();
            //    grid.Size = new SizeF(sz.X, sz.Y);
            //    var gridRender = TtElementRenderDevice.CreateGraphElementRender(grid);
            //    if (gridRender != null)
            //        gridRender.Draw(grid, ref elementRenderingContext);

            //    if (graph is IContextMeunable meunableGraph)
            //    {
            //        meunableGraph.SetContextMenuableId(PopupMenu);
            //        if (ImGuiAPI.IsMouseClicked(ImGuiMouseButton_.ImGuiMouseButton_Right, false)
            //            && context.ViewPort.IsInViewport(ImGuiAPI.GetMousePos()))
            //        {
            //            var pos = context.ViewPortInverseTransform(ImGuiAPI.GetMousePos());
            //            if (graph.HitCheck(pos))
            //            {
            //                ImGuiAPI.CloseCurrentPopup();
            //                meunableGraph.ConstructContextMenu(ref elementRenderingContext, PopupMenu);
            //                PopupMenu.OpenPopup();
            //            }
            //        }

            //        PopupMenu.Draw(ref elementRenderingContext);
            //    }

            //    foreach (var element in graph.Elements)
            //    {
            //        var elementRender = TtElementRenderDevice.CreateGraphElementRender(element);
            //        if (elementRender != null)
            //        {
            //            if (element is ILayoutable layoutable)
            //            {
            //                var size = layoutable.Measuring(new SizeF());
            //                layoutable.Arranging(new Rect(element.Location, size));
            //            }
            //            elementRender.Draw(element, ref elementRenderingContext);
            //        }
            //    }
            //    TtMouseEventProcesser.Instance.Processing(graph, ref elementRenderingContext);
            //}
            //ImGuiAPI.EndChild();
        }
    }
}
