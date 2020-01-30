#pragma once
#include "../../../Graphics/GfxPreHead.h"
#include "GfxFileImportOption.h"
#include "GfxAssetCreater.h"
NS_BEGIN
class GfxAsset_File : public VIUnknown
{
public:
	RTTI_DEF(GfxAsset_File, 0x3b490cb85d1077bf, true);
	GfxAsset_File();
	~GfxAsset_File();
public:
	GfxFileImportOption* FileOption;
	std::map<UINT, GfxAssetCreater*> AssetCreaters;
	void SetImportOption(GfxFileImportOption* fileOption) { FileOption = fileOption; };
	void AddCreater(UINT index, GfxAssetCreater* creater);
};


NS_END