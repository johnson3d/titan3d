using EngineNS.Bricks.CodeBuilder;
using EngineNS.Bricks.CodeBuilder.MacrossNode;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Design.ConnectingLine;
using EngineNS.DesignMacross.Design.Expressions;
using EngineNS.DesignMacross.Design.Statement;
using EngineNS.Rtti;

namespace EngineNS.DesignMacross.Design.Statement
{
    [GraphElement(typeof(TtGraphElement_MethodInvokeReflected))]
    public class TtMethodInvokeReflectedDescription : TtStatementDescription
    {
        [Rtti.Meta]
        public TtClassMeta.TtMethodMeta MethodMeta { get; set; } = null;
        public TtMethodInvokeReflectedDescription()
        {
            AddExecutionInPin(new());
            AddExecutionOutPin(new());
        }
        public void SetMethodMeta(TtClassMeta.TtMethodMeta methodMeta)
        {
            MethodMeta = methodMeta;
            Name = MethodMeta.MethodName;
            var paras = MethodMeta.GetParameters();
            foreach (var para in paras)
            {
                var pin = new TtDataInPinDescription
                {
                    TypeDesc = para.ParameterType,
                    Name = para.Name,
                    Parent = this
                };
                AddDataInPin(pin);
            }
            {
                var pin = new TtDataOutPinDescription
                {
                    Name = "Result",
                    Parent = this,
                    TypeDesc = MethodMeta.ReturnType
                };
                AddDataOutPin(pin);
            }
        }
        public override UStatementBase BuildStatement(ref FStatementBuildContext statementBuildContext)
        {
            var methodDesc = statementBuildContext.MethodDescription as TtMethodDescription;

            UMethodInvokeStatement methodInvoke = new UMethodInvokeStatement()
            {
                MethodName = MethodMeta.MethodName,
                Method = MethodMeta,
                Host = new UClassReferenceExpression() { Class = MethodMeta.DeclaringType }
            };
            foreach(var pin in DataInPins)
            {
                var linkedPin = methodDesc.GetLinkedDataPin(pin);
                if(linkedPin == null)
                {
                    //TODO: TtMethodInvokeReflectedDescription 要报错
                }
                else
                {
                    System.Diagnostics.Debug.Assert(linkedPin is TtDataOutPinDescription);
                    var buildContext = new FExpressionBuildContext() { MethodDescription = statementBuildContext.MethodDescription, Sequence = statementBuildContext.ExecuteSequenceStatement };
                    var expression = (linkedPin.Parent as TtExpressionDescription).BuildExpression(ref buildContext);
                    methodInvoke.Arguments.Add(new UMethodInvokeArgumentExpression(expression));
                }
            }
            statementBuildContext.AddStatement(methodInvoke);

            var executionOutPin = ExecutionOutPins[0];
            var linkedExecPin = methodDesc.GetLinkedExecutionPin(executionOutPin);
            if (linkedExecPin == null)
            {
                //空语句
            }
            else
            {
                System.Diagnostics.Debug.Assert(linkedExecPin is TtExecutionInPinDescription);
                (linkedExecPin.Parent as TtStatementDescription).BuildStatement(ref statementBuildContext);
            }
            return methodInvoke;
        }
    }
}
