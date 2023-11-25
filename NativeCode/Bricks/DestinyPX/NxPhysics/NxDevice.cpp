#include "NxDevice.h"
#include "NxScene.h"
#include "NxShape.h"
#include "NxActor.h"
#include "NxJoint.h"

NS_BEGIN

namespace NxPhysics
{
	NxScene* NxDevice::CreateScene(const NxSceneDesc* desc)
	{
		return new NxScene();
	}
	NxRigidBody* NxDevice::CreateRigidBody(const NxRigidBodyDesc* desc)
	{
		return new NxRigidBody();
	}
}

NS_END