using EngineNS.Bricks.CodeBuilder;
using EngineNS.Bricks.CodeBuilder.MacrossNode;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Design.ConnectingLine;
using EngineNS.DesignMacross.Design.Expressions;
using EngineNS.DesignMacross.Design.Statement;
using EngineNS.Rtti;
using Org.BouncyCastle.Asn1.X509.Qualified;
using System.Reflection;

namespace EngineNS.DesignMacross.Design.Statement
{
    [GraphElement(typeof(TtGraphElement_StatementDescription))]
    public class TtMethodInvokeDescription : TtStatementDescription
    {
        public static TtMethodInvokeDescription Create(TtClassMeta.TtMethodMeta methodMeta)
        {
            var methodInvoke = new TtMethodInvokeDescription();
            methodInvoke.Name = methodMeta.MethodName;
            methodInvoke.IsStatic = methodMeta.IsStatic;
            methodInvoke.DeclaringType = methodMeta.DeclaringType;
            if (!methodInvoke.IsStatic)
            {
                methodInvoke.AddDataInPin(new() { Name = "Host", TypeDesc = methodMeta.DeclaringType });
            }
            methodInvoke.ReturnType = methodMeta.ReturnType;
            var paras = methodMeta.GetParameters();
            foreach (var para in paras)
            {
                methodInvoke.AddDataInPin(new TtDataInPinDescription
                {
                    TypeDesc = para.ParameterType,
                    Name = para.Name,
                });
            }
            //ReturnType OutPin Index 0
            if (methodMeta.ReturnType != TtTypeDesc.TypeOf(typeof(void)))
            {
                methodInvoke.AddDataOutPin(new TtDataOutPinDescription
                {
                    Name = "Result",
                    TypeDesc = methodMeta.ReturnType
                });
            }
                
            return methodInvoke;
        }
        public static TtMethodInvokeDescription Create(MethodInfo methodInfo)
        {
            var methodInvoke = new TtMethodInvokeDescription();
            methodInvoke.Name = methodInfo.Name;
            methodInvoke.DeclaringType = TtTypeDesc.TypeOf(methodInfo.DeclaringType);
            methodInvoke.IsStatic = methodInfo.IsStatic;
            if(!methodInvoke.IsStatic)
            {
                methodInvoke.AddDataInPin(new() { Name = "Host", TypeDesc = methodInvoke.DeclaringType });
            }
            foreach (var para in methodInfo.GetParameters())
            {
                methodInvoke.AddDataInPin(new() { Name = para.Name, TypeDesc = TtTypeDesc.TypeOf(para.ParameterType) });
            }
            methodInvoke.ReturnType = TtTypeDesc.TypeOf(methodInfo.ReturnType);
            if (methodInfo.ReturnType != typeof(void))
            {
                //ReturnType OutPin Index 0
                methodInvoke.AddDataOutPin(new() { Name = "Result", TypeDesc = TtTypeDesc.TypeOf(methodInfo.ReturnType) });
            }
            return methodInvoke;
        }
        //ReturnType OutPin Index 0
        [Rtti.Meta]
        public bool IsStatic { get; set; } = false;
        [Rtti.Meta]
        public TtTypeDesc DeclaringType { get; set; } = null;
        [Rtti.Meta]
        public TtTypeDesc ReturnType { get; set; } = TtTypeDesc.TypeOf(typeof(void));
        public TtMethodInvokeDescription()
        {
            AddExecutionInPin(new());
            AddExecutionOutPin(new());
        }
        public TtDataInPinDescription GetHostPin()
        {
            if(!IsStatic)
            {
                return DataInPins[0];
            }
            return null;
        }

        public override TtStatementBase BuildStatement(ref FStatementBuildContext statementBuildContext)
        {
            var methodDesc = statementBuildContext.MethodDescription as TtMethodDescription;

            TtMethodInvokeStatement methodInvoke = new TtMethodInvokeStatement()
            {
                MethodName = Name,
            };
            if (IsStatic)
            {
                methodInvoke.Host = new TtClassReferenceExpression() { Class = DeclaringType };
            }
            if (ReturnType != TtTypeDesc.TypeOf(typeof(void)))
            {
                var resultVarDeclaration = new TtVariableDeclaration()
                {
                    VariableType = new TtTypeReference(ReturnType),
                    InitValue = new TtDefaultValueExpression(ReturnType),
                    VariableName = "result_" + Name + "_" + (uint)Id.ToString().GetHashCode(),
                };
                statementBuildContext.AddStatement(resultVarDeclaration);
                methodInvoke.ReturnValue = resultVarDeclaration;
            }
            foreach (var pin in DataInPins)
            {
                var linkedDataPin = methodDesc.GetLinkedDataPin(pin);
                if (linkedDataPin == null)
                {
                    //TODO: TtMethodInvokeReflectedDescription 要报错
                }
                else
                {
                    System.Diagnostics.Debug.Assert(linkedDataPin is TtDataOutPinDescription);
                    var buildContext = new FExpressionBuildContext() { MethodDescription = statementBuildContext.MethodDescription, Sequence = statementBuildContext.ExecuteSequenceStatement };
                    var linkedDesc = linkedDataPin.Parent;
                    if (linkedDesc is TtExpressionDescription linkedExpressionDesc)
                    {
                        var exp = linkedExpressionDesc.BuildExpression(ref buildContext);
                        methodInvoke.Arguments.Add(new TtMethodInvokeArgumentExpression(exp));
                    }
                    if (linkedDesc is TtStatementDescription linkedStatementDesc)
                    {
                        var exp = linkedStatementDesc.BuildExpressionForOutPin(linkedDataPin);
                        methodInvoke.Arguments.Add(new TtMethodInvokeArgumentExpression(exp));
                    }
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
        public override TtExpressionBase BuildExpressionForOutPin(TtDataPinDescription pin)
        {
            if (pin == DataOutPins[0])
            {
                return new TtVariableReferenceExpression("result_" + Name + "_" + (uint)Id.ToString().GetHashCode());
            }
            return null;
        }
    }
}
