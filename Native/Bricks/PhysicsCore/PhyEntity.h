#pragma once
#include "../../Graphics/GfxPreHead.h"
#include "../../3rd/PhysX-3.4/PhysX_3.4/Include/PxPhysics.h"
#include "../../3rd/PhysX-3.4/PhysX_3.4/Include/PxPhysicsAPI.h"

NS_BEGIN

enum PhyEntityType
{
	Phy_Context,
	Phy_Scene,
	Phy_Material,
	Phy_Controller,
	Phy_Actor,
	Phy_Shape,
	Unknown
};

enum PhyFeatureFlag
{
	Articulations = (1 << 0),
	HeightFields = (1 << 1),
	Cloth = (1 << 2),
	Particles = (1 << 3)
};

enum EPhyActorType
{
	PAT_Dynamic,
	PAT_Static,
};

class PhyEntity : public VIUnknown
{
private:
	void*			mCSharpHandle;
public:
	RTTI_DEF(PhyEntity, 0x3212bf2c5befa50f, false)
	PhyEntity()
	{
		mCSharpHandle = nullptr;
		EntityType = Unknown;
	}
	~PhyEntity();
	VDef_ReadWrite(void*, CSharpHandle, m);
	PhyEntityType EntityType;
	PhyEntityType  GetEntityType() { return EntityType; };
};

NS_END