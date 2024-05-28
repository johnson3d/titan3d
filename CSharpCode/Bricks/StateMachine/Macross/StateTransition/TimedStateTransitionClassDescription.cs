using EngineNS.Bricks.CodeBuilder;
using EngineNS.Bricks.StateMachine.Macross.CompoundState;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Design;
using EngineNS.Rtti;
using System.Diagnostics;

namespace EngineNS.Bricks.StateMachine.Macross.StateTransition
{
    [Graph(typeof(TtGraph_TimedStateTransition))]
    public class TtTransitionCheckConditionMethodDescription : TtMethodDescription
    {

    }
    [GraphElement(typeof(TtGraphElement_TimedStateTransition))]
    public class TtTimedStateTransitionClassDescription : TtDesignableVariableDescription
    {
        public override string Name
        {
            get
            {
                var FromName = From == null ? FromId.ToString() : From.Name;
                var ToName = To == null ? ToId.ToString() : To.Name;
                return $"Transition_From_{FromName}_To_{ToName}";
            }
            set { }
        }

        [Rtti.Meta]
        public Guid FromId { get; set; }
        [Rtti.Meta]
        public Guid ToId { get; set; }
        private IDescription mFrom = null;
        public IDescription From
        {
            get
            {
                if (mFrom == null)
                {
                    if (Parent != null && Parent.Parent is TtTimedCompoundStateClassDescription compoundStateDesc)
                    {
                        if (compoundStateDesc.Entry.Id == FromId)
                        {
                            mFrom = compoundStateDesc.Entry;
                        }
                        else
                        {
                            mFrom = compoundStateDesc.States.Find((candidate) => { return candidate.Id == FromId; });
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
                Debug.Assert(mFrom.Id == FromId);
                return mFrom;
            }
        }
        private IDescription mTo = null;
        public IDescription To
        {
            get
            {
                if (mTo == null)
                {
                    if (Parent != null && Parent.Parent is TtTimedCompoundStateClassDescription compoundStateDesc)
                    {
                        mTo = compoundStateDesc.States.Find((candidate) => { return candidate.Id == ToId; });
                        if (mTo == null)
                        {
                            mTo = compoundStateDesc.Hubs.Find((candidate) => { return candidate.Id == ToId; });
                        }
                        Debug.Assert(mTo != null);
                    }
                    else
                    {
                        return null;
                    }
                }
                Debug.Assert(mTo.Id == ToId);
                return mTo;
            }
        }

        [Rtti.Meta]
        public TtTransitionCheckConditionMethodDescription CheckConditionMethodDescription { get; set; } = null;
        public TtTimedStateTransitionClassDescription()
        {
            CheckConditionMethodDescription = new TtTransitionCheckConditionMethodDescription()
            {
                Name = "CheckCondition",
                Parent = this,
                IsOverride = true,
            };
            CheckConditionMethodDescription.SetReturnValue(UTypeDesc.TypeOf<bool>());
            //OverrideCheckConditionMethodDescription.Arguments.Add(new UMethodArgumentDeclaration { OperationType = EMethodArgumentAttribute.In, VariableType = new UTypeReference(UTypeDesc.TypeOf<TtStateMachineContext>()), VariableName = "context" });
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
