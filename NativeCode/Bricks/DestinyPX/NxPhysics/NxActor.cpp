#include "NxActor.h"
#include "NxScene.h"

NS_BEGIN

namespace NxPhysics
{
	ENGINE_RTTI_IMPL(NxActor);
	ENGINE_RTTI_IMPL(NxRigidBody);

	void NxActor::DetachScene()
	{
		auto scene = mScene.GetPtr();
		if (scene != nullptr)
		{
			scene->RemoveActor(this);
		}
		mScene.FromObject(nullptr);
	}

	void NxRigidBody::UpdateCentroid()
	{
		mShapeData.Centroid = NxVector3::Zero();
		mShapeData.Mass = NxReal::Zero();
		for (auto& i : mShapes)
		{
			auto pos = i->mTransform.TransformPosition(i->mTransform.Position);
			mShapeData.Centroid += pos * i->mShapeData.Mass;
			mShapeData.Mass += i->mShapeData.Mass;
		}
		mShapeData.Centroid /= mShapeData.Mass;
	}
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
	void NxRigidBody::AddForce(const NxVector3& offset, const NxVector3& dir, const NxReal& force)
	{

	}
}

NS_END