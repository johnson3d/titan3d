#pragma once
#include "../NxTransform.h"

namespace NxMath
{
	template <typename Type = NxReal<NxFloat32>>
	struct NxSphere
	{
		using ThisType = NxSphere<Type>;
		using Vector3 = NxVector3<Type>;
		using Real = Type;
		Vector3 Center;
		Real Radius;
		NxSphere(Vector3 center, Real radius)
		{
			Center = center;
			Radius = radius;
		}
		inline static EContainmentType Contains(const ThisType& sphere1, const ThisType& sphere2)
		{
			Real distance;
			Real x = sphere1.Center.X - sphere2.Center.X;
			Real y = sphere1.Center.Y - sphere2.Center.Y;
			Real z = sphere1.Center.Z - sphere2.Center.Z;

			distance = (Real::Sqrt((x * x) + (y * y) + (z * z)));
			Real radius = sphere1.Radius;
			Real radius2 = sphere2.Radius;

			if (radius + radius2 < distance)
				return EContainmentType::Disjoint;

			if (radius - radius2 < distance)
				return EContainmentType::Intersects;

			return EContainmentType::Contains;
		}
	};
}