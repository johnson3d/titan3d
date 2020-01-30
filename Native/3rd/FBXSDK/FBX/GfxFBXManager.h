#pragma once
#include "../../../Graphics/GfxPreHead.h"
#include "../2019.2/include/fbxsdk.h"

NS_BEGIN
class GfxFBXManager : public VIUnknown
{
	RTTI_DEF(GfxFBXManager, 0xd4f225b75d49194d, true);
public:
	GfxFBXManager();
	~GfxFBXManager();
	FbxManager* GetSDKManager();
	
	void InitializeSdkManager();

	void DestroySdkObjects(FbxManager* pSdkManager,bool pExitStatus);

	const char *GetReaderOFNFilters();

	const char *GetWriterSFNFilters();

	const char *GetFileFormatExt(
		const int pWriteFileFormat
	);
	FbxIOSettings* IOSettings()
	{
		return SdkManager->GetIOSettings();
	}
public:
	FbxManager* SdkManager;
};

NS_END