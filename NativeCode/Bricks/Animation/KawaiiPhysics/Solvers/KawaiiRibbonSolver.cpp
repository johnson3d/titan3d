#include "KawaiiRibbonSolver.h"
#include <cmath>

NS_BEGIN

namespace KawaiiPhysics
{
	// =====================================================================
	// FBM noise built on the engine's Perlin gradient field (Base/perlin).
	// =====================================================================

	static float RibbonFBM(Perlin* Field, float X, float Y, int32_t Octaves, float Persistence)
	{
		float value = 0.0f;
		float amplitude = 1.0f;
		float frequency = 1.0f;
		float maxAmplitude = 0.0f;

		const int32_t octaves = KawaiiClamp(Octaves, 1, 8);
		for (int32_t i = 0; i < octaves; ++i)
		{
			value += amplitude * (float)Field->Get(X * frequency, Y * frequency);
			maxAmplitude += amplitude;
			amplitude *= Persistence;
			frequency *= 2.0f;
		}
		return (maxAmplitude > KAWAII_SMALL_NUMBER) ? (value / maxAmplitude) : 0.0f;
	}

	static float EvaluateRibbonWave(const FKawaiiRibbonData& Ribbon, Perlin* NoiseField, float Phase, float SpatialCoord)
	{
		const float sine = sinf(Phase);
		if (!NoiseField || Ribbon.NoiseMix < KAWAII_SMALL_NUMBER)
			return sine;

		const float noise = RibbonFBM(
			NoiseField,
			Phase * (1.0f / (2.0f * KAWAII_PI)) * Ribbon.NoiseScale,
			SpatialCoord * Ribbon.NoiseScale,
			Ribbon.NoiseLayers,
			Ribbon.NoiseRoughness);

		const float mix = KawaiiClamp(Ribbon.NoiseMix, 0.0f, 1.0f);
		return sine * (1.0f - mix) + noise * mix;
	}

	// =====================================================================
	// Initialization
	// =====================================================================

	void KawaiiRibbonSolver::Initialize(
		const std::vector<FKawaiiRibbonSetup>& Setups,
		const std::vector<v3dxVector3>& BonePositions,
		const std::vector<v3dxQuaternion>& BoneRotations,
		const std::vector<v3dxVector3>& BoneScales,
		const std::vector<int32_t>& ParentIndices)
	{
		Ribbons.clear();
		Ribbons.resize(Setups.size());
		RibbonTime = 0.0f;

		for (size_t i = 0; i < Setups.size(); ++i)
		{
			const FKawaiiRibbonSetup& setup = Setups[i];
			FKawaiiRibbonData& ribbon = Ribbons[i];

			ribbon.Name = setup.Name;
			ribbon.LODThreshold = setup.LODThreshold;
			ribbon.TailBoneForwardAxis = setup.TailBoneForwardAxis;
			ribbon.TailBoneLength = setup.TailBoneLength;

			ribbon.SwingAngleRadians = std::max(0.0f, setup.SwingAngleDegrees) * KAWAII_DEG_TO_RAD;
			ribbon.SwingPlaneAngleRadians = KawaiiClamp(setup.SwingPlaneAngleDegrees, -90.0f, 90.0f) * KAWAII_DEG_TO_RAD;
			ribbon.RestTiltRadians = KawaiiClamp(setup.RestTiltAngleDegrees, -90.0f, 90.0f) * KAWAII_DEG_TO_RAD;

			ribbon.SwayFrequency = std::max(0.0f, setup.SwayFrequency);
			ribbon.Inertia = KawaiiClamp(setup.Inertia, 0.0f, 1.0f);
			ribbon.InertiaFalloff = KawaiiClamp(setup.InertiaFalloff, 0.0f, 1.0f);
			ribbon.TipAmplify = std::max(0.0f, setup.TipAmplify);
			ribbon.AmplifyCurvePower = std::max(0.2f, setup.AmplifyCurvePower);

			ribbon.NoiseMix = KawaiiClamp(setup.NoiseMix, 0.0f, 1.0f);
			ribbon.NoiseLayers = KawaiiClamp(setup.NoiseLayers, 1, 8);
			ribbon.NoiseRoughness = KawaiiClamp(setup.NoiseRoughness, 0.1f, 1.0f);
			ribbon.NoiseScale = std::max(0.1f, setup.NoiseScale);

			ribbon.WindResponse = std::max(0.0f, setup.WindResponse);
			ribbon.WindGustiness = std::max(0.0f, setup.WindGustiness);
			ribbon.GustFrequency = std::max(0.0f, setup.GustFrequency);

			ribbon.SwingAmplitudeCurve = setup.SwingAmplitudeCurve;
			ribbon.WindInfluenceCurve = setup.WindInfluenceCurve;

			SimJointHelpers::BuildChainParticles(
				BonePositions, BoneRotations, BoneScales, ParentIndices,
				setup.RootBoneIndex, setup.EndBoneIndex,
				setup.TailBoneForwardAxis, setup.TailBoneLength,
				ribbon.Particles);

			ribbon.SwingRotations.assign(ribbon.Particles.size(), v3dxQuaternion(0, 0, 0, 1));

			if (!ribbon.Particles.empty())
				ribbon.TotalLength = ribbon.Particles.back().LengthFromRoot;
		}

		// Allocate the shared noise field only if something uses noise.
		bool bNeedsNoise = false;
		for (const auto& ribbon : Ribbons)
		{
			if (ribbon.NoiseMix >= KAWAII_SMALL_NUMBER)
			{
				bNeedsNoise = true;
				break;
			}
		}
		if (bNeedsNoise && NoiseField == nullptr)
			NoiseField.UnsafeSet(new Perlin(1, 1.0, 1.0, 0x4B1B));
	}

	// =====================================================================
	// Simulate: pose -> flutter -> wind -> output
	// =====================================================================

	void KawaiiRibbonSolver::Simulate(const FKawaiiPhysicsContext& Context)
	{
		const float dt = Context.SubstepDeltaTime > 0.0f ? Context.SubstepDeltaTime : Context.DeltaTime;
		if (dt < KAWAII_SMALL_NUMBER) return;

		RibbonTime += dt;

		for (auto& ribbon : Ribbons)
		{
			if (!ribbon.IsLODValid(Context.SimulationLOD))
			{
				for (auto& P : ribbon.Particles)
					P.Position = P.PosePosition;
				continue;
			}

			if (ribbon.Particles.size() < 2)
			{
				for (auto& P : ribbon.Particles)
					P.Position = P.PosePosition;
				continue;
			}

			// Start from the animated pose every frame.
			for (auto& P : ribbon.Particles)
				P.Position = P.PosePosition;

			ApplyFlutter(ribbon, RibbonTime);

			if (Context.bEnableWind)
				ApplyWind(ribbon, Context.WindForce, RibbonTime, dt);

			// Pin root
			ribbon.Particles[0].Position = ribbon.Particles[0].PosePosition;
		}
	}

	// =====================================================================
	// ApplyFlutter - cascaded first-order low-pass filter (analytical)
	// =====================================================================

	void KawaiiRibbonSolver::ApplyFlutter(FKawaiiRibbonData& Ribbon, float Time) const
	{
		const int32_t numParticles = (int32_t)Ribbon.Particles.size();
		if (numParticles < 2) return;

		const v3dxVector3 rootChainDir = Vec3SafeNormal(
			Ribbon.Particles[1].RestPosition - Ribbon.Particles[0].RestPosition);
		if (Vec3IsNearlyZero(rootChainDir)) return;

		// Build swing axis perpendicular to the chain rest direction.
		v3dxVector3 baseAxis = Vec3Cross(rootChainDir, v3dxVector3(0, 0, 1));
		if (Vec3IsNearlyZero(baseAxis))
			baseAxis = Vec3Cross(rootChainDir, v3dxVector3(0, 1, 0));
		if (Vec3IsNearlyZero(baseAxis))
			baseAxis = Vec3Cross(rootChainDir, v3dxVector3(1, 0, 0));
		baseAxis = Vec3SafeNormal(baseAxis);
		if (Vec3IsNearlyZero(baseAxis)) return;

		const v3dxVector3 swingPlaneNormal = Vec3SafeNormal(
			QuatRotateVector(QuatFromAxisAngle(rootChainDir, Ribbon.SwingPlaneAngleRadians), baseAxis));
		if (Vec3IsNearlyZero(swingPlaneNormal)) return;

		const float omega = Ribbon.SwayFrequency * 2.0f * KAWAII_PI;
		const float tauBase = (2.0f / std::max(omega, KAWAII_SMALL_NUMBER)) * Ribbon.Inertia;

		if ((int32_t)Ribbon.SwingRotations.size() != numParticles)
			Ribbon.SwingRotations.assign(numParticles, v3dxQuaternion(0, 0, 0, 1));
		Ribbon.SwingRotations[0] = v3dxQuaternion(0, 0, 0, 1);

		Ribbon.Particles[0].Position = Ribbon.Particles[0].PosePosition;

		v3dxVector3 accumulatedPosition = Ribbon.Particles[0].PosePosition;
		float accumulatedPhaseLag = 0.0f;

		for (int32_t i = 1; i < numParticles; ++i)
		{
			FSimParticle& joint = Ribbon.Particles[i];
			const FSimParticle& parent = Ribbon.Particles[i - 1];

			if (joint.PinMode != KPM_Dynamic && !joint.bDummy)
			{
				joint.Position = joint.PosePosition;
				accumulatedPosition = joint.PosePosition;
				Ribbon.SwingRotations[i] = v3dxQuaternion(0, 0, 0, 1);
				continue;
			}

			const float segmentRestLength = Vec3Distance(joint.RestPosition, parent.RestPosition);
			const float normalizedIndex = (float)i / (float)(numParticles - 1);
			const float tauLocal = tauBase * (1.0f + Ribbon.InertiaFalloff * normalizedIndex);

			accumulatedPhaseLag += atanf(omega * tauLocal);

			float amplitude = Ribbon.SwingAngleRadians;
			if (!Ribbon.SwingAmplitudeCurve.IsEmpty())
			{
				amplitude *= std::max(0.0f, Ribbon.SwingAmplitudeCurve.Evaluate(joint.NormalizedLength));
			}
			else
			{
				const float shapedNL = powf(joint.NormalizedLength, Ribbon.AmplifyCurvePower);
				amplitude *= 1.0f + (Ribbon.TipAmplify - 1.0f) * shapedNL;
			}

			const float wave = EvaluateRibbonWave(Ribbon, NoiseField.GetPtr(), Time * omega - accumulatedPhaseLag, joint.NormalizedLength);
			const float segmentAngle = Ribbon.RestTiltRadians + amplitude * wave;

			const v3dxVector3 parentToChildRest = Vec3SafeNormal(joint.RestPosition - parent.RestPosition);
			const v3dxQuaternion localSwing = QuatFromAxisAngle(swingPlaneNormal, segmentAngle);
			const v3dxVector3 swungDirection = QuatRotateVector(localSwing, parentToChildRest);

			accumulatedPosition = accumulatedPosition + swungDirection * segmentRestLength;
			joint.Position = accumulatedPosition;
			Ribbon.SwingRotations[i] = localSwing;
		}
	}

	// =====================================================================
	// ApplyWind - steady wind plus sinusoidal gusts as a positional offset
	// =====================================================================

	void KawaiiRibbonSolver::ApplyWind(FKawaiiRibbonData& Ribbon, const v3dxVector3& WindForceCS, float Time, float dt) const
	{
		const int32_t numParticles = (int32_t)Ribbon.Particles.size();
		if (numParticles < 2) return;
		if (Vec3IsNearlyZero(WindForceCS) || Ribbon.WindResponse < KAWAII_SMALL_NUMBER) return;

		for (int32_t i = 1; i < numParticles; ++i)
		{
			FSimParticle& joint = Ribbon.Particles[i];
			if (joint.PinMode != KPM_Dynamic && !joint.bDummy) continue;

			const float localWindScale = Ribbon.WindResponse
				* std::max(0.0f, Ribbon.WindInfluenceCurve.Evaluate(joint.NormalizedLength));

			const float turbulence = 1.0f + Ribbon.WindGustiness
				* sinf(Time * Ribbon.GustFrequency * 2.0f * KAWAII_PI + joint.NormalizedLength * 5.0f);

			const v3dxVector3 windForce = WindForceCS * (localWindScale * turbulence);
			// InverseMass on a kinematic particle is 0, which is fine: it just won't move.
			joint.Position = joint.Position + windForce * (joint.InverseMass * dt);
		}
	}

	// =====================================================================
	// Reset / pose update
	// =====================================================================

	void KawaiiRibbonSolver::ResetDynamics()
	{
		for (auto& ribbon : Ribbons)
		{
			for (auto& P : ribbon.Particles)
				P.SnapToPose();
		}
	}

	void KawaiiRibbonSolver::UpdatePose(
		const std::vector<v3dxVector3>& BonePositions,
		const std::vector<v3dxQuaternion>& BoneRotations,
		const std::vector<v3dxVector3>& BoneScales)
	{
		for (auto& ribbon : Ribbons)
		{
			for (size_t i = 0; i < ribbon.Particles.size(); ++i)
			{
				FSimParticle& P = ribbon.Particles[i];
				if (P.bDummy)
				{
					if (i > 0)
						P.UpdateDummyFromParent(ribbon.Particles[i - 1], ribbon.TailBoneForwardAxis, ribbon.TailBoneLength);
					continue;
				}

				const int32_t boneIdx = P.BoneIndex;
				if (boneIdx >= 0 && boneIdx < (int32_t)BonePositions.size())
					P.UpdatePoseFromExternal(BonePositions[boneIdx], BoneRotations[boneIdx], BoneScales[boneIdx]);
			}
		}
	}

} // namespace KawaiiPhysics

NS_END
