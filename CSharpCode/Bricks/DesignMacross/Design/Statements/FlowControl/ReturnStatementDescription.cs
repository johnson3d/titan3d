using EngineNS.Bricks.CodeBuilder;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Render;
using EngineNS.DesignMacross.Design.ConnectingLine;
using EngineNS.DesignMacross.Design.Expressions;
using EngineNS.Rtti;

namespace EngineNS.DesignMacross.Design.Statement
{
    [GraphElement(typeof(TtGraphElement_ReturnStatementDescription), 500, 400)]
    [ContextMenu("Return", "FlowControl\\Return", UDesignMacross.MacrossScriptEditorKeyword)]
    public class TtReturnStatementDescription : TtStatementDescription
    {
        [Rtti.Meta]
        public TtTypeDesc ReturnType { get => MethodDescription.ReturnValueType; }
        public TtMethodDescription MethodDescription { get => Parent as TtMethodDescription; }
        public override IDescription Parent 
        { 
            get => base.Parent;
            set
            {
                base.Parent = value;
                AddDataInPin(new() { TypeDesc = ReturnType });
            }
        }
        public bool ShowExecPin = true;
        public TtReturnStatementDescription() 
        {
            Name = "Return";
            AddExecutionInPin(new());
        }
        
        public void SetReturnType(TtTypeDesc typeDesc)
        {
            //ReturnType = typeDesc;
            
        }
        public override TtStatementBase BuildStatement(ref FStatementBuildContext statementBuildContext)
        {
            var linkedPin = statementBuildContext.MethodDescription.GetLinkedDataPin(DataInPins[0]);
            if(linkedPin != null)
            {
                TtExpressionBase rightSide = null;
                if(linkedPin.Parent is TtExpressionDescription expressionDescription)
                {
                    FExpressionBuildContext buildContext = new() { MethodDescription = statementBuildContext.MethodDescription };
                    rightSide = expressionDescription.BuildExpression(ref buildContext);
                }
                if(linkedPin.Parent is TtStatementDescription statementDescription)
                {
                    rightSide = statementDescription.BuildExpressionForOutPin(linkedPin);
                }
                var returnValueAssign = TtASTBuildUtil.CreateAssignOperatorStatement(new TtVariableReferenceExpression("returnedValue"), rightSide);
                statementBuildContext.AddStatement(returnValueAssign);
            }

            TtReturnStatement returnStatement = new();
            statementBuildContext.AddStatement(returnStatement);
            return returnStatement;
        }
    }
    [ImGuiElementRender(typeof(TtGraphElementRender_StatementDescription))]
    public class TtGraphElement_ReturnStatementDescription : TtGraphElement_StatementDescription
    {
        public TtReturnStatementDescription ReturnStatementDescription { get => Description as TtReturnStatementDescription; }
        public TtGraphElement_ReturnStatementDescription(IDescription description, IGraphElementStyle style) : base(description, style)
        {
        }
        public override void OnSelected(ref FGraphElementRenderingContext context)
        {
            context.EditorInteroperation.PGMember.Target = Description.Parent;
        }
    }
}
