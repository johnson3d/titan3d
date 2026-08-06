#pragma once
#include "SimParticle.h"
#include "../KawaiiPhySettings.h"
#include <vector>

NS_BEGIN

namespace KawaiiPhysics
{
	// =====================================================================
	// FCosseratRodElement - A single rod segment between two centerline nodes
	// =====================================================================

	struct FCosseratRodElement
	{
		v3dxVector3 RestPosition = v3dxVector3(0, 0, 0);
		v3dxQuaternion RestOrientation = v3dxQuaternion(0, 0, 0, 1);
		v3dxQuaternion Orientation = v3dxQuaternion(0, 0, 0, 1);
		v3dxQuaternion PrevOrientation = v3dxQuaternion(0, 0, 0, 1);

		float RestLength = 0.0f;
		v3dxVector3 RestDarboux = v3dxVector3(0, 0, 0);

		bool bIsRoot = false;
	};

	// =====================================================================
	// FCosseratRodData - Runtime data for a single Cosserat rod
	// =====================================================================

	struct FCosseratRodData
	{
		std::string Name;
		std::vector<FSimParticle> Particles;
		std::vector<FCosseratRodElement> Elements;
		std::vector<FSimParticle*> FlatParticles;

		float StretchAndShearStiffness = 1.0f;
		float BendAndTwistStiffness = 0.05f;
		float PointAttachmentStiffness = 0.10f;
		float OrientationAttachmentStiffness = 0.05f;

		// Per-segment stiffness multiplier curves (empty -> 1.0). Sampled by normalized
		// element/particle position along the rod. Copied from FKawaiiRodSetup in BuildRods.
		FKawaiiCurve StretchAndShearStiffnessCurve;
		FKawaiiCurve BendAndTwistStiffnessCurve;
		FKawaiiCurve PointAttachmentStiffnessCurve;
		FKawaiiCurve OrientationAttachmentStiffnessCurve;

		int32_t LODThreshold = -1;

		bool IsLODValid(int32_t SimLOD) const
		{
			return LODThreshold < 0 || SimLOD <= LODThreshold;
		}
	};

	// =====================================================================
	// CosseratRodSolver - Cosserat rod XPBD constraint solver
	// =====================================================================

	namespace CosseratRodSolver
	{
		// Initialize rod elements from particle chain
		void InitializeRodElements(FCosseratRodData& Rod);

		// Compute the Darboux vector (material curvature) between two orientations
		v3dxVector3 ComputeDarbouxVector(const v3dxQuaternion& Q1, const v3dxQuaternion& Q2, float SegmentLength);

		// XPBD stretch and shear constraint
		void SolveStretchShearConstraint(
			FCosseratRodData& Rod,
			int32_t ElementIndex,
			float dt);

		// XPBD bend and twist constraint
		void SolveBendTwistConstraint(
			FCosseratRodData& Rod,
			int32_t ElementIndex,
			float dt);

		// Point attachment constraint (position matching to animated pose)
		void SolvePointAttachmentConstraint(
			FCosseratRodData& Rod,
			float dt);

		// Orientation attachment constraint (orientation matching to animated pose)
		void SolveOrientationAttachmentConstraint(
			FCosseratRodData& Rod,
			float dt);

		// Full solve step for one rod
		void SolveRod(
			FCosseratRodData& Rod,
			float dt,
			int32_t Iterations);

		// Update element orientations from particle positions
		void UpdateElementOrientations(FCosseratRodData& Rod);
	}

} // namespace KawaiiPhysics

NS_END
