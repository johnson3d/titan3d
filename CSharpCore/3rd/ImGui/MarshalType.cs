using System;
using System.Collections.Generic;
using System.Text;

using EngineNS;

public struct Wchar16
{
    public UInt16 Value;
}
public struct Wchar32
{
    public UInt32 Value;
}

[CppBool.PropEditor()]
public struct CppBool
{
    public class PropEditor : CSharpCode.Controls.PropertyGrid.PGCustomValueEditorAttribute
    {
        public override unsafe void OnDraw(System.Reflection.PropertyInfo prop, object target, object value, CSharpCode.Controls.PropertyGrid.PropertyGrid pg, List<KeyValuePair<object, System.Reflection.PropertyInfo>> callstack)
        {
            ImGuiAPI.SetNextItemWidth(-1);
            var v = (CppBool)value;
            var saved = v;
            ImGuiAPI.Checkbox($"##{prop.Name}", ref v);
            if (v != saved)
            {
                foreach (var j in pg.TargetObjects)
                {
                    CSharpCode.Controls.PropertyGrid.PropertyGrid.SetValue(j, callstack, prop, target, v);
                }
            }
            ImGuiAPI.NextColumn();
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