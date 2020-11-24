using System;
using System.Collections.Generic;
using System.Text;

public struct Wchar16
{
    public UInt16 Value;
}
public struct Wchar32
{
    public UInt32 Value;
}

public struct CppBool
{
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
    public override bool Equals(object obj)
    {
        return ((CppBool)obj).Value == Value;
    }
    public override int GetHashCode()
    {
        return Value;
    }
}