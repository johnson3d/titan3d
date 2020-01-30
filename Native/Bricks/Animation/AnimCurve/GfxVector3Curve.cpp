#include "GfxVector3Curve.h"

#define new VNEW
NS_BEGIN
RTTI_IMPL(EngineNS::GfxVector3Curve, EngineNS::VIUnknown);
GfxVector3Curve::GfxVector3Curve()
{
	mType = Type_Vector3;
}

GfxVector3Curve::~GfxVector3Curve()
{

}

void GfxVector3Curve::Evaluate(float curveT, CurveResult& result)
{
	result.Type = Type_Vector3;
	if (mXCurve&& mXCurve->GetKeyCount() > 0)
		result.Vector3Result.x = mXCurve->Evaluate(curveT);
	if (mYCurve&& mYCurve->GetKeyCount() > 0)
		result.Vector3Result.y = mYCurve->Evaluate(curveT);
	if (mZCurve && mZCurve->GetKeyCount() > 0)
		result.Vector3Result.z = mZCurve->Evaluate(curveT);
}

void GfxVector3Curve::EvaluateClamp(float curveT, CurveResult& result)
{
	result.Type = Type_Vector3;
	if (mXCurve&& mXCurve->GetKeyCount() > 0)
		result.Vector3Result.x = mXCurve->EvaluateClamp(curveT);
	if (mYCurve&& mXCurve->GetKeyCount() > 0)
		result.Vector3Result.y = mYCurve->EvaluateClamp(curveT);
	if (mZCurve && mZCurve->GetKeyCount() > 0)
		result.Vector3Result.z = mZCurve->EvaluateClamp(curveT);
}

void WriteCurve(XNDNode* node, GfxCurveTpl<float>* curve, int index)
{
	std::string dataName("CurveData");
	if (curve != nullptr)
	{
		auto att = node->AddAttrib((dataName + std::to_string(index)).c_str());
		UINT count = curve->GetKeyCount();
		att->BeginWrite();
		att->Write(count);
		if (count > 0)
		{
			att->Write(curve->GetPreInfinity());
			att->Write(curve->GetGetPostInfinity());
			//auto key0 = curve->GetKeyData().data()[0];
			//auto size = sizeof(CurveKeyTpl<float>);
			att->Write(&curve->GetKeyData()[0], sizeof(CurveKeyTpl<float>)*count);
		}
		att->EndWrite();
	}
}

void GfxVector3Curve::Save2Xnd(XNDNode* node)
{
	WriteCurve(node, mXCurve, 0);
	WriteCurve(node, mYCurve, 1);
	WriteCurve(node, mZCurve, 2);
}

GfxCurveTpl<float>* ReadCurve(XNDNode* node, int index)
{
	GfxCurveTpl<float>* curve = nullptr;
	std::string dataName("CurveData");
	auto att = node->GetAttrib((dataName + std::to_string(index)).c_str());
	if (att)
	{
		UINT count = 0;
		att->BeginRead(__FILE__, __LINE__);
		att->Read(count);
		if (count > 0)
		{
			curve = new GfxCurveTpl<float>();
			WrapMode pre, post;
			att->Read(pre);
			curve->SetPreInfinity(pre);
			att->Read(post);
			curve->SetPostInfinity(post);
			curve->GetKeyData().resize(count);
			att->Read(&curve->GetKeyData()[0], sizeof(CurveKeyTpl<float>)*count);
		}
		att->EndRead();
	}
	return curve;
}

vBOOL GfxVector3Curve::LoadXnd(IRenderContext* rc, XNDNode* node)
{
	//mXCurve = new GfxCurveTpl<float>();
	mXCurve = ReadCurve(node, 0);
	//mYCurve = new GfxCurveTpl<float>();
	mYCurve = ReadCurve(node, 1);
	//mZCurve = new GfxCurveTpl<float>();
	mZCurve = ReadCurve(node, 2);
	return TRUE;
	//std::string dataName("CurveData");
	//for (int i = 0; i < 3; ++i)
	//{
	//	auto att = node->GetAttrib((dataName + std::to_string(i)).c_str());
	//	if (att)
	//	{
	//		UINT count = 0;
	//		att->BeginRead(__FILE__, __LINE__);
	//		att->Read(count);
	//		if (count > 0)
	//		{
	//			mCurves[i] = new GfxCurveTpl<float>();
	//			WrapMode pre, post;
	//			att->Read(pre);
	//			mCurves[i]->SetPreInfinity(pre);
	//			att->Read(post);
	//			mCurves[i]->SetPostInfinity(post);
	//			mCurves[i]->GetKeyData().resize(count);
	//			att->Read(&mCurves[i]->GetKeyData()[0], sizeof(CurveKeyTpl<float>)*count);
	//		}
	//		att->EndRead();
	//	}
	//}
	//return TRUE;
}


NS_END
