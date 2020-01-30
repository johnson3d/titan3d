#pragma once
#include "../../../Graphics/GfxPreHead.h"
#include "GfxAssetImportOption.h"
#include "GfxFBXImporter.h"
NS_BEGIN
class GfxFileImportOption : public GfxAssetImportOption
{
public:
	RTTI_DEF(GfxFileImportOption, 0xa3d2ffb25d106c68, true);
	GfxFileImportOption();
	~GfxFileImportOption();

public:
	std::map<UINT,GfxAssetImportOption*>  ObjectOptions;
	std::string mCreater;
	SystemUnit mFileSystemUnit;
	vBOOL mConvertSceneUnit;
	float mScaleFactor;
	void SetConvertSceneUnit(vBOOL convert) { mConvertSceneUnit = convert; }
	vBOOL GetConvertSceneUnit() { return mConvertSceneUnit; }
	UINT GetAssetCount();
	ImportAssetType GetAssetType(UINT index);
	GfxAssetImportOption* GetAssetImportOption(UINT index);
	const char* GetFileCreater() {return mCreater.c_str(); }
	SystemUnit GetFileSystemUnit() { return mFileSystemUnit; }
};

NS_END