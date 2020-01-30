#include "GfxFileImportOption.h"
#define new VNEW
NS_BEGIN
RTTI_IMPL(EngineNS::GfxFileImportOption, EngineNS::VIUnknown);
GfxFileImportOption::GfxFileImportOption()
{
	mConvertSceneUnit = TRUE;
	mScaleFactor = 1.0f;
}


ImportAssetType GfxFileImportOption::GetAssetType(UINT index)
{
	auto it = ObjectOptions.begin();
	int i = 0;
	for (it,i =0; it != ObjectOptions.end(); it++,i++)
	{
		if (i == index)
			return (*it).second->GetAssetType();

	}
	return  IAT_Unknown;
}

GfxAssetImportOption* GfxFileImportOption::GetAssetImportOption(UINT index)
{
	auto it = ObjectOptions.begin();
	int i = 0;
	for (it, i = 0; it != ObjectOptions.end(); it++, i++)
	{
		if (i == index)
			return (*it).second;

	}
	return  NULL;
}

UINT GfxFileImportOption::GetAssetCount()
{
	return (UINT)ObjectOptions.size();
}

GfxFileImportOption ::~GfxFileImportOption()
{
}
NS_END

using namespace EngineNS;


extern "C"
{
	CSharpReturnAPI0(UINT, EngineNS, GfxFileImportOption, GetAssetCount);
	CSharpReturnAPI1(ImportAssetType, EngineNS, GfxFileImportOption, GetAssetType,UINT);
	CSharpReturnAPI1(GfxAssetImportOption*, EngineNS, GfxFileImportOption, GetAssetImportOption, UINT);
	CSharpReturnAPI0(const char*, EngineNS, GfxFileImportOption, GetFileCreater);
	CSharpReturnAPI0(vBOOL, EngineNS, GfxFileImportOption, GetConvertSceneUnit);
	CSharpAPI1(EngineNS, GfxFileImportOption, SetConvertSceneUnit,vBOOL);
	CSharpReturnAPI0(SystemUnit, EngineNS, GfxFileImportOption, GetFileSystemUnit);
}