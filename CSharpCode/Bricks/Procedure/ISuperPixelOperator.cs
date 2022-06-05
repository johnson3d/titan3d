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
            if (resultType != Rtti.UTypeDescGetter<Vector3>.TypeDesc && resultType != leftType && resultType != rightType)
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
            if (resultType != Rtti.UTypeDescGetter<Vector3>.TypeDesc && resultType != leftType && resultType != rightType)
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
            if (resultType != Rtti.UTypeDescGetter<Vector3>.TypeDesc && resultType != leftType)
            {
                return;
            }
            if (rightType == Rtti.UTypeDescGetter<Vector3>.TypeDesc)
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
            if (resultType != Rtti.UTypeDescGetter<Vector3>.TypeDesc && resultType != leftType && resultType != rightType)
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
    }
    public struct FInt2Operator : ISuperPixelOperator<Int32_2>
    {
        public Rtti.UTypeDesc ElementType
        {
            get
            {
                return Rtti.UTypeDescGetter<Int32_2>.TypeDesc;
            }
        }
        public Rtti.UTypeDesc BufferType
        {
            get
            {
                return Rtti.UTypeDescGetter<USuperBuffer<Int32_2, FInt2Operator>>.TypeDesc;
            }
        }
        public Int32_2 MaxValue { get => Int32_2.MaxValue; }
        public Int32_2 MinValue { get => Int32_2.MinValue; }
        public int Compare(in Int32_2 left, in Int32_2 right)
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
            ref var sVec3 = ref *(Int32_2*)src;
            ref var tVec3 = ref *(Int32_2*)tar;
            tVec3 = Int32_2.Maximize(in sVec3, in tVec3);
        }
        public unsafe void SetIfLessThan(Rtti.UTypeDesc tarTyp, void* tar, Rtti.UTypeDesc srcType, void* src)
        {
            ref var sVec3 = ref *(Int32_2*)src;
            ref var tVec3 = ref *(Int32_2*)tar;
            tVec3 = Int32_2.Minimize(in sVec3, in tVec3);
        }
        public Int32_2 Add(in Int32_2 left, in Int32_2 right)
        {
            return left + right;
        }
        public unsafe void SetAsMaxValue(void* tar)
        {
            (*(Int32_2*)tar) = Int32_2.MaxValue;
        }
        public unsafe void SetAsMinValue(void* tar)
        {
            (*(Int32_2*)tar) = Int32_2.MinValue;
        }
        public unsafe void Copy(Rtti.UTypeDesc tarTyp, void* tar, Rtti.UTypeDesc srcType, void* src)
        {
            (*(Int32_2*)tar) = (*(Int32_2*)src);
        }
        public unsafe void Add(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right)
        {
            if (resultType != Rtti.UTypeDescGetter<Int32_2>.TypeDesc && resultType != leftType && resultType != rightType)
            {
                return;
            }
            Int32_2 rValue = Int32_2.Zero;
            if (right != (void*)0)
            {
                rValue = *(Int32_2*)right;
            }
            (*(Int32_2*)result) = (*(Int32_2*)left) + rValue;
        }
        public unsafe void Sub(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right)
        {
            if (resultType != Rtti.UTypeDescGetter<Int32_2>.TypeDesc && resultType != leftType && resultType != rightType)
            {
                return;
            }
            Int32_2 rValue = Int32_2.Zero;
            if (right != (void*)0)
            {
                rValue = *(Int32_2*)right;
            }
            (*(Int32_2*)result) = (*(Int32_2*)left) - rValue;
        }
        public unsafe void Mul(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right)
        {
            if (resultType != Rtti.UTypeDescGetter<Int32_2>.TypeDesc && resultType != leftType)
            {
                return;
            }
            if (rightType == Rtti.UTypeDescGetter<Int32_2>.TypeDesc)
            {
                Int32_2 rValue = Int32_2.One;
                if (right != (void*)0)
                {
                    rValue = *(Int32_2*)right;
                }
                (*(Int32_2*)result) = (*(Int32_2*)left) * rValue;
            }
            else if (rightType == Rtti.UTypeDescGetter<int>.TypeDesc)
            {
                int rValue = 1;
                if (right != (void*)0)
                {
                    rValue = *(int*)right;
                }
                (*(Int32_2*)result) = (*(Int32_2*)left) * rValue;
            }
        }
        public unsafe void Div(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right)
        {
            if (resultType != Rtti.UTypeDescGetter<Vector3>.TypeDesc && resultType != leftType && resultType != rightType)
            {
                return;
            }
            Int32_2 rValue = Int32_2.One;
            if (right != (void*)0)
            {
                rValue = *(Int32_2*)right;
            }
            (*(Int32_2*)result) = (*(Int32_2*)left) / rValue;
        }
    }
    public struct FInt3Operator : ISuperPixelOperator<Int32_3>
    {
        public Rtti.UTypeDesc ElementType
        {
            get
            {
                return Rtti.UTypeDescGetter<Int32_3>.TypeDesc;
            }
        }
        public Rtti.UTypeDesc BufferType
        {
            get
            {
                return Rtti.UTypeDescGetter<USuperBuffer<Int32_3, FInt3Operator>>.TypeDesc;
            }
        }
        public Int32_3 MaxValue { get => Int32_3.MaxValue; }
        public Int32_3 MinValue { get => Int32_3.MinValue; }
        public int Compare(in Int32_3 left, in Int32_3 right)
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
            ref var sVec3 = ref *(Int32_3*)src;
            ref var tVec3 = ref *(Int32_3*)tar;
            tVec3 = Int32_3.Maximize(in sVec3, in tVec3);
        }
        public unsafe void SetIfLessThan(Rtti.UTypeDesc tarTyp, void* tar, Rtti.UTypeDesc srcType, void* src)
        {
            ref var sVec3 = ref *(Int32_3*)src;
            ref var tVec3 = ref *(Int32_3*)tar;
            tVec3 = Int32_3.Minimize(in sVec3, in tVec3);
        }
        public Int32_3 Add(in Int32_3 left, in Int32_3 right)
        {
            return left + right;
        }
        public unsafe void SetAsMaxValue(void* tar)
        {
            (*(Int32_3*)tar) = Int32_3.MaxValue;
        }
        public unsafe void SetAsMinValue(void* tar)
        {
            (*(Int32_3*)tar) = Int32_3.MinValue;
        }
        public unsafe void Copy(Rtti.UTypeDesc tarTyp, void* tar, Rtti.UTypeDesc srcType, void* src)
        {
            (*(Int32_3*)tar) = (*(Int32_3*)src);
        }
        public unsafe void Add(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right)
        {
            if (resultType != Rtti.UTypeDescGetter<Int32_3>.TypeDesc && resultType != leftType && resultType != rightType)
            {
                return;
            }
            Int32_3 rValue = Int32_3.Zero;
            if (right != (void*)0)
            {
                rValue = *(Int32_3*)right;
            }
            (*(Int32_3*)result) = (*(Int32_3*)left) + rValue;
        }
        public unsafe void Sub(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right)
        {
            if (resultType != Rtti.UTypeDescGetter<Int32_3>.TypeDesc && resultType != leftType && resultType != rightType)
            {
                return;
            }
            Int32_3 rValue = Int32_3.Zero;
            if (right != (void*)0)
            {
                rValue = *(Int32_3*)right;
            }
            (*(Int32_3*)result) = (*(Int32_3*)left) - rValue;
        }
        public unsafe void Mul(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right)
        {
            if (resultType != Rtti.UTypeDescGetter<Int32_3>.TypeDesc && resultType != leftType)
            {
                return;
            }
            if (rightType == Rtti.UTypeDescGetter<Int32_3>.TypeDesc)
            {
                Int32_3 rValue = Int32_3.One;
                if (right != (void*)0)
                {
                    rValue = *(Int32_3*)right;
                }
                (*(Int32_3*)result) = (*(Int32_3*)left) * rValue;
            }
            else if (rightType == Rtti.UTypeDescGetter<int>.TypeDesc)
            {
                int rValue = 1;
                if (right != (void*)0)
                {
                    rValue = *(int*)right;
                }
                (*(Int32_3*)result) = (*(Int32_3*)left) * rValue;
            }
        }
        public unsafe void Div(Rtti.UTypeDesc resultType, void* result, Rtti.UTypeDesc leftType, void* left, Rtti.UTypeDesc rightType, void* right)
        {
            if (resultType != Rtti.UTypeDescGetter<Vector3>.TypeDesc && resultType != leftType && resultType != rightType)
            {
                return;
            }
            Int32_3 rValue = Int32_3.One;
            if (right != (void*)0)
            {
                rValue = *(Int32_3*)right;
            }
            (*(Int32_3*)result) = (*(Int32_3*)left) / rValue;
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
            if (resultType != Rtti.UTypeDescGetter<Vector3>.TypeDesc && resultType != leftType && resultType != rightType)
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
            if (resultType != Rtti.UTypeDescGetter<Vector3>.TypeDesc && resultType != leftType)
            {
                return;
            }
            if (rightType == Rtti.UTypeDescGetter<Vector3>.TypeDesc)
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
            if (resultType != Rtti.UTypeDescGetter<Vector3>.TypeDesc && resultType != leftType && resultType != rightType)
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
            ref var sVec3 = ref *(Int32_3*)src;
            ref var tVec3 = ref *(Int32_3*)tar;
            tVec3 = Int32_3.Maximize(in sVec3, in tVec3);
        }
        public unsafe void SetIfLessThan(Rtti.UTypeDesc tarTyp, void* tar, Rtti.UTypeDesc srcType, void* src)
        {
            ref var sVec3 = ref *(Int32_3*)src;
            ref var tVec3 = ref *(Int32_3*)tar;
            tVec3 = Int32_3.Minimize(in sVec3, in tVec3);
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
    }
}
