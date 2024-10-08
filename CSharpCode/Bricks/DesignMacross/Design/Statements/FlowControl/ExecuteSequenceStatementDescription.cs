using EngineNS.Bricks.CodeBuilder;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Design.ConnectingLine;
using EngineNS.DesignMacross.Design.Expressions;
using EngineNS.DesignMacross.Design.Statements;
using EngineNS.Rtti;
using NPOI.OpenXmlFormats.Dml;

namespace EngineNS.DesignMacross.Design.Statement
{
    [GraphElement(typeof(TtGraphElement_ExecuteSequenceOutPin))]
    public class TtExecuteSequenceOutPinDescription : TtExecutionOutPinDescription
    {
        [Rtti.Meta]
        public bool Deleteable { get; set; } = true;
    }

    [GraphElement(typeof(TtGraphElement_StatementDescription))]
    [ContextMenu("ExecuteSequence", "FlowControl\\ExecuteSequence", UDesignMacross.MacrossScriptEditorKeyword)]
    public class TtExecuteSequenceStatementDescription : TtStatementDescription
    {
        public TtExecuteSequenceStatementDescription() 
        {
            Name = "ExecuteSequence";
            AddExecutionInPin(new());
            AddExecutionOutPin(new());
            AddExecutionOutPin(new TtExecuteSequenceOutPinDescription()
            { 
                Deleteable= false,
            });
        }

        public override TtStatementBase BuildStatement(ref FStatementBuildContext statementBuildContext)
        {
            var methodDesc = statementBuildContext.MethodDescription as TtMethodDescription;
            TtExecuteSequenceStatement retValue = new TtExecuteSequenceStatement();
            for(int i=0; i< ExecutionOutPins.Count; i++)
            {
                var pin = ExecutionOutPins[i];
                var linkedPin = methodDesc.GetLinkedExecutionPin(pin);
                if(linkedPin != null)
                {
                    var statement = (linkedPin.Parent as TtStatementDescription).BuildStatement(ref statementBuildContext);
                    retValue.Sequence.Add(statement);
                }
            }
            statementBuildContext.AddStatement(retValue);
            return retValue;
        }
    }
}
