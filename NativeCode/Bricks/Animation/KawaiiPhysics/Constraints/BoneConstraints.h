#pragma once
#include "../Solvers/SimParticle.h"

NS_BEGIN

namespace KawaiiPhysics
{
	// Bone-level constraints: length preservation, angle limits, rotation limits
	struct FBoneConstraints
	{
		static void ApplyLengthConstraint(float BoneLengthConstraintBlend, FSimParticle& Particle, const FSimParticle& ParentJoint);
		static void ApplyAngleLimit(FSimParticle& Particle, const FSimParticle& ParentJoint);
		static void ApplyRotationLimits(FSimParticle& Particle, const FSimParticle& ParentJoint, const FKawaiiRotationLimits& RotationLimits);

		// Projects velocity at the angle limit boundary to reduce boundary oscillation
		static void ProjectVelocityAtAngleLimit(FSimParticle& Particle, const FSimParticle& ParentJoint);
	};

} // namespace KawaiiPhysics

NS_END
