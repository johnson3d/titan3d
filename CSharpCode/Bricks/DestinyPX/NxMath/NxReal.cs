using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.NxMath
{
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 4)]
    public struct NxFloat32
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
    }
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 4)]
    public unsafe struct NxFixed64
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
        public static NxFixed64 operator +(NxFixed64 left, NxFixed64 right)
        {
            return new NxFixed64(left.mValue + right.mValue);
        }
    }
}
