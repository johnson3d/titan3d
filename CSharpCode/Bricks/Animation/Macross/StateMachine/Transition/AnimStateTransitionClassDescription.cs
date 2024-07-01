using EngineNS.Animation.StateMachine;
using EngineNS.Bricks.CodeBuilder;
using EngineNS.Bricks.StateMachine.Macross.CompoundState;
using EngineNS.DesignMacross;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Design;
using EngineNS.DesignMacross.Design.ConnectingLine;
using EngineNS.Rtti;
using System.Diagnostics;

namespace EngineNS.Bricks.StateMachine.Macross.StateTransition
{
    [GraphElement(typeof(TtGraphElement_TimedStateTransition))]
    public class TtAnimStateTransitionClassDescription : TtTimedStateTransitionClassDescription
    {
        public TtAnimStateTransitionClassDescription() 
        {
            CheckConditionMethodDescription = new TtTransitionCheckConditionMethodDescription(UTypeDesc.TypeOf<TtAnimStateMachineContext>())
            {
                Parent = this,
            };
        }

        public override List<UClassDeclaration> BuildClassDeclarations(ref FClassBuildContext classBuildContext)
        {
            SupperClassNames.Clear();
            SupperClassNames.Add($"EngineNS.Animation.StateMachine.TtAnimStateTransition<{classBuildContext.MainClassDescription.ClassName}>");
            UClassDeclaration thisClassDeclaration = TtASTBuildUtil.BuildClassDeclaration(this, ref classBuildContext);
            FClassBuildContext transitionClassBuildContext = new FClassBuildContext()
            {
                MainClassDescription = classBuildContext.MainClassDescription,
                ClassDeclaration = thisClassDeclaration,
            };
            thisClassDeclaration.AddMethod(BuildOverrideCheckConditionMethod(ref transitionClassBuildContext));
            return new List<UClassDeclaration>() { thisClassDeclaration };
        }

        public override UVariableDeclaration BuildVariableDeclaration(ref FClassBuildContext classBuildContext)
        {
            return TtASTBuildUtil.CreateVariableDeclaration(this, ref classBuildContext);
        }

        #region Internal AST Build
        private UMethodDeclaration BuildOverrideCheckConditionMethod(ref FClassBuildContext classBuildContext)
        {
            return CheckConditionMethodDescription.BuildMethodDeclaration(ref classBuildContext);
        }

        #endregion

        public override void OnPreRead(object tagObject, object hostObject, bool fromXml)
        {
            base.OnPreRead(tagObject, hostObject, fromXml);
        }
    }
}
