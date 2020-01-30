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
	CSharpAPI2(EngineNS, GfxICurve, EvaluateNative, float, CurveResult*);
	CSharpReturnAPI2(vBOOL, EngineNS, GfxICurve, LoadXnd, IRenderContext*, XNDNode*);
	CSharpAPI1(EngineNS, GfxICurve, Save2Xnd, XNDNode*);
	CSharpReturnAPI0(CurveType, EngineNS, GfxICurve, GetCurveType);
	CSharpReturnAPI0(UINT, EngineNS, GfxICurve, GetKeyCount);
}