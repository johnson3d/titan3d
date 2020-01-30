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
	CSharpAPI1(EngineNS, GfxBonePose, GetTransform, GfxBoneTransform*);
	CSharpAPI1(EngineNS, GfxBonePose, SetTransform, GfxBoneTransform*);
	CSharpAPI1(EngineNS, GfxBonePose, GetMotionData, GfxMotionState*);
	CSharpAPI1(EngineNS, GfxBonePose, SetMotionData, GfxMotionState*);
	CSharpAPI1(EngineNS, GfxBonePose, SetReferenceBone, GfxBone*);
}