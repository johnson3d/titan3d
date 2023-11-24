#include "NxDevice.h"
#include "NxScene.h"
#include "NxActor.h"

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
