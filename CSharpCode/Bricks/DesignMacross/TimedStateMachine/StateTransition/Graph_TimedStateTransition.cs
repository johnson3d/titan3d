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

    public class TtTimedStateTransitionMethodGraph : UMacrossMethodGraph
    {
        public override void UpdateCanvasMenus()
        {
            base.UpdateCanvasMenus();

        }
        public static TtTimedStateTransitionMethodGraph CreateGraph(IMacrossMethodHolder kls, UMethodDeclaration method = null)
        {
            var result = new TtTimedStateTransitionMethodGraph();
            result.MacrossEditor = kls;
            result.Initialize();
            //result.FunctionName = funName;
            //if (result.Function == null)
            //    return null;
            if (method != null)
            {
                var methodData = MethodData.CreateFromMethod(result, method);
                result.MethodDatas.Add(methodData);
                result.AddNode(methodData.StartNode);
                result.GraphName = method.MethodName;
            }
            return result;
        }
    }


    [ImGuiElementRender(typeof(TtGraph_TimedStateTransitionRender))]
    public class TtGraph_TimedStateTransition : TtGraph, IContextMeunable
    {
        public TtTimedStateTransitionClassDescription TimedStateTransitionClass { get => Description as TtTimedStateTransitionClassDescription; }
        UMacrossMethodGraph mGraph;
        public UMacrossMethodGraph Graph => mGraph;

        TtTransitionMacrossHolder mMacrossHolder;

        public TtGraph_TimedStateTransition(IDescription description) : base(description)
        {
            var desc = description as TtTimedStateTransitionClassDescription;

            mMacrossHolder = new TtTransitionMacrossHolder();
            FClassBuildContext classBuildContext = new();
            mGraph = TtTimedStateTransitionMethodGraph.CreateGraph(mMacrossHolder, desc.OverrideCheckConditionMethodDescription.BuildMethodDeclaration(ref classBuildContext));
            mGraph.GraphName = description.Name;
        }

        public override void ConstructContextMenu(ref FGraphElementRenderingContext context, TtPopupMenu PopupMenu)
        {
            PopupMenu.Reset();
        }

        public override void ConstructElements(ref FGraphRenderingContext context)
        {
            
        }
    }
    public class TtGraph_TimedStateTransitionRender : IGraphRender
    {
        UGraphRenderer mGraphRender;

        public void Draw(IRenderableElement renderableElement, ref FGraphRenderingContext context)
        {
            var graph = renderableElement as TtGraph_TimedStateTransition;
            if (graph == null)
                return;

            if(mGraphRender == null)
            {
                mGraphRender = new UGraphRenderer();
                mGraphRender.SetGraph(graph.Graph);
                mGraphRender.DrawInherit = false;
            }

            mGraphRender.OnDraw();
        }
    }
}
