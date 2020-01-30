#pragma once
#include "../../../Graphics/GfxPreHead.h"
#include <fbxsdk.h>
#include "GfxAssetImportOption.h"
NS_BEGIN

enum AssetImportMessageType
{
	AMT_Import = 0,
	AMT_Warning,
	AMT_Error,
};
class GfxFileImportOption;
class GfxFBXManager;
typedef void (WINAPI *FOnImportMessageHandle)(void* handle, int type,int level, const char* info, float percent);
class GfxAssetCreater : public VIUnknown
{
public:
	RTTI_DEF(GfxAssetCreater, 0x121f4b4a5d107729, true);
	GfxAssetCreater();
	~GfxAssetCreater();
public:
	virtual void Process(IRenderContext* rc, FbxScene* scene, GfxFileImportOption* fileImportOption, GfxFBXManager* manager) {};
	ImportAssetType mAssetType;
	GfxAssetImportOption* mAssetImportOption;
	void SetAssetType(ImportAssetType type) { mAssetType = type; };
	void SetImportOption(GfxAssetImportOption* importOption) { mAssetImportOption = importOption; };

	void SetCShaprHandle(void* handle) { mCSharpHandle = handle; }
	void SetFOnImportMessageHandle(FOnImportMessageHandle handle)
	{
		_OnImportMessageDumping = handle;
	}
	virtual void OnImportMessageDumping(AssetImportMessageType messageType, int level, const char* info, float percent = 0.0f);
protected:
	void* mCSharpHandle;
	FOnImportMessageHandle _OnImportMessageDumping;
};

NS_END