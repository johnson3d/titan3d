#include "NxJoint.h"
#include "NxActor.h"

NS_BEGIN

namespace NxPhysics
{
	ENGINE_RTTI_IMPL(NxJoint);
	ENGINE_RTTI_IMPL(NxDistanceJoint);
	NxReal NxDistanceJoint::CalcLimitMin(const NxRigidBody* body0, const NxRigidBody* body1)
	{
		if (body0->mShapes.size() == 0 || body1->mShapes.size() == 0)
		{
			return NxReal::Zero();
		}
		if (body0->mShapes.size() == 1 && body1->mShapes.size() == 1)
		{
			const auto& shape0 = body0->mShapes[0];
			const auto& shape1 = body1->mShapes[0];

			if (shape0->GetRtti() == GetClassObject<NxSphereShape>() &&
				shape1->GetRtti() == GetClassObject<NxSphereShape>())
			{
				return ((NxSphereShape*)shape0.GetPtr())->mDesc.Radius + ((NxSphereShape*)shape1.GetPtr())->mDesc.Radius;
			}
		}

		return NxReal::Zero();
	}
	void NxDistanceJoint::SolveConstraint(NxScene* scene, const NxReal& time)
	{
		auto& mBody0 = mActorPair.first;
		auto& mBody1 = mActorPair.second;
		auto dir = mBody1->GetTryTransform()->Position - mBody0->GetTryTransform()->Position;
		dir.Normalize();
		auto delta = FixDistance(mBody0->GetTryTransform()->Position, mBody1->GetTryTransform()->Position, mLimitMin, mLimitMax);
		auto length = delta.Length();
		delta = dir * length;
		auto w0 = mBody0->GetShapeData()->InvMass;
		auto w1 = mBody1->GetShapeData()->InvMass;

		auto f0 = w0 / (w0 + w1);
		auto f1 = w1 / (w0 + w1);//1-f0
		auto m0 = delta * f0;
		auto m1 = delta * f1;
		mBody0->GetTryTransform()->Position -= m0;
		mBody1->GetTryTransform()->Position += m1;

		D2R(0.5);
	}
}

NS_END


