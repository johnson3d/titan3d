#include "Nv5PhyActor.h"
#include "Nv5PhyScene.h"
#include "Nv5PhyShape.h"

#define new VNEW

NS_BEGIN

ENGINE_RTTI_IMPL(EngineNS::Nv5PhyActor);

Nv5PhyActor::Nv5PhyActor()
{
	mActor = nullptr;
	EntityType = Phy_Actor;
}

Nv5PhyActor::~Nv5PhyActor()
{
	Cleanup();
}

void Nv5PhyActor::Cleanup()
{
	if (mActor != nullptr)
	{
		auto scene = mScene.GetPtr();
		if (scene != nullptr)
		{
			physx::PxSceneWriteLock writeLock(*scene->mScene);
			mActor->userData = nullptr;
			scene->mScene->removeActor(*mActor);
			mActor->release();
		}
		else
		{
			mActor->userData = nullptr;
			mActor->release();
		}
		mActor = nullptr;
	}
}

void Nv5PhyActor::BindPhysX()
{
	//bind this to pxActor's user data
	ASSERT(mActor);
	mActor->userData = this;

	//((physx::PxRigidActor*)mActor)->setGlobalPose
}

bool Nv5PhyActor::AddToScene(PhyScene* scene1)
{
 	auto scene = (Nv5PhyScene*)scene1;
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
bool Nv5PhyActor::RemoveFromScene(PhyScene* scene)
{
	auto current = mScene.GetCastPtr<Nv5PhyScene>();
	mScene.FromObject(nullptr);
	if(current != scene)
		return true;

	if (current != nullptr)
	{
		//scene remove
		if (current->mScene != nullptr)
		{
			physx::PxSceneWriteLock writeLock(*current->mScene);
			current->mScene->removeActor(*mActor, true);
		}
	}
	return true;
}

void Nv5PhyActor::UpdateTransform()
{
	if (mActor->getType() == physx::PxActorType::Enum::eRIGID_STATIC ||
		mActor->getType() == physx::PxActorType::Enum::eRIGID_DYNAMIC )
	{
		auto pRigidActor = (physx::PxRigidActor*)mActor;

		const auto& p = pRigidActor->getGlobalPose().p;
		mPosition.X = p.x;
		mPosition.Y = p.y;
		mPosition.Z = p.z;
		
		const auto& q = pRigidActor->getGlobalPose().q;
		mRotation.X = q.x;
		mRotation.Y = q.y;
		mRotation.Z = q.z;
		mRotation.W = q.w;
	}
}

bool Nv5PhyActor::SetPose2Physics(const physx::PxTransform* transform, bool autowake)
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

bool Nv5PhyActor::AttachShape(PhyShape* shape1, const physx::PxTransform* relativePose)
{
	Nv5PhyShape* shape = (Nv5PhyShape*)shape1;
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

void Nv5PhyActor::DetachShape(PhyShape* shape1, bool wakeOnLostTouch)
{
	Nv5PhyShape* shape = (Nv5PhyShape*)shape1;
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

bool Nv5PhyActor::SetRigidBodyFlag(EPhyRigidBodyFlag flag, bool value)
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
bool Nv5PhyActor::SetActorFlag(EPhyActorFlag flag, bool value)
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
float Nv5PhyActor::GetMinCCDAdvanceCoefficient()
{
	if (mActor->is<physx::PxRigidDynamic>())
	{
		auto actor = (physx::PxRigidBody*)mActor;
		return actor->getMinCCDAdvanceCoefficient();
	}
	return -1;
}

void Nv5PhyActor::SetMinCCDAdvanceCoefficient(float advanceCoefficient)
{
	if (mActor->is<physx::PxRigidDynamic>())
	{
		auto actor = (physx::PxRigidBody*)mActor;
		actor->setMinCCDAdvanceCoefficient(advanceCoefficient);
	}
}

NS_END
