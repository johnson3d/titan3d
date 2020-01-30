#pragma once
#include "../GfxPreHead.h"

NS_BEGIN

class GfxMesh;
class GfxMaterialInstance;
class GfxMaterialPrimitive : public VIUnknown
{
public:
	RTTI_DEF(GfxMaterialPrimitive, 0x2ace7a745b0e52f7, true);
	GfxMaterialPrimitive();
	~GfxMaterialPrimitive();

	void SetMaterial(GfxMaterialInstance* material);
	GfxMaterialInstance* GetMaterial();
protected:
	AutoRef<GfxMaterialInstance>	mMaterial;
	AutoRef<IConstantBuffer>		mShaderData;
};

NS_END