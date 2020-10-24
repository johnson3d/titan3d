#include "PhyMaterial.h"
#include "PhyScene.h"
#include "PhyActor.h"

#define new VNEW

NS_BEGIN

RTTI_IMPL(EngineNS::PhyMaterial, EngineNS::PhyEntity);

PhyMaterial::PhyMaterial()
{
	mMaterial = nullptr;
	EntityType = Phy_Material;
}

PhyMaterial::~PhyMaterial()
{
	Cleanup();
}

void PhyMaterial::Cleanup()
{
	if (mMaterial != nullptr)
	{
		mMaterial->userData = nullptr;
		mMaterial->release();
		mMaterial = nullptr;
	}
}

void PhyMaterial::BindPhysX()
{
	ASSERT(mMaterial);
	mMaterial->userData = this;
}


NS_END

using namespace EngineNS;

extern "C"
{
	Cpp2CS0(EngineNS, PhyMaterial, GetDynamicFriction);
	Cpp2CS1(EngineNS, PhyMaterial, SetDynamicFriction);
	Cpp2CS0(EngineNS, PhyMaterial, GetStaticFriction);
	Cpp2CS1(EngineNS, PhyMaterial, SetStaticFriction);
	Cpp2CS0(EngineNS, PhyMaterial, GetRestitution);
	Cpp2CS1(EngineNS, PhyMaterial, SetRestitution);
}