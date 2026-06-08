#pragma once
#include "../KawaiiPhySettings.h"

NS_BEGIN

namespace KawaiiPhysics
{
	// =====================================================================
	// FSimParticle - Particle data for runtime physics simulation.
	//
	// Each particle corresponds to a node on a bone chain, holding simulation
	// state such as position, rotation, and velocity, as well as constraint
	// parameters like PinMode and InverseMass.
	// =====================================================================

	struct FSimParticle
	{
		// -----------------------------------------------------------------
		// Bone reference and physics settings
		// -----------------------------------------------------------------
		int32_t BoneIndex = -1;
		int32_t KinematicBoneIndex = -1;
		FKawaiiPhySettings PhysicsSettings;
		EKawaiiPinMode PinMode = KPM_Dynamic;
		float InverseMass = 1.0f;

		// -----------------------------------------------------------------
		// Simulation state
		// -----------------------------------------------------------------
		v3dxVector3 Velocity = v3dxVector3(0, 0, 0);

		v3dxVector3 Position = v3dxVector3(0, 0, 0);
		v3dxVector3 PrevPosition = v3dxVector3(0, 0, 0);
		v3dxVector3 PosePosition = v3dxVector3(0, 0, 0);
		v3dxVector3 RestPosition = v3dxVector3(0, 0, 0);
		v3dxVector3 FrameStartPosition = v3dxVector3(0, 0, 0);

		v3dxQuaternion PrevRotation = v3dxQuaternion(0, 0, 0, 1);
		v3dxQuaternion PoseRotation = v3dxQuaternion(0, 0, 0, 1);
		v3dxQuaternion RefRotation = v3dxQuaternion(0, 0, 0, 1);

		v3dxVector3 PoseScale = v3dxVector3(1, 1, 1);

		// -----------------------------------------------------------------
		// Flags and auxiliary data
		// -----------------------------------------------------------------
		bool bDummy = false;
		bool bCollision = true;
		bool IsLODValidToEvaluate = true;

		float LengthFromRoot = 0.0f;
		float NormalizedLength = 0.0f;
		float Friction = 0.0f;
		v3dxVector3 ContactNormal = v3dxVector3(0, 0, 0);

		// -----------------------------------------------------------------
		// Initialization interface
		// -----------------------------------------------------------------

		void Initialize(const v3dxVector3& InRestPosition, const v3dxQuaternion& InRefRotation,
						const v3dxVector3& InPosePosition, const v3dxQuaternion& InPoseRotation,
						const v3dxVector3& InPoseScale);

		void InitializeDummy(ETailBoneAxis ForwardAxis, float TailBoneLength, const FSimParticle& EndJoint);

		// -----------------------------------------------------------------
		// Query interface
		// -----------------------------------------------------------------

		const v3dxVector3& GetPosition() const
		{
			if (PinMode == KPM_Kinematic && KinematicBoneIndex >= 0)
				return PosePosition;
			return Position;
		}

		float GetWeight() const
		{
			return (PinMode == KPM_Dynamic) ? 1.0f : 0.0f;
		}

		// -----------------------------------------------------------------
		// Reset simulation state to current pose
		// -----------------------------------------------------------------

		void SnapToPose()
		{
			Position = PosePosition;
			PrevPosition = PosePosition;
			PrevRotation = PoseRotation;
			Velocity = v3dxVector3(0, 0, 0);
		}

		void UpdatePoseFromExternal(const v3dxVector3& InPosition, const v3dxQuaternion& InRotation, const v3dxVector3& InScale)
		{
			bool wasInvalid = !IsLODValidToEvaluate;
			PosePosition = InPosition;
			PoseRotation = InRotation;
			PoseScale = InScale;
			IsLODValidToEvaluate = true;

			if (wasInvalid)
			{
				SnapToPose();
				if (PinMode == KPM_Kinematic || PinMode == KPM_Static)
					InverseMass = 0.0f;
			}
		}

		void UpdateDummyFromParent(const FSimParticle& ParentJoint, ETailBoneAxis ForwardAxis, float TailBoneLength)
		{
			if (!ParentJoint.IsLODValidToEvaluate)
			{
				IsLODValidToEvaluate = false;
				return;
			}

			PosePosition = ParentJoint.PosePosition + GetBoneForwardVector(ForwardAxis, ParentJoint.PoseRotation) * TailBoneLength;
			PoseRotation = ParentJoint.PoseRotation;
			PoseScale = ParentJoint.PoseScale;

			if (!IsLODValidToEvaluate)
			{
				SnapToPose();
				bDummy = true;
			}
			IsLODValidToEvaluate = ParentJoint.IsLODValidToEvaluate;
		}
	};

} // namespace KawaiiPhysics

NS_END
