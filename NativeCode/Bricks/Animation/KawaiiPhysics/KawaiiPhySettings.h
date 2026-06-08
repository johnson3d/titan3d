#pragma once
#include "KawaiiTypes.h"

NS_BEGIN

namespace KawaiiPhysics
{
	// =====================================================================
	// FKawaiiRange / FKawaiiRotationLimits - Rotation limit ranges
	// =====================================================================

	struct FKawaiiRange
	{
		bool bEnabled = false;
		float Min = -90.0f;
		float Max = 90.0f;
	};

	struct FKawaiiRotationLimits
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

	struct FKawaiiPhySettings
	{
		float Stiffness = 0.05f;
		float Damping = 0.1f;
		float WorldDampingLocation = 0.8f;
		float WorldDampingRotation = 0.8f;
		float LimitAngle = 0.0f;
		float Radius = 3.0f;
		float WindCoefficient = 1.0f;
		float DragCoefficient = 0.0f;
		float MaxFrameDisplacement = 0.0f;
	};

	// =====================================================================
	// FKawaiiPhysicsContext - Per-frame simulation context
	// =====================================================================

	struct FKawaiiPhysicsContext
	{
		float DeltaTime = 0.0f;
		float SubstepDeltaTime = 0.0f;
		int32_t SimulationLOD = 0;

		v3dxVector3 ComponentLocation = v3dxVector3(0, 0, 0);
		v3dxQuaternion ComponentRotation = v3dxQuaternion(0, 0, 0, 1);
		v3dxVector3 ComponentScale = v3dxVector3(1, 1, 1);

		v3dxVector3 PrevComponentLocation = v3dxVector3(0, 0, 0);
		v3dxQuaternion PrevComponentRotation = v3dxQuaternion(0, 0, 0, 1);

		v3dxVector3 Gravity = v3dxVector3(0, 0, -980.0f);
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
		float TeleportSpeedThreshold = 300.0f;
		float TeleportRotationThreshold = -1.0f;
		float TeleportAngularSpeedThreshold = 100.0f;

		EKawaiiCurveEvalMode PhysicsCurveMode = KCEM_LengthRate;
	};

	// =====================================================================
	// FKawaiiChainSetup - Chain simulation structure definition (engine-side)
	// =====================================================================

	struct FKawaiiChainSetup
	{
		std::string Name;
		int32_t RootBoneIndex = -1;
		int32_t EndBoneIndex = -1;
		bool bDirectPathOnly = false;
		bool bRootCollision = false;
		bool bConstrainBoneLength = true;
		float BoneLengthConstraintBlend = 1.0f;
		FKawaiiRotationLimits RotationLimits;
		float TailBoneLength = 0.0f;
		ETailBoneAxis TailBoneForwardAxis = TBA_X_Positive;
		int32_t LODThreshold = -1;
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

		float ClothThickness = 1.0f;
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
		int32_t LODThreshold = -1;
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
