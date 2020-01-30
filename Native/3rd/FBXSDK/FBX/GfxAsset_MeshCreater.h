#pragma once
#include "../../../Graphics/GfxPreHead.h"
#include "GfxAssetCreater.h"
#include <fbxsdk.h>
#include "../../Graphics/Mesh/GfxMeshPrimitives.h"
#include "../../Bricks/Animation/Skeleton/GfxBone.h"
NS_BEGIN

class BoneCluster;
class GfxFBXManager;
class GfxSkeleton;
class GfxAsset_MeshCreater : GfxAssetCreater
{
public:
	RTTI_DEF(GfxAsset_MeshCreater, 0xbe561db15d107780, true);
	GfxAsset_MeshCreater();
	~GfxAsset_MeshCreater();
public:
	GfxMeshPrimitives* mMeshPrimitives;
	GfxSkeleton* mFullSkeleton;
	GfxSkeleton* GetFullSkeleton() { return mFullSkeleton; };
	void SetMeshPrimitives(GfxMeshPrimitives* meshPrimitives) { mMeshPrimitives = meshPrimitives; }
	virtual void Process(IRenderContext* rc, FbxScene* scene, GfxFileImportOption* fileImportOption, GfxFBXManager* manager) override;
	//v3dxMatrix4 ComputeTotalMatrix(FbxNode* Node, FbxScene* scene);
	void RecursionCalculateBone(FbxNode* boneNode, GfxSkeleton* skeleton, std::vector<BoneCluster> boneClusters);
};

NS_END