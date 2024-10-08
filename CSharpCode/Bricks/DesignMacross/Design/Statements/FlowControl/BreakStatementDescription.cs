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
    [ContextMenu("Break", "FlowControl\\Break", UDesignMacross.MacrossScriptEditorKeyword)]
    public class TtBreakStatementDescription : TtStatementDescription
    {
        public TtBreakStatementDescription()
        {
            Name = "Break";
            AddExecutionInPin(new());
        }
        public override TtStatementBase BuildStatement(ref FStatementBuildContext statementBuildContext)
        {
            var retValue = new TtBreakStatement();
            statementBuildContext.AddStatement(retValue);
            return retValue;
        }
    }
}
