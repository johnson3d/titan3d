using System;
using System.Collections;

namespace EngineNS.Rtti
{
    public partial class TtNativeCoreProvider
    {
        unsafe static readonly CoreSDK.FDelegate_FNativeCoreListGetCount NativeCoreListGetCount = NativeCoreListGetCountImpl;
        unsafe static readonly CoreSDK.FDelegate_FNativeCoreListAdd NativeCoreListAdd = NativeCoreListAddImpl;
        unsafe static readonly CoreSDK.FDelegate_FFreeManagedObjectGCHandle NativeCoreListClear = NativeCoreListClearImpl;
        unsafe static readonly CoreSDK.FDelegate_FNativeCoreListRemoveAt NativeCoreListRemoveAt = NativeCoreListRemoveAtImpl;
        unsafe static readonly CoreSDK.FDelegate_FNativeCoreListGetValue NativeCoreListGetValue = NativeCoreListGetValueImpl;
        unsafe static readonly CoreSDK.FDelegate_FNativeCoreArrayPinElementAddress NativeCoreArrayPinElementAddress = NativeCoreArrayPinElementAddressImpl;
        unsafe static readonly CoreSDK.FDelegate_FGetManagedObjectFromGCHandle NativeCorePinGCHandle = NativeCorePinGCHandleImpl;
        unsafe static readonly CoreSDK.FDelegate_FFreeManagedObjectGCHandle NativeCoreFreeGCHandle = NativeCoreFreeGCHandleImpl;
        unsafe static readonly CoreSDK.FDelegate_FNativeCoreGetPropertyValue NativeCoreGetPropertyValue = NativeCoreGetPropertyValueImpl;
        unsafe static readonly CoreSDK.FDelegate_FNativeCoreGetPropertyValue NativeCoreSetPropertyValue = NativeCoreSetPropertyValueImpl;

        static TtNativeCoreProvider()
        {
            InitializeNativeCoreBridge();
        }

        public static void InitializeNativeCoreBridge()
        {
            CoreSDK.SetNativeCoreListGetCountCallback(NativeCoreListGetCount);
            CoreSDK.SetNativeCoreListAddCallback(NativeCoreListAdd);
            CoreSDK.SetNativeCoreListClearCallback(NativeCoreListClear);
            CoreSDK.SetNativeCoreListRemoveAtCallback(NativeCoreListRemoveAt);
            CoreSDK.SetNativeCoreListGetValueCallback(NativeCoreListGetValue);
            CoreSDK.SetNativeCoreArrayPinElementAddressCallback(NativeCoreArrayPinElementAddress);
            CoreSDK.SetNativeCorePinGCHandleCallback(NativeCorePinGCHandle);
            CoreSDK.SetNativeCoreFreeGCHandleCallback(NativeCoreFreeGCHandle);
            CoreSDK.SetNativeCoreGetPropertyValueCallback(NativeCoreGetPropertyValue);
            CoreSDK.SetNativeCoreSetPropertyValueCallback(NativeCoreSetPropertyValue);
        }

        public static void FinalCleanupNativeCoreBridge()
        {
            CoreSDK.SetNativeCoreListGetCountCallback(null);
            CoreSDK.SetNativeCoreListAddCallback(null);
            CoreSDK.SetNativeCoreListClearCallback(null);
            CoreSDK.SetNativeCoreListRemoveAtCallback(null);
            CoreSDK.SetNativeCoreListGetValueCallback(null);
            CoreSDK.SetNativeCoreArrayPinElementAddressCallback(null);
            CoreSDK.SetNativeCorePinGCHandleCallback(null);
            CoreSDK.SetNativeCoreFreeGCHandleCallback(null);
            CoreSDK.SetNativeCoreGetPropertyValueCallback(null);
            CoreSDK.SetNativeCoreSetPropertyValueCallback(null);
        }

        public static T ObjectFromGCHandle<T>(IntPtr ptr) where T : class
        {
            if (ptr == IntPtr.Zero)
                return null;
            return System.Runtime.InteropServices.GCHandle.FromIntPtr(ptr).Target as T;
        }

        public unsafe static string MarshalPtrAnsi(void* ptr)
        {
            return System.Runtime.InteropServices.Marshal.PtrToStringAnsi((IntPtr)ptr);
        }

        public unsafe static string MarshalPtrAnsi(IntPtr ptr)
        {
            return System.Runtime.InteropServices.Marshal.PtrToStringAnsi(ptr);
        }

        public unsafe static string MarshalPtrUtf8(void* ptr)
        {
            return System.Runtime.InteropServices.Marshal.PtrToStringUTF8((IntPtr)ptr);
        }

        public unsafe static string MarshalPtrUtf8(IntPtr ptr)
        {
            return System.Runtime.InteropServices.Marshal.PtrToStringUTF8(ptr);
        }

        public static object BoxValue(ref Support.TtAnyValue v)
        {
            switch (v.ValueType)
            {
                case Support.TtAnyValue.EValueType.Unknown:
                    return null;
                case Support.TtAnyValue.EValueType.Struct:
                    {
                        var type = Rtti.TtTypeDesc.TypeOf(v.Struct.mTypeName.c_str());
                        if (type == null)
                            return null;
                        return System.Runtime.InteropServices.Marshal.PtrToStructure(v.Struct.mStructPointer, type.SystemType);
                    }
                case Support.TtAnyValue.EValueType.ManagedHandle:
                    return v.GCHandle.Target;
                case Support.TtAnyValue.EValueType.I8:
                    return v.I8Value;
                case Support.TtAnyValue.EValueType.I16:
                    return v.I16Value;
                case Support.TtAnyValue.EValueType.I32:
                    return v.I32Value;
                case Support.TtAnyValue.EValueType.I64:
                    return v.I64Value;
                case Support.TtAnyValue.EValueType.UI8:
                    return v.UI8Value;
                case Support.TtAnyValue.EValueType.UI16:
                    return v.UI16Value;
                case Support.TtAnyValue.EValueType.UI32:
                    return v.UI32Value;
                case Support.TtAnyValue.EValueType.UI64:
                    return v.UI64Value;
                case Support.TtAnyValue.EValueType.F32:
                    return v.F32Value;
                case Support.TtAnyValue.EValueType.F64:
                    return v.F64Value;
                case Support.TtAnyValue.EValueType.Ptr:
                    return v.Pointer;
                case Support.TtAnyValue.EValueType.V2:
                    return v.V2;
                case Support.TtAnyValue.EValueType.V3:
                    return v.V3;
                case Support.TtAnyValue.EValueType.V4:
                    return v.V4;
                case Support.TtAnyValue.EValueType.Bool:
                    return v.BoolValue;
                default:
                    return null;
            }
        }

        public static unsafe void UnboxObject(object obj, Support.TtAnyValue* v)
        {
            if (obj == null || v == null)
                return;

            var t = obj.GetType();
            if (t == typeof(sbyte))
            {
                v->SetI8((sbyte)obj);
            }
            else if (t == typeof(short))
            {
                v->SetI16((short)obj);
            }
            else if (t == typeof(int))
            {
                v->SetI32((int)obj);
            }
            else if (t == typeof(long))
            {
                v->SetI64((long)obj);
            }
            else if (t == typeof(byte))
            {
                v->SetUI8((byte)obj);
            }
            else if (t == typeof(ushort))
            {
                v->SetUI16((ushort)obj);
            }
            else if (t == typeof(uint))
            {
                v->SetUI32((uint)obj);
            }
            else if (t == typeof(ulong))
            {
                v->SetUI64((ulong)obj);
            }
            else if (t == typeof(float))
            {
                v->SetF32((float)obj);
            }
            else if (t == typeof(double))
            {
                v->SetF64((double)obj);
            }
            else if (t == typeof(bool))
            {
                v->SetBool((bool)obj);
            }
            else if (t == typeof(Vector2))
            {
                v->SetVector2((Vector2)obj);
            }
            else if (t == typeof(Vector3))
            {
                v->SetVector3((Vector3)obj);
            }
            else if (t == typeof(Quaternion))
            {
                v->SetQuaternion((Quaternion)obj);
            }
            else if (t == typeof(IntPtr))
            {
                v->SetPointer((IntPtr)obj);
            }
            else if (t.IsValueType)
            {
                v->SetStruct(obj);
            }
            else
            {
                v->SetManagedHandle(System.Runtime.InteropServices.GCHandle.Alloc(obj));
            }
        }

        unsafe static int NativeCoreListGetCountImpl(void* listHandle)
        {
            var lst = ObjectFromGCHandle<IList>((IntPtr)listHandle);
            return lst?.Count ?? 0;
        }

        unsafe static void NativeCoreListAddImpl(void* listHandle, Support.TtAnyValue* v)
        {
            var lst = ObjectFromGCHandle<IList>((IntPtr)listHandle);
            if (lst == null || v == null)
                return;
            lst.Add(BoxValue(ref *v));
        }

        unsafe static void NativeCoreListClearImpl(void* listHandle)
        {
            var lst = ObjectFromGCHandle<IList>((IntPtr)listHandle);
            lst?.Clear();
        }

        unsafe static void NativeCoreListRemoveAtImpl(void* listHandle, int index)
        {
            var lst = ObjectFromGCHandle<IList>((IntPtr)listHandle);
            if (lst == null || index < 0 || index >= lst.Count)
                return;
            lst.RemoveAt(index);
        }

        unsafe static void NativeCoreListGetValueImpl(void* listHandle, int index, Support.TtAnyValue* v)
        {
            var lst = ObjectFromGCHandle<IList>((IntPtr)listHandle);
            if (lst == null || v == null || index < 0 || index >= lst.Count)
                return;
            UnboxObject(lst[index], v);
        }

        unsafe static void* NativeCoreArrayPinElementAddressImpl(void* arrayHandle, int index)
        {
            var array = ObjectFromGCHandle<Array>((IntPtr)arrayHandle);
            if (array == null || index < 0 || index >= array.Length)
                return null;
            return System.Runtime.InteropServices.Marshal.UnsafeAddrOfPinnedArrayElement(array, index).ToPointer();
        }

        unsafe static void* NativeCorePinGCHandleImpl(void* objectHandle)
        {
            if (objectHandle == null)
                return null;
            var handle = System.Runtime.InteropServices.GCHandle.FromIntPtr((IntPtr)objectHandle);
            var pinHandle = System.Runtime.InteropServices.GCHandle.Alloc(handle.Target, System.Runtime.InteropServices.GCHandleType.Pinned);
            return System.Runtime.InteropServices.GCHandle.ToIntPtr(pinHandle).ToPointer();
        }

        unsafe static void NativeCoreFreeGCHandleImpl(void* pinnedHandle)
        {
            if (pinnedHandle == null)
                return;
            var handle = System.Runtime.InteropServices.GCHandle.FromIntPtr((IntPtr)pinnedHandle);
            handle.Free();
        }

        unsafe static void NativeCoreGetPropertyValueImpl(void* hostHandle, sbyte* propName, Support.TtAnyValue* outValue)
        {
            if (hostHandle == null || propName == null || outValue == null)
                return;
            var handle = System.Runtime.InteropServices.GCHandle.FromIntPtr((IntPtr)hostHandle);
            if (handle.Target == null)
                return;
            var name = MarshalPtrAnsi(propName);
            var prop = handle.Target.GetType().GetProperty(name);
            if (prop == null)
                return;
            var propValue = prop.GetValue(handle.Target);
            if (propValue == null)
                return;

            UnboxObject(propValue, outValue);
        }

        unsafe static void NativeCoreSetPropertyValueImpl(void* hostHandle, sbyte* propName, Support.TtAnyValue* v)
        {
            if (hostHandle == null || propName == null || v == null)
                return;
            var handle = System.Runtime.InteropServices.GCHandle.FromIntPtr((IntPtr)hostHandle);
            if (handle.Target == null)
                return;
            var name = MarshalPtrAnsi(propName);
            var prop = handle.Target.GetType().GetProperty(name);
            if (prop == null)
                return;
            var value = BoxValue(ref *v);
            prop.SetValue(handle.Target, value);
        }
    }
}
