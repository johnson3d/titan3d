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
	};
}


