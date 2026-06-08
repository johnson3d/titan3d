#include "ClothUntangling.h"

NS_BEGIN

namespace KawaiiPhysics
{
namespace ClothUntangling
{
	void SolvePointFaceCollisions(
		const FSimSurface& InnerSurface,
		const FSimSurface& OuterSurface,
		const FParticleAccessor& GetParticle,
		float Thickness,
		float Friction,
		bool bUseLayerNormal)
	{
		v3dxVector3 layerNormal(0, 0, 0);
		if (bUseLayerNormal)
			layerNormal = ComputeSurfaceNormal(InnerSurface, GetParticle);

		// For each outer particle, check against inner triangles
		std::vector<FSimParticle*> checkedParticles;
		for (const FSimTriangle& innerTri : InnerSurface.Triangles)
		{
			FSimParticle* q0 = GetParticle(innerTri.Indices[0]);
			FSimParticle* q1 = GetParticle(innerTri.Indices[1]);
			FSimParticle* q2 = GetParticle(innerTri.Indices[2]);
			if (!q0 || !q1 || !q2) continue;

			for (const FSimTriangle& outerTri : OuterSurface.Triangles)
			{
				for (int i = 0; i < 3; ++i)
				{
					FSimParticle* p = GetParticle(outerTri.Indices[i]);
					if (!p || p->PinMode != KPM_Dynamic) continue;

					CollisionUtils::PointTriangleContact(p, q0, q1, q2, Thickness, Friction, bUseLayerNormal);
				}
			}
		}

		// For each inner particle, check against outer triangles
		for (const FSimTriangle& outerTri : OuterSurface.Triangles)
		{
			FSimParticle* q0 = GetParticle(outerTri.Indices[0]);
			FSimParticle* q1 = GetParticle(outerTri.Indices[1]);
			FSimParticle* q2 = GetParticle(outerTri.Indices[2]);
			if (!q0 || !q1 || !q2) continue;

			for (const FSimTriangle& innerTri : InnerSurface.Triangles)
			{
				for (int i = 0; i < 3; ++i)
				{
					FSimParticle* p = GetParticle(innerTri.Indices[i]);
					if (!p || p->PinMode != KPM_Dynamic) continue;

					CollisionUtils::PointTriangleContact(p, q0, q1, q2, Thickness, Friction, bUseLayerNormal);
				}
			}
		}
	}

	void SolveEdgeEdgeCollisions(
		const FSimSurface& InnerSurface,
		const FSimSurface& OuterSurface,
		const FParticleAccessor& GetParticle,
		float Thickness,
		float Friction,
		bool bUseLayerNormal)
	{
		v3dxVector3 layerNormal(0, 0, 0);
		if (bUseLayerNormal)
			layerNormal = ComputeSurfaceNormal(InnerSurface, GetParticle);

		for (const FSimEdge& innerEdge : InnerSurface.Edges)
		{
			FSimParticle* q0 = GetParticle(innerEdge.Indices[0]);
			FSimParticle* q1 = GetParticle(innerEdge.Indices[1]);
			if (!q0 || !q1) continue;

			for (const FSimEdge& outerEdge : OuterSurface.Edges)
			{
				FSimParticle* p0 = GetParticle(outerEdge.Indices[0]);
				FSimParticle* p1 = GetParticle(outerEdge.Indices[1]);
				if (!p0 || !p1) continue;

				CollisionUtils::EdgeEdgeContact(p0, p1, q0, q1, Thickness, Friction, layerNormal, bUseLayerNormal);
			}
		}
	}

	v3dxVector3 ComputeSurfaceNormal(
		const FSimSurface& Surface,
		const FParticleAccessor& GetParticle)
	{
		v3dxVector3 avgNormal(0, 0, 0);
		int validCount = 0;

		for (const FSimTriangle& tri : Surface.Triangles)
		{
			const FSimParticle* p0 = GetParticle(tri.Indices[0]);
			const FSimParticle* p1 = GetParticle(tri.Indices[1]);
			const FSimParticle* p2 = GetParticle(tri.Indices[2]);
			if (!p0 || !p1 || !p2) continue;

			v3dxVector3 normal = Vec3Cross(p1->Position - p0->Position, p2->Position - p0->Position);
			float len = Vec3Length(normal);
			if (len > KAWAII_SMALL_NUMBER)
			{
				avgNormal = avgNormal + normal * (1.0f / len);
				++validCount;
			}
		}

		if (validCount > 0)
			return Vec3SafeNormal(avgNormal);

		return v3dxVector3(0, 1, 0);
	}

} // namespace ClothUntangling
} // namespace KawaiiPhysics

NS_END
