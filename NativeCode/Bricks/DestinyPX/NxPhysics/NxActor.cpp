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

	bool NxRigidBody::Init(const NxRigidBodyDesc& desc)
	{
		mDesc = desc;
		return true;
	}
	void NxRigidBody::UpdateCentroid()
	{
		if (mDesc.InvMass != NxReal::Zero())
		{
			mDesc.Mass = NxReal::Zero();
			for (auto& i : mShapes)
			{
				mDesc.Mass += i->mShapeData.Mass;
				auto pos = i->mTransform.TransformPosition(i->mTransform.Position);
				mDesc.Centroid += pos * i->mShapeData.Mass;
			}
			mDesc.Centroid = NxVector3::Zero();
			mDesc.InvMass = NxReal::One() / mDesc.Mass;
		}

		mDesc.Inertia = NxVector3::Zero();
		for (auto& i : mShapes)
		{
			auto pos = i->mTransform.TransformPosition(i->mTransform.Position);
			auto r = pos - mDesc.Centroid;
			mDesc.Inertia += r * r;
		}

		SetInertia(NxVector3::One());

		UpdateRotation();
	}
	void NxRigidBody::UpdateRotation()
	{

	}
	void NxRigidBody::TryStep(const NxReal& time)
	{
		//mVelocity += acceleration * time;
		mTryTransform.Position = mTransform.Position + mVelocity * time;

		auto accOmega = mDesc.InertiaInverse * (NxVector3::Zero() - NxVector3::Cross(mAngularVelocity, mDesc.Inertia * mAngularVelocity));
		mAngularVelocity += accOmega * time;
		
		auto deltaQuat = NxQuat(mAngularVelocity.X, mAngularVelocity.Y, mAngularVelocity.Z, 0) * mTransform.Quat;
		mTryTransform.Quat += deltaQuat * time * NxReal::F_0_5();
		mTryTransform.Quat.Normalize();
	}
	void NxRigidBody::SolveStep(const NxReal& time)
	{

	}
	void NxRigidBody::FixStep(const NxReal& time)
	{
		mVelocity = (mTryTransform.Position - mTransform.Position) / time;
		mTransform.Position = mTryTransform.Position;
		auto invQ = mTransform.Quat.Invert();
		auto deltaQ = mTryTransform.Quat * invQ;
		mAngularVelocity = deltaQ.XYZ() * (NxReal::F_2_0() / time);
		if (deltaQ.W < NxReal::Zero())
			mAngularVelocity = -mAngularVelocity;
		
		mTransform.Quat = mTryTransform.Quat;		
		UpdateRotation();
	}
	void NxRigidBody::AddForce(const NxVector3& offset, const NxVector3& dir, const NxReal& force)
	{

	}
}

NS_END