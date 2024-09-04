using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Rtti
{
    [Editor.UCs2Cpp]
    public partial class UNativeCoreProvider
    {
        static UNativeCoreProvider()
        {
            InitCallbacks();
        }
        public static T ObjectFromGCHandle<T>(IntPtr ptr) where T : class
        {
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
        public static object BoxValue(ref Support.TtAnyValue v)
        {
            switch (v.ValueType)
            {
                case Support.TtAnyValue.EValueType.Unknown:
                    return null;
                case Support.TtAnyValue.EValueType.Struct:
                    {
                        var type = Rtti.UTypeDesc.TypeOf(v.Struct.mTypeName.c_str());
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
                default:
                    return null;
            }
        }
        public static unsafe void UnboxObject(object obj, Support.TtAnyValue* v)
        {
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
        [Editor.UCs2Cpp]
        public int List_GetCount(IList lst)
        {
            return lst.Count;
        }
        [Editor.UCs2Cpp]
        public unsafe void List_Add(IList lst, Support.TtAnyValue* v)
        {
            lst.Add(BoxValue(ref *v));
        }
        [Editor.UCs2Cpp]
        public void List_Clear(IList lst)
        {
            lst.Clear();
        }
        [Editor.UCs2Cpp]
        public void List_RemoveAt(IList lst, int index)
        {
            lst.RemoveAt(index);
        }
        [Editor.UCs2Cpp]
        public unsafe void List_GetValue(IList lst, int index, Support.TtAnyValue* v)
        {
            UnboxObject(lst[index], v);
        }
        [Editor.UCs2Cpp]
        public unsafe void* Array_PinElementAddress(Array array, int index)
        {
            return System.Runtime.InteropServices.Marshal.UnsafeAddrOfPinnedArrayElement(array, index).ToPointer();
        }
        [Editor.UCs2Cpp]
        public IntPtr PinGCHandle(IntPtr argHandle)
        {
            var handle = System.Runtime.InteropServices.GCHandle.FromIntPtr(argHandle);
            var pinHandle = System.Runtime.InteropServices.GCHandle.Alloc(handle.Target, System.Runtime.InteropServices.GCHandleType.Pinned);
            return System.Runtime.InteropServices.GCHandle.ToIntPtr(pinHandle);
        }
        [Editor.UCs2Cpp]
        public void FreeGCHandle(IntPtr argHandle)
        {
            var handle = System.Runtime.InteropServices.GCHandle.FromIntPtr(argHandle);
            handle.Free();
        }
        [Editor.UCs2Cpp]
        public unsafe void GetPropertyValue(IntPtr hostHandle, string propName, Support.TtAnyValue* outValue)
        {
            var handle = System.Runtime.InteropServices.GCHandle.FromIntPtr(hostHandle);
            if (handle.Target == null)
                return;
            var prop = handle.Target.GetType().GetProperty(propName);
            if (prop == null)
                return;
            var propValue = prop.GetValue(handle.Target);
            if (propValue == null)
                return;

            UnboxObject(propValue, outValue);
        }
        [Editor.UCs2Cpp]
        public unsafe void SetPropertyValue(IntPtr hostHandle, string propName, Support.TtAnyValue* v)
        {
            var handle = System.Runtime.InteropServices.GCHandle.FromIntPtr(hostHandle);
            if (handle.Target == null)
                return;
            var prop = handle.Target.GetType().GetProperty(propName);
            if (prop == null)
                return;
            var value = BoxValue(ref *v);
            prop.SetValue(handle.Target, value);
        }
    }
}
