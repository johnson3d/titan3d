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


    [ImGuiElementRender(typeof(TtGraph_TimedStateTransitionRender))]
    public class TtGraph_TimedStateTransition : TtGraph, IContextMeunable
    {
        IMethodDescription mTransitionMethod;
        UMacrossMethodGraph mGraph;
        public UMacrossMethodGraph Graph => mGraph;

        TtTransitionMacrossHolder mMacrossHolder;

        public TtGraph_TimedStateTransition(IDescription description) : base(description)
        {
            var desc = description as TtTimedStateTransitionClassDescription;
            var methodName = desc.Name + "_func";
            mTransitionMethod = new TtMethodDescription()
            {
                Name = methodName,
                MethodName = methodName,
                ReturnValue = new UVariableDeclaration()
                {
                    VariableType = new UTypeReference(typeof(bool)),
                    InitValue = new UDefaultValueExpression(typeof(bool))
                }
            };
            mTransitionMethod.ReturnValue.VariableName = "ret_" + mTransitionMethod.Id.ToString().GetHashCode();

            desc.Methods.Add(mTransitionMethod);

            mMacrossHolder = new TtTransitionMacrossHolder();
            mGraph = UMacrossMethodGraph.NewGraph(mMacrossHolder, mTransitionMethod.BuildMethodDeclaration());
            mGraph.GraphName = description.Name;
        }

        public TtPopupMenu PopupMenu { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void DrawContextMenu(ref FGraphElementRenderingContext context)
        {
            throw new NotImplementedException();
        }

        public void OpenContextMeun()
        {

        }

        public void UpdateContextMenu(ref FGraphElementRenderingContext context)
        {

        }

        public override void Construct()
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
