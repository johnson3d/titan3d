#pragma once
#include "NxVector.h"

namespace NxMath
{
	template <typename Type = NxReal<NxFloat>>
	struct NxQuat : public NxVector4<Type>
	{
		using Vector3 = NxVector3<Type>;
		static constexpr NxQuat GetIdentity() 
		{
			return NxQuat(0, 0, 0, 1.0f);
		}
		inline static NxQuat RotationAxis(const Vector3& in_axis, const Type& angle)
		{
			NxQuat result;

			Vector3 axis;
			if (Vector3::Normalize(in_axis, axis) == false)
			{
				return GetIdentity();
			}

			auto half = angle * 0.5f;
			auto sin = Type::Sin(half);
			auto cos = Type::Cos(half);

			result.X = axis.X * sin;
			result.Y = axis.Y * sin;
			result.Z = axis.Z * sin;
			result.W = cos;

			return result;
		}
		inline static NxQuat Multiply(const NxQuat& left, const NxQuat& right)
		{
			NxQuat quaternion;
			auto lx = left.X;
			auto ly = left.Y;
			auto lz = left.Z;
			auto lw = left.W;
			auto rx = right.X;
			auto ry = right.Y;
			auto rz = right.Z;
			auto rw = right.W;

			quaternion.X = (rx * lw + lx * rw + ry * lz) - (rz * ly);
			quaternion.Y = (ry * lw + ly * rw + rz * lx) - (rx * lz);
			quaternion.Z = (rz * lw + lz * rw + rx * ly) - (ry * lx);
			quaternion.W = (rw * lw) - (rx * lx + ry * ly + rz * lz);

			return quaternion;
		}
		inline static NxQuat Multiply2(const NxQuat& pq1, const NxQuat& pq2)
		{
			NxQuat quaternion;
			quaternion.X = pq2.W * pq1.X + pq2.X * pq1.W + pq2.Y * pq1.Z - pq2.Z * pq1.Y;
			quaternion.Y = pq2.W * pq1.Y - pq2.X * pq1.Z + pq2.Y * pq1.W + pq2.Z * pq1.X;
			quaternion.Z = pq2.W * pq1.Z + pq2.X * pq1.Y - pq2.Y * pq1.X + pq2.Z * pq1.W;
			quaternion.W = pq2.W * pq1.W - pq2.X * pq1.X - pq2.Y * pq1.Y - pq2.Z * pq1.Z;
			return quaternion;
		}
		inline static Vector3 RotateVector3(const NxQuat& quaternion, const Vector3& vec)
		{
			return quaternion * vec;
		}
		friend static NxQuat operator *(const NxQuat& pq1, const NxQuat& pq2)
		{
			return Multiply2(pq1, pq2);
		}
		friend static Vector3 operator *(const NxQuat& rotation, const Vector3& point)
		{
			// http://people.csail.mit.edu/bkph/articles/Quaternions.pdf
			// V' = V + 2w(Q x V) + (2Q x (Q x V))
			// refactor:
			// V' = V + w(2(Q x V)) + (Q x (2(Q x V)))
			// T = 2(Q x V);
			// V' = V + w*(T) + (Q x T)

			Vector3 Q(rotation.X, rotation.Y, rotation.Z);
			auto T = 2.0f * Vector3::Cross(Q, point);
			auto Result = point + (rotation.W * T) + Vector3::Cross(Q, T);
			return Result;
		}
		inline static Vector3 UnrotateVector3(const NxQuat& quaternion, const Vector3& vec)
		{
			NxQuat invQuat;
			invQuat.X = -quaternion.X;
			invQuat.Y = -quaternion.Y;
			invQuat.Z = -quaternion.Z;
			invQuat.W = quaternion.W;
			return invQuat * vec;
		}
		inline NxQuat RotationFrowTwoVector(const Vector3& from, const Vector3& to)
		{
			auto axis = Vector3::Cross(from, to);
			auto dv = Vector3::Dot(from, to);
			auto angle = Type::ACos(dv);
			return RotationAxis(axis, angle);
		}
		/*friend static NxQuat operator *(const NxQuat& quaternion, Type scale)
		{
			NxQuat result;
			result.X = quaternion.X * scale;
			result.Y = quaternion.Y * scale;
			result.Z = quaternion.Z * scale;
			result.W = quaternion.W * scale;
			return result;
		}*/
	};
}


