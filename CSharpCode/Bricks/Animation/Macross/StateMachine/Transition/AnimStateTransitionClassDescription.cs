using EngineNS.Bricks.CodeBuilder;
using EngineNS.Bricks.StateMachine.Macross.CompoundState;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Design;
using EngineNS.Rtti;
using System.Diagnostics;

namespace EngineNS.Bricks.StateMachine.Macross.StateTransition
{
    [GraphElement(typeof(TtGraphElement_TimedStateTransition))]
    public class TtAnimStateTransitionClassDescription : TtTimedStateTransitionClassDescription
    {
        public TtAnimStateTransitionClassDescription() 
        {
            
        }

        public override List<UClassDeclaration> BuildClassDeclarations(ref FClassBuildContext classBuildContext)
        {
            SupperClassNames.Clear();
            SupperClassNames.Add($"EngineNS.Bricks.StateMachine.TimedSM.TtTimedStateTransition<{classBuildContext.MainClassDescription.ClassName}>");
            UClassDeclaration thisClassDeclaration = TtDescriptionASTBuildUtil.BuildDefaultPartForClassDeclaration(this, ref classBuildContext);
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
            return TtDescriptionASTBuildUtil.BuildDefaultPartForVariableDeclaration(this, ref classBuildContext);
        }

        #region Internal AST Build
        public UMethodDeclaration BuildOverrideCheckConditionMethod(ref FClassBuildContext classBuildContext)
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
