using EngineNS.Bricks.CodeBuilder;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Design.ConnectingLine;
using EngineNS.DesignMacross.Design.Statement;
using EngineNS.Rtti;

namespace EngineNS.DesignMacross.Design.Expressions
{
    [GraphElement(typeof(TtGraphElement_VarSet))]
    public class TtVarSetDescription : TtStatementDescription
    {
        public override string Name
        {
            get
            {
                if (mVariableDescription != null)
                {
                    return "Var" + mVariableDescription.Name;
                }
                return "";
            }
        }
        TtVariableDescription mVariableDescription = null;
        [Rtti.Meta]
        public Guid VariableId { get; set; } = Guid.Empty;
        public TtTypeDesc VarTypeDesc { get => mVariableDescription?.VariableType.TypeDesc; }

        public TtVarSetDescription()
        {
            AddExecutionInPin(new() { Name = "" });
            AddExecutionOutPin(new() { Name = "" });
            AddDataInPin(new() { Name = "Set", TypeDesc = TtTypeDesc.TypeOf<bool>() });
            AddDataOutPin(new() { Name = "Get", TypeDesc = TtTypeDesc.TypeOf<bool>() });
        }
        public TtVarSetDescription(Guid varId, TtTypeDesc varTypeDesc)
        {
            VariableId = varId;
            AddExecutionInPin(new() { Name = "" });
            AddExecutionOutPin(new() { Name = "" });
            AddDataInPin(new() { Name = "Set", TypeDesc = varTypeDesc });
            AddDataOutPin(new() { Name = "Get", TypeDesc = varTypeDesc });
        }

        public override void UpdateData(ref FDescriptionUpdateContext updateContext)
        {
            if (mVariableDescription == null && updateContext.ClassDescription is TtClassDescription classDesc)
            {
                foreach (var variable in classDesc.Variables)
                {
                    if (variable.Id == VariableId)
                    {
                        mVariableDescription = variable as TtVariableDescription;
                    }
                }
            }
            var outPin = DataOutPins[0];
            var inPin = DataInPins[0];
            if (VarTypeDesc != null && outPin.TypeDesc != VarTypeDesc)
            {
                outPin.TypeDesc = VarTypeDesc;
                inPin.TypeDesc = VarTypeDesc;
            }
            base.UpdateData(ref updateContext);
        }

        public override TtStatementBase BuildStatement(ref FStatementBuildContext statementBuildContext)
        {
            var linkedDataPin = statementBuildContext.MethodDescription.GetLinkedDataPin(DataPins[0]);
            if (linkedDataPin != null)
            {
                System.Diagnostics.Debug.Assert(linkedDataPin is TtDataOutPinDescription);
                FExpressionBuildContext buildContext = new() { MethodDescription = statementBuildContext.MethodDescription };
                if (linkedDataPin.Parent is TtExpressionDescription expressionDescription)
                {
                    var right = (linkedDataPin.Parent as TtExpressionDescription).BuildExpression(ref buildContext);
                    var assign = TtASTBuildUtil.CreateAssignOperatorStatement(
                                    new TtVariableReferenceExpression(mVariableDescription.VariableName, new TtVariableReferenceExpression("CenterData")),
                                    right);
                    statementBuildContext.AddStatement(assign);
                }
                if (linkedDataPin.Parent is TtStatementDescription statementDescription)
                {
                    var right = (linkedDataPin.Parent as TtStatementDescription).BuildExpressionForOutPin(linkedDataPin);
                    var assign = TtASTBuildUtil.CreateAssignOperatorStatement(
                                    new TtVariableReferenceExpression(mVariableDescription.VariableName, new TtSelfReferenceExpression()), 
                                    right);
                    statementBuildContext.AddStatement(assign);
                }
            }
            else
            {
                var right = new TtDefaultValueExpression() { Type = new TtTypeReference(linkedDataPin.TypeDesc) };
                var assign = TtASTBuildUtil.CreateAssignOperatorStatement(
                                new TtVariableReferenceExpression(mVariableDescription.VariableName, new TtVariableReferenceExpression("CenterData")),
                                right);
                statementBuildContext.AddStatement(assign);
            }

            var executionOutPin = ExecutionOutPins[0];
            var linkedExecPin = statementBuildContext.MethodDescription.GetLinkedExecutionPin(executionOutPin);
            if (linkedExecPin != null)
            {
                FStatementBuildContext buildContext = new() { ExecuteSequenceStatement = new(), MethodDescription = statementBuildContext.MethodDescription };
                (linkedExecPin.Parent as TtStatementDescription).BuildStatement(ref buildContext);
                statementBuildContext.AddStatement(buildContext.ExecuteSequenceStatement);
            }
            

            return base.BuildStatement(ref statementBuildContext);
        }
        public override TtExpressionBase BuildExpressionForOutPin(TtDataPinDescription pin)
        {
            return new TtVariableReferenceExpression(Name);
        }
    }
}
