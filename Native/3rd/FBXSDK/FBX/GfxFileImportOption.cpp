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
	Cpp2CS0(EngineNS, GfxFileImportOption, GetAssetCount);
	Cpp2CS1(EngineNS, GfxFileImportOption, GetAssetType);
	Cpp2CS1(EngineNS, GfxFileImportOption, GetAssetImportOption);
	Cpp2CS0(EngineNS, GfxFileImportOption, GetFileCreater);
	Cpp2CS0(EngineNS, GfxFileImportOption, GetConvertSceneUnit);
	Cpp2CS1(EngineNS, GfxFileImportOption, SetConvertSceneUnit);
	Cpp2CS0(EngineNS, GfxFileImportOption, GetFileSystemUnit);
}