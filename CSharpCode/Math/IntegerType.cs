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
    public struct Int32_2 : IEquatable<Int32_2>
    {
        public readonly static Int32_2 Zero = new Int32_2(0, 0);
        public readonly static Int32_2 One = new Int32_2(1, 1);
        public readonly static Int32_2 MinusOne = new Int32_2(-1, -1);
        public readonly static Int32_2 MaxValue = new Int32_2(int.MaxValue, int.MaxValue);
        public readonly static Int32_2 MinValue = new Int32_2(int.MinValue, int.MinValue);

        public int X;
        public int Y;
        public Int32_2(int InX, int InY)
        {
            X = InX;
            Y = InY;
        }
        public override bool Equals(object value)
        {
            if (value == null)
                return false;

            if (value.GetType() != GetType())
                return false;

            return Equals((Int32_2)(value));
        }
        public override int GetHashCode()
        {
            return X + Y;
        }
        public bool Equals(Int32_2 other)
        {
            if (X == other.X && Y == other.Y)
                return true;
            return false;
        }
        public static bool operator ==(in Int32_2 left, in Int32_2 right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(in Int32_2 left, in Int32_2 right)
        {
            return !left.Equals(right);
        }
        public static Int32_2 Maximize(in Int32_2 left, in Int32_2 right)
        {
            Int32_2 vector;
            vector.X = (left.X > right.X) ? left.X : right.X;
            vector.Y = (left.Y > right.Y) ? left.Y : right.Y;
            return vector;
        }
        public static Int32_2 Minimize(in Int32_2 left, in Int32_2 right)
        {
            Int32_2 vector;
            vector.X = (left.X < right.X) ? left.X : right.X;
            vector.Y = (left.Y < right.Y) ? left.Y : right.Y;
            return vector;
        }

        public static Int32_2 operator +(in Int32_2 left, in Int32_2 right)
        {
            Int32_2 result;
            result.X = left.X + right.X;
            result.Y = left.Y + right.Y;
            return result;
        }
        public static Int32_2 operator -(in Int32_2 left, in Int32_2 right)
        {
            Int32_2 result;
            result.X = left.X - right.X;
            result.Y = left.Y - right.Y;
            return result;
        }
        public static Int32_2 operator +(in Int32_2 value)
        {
            Int32_2 result;
            result.X = +value.X;
            result.Y = +value.Y;
            return result;
        }
        public static Int32_2 operator *(in Int32_2 value, int scale)
        {
            Int32_2 result;
            result.X = value.X * scale;
            result.Y = value.Y * scale;
            return result;
        }
        public static Int32_2 operator *(in Int32_2 left, in Int32_2 right)
        {
            Int32_2 result;
            result.X = left.X * right.X;
            result.Y = left.Y * right.Y;
            return result;
        }
        public static Int32_2 operator /(in Int32_2 left, in Int32_2 right)
        {
            Int32_2 result;
            result.X = left.X / right.X;
            result.Y = left.Y / right.Y;
            return result;
        }
        public static Int32_2 operator /(in Int32_2 value, int scale)
        {
            Int32_2 result;
            result.X = value.X / scale;
            result.Y = value.Y / scale;
            return result;
        }
    }
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Int32_3 : System.IEquatable<Int32_3>
    {
        public Int32_3(int _x, int _y, int _z)
        {
            X = _x;
            Y = _y;
            Z = _z;
        }
        public override bool Equals(object value)
        {
            if (value == null)
                return false;

            if (value.GetType() != GetType())
                return false;

            return Equals((UInt32_3)value);
        }
        public bool Equals(Int32_3 value)
        {
            return (X == value.X && Y == value.Y && Z == value.Z);
        }
        public override int GetHashCode()
        {
            return X.GetHashCode() + Y.GetHashCode() + Z.GetHashCode();
        }
        public int X;
        public int Y;
        public int Z;
        public readonly static Int32_3 Zero = new Int32_3(0, 0, 0);
        public readonly static Int32_3 One = new Int32_3(1, 1, 1);
        public readonly static Int32_3 MinusOne = new Int32_3(-1, -1, -1);
        public readonly static Int32_3 MaxValue = new Int32_3(int.MaxValue, int.MaxValue, int.MaxValue);
        public readonly static Int32_3 MinValue = new Int32_3(int.MinValue, int.MinValue, int.MinValue);

        public static Int32_3 Maximize(in Int32_3 left, in Int32_3 right)
        {
            Int32_3 vector;
            vector.X = (left.X > right.X) ? left.X : right.X;
            vector.Y = (left.Y > right.Y) ? left.Y : right.Y;
            vector.Z = (left.Z > right.Z) ? left.Z : right.Z;
            return vector;
        }
        public static Int32_3 Minimize(in Int32_3 left, in Int32_3 right)
        {
            Int32_3 vector;
            vector.X = (left.X < right.X) ? left.X : right.X;
            vector.Y = (left.Y < right.Y) ? left.Y : right.Y;
            vector.Z = (left.Z < right.Z) ? left.Z : right.Z;
            return vector;
        }

        public static Int32_3 operator +(in Int32_3 left, in Int32_3 right)
        {
            Int32_3 result;
            result.X = left.X + right.X;
            result.Y = left.Y + right.Y;
            result.Z = left.Z + right.Z;
            return result;
        }
        public static Int32_3 operator -(in Int32_3 left, in Int32_3 right)
        {
            Int32_3 result;
            result.X = left.X - right.X;
            result.Y = left.Y - right.Y;
            result.Z = left.Z - right.Z;
            return result;
        }
        public static Int32_3 operator +(in Int32_3 value)
        {
            Int32_3 result;
            result.X = +value.X;
            result.Y = +value.Y;
            result.Z = +value.Z;
            return result;
        }
        public static Int32_3 operator *(in Int32_3 value, int scale)
        {
            Int32_3 result;
            result.X = value.X * scale;
            result.Y = value.Y * scale;
            result.Z = value.Z * scale;
            return result;
        }
        public static Int32_3 operator *(in Int32_3 left, in Int32_3 right)
        {
            Int32_3 result;
            result.X = left.X * right.X;
            result.Y = left.Y * right.Y;
            result.Z = left.Z * right.Z;
            return result;
        }
        public static Int32_3 operator /(in Int32_3 left, in Int32_3 right)
        {
            Int32_3 result;
            result.X = left.X / right.X;
            result.Y = left.Y / right.Y;
            result.Z = left.Z / right.Z;
            return result;
        }
        public static Int32_3 operator /(in Int32_3 value, int scale)
        {
            Int32_3 result;
            result.X = value.X / scale;
            result.Y = value.Y / scale;
            result.Z = value.Z / scale;
            return result;
        }
        public static bool operator ==(in Int32_3 left, in Int32_3 right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(in Int32_3 left, in Int32_3 right)
        {
            return !left.Equals(right);
        }
    }
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Int32_4
    {
        public int X;
        public int Y;
        public int Z;
        public int W;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct UInt32_2
    {
        public UInt32 X;
        public UInt32 Y;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct UInt32_3 : System.IEquatable<UInt32_3>
    {
        public UInt32_3(uint InX, uint InY, uint InZ)
        {
            X = InX;
            Y = InY;
            Z = InZ;
        }
        public static UInt32_3 Zero = new UInt32_3(0, 0, 0);
        public static UInt32_3 One = new UInt32_3(1, 1, 1);
        public UInt32 X;
        public UInt32 Y;
        public UInt32 Z;
        public override bool Equals(object value)
        {
            if (value == null)
                return false;

            if (value.GetType() != GetType())
                return false;

            return Equals((UInt32_3)value);
        }
        public bool Equals(UInt32_3 value)
        {
            return (X == value.X && Y == value.Y && Z == value.Z);
        }
        public override int GetHashCode()
        {
            return X.GetHashCode() + Y.GetHashCode() + Z.GetHashCode();
        }
        public static UInt32_3 Maximize(in UInt32_3 left, in UInt32_3 right)
        {
            UInt32_3 vector;
            vector.X = (left.X > right.X) ? left.X : right.X;
            vector.Y = (left.Y > right.Y) ? left.Y : right.Y;
            vector.Z = (left.Z > right.Z) ? left.Z : right.Z;
            return vector;
        }
        public static UInt32_3 Minimize(in UInt32_3 left, in UInt32_3 right)
        {
            UInt32_3 vector;
            vector.X = (left.X < right.X) ? left.X : right.X;
            vector.Y = (left.Y < right.Y) ? left.Y : right.Y;
            vector.Z = (left.Z < right.Z) ? left.Z : right.Z;
            return vector;
        }

        public static UInt32_3 operator +(in UInt32_3 left, in UInt32_3 right)
        {
            UInt32_3 result;
            result.X = left.X + right.X;
            result.Y = left.Y + right.Y;
            result.Z = left.Z + right.Z;
            return result;
        }
        public static UInt32_3 operator -(in UInt32_3 left, in UInt32_3 right)
        {
            UInt32_3 result;
            result.X = left.X - right.X;
            result.Y = left.Y - right.Y;
            result.Z = left.Z - right.Z;
            return result;
        }
        public static UInt32_3 operator +(in UInt32_3 value)
        {
            UInt32_3 result;
            result.X = +value.X;
            result.Y = +value.Y;
            result.Z = +value.Z;
            return result;
        }
        public static UInt32_3 operator *(in UInt32_3 value, uint scale)
        {
            UInt32_3 result;
            result.X = value.X * scale;
            result.Y = value.Y * scale;
            result.Z = value.Z * scale;
            return result;
        }
        public static UInt32_3 operator *(in UInt32_3 left, in UInt32_3 right)
        {
            UInt32_3 result;
            result.X = left.X * right.X;
            result.Y = left.Y * right.Y;
            result.Z = left.Z * right.Z;
            return result;
        }
        public static UInt32_3 operator /(in UInt32_3 left, in UInt32_3 right)
        {
            UInt32_3 result;
            result.X = left.X / right.X;
            result.Y = left.Y / right.Y;
            result.Z = left.Z / right.Z;
            return result;
        }
        public static UInt32_3 operator /(in UInt32_3 value, uint scale)
        {
            UInt32_3 result;
            result.X = value.X / scale;
            result.Y = value.Y / scale;
            result.Z = value.Z / scale;
            return result;
        }
        public static bool operator ==(in UInt32_3 left, in UInt32_3 right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(in UInt32_3 left, in UInt32_3 right)
        {
            return !left.Equals(right);
        }
        public UInt32 MaxSide()
        {
            return CoreDefine.Max(X, CoreDefine.Max(Y, Z));
        }
        public bool AnyNotZero()
        {
            return (X > 0) || (Y > 0) || (Z > 0);
        }
    }

    [StructLayout(LayoutKind.Explicit, Pack = 4)]
    public struct UInt32_4
    {
        public readonly static UInt32_4 Zero = new UInt32_4();
        public readonly static UInt32_4 One = CreateInstance(1,1,1,1);
        static UInt32_4 CreateInstance(uint _x, uint _y, uint _z, uint _w)
        {
            UInt32_4 result = new UInt32_4();
            result.X = _x;
            result.Y = _y;
            result.Z = _z;
            result.W = _w;
            return result;
        }
        public void SetValue(uint _x, uint _y, uint _z, uint _w)
        {
            X = _x;
            Y = _y;
            Z = _z;
            W = _w;
        }
        public UInt32 this[uint index]
        {
            get
            {
                System.Diagnostics.Debug.Assert(index < 4);
                unsafe
                {
                    fixed (uint* p = &X)
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
                    fixed (uint* p = &X)
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
                    fixed (uint* p = &X)
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
        [Rtti.Meta]
        public UInt32 X;
        [FieldOffset(4)]
        [Rtti.Meta]
        public UInt32 Y;
        [FieldOffset(8)]
        [Rtti.Meta]
        public UInt32 Z;
        [FieldOffset(12)]
        [Rtti.Meta]
        public UInt32 W;

        [FieldOffset(0)]
        [Rtti.Meta]
        public byte x0;
        [FieldOffset(1)]
        [Rtti.Meta]
        public byte x1;
        [FieldOffset(2)]
        [Rtti.Meta]
        public byte x2;
        [FieldOffset(3)]
        [Rtti.Meta]
        public byte x3;

        [FieldOffset(4)]
        [Rtti.Meta]
        public byte y0;
        [FieldOffset(5)]
        [Rtti.Meta]
        public byte y1;
        [FieldOffset(6)]
        [Rtti.Meta]
        public byte y2;
        [FieldOffset(7)]
        [Rtti.Meta]
        public byte y3;

        [FieldOffset(8)]
        [Rtti.Meta]
        public byte z0;
        [FieldOffset(9)]
        [Rtti.Meta]
        public byte z1;
        [FieldOffset(10)]
        [Rtti.Meta]
        public byte z2;
        [FieldOffset(11)]
        [Rtti.Meta]
        public byte z3;

        [FieldOffset(12)]
        [Rtti.Meta]
        public byte w0;
        [FieldOffset(13)]
        [Rtti.Meta]
        public byte w1;
        [FieldOffset(14)]
        [Rtti.Meta]
        public byte w2;
        [FieldOffset(15)]
        [Rtti.Meta]
        public byte w3;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct UInt8_2
    {
        public byte X;
        public byte Y;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct UInt8_3
    {
        public byte X;
        public byte Y;
        public byte Z;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct UInt8_4
    {
        [Rtti.Meta]
        public byte X;
        [Rtti.Meta]
        public byte Y;
        [Rtti.Meta]
        public byte Z;
        [Rtti.Meta]
        public byte W;
    }
}
