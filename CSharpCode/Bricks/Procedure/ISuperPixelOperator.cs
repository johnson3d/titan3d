using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Procedure
{
    public interface ISuperPixelOperatorBase
    {
        Rtti.UTypeDesc ElementType { get; }
        Rtti.UTypeDesc BufferType { get; }
        unsafe int Compare(void* left, void* right);
        unsafe void SetAsMaxValue(void* tar);
        unsafe void SetAsMinValue(void* tar);
        unsafe void Copy(Rtti.UTypeDesc tarTyp, void* tar, Rtti.UTypeDesc srcType, void* src);
        unsafe void SetIfGreateThan(Rtti.UTypeDesc tarTyp, void* tar, Rtti.UTypeDesc srcType, void* src);
        unsafe void SetIfLessThan(Rtti.UTypeDesc tarTyp, void* tar, Rtti.UTypeDesc srcType, void* src);
        unsafe void Add(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right);
        unsafe void Sub(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right);
        unsafe void Mul(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right);
        unsafe void Div(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right);
        unsafe void Lerp(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right, float factor);
        unsafe void Max(void* result, void* left, void* right);
        unsafe void Min(void* result, void* left, void* right);
        unsafe void Abs(void* result, void* left);
    }

    public interface ISuperPixelOperator<T> : ISuperPixelOperatorBase where T : unmanaged
    {
        T MaxValue { get; }
        T MinValue { get; }
        int Compare(in T left, in T right);
        T Add(in T left, in T right);
    }
    public struct FFloatOperator : ISuperPixelOperator<float>
    {
        public Rtti.UTypeDesc ElementType
        {
            get
            {
                return Rtti.UTypeDescGetter<float>.TypeDesc;
            }
        }
        public Rtti.UTypeDesc BufferType
        {
            get
            {
                return Rtti.UTypeDescGetter<USuperBuffer<float, FFloatOperator>>.TypeDesc;
            }
        }
        public float MaxValue { get => float.MaxValue; }
        public float MinValue { get => float.MinValue; }
        public unsafe int Compare(void* left, void* right)
        {
            ref var l = ref *(float*)left;
            ref var r = ref *(float*)right;
            return l.CompareTo(r);
        }

        public int Compare(in float left, in float right)
        {
            return left.CompareTo(right);
        }
        public unsafe void SetIfGreateThan(Rtti.UTypeDesc tarTyp, void* tar, Rtti.UTypeDesc srcType, void* src)
        {
            if (*(float*)src > *(float*)tar)
            {
                *(float*)tar = *(float*)src;
            }
        }
        public unsafe void SetIfLessThan(Rtti.UTypeDesc tarTyp, void* tar, Rtti.UTypeDesc srcType, void* src)
        {
            if (*(float*)src < *(float*)tar)
            {
                *(float*)tar = *(float*)src;
            }
        }
        public float Add(in float left, in float right)
        {
            return left + right;
        }
        public unsafe void SetAsMaxValue(void* tar)
        {
            (*(float*)tar) = float.MaxValue;
        }
        public unsafe void SetAsMinValue(void* tar)
        {
            (*(float*)tar) = float.MinValue;
        }
        public unsafe void Copy(Rtti.UTypeDesc tarTyp, void* tar, Rtti.UTypeDesc srcType, void* src)
        {
            (*(float*)tar) = (*(float*)src);
        }
        public unsafe void Add(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right)
        {
            if (resultType != Rtti.UTypeDescGetter<float>.TypeDesc && resultType != leftType && resultType != rightType)
            {
                return;
            }
            float rValue = 0;
            if (right != (void*)0)
            {
                rValue = *(float*)right;
            }
            (*(float*)result) = (*(float*)left) + rValue;
        }
        public unsafe void Sub(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right)
        {
            if (resultType != Rtti.UTypeDescGetter<float>.TypeDesc && resultType != leftType && resultType != rightType)
            {
                return;
            }
            float rValue = 0;
            if (right != (void*)0)
            {
                rValue = *(float*)right;
            }
            (*(float*)result) = (*(float*)left) - rValue;
        }
        public unsafe void Mul(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right)
        {
            if (resultType != Rtti.UTypeDescGetter<float>.TypeDesc && resultType != leftType && resultType != rightType)
            {
                return;
            }
            float rValue = 1;
            if (right != (void*)0)
            {
                rValue = *(float*)right;
            }
            (*(float*)result) = (*(float*)left) * rValue;
        }
        public unsafe void Div(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right)
        {
            if (resultType != Rtti.UTypeDescGetter<float>.TypeDesc && resultType != leftType && resultType != rightType)
            {
                return;
            }
            float rValue = 1;
            if (right != (void*)0)
            {
                rValue = *(float*)right;
            }
            (*(float*)result) = (*(float*)left) / rValue;
        }
        public unsafe void Lerp(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right, float factor)
        {
            if (resultType != Rtti.UTypeDescGetter<float>.TypeDesc && resultType != leftType && resultType != rightType)
            {
                return;
            }
            float rValue = 1;
            if (right != (void*)0)
            {
                rValue = *(float*)right;
            }
            (*(float*)result) = MathHelper.Lerp(*(float*)left, rValue, factor);
        }
        public unsafe void Max(void* result, void* left, void* right)
        {
            ref var l = ref *(float*)left;
            ref var r = ref *(float*)right;
            ref var t = ref *(float*)result;

            t = MathHelper.Max(l, r);
        }
        public unsafe void Min(void* result, void* left, void* right)
        {
            ref var l = ref *(float*)left;
            ref var r = ref *(float*)right;
            ref var t = ref *(float*)result;

            t = MathHelper.Min(l, r);
        }
        public unsafe void Abs(void* result, void* left)
        {
            ref var l = ref *(float*)left;
            ref var t = ref *(float*)result;
            t = MathHelper.Abs(l);
        }
    }
    public struct FFloat2Operator : ISuperPixelOperator<Vector2>
    {
        public Rtti.UTypeDesc ElementType
        {
            get
            {
                return Rtti.UTypeDescGetter<Vector2>.TypeDesc;
            }
        }
        public Rtti.UTypeDesc BufferType
        {
            get
            {
                return Rtti.UTypeDescGetter<USuperBuffer<Vector2, FFloat2Operator>>.TypeDesc;
            }
        }
        public Vector2 MaxValue { get => Vector2.MaxValue; }
        public Vector2 MinValue { get => Vector2.MinValue; }
        public unsafe int Compare(void* left, void* right)
        {
            ref var l = ref *(Vector2*)left;
            ref var r = ref *(Vector2*)right;
            return Compare(in l, in r);
        }
        public int Compare(in Vector2 left, in Vector2 right)
        {
            //if (left.X < right.X ||
            //    left.Y < right.Y ||
            //    left.Z < right.Z)
            //    return -1;
            //else 
            //return left.CompareTo(right);
            return 0;
        }
        public unsafe void SetIfGreateThan(Rtti.UTypeDesc tarTyp, void* tar, Rtti.UTypeDesc srcType, void* src)
        {
            ref var sVec3 = ref *(Vector2*)src;
            ref var tVec3 = ref *(Vector2*)tar;
            tVec3 = Vector2.Maximize(in sVec3, in tVec3);
        }
        public unsafe void SetIfLessThan(Rtti.UTypeDesc tarTyp, void* tar, Rtti.UTypeDesc srcType, void* src)
        {
            ref var sVec3 = ref *(Vector2*)src;
            ref var tVec3 = ref *(Vector2*)tar;
            tVec3 = Vector2.Minimize(in sVec3, in tVec3);
        }
        public Vector2 Add(in Vector2 left, in Vector2 right)
        {
            return left + right;
        }
        public unsafe void SetAsMaxValue(void* tar)
        {
            (*(Vector2*)tar) = Vector2.MaxValue;
        }
        public unsafe void SetAsMinValue(void* tar)
        {
            (*(Vector2*)tar) = Vector2.MinValue;
        }
        public unsafe void Copy(Rtti.UTypeDesc tarTyp, void* tar, Rtti.UTypeDesc srcType, void* src)
        {
            (*(Vector2*)tar) = (*(Vector2*)src);
        }
        public unsafe void Add(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right)
        {
            if (resultType != Rtti.UTypeDescGetter<Vector2>.TypeDesc && resultType != leftType && resultType != rightType)
            {
                return;
            }
            var rValue = Vector2.Zero;
            if (right != (void*)0)
            {
                rValue = *(Vector2*)right;
            }
            (*(Vector2*)result) = (*(Vector2*)left) + rValue;
        }
        public unsafe void Sub(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right)
        {
            if (resultType != Rtti.UTypeDescGetter<Vector2>.TypeDesc && resultType != leftType && resultType != rightType)
            {
                return;
            }
            var rValue = Vector2.Zero;
            if (right != (void*)0)
            {
                rValue = *(Vector2*)right;
            }
            (*(Vector2*)result) = (*(Vector2*)left) - rValue;
        }
        public unsafe void Mul(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right)
        {
            if (resultType != Rtti.UTypeDescGetter<Vector2>.TypeDesc && resultType != leftType)
            {
                return;
            }
            if (rightType == Rtti.UTypeDescGetter<Vector2>.TypeDesc)
            {
                var rValue = Vector2.One;
                if (right != (void*)0)
                {
                    rValue = *(Vector2*)right;
                }
                (*(Vector2*)result) = (*(Vector2*)left) * rValue;
            }
            else if (rightType == Rtti.UTypeDescGetter<float>.TypeDesc)
            {
                float rValue = 1.0f;
                if (right != (void*)0)
                {
                    rValue = *(float*)right;
                }
                (*(Vector2*)result) = (*(Vector2*)left) * rValue;
            }
        }
        public unsafe void Div(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right)
        {
            if (resultType != Rtti.UTypeDescGetter<Vector2>.TypeDesc && resultType != leftType && resultType != rightType)
            {
                return;
            }
            var rValue = Vector2.One;
            if (right != (void*)0)
            {
                rValue = *(Vector2*)right;
            }
            (*(Vector2*)result) = (*(Vector2*)left) / rValue;
        }
        public unsafe void Lerp(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right, float factor)
        {
            if (resultType != Rtti.UTypeDescGetter<Vector2>.TypeDesc && resultType != leftType && resultType != rightType)
            {
                return;
            }
            var rValue = Vector2.One;
            if (right != (void*)0)
            {
                rValue = *(Vector2*)right;
            }
            (*(Vector2*)result) = Vector2.Lerp(*(Vector2*)left, rValue, factor);
        }
        public unsafe void Max(void* result, void* left, void* right)
        {
            ref var l = ref *(Vector2*)left;
            ref var r = ref *(Vector2*)right;
            ref var t = ref *(Vector2*)result;

            t = Vector2.Maximize(l, r);
        }
        public unsafe void Min(void* result, void* left, void* right)
        {
            ref var l = ref *(Vector2*)left;
            ref var r = ref *(Vector2*)right;
            ref var t = ref *(Vector2*)result;

            t = Vector2.Minimize(l, r);
        }
        public unsafe void Abs(void* result, void* left)
        {
            ref var l = ref *(Vector2*)left;
            ref var t = ref *(Vector2*)result;
            t.X = MathHelper.Abs(l.X);
            t.Y = MathHelper.Abs(l.Y);
        }
    }
    public struct FFloat3Operator : ISuperPixelOperator<Vector3>
    {
        public Rtti.UTypeDesc ElementType
        {
            get
            {
                return Rtti.UTypeDescGetter<Vector3>.TypeDesc;
            }
        }
        public Rtti.UTypeDesc BufferType
        {
            get
            {
                return Rtti.UTypeDescGetter<USuperBuffer<Vector3, FFloat3Operator>>.TypeDesc;
            }
        }
        public Vector3 MaxValue { get => Vector3.MaxValue; }
        public Vector3 MinValue { get => Vector3.MinValue; }
        public unsafe int Compare(void* left, void* right)
        {
            ref var l = ref *(Vector3*)left;
            ref var r = ref *(Vector3*)right;
            return Compare(in l, in r);
        }
        public int Compare(in Vector3 left, in Vector3 right)
        {
            //if (left.X < right.X ||
            //    left.Y < right.Y ||
            //    left.Z < right.Z)
            //    return -1;
            //else 
            //return left.CompareTo(right);
            return 0;
        }
        public unsafe void SetIfGreateThan(Rtti.UTypeDesc tarTyp, void* tar, Rtti.UTypeDesc srcType, void* src)
        {
            ref var sVec3 = ref *(Vector3*)src;
            ref var tVec3 = ref *(Vector3*)tar;
            tVec3 = Vector3.Maximize(in sVec3, in tVec3);
        }
        public unsafe void SetIfLessThan(Rtti.UTypeDesc tarTyp, void* tar, Rtti.UTypeDesc srcType, void* src)
        {
            ref var sVec3 = ref *(Vector3*)src;
            ref var tVec3 = ref *(Vector3*)tar;
            tVec3 = Vector3.Minimize(in sVec3, in tVec3);
        }
        public Vector3 Add(in Vector3 left, in Vector3 right)
        {
            return left + right;
        }
        public unsafe void SetAsMaxValue(void* tar)
        {
            (*(Vector3*)tar) = Vector3.MaxValue;
        }
        public unsafe void SetAsMinValue(void* tar)
        {
            (*(Vector3*)tar) = Vector3.MinValue;
        }
        public unsafe void Copy(Rtti.UTypeDesc tarTyp, void* tar, Rtti.UTypeDesc srcType, void* src)
        {
            (*(Vector3*)tar) = (*(Vector3*)src);
        }
        public unsafe void Add(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right)
        {
            if (resultType != Rtti.UTypeDescGetter<Vector3>.TypeDesc && resultType != leftType && resultType != rightType)
            {
                return;
            }
            Vector3 rValue = Vector3.Zero;
            if (right != (void*)0)
            {
                rValue = *(Vector3*)right;
            }
            (*(Vector3*)result) = (*(Vector3*)left) + rValue;
        }
        public unsafe void Sub(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right)
        {
            if (resultType != Rtti.UTypeDescGetter<Vector3>.TypeDesc && resultType != leftType && resultType != rightType)
            {
                return;
            }
            Vector3 rValue = Vector3.Zero;
            if (right != (void*)0)
            {
                rValue = *(Vector3*)right;
            }
            (*(Vector3*)result) = (*(Vector3*)left) - rValue;
        }
        public unsafe void Mul(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right)
        {
            if (resultType != Rtti.UTypeDescGetter<Vector3>.TypeDesc && resultType != leftType)
            {
                return;
            }
            if (rightType == Rtti.UTypeDescGetter<Vector3>.TypeDesc)
            {
                Vector3 rValue = Vector3.One;
                if (right != (void*)0)
                {
                    rValue = *(Vector3*)right;
                }
                (*(Vector3*)result) = (*(Vector3*)left) * rValue;
            }
            else if (rightType == Rtti.UTypeDescGetter<float>.TypeDesc)
            {
                float rValue = 1.0f;
                if (right != (void*)0)
                {
                    rValue = *(float*)right;
                }
                (*(Vector3*)result) = (*(Vector3*)left) * rValue;
            }
        }
        public unsafe void Div(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right)
        {
            if (resultType != Rtti.UTypeDescGetter<Vector3>.TypeDesc && resultType != leftType && resultType != rightType)
            {
                return;
            }
            Vector3 rValue = Vector3.One;
            if (right != (void*)0)
            {
                rValue = *(Vector3*)right;
            }
            (*(Vector3*)result) = (*(Vector3*)left) / rValue;
        }
        public unsafe void Lerp(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right, float factor)
        {
            if (resultType != Rtti.UTypeDescGetter<Vector3>.TypeDesc && resultType != leftType && resultType != rightType)
            {
                return;
            }
            var rValue = Vector3.One;
            if (right != (void*)0)
            {
                rValue = *(Vector3*)right;
            }
            (*(Vector3*)result) = Vector3.Lerp(*(Vector3*)left, rValue, factor);
        }
        public unsafe void Max(void* result, void* left, void* right)
        {
            ref var l = ref *(Vector3*)left;
            ref var r = ref *(Vector3*)right;
            ref var t = ref *(Vector3*)result;

            t = Vector3.Maximize(l, r);
        }
        public unsafe void Min(void* result, void* left, void* right)
        {
            ref var l = ref *(Vector3*)left;
            ref var r = ref *(Vector3*)right;
            ref var t = ref *(Vector3*)result;

            t = Vector3.Minimize(l, r);
        }
        public unsafe void Abs(void* result, void* left)
        {
            ref var l = ref *(Vector3*)left;
            ref var t = ref *(Vector3*)result;
            t.X = MathHelper.Abs(l.X);
            t.Y = MathHelper.Abs(l.Y);
            t.Z = MathHelper.Abs(l.Z);
        }
    }
    public struct FFloat4Operator : ISuperPixelOperator<Vector4>
    {
        public Rtti.UTypeDesc ElementType
        {
            get
            {
                return Rtti.UTypeDescGetter<Vector4>.TypeDesc;
            }
        }
        public Rtti.UTypeDesc BufferType
        {
            get
            {
                return Rtti.UTypeDescGetter<USuperBuffer<Vector4, FFloat4Operator>>.TypeDesc;
            }
        }
        public Vector4 MaxValue { get => Vector4.MaxValue; }
        public Vector4 MinValue { get => Vector4.MinValue; }
        public unsafe int Compare(void* left, void* right)
        {
            ref var l = ref *(Vector4*)left;
            ref var r = ref *(Vector4*)right;
            return Compare(in l, in r);
        }
        public int Compare(in Vector4 left, in Vector4 right)
        {
            //if (left.X < right.X ||
            //    left.Y < right.Y ||
            //    left.Z < right.Z)
            //    return -1;
            //else 
            //return left.CompareTo(right);
            return 0;
        }
        public unsafe void SetIfGreateThan(Rtti.UTypeDesc tarTyp, void* tar, Rtti.UTypeDesc srcType, void* src)
        {
            ref var sVec3 = ref *(Vector4*)src;
            ref var tVec3 = ref *(Vector4*)tar;
            tVec3 = Vector4.Maximize(in sVec3, in tVec3);
        }
        public unsafe void SetIfLessThan(Rtti.UTypeDesc tarTyp, void* tar, Rtti.UTypeDesc srcType, void* src)
        {
            ref var sVec3 = ref *(Vector4*)src;
            ref var tVec3 = ref *(Vector4*)tar;
            tVec3 = Vector4.Minimize(in sVec3, in tVec3);
        }
        public Vector4 Add(in Vector4 left, in Vector4 right)
        {
            return left + right;
        }
        public unsafe void SetAsMaxValue(void* tar)
        {
            (*(Vector4*)tar) = Vector4.MaxValue;
        }
        public unsafe void SetAsMinValue(void* tar)
        {
            (*(Vector4*)tar) = Vector4.MinValue;
        }
        public unsafe void Copy(Rtti.UTypeDesc tarTyp, void* tar, Rtti.UTypeDesc srcType, void* src)
        {
            (*(Vector4*)tar) = (*(Vector4*)src);
        }
        public unsafe void Add(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right)
        {
            if (resultType != Rtti.UTypeDescGetter<Vector4>.TypeDesc && resultType != leftType && resultType != rightType)
            {
                return;
            }
            Vector4 rValue = Vector4.Zero;
            if (right != (void*)0)
            {
                rValue = *(Vector4*)right;
            }
            (*(Vector4*)result) = (*(Vector4*)left) + rValue;
        }
        public unsafe void Sub(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right)
        {
            if (resultType != Rtti.UTypeDescGetter<Vector3>.TypeDesc && resultType != leftType && resultType != rightType)
            {
                return;
            }
            Vector4 rValue = Vector4.Zero;
            if (right != (void*)0)
            {
                rValue = *(Vector4*)right;
            }
            (*(Vector4*)result) = (*(Vector4*)left) - rValue;
        }
        public unsafe void Mul(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right)
        {
            if (resultType != Rtti.UTypeDescGetter<Vector3>.TypeDesc && resultType != leftType)
            {
                return;
            }
            if (rightType == Rtti.UTypeDescGetter<Vector4>.TypeDesc)
            {
                Vector4 rValue = Vector4.One;
                if (right != (void*)0)
                {
                    rValue = *(Vector4*)right;
                }
                (*(Vector4*)result) = (*(Vector4*)left) * rValue;
            }
            else if (rightType == Rtti.UTypeDescGetter<float>.TypeDesc)
            {
                float rValue = 1.0f;
                if (right != (void*)0)
                {
                    rValue = *(float*)right;
                }
                (*(Vector4*)result) = (*(Vector4*)left) * rValue;
            }
        }
        public unsafe void Div(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right)
        {
            if (resultType != Rtti.UTypeDescGetter<Vector3>.TypeDesc && resultType != leftType && resultType != rightType)
            {
                return;
            }
            Vector4 rValue = Vector4.One;
            if (right != (void*)0)
            {
                rValue = *(Vector4*)right;
            }
            (*(Vector4*)result) = (*(Vector4*)left) / rValue;
        }
        public unsafe void Lerp(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right, float factor)
        {
            if (resultType != Rtti.UTypeDescGetter<Vector4>.TypeDesc && resultType != leftType && resultType != rightType)
            {
                return;
            }
            var rValue = Vector4.One;
            if (right != (void*)0)
            {
                rValue = *(Vector4*)right;
            }
            (*(Vector4*)result) = Vector4.Lerp(*(Vector4*)left, rValue, factor);
        }
        public unsafe void Max(void* result, void* left, void* right)
        {
            ref var l = ref *(Vector4*)left;
            ref var r = ref *(Vector4*)right;
            ref var t = ref *(Vector4*)result;

            t = Vector4.Maximize(l, r);
        }
        public unsafe void Min(void* result, void* left, void* right)
        {
            ref var l = ref *(Vector4*)left;
            ref var r = ref *(Vector4*)right;
            ref var t = ref *(Vector4*)result;

            t = Vector4.Minimize(l, r);
        }
        public unsafe void Abs(void* result, void* left)
        {
            ref var l = ref *(Vector4*)left;
            ref var t = ref *(Vector4*)result;
            t.X = MathHelper.Abs(l.X);
            t.Y = MathHelper.Abs(l.Y);
            t.Z = MathHelper.Abs(l.Z);
            t.W = MathHelper.Abs(l.W);
        }
    }
    public struct FQuaternionOperator : ISuperPixelOperator<Quaternion>
    {
        public Rtti.UTypeDesc ElementType
        {
            get
            {
                return Rtti.UTypeDescGetter<Quaternion>.TypeDesc;
            }
        }
        public Rtti.UTypeDesc BufferType
        {
            get
            {
                return Rtti.UTypeDescGetter<USuperBuffer<Quaternion, FQuaternionOperator>>.TypeDesc;
            }
        }
        public Quaternion MaxValue { get => Quaternion.Identity; }
        public Quaternion MinValue { get => Quaternion.Identity; }
        public unsafe int Compare(void* left, void* right)
        {
            ref var l = ref *(Quaternion*)left;
            ref var r = ref *(Quaternion*)right;
            return Compare(in l, in r);
        }
        public int Compare(in Quaternion left, in Quaternion right)
        {
            return 0;
        }
        public unsafe void SetIfGreateThan(Rtti.UTypeDesc tarTyp, void* tar, Rtti.UTypeDesc srcType, void* src)
        {
        }
        public unsafe void SetIfLessThan(Rtti.UTypeDesc tarTyp, void* tar, Rtti.UTypeDesc srcType, void* src)
        {
        }
        public Quaternion Add(in Quaternion left, in Quaternion right)
        {
            return Quaternion.Add(left, right);
        }
        public unsafe void SetAsMaxValue(void* tar)
        {
            (*(Quaternion*)tar) = Quaternion.Identity;
        }
        public unsafe void SetAsMinValue(void* tar)
        {
            (*(Quaternion*)tar) = Quaternion.Identity;
        }
        public unsafe void Copy(Rtti.UTypeDesc tarTyp, void* tar, Rtti.UTypeDesc srcType, void* src)
        {
            (*(Quaternion*)tar) = (*(Quaternion*)src);
        }
        public unsafe void Add(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right)
        {
            if (resultType != Rtti.UTypeDescGetter<Quaternion>.TypeDesc && resultType != leftType && resultType != rightType)
                return;
            Quaternion rValue = Quaternion.Identity;
            if (right != (void*)0)
                rValue = *(Quaternion*)right;
            *(Quaternion*)result = Quaternion.Add(in *(Quaternion*)left, in *(Quaternion*)right);
        }
        public unsafe void Sub(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right)
        {
            if (resultType != Rtti.UTypeDescGetter<Quaternion>.TypeDesc && resultType != leftType && resultType != rightType)
                return;
            Quaternion rValue = Quaternion.Identity;
            if(right != (void*)0)
                rValue = *(Quaternion*)(right);
            *(Quaternion*)result = Quaternion.Subtract(in *(Quaternion*)left, in *(Quaternion*)right);
        }
        public unsafe void Mul(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right)
        {
            if (resultType != Rtti.UTypeDescGetter<Quaternion>.TypeDesc && resultType != leftType && resultType != rightType)
                return;
            Quaternion rValue = Quaternion.Identity;
            if (right != (void*)0)
                rValue = *(Quaternion*)right;
            *(Quaternion*)result = Quaternion.Multiply(in *(Quaternion*)left, in *(Quaternion*)right);
        }
        public unsafe void Div(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right)
        {
            if (resultType != Rtti.UTypeDescGetter<Quaternion>.TypeDesc && resultType != leftType && resultType != rightType)
                return;
            Quaternion rValue = Quaternion.Identity;
            if(right != (void*)0)
                rValue = *(Quaternion*)right;
            *(Quaternion*)result = Quaternion.Divide(in *(Quaternion*)left, in *(Quaternion*)right);
        }
        public unsafe void Lerp(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right, float factor)
        {
            if (resultType != Rtti.UTypeDescGetter<Quaternion>.TypeDesc && resultType != leftType && leftType != rightType)
                return;
            var rValue = Quaternion.Identity;
            if (right != (void*)0)
                rValue = *(Quaternion*)right;
            *(Quaternion*)result = Quaternion.Lerp(in *(Quaternion*)left, in *(Quaternion*)right, factor);
        }
        public unsafe void Max(void* result, void* left, void* right)
        {
        }
        public unsafe void Min(void* result, void* left, void* right)
        {
        }
        public unsafe void Abs(void* result, void* left)
        {
        }
    }

    public struct FIntOperator : ISuperPixelOperator<int>
    {
        public Rtti.UTypeDesc ElementType
        {
            get
            {
                return Rtti.UTypeDescGetter<int>.TypeDesc;
            }
        }
        public Rtti.UTypeDesc BufferType
        {
            get
            {
                return Rtti.UTypeDescGetter<USuperBuffer<int, FIntOperator>>.TypeDesc;
            }
        }
        public int MaxValue { get => int.MaxValue; }
        public int MinValue { get => int.MinValue; }
        public unsafe int Compare(void* left, void* right)
        {
            ref var l = ref *(int*)left;
            ref var r = ref *(int*)right;
            return Compare(in l, in r);
        }
        public int Compare(in int left, in int right)
        {
            return left.CompareTo(right);
        }
        public unsafe void SetIfGreateThan(Rtti.UTypeDesc tarTyp, void* tar, Rtti.UTypeDesc srcType, void* src)
        {
            if (*(int*)src > *(int*)tar)
            {
                *(int*)tar = *(int*)src;
            }
        }
        public unsafe void SetIfLessThan(Rtti.UTypeDesc tarTyp, void* tar, Rtti.UTypeDesc srcType, void* src)
        {
            if (*(int*)src < *(int*)tar)
            {
                *(int*)tar = *(int*)src;
            }
        }
        public int Add(in int left, in int right)
        {
            return left + right;
        }
        public unsafe void SetAsMaxValue(void* tar)
        {
            (*(int*)tar) = int.MaxValue;
        }
        public unsafe void SetAsMinValue(void* tar)
        {
            (*(int*)tar) = int.MinValue;
        }
        public unsafe void Copy(Rtti.UTypeDesc tarTyp, void* tar, Rtti.UTypeDesc srcType, void* src)
        {
            (*(int*)tar) = (*(int*)src);
        }
        public unsafe void Add(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right)
        {
            if (resultType != Rtti.UTypeDescGetter<int>.TypeDesc && resultType != leftType && resultType != rightType)
            {
                return;
            }
            int rValue = 0;
            if (right != (void*)0)
            {
                rValue = *(int*)right;
            }
            (*(int*)result) = (*(int*)left) + rValue;
        }
        public unsafe void Sub(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right)
        {
            if (resultType != Rtti.UTypeDescGetter<int>.TypeDesc && resultType != leftType && resultType != rightType)
            {
                return;
            }
            int rValue = 0;
            if (right != (void*)0)
            {
                rValue = *(int*)right;
            }
            (*(int*)result) = (*(int*)left) - rValue;
        }
        public unsafe void Mul(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right)
        {
            if (resultType != Rtti.UTypeDescGetter<int>.TypeDesc && resultType != leftType && resultType != rightType)
            {
                return;
            }
            int rValue = 1;
            if (right != (void*)0)
            {
                rValue = *(int*)right;
            }
            (*(int*)result) = (*(int*)left) * rValue;
        }
        public unsafe void Div(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right)
        {
            if (resultType != Rtti.UTypeDescGetter<int>.TypeDesc && resultType != leftType && resultType != rightType)
            {
                return;
            }
            int rValue = 1;
            if (right != (void*)0)
            {
                rValue = *(int*)right;
            }
            (*(int*)result) = (*(int*)left) / rValue;
        }
        public unsafe void Lerp(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right, float factor)
        {
            if (resultType != Rtti.UTypeDescGetter<int>.TypeDesc && resultType != leftType && resultType != rightType)
            {
                return;
            }
            int rValue = 1;
            if (right != (void*)0)
            {
                rValue = *(int*)right;
            }
            (*(int*)result) = (int)MathHelper.Lerp((float)*(int*)left, (float)rValue, factor);
        }
        public unsafe void Max(void* result, void* left, void* right)
        {
            ref var l = ref *(int*)left;
            ref var r = ref *(int*)right;
            ref var t = ref *(int*)result;

            t = MathHelper.Max(l, r);
        }
        public unsafe void Min(void* result, void* left, void* right)
        {
            ref var l = ref *(int*)left;
            ref var r = ref *(int*)right;
            ref var t = ref *(int*)result;

            t = MathHelper.Min(l, r);
        }
        public unsafe void Abs(void* result, void* left)
        {
            ref var l = ref *(int*)left;
            ref var t = ref *(int*)result;
            t = MathHelper.Abs(l);
        }
    }
    public struct FInt2Operator : ISuperPixelOperator<Vector2i>
    {
        public Rtti.UTypeDesc ElementType
        {
            get
            {
                return Rtti.UTypeDescGetter<Vector2i>.TypeDesc;
            }
        }
        public Rtti.UTypeDesc BufferType
        {
            get
            {
                return Rtti.UTypeDescGetter<USuperBuffer<Vector2i, FInt2Operator>>.TypeDesc;
            }
        }
        public Vector2i MaxValue { get => Vector2i.MaxValue; }
        public Vector2i MinValue { get => Vector2i.MinValue; }
        public unsafe int Compare(void* left, void* right)
        {
            ref var l = ref *(Vector2i*)left;
            ref var r = ref *(Vector2i*)right;
            return Compare(in l, in r);
        }
        public int Compare(in Vector2i left, in Vector2i right)
        {
            //if (left.X < right.X ||
            //    left.Y < right.Y ||
            //    left.Z < right.Z)
            //    return -1;
            //else 
            //return left.CompareTo(right);
            return 0;
        }
        public unsafe void SetIfGreateThan(Rtti.UTypeDesc tarTyp, void* tar, Rtti.UTypeDesc srcType, void* src)
        {
            ref var sVec3 = ref *(Vector2i*)src;
            ref var tVec3 = ref *(Vector2i*)tar;
            tVec3 = Vector2i.Maximize(in sVec3, in tVec3);
        }
        public unsafe void SetIfLessThan(Rtti.UTypeDesc tarTyp, void* tar, Rtti.UTypeDesc srcType, void* src)
        {
            ref var sVec3 = ref *(Vector2i*)src;
            ref var tVec3 = ref *(Vector2i*)tar;
            tVec3 = Vector2i.Minimize(in sVec3, in tVec3);
        }
        public Vector2i Add(in Vector2i left, in Vector2i right)
        {
            return left + right;
        }
        public unsafe void SetAsMaxValue(void* tar)
        {
            (*(Vector2i*)tar) = Vector2i.MaxValue;
        }
        public unsafe void SetAsMinValue(void* tar)
        {
            (*(Vector2i*)tar) = Vector2i.MinValue;
        }
        public unsafe void Copy(Rtti.UTypeDesc tarTyp, void* tar, Rtti.UTypeDesc srcType, void* src)
        {
            (*(Vector2i*)tar) = (*(Vector2i*)src);
        }
        public unsafe void Add(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right)
        {
            if (resultType != Rtti.UTypeDescGetter<Vector2i>.TypeDesc && resultType != leftType && resultType != rightType)
            {
                return;
            }
            Vector2i rValue = Vector2i.Zero;
            if (right != (void*)0)
            {
                rValue = *(Vector2i*)right;
            }
            (*(Vector2i*)result) = (*(Vector2i*)left) + rValue;
        }
        public unsafe void Sub(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right)
        {
            if (resultType != Rtti.UTypeDescGetter<Vector2i>.TypeDesc && resultType != leftType && resultType != rightType)
            {
                return;
            }
            Vector2i rValue = Vector2i.Zero;
            if (right != (void*)0)
            {
                rValue = *(Vector2i*)right;
            }
            (*(Vector2i*)result) = (*(Vector2i*)left) - rValue;
        }
        public unsafe void Mul(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right)
        {
            if (resultType != Rtti.UTypeDescGetter<Vector2i>.TypeDesc && resultType != leftType)
            {
                return;
            }
            if (rightType == Rtti.UTypeDescGetter<Vector2i>.TypeDesc)
            {
                Vector2i rValue = Vector2i.One;
                if (right != (void*)0)
                {
                    rValue = *(Vector2i*)right;
                }
                (*(Vector2i*)result) = (*(Vector2i*)left) * rValue;
            }
            else if (rightType == Rtti.UTypeDescGetter<int>.TypeDesc)
            {
                int rValue = 1;
                if (right != (void*)0)
                {
                    rValue = *(int*)right;
                }
                (*(Vector2i*)result) = (*(Vector2i*)left) * rValue;
            }
        }
        public unsafe void Div(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right)
        {
            if (resultType != Rtti.UTypeDescGetter<Vector2i>.TypeDesc && resultType != leftType && resultType != rightType)
            {
                return;
            }
            Vector2i rValue = Vector2i.One;
            if (right != (void*)0)
            {
                rValue = *(Vector2i*)right;
            }
            (*(Vector2i*)result) = (*(Vector2i*)left) / rValue;
        }
        public unsafe void Lerp(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right, float factor)
        {
            if (resultType != Rtti.UTypeDescGetter<Vector2i>.TypeDesc && resultType != leftType && resultType != rightType)
            {
                return;
            }
            var rValue = Vector2i.One;
            if (right != (void*)0)
            {
                rValue = *(Vector2i*)right;
            }
            var l =(Vector2i*)left;
            var r = (Vector2i*)result;
            r->X = (int)MathHelper.Lerp((float)l->X, (float)rValue.X, factor);
            r->Y = (int)MathHelper.Lerp((float)l->Y, (float)rValue.Y, factor);
        }
        public unsafe void Max(void* result, void* left, void* right)
        {
            ref var l = ref *(Vector2i*)left;
            ref var r = ref *(Vector2i*)right;
            ref var t = ref *(Vector2i*)result;

            t = Vector2i.Maximize(l, r);
        }
        public unsafe void Min(void* result, void* left, void* right)
        {
            ref var l = ref *(Vector2i*)left;
            ref var r = ref *(Vector2i*)right;
            ref var t = ref *(Vector2i*)result;

            t = Vector2i.Minimize(l, r);
        }
        public unsafe void Abs(void* result, void* left)
        {
            ref var l = ref *(Vector2i*)left;
            ref var t = ref *(Vector2i*)result;
            t.X = MathHelper.Abs(l.X);
            t.Y = MathHelper.Abs(l.Y);
        }
    }
    public struct FInt3Operator : ISuperPixelOperator<Vector3i>
    {
        public Rtti.UTypeDesc ElementType
        {
            get
            {
                return Rtti.UTypeDescGetter<Vector3i>.TypeDesc;
            }
        }
        public Rtti.UTypeDesc BufferType
        {
            get
            {
                return Rtti.UTypeDescGetter<USuperBuffer<Vector3i, FInt3Operator>>.TypeDesc;
            }
        }
        public Vector3i MaxValue { get => Vector3i.MaxValue; }
        public Vector3i MinValue { get => Vector3i.MinValue; }
        public unsafe int Compare(void* left, void* right)
        {
            ref var l = ref *(Vector3i*)left;
            ref var r = ref *(Vector3i*)right;
            return Compare(in l, in r);
        }
        public int Compare(in Vector3i left, in Vector3i right)
        {
            //if (left.X < right.X ||
            //    left.Y < right.Y ||
            //    left.Z < right.Z)
            //    return -1;
            //else 
            //return left.CompareTo(right);
            return 0;
        }
        public unsafe void SetIfGreateThan(Rtti.UTypeDesc tarTyp, void* tar, Rtti.UTypeDesc srcType, void* src)
        {
            ref var sVec3 = ref *(Vector3i*)src;
            ref var tVec3 = ref *(Vector3i*)tar;
            tVec3 = Vector3i.Maximize(in sVec3, in tVec3);
        }
        public unsafe void SetIfLessThan(Rtti.UTypeDesc tarTyp, void* tar, Rtti.UTypeDesc srcType, void* src)
        {
            ref var sVec3 = ref *(Vector3i*)src;
            ref var tVec3 = ref *(Vector3i*)tar;
            tVec3 = Vector3i.Minimize(in sVec3, in tVec3);
        }
        public Vector3i Add(in Vector3i left, in Vector3i right)
        {
            return left + right;
        }
        public unsafe void SetAsMaxValue(void* tar)
        {
            (*(Vector3i*)tar) = Vector3i.MaxValue;
        }
        public unsafe void SetAsMinValue(void* tar)
        {
            (*(Vector3i*)tar) = Vector3i.MinValue;
        }
        public unsafe void Copy(Rtti.UTypeDesc tarTyp, void* tar, Rtti.UTypeDesc srcType, void* src)
        {
            (*(Vector3i*)tar) = (*(Vector3i*)src);
        }
        public unsafe void Add(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right)
        {
            if (resultType != Rtti.UTypeDescGetter<Vector3i>.TypeDesc && resultType != leftType && resultType != rightType)
            {
                return;
            }
            Vector3i rValue = Vector3i.Zero;
            if (right != (void*)0)
            {
                rValue = *(Vector3i*)right;
            }
            (*(Vector3i*)result) = (*(Vector3i*)left) + rValue;
        }
        public unsafe void Sub(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right)
        {
            if (resultType != Rtti.UTypeDescGetter<Vector3i>.TypeDesc && resultType != leftType && resultType != rightType)
            {
                return;
            }
            Vector3i rValue = Vector3i.Zero;
            if (right != (void*)0)
            {
                rValue = *(Vector3i*)right;
            }
            (*(Vector3i*)result) = (*(Vector3i*)left) - rValue;
        }
        public unsafe void Mul(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right)
        {
            if (resultType != Rtti.UTypeDescGetter<Vector3i>.TypeDesc && resultType != leftType)
            {
                return;
            }
            if (rightType == Rtti.UTypeDescGetter<Vector3i>.TypeDesc)
            {
                Vector3i rValue = Vector3i.One;
                if (right != (void*)0)
                {
                    rValue = *(Vector3i*)right;
                }
                (*(Vector3i*)result) = (*(Vector3i*)left) * rValue;
            }
            else if (rightType == Rtti.UTypeDescGetter<int>.TypeDesc)
            {
                int rValue = 1;
                if (right != (void*)0)
                {
                    rValue = *(int*)right;
                }
                (*(Vector3i*)result) = (*(Vector3i*)left) * rValue;
            }
        }
        public unsafe void Div(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right)
        {
            if (resultType != Rtti.UTypeDescGetter<Vector3i>.TypeDesc && resultType != leftType && resultType != rightType)
            {
                return;
            }
            Vector3i rValue = Vector3i.One;
            if (right != (void*)0)
            {
                rValue = *(Vector3i*)right;
            }
            (*(Vector3i*)result) = (*(Vector3i*)left) / rValue;
        }
        public unsafe void Lerp(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right, float factor)
        {
            if (resultType != Rtti.UTypeDescGetter<Vector3i>.TypeDesc && resultType != leftType && resultType != rightType)
            {
                return;
            }
            var rValue = Vector3i.One;
            if (right != (void*)0)
            {
                rValue = *(Vector3i*)right;
            }
            var l = (Vector3i*)left;
            var r = (Vector3i*)result;
            r->X = (int)MathHelper.Lerp((float)l->X, (float)rValue.X, factor);
            r->Y = (int)MathHelper.Lerp((float)l->Y, (float)rValue.Y, factor);
            r->Z = (int)MathHelper.Lerp((float)l->Z, (float)rValue.Z, factor);
        }
        public unsafe void Max(void* result, void* left, void* right)
        {
            ref var l = ref *(Vector3i*)left;
            ref var r = ref *(Vector3i*)right;
            ref var t = ref *(Vector3i*)result;

            t = Vector3i.Maximize(l, r);
        }
        public unsafe void Min(void* result, void* left, void* right)
        {
            ref var l = ref *(Vector3i*)left;
            ref var r = ref *(Vector3i*)right;
            ref var t = ref *(Vector3i*)result;

            t = Vector3i.Minimize(l, r);
        }
        public unsafe void Abs(void* result, void* left)
        {
            ref var l = ref *(Vector3i*)left;
            ref var t = ref *(Vector3i*)result;
            t.X = MathHelper.Abs(l.X);
            t.Y = MathHelper.Abs(l.Y);
            t.Z = MathHelper.Abs(l.Z);
        }
    }

    public struct FDouble3Operator : ISuperPixelOperator<DVector3>
    {
        public Rtti.UTypeDesc ElementType
        {
            get
            {
                return Rtti.UTypeDescGetter<DVector3>.TypeDesc;
            }
        }
        public Rtti.UTypeDesc BufferType
        {
            get
            {
                return Rtti.UTypeDescGetter<USuperBuffer<DVector3, FDouble3Operator>>.TypeDesc;
            }
        }
        public DVector3 MaxValue { get => DVector3.MaxValue; }
        public DVector3 MinValue { get => DVector3.MinValue; }
        public unsafe int Compare(void* left, void* right)
        {
            ref var l = ref *(DVector3*)left;
            ref var r = ref *(DVector3*)right;
            return Compare(in l, in r);
        }
        public int Compare(in DVector3 left, in DVector3 right)
        {
            //if (left.X < right.X ||
            //    left.Y < right.Y ||
            //    left.Z < right.Z)
            //    return -1;
            //else 
            //return left.CompareTo(right);
            return 0;
        }
        public unsafe void SetIfGreateThan(Rtti.UTypeDesc tarTyp, void* tar, Rtti.UTypeDesc srcType, void* src)
        {
            ref var sVec3 = ref *(DVector3*)src;
            ref var tVec3 = ref *(DVector3*)tar;
            tVec3 = DVector3.Maximize(in sVec3, in tVec3);
        }
        public unsafe void SetIfLessThan(Rtti.UTypeDesc tarTyp, void* tar, Rtti.UTypeDesc srcType, void* src)
        {
            ref var sVec3 = ref *(DVector3*)src;
            ref var tVec3 = ref *(DVector3*)tar;
            tVec3 = DVector3.Minimize(in sVec3, in tVec3);
        }
        public DVector3 Add(in DVector3 left, in DVector3 right)
        {
            return left + right;
        }
        public unsafe void SetAsMaxValue(void* tar)
        {
            (*(DVector3*)tar) = DVector3.MaxValue;
        }
        public unsafe void SetAsMinValue(void* tar)
        {
            (*(DVector3*)tar) = DVector3.MinValue;
        }
        public unsafe void Copy(Rtti.UTypeDesc tarTyp, void* tar, Rtti.UTypeDesc srcType, void* src)
        {
            (*(DVector3*)tar) = (*(DVector3*)src);
        }
        public unsafe void Add(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right)
        {
            if (resultType != Rtti.UTypeDescGetter<DVector3>.TypeDesc && resultType != leftType && resultType != rightType)
            {
                return;
            }
            DVector3 rValue = DVector3.Zero;
            if (right != (void*)0)
            {
                rValue = *(DVector3*)right;
            }
            (*(DVector3*)result) = (*(DVector3*)left) + rValue;
        }
        public unsafe void Sub(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right)
        {
            if (resultType != Rtti.UTypeDescGetter<DVector3>.TypeDesc && resultType != leftType && resultType != rightType)
            {
                return;
            }
            DVector3 rValue = DVector3.Zero;
            if (right != (void*)0)
            {
                rValue = *(DVector3*)right;
            }
            (*(DVector3*)result) = (*(DVector3*)left) - rValue;
        }
        public unsafe void Mul(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right)
        {
            if (resultType != Rtti.UTypeDescGetter<DVector3>.TypeDesc && resultType != leftType)
            {
                return;
            }
            if (rightType == Rtti.UTypeDescGetter<DVector3>.TypeDesc)
            {
                DVector3 rValue = DVector3.One;
                if (right != (void*)0)
                {
                    rValue = *(DVector3*)right;
                }
                (*(DVector3*)result) = (*(DVector3*)left) * rValue;
            }
            else if (rightType == Rtti.UTypeDescGetter<float>.TypeDesc)
            {
                float rValue = 1.0f;
                if (right != (void*)0)
                {
                    rValue = *(float*)right;
                }
                (*(DVector3*)result) = (*(DVector3*)left) * rValue;
            }
        }
        public unsafe void Div(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right)
        {
            if (resultType != Rtti.UTypeDescGetter<DVector3>.TypeDesc && resultType != leftType && resultType != rightType)
            {
                return;
            }
            DVector3 rValue = DVector3.One;
            if (right != (void*)0)
            {
                rValue = *(DVector3*)right;
            }
            (*(DVector3*)result) = (*(DVector3*)left) / rValue;
        }
        public unsafe void Lerp(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right, float factor)
        {
            if (resultType != Rtti.UTypeDescGetter<DVector3>.TypeDesc && resultType != leftType && resultType != rightType)
            {
                return;
            }
            var rValue = DVector3.One;
            if (right != (void*)0)
            {
                rValue = *(DVector3*)right;
            }
            *(DVector3*)result = DVector3.Lerp(*(DVector3*)left, rValue, factor);
        }
        public unsafe void Max(void* result, void* left, void* right)
        {
            ref var l = ref *(DVector3*)left;
            ref var r = ref *(DVector3*)right;
            ref var t = ref *(DVector3*)result;

            t = DVector3.Maximize(l, r);
        }
        public unsafe void Min(void* result, void* left, void* right)
        {
            ref var l = ref *(DVector3*)left;
            ref var r = ref *(DVector3*)right;
            ref var t = ref *(DVector3*)result;

            t = DVector3.Minimize(l, r);
        }
        public unsafe void Abs(void* result, void* left)
        {
            ref var l = ref *(DVector3*)left;
            ref var t = ref *(DVector3*)result;
            t.X = MathHelper.Abs(l.X);
            t.Y = MathHelper.Abs(l.Y);
            t.Z = MathHelper.Abs(l.Z);
        }
    }
    public struct FByteOperator : ISuperPixelOperator<byte>
    {
        public Rtti.UTypeDesc ElementType
        {
            get
            {
                return Rtti.UTypeDescGetter<byte>.TypeDesc;
            }
        }
        public Rtti.UTypeDesc BufferType
        {
            get
            {
                return Rtti.UTypeDescGetter<USuperBuffer<byte, FByteOperator>>.TypeDesc;
            }
        }
        public byte MaxValue { get => byte.MaxValue; }
        public byte MinValue { get => byte.MinValue; }
        public unsafe int Compare(void* left, void* right)
        {
            ref var l = ref *(byte*)left;
            ref var r = ref *(byte*)right;
            return l.CompareTo(r);
        }
        public int Compare(in byte left, in byte right)
        {
            return left.CompareTo(right);
        }
        public unsafe void SetIfGreateThan(Rtti.UTypeDesc tarTyp, void* tar, Rtti.UTypeDesc srcType, void* src)
        {
            if (*(byte*)src > *(byte*)tar)
            {
                *(byte*)tar = *(byte*)src;
            }
        }
        public unsafe void SetIfLessThan(Rtti.UTypeDesc tarTyp, void* tar, Rtti.UTypeDesc srcType, void* src)
        {
            if (*(byte*)src < *(byte*)tar)
            {
                *(byte*)tar = *(byte*)src;
            }
        }
        public byte Add(in byte left, in byte right)
        {
            return (byte)(left + right);
        }
        public unsafe void SetAsMaxValue(void* tar)
        {
            (*(byte*)tar) = byte.MaxValue;
        }
        public unsafe void SetAsMinValue(void* tar)
        {
            (*(byte*)tar) = byte.MinValue;
        }
        public unsafe void Copy(Rtti.UTypeDesc tarTyp, void* tar, Rtti.UTypeDesc srcType, void* src)
        {
            (*(byte*)tar) = (*(byte*)src);
        }
        public unsafe void Add(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right)
        {
            if (resultType != Rtti.UTypeDescGetter<byte>.TypeDesc && resultType != leftType && resultType != rightType)
            {
                return;
            }
            byte rValue = 0;
            if (right != (void*)0)
            {
                rValue = *(byte*)right;
            }
            (*(byte*)result) = (byte)((*(byte*)left) + rValue);
        }
        public unsafe void Sub(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right)
        {
            if (resultType != Rtti.UTypeDescGetter<byte>.TypeDesc && resultType != leftType && resultType != rightType)
            {
                return;
            }
            byte rValue = 0;
            if (right != (void*)0)
            {
                rValue = *(byte*)right;
            }
            (*(byte*)result) = (byte)((*(byte*)left) - rValue);
        }
        public unsafe void Mul(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right)
        {
            if (resultType != Rtti.UTypeDescGetter<byte>.TypeDesc && resultType != leftType && resultType != rightType)
            {
                return;
            }
            byte rValue = 1;
            if (right != (void*)0)
            {
                rValue = *(byte*)right;
            }
            (*(byte*)result) = (byte)((*(byte*)left) * rValue);
        }
        public unsafe void Div(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right)
        {
            if (resultType != Rtti.UTypeDescGetter<byte>.TypeDesc && resultType != leftType && resultType != rightType)
            {
                return;
            }
            byte rValue = 1;
            if (right != (void*)0)
            {
                rValue = *(byte*)right;
            }
            (*(byte*)result) = (byte)((*(byte*)left) / rValue);
        }
        public unsafe void Lerp(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right, float factor)
        {
            if (resultType != Rtti.UTypeDescGetter<byte>.TypeDesc && resultType != leftType && resultType != rightType)
            {
                return;
            }
            byte rValue = 1;
            if (right != (void*)0)
            {
                rValue = *(byte*)right;
            }
            *(byte*)result = (byte)MathHelper.Lerp((float)*(byte*)left, (float)rValue, factor);
        }
        public unsafe void Max(void* result, void* left, void* right)
        {
            ref var l = ref *(byte*)left;
            ref var r = ref *(byte*)right;
            ref var t = ref *(byte*)result;

            t = (byte)MathHelper.Max((int)l, (int)r);
        }
        public unsafe void Min(void* result, void* left, void* right)
        {
            ref var l = ref *(byte*)left;
            ref var r = ref *(byte*)right;
            ref var t = ref *(byte*)result;

            t = (byte)MathHelper.Min((int)l, (int)r);
        }
        public unsafe void Abs(void* result, void* left)
        {
            ref var l = ref *(byte*)left;
            ref var t = ref *(byte*)result;
            t = (byte)MathHelper.Abs((int)l);
        }
    }
    public struct FSByteOperator : ISuperPixelOperator<sbyte>
    {
        public Rtti.UTypeDesc ElementType
        {
            get
            {
                return Rtti.UTypeDescGetter<sbyte>.TypeDesc;
            }
        }
        public Rtti.UTypeDesc BufferType
        {
            get
            {
                return Rtti.UTypeDescGetter<USuperBuffer<sbyte, FSByteOperator>>.TypeDesc;
            }
        }
        public sbyte MaxValue { get => sbyte.MaxValue; }
        public sbyte MinValue { get => sbyte.MinValue; }
        public unsafe int Compare(void* left, void* right)
        {
            ref var l = ref *(sbyte*)left;
            ref var r = ref *(sbyte*)right;
            return l.CompareTo(r);
        }
        public int Compare(in sbyte left, in sbyte right)
        {
            return left.CompareTo(right);
        }
        public unsafe void SetIfGreateThan(Rtti.UTypeDesc tarTyp, void* tar, Rtti.UTypeDesc srcType, void* src)
        {
            if (*(sbyte*)src > *(sbyte*)tar)
            {
                *(sbyte*)tar = *(sbyte*)src;
            }
        }
        public unsafe void SetIfLessThan(Rtti.UTypeDesc tarTyp, void* tar, Rtti.UTypeDesc srcType, void* src)
        {
            if (*(sbyte*)src < *(sbyte*)tar)
            {
                *(sbyte*)tar = *(sbyte*)src;
            }
        }
        public sbyte Add(in sbyte left, in sbyte right)
        {
            return (sbyte)(left + right);
        }
        public unsafe void SetAsMaxValue(void* tar)
        {
            (*(sbyte*)tar) = sbyte.MaxValue;
        }
        public unsafe void SetAsMinValue(void* tar)
        {
            (*(sbyte*)tar) = sbyte.MinValue;
        }
        public unsafe void Copy(Rtti.UTypeDesc tarTyp, void* tar, Rtti.UTypeDesc srcType, void* src)
        {
            (*(sbyte*)tar) = (*(sbyte*)src);
        }
        public unsafe void Add(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right)
        {
            if (resultType != Rtti.UTypeDescGetter<sbyte>.TypeDesc && resultType != leftType && resultType != rightType)
            {
                return;
            }
            sbyte rValue = 0;
            if (right != (void*)0)
            {
                rValue = *(sbyte*)right;
            }
            (*(sbyte*)result) = (sbyte)((*(sbyte*)left) + rValue);
        }
        public unsafe void Sub(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right)
        {
            if (resultType != Rtti.UTypeDescGetter<sbyte>.TypeDesc && resultType != leftType && resultType != rightType)
            {
                return;
            }
            sbyte rValue = 0;
            if (right != (void*)0)
            {
                rValue = *(sbyte*)right;
            }
            (*(sbyte*)result) = (sbyte)((*(sbyte*)left) - rValue);
        }
        public unsafe void Mul(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right)
        {
            if (resultType != Rtti.UTypeDescGetter<sbyte>.TypeDesc && resultType != leftType && resultType != rightType)
            {
                return;
            }
            sbyte rValue = 1;
            if (right != (void*)0)
            {
                rValue = *(sbyte*)right;
            }
            (*(sbyte*)result) = (sbyte)((*(sbyte*)left) * rValue);
        }
        public unsafe void Div(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right)
        {
            if (resultType != Rtti.UTypeDescGetter<sbyte>.TypeDesc && resultType != leftType && resultType != rightType)
            {
                return;
            }
            sbyte rValue = 1;
            if (right != (void*)0)
            {
                rValue = *(sbyte*)right;
            }
            (*(sbyte*)result) = (sbyte)((*(sbyte*)left) / rValue);
        }
        public unsafe void Lerp(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right, float factor)
        {
            if (resultType != Rtti.UTypeDescGetter<sbyte>.TypeDesc && resultType != leftType && resultType != rightType)
            {
                return;
            }
            sbyte rValue = 1;
            if (right != (void*)0)
            {
                rValue = *(sbyte*)right;
            }
            *(sbyte*)result = (sbyte)MathHelper.Lerp((float)*(sbyte*)left, (float)rValue, factor);
        }
        public unsafe void Max(void* result, void* left, void* right)
        {
            ref var l = ref *(sbyte*)left;
            ref var r = ref *(sbyte*)right;
            ref var t = ref *(sbyte*)result;

            t = (sbyte)MathHelper.Max((int)l, (int)r);
        }
        public unsafe void Min(void* result, void* left, void* right)
        {
            ref var l = ref *(sbyte*)left;
            ref var r = ref *(sbyte*)right;
            ref var t = ref *(sbyte*)result;

            t = (sbyte)MathHelper.Min((int)l, (int)r);
        }
        public unsafe void Abs(void* result, void* left)
        {
            ref var l = ref *(sbyte*)left;
            ref var t = ref *(sbyte*)result;
            t = (sbyte)MathHelper.Abs((int)l);
        }
    }
    public struct FTransformOperator : ISuperPixelOperator<FTransform>
    {
        public Rtti.UTypeDesc ElementType
        {
            get
            {
                return Rtti.UTypeDescGetter<FTransform>.TypeDesc;
            }
        }
        public Rtti.UTypeDesc BufferType
        {
            get
            {
                return Rtti.UTypeDescGetter<USuperBuffer<FTransform, FTransformOperator>>.TypeDesc;
            }
        }
        public FTransform MaxValue { get => FTransform.Identity; }
        public FTransform MinValue { get => FTransform.Identity; }
        public unsafe int Compare(void* left, void* right)
        {
            ref var l = ref *(FTransform*)left;
            ref var r = ref *(FTransform*)right;
            return Compare(in l, in r);
        }
        public int Compare(in FTransform left, in FTransform right)
        {
            //if (left.X < right.X ||
            //    left.Y < right.Y ||
            //    left.Z < right.Z)
            //    return -1;
            //else 
            //return left.CompareTo(right);
            return 0;
        }
        public unsafe void SetIfGreateThan(Rtti.UTypeDesc tarTyp, void* tar, Rtti.UTypeDesc srcType, void* src)
        {
            //ref var sVec3 = ref *(Vector3i*)src;
            //ref var tVec3 = ref *(Vector3i*)tar;
            //tVec3 = Vector3i.Maximize(in sVec3, in tVec3);
        }
        public unsafe void SetIfLessThan(Rtti.UTypeDesc tarTyp, void* tar, Rtti.UTypeDesc srcType, void* src)
        {
            //ref var sVec3 = ref *(Vector3i*)src;
            //ref var tVec3 = ref *(Vector3i*)tar;
            //tVec3 = Vector3i.Minimize(in sVec3, in tVec3);
        }
        public FTransform Add(in FTransform left, in FTransform right)
        {
            FTransform result;
            FTransform.Multiply(out result, in left, in right);
            return result;
        }
        public unsafe void SetAsMaxValue(void* tar)
        {
            (*(FTransform*)tar) = FTransform.Identity;
        }
        public unsafe void SetAsMinValue(void* tar)
        {
            (*(FTransform*)tar) = FTransform.Identity;
        }
        public unsafe void Copy(Rtti.UTypeDesc tarTyp, void* tar, Rtti.UTypeDesc srcType, void* src)
        {
            (*(FTransform*)tar) = (*(FTransform*)src);
        }
        public unsafe void Add(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right)
        {
            if (resultType != Rtti.UTypeDescGetter<FTransform>.TypeDesc && resultType != leftType && resultType != rightType)
            {
                return;
            }
            FTransform rValue = FTransform.Identity;
            if (right != (void*)0)
            {
                rValue = *(FTransform*)right;
            }
            FTransform.Multiply(out *(FTransform*)result, in *(FTransform*)left, in *(FTransform*)right);
        }
        public unsafe void Sub(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right)
        {
            if (resultType != Rtti.UTypeDescGetter<FTransform>.TypeDesc && resultType != leftType && resultType != rightType)
            {
                return;
            }
            FTransform rValue = FTransform.Identity;
            if (right != (void*)0)
            {
                rValue = *(FTransform*)right;
            }
            FTransform.Multiply(out *(FTransform*)result, in *(FTransform*)left, in *(FTransform*)right);
        }
        public unsafe void Mul(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right)
        {
            if (resultType != Rtti.UTypeDescGetter<FTransform>.TypeDesc && resultType != leftType && resultType != rightType)
            {
                return;
            }
            FTransform rValue = FTransform.Identity;
            if (right != (void*)0)
            {
                rValue = *(FTransform*)right;
            }
            FTransform.Multiply(out *(FTransform*)result, in *(FTransform*)left, in *(FTransform*)right);
        }
        public unsafe void Div(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right)
        {
            if (resultType != Rtti.UTypeDescGetter<FTransform>.TypeDesc && resultType != leftType && resultType != rightType)
            {
                return;
            }
            FTransform rValue = FTransform.Identity;
            if (right != (void*)0)
            {
                rValue = *(FTransform*)right;
            }
            FTransform.Multiply(out *(FTransform*)result, in *(FTransform*)left, in *(FTransform*)right);
        }
        public unsafe void Lerp(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right, float factor)
        {
            if (resultType != Rtti.UTypeDescGetter<FTransform>.TypeDesc && resultType != leftType && resultType != rightType)
            {
                return;
            }
            var rValue = FTransform.Identity;
            if (right != (void*)0)
            {
                rValue = *(FTransform*)right;
            }
            *(FTransform*)result = *(FTransform*)left;
            ((FTransform*)result)->BlendWith(in rValue, factor);
        }
        public unsafe void Max(void* result, void* left, void* right)
        {
            ref var l = ref *(FTransform*)left;
            ref var r = ref *(FTransform*)right;
            ref var t = ref *(FTransform*)result;

            //t = FTransform.Max((int)l, (int)r);
        }
        public unsafe void Min(void* result, void* left, void* right)
        {
            ref var l = ref *(FTransform*)left;
            ref var r = ref *(FTransform*)right;
            ref var t = ref *(FTransform*)result;

            //t = (sbyte)MathHelper.Min((int)l, (int)r);
        }
        public unsafe void Abs(void* result, void* left)
        {
            ref var l = ref *(FTransform*)left;
            ref var t = ref *(FTransform*)result;
            //t = (sbyte)MathHelper.Abs((int)l);
        }
    }

    public struct FSquareSurfaceOperator : ISuperPixelOperator<FSquareSurface>
    {
        public Rtti.UTypeDesc ElementType
        {
            get
            {
                return Rtti.UTypeDescGetter<FSquareSurface>.TypeDesc;
            }
        }
        public Rtti.UTypeDesc BufferType
        {
            get
            {
                return Rtti.UTypeDescGetter<USuperBuffer<FSquareSurface, FSquareSurfaceOperator>>.TypeDesc;
            }
        }
        public FSquareSurface MaxValue { get => FSquareSurface.Identity; }
        public FSquareSurface MinValue { get => FSquareSurface.Identity; }
        public unsafe int Compare(void* left, void* right)
        {
            ref var l = ref *(FSquareSurface*)left;
            ref var r = ref *(FSquareSurface*)right;
            return Compare(in l, in r);
        }
        public int Compare(in FSquareSurface left, in FSquareSurface right)
        {
            //if (left.X < right.X ||
            //    left.Y < right.Y ||
            //    left.Z < right.Z)
            //    return -1;
            //else 
            //return left.CompareTo(right);
            return 0;
        }
        public unsafe void SetIfGreateThan(Rtti.UTypeDesc tarTyp, void* tar, Rtti.UTypeDesc srcType, void* src)
        {
            ref var sVec3 = ref *(FSquareSurface*)src;
            ref var tVec3 = ref *(FSquareSurface*)tar;
            tVec3 = FSquareSurface.Maximize(in sVec3, in tVec3);
        }
        public unsafe void SetIfLessThan(Rtti.UTypeDesc tarTyp, void* tar, Rtti.UTypeDesc srcType, void* src)
        {
            ref var sVec3 = ref *(FSquareSurface*)src;
            ref var tVec3 = ref *(FSquareSurface*)tar;
            tVec3 = FSquareSurface.Minimize(in sVec3, in tVec3);
        }
        public FSquareSurface Add(in FSquareSurface left, in FSquareSurface right)
        {
            return left;
        }
        public unsafe void SetAsMaxValue(void* tar)
        {
            (*(FSquareSurface*)tar) = FSquareSurface.Identity;
        }
        public unsafe void SetAsMinValue(void* tar)
        {
            (*(FSquareSurface*)tar) = FSquareSurface.Identity;
        }
        public unsafe void Copy(Rtti.UTypeDesc tarTyp, void* tar, Rtti.UTypeDesc srcType, void* src)
        {
            (*(FSquareSurface*)tar) = (*(FSquareSurface*)src);
        }
        public unsafe void Add(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right)
        {
            
        }
        public unsafe void Sub(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right)
        {
            
        }
        public unsafe void Mul(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right)
        {
            
        }
        public unsafe void Div(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right)
        {
            
        }
        public unsafe void Lerp(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right, float factor)
        {

        }
        public unsafe void Max(void* result, void* left, void* right)
        {
            ref var l = ref *(FSquareSurface*)left;
            ref var r = ref *(FSquareSurface*)right;
            ref var t = ref *(FSquareSurface*)result;

            //t = FTransform.Max((int)l, (int)r);
        }
        public unsafe void Min(void* result, void* left, void* right)
        {
            ref var l = ref *(FSquareSurface*)left;
            ref var r = ref *(FSquareSurface*)right;
            ref var t = ref *(FSquareSurface*)result;

            //t = (sbyte)MathHelper.Min((int)l, (int)r);
        }
        public unsafe void Abs(void* result, void* left)
        {
            ref var l = ref *(FSquareSurface*)left;
            ref var t = ref *(FSquareSurface*)result;
            //t = (sbyte)MathHelper.Abs((int)l);
        }
    }
}
