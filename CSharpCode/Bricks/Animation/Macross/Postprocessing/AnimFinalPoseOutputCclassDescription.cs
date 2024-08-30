using EngineNS.Animation.Asset;
using EngineNS.Animation.BlendTree;
using EngineNS.Animation.Macross.BlendTree;
using EngineNS.Animation.SkeletonAnimation.AnimatablePose;
using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;
using EngineNS.Bricks.CodeBuilder;
using EngineNS.DesignMacross;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Outline;
using EngineNS.DesignMacross.Design;
using EngineNS.Rtti;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.Animation.Macross.Postprocessing
{
    [OutlineElement_Leaf(typeof(TtOutlineElement_AnimFinalPoseOutput))]
    [Designable(typeof(TtLocalSpacePoseBlendTree), "AnimFinalPose")]
    [Graph(typeof(TtGraph_BlendTree))]
    public class TtAnimFinalPoseOutputCclassDescription : TtBlendTreeClassDescription
    {
        [Rtti.Meta]
        [Category("Option")]
        public override string Name { get; set; } = "AnimFinalPose";
        public override List<UClassDeclaration> BuildClassDeclarations(ref FClassBuildContext classBuildContext)
        {
            SupperClassNames.Clear();
            SupperClassNames.Add($"EngineNS.Animation.BlendTree.TtLocalSpacePoseBlendTree");
            List<UClassDeclaration> classDeclarationsBuilded = new List<UClassDeclaration>();
            var thisClassDeclaration = TtASTBuildUtil.BuildClassDeclaration(this, ref classBuildContext);
            foreach(var blendTreeNode in Nodes)
            {
                classDeclarationsBuilded.AddRange(blendTreeNode.BuildClassDeclarations(ref classBuildContext));
                thisClassDeclaration.Properties.Add(blendTreeNode.BuildVariableDeclaration(ref classBuildContext));
            }
            classDeclarationsBuilded.Add(thisClassDeclaration);
            return classDeclarationsBuilded;
        }
        public override UVariableDeclaration BuildVariableDeclaration(ref FClassBuildContext classBuildContext)
        {
            return TtASTBuildUtil.CreateVariableDeclaration(this, ref classBuildContext);
        }
        void GenerateCodeInInitMethod(UClassDeclaration classDeclaration)
        {
            var initMethod = classDeclaration.FindMethod("Initialize");
            if (initMethod == null)
            {
                initMethod = TtASTBuildUtil.CreateInitMethodDeclaration();
                classDeclaration.AddMethod(initMethod);
            }
            var blendTreeContextTypeName = "EngineNS.Animation.BlendTree.FAnimBlendTreeContext";
            var blendTreeContextCreate = TtASTBuildUtil.CreateVariableDeclaration("blendTreeContext", new UTypeReference(typeof(FAnimBlendTreeContext)), new UCreateObjectExpression(blendTreeContextTypeName));
            initMethod.MethodBody.Sequence.Add(blendTreeContextCreate);

            var animatableSkeletonPoseCreate = TtASTBuildUtil.CreateVariableDeclaration("animatableSkeletonPose", new UTypeReference(typeof(TtAnimatableSkeletonPose)), new UNullValueExpression());
            initMethod.MethodBody.Sequence.Add(animatableSkeletonPoseCreate);

            var getAnimatablePoseFromNode = new UMethodInvokeStatement("CreateAnimatableSkeletonPoseFromeNode",
                animatableSkeletonPoseCreate,
                new UClassReferenceExpression(UTypeDesc.TypeOf<TtAnimUtil>()),
                new UMethodInvokeArgumentExpression { Expression = new UVariableReferenceExpression("MacrossNode") }
                );
            initMethod.MethodBody.Sequence.Add(getAnimatablePoseFromNode);


            var blendTreeContextAnimatablePoseAssign = TtASTBuildUtil.CreateAssignOperatorStatement(
                new UVariableReferenceExpression("AnimatableSkeletonPose", new UVariableReferenceExpression("blendTreeContext")), 
                new UVariableReferenceExpression("animatableSkeletonPose"));
            initMethod.MethodBody.Sequence.Add(blendTreeContextAnimatablePoseAssign);

            var finalPoseAssign = TtASTBuildUtil.CreateAssignOperatorStatement(new UVariableReferenceExpression(VariableName), new UCreateObjectExpression(VariableType.TypeFullName));
            initMethod.MethodBody.Sequence.Add(finalPoseAssign);

            var finalPoseInitializeInvoke = new UMethodInvokeStatement("Initialize",
                null, new UVariableReferenceExpression(VariableName),
                new UMethodInvokeArgumentExpression { OperationType = EMethodArgumentAttribute.Ref, Expression = new UVariableReferenceExpression("blendTreeContext") });
            finalPoseInitializeInvoke.IsAsync = true;
            initMethod.MethodBody.Sequence.Add(finalPoseInitializeInvoke);

            var runtimePoseCreate = TtASTBuildUtil.CreateVariableDeclaration("runtimePose", new UTypeReference(typeof(TtLocalSpaceRuntimePose)), new UNullValueExpression());
            initMethod.MethodBody.Sequence.Add(runtimePoseCreate);

            var bindRuntimeSkeletonPoseToNode = new UMethodInvokeStatement("BindRuntimeSkeletonPoseToNode",
                runtimePoseCreate,
                new UClassReferenceExpression(UTypeDesc.TypeOf<TtAnimUtil>()),
                new UMethodInvokeArgumentExpression { Expression = new UVariableReferenceExpression("MacrossNode") }
                );
            initMethod.MethodBody.Sequence.Add(bindRuntimeSkeletonPoseToNode);

            var blendTreeOutPoseAssign = TtASTBuildUtil.CreateAssignOperatorStatement(
                new UVariableReferenceExpression("OutPose", new UVariableReferenceExpression(VariableName)), 
                new UVariableReferenceExpression("runtimePose"));
            initMethod.MethodBody.Sequence.Add(blendTreeOutPoseAssign);
        }
        public override void GenerateCodeInClass(UClassDeclaration classDeclaration)
        {
            base.GenerateCodeInClass(classDeclaration);

            GenerateCodeInInitMethod(classDeclaration);

        }
    }
}
