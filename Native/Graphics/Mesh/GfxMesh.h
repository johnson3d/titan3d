#pragma once
#include "../GfxPreHead.h"

NS_BEGIN

class GfxMaterialPrimitive;
class GfxMeshPrimitives;
class GfxMdfQueue;

class GfxMesh : public VIUnknown
{
public:
	RTTI_DEF(GfxMesh, 0xcfd3ff4e5b0e507c, true);
	GfxMesh();
	~GfxMesh();
	vBOOL Init(const char* name, GfxMeshPrimitives* geom, GfxMdfQueue* mdf);
	//vBOOL SetMaterial(UINT index, GfxMaterialPrimitive* material);
	void SetMeshPrimitives(GfxMeshPrimitives* geom);
	void SetGfxMdfQueue(GfxMdfQueue* mdf);

	//void Save2Xnd(XNDNode* node);
	//vBOOL LoadXnd(XNDNode* node);

	//const char* GetName() const;
	//void SetName(const char* name);
	//const char* GetGeomName() const;
	UINT GetAtomNumber() const;
	//const char* GetMaterailName(UINT index) const;
protected:
	//RName						mName;
	//RName						mGeomName;
	//std::vector<RName>			mMaterialNames;

	AutoRef<GfxMeshPrimitives>				mGeoms;
	AutoRef<GfxMdfQueue>					mMdfQueue;
	//std::vector<GfxMaterialPrimitive*>		mMaterials;
};

NS_END