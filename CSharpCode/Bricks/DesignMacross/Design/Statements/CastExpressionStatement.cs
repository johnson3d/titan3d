using EngineNS.Bricks.CodeBuilder;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Design.ConnectingLine;
using EngineNS.DesignMacross.Design.Statement;
using EngineNS.DesignMacross.Design.Statements;
using EngineNS.Rtti;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.DesignMacross.Design.Expressions
{
    [ContextMenu("Cast", "Data\\Cast", UDesignMacross.MacrossScriptEditorKeyword)]
    [GraphElement(typeof(TtGraphElement_CastStatementDescription))]
    public class TtCastStatementDescription : TtStatementDescription
    {
        public TtTypeDesc TargetType
        {
            get
            {
                return DataOutPins[0].TypeDesc;
            }
            set
            {
                DataOutPins[0].TypeDesc = value;
            }
        }
        public TtTypeDesc SourceType
        {
            get
            {
                return DataInPins[0].TypeDesc;
            }
            set
            {
                DataInPins[0].TypeDesc = value;
            }
        }
        public TtDataInPinDescription SourcePin
        {
            get=> DataInPins[0];
        }
        public TtCastStatementDescription() 
        {
            Name = "Cast";
            AddExecutionInPin(new());
            AddExecutionOutPin(new() { Name = "Succeed" });
            AddExecutionOutPin(new() { Name = "Failed" });

            AddDataInPin(new());
            AddDataOutPin(new());
        }
        public override TtStatementBase BuildStatement(ref FStatementBuildContext statementBuildContext)
        {
            new TtBinaryOperatorExpression()
            {
                Operation = TtBinaryOperatorExpression.EBinaryOperation.Is,
            };
            TtCastExpression castExpression = new();
            var ifStatement = new TtIfStatement();
            return base.BuildStatement(ref statementBuildContext);
        }
        public override TtExpressionBase BuildExpressionForOutPin(TtDataPinDescription pin)
        {
            return base.BuildExpressionForOutPin(pin);
        }
        public override void OnPinConnected(TtDataPinDescription selfPin, TtDataPinDescription connectedPin, TtMethodDescription methodDescription)
        {
            if(SourcePin == selfPin)
            {
                SourceType = connectedPin.TypeDesc;
            }
        }
        public override void OnPinDisConnected(TtDataPinDescription selfPin, TtDataPinDescription connectedPin, TtMethodDescription methodDescription)
        {
            if (SourcePin == selfPin)
            {
                SourceType = null;
            }
        }
    }
}
