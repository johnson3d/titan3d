#include "NxJoint.h"
#include "NxActor.h"

NS_BEGIN

namespace NxPhysics
{
	ENGINE_RTTI_IMPL(NxJoint);
	ENGINE_RTTI_IMPL(NxDistanceJoint);

	void NxDistanceJoint::SolveConstraint(NxScene* scene, const NxReal& time)
	{
		auto delta = mBody0->mTryTransform.Position - mBody1->mTryTransform.Position;
		delta += FixDistance(mBody0->mTryTransform.Position, mBody1->mTryTransform.Position, mLimitMin, mLimitMax);
		auto length = delta.Length();
		NxVector3 n;
		NxVector3::Normalize(delta, n);
		auto w0 = mBody0->mShapeData.InvMass;
		auto w1 = mBody1->mShapeData.InvMass;

		mBody0->mTryTransform.Position += delta * w0 / (w0 + w1);
		mBody1->mTryTransform.Position -= delta * w1 / (w0 + w1);

		D2R(0.5);
	}
}

NS_END


