#include "GfxBoneCurve.h"
#include "../Skeleton/GfxBone.h"

#define new VNEW
NS_BEGIN
RTTI_IMPL(EngineNS::GfxBoneCurve, EngineNS::VIUnknown);
GfxBoneCurve::GfxBoneCurve()
{
	mType = Type_Bone;
}

GfxBoneCurve::~GfxBoneCurve()
{

}

void GfxBoneCurve::EvaluateMotionState(float curveT, GfxMotionState* motionData)
{
	if (mMotionStateCurve)
		*motionData = mMotionStateCurve->Evaluate(curveT);
}

void GfxBoneCurve::Evaluate(float curveT, CurveResult& result)
{
	result.Type = Type_Bone;
	CurveResult temp;
	if (mPosCurve)
	{
		temp.Vector3Result = result.BoneSRTResult.Position;
		mPosCurve->Evaluate(curveT, temp);
		result.BoneSRTResult.Position = temp.Vector3Result;
	}
	if (mRotationCurve)
	{
		temp.QuaternionResult = result.BoneSRTResult.Rotation;
		mRotationCurve->Evaluate(curveT, temp);
		auto quat = temp.QuaternionResult;
		v3dxVector3 rot;
		v3dxYawPitchRollQuaternionRotation(*(v3dxQuaternion*)&quat, &rot);
		result.BoneSRTResult.Rotation = temp.QuaternionResult;
	}
	if (mScaleCurve)
	{
		temp.Vector3Result = result.BoneSRTResult.Scale;
		mScaleCurve->Evaluate(curveT, temp);
		result.BoneSRTResult.Scale = temp.Vector3Result;
	}
}

void GfxBoneCurve::Save2Xnd(XNDNode* node)
{
	auto att = node->AddAttrib("Desc");
	att->BeginWrite();
	att->Write(mBoneData);
	att->EndWrite();
	if (mPosCurve)
	{
		auto curveNode = node->AddNode("Pos", 0, 0);
		mPosCurve->Save2Xnd(curveNode);
	}
	if (mRotationCurve)
	{
		auto curveNode = node->AddNode("Rotation", 0, 0);
		mRotationCurve->Save2Xnd(curveNode);
	}
	if (mScaleCurve)
	{
		auto curveNode = node->AddNode("Scale", 0, 0);
		mScaleCurve->Save2Xnd(curveNode);
	}
	if (mMotionStateCurve)
	{
		auto curveNode = node->AddNode("MotionState", 0, 0);
		mMotionStateCurve->Save2Xnd(curveNode);
	}
}

vBOOL GfxBoneCurve::LoadXnd(IRenderContext* rc, XNDNode* node)
{
	auto att = node->GetAttrib("Desc");
	if (att)
	{
		att->BeginWrite();
		att->Write(mBoneData);
		att->EndWrite();
	}
	auto curveNode = node->GetChild("Pos");
	if (curveNode)
	{
		mPosCurve = new GfxVector3Curve();
		mPosCurve->LoadXnd(rc, curveNode);
	}
	curveNode = node->GetChild("Rotation");
	if (curveNode)
	{
		mRotationCurve = new GfxQuaternionCurve();
		mRotationCurve->LoadXnd(rc, curveNode);
	}
	curveNode = node->GetChild("Scale");
	if (curveNode)
	{
		mScaleCurve = new GfxVector3Curve();
		mScaleCurve->LoadXnd(rc, curveNode);
	}
	curveNode = node->GetChild("MotionState");
	if (curveNode)
	{
		mMotionStateCurve = new GfxMotionStateCurve();
		mMotionStateCurve->LoadXnd(rc, curveNode);
	}

	return TRUE;
}

NS_END


using namespace EngineNS;
extern "C"
{
	Cpp2CS2(EngineNS, GfxBoneCurve, EvaluateMotionState);
}