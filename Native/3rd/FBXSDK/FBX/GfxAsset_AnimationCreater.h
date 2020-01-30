#pragma once
#include "../../../Graphics/GfxPreHead.h"
#include "GfxAssetCreater.h"
#include <fbxsdk.h>
#include "../../Bricks/Animation/AnimElement/GfxAnimationElement.h"
NS_BEGIN
struct FullAnimTime
{
	std::vector<FbxTime> FullTime;
	void Add(FbxTime time)
	{
		for (int i = 0; i < FullTime.size(); ++i)
		{
			if (FullTime[i] == time)
				return;
		}
		FullTime.push_back(time);
	}
};
class GfxSkeletonAnimationElement;
class GfxFileImportOption;
class GfxVector3Curve;
class GfxQuaternionCurve;
class GfxFBXManager;

class GfxAsset_AnimationCreater : GfxAssetCreater
{
public:
	RTTI_DEF(GfxAsset_AnimationCreater, 0x207341e85d108f39, true);
	GfxAsset_AnimationCreater();
	~GfxAsset_AnimationCreater();
	virtual void Process(IRenderContext* rc, FbxScene* scene, GfxFileImportOption* fileImportOption, GfxFBXManager* manager) override;
public:
	std::map<UINT, GfxAnimationElement*> mElements;
	UINT GetElementCount() { return (UINT)mElements.size(); }
	AnimationElementType GetElementType(UINT index);
	GfxAnimationElement* GetElement(UINT index);
	void CreateBoneAnimationCurves(FbxNode* node, GfxSkeletonAnimationElement* skeletonElement, float scale, bool isRootNode);
protected:
	GfxAnimationImportOption* GetAnimationImportOption() { return (GfxAnimationImportOption*)mAssetImportOption; }
	void GetBoneAnimationCurveRecursion(FbxNode* node, GfxSkeletonAnimationElement* skeletonElement, float scale, bool isRootNode);
	void GetPropertyAnimationCurveRecursion(FbxNode* node, GfxAnimationElement* propertyElement, float scale, bool isRootNode);
	std::string GetHierarchyNodeName(FbxNode* node);
	GfxVector3Curve* GetNodeLclTranslationCurve(FbxNode* node, float scale);
	GfxQuaternionCurve* GetNodeLclRotationCurve(FbxNode* node, float scale);
	GfxVector3Curve* GetNodeLclScaleCurve(FbxNode* node, float scale);
	bool IsHaveAnimCurve(FbxNode* node, FbxAnimLayer* animLayer);
};

NS_END