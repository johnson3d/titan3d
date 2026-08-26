#pragma once
#include "SimParticle.h"
#include "SimJointHelpers.h"
#include "../KawaiiPhySettings.h"
#include "../../../../Base/perlin/perlin.h"
#include <vector>

NS_BEGIN

namespace KawaiiPhysics
{
	// =====================================================================
	// FKawaiiRibbonData - Runtime data for a single ribbon
	// =====================================================================
	//
	// A ribbon is a pure kinematic wave generator: no constraints, no collision, no dynamics
	// state. Every frame it rebuilds its joint positions analytically from the animated pose,
	// so there is nothing to integrate or snap on teleport. Angles are stored in RADIANS here;
	// the editor-facing FKawaiiRibbonSetup authors degrees and Initialize converts once.

	struct FKawaiiRibbonData
	{
		std::string Name;
		std::vector<FSimParticle> Particles;
		float TotalLength = 0.0f;

		int32_t LODThreshold = -1;
		ETailBoneAxis TailBoneForwardAxis = TBA_X_Positive;
		float TailBoneLength = 0.0f;

		// Sway
		float SwingAngleRadians = 30.0f * KAWAII_DEG_TO_RAD;
		float SwayFrequency = 1.5f;
		float Inertia = 0.5f;
		float InertiaFalloff = 0.3f;
		float TipAmplify = 1.5f;
		float AmplifyCurvePower = 2.0f;

		// Swing plane, relative to the chain rest direction
		float SwingPlaneAngleRadians = 0.0f;
		float RestTiltRadians = 0.0f;

		// Fractal noise
		float NoiseMix = 0.0f;
		int32_t NoiseLayers = 4;
		float NoiseRoughness = 0.5f;
		float NoiseScale = 1.0f;

		// Wind
		float WindResponse = 2.0f;
		float WindGustiness = 0.5f;
		float GustFrequency = 2.0f;

		FKawaiiCurve SwingAmplitudeCurve;
		FKawaiiCurve WindInfluenceCurve;

		// Per-joint local swing rotation produced by the flutter pass. Kept so a caller that
		// wants bone rotations (rather than positions) does not have to re-derive them.
		std::vector<v3dxQuaternion> SwingRotations;

		bool IsLODValid(int32_t SimLOD) const
		{
			return LODThreshold < 0 || SimLOD <= LODThreshold;
		}
	};

	// =====================================================================
	// KawaiiRibbonSolver - Top-level ribbon wave generator
	// =====================================================================
	//
	// This is NOT a physics solver. It produces an authored idle/wind wave and outputs joint
	// positions; inertia, collision and constraints are intentionally out of scope and are
	// expected to be applied by a downstream KawaiiPhysics node that consumes this output pose.
	//
	// The wave is a cascade of first-order low-pass filters along the chain:
	//   Root joint:  theta_0 = A * sin(omega * t)
	//   Child joint: applies one LPF stage to its parent's angle
	//       phase lag per stage  = atan(omega * tau)   (monotonic, accumulates down the chain)
	//       amplitude            = A * shaping(NormalizedLength)
	//
	// tau comes from Inertia and grows along the chain via InertiaFalloff. Decoupling amplitude
	// from phase is what keeps the wave clean and unconditionally stable: the LPF gain is always
	// <= 1, so the ribbon cannot blow up regardless of frequency or segment count. Positions are
	// accumulated from the root using rest directions plus each joint's swing offset, so one
	// joint crossing zero never collapses the joints below it.

	class KawaiiRibbonSolver
	{
	public:
		void Initialize(
			const std::vector<FKawaiiRibbonSetup>& Setups,
			const std::vector<v3dxVector3>& BonePositions,
			const std::vector<v3dxQuaternion>& BoneRotations,
			const std::vector<v3dxVector3>& BoneScales,
			const std::vector<int32_t>& ParentIndices);

		// Run one frame. No collider argument: a ribbon does not collide (that is the
		// downstream KawaiiPhysics node's job).
		void Simulate(const FKawaiiPhysicsContext& Context);

		void ResetDynamics();

		void UpdatePose(
			const std::vector<v3dxVector3>& BonePositions,
			const std::vector<v3dxQuaternion>& BoneRotations,
			const std::vector<v3dxVector3>& BoneScales);

		const std::vector<FKawaiiRibbonData>& GetRibbons() const { return Ribbons; }

	private:
		std::vector<FKawaiiRibbonData> Ribbons;

		// Accumulated simulation time driving the wave phase. Advanced once per Simulate().
		float RibbonTime = 0.0f;

		// Shared Perlin gradient-noise field, allocated by Initialize only when some ribbon
		// actually uses noise (the tables cost ~100KB).
		//
		// Constructed as ONE octave with unit frequency / amplitude on purpose: Perlin's own
		// octave loop hardcodes a gain of 0.5 and takes its octave count at construction, so
		// using it would strand NoiseRoughness and NoiseLayers as dead parameters. We use it
		// purely as the base gradient field and run the octaves ourselves.
		AutoPtr<Perlin> NoiseField;

		// Analytical flutter: writes Position (and SwingRotations) for every joint.
		void ApplyFlutter(FKawaiiRibbonData& Ribbon, float Time) const;

		// Wind as a positional offset on top of the flutter result.
		void ApplyWind(FKawaiiRibbonData& Ribbon, const v3dxVector3& WindForceCS, float Time, float dt) const;
	};

} // namespace KawaiiPhysics

NS_END
