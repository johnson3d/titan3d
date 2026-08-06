#include "CollisionUtils.h"
#include "../Solvers/SimParticle.h"
#include <cmath>

NS_BEGIN

namespace KawaiiPhysics
{
	namespace CollisionUtils
	{
		static v3dxVector3 ProjectPointToTriangle(const v3dxVector3& Point, const v3dxVector3& A,
			const v3dxVector3& B, const v3dxVector3& C, float& U, float& V, float& W)
		{
			const v3dxVector3 TriNormal = Vec3SafeNormal(Vec3Cross(B - A, C - A));
			const v3dxVector3 AToPoint = Point - A;
			const float Dist = Vec3Dot(AToPoint, TriNormal);
			const v3dxVector3 ProjectedPoint = Point - TriNormal * Dist;

			const v3dxVector3 V0 = B - A;
			const v3dxVector3 V1 = C - A;
			const v3dxVector3 V2 = ProjectedPoint - A;

			const float D00 = Vec3Dot(V0, V0);
			const float D01 = Vec3Dot(V0, V1);
			const float D11 = Vec3Dot(V1, V1);
			const float D20 = Vec3Dot(V2, V0);
			const float D21 = Vec3Dot(V2, V1);

			const float Denom = D00 * D11 - D01 * D01;
			if (fabsf(Denom) < KAWAII_SMALL_NUMBER)
			{
				U = V = W = -1.0f;
				return ProjectedPoint;
			}

			V = (D11 * D20 - D01 * D21) / Denom;
			W = (D00 * D21 - D01 * D20) / Denom;
			U = 1.0f - V - W;

			return ProjectedPoint;
		}

		bool PointTriangleContact(FSimParticle* P, FSimParticle* Q0, FSimParticle* Q1, FSimParticle* Q2,
			float Thickness, float TriangleFriction, bool bUseOuterNormal)
		{
			if (!P || !Q0 || !Q1 || !Q2) return false;

			const v3dxVector3 PrevP = P->Position;
			const v3dxVector3 PrevQ0 = Q0->Position;
			const v3dxVector3 PrevQ1 = Q1->Position;
			const v3dxVector3 PrevQ2 = Q2->Position;

			const bool bContact = PointTriangleContact(P->Position, Q0->Position, Q1->Position, Q2->Position,
				P->PrevPosition, Q0->PrevPosition, Q1->PrevPosition, Q2->PrevPosition,
				Thickness, TriangleFriction, P->Position, Q0->Position, Q1->Position, Q2->Position, bUseOuterNormal);

			if (bContact && TriangleFriction > KAWAII_SMALL_NUMBER)
			{
				auto SetFriction = [&](FSimParticle* Part, const v3dxVector3& Prev)
					{
						v3dxVector3 PushOut = Part->Position - Prev;
						if (!Vec3IsNearlyZero(PushOut))
						{
							Part->ContactNormal = Vec3SafeNormal(PushOut);
							Part->Friction = std::max(Part->Friction, TriangleFriction);
						}
					};
				SetFriction(P, PrevP);
				SetFriction(Q0, PrevQ0);
				SetFriction(Q1, PrevQ1);
				SetFriction(Q2, PrevQ2);
			}
			return bContact;
		}

		bool EdgeEdgeContact(FSimParticle* P0, FSimParticle* P1, FSimParticle* Q0, FSimParticle* Q1,
			float Thickness, float EdgeFriction, const v3dxVector3& OuterNormal, bool bUseOuterNormal)
		{
			if (!P0 || !P1 || !Q0 || !Q1) return false;

			const v3dxVector3 PrevP0 = P0->Position;
			const v3dxVector3 PrevP1 = P1->Position;
			const v3dxVector3 PrevQ0 = Q0->Position;
			const v3dxVector3 PrevQ1 = Q1->Position;

			const bool bContact = EdgeEdgeContact(P0->Position, P1->Position, Q0->Position, Q1->Position,
				P0->PrevPosition, P1->PrevPosition, Q0->PrevPosition, Q1->PrevPosition,
				Thickness, EdgeFriction, OuterNormal, P0->Position, P1->Position, Q0->Position, Q1->Position, bUseOuterNormal);

			if (bContact && EdgeFriction > KAWAII_SMALL_NUMBER)
			{
				auto SetFriction = [&](FSimParticle* Part, const v3dxVector3& Prev)
					{
						v3dxVector3 PushOut = Part->Position - Prev;
						if (!Vec3IsNearlyZero(PushOut))
						{
							Part->ContactNormal = Vec3SafeNormal(PushOut);
							Part->Friction = std::max(Part->Friction, EdgeFriction);
						}
					};
				SetFriction(P0, PrevP0);
				SetFriction(P1, PrevP1);
				SetFriction(Q0, PrevQ0);
				SetFriction(Q1, PrevQ1);
			}
			return bContact;
		}

		bool PointTriangleContact(const v3dxVector3& P, const v3dxVector3& Q0, const v3dxVector3& Q1, const v3dxVector3& Q2,
			const v3dxVector3& P_Prev, const v3dxVector3& Q0_Prev, const v3dxVector3& Q1_Prev, const v3dxVector3& Q2_Prev,
			float Thickness, float TriangleFriction,
			v3dxVector3& OutP, v3dxVector3& OutQ0, v3dxVector3& OutQ1, v3dxVector3& OutQ2,
			bool bUseOuterNormal)
		{
			OutP = P; OutQ0 = Q0; OutQ1 = Q1; OutQ2 = Q2;

			float U, V, W;
			const v3dxVector3 ProjectedPoint = ProjectPointToTriangle(P, Q0, Q1, Q2, U, V, W);
			if (U < 0.0f || V < 0.0f || W < 0.0f) return false;

			const v3dxVector3 TriOuterNormal = Vec3SafeNormal(Vec3Cross(Q2 - Q1, Q0 - Q1));
			const v3dxVector3 TriNormal = bUseOuterNormal ? TriOuterNormal : Vec3SafeNormal(P - ProjectedPoint);

			const float VerticalDistance = Vec3Dot(P - ProjectedPoint, TriNormal);
			const float VerticalDistanceAbs = fabsf(VerticalDistance);

			if (VerticalDistanceAbs > Thickness) return false;
			if (!bUseOuterNormal && VerticalDistanceAbs < KAWAII_SMALL_NUMBER) return false;

			const float PenetrationDepth = Thickness - VerticalDistance;
			const float TotalWeight = 1.0f + U * U + V * V + W * W;

			OutP = OutP + TriNormal * (1.0f / TotalWeight * PenetrationDepth);
			OutQ0 = OutQ0 - TriNormal * (U / TotalWeight * PenetrationDepth);
			OutQ1 = OutQ1 - TriNormal * (V / TotalWeight * PenetrationDepth);
			OutQ2 = OutQ2 - TriNormal * (W / TotalWeight * PenetrationDepth);

			return true;
		}

		bool EdgeEdgeContact(const v3dxVector3& P0, const v3dxVector3& P1, const v3dxVector3& Q0, const v3dxVector3& Q1,
			const v3dxVector3& P0_Prev, const v3dxVector3& P1_Prev, const v3dxVector3& Q0_Prev, const v3dxVector3& Q1_Prev,
			float Thickness, float EdgeFriction, const v3dxVector3& OuterNormal,
			v3dxVector3& OutP0, v3dxVector3& OutP1, v3dxVector3& OutQ0, v3dxVector3& OutQ1,
			bool bUseOuterNormal)
		{
			OutP0 = P0; OutP1 = P1; OutQ0 = Q0; OutQ1 = Q1;

			float tP, tQ;
			v3dxVector3 PClosestPoint, QClosestPoint;
			ClosestPointsOnTwoLines(P0, P1, Q0, Q1, tP, tQ, PClosestPoint, QClosestPoint);

			if (tP < 0.0f || tP > 1.0f || tQ < 0.0f || tQ > 1.0f)
				return false;

			const float Distance = Vec3Distance(PClosestPoint, QClosestPoint);
			if (Distance > Thickness) return false;
			if (Distance < 1e-5f && !bUseOuterNormal) return false;

			v3dxVector3 CollisionNormal = Vec3SafeNormal(PClosestPoint - QClosestPoint);
			float PushDistance = Thickness - Distance;

			if (bUseOuterNormal)
			{
				if (Vec3Dot(CollisionNormal, OuterNormal) < 0.0f)
				{
					CollisionNormal = CollisionNormal * -1.0f;
					PushDistance = Thickness + Distance;
				}
			}

			const float TotalWeight = tP * tP + (1.0f - tP) * (1.0f - tP) + tQ * tQ + (1.0f - tQ) * (1.0f - tQ);
			OutP0 = OutP0 + CollisionNormal * (PushDistance * (1.0f - tP) / TotalWeight);
			OutP1 = OutP1 + CollisionNormal * (PushDistance * tP / TotalWeight);
			OutQ0 = OutQ0 - CollisionNormal * (PushDistance * (1.0f - tQ) / TotalWeight);
			OutQ1 = OutQ1 - CollisionNormal * (PushDistance * tQ / TotalWeight);

			return true;
		}

		void ClosestPointsOnTwoLines(const v3dxVector3& LineP1, const v3dxVector3& LineP2,
			const v3dxVector3& LineQ1, const v3dxVector3& LineQ2,
			float& tP, float& tQ, v3dxVector3& OutPointOnP, v3dxVector3& OutPointOnQ)
		{
			const v3dxVector3 DirP = LineP2 - LineP1;
			const v3dxVector3 DirQ = LineQ2 - LineQ1;
			const v3dxVector3 W = LineP1 - LineQ1;

			const float a = Vec3Dot(DirP, DirP);
			const float b = Vec3Dot(DirP, DirQ);
			const float c = Vec3Dot(DirQ, DirQ);
			const float d = Vec3Dot(DirP, W);
			const float e = Vec3Dot(DirQ, W);

			const float Denom = a * c - b * b;

			if (fabsf(Denom) < KAWAII_SMALL_NUMBER)
			{
				tP = 0.0f;
				tQ = (b > c) ? (d / b) : (e / c);
			}
			else
			{
				tP = (b * e - c * d) / Denom;
				tQ = (a * e - b * d) / Denom;
			}

			tP = KawaiiClamp(tP, 0.0f, 1.0f);
			tQ = KawaiiClamp(tQ, 0.0f, 1.0f);

			OutPointOnP = LineP1 + DirP * tP;
			OutPointOnQ = LineQ1 + DirQ * tQ;
		}

		bool LineSegment_SphereIntersection(const v3dxVector3& SphereCenter, float SphereRadius,
			const v3dxVector3& Point1, const v3dxVector3& Point2,
			v3dxVector3& OutPointOnLine, v3dxVector3& OutPointOnCollider, float& OutRadius)
		{
			const v3dxVector3 LineDir = Point2 - Point1;
			const float LineLen = Vec3Length(LineDir);
			if (LineLen < KAWAII_SMALL_NUMBER) return false;

			const v3dxVector3 LineDirN = LineDir * (1.0f / LineLen);
			const v3dxVector3 ToCenter = SphereCenter - Point1;
			float t = Vec3Dot(ToCenter, LineDirN);
			t = KawaiiClamp(t, 0.0f, LineLen);

			OutPointOnLine = Point1 + LineDirN * t;
			const float DistSqr = Vec3LengthSqr(OutPointOnLine - SphereCenter);
			OutRadius = SphereRadius;

			if (DistSqr < SphereRadius * SphereRadius)
			{
				OutPointOnCollider = SphereCenter + Vec3SafeNormal(OutPointOnLine - SphereCenter) * SphereRadius;
				return true;
			}
			return false;
		}

		bool LineSegment_CapsuleIntersection(const v3dxVector3& CapsulePos, const v3dxVector3& CapsuleDir,
			float CapsuleRadius, const v3dxVector3& Point1, const v3dxVector3& Point2,
			v3dxVector3& OutPointOnLine, v3dxVector3& OutPointOnCollider, float& OutRadius)
		{
			const float CapsuleLen = Vec3Length(CapsuleDir);
			if (CapsuleLen < KAWAII_SMALL_NUMBER) return false;

			const v3dxVector3 CapsuleDirN = CapsuleDir * (1.0f / CapsuleLen);
			const v3dxVector3 CapsuleEnd = CapsulePos + CapsuleDir;

			// Check hemisphere ends
			if (LineSegment_SphereIntersection(CapsulePos, CapsuleRadius, Point1, Point2, OutPointOnLine, OutPointOnCollider, OutRadius))
				return true;
			if (LineSegment_SphereIntersection(CapsuleEnd, CapsuleRadius, Point1, Point2, OutPointOnLine, OutPointOnCollider, OutRadius))
				return true;

			// Check cylindrical body
			float tP, tQ;
			v3dxVector3 PointOnLine, PointOnCapsule;
			ClosestPointsOnTwoLines(Point1, Point2, CapsulePos, CapsuleEnd, tP, tQ, PointOnLine, PointOnCapsule);

			if (tP >= 0.0f && tP <= 1.0f && tQ >= 0.0f && tQ <= 1.0f)
			{
				const float Dist = Vec3Distance(PointOnLine, PointOnCapsule);
				if (Dist < CapsuleRadius)
				{
					OutPointOnLine = PointOnLine;
					OutPointOnCollider = PointOnCapsule + Vec3SafeNormal(PointOnLine - PointOnCapsule) * CapsuleRadius;
					OutRadius = CapsuleRadius;
					return true;
				}
			}
			return false;
		}

		void PushOutFromLineSegment(FSimParticle& JointA, FSimParticle& JointB,
			const v3dxVector3& PointOnLine, const v3dxVector3& PointOnCollider, float IntersectionRadius)
		{
			const v3dxVector3 PushDir = Vec3SafeNormal(PointOnLine - PointOnCollider);
			const float CurrentDist = Vec3Distance(PointOnLine, PointOnCollider);
			const float PushDist = std::max(0.0f, IntersectionRadius - CurrentDist);

			if (PushDist < KAWAII_SMALL_NUMBER) return;

			const float DistA = Vec3Distance(JointA.Position, PointOnLine);
			const float DistB = Vec3Distance(JointB.Position, PointOnLine);
			const float TotalDist = DistA + DistB;

			if (TotalDist < KAWAII_SMALL_NUMBER) return;

			const float WeightA = JointA.GetWeight() * (1.0f - DistA / TotalDist);
			const float WeightB = JointB.GetWeight() * (1.0f - DistB / TotalDist);
			const float TotalWeight = WeightA + WeightB;

			if (TotalWeight < KAWAII_SMALL_NUMBER) return;

			JointA.Position = JointA.Position + PushDir * (PushDist * WeightA / TotalWeight);
			JointB.Position = JointB.Position + PushDir * (PushDist * WeightB / TotalWeight);
		}

	} // namespace CollisionUtils
} // namespace KawaiiPhysics

NS_END
