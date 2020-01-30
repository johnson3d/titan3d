#include "GfxQuaternionCurve.h"

#define new VNEW
NS_BEGIN
RTTI_IMPL(EngineNS::GfxQuaternionCurve, EngineNS::VIUnknown);
GfxQuaternionCurve::GfxQuaternionCurve()
{
	mType = Type_Quaternion;
}

GfxQuaternionCurve::~GfxQuaternionCurve()
{

}

void GfxQuaternionCurve::Evaluate(float curveT, CurveResult& result)
{
	result.Type = Type_Quaternion;
	auto quat = mCurve.Evaluate(curveT);
	quat.normalize();
	result.QuaternionResult = quat;
}

void GfxQuaternionCurve::EvaluateClamp(float curveT, CurveResult& result)
{
	result.Type = Type_Quaternion;
	auto quat = mCurve.EvaluateClamp(curveT);
	quat.normalize();
	result.QuaternionResult = quat;
}

void GfxQuaternionCurve::Save2Xnd(XNDNode* node)
{
	auto att = node->AddAttrib("CurveData");
	UINT count = mCurve.GetKeyCount();
	att->BeginWrite();
	att->Write(count);
	if (count > 0)
	{
		att->Write(mCurve.GetPreInfinity());
		att->Write(mCurve.GetGetPostInfinity());
		auto key0 = mCurve.GetKeyData().data()[0];
		//auto size = sizeof(CurveKeyTpl<v3dxQuaternion>);
		att->Write(&mCurve.GetKeyData()[0], sizeof(CurveKeyTpl<v3dxQuaternion>)*count);
	}
	att->EndWrite();
}

vBOOL GfxQuaternionCurve::LoadXnd(IRenderContext* rc, XNDNode* node)
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
			att->Read(&mCurve.GetKeyData()[0], sizeof(CurveKeyTpl<v3dxQuaternion>)*count);
		}
		att->EndRead();
		return TRUE;
	}
	return FALSE;
}

NS_END
