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
    public class TtTransitionMacrossHolder : IMacrossMethodHolder
    {
        public MemberVar DraggingMember { get; set; }
        public bool IsDraggingMember { get; set; }

        UClassDeclaration mDefClass = new UClassDeclaration();
        public UClassDeclaration DefClass => mDefClass;
        UCSharpCodeGenerator mCSCodeGen = new UCSharpCodeGenerator();
        public UCSharpCodeGenerator CSCodeGen => mCSCodeGen;
        List<UMacrossMethodGraph> mMethods = new List<UMacrossMethodGraph>();
        public List<UMacrossMethodGraph> Methods => mMethods;

        public PropertyGrid PGMember { get; set; }

        public void RemoveMethod(UMacrossMethodGraph method)
        {

        }

        public void SetConfigUnionNode(IUnionNode node)
        {

        }
    }

    public class TtDesignMacrossMethodGraph : UMacrossMethodGraph
    {
        public override void UpdateCanvasMenus()
        {
            base.UpdateCanvasMenus();

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
            mGraph = UMacrossMethodGraph.NewGraph(mMacrossHolder, desc.OverrideCheckConditionMethodDescription.BuildMethodDeclaration(ref classBuildContext));
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
