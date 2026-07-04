using EngineNS.Bricks.CodeBuilder;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Render;
using EngineNS.DesignMacross.Design.ConnectingLine;
using EngineNS.DesignMacross.Design.Expressions;
using EngineNS.DesignMacross.Design.Statement;
using EngineNS.DesignMacross.Editor;
using EngineNS.EGui.Controls;
using EngineNS.Rtti;
using Microsoft.CodeAnalysis;
using SixLabors.Fonts;
using System.Collections;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace EngineNS.DesignMacross.Design
{
    [ImGuiElementRender(typeof(TtGraphElementRender_StatementDescription))]
    public class TtGraphElement_MethodStartDescription : TtGraphElement_StatementDescription
    {
        public TtMethodStartDescription MethodStartDescription { get => Description as TtMethodStartDescription; }
        public TtGraphElement_MethodStartDescription(IDescription description, IGraphElementStyle style) : base(description, style)
        {

        }
        public override void OnSelected(ref FMouseEventContext context)
        {
            context.GraphElementRenderingContext.EditorInteroperation.PGMember.Target = Description.Parent;
            IsSelected = true;
            context.GraphElementRenderingContext.DesignedGraph.SelecteGraphElement(this);
        }
    }
    public struct ElementLocation
    {
        public Guid Id;
        public Vector2 Location;
    }
    [ImGuiElementRender(typeof(TtGraph_MethodRender))]
    public class TtGraph_Method : TtGraph, IContextMeunable
    {
        public virtual TtMethodDescription MethodDescription { get => Description as TtMethodDescription; }
        public TtGraph_Method(IDescription description) : base(description)
        {

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
            elementRenderingContext.DesignedClassDescription = context.DesignedClassDescription;
            elementRenderingContext.DesignedGraph = this;

            foreach (var property in MethodDescription.GetType().GetProperties())
            {
                var drawInGraphAttribute = property.GetCustomAttribute<DrawInGraphAttribute>();
                if (drawInGraphAttribute == null)
                {
                    continue;
                }
                if (property.PropertyType.IsGenericType)
                {
                    if (property.PropertyType.GetInterface("IList") != null)
                    {
                        var propertyValueList = property.GetValue(MethodDescription) as IList;
                        foreach (var propertyValue in propertyValueList)
                        {
                            var graphElementAttribute = GraphElementAttribute.GetAttributeWithSpecificClassType<IGraphElement>(propertyValue.GetType());
                            if (graphElementAttribute == null)
                                continue;

                            Debug.Assert(graphElementAttribute != null);
                            Debug.Assert(propertyValue is IDescription);
                            var desc = propertyValue as IDescription;

                            if (!context.GraphElementStyleManager.Contains(desc.Id))
                            {
                                //set default location
                                var style = context.GraphElementStyleManager.GetOrAdd(desc);
                                style.Location = graphElementAttribute.DefaultLocation;
                            }

                            var instance = TtDescriptionGraphElementsPoolManager.Instance.GetDescriptionGraphElement(graphElementAttribute.ClassType, desc, context.GraphElementStyleManager.GetOrAdd(desc));
                            instance.Parent = this;
                            Elements.Add(instance);
                            context.DescriptionsElement.Add(desc.Id, instance);
                        }
                    }
                }
                else
                {
                    var propertyValue = property.GetValue(MethodDescription);
                    Debug.Assert(propertyValue is IDescription);
                    var desc = propertyValue as IDescription;
                    var graphElementAttribute = GraphElementAttribute.GetAttributeWithSpecificClassType<IGraphElement>(propertyValue.GetType());

                    if (!context.GraphElementStyleManager.Contains(desc.Id))
                    {
                        //set default location
                        var style = context.GraphElementStyleManager.GetOrAdd(desc);
                        style.Location = graphElementAttribute.DefaultLocation;
                    }

                    var instance = TtDescriptionGraphElementsPoolManager.Instance.GetDescriptionGraphElement(graphElementAttribute.ClassType, desc, context.GraphElementStyleManager.GetOrAdd(desc));
                    instance.Parent = this;
                    Elements.Add(instance);
                    context.DescriptionsElement.Add(desc.Id, instance);
                }

            }
            foreach (var element in Elements)
            {
                element.ConstructElements(ref elementRenderingContext);
            }
        }
        public override void AfterConstructElements(ref FGraphRenderingContext context)
        {
            FGraphElementRenderingContext elementRenderingContext = default;
            elementRenderingContext.Camera = context.Camera;
            elementRenderingContext.ViewPort = context.ViewPort;
            elementRenderingContext.CommandHistory = context.CommandHistory;
            elementRenderingContext.EditorInteroperation = context.EditorInteroperation;
            elementRenderingContext.GraphElementStyleManager = context.GraphElementStyleManager;
            elementRenderingContext.DescriptionsElement = context.DescriptionsElement;
            foreach (var element in context.DescriptionsElement)
            {
                if (element.Value is IDescriptionGraphElement descriptionGraphElement)
                {
                    descriptionGraphElement.AfterConstructElements(ref elementRenderingContext);
                }
            }
        }


        public override void ConstructLinkedPinContextMenu(ref FGraphElementRenderingContext context, TtPopupMenu popupMenu)
        {
            popupMenu.bHasSearchBox = true;
            TtMethodGraphLinkedPinContextMenuUtil.ConstructMenuItemsAboutContextMenuAttribute(ref context, popupMenu, this);
            TtMethodGraphLinkedPinContextMenuUtil.ConstructMenuItemsAboutMetas(ref context, popupMenu, this);
            TtMethodGraphLinkedPinContextMenuUtil.ConstructMenuItemsAboutDesignedClass(ref context, popupMenu, this);
            TtMethodGraphLinkedPinContextMenuUtil.ConstructMenuItemsAboutClassPropertiesAndMethods_OutPin(ref context, popupMenu, this);
        }

        #region IContextMeunable
        public override void ConstructContextMenu(ref FGraphElementRenderingContext context, TtPopupMenu popupMenu)
        {
            popupMenu.bHasSearchBox = true;
            TtMethodGraphContextMenuUtil.ConstructMenuItemsAboutContextMenuAttribute(ref context, popupMenu, this);
            TtMethodGraphContextMenuUtil.ConstructMenuItemsAboutDesignedClass(ref context, popupMenu, this);
            TtMethodGraphContextMenuUtil.ConstructMenuItemsAboutMetas(ref context, popupMenu, this);
        }

   
        #endregion IContextMeunable

    }
    public class TtGraph_MethodRender : IGraphRender
    {
        public void Draw(IRenderableElement renderableElement, ref FGraphRenderingContext context)
        {
            var graph = renderableElement as TtGraph;
            if (graph == null)
                return;
            if (ImGuiAPI.BeginChild(graph.Name + "_Graph", in Vector2.Zero, ImGuiChildFlags_.ImGuiChildFlags_None, ImGuiWindowFlags_.ImGuiWindowFlags_NoMove | ImGuiWindowFlags_.ImGuiWindowFlags_NoScrollbar | ImGuiWindowFlags_.ImGuiWindowFlags_NoScrollWithMouse))
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
                elementRenderingContext.DesignedClassDescription = context.DesignedClassDescription;
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
                graph.Elements.Sort((e1, e2) => 
                {
                    if(e1 is TtGraphElement_Line)
                    {
                        if(e2 is TtGraphElement_Line)
                        {
                            return 0;
                        }
                        else
                        {
                            return -1;
                        }
                    }
                    else
                    {
                        if (e2 is TtGraphElement_Line)
                        {
                            return 1;
                        }
                        else
                        {
                            return 0;
                        }
                    }
                });
                foreach (var element in graph.Elements)
                {
                    var elementRender = TtElementRenderDevice.CreateGraphElementRender(element);
                    if (elementRender != null)
                    {
                        elementRender.Draw(element, ref elementRenderingContext);
                    }
                }

                if (graph.PreviewLine != null)
                {
                    var previewLineRender = TtElementRenderDevice.CreateGraphElementRender(graph.PreviewLine);
                    if (previewLineRender != null)
                    {
                        previewLineRender.Draw(graph.PreviewLine, ref elementRenderingContext);
                    }
                }
                if ((graph.SelectingRect != null))
                {
                    var selectingRectRender = TtElementRenderDevice.CreateGraphElementRender(graph.SelectingRect);
                    if (selectingRectRender != null)
                    {
                        selectingRectRender.Draw(graph.SelectingRect, ref elementRenderingContext);
                    }
                }
                if (ImGuiAPI.IsKeyDown(ImGuiKey.ImGuiKey_LeftCtrl))
                {
                    graph.CanMultiSelect = true;
                }
                else
                {
                    graph.CanMultiSelect = false;
                }
                TtMouseEventProcesser.Instance.Processing(graph, ref elementRenderingContext);
                TtGraphContextMenuHandler.Instance.HandleContextMenu(TtMouseEventProcesser.Instance.LastElement, ref elementRenderingContext);
            }
            ImGuiAPI.EndChild();
        }
    }
}
