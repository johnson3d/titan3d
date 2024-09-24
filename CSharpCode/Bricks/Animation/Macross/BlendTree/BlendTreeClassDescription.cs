using EngineNS.Animation.BlendTree;
using EngineNS.Bricks.Animation.Macross.StateMachine.CompoundState;
using EngineNS.Bricks.CodeBuilder;
using EngineNS.Bricks.StateMachine.Macross.SubState;
using EngineNS.Bricks.StateMachine;
using EngineNS.DesignMacross;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Outline;
using EngineNS.DesignMacross.Design;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using EngineNS.DesignMacross.Design.ConnectingLine;
using EngineNS.Rtti;
using System.Linq.Expressions;
using EngineNS.Animation.Macross.BlendTree.Node;
using EngineNS.DesignMacross.Base.Description;
using Assimp;
using EngineNS.Animation.SkeletonAnimation.AnimatablePose;
using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;

namespace EngineNS.Animation.Macross.BlendTree
{
    public class AnimBlendTreeContextMenuAttribute : ContextMenuAttribute
    {
        public AnimBlendTreeContextMenuAttribute(string filterStrings, string menuPaths, params string[] keyStrings) : base(filterStrings, menuPaths, keyStrings)
        {
        }
    }

    [OutlineElement_Leaf(typeof(TtOutlineElement_BlendTreeGraph))]
    [Designable(typeof(TtLocalSpacePoseBlendTree), "BlendTree")]
    [Graph(typeof(TtGraph_BlendTree))]
    public class TtBlendTreeClassDescription : TtDesignableVariableDescription
    {
        [Rtti.Meta]
        [Category("Option")]
        public override string Name { get; set; } = "BlendTree";
        [Rtti.Meta]
        [DrawInGraph]
        public TtBlendTree_PoseOutputClassDescription PoseOutput { get; set; } = new TtBlendTree_PoseOutputClassDescription();
        [Rtti.Meta]
        [DrawInGraph]
        public List<TtBlendTreeNodeClassDescription> Nodes { get; set; } = new List<TtBlendTreeNodeClassDescription>();
        [Rtti.Meta, DrawInGraph]
        public List<TtPoseLineDescription> PoseLines { get; set; } = new();
        public bool AddNode(TtBlendTreeNodeClassDescription node)
        {
            Nodes.Add(node);
            node.Parent = this;
            return true;
        }
        public bool RemoveNode(TtBlendTreeNodeClassDescription node)
        {
            Nodes.Remove(node);
            node.Parent = null;
            return true;
        }
        public void AddPoseLine(TtPoseLineDescription poseLine)
        {
            PoseLines.Add(poseLine);
            poseLine.Parent = this;
        }

        public bool RemovePoseLine(TtPoseLineDescription poseLine)
        {
            PoseLines.Remove(poseLine);
            poseLine.Parent = null;
            return true;
        }

        public TtPosePinDescription GetLinkedPosePin(TtPosePinDescription posePin)
        {
            var linkedPinId = Guid.Empty;
            foreach (var dataLine in PoseLines)
            {
                if (dataLine.FromId == posePin.Id)
                {
                    linkedPinId = dataLine.ToId;
                    break;
                }
                if (dataLine.ToId == posePin.Id)
                {
                    linkedPinId = dataLine.FromId;
                    break;
                }
            }
            foreach (var node in Nodes)
            {
                if (node.TryGetPosePin(linkedPinId, out var linkedPin))
                {
                    return linkedPin;
                }
            }
            return null;
        }

        public override List<TtClassDeclaration> BuildClassDeclarations(ref FClassBuildContext classBuildContext)
        {
            SupperClassNames.Clear();
            SupperClassNames.Add($"EngineNS.Animation.BlendTree.TtLocalSpacePoseFinalBlendTree<{classBuildContext.MainClassDescription.ClassName}>");
            List<TtClassDeclaration> classDeclarationsBuilded = new();
            var thisClassDeclaration = TtASTBuildUtil.BuildClassDeclaration(this, ref classBuildContext);

            classDeclarationsBuilded.AddRange(PoseOutput.BuildClassDeclarations(ref classBuildContext));
            thisClassDeclaration.Properties.Add(PoseOutput.BuildVariableDeclaration(ref classBuildContext));
            foreach (var blendTreeNode in Nodes)
            {
                classDeclarationsBuilded.AddRange(blendTreeNode.BuildClassDeclarations(ref classBuildContext));
                thisClassDeclaration.Properties.Add(blendTreeNode.BuildVariableDeclaration(ref classBuildContext));
            }
            thisClassDeclaration.AddMethod(BuildOverrideInitializeMethod());
            classDeclarationsBuilded.Add(thisClassDeclaration);
            return classDeclarationsBuilded;
        }

        public override TtVariableDeclaration BuildVariableDeclaration(ref FClassBuildContext classBuildContext)
        {
            return TtASTBuildUtil.CreateVariableDeclaration(this, ref classBuildContext);
        }
        void GenerateCodeInMainClassInitMethod(TtClassDeclaration classDeclaration, ref FClassBuildContext classBuildContext)
        {
            var initMethod = classDeclaration.FindMethod("Initialize");
            if (initMethod == null)
            {
                initMethod = TtASTBuildUtil.CreateInitMethodDeclaration();
                classDeclaration.AddMethod(initMethod);
            }

            var blendTreeContext_VarName = "blendTreeContext" + VariableName;

            var blendTreeContextCreate = TtASTBuildUtil.CreateVariableDeclaration(blendTreeContext_VarName,
                new TtTypeReference(typeof(FAnimBlendTreeContext)),
                new TtCreateObjectExpression(typeof(FAnimBlendTreeContext).FullName));
            initMethod.MethodBody.Sequence.Add(blendTreeContextCreate);

            var animatableSkeletonPose_VarName = "animatableSkeletonPose" + VariableName;

            var animatableSkeletonPoseCreate = TtASTBuildUtil.CreateVariableDeclaration(animatableSkeletonPose_VarName,
                new TtTypeReference(typeof(TtAnimatableSkeletonPose)), new TtNullValueExpression());
            initMethod.MethodBody.Sequence.Add(animatableSkeletonPoseCreate);

            var getAnimatablePoseFromNode = new TtMethodInvokeStatement("CreateAnimatableSkeletonPoseFromeNode",
                animatableSkeletonPoseCreate,
                new TtClassReferenceExpression(TtTypeDesc.TypeOf<TtAnimUtil>()),
                new TtMethodInvokeArgumentExpression { Expression = new TtVariableReferenceExpression("MacrossNode") }
                );
            initMethod.MethodBody.Sequence.Add(getAnimatablePoseFromNode);


            var blendTreeContextAnimatablePoseAssign = TtASTBuildUtil.CreateAssignOperatorStatement(
                new TtVariableReferenceExpression("AnimatableSkeletonPose", new TtVariableReferenceExpression(blendTreeContext_VarName)),
                new TtVariableReferenceExpression(animatableSkeletonPose_VarName));
            initMethod.MethodBody.Sequence.Add(blendTreeContextAnimatablePoseAssign);

            var finalPoseAssign = TtASTBuildUtil.CreateAssignOperatorStatement(new TtVariableReferenceExpression(VariableName), new TtCreateObjectExpression(VariableType.TypeFullName));
            initMethod.MethodBody.Sequence.Add(finalPoseAssign);

            var centerDataAssign = TtASTBuildUtil.CreateAssignOperatorStatement(
                new TtVariableReferenceExpression("CenterData", new TtVariableReferenceExpression(VariableName)),
                new TtSelfReferenceExpression());
            initMethod.MethodBody.Sequence.Add(centerDataAssign);

            var finalPoseInitializeInvoke = new TtMethodInvokeStatement("Initialize",
                null, new TtVariableReferenceExpression(VariableName),
                new TtMethodInvokeArgumentExpression { OperationType = EMethodArgumentAttribute.Default, Expression = new TtVariableReferenceExpression(blendTreeContext_VarName) });
            finalPoseInitializeInvoke.IsAsync = true;
            initMethod.MethodBody.Sequence.Add(finalPoseInitializeInvoke);

            var runtimePose_VarName = "runtimePose";
            var runtimePoseCreate = TtASTBuildUtil.CreateVariableDeclaration(runtimePose_VarName, new TtTypeReference(typeof(TtLocalSpaceRuntimePose)), new TtNullValueExpression());
            initMethod.MethodBody.Sequence.Add(runtimePoseCreate);

            var bindRuntimeSkeletonPoseToNode = new TtMethodInvokeStatement("BindRuntimeSkeletonPoseToNode",
                runtimePoseCreate,
                new TtClassReferenceExpression(TtTypeDesc.TypeOf<TtAnimUtil>()),
                new TtMethodInvokeArgumentExpression { Expression = new TtVariableReferenceExpression("MacrossNode") }
                );
            initMethod.MethodBody.Sequence.Add(bindRuntimeSkeletonPoseToNode);

            var setPose = new TtMethodInvokeStatement("SetPose",
                null,
                new TtVariableReferenceExpression(VariableName),
                new TtMethodInvokeArgumentExpression { Expression = new TtVariableReferenceExpression(runtimePose_VarName) }
                );
            initMethod.MethodBody.Sequence.Add(setPose);

            var baseInitializeInvoke = new TtMethodInvokeStatement("Initialize", null, new TtBaseReferenceExpression());
            baseInitializeInvoke.IsAsync = true;
            initMethod.MethodBody.Sequence.Add(baseInitializeInvoke);
        }

        void GenerateCodeInMainClassTickMethod(TtClassDeclaration classDeclaration, ref FClassBuildContext classBuildContext)
        {
            var tickMethod = classDeclaration.FindMethod("Tick");
            if (tickMethod == null)
            {
                tickMethod = TtASTBuildUtil.CreateTickMethodDeclaration();
                classDeclaration.AddMethod(tickMethod);
            }
            var blendTreeContext_VarName = "blendTreeContext" + VariableName;

            var blendTreeContextCreate = TtASTBuildUtil.CreateVariableDeclaration(blendTreeContext_VarName,
                new TtTypeReference(typeof(FAnimBlendTreeContext)),
                new TtCreateObjectExpression(typeof(FAnimBlendTreeContext).FullName));
            tickMethod.MethodBody.Sequence.Add(blendTreeContextCreate);

            var finalPoseInitializeInvoke = new TtMethodInvokeStatement("Tick",
                null, new TtVariableReferenceExpression(VariableName),
                new TtMethodInvokeArgumentExpression { OperationType = EMethodArgumentAttribute.Default, Expression = new TtVariableReferenceExpression("elapseSecond") },
                new TtMethodInvokeArgumentExpression { OperationType = EMethodArgumentAttribute.Ref, Expression = new TtVariableReferenceExpression(blendTreeContext_VarName) });
            tickMethod.MethodBody.Sequence.Add(finalPoseInitializeInvoke);

        }
        public override void GenerateCodeInClass(TtClassDeclaration classDeclaration, ref FClassBuildContext classBuildContext)
        {
            base.GenerateCodeInClass(classDeclaration, ref classBuildContext);

            GenerateCodeInMainClassInitMethod(classDeclaration, ref classBuildContext);

            GenerateCodeInMainClassTickMethod(classDeclaration, ref classBuildContext);
        }

        #region Internal AST Build
        private TtMethodDeclaration BuildOverrideInitializeMethod()
        {
            var methodDeclaration = TtAnimASTBuildUtil.CreateBlendTreeOverridedInitMethodStatement();
            TtAnimASTBuildUtil.CreateBaseInitInvokeStatement(methodDeclaration);
            TtAnimASTBuildUtil.CreateNewThenCenterDataAssignThenInitInvokeStatement(PoseOutput, methodDeclaration);
            foreach (var node in Nodes)
            {
                TtAnimASTBuildUtil.CreateNewThenCenterDataAssignThenInitInvokeStatement(node, methodDeclaration);
            }

            FBlendTreeBuildContext buildContext = new() { ExecuteSequenceStatement = new(), BlendTreeDescription = this };
            PoseOutput.BuildBlendTreeStatement(ref buildContext);
            methodDeclaration.MethodBody.Sequence.Add(buildContext.ExecuteSequenceStatement);
            var fromNodeAssin = TtASTBuildUtil.CreateAssignOperatorStatement(
                    new TtVariableReferenceExpression("FromNode"),
                    new TtVariableReferenceExpression(PoseOutput.VariableName));
            methodDeclaration.MethodBody.Sequence.Add(fromNodeAssin);

            var returnValueAssign = TtASTBuildUtil.CreateAssignOperatorStatement(
                                        new TtVariableReferenceExpression(methodDeclaration.ReturnValue.VariableName),
                                        new TtPrimitiveExpression(true));
            methodDeclaration.MethodBody.Sequence.Add(returnValueAssign);
            return methodDeclaration;
        }
        #endregion

        public TtBlendTreeNodeClassDescription GetLinkedBlendTreeNode(TtPosePinDescription pin)
        {
            Guid linkedPinId = Guid.Empty;
            foreach(var line in PoseLines)
            {
                if (line.FromId == pin.Id)
                {
                    linkedPinId = line.ToId;
                }
                if (line.ToId == pin.Id)
                {
                    linkedPinId = line.FromId;
                }
            }
            if(linkedPinId == Guid.Empty)
            {
                return null;
            }
            foreach (var node in Nodes) 
            {
                if(node.TryGetPosePin(linkedPinId, out var linkedPin))
                {
                    return node;
                }
            }
            return null;
        }
    }
}
