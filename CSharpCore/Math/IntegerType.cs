using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Byte4
    {
        public byte X;
        public byte Y;
        public byte Z;
        public byte W;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Int32_2
    {
        public int x;
        public int y;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Int32_3
    {
        public int x;
        public int y;
        public int z;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Int32_4
    {
        public int x;
        public int y;
        public int z;
        public int w;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct UInt32_2
    {
        public UInt32 x;
        public UInt32 y;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct UInt32_3
    {
        public UInt32 x;
        public UInt32 y;
        public UInt32 z;
    }

    [StructLayout(LayoutKind.Explicit, Pack = 4)]
    public struct UInt32_4
    {
        public void SetValue(uint _x, uint _y, uint _z, uint _w)
        {
            x = _x;
            y = _y;
            z = _z;
            w = _w;
        }
        public UInt32 this[uint index]
        {
            get
            {
                System.Diagnostics.Debug.Assert(index < 4);
                unsafe
                {
                    fixed (uint* p = &x)
                    {
                        return p[index];
                    }
                }
            }

            set
            {
                System.Diagnostics.Debug.Assert(index < 4);
                unsafe
                {
                    fixed (uint* p = &x)
                    {
                        p[index] = value;
                    }
                }
            }
        }
        public byte this[uint row,uint col]
        {
            get
            {
                System.Diagnostics.Debug.Assert(row < 4);
                System.Diagnostics.Debug.Assert(col < 4);
                unsafe
                {
                    fixed (byte* p = &x0)
                    {
                        return p[row*4+col];
                    }
                }
            }

            set
            {
                System.Diagnostics.Debug.Assert(row < 4);
                System.Diagnostics.Debug.Assert(col < 4);
                unsafe
                {
                    fixed (uint* p = &x)
                    {
                        p[row * 4 + col] = value;
                    }
                }
            }
        }
        public static UInt32_4 Lerp(ref UInt32_4 l, ref UInt32_4 r, float v)
        {
            float fa = 1 - v;
            var result = new UInt32_4();
            result.x0 = (byte)((float)l.x0 * fa + (float)r.x0 * v);
            result.x1 = (byte)((float)l.x1 * fa + (float)r.x1 * v);
            result.x2 = (byte)((float)l.x2 * fa + (float)r.x2 * v);
            result.x3 = (byte)((float)l.x3 * fa + (float)r.x3 * v);

            result.y0 = (byte)((float)l.y0 * fa + (float)r.y0 * v);
            result.y1 = (byte)((float)l.y1 * fa + (float)r.y1 * v);
            result.y2 = (byte)((float)l.y2 * fa + (float)r.y2 * v);
            result.y3 = (byte)((float)l.y3 * fa + (float)r.y3 * v);

            result.z0 = (byte)((float)l.z0 * fa + (float)r.z0 * v);
            result.z1 = (byte)((float)l.z1 * fa + (float)r.z1 * v);
            result.z2 = (byte)((float)l.z2 * fa + (float)r.z2 * v);
            result.z3 = (byte)((float)l.z3 * fa + (float)r.z3 * v);

            result.w0 = (byte)((float)l.w0 * fa + (float)r.w0 * v);
            result.w1 = (byte)((float)l.w1 * fa + (float)r.w1 * v);
            result.w2 = (byte)((float)l.w2 * fa + (float)r.w2 * v);
            result.w3 = (byte)((float)l.w3 * fa + (float)r.w3 * v);
            return result;
        }
        [FieldOffset(0)]
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Editor.Editor_ShowInPropertyGrid]
        public UInt32 x;
        [FieldOffset(4)]
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Editor.Editor_ShowInPropertyGrid]
        public UInt32 y;
        [FieldOffset(8)]
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Editor.Editor_ShowInPropertyGrid]
        public UInt32 z;
        [FieldOffset(12)]
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Editor.Editor_ShowInPropertyGrid]
        public UInt32 w;

        [FieldOffset(0)]
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Editor.Editor_ShowInPropertyGrid]
        public byte x0;
        [FieldOffset(1)]
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Editor.Editor_ShowInPropertyGrid]
        public byte x1;
        [FieldOffset(2)]
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Editor.Editor_ShowInPropertyGrid]
        public byte x2;
        [FieldOffset(3)]
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Editor.Editor_ShowInPropertyGrid]
        public byte x3;

        [FieldOffset(4)]
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Editor.Editor_ShowInPropertyGrid]
        public byte y0;
        [FieldOffset(5)]
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Editor.Editor_ShowInPropertyGrid]
        public byte y1;
        [FieldOffset(6)]
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Editor.Editor_ShowInPropertyGrid]
        public byte y2;
        [FieldOffset(7)]
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Editor.Editor_ShowInPropertyGrid]
        public byte y3;

        [FieldOffset(8)]
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Editor.Editor_ShowInPropertyGrid]
        public byte z0;
        [FieldOffset(9)]
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Editor.Editor_ShowInPropertyGrid]
        public byte z1;
        [FieldOffset(10)]
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Editor.Editor_ShowInPropertyGrid]
        public byte z2;
        [FieldOffset(11)]
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Editor.Editor_ShowInPropertyGrid]
        public byte z3;

        [FieldOffset(12)]
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Editor.Editor_ShowInPropertyGrid]
        public byte w0;
        [FieldOffset(13)]
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Editor.Editor_ShowInPropertyGrid]
        public byte w1;
        [FieldOffset(14)]
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Editor.Editor_ShowInPropertyGrid]
        public byte w2;
        [FieldOffset(15)]
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Editor.Editor_ShowInPropertyGrid]
        public byte w3;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct UInt8_2
    {
        public byte x;
        public byte y;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct UInt8_3
    {
        public byte x;
        public byte y;
        public byte z;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct UInt8_4
    {
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Editor.Editor_ShowInPropertyGrid]
        public byte x;
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Editor.Editor_ShowInPropertyGrid]
        public byte y;
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Editor.Editor_ShowInPropertyGrid]
        public byte z;
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Editor.Editor_ShowInPropertyGrid]
        public byte w;
    }
}
