#include "Nv5PhyMaterial.h"
#include "Nv5PhyScene.h"
#include "Nv5PhyActor.h"

#define new VNEW

NS_BEGIN

ENGINE_RTTI_IMPL(EngineNS::Nv5PhyMaterial);

Nv5PhyMaterial::Nv5PhyMaterial()
{
	mMaterial = nullptr;
	EntityType = Phy_Material;
}

Nv5PhyMaterial::~Nv5PhyMaterial()
{
	Cleanup();
}

void Nv5PhyMaterial::Cleanup()
{
	if (mMaterial != nullptr)
	{
		mMaterial->userData = nullptr;
		mMaterial->release();
		mMaterial = nullptr;
	}
}

void Nv5PhyMaterial::BindPhysX()
{
	ASSERT(mMaterial);
	mMaterial->userData = this;
}

NS_END
