#include "NxPreHead.h"

NS_BEGIN

namespace NxPhysics
{
	class TR_CLASS()
		NxDevice : public NxBase
	{
	public:
		NxDevice() {}
		NxScene* CreateScene(const NxSceneDesc* desc);
		NxRigidBody* CreateRigidBody(const NxRigidBodyDesc* desc);
		NxSphereShape* CreateSphereShape(const NxSphereShapeDesc* desc);
		NxBoxShape* CreateBoxShape(const NxBoxShapeDesc* desc);
	};
}

NS_END