#include "GfxMaterialPrimitive.h"
#include "../GfxMaterial.h"

#define new VNEW

NS_BEGIN

RTTI_IMPL(EngineNS::GfxMaterialPrimitive, EngineNS::VIUnknown);

GfxMaterialPrimitive::GfxMaterialPrimitive()
{
}

GfxMaterialPrimitive::~GfxMaterialPrimitive()
{
}

void GfxMaterialPrimitive::SetMaterial(GfxMaterialInstance* material)
{
	mMaterial.StrongRef(material);
}

GfxMaterialInstance* GfxMaterialPrimitive::GetMaterial()
{
	return mMaterial;
}

NS_END

using namespace EngineNS;

extern "C"
{
	Cpp2CS1(EngineNS, GfxMaterialPrimitive, SetMaterial);
	Cpp2CS0(EngineNS, GfxMaterialPrimitive, GetMaterial);
}