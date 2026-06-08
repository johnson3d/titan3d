#include "XPBDSolver.h"

NS_BEGIN

namespace KawaiiPhysics
{
namespace XPBDSolver
{
	void ApplyAerodynamics(
		std::vector<FSimParticle>& Particles,
		float dt,
		const v3dxVector3& WindCS)
	{
		for (FSimParticle& P : Particles)
		{
			if (P.PinMode != KPM_Dynamic) continue;

			const float DragCoeff = P.PhysicsSettings.DragCoefficient;
			if (DragCoeff < KAWAII_SMALL_NUMBER) continue;

			const v3dxVector3 ParticleWind = WindCS * P.PhysicsSettings.WindCoefficient;
			const v3dxVector3 RelVel = P.Velocity - ParticleWind;
			const float RelSpeed = Vec3Length(RelVel);
			if (RelSpeed < KAWAII_SMALL_NUMBER) continue;

			const float Scale = std::min(0.5f * DragCoeff * RelSpeed * P.InverseMass * dt, 1.0f);
			P.Velocity = P.Velocity - RelVel * Scale;
		}
	}

	void PredictPositions(
		std::vector<FSimParticle>& Particles,
		float dt,
		const v3dxVector3& Gravity,
		const v3dxVector3& WindForceCS)
	{
		for (FSimParticle& P : Particles)
		{
			if (P.PinMode != KPM_Dynamic) continue;

			const v3dxVector3 Acceleration = Gravity + WindForceCS * P.PhysicsSettings.WindCoefficient * P.InverseMass;
			P.Velocity = P.Velocity + Acceleration * dt;
			P.PrevPosition = P.Position;
			P.Position = P.Position + P.Velocity * dt;
		}
	}

	void UpdateVelocities(
		std::vector<FSimParticle>& Particles,
		float dt)
	{
		const float InvDt = (dt > KAWAII_SMALL_NUMBER) ? (1.0f / dt) : 0.0f;

		for (FSimParticle& P : Particles)
		{
			if (P.PinMode == KPM_Dynamic)
			{
				P.Velocity = (P.Position - P.PrevPosition) * InvDt;
			}
			else
			{
				P.Velocity = v3dxVector3(0, 0, 0);
			}
		}
	}

	void ApplyDamping(
		std::vector<FSimParticle>& Particles,
		float dt,
		float SpeedScale)
	{
		for (FSimParticle& P : Particles)
		{
			if (P.PinMode != KPM_Dynamic) continue;
			P.Velocity = P.Velocity * ((1.0f - KawaiiClamp(P.PhysicsSettings.Damping, 0.0f, 1.0f)) * SpeedScale);
		}
	}

	void ApplyContactFriction(std::vector<FSimParticle>& Particles)
	{
		for (FSimParticle& P : Particles)
		{
			if (P.PinMode != KPM_Dynamic) continue;
			if (P.Friction < KAWAII_SMALL_NUMBER || Vec3IsNearlyZero(P.ContactNormal)) continue;

			const v3dxVector3 Normal = Vec3SafeNormal(P.ContactNormal);
			const v3dxVector3 VelNormal = Normal * Vec3Dot(P.Velocity, Normal);
			const v3dxVector3 VelTangent = P.Velocity - VelNormal;
			const float TangentSpeed = Vec3Length(VelTangent);

			if (TangentSpeed > KAWAII_SMALL_NUMBER)
			{
				const float MaxFriction = P.Friction * Vec3Length(VelNormal);
				const float Scale = std::max(0.0f, 1.0f - MaxFriction / TangentSpeed);
				P.Velocity = VelNormal + VelTangent * Scale;
			}

			P.Friction = 0.0f;
			P.ContactNormal = v3dxVector3(0, 0, 0);
		}
	}

	void ClampAndSleep(
		std::vector<FSimParticle>& Particles,
		float MaxVelocity,
		float SleepThreshold)
	{
		const float MaxSpeedSqr = MaxVelocity * MaxVelocity;

		for (FSimParticle& P : Particles)
		{
			if (P.PinMode != KPM_Dynamic) continue;

			const float SpeedSqr = Vec3LengthSqr(P.Velocity);

			if (MaxVelocity > 0.0f && SpeedSqr > MaxSpeedSqr)
			{
				P.Velocity = P.Velocity * (MaxVelocity / sqrtf(SpeedSqr));
			}

			if (0.5f * SpeedSqr <= SleepThreshold)
			{
				P.Position = P.PrevPosition;
				P.Velocity = v3dxVector3(0, 0, 0);
			}
		}
	}

	void ClampDisplacement(std::vector<FSimParticle>& Particles)
	{
		for (FSimParticle& P : Particles)
		{
			if (P.PinMode != KPM_Dynamic) continue;
			if (P.PhysicsSettings.MaxFrameDisplacement <= 0.0f) continue;

			const v3dxVector3 Displacement = P.Position - P.FrameStartPosition;
			const float DistSqr = Vec3LengthSqr(Displacement);
			const float MaxDist = P.PhysicsSettings.MaxFrameDisplacement;

			if (DistSqr > MaxDist * MaxDist)
			{
				P.Position = P.FrameStartPosition + Displacement * (MaxDist / sqrtf(DistSqr));
			}
		}
	}

	// Single constraint solving kernel
	static inline void SolveSingleDistanceConstraint(
		std::vector<FSimParticle*>& Particles,
		FDistanceConstraint& C,
		float dtSqr)
	{
		FSimParticle* A = Particles[C.IndexA];
		FSimParticle* B = Particles[C.IndexB];

		if (!A->IsLODValidToEvaluate || !B->IsLODValidToEvaluate)
			return;

		const float w1 = (A->PinMode == KPM_Dynamic) ? A->InverseMass : 0.0f;
		const float w2 = (B->PinMode == KPM_Dynamic) ? B->InverseMass : 0.0f;
		const float WSum = w1 + w2;
		if (WSum < KAWAII_SMALL_NUMBER) return;

		v3dxVector3 Delta = A->Position - B->Position;
		const float d = Vec3Length(Delta);
		if (d < KAWAII_SMALL_NUMBER) return;

		const float Constraint = d - C.RestLength;
		const float Compliance = (Constraint > 0.0f) ? C.StretchCompliance : C.ShrinkCompliance;
		const float Alpha = Compliance / (dtSqr + KAWAII_SMALL_NUMBER);
		const float DLambda = (-Constraint - Alpha * C.Lambda) / (WSum + Alpha + KAWAII_SMALL_NUMBER);
		const v3dxVector3 Correction = Delta * (DLambda / d);

		A->Position = A->Position + Correction * w1;
		B->Position = B->Position - Correction * w2;
		C.Lambda += DLambda;
	}

	void SolveDistanceConstraints(
		std::vector<FSimParticle*>& Particles,
		std::vector<FDistanceConstraint>& Constraints,
		float dt)
	{
		const float dtSqr = dt * dt;
		for (FDistanceConstraint& C : Constraints)
			SolveSingleDistanceConstraint(Particles, C, dtSqr);
	}

	void SolveDistanceConstraintsBidirectional(
		std::vector<FSimParticle*>& Particles,
		std::vector<FDistanceConstraint>& Constraints,
		float dt)
	{
		const float dtSqr = dt * dt;
		const int32_t Num = (int32_t)Constraints.size();

		// Forward pass
		for (int32_t i = 0; i < Num; ++i)
			SolveSingleDistanceConstraint(Particles, Constraints[i], dtSqr);

		// Backward pass
		for (int32_t i = Num - 1; i >= 0; --i)
			SolveSingleDistanceConstraint(Particles, Constraints[i], dtSqr);
	}

	void SolveBendingConstraints(
		std::vector<FSimParticle*>& Particles,
		std::vector<FBendingConstraint>& Constraints,
		float dt,
		bool bReverse)
	{
		const float dtSqr = dt * dt;

		auto SolveOne = [&](FBendingConstraint& C)
		{
			FSimParticle* P1 = Particles[C.IndexP1];
			FSimParticle* Pivot = Particles[C.IndexPivot];
			FSimParticle* P2 = Particles[C.IndexP2];

			if (!P1->IsLODValidToEvaluate || !Pivot->IsLODValidToEvaluate || !P2->IsLODValidToEvaluate)
				return;

			const float w1 = (P1->PinMode == KPM_Dynamic) ? P1->InverseMass : 0.0f;
			const float w2 = (P2->PinMode == KPM_Dynamic) ? P2->InverseMass : 0.0f;
			const float w3 = (Pivot->PinMode == KPM_Dynamic) ? Pivot->InverseMass : 0.0f;

			// Centroid
			const v3dxVector3 Centroid = (P1->Position + P2->Position + Pivot->Position) * (1.0f / 3.0f);
			const v3dxVector3 BendVector = Pivot->Position - Centroid;
			const float BendLength = Vec3Length(BendVector);
			if (BendLength < KAWAII_SMALL_NUMBER) return;

			const float Constraint = BendLength - C.RestLength;

			// Dead zone
			if (fabsf(Constraint) <= C.MaxBending) return;

			// WSum = (w1 + w2 + 4*w3) / 9
			const float WSum = (w1 + w2 + 4.0f * w3) / 9.0f;
			if (WSum < KAWAII_SMALL_NUMBER) return;

			const float Alpha = C.Compliance / (dtSqr + KAWAII_SMALL_NUMBER);
			const float DLambda = (-Constraint - Alpha * C.Lambda) / (WSum + Alpha + KAWAII_SMALL_NUMBER);
			const v3dxVector3 BendDir = BendVector * (1.0f / BendLength);

			P1->Position = P1->Position - BendDir * (DLambda * w1 / 3.0f);
			P2->Position = P2->Position - BendDir * (DLambda * w2 / 3.0f);
			Pivot->Position = Pivot->Position + BendDir * (DLambda * w3 * 2.0f / 3.0f);
			C.Lambda += DLambda;
		};

		if (bReverse)
		{
			for (int32_t i = (int32_t)Constraints.size() - 1; i >= 0; --i)
				SolveOne(Constraints[i]);
		}
		else
		{
			for (auto& C : Constraints)
				SolveOne(C);
		}
	}

} // namespace XPBDSolver
} // namespace KawaiiPhysics

NS_END
