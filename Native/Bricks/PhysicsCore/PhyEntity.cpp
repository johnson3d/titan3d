#include "PhyEntity.h"

#define new VNEW

NS_BEGIN

RTTI_IMPL(EngineNS::PhyEntity, EngineNS::VIUnknown);

PhyEntity::~PhyEntity()
{

}

NS_END

using namespace EngineNS;

extern "C"
{
	CSharpAPI1(EngineNS, PhyEntity, SetCSharpHandle, void*);
	CSharpReturnAPI0(void*, EngineNS, PhyEntity, GetCSharpHandle);
	CSharpReturnAPI0(PhyEntityType, EngineNS, PhyEntity, GetEntityType);
}