#include "GfxAsset_File.h"

#define new VNEW
NS_BEGIN
RTTI_IMPL(EngineNS::GfxAsset_File, EngineNS::VIUnknown);

GfxAsset_File::GfxAsset_File()
{
}


GfxAsset_File::~GfxAsset_File()
{
}
void GfxAsset_File::AddCreater(UINT index, GfxAssetCreater* creater)
{
	AssetCreaters.insert(std::make_pair(index, creater));
}



NS_END

using namespace EngineNS;
extern "C"
{
	CSharpAPI1(EngineNS, GfxAsset_File, SetImportOption, GfxFileImportOption*);
	CSharpAPI2(EngineNS, GfxAsset_File, AddCreater, UINT, GfxAssetCreater*);
}