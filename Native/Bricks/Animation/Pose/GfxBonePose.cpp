#include "GfxBonePose.h"
#define new VNEW

NS_BEGIN
RTTI_IMPL(EngineNS::GfxBonePose, EngineNS::VIUnknown);
GfxBonePose::GfxBonePose()
{
}


GfxBonePose::~GfxBonePose()
{
}
NS_END

using namespace EngineNS;


extern "C"
{
	Cpp2CS1(EngineNS, GfxBonePose, GetTransform);
	Cpp2CS1(EngineNS, GfxBonePose, SetTransform);
	Cpp2CS1(EngineNS, GfxBonePose, GetMotionData);
	Cpp2CS1(EngineNS, GfxBonePose, SetMotionData);
	Cpp2CS1(EngineNS, GfxBonePose, SetReferenceBone);
}