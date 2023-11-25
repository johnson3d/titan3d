#include "NxActor.h"
#include "NxShape.h"

NS_BEGIN

namespace NxPhysics
{
	ENGINE_RTTI_IMPL(NxRigidBody);

	void NxRigidBody::TryStep(const NxReal& time)
	{
		mTryTransform.Position = mTransform.Position + mVelocity * time;
	}
	void NxRigidBody::SolveStep(const NxReal& time)
	{

	}
	void NxRigidBody::FixStep(const NxReal& time)
	{
		mVelocity = (mTryTransform.Position - mTransform.Position) / time;
		mTransform.Position = mTryTransform.Position;
	}
}

NS_END