using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.UISystem
{
    public class Program
    {
        public static string ControlDragType
        {
            get { return "UIControl"; }
        }
        public static string HierarchyControlDragType
        {
            get { return "HierarchyControl"; }
        }
    }

    internal static class FloatUtil
    {
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit)]
        private struct NanUnion
        {
            [System.Runtime.InteropServices.FieldOffset(0)]
            internal float FloatValue;
            [System.Runtime.InteropServices.FieldOffset(0)]
            internal UInt64 UintValue;
        }
        // 此IsNaN比float.IsNaN()性能好，在性能敏感代码处使用
        public static bool IsNaN(float value)
        {
            NanUnion t = new NanUnion();
            t.FloatValue = value;

            UInt64 exp = t.UintValue & 0xfff0000000000000;
            UInt64 man = t.UintValue & 0x000fffffffffffff;

            return (exp == 0x7ff0000000000000 || exp == 0xfff0000000000000) && (man != 0);
        }
    }
}
