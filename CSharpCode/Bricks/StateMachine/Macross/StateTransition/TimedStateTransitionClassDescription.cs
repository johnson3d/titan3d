using EngineNS.Bricks.CodeBuilder;
using EngineNS.Bricks.StateMachine.Macross.CompoundState;
using EngineNS.Bricks.StateMachine.TimedSM;
using EngineNS.DesignMacross;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Design;
using EngineNS.DesignMacross.Design.ConnectingLine;
using EngineNS.Rtti;
using System.Diagnostics;

namespace EngineNS.Bricks.StateMachine.Macross.StateTransition
{
    [Graph(typeof(TtGraph_TimedStateTransition))]
    public class TtTransitionCheckConditionMethodDescription : TtMethodDescription
    {
        public TtTransitionCheckConditionMethodDescription(TtTypeDesc contextTypeDesc)
        {
            Name = "CheckCondition";
            IsOverride = true;
            ReturnValueType = TtTypeDesc.TypeOf<bool>();
            AddArgument(new TtMethodArgumentDescription { OperationType = EMethodArgumentAttribute.In, VariableType = contextTypeDesc, Name = "context" });
            var endNode = new TtMethodEndDescription() { Parent = this };
            var dataPinIn = new TtDataInPinDescription() { TypeDesc = TtTypeDesc.TypeOf<bool>() , Parent = this };
            endNode.DataInPins.Add(dataPinIn);
            AddStatement(endNode);
            AddExecutionLine(new() { Parent = this, FromId = Start.GetExecutionOutPins()[0].Id, ToId = endNode.ExecutionInPins[0].Id });
        }
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
            CheckConditionMethodDescription = new TtTransitionCheckConditionMethodDescription(TtTypeDesc.TypeOf<TtStateMachineContext>())
            {
                Parent = this,
            };
        }

        public override List<TtClassDeclaration> BuildClassDeclarations(ref FClassBuildContext classBuildContext)
        {
            SupperClassNames.Clear();
            SupperClassNames.Add($"EngineNS.Bricks.StateMachine.TimedSM.TtTimedStateTransition<{classBuildContext.MainClassDescription.ClassName}>");
            TtClassDeclaration thisClassDeclaration = TtASTBuildUtil.BuildClassDeclaration(this, ref classBuildContext);
            FClassBuildContext transitionClassBuildContext = new FClassBuildContext()
            {
                MainClassDescription = classBuildContext.MainClassDescription,
                ClassDeclaration = thisClassDeclaration,
            };
            thisClassDeclaration.AddMethod(BuildOverrideCheckConditionMethod(ref transitionClassBuildContext));
            return new List<TtClassDeclaration>() { thisClassDeclaration };
        }

        public override TtVariableDeclaration BuildVariableDeclaration(ref FClassBuildContext classBuildContext)
        {
            return TtASTBuildUtil.CreateVariableDeclaration(this, ref classBuildContext);
        }

        #region Internal AST Build
        private TtMethodDeclaration BuildOverrideCheckConditionMethod(ref FClassBuildContext classBuildContext)
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
