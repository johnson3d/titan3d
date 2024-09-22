using EngineNS.Rtti;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace EngineNS.Bricks.CodeBuilder
{
    public class UCSharpCodeGenerator : TtCodeGeneratorBase
    {
        public UCSharpCodeGenerator()
        {
            mSegmentStartStr = "{";
            mSegmentEndStr = "}";
            mIndentStr = "    ";
        }

        public static void GenCommentCodes(TtCommentStatement comment, ref TtCodeGeneratorData data, ref string sourceCode)
        {
            if (comment == null)
                return;

            var commentGen = data.CodeGen.GetCodeObjectGen(comment.GetType());
            commentGen.GenCodes(comment, ref sourceCode, ref data);
        }

        class UDebuggerSetWatchVariableCodeGen : ICodeObjectGen
        {
            public void GenCodes(TtCodeObject obj, ref string sourceCode, ref TtCodeGeneratorData data)
            {
                if (data.CodeGen.IsEditorDebug)
                {
                    var exp = obj as TtDebuggerSetWatchVariable;
                    var codeStr = $"mFrame_{data.Method.UniqueMethodName}.SetWatchVariable(\"{exp.VariableName}\", ";
                    var varGen = data.CodeGen.GetCodeObjectGen(exp.VariableValue.GetType());
                    varGen.GenCodes(exp.VariableValue, ref codeStr, ref data);
                    codeStr += ");";
                    data.CodeGen.AddLine(codeStr, ref sourceCode);
                }   
            }
        }
        class UDebuggerTryBreakCodeGen : ICodeObjectGen
        {
            public void GenCodes(TtCodeObject obj, ref string sourceCode, ref TtCodeGeneratorData data)
            {
                if (data.CodeGen.IsEditorDebug)
                {
                    var exp = obj as TtDebuggerTryBreak;
                    data.CodeGen.AddLine($"{exp.BreakName}.TryBreak();", ref sourceCode);
                }
            }
        }
        class TtAttributeCodeGen : ICodeObjectGen
        {
            public void GenCodes(TtCodeObject obj, ref string sourceCode, ref TtCodeGeneratorData data)
            {
                var varDec = obj as TtAttribute;
                string codeStr = "[";
                codeStr += data.CodeGen.GetTypeString(varDec.AttributeType);
                if(varDec.Arguments.Count > 0)
                {
                    codeStr += "(";
                    for(int i=0; i<varDec.Arguments.Count; i++)
                    {
                        var gen = data.CodeGen.GetCodeObjectGen(varDec.Arguments[i].GetType());
                        gen.GenCodes(varDec.Arguments[i], ref codeStr, ref data);
                        codeStr += ",";
                    }
                    codeStr = codeStr.TrimEnd(',');
                    codeStr += ")";
                }
                codeStr += "]";
                data.CodeGen.AddLine(codeStr, ref sourceCode);
            }
        }
        class UVariableDeclarationCodeGen : ICodeObjectGen
        {
            public bool IsClassMember = false;
            public bool IsProperty = false;
            public void GenCodes(TtCodeObject obj, ref string sourceCode, ref TtCodeGeneratorData data)
            {
                var varDec = obj as TtVariableDeclaration;
                GenCommentCodes(varDec.Comment, ref data, ref sourceCode);
                var typeString = data.CodeGen.GetTypeString(varDec.VariableType);
                if(varDec.IsBindable)
                {
                    string memberCode = "";
                    memberCode += typeString + " m" + varDec.VariableName;
                    if (varDec.InitValue != null)
                    {
                        memberCode += " = ";
                        var initClsGen = data.CodeGen.GetCodeObjectGen(varDec.InitValue.GetType());
                        initClsGen.GenCodes(varDec.InitValue, ref memberCode, ref data);
                        memberCode += ";";
                    }
                    else if (!IsProperty)
                        memberCode += ";";
                    data.CodeGen.AddLine(memberCode, ref sourceCode);
                    data.CodeGen.AddLine("[EngineNS.UI.Bind.BindPropertyAttribute]", ref sourceCode);
                }
                for(int i=0; i<varDec.Attributes.Count; i++)
                {
                    var attGen = data.CodeGen.GetCodeObjectGen(varDec.Attributes[i].GetType());
                    attGen.GenCodes(varDec.Attributes[i], ref sourceCode, ref data);
                }
                if (varDec.IsAutoSaveLoad && (IsProperty || varDec.IsBindable))
                    data.CodeGen.AddLine("[EngineNS.Rtti.Meta]", ref sourceCode);
                string codeStr = "";
                if(IsClassMember)
                {
                    if(varDec.IsBindable)
                    {
                        codeStr += "public ";
                    }
                    else
                    {
                        switch(varDec.VisitMode)
                        {
                            case EVisisMode.Local:
                            case EVisisMode.Private:
                                codeStr += "private ";
                                break;
                            case EVisisMode.Public:
                                codeStr += "public ";
                                break;
                            case EVisisMode.Protected:
                                codeStr += "protected ";
                                break;
                            case EVisisMode.Internal:
                                codeStr += "internal ";
                                break;
                        }
                    }
                }
                codeStr += typeString + " " + varDec.VariableName;
                if(varDec.IsBindable) // bindable must be a property
                {
                    if (varDec.IsBindable)
                    {
                            codeStr += @$"
        {{
            get => m{varDec.VariableName};
            set
            {{
                OnValueChange(value, m{varDec.VariableName});
                m{varDec.VariableName} = value;
            }}
        }}";
                    }
                }
                else
                {
                    if (IsProperty)
                    {
                        codeStr += " { get; set; }";
                    }
                    if (varDec.InitValue != null)
                    {
                        codeStr += " = ";
                        var initClsGen = data.CodeGen.GetCodeObjectGen(varDec.InitValue.GetType());
                        initClsGen.GenCodes(varDec.InitValue, ref codeStr, ref data);
                        codeStr += ";";
                    }
                    else if (!IsProperty)
                        codeStr += ";";
                }
                data.CodeGen.AddLine(codeStr, ref sourceCode);

                if(varDec.Next != null)
                {
                    var nextGen = data.CodeGen.GetCodeObjectGen(varDec.Next.GetType());
                    nextGen.GenCodes(varDec.Next, ref sourceCode, ref data);
                }
            }
        }

        class UMethodDeclarationCodeGen : ICodeObjectGen
        {
            public void GenCodes(TtCodeObject obj, ref string sourceCode, ref TtCodeGeneratorData data)
            {
                var methodDec = obj as TtMethodDeclaration;

                // debugger code
                var frameName = $"mFrame_{methodDec.UniqueMethodName}";
                data.CodeGen.AddLine($"EngineNS.Macross.TtMacrossStackFrame {frameName} = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName(\"{data.AssetName.Name}\", {data.AssetName.RNameType.GetType().FullName.Replace("+", ".")}.{data.AssetName.RNameType.ToString()}));", ref sourceCode);

                GenCommentCodes(methodDec.Comment, ref data, ref sourceCode);
                data.Method = methodDec;
                for(int i=0; i<methodDec.Attributes.Count; i++)
                {
                    var attGen = data.CodeGen.GetCodeObjectGen(methodDec.Attributes[i].GetType());
                    attGen.GenCodes(methodDec.Attributes[i], ref sourceCode, ref data);
                }
                string methodDecStr = "";
                switch(methodDec.VisitMode)
                {
                    case EVisisMode.Protected:
                        methodDecStr += "protected ";
                        break;
                    case EVisisMode.Public:
                        methodDecStr += "public ";
                        break;
                    default:
                        methodDecStr += "private ";
                        break;
                }
                string unsafePosCode = "@@@unsafe@@@";
                methodDecStr += unsafePosCode;
                if (methodDec.IsOverride)
                    methodDecStr += "override ";
                bool bTest = false;
                if (bTest)
                {
                    methodDec.AsyncType = TtMethodDeclaration.EAsyncType.None;
                    methodDec.AsyncType = TtMethodDeclaration.EAsyncType.SystemTask;
                }
                switch (methodDec.AsyncType)
                {
                    case TtMethodDeclaration.EAsyncType.None:
                        methodDecStr += ((methodDec.ReturnValue != null) ? data.CodeGen.GetTypeString(methodDec.ReturnValue.VariableType) : "void");
                        break;
                    case TtMethodDeclaration.EAsyncType.SystemTask:
                        methodDecStr += "async System.Threading.Tasks.Task";
                        methodDecStr += ((methodDec.ReturnValue != null) ? ("<" + data.CodeGen.GetTypeString(methodDec.ReturnValue.VariableType) + ">") : "");
                        break;
                    case TtMethodDeclaration.EAsyncType.CustomTask:
                        methodDecStr += "async EngineNS.Thread.Async.TtTask";
                        methodDecStr += ((methodDec.ReturnValue != null) ? ("<" + data.CodeGen.GetTypeString(methodDec.ReturnValue.VariableType) + ">") : "");
                        break;
                }

                methodDecStr += " " + data.Method.MethodName + "(";
                for(int i= 0; i<methodDec.Arguments.Count; i++)
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
                            methodDecStr += "ref ";
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
                    string awaitDummyPosCode = "@@@await@@@";
                    data.CodeGen.AddLine(awaitDummyPosCode, ref sourceCode);

                    data.CodeGen.AddLine($"using(var guard_{methodDec.MethodName} = new EngineNS.Macross.TtMacrossStackGuard({frameName}))", ref sourceCode);
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
                                    if (arg.InitValue != null)
                                    {
                                        var argInitStr = arg.VariableName + " = ";
                                        var argInitGen = data.CodeGen.GetCodeObjectGen(arg.InitValue.GetType());
                                        argInitGen.GenCodes(arg.InitValue, ref argInitStr, ref data);
                                        argInitStr += ";";
                                        data.CodeGen.AddLine(argInitStr, ref sourceCode);
                                    }
                                    break;
                            }
                            if (data.CodeGen.IsEditorDebug)
                                data.CodeGen.AddLine($"{frameName}.SetWatchVariable(\"{arg.VariableName}\", {arg.VariableName});", ref sourceCode);
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
                    var unsafeIdx = sourceCode.LastIndexOf(unsafePosCode);
                    sourceCode = sourceCode.Remove(unsafeIdx, unsafePosCode.Length);
                    if (methodDec.HasUnsafeCode || methodDec.IsUnsafe)
                        sourceCode = sourceCode.Insert(unsafeIdx, "unsafe ");

                    var awaitDummyIdx = sourceCode.LastIndexOf(awaitDummyPosCode);
                    sourceCode = sourceCode.Remove(awaitDummyIdx, awaitDummyPosCode.Length);
                    if (!methodDec.HasAwaitCode && methodDec.AsyncType != TtMethodDeclaration.EAsyncType.None)
                        sourceCode = sourceCode.Insert(awaitDummyIdx, $"await {typeof(EngineNS.Thread.TtAsyncDummyClass).FullName}.DummyFunc();");
                    else
                        sourceCode = sourceCode.Remove(awaitDummyIdx, data.CodeGen.CurIndentStr.Length + 1);
                }
                data.CodeGen.PopSegment(ref sourceCode, in data);
                data.Method.ResetRuntimeData();
            }
        }

        class UClassDeclarationCodeGen : ICodeObjectGen
        {
            public void GenCodes(TtCodeObject obj, ref string sourceCode, ref TtCodeGeneratorData data)
            {
                var classDec = obj as TtClassDeclaration;
                var codeGen = data.CodeGen as UCSharpCodeGenerator;
                if(data.Namespace != null)
                {
                    data.CodeGen.AddLine("namespace " + data.Namespace.Namespace, ref sourceCode);
                    data.CodeGen.PushSegment(ref sourceCode);
                }
                GenCommentCodes(classDec.Comment, ref data, ref sourceCode);
                data.CodeGen.AddLine("[EngineNS.Macross.TtMacross]", ref sourceCode);
                string tempCode = "";
                switch(classDec.VisitMode)
                {
                    case EVisisMode.Public:
                        tempCode += "public ";
                        break;
                    case EVisisMode.Internal:
                        tempCode += "internal ";
                        break;
                    default:
                        break;
                }
                classDec.IsUnsafe = false;
                if (classDec.IsStruct)
                    tempCode += (classDec.IsUnsafe ? "unsafe " : "") + "partial struct ";
                else
                    tempCode += (classDec.IsUnsafe ? "unsafe " : "") + "partial class ";

                tempCode += classDec.ClassName;
                if(classDec.SupperClassNames.Count > 0)
                {
                    tempCode += " : ";
                    for (int i = 0; i < classDec.SupperClassNames.Count; i++)
                    {
                        tempCode += classDec.SupperClassNames[i] + ",";
                    }
                    tempCode = tempCode.TrimEnd(',');
                }
                data.CodeGen.AddLine(tempCode, ref sourceCode);
                data.CodeGen.PushSegment(ref sourceCode, in data);
                {
                    for(int i=0; i<classDec.Properties.Count; i++)
                    {
                        var mem = classDec.Properties[i];
                        var memCodeGen = data.CodeGen.GetCodeObjectGen(mem.GetType()) as UVariableDeclarationCodeGen;
                        memCodeGen.IsClassMember = true;
                        memCodeGen.IsProperty = true;
                        memCodeGen.GenCodes(mem, ref sourceCode, ref data);
                        memCodeGen.IsClassMember = false;
                        memCodeGen.IsProperty = false;
                    }

                    for(int i=0; i<classDec.PreDefineVariables.Count; i++)
                    {
                        if (data.CodeGen.IsEditorDebug == false &&
                            classDec.PreDefineVariables[i].VariableType.TypeDesc != null &&
                            classDec.PreDefineVariables[i].VariableType.TypeDesc.SystemType == typeof(Macross.TtMacrossBreak))
                        {
                            continue;
                        }
                        var gen = data.CodeGen.GetCodeObjectGen(classDec.PreDefineVariables[i].GetType()) as UVariableDeclarationCodeGen;
                        gen.IsClassMember = true;
                        gen.GenCodes(classDec.PreDefineVariables[i], ref sourceCode, ref data);
                        gen.IsClassMember = false;
                    }

                    for(int i=0; i<classDec.Methods.Count; i++)
                    {
                        var methodDecGen = data.CodeGen.GetCodeObjectGen(classDec.Methods[i].GetType());
                        methodDecGen.GenCodes(classDec.Methods[i], ref sourceCode, ref data);
                    }
                }
                data.CodeGen.PopSegment(ref sourceCode, in data);
                if(data.Namespace != null)
                {
                    data.CodeGen.PopSegment(ref sourceCode);
                }
            }
        }

        class UClassReferenceExpressionCodeGen : ICodeObjectGen
        {
            public void GenCodes(TtCodeObject obj, ref string sourceCode, ref TtCodeGeneratorData data)
            {
                var clsRefExp = obj as TtClassReferenceExpression;
                sourceCode += data.CodeGen.GetTypeString(clsRefExp.Class);
            }
        }

        class UVariableReferenceExpressionCodeGen : ICodeObjectGen
        {
            public void GenCodes(TtCodeObject obj, ref string sourceCode, ref TtCodeGeneratorData data)
            {
                var varRefExp = obj as TtVariableReferenceExpression;
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
                        if (meta != null && meta.IsCanRefForMacross)
                        {
                            sourceCode += "m" + meta.ShaderName;
                            return;
                        }
                    }
                }
                sourceCode += varRefExp.VariableName;
            }
        }

        class USelfReferenceExpressionCodeGen : ICodeObjectGen
        {
            public void GenCodes(TtCodeObject obj, ref string sourceCode, ref TtCodeGeneratorData data)
            {
                sourceCode += "this";
            }
        }
        class UBaseReferenceExpresiionCodeGen : ICodeObjectGen
        {
            public void GenCodes(TtCodeObject obj, ref string sourceCode, ref TtCodeGeneratorData data)
            {
                sourceCode += "base";
            }
        }
        class UMethodInvokeArgumentExpressionCodeGen : ICodeObjectGen
        {
            public void GenCodes(TtCodeObject obj, ref string sourceCode, ref TtCodeGeneratorData data)
            {
                var miArgExp = obj as TtMethodInvokeArgumentExpression;
                switch(miArgExp.OperationType)
                {
                    case EMethodArgumentAttribute.In:
                        sourceCode += "in ";
                        break;
                    case EMethodArgumentAttribute.Out:
                        sourceCode += "out ";
                        break;
                    case EMethodArgumentAttribute.Ref:
                        sourceCode += "ref ";
                        break;
                }
                var argExp = data.CodeGen.GetCodeObjectGen(miArgExp.Expression.GetType());
                argExp.GenCodes(miArgExp.Expression, ref sourceCode, ref data);
            }
        }
        class UMethodInvokeStatementCodeGen : ICodeObjectGen
        {
            public void GenInvokeExpression(TtCodeObject obj, ref string sourceCode, ref TtCodeGeneratorData data)
            {
                var methodInvokeExp = obj as TtMethodInvokeStatement;
                if (methodInvokeExp.IsAsync)
                    sourceCode += "await ";
                if(methodInvokeExp.Host != null)
                {
                    var hostGen = data.CodeGen.GetCodeObjectGen(methodInvokeExp.Host.GetType());
                    hostGen.GenCodes(methodInvokeExp.Host, ref sourceCode, ref data);
                    sourceCode += ".";
                }
                sourceCode += methodInvokeExp.MethodName;
                if(methodInvokeExp.GenericTypes.Count > 0)
                {
                    sourceCode += "<";
                    for (int i = 0; i < methodInvokeExp.GenericTypes.Count; i++)
                    {
                        sourceCode += data.CodeGen.GetTypeString(methodInvokeExp.GenericTypes[i]) + ",";
                    }
                    sourceCode = sourceCode.TrimEnd(',');
                    sourceCode += ">";
                }
                sourceCode += "(";
                if(methodInvokeExp.Arguments.Count > 0)
                {
                    for(int i=0; i<methodInvokeExp.Arguments.Count; i++)
                    {
                        var argExp = data.CodeGen.GetCodeObjectGen(methodInvokeExp.Arguments[i].GetType());
                        argExp.GenCodes(methodInvokeExp.Arguments[i], ref sourceCode, ref data);
                        sourceCode += ",";
                    }
                    sourceCode = sourceCode.TrimEnd(',');
                }
                sourceCode += ")";
            }
            public void GenCodes(TtCodeObject obj, ref string sourceCode, ref TtCodeGeneratorData data)
            {
                var methodInvokeExp = obj as TtMethodInvokeStatement;
                string invokeStr = "";
                if (methodInvokeExp.ReturnValue != null)
                {
                    if (methodInvokeExp.DeclarationReturnValue)
                        invokeStr += data.CodeGen.GetTypeString(methodInvokeExp.ReturnValue.VariableType) + " ";
                    invokeStr += methodInvokeExp.ReturnValue.VariableName + " = " + (methodInvokeExp.IsReturnRef ? "ref " : "");
                    if (methodInvokeExp.ForceCastReturnType)
                        invokeStr += "(" + data.CodeGen.GetTypeString(methodInvokeExp.ReturnValue.VariableType) + ")";
                }

                GenInvokeExpression(obj, ref invokeStr, ref data);
                invokeStr += ";";
                data.CodeGen.AddLine(invokeStr, ref sourceCode);

                if (methodInvokeExp.IsUnsafe)
                    data.Method.HasUnsafeCode = true;
                if (methodInvokeExp.IsAsync)
                    data.Method.HasAwaitCode = true;
            }
        }

        class ULambdaExpressionCodeGen : ICodeObjectGen
        {
            public void GenCodes(TtCodeObject obj, ref string sourceCode, ref TtCodeGeneratorData data)
            {
                var exp = obj as TtLambdaExpression;
                if(exp.MethodInvoke != null && exp.LambdaArguments.Count == exp.MethodInvoke.Arguments.Count)
                {
                    sourceCode += exp.MethodInvoke.MethodName;
                }
                else
                {
                    if (exp.IsAsync)
                        sourceCode += "(async (";
                    else
                        sourceCode += "((";
                    if(exp.LambdaArguments.Count > 0)
                    {
                        for(int i=0; i<exp.LambdaArguments.Count; i++)
                        {
                            var argExp = data.CodeGen.GetCodeObjectGen(exp.LambdaArguments[i].GetType());
                            argExp.GenCodes(exp.LambdaArguments[i], ref sourceCode, ref data);
                            sourceCode += ",";
                        }
                        sourceCode = sourceCode.TrimEnd(',');
                    }
                    sourceCode += ")=> { ";
                    if(exp.MethodInvoke != null)
                    {
                        if(exp.ReturnType != null)
                            sourceCode += "return ";
                        var bodyCodeGen = data.CodeGen.GetCodeObjectGen(exp.MethodInvoke.GetType()) as UMethodInvokeStatementCodeGen;
                        bodyCodeGen.GenInvokeExpression(exp.MethodInvoke, ref sourceCode, ref data);
                        sourceCode += ";";
                    }
                    else if(exp.ReturnType != null)
                    {
                        sourceCode += "return default(" + data.CodeGen.GetTypeString(exp.ReturnType) + ");";
                    }
                    sourceCode += " })";
                }
            }
        }

        class UAssignOperatorStatementCodeGen : ICodeObjectGen
        {
            public void GenCodes(TtCodeObject obj, ref string sourceCode, ref TtCodeGeneratorData data)
            {
                var assignOpExp = obj as TtAssignOperatorStatement;
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
            public void GenCodes(TtCodeObject obj, ref string sourceCode, ref TtCodeGeneratorData data)
            {
                var binOpExp = obj as TtBinaryOperatorExpression;
                if(binOpExp.Cell)
                    sourceCode += "(";
                var leftGen = data.CodeGen.GetCodeObjectGen(binOpExp.Left.GetType());
                leftGen.GenCodes(binOpExp.Left, ref sourceCode, ref data);
                switch(binOpExp.Operation)
                {
                    case TtBinaryOperatorExpression.EBinaryOperation.Add:
                        sourceCode += " + ";
                        break;
                    case TtBinaryOperatorExpression.EBinaryOperation.Subtract:
                        sourceCode += " - ";
                        break;
                    case TtBinaryOperatorExpression.EBinaryOperation.Multiply:
                        sourceCode += " * ";
                        break;
                    case TtBinaryOperatorExpression.EBinaryOperation.Divide:
                        sourceCode += " / ";
                        break;
                    case TtBinaryOperatorExpression.EBinaryOperation.Modulus:
                        sourceCode += " % ";
                        break;
                    case TtBinaryOperatorExpression.EBinaryOperation.Inequality:
                        sourceCode += " != ";
                        break;
                    case TtBinaryOperatorExpression.EBinaryOperation.Equality:
                        sourceCode += " == ";
                        break;
                    case TtBinaryOperatorExpression.EBinaryOperation.NotEquality:
                        sourceCode += " != ";
                        break;
                    case TtBinaryOperatorExpression.EBinaryOperation.BitwiseOr:
                        sourceCode += " | ";
                        break;
                    case TtBinaryOperatorExpression.EBinaryOperation.BitwiseXOR:
                        sourceCode += " ^ ";
                        break;
                    case TtBinaryOperatorExpression.EBinaryOperation.BitwiseAnd:
                        sourceCode += " & ";
                        break;
                    case TtBinaryOperatorExpression.EBinaryOperation.BitwiseLeftShift:
                        sourceCode += " << ";
                        break;
                    case TtBinaryOperatorExpression.EBinaryOperation.BitwiseRightShift:
                        sourceCode += " >> ";
                        break;
                    case TtBinaryOperatorExpression.EBinaryOperation.BooleanOr:
                        sourceCode += " || ";
                        break;
                    case TtBinaryOperatorExpression.EBinaryOperation.BooleanAnd:
                        sourceCode += " && ";
                        break;
                    case TtBinaryOperatorExpression.EBinaryOperation.LessThan:
                        sourceCode += " < ";
                        break;
                    case TtBinaryOperatorExpression.EBinaryOperation.LessThanOrEqual:
                        sourceCode += " <= ";
                        break;
                    case TtBinaryOperatorExpression.EBinaryOperation.GreaterThan:
                        sourceCode += " > ";
                        break;
                    case TtBinaryOperatorExpression.EBinaryOperation.GreaterThanOrEqual:
                        sourceCode += " >= ";
                        break;
                    case TtBinaryOperatorExpression.EBinaryOperation.AddAssignment:
                        sourceCode += " += ";
                        break;
                    case TtBinaryOperatorExpression.EBinaryOperation.SubtractAssignment:
                        sourceCode += " -= ";
                        break;
                    case TtBinaryOperatorExpression.EBinaryOperation.Is:
                        sourceCode += " is ";
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
            public void GenCodes(TtCodeObject obj, ref string sourceCode, ref TtCodeGeneratorData data)
            {
                var unaryOpExp = obj as TtUnaryOperatorExpression;
                var valGen = data.CodeGen.GetCodeObjectGen(unaryOpExp.Value.GetType());
                switch(unaryOpExp.Operation)
                {
                    case TtUnaryOperatorExpression.EUnaryOperation.Negative:
                        {
                            sourceCode += "(-";
                            valGen.GenCodes(unaryOpExp.Value, ref sourceCode, ref data);
                            sourceCode += ")";
                        }
                        break;
                    case TtUnaryOperatorExpression.EUnaryOperation.BooleanNot:
                        {
                            sourceCode += "(!";
                            valGen.GenCodes(unaryOpExp.Value, ref sourceCode, ref data);
                            sourceCode += ")";
                        }
                        break;
                    case TtUnaryOperatorExpression.EUnaryOperation.BitwiseNot:
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
            public void GenCodes(TtCodeObject obj, ref string sourceCode, ref TtCodeGeneratorData data)
            {
                var indexerOpExp = obj as TtIndexerOperatorExpression;
                var tagGen = data.CodeGen.GetCodeObjectGen(indexerOpExp.Target.GetType());
                tagGen.GenCodes(indexerOpExp.Target, ref sourceCode, ref data);
                for(int i=0; i<indexerOpExp.Indices.Count; i++)
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
            public void GenCodes(TtCodeObject obj, ref string sourceCode, ref TtCodeGeneratorData data)
            {
                var primitiveExp = obj as TtPrimitiveExpression;
                if (primitiveExp.Type.IsEqual(typeof(Vector2)) ||
                    primitiveExp.Type.IsEqual(typeof(Vector3)) ||
                    primitiveExp.Type.IsEqual(typeof(Vector4)))
                    sourceCode += primitiveExp.Type.FullName + "(" + primitiveExp.ValueStr + ")";
                else if (primitiveExp.Type.IsEqual(typeof(string)))
                    sourceCode += $"\"{primitiveExp.ValueStr}\"";
                else if (primitiveExp.Type.IsEqual(typeof(float)))
                    sourceCode += primitiveExp.ValueStr + "f";
                else if(primitiveExp.Type.IsEnum)
                    sourceCode += data.CodeGen.GetTypeString(primitiveExp.Type) + "." + primitiveExp.ValueStr;
                else
                    sourceCode += primitiveExp.ValueStr;
            }
        }

        class UCastExpressionCodeGen : ICodeObjectGen
        {
            public void GenCodes(TtCodeObject obj, ref string sourceCode, ref TtCodeGeneratorData data)
            {
                var castExp = obj as TtCastExpression;

                var baseExp = castExp.Expression as TtCastExpression;
                while(true)
                {
                    if (baseExp != null)
                    {
                        if (baseExp.SourceType == castExp.TargetType)
                        {
                            var eGen = data.CodeGen.GetCodeObjectGen(baseExp.Expression.GetType());
                            eGen.GenCodes(baseExp.Expression, ref sourceCode, ref data);
                            return;
                        }
                    }
                    else
                        break;

                    baseExp = baseExp.Expression as TtCastExpression;
                }

                string srcExpStr = "";
                var expGen = data.CodeGen.GetCodeObjectGen(castExp.Expression.GetType());
                expGen.GenCodes(castExp.Expression, ref srcExpStr, ref data);
                if (castExp.TargetType.IsEqual(typeof(sbyte)))
                {
                    if(castExp.SourceType.IsEqual(typeof(string)))
                        sourceCode += $"System.Convert.ToSByte({srcExpStr})";
                    else
                        sourceCode += $"(System.SByte)({srcExpStr})";
                }
                else if (castExp.TargetType.IsEqual(typeof(Int16)))
                {
                    if (castExp.SourceType.IsEqual(typeof(string)))
                        sourceCode += $"System.Convert.ToInt16({srcExpStr})";
                    else
                        sourceCode += $"(System.Int16)({srcExpStr})";
                }
                else if (castExp.TargetType.IsEqual(typeof(Int32)))
                {
                    if (castExp.SourceType.IsEqual(typeof(string)))
                        sourceCode += $"System.Convert.ToInt32({srcExpStr})";
                    else
                        sourceCode += $"(System.Int32)({srcExpStr})";
                }
                else if (castExp.TargetType.IsEqual(typeof(Int64)))
                {
                    if (castExp.SourceType.IsEqual(typeof(string)))
                        sourceCode += $"System.Convert.ToInt64({srcExpStr})";
                    else
                        sourceCode += $"(System.Int64)({srcExpStr})";
                }
                else if (castExp.TargetType.IsEqual(typeof(byte)))
                {
                    if (castExp.SourceType.IsEqual(typeof(string)))
                        sourceCode += $"System.Convert.ToByte({srcExpStr})";
                    else
                        sourceCode += $"(System.Byte)({srcExpStr})";
                }
                else if (castExp.TargetType.IsEqual(typeof(UInt16)))
                {
                    if (castExp.SourceType.IsEqual(typeof(string)))
                        sourceCode += $"System.Convert.ToUInt16({srcExpStr})";
                    else
                        sourceCode += $"(System.UInt16)({srcExpStr})";
                }
                else if (castExp.TargetType.IsEqual(typeof(UInt32)))
                {
                    if (castExp.SourceType.IsEqual(typeof(string)))
                        sourceCode += $"System.Convert.ToUInt32({srcExpStr})";
                    else
                        sourceCode += $"(System.UInt32)({srcExpStr})";
                }
                else if (castExp.TargetType.IsEqual(typeof(UInt64)))
                {
                    if (castExp.SourceType.IsEqual(typeof(string)))
                        sourceCode += $"System.Convert.ToUInt64({srcExpStr})";
                    else
                        sourceCode += $"(System.UInt64)({srcExpStr})";
                }
                else if (castExp.TargetType.IsEqual(typeof(float)))
                {
                    if (castExp.SourceType.IsEqual(typeof(string)))
                        sourceCode += $"System.Convert.ToSingle({srcExpStr})";
                    else
                        sourceCode += $"(System.Single)({srcExpStr})";
                }
                else if (castExp.TargetType.IsEqual(typeof(double)))
                {
                    if (castExp.SourceType.IsEqual(typeof(string)))
                        sourceCode += $"System.Convert.ToDouble({srcExpStr})";
                    else
                        sourceCode += $"(System.Double)({srcExpStr})";
                }
                else if(castExp.TargetType.IsEqual(typeof(string)))
                    sourceCode += $"System.Convert.ToString({srcExpStr})";
                else
                    sourceCode += $"(({data.CodeGen.GetTypeString(castExp.TargetType)}){srcExpStr})";
            }
        }

        class UCreateObjectExpressionCodeGen : ICodeObjectGen
        {
            public void GenCodes(TtCodeObject obj, ref string sourceCode, ref TtCodeGeneratorData data)
            {
                var createExp = obj as TtCreateObjectExpression;
                sourceCode += "new " + createExp.TypeName + "(";
                for(int i=0; i<createExp.Parameters.Count; i++)
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
            public void GenCodes(TtCodeObject obj, ref string sourceCode, ref TtCodeGeneratorData data)
            {
                var defaultValExp = obj as TtDefaultValueExpression;
                if (defaultValExp.Type.IsRefType == false)
                {
                    sourceCode += "default(" + data.CodeGen.GetTypeString(defaultValExp.Type) + ")";
                }
                else
                {   
                    sourceCode += $"ref EngineNS.Rtti.UTypeDescGetter<{defaultValExp.Type.TypeFullName.Substring(4)}>.DefaultObject";
                }
            }
        }

        class UNullValueExpressionCodeGen : ICodeObjectGen
        {
            public void GenCodes(TtCodeObject obj, ref string sourceCode, ref TtCodeGeneratorData data)
            {
                sourceCode += "null";
            }
        }

        class TtTypeOfExpressionCodeGen : ICodeObjectGen
        {
            public void GenCodes(TtCodeObject obj, ref string sourceCode, ref TtCodeGeneratorData data)
            {
                var ex = obj as TtTypeOfExpression;
                sourceCode += "typeof(" + data.CodeGen.GetTypeString(ex.Variable) + ")";
            }
        }

        class UExecuteSequenceStatementCodeGen : ICodeObjectGen
        {
            public void GenCodes(TtCodeObject obj, ref string sourceCode, ref TtCodeGeneratorData data)
            {
                var exeSeqExp = obj as TtExecuteSequenceStatement;
                for(int i=0; i<exeSeqExp.Sequence.Count; i++)
                {
                    var seqGen = data.CodeGen.GetCodeObjectGen(exeSeqExp.Sequence[i].GetType());
                    seqGen.GenCodes(exeSeqExp.Sequence[i], ref sourceCode, ref data);
                }
            }
        }

        class UReturnStatementCodeGen : ICodeObjectGen
        {
            public void GenCodes(TtCodeObject obj, ref string sourceCode, ref TtCodeGeneratorData data)
            {
                var retExp = obj as TtReturnStatement;
                string retStr = "return";
                if(data.Method.ReturnValue != null)
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
            public void GenCodes(TtCodeObject obj, ref string sourceCode, ref TtCodeGeneratorData data)
            {
                var ifExp = obj as TtIfStatement;
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
                for(int i=0; i<ifExp.ElseIfs.Count; i++)
                {
                    string elseIfStr = "else if (";
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

                if(ifExp.Next != null)
                {
                    var nextGen = data.CodeGen.GetCodeObjectGen(ifExp.Next.GetType());
                    nextGen.GenCodes(ifExp.Next, ref sourceCode, ref data);
                }
            }
        }

        class UForLoopStatementCodeGen : ICodeObjectGen
        {
            public void GenCodes(TtCodeObject obj, ref string sourceCode, ref TtCodeGeneratorData data)
            {
                var forExp = obj as TtForLoopStatement;
                string forStr = "for (var " + forExp.LoopIndexName + " = ";
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
                    if(forExp.LoopBody != null)
                    {
                        var bodyGen = data.CodeGen.GetCodeObjectGen(forExp.LoopBody.GetType());
                        bodyGen.GenCodes(forExp.LoopBody, ref sourceCode, ref data);
                    }
                }
                data.CodeGen.PopSegment(ref sourceCode, in data);

                if(forExp.Next != null)
                {
                    var nextGen = data.CodeGen.GetCodeObjectGen(forExp.Next.GetType());
                    nextGen.GenCodes(forExp.Next, ref sourceCode, ref data);
                }
            }
        }

        class UWhileLoopStatementCodeGen : ICodeObjectGen
        {
            public void GenCodes(TtCodeObject obj, ref string sourceCode, ref TtCodeGeneratorData data)
            {
                var whileExp = obj as TtWhileLoopStatement;
                string whileStr = "while (";
                var condExpGen = data.CodeGen.GetCodeObjectGen(whileExp.Condition.GetType());
                condExpGen.GenCodes(whileExp.Condition, ref whileStr, ref data);
                whileStr += ")";
                data.CodeGen.AddLine(whileStr, ref sourceCode);
                data.CodeGen.PushSegment(ref sourceCode, in data);
                {
                    if(whileExp.LoopBody != null)
                    {
                        var bodyGen = data.CodeGen.GetCodeObjectGen(whileExp.LoopBody.GetType());
                        bodyGen.GenCodes(whileExp.LoopBody, ref sourceCode, ref data);
                    }
                }
                data.CodeGen.PopSegment(ref sourceCode, in data);
            }
        }

        class UContinueStatementCodeGen : ICodeObjectGen
        {
            public void GenCodes(TtCodeObject obj, ref string sourceCode, ref TtCodeGeneratorData data)
            {
                data.CodeGen.AddLine("continue;", ref sourceCode);
            }
        }

        class UBreakStatementCodeGen : ICodeObjectGen
        {
            public void GenCodes(TtCodeObject obj, ref string sourceCode, ref TtCodeGeneratorData data)
            {
                data.CodeGen.AddLine("break;", ref sourceCode);
            }
        }

        class UCommentStatementCodeGen : ICodeObjectGen
        {
            public void GenCodes(TtCodeObject obj, ref string sourceCode, ref TtCodeGeneratorData data)
            {
                var commentExp = obj as TtCommentStatement;
                if(!string.IsNullOrEmpty(commentExp.CommentString))
                {
                    var comment = "//" + commentExp.CommentString;
                    data.CodeGen.AddLine(comment, ref sourceCode);
                }
            }
        }

        class UExpressionStatementCodeGen : ICodeObjectGen
        {
            public void GenCodes(TtCodeObject obj, ref string sourceCode, ref TtCodeGeneratorData data)
            {
                var expSt = obj as TtExpressionStatement;
                var expGen = data.CodeGen.GetCodeObjectGen(expSt.Expression.GetType());
                string tempCode = "";
                expGen.GenCodes(expSt.Expression, ref tempCode, ref data);
                tempCode += ";";
                data.CodeGen.AddLine(tempCode, ref sourceCode);

                if(expSt.Next != null)
                {
                    var nextGen = data.CodeGen.GetCodeObjectGen(expSt.Next.GetType());
                    nextGen.GenCodes(expSt.Next, ref sourceCode, ref data);
                }
            }
        }

        UDebuggerSetWatchVariableCodeGen mDebuggerSetWatchVariableCodeGen = new UDebuggerSetWatchVariableCodeGen();
        UDebuggerTryBreakCodeGen mDebuggerTryBreakCodeGen = new UDebuggerTryBreakCodeGen();
        TtAttributeCodeGen mAttributeCodeGen = new TtAttributeCodeGen();
        UVariableDeclarationCodeGen mVariableDeclarationCodeGen = new UVariableDeclarationCodeGen();
        UMethodDeclarationCodeGen mMethodDeclarationCodeGen = new UMethodDeclarationCodeGen();
        UClassDeclarationCodeGen mClassDeclarationCodeGen = new UClassDeclarationCodeGen();
        UClassReferenceExpressionCodeGen mClassReferenceExpressionCodeGen = new UClassReferenceExpressionCodeGen();
        UVariableReferenceExpressionCodeGen mVariableReferenceExpressionCodeGen = new UVariableReferenceExpressionCodeGen();
        USelfReferenceExpressionCodeGen mSelfReferenceExpressionCodeGen = new USelfReferenceExpressionCodeGen();
        UBaseReferenceExpresiionCodeGen mBaseReferenceExpresiionCodeGen = new UBaseReferenceExpresiionCodeGen();
        UMethodInvokeArgumentExpressionCodeGen mMethodInvokeArgumentExpressionCodeGen = new UMethodInvokeArgumentExpressionCodeGen();
        UMethodInvokeStatementCodeGen mMethodInvokeStatementCodeGen = new UMethodInvokeStatementCodeGen();
        ULambdaExpressionCodeGen mLambdaExpressionCodeGen = new ULambdaExpressionCodeGen();
        UAssignOperatorStatementCodeGen mAssignOperatorStatementCodeGen = new UAssignOperatorStatementCodeGen();
        UBinaryOperatorExpressionCodeGen mBinaryOperatorExpressionCodeGen = new UBinaryOperatorExpressionCodeGen();
        UUnaryOperatorExpressionCodeGen mUnaryOperatorExpressionCodeGen = new UUnaryOperatorExpressionCodeGen();
        UIndexerOperatorExpressionCodeGen mIndexerOperatorExpressionCodeGen = new UIndexerOperatorExpressionCodeGen();
        UPrimitiveExpressionCodeGen mPrimitiveExpressionCodeGen = new UPrimitiveExpressionCodeGen();
        UCastExpressionCodeGen mCastExpressionCodeGen = new UCastExpressionCodeGen();
        UCreateObjectExpressionCodeGen mCreateObjectExpressionCodeGen = new UCreateObjectExpressionCodeGen();
        UDefaultValueExpressionCodeGen mDefaultValueExpressionCodeGen = new UDefaultValueExpressionCodeGen();
        UNullValueExpressionCodeGen mNullValueExpressionCodeGen = new UNullValueExpressionCodeGen();
        TtTypeOfExpressionCodeGen mTypeOfExpressionCodeGen = new TtTypeOfExpressionCodeGen();
        UExecuteSequenceStatementCodeGen mExecuteSequenceStatementCodeGen = new UExecuteSequenceStatementCodeGen();
        UReturnStatementCodeGen mReturnStatementCodeGen = new UReturnStatementCodeGen();
        UIfStatementCodeGen mIfStatementCodeGen = new UIfStatementCodeGen();
        UForLoopStatementCodeGen mForLoopStatementCodeGen = new UForLoopStatementCodeGen();
        UWhileLoopStatementCodeGen mWhileLoopStatementCodeGen = new UWhileLoopStatementCodeGen();
        UContinueStatementCodeGen mContinueStatementCodeGen = new UContinueStatementCodeGen();
        UBreakStatementCodeGen mBreakStatementCodeGen = new UBreakStatementCodeGen();
        UCommentStatementCodeGen mCommentStatementCodeGen = new UCommentStatementCodeGen();
        UExpressionStatementCodeGen mExpressionStatementCodeGen = new UExpressionStatementCodeGen();

        public override ICodeObjectGen GetCodeObjectGen(Rtti.TtTypeDesc type)
        {
            if (type.IsEqual(typeof(TtDebuggerSetWatchVariable)))
                return mDebuggerSetWatchVariableCodeGen;
            else if (type.IsEqual(typeof(TtDebuggerTryBreak)))
                return mDebuggerTryBreakCodeGen;
            else if (type.IsEqual(typeof(TtAttribute)))
                return mAttributeCodeGen;
            else if (type.IsEqual(typeof(TtVariableDeclaration)))
                return mVariableDeclarationCodeGen;
            else if (type.IsEqual(typeof(TtMethodDeclaration)))
                return mMethodDeclarationCodeGen;
            else if (type.IsEqual(typeof(TtClassDeclaration)))
                return mClassDeclarationCodeGen;
            else if (type.IsEqual(typeof(TtClassReferenceExpression)))
                return mClassReferenceExpressionCodeGen;
            else if (type.IsEqual(typeof(TtVariableReferenceExpression)))
                return mVariableReferenceExpressionCodeGen;
            else if (type.IsEqual(typeof(TtSelfReferenceExpression)))
                return mSelfReferenceExpressionCodeGen;
            else if (type.IsEqual(typeof(TtBaseReferenceExpression)))
                return mBaseReferenceExpresiionCodeGen;
            else if (type.IsEqual(typeof(TtMethodInvokeArgumentExpression)))
                return mMethodInvokeArgumentExpressionCodeGen;
            else if (type.IsEqual(typeof(TtMethodInvokeStatement)))
                return mMethodInvokeStatementCodeGen;
            else if (type.IsEqual(typeof(TtLambdaExpression)))
                return mLambdaExpressionCodeGen;
            else if (type.IsEqual(typeof(TtAssignOperatorStatement)))
                return mAssignOperatorStatementCodeGen;
            else if (type.IsEqual(typeof(TtBinaryOperatorExpression)))
                return mBinaryOperatorExpressionCodeGen;
            else if (type.IsEqual(typeof(TtUnaryOperatorExpression)))
                return mUnaryOperatorExpressionCodeGen;
            else if (type.IsEqual(typeof(TtIndexerOperatorExpression)))
                return mIndexerOperatorExpressionCodeGen;
            else if (type.IsEqual(typeof(TtPrimitiveExpression)))
                return mPrimitiveExpressionCodeGen;
            else if (type.IsEqual(typeof(TtCastExpression)))
                return mCastExpressionCodeGen;
            else if (type.IsEqual(typeof(TtCreateObjectExpression)))
                return mCreateObjectExpressionCodeGen;
            else if (type.IsEqual(typeof(TtDefaultValueExpression)))
                return mDefaultValueExpressionCodeGen;
            else if (type.IsEqual(typeof(TtNullValueExpression)))
                return mNullValueExpressionCodeGen;
            else if (type.IsEqual(typeof(TtTypeOfExpression)))
                return mTypeOfExpressionCodeGen;
            else if (type.IsEqual(typeof(TtExecuteSequenceStatement)))
                return mExecuteSequenceStatementCodeGen;
            else if (type.IsEqual(typeof(TtReturnStatement)))
                return mReturnStatementCodeGen;
            else if (type.IsEqual(typeof(TtIfStatement)))
                return mIfStatementCodeGen;
            else if (type.IsEqual(typeof(TtForLoopStatement)))
                return mForLoopStatementCodeGen;
            else if (type.IsEqual(typeof(TtWhileLoopStatement)))
                return mWhileLoopStatementCodeGen;
            else if (type.IsEqual(typeof(TtContinueStatement)))
                return mContinueStatementCodeGen;
            else if (type.IsEqual(typeof(TtBreakStatement)))
                return mBreakStatementCodeGen;
            else if (type.IsEqual(typeof(TtCommentStatement)))
                return mCommentStatementCodeGen;
            else if (type.IsEqual(typeof(TtExpressionStatement)))
                return mExpressionStatementCodeGen;
            return null;
        }

        public override string GetTypeString(TtTypeReference t)
        {
            if (t.TypeDesc != null)
                return GetTypeString(t.TypeDesc);
            else
                return t.TypeFullName;
        }
        public override string GetTypeString(TtTypeDesc t)
        {
            if (t == null)
                return "";
            return base.GetTypeString(t);
            //if (t.SystemType != null)
            //{
            //    return Rtti.TtTypeDesc.GetCSharpTypeNameString(t.SystemType);
            //}
            //else
            //    return t.FullName;
        }
    }

    public class UTest_CSharpBackend
    {
        public void UnitTestEntrance()
        {

        }
    }
}
