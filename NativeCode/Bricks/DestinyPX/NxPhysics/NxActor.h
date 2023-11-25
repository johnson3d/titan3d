#pragma once
#include "NxPreHead.h"

NS_BEGIN

namespace NxPhysics
{
	class NxActor : public NxEntity
	{
	public:
		NxWeakRef<NxScene> mScene;
	public:
		virtual void TryStep(const NxReal& time) = 0;
		virtual void SolveStep(const NxReal& time) = 0;
		virtual void FixStep(const NxReal& time) = 0;
	};

	struct NxRigidBodyDesc
	{

	};
	class NxRigidBody : public NxActor
	{
	public:
		NxPQ mTransform;
		NxPQ mTryTransform;
		NxVector3 mVelocity;

		std::vector<NxAutoRef<NxShape>> mShapes;
	public:
		ENGINE_RTTI(NxRigidBody);
		virtual const NxPQ* GetTransform() const override 
		{
			return &mTransform;
		}
		virtual NxPQ* GetTransform() override
		{
			return &mTransform;
		}

		virtual void TryStep(const NxReal& time) override;
		virtual void SolveStep(const NxReal& time) override;
		virtual void FixStep(const NxReal& time) override;
	};
}

NS_END