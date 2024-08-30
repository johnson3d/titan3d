using EngineNS.Animation.Macross;
using EngineNS.Animation.StateMachine;
using EngineNS.Bricks.Animation.Macross.StateMachine.CompoundState;
using EngineNS.Bricks.CodeBuilder;
using EngineNS.Bricks.StateMachine.Macross;
using EngineNS.Bricks.StateMachine.Macross.CompoundState;
using EngineNS.DesignMacross;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Outline;
using EngineNS.DesignMacross.Design;
using EngineNS.Rtti;
using System.ComponentModel;

namespace EngineNS.Bricks.Animation.Macross.StateMachine
{
    [OutlineElement_Branch(typeof(TtOutlineElement_AnimStateMachine))]
    [Designable(typeof(TtAnimStateMachine), "AnimStateMachine")]
    public class TtAnimStateMachineClassDescription : TtTimedStateMachineClassDescription
    {
        [Rtti.Meta]
        [Category("Option")]
        public override string Name { get; set; } = "AnimStateMachine";
        [Rtti.Meta]
        [OutlineElement_List(typeof(TtOutlineElementsList_AnimCompoundStates), true)]
        public override List<TtTimedCompoundStateClassDescription> CompoundStates { get; set; } = new();
        public override List<UClassDeclaration> BuildClassDeclarations(ref FClassBuildContext classBuildContext)
        {
            SupperClassNames.Clear();
            SupperClassNames.Add($"EngineNS.Animation.StateMachine.TtAnimStateMachine<{classBuildContext.MainClassDescription.ClassName}>");
            List<UClassDeclaration> classDeclarationsBuilded = new List<UClassDeclaration>();
            UClassDeclaration thisClassDeclaration = TtASTBuildUtil.BuildClassDeclaration(this, ref classBuildContext);


            foreach (var compoundState in CompoundStates)
            {
                classDeclarationsBuilded.AddRange(compoundState.BuildClassDeclarations(ref classBuildContext));
                var compoundStateProperty = compoundState.BuildVariableDeclaration(ref classBuildContext);
                compoundStateProperty.VisitMode = EVisisMode.Public;
                thisClassDeclaration.Properties.Add(compoundStateProperty);
            }
            thisClassDeclaration.AddMethod(BuildOverrideInitializeMethod(ref classBuildContext));
            classDeclarationsBuilded.Add(thisClassDeclaration);
            return classDeclarationsBuilded;
        }
        bool TryInitializeMethodInClassDescription(ref FClassBuildContext classBuildContext, out UMethodDeclaration initMethodDescription)
        {
            foreach (var initMethod in classBuildContext.ClassDeclaration.Methods)
            {
                if (initMethod.MethodName == "Initialize")
                {
                    initMethodDescription = initMethod;
                    return true;
                }
            }
            initMethodDescription = null;
            return false;
        }
        public override UVariableDeclaration BuildVariableDeclaration(ref FClassBuildContext classBuildContext)
        {
            //generate code in initialize method for binding pose
            if(!classBuildContext.IsGenerateBindingPoseInit)
            {
                classBuildContext.IsGenerateBindingPoseInit = true;
                UMethodDeclaration initMethodDescription = null;
                if (TryInitializeMethodInClassDescription(ref classBuildContext, out var existInitMethodDescription))
                {
                    initMethodDescription = existInitMethodDescription;
                }
                else
                {
                    initMethodDescription = TtASTBuildUtil.CreateMethodDeclaration("Initialize", null, null);
                    classBuildContext.ClassDeclaration.Methods.Add(initMethodDescription);
                }
                var animStateMachineContextVar = TtASTBuildUtil.CreateVariableDeclaration("AnimStateMachineContext", new UTypeReference(typeof(TtAnimStateMachineContext)), new UNullValueExpression());
                classBuildContext.ClassDeclaration.Properties.Add(animStateMachineContextVar);
                initMethodDescription.MethodBody.Sequence.Insert(0, animStateMachineContextVar);
            }
            //generate code in tick method for anim

            return TtASTBuildUtil.CreateVariableDeclaration(this, ref classBuildContext);
        }

        #region Internal AST Build
        private UMethodDeclaration BuildOverrideInitializeMethod(ref FClassBuildContext classBuildContext)
        {
            var methodDeclaration = TtAnimASTBuildUtil.CreateOverridedInitMethodStatement();

            foreach (var compoundState in CompoundStates)
            {
                var compoundStateAssignLH = new UVariableReferenceExpression(compoundState.VariableName);
                var compoundStateAssignRH = new UCreateObjectExpression(compoundState.VariableType.TypeFullName);
                var compoundStateAssign = TtASTBuildUtil.CreateAssignOperatorStatement(compoundStateAssignLH, compoundStateAssignRH);
                methodDeclaration.MethodBody.Sequence.Add(compoundStateAssign);

                var stateMachineAssignLH = new UVariableReferenceExpression("StateMachine", new UVariableReferenceExpression(compoundState.VariableName));
                var stateMachineAssignRH = new USelfReferenceExpression();
                var stateMachineAssign = TtASTBuildUtil.CreateAssignOperatorStatement(stateMachineAssignLH, stateMachineAssignRH);
                methodDeclaration.MethodBody.Sequence.Add(stateMachineAssign);
            }
            foreach (var compoundState in CompoundStates)
            {
                var initializeMethodInvoke = new UMethodInvokeStatement();
                initializeMethodInvoke.Host = new UVariableReferenceExpression(compoundState.VariableName);
                initializeMethodInvoke.MethodName = "Initialize";
                initializeMethodInvoke.Arguments.Add(new UMethodInvokeArgumentExpression { Expression = new UVariableReferenceExpression("context") });
                initializeMethodInvoke.IsAsync = true;
                methodDeclaration.MethodBody.Sequence.Add(initializeMethodInvoke);
            }
            var returnValueAssign = TtASTBuildUtil.CreateAssignOperatorStatement(
                                        new UVariableReferenceExpression(methodDeclaration.ReturnValue.VariableName),
                                        new UPrimitiveExpression(true));
            methodDeclaration.MethodBody.Sequence.Add(returnValueAssign);
            return methodDeclaration;
        }
        #endregion
    }
}
