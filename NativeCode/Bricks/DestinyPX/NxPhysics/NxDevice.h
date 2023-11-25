#include "NxPreHead.h"

NS_BEGIN

namespace NxPhysics
{
	class NxDevice
	{
	public:
		NxScene* CreateScene(const NxSceneDesc* desc);
		NxRigidBody* CreateRigidBody(const NxRigidBodyDesc* desc);
	};
}

NS_END