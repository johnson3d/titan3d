#pragma once
#include "SimParticle.h"
#include "../Constraints/DistanceConstraint.h"
#include "../Constraints/BendingConstraint.h"

NS_BEGIN

namespace KawaiiPhysics
{
	// XPBD solver utility (pure static methods).
	// AnimNode calls these methods each substep to assemble the pipeline:
	//   PredictPositions -> ClearLambdas -> constraint solving -> UpdateVelocities -> ApplyDamping -> ClampAndSleep
	namespace XPBDSolver
	{
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
