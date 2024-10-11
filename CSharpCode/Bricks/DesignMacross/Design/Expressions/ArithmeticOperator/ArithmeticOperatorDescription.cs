using EngineNS.Animation.Macross.BlendTree;
using EngineNS.Bricks.CodeBuilder;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Design.ConnectingLine;
using EngineNS.Rtti;
using Mono.Cecil;

namespace EngineNS.DesignMacross.Design.Expressions
{
    public class TtBinaryArithmeticOperatorDescription : TtExpressionDescription
    {
        [Rtti.Meta]
        public TtBinaryOperatorExpression.EBinaryOperation Op { get; set; }
        public TtBinaryArithmeticOperatorDescription()
        {
            AddDataInPin(new() { Name = "" });
            AddDataInPin(new() { Name = "" });
            AddDataOutPin(new() { Name = "=" });
            
        }

        public override TtExpressionBase BuildExpression(ref FExpressionBuildContext expressionBuildContext)
        {
            var methodDesc = expressionBuildContext.MethodDescription as TtMethodDescription;

            TtBinaryOperatorExpression expression = new();
            expression.Operation = Op;
            var dataInPin_Left = DataInPins[0];
            var dataInPin_Right = DataInPins[1];
            var leftLinkedDataPin = methodDesc.GetLinkedDataPin(dataInPin_Left);
            if (leftLinkedDataPin != null)
            {
                System.Diagnostics.Debug.Assert(leftLinkedDataPin is TtDataOutPinDescription);
                FExpressionBuildContext buildContext = new() { MethodDescription = expressionBuildContext.MethodDescription };
                expression.Left = (leftLinkedDataPin.Parent as TtExpressionDescription).BuildExpression(ref buildContext);
            }
            var rightLinkedDataPin = methodDesc.GetLinkedDataPin(dataInPin_Right);
            if (rightLinkedDataPin != null)
            {
                System.Diagnostics.Debug.Assert(rightLinkedDataPin is TtDataOutPinDescription);
                FExpressionBuildContext buildContext = new() { MethodDescription = expressionBuildContext.MethodDescription };
                expression.Right = (rightLinkedDataPin.Parent as TtExpressionDescription).BuildExpression(ref buildContext);
            }
            return expression;
        }
        public void PinTypeSpreading(TtDataPinDescription dataPin, TtMethodDescription methodDescription)
        {
            foreach(var otherPin in DataInPins)
            {
                if (otherPin == dataPin)
                    continue;

                if(dataPin.TypeDesc != null)
                {
                    if(otherPin.TypeDesc == null)
                    {
                        otherPin.TypeDesc = dataPin.TypeDesc;
                    }
                }
                else
                {
                    if(otherPin.TypeDesc != null)
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
                        var linkedPin = methodDescription.GetLinkedDataPin(dataPin);
                        if(linkedPin != null && linkedPin.TypeDesc == null)
                        {
                            if(linkedPin.Parent is TtBinaryArithmeticOperatorDescription valueOperatorDescription)
                            {
                                linkedPin.TypeDesc = dataPin.TypeDesc;
                                valueOperatorDescription.PinTypeSpreading(linkedPin, methodDescription);
                            }
                            if (linkedPin.Parent is TtBinaryLogicOperatorDescription logicOperatorDescription)
                            {
                                linkedPin.TypeDesc = dataPin.TypeDesc;
                                logicOperatorDescription.PinTypeSpreading(linkedPin, methodDescription);
                            }

                        }
                    }
                }
            }
        }
        public override void OnPinConnected(TtDataPinDescription selfPin, TtDataPinDescription connectedPin, TtMethodDescription methodDescription)
        {
            if(selfPin.TypeDesc == null)
            {
                selfPin.TypeDesc = connectedPin.TypeDesc;
                PinTypeSpreading(selfPin, methodDescription);
            }
        }
        public override void OnPinDisConnected(TtDataPinDescription selfPin, TtDataPinDescription disConnectedPin, TtMethodDescription methodDescription)
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
        [Rtti.Meta]
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
        public override bool IsPinsLinkable(TtDataPinDescription selfPin, TtDataPinDescription targetPin)
        {
            if (selfPin == TargetPin && selfPin.TypeDesc != null && selfPin.TypeDesc.SystemType.IsArray)
            {
                return false;
            }
            return base.IsPinsLinkable(selfPin, targetPin);
        }
        public override void OnPinConnected(TtDataPinDescription selfPin, TtDataPinDescription connectedPin, TtMethodDescription methodDescription)
        {
            if (selfPin == TargetPin && selfPin.TypeDesc.SystemType.IsArray)
            {
                ArrayDimension = selfPin.TypeDesc.SystemType.GetArrayRank();
            }
        }
        public override TtExpressionBase BuildExpression(ref FExpressionBuildContext expressionBuildContext)
        {
            var methodDesc = expressionBuildContext.MethodDescription as TtMethodDescription;
            var expression = new TtIndexerOperatorExpression();
            var leftLinkedDataPin = methodDesc.GetLinkedDataPin(TargetPin);
            if (leftLinkedDataPin != null)
            {
                System.Diagnostics.Debug.Assert(leftLinkedDataPin is TtDataOutPinDescription);
                FExpressionBuildContext buildContext = new() { MethodDescription = expressionBuildContext.MethodDescription };
                expression.Target = (leftLinkedDataPin.Parent as TtExpressionDescription).BuildExpression(ref buildContext);
            }

            expression.Indices.Clear();
            for (int i = 0; i < ArrayDimension; i++)
            {
                var dataInPin_index = DataInPins[1 + i];
                var indexLinkedDataPin = methodDesc.GetLinkedDataPin(dataInPin_index);
                if (indexLinkedDataPin != null)
                {
                    System.Diagnostics.Debug.Assert(indexLinkedDataPin is TtDataOutPinDescription);
                    FExpressionBuildContext buildContext = new() { MethodDescription = expressionBuildContext.MethodDescription };
                    expression.Indices.Add((indexLinkedDataPin.Parent as TtExpressionDescription).BuildExpression(ref buildContext));
                }
            }
            return expression;
        }
    }
}
