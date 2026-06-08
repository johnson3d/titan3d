#pragma once
#include "../KawaiiTypes.h"
#include <memory>

NS_BEGIN

namespace KawaiiPhysics
{
	struct FSimParticle;
	struct FBoundBoxHandle;

	// Abstract base class for all colliders; manages the BoundingBox and exposes the collision interface.
	struct FColliderBase
	{
		FColliderBase();
		FColliderBase(const FColliderBase& Other);
		FColliderBase(FColliderBase&& Other) noexcept;
		FColliderBase& operator=(const FColliderBase& Other);
		FColliderBase& operator=(FColliderBase&& Other) noexcept;
		virtual ~FColliderBase();

		FKawaiiAABB PrevBoundBox;
		EKawaiiColliderType ColliderType = KCT_Unknown;
		std::shared_ptr<FBoundBoxHandle> BoundBoxHandle;

		virtual FKawaiiAABB CalcCurrentBoundBox() { return FKawaiiAABB(); }

		void UpdateBoundBox();

		EKawaiiColliderType GetColliderType() const { return ColliderType; }

		virtual bool OnPointCollision(FSimParticle& Joint, float CollisionSubStep, uint32_t InAnimUid,
			float RadiusIncreaseCoefficient) const { return false; }

		virtual bool OnLineCollision(FSimParticle& JointA, FSimParticle& JointB, float CollisionSubStep,
			uint32_t InAnimUid) const { return false; }

		void RebindHandle();
	};

	// Lightweight handle holding the current valid AABB and a raw pointer to the owning Collider.
	struct FBoundBoxHandle
	{
		FKawaiiAABB BoundBox;
		FColliderBase* Owner = nullptr;

		EKawaiiColliderType GetColliderType() const { return Owner ? Owner->GetColliderType() : KCT_Unknown; }
		v3dxVector3 GetCenterLocation() const { return BoundBox.GetCenter(); }

		bool OnPointCollision(FSimParticle& Joint, float CollisionSubStep, uint32_t AnimUid,
			float RadiusIncreaseCoefficient) const
		{
			return Owner ? Owner->OnPointCollision(Joint, CollisionSubStep, AnimUid, RadiusIncreaseCoefficient) : false;
		}

		bool OnLineCollision(FSimParticle& JointA, FSimParticle& JointB, float CollisionSubStep, uint32_t AnimUid) const
		{
			return Owner ? Owner->OnLineCollision(JointA, JointB, CollisionSubStep, AnimUid) : false;
		}
	};

} // namespace KawaiiPhysics

NS_END
