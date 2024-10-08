using EngineNS.Bricks.CodeBuilder;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Design.Statement;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.DesignMacross.Design.Statements
{
    [GraphElement(typeof(TtGraphElement_StatementDescription))]
    [ContextMenu("Continue", "FlowControl\\Continue", UDesignMacross.MacrossScriptEditorKeyword)]
    public class TtContinueStatementDescription : TtStatementDescription
    {
        public TtContinueStatementDescription()
        {
            Name = "Continue";
            AddExecutionInPin(new());
        }
        public override TtStatementBase BuildStatement(ref FStatementBuildContext statementBuildContext)
        {
            var retValue = new TtContinueStatement();
            statementBuildContext.AddStatement(retValue);
            return retValue;
        }
    }
}
