using EngineNS.Animation.Asset;
using EngineNS.Bricks.CodeBuilder;
using EngineNS.Bricks.CodeBuilder.MacrossNode;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Design.ConnectingLine;
using EngineNS.DesignMacross.Design.Expressions;
using EngineNS.DesignMacross.Design.Statement;
using EngineNS.Rtti;
using NPOI.SS.Formula.Functions;
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
                if (methodMeta.ReturnType.IsSubclassOf(typeof(System.Threading.Tasks.Task)) ||
                     methodMeta.ReturnType.GetInterface(nameof(EngineNS.Thread.Async.ITask)) != null)
                {
                    methodInvoke.IsAsync = true;
                    if(methodMeta.ReturnType.GetGenericArguments().Length != 0)
                    {
                        methodInvoke.AddDataOutPin(new TtDataOutPinDescription
                        {
                            Name = "Result",
                            TypeDesc = TtTypeDesc.TypeOf(methodMeta.ReturnType.GetGenericArguments()[0])
                        });
                    }
                }
                else
                {
                    methodInvoke.AddDataOutPin(new TtDataOutPinDescription
                    {
                        Name = "Result",
                        TypeDesc = methodMeta.ReturnType
                    });
                }
            }

            return methodInvoke;
        }
        public static TtMethodInvokeDescription Create(MethodInfo methodInfo)
        {
            var methodInvoke = new TtMethodInvokeDescription();
            methodInvoke.Name = methodInfo.Name;
            methodInvoke.DeclaringType = TtTypeDesc.TypeOf(methodInfo.DeclaringType);
            methodInvoke.IsStatic = methodInfo.IsStatic;
            if (!methodInvoke.IsStatic)
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
                if (methodInfo.ReturnType.IsSubclassOf(typeof(System.Threading.Tasks.Task)) ||
                     methodInfo.ReturnType.GetInterface(nameof(EngineNS.Thread.Async.ITask)) != null)
                {
                    methodInvoke.IsAsync = true;
                    if(methodInfo.ReturnType.GetGenericArguments().Length != 0)
                    {
                        methodInvoke.AddDataOutPin(new TtDataOutPinDescription
                        {
                            Name = "Result",
                            TypeDesc = TtTypeDesc.TypeOf(methodInfo.ReturnType.GetGenericArguments()[0])
                        });
                    }
                }
                else
                {
                    methodInvoke.AddDataOutPin(new TtDataOutPinDescription
                    {
                        Name = "Result",
                        TypeDesc = TtTypeDesc.TypeOf(methodInfo.ReturnType)
                    });
                }
            }
            return methodInvoke;
        }
        //ReturnType OutPin Index 0
        [Rtti.Meta("")]
        public bool IsStatic { get; set; } = false;
        [Rtti.Meta("")]
        public bool IsAsync { get; set; } = false;
        [Rtti.Meta("")]
        public TtTypeDesc DeclaringType { get; set; } = null;
        [Rtti.Meta("")]
        public TtTypeDesc ReturnType { get; set; } = TtTypeDesc.TypeOf(typeof(void));
        public TtMethodInvokeDescription()
        {
            AddExecutionInPin(new());
            AddExecutionOutPin(new());
        }
        public TtDataInPinDescription GetHostPin()
        {
            if (!IsStatic)
            {
                return DataInPins[0];
            }
            return null;
        }

        public override TtStatementBase BuildStatement(ref FStatementBuildContext statementBuildContext)
        {
            var graphDesc = statementBuildContext.OwnerDescription;

            TtMethodInvokeStatement methodInvoke = new TtMethodInvokeStatement()
            {
                MethodName = Name,
            };
            methodInvoke.IsAsync = IsAsync;
            if (IsStatic)
            {
                methodInvoke.Host = new TtClassReferenceExpression() { Class = DeclaringType };
            }
            else
            {
                var hostPin = DataInPins[0];
                if (hostPin != null)
                {
                    var linkedHostPin = IDataLineOperator.GetLinkedDataPin(graphDesc, hostPin);
                    var buildContext = new FExpressionBuildContext() { OwnerDescription = statementBuildContext.OwnerDescription, Sequence = statementBuildContext.ExecuteSequenceStatement, ClassBuildContext = statementBuildContext.ClassBuildContext };
                    var linkedDesc = linkedHostPin.Parent;
                    if (linkedDesc is TtExpressionDescription linkedExpressionDesc)
                    {
                        methodInvoke.Host = linkedExpressionDesc.BuildExpression(ref buildContext);
                    }
                    if (linkedDesc is TtStatementDescription linkedStatementDesc)
                    {
                        methodInvoke.Host = linkedStatementDesc.BuildExpressionForOutPin(linkedHostPin);
                    }
                }
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
            var otherInPins = new List<TtDataPinDescription>(DataInPins);
            if (!IsStatic)
            {
                otherInPins.RemoveAt(0);
            }
            foreach (var pin in otherInPins)
            {
                var linkedDataPin = IDataLineOperator.GetLinkedDataPin(graphDesc, pin);
                if (linkedDataPin == null)
                {
                    var dataInPin = pin as TtDataInPinDescription;
                    if(dataInPin.TypeVaule != null)
                    {
                        if(dataInPin.TypeDesc == TtTypeDesc.TypeOf<RName>())
                        {
                            var argName = "result_MethodArg" + ((uint)pin.GetHashCode()).ToString();
                            var argVarDec = TtASTBuildUtil.CreateVariableDeclaration(argName, new TtTypeReference(dataInPin.TypeDesc), new TtDefaultValueExpression(dataInPin.TypeDesc));
                            var argStatement = new TtMethodInvokeStatement("ParseFrom",
                                   argVarDec,
                                   new TtClassReferenceExpression(dataInPin.TypeDesc),
                                   new TtMethodInvokeArgumentExpression { Expression = new TtPrimitiveExpression(dataInPin.TypeDesc, dataInPin.TypeVaule) });
                            argStatement.ReturnValue = argVarDec;
                            
                            statementBuildContext.AddStatement(argVarDec);
                            statementBuildContext.AddStatement(argStatement);
                            methodInvoke.Arguments.Add(new TtMethodInvokeArgumentExpression(new TtVariableReferenceExpression(argName)));
                        }
                        else
                        {
                            var argName = "result_MethodArg" + ((uint)pin.GetHashCode()).ToString();
                            var argVarDec = TtASTBuildUtil.CreateVariableDeclaration(argName, new TtTypeReference(dataInPin.TypeDesc), new TtDefaultValueExpression(dataInPin.TypeDesc));
                            //var argStatement = new TtMethodInvokeStatement("Parse",
                            //       argVarDec,
                            //       new TtClassReferenceExpression(dataInPin.TypeDesc),
                            //       new TtMethodInvokeArgumentExpression { Expression = new TtPrimitiveExpression(dataInPin.TypeDesc, dataInPin.TypeVaule) });
                            //argStatement.ReturnValue = argVarDec;
                            var argVarDecAssign = TtASTBuildUtil.CreateAssignOperatorStatement(new TtVariableReferenceExpression(argName), new TtPrimitiveExpression(dataInPin.TypeDesc, dataInPin.TypeVaule));
                            statementBuildContext.AddStatement(argVarDec);
                            statementBuildContext.AddStatement(argVarDecAssign);
                            methodInvoke.Arguments.Add(new TtMethodInvokeArgumentExpression(new TtVariableReferenceExpression(argName)));
                        }
                    }
                }
                else
                {
                    System.Diagnostics.Debug.Assert(linkedDataPin is TtDataOutPinDescription);
                    var buildContext = new FExpressionBuildContext() { OwnerDescription = statementBuildContext.OwnerDescription, Sequence = statementBuildContext.ExecuteSequenceStatement, ClassBuildContext = statementBuildContext.ClassBuildContext };
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
            var linkedExecPin = IExecutionLineOperator.GetLinkedExecutionPin(graphDesc, executionOutPin);
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
