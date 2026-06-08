#pragma once
#include "SimParticle.h"
#include "XPBDSolver.h"
#include "SimJointHelpers.h"
#include "../Constraints/BoneConstraints.h"
#include "../Collision/TopLevelBVH.h"
#include "../KawaiiPhySettings.h"
#include <vector>

NS_BEGIN

namespace KawaiiPhysics
{
	// =====================================================================
	// FKawaiiChainData - Runtime data for a single chain simulation
	// =====================================================================

	struct FKawaiiChainData
	{
		std::string Name;
		std::vector<FSimParticle> Particles;
		std::vector<FBendingConstraint> BendingConstraints;
		std::vector<FSimParticle*> FlatParticles;
		float TotalLength = 0.0f;

		bool bConstrainBoneLength = false;
		float BoneLengthConstraintBlend = 1.0f;
		int32_t LODThreshold = -1;
		bool bRootCollision = false;
		FKawaiiRotationLimits RotationLimits;
		ETailBoneAxis TailBoneForwardAxis = TBA_X_Positive;
		float TailBoneLength = 0.0f;

		bool IsLODValid(int32_t SimLOD) const
		{
			return LODThreshold < 0 || SimLOD <= LODThreshold;
		}
	};

	// =====================================================================
	// KawaiiChainSolver - Top-level chain simulation interface
	// =====================================================================

	class KawaiiChainSolver
	{
	public:
		// Initialize chain data from setup + bone transforms
		void Initialize(
			const std::vector<FKawaiiChainSetup>& Setups,
			const std::vector<v3dxVector3>& BonePositions,
			const std::vector<v3dxQuaternion>& BoneRotations,
			const std::vector<v3dxVector3>& BoneScales,
			const std::vector<int32_t>& ParentIndices);

		// Run one full simulation frame
		void Simulate(const FKawaiiPhysicsContext& Context, FTopLevelBVH* ColliderBVH = nullptr);

		// Reset all dynamics (teleport)
		void ResetDynamics();

		// Update pose data from external bone transforms
		void UpdatePose(
			const std::vector<v3dxVector3>& BonePositions,
			const std::vector<v3dxQuaternion>& BoneRotations,
			const std::vector<v3dxVector3>& BoneScales);

		// Get results: returns particle positions for writing back to bones
		const std::vector<FKawaiiChainData>& GetChains() const { return Chains; }

		// Chain-level settings
		bool bEnableSegmentCollision = false;
		int32_t CollisionSubSteps = 1;

	private:
		std::vector<FKawaiiChainData> Chains;

		void SimulateChain(FKawaiiChainData& Chain, const FKawaiiPhysicsContext& Context, FTopLevelBVH* ColliderBVH);
		void HandleCollision(FKawaiiChainData& Chain, const FKawaiiPhysicsContext& Context, FTopLevelBVH* ColliderBVH);
	};

} // namespace KawaiiPhysics

NS_END
