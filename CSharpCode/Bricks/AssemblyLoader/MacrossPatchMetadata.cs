using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text;

namespace EngineNS.Macross
{
    /// <summary>
    /// 读取补丁 dll 元数据中 __Patch 类声明的方法；签名格式与入口桩一致（宿主类名::方法名(参数全名,...)）。
    /// </summary>
    public static class TtMacrossPatchMetadata
    {
        public sealed class FMethod
        {
            public string Name;
            public string Signature;
            public string ReturnType;
        }

        /// <summary>
        /// 返回 patchClassFullName 声明的方法，键为补丁类全名；构造函数和编译器生成的方法（名字以 '&lt;' 开头）不计入。
        /// </summary>
        public static Dictionary<string, List<FMethod>> ReadPatchMethods(string patchDllPath, ICollection<string> patchClassFullNames, string patchClassSuffix)
        {
            var result = new Dictionary<string, List<FMethod>>();
            using var stream = File.OpenRead(patchDllPath);
            using var pe = new PEReader(stream);
            var reader = pe.GetMetadataReader();
            var provider = new FTypeNameProvider();
            foreach (var typeHandle in reader.TypeDefinitions)
            {
                var type = reader.GetTypeDefinition(typeHandle);
                var fullName = FTypeNameProvider.GetFullName(reader, type);
                if (!patchClassFullNames.Contains(fullName))
                    continue;
                var hostClass = fullName.Substring(0, fullName.Length - patchClassSuffix.Length);
                var methods = new List<FMethod>();
                foreach (var methodHandle in type.GetMethods())
                {
                    var method = reader.GetMethodDefinition(methodHandle);
                    var name = reader.GetString(method.Name);
                    if (name == ".ctor" || name == ".cctor" || name.StartsWith("<", StringComparison.Ordinal))
                        continue;
                    var sig = method.DecodeSignature(provider, null);
                    methods.Add(new FMethod
                    {
                        Name = name,
                        Signature = BuildSignature(hostClass, name, sig.ParameterTypes),
                        ReturnType = sig.ReturnType,
                    });
                }
                result[fullName] = methods;
            }
            return result;
        }

        public static string BuildSignature(string hostClass, string methodName, IReadOnlyList<string> parameterTypes)
        {
            var sb = new StringBuilder();
            sb.Append(hostClass).Append("::").Append(methodName).Append('(');
            for (var i = 0; i < parameterTypes.Count; ++i)
            {
                if (i > 0)
                    sb.Append(',');
                sb.Append(parameterTypes[i]);
            }
            return sb.Append(')').ToString();
        }

        /// <summary>
        /// 宿主方法的签名，类型名规则与 Rtti.TtTypeDesc.GetCSharpTypeNameString 一致。
        /// </summary>
        public static string BuildSignature(string hostClass, MethodInfo method)
        {
            var parameters = method.GetParameters();
            var types = new string[parameters.Length];
            for (var i = 0; i < parameters.Length; ++i)
                types[i] = Rtti.TtTypeDesc.GetCSharpTypeNameString(parameters[i].ParameterType);
            return BuildSignature(hostClass, method.Name, types);
        }

        // 类型名对齐 GetCSharpTypeNameString：嵌套类型用 '.'，泛型去掉 `N 后接 <参数>
        private sealed class FTypeNameProvider : ISignatureTypeProvider<string, object>
        {
            public static string GetFullName(MetadataReader reader, TypeDefinition type)
            {
                var name = reader.GetString(type.Name);
                if (type.IsNested)
                    return GetFullName(reader, reader.GetTypeDefinition(type.GetDeclaringType())) + "." + name;
                var ns = reader.GetString(type.Namespace);
                return string.IsNullOrEmpty(ns) ? name : ns + "." + name;
            }

            private static string GetFullName(MetadataReader reader, TypeReference type)
            {
                var name = reader.GetString(type.Name);
                if (type.ResolutionScope.Kind == HandleKind.TypeReference)
                    return GetFullName(reader, reader.GetTypeReference((TypeReferenceHandle)type.ResolutionScope)) + "." + name;
                var ns = reader.GetString(type.Namespace);
                return string.IsNullOrEmpty(ns) ? name : ns + "." + name;
            }

            public string GetPrimitiveType(PrimitiveTypeCode typeCode) => "System." + typeCode;
            public string GetTypeFromDefinition(MetadataReader reader, TypeDefinitionHandle handle, byte rawTypeKind) => GetFullName(reader, reader.GetTypeDefinition(handle));
            public string GetTypeFromReference(MetadataReader reader, TypeReferenceHandle handle, byte rawTypeKind) => GetFullName(reader, reader.GetTypeReference(handle));
            public string GetTypeFromSpecification(MetadataReader reader, object genericContext, TypeSpecificationHandle handle, byte rawTypeKind) =>
                reader.GetTypeSpecification(handle).DecodeSignature(this, genericContext);
            public string GetSZArrayType(string elementType) => elementType + "[]";
            public string GetArrayType(string elementType, ArrayShape shape) => elementType + "[" + new string(',', shape.Rank - 1) + "]";
            public string GetByReferenceType(string elementType) => elementType + "&";
            public string GetPointerType(string elementType) => elementType + "*";
            public string GetPinnedType(string elementType) => elementType;
            public string GetModifiedType(string modifier, string unmodifiedType, bool isRequired) => unmodifiedType;
            public string GetFunctionPointerType(MethodSignature<string> signature) => "method*";
            public string GetGenericMethodParameter(object genericContext, int index) => "!!" + index;
            public string GetGenericTypeParameter(object genericContext, int index) => "!" + index;

            public string GetGenericInstantiation(string genericType, ImmutableArray<string> typeArguments)
            {
                var tick = genericType.LastIndexOf('`');
                return (tick < 0 ? genericType : genericType.Substring(0, tick)) + "<" + string.Join(",", typeArguments) + ">";
            }
        }
    }
}
