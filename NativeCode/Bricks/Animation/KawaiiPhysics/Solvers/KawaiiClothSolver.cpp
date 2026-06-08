#include "KawaiiClothSolver.h"

NS_BEGIN

namespace KawaiiPhysics
{
	void KawaiiClothSolver::Initialize(
		const std::vector<FKawaiiClothSetup>& Setups,
		const std::vector<v3dxVector3>& BonePositions,
		const std::vector<v3dxQuaternion>& BoneRotations,
		const std::vector<v3dxVector3>& BoneScales,
		const std::vector<int32_t>& ParentIndices)
	{
		ClothMeshes.clear();
		ClothMeshes.resize(Setups.size());

		for (size_t meshIdx = 0; meshIdx < Setups.size(); ++meshIdx)
		{
			const FKawaiiClothSetup& setup = Setups[meshIdx];
			FKawaiiClothData& cloth = ClothMeshes[meshIdx];
			cloth.Name = setup.Name;
			cloth.ClothDefIndex = (int32_t)meshIdx;
			cloth.bConstrainBoneLength = setup.bConstrainBoneLength;
			cloth.BoneLengthConstraintBlend = setup.BoneLengthConstraintBlend;
			cloth.LODThreshold = setup.LODThreshold;
			cloth.bRootCollision = setup.bRootCollision;
			cloth.TailBoneForwardAxis = setup.TailBoneForwardAxis;
			cloth.TailBoneLength = setup.TailBoneLength;

			// Build chain table - each chain is built independently
			// For cloth, multiple chains share horizontal constraints
			// The caller is responsible for providing multiple root/end pairs via Setups
			std::vector<FSimParticle> singleChain;
			SimJointHelpers::BuildChainParticles(
				BonePositions, BoneRotations, BoneScales, ParentIndices,
				setup.RootBoneIndex, setup.EndBoneIndex,
				setup.TailBoneForwardAxis, setup.TailBoneLength,
				singleChain);

			cloth.ChainTable.push_back(singleChain);

			// Build flat pointers
			SimJointHelpers::BuildFlatParticlePointers(cloth.ChainTable, cloth.FlatParticles, cloth.ChainOffsets);

			// Build vertical constraints per chain
			for (auto& chain : cloth.ChainTable)
			{
				std::vector<FDistanceConstraint> vConstraints;
				SimJointHelpers::BuildVerticalConstraints(chain, vConstraints,
					setup.VerticalShrinkStiffness.IsEmpty() ? 0.0f : (1.0f - setup.VerticalShrinkStiffness.Evaluate(0.5f)),
					setup.VerticalStretchStiffness.IsEmpty() ? 0.0f : (1.0f - setup.VerticalStretchStiffness.Evaluate(0.5f)));

				// Remap indices to flat
				int32_t chainIdx = (int32_t)(&chain - &cloth.ChainTable[0]);
				int32_t offset = cloth.ChainOffsets[chainIdx];
				for (auto& c : vConstraints)
				{
					c.IndexA += offset;
					c.IndexB += offset;
				}
				cloth.VerticalConstraints.insert(cloth.VerticalConstraints.end(), vConstraints.begin(), vConstraints.end());
			}

			// Build horizontal constraints
			if (setup.bStructuralHorizontalConstraint)
			{
				SimJointHelpers::BuildHorizontalConstraints(cloth.ChainTable, cloth.ChainOffsets,
					cloth.HorizontalConstraints, setup.bLoopChains,
					setup.HorizontalShrinkStiffness.IsEmpty() ? 0.0f : (1.0f - setup.HorizontalShrinkStiffness.Evaluate(0.5f)),
					setup.HorizontalStretchStiffness.IsEmpty() ? 0.0f : (1.0f - setup.HorizontalStretchStiffness.Evaluate(0.5f)));
			}

			// Build shear constraints
			if (setup.bShearConstraint)
			{
				SimJointHelpers::BuildShearConstraints(cloth.ChainTable, cloth.ChainOffsets,
					cloth.ShearConstraints, setup.bLoopChains,
					setup.ShearShrinkStiffness.IsEmpty() ? 0.0f : (1.0f - setup.ShearShrinkStiffness.Evaluate(0.5f)),
					setup.ShearStretchStiffness.IsEmpty() ? 0.0f : (1.0f - setup.ShearStretchStiffness.Evaluate(0.5f)));
			}

			// Build vertical bending constraints
			if (setup.bBendingVerticalConstraint)
			{
				for (auto& chain : cloth.ChainTable)
				{
					std::vector<FBendingConstraint> bConstraints;
					float compliance = setup.VerticalBendStiffness.IsEmpty() ? 0.0f : (1.0f - setup.VerticalBendStiffness.Evaluate(0.5f));
					SimJointHelpers::BuildChainBendingConstraints(chain, bConstraints, compliance, setup.VerticalBendDeadZone);

					int32_t chainIdx = (int32_t)(&chain - &cloth.ChainTable[0]);
					int32_t offset = cloth.ChainOffsets[chainIdx];
					for (auto& bc : bConstraints)
					{
						bc.IndexP1 += offset;
						bc.IndexPivot += offset;
						bc.IndexP2 += offset;
					}
					cloth.VerticalBendingConstraints.insert(cloth.VerticalBendingConstraints.end(), bConstraints.begin(), bConstraints.end());
				}
			}

			// Build horizontal bending constraints
			if (setup.bBendingHorizontalConstraint)
			{
				float compliance = setup.HorizontalBendStiffness.IsEmpty() ? 0.0f : (1.0f - setup.HorizontalBendStiffness.Evaluate(0.5f));
				SimJointHelpers::BuildHorizontalBendingConstraints(cloth.ChainTable, cloth.ChainOffsets,
					cloth.HorizontalBendingConstraints, setup.bLoopChains, compliance, setup.HorizontalBendDeadZone);
			}

			// Build cloth surface for multi-layer collision
			SimJointHelpers::BuildClothSurface(cloth.ChainTable, setup.bLoopChains, cloth.ClothSurface);

			// Compute max chain length
			cloth.MaxChainLength = 0.0f;
			for (auto& chain : cloth.ChainTable)
			{
				if (!chain.empty())
					cloth.MaxChainLength = std::max(cloth.MaxChainLength, chain.back().LengthFromRoot);
			}
		}
	}

	void KawaiiClothSolver::Simulate(const FKawaiiPhysicsContext& Context, FTopLevelBVH* ColliderBVH)
	{
		for (auto& cloth : ClothMeshes)
		{
			if (!cloth.IsLODValid(Context.SimulationLOD)) continue;
			SimulateCloth(cloth, Context, ColliderBVH);
		}
	}

	void KawaiiClothSolver::SimulateCloth(FKawaiiClothData& Cloth, const FKawaiiPhysicsContext& Context, FTopLevelBVH* ColliderBVH)
	{
		if (Cloth.FlatParticles.empty()) return;

		const float dt = Context.SubstepDeltaTime > 0.0f ? Context.SubstepDeltaTime : Context.DeltaTime;
		if (dt < KAWAII_SMALL_NUMBER) return;

		// Collect all particles for bulk operations
		std::vector<FSimParticle> allParticlesFlat;
		for (auto& chain : Cloth.ChainTable)
			allParticlesFlat.insert(allParticlesFlat.end(), chain.begin(), chain.end());

		// Record frame start positions
		for (auto* P : Cloth.FlatParticles)
			P->FrameStartPosition = P->Position;

		// Aerodynamics
		if (Context.bEnableWind)
		{
			for (auto& chain : Cloth.ChainTable)
				XPBDSolver::ApplyAerodynamics(chain, dt, Context.WindForce);
		}

		// Predict positions
		v3dxVector3 gravity = Context.Gravity * Context.GravityScale;
		for (auto& chain : Cloth.ChainTable)
			XPBDSolver::PredictPositions(chain, dt, gravity, Context.WindForce);

		// Clear lambdas
		XPBDSolver::ClearConstraintLambdas(Cloth.VerticalConstraints);
		XPBDSolver::ClearConstraintLambdas(Cloth.HorizontalConstraints);
		XPBDSolver::ClearConstraintLambdas(Cloth.ShearConstraints);
		XPBDSolver::ClearConstraintLambdas(Cloth.VerticalBendingConstraints);
		XPBDSolver::ClearConstraintLambdas(Cloth.HorizontalBendingConstraints);

		// Constraint iterations
		for (int32_t iter = 0; iter < Context.ConstraintIterations; ++iter)
		{
			bool bReverse = (iter % 2 == 1);

			// Distance constraints
			if (bReverse)
			{
				XPBDSolver::SolveDistanceConstraintsBidirectional(Cloth.FlatParticles, Cloth.VerticalConstraints, dt);
				XPBDSolver::SolveDistanceConstraints(Cloth.FlatParticles, Cloth.HorizontalConstraints, dt);
				XPBDSolver::SolveDistanceConstraints(Cloth.FlatParticles, Cloth.ShearConstraints, dt);
			}
			else
			{
				XPBDSolver::SolveDistanceConstraints(Cloth.FlatParticles, Cloth.VerticalConstraints, dt);
				XPBDSolver::SolveDistanceConstraints(Cloth.FlatParticles, Cloth.HorizontalConstraints, dt);
				XPBDSolver::SolveDistanceConstraints(Cloth.FlatParticles, Cloth.ShearConstraints, dt);
			}

			// Bending constraints
			XPBDSolver::SolveBendingConstraints(Cloth.FlatParticles, Cloth.VerticalBendingConstraints, dt, bReverse);
			XPBDSolver::SolveBendingConstraints(Cloth.FlatParticles, Cloth.HorizontalBendingConstraints, dt, bReverse);

			// Bone constraints per chain
			for (auto& chain : Cloth.ChainTable)
			{
				for (size_t j = 1; j < chain.size(); ++j)
				{
					if (Cloth.bConstrainBoneLength)
						FBoneConstraints::ApplyLengthConstraint(Cloth.BoneLengthConstraintBlend, chain[j], chain[j - 1]);

					FBoneConstraints::ApplyAngleLimit(chain[j], chain[j - 1]);
				}
			}

			// Collider collision (every N iterations)
			if (CollisionIterationInterval > 0 && iter % CollisionIterationInterval == 0)
				HandleCollision(Cloth, Context, ColliderBVH);

			// Multi-layer collision
			if (bMultiLayerCollision && !CollisionLayerGroups.empty())
				HandleMultiLayerCollision(iter);
		}

		// Update velocities
		for (auto& chain : Cloth.ChainTable)
			XPBDSolver::UpdateVelocities(chain, dt);

		// Damping
		for (auto& chain : Cloth.ChainTable)
			XPBDSolver::ApplyDamping(chain, dt, Context.SpeedScale);

		// Contact friction
		for (auto& chain : Cloth.ChainTable)
			XPBDSolver::ApplyContactFriction(chain);

		// Clamp velocity & sleep
		for (auto& chain : Cloth.ChainTable)
			XPBDSolver::ClampAndSleep(chain, Context.MaxSpeed, Context.SleepThreshold);

		// Displacement clamping
		for (auto& chain : Cloth.ChainTable)
			XPBDSolver::ClampDisplacement(chain);
	}

	void KawaiiClothSolver::HandleCollision(FKawaiiClothData& Cloth, const FKawaiiPhysicsContext& Context, FTopLevelBVH* ColliderBVH)
	{
		if (!ColliderBVH || ColliderBVH->IsEmpty()) return;

		int32_t subSteps = std::max(1, CollisionSubSteps);

		for (int32_t step = 0; step < subSteps; ++step)
		{
			float subStepFraction = (float)(step + 1) / (float)subSteps;
			int32_t startIdx = Cloth.bRootCollision ? 0 : 1;

			for (auto& chain : Cloth.ChainTable)
			{
				for (int32_t i = startIdx; i < (int32_t)chain.size(); ++i)
				{
					FSimParticle& P = chain[i];
					if (P.PinMode != KPM_Dynamic || !P.bCollision) continue;

					FKawaiiAABB queryBox = FKawaiiAABB::BuildAABB(P.Position, P.PhysicsSettings.Radius * 2.0f);
					std::vector<std::shared_ptr<FBoundBoxHandle>> overlaps;
					ColliderBVH->QueryOverlap(queryBox, overlaps);

					for (auto& handle : overlaps)
					{
						if (handle && handle->Owner)
							handle->OnPointCollision(P, subStepFraction, 0, 1.0f);
					}
				}
			}
		}
	}

	void KawaiiClothSolver::HandleMultiLayerCollision(int32_t Iteration)
	{
		for (const FKawaiiLayerGroup& group : CollisionLayerGroups)
		{
			for (const FKawaiiLayerPair& pair : group.LayerPairs)
			{
				if (pair.InnerLayerIndex < 0 || pair.InnerLayerIndex >= (int32_t)ClothMeshes.size()) continue;
				if (pair.OuterLayerIndex < 0 || pair.OuterLayerIndex >= (int32_t)ClothMeshes.size()) continue;

				FKawaiiClothData& inner = ClothMeshes[pair.InnerLayerIndex];
				FKawaiiClothData& outer = ClothMeshes[pair.OuterLayerIndex];

				if (!inner.ClothSurface.IsValid() || !outer.ClothSurface.IsValid()) continue;

				auto GetParticle = [&](const FSimJointIndex& Idx) -> FSimParticle*
				{
					// Determine which cloth this index belongs to based on ChainIndex range
					// Inner layer uses its own ChainTable, outer uses its own
					// The FSimJointIndex stores {ChainIndex, JointIndex} relative to the mesh
					if (Idx.ChainIndex >= 0 && Idx.ChainIndex < (int32_t)inner.ChainTable.size())
						return inner.GetParticle(Idx);
					// Try outer
					FSimJointIndex outerIdx = Idx;
					outerIdx.ChainIndex -= (int32_t)inner.ChainTable.size();
					return outer.GetParticle(outerIdx);
				};

				// Use inner/outer accessors directly
				auto GetInnerParticle = [&](const FSimJointIndex& Idx) -> FSimParticle* { return inner.GetParticle(Idx); };
				auto GetOuterParticle = [&](const FSimJointIndex& Idx) -> FSimParticle* { return outer.GetParticle(Idx); };

				// Create a combined accessor
				auto CombinedAccessor = [&](const FSimJointIndex& Idx) -> FSimParticle*
				{
					// Try inner first, then outer
					FSimParticle* p = inner.GetParticle(Idx);
					if (!p) p = outer.GetParticle(Idx);
					return p;
				};

				// Point-face
				ClothUntangling::SolvePointFaceCollisions(
					inner.ClothSurface, outer.ClothSurface,
					CombinedAccessor, group.LayerThickness, group.LayerFriction, bUseLayerNormal);

				// Edge-edge
				if (bEdgeEdgeCollision)
				{
					ClothUntangling::SolveEdgeEdgeCollisions(
						inner.ClothSurface, outer.ClothSurface,
						CombinedAccessor, group.LayerThickness, group.LayerFriction, bUseLayerNormal);
				}
			}
		}
	}

	void KawaiiClothSolver::ResetDynamics()
	{
		for (auto& cloth : ClothMeshes)
		{
			for (auto& chain : cloth.ChainTable)
			{
				for (auto& P : chain)
					P.SnapToPose();
			}
		}
	}

	void KawaiiClothSolver::UpdatePose(
		const std::vector<v3dxVector3>& BonePositions,
		const std::vector<v3dxQuaternion>& BoneRotations,
		const std::vector<v3dxVector3>& BoneScales)
	{
		for (auto& cloth : ClothMeshes)
		{
			for (auto& chain : cloth.ChainTable)
			{
				for (size_t i = 0; i < chain.size(); ++i)
				{
					FSimParticle& P = chain[i];
					if (P.bDummy)
					{
						if (i > 0)
							P.UpdateDummyFromParent(chain[i - 1], cloth.TailBoneForwardAxis, cloth.TailBoneLength);
						continue;
					}

					int32_t boneIdx = P.BoneIndex;
					if (boneIdx >= 0 && boneIdx < (int32_t)BonePositions.size())
						P.UpdatePoseFromExternal(BonePositions[boneIdx], BoneRotations[boneIdx], BoneScales[boneIdx]);
				}
			}

			// Rebuild flat pointers (pointers may have invalidated if vectors resized)
			SimJointHelpers::BuildFlatParticlePointers(cloth.ChainTable, cloth.FlatParticles, cloth.ChainOffsets);
		}
	}

} // namespace KawaiiPhysics

NS_END
