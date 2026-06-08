#include "SimParticle.h"

NS_BEGIN

namespace KawaiiPhysics
{
	void FSimParticle::Initialize(const v3dxVector3& InRestPosition, const v3dxQuaternion& InRefRotation,
								  const v3dxVector3& InPosePosition, const v3dxQuaternion& InPoseRotation,
								  const v3dxVector3& InPoseScale)
	{
		RestPosition = InRestPosition;
		RefRotation = InRefRotation;

		if (PinMode == KPM_Kinematic || PinMode == KPM_Static)
			InverseMass = 0.0f;

		bDummy = false;
		Position = InPosePosition;
		PrevPosition = InPosePosition;
		PosePosition = InPosePosition;
		PrevRotation = InPoseRotation;
		PoseRotation = InPoseRotation;
		PoseScale = InPoseScale;
		IsLODValidToEvaluate = true;
	}

	void FSimParticle::InitializeDummy(ETailBoneAxis ForwardAxis, float TailBoneLength, const FSimParticle& EndJoint)
	{
		bDummy = true;
		IsLODValidToEvaluate = EndJoint.IsLODValidToEvaluate;
		RestPosition = EndJoint.RestPosition + GetBoneForwardVector(ForwardAxis, EndJoint.RefRotation) * TailBoneLength;
		RefRotation = EndJoint.RefRotation;

		if (EndJoint.IsLODValidToEvaluate)
		{
			Position = EndJoint.Position + GetBoneForwardVector(ForwardAxis, EndJoint.PrevRotation) * TailBoneLength;
			PrevPosition = Position;
			PosePosition = Position;
			PrevRotation = EndJoint.PrevRotation;
			PoseRotation = PrevRotation;
			PoseScale = EndJoint.PoseScale;
		}
	}

} // namespace KawaiiPhysics

NS_END
