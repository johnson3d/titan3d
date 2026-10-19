using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace EngineNS.LeanCLR
{
    /// <summary>
    /// 为 LeanCLR raw buffer 调用提供基于反射的非托管值类型装包。
    /// 引用类型参数只接受 TtLeanClrObject（写入 VM 对象指针）与 string（按值拷贝成 VM 字符串，null 透传）；
    /// string 返回值走 InvokeString / InvokeInstanceString。数组以及 ref/out 参数不在支持范围内。
    /// </summary>
    public static unsafe class TtLeanClrInvokeHelper
    {
        private const int StackObjectByteSize = 8;
        private const string StringTypeName = "System.String";
        // VM 字符串布局偏移，首次读取返回值时向 native 查询；不放静态初始化，避免无 LeanCLR 平台触发 P/Invoke
        private static int sStringLengthOffset = -1;
        private static int sStringCharsOffset = -1;
        private static readonly MethodInfo sWriteValueMethod = typeof(TtLeanClrInvokeHelper).GetMethod(
            nameof(WriteValue), BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly ConcurrentDictionary<Type, bool> sUnmanagedTypes = new();

        public static TResult Invoke<TResult>(TtLeanClrRuntime runtime, TtLeanClrMethod method, params object[] arguments)
            where TResult : unmanaged
        {
            ValidateRuntimeAndMethod(runtime, method);
            ValidateReturnType<TResult>(method);

            var argBufferByteSize = checked((int)method.GetArgBufferByteSize());
            var retBufferByteSize = checked((int)method.GetRetBufferByteSize());
            if (retBufferByteSize < sizeof(TResult))
                throw new InvalidOperationException($"LeanCLR return buffer is too small for {typeof(TResult).FullName}.");

            var argBufferPointer = stackalloc byte[argBufferByteSize];
            var retBufferPointer = stackalloc byte[retBufferByteSize];
            {
                PackArguments(runtime, method, arguments, argBufferPointer, argBufferByteSize);
                if (!runtime.InvokeWithBuffer(method, argBufferPointer, (uint)argBufferByteSize, retBufferPointer, (uint)retBufferByteSize))
                    throw new InvalidOperationException($"LeanCLR failed to invoke {method.GetSignature().Text}.");
                return ReadValue<TResult>(retBufferPointer);
            }
        }

        public static string InvokeString(TtLeanClrRuntime runtime, TtLeanClrMethod method, params object[] arguments)
        {
            ValidateRuntimeAndMethod(runtime, method);
            ValidateStringReturnType(method);

            var argBufferByteSize = checked((int)method.GetArgBufferByteSize());
            var retBufferByteSize = checked((int)method.GetRetBufferByteSize());
            if (retBufferByteSize < sizeof(void*))
                throw new InvalidOperationException("LeanCLR return buffer is too small for System.String.");

            var argBufferPointer = stackalloc byte[argBufferByteSize];
            var retBufferPointer = stackalloc byte[retBufferByteSize];
            {
                PackArguments(runtime, method, arguments, argBufferPointer, argBufferByteSize);
                if (!runtime.InvokeWithBuffer(method, argBufferPointer, (uint)argBufferByteSize, retBufferPointer, (uint)retBufferByteSize))
                    throw new InvalidOperationException($"LeanCLR failed to invoke {method.GetSignature().Text}.");
                return ReadVmString(*(void**)retBufferPointer);
            }
        }

        public static void Invoke(TtLeanClrRuntime runtime, TtLeanClrMethod method, params object[] arguments)
        {
            ValidateRuntimeAndMethod(runtime, method);
            if (!string.Equals(method.GetReturnType().Text, "System.Void", StringComparison.Ordinal))
                throw new InvalidOperationException($"LeanCLR method {method.GetSignature().Text} does not return System.Void.");

            var argBufferByteSize = checked((int)method.GetArgBufferByteSize());
            var retBufferByteSize = checked((int)method.GetRetBufferByteSize());
            var argBuffer = new byte[argBufferByteSize];
            var retBuffer = new byte[retBufferByteSize];
            fixed (byte* argBufferPointer = argBuffer)
            fixed (byte* retBufferPointer = retBuffer)
            {
                PackArguments(runtime, method, arguments, argBufferPointer, argBufferByteSize);
                if (!runtime.InvokeWithBuffer(method, argBufferPointer, (uint)argBufferByteSize, retBufferPointer, (uint)retBufferByteSize))
                    throw new InvalidOperationException($"LeanCLR failed to invoke {method.GetSignature().Text}.");
            }
        }

        public static TResult InvokeInstance<TResult>(TtLeanClrRuntime runtime, TtLeanClrObject instance, TtLeanClrMethod method, params object[] arguments)
            where TResult : unmanaged
        {
            ValidateRuntimeAndMethod(runtime, method);
            ValidateReturnType<TResult>(method);
            if (!instance.IsValidPointer)
                throw new ArgumentException("A valid LeanCLR object is required.", nameof(instance));

            var totalArgBufferByteSize = checked((int)method.GetArgBufferByteSize());
            var retBufferByteSize = checked((int)method.GetRetBufferByteSize());
            if (retBufferByteSize < sizeof(TResult))
                throw new InvalidOperationException($"LeanCLR return buffer is too small for {typeof(TResult).FullName}.");

            // For instance methods the native side prepends 'this'; user buffer excludes it.
            var userArgBufferByteSize = totalArgBufferByteSize - StackObjectByteSize;
            if (userArgBufferByteSize < 0)
                throw new InvalidOperationException("LeanCLR arg buffer is invalid for an instance method.");

            var argBufferPointer = userArgBufferByteSize > 0 ? stackalloc byte[userArgBufferByteSize] : Span<byte>.Empty;
            var retBufferPointer = stackalloc byte[retBufferByteSize];
            fixed (byte* argPtr = argBufferPointer)
            {
                PackArguments(runtime, method, arguments, argPtr, userArgBufferByteSize);
                if (!runtime.InvokeInstanceWithBuffer(instance, method, argPtr, (uint)userArgBufferByteSize, retBufferPointer, (uint)retBufferByteSize))
                    throw new InvalidOperationException($"LeanCLR failed to invoke instance method {method.GetSignature().Text}.");
                return ReadValue<TResult>(retBufferPointer);
            }
        }

        public static string InvokeInstanceString(TtLeanClrRuntime runtime, TtLeanClrObject instance, TtLeanClrMethod method, params object[] arguments)
        {
            ValidateRuntimeAndMethod(runtime, method);
            ValidateStringReturnType(method);
            if (!instance.IsValidPointer)
                throw new ArgumentException("A valid LeanCLR object is required.", nameof(instance));

            var totalArgBufferByteSize = checked((int)method.GetArgBufferByteSize());
            var retBufferByteSize = checked((int)method.GetRetBufferByteSize());
            if (retBufferByteSize < sizeof(void*))
                throw new InvalidOperationException("LeanCLR return buffer is too small for System.String.");

            var userArgBufferByteSize = totalArgBufferByteSize - StackObjectByteSize;
            if (userArgBufferByteSize < 0)
                throw new InvalidOperationException("LeanCLR arg buffer is invalid for an instance method.");

            var argBufferPointer = userArgBufferByteSize > 0 ? stackalloc byte[userArgBufferByteSize] : Span<byte>.Empty;
            var retBufferPointer = stackalloc byte[retBufferByteSize];
            fixed (byte* argPtr = argBufferPointer)
            {
                PackArguments(runtime, method, arguments, argPtr, userArgBufferByteSize);
                if (!runtime.InvokeInstanceWithBuffer(instance, method, argPtr, (uint)userArgBufferByteSize, retBufferPointer, (uint)retBufferByteSize))
                    throw new InvalidOperationException($"LeanCLR failed to invoke instance method {method.GetSignature().Text}.");
                return ReadVmString(*(void**)retBufferPointer);
            }
        }

        public static void InvokeInstance(TtLeanClrRuntime runtime, TtLeanClrObject instance, TtLeanClrMethod method, params object[] arguments)
        {
            ValidateRuntimeAndMethod(runtime, method);
            if (!instance.IsValidPointer)
                throw new ArgumentException("A valid LeanCLR object is required.", nameof(instance));
            if (!string.Equals(method.GetReturnType().Text, "System.Void", StringComparison.Ordinal))
                throw new InvalidOperationException($"LeanCLR method {method.GetSignature().Text} does not return System.Void.");

            var totalArgBufferByteSize = checked((int)method.GetArgBufferByteSize());
            var retBufferByteSize = checked((int)method.GetRetBufferByteSize());
            var userArgBufferByteSize = totalArgBufferByteSize - StackObjectByteSize;
            if (userArgBufferByteSize < 0)
                throw new InvalidOperationException("LeanCLR arg buffer is invalid for an instance method.");

            var argBuffer = new byte[userArgBufferByteSize];
            var retBuffer = new byte[retBufferByteSize];
            fixed (byte* argBufferPointer = argBuffer)
            fixed (byte* retBufferPointer = retBuffer)
            {
                PackArguments(runtime, method, arguments, argBufferPointer, userArgBufferByteSize);
                if (!runtime.InvokeInstanceWithBuffer(instance, method, argBufferPointer, (uint)userArgBufferByteSize, retBufferPointer, (uint)retBufferByteSize))
                    throw new InvalidOperationException($"LeanCLR failed to invoke instance method {method.GetSignature().Text}.");
            }
        }

        private static void ValidateRuntimeAndMethod(TtLeanClrRuntime runtime, TtLeanClrMethod method)
        {
            if (!runtime.IsValidPointer || !runtime.IsInitialized())
                throw new InvalidOperationException("LeanCLR runtime is not initialized.");
            if (!method.IsValidPointer)
                throw new ArgumentException("A valid LeanCLR method is required.", nameof(method));
        }

        private static void ValidateReturnType<TResult>(TtLeanClrMethod method)
            where TResult : unmanaged
        {
            var actualType = typeof(TResult);
            var expectedTypeName = method.GetReturnType().Text;
            if (!IsMatchingTypeName(actualType, expectedTypeName))
                throw new InvalidOperationException(
                    $"LeanCLR method {method.GetSignature().Text} returns {expectedTypeName}, not {actualType.FullName}.");
        }

        private static void ValidateStringReturnType(TtLeanClrMethod method)
        {
            var expectedTypeName = method.GetReturnType().Text;
            if (!string.Equals(expectedTypeName, StringTypeName, StringComparison.Ordinal))
                throw new InvalidOperationException(
                    $"LeanCLR method {method.GetSignature().Text} returns {expectedTypeName}, not {StringTypeName}.");
        }

        private static void PackArguments(TtLeanClrRuntime runtime, TtLeanClrMethod method, object[] arguments, byte* buffer, int bufferByteSize)
        {
            arguments ??= Array.Empty<object>();
            var expectedCount = checked((int)method.GetArgumentCount());
            if (arguments.Length != expectedCount)
                throw new ArgumentException(
                    $"LeanCLR method {method.GetSignature().Text} expects {expectedCount} arguments, but received {arguments.Length}.",
                    nameof(arguments));

            var offset = 0;
            for (var index = 0; index < arguments.Length; ++index)
            {
                var expectedTypeName = method.GetArgumentType((uint)index).Text;
                if (string.Equals(expectedTypeName, StringTypeName, StringComparison.Ordinal))
                {
                    if (arguments[index] != null && arguments[index] is not string)
                        throw new ArgumentException(
                            $"LeanCLR argument {index} expects {StringTypeName}, but received {arguments[index].GetType().FullName}.", nameof(arguments));
                    if (offset + StackObjectByteSize > bufferByteSize)
                        throw new ArgumentException("LeanCLR arguments exceed the method argument buffer.", nameof(arguments));
                    // 新建的 VM 字符串未入根；LeanCLR 分配不触发 GC，调用返回前指针有效
                    *(void**)(buffer + offset) = NewVmString(runtime, (string)arguments[index]);
                    offset += StackObjectByteSize;
                    continue;
                }

                var value = arguments[index] ?? throw new ArgumentNullException(
                    nameof(arguments), $"LeanCLR argument {index} cannot be null.");
                if (value is TtLeanClrObject vmObject)
                {
                    var target = vmObject.IsValidPointer ? vmObject.GetTarget() : null;
                    if (target == null)
                        throw new ArgumentException($"LeanCLR argument {index} is not a live LeanCLR object.", nameof(arguments));
                    if (offset + StackObjectByteSize > bufferByteSize)
                        throw new ArgumentException("LeanCLR arguments exceed the method argument buffer.", nameof(arguments));
                    *(void**)(buffer + offset) = target;
                    offset += StackObjectByteSize;
                    continue;
                }
                var valueType = value.GetType();
                if (!sUnmanagedTypes.GetOrAdd(valueType, static type => IsUnmanaged(type)))
                    throw new ArgumentException(
                        $"LeanCLR argument {index} type {valueType.FullName} is not an unmanaged value type.", nameof(arguments));

                if (!IsMatchingTypeName(valueType, expectedTypeName))
                    throw new ArgumentException(
                        $"LeanCLR argument {index} expects {expectedTypeName}, but received {valueType.FullName}.", nameof(arguments));

                var writeMethod = sWriteValueMethod.MakeGenericMethod(valueType);
                var valueByteSize = (int)writeMethod.Invoke(null, new object[] { (IntPtr)(buffer + offset), value });
                offset += AlignToStackObject(valueByteSize);
                if (offset > bufferByteSize)
                    throw new ArgumentException("LeanCLR arguments exceed the method argument buffer.", nameof(arguments));
            }

            if (offset != bufferByteSize)
                throw new ArgumentException(
                    $"LeanCLR arguments occupy {offset} bytes, but the method requires {bufferByteSize} bytes.", nameof(arguments));
        }

        private static void* NewVmString(TtLeanClrRuntime runtime, string value)
        {
            if (value == null)
                return null;
            void* result;
            fixed (char* chars = value)
                result = runtime.NewString(chars, value.Length);
            if (result == null)
                throw new InvalidOperationException("LeanCLR failed to create System.String.");
            return result;
        }

        private static string ReadVmString(void* vmString)
        {
            if (vmString == null)
                return null;
            if (sStringCharsOffset < 0)
            {
                sStringLengthOffset = TtLeanClrRuntime.GetStringLengthOffset();
                sStringCharsOffset = TtLeanClrRuntime.GetStringCharsOffset();
            }
            var p = (byte*)vmString;
            return new string((char*)(p + sStringCharsOffset), 0, *(int*)(p + sStringLengthOffset));
        }

        private static int WriteValue<T>(IntPtr destination, object value)
            where T : unmanaged
        {
            var typedValue = (T)value;
            Buffer.MemoryCopy(&typedValue, destination.ToPointer(), sizeof(T), sizeof(T));
            return sizeof(T);
        }

        private static T ReadValue<T>(byte* source)
            where T : unmanaged
        {
            T result;
            Buffer.MemoryCopy(source, &result, sizeof(T), sizeof(T));
            return result;
        }

        private static int AlignToStackObject(int byteSize)
        {
            return checked((byteSize + StackObjectByteSize - 1) / StackObjectByteSize * StackObjectByteSize);
        }

        private static bool IsMatchingTypeName(Type type, string leanClrTypeName)
        {
            var typeName = type.FullName;
            if (typeName == null || leanClrTypeName == null)
                return false;
            return string.Equals(typeName.Replace('+', '.'), leanClrTypeName.Replace('/', '.'), StringComparison.Ordinal);
        }

        private static bool IsUnmanaged(Type type)
        {
            if (type.IsPointer || type.IsPrimitive || type.IsEnum)
                return true;
            if (!type.IsValueType)
                return false;

            foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (!IsUnmanaged(field.FieldType))
                    return false;
            }
            return true;
        }
    }
}
