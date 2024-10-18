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
        public override string Name { get => VariableDescription?.Name; set => VariableDescription.Name = value; }
        public IVariableDescription VariableDescription
        {
            get
            {
                if (ParentClass != null)
                {
                    var variable = ParentClass.GetVariable(VariableId);
                    if (variable != null)
                    {
                        return variable;
                    }
                }
                return null;
            }
        }
        [Rtti.Meta]
        public Guid VariableId { get; set; } = Guid.Empty;
        public TtTypeDesc VarTypeDesc
        {
            get 
            {
                if(ParentClass != null)
                {
                    var variable = ParentClass.GetVariable(VariableId);
                    if(variable != null)
                    {
                        return variable.VariableType.TypeDesc;
                    }
                }
                return null;
            }
        }
        public TtClassDescription ParentClass { get => Parent.Parent as TtClassDescription; }

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
                                    new TtVariableReferenceExpression(Name), right);
                    statementBuildContext.AddStatement(assign);
                }
                if (linkedDataPin.Parent is TtStatementDescription statementDescription)
                {
                    var right = (linkedDataPin.Parent as TtStatementDescription).BuildExpressionForOutPin(linkedDataPin);
                    var assign = TtASTBuildUtil.CreateAssignOperatorStatement(
                                    new TtVariableReferenceExpression(Name), right);
                    statementBuildContext.AddStatement(assign);
                }
            }
            else
            {
                var right = new TtDefaultValueExpression() { Type = new TtTypeReference(linkedDataPin.TypeDesc) };
                var assign = TtASTBuildUtil.CreateAssignOperatorStatement(
                                new TtVariableReferenceExpression(Name), right);
                statementBuildContext.AddStatement(assign);
            }
            
            return base.BuildStatement(ref statementBuildContext);
        }
        public override TtExpressionBase BuildExpressionForOutPin(TtDataPinDescription pin)
        {
            return new TtVariableReferenceExpression(Name);
        }
    }
}
