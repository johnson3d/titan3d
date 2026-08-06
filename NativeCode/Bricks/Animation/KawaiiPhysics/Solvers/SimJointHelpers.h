#pragma once
#include "SimParticle.h"
#include "../Constraints/DistanceConstraint.h"
#include "../Constraints/BendingConstraint.h"
#include "../Collision/ClothUntangling.h"
#include <vector>

NS_BEGIN

namespace KawaiiPhysics
{
	// Helper functions for building chain / cloth simulation structures from bone data.

	namespace SimJointHelpers
	{
		// Build a flat particle array from a chain of bone transforms.
		// BonePositions/BoneRotations/BoneScales should be in component space.
		// ParentIndices[i] = parent bone index of bone i, or -1 for root.
		void BuildChainParticles(
			const std::vector<v3dxVector3>& BonePositions,
			const std::vector<v3dxQuaternion>& BoneRotations,
			const std::vector<v3dxVector3>& BoneScales,
			const std::vector<int32_t>& ParentIndices,
			int32_t RootBoneIndex,
			int32_t EndBoneIndex,
			ETailBoneAxis TailBoneForwardAxis,
			float TailBoneLength,
			std::vector<FSimParticle>& OutParticles);

		FKawaiiPhySettings MakeRandomizedPhysicsSettings(
			const FKawaiiPhySettings& PhysicsSettings,
			const FKawaiiPhySettings& PhysicsSettingsRandom,
			uint32_t RandomSeed);

		void ApplyPhysicsSettings(
			std::vector<FSimParticle>& Particles,
			const FKawaiiPhySettings& PhysicsSettings,
			const FKawaiiPhySettings& PhysicsSettingsRandom,
			uint32_t RandomSeed);

		// Per-particle physics settings with along-chain parameter curves.
		// Base = MakeRandomizedPhysicsSettings(Setup); then each field is multiplied by its
		// curve sampled at rate (Setup.CurveMode: index rate or NormalizedLength), clamped.
		// Empty curve -> multiplier 1.0 (== uniform, matches ApplyPhysicsSettings behaviour).
		void UpdatePhysicsSettings(
			std::vector<FSimParticle>& Particles,
			const FKawaiiChainSetup& Setup,
			uint32_t RandomSeed);

		// Compute rest lengths for distance constraints
		void BuildVerticalConstraints(
			const std::vector<FSimParticle>& Particles,
			std::vector<FDistanceConstraint>& OutConstraints,
			float DefaultShrinkCompliance = 0.0f,
			float DefaultStretchCompliance = 0.0f);

		// Build horizontal constraints for cloth (connecting particles at the same depth across chains)
		void BuildHorizontalConstraints(
			const std::vector<std::vector<FSimParticle>>& ChainTable,
			const std::vector<int32_t>& ChainOffsets,
			std::vector<FDistanceConstraint>& OutConstraints,
			bool bLoop = false,
			float DefaultShrinkCompliance = 0.0f,
			float DefaultStretchCompliance = 0.0f);

		// Build shear constraints for cloth (diagonal connections)
		void BuildShearConstraints(
			const std::vector<std::vector<FSimParticle>>& ChainTable,
			const std::vector<int32_t>& ChainOffsets,
			std::vector<FDistanceConstraint>& OutConstraints,
			bool bLoop = false,
			float DefaultShrinkCompliance = 0.0f,
			float DefaultStretchCompliance = 0.0f);

		// Build bending constraints for a single chain
		void BuildChainBendingConstraints(
			const std::vector<FSimParticle>& Particles,
			std::vector<FBendingConstraint>& OutConstraints,
			float DefaultCompliance = 0.0f,
			float DefaultDeadZone = 0.0f);

		// Build bending constraints across chains (horizontal bending for cloth)
		void BuildHorizontalBendingConstraints(
			const std::vector<std::vector<FSimParticle>>& ChainTable,
			const std::vector<int32_t>& ChainOffsets,
			std::vector<FBendingConstraint>& OutConstraints,
			bool bLoop = false,
			float DefaultCompliance = 0.0f,
			float DefaultDeadZone = 0.0f);

		// Build triangulated surface for cloth collision
		void BuildClothSurface(
			const std::vector<std::vector<FSimParticle>>& ChainTable,
			bool bLoop,
			FSimSurface& OutSurface);

		// Compute LengthFromRoot and NormalizedLength for each particle in a chain
		void ComputeChainLengths(std::vector<FSimParticle>& Particles);

		// Build a flat index array from a 2D chain table
		void BuildFlatParticlePointers(
			std::vector<std::vector<FSimParticle>>& ChainTable,
			std::vector<FSimParticle*>& OutFlat,
			std::vector<int32_t>& OutChainOffsets);
	}

} // namespace KawaiiPhysics

NS_END
