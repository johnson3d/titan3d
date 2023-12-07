using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.NxMath
{
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 4)]
    public struct NxFloat32 : IEquatable<NxFloat32>, IComparable<NxFloat32>
    {
        public static NxFloat32 FromFloat(float v)
        {
            return new NxFloat32(v);
        }
        public NxFloat32(float v)
        {
            mValue = v;
        }
        public float mValue;
        public override int GetHashCode()
        {
            return mValue.GetHashCode();
        }
        public override string ToString()
        {
            return mValue.ToString();
        }
        public float AsSingle()
        {
            return mValue;
        }
        public double AsDouble()
        {
            return mValue;
        }
        public static NxFloat32 operator -(in NxFloat32 left)
        {
            return new NxFloat32(-left.mValue);
        }
        public static NxFloat32 operator +(in NxFloat32 left, in NxFloat32 right)
        {
            return new NxFloat32(left.mValue + right.mValue);
        }
        public static NxFloat32 operator -(in NxFloat32 left, in NxFloat32 right)
        {
            return new NxFloat32(left.mValue - right.mValue);
        }
        public static NxFloat32 operator *(in NxFloat32 left, in NxFloat32 right)
        {
            return new NxFloat32(left.mValue * right.mValue);
        }
        public static NxFloat32 operator /(in NxFloat32 left, in NxFloat32 right)
        {
            return new NxFloat32(left.mValue / right.mValue);
        }
        public override bool Equals(object other)
        {
            if(other is not NxFloat32) 
            { 
                return false; 
            }
            return mValue == ((NxFloat32)other).mValue;
        }
        public bool Equals(NxFloat32 other)
        {
            return mValue == other.mValue;
        }
        public int CompareTo(NxFloat32 other)
        {
            if (mValue > other.mValue)
                return 1;
            else if (mValue < other.mValue)
                return -1;
            else
                return 0;
        }
        public static bool operator ==(in NxFloat32 left, in NxFloat32 right)
        {
            return left.mValue == right.mValue;
        }
        public static bool operator !=(in NxFloat32 left, in NxFloat32 right)
        {
            return left.mValue == right.mValue;
        }
        public static bool operator >(in NxFloat32 left, in NxFloat32 right)
        {
            return left.mValue > right.mValue;
        }
        public static bool operator <(in NxFloat32 left, in NxFloat32 right)
        {
            return left.mValue < right.mValue;
        }
        public static bool operator >=(in NxFloat32 left, in NxFloat32 right)
        {
            return left.mValue >= right.mValue;
        }
        public static bool operator <=(in NxFloat32 left, in NxFloat32 right)
        {
            return left.mValue <= right.mValue;
        }
    }
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 4)]
    public unsafe struct NxFixed64 : IEquatable<NxFixed64>, IComparable<NxFixed64>
    {
        public static NxFixed64 FromFloat(float v)
        {
            return new NxFixed64(v);
        }
        public NxFixed64(Int64 v)
        {
            mValue = v;
        }
        public NxFixed64(float v)
        {
            mValue = (Int64)((double)v * (double)Scalar);
        }
        public Int64 mValue;
        public override int GetHashCode()
        {
            return mValue.GetHashCode();
        }
        public override string ToString()
        {
            return AsDouble().ToString();
        }
        
        public const uint BitWidth = (uint)sizeof(Int64) * 8;
        public const uint FracBit = 24;
        public const uint IntBit = BitWidth - FracBit - 1;
        public const UInt64 SignedMask = ((ulong)1) << (int)(BitWidth - 1);
        public const UInt64 FractionMask = (0xffffffffffffffff >> ((int)(BitWidth - FracBit)));
        public const UInt64 IntegerMask = (~FractionMask) & (~SignedMask);
        public const UInt64 Scalar = FractionMask + 1;

        public float AsSingle()
        {
            return mValue;
        }
        public double AsDouble()
		{
			return (double) mValue / (double) Scalar;
        }
        public static NxFixed64 operator -(in NxFixed64 left)
        {
            return new NxFixed64(-left.mValue);
        }
        public static NxFixed64 operator +(in NxFixed64 left, in NxFixed64 right)
        {
            return new NxFixed64(left.mValue + right.mValue);
        }
        public static NxFixed64 operator -(in NxFixed64 left, in NxFixed64 right)
        {
            return new NxFixed64(left.mValue - right.mValue);
        }
        public static NxFixed64 operator *(in NxFixed64 left, in NxFixed64 right)
        {
            return new NxFixed64(left.mValue * right.mValue >> (int)FracBit);
        }
        public static NxFixed64 operator /(in NxFixed64 left, in NxFixed64 right)
        {
            return new NxFixed64((left.mValue << (int)FracBit) / right.mValue);
        }
        public override bool Equals(object other)
        {
            if (other is not NxFixed64)
            {
                return false;
            }
            return mValue == ((NxFixed64)other).mValue;
        }
        public bool Equals(NxFixed64 other)
        {
            return mValue == other.mValue;
        }
        public int CompareTo(NxFixed64 other)
        {
            if (mValue > other.mValue)
                return 1;
            else if (mValue < other.mValue)
                return -1;
            else
                return 0;
        }
        public static bool operator ==(in NxFixed64 left, in NxFixed64 right)
        {
            return left.mValue == right.mValue;
        }
        public static bool operator !=(in NxFixed64 left, in NxFixed64 right)
        {
            return left.mValue == right.mValue;
        }
        public static bool operator >(in NxFixed64 left, in NxFixed64 right)
        {
            return left.mValue > right.mValue;
        }
        public static bool operator <(in NxFixed64 left, in NxFixed64 right)
        {
            return left.mValue < right.mValue;
        }
        public static bool operator >=(in NxFixed64 left, in NxFloat32 right)
        {
            return left.mValue >= right.mValue;
        }
        public static bool operator <=(in NxFixed64 left, in NxFloat32 right)
        {
            return left.mValue <= right.mValue;
        }
    }
}
