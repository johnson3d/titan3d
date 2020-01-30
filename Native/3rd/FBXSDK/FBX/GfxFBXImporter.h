#pragma once
#include "../../../Graphics/GfxPreHead.h"
#include <fbxsdk.h>
NS_BEGIN

class GfxFileImportOption;
class GfxAsset_File;
class GfxFBXManager;
enum SystemUnit
{
	SU_mm,
	SU_dm,
	SU_cm,
	SU_m,
	SU_km,
	SU_Inch,
	SU_Foot,
	SU_Mile,
	SU_Yard,
	SU_Custom,
};
class GfxFBXImporter : public VIUnknown
{
public:
	RTTI_DEF(GfxFBXImporter, 0xaa89cfa55cf8b343, true);
	GfxFBXImporter();
	~GfxFBXImporter();
public:
	void ConvertScene(FbxScene* scene, GfxFileImportOption* fileOption);
	vBOOL PreImport(const char* fileName, GfxFileImportOption* option, GfxFBXManager* manager);
	vBOOL Import(IRenderContext* rc, const char* fileName, GfxAsset_File* assetFile, GfxFBXManager* manager);
protected:
	void GetAssetImportOption(FbxScene* scene, GfxFileImportOption* fileOption);
	void GetFbxNodeAssetImportOption(FbxNode* node, GfxFileImportOption* fileImportOption);
	void GetFbxNodeAnimationImportOption(FbxScene* scene, FbxAnimStack * animStack, FbxAnimLayer* animLayer, FbxNode* node, GfxFileImportOption* fileImportOption, bool skeletonAnimMode);
	bool IsHaveAnimCurve(FbxNode* node, FbxAnimLayer* animLayer);
	bool IsHaveAnimCurve(FbxNodeAttribute* node, FbxAnimLayer* animLayer);
	bool IsSkeletonHaveAnimCurve(FbxNode* node, FbxAnimLayer* animLayer);
protected:
	std::map<UINT, FbxScene*> mSceneMap;
	std::map<UINT, GfxFileImportOption*> mImportOptionsMap;
	FbxArray<FbxString*> mAnimStackNameArray;
};

NS_END