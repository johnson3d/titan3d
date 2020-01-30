#include "GfxSkeletonAnimation.h"
#include "Pose/GfxAnimationPose.h"
#include "GfxBoneAnim.h"
#include "AnimCurve/GfxVector3Curve.h"
#include "AnimCurve/GfxQuaternionCurve.h"
#include "AnimCurve/GfxCurveTpl.h"
#include "AnimCurve/GfxBoneCurve.h"
#include "AnimElement/GfxAnimationElement.h"
#include "AnimElement/GfxSkeletonAnimationElement.h"

#define new VNEW
NS_BEGIN

void EnsureQuaternionContinuityPreserveSlope(GfxQuaternionCurve& curve)
{
	if (!curve.IsValid())
		return;

	int keyCount = curve.GetKeyCount();

	auto last = curve.GetKey(keyCount - 1).Value;
	for (int i = 0; i < keyCount; i++)
	{
		auto cur= curve.GetKey(i).Value;
		if (cur.dot(last) < 0.0F)
		{
			cur = v3dxQuaternion(-cur.x, -cur.y, -cur.z, -cur.w);
			curve.GetKey(i).Value = cur;
			curve.GetKey(i).InSlope = -curve.GetKey(i).InSlope;
			curve.GetKey(i).OutSlope = -curve.GetKey(i).OutSlope;
		}
		last = cur;
	}
}
void HermiteCooficients(double t, double& a, double& b, double& c, double& d)
{
	double t2 = t * t;
	double t3 = t2 * t;

	a = 2.0F * t3 - 3.0F * t2 + 1.0F;
	b = t3 - 2.0F * t2 + t;
	c = t3 - t2;
	d = -2.0F * t3 + 3.0F * t2;
}
void FitTangents(CurveKeyTpl<v3dxQuaternion>& key0, CurveKeyTpl<v3dxQuaternion>& key1, float time1, float time2, const v3dxQuaternion& value1, const v3dxQuaternion& value2)
{
	const float dt = key1.Time - key0.Time;


	if (fabsf(dt) < std::numeric_limits<float>::epsilon())
	{
		key0.OutSlope = v3dxQuaternion::ZERO;
		key1.InSlope = v3dxQuaternion::ZERO;
	}
	else
	{
		// p0 and p1 for Hermite curve interpolation equation
		const v3dxQuaternion p0 = key0.Value;
		const v3dxQuaternion p1 = key1.Value;
		// Hermite coefficients at points time1 and time2
		double a1, b1, c1, d1;
		double a2, b2, c2, d2;

		// TODO : try using doubles, because it doesn't work well when p0==p1==v0==v1
		HermiteCooficients(time1, a1, b1, c1, d1);
		HermiteCooficients(time2, a2, b2, c2, d2);

		for (int i = 0; i < 4; ++i)
		{
			// we need to solve these two equations in order to find m0 and m1
			// b1 * m0 + c1 * m1 = v0 - a1 * p0 - d1 * p1;
			// b2 * m0 + c2 * m1 = v1 - a2 * p0 - d2 * p1;

			// c1, c2 is never equal 0, because time1 and time2 not equal to 0

			// divide by c1 and c2
			// b1 / c1 * m0 + m1 = (v0 - a1 * p0 - d1 * p1) / c1;
			// b2 / c2 * m0 + m1 = (v1 - a2 * p0 - d2 * p1) / c2;

			// subtract one from another
			// b1 / c1 * m0 - b2 / c2 * m0 = (v0 - a1 * p0 - d1 * p1) / c1 - (v1 - a2 * p0 - d2 * p1) / c2;

			// solve for m0
			// (b1 / c1 - b2 / c2) * m0 = (v0 - a1 * p0 - d1 * p1) / c1 - (v1 - a2 * p0 - d2 * p1) / c2;

			const double v0 = value1[i];
			const double v1 = value2[i];
			const double pp0 = p0[i];
			const double pp1 = p1[i];

			// calculate m0
			const double m0 = ((v0 - a1 * pp0 - d1 * pp1) / c1 - (v1 - a2 * pp0 - d2 * pp1) / c2) / (b1 / c1 - b2 / c2);

			// solve for m1 using m0
			// c1 * m1 = p0 - a1 * p0 - d1 * p1 - b1 * m0;

			// calculate m1
			const double m1 = (v0 - a1 * pp0 - d1 * pp1 - b1 * m0) / c1;

			key0.OutSlope[i] = static_cast<float>(m0 / dt);
			key1.InSlope[i] = static_cast<float>(m1 / dt);
		}
	}
}

RTTI_IMPL(EngineNS::GfxSkeletonAnimation, EngineNS::VIUnknown);
GfxSkeletonAnimation::GfxSkeletonAnimation()
{
}

GfxSkeletonAnimation::~GfxSkeletonAnimation()
{
}

void EngineNS::GfxSkeletonAnimation::InitBySkeletonActioin(GfxSkeletonAction* action)
{
	GfxSkeletonAnimationElement* skeletonAnimEle = new GfxSkeletonAnimationElement();
	for (UINT i = 0; i < action->mBonesAnim.size(); ++i)
	{
		GfxAnimationElement* animElement = new GfxAnimationElement();
		GfxBoneCurve* curve = new GfxBoneCurve();
		GfxBoneAnim* anim = action->mBonesAnim[i];
		curve->mBoneData = anim->mBoneData;
		animElement->SetCurve(curve);
		GfxAnimationElementDesc* desc = new GfxAnimationElementDesc();
		desc->Name = curve->mBoneData->Name;
		desc->NameHash = curve->mBoneData->NameHash;
		desc->Parent = curve->mBoneData->Parent;
		desc->ParentHash = curve->mBoneData->ParentHash;
		desc->GrantParent = curve->mBoneData->GrantParent;
		desc->GrantParentHash = curve->mBoneData->GrantParentHash;
		animElement->SetAnimationElementDesc(desc);
		animElement->SetAnimationElementType(AET_Bone);
		//if(anim->mBoneData->NameHash != 1247896265)
		//	continue;
		float startTime = 0; float endTime = 0;
		v3dxVector3 startPos, endPos;
		v3dxVector3 lastOutSlope, currentOutSlope;
		GfxVector3Curve* posCurve = new GfxVector3Curve();
		posCurve->mXCurve->SetPostInfinity(WrapMode_Clamp);
		posCurve->mYCurve->SetPostInfinity(WrapMode_Clamp);
		posCurve->mZCurve->SetPostInfinity(WrapMode_Clamp);
		UINT start = 0;
		UINT count = anim->mPosTimeKeys.GetKeyCount();
		UINT end = count + start;
		for (UINT j = start; j < end; ++j)
		{
			CurveKeyTpl<float> currentXKey, currentYKey, currentZKey;
			
			startTime = (float)anim->mPosTimeKeys[j] * 0.001f;
			startPos = anim->mPosFrames[j];

			currentXKey.Time = startTime;
			currentXKey.Value = startPos.x;

			currentYKey.Time = startTime;
			currentYKey.Value = startPos.y;

			currentZKey.Time = startTime;
			currentZKey.Value = startPos.z;
			posCurve->mXCurve->AddKey(currentXKey);
			posCurve->mYCurve->AddKey(currentYKey);
			posCurve->mZCurve->AddKey(currentZKey);
		}
		for (int j = 0; j < (int)count - 1; ++j)
		{
			CurveKeyTpl<v3dxVector3> key0, key1;
			key0.Time =    posCurve->mXCurve->GetKey(j).Time;
			key0.Value.x = posCurve->mXCurve->GetKey(j).Value;
			key0.Value.y = posCurve->mYCurve->GetKey(j).Value;
			key0.Value.z = posCurve->mZCurve->GetKey(j).Value;
			key1.Time =    posCurve->mXCurve->GetKey(j + 1).Time;
			key1.Value.x = posCurve->mXCurve->GetKey(j + 1).Value;
			key1.Value.y = posCurve->mYCurve->GetKey(j + 1).Value;
			key1.Value.z = posCurve->mZCurve->GetKey(j + 1).Value;
			v3dxVector3 in, out;
			out = (key1.Value - key0.Value) / (key1.Time - key0.Time);
			posCurve->mXCurve->GetKey(j).OutSlope = out.x;
			posCurve->mYCurve->GetKey(j).OutSlope = out.y;
			posCurve->mZCurve->GetKey(j).OutSlope = out.z;
			posCurve->mXCurve->GetKey(j + 1).InSlope = out.x;
			posCurve->mYCurve->GetKey(j + 1).InSlope = out.y;
			posCurve->mZCurve->GetKey(j + 1).InSlope = out.z;
		}
		curve->mPosCurve = posCurve;

		v3dxQuaternion startRot, endRot;
		v3dxQuaternion lastOutSlopeRot, currentOutSlopeRot;
		GfxQuaternionCurve* rotCurve = new GfxQuaternionCurve();
		rotCurve->mCurve.SetPostInfinity(WrapMode_Clamp);
		rotCurve->mCurve.SetPreInfinity(WrapMode_Clamp);
		CurveKeyTpl<v3dxQuaternion> currentRotKey;
		count = anim->mRotateTimeKeys.GetKeyCount();
		end = count + start;
		for (UINT j = start; j < end; ++j)
		{
			startTime = (float)anim->mRotateTimeKeys[j] * 0.001f;
			startRot = anim->mRotateFrames[j];

			currentRotKey.Time = startTime;
			currentRotKey.Value = startRot;
			rotCurve->mCurve.AddKey(currentRotKey);
		}
		for (int j = 0; j < (int)count - 1; ++j)
		{
			CurveKeyTpl<v3dxQuaternion>& key0 = rotCurve->GetKey(j);
			CurveKeyTpl<v3dxQuaternion>& key1 = rotCurve->GetKey(j + 1);
			if (key0.Value.dot(key1.Value) < 0)
			{
				key1.Value = -key1.Value;
			}
		}
		for (int j = 0; j < (int)count - 1; ++j)
		{
			CurveKeyTpl<v3dxQuaternion>& key0 = rotCurve->GetKey(j);
			CurveKeyTpl<v3dxQuaternion>& key1 = rotCurve->GetKey(j + 1);
			v3dxVector3 lV, rV;
			v3dxYawPitchRollQuaternionRotation(key0.Value, &lV);
			v3dxYawPitchRollQuaternionRotation(key1.Value, &rV);
			float lTime = key0.Time;
			float rTime = key1.Time;
			v3dxVector3 lVDelta, rVDelta;
			v3dxVec3Lerp(&lVDelta, &lV, &rV, 0.001F);
			v3dxVec3Lerp(&rVDelta, &lV, &rV, 0.999F);
			v3dxQuaternion lR, rR;

// 			v3dxQuaternionRotationYawPitchRoll(&lR, lVDelta.y, lVDelta.x, lVDelta.z);
// 			v3dxQuaternionRotationYawPitchRoll(&rR, rVDelta.y, rVDelta.x, rVDelta.z);
			v3dxQuaternionSlerp(&lR, &key0.Value, &key1.Value, 0.001f);
			v3dxQuaternionSlerp(&rR, &key0.Value, &key1.Value, 0.999f);
			/*if (lR.dot(key0.Value) < 0.0F)
				lR = -lR;
			if (rR.dot(key1.Value) < 0.0F)
				rR = -rR;*/
			key0.OutSlope = (lR - key0.Value) * 1000 / (rTime - lTime);
			key1.InSlope = (key1.Value - rR) * 1000 / (rTime - lTime);
			if (key0.OutSlope.x < -10 || key0.OutSlope.x >10)
			{
				int s = 0;
			}
		}
		EnsureQuaternionContinuityPreserveSlope(*rotCurve);
		curve->mRotationCurve = rotCurve;

		GfxVector3Curve* scaleCurve = new GfxVector3Curve();
		curve->mScaleCurve = scaleCurve;

		skeletonAnimEle->AddElement(animElement);
		mBoneCurves.push_back(curve);
		mBoneCurveMap.insert(std::make_pair(curve->mBoneData->NameHash, curve));
	}
}

void EngineNS::GfxSkeletonAnimation::EvaluatePose(float time, GfxAnimationPose* outPose, vBOOL evaluateMotionState)
{
	ASSERT(outPose);
	for (auto i : outPose->mBones)
	{
		auto boneCurve = mBoneCurveMap[i->mSharedData->NameHash];
		if (boneCurve == nullptr)
		{
			auto parentBone = outPose->FindBone(i->mSharedData->ParentHash);
			auto mat = i->mSharedData->InitMatrix;
			if (parentBone != NULL)
			{
				v3dxMatrix4 parentInvMat = v3dxMatrix4::IDENTITY;;
				parentInvMat = parentBone->mSharedData->InvInitMatrix;
				mat = mat * parentInvMat;
			}
			v3dxVector3 pos, scale;
			v3dxQuaternion rot;
			mat.ExtractionTrans(pos);
			mat.ExtractionRotation(rot);
			mat.ExtractionScale(scale);

			i->Transform.Position = pos;
			i->Transform.Rotation = rot;
			i->Transform.Scale = scale;
		}
		else
		{
			CurveResult transform;
			boneCurve->Evaluate(time, transform);
			i->Transform = *(GfxTransform*)&transform;
			if (evaluateMotionState)
				boneCurve->EvaluateMotionState(time, &(i->MotionData));
			if (!boneCurve->GetPosCurve()->GetKeyCount())
			{
				auto parentBone = outPose->FindBone(i->mSharedData->ParentHash);
				auto mat = i->mSharedData->InitMatrix;
				if (parentBone != NULL)
				{
					v3dxMatrix4 parentInvMat = v3dxMatrix4::IDENTITY;;
					parentInvMat = parentBone->mSharedData->InvInitMatrix;
					mat = mat * parentInvMat;
				}
				v3dxVector3 pos, scale;
				v3dxQuaternion rot;
				mat.ExtractionTrans(pos);
				i->Transform.Position = pos;
			}
		}
	}
	auto root = outPose->GetRoot();
	if (root != nullptr)
	{
		root->AbsTransform = root->Transform;
		root->LinkBone(outPose);
	}
	else
	{
		ASSERT(FALSE);
	}
	for (auto bone : outPose->mBones)
	{
		for (auto j : bone->GrantChildren)
		{
			auto grantBone = outPose->GetBone(j);
			ASSERT(grantBone != nullptr);
			if (grantBone == nullptr)
				continue;
			//grantBone->AbsTransform.Rotation.slerp(bone->AbsTransform.Rotation, grantBone->mSharedData->GrantWeight);
			grantBone->Transform.Rotation.slerp(bone->Transform.Rotation, grantBone->mSharedData->GrantWeight);
		}
	}
}
NS_END
using namespace EngineNS;

extern "C"
{
	CSharpAPI3(EngineNS, GfxSkeletonAnimation, EvaluatePose, float, GfxAnimationPose*, vBOOL);
	CSharpAPI1(EngineNS, GfxSkeletonAnimation, InitBySkeletonActioin, GfxSkeletonAction*);
	//CSharpReturnAPI0(int, EngineNS, GfxCurve, GetKeyCount);
	//CSharpReturnAPI1(float, EngineNS, GfxCurve, Evaluate, float);
}