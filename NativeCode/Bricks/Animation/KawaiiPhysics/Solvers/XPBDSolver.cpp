#include "XPBDSolver.h"

NS_BEGIN

namespace KawaiiPhysics
{
namespace XPBDSolver
{
	void ApplyWorldMoveLag(
		std::vector<FSimParticle>& Particles,
		const FKawaiiPhysicsContext& Context)
	{
		// Component translation this frame, expressed in mesh space.
		const v3dxVector3 deltaLocCS = QuatRotateVector(
			QuatInverse(Context.ComponentRotation),
			Context.ComponentLocation - Context.PrevComponentLocation);

		// Mesh-space operator that undoes this frame's component rotation: Inverse(Cur) * Prev
		// maps a mesh-space point onto the mesh-space position that keeps its previous world
		// orientation, i.e. "the particle did not turn with the actor".
		const v3dxQuaternion lagRot = QuatMultiply(
			QuatInverse(Context.ComponentRotation), Context.PrevComponentRotation);

		const bool bMoved = !Vec3IsNearlyZero(deltaLocCS);
		const bool bRotated = fabsf(lagRot.W) < 1.0f - KAWAII_SMALL_NUMBER;
		if (!bMoved && !bRotated) return;

		for (FSimParticle& P : Particles)
		{
			if (P.PinMode != KPM_Dynamic) continue;

			// Position and PrevPosition are shifted together on purpose: a change of reference
			// frame must not inject velocity by itself. The resulting offset from the (kinematic)
			// root is what the length constraint turns into a real pull, and UpdateVelocities
			// then converts that pull into velocity - that is the hair flow.
			if (bMoved)
			{
				const v3dxVector3 lag = deltaLocCS * KawaiiClamp(P.PhysicsSettings.WorldDampingLocation, 0.0f, 1.0f);
				P.Position = P.Position - lag;
				P.PrevPosition = P.PrevPosition - lag;
			}

			if (bRotated)
			{
				const float w = KawaiiClamp(P.PhysicsSettings.WorldDampingRotation, 0.0f, 1.0f);
				const v3dxVector3 rotated = QuatRotateVector(lagRot, P.Position);
				const v3dxVector3 rotatedPrev = QuatRotateVector(lagRot, P.PrevPosition);
				P.Position = P.Position + (rotated - P.Position) * w;
				P.PrevPosition = P.PrevPosition + (rotatedPrev - P.PrevPosition) * w;
			}
		}
	}

	void ApplyPoseStiffness(
		std::vector<FSimParticle>& Particles,
		float dt,
		int32_t TargetFPS)
	{
		if (Particles.size() < 2) return;

		// Frame-rate independent blend: alpha = 1 - (1 - Stiffness)^(dt * TargetFPS), so an
		// authored Stiffness keeps the same feel at 30 / 60 / 120 fps.
		const float exponent = dt * (float)((TargetFPS > 0) ? TargetFPS : 60);

		// Root -> tip so the parent's already corrected Position is used as the anchor: the whole
		// chain may lag as a unit while every joint is still pulled back toward its animated shape.
		for (size_t i = 1; i < Particles.size(); ++i)
		{
			FSimParticle& P = Particles[i];
			if (P.PinMode != KPM_Dynamic) continue;

			const float stiffness = KawaiiClamp(P.PhysicsSettings.Stiffness, 0.0f, 1.0f);
			if (stiffness < KAWAII_SMALL_NUMBER) continue;

			// Target = the animated offset from the parent applied at the parent's simulated
			// position (upstream KawaiiPhysics "Pull to Pose Location").
			const FSimParticle& Parent = Particles[i - 1];
			const v3dxVector3 target = Parent.Position + (P.PosePosition - Parent.PosePosition);

			const float alpha = (exponent > 0.0f) ? (1.0f - powf(1.0f - stiffness, exponent)) : stiffness;

			// PrevPosition is intentionally left alone: this correction should show up as velocity
			// in UpdateVelocities, which is what makes the chain spring back instead of teleporting.
			P.Position = P.Position + (target - P.Position) * alpha;
		}
	}

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
