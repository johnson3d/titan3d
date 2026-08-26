#pragma once
#include "KawaiiTypes.h"

NS_BEGIN

namespace KawaiiPhysics
{
	// =====================================================================
	// FKawaiiRange / FKawaiiRotationLimits - Rotation limit ranges
	// =====================================================================

	struct TR_CLASS(SV_LayoutStruct = 8)
		FKawaiiRange
	{
		bool bEnabled = false;
		float Min = -90.0f;
		float Max = 90.0f;
	};

	struct TR_CLASS(SV_LayoutStruct = 8)
		FKawaiiRotationLimits
	{
		FKawaiiRange PitchLimit;
		FKawaiiRange YawLimit;
		FKawaiiRange RollLimit;

		bool IsEnabled() const
		{
			return PitchLimit.bEnabled || YawLimit.bEnabled || RollLimit.bEnabled;
		}
	};

	// =====================================================================
	// FKawaiiPhySettings - Base physics simulation parameters per particle
	// =====================================================================
	//
	// UNIT CONVENTION: all lengths / speeds / accelerations here are in the engine's
	// skeleton mesh space, which is METERS (a humanoid is ~1.5 tall, PhysX main scene
	// gravity is -9.8). The upstream KawaiiPhysics plugin is a UE plugin and authored
	// every default in CENTIMETERS; those numbers must be divided by 100 when ported.
	// Leaving a cm-era default in place (e.g. Gravity -980) makes one frame of free fall
	// longer than a whole bone chain, and the length constraint then pins the chain
	// straight down every frame == "hair hangs dead, never swings".

	struct TR_CLASS(SV_LayoutStruct = 8)
		FKawaiiPhySettings
	{
		// Pull back toward the animated pose. 0 = pure physics (chain never returns to the
		// animated shape), 1 = snap to animation. Applied by XPBDSolver::ApplyPoseStiffness.
		float Stiffness = 0.05f;
		float Damping = 0.1f;
		// How much of the component's world motion this particle does NOT follow (= lag).
		// 1 = keeps its world position (max flow), 0 = rigidly glued to the actor (no flow).
		// Applied by XPBDSolver::ApplyWorldMoveLag.
		float WorldDampingLocation = 0.8f;
		float WorldDampingRotation = 0.8f;
		float LimitAngle = 0.0f;
		float Radius = 0.03f;            // meters (upstream cm default was 3.0)
		float WindCoefficient = 1.0f;
		float DragCoefficient = 0.0f;
		float MaxFrameDisplacement = 0.0f;
	};

	// =====================================================================
	// FKawaiiPhysicsContext - Per-frame simulation context
	// =====================================================================

	struct TR_CLASS(SV_LayoutStruct = 8)FKawaiiPhysicsContext
	{
		float DeltaTime = 0.0f;
		float SubstepDeltaTime = 0.0f;
		int32_t SimulationLOD = 0;

		v3dxVector3 ComponentLocation = v3dxVector3(0, 0, 0);
		v3dxQuaternion ComponentRotation = v3dxQuaternion(0, 0, 0, 1);
		v3dxVector3 ComponentScale = v3dxVector3(1, 1, 1);

		v3dxVector3 PrevComponentLocation = v3dxVector3(0, 0, 0);
		v3dxQuaternion PrevComponentRotation = v3dxQuaternion(0, 0, 0, 1);

		// Engine world/skeleton space is Y-up AND METERS: matches the PhysX main scene
		// gravity (0,-9.8,0) and the skeleton mesh-space where +Y points up (foot Y~0.06,
		// head top Y~1.47). Two historical bugs lived on this single line:
		//   1) Z-up (0,0,-980) made chains fall toward -Z ("backwards") and curl into a ball;
		//   2) the cm-era magnitude 980 is 100x too strong for a meter-scale skeleton, so one
		//      frame of free fall (~0.27m at 60fps) exceeded a whole twintail segment and the
		//      bone-length constraint pinned the chain straight down every frame.
		v3dxVector3 Gravity = v3dxVector3(0, -9.8f, 0);
		float GravityScale = 1.0f;

		v3dxVector3 WindForce = v3dxVector3(0, 0, 0);
		bool bEnableWind = false;

		float SpeedScale = 1.0f;
		float MaxSpeed = -1.0f;
		float SleepThreshold = 0.0f;

		int32_t ConstraintIterations = 3;
		int32_t CollisionSubSteps = 1;
		int32_t SimulationFPS = 120;

		float TeleportDistanceThreshold = -1.0f;
		float TeleportSpeedThreshold = 3.0f;         // m/s (upstream cm default was 300)
		float TeleportRotationThreshold = -1.0f;
		float TeleportAngularSpeedThreshold = 100.0f;

		EKawaiiCurveEvalMode PhysicsCurveMode = KCEM_LengthRate;
	};

	// =====================================================================
	// FKawaiiChainSetup - Chain simulation structure definition (engine-side)
	// =====================================================================

	struct TR_CLASS(SV_LayoutStruct = 8)
		FKawaiiChainSetup
	{
		std::string Name;
		int32_t RootBoneIndex = -1;
		int32_t EndBoneIndex = -1;
		bool bDirectPathOnly = false;
		bool bRootCollision = false;
		bool bConstrainBoneLength = true;
		float BoneLengthConstraintBlend = 1.0f;
		FKawaiiRotationLimits RotationLimits;
		FKawaiiPhySettings PhysicsSettings;
		FKawaiiPhySettings PhysicsSettingsRandom;
		float TailBoneLength = 0.0f;
		ETailBoneAxis TailBoneForwardAxis = TBA_X_Positive;
		int32_t LODThreshold = -1;

		// Per-bone parameter curves sampled by CurveMode along the chain. Each is a multiplier
		// on the corresponding base scalar in PhysicsSettings (empty curve -> 1.0 = uniform).
		// Applied per particle in SimJointHelpers::UpdatePhysicsSettings.
		EKawaiiCurveEvalMode CurveMode = KCEM_LengthRate;
		FKawaiiCurve StiffnessCurve;
		FKawaiiCurve DampingCurve;
		FKawaiiCurve WorldDampingLocationCurve;
		FKawaiiCurve WorldDampingRotationCurve;
		FKawaiiCurve LimitAngleCurve;
		FKawaiiCurve RadiusCurve;
		FKawaiiCurve DragCurve;
		FKawaiiCurve WindCurve;
	};

	// =====================================================================
	// FKawaiiClothSetup - Cloth simulation structure definition
	// =====================================================================

	struct FKawaiiClothSetup : public FKawaiiChainSetup
	{
		bool bLoopChains = false;
		int32_t VerticalConstraintStep = 1;

		bool bStructuralVerticalConstraint = true;
		bool bStructuralVerticalCollision = true;
		FKawaiiCurve VerticalShrinkStiffness;
		FKawaiiCurve VerticalStretchStiffness;

		bool bStructuralHorizontalConstraint = false;
		bool bStructuralHorizontalCollision = false;
		FKawaiiCurve HorizontalShrinkStiffness;
		FKawaiiCurve HorizontalStretchStiffness;

		bool bBendingVerticalConstraint = false;
		FKawaiiCurve VerticalBendStiffness;
		float VerticalBendDeadZone = 0.0f;

		bool bBendingHorizontalConstraint = false;
		FKawaiiCurve HorizontalBendStiffness;
		float HorizontalBendDeadZone = 0.0f;

		bool bShearConstraint = false;
		bool bShearCollision = false;
		FKawaiiCurve ShearShrinkStiffness;
		FKawaiiCurve ShearStretchStiffness;

		float ClothThickness = 0.01f;                // meters (upstream cm default was 1.0)
		float ClothFriction = 0.2f;
	};

	// =====================================================================
	// FKawaiiRodSetup - Cosserat rod simulation structure definition
	// =====================================================================

	struct FKawaiiRodSetup
	{
		std::string Name;
		int32_t RootBoneIndex = -1;
		int32_t EndBoneIndex = -1;
		float StretchAndShearStiffness = 1.0f;
		float BendAndTwistStiffness = 0.05f;
		float PointAttachmentStiffness = 0.10f;
		float OrientationAttachmentStiffness = 0.05f;
		FKawaiiPhySettings PhysicsSettings;
		FKawaiiPhySettings PhysicsSettingsRandom;
		int32_t LODThreshold = -1;

		// Per-segment stiffness curves (multiplier on the base scalar; empty -> 1.0).
		FKawaiiCurve StretchAndShearStiffnessCurve;
		FKawaiiCurve BendAndTwistStiffnessCurve;
		FKawaiiCurve PointAttachmentStiffnessCurve;
		FKawaiiCurve OrientationAttachmentStiffnessCurve;
	};

	// =====================================================================
	// FKawaiiRibbonSetup - Ribbon simulation structure definition
	// =====================================================================
	//
	// A ribbon is a PURE KINEMATIC wave generator, not a solver: it drives each joint by an
	// analytical cascaded low-pass filter (see KawaiiRibbonSolver.h) plus optional wind, and
	// outputs positions. It deliberately owns NO physics: no gravity, no collision, no bone
	// length / angle / rotation constraints, no world-inertia. Those all live in KawaiiPhysics.
	// The intended use is to feed a ribbon node's output pose into a downstream KawaiiPhysics
	// node, which then applies inertia / collision / constraints on top of the authored wave.
	// Keeping any of that here would just duplicate the downstream node's knobs.
	//
	// UNIT CONVENTION: angles here are DEGREES (editor-facing) and are converted to radians
	// once when the runtime data is built. Lengths are meters, like everywhere else.

	struct FKawaiiRibbonSetup
	{
		std::string Name;
		int32_t RootBoneIndex = -1;
		int32_t EndBoneIndex = -1;
		float TailBoneLength = 0.0f;
		ETailBoneAxis TailBoneForwardAxis = TBA_X_Positive;
		int32_t LODThreshold = -1;

		// ---- Sway ----
		// Maximum swing angle of the root joint.
		float SwingAngleDegrees = 30.0f;
		// Oscillations per second.
		float SwayFrequency = 1.5f;
		// Phase lag of each joint behind its parent. 0 = rigid follow, 1 = heavy trailing wave.
		float Inertia = 0.5f;
		// How much that lag grows from root to tip. 0 = uniform, 1 = tip lags far more.
		float InertiaFalloff = 0.3f;
		// Tip amplitude multiplier relative to the root (ignored when SwingAmplitudeCurve is set).
		float TipAmplify = 1.5f;
		// Shape of the root->tip amplitude growth: <1 front-loaded, 1 linear, 2 accelerating.
		float AmplifyCurvePower = 2.0f;

		// ---- Swing plane ----
		// Both angles are relative to the chain's own rest direction, so they mean the same
		// thing regardless of how the ribbon is boned. The chain direction always lies in the
		// swing plane.
		// Rotation of the swing plane about the chain: 0 swings fore/aft, +-90 swings sideways.
		float SwingPlaneAngleDegrees = 0.0f;
		// Where the ribbon rests inside that plane - the 0 degree the oscillation centres on.
		float RestTiltAngleDegrees = 0.0f;

		// ---- Organic noise (fractal brownian motion) ----
		// Blend between the pure sine wave (0) and fractal noise (1).
		float NoiseMix = 0.0f;
		int32_t NoiseLayers = 4;
		// Amplitude decay per octave: lower = smoother, higher = rougher.
		float NoiseRoughness = 0.5f;
		float NoiseScale = 1.0f;

		// ---- Wind ----
		// Wind is kept on the ribbon (not delegated downstream) because gust modulation and the
		// per-length wind curve are part of the authored wave feel and have no KawaiiPhysics
		// equivalent.
		float WindResponse = 2.0f;
		// Sinusoidal gust modulation on top of the steady wind.
		float WindGustiness = 0.5f;
		float GustFrequency = 2.0f;

		// Sampled by NormalizedLength (0 = root, 1 = tip). Empty -> Evaluate returns 1.
		// SwingAmplitudeCurve overrides the TipAmplify / AmplifyCurvePower shaping when set.
		FKawaiiCurve SwingAmplitudeCurve;
		FKawaiiCurve WindInfluenceCurve;
	};

	// =====================================================================
	// LOD Settings
	// =====================================================================

	struct FChainLODSettings
	{
		int32_t LODLevel = 0;
		bool bEnableSegmentCollision = false;
		int32_t CollisionSubSteps = 1;
		float SpeedScale = 1.0f;
		bool bDisableSimulation = false;
	};

} // namespace KawaiiPhysics

NS_END
