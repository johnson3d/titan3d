#include "BoneConstraints.h"
#include <cmath>

NS_BEGIN

namespace KawaiiPhysics
{
	void FBoneConstraints::ApplyLengthConstraint(float BoneLengthConstraintBlend, FSimParticle& Particle, const FSimParticle& ParentJoint)
	{
		if (!Particle.IsLODValidToEvaluate || !ParentJoint.IsLODValidToEvaluate)
			return;

		const float InitBoneLength = Particle.LengthFromRoot - ParentJoint.LengthFromRoot;
		const float CurrentLength = Vec3Length(Particle.Position - ParentJoint.Position);
		const v3dxVector3 Dir = Vec3SafeNormal(Particle.Position - ParentJoint.Position);
		const float BlendedLength = (1.0f - BoneLengthConstraintBlend) * CurrentLength + BoneLengthConstraintBlend * InitBoneLength;
		Particle.Position = ParentJoint.Position + Dir * BlendedLength;
	}

	void FBoneConstraints::ApplyAngleLimit(FSimParticle& Particle, const FSimParticle& ParentJoint)
	{
		if (Particle.PhysicsSettings.LimitAngle <= 0.0f || !Particle.IsLODValidToEvaluate || !ParentJoint.IsLODValidToEvaluate)
			return;

		const v3dxVector3 ToChild = Particle.Position - ParentJoint.Position;
		const float BoneLength = Vec3Length(ToChild);
		if (BoneLength < KAWAII_SMALL_NUMBER) return;

		const v3dxVector3 BoneDir = ToChild * (1.0f / BoneLength);
		const v3dxVector3 PoseDir = Vec3SafeNormal(Particle.PosePosition - ParentJoint.PosePosition);
		if (Vec3IsNearlyZero(PoseDir)) return;

		const float CosAngle = Vec3Dot(PoseDir, BoneDir);
		const float LimitRad = Particle.PhysicsSettings.LimitAngle * (3.14159265f / 180.0f);
		const float CosLimit = cosf(LimitRad);

		if (CosAngle >= CosLimit) return;

		const v3dxVector3 PerpDir = Vec3SafeNormal(BoneDir - PoseDir * CosAngle);
		if (Vec3IsNearlyZero(PerpDir)) return;

		const v3dxVector3 ClampedDir = PoseDir * CosLimit + PerpDir * sinf(LimitRad);
		Particle.Position = ParentJoint.Position + ClampedDir * BoneLength;
	}

	void FBoneConstraints::ApplyRotationLimits(FSimParticle& Particle, const FSimParticle& ParentJoint, const FKawaiiRotationLimits& RotationLimits)
	{
		if (!Particle.IsLODValidToEvaluate || !ParentJoint.IsLODValidToEvaluate)
			return;
		if (!RotationLimits.IsEnabled())
			return;

		const v3dxVector3 PoseVector = Particle.PosePosition - ParentJoint.PosePosition;
		v3dxVector3 SimulateVector = Particle.GetPosition() - ParentJoint.GetPosition();

		// Compute rotation from pose to simulated direction
		v3dxVector3 poseDir = Vec3SafeNormal(PoseVector);
		v3dxVector3 simDir = Vec3SafeNormal(SimulateVector);

		if (Vec3IsNearlyZero(poseDir) || Vec3IsNearlyZero(simDir))
			return;

		// Decompose the rotation into Euler angles and clamp
		v3dxVector3 crossVec = Vec3Cross(poseDir, simDir);
		float dotVal = Vec3Dot(poseDir, simDir);
		dotVal = KawaiiClamp(dotVal, -1.0f, 1.0f);
		float angle = acosf(dotVal);

		if (angle < KAWAII_SMALL_NUMBER) return;

		v3dxVector3 axis = Vec3SafeNormal(crossVec);
		if (Vec3IsNearlyZero(axis)) return;

		// Decompose angle into pitch/yaw/roll components relative to pose orientation
		v3dxVector3 localAxis = QuatRotateVector(QuatInverse(Particle.PoseRotation), axis);
		float pitchAngle = localAxis.X * angle * (180.0f / 3.14159265f);
		float yawAngle = localAxis.Y * angle * (180.0f / 3.14159265f);
		float rollAngle = localAxis.Z * angle * (180.0f / 3.14159265f);

		bool clamped = false;
		if (RotationLimits.PitchLimit.bEnabled)
		{
			float clampedPitch = KawaiiClampAngle(pitchAngle, RotationLimits.PitchLimit.Min, RotationLimits.PitchLimit.Max);
			if (fabsf(clampedPitch - pitchAngle) > KAWAII_SMALL_NUMBER) { pitchAngle = clampedPitch; clamped = true; }
		}
		if (RotationLimits.YawLimit.bEnabled)
		{
			float clampedYaw = KawaiiClampAngle(yawAngle, RotationLimits.YawLimit.Min, RotationLimits.YawLimit.Max);
			if (fabsf(clampedYaw - yawAngle) > KAWAII_SMALL_NUMBER) { yawAngle = clampedYaw; clamped = true; }
		}
		if (RotationLimits.RollLimit.bEnabled)
		{
			float clampedRoll = KawaiiClampAngle(rollAngle, RotationLimits.RollLimit.Min, RotationLimits.RollLimit.Max);
			if (fabsf(clampedRoll - rollAngle) > KAWAII_SMALL_NUMBER) { rollAngle = clampedRoll; clamped = true; }
		}

		if (!clamped) return;

		// Reconstruct the clamped local axis and angle
		float clampedAngleRad = sqrtf(pitchAngle * pitchAngle + yawAngle * yawAngle + rollAngle * rollAngle) * (3.14159265f / 180.0f);
		if (clampedAngleRad < KAWAII_SMALL_NUMBER) return;

		v3dxVector3 clampedLocalAxis(pitchAngle, yawAngle, rollAngle);
		clampedLocalAxis = Vec3SafeNormal(clampedLocalAxis);
		v3dxVector3 worldAxis = QuatRotateVector(Particle.PoseRotation, clampedLocalAxis);

		float halfAngle = clampedAngleRad * 0.5f;
		float sinHalf = sinf(halfAngle);
		v3dxQuaternion clampedRot(worldAxis.X * sinHalf, worldAxis.Y * sinHalf, worldAxis.Z * sinHalf, cosf(halfAngle));

		v3dxVector3 newSimVector = QuatRotateVector(clampedRot, PoseVector);
		Particle.Position = ParentJoint.GetPosition() + newSimVector;
	}

	void FBoneConstraints::ProjectVelocityAtAngleLimit(FSimParticle& Particle, const FSimParticle& ParentJoint)
	{
		if (Particle.PhysicsSettings.LimitAngle <= 0.0f || !Particle.IsLODValidToEvaluate || !ParentJoint.IsLODValidToEvaluate)
			return;
		if (Particle.PinMode != KPM_Dynamic)
			return;

		const v3dxVector3 ToChild = Particle.Position - ParentJoint.Position;
		const float BoneLength = Vec3Length(ToChild);
		if (BoneLength < KAWAII_SMALL_NUMBER) return;

		const v3dxVector3 BoneDir = ToChild * (1.0f / BoneLength);
		const v3dxVector3 PoseDir = Vec3SafeNormal(Particle.PosePosition - ParentJoint.PosePosition);
		if (Vec3IsNearlyZero(PoseDir)) return;

		const float CosAngle = Vec3Dot(PoseDir, BoneDir);
		const float CosLimit = cosf(Particle.PhysicsSettings.LimitAngle * (3.14159265f / 180.0f));

		// Only project when the particle is near the cone boundary
		static constexpr float CosBoundaryTolerance = 0.015f;
		if (CosAngle > CosLimit + CosBoundaryTolerance) return;

		const v3dxVector3 Inward = PoseDir - BoneDir * CosAngle;
		const v3dxVector3 InwardDir = Vec3SafeNormal(Inward);
		if (Vec3IsNearlyZero(InwardDir)) return;

		const float VdotN = Vec3Dot(Particle.Velocity, InwardDir);
		if (VdotN < 0.0f)
		{
			Particle.Velocity = Particle.Velocity - InwardDir * VdotN;

			static constexpr float TangentialFriction = 0.15f;
			const v3dxVector3 TangentialVel = Particle.Velocity - InwardDir * Vec3Dot(Particle.Velocity, InwardDir);
			Particle.Velocity = Particle.Velocity - TangentialVel * TangentialFriction;
		}
	}

} // namespace KawaiiPhysics

NS_END
