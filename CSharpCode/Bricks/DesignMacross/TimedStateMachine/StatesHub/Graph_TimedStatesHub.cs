using EngineNS.Bricks.CodeBuilder;
using EngineNS.Bricks.NodeGraph;
using EngineNS.DesignMacross.Editor;
using EngineNS.DesignMacross.Base.Graph;
using System;
using System.Collections.Generic;
using System.Text;
using EngineNS.DesignMacross.Base.Graph.Elements;
using EngineNS.DesignMacross.Base.Outline;
using System.Collections;
using System.Reflection;
using EngineNS.Rtti;
using System.Linq;
using EngineNS.DesignMacross.Base.Render;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.TimedStateMachine.StatesHub;

namespace EngineNS.DesignMacross.TimedStateMachine
{
    [ImGuiElementRender(typeof(TtGraph_TimedStatesHubRender))]
    public class TtGraph_TimedStatesHub : TtGraph, IContextMeunable
    {
        public TtCommandHistory CommandHistory { get; set; } = new TtCommandHistory();
        public TtPopupMenu PopupMenu { get; set; } = new TtPopupMenu("TimedStateMachineGraphContextMenu");
        public override IDescription Description
        {
            get
            {
                return base.Description;
            }
            set
            {
                base.Description = value;
                TimedStatesHubClassDescription.StatesHubBridges.CollectionChanged -= StatesHubBridges_CollectionChanged;
                TimedStatesHubClassDescription.StatesHubBridges.CollectionChanged += StatesHubBridges_CollectionChanged; 
                TimedStatesHubClassDescription.States.CollectionChanged -= States_CollectionChanged;
                TimedStatesHubClassDescription.States.CollectionChanged += States_CollectionChanged;
            }
        }

        [Rtti.Meta]
        public TtGraphElement_TimedStatesHubEntry EntryElement { get; set; } = null;
        public TtTimedStatesHubClassDescription TimedStatesHubClassDescription { get => Description as TtTimedStatesHubClassDescription; }
        public Dictionary<IDescription, IGraphElement> GraphElementDic { get; set; } = new Dictionary<IDescription, IGraphElement>();
        public TtGraph_TimedStatesHub(IDescription description) :base(description)
        {
            Description = description;
            EntryElement = new TtGraphElement_TimedStatesHubEntry(description);
            EntryElement.Construct();
            EntryElement.Parent = this;
            Elements.Add(EntryElement);
        }
        public override void Construct()
        {
            Elements.Clear();
            Elements.Add(EntryElement);
            if (Description is TtTimedStatesHubClassDescription statesHubClassDescription)
            {
                foreach (var state in statesHubClassDescription.States)
                {
                    var graphElementAttribute = GraphElementAttribute.GetAttributeWithSpecificClassType<IGraphElement>(state.GetType());
                    //var graphElementAttribute = state.GetType().GetCustomAttribute<GraphElementAttribute>();
                    if (graphElementAttribute != null)
                    {
                        if (GraphElementDic.ContainsKey(state))
                        {
                            Elements.Add(GraphElementDic[state]);
                        }
                        else
                        {
                            var instance = UTypeDescManager.CreateInstance(graphElementAttribute.ClassType, new object[] { state }) as IGraphElement;
                            instance.Description = state;
                            instance.Construct();
                            instance.Parent = this;
                            Elements.Add(instance);
                            GraphElementDic.Add(state, instance);
                        }
                    }
                }
                foreach (var hubBridge in statesHubClassDescription.StatesHubBridges)
                {
                    var graphElementAttribute = GraphElementAttribute.GetAttributeWithSpecificClassType<IGraphElement>(hubBridge.GetType());
                    //var graphElementAttribute = hubBridge.GetType().GetCustomAttribute<GraphElementAttribute>();
                    if (graphElementAttribute != null)
                    {
                        if (GraphElementDic.ContainsKey(hubBridge))
                        {
                            Elements.Add(GraphElementDic[hubBridge]);
                        }
                        else
                        {
                            var instance = UTypeDescManager.CreateInstance(graphElementAttribute.ClassType) as IGraphElement;
                            instance.Description = hubBridge;
                            instance.Construct();
                            instance.Parent = this;
                            Elements.Add(instance);
                            GraphElementDic.Add(hubBridge, instance);
                        }
                    }
                }
            }
        }
        private void States_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //TODO: States_CollectionChanged
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    break;
            }
            Construct();
        }
        private void StatesHubBridges_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //TODO: StatesHubBridges_CollectionChanged
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    break;
            }
            Construct();
        }

        #region IContextMeunable
        public void OpenContextMeun()
        {
            PopupMenu.OpenPopup();
        }

        public void DrawContextMenu(ref FGraphElementRenderingContext context)
        {
            PopupMenu.Draw(ref context);
        }

        public void UpdateContextMenu(ref FGraphElementRenderingContext context)
        {
            PopupMenu.Reset();
            //for now just put here, util we have the init method
            foreach (var service in Rtti.UTypeDescManager.Instance.Services.Values)
            {
                foreach (var typeDesc in service.Types.Values)
                {
                    var att = typeDesc.GetCustomAttribute<StateMachineContextMenuAttribute>(true);
                    if (att != null)
                    {
                        TtMenuUtil.ConstructMenuItem(PopupMenu.Menu, typeDesc, att.MenuPaths, att.FilterStrings,
                             (UMenuItem item, object sender) =>
                             {
                                 var popMenu = sender as TtPopupMenu;
                                 if (Rtti.UTypeDescManager.CreateInstance(typeDesc) is TtTimedStateClassDescription state)
                                 {
                                     state.Name = GetValidNodeName(state.Name);
                                     var graphElementAttribute = GraphElementAttribute.GetAttributeWithSpecificClassType<IGraphElement>(state.GetType());
                                     if (graphElementAttribute != null)
                                     {
                                         IGraphElement node = null;
                                         node = UTypeDescManager.CreateInstance(graphElementAttribute.ClassType, new object[] { state }) as IGraphElement;
                                         node.Parent = this;
                                         state.Location = popMenu.PopedPosition;
                                         GraphElementDic.Add(state, node);
                                     }
                                     TimedStatesHubClassDescription.States.Add(state);
                                 }
                             });

                    }
                }
            }

            foreach (var hub in TimedStatesHubClassDescription?.StateMachineClassDescription.Hubs)
            {
                if (hub == TimedStatesHubClassDescription)
                {
                    continue;
                }
                var typeDesc = UTypeDesc.TypeOf(hub.GetType());
                string[] menuPaths = new[] { "StateMachine", "HubBridges", hub.Name };
                string filterStrings = hub.Name;
                TtMenuUtil.ConstructMenuItem(PopupMenu.Menu, typeDesc, menuPaths, filterStrings,
                             (UMenuItem item, object sender) =>
                             {
                                 var popMenu = sender as TtPopupMenu;
                                 var hubBridge = new TtTimedStatesHubBridgeClassDescription
                                 {
                                     TimedStatesHubClassDescription = TimedStatesHubClassDescription,
                                     Name = hub.Name + "_HubBridge",
                                     Location = popMenu.PopedPosition
                                 };
                                 IGraphElement node = new TtGraphElement_TimedStatesHubBridge(hubBridge)
                                 {
                                     Parent = this,
                                 };
                                 GraphElementDic.Add(hubBridge, node);
                                 TimedStatesHubClassDescription.StatesHubBridges.Add(hubBridge);

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
    public class TtGraph_TimedStatesHubRender : IGraphRender
    {
        public void Draw(IRenderableElement renderableElement, ref FGraphRenderingContext context)
        {
            var timedStatesHubGraph = renderableElement as TtGraph_TimedStatesHub;
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

                context.CommandHistory = timedStatesHubGraph.CommandHistory;
                context.ViewPort = timedStatesHubGraph.ViewPort;
                context.Camera = timedStatesHubGraph.Camera;
                //

                FGraphElementRenderingContext elementRenderingContext = default;
                elementRenderingContext.Camera = context.Camera;
                elementRenderingContext.ViewPort = context.ViewPort;
                elementRenderingContext.CommandHistory = timedStatesHubGraph.CommandHistory;
                elementRenderingContext.EditorInteroperation = context.EditorInteroperation;

                TtGraphElement_GridLine grid = new TtGraphElement_GridLine();
                grid.Size = new SizeF(sz.X, sz.Y);
                var gridRender = TtElementRenderDevice.CreateGraphElementRender(grid);
                if (gridRender != null)
                    gridRender.Draw(grid, ref elementRenderingContext);

                if (timedStatesHubGraph is IContextMeunable meunableGraph)
                {
                    if (ImGuiAPI.IsMouseClicked(ImGuiMouseButton_.ImGuiMouseButton_Right, false)
                        && context.ViewPort.IsInViewPort(ImGuiAPI.GetMousePos()))
                    {
                        var pos = context.ViewPortInverseTransform(ImGuiAPI.GetMousePos());
                        if (timedStatesHubGraph.HitCheck(pos))
                        {
                            ImGuiAPI.CloseCurrentPopup();
                            meunableGraph.UpdateContextMenu(ref elementRenderingContext);
                            meunableGraph.OpenContextMeun();
                        }
                    }
                    meunableGraph.DrawContextMenu(ref elementRenderingContext);
                }

                foreach (var element in timedStatesHubGraph.Elements)
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
                TtMouseEventProcesser.Instance.Processing(timedStatesHubGraph, context);
            }
            ImGuiAPI.EndChild();
        }
    }
}
