#pragma once
#include "SimParticle.h"
#include "XPBDSolver.h"
#include "SimJointHelpers.h"
#include "../Constraints/BoneConstraints.h"
#include "../Collision/TopLevelBVH.h"
#include "../Collision/ClothUntangling.h"
#include "../KawaiiPhySettings.h"
#include <vector>

NS_BEGIN

namespace KawaiiPhysics
{
	// =====================================================================
	// FKawaiiClothData - Runtime data for a single cloth mesh simulation
	// =====================================================================

	struct FKawaiiClothData
	{
		std::string Name;
		std::vector<std::vector<FSimParticle>> ChainTable;
		std::vector<FSimParticle*> FlatParticles;
		std::vector<int32_t> ChainOffsets;

		std::vector<FDistanceConstraint> VerticalConstraints;
		std::vector<FDistanceConstraint> HorizontalConstraints;
		std::vector<FBendingConstraint> VerticalBendingConstraints;
		std::vector<FBendingConstraint> HorizontalBendingConstraints;
		std::vector<FDistanceConstraint> ShearConstraints;

		FSimSurface ClothSurface;
		float MaxChainLength = 0.0f;
		int32_t ClothDefIndex = 0;

		bool bConstrainBoneLength = false;
		float BoneLengthConstraintBlend = 1.0f;
		int32_t LODThreshold = -1;
		bool bRootCollision = false;
		ETailBoneAxis TailBoneForwardAxis = TBA_X_Positive;
		float TailBoneLength = 0.0f;

		FSimParticle* GetParticle(const FSimJointIndex& Idx)
		{
			if (Idx.ChainIndex < 0 || Idx.ChainIndex >= (int32_t)ChainTable.size()) return nullptr;
			if (Idx.JointIndex < 0 || Idx.JointIndex >= (int32_t)ChainTable[Idx.ChainIndex].size()) return nullptr;
			return &ChainTable[Idx.ChainIndex][Idx.JointIndex];
		}

		bool IsLODValid(int32_t SimLOD) const
		{
			return LODThreshold < 0 || SimLOD <= LODThreshold;
		}
	};

	// =====================================================================
	// KawaiiClothSolver - Top-level cloth simulation interface
	// =====================================================================

	class KawaiiClothSolver
	{
	public:
		void Initialize(
			const std::vector<FKawaiiClothSetup>& Setups,
			const std::vector<v3dxVector3>& BonePositions,
			const std::vector<v3dxQuaternion>& BoneRotations,
			const std::vector<v3dxVector3>& BoneScales,
			const std::vector<int32_t>& ParentIndices);

		void Simulate(const FKawaiiPhysicsContext& Context, FTopLevelBVH* ColliderBVH = nullptr);

		void ResetDynamics();

		void UpdatePose(
			const std::vector<v3dxVector3>& BonePositions,
			const std::vector<v3dxQuaternion>& BoneRotations,
			const std::vector<v3dxVector3>& BoneScales);

		const std::vector<FKawaiiClothData>& GetClothMeshes() const { return ClothMeshes; }

		// Multi-layer collision settings
		bool bMultiLayerCollision = true;
		bool bEdgeEdgeCollision = true;
		bool bUseLayerNormal = false;
		std::vector<FKawaiiLayerGroup> CollisionLayerGroups;
		int32_t CollisionIterationInterval = 3;
		int32_t CollisionSubSteps = 1;

	private:
		std::vector<FKawaiiClothData> ClothMeshes;

		void SimulateCloth(FKawaiiClothData& Cloth, const FKawaiiPhysicsContext& Context, FTopLevelBVH* ColliderBVH);
		void HandleCollision(FKawaiiClothData& Cloth, const FKawaiiPhysicsContext& Context, FTopLevelBVH* ColliderBVH);
		void HandleMultiLayerCollision(int32_t Iteration);
	};

} // namespace KawaiiPhysics

NS_END
