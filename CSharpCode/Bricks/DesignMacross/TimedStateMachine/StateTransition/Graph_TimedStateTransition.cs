using EngineNS.Bricks.CodeBuilder;
using EngineNS.Bricks.CodeBuilder.MacrossNode;
using EngineNS.Bricks.NodeGraph;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Render;
using EngineNS.DesignMacross.Design;
using EngineNS.EGui.Controls.PropertyGrid;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.DesignMacross.TimedStateMachine.StateTransition
{
    public class TtGraph_TransitionCheckConditionMethod : TtGraph_Method
    {
        public TtGraph_TransitionCheckConditionMethod(IDescription description) : base(description)
        {

        }
        int VariableCount = 0;
        int MethodCount = 0;
        public override void ConstructElements(ref FGraphRenderingContext context)
        {
            var classDesc = MethodDescription.Parent as TtTimedStateTransitionClassDescription;
            if (MethodDescription.MethodGraph == null)
            {
                var macrossHolder = new TtTransitionMacrossHolder();
                FClassBuildContext classBuildContext = new() { MainClassDescription = classDesc };
                var classDec = TtDescriptionASTBuildUtil.BuildDefaultPartForClassDeclaration(classDesc, ref classBuildContext);
                macrossHolder.DefClass = classDec;
                MethodDescription.MethodGraph = TtDesignMacrossMethodGraph.CreateGraph(macrossHolder, TtDescriptionASTBuildUtil.BuildDefaultPartForMethodDeclaration(classDesc.OverrideCheckConditionMethodDescription, ref classBuildContext));
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
    [ImGuiElementRender(typeof(TtGraph_TimedStateTransitionRender))]
    public class TtGraph_TimedStateTransition : TtGraph, IContextMeunable
    {
        public TtTimedStateTransitionClassDescription TimedStateTransitionClassDescription { get => Description as TtTimedStateTransitionClassDescription; }
        public TtGraph_TransitionCheckConditionMethod Graph_TransitionCheckConditionMethod = null;
        public TtGraph_TimedStateTransition(IDescription description) : base(description)
        {

        }

        public override void ConstructContextMenu(ref FGraphElementRenderingContext context, TtPopupMenu PopupMenu)
        {

        }

        public override void ConstructElements(ref FGraphRenderingContext context)
        {
            if(Graph_TransitionCheckConditionMethod == null)
            {
                Graph_TransitionCheckConditionMethod = new TtGraph_TransitionCheckConditionMethod(TimedStateTransitionClassDescription.OverrideCheckConditionMethodDescription);
            }
            Graph_TransitionCheckConditionMethod.ConstructElements(ref context);
        }
    }
    public class TtGraph_TimedStateTransitionRender : IGraphRender
    {
        TtGraph_MethodRender Graph_MethodRender = new TtGraph_MethodRender();
        //internal UGraphRenderer mGraphRender = null;

        public void Draw(IRenderableElement renderableElement, ref FGraphRenderingContext context)
        {
            var graph = renderableElement as TtGraph_TimedStateTransition;
            if (graph == null)
                return;

            var desc = graph.TimedStateTransitionClassDescription;
            Graph_MethodRender.Draw(graph.Graph_TransitionCheckConditionMethod, ref context);  
        }
    }
}
