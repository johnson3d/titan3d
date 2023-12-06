using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using EngineNS;
using NPOI.SS.Formula.PTG;
using System.Linq;
using Org.BouncyCastle.Crypto.Agreement;

public struct Wchar16
{
    public Wchar16(UInt16 v)
    {
        Value = v;
    }
    public UInt16 Value;
}
public struct Wchar32
{
    public Wchar32(UInt32 v)
    {
        Value = v;
    }
    public UInt32 Value;
}

public struct wchar_t
{
#if PWindow
    public ushort Value;
#else
    public uint Value;
#endif
}


[CppBool.PropEditor()]
public struct CppBool
{
    public class PropEditor : EngineNS.EGui.Controls.PropertyGrid.PGCustomValueEditorAttribute
    {
        public override unsafe bool OnDraw(in EditorInfo info, out object newValue)
        {
            newValue = info.Value;
            bool valueChanged = false;
            ImGuiAPI.SetNextItemWidth(-1);
            var v = (CppBool)info.Value;
            bool v2 = v;
            var saved = v;
            EngineNS.EGui.UIProxy.CheckBox.DrawCheckBox($"##{info.Name}", ref v2);
            if (v != saved)
            {
                newValue = v;
                valueChanged = true;
            }
            return valueChanged;
        }
    }
    public sbyte Value;
    public CppBool(bool v)
    {
        Value = v ? (sbyte)1 : (sbyte)0;
    }
    public override string ToString()
    {
        return Value != 0 ? "true" : "false";
    }
    public static CppBool FromBoolean(bool v)
    {
        CppBool result;
        result.Value = v ? (sbyte)1 : (sbyte)0;
        return result;
    }
    public static implicit operator bool(CppBool v)
    {
        return v.Value == 0 ? false : true;
    }
    public static bool operator ==(bool lh, CppBool v)
    {
        return (v.Value == 0 ? false : true) == lh;
    }
    public static bool operator !=(bool lh, CppBool v)
    {
        return (v.Value == 0 ? false : true) != lh;
    }
    public static bool operator ==(CppBool lh, bool rh)
    {
        return (lh.Value == 0 ? false : true) == rh;
    }
    public static bool operator !=(CppBool lh, bool rh)
    {
        return (lh.Value == 0 ? false : true) != rh;
    }
    public static bool operator ==(CppBool lh, CppBool rh)
    {
        return lh.Value == rh.Value;
    }
    public static bool operator !=(CppBool lh, CppBool rh)
    {
        return lh.Value != rh.Value;
    }
    public override bool Equals(object obj)
    {
        return ((CppBool)obj).Value == Value;
    }
    public bool Equals(CppBool obj)
    {
        return obj.Value == Value;
    }
    public override int GetHashCode()
    {
        return Value;
    }
}

[vBOOL.PropEditor()]
public struct vBOOL
{
    public class PropEditor : EngineNS.EGui.Controls.PropertyGrid.PGCustomValueEditorAttribute
    {
        public override unsafe bool OnDraw(in EditorInfo info, out object newValue)
        {
            newValue = info.Value;
            ImGuiAPI.SetNextItemWidth(-1);
            var v = (bool)((vBOOL)info.Value);
            var saved = (bool)v;
            EngineNS.EGui.UIProxy.CheckBox.DrawCheckBox($"##{info.Name}", ref v);
            if (v != saved)
            {
                newValue = vBOOL.FromBoolean(v);
                return true;
            }
            return false;
        }
    }
    public int Value;
    public static vBOOL FromBoolean(bool v)
    {
        vBOOL result;
        result.Value = v ? 1 : 0;
        return result;
    }
    public static implicit operator bool(vBOOL v)
    {
        return v.Value == 0 ? false : true;
    }
    public static bool operator ==(bool lh, vBOOL v)
    {
        return (v.Value == 0 ? false : true) == lh;
    }
    public static bool operator !=(bool lh, vBOOL v)
    {
        return (v.Value == 0 ? false : true) != lh;
    }
    public static bool operator ==(vBOOL lh, bool rh)
    {
        return (lh.Value == 0 ? false : true) == rh;
    }
    public static bool operator !=(vBOOL lh, bool rh)
    {
        return (lh.Value == 0 ? false : true) != rh;
    }
    public override bool Equals(object obj)
    {
        return ((vBOOL)obj).Value == Value;
    }
    public override int GetHashCode()
    {
        return Value;
    }
}

//接口数据为utf-8编码所设置
public class UTF8Marshaler : System.Runtime.InteropServices.ICustomMarshaler
{
    public void CleanUpManagedData(object managedObj)
    {
    }

    public unsafe void CleanUpNativeData(IntPtr pNativeData)
    {
        CoreSDK.Free(pNativeData.ToPointer());
    }

    public int GetNativeDataSize()
    {
        return -1;
    }

    public unsafe IntPtr MarshalManagedToNative(object managedObj)
    {
        if (object.ReferenceEquals(managedObj, null))
            return IntPtr.Zero;
        
        if (!(managedObj is string))
            throw new InvalidOperationException();

        var str = managedObj as string;
        var len = Encoding.UTF8.GetByteCount(str);
        IntPtr ptr = (IntPtr)CoreSDK.Alloc((uint)len + 1, "MarshalType.cs", 190);
        var utf8Buffer = new Span<byte>(ptr.ToPointer(), len);
        Encoding.UTF8.GetBytes(str, utf8Buffer);
        ((byte*)ptr.ToPointer())[len] = 0;
        return ptr;
    }

    public unsafe object MarshalNativeToManaged(IntPtr pNativeData)
    {
        if (pNativeData == IntPtr.Zero)
            return null;

        var len = (int)CoreSDK.SDK_StrLen(pNativeData.ToPointer());
        var utf8Buffer = new ReadOnlySpan<byte>(pNativeData.ToPointer(), len);
        return Encoding.UTF8.GetString(utf8Buffer);
    }

    private static UTF8Marshaler instance = new UTF8Marshaler();
    public static ICustomMarshaler GetInstance(string cookie)
    {
        return instance;
    }
}

//namespace System.Collections.Generic
//{

//}

public static class ListExtra
{
    public static List<T> CreateList<T>(int sz, T c = default(T))
    {
        var result = new List<T>();
        result.Resize(sz, c);
        return result;
    }
    public static List<T> CreateList<T>(T[] src)
    {
        var result = new List<T>();
        result.Resize(src.Length);
        for (int i = 0; i < src.Length; i++)
        {
            result[i] = src[i];
        }
        return result;
    }
    public static void Resize<T>(this List<T> list, int sz, T c = default(T))
    {
        int cur = list.Count;
        if (sz < cur)
        {
            list.RemoveRange(sz, cur - sz);
        }
        else if (sz > cur)
        {
            if (sz > list.Capacity)//this bit is purely an optimisation, to avoid multiple automatic capacity changes.
                list.Capacity = sz;
            list.AddRange(Enumerable.Repeat(c, sz - cur));
        }
    }
}