#include "GfxICurve.h"

#define new VNEW
NS_BEGIN
RTTI_IMPL(EngineNS::GfxICurve, EngineNS::VIUnknown);
GfxICurve::GfxICurve()
{
}

GfxICurve::~GfxICurve()
{
}

void GfxICurve::EvaluateClamp(float curveT, CurveResult& result)
{

}

void GfxICurve::Evaluate(float curveT, CurveResult& result)
{

}

void GfxICurve::EvaluateNative(float curveT, CurveResult* result)
{
	Evaluate(curveT, *result);
}
NS_END

using namespace EngineNS;
extern "C"
{
	Cpp2CS2(EngineNS, GfxICurve, EvaluateNative);
	Cpp2CS2(EngineNS, GfxICurve, LoadXnd);
	Cpp2CS1(EngineNS, GfxICurve, Save2Xnd);
	Cpp2CS0(EngineNS, GfxICurve, GetCurveType);
	Cpp2CS0(EngineNS, GfxICurve, GetKeyCount);
}