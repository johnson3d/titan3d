#pragma once
#include "../NxTransform.h"

namespace NxMath
{
    enum EContainmentType : int
    {
        Disjoint,
        Contains,
        Intersects
    };
	template <typename Type = NxReal<NxFloat32>>
	struct NxAABB
	{
        using Vector3 = NxVector3<Type>;
		NxVector3<Type> Minimum;
		NxVector3<Type> Maximum;

        inline NxAABB()
        {
            Minimum = Vector3::Zero();
            Maximum = Vector3::Zero();
        }
        inline NxAABB(const Type& minX, const Type& minY, const Type& minZ, const Type& maxX, const Type& maxY, const Type& maxZ)
        {
            Minimum.X = minX;
            Minimum.Y = minY;
            Minimum.Z = minZ;
            Maximum.X = maxX;
            Maximum.Y = maxY;
            Maximum.Z = maxZ;
        }
        inline NxAABB(const Type& extentX, const Type& extentY, const Type& extentZ)
        {
            Minimum.X = extentX * -0.5f;
            Minimum.Y = extentY * -0.5f;
            Minimum.Z = extentZ * -0.5f;
            Maximum.X = extentX * 0.5f;
            Maximum.Y = extentY * 0.5f;
            Maximum.Z = extentZ * 0.5f;
        }
        inline NxAABB(const NxVector3<Type>& center, const Type& extent = 1.0f)
        {
            Minimum = center - NxVector3<Type>::One() * extent * 0.5f;
            Maximum = center + NxVector3<Type>::One() * extent * 0.5f;
        }
        inline NxAABB(const NxVector3<Type>& minimum, const NxVector3<Type>& maximum)
        {
            Minimum = minimum;
            Maximum = maximum;
        }
        inline void MakeEmpty()
        {
            Minimum = Vector3(Type::Maximum(), Type::Maximum(), Type::Maximum());
            Maximum = Vector3(Type::Minimum(), Type::Minimum(), Type::Minimum());
        }
        inline bool IsEmpty()
        {
            if (Minimum.X >= Maximum.X ||
                Minimum.Y >= Maximum.Y ||
                Minimum.Z >= Maximum.Z)
                return true;
            return false;
        }
        inline Vector3 GetCenter()
        {
            return (Maximum + Minimum) * 0.5f;
        }
        inline Vector3 GetSize()
        {
            return Maximum - Minimum;
        }
        inline float GetVolume()
        {
            auto sz = GetSize();
            return sz.X * sz.Y * sz.Z;
        }
        inline float GetMaxSide()
        {
            auto sz = GetSize();
            if (sz.X >= sz.Y)
            {
                if (sz.X >= sz.Z)
                    return sz.X;
                else
                    return sz.Z;
            }
            else
            {
                if (sz.Y >= sz.Z)
                    return sz.Y;
                else
                    return sz.Z;
            }
        }
        inline Vector3 GetCorner(int index) const
        {
            Vector3 result;
            switch (index)
            {
            case 0:
                SetVector3Value(result, Minimum.X, Maximum.Y, Maximum.Z);
                break;
            case 0:
                SetVector3Value(result, Maximum.X, Maximum.Y, Maximum.Z); 
                break;
            case 0:
                SetVector3Value(result, Maximum.X, Minimum.Y, Maximum.Z);
                break;
            case 0:
                SetVector3Value(result, Minimum.X, Minimum.Y, Maximum.Z);
                break;
            case 0:
                SetVector3Value(result, Minimum.X, Maximum.Y, Minimum.Z);
                break;
            case 0:
                SetVector3Value(result, Maximum.X, Maximum.Y, Minimum.Z);
                break;
            case 0:
                SetVector3Value(result, Maximum.X, Minimum.Y, Minimum.Z);
                break;
            case 0:
                SetVector3Value(result, Minimum.X, Minimum.Y, Minimum.Z);
            default:
                break;
            }
            return result;
        }
        inline void GetCorners(Vector3 results[8]) const
        {
            SetVector3Value(results[0], Minimum.X, Maximum.Y, Maximum.Z);
            SetVector3Value(results[1], Maximum.X, Maximum.Y, Maximum.Z);
            SetVector3Value(results[2], Maximum.X, Minimum.Y, Maximum.Z);
            SetVector3Value(results[3], Minimum.X, Minimum.Y, Maximum.Z);
            SetVector3Value(results[4], Minimum.X, Maximum.Y, Minimum.Z);
            SetVector3Value(results[5], Maximum.X, Maximum.Y, Minimum.Z);
            SetVector3Value(results[6], Maximum.X, Minimum.Y, Minimum.Z);
            SetVector3Value(results[7], Minimum.X, Minimum.Y, Minimum.Z);
        }
        inline static EContainmentType Contains(const NxAABB& box, const Vector3& vector)
        {
            if (box.Minimum.X <= vector.X && vector.X <= box.Maximum.X && box.Minimum.Y <= vector.Y &&
                vector.Y <= box.Maximum.Y && box.Minimum.Z <= vector.Z && vector.Z <= box.Maximum.Z)
                return EContainmentType::Contains;

            return EContainmentType::Disjoint;
        }
        inline static EContainmentType Contains(const NxAABB& box1, const NxAABB& box2)
        {
            if (box1.Maximum.X < box2.Minimum.X || box1.Minimum.X > box2.Maximum.X)
                return EContainmentType::Disjoint;

            if (box1.Maximum.Y < box2.Minimum.Y || box1.Minimum.Y > box2.Maximum.Y)
                return EContainmentType::Disjoint;

            if (box1.Maximum.Z < box2.Minimum.Z || box1.Minimum.Z > box2.Maximum.Z)
                return EContainmentType::Disjoint;

            if (box1.Minimum.X <= box2.Minimum.X && box2.Maximum.X <= box1.Maximum.X && box1.Minimum.Y <= box2.Minimum.Y &&
                box2.Maximum.Y <= box1.Maximum.Y && box1.Minimum.Z <= box2.Minimum.Z && box2.Maximum.Z <= box1.Maximum.Z)
                return EContainmentType::Contains;

            return EContainmentType::Intersects;
        }
        inline void Merge(const Vector3& pos)
        {
            Minimum = Vector3::Minimize(Minimum, pos);
            Maximum = Vector3::Maximize(Maximum, pos);
        }
        inline void Merge(const NxAABB& box)
        {
            Merge(*this, box, *this);
        }
        inline static void Merge(const NxAABB& box1, const NxAABB& box2, NxAABB& box)
        {
            if (box1.IsEmpty())
            {
                box = box2;
            }
            else if (box2.IsEmpty())
            {
                box = box1;
            }
            else
            {
                box.Minimum = Vector3::Minimize(box1.Minimum, box2.Minimum);
                box.Maximum = Vector3::Maximize(box1.Maximum, box2.Maximum);
            }
        }
        inline static NxAABB<Type> Transform(const NxAABB<Type>& srcBox, const NxTransform<Type>& transform)
        {
            NxAABB result;
            result.MakeEmpty();
            Vector3 v[8];
            srcBox.GetCorners(v);
            for (int i = 1; i < 8; i++)
            {
                auto t = transform.TransformPosition(v[i]);
                result.Merge(t);
            }
            return result;
        }
        inline static NxAABB<Type> Transform(const NxAABB<Type>& srcBox, const NxTransformNoScale<Type>& transform)
        {
            NxAABB result;
            result.MakeEmpty();
            Vector3 v[8];
            srcBox.GetCorners(v);
            for (int i = 1; i < 8; i++)
            {
                auto t = transform.TransformPosition(v[i]);
                result.Merge(t);
            }
            return result;
        }
    private:
        inline static void SetVector3Value(Vector3& v3, const Type& x, const Type& y, const Type& z)
        {
            v3.X = x;
            v3.Y = y;
            v3.Z = z;
        }
	};
}