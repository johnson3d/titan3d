using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EngineNS.LeanCLR.Generator
{
    /// <summary>
    /// 为游戏插件中 [TtLeanClr] 类的 [Rtti.Meta]（无 NoMacrossUseable）方法生成：
    /// TtLeanClrRegistry.g.cs（intrinsic 代理 + 注册表，宿主插件编译）与 TtLeanClrStubs.g.cs（VM 桩源码，桩工程编译）。
    /// </summary>
    [Generator(LanguageNames.CSharp)]
    public sealed class TtLeanClrGenerator : IIncrementalGenerator
    {
        internal const string LeanClrAttributeName = "EngineNS.LeanCLR.TtLeanClrAttribute";
        internal const string MetaAttributeName = "EngineNS.Rtti.MetaAttribute";
        internal const string HostObjectName = "global::EngineNS.LeanCLR.TtLeanClrHostObject";
        private const ulong NoMacrossUseable = 1;

        internal static readonly DiagnosticDescriptor UnsupportedMethod = new DiagnosticDescriptor(
            "TTLC001",
            "LeanCLR 代理不支持该方法",
            "{0}.{1} 已跳过：{2} 不受 LeanCLR 代理支持",
            "LeanCLR",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        internal static readonly DiagnosticDescriptor UnsupportedClass = new DiagnosticDescriptor(
            "TTLC002",
            "LeanCLR 代理不支持该类型",
            "{0} 已跳过：LeanCLR 代理不支持嵌套类或泛型类",
            "LeanCLR",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var classes = context.SyntaxProvider.ForAttributeWithMetadataName(
                    LeanClrAttributeName,
                    static (node, _) => node is ClassDeclarationSyntax,
                    static (ctx, ct) => ExtractClass((INamedTypeSymbol)ctx.TargetSymbol, ct))
                .Collect();
            var assemblyName = context.CompilationProvider.Select(static (c, _) => c.AssemblyName ?? string.Empty);

            context.RegisterSourceOutput(classes.Combine(assemblyName), static (spc, input) => Emit(spc, input.Left, input.Right));
        }

        private static void Emit(SourceProductionContext context, ImmutableArray<ClassResult> results, string assemblyName)
        {
            var models = new List<ClassModel>();
            var seen = new HashSet<string>();
            foreach (var result in results)
            {
                foreach (var diagnostic in result.Diagnostics)
                {
                    var descriptor = diagnostic.Id == UnsupportedClass.Id ? UnsupportedClass : UnsupportedMethod;
                    context.ReportDiagnostic(Diagnostic.Create(descriptor, diagnostic.Location?.ToLocation(), diagnostic.Arguments.Cast<object>().ToArray()));
                }
                // 分部类的每个带特性的声明都会产出一次
                if (result.Class != null && seen.Add(result.Class.FullName))
                    models.Add(result.Class);
            }
            models.Sort((l, r) => string.CompareOrdinal(l.FullName, r.FullName));

            // 桩文件始终输出，桩工程据此收集；没有宿主类时注册表不输出
            context.AddSource("TtLeanClrStubs.g.cs", LeanClrEmitter.EmitStubs(models));
            if (models.Count > 0)
                context.AddSource("TtLeanClrRegistry.g.cs", LeanClrEmitter.EmitRegistry(models, assemblyName));
        }

        internal static ClassResult ExtractClass(INamedTypeSymbol type, CancellationToken cancellationToken)
        {
            var diagnostics = new List<DiagnosticInfo>();
            if (type.ContainingType != null || type.IsGenericType)
            {
                diagnostics.Add(new DiagnosticInfo(UnsupportedClass.Id, LocationInfo.From(type.Locations.FirstOrDefault()),
                    new EquatableArray<string>(new[] { type.ToDisplayString() })));
                return new ClassResult(null, new EquatableArray<DiagnosticInfo>(diagnostics.ToArray()));
            }

            var fullName = GetFullName(type);
            var methods = new List<MethodModel>();
            foreach (var method in type.GetMembers().OfType<IMethodSymbol>())
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (method.MethodKind != MethodKind.Ordinary || !IsExposed(method))
                    continue;
                var model = ExtractMethod(type, fullName, method, out var reason);
                if (model == null)
                {
                    diagnostics.Add(new DiagnosticInfo(UnsupportedMethod.Id, LocationInfo.From(method.Locations.FirstOrDefault()),
                        new EquatableArray<string>(new[] { fullName, method.Name, reason! })));
                    continue;
                }
                methods.Add(model);
            }

            var stubBase = IsHostType(type.BaseType) ? "global::" + GetFullName(type.BaseType!) : HostObjectName;
            var ns = type.ContainingNamespace.IsGlobalNamespace ? string.Empty : type.ContainingNamespace.ToDisplayString();
            var classModel = new ClassModel(fullName, ns, type.Name, "global::" + fullName, stubBase,
                new EquatableArray<MethodModel>(methods.ToArray()));
            return new ClassResult(classModel, new EquatableArray<DiagnosticInfo>(diagnostics.ToArray()));
        }

        private static MethodModel? ExtractMethod(INamedTypeSymbol type, string fullName, IMethodSymbol method, out string? reason)
        {
            reason = null;
            if (method.IsGenericMethod)
            {
                reason = "泛型方法";
                return null;
            }
            if (method.ReturnsByRef || method.ReturnsByRefReadonly)
            {
                reason = "ref 返回值";
                return null;
            }
            var returnType = MapType(method.ReturnType, allowVoid: true);
            if (returnType == null)
            {
                reason = $"返回类型 {method.ReturnType.ToDisplayString()}";
                return null;
            }

            var parameters = new List<ParameterModel>();
            foreach (var parameter in method.Parameters)
            {
                if (parameter.RefKind != RefKind.None)
                {
                    reason = $"ref/out/in 参数 {parameter.Name}";
                    return null;
                }
                if (parameter.IsParams)
                {
                    reason = $"params 参数 {parameter.Name}";
                    return null;
                }
                var parameterType = MapType(parameter.Type, allowVoid: false);
                if (parameterType == null)
                {
                    reason = $"参数 {parameter.Name} 的类型 {parameter.Type.ToDisplayString()}";
                    return null;
                }
                parameters.Add(new ParameterModel(parameter.Name, parameterType));
            }

            var needsAccessor = method.DeclaredAccessibility is Accessibility.Private or Accessibility.Protected or Accessibility.ProtectedAndInternal;
            if (needsAccessor && type.IsStatic)
            {
                // UnsafeAccessor 需要以目标类型作参数，静态类不能作参数类型
                reason = "静态类的非公开方法";
                return null;
            }

            var signature = fullName + "::" + method.Name + "(" + string.Join(",", parameters.Select(p => p.Type.MetadataName)) + ")";
            return new MethodModel(method.Name, method.IsStatic, needsAccessor, returnType,
                new EquatableArray<ParameterModel>(parameters.ToArray()), signature);
        }

        private static bool IsExposed(IMethodSymbol method)
        {
            foreach (var attribute in method.GetAttributes())
            {
                if (attribute.AttributeClass?.ToDisplayString() != MetaAttributeName)
                    continue;
                foreach (var argument in attribute.NamedArguments)
                {
                    if (argument.Key == "Flags" && argument.Value.Value != null &&
                        (System.Convert.ToUInt64(argument.Value.Value) & NoMacrossUseable) != 0)
                        return false;
                }
                return true;
            }
            return false;
        }

        private static bool IsHostType(ITypeSymbol? type)
        {
            return type is INamedTypeSymbol named &&
                   named.TypeKind == TypeKind.Class &&
                   named.ContainingType == null &&
                   !named.IsGenericType &&
                   named.GetAttributes().Any(a => a.AttributeClass?.ToDisplayString() == LeanClrAttributeName);
        }

        private static string GetFullName(INamedTypeSymbol type)
        {
            return type.ContainingNamespace.IsGlobalNamespace
                ? type.MetadataName
                : type.ContainingNamespace.ToDisplayString() + "." + type.MetadataName;
        }

        private static TypeModel? MapType(ITypeSymbol type, bool allowVoid)
        {
            switch (type.SpecialType)
            {
                case SpecialType.System_Void: return allowVoid ? new TypeModel(ETypeKind.Void, "System.Void", "void") : null;
                case SpecialType.System_Boolean: return new TypeModel(ETypeKind.Boolean, "System.Boolean", "bool");
                case SpecialType.System_Char: return new TypeModel(ETypeKind.Char, "System.Char", "char");
                case SpecialType.System_SByte: return new TypeModel(ETypeKind.SByte, "System.SByte", "sbyte");
                case SpecialType.System_Byte: return new TypeModel(ETypeKind.Byte, "System.Byte", "byte");
                case SpecialType.System_Int16: return new TypeModel(ETypeKind.Int16, "System.Int16", "short");
                case SpecialType.System_UInt16: return new TypeModel(ETypeKind.UInt16, "System.UInt16", "ushort");
                case SpecialType.System_Int32: return new TypeModel(ETypeKind.Int32, "System.Int32", "int");
                case SpecialType.System_UInt32: return new TypeModel(ETypeKind.UInt32, "System.UInt32", "uint");
                case SpecialType.System_Int64: return new TypeModel(ETypeKind.Int64, "System.Int64", "long");
                case SpecialType.System_UInt64: return new TypeModel(ETypeKind.UInt64, "System.UInt64", "ulong");
                case SpecialType.System_Single: return new TypeModel(ETypeKind.Single, "System.Single", "float");
                case SpecialType.System_Double: return new TypeModel(ETypeKind.Double, "System.Double", "double");
                case SpecialType.System_String: return new TypeModel(ETypeKind.String, "System.String", "string");
            }
            if (IsHostType(type))
            {
                var fullName = GetFullName((INamedTypeSymbol)type);
                return new TypeModel(ETypeKind.HostObject, fullName, "global::" + fullName);
            }
            // enum → 按底层整数类型传递
            if (type.TypeKind == TypeKind.Enum && type is INamedTypeSymbol enumType)
            {
                var underlying = MapType(enumType.EnumUnderlyingType!, allowVoid: false);
                if (underlying != null)
                {
                    var fullName = GetFullName(enumType);
                    return new TypeModel(ETypeKind.Enum, fullName, "global::" + fullName, underlying.CSharpName);
                }
            }
            // blittable struct → 按字节拷贝
            if (type.TypeKind == TypeKind.Struct && type is INamedTypeSymbol structType &&
                !structType.IsGenericType && structType.ContainingType == null)
            {
                var size = TryGetBlittableSize(structType);
                if (size > 0)
                {
                    var fullName = GetFullName(structType);
                    return new TypeModel(ETypeKind.BlittableStruct, fullName, "global::" + fullName, SizeInBytes: size);
                }
            }
            return null;
        }

        /// <summary>
        /// 计算 blittable struct 大小；返回 0 表示不可 blittable。
        /// 要求：有 StructLayout(Sequential/Explicit)，所有实例字段均为基础数值类型或已知 blittable struct。
        /// </summary>
        private static int TryGetBlittableSize(INamedTypeSymbol structType)
        {
            var hasLayout = false;
            foreach (var attr in structType.GetAttributes())
            {
                if (attr.AttributeClass?.ToDisplayString() == "System.Runtime.InteropServices.StructLayoutAttribute")
                {
                    hasLayout = true;
                    break;
                }
            }
            if (!hasLayout)
                return 0;

            var totalSize = 0;
            foreach (var member in structType.GetMembers())
            {
                if (member is IFieldSymbol field && !field.IsStatic && !field.IsConst)
                {
                    var fieldSize = GetPrimitiveSize(field.Type);
                    if (fieldSize <= 0)
                    {
                        // 递归检查嵌套 blittable struct
                        if (field.Type.TypeKind == TypeKind.Struct && field.Type is INamedTypeSymbol nestedStruct &&
                            !nestedStruct.IsGenericType)
                        {
                            fieldSize = TryGetBlittableSize(nestedStruct);
                        }
                        if (fieldSize <= 0)
                            return 0;
                    }
                    totalSize += fieldSize;
                }
            }
            return totalSize > 0 ? totalSize : 0;
        }

        private static int GetPrimitiveSize(ITypeSymbol type)
        {
            switch (type.SpecialType)
            {
                case SpecialType.System_Boolean:
                case SpecialType.System_SByte:
                case SpecialType.System_Byte:
                    return 1;
                case SpecialType.System_Char:
                case SpecialType.System_Int16:
                case SpecialType.System_UInt16:
                    return 2;
                case SpecialType.System_Int32:
                case SpecialType.System_UInt32:
                case SpecialType.System_Single:
                    return 4;
                case SpecialType.System_Int64:
                case SpecialType.System_UInt64:
                case SpecialType.System_Double:
                    return 8;
                default:
                    return 0;
            }
        }

        internal static string EscapeIdentifier(string name)
        {
            return SyntaxFacts.GetKeywordKind(name) != SyntaxKind.None ? "@" + name : name;
        }
    }
}
