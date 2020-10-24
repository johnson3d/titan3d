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
	Cpp2CS1(EngineNS, PhyEntity, SetCSharpHandle);
	Cpp2CS0(EngineNS, PhyEntity, GetCSharpHandle);
	Cpp2CS0(EngineNS, PhyEntity, GetEntityType);
}