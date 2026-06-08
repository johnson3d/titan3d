#include "KawaiiChainSolver.h"

NS_BEGIN

namespace KawaiiPhysics
{
	void KawaiiChainSolver::Initialize(
		const std::vector<FKawaiiChainSetup>& Setups,
		const std::vector<v3dxVector3>& BonePositions,
		const std::vector<v3dxQuaternion>& BoneRotations,
		const std::vector<v3dxVector3>& BoneScales,
		const std::vector<int32_t>& ParentIndices)
	{
		Chains.clear();
		Chains.resize(Setups.size());

		for (size_t i = 0; i < Setups.size(); ++i)
		{
			const FKawaiiChainSetup& setup = Setups[i];
			FKawaiiChainData& chain = Chains[i];
			chain.Name = setup.Name;
			chain.bConstrainBoneLength = setup.bConstrainBoneLength;
			chain.BoneLengthConstraintBlend = setup.BoneLengthConstraintBlend;
			chain.LODThreshold = setup.LODThreshold;
			chain.bRootCollision = setup.bRootCollision;
			chain.RotationLimits = setup.RotationLimits;
			chain.TailBoneForwardAxis = setup.TailBoneForwardAxis;
			chain.TailBoneLength = setup.TailBoneLength;

			SimJointHelpers::BuildChainParticles(
				BonePositions, BoneRotations, BoneScales, ParentIndices,
				setup.RootBoneIndex, setup.EndBoneIndex,
				setup.TailBoneForwardAxis, setup.TailBoneLength,
				chain.Particles);

			SimJointHelpers::BuildChainBendingConstraints(chain.Particles, chain.BendingConstraints);

			chain.FlatParticles.clear();
			for (auto& p : chain.Particles)
				chain.FlatParticles.push_back(&p);

			if (!chain.Particles.empty())
				chain.TotalLength = chain.Particles.back().LengthFromRoot;
		}
	}

	void KawaiiChainSolver::Simulate(const FKawaiiPhysicsContext& Context, FTopLevelBVH* ColliderBVH)
	{
		for (auto& chain : Chains)
		{
			if (!chain.IsLODValid(Context.SimulationLOD)) continue;
			SimulateChain(chain, Context, ColliderBVH);
		}
	}

	void KawaiiChainSolver::SimulateChain(FKawaiiChainData& Chain, const FKawaiiPhysicsContext& Context, FTopLevelBVH* ColliderBVH)
	{
		if (Chain.Particles.size() < 2) return;

		const float dt = Context.SubstepDeltaTime > 0.0f ? Context.SubstepDeltaTime : Context.DeltaTime;
		if (dt < KAWAII_SMALL_NUMBER) return;

		// Record frame start positions
		for (auto& P : Chain.Particles)
			P.FrameStartPosition = P.Position;

		// Aerodynamics
		if (Context.bEnableWind)
			XPBDSolver::ApplyAerodynamics(Chain.Particles, dt, Context.WindForce);

		// Predict positions
		v3dxVector3 gravity = Context.Gravity * Context.GravityScale;
		XPBDSolver::PredictPositions(Chain.Particles, dt, gravity, Context.WindForce);

		// Clear constraint lambdas
		XPBDSolver::ClearConstraintLambdas(Chain.BendingConstraints);

		// Constraint iterations
		for (int32_t iter = 0; iter < Context.ConstraintIterations; ++iter)
		{
			// Bending constraints
			XPBDSolver::SolveBendingConstraints(Chain.FlatParticles, Chain.BendingConstraints, dt, (iter % 2 == 1));

			// Bone constraints
			for (size_t i = 1; i < Chain.Particles.size(); ++i)
			{
				if (Chain.bConstrainBoneLength)
					FBoneConstraints::ApplyLengthConstraint(Chain.BoneLengthConstraintBlend, Chain.Particles[i], Chain.Particles[i - 1]);

				FBoneConstraints::ApplyAngleLimit(Chain.Particles[i], Chain.Particles[i - 1]);

				if (Chain.RotationLimits.IsEnabled())
					FBoneConstraints::ApplyRotationLimits(Chain.Particles[i], Chain.Particles[i - 1], Chain.RotationLimits);
			}
		}

		// Collision
		HandleCollision(Chain, Context, ColliderBVH);

		// Update velocities
		XPBDSolver::UpdateVelocities(Chain.Particles, dt);

		// Damping
		XPBDSolver::ApplyDamping(Chain.Particles, dt, Context.SpeedScale);

		// Contact friction
		XPBDSolver::ApplyContactFriction(Chain.Particles);

		// Velocity projection at angle limit boundary
		for (size_t i = 1; i < Chain.Particles.size(); ++i)
			FBoneConstraints::ProjectVelocityAtAngleLimit(Chain.Particles[i], Chain.Particles[i - 1]);

		// Clamp velocity & sleep
		XPBDSolver::ClampAndSleep(Chain.Particles, Context.MaxSpeed, Context.SleepThreshold);

		// Displacement clamping
		XPBDSolver::ClampDisplacement(Chain.Particles);
	}

	void KawaiiChainSolver::HandleCollision(FKawaiiChainData& Chain, const FKawaiiPhysicsContext& Context, FTopLevelBVH* ColliderBVH)
	{
		if (!ColliderBVH || ColliderBVH->IsEmpty()) return;

		int32_t subSteps = std::max(1, CollisionSubSteps);

		for (int32_t step = 0; step < subSteps; ++step)
		{
			float subStepFraction = (float)(step + 1) / (float)subSteps;
			int32_t startIdx = Chain.bRootCollision ? 0 : 1;

			for (int32_t i = startIdx; i < (int32_t)Chain.Particles.size(); ++i)
			{
				FSimParticle& P = Chain.Particles[i];
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

			// Segment collision
			if (bEnableSegmentCollision)
			{
				for (int32_t i = startIdx; i + 1 < (int32_t)Chain.Particles.size(); ++i)
				{
					FSimParticle& A = Chain.Particles[i];
					FSimParticle& B = Chain.Particles[i + 1];
					if (!A.bCollision || !B.bCollision) continue;

					FKawaiiAABB segBox;
					segBox.Expand(A.Position);
					segBox.Expand(B.Position);
					segBox.ExpandByRadius(std::max(A.PhysicsSettings.Radius, B.PhysicsSettings.Radius));

					std::vector<std::shared_ptr<FBoundBoxHandle>> overlaps;
					ColliderBVH->QueryOverlap(segBox, overlaps);

					for (auto& handle : overlaps)
					{
						if (handle && handle->Owner)
							handle->OnLineCollision(A, B, subStepFraction, 0);
					}
				}
			}
		}
	}

	void KawaiiChainSolver::ResetDynamics()
	{
		for (auto& chain : Chains)
		{
			for (auto& P : chain.Particles)
				P.SnapToPose();
		}
	}

	void KawaiiChainSolver::UpdatePose(
		const std::vector<v3dxVector3>& BonePositions,
		const std::vector<v3dxQuaternion>& BoneRotations,
		const std::vector<v3dxVector3>& BoneScales)
	{
		for (auto& chain : Chains)
		{
			for (size_t i = 0; i < chain.Particles.size(); ++i)
			{
				FSimParticle& P = chain.Particles[i];
				if (P.bDummy)
				{
					if (i > 0)
						P.UpdateDummyFromParent(chain.Particles[i - 1], chain.TailBoneForwardAxis, chain.TailBoneLength);
					continue;
				}

				int32_t boneIdx = P.BoneIndex;
				if (boneIdx >= 0 && boneIdx < (int32_t)BonePositions.size())
				{
					P.UpdatePoseFromExternal(BonePositions[boneIdx], BoneRotations[boneIdx], BoneScales[boneIdx]);
				}
			}
		}
	}

} // namespace KawaiiPhysics

NS_END
