#pragma once
#include "../KawaiiTypes.h"

NS_BEGIN

namespace KawaiiPhysics
{
	struct FSimParticle;

	namespace CollisionUtils
	{
		bool LineSegment_SphereIntersection(const v3dxVector3& SphereCenter, float SphereRadius,
			const v3dxVector3& Point1, const v3dxVector3& Point2,
			v3dxVector3& OutPointOnLine, v3dxVector3& OutPointOnCollider, float& OutRadius);

		bool LineSegment_CapsuleIntersection(const v3dxVector3& CapsulePos, const v3dxVector3& CapsuleDir,
			float CapsuleRadius, const v3dxVector3& Point1, const v3dxVector3& Point2,
			v3dxVector3& OutPointOnLine, v3dxVector3& OutPointOnCollider, float& OutRadius);

		void PushOutFromLineSegment(FSimParticle& JointA, FSimParticle& JointB,
			const v3dxVector3& PointOnLine, const v3dxVector3& PointOnCollider, float IntersectionRadius);

		void ClosestPointsOnTwoLines(const v3dxVector3& LineP1, const v3dxVector3& LineP2,
			const v3dxVector3& LineQ1, const v3dxVector3& LineQ2,
			float& tP, float& tQ, v3dxVector3& OutPointOnP, v3dxVector3& OutPointOnQ);

		bool PointTriangleContact(FSimParticle* P, FSimParticle* Q0, FSimParticle* Q1, FSimParticle* Q2,
			float Thickness, float TriangleFriction, bool bUseOuterNormal = false);

		bool EdgeEdgeContact(FSimParticle* P0, FSimParticle* P1, FSimParticle* Q0, FSimParticle* Q1,
			float Thickness, float EdgeFriction, const v3dxVector3& OuterNormal, bool bUseOuterNormal = false);

		bool PointTriangleContact(const v3dxVector3& P, const v3dxVector3& Q0, const v3dxVector3& Q1, const v3dxVector3& Q2,
			const v3dxVector3& P_Prev, const v3dxVector3& Q0_Prev, const v3dxVector3& Q1_Prev, const v3dxVector3& Q2_Prev,
			float Thickness, float TriangleFriction,
			v3dxVector3& OutP, v3dxVector3& OutQ0, v3dxVector3& OutQ1, v3dxVector3& OutQ2,
			bool bUseOuterNormal = false);

		bool EdgeEdgeContact(const v3dxVector3& P0, const v3dxVector3& P1, const v3dxVector3& Q0, const v3dxVector3& Q1,
			const v3dxVector3& P0_Prev, const v3dxVector3& P1_Prev, const v3dxVector3& Q0_Prev, const v3dxVector3& Q1_Prev,
			float Thickness, float EdgeFriction, const v3dxVector3& OuterNormal,
			v3dxVector3& OutP0, v3dxVector3& OutP1, v3dxVector3& OutQ0, v3dxVector3& OutQ1,
			bool bUseOuterNormal = false);
	}

} // namespace KawaiiPhysics

NS_END
