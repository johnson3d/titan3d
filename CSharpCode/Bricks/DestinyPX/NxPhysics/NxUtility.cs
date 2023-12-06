#define NXREAL_F32
//#define NXREAL_40_24
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NPOI.SS.Formula.Eval;
using NPOI.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.NxPhysics
{
    public struct NxRealUtility
    {
#if NXREAL_F32
        public const int NxRealSize = 4;
#else
        public const int NxRealSize = 8;
#endif
    }
    partial struct PxVector3
    {
        [System.Runtime.InteropServices.FieldOffset(0)]
        public NxReal X;
        [System.Runtime.InteropServices.FieldOffset(1 * NxRealUtility.NxRealSize)]
        public NxReal Y;
        [System.Runtime.InteropServices.FieldOffset(2 * NxRealUtility.NxRealSize)]
        public NxReal Z;
        public PxVector3(in NxReal x, in NxReal y, in NxReal z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public override string ToString()
        {
            return $"{X},{Y},{Z}";
        }
        public static PxVector3 operator +(PxVector3 left, PxVector3 right)
        {
            return PxVector3.Add(in left, in right);
        }
        public static PxVector3 operator -(PxVector3 left, PxVector3 right)
        {
            return PxVector3.Sub(in left, in right);
        }
        public static PxVector3 operator *(PxVector3 left, PxVector3 right)
        {
            return PxVector3.Mul(in left, in right);
        }
        public static PxVector3 operator /(PxVector3 left, PxVector3 right)
        {
            return PxVector3.Div(in left, in right);
        }
    }
    partial struct PxQuat
    {
        [System.Runtime.InteropServices.FieldOffset(0)]
        public NxReal X;
        [System.Runtime.InteropServices.FieldOffset(1 * NxRealUtility.NxRealSize)]
        public NxReal Y;
        [System.Runtime.InteropServices.FieldOffset(2 * NxRealUtility.NxRealSize)]
        public NxReal Z;
        [System.Runtime.InteropServices.FieldOffset(3 * NxRealUtility.NxRealSize)]
        public NxReal W;
        public override string ToString()
        {
            return $"{X},{Y},{Z},{W}";
        }
        public PxQuat(in NxReal x, in NxReal y, in NxReal z, in NxReal w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }
    }
    partial struct PxPQ
    {
        public unsafe override string ToString()
        {
            return $"Postion=({*GetPosition()});Quat=({*GetQuat()})";
        }
    }

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 4)]
    public struct NxReal
    {
#if NXREAL_F32
        public NxMath.NxFloat32 mValue;
        public static NxReal ByF32(float v)
        {
            return new NxReal() { mValue = NxMath.NxFloat32.FromFloat(v) };
        }
#else
        public NxMath.NxFixed64 mValue;
        public static NxReal ByF32(float v)
        {
            return new NxReal() { mValue = NxMath.NxFixed64.FromFloat(v) };
        }
#endif
        public override string ToString()
        {
            return AsDouble().ToString();
        }
        public float AsSingle()
        {
            return mValue.AsSingle();
        }
        public double AsDouble()
        {
            return mValue.AsDouble();
        }
    }
}
