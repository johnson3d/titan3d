using EngineNS.Animation.Macross.BlendTree;
using EngineNS.Bricks.CodeBuilder;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Design.ConnectingLine;
using EngineNS.DesignMacross.Design.Statement;
using EngineNS.Rtti;
using Mono.Cecil;

namespace EngineNS.DesignMacross.Design.Expressions
{
    public class TtBinaryArithmeticOperatorDescription : TtExpressionDescription
    {
        [Rtti.Meta("")]
        public TtBinaryOperatorExpression.EBinaryOperation Op { get; set; }
        public TtBinaryArithmeticOperatorDescription()
        {
            AddDataInPin(new() { Name = "" });
            AddDataInPin(new() { Name = "" });
            AddDataOutPin(new() { Name = "=" });

        }

        public override bool IsDataPinsLinkable(TtDataPinDescription selfPin, TtDataPinDescription targetPin)
        {
            if (base.IsDataPinsLinkable(selfPin, targetPin))
            {
                return true;
            }
            if (selfPin.TypeDesc == null)
            {
                return true;
            }
            return false;
        }

        public override TtExpressionBase BuildExpression(ref FExpressionBuildContext expressionBuildContext)
        {
            var graphDesc = expressionBuildContext.OwnerDescription;

            TtBinaryOperatorExpression expression = new();
            expression.Operation = Op;
            var dataInPin_Left = DataInPins[0];
            var dataInPin_Right = DataInPins[1];
            var leftLinkedDataPin = IDataLineOperator.GetLinkedDataPin(graphDesc, dataInPin_Left);
            if (leftLinkedDataPin != null)
            {
                System.Diagnostics.Debug.Assert(leftLinkedDataPin is TtDataOutPinDescription);
                if (leftLinkedDataPin.Parent is TtExpressionDescription expressionDescription)
                {
                    FExpressionBuildContext buildContext = new() { OwnerDescription = expressionBuildContext.OwnerDescription, ClassBuildContext = expressionBuildContext.ClassBuildContext };
                    expression.Left = (leftLinkedDataPin.Parent as TtExpressionDescription).BuildExpression(ref buildContext);
                }
                if (leftLinkedDataPin.Parent is TtPureStatementDescription pureStatementDescription)
                {
                    FStatementBuildContext buildContext = new() { ExecuteSequenceStatement = expressionBuildContext.Sequence, OwnerDescription = expressionBuildContext.OwnerDescription, ClassBuildContext = expressionBuildContext.ClassBuildContext };
                    var pureState = (leftLinkedDataPin.Parent as TtStatementDescription).BuildStatement(ref buildContext);
                    expressionBuildContext.Sequence.Sequence.Add(pureState);
                    expression.Left = (leftLinkedDataPin.Parent as TtStatementDescription).BuildExpressionForOutPin(leftLinkedDataPin);
                }
                else if (leftLinkedDataPin.Parent is TtStatementDescription statementDescription)
                {
                    expression.Left = (leftLinkedDataPin.Parent as TtStatementDescription).BuildExpressionForOutPin(leftLinkedDataPin);
                }
            }
            var rightLinkedDataPin = IDataLineOperator.GetLinkedDataPin(graphDesc, dataInPin_Right);
            if (rightLinkedDataPin != null)
            {
                System.Diagnostics.Debug.Assert(rightLinkedDataPin is TtDataOutPinDescription);

                if (rightLinkedDataPin.Parent is TtExpressionDescription expressionDescription)
                {
                    FExpressionBuildContext buildContext = new() { OwnerDescription = expressionBuildContext.OwnerDescription, ClassBuildContext = expressionBuildContext.ClassBuildContext };
                    expression.Right = (rightLinkedDataPin.Parent as TtExpressionDescription).BuildExpression(ref buildContext);
                }
                if (rightLinkedDataPin.Parent is TtPureStatementDescription pureStatementDescription)
                {
                    FStatementBuildContext buildContext = new() { ExecuteSequenceStatement = expressionBuildContext.Sequence, OwnerDescription = expressionBuildContext.OwnerDescription, ClassBuildContext = expressionBuildContext.ClassBuildContext };
                    var pureState = (rightLinkedDataPin.Parent as TtStatementDescription).BuildStatement(ref buildContext);
                    expressionBuildContext.Sequence.Sequence.Add(pureState);
                    expression.Right = (rightLinkedDataPin.Parent as TtStatementDescription).BuildExpressionForOutPin(rightLinkedDataPin);
                }
                else if (rightLinkedDataPin.Parent is TtStatementDescription statementDescription)
                {
                    expression.Right = (rightLinkedDataPin.Parent as TtStatementDescription).BuildExpressionForOutPin(rightLinkedDataPin);
                }
            }
            return expression;
        }
        public void PinTypeSpreading(TtDataPinDescription dataPin, IDescription graphDescription)
        {
            foreach (var otherPin in DataInPins)
            {
                if (otherPin == dataPin)
                    continue;

                if (dataPin.TypeDesc != null)
                {
                    if (otherPin.TypeDesc == null)
                    {
                        otherPin.TypeDesc = dataPin.TypeDesc;
                    }
                }
                else
                {
                    if (otherPin.TypeDesc != null)
                    {
                        dataPin.TypeDesc = otherPin.TypeDesc;
                    }
                }
            }
            foreach (var otherPin in DataOutPins)
            {
                if (otherPin == dataPin)
                    continue;

                if (dataPin.TypeDesc != null)
                {
                    if (otherPin.TypeDesc == null)
                    {
                        otherPin.TypeDesc = dataPin.TypeDesc;
                        var linkedPin = IDataLineOperator.GetLinkedDataPin(graphDescription, dataPin);
                        if (linkedPin != null && linkedPin.TypeDesc == null)
                        {
                            if (linkedPin.Parent is TtBinaryArithmeticOperatorDescription valueOperatorDescription)
                            {
                                linkedPin.TypeDesc = dataPin.TypeDesc;
                                valueOperatorDescription.PinTypeSpreading(linkedPin, graphDescription);
                            }
                            if (linkedPin.Parent is TtBinaryLogicOperatorDescription logicOperatorDescription)
                            {
                                linkedPin.TypeDesc = dataPin.TypeDesc;
                                logicOperatorDescription.PinTypeSpreading(linkedPin, graphDescription);
                            }

                        }
                    }
                }
            }
        }
        public override void OnDataPinConnected(TtDataPinDescription selfPin, TtDataPinDescription connectedPin, IDescription graphDescription)
        {
            if (selfPin.TypeDesc == null)
            {
                selfPin.TypeDesc = connectedPin.TypeDesc;
                PinTypeSpreading(selfPin, graphDescription);
            }
        }
        public override void OnDataPinDisConnected(TtDataPinDescription selfPin, TtDataPinDescription disConnectedPin, IDescription graphDescription)
        {
            selfPin.TypeDesc = null;
        }
    }

    [ContextMenu("indexer,[]", "ValueOperation\\[]", UDesignMacross.MacrossScriptEditorKeyword)]
    [GraphElement(typeof(TtGraphElement_ValueOperator))]
    public class TtIndexerOperatorDescription : TtExpressionDescription
    {
        public override string Name
        {
            get
            {
                return "[*]";
            }
        }
        int mArrayDimension = 0;
        [Rtti.Meta("")]
        public int ArrayDimension
        {
            get => mArrayDimension;
            set
            {
                mArrayDimension = value;
                DataInPins.RemoveAt(1);
                for (int i = 0; i < mArrayDimension; i++)
                {
                    AddDataInPin(new() { Name = $"dim{i}", TypeDesc = TtTypeDescGetter<int>.TypeDesc });
                }
            }
        }
        TtDataInPinDescription TargetPin = new() { Name = "Array" };
        public TtIndexerOperatorDescription()
        {
            AddDataInPin(TargetPin);

            AddDataOutPin(new() { Name = "[]" });
        }
        public override bool IsDataPinsLinkable(TtDataPinDescription selfPin, TtDataPinDescription targetPin)
        {
            if (selfPin == TargetPin && selfPin.TypeDesc != null && selfPin.TypeDesc.SystemType.IsArray)
            {
                return false;
            }
            return base.IsDataPinsLinkable(selfPin, targetPin);
        }
        public override void OnDataPinConnected(TtDataPinDescription selfPin, TtDataPinDescription connectedPin, IDescription graphDescription)
        {
            if (selfPin == TargetPin && selfPin.TypeDesc.SystemType.IsArray)
            {
                ArrayDimension = selfPin.TypeDesc.SystemType.GetArrayRank();
            }
        }
        public override TtExpressionBase BuildExpression(ref FExpressionBuildContext expressionBuildContext)
        {
            var graphDesc = expressionBuildContext.OwnerDescription;
            var expression = new TtIndexerOperatorExpression();
            var leftLinkedDataPin = IDataLineOperator.GetLinkedDataPin(graphDesc, TargetPin);
            if (leftLinkedDataPin != null)
            {
                System.Diagnostics.Debug.Assert(leftLinkedDataPin is TtDataOutPinDescription);
                FExpressionBuildContext buildContext = new() { OwnerDescription = expressionBuildContext.OwnerDescription, ClassBuildContext = expressionBuildContext.ClassBuildContext };
                expression.Target = (leftLinkedDataPin.Parent as TtExpressionDescription).BuildExpression(ref buildContext);
            }

            expression.Indices.Clear();
            for (int i = 0; i < ArrayDimension; i++)
            {
                var dataInPin_index = DataInPins[1 + i];
                var indexLinkedDataPin = IDataLineOperator.GetLinkedDataPin(graphDesc, dataInPin_index);
                if (indexLinkedDataPin != null)
                {
                    System.Diagnostics.Debug.Assert(indexLinkedDataPin is TtDataOutPinDescription);
                    FExpressionBuildContext buildContext = new() { OwnerDescription = expressionBuildContext.OwnerDescription, ClassBuildContext = expressionBuildContext.ClassBuildContext };
                    expression.Indices.Add((indexLinkedDataPin.Parent as TtExpressionDescription).BuildExpression(ref buildContext));
                }
            }
            return expression;
        }
    }
}
