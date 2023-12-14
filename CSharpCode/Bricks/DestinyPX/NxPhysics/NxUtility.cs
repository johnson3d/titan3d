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
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 4)]
    public struct NxReal : IEquatable<NxReal>, IComparable<NxReal>
    {
#if NXREAL_F32
        public const int NxRealSize = 4;
        public NxMath.NxFloat32 mValue;
        public static NxReal ByF32(float v)
        {
            return new NxReal() { mValue = NxMath.NxFloat32.FromFloat(v) };
        }
        public static NxReal ByI32(int v)
        {
            return new NxReal() { mValue = NxMath.NxFloat32.FromFloat((float)v) };
        }
        public NxReal(in NxMath.NxFloat32 v)
        {
            mValue = v;
        }
#else
        public const int NxRealSize = 8;
        public NxMath.NxFixed64 mValue;
        public static NxReal ByF32(float v)
        {
            return new NxReal() { mValue = NxMath.NxFixed64.FromFloat(v) };
        }
        public static NxReal ByI32(int v)
        {
            return new NxReal() { mValue = NxMath.NxFloat32.FromFloat((float)v) };
        }
        public NxReal(in NxMath.NxFixed64 v)
        {
            mValue = v;
        }
#endif
        public override int GetHashCode()
        {
            return mValue.GetHashCode();
        }
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

        public static NxReal operator -(in NxReal left)
        {
            return new NxReal(-left.mValue);
        }
        public static NxReal operator +(in NxReal left, in NxReal right)
        {
            return new NxReal(left.mValue + right.mValue);
        }
        public static NxReal operator -(in NxReal left, in NxReal right)
        {
            return new NxReal(left.mValue - right.mValue);
        }
        public static NxReal operator *(in NxReal left, in NxReal right)
        {
            return new NxReal(left.mValue * right.mValue);
        }
        public static NxReal operator /(in NxReal left, in NxReal right)
        {
            return new NxReal(left.mValue / right.mValue);
        }
        public override bool Equals(object other)
        {
            if (other is not NxReal)
            {
                return false;
            }
            return mValue.Equals(((NxReal)other).mValue);
        }
        public bool Equals(NxReal other)
        {
            return mValue == other.mValue;
        }
        public int CompareTo(NxReal other)
        {
            if (mValue > other.mValue)
                return 1;
            else if (mValue < other.mValue)
                return -1;
            else
                return 0;
        }
        public static bool operator ==(in NxReal left, in NxReal right)
        {
            return left.mValue == right.mValue;
        }
        public static bool operator !=(in NxReal left, in NxReal right)
        {
            return left.mValue == right.mValue;
        }
        public static bool operator >(in NxReal left, in NxReal right)
        {
            return left.mValue > right.mValue;
        }
        public static bool operator <(in NxReal left, in NxReal right)
        {
            return left.mValue < right.mValue;
        }
        public static bool operator >=(in NxReal left, in NxReal right)
        {
            return left.mValue >= right.mValue;
        }
        public static bool operator <=(in NxReal left, in NxReal right)
        {
            return left.mValue <= right.mValue;
        }
    }

    partial struct PxVector3
    {
        [System.Runtime.InteropServices.FieldOffset(0)]
        public NxReal X;
        [System.Runtime.InteropServices.FieldOffset(1 * NxReal.NxRealSize)]
        public NxReal Y;
        [System.Runtime.InteropServices.FieldOffset(2 * NxReal.NxRealSize)]
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
        [System.Runtime.InteropServices.FieldOffset(1 * NxReal.NxRealSize)]
        public NxReal Y;
        [System.Runtime.InteropServices.FieldOffset(2 * NxReal.NxRealSize)]
        public NxReal Z;
        [System.Runtime.InteropServices.FieldOffset(3 * NxReal.NxRealSize)]
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
        public static readonly PxQuat Identity = new PxQuat(NxReal.ByI32(0), NxReal.ByI32(0), NxReal.ByI32(0), NxReal.ByI32(1));
    }
    partial struct PxPQ
    {
        [System.Runtime.InteropServices.FieldOffset(0)]
        public PxQuat Quat;
        [System.Runtime.InteropServices.FieldOffset(4 * NxReal.NxRealSize)]
        public PxVector3 Position;
        public unsafe override string ToString()
        {
            return $"Quat=({Quat});Postion=({Position})";
        }
    }

    public class TtRandom : AuxPtrType<EngineNS.NxPhysics.NxRandom>
    {
        public TtRandom(UInt64 seed)
        {
            mCoreObject = NxRandom.CreateInstance(seed);
        }
    }
}
