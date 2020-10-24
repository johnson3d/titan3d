#include "GfxAssetCreater.h"

#define new VNEW
NS_BEGIN
RTTI_IMPL(EngineNS::GfxAssetCreater, EngineNS::VIUnknown);

GfxAssetCreater::GfxAssetCreater()
{
}


GfxAssetCreater::~GfxAssetCreater()
{
}
void GfxAssetCreater::OnImportMessageDumping(AssetImportMessageType messageType, int level, const char* info, float percent)
{
	if (_OnImportMessageDumping == nullptr)
		return;
	if (mCSharpHandle == nullptr)
		return;
	_OnImportMessageDumping(mCSharpHandle, messageType, level, info, percent);
}


NS_END

using namespace EngineNS;
extern "C"
{
	Cpp2CS1(EngineNS, GfxAssetCreater, SetAssetType);
	Cpp2CS1(EngineNS, GfxAssetCreater, SetImportOption);
	Cpp2CS1(EngineNS, GfxAssetCreater, SetFOnImportMessageHandle);
	Cpp2CS1(EngineNS, GfxAssetCreater, SetCShaprHandle);
}