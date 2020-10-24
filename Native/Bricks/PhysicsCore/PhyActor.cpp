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

vBOOL PhyActor::AddToScene(PhyScene* scene)
{
	auto prev = mScene.GetPtr();
	if (prev == scene)
		return TRUE;
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
	return TRUE;
}

void PhyActor::UpdateTransform(const physx::PxActiveTransform* transform)
{
	mPosition = *((v3dxVector3*)&transform->actor2World.p);
	mRotation = *((v3dxQuaternion*)&transform->actor2World.q);
}

vBOOL PhyActor::SetPose2Physics(const physx::PxTransform* transform, vBOOL autowake)
{
	if (mActor->is<physx::PxRigidActor>())
	{
		auto actor = (physx::PxRigidActor*)mActor;
		mPosition = *(v3dxVector3*)&transform->p;
		mRotation = *(v3dxQuaternion*)&transform->q;
		if (mScene.IsValid() && mScene.GetPtr())
		{
			physx::PxSceneWriteLock Lock(*mScene.GetPtr()->mScene);
			actor->setGlobalPose(*transform, autowake ? true : false);
			return TRUE;

		}
		else
		{
			actor->setGlobalPose(*transform, autowake ? true : false);
			return TRUE;
		}
	}
	return FALSE;
}

vBOOL PhyActor::AttachShape(PhyShape* shape, const physx::PxTransform* relativePose)
{
	if (mScene.IsValid() && mScene.GetPtr())
	{
		physx::PxSceneWriteLock Lock(*mScene.GetPtr()->mScene);
		if (mActor->is<physx::PxRigidActor>())
		{
			auto actor = (physx::PxRigidActor*)mActor;
			shape->mShape->setLocalPose(*relativePose);
			actor->attachShape(*shape->mShape);
			return TRUE;
		}
		return FALSE;
	}
	else
	{
		if (mActor->is<physx::PxRigidActor>())
		{
			auto actor = (physx::PxRigidActor*)mActor;
			shape->mShape->setLocalPose(*relativePose);
			actor->attachShape(*shape->mShape);
			return TRUE;
		}
		return FALSE;
	}

}

void PhyActor::DetachShape(PhyShape* shape, vBOOL wakeOnLostTouch)
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

vBOOL PhyActor::SetRigidBodyFlag(UINT flag, vBOOL value)
{
	if (mScene.IsValid() && mScene.GetPtr())
	{
		physx::PxSceneWriteLock Lock(*mScene.GetPtr()->mScene);
		if (mActor->is<physx::PxRigidBody>())
		{
			auto actor = (physx::PxRigidBody*)mActor;
			actor->setRigidBodyFlag((physx::PxRigidBodyFlag::Enum)flag, value);
			return TRUE;

		}
	}
	else
	{
		if (mActor->is<physx::PxRigidBody>())
		{
			auto actor = (physx::PxRigidBody*)mActor;
			actor->setRigidBodyFlag((physx::PxRigidBodyFlag::Enum)flag, value);
			return TRUE;
		}
	}
	return FALSE;
}
vBOOL EngineNS::PhyActor::SetActorFlag(UINT flag, vBOOL value)
{
	if (!mActor)
		return FALSE;
	if (mScene.IsValid() && mScene.GetPtr())
	{
		physx::PxSceneWriteLock Lock(*mScene.GetPtr()->mScene);
		mActor->setActorFlag((physx::PxActorFlag::Enum)flag, value);
	}
	else
	{
		mActor->setActorFlag((physx::PxActorFlag::Enum)flag, value);
	}
	return TRUE;
}


NS_END

using namespace EngineNS;

template <>
struct Type2TypeConverter<v3dxVector3>
{
	typedef v3dVector3_t		TarType;
};

template <>
struct Type2TypeConverter<v3dxQuaternion>
{
	typedef v3dVector4_t		TarType;
}; 

extern "C"
{
	Cpp2CS1(EngineNS, PhyActor, AddToScene);
	Cpp2CS0(EngineNS, PhyActor, GetPosition);
	Cpp2CS0(EngineNS, PhyActor, GetRotation);

	Cpp2CS2(EngineNS, PhyActor, SetPose2Physics);
	Cpp2CS2(EngineNS, PhyActor, SetRigidBodyFlag);
	Cpp2CS2(EngineNS, PhyActor, SetActorFlag);
}