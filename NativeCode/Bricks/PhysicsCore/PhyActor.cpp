#include "PhyActor.h"
#include "PhyScene.h"
#include "PhyShape.h"

#define new VNEW

NS_BEGIN

ENGINE_RTTI_IMPL(EngineNS::PhyActor);

PhyActor::PhyActor()
{
	EntityType = Phy_Actor;
}

PhyActor::~PhyActor()
{
	Cleanup();
}

NS_END
