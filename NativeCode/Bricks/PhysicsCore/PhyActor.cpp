#include "PhyActor.h"
#include "PhyScene.h"
#include "PhyShape.h"

#define new VNEW

NS_BEGIN

RTTI_IMPL(EngineNS::PhyActor, EngineNS::PhyEntity);

PhyActor::PhyActor()
{
	mActor = nullptr;
	EntityType = Phy_Actor;
}

PhyActor::~PhyActor()
{
	Cleanup();
}

void PhyActor::Cleanup()
{
	if (mActor != nullptr)
	{
		//unbind this from pxActor's user data
		mActor->userData = nullptr;

		//destroy pxActor
		auto scene = mScene.GetPtr();
		if (scene != nullptr)
		{
			physx::PxSceneWriteLock writeLock(*scene->mScene);
			scene->mScene->removeActor(*mActor);
		}
		mActor->release();
		mActor = nullptr;
	}
}

void PhyActor::BindPhysX()
{
	//bind this to pxActor's user data
	ASSERT(mActor);
	mActor->userData = this;

	//((physx::PxRigidActor*)mActor)->setGlobalPose
}

bool PhyActor::AddToScene(PhyScene* scene)
{
	auto prev = mScene.GetPtr();
	if (prev == scene)
		return true;
	if (prev != nullptr)
	{
		//scene remove
		if (prev->mScene != nullptr)
		{
			physx::PxSceneWriteLock writeLock(*prev->mScene);
			prev->mScene->removeActor(*mActor, true);
		}
	}

	mScene.FromObject(scene);
	if (scene != nullptr)
	{
		if (scene->mScene != nullptr)
		{
			physx::PxSceneWriteLock writeLock(*scene->mScene);
			scene->mScene->addActor(*mActor);
		}
	}
	return true;
}

void PhyActor::UpdateTransform()
{
	if (mActor->getType() == physx::PxActorType::Enum::eRIGID_STATIC ||
		mActor->getType() == physx::PxActorType::Enum::eRIGID_DYNAMIC )
	{
		auto pRigidActor = (physx::PxRigidActor*)mActor;

		const auto& p = pRigidActor->getGlobalPose().p;
		mPosition.x = p.x;
		mPosition.y = p.y;
		mPosition.z = p.z;
		
		const auto& q = pRigidActor->getGlobalPose().q;
		mRotation.x = q.x;
		mRotation.y = q.y;
		mRotation.z = q.z;
		mRotation.w = q.w;
	}
}

bool PhyActor::SetPose2Physics(const physx::PxTransform* transform, bool autowake)
{
	if (mActor->is<physx::PxRigidActor>())
	{
		auto actor = (physx::PxRigidActor*)mActor;
		mPosition = *(v3dxVector3*)&transform->p;
		mRotation = *(v3dxQuaternion*)&transform->q;
		if (mScene.IsValid() && mScene.GetPtr())
		{
			physx::PxSceneWriteLock Lock(*mScene.GetPtr()->mScene);
			actor->setGlobalPose(*transform, autowake);
			return true;

		}
		else
		{
			actor->setGlobalPose(*transform, autowake);
			return true;
		}
	}
	return FALSE;
}

bool PhyActor::AttachShape(PhyShape* shape, const physx::PxTransform* relativePose)
{
	if (mScene.IsValid() && mScene.GetPtr())
	{
		physx::PxSceneWriteLock Lock(*mScene.GetPtr()->mScene);
		if (mActor->is<physx::PxRigidActor>())
		{
			auto actor = (physx::PxRigidActor*)mActor;
			shape->mShape->setLocalPose(*relativePose);
			return actor->attachShape(*shape->mShape);
		}
		return false;
	}
	else
	{
		if (mActor->is<physx::PxRigidActor>())
		{
			auto actor = (physx::PxRigidActor*)mActor;
			shape->mShape->setLocalPose(*relativePose);
			return actor->attachShape(*shape->mShape);
		}
		return false;
	}
}

void PhyActor::DetachShape(PhyShape* shape, bool wakeOnLostTouch)
{
	if (mScene.IsValid() && mScene.GetPtr())
	{
		physx::PxSceneWriteLock Lock(*mScene.GetPtr()->mScene);
		if (mActor->is<physx::PxRigidActor>())
		{
			auto actor = (physx::PxRigidActor*)mActor;
			actor->detachShape(*shape->mShape);
		}
	}
	else
	{
		if (mActor->is<physx::PxRigidActor>())
		{
			auto actor = (physx::PxRigidActor*)mActor;
			actor->detachShape(*shape->mShape);
		}
	}
}

bool PhyActor::SetRigidBodyFlag(EPhyRigidBodyFlag flag, bool value)
{
	if (mScene.IsValid() && mScene.GetPtr())
	{
		physx::PxSceneWriteLock Lock(*mScene.GetPtr()->mScene);
		if (mActor->is<physx::PxRigidBody>())
		{
			auto actor = (physx::PxRigidBody*)mActor;
			actor->setRigidBodyFlag((physx::PxRigidBodyFlag::Enum)flag, value);
			return true;

		}
	}
	else
	{
		if (mActor->is<physx::PxRigidBody>())
		{
			auto actor = (physx::PxRigidBody*)mActor;
			actor->setRigidBodyFlag((physx::PxRigidBodyFlag::Enum)flag, value);
			return true;
		}
	}
	return FALSE;
}
bool PhyActor::SetActorFlag(EPhyActorFlag flag, bool value)
{
	if (!mActor)
		return false;
	if (mScene.IsValid() && mScene.GetPtr())
	{
		physx::PxSceneWriteLock Lock(*mScene.GetPtr()->mScene);
		mActor->setActorFlag((physx::PxActorFlag::Enum)flag, value);
	}
	else
	{
		mActor->setActorFlag((physx::PxActorFlag::Enum)flag, value);
	}
	return true;
}
float PhyActor::GetMinCCDAdvanceCoefficient()
{
	if (mActor->is<physx::PxRigidDynamic>())
	{
		auto actor = (physx::PxRigidBody*)mActor;
		return actor->getMinCCDAdvanceCoefficient();
	}
	return -1;
}

void PhyActor::SetMinCCDAdvanceCoefficient(float advanceCoefficient)
{
	if (mActor->is<physx::PxRigidDynamic>())
	{
		auto actor = (physx::PxRigidBody*)mActor;
		actor->setMinCCDAdvanceCoefficient(advanceCoefficient);
	}
}

NS_END

