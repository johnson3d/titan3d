using EngineNS.Bricks.CodeBuilder;
using EngineNS.Bricks.NodeGraph;
using EngineNS.DesignMacross.Editor;
using EngineNS.DesignMacross.Base.Graph;
using System;
using System.Collections.Generic;
using System.Text;
using EngineNS.DesignMacross.Base.Outline;
using System.Collections;
using System.Reflection;
using EngineNS.Rtti;
using System.Linq;
using EngineNS.DesignMacross.Base.Render;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.TimedStateMachine.CompoundState;
using System.Diagnostics;
using EngineNS.EGui.Controls;

namespace EngineNS.DesignMacross.TimedStateMachine.CompoundState
{
    [ImGuiElementRender(typeof(TtGraph_TimedCompoundStateRender))]
    public class TtGraph_TimedCompoundState : TtGraph
    {
        public TtCommandHistory CommandHistory { get; set; } = new TtCommandHistory();
        public TtGraphElement_TimedCompoundStateEntry EntryElement { get; set; } = null;
        public TtTimedCompoundStateClassDescription TimedCompoundStateClassDescription { get => Description as TtTimedCompoundStateClassDescription; }
        public TtGraph_TimedCompoundState(IDescription description) : base(description)
        {
            Description = description;

        }

        public override void ConstructElements(ref FGraphRenderingContext context)
        {
            Elements.Clear();
            foreach (var location in ElementLocations)
            {
                var style = context.GraphElementStyleManager.GetOrAdd(location.Id);
                style.Location = location.Location;
            }
            ElementLocations.Clear();
            FGraphElementRenderingContext elementRenderingContext = default;
            elementRenderingContext.Camera = context.Camera;
            elementRenderingContext.ViewPort = context.ViewPort;
            elementRenderingContext.CommandHistory = context.CommandHistory;
            elementRenderingContext.EditorInteroperation = context.EditorInteroperation;
            elementRenderingContext.GraphElementStyleManager = context.GraphElementStyleManager;
            elementRenderingContext.DescriptionsElement = context.DescriptionsElement;

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
                            var instance = TtDescriptionGraphElementsPoolManager.Instance.GetDescriptionGraphElement(graphElementAttribute.ClassType, desc, context.GraphElementStyleManager.GetOrAdd(desc.Id));
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

        #region IContextMeunable
        struct ElementLocation
        {
            public Guid Id;
            public Vector2 Location;
        }
        List<ElementLocation> ElementLocations = new List<ElementLocation>();

        public override void ConstructContextMenu(ref FGraphElementRenderingContext context, TtPopupMenu PopupMenu)
        {
            PopupMenu.StringId = Name + "_" + Id + "_" + "ContextMenu";
            PopupMenu.Reset();
            PopupMenu.bHasSearchBox = true;
            var cmdHistory = context.CommandHistory;
            //for now just put here, util we have the init method
            foreach (var service in Rtti.UTypeDescManager.Instance.Services.Values)
            {
                foreach (var typeDesc in service.Types.Values)
                {
                    var att = typeDesc.GetCustomAttribute<StateMachineContextMenuAttribute>(true);
                    if (att != null)
                    {
                        TtMenuUtil.ConstructMenuItem(PopupMenu.Menu, typeDesc, att.MenuPaths, att.FilterStrings,
                             (TtMenuItem item, object sender) =>
                             {
                                 var popMenu = sender as TtPopupMenu;
                                 if (Rtti.UTypeDescManager.CreateInstance(typeDesc) is TtTimedSubStateClassDescription state)
                                 {
                                     state.Name = GetValidNodeName(state.Name);
                                     ElementLocations.Add(new ElementLocation { Id = state.Id, Location = popMenu.PopedPosition });
                                     cmdHistory.CreateAndExtuteCommand("AddState",
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
                var typeDesc = UTypeDesc.TypeOf(compoundState.GetType());
                string[] menuPaths = new[] { "StateMachine", "Hub", compoundState.Name };
                string filterStrings = compoundState.Name;
                TtMenuUtil.ConstructMenuItem(PopupMenu.Menu, typeDesc, menuPaths, filterStrings,
                             (TtMenuItem item, object sender) =>
                             {
                                 var popMenu = sender as TtPopupMenu;
                                 var hubDesc = new TtTimedCompoundStateHubClassDescription
                                 {
                                     TimedCompoundStateClassDescriptionId = compoundState.Id,
                                     TimedCompoundStateClassDescription = compoundState,
                                 };
                                 ElementLocations.Add(new ElementLocation { Id = hubDesc.Id, Location = popMenu.PopedPosition });
                                 cmdHistory.CreateAndExtuteCommand("AddHub",
                                   (data) => { TimedCompoundStateClassDescription.AddHub(hubDesc);},
                                   (data) => { TimedCompoundStateClassDescription.RemoveHub(hubDesc);});

                             });
            }

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
    public class TtGraph_TimedCompoundStateRender : IGraphRender
    {
        TtPopupMenu PopupMenu { get; set; } = new TtPopupMenu("Graph_TimedStatesHubRender");
        public void Draw(IRenderableElement renderableElement, ref FGraphRenderingContext context)
        {
            var timedStatesHubGraph = renderableElement as TtGraph_TimedCompoundState;
            if (timedStatesHubGraph == null)
                return;

            if (ImGuiAPI.BeginChild(timedStatesHubGraph.Name + "_Graph", in Vector2.Zero, false, ImGuiWindowFlags_.ImGuiWindowFlags_NoMove | ImGuiWindowFlags_.ImGuiWindowFlags_NoScrollbar | ImGuiWindowFlags_.ImGuiWindowFlags_NoScrollWithMouse))
            {
                var cmd = ImGuiAPI.GetWindowDrawList();

                Vector2 sz = ImGuiAPI.GetWindowContentRegionMax() - ImGuiAPI.GetWindowContentRegionMin();
                var winPos = ImGuiAPI.GetWindowPos();
                // initialize
                timedStatesHubGraph.Size = new SizeF(sz.X, sz.Y);
                timedStatesHubGraph.ViewPort.Location = winPos;
                timedStatesHubGraph.ViewPort.Size = new SizeF(sz.X, sz.Y);
                timedStatesHubGraph.Camera.Size = new SizeF(sz.X, sz.Y);

                timedStatesHubGraph.CommandHistory = context.CommandHistory;
                context.ViewPort = timedStatesHubGraph.ViewPort;
                context.Camera = timedStatesHubGraph.Camera;
                //

                FGraphElementRenderingContext elementRenderingContext = default;
                elementRenderingContext.Camera = context.Camera;
                elementRenderingContext.ViewPort = context.ViewPort;
                elementRenderingContext.CommandHistory = timedStatesHubGraph.CommandHistory;
                elementRenderingContext.EditorInteroperation = context.EditorInteroperation;
                elementRenderingContext.GraphElementStyleManager = context.GraphElementStyleManager;
                elementRenderingContext.DescriptionsElement = context.DescriptionsElement;

                TtGraphElement_GridLine grid = new TtGraphElement_GridLine();
                grid.Size = new SizeF(sz.X, sz.Y);
                var gridRender = TtElementRenderDevice.CreateGraphElementRender(grid);
                if (gridRender != null)
                    gridRender.Draw(grid, ref elementRenderingContext);

                if (timedStatesHubGraph is IContextMeunable meunableGraph)
                {
                    meunableGraph.SetContextMenuableId(PopupMenu);
                    if (ImGuiAPI.IsMouseClicked(ImGuiMouseButton_.ImGuiMouseButton_Right, false)
                        && context.ViewPort.IsInViewport(ImGuiAPI.GetMousePos()))
                    {
                        var pos = context.ViewPortInverseTransform(ImGuiAPI.GetMousePos());
                        if (timedStatesHubGraph.HitCheck(pos))
                        {
                            ImGuiAPI.CloseCurrentPopup();

                            meunableGraph.ConstructContextMenu(ref elementRenderingContext, PopupMenu);
                            PopupMenu.OpenPopup();
                        }
                    }
                    PopupMenu.Draw(ref elementRenderingContext);
                }
                foreach (var element in timedStatesHubGraph.Elements)
                {
                    if (element is ILayoutable layoutable)
                    {
                        var size = layoutable.Measuring(new SizeF());
                        layoutable.Arranging(new Rect(element.Location, size));
                    }
                }
                foreach (var element in timedStatesHubGraph.Elements)
                {
                    var elementRender = TtElementRenderDevice.CreateGraphElementRender(element);
                    if (elementRender != null)
                    {
                        elementRender.Draw(element, ref elementRenderingContext);
                    }
                }
                TtMouseEventProcesser.Instance.Processing(timedStatesHubGraph, ref elementRenderingContext);
            }
            ImGuiAPI.EndChild();
        }
    }
}
