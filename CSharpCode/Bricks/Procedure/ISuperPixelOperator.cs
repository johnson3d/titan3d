using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Procedure
{
    public interface ISuperPixelOperatorBase
    {
        Rtti.UTypeDesc ElementType { get; }
        Rtti.UTypeDesc BufferType { get; }
        unsafe void SetAsMaxValue(void* tar);
        unsafe void SetAsMinValue(void* tar);
        unsafe void Copy(Rtti.UTypeDesc tarTyp, void* tar, Rtti.UTypeDesc srcType, void* src);
        unsafe void SetIfGreateThan(Rtti.UTypeDesc tarTyp, void* tar, Rtti.UTypeDesc srcType, void* src);
        unsafe void SetIfLessThan(Rtti.UTypeDesc tarTyp, void* tar, Rtti.UTypeDesc srcType, void* src);
        unsafe void Add(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right);
        unsafe void Sub(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right);
        unsafe void Mul(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right);
        unsafe void Div(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right);
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
    }
}
