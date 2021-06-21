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
            ImGuiAPI.Checkbox($"##{info.Name}", &v2);
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
            var v = ((vBOOL)info.Value);
            var saved = v;
            ImGuiAPI.Checkbox($"##{info.Name}", (bool*)&v);
            if (v.Value != saved.Value)
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