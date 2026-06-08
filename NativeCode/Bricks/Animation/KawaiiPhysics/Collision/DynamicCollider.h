#pragma once
#include "ColliderBase.h"
#include "CollisionUtils.h"
#include "../Solvers/SimParticle.h"

NS_BEGIN

namespace KawaiiPhysics
{
	// =====================================================================
	// FSphereCollider
	// =====================================================================

	struct FSphereCollider : public FColliderBase
	{
		v3dxVector3 Center = v3dxVector3(0, 0, 0);
		float Radius = 5.0f;

		FSphereCollider() { ColliderType = KCT_Sphere; }

		FKawaiiAABB CalcCurrentBoundBox() override
		{
			return FKawaiiAABB::BuildAABB(Center, Radius);
		}

		bool OnPointCollision(FSimParticle& Joint, float CollisionSubStep, uint32_t InAnimUid,
			float RadiusIncreaseCoefficient) const override
		{
			if (Joint.PinMode != KPM_Dynamic) return false;
			const float TotalRadius = Radius * RadiusIncreaseCoefficient + Joint.PhysicsSettings.Radius;
			const v3dxVector3 Delta = Joint.Position - Center;
			const float DistSqr = Vec3LengthSqr(Delta);

			if (DistSqr < TotalRadius * TotalRadius)
			{
				const float Dist = sqrtf(DistSqr);
				if (Dist < KAWAII_SMALL_NUMBER) return false;
				const v3dxVector3 Normal = Delta * (1.0f / Dist);
				Joint.Position = Center + Normal * TotalRadius;
				Joint.ContactNormal = Normal;
				Joint.Friction = std::max(Joint.Friction, 0.0f);
				return true;
			}
			return false;
		}

		bool OnLineCollision(FSimParticle& JointA, FSimParticle& JointB, float CollisionSubStep, uint32_t InAnimUid) const override
		{
			v3dxVector3 PointOnLine, PointOnCollider;
			float OutRadius;
			if (CollisionUtils::LineSegment_SphereIntersection(Center, Radius, JointA.Position, JointB.Position,
				PointOnLine, PointOnCollider, OutRadius))
			{
				CollisionUtils::PushOutFromLineSegment(JointA, JointB, PointOnLine, PointOnCollider, OutRadius);
				return true;
			}
			return false;
		}
	};

	// =====================================================================
	// FCapsuleCollider
	// =====================================================================

	struct FCapsuleCollider : public FColliderBase
	{
		v3dxVector3 Center = v3dxVector3(0, 0, 0);
		v3dxVector3 Direction = v3dxVector3(0, 0, 1);
		float Radius = 5.0f;
		float HalfLength = 10.0f;

		FCapsuleCollider() { ColliderType = KCT_Capsule; }

		v3dxVector3 GetStartPoint() const { return Center - Direction * HalfLength; }
		v3dxVector3 GetEndPoint() const { return Center + Direction * HalfLength; }
		v3dxVector3 GetAxisVector() const { return Direction * HalfLength * 2.0f; }

		FKawaiiAABB CalcCurrentBoundBox() override
		{
			FKawaiiAABB box;
			v3dxVector3 start = GetStartPoint();
			v3dxVector3 end = GetEndPoint();
			box.Expand(start);
			box.Expand(end);
			box.ExpandByRadius(Radius);
			return box;
		}

		bool OnPointCollision(FSimParticle& Joint, float CollisionSubStep, uint32_t InAnimUid,
			float RadiusIncreaseCoefficient) const override
		{
			if (Joint.PinMode != KPM_Dynamic) return false;
			const float TotalRadius = Radius * RadiusIncreaseCoefficient + Joint.PhysicsSettings.Radius;

			v3dxVector3 start = GetStartPoint();
			v3dxVector3 end = GetEndPoint();
			v3dxVector3 axisDir = Vec3SafeNormal(end - start);
			float axisLen = Vec3Length(end - start);

			float t = Vec3Dot(Joint.Position - start, axisDir);
			t = KawaiiClamp(t, 0.0f, axisLen);
			v3dxVector3 closestOnAxis = start + axisDir * t;

			v3dxVector3 delta = Joint.Position - closestOnAxis;
			float distSqr = Vec3LengthSqr(delta);

			if (distSqr < TotalRadius * TotalRadius)
			{
				float dist = sqrtf(distSqr);
				if (dist < KAWAII_SMALL_NUMBER) return false;
				v3dxVector3 normal = delta * (1.0f / dist);
				Joint.Position = closestOnAxis + normal * TotalRadius;
				Joint.ContactNormal = normal;
				return true;
			}
			return false;
		}

		bool OnLineCollision(FSimParticle& JointA, FSimParticle& JointB, float CollisionSubStep, uint32_t InAnimUid) const override
		{
			v3dxVector3 PointOnLine, PointOnCollider;
			float OutRadius;
			if (CollisionUtils::LineSegment_CapsuleIntersection(GetStartPoint(), GetAxisVector(),
				Radius, JointA.Position, JointB.Position, PointOnLine, PointOnCollider, OutRadius))
			{
				CollisionUtils::PushOutFromLineSegment(JointA, JointB, PointOnLine, PointOnCollider, OutRadius);
				return true;
			}
			return false;
		}
	};

	// =====================================================================
	// FPlaneCollider
	// =====================================================================

	struct FPlaneCollider : public FColliderBase
	{
		v3dxVector3 Origin = v3dxVector3(0, 0, 0);
		v3dxVector3 Normal = v3dxVector3(0, 1, 0);

		FPlaneCollider() { ColliderType = KCT_Plane; }

		FKawaiiAABB CalcCurrentBoundBox() override
		{
			// Infinite plane - use a large box
			return FKawaiiAABB(
				v3dxVector3(-1e6f, -1e6f, -1e6f),
				v3dxVector3(1e6f, 1e6f, 1e6f));
		}

		bool OnPointCollision(FSimParticle& Joint, float CollisionSubStep, uint32_t InAnimUid,
			float RadiusIncreaseCoefficient) const override
		{
			if (Joint.PinMode != KPM_Dynamic) return false;
			const float ParticleRadius = Joint.PhysicsSettings.Radius;
			const float SignedDist = Vec3Dot(Joint.Position - Origin, Normal);

			if (SignedDist < ParticleRadius)
			{
				Joint.Position = Joint.Position + Normal * (ParticleRadius - SignedDist);
				Joint.ContactNormal = Normal;
				return true;
			}
			return false;
		}
	};

	// =====================================================================
	// FBoxCollider
	// =====================================================================

	struct FBoxCollider : public FColliderBase
	{
		v3dxVector3 Center = v3dxVector3(0, 0, 0);
		v3dxQuaternion Rotation = v3dxQuaternion(0, 0, 0, 1);
		v3dxVector3 HalfExtent = v3dxVector3(5, 5, 5);

		FBoxCollider() { ColliderType = KCT_Box; }

		FKawaiiAABB CalcCurrentBoundBox() override
		{
			FKawaiiAABB box;
			// Calculate 8 corners
			for (int i = 0; i < 8; ++i)
			{
				v3dxVector3 corner(
					(i & 1) ? HalfExtent.X : -HalfExtent.X,
					(i & 2) ? HalfExtent.Y : -HalfExtent.Y,
					(i & 4) ? HalfExtent.Z : -HalfExtent.Z
				);
				corner = QuatRotateVector(Rotation, corner) + Center;
				box.Expand(corner);
			}
			return box;
		}

		bool OnPointCollision(FSimParticle& Joint, float CollisionSubStep, uint32_t InAnimUid,
			float RadiusIncreaseCoefficient) const override
		{
			if (Joint.PinMode != KPM_Dynamic) return false;

			// Transform to local box space
			v3dxQuaternion invRot = QuatInverse(Rotation);
			v3dxVector3 localPos = QuatRotateVector(invRot, Joint.Position - Center);
			float particleRadius = Joint.PhysicsSettings.Radius;
			v3dxVector3 expandedHalf = HalfExtent + v3dxVector3(particleRadius, particleRadius, particleRadius);

			// Check if inside expanded box
			if (fabsf(localPos.X) > expandedHalf.X || fabsf(localPos.Y) > expandedHalf.Y || fabsf(localPos.Z) > expandedHalf.Z)
				return false;

			// Find closest face
			float minPen = FLT_MAX;
			v3dxVector3 localNormal(0, 0, 0);

			float penX = expandedHalf.X - fabsf(localPos.X);
			float penY = expandedHalf.Y - fabsf(localPos.Y);
			float penZ = expandedHalf.Z - fabsf(localPos.Z);

			if (penX < minPen) { minPen = penX; localNormal = v3dxVector3(localPos.X > 0 ? 1.0f : -1.0f, 0, 0); }
			if (penY < minPen) { minPen = penY; localNormal = v3dxVector3(0, localPos.Y > 0 ? 1.0f : -1.0f, 0); }
			if (penZ < minPen) { minPen = penZ; localNormal = v3dxVector3(0, 0, localPos.Z > 0 ? 1.0f : -1.0f); }

			v3dxVector3 worldNormal = QuatRotateVector(Rotation, localNormal);
			Joint.Position = Joint.Position + worldNormal * minPen;
			Joint.ContactNormal = worldNormal;
			return true;
		}
	};

} // namespace KawaiiPhysics

NS_END
