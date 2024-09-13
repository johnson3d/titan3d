using EngineNS.Animation.Macross;
using EngineNS.Animation.Macross.BlendTree.Node;
using EngineNS.Animation.StateMachine;
using EngineNS.Bricks.CodeBuilder;
using EngineNS.Bricks.StateMachine.Macross;
using EngineNS.Bricks.StateMachine.Macross.StateAttachment;
using EngineNS.Bricks.StateMachine.Macross.StateTransition;
using EngineNS.Bricks.StateMachine.Macross.SubState;
using EngineNS.DesignMacross;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Outline;
using EngineNS.DesignMacross.Design;
using EngineNS.DesignMacross.Design.ConnectingLine;
using EngineNS.DesignMacross.Design.Statement;
using EngineNS.Rtti;
using System.ComponentModel;
using System.Net.Mail;

namespace EngineNS.Animation.Macross.BlendTree
{
    [GraphElement(typeof(TtGraphElement_BlendTree_PoseOutput))]
    public class TtBlendTree_PoseOutputClassDescription : TtBlendTreeNodeClassDescription
    {
        public override string Name { get => "PoseOutput"; }
        public TtBlendTree_PoseOutputClassDescription()
        {
            AddPoseInPin(new TtPoseInPinDescription());
        }
        public TtPoseInPinDescription InPin
        {
            get
            {
                return PoseInPins[0];
            }
        }

        public override List<TtClassDeclaration> BuildClassDeclarations(ref FClassBuildContext classBuildContext)
        {
            SupperClassNames.Clear();
            SupperClassNames.Add($"EngineNS.Animation.BlendTree.Node.TtLocalSpaceBlendTree_PoseOutput<{classBuildContext.MainClassDescription.ClassName}>");
            List<TtClassDeclaration> classDeclarationsBuilded = new();
            var thisClassDeclaration = TtASTBuildUtil.BuildClassDeclaration(this, ref classBuildContext);
            thisClassDeclaration.AddMethod(BuildOverrideInitializeMethod());
            classDeclarationsBuilded.Add(thisClassDeclaration);
            return classDeclarationsBuilded;
        }

        public override TtStatementBase BuildBlendTreeStatement(ref FBlendTreeBuildContext blendTreeBuildContext)
        {
            var blendTreeNode = blendTreeBuildContext.BlendTreeDescription;
            var linkedNode = blendTreeNode.GetLinkedBlendTreeNode(InPin);
            if (linkedNode != null)
            {
                var fromNodeAssin = TtASTBuildUtil.CreateAssignOperatorStatement(
                    new TtVariableReferenceExpression("FromNode", new TtVariableReferenceExpression(VariableName)),
                    new TtVariableReferenceExpression(linkedNode.VariableName));
                blendTreeBuildContext.AddStatement(fromNodeAssin);

                FBlendTreeBuildContext buildContext = new() { ExecuteSequenceStatement = new(), BlendTreeDescription = blendTreeBuildContext.BlendTreeDescription};
                var statement = linkedNode.BuildBlendTreeStatement(ref blendTreeBuildContext);
                blendTreeBuildContext.AddStatement(buildContext.ExecuteSequenceStatement);
                return statement;
            }
            else
            {
                //empty method
            }
            return null;
        }

        #region Internal AST Build
        private TtMethodDeclaration BuildOverrideInitializeMethod()
        {
            var methodDeclaration = TtAnimASTBuildUtil.CreateBlendTreeOverridedInitMethodStatement();
            TtAnimASTBuildUtil.CreateBaseInitInvokeStatement(methodDeclaration);

            var returnValueAssign = TtASTBuildUtil.CreateAssignOperatorStatement(
                                        new TtVariableReferenceExpression(methodDeclaration.ReturnValue.VariableName),
                                        new TtPrimitiveExpression(true));
            methodDeclaration.MethodBody.Sequence.Add(returnValueAssign);
            return methodDeclaration;
        }
        #endregion
    }
}
