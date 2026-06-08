#pragma once
#include "ColliderBase.h"
#include "CollisionUtils.h"
#include "../Solvers/SimParticle.h"
#include <vector>

NS_BEGIN

namespace KawaiiPhysics
{
	// Triangle mesh collider for complex geometry collision
	struct FMeshTriangle
	{
		v3dxVector3 V0, V1, V2;
		v3dxVector3 Normal;

		void CalcNormal()
		{
			Normal = Vec3SafeNormal(Vec3Cross(V1 - V0, V2 - V0));
		}
	};

	struct FMeshCollider : public FColliderBase
	{
		std::vector<FMeshTriangle> Triangles;
		FKawaiiAABB MeshBounds;

		FMeshCollider() { ColliderType = KCT_Mesh; }

		void BuildFromVertices(const std::vector<v3dxVector3>& Vertices, const std::vector<uint32_t>& Indices)
		{
			Triangles.clear();
			MeshBounds = FKawaiiAABB();

			for (size_t i = 0; i + 2 < Indices.size(); i += 3)
			{
				FMeshTriangle tri;
				tri.V0 = Vertices[Indices[i]];
				tri.V1 = Vertices[Indices[i + 1]];
				tri.V2 = Vertices[Indices[i + 2]];
				tri.CalcNormal();
				Triangles.push_back(tri);

				MeshBounds.Expand(tri.V0);
				MeshBounds.Expand(tri.V1);
				MeshBounds.Expand(tri.V2);
			}
		}

		FKawaiiAABB CalcCurrentBoundBox() override { return MeshBounds; }

		bool OnPointCollision(FSimParticle& Joint, float CollisionSubStep, uint32_t InAnimUid,
			float RadiusIncreaseCoefficient) const override
		{
			if (Joint.PinMode != KPM_Dynamic) return false;

			const float ParticleRadius = Joint.PhysicsSettings.Radius;
			bool anyCollision = false;

			for (const FMeshTriangle& Tri : Triangles)
			{
				const v3dxVector3 ToPoint = Joint.Position - Tri.V0;
				const float SignedDist = Vec3Dot(ToPoint, Tri.Normal);

				if (fabsf(SignedDist) > ParticleRadius) continue;

				// Project point onto triangle plane
				const v3dxVector3 Projected = Joint.Position - Tri.Normal * SignedDist;

				// Barycentric test
				const v3dxVector3 E0 = Tri.V1 - Tri.V0;
				const v3dxVector3 E1 = Tri.V2 - Tri.V0;
				const v3dxVector3 V2P = Projected - Tri.V0;

				const float D00 = Vec3Dot(E0, E0);
				const float D01 = Vec3Dot(E0, E1);
				const float D11 = Vec3Dot(E1, E1);
				const float D20 = Vec3Dot(V2P, E0);
				const float D21 = Vec3Dot(V2P, E1);
				const float Denom = D00 * D11 - D01 * D01;
				if (fabsf(Denom) < KAWAII_SMALL_NUMBER) continue;

				const float U = (D11 * D20 - D01 * D21) / Denom;
				const float V = (D00 * D21 - D01 * D20) / Denom;

				if (U < 0.0f || V < 0.0f || (U + V) > 1.0f) continue;

				// Push out
				const float PushDist = ParticleRadius - SignedDist;
				Joint.Position = Joint.Position + Tri.Normal * PushDist;
				Joint.ContactNormal = Tri.Normal;
				Joint.Friction = std::max(Joint.Friction, 0.2f);
				anyCollision = true;
			}
			return anyCollision;
		}
	};

} // namespace KawaiiPhysics

NS_END
