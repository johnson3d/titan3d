using System;
using System.Text;

namespace EngineNS.Bricks.CodeBuilder
{
    /// <summary>
    /// 生成 Macross 方法首行的救命热更入口桩；签名与 MethodId 规则见 EngineNS.Macross.TtMacrossPatchId。
    /// </summary>
    public static class TtMacrossPatchStubCodeGen
    {
        public const int MaxArguments = 8;

        public static bool CanPatch(TtClassDeclaration cls, TtMethodDeclaration method)
        {
            if (cls == null || cls.IsStruct)
                return false;
            if (!string.IsNullOrEmpty(method.RawBodyCode) || method.AsyncType != TtMethodDeclaration.EAsyncType.None)
                return false;
            if (method.Host != null || method.IsUnsafe || method.Arguments.Count > MaxArguments)
                return false;
            foreach (var arg in method.Arguments)
            {
                if (arg.OperationType != EMethodArgumentAttribute.Default || arg.IsParamArray)
                    return false;
            }
            return true;
        }

        public static string BuildSignature(string classFullName, TtMethodDeclaration method, Func<TtTypeReference, string> getTypeString)
        {
            var sb = new StringBuilder();
            sb.Append(classFullName).Append("::").Append(method.MethodName).Append('(');
            for (var i = 0; i < method.Arguments.Count; ++i)
            {
                if (i > 0)
                    sb.Append(',');
                sb.Append(getTypeString(method.Arguments[i].VariableType));
            }
            return sb.Append(')').ToString();
        }

        public static string[] BuildStub(ulong methodId, TtMethodDeclaration method, Func<TtTypeReference, string> getTypeString)
        {
            var sb = new StringBuilder();
            sb.Append("if (EngineNS.Macross.TtMacrossPatch.Active && EngineNS.Macross.TtMacrossPatch.TryInvoke(0x")
              .Append(methodId.ToString("X16")).Append("UL, this");
            if (method.ReturnValue != null)
                sb.Append(", out ").Append(getTypeString(method.ReturnValue.VariableType)).Append(" __mcPatchRet");
            foreach (var arg in method.Arguments)
                sb.Append(", ").Append(arg.VariableName);
            sb.Append("))");
            return new[] { sb.ToString(), method.ReturnValue != null ? "    return __mcPatchRet;" : "    return;" };
        }
    }
}
