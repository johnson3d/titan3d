using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace System.Runtime.CompilerServices
{
    // netstandard2.0 缺少 record/init 所需的标记类型
    internal static class IsExternalInit
    {
    }
}

namespace EngineNS.LeanCLR.Generator
{
    /// <summary>
    /// 值相等的只读数组，保证增量管线的模型可比较、可缓存。
    /// </summary>
    internal readonly struct EquatableArray<T> : IEquatable<EquatableArray<T>>, IEnumerable<T>
        where T : IEquatable<T>
    {
        private readonly T[]? mItems;

        public EquatableArray(T[] items)
        {
            mItems = items;
        }

        public int Count => mItems?.Length ?? 0;
        public T this[int index] => mItems![index];

        public bool Equals(EquatableArray<T> other)
        {
            if (Count != other.Count)
                return false;
            for (var i = 0; i < Count; ++i)
            {
                if (!EqualityComparer<T>.Default.Equals(mItems![i], other.mItems![i]))
                    return false;
            }
            return true;
        }

        public override bool Equals(object? obj) => obj is EquatableArray<T> other && Equals(other);

        public override int GetHashCode()
        {
            var hash = 17;
            foreach (var item in this)
                hash = hash * 31 + (item?.GetHashCode() ?? 0);
            return hash;
        }

        public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)(mItems ?? Array.Empty<T>())).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    internal enum ETypeKind
    {
        Void,
        Boolean,
        Char,
        SByte,
        Byte,
        Int16,
        UInt16,
        Int32,
        UInt32,
        Int64,
        UInt64,
        Single,
        Double,
        String,
        HostObject,
        Enum,
        BlittableStruct,
    }

    /// <param name="MetadataName">签名里的类型名，如 System.Int32、Game.Actor</param>
    /// <param name="CSharpName">生成代码里的类型名，如 int、global::Game.Actor</param>
    /// <param name="UnderlyingCSharpName">enum 底层类型的 CSharpName，如 byte、int；非 enum 时为 null</param>
    /// <param name="SizeInBytes">blittable struct 的大小；非 struct 时为 0</param>
    internal sealed record TypeModel(ETypeKind Kind, string MetadataName, string CSharpName, string? UnderlyingCSharpName = null, int SizeInBytes = 0);

    internal sealed record ParameterModel(string Name, TypeModel Type);

    /// <param name="NeedsAccessor">private/protected 方法，注册表需通过 UnsafeAccessor 调用</param>
    internal sealed record MethodModel(
        string Name,
        bool IsStatic,
        bool NeedsAccessor,
        TypeModel ReturnType,
        EquatableArray<ParameterModel> Parameters,
        string Signature);

    /// <param name="FullName">VM 中的类全名（也是桩类全名），如 Game.Actor</param>
    /// <param name="CSharpName">global::Game.Actor</param>
    /// <param name="StubBaseName">桩基类：已标注的父类或 TtLeanClrHostObject</param>
    internal sealed record ClassModel(
        string FullName,
        string Namespace,
        string Name,
        string CSharpName,
        string StubBaseName,
        EquatableArray<MethodModel> Methods);

    internal sealed record LocationInfo(string FilePath, TextSpan Span, LinePositionSpan LineSpan)
    {
        public static LocationInfo? From(Location? location)
        {
            if (location == null || location.SourceTree == null)
                return null;
            return new LocationInfo(location.SourceTree.FilePath, location.SourceSpan, location.GetLineSpan().Span);
        }

        public Location ToLocation() => Location.Create(FilePath, Span, LineSpan);
    }

    internal sealed record DiagnosticInfo(string Id, LocationInfo? Location, EquatableArray<string> Arguments);

    /// <summary>
    /// 单个 [TtLeanClr] 类的提取结果；Class 为 null 表示整类被跳过。
    /// </summary>
    internal sealed record ClassResult(ClassModel? Class, EquatableArray<DiagnosticInfo> Diagnostics);
}
