#include "NxPreHead.h"

namespace NxPhysics
{
	class NxDevice
	{
	public:
		NxScene* CreateScene(const NxSceneDesc* desc);
		NxRigidBody* CreateRigidBody(const NxRigidBodyDesc* desc);
	};
}

