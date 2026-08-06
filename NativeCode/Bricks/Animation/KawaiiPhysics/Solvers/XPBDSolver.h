#pragma once
#include "SimParticle.h"
#include "../Constraints/DistanceConstraint.h"
#include "../Constraints/BendingConstraint.h"

NS_BEGIN

namespace KawaiiPhysics
{
	// XPBD solver utility (pure static methods).
	// AnimNode calls these methods each substep to assemble the pipeline:
	//   ApplyWorldMoveLag -> PredictPositions -> ApplyPoseStiffness -> ClearLambdas ->
	//   constraint solving -> UpdateVelocities -> ApplyDamping -> ClampAndSleep
	namespace XPBDSolver
	{
		// Frame-of-reference lag caused by the component (actor) moving/rotating in world.
		// Physics runs in mesh space, so a particle that stays put in mesh space follows the
		// actor perfectly and shows no inertia; this pulls each dynamic particle back by the
		// part of the component motion it is not supposed to follow, weighted per particle by
		// WorldDampingLocation / WorldDampingRotation. Call once per frame before gravity.
		void ApplyWorldMoveLag(
			std::vector<FSimParticle>& Particles,
			const FKawaiiPhysicsContext& Context);

		// "Pull to pose": per-particle Stiffness spring toward the animated bone offset taken
		// from the parent's simulated position. This is the only force that brings a chain back
		// to its animated shape - without it gravity is the sole input and the chain just hangs.
		// Particles must be ordered root -> tip. Call once per frame after PredictPositions.
		void ApplyPoseStiffness(
			std::vector<FSimParticle>& Particles,
			float dt,
			int32_t TargetFPS);

		// Semi-implicit Euler position prediction
		void PredictPositions(
			std::vector<FSimParticle>& Particles,
			float dt,
			const v3dxVector3& Gravity,
			const v3dxVector3& WindForceCS);

		// Quadratic aerodynamic drag, modifies velocity before PredictPositions
		void ApplyAerodynamics(
			std::vector<FSimParticle>& Particles,
			float dt,
			const v3dxVector3& WindCS);

		// Extracts velocity from constraint-corrected positions
		void UpdateVelocities(
			std::vector<FSimParticle>& Particles,
			float dt);

		// Per-particle velocity damping
		void ApplyDamping(
			std::vector<FSimParticle>& Particles,
			float dt,
			float SpeedScale);

		// Coulomb contact friction
		void ApplyContactFriction(std::vector<FSimParticle>& Particles);

		// Velocity clamping + kinetic energy sleep
		void ClampAndSleep(
			std::vector<FSimParticle>& Particles,
			float MaxVelocity,
			float SleepThreshold);

		// Per-particle frame displacement clamping
		void ClampDisplacement(std::vector<FSimParticle>& Particles);

		// XPBD distance constraint with directional compliance
		void SolveDistanceConstraints(
			std::vector<FSimParticle*>& Particles,
			std::vector<FDistanceConstraint>& Constraints,
			float dt);

		// Forward-backward Gauss-Seidel sweep distance constraint solving
		void SolveDistanceConstraintsBidirectional(
			std::vector<FSimParticle*>& Particles,
			std::vector<FDistanceConstraint>& Constraints,
			float dt);

		// Clear constraint lambdas
		template<typename TConstraint>
		void ClearConstraintLambdas(std::vector<TConstraint>& Constraints)
		{
			for (TConstraint& C : Constraints)
				C.Lambda = 0.0f;
		}

		// Centroid-based bending constraint
		void SolveBendingConstraints(
			std::vector<FSimParticle*>& Particles,
			std::vector<FBendingConstraint>& Constraints,
			float dt,
			bool bReverse = false);

	} // namespace XPBDSolver

} // namespace KawaiiPhysics

NS_END
