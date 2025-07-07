#include "NvPhyMaterial.h"
#include "NvPhyScene.h"
#include "NvPhyActor.h"

#define new VNEW

NS_BEGIN

ENGINE_RTTI_IMPL(EngineNS::NvPhyMaterial);

NvPhyMaterial::NvPhyMaterial()
{
	mMaterial = nullptr;
	EntityType = Phy_Material;
}

NvPhyMaterial::~NvPhyMaterial()
{
	Cleanup();
}

void NvPhyMaterial::Cleanup()
{
	if (mMaterial != nullptr)
	{
		mMaterial->userData = nullptr;
		mMaterial->release();
		mMaterial = nullptr;
	}
}

void NvPhyMaterial::BindPhysX()
{
	ASSERT(mMaterial);
	mMaterial->userData = this;
}

NS_END
