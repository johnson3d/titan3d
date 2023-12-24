#include "NxActor.h"
#include "NxScene.h"
#include "NxShape.h"

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
				auto& pos = i->mTransform.Position;
				mDesc.Centroid += pos * i->mShapeData.Mass;
			}
			mDesc.Centroid = NxVector3::Zero();
			mDesc.InvMass = NxReal::One() / mDesc.Mass;
		}

		mDesc.Inertia[0] = NxVector3::Zero();
		mDesc.Inertia[1] = NxVector3::Zero();
		mDesc.Inertia[2] = NxVector3::Zero();
		for (auto& i : mShapes)
		{
			auto& r = i->mTransform.Position;
			mDesc.Inertia[0] += r.X * r.X * i->mShapeData.Mass;
			mDesc.Inertia[1] += r.Y * r.Y * i->mShapeData.Mass;
			mDesc.Inertia[2] += r.Z * r.Z * i->mShapeData.Mass;
		}

		SetInertia(NxVector3::One());

		UpdateRotation();

		OnUpdatedTransform();
	}
	void NxRigidBody::UpdateRotation()
	{

	}
	void NxRigidBody::OnUpdatedTransform()
	{
		for (auto& i : mShapes)
		{
			NxPQ::Multiply(i->mTransform, mTransform, i->mShapeData.LocalTransform);
		}
		UpdateWorldAABB();
	}
	void NxRigidBody::Forward(const NxReal& time)
	{
		mPrevTransform = mTransform;
		//mVelocity += acceleration * time;
		mTransform.Position = mPrevTransform.Position + mVelocity * time;
		
		//https://www.cnblogs.com/ggg-327931457/p/12796155.html
		auto accOmega = NxVector3(mDesc.InertiaInverse[0].X, mDesc.InertiaInverse[1].Y, mDesc.InertiaInverse[2].Z)
			* (NxVector3::Zero() - NxVector3::Cross(mAngularVelocity,
			NxVector3(mDesc.Inertia[0].X, mDesc.Inertia[1].Y, mDesc.Inertia[2].Z) * mAngularVelocity));
		mAngularVelocity += accOmega * time;
		
		auto deltaQuat = NxQuat(mAngularVelocity.X, mAngularVelocity.Y, mAngularVelocity.Z, 0) * mTransform.Quat;
		mTransform.Quat += deltaQuat * time * NxReal::F_0_5();
		mTransform.Quat.Normalize();

		OnUpdatedTransform();
	}
	void NxRigidBody::SolveStep(const NxReal& time)
	{

	}
	void NxRigidBody::UpdateStep(const NxReal& time)
	{
		mVelocity = (mTransform.Position - mPrevTransform.Position) / time;
		mPrevTransform.Position = mTransform.Position;
		auto invQ = mPrevTransform.Quat.Invert();
		auto deltaQ = mTransform.Quat * invQ;
		mAngularVelocity = deltaQ.XYZ() * (NxReal::F_2_0() / time);
		if (deltaQ.W < NxReal::Zero())
			mAngularVelocity = -mAngularVelocity;
		
		mPrevTransform.Quat = mTransform.Quat;		
		UpdateRotation();
	}
	void NxRigidBody::AddForce(const NxVector3& offset, const NxVector3& dir, const NxReal& force)
	{

	}
}

NS_END