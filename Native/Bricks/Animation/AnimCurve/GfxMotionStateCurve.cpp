#include "GfxMotionStateCurve.h"

#define new VNEW
NS_BEGIN
GfxMotionStateCurve::GfxMotionStateCurve()
{

}

GfxMotionStateCurve::~GfxMotionStateCurve()
{

}

GfxMotionState GfxMotionStateCurve::Evaluate(float curveT) const
{
	GfxMotionState result;
	CurveResult temp;
	mPosCurve->Evaluate(curveT, temp);
	result.Position = temp.Vector3Result;
	mVelocityCurve->Evaluate(curveT, temp);
	result.Velocity = temp.Vector3Result;
	return result;
}

void GfxMotionStateCurve::Save2Xnd(XNDNode* node)
{
	if (mPosCurve)
	{
		auto curveNode = node->AddNode("Pos", 0, 0);
		mPosCurve->Save2Xnd(curveNode);
	}
	if (mVelocityCurve)
	{
		auto curveNode = node->AddNode("Velocity", 0, 0);
		mVelocityCurve->Save2Xnd(curveNode);
	}
}

vBOOL GfxMotionStateCurve::LoadXnd(IRenderContext* rc, XNDNode* node)
{
	auto curveNode = node->GetChild("Pos");
	if (curveNode)
	{
		mPosCurve = new GfxVector3Curve();
		mPosCurve->LoadXnd(rc, curveNode);
	}
	curveNode = node->GetChild("Velocity");
	if (curveNode)
	{
		mVelocityCurve = new GfxVector3Curve();
		mVelocityCurve->LoadXnd(rc, curveNode);
	}
	return TRUE;
}

NS_END
