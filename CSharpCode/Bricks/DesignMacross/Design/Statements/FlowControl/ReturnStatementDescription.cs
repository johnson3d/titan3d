using EngineNS.Bricks.CodeBuilder;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Design.ConnectingLine;
using EngineNS.DesignMacross.Design.Expressions;
using EngineNS.Rtti;

namespace EngineNS.DesignMacross.Design.Statement
{
    [GraphElement(typeof(TtGraphElement_StatementDescription))]
    [ContextMenu("Return", "FlowControl\\Return", UDesignMacross.MacrossScriptEditorKeyword)]
    public class TtReturnStatementDescription : TtStatementDescription
    {
        [Rtti.Meta]
        public TtTypeDesc ReturnType { get; private set; } = null;
        public bool ShowExecPin = true;
        public TtReturnStatementDescription() 
        {
            Name = "Return";
            AddExecutionInPin(new());
        }
        
        public void SetReturnType(TtTypeDesc typeDesc)
        {
            ReturnType = typeDesc;
            AddDataInPin(new() { TypeDesc = typeDesc });
        }
        public override TtStatementBase BuildStatement(ref FStatementBuildContext statementBuildContext)
        {
            TtReturnStatement returnStatement = new()
            {

            };
            return returnStatement;
        }
    }
}
