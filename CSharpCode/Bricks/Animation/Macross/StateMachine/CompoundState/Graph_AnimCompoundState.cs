using EngineNS.Bricks.Animation.Macross.StateMachine.SubState;
using EngineNS.Bricks.StateMachine.Macross;
using EngineNS.Bricks.StateMachine.Macross.CompoundState;
using EngineNS.DesignMacross;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Render;
using EngineNS.DesignMacross.Design;
using EngineNS.DesignMacross.Editor;
using EngineNS.EGui.Controls;
using EngineNS.Rtti;
using SixLabors.Fonts;
using System.Collections;
using System.Diagnostics;
using System.Reflection;

namespace EngineNS.Bricks.Animation.Macross.StateMachine.CompoundState
{
    [ImGuiElementRender(typeof(TtGraph_AnimCompoundStateRender))]
    public class TtGraph_AnimCompoundState : TtGraph_TimedCompoundState
    {
        public TtGraph_AnimCompoundState(IDescription description) : base(description)
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
            elementRenderingContext.DesignedClassDescription = context.DesignedClassDescription;
            elementRenderingContext.DesignedGraph = this;

            foreach (var property in Description.GetType().GetProperties())
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
                        var propertyValueList = property.GetValue(Description) as IList;
                        foreach (var propertyValue in propertyValueList)
                        {
                            var graphElementAttribute = GraphElementAttribute.GetAttributeWithSpecificClassType<IGraphElement>(propertyValue.GetType());
                            Debug.Assert(graphElementAttribute != null);
                            Debug.Assert(propertyValue is IDescription);
                            var desc = propertyValue as IDescription;
                            var instance = TtDescriptionGraphElementsPoolManager.Instance.GetDescriptionGraphElement(graphElementAttribute.ClassType, desc, context.GraphElementStyleManager.GetOrAdd(desc));
                            instance.Parent = this;
                            Elements.Add(instance);
                            context.DescriptionsElement.Add(desc.Id, instance);
                        }
                    }
                }
                else
                {
                    var propertyValue = property.GetValue(Description);
                    Debug.Assert(propertyValue is IDescription);
                    var desc = propertyValue as IDescription;
                    var graphElementAttribute = GraphElementAttribute.GetAttributeWithSpecificClassType<IGraphElement>(propertyValue.GetType());
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
            
        }

        public override void ConstructContextMenu(ref FGraphElementRenderingContext context, TtPopupMenu popupMenu)
        {
            popupMenu.bHasSearchBox = true;
            var cmdHistory = context.CommandHistory;
            var graphElementStyleManager = context.GraphElementStyleManager;
            var tempContext = context;
            //for now just put here, util we have the init method
            foreach (var service in Rtti.TtTypeDescManager.Instance.Services.Values)
            {
                foreach (var typeDesc in service.Types.Values)
                {
                    var att = typeDesc.GetCustomAttribute<StateMachineContextMenuAttribute>(true);
                    if (att != null && att.KeyStrings.Contains(UDesignMacross.MacrossAnimEditorKeyword))
                    {
                        TtMenuUtil.ConstructMenuItem(popupMenu.Menu, typeDesc, att.MenuPaths, att.FilterStrings,
                             (TtMenuItem item, object sender) =>
                             {
                                 var popMenu = sender as TtPopupMenu;
                                 var popedPosition = tempContext.CameraTransform(popMenu.PopedPosition);
                                 if (Rtti.TtTypeDescManager.CreateInstance(typeDesc) is TtAnimSubStateClassDescription state)
                                 {
                                     state.Name = GetValidNodeName(state.Name);
                                     var style = graphElementStyleManager.GetOrAdd(state, popedPosition);
                                     cmdHistory.CreateAndExtuteCommand("AddAnimState",
                                         (data) => { TimedCompoundStateClassDescription.AddState(state); },
                                         (data) => { TimedCompoundStateClassDescription.RemoveState(state); });

                                 }
                             });

                    }
                }
            }

            foreach (var compoundState in TimedCompoundStateClassDescription?.StateMachineClassDescription.CompoundStates)
            {
                if (compoundState == TimedCompoundStateClassDescription)
                {
                    continue;
                }
                var typeDesc = TtTypeDesc.TypeOf(compoundState.GetType());
                string[] menuPaths = new[] { "StateMachine", "Hub", compoundState.Name };
                string filterStrings = compoundState.Name;
                TtMenuUtil.ConstructMenuItem(popupMenu.Menu, typeDesc, menuPaths, filterStrings,
                             (TtMenuItem item, object sender) =>
                             {
                                 var popMenu = sender as TtPopupMenu;
                                 var popedPosition = tempContext.CameraTransform(popMenu.PopedPosition);
                                 var hubDesc = new TtTimedCompoundStateHubClassDescription
                                 {
                                     TimedCompoundStateClassDescriptionId = compoundState.Id,
                                     TimedCompoundStateClassDescription = compoundState,
                                 };
                                 var style = graphElementStyleManager.GetOrAdd(hubDesc, popedPosition);
                                 cmdHistory.CreateAndExtuteCommand("AddHub",
                                   (data) => { TimedCompoundStateClassDescription.AddHub(hubDesc); },
                                   (data) => { TimedCompoundStateClassDescription.RemoveHub(hubDesc); });

                             });
            }
        }
    }
    public class TtGraph_AnimCompoundStateRender : IGraphRender
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
