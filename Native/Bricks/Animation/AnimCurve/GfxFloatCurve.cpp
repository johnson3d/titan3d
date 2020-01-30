#include "GfxFloatCurve.h"
#include "GfxCurveTpl.cpp"

#define new VNEW
NS_BEGIN
RTTI_IMPL(EngineNS::GfxFloatCurve, EngineNS::VIUnknown);
GfxFloatCurve::GfxFloatCurve()
{
	mType = Type_Float;
}

GfxFloatCurve::~GfxFloatCurve()
{

}


void GfxFloatCurve::Evaluate(float curveT, CurveResult& result)
{
	result.Type = Type_Float;
	result.FloatResult = mCurve.Evaluate(curveT);
}

void GfxFloatCurve::EvaluateClamp(float curveT, CurveResult& result)
{
	result.Type = Type_Float;
	result.FloatResult = mCurve.EvaluateClamp(curveT);
}

void GfxFloatCurve::Save2Xnd(XNDNode* node)
{
	auto att = node->AddAttrib("CurveData");
	UINT count = mCurve.GetKeyCount();
	att->BeginWrite();
	att->Write(count);
	if (count > 0)
	{
		att->Write(mCurve.GetPreInfinity());
		att->Write(mCurve.GetGetPostInfinity());
		//auto key0 = mCurve.GetKeyData().data()[0];
		//auto size = sizeof(CurveKeyTpl<float>);
		att->Write(&mCurve.GetKeyData()[0], sizeof(CurveKeyTpl<float>)*count);
	}
	att->EndWrite();
}

vBOOL GfxFloatCurve::LoadXnd(IRenderContext* rc, XNDNode* node)
{
	auto att = node->GetAttrib("CurveData");
	if (att)
	{
		UINT count = 0;
		att->BeginRead(__FILE__, __LINE__);
		att->Read(count);
		if (count > 0)
		{
			WrapMode pre, post;
			att->Read(pre);
			mCurve.SetPreInfinity(pre);
			att->Read(post);
			mCurve.SetPostInfinity(post);
			mCurve.GetKeyData().resize(count);
			att->Read(&mCurve.GetKeyData()[0], sizeof(CurveKeyTpl<float>)*count);
		}
		att->EndRead();
	}
	return TRUE;
}


NS_END
