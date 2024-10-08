using EngineNS.Bricks.CodeBuilder;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Render;
using EngineNS.DesignMacross.Design.ConnectingLine;
using EngineNS.DesignMacross.Design.Expressions;
using EngineNS.DesignMacross.Editor;
using EngineNS.EGui.Controls;
using EngineNS.Rtti;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using EngineNS.DesignMacross.Design.Statement;
using Microsoft.CodeAnalysis;
using System.Linq.Expressions;

namespace EngineNS.DesignMacross.Design
{
    [ImGuiElementRender(typeof(TtGraphElementRender_StatementDescription))]
    public class TtGraphElement_MethodStartDescription : TtGraphElement_StatementDescription
    {
        public TtMethodStartDescription MethodStartDescription { get => Description as TtMethodStartDescription; }
        public TtGraphElement_MethodStartDescription(IDescription description, IGraphElementStyle style) : base(description, style)
        {

        }
        public override void OnSelected(ref FGraphElementRenderingContext context)
        {
            context.EditorInteroperation.PGMember.Target = Description.Parent;
        }
    }
    [ImGuiElementRender(typeof(TtGraphElementRender_StatementDescription))]
    public class TtGraphElement_MethodEndDescription : TtGraphElement_StatementDescription
    {
        public TtMethodEndDescription MethodEndDescription { get => Description as TtMethodEndDescription; }
        public TtGraphElement_MethodEndDescription(IDescription description, IGraphElementStyle style) : base(description, style)
        {
        }
        public override void OnSelected(ref FGraphElementRenderingContext context)
        {
            context.EditorInteroperation.PGMember.Target = Description.Parent;
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
        public TtGraphElement_PreviewExecutionLine PreviewExecutionLine { get; set; } = null;
        public TtGraphElement_PreviewDataLine PreviewDataLine { get; set; } = null;
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
                            Debug.Assert(graphElementAttribute != null);
                            Debug.Assert(propertyValue is IDescription);
                            var desc = propertyValue as IDescription;

                            if (!context.GraphElementStyleManager.Contains(desc.Id))
                            {
                                //set default location
                                var style = context.GraphElementStyleManager.GetOrAdd(desc.Id);
                                style.Location = graphElementAttribute.DefaultLocation;
                            }

                            var instance = TtDescriptionGraphElementsPoolManager.Instance.GetDescriptionGraphElement(graphElementAttribute.ClassType, desc, context.GraphElementStyleManager.GetOrAdd(desc.Id));
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
                        var style = context.GraphElementStyleManager.GetOrAdd(desc.Id);
                        style.Location = graphElementAttribute.DefaultLocation;
                    }

                    var instance = TtDescriptionGraphElementsPoolManager.Instance.GetDescriptionGraphElement(graphElementAttribute.ClassType, desc, context.GraphElementStyleManager.GetOrAdd(desc.Id));
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

        public override void OnMouseLeftButtonUp(ref FGraphElementRenderingContext context)
        {
            if (PreviewDataLine != null || PreviewExecutionLine != null)
            {
                TtContextMenuHandler.Instance.HandleLinkedPinContextMenu(this, ref context);
                PreviewDataLine = null;
                PreviewExecutionLine = null;
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
            var methodGraph = renderableElement as TtGraph_Method;
            if (methodGraph == null)
                return;
            if (ImGuiAPI.BeginChild(methodGraph.Name + "_Graph", in Vector2.Zero, ImGuiChildFlags_.ImGuiChildFlags_None, ImGuiWindowFlags_.ImGuiWindowFlags_NoMove | ImGuiWindowFlags_.ImGuiWindowFlags_NoScrollbar | ImGuiWindowFlags_.ImGuiWindowFlags_NoScrollWithMouse))
            {
                var cmd = ImGuiAPI.GetWindowDrawList();

                Vector2 sz = ImGuiAPI.GetWindowContentRegionMax() - ImGuiAPI.GetWindowContentRegionMin();
                var winPos = ImGuiAPI.GetWindowPos();
                // initialize
                methodGraph.Size = new SizeF(sz.X, sz.Y);
                methodGraph.ViewPort.Location = winPos;
                methodGraph.ViewPort.Size = new SizeF(sz.X, sz.Y);
                methodGraph.Camera.Size = new SizeF(sz.X, sz.Y);

                methodGraph.CommandHistory = context.CommandHistory;
                context.ViewPort = methodGraph.ViewPort;
                context.Camera = methodGraph.Camera;
                //

                FGraphElementRenderingContext elementRenderingContext = default;
                elementRenderingContext.Camera = context.Camera;
                elementRenderingContext.ViewPort = context.ViewPort;
                elementRenderingContext.CommandHistory = methodGraph.CommandHistory;
                elementRenderingContext.EditorInteroperation = context.EditorInteroperation;
                elementRenderingContext.GraphElementStyleManager = context.GraphElementStyleManager;
                elementRenderingContext.DescriptionsElement = context.DescriptionsElement;
                elementRenderingContext.DesignedClassDescription = context.DesignedClassDescription;
                elementRenderingContext.DesignedGraph = methodGraph;

                TtGraphElement_GridLine grid = new TtGraphElement_GridLine();
                grid.Size = new SizeF(sz.X, sz.Y);
                var gridRender = TtElementRenderDevice.CreateGraphElementRender(grid);
                if (gridRender != null)
                    gridRender.Draw(grid, ref elementRenderingContext);

                foreach (var element in methodGraph.Elements)
                {
                    if (element is ILayoutable layoutable)
                    {
                        var size = layoutable.Measuring(new SizeF());
                        layoutable.Arranging(new Rect(element.Location, size));
                    }
                }
                foreach (var element in methodGraph.Elements)
                {
                    var elementRender = TtElementRenderDevice.CreateGraphElementRender(element);
                    if (elementRender != null)
                    {
                        elementRender.Draw(element, ref elementRenderingContext);
                    }
                }

                if (methodGraph.PreviewExecutionLine != null)
                {
                    var previewExecutionLineRender = TtElementRenderDevice.CreateGraphElementRender(methodGraph.PreviewExecutionLine);
                    if (previewExecutionLineRender != null)
                    {
                        previewExecutionLineRender.Draw(methodGraph.PreviewExecutionLine, ref elementRenderingContext);
                    }
                }
                if (methodGraph.PreviewDataLine != null)
                {
                    var previewDataLineRender = TtElementRenderDevice.CreateGraphElementRender(methodGraph.PreviewDataLine);
                    if (previewDataLineRender != null)
                    {
                        previewDataLineRender.Draw(methodGraph.PreviewDataLine, ref elementRenderingContext);
                    }
                }

                TtMouseEventProcesser.Instance.Processing(methodGraph, ref elementRenderingContext);
                TtContextMenuHandler.Instance.HandleContextMenu(TtMouseEventProcesser.Instance.LastElement, ref elementRenderingContext);
            }
            ImGuiAPI.EndChild();
        }
    }
}
