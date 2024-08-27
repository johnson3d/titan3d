using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace EngineNS.Bricks.CodeBuilder
{
    public class UHLSLCodeGenerator : UCodeGeneratorBase
    {
        public UHLSLCodeGenerator()
        {
            mSegmentStartStr = "{";
            mSegmentEndStr = "}";
            mIndentStr = "    ";
        }

        public static void GenCommentCodes(UCommentStatement comment, ref UCodeGeneratorData data, ref string sourceCode)
        {
            if (comment == null)
                return;

            var commentGen = data.CodeGen.GetCodeObjectGen(comment.GetType());
            commentGen.GenCodes(comment, ref sourceCode, ref data);
        }

        class UVariableDeclarationCodeGen : ICodeObjectGen
        {
            public void GenCodes(UCodeObject obj, ref string sourceCode, ref UCodeGeneratorData data)
            {
                var varDec = obj as UVariableDeclaration;
                GenCommentCodes(varDec.Comment, ref data, ref sourceCode);
                string codeStr = "";
                codeStr += data.CodeGen.GetTypeString(varDec.VariableType) + " " + varDec.VariableName;
                if (varDec.InitValue != null)
                {
                    codeStr += " = ";
                    var initClsGen = data.CodeGen.GetCodeObjectGen(varDec.InitValue.GetType());
                    initClsGen.GenCodes(varDec.InitValue, ref codeStr, ref data);
                    codeStr += ";";
                    data.CodeGen.AddLine(codeStr, ref sourceCode);
                }
                else
                {
                    codeStr += ";";
                    data.CodeGen.AddLine(codeStr, ref sourceCode);
                }

                if(varDec.Next != null)
                {
                    var nextGen = data.CodeGen.GetCodeObjectGen(varDec.Next.GetType());
                    nextGen.GenCodes(varDec.Next, ref sourceCode, ref data);
                }
            }
        }

        class TtIncludeDeclarationCodeGen : ICodeObjectGen
        {
            public void GenCodes(UCodeObject obj, ref string sourceCode, ref UCodeGeneratorData data)
            {
                var varDec = obj as TtIncludeDeclaration;
                string codeStr = $"#include \"{varDec.FilePath}\"";
                data.CodeGen.AddLine(codeStr, ref sourceCode);
            }
        }

        class UMethodDeclarationCodeGen : ICodeObjectGen
        {
            public void GenCodes(UCodeObject obj, ref string sourceCode, ref UCodeGeneratorData data)
            {
                var methodDec = obj as UMethodDeclaration;
                GenCommentCodes(methodDec.Comment, ref data, ref sourceCode);
                data.Method = methodDec;
                string methodDecStr = (methodDec.ReturnValue != null) ? data.CodeGen.GetTypeString(methodDec.ReturnValue.VariableType) : "void" + " " + data.Method.MethodName + "(";
                for(int i=0; i<methodDec.Arguments.Count; i++)
                {
                    switch(methodDec.Arguments[i].OperationType)
                    {
                        case EMethodArgumentAttribute.In:
                            methodDecStr += "in ";
                            break;
                        case EMethodArgumentAttribute.Out:
                            methodDecStr += "out ";
                            break;
                        case EMethodArgumentAttribute.Ref:
                            methodDecStr += "inout ";
                            break;
                    }
                    methodDecStr += data.CodeGen.GetTypeString(methodDec.Arguments[i].VariableType) + " " + methodDec.Arguments[i].VariableName + ",";
                }
                if (methodDec.Arguments.Count > 0)
                    methodDecStr = methodDecStr.TrimEnd(',');
                methodDecStr += ")";
                data.CodeGen.AddLine(methodDecStr, ref sourceCode);
                data.CodeGen.PushSegment(ref sourceCode, in data);
                {
                    if(methodDec.ReturnValue != null)
                    {
                        var retValGen = data.CodeGen.GetCodeObjectGen(methodDec.ReturnValue.GetType());
                        retValGen.GenCodes(methodDec.ReturnValue, ref sourceCode, ref data);
                    }
                    for(int i=0; i<methodDec.Arguments.Count; i++)
                    {
                        var arg = methodDec.Arguments[i];
                        switch(arg.OperationType)
                        {
                            case EMethodArgumentAttribute.Out:
                                if(arg.InitValue != null)
                                {
                                    var argInitStr = arg.VariableName + " = ";
                                    var argInitGen = data.CodeGen.GetCodeObjectGen(arg.InitValue.GetType());
                                    argInitGen.GenCodes(arg.InitValue, ref argInitStr, ref data);
                                    argInitStr += ";";
                                    data.CodeGen.AddLine(argInitStr, ref sourceCode);
                                }
                                break;
                        }
                    }
                    for(int i=0; i<methodDec.LocalVariables.Count; i++)
                    {
                        var localVarCodeGen = data.CodeGen.GetCodeObjectGen(methodDec.LocalVariables[i].GetType());
                        localVarCodeGen.GenCodes(methodDec.LocalVariables[i], ref sourceCode, ref data);
                    }

                    var bodyCodeGen = data.CodeGen.GetCodeObjectGen(methodDec.MethodBody.GetType());
                    bodyCodeGen.GenCodes(methodDec.MethodBody, ref sourceCode, ref data);

                    if(!methodDec.ReturnHasGenerated && methodDec.ReturnValue != null)
                    {
                        var retCode = "return " + methodDec.ReturnValue.VariableName + ";";
                        data.CodeGen.AddLine(retCode, ref sourceCode);
                    }
                }
                data.CodeGen.PopSegment(ref sourceCode, in data);
                data.Method.ResetRuntimeData();
            }
        }

        class UClassDeclarationCodeGen : ICodeObjectGen
        {
            public void GenCodes(UCodeObject obj, ref string sourceCode, ref UCodeGeneratorData data)
            {
                var classDec = obj as UClassDeclaration;
                var codeGen = data.CodeGen as UCSharpCodeGenerator;
                GenCommentCodes(classDec.Comment, ref data, ref sourceCode);
                string tempCode = "struct ";

                tempCode += classDec.ClassName;
                data.CodeGen.AddLine(tempCode, ref sourceCode);
                data.CodeGen.PushSegment(ref sourceCode, in data);
                {
                    for(int i=0; i<classDec.Properties.Count; i++)
                    {
                        var mem = classDec.Properties[i];
                        var memCodeGen = data.CodeGen.GetCodeObjectGen(mem.GetType()) as UVariableDeclarationCodeGen;
                        memCodeGen.GenCodes(mem, ref sourceCode, ref data);
                    }

                    for (int i = 0; i < classDec.Methods.Count; i++)
                    {
                        var methodDecGen = data.CodeGen.GetCodeObjectGen(classDec.Methods[i].GetType());
                        methodDecGen.GenCodes(classDec.Methods[i], ref sourceCode, ref data);
                    }
                }
                data.CodeGen.PopSegment(ref sourceCode, in data, true);

                //for (int i = 0; i < classDec.Methods.Count; i++)
                //{
                //    var methodDecGen = data.CodeGen.GetCodeObjectGen(classDec.Methods[i].GetType());
                //    methodDecGen.GenCodes(classDec.Methods[i], ref sourceCode, ref data);
                //}
            }
        }

        class UClassReferenceExpressionCodeGen : ICodeObjectGen
        {
            public void GenCodes(UCodeObject obj, ref string sourceCode, ref UCodeGeneratorData data)
            {
                //HLSL don't have static function
                //var clsRefExp = obj as UClassReferenceExpression;
                //sourceCode += data.CodeGen.GetTypeString(clsRefExp.Class);
            }
        }

        class UVariableReferenceExpressionCodeGen : ICodeObjectGen
        {
            public void GenCodes(UCodeObject obj, ref string sourceCode, ref UCodeGeneratorData data)
            {
                var varRefExp = obj as UVariableReferenceExpression;
                if(varRefExp.Host != null)
                {
                    var hostGen = data.CodeGen.GetCodeObjectGen(varRefExp.Host.GetType());
                    hostGen.GenCodes(varRefExp.Host, ref sourceCode, ref data);
                    sourceCode += ".";
                }
                if (varRefExp.PropertyDeclClass != null && varRefExp.VariableName != null)
                {
                    var prop = varRefExp.PropertyDeclClass.SystemType.GetProperty(varRefExp.VariableName);
                    if (prop != null)
                    {
                        var meta = prop.GetCustomAttribute<Rtti.MetaAttribute>();
                        if (meta != null && meta.ShaderName != null)
                        {
                            sourceCode += meta.ShaderName;
                            return;
                        }
                    }
                }
                sourceCode += varRefExp.VariableName;
            }
        }

        class USelfReferenceExpressionCodeGen : ICodeObjectGen
        {
            public void GenCodes(UCodeObject obj, ref string sourceCode, ref UCodeGeneratorData data)
            {
            }
        }
        class UMethodInvokeArgumentExpressionCodeGen : ICodeObjectGen
        {
            public void GenCodes(UCodeObject obj, ref string sourceCode, ref UCodeGeneratorData data)
            {
                var miArgExp = obj as UMethodInvokeArgumentExpression;
                var argExp = data.CodeGen.GetCodeObjectGen(miArgExp.Expression.GetType());
                argExp.GenCodes(miArgExp.Expression, ref sourceCode, ref data);
            }
        }
        class UMethodInvokeStatementCodeGen : ICodeObjectGen
        {
            public void GenCodes(UCodeObject obj, ref string sourceCode, ref UCodeGeneratorData data)
            {
                var methodInvokeExp = obj as UMethodInvokeStatement;
                string invokeStr = "";
                if(methodInvokeExp.ReturnValue != null)
                {
                    if (methodInvokeExp.DeclarationReturnValue)
                        invokeStr += data.CodeGen.GetTypeString(methodInvokeExp.ReturnValue.VariableType) + " ";
                    invokeStr += methodInvokeExp.ReturnValue.VariableName + " = ";
                }
                if (methodInvokeExp.Host != null)
                {
                    var hostGen = data.CodeGen.GetCodeObjectGen(methodInvokeExp.Host.GetType());
                    hostGen.GenCodes(methodInvokeExp.Host, ref invokeStr, ref data);
                    var klsRef = methodInvokeExp.Host as UClassReferenceExpression;
                    if (klsRef == null)
                        invokeStr += ".";
                }

                var methodName = methodInvokeExp.MethodName;
                if (methodInvokeExp.Method != null)
                {
                    var attr = methodInvokeExp.Method.GetFirstCustomAttribute<Rtti.MetaAttribute>(false);
                    if (attr != null && attr.ShaderName != null)
                    {
                        methodName = attr.ShaderName;
                    }
                }
                invokeStr += methodName + "(";
                if (methodInvokeExp.Arguments.Count > 0)
                {
                    for(int i=0; i<methodInvokeExp.Arguments.Count; i++)
                    {
                        var argExp = data.CodeGen.GetCodeObjectGen(methodInvokeExp.Arguments[i].GetType());
                        argExp.GenCodes(methodInvokeExp.Arguments[i], ref invokeStr, ref data);
                        invokeStr += ",";
                    }
                    invokeStr = invokeStr.TrimEnd(',');
                }
                invokeStr += ");";
                data.CodeGen.AddLine(invokeStr, ref sourceCode);
            }
        }

        class UAssignOperatorStatementCodeGen : ICodeObjectGen
        {
            public void GenCodes(UCodeObject obj, ref string sourceCode, ref UCodeGeneratorData data)
            {
                var assignOpExp = obj as UAssignOperatorStatement;
                string assignStr = "";
                var toGen = data.CodeGen.GetCodeObjectGen(assignOpExp.To.GetType());
                toGen.GenCodes(assignOpExp.To, ref assignStr, ref data);
                assignStr += " = ";
                var fromGen = data.CodeGen.GetCodeObjectGen(assignOpExp.From.GetType());
                fromGen.GenCodes(assignOpExp.From, ref assignStr, ref data);
                assignStr += ";";
                data.CodeGen.AddLine(assignStr, ref sourceCode);

                if(assignOpExp.Next != null)
                {
                    var nextGen = data.CodeGen.GetCodeObjectGen(assignOpExp.Next.GetType());
                    nextGen.GenCodes(assignOpExp.Next, ref sourceCode, ref data);
                }
            }
        }

        class UBinaryOperatorExpressionCodeGen : ICodeObjectGen
        {
            public void GenCodes(UCodeObject obj, ref string sourceCode, ref UCodeGeneratorData data)
            {
                var binOpExp = obj as UBinaryOperatorExpression;
                if (binOpExp.Cell)
                    sourceCode += "(";
                var leftGen = data.CodeGen.GetCodeObjectGen(binOpExp.Left.GetType());
                leftGen.GenCodes(binOpExp.Left, ref sourceCode, ref data);
                switch(binOpExp.Operation)
                {
                    case UBinaryOperatorExpression.EBinaryOperation.Add:
                        sourceCode += " + ";
                        break;
                    case UBinaryOperatorExpression.EBinaryOperation.Subtract:
                        sourceCode += " - ";
                        break;
                    case UBinaryOperatorExpression.EBinaryOperation.Multiply:
                        sourceCode += " * ";
                        break;
                    case UBinaryOperatorExpression.EBinaryOperation.Divide:
                        sourceCode += " / ";
                        break;
                    case UBinaryOperatorExpression.EBinaryOperation.Modulus:
                        sourceCode += " % ";
                        break;
                    case UBinaryOperatorExpression.EBinaryOperation.Inequality:
                        sourceCode += " != ";
                        break;
                    case UBinaryOperatorExpression.EBinaryOperation.Equality:
                        sourceCode += " == ";
                        break;
                    case UBinaryOperatorExpression.EBinaryOperation.BitwiseOr:
                        sourceCode += " | ";
                        break;
                    case UBinaryOperatorExpression.EBinaryOperation.BitwiseXOR:
                        sourceCode += " ^ ";
                        break;
                    case UBinaryOperatorExpression.EBinaryOperation.BitwiseAnd:
                        sourceCode += " & ";
                        break;
                    case UBinaryOperatorExpression.EBinaryOperation.BitwiseLeftShift:
                        sourceCode += " << ";
                        break;
                    case UBinaryOperatorExpression.EBinaryOperation.BitwiseRightShift:
                        sourceCode += " >> ";
                        break;
                    case UBinaryOperatorExpression.EBinaryOperation.BooleanOr:
                        sourceCode += " || ";
                        break;
                    case UBinaryOperatorExpression.EBinaryOperation.BooleanAnd:
                        sourceCode += " && ";
                        break;
                    case UBinaryOperatorExpression.EBinaryOperation.LessThan:
                        sourceCode += " < ";
                        break;
                    case UBinaryOperatorExpression.EBinaryOperation.LessThanOrEqual:
                        sourceCode += " <= ";
                        break;
                    case UBinaryOperatorExpression.EBinaryOperation.GreaterThan:
                        sourceCode += " > ";
                        break;
                    case UBinaryOperatorExpression.EBinaryOperation.GreaterThanOrEqual:
                        sourceCode += " >= ";
                        break;
                    case UBinaryOperatorExpression.EBinaryOperation.AddAssignment:
                        sourceCode += " += ";
                        break;
                    case UBinaryOperatorExpression.EBinaryOperation.SubtractAssignment:
                        sourceCode += " -= ";
                        break;
                }
                var rightGen = data.CodeGen.GetCodeObjectGen(binOpExp.Right.GetType());
                rightGen.GenCodes(binOpExp.Right, ref sourceCode, ref data);
                if (binOpExp.Cell)
                    sourceCode += ")";
            }
        }

        class UUnaryOperatorExpressionCodeGen : ICodeObjectGen
        {
            public void GenCodes(UCodeObject obj, ref string sourceCode, ref UCodeGeneratorData data)
            {
                var unaryOpExp = obj as UUnaryOperatorExpression;
                var valGen = data.CodeGen.GetCodeObjectGen(unaryOpExp.Value.GetType());
                switch (unaryOpExp.Operation)
                {
                    case UUnaryOperatorExpression.EUnaryOperation.Negative:
                        {
                            sourceCode += "(-";
                            valGen.GenCodes(unaryOpExp.Value, ref sourceCode, ref data);
                            sourceCode += ")";
                        }
                        break;
                    case UUnaryOperatorExpression.EUnaryOperation.BooleanNot:
                        {
                            sourceCode += "(!";
                            valGen.GenCodes(unaryOpExp.Value, ref sourceCode, ref data);
                            sourceCode += ")";
                        }
                        break;
                    case UUnaryOperatorExpression.EUnaryOperation.BitwiseNot:
                        {
                            sourceCode += "(~";
                            valGen.GenCodes(unaryOpExp.Value, ref sourceCode, ref data);
                            sourceCode += ")";
                        }
                        break;
                }
            }
        }

        class UIndexerOperatorExpressionCodeGen : ICodeObjectGen
        {
            public void GenCodes(UCodeObject obj, ref string sourceCode, ref UCodeGeneratorData data)
            {
                var indexerOpExp = obj as UIndexerOperatorExpression;
                var tagGen = data.CodeGen.GetCodeObjectGen(indexerOpExp.Target.GetType());
                tagGen.GenCodes(indexerOpExp.Target, ref sourceCode, ref data);
                for (int i = 0; i < indexerOpExp.Indices.Count; i++)
                {
                    sourceCode += "[";
                    var idxGen = data.CodeGen.GetCodeObjectGen(indexerOpExp.Indices[i].GetType());
                    idxGen.GenCodes(indexerOpExp.Indices[i], ref sourceCode, ref data);
                    sourceCode += "]";
                }
            }
        }

        class UPrimitiveExpressionCodeGen : ICodeObjectGen
        {
            public void GenCodes(UCodeObject obj, ref string sourceCode, ref UCodeGeneratorData data)
            {
                var primitiveExp = obj as UPrimitiveExpression;
                var typeStr = data.CodeGen.GetTypeString(primitiveExp.Type);
                switch(typeStr)
                {
                    case "float2":
                    case "float3":
                    case "float4":
                    case "int2":
                    case "int3":
                    case "int4":
                    case "uint2":
                    case "uint3":
                    case "uint4":
                        {
                            //sourceCode += typeStr + "(" + primitiveExp.ValueStr + ")";
                            if (primitiveExp.ObjectStr != null)
                            {
                                var attr = primitiveExp.Type.SystemType.GetType().GetCustomAttribute<Rtti.MetaAttribute>();
                                if (attr != null)
                                {
                                    sourceCode += typeStr + "(" + primitiveExp.ObjectStr + ")";
                                }
                                else
                                {
                                    sourceCode += typeStr + "(" + primitiveExp.ObjectStr + ")";
                                }
                            }
                            else
                            {
                                sourceCode += typeStr + "(" + primitiveExp.ObjectStr + ")";
                            }
                        }
                        break;
                    default:
                        {
                            if (primitiveExp.Type.IsEnum)
                            {
                                var attr = primitiveExp.Type.GetCustomAttribute<EngineNS.Editor.ShaderCompiler.TtShaderDefineAttribute>(false);
                                if (attr != null)
                                {
                                    sourceCode += $"{attr.ShaderName}_{primitiveExp.ValueStr}";
                                }
                                else
                                {
                                    object eValue;
                                    Enum.TryParse(primitiveExp.Type.SystemType, primitiveExp.ValueStr, out eValue);
                                    sourceCode += $"{(int)eValue}";
                                }
                            }
                            else
                            {
                                sourceCode += primitiveExp.ValueStr;
                            }
                        }
                        break;
                }
            }
        }

        class UCastExpressionCodeGen : ICodeObjectGen
        {
            public void GenCodes(UCodeObject obj, ref string sourceCode, ref UCodeGeneratorData data)
            {
                var castExp = obj as UCastExpression;
                sourceCode += "(" + data.CodeGen.GetTypeString(castExp.TargetType) + ")";
                var expGen = data.CodeGen.GetCodeObjectGen(castExp.Expression.GetType());
                expGen.GenCodes(castExp.Expression, ref sourceCode, ref data);
            }
        }

        class UCreateObjectExpressionCodeGen : ICodeObjectGen
        {
            public void GenCodes(UCodeObject obj, ref string sourceCode, ref UCodeGeneratorData data)
            {
                var createExp = obj as UCreateObjectExpression;
                sourceCode += createExp.TypeName + "(";
                for (int i = 0; i < createExp.Parameters.Count; i++)
                {
                    var paramGen = data.CodeGen.GetCodeObjectGen(createExp.Parameters[i].GetType());
                    paramGen.GenCodes(createExp.Parameters[i], ref sourceCode, ref data);
                    sourceCode += ",";
                }
                sourceCode = sourceCode.TrimEnd(',');
                sourceCode += ")";
            }
        }

        class UDefaultValueExpressionCodeGen : ICodeObjectGen
        {
            public void GenCodes(UCodeObject obj, ref string sourceCode, ref UCodeGeneratorData data)
            {
                var defaultValExp = obj as UDefaultValueExpression;
                switch(data.CodeGen.GetTypeString(defaultValExp.Type))
                {
                    case "float":
                    case "int":
                    case "uint":
                    case "half":
                        sourceCode += "0";
                        break;
                    case "float2":
                        sourceCode += "(float2)0";
                        break;
                    case "int2":
                        sourceCode += "(int2)0";
                        break;
                    case "uint2":
                        sourceCode += "(uint2)0";
                        break;
                    case "half2":
                        sourceCode += "(half2)0";
                        break;
                    case "float3":
                        sourceCode += "(float3)0";
                        break;
                    case "int3":
                        sourceCode += "(int3)0";
                        break;
                    case "uint3":
                        sourceCode += "(uint3)0";
                        break;
                    case "half3":
                        sourceCode += "(half3)0";
                        break;
                    case "float4":
                        sourceCode += "(float4)0";
                        break;
                    case "int4":
                        sourceCode += "(int4)0";
                        break;
                    case "uint4":
                        sourceCode += "(uint4)0";
                        break;
                    case "half4":
                        sourceCode += "(half4)0";
                        break;
                    default:
                        {
                            var meta = defaultValExp.Type.TypeDesc.GetCustomAttribute<Rtti.MetaAttribute>(false);
                            if (meta != null)
                            {
                                sourceCode += $"({meta.ShaderName})0";
                            }
                            else
                            {
                                System.Diagnostics.Debug.Assert(false);
                                sourceCode += $"({defaultValExp.Type.TypeFullName})0";
                            }
                        }
                        break;
                }
            }
        }

        class UNullValueExpressionCodeGen : ICodeObjectGen
        {
            public void GenCodes(UCodeObject obj, ref string sourceCode, ref UCodeGeneratorData data)
            {
                sourceCode += "NULL";
            }
        }

        class UExecuteSequenceStatementCodeGen : ICodeObjectGen
        {
            public void GenCodes(UCodeObject obj, ref string sourceCode, ref UCodeGeneratorData data)
            {
                var exeSeqExp = obj as UExecuteSequenceStatement;
                for (int i = 0; i < exeSeqExp.Sequence.Count; i++)
                {
                    var seqGen = data.CodeGen.GetCodeObjectGen(exeSeqExp.Sequence[i].GetType());
                    seqGen.GenCodes(exeSeqExp.Sequence[i], ref sourceCode, ref data);
                }
            }
        }

        class UReturnStatementCodeGen : ICodeObjectGen
        {
            public void GenCodes(UCodeObject obj, ref string sourceCode, ref UCodeGeneratorData data)
            {
                var retExp = obj as UReturnStatement;
                string retStr = "return";
                if (data.Method.ReturnValue != null)
                {
                    retStr += " " + data.Method.ReturnValue.VariableName + ";";
                }
                else
                {
                    retStr += ";";
                }
                data.CodeGen.AddLine(retStr, ref sourceCode);
                if (data.Method.MethodSegmentDeep <= 1)
                    data.Method.ReturnHasGenerated = true;
            }
        }

        class UIfStatementCodeGen : ICodeObjectGen
        {
            public void GenCodes(UCodeObject obj, ref string sourceCode, ref UCodeGeneratorData data)
            {
                var ifExp = obj as UIfStatement;
                string ifStr = "if (";
                var condGen = data.CodeGen.GetCodeObjectGen(ifExp.Condition.GetType());
                condGen.GenCodes(ifExp.Condition, ref ifStr, ref data);
                ifStr += ")";
                data.CodeGen.AddLine(ifStr, ref sourceCode);
                data.CodeGen.PushSegment(ref sourceCode, in data);
                {
                    var trueGen = data.CodeGen.GetCodeObjectGen(ifExp.TrueStatement.GetType());
                    trueGen.GenCodes(ifExp.TrueStatement, ref sourceCode, ref data);
                }
                data.CodeGen.PopSegment(ref sourceCode, in data);
                for (int i = 0; i < ifExp.ElseIfs.Count; i++)
                {
                    string elseIfStr = "elif (";
                    var elseIfCondGen = data.CodeGen.GetCodeObjectGen(ifExp.ElseIfs[i].Condition.GetType());
                    elseIfCondGen.GenCodes(ifExp.ElseIfs[i].Condition, ref elseIfStr, ref data);
                    elseIfStr += ")";
                    data.CodeGen.AddLine(elseIfStr, ref sourceCode);
                    data.CodeGen.PushSegment(ref sourceCode, in data);
                    {
                        var elseIfGen = data.CodeGen.GetCodeObjectGen(ifExp.ElseIfs[i].TrueStatement.GetType());
                        elseIfGen.GenCodes(ifExp.ElseIfs[i].TrueStatement, ref sourceCode, ref data);
                    }
                    data.CodeGen.PopSegment(ref sourceCode, in data);
                }
                if (ifExp.FalseStatement != null)
                {
                    data.CodeGen.AddLine("else", ref sourceCode);
                    data.CodeGen.PushSegment(ref sourceCode, in data);
                    {
                        var falseGen = data.CodeGen.GetCodeObjectGen(ifExp.FalseStatement.GetType());
                        falseGen.GenCodes(ifExp.FalseStatement, ref sourceCode, ref data);
                    }
                    data.CodeGen.PopSegment(ref sourceCode, in data);
                }

                if (ifExp.Next != null)
                {
                    var nextGen = data.CodeGen.GetCodeObjectGen(ifExp.Next.GetType());
                    nextGen.GenCodes(ifExp.Next, ref sourceCode, ref data);
                }
            }
        }

        class UForLoopStatementCodeGen : ICodeObjectGen
        {
            public void GenCodes(UCodeObject obj, ref string sourceCode, ref UCodeGeneratorData data)
            {
                var forExp = obj as UForLoopStatement;
                string forStr = "for (" + forExp.LoopIndexName + " = ";
                var beginGen = data.CodeGen.GetCodeObjectGen(forExp.BeginExpression.GetType());
                beginGen.GenCodes(forExp.BeginExpression, ref forStr, ref data);
                forStr += "; " + forExp.LoopIndexName + (forExp.IncludeEnd ? " <= " : " < ");
                var condGen = data.CodeGen.GetCodeObjectGen(forExp.EndExpression.GetType());
                condGen.GenCodes(forExp.EndExpression, ref forStr, ref data);
                forStr += "; " + forExp.LoopIndexName + " = " + forExp.LoopIndexName + " + ";
                var stepGen = data.CodeGen.GetCodeObjectGen(forExp.StepExpression.GetType());
                stepGen.GenCodes(forExp.StepExpression, ref forStr, ref data);
                forStr += ")";
                data.CodeGen.AddLine(forStr, ref sourceCode);
                data.CodeGen.PushSegment(ref sourceCode, in data);
                {
                    var bodyGen = data.CodeGen.GetCodeObjectGen(forExp.LoopBody.GetType());
                    bodyGen.GenCodes(forExp.LoopBody, ref sourceCode, ref data);
                }
                data.CodeGen.PopSegment(ref sourceCode, in data);

                if (forExp.Next != null)
                {
                    var nextGen = data.CodeGen.GetCodeObjectGen(forExp.Next.GetType());
                    nextGen.GenCodes(forExp.Next, ref sourceCode, ref data);
                }
            }
        }

        class UWhileLoopStatementCodeGen : ICodeObjectGen
        {
            public void GenCodes(UCodeObject obj, ref string sourceCode, ref UCodeGeneratorData data)
            {
                var whileExp = obj as UWhileLoopStatement;
                string whileStr = "while (";
                var condExpGen = data.CodeGen.GetCodeObjectGen(whileExp.Condition.GetType());
                condExpGen.GenCodes(whileExp.Condition, ref whileStr, ref data);
                whileStr += ")";
                data.CodeGen.AddLine(whileStr, ref sourceCode);
                data.CodeGen.PushSegment(ref sourceCode, in data);
                {
                    var bodyGen = data.CodeGen.GetCodeObjectGen(whileExp.LoopBody.GetType());
                    bodyGen.GenCodes(whileExp.LoopBody, ref sourceCode, ref data);
                }
                data.CodeGen.PopSegment(ref sourceCode, in data);
            }
        }

        class UContinueStatementCodeGen : ICodeObjectGen
        {
            public void GenCodes(UCodeObject obj, ref string sourceCode, ref UCodeGeneratorData data)
            {
                data.CodeGen.AddLine("continue;", ref sourceCode);
            }
        }

        class UBreakStatementCodeGen : ICodeObjectGen
        {
            public void GenCodes(UCodeObject obj, ref string sourceCode, ref UCodeGeneratorData data)
            {
                data.CodeGen.AddLine("break;", ref sourceCode);
            }
        }

        class UCommentStatementCodeGen : ICodeObjectGen
        {
            public void GenCodes(UCodeObject obj, ref string sourceCode, ref UCodeGeneratorData data)
            {
                var commentExp = obj as UCommentStatement;
                if(!string.IsNullOrEmpty(commentExp.CommentString))
                {
                    var comment = "//" + commentExp.CommentString;
                    data.CodeGen.AddLine(comment, ref sourceCode);
                }
            }
        }

        class UExpressionStatementCodeGen : ICodeObjectGen
        {
            public void GenCodes(UCodeObject obj, ref string sourceCode, ref UCodeGeneratorData data)
            {
                var expSt = obj as UExpressionStatement;
                var expGen = data.CodeGen.GetCodeObjectGen(expSt.Expression.GetType());
                string tempCode = "";
                expGen.GenCodes(expSt.Expression, ref tempCode, ref data);
                tempCode += ";";
                data.CodeGen.AddLine(tempCode, ref sourceCode);

                if (expSt.Next != null)
                {
                    var nextGen = data.CodeGen.GetCodeObjectGen(expSt.Next.GetType());
                    nextGen.GenCodes(expSt.Next, ref sourceCode, ref data);
                }
            }
        }

        class UDebuggerTryBreakCodeGen : ICodeObjectGen
        {
            public void GenCodes(UCodeObject obj, ref string sourceCode, ref UCodeGeneratorData data)
            {
                var exp = obj as UDebuggerTryBreak;
            }
        }

        class UDebuggerSetWatchVariableCodeGen : ICodeObjectGen
        {
            public void GenCodes(UCodeObject obj, ref string sourceCode, ref UCodeGeneratorData data)
            {
                var exp = obj as UDebuggerSetWatchVariable;
            }
        }

        UVariableDeclarationCodeGen mVariableDeclarationCodeGen = new UVariableDeclarationCodeGen();
        TtIncludeDeclarationCodeGen mIncludeDeclarationCodeGen = new TtIncludeDeclarationCodeGen();
        UMethodDeclarationCodeGen mMethodDeclarationCodeGen = new UMethodDeclarationCodeGen();
        UClassDeclarationCodeGen mClassDeclarationCodeGen = new UClassDeclarationCodeGen();
        UClassReferenceExpressionCodeGen mClassReferenceExpressionCodeGen = new UClassReferenceExpressionCodeGen();
        UVariableReferenceExpressionCodeGen mVariableReferenceExpressionCodeGen = new UVariableReferenceExpressionCodeGen();
        USelfReferenceExpressionCodeGen mSelfReferenceExpressionCodeGen = new USelfReferenceExpressionCodeGen();
        UMethodInvokeArgumentExpressionCodeGen mMethodInvokeArgumentExpressionCodeGen = new UMethodInvokeArgumentExpressionCodeGen();
        UMethodInvokeStatementCodeGen mMethodInvokeStatementCodeGen = new UMethodInvokeStatementCodeGen();
        UAssignOperatorStatementCodeGen mAssignOperatorStatementCodeGen = new UAssignOperatorStatementCodeGen();
        UBinaryOperatorExpressionCodeGen mBinaryOperatorExpressionCodeGen = new UBinaryOperatorExpressionCodeGen();
        UUnaryOperatorExpressionCodeGen mUnaryOperatorExpressionCodeGen = new UUnaryOperatorExpressionCodeGen();
        UIndexerOperatorExpressionCodeGen mIndexerOperatorExpressionCodeGen = new UIndexerOperatorExpressionCodeGen();
        UPrimitiveExpressionCodeGen mPrimitiveExpressionCodeGen = new UPrimitiveExpressionCodeGen();
        UCastExpressionCodeGen mCastExpressionCodeGen = new UCastExpressionCodeGen();
        UCreateObjectExpressionCodeGen mCreateObjectExpressionCodeGen = new UCreateObjectExpressionCodeGen();
        UDefaultValueExpressionCodeGen mDefaultValueExpressionCodeGen = new UDefaultValueExpressionCodeGen();
        UDefaultValueExpressionCodeGen mNullValueExpressionCodeGen = new UDefaultValueExpressionCodeGen();
        UExecuteSequenceStatementCodeGen mExecuteSequenceStatementCodeGen = new UExecuteSequenceStatementCodeGen();
        UReturnStatementCodeGen mReturnStatementCodeGen = new UReturnStatementCodeGen();
        UIfStatementCodeGen mIfStatementCodeGen = new UIfStatementCodeGen();
        UForLoopStatementCodeGen mForLoopStatementCodeGen = new UForLoopStatementCodeGen();
        UWhileLoopStatementCodeGen mWhileLoopStatementCodeGen = new UWhileLoopStatementCodeGen();
        UContinueStatementCodeGen mContinueStatementCodeGen = new UContinueStatementCodeGen();
        UBreakStatementCodeGen mBreakStatementCodeGen = new UBreakStatementCodeGen();
        UCommentStatementCodeGen mCommentStatementCodeGen = new UCommentStatementCodeGen();
        UExpressionStatementCodeGen mExpressionStatementCodeGen = new UExpressionStatementCodeGen();
        UDebuggerTryBreakCodeGen mDebuggerTryBreakCodeGen = new UDebuggerTryBreakCodeGen();
        UDebuggerSetWatchVariableCodeGen mDebuggerSetWatchVariableCodeGen = new UDebuggerSetWatchVariableCodeGen();
        public override ICodeObjectGen GetCodeObjectGen(Rtti.UTypeDesc type)
        {
            if (type.IsEqual(typeof(UVariableDeclaration)))
                return mVariableDeclarationCodeGen;
            if (type.IsEqual(typeof(TtIncludeDeclaration)))
                return mIncludeDeclarationCodeGen;
            else if (type.IsEqual(typeof(UMethodDeclaration)))
                return mMethodDeclarationCodeGen;
            else if (type.IsEqual(typeof(UClassDeclaration)))
                return mClassDeclarationCodeGen;
            else if (type.IsEqual(typeof(UClassReferenceExpression)))
                return mClassReferenceExpressionCodeGen;
            else if (type.IsEqual(typeof(UVariableReferenceExpression)))
                return mVariableReferenceExpressionCodeGen;
            else if (type.IsEqual(typeof(USelfReferenceExpression)))
                return mSelfReferenceExpressionCodeGen;
            else if (type.IsEqual(typeof(UMethodInvokeArgumentExpression)))
                return mMethodInvokeArgumentExpressionCodeGen;
            else if (type.IsEqual(typeof(UMethodInvokeStatement)))
                return mMethodInvokeStatementCodeGen;
            else if (type.IsEqual(typeof(UAssignOperatorStatement)))
                return mAssignOperatorStatementCodeGen;
            else if (type.IsEqual(typeof(UBinaryOperatorExpression)))
                return mBinaryOperatorExpressionCodeGen;
            else if (type.IsEqual(typeof(UUnaryOperatorExpression)))
                return mUnaryOperatorExpressionCodeGen;
            else if (type.IsEqual(typeof(UIndexerOperatorExpression)))
                return mIndexerOperatorExpressionCodeGen;
            else if (type.IsEqual(typeof(UPrimitiveExpression)))
                return mPrimitiveExpressionCodeGen;
            else if (type.IsEqual(typeof(UCastExpression)))
                return mCastExpressionCodeGen;
            else if (type.IsEqual(typeof(UCreateObjectExpression)))
                return mCreateObjectExpressionCodeGen;
            else if (type.IsEqual(typeof(UDefaultValueExpression)))
                return mDefaultValueExpressionCodeGen;
            else if (type.IsEqual(typeof(UDefaultValueExpression)))
                return mNullValueExpressionCodeGen;
            else if (type.IsEqual(typeof(UExecuteSequenceStatement)))
                return mExecuteSequenceStatementCodeGen;
            else if (type.IsEqual(typeof(UReturnStatement)))
                return mReturnStatementCodeGen;
            else if (type.IsEqual(typeof(UIfStatement)))
                return mIfStatementCodeGen;
            else if (type.IsEqual(typeof(UForLoopStatement)))
                return mForLoopStatementCodeGen;
            else if (type.IsEqual(typeof(UWhileLoopStatement)))
                return mWhileLoopStatementCodeGen;
            else if (type.IsEqual(typeof(UContinueStatement)))
                return mContinueStatementCodeGen;
            else if (type.IsEqual(typeof(UBreakStatement)))
                return mBreakStatementCodeGen;
            else if (type.IsEqual(typeof(UCommentStatement)))
                return mCommentStatementCodeGen;
            else if (type.IsEqual(typeof(UExpressionStatement)))
                return mExpressionStatementCodeGen;
            else if (type.IsEqual(typeof(UDebuggerTryBreak)))
                return mDebuggerTryBreakCodeGen;
            else if (type.IsEqual(typeof(UDebuggerSetWatchVariable)))
                return mDebuggerSetWatchVariableCodeGen; 
            System.Diagnostics.Debug.Assert(false);
            return null;
        }
        public override string GetTypeString(Rtti.UTypeDesc t)
        {
            if (t.IsEqual(typeof(float)))
                return "float";
            else if (t.IsEqual(typeof(Vector2)))
                return "float2";
            else if (t.IsEqual(typeof(Vector3)))
                return "float3";
            else if (t.IsEqual(typeof(Vector4)))
                return "float4";
            else if (t.IsEqual(typeof(Color4f)))
                return "float4";
            else if (t.IsEqual(typeof(Matrix)))
                return "matrix";
            else if (t.IsEqual(typeof(int)))
                return "int";
            else if (t.IsEqual(typeof(Vector2i)))
                return "int2";
            else if (t.IsEqual(typeof(Vector3i)))
                return "int3";
            else if (t.IsEqual(typeof(Vector4i)))
                return "int4";
            else if (t.IsEqual(typeof(uint)))
                return "uint";
            else if (t.IsEqual(typeof(Vector2ui)))
                return "uint2";
            else if (t.IsEqual(typeof(Vector3ui)))
                return "uint3";
            else if (t.IsEqual(typeof(Vector4ui)))
                return "uint4";
            else if (t.IsEqual(typeof(EngineNS.Bricks.CodeBuilder.ShaderNode.Var.Texture2D)))
                return "Texture2D";
            else if (t.IsEqual(typeof(EngineNS.Bricks.CodeBuilder.ShaderNode.Var.Texture2DArray)))
                return "Texture2DArray";
            else if (t.IsEqual(typeof(EngineNS.Bricks.CodeBuilder.ShaderNode.Var.SamplerState)))
                return "SamplerState";
            return t.FullName;
        }
        public override string GetTypeString(UTypeReference t)
        {
            if (t.TypeDesc != null)
            {
                var meta = t.TypeDesc.GetCustomAttribute<Rtti.MetaAttribute>(false);
                if (meta != null && meta.ShaderName != null)
                {
                    return meta.ShaderName;
                }
                else
                {
                    var meta1 = t.TypeDesc.GetCustomAttribute<EngineNS.Editor.ShaderCompiler.TtShaderDefineAttribute>(false);
                    if (meta1 != null && meta1.ShaderName != null)
                    {
                        return meta1.ShaderName;
                    }
                }
                return GetTypeString(t.TypeDesc);
            }
            return t.TypeFullName;
        }
    }
}
