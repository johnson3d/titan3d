#include "GfxBoneAnim.h"
#include "AnimCurve/GfxBoneCurve.h"
#include "AnimElement/GfxAnimationElement.h"

#define new VNEW

NS_BEGIN

GfxBoneAnim::GfxBoneAnim(): mMotionDataSize(0)
{
}

GfxBoneAnim::~GfxBoneAnim()
{
}

void GfxBoneAnim::Save2Xnd(XNDAttrib* headAttrib, XNDAttrib* dataAttrib)
{
	//headAttrib
	headAttrib->SetVersion(2);
	headAttrib->WriteText(mBoneData->Name.c_str());
	headAttrib->WriteText(mBoneData->Parent.c_str());

	mPosTimeKeys.Save(*headAttrib);
	mScaleTimeKeys.Save(*headAttrib);
	mRotateTimeKeys.Save(*headAttrib);
	headAttrib->Write((UINT)mMotionDatas.size());


	//dataAttrib
	UINT uAnimFrame = mPosTimeKeys.GetKeyCount();
	if (uAnimFrame > 0)
		dataAttrib->Write(&mPosFrames[0], sizeof(v3dxVector3)*uAnimFrame);

	uAnimFrame = mScaleTimeKeys.GetKeyCount();
	if (uAnimFrame > 0)
		dataAttrib->Write(&mScaleFrames[0], sizeof(v3dxVector3)*uAnimFrame);

	uAnimFrame = mRotateTimeKeys.GetKeyCount();
	if (uAnimFrame > 0)
		dataAttrib->Write(&mRotateFrames[0], sizeof(v3dxQuaternion)*uAnimFrame);

	uAnimFrame = (UINT)mMotionDatas.size();
	if (uAnimFrame > 0)
		dataAttrib->Write(&mMotionDatas[0], sizeof(GfxMotionState)*uAnimFrame);
}

void EngineNS::GfxBoneAnim::GetBoneCurve(GfxBoneCurve* curve)
{
	GfxBoneAnim* anim = this;
	curve->mBoneData = anim->mBoneData;
	//if(anim->mBoneData->NameHash != 1247896265)
	//	continue;
	float startTime = 0; float endTime = 0;
	v3dxVector3 startPos, endPos;
	v3dxVector3 lastOutSlope, currentOutSlope;
	UINT start = 0;
	UINT count = anim->mPosTimeKeys.GetKeyCount();
	UINT end = count + start;
	GfxVector3Curve* posCurve = new GfxVector3Curve();
	posCurve->mXCurve->SetPostInfinity(WrapMode_Clamp);
	posCurve->mYCurve->SetPostInfinity(WrapMode_Clamp);
	posCurve->mZCurve->SetPostInfinity(WrapMode_Clamp);
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
		key0.Time =	   posCurve->mXCurve->GetKey(j).Time;
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
	//EnsureQuaternionContinuityPreserveSlope(*rotCurve);
	curve->mRotationCurve = rotCurve;

	GfxVector3Curve* scaleCurve = new GfxVector3Curve();
	curve->mScaleCurve = scaleCurve;
}

bool GfxBoneAnim::LoadHead(XNDAttrib* attrib)
{
	switch (attrib->GetVersion())
	{
	case 1:
		return LoadHead_Ver1(attrib);
	case 2:
		return LoadHead_Ver2(attrib);
	default:
		break;
	}
	std::string mBoneName;
	attrib->ReadText(mBoneName);
	int mBoneNameHashID;
	attrib->Read(mBoneNameHashID);

	UINT mIndexInSkeleton;
	attrib->Read(mIndexInSkeleton);
	float mBlendFactor;
	attrib->Read(mBlendFactor);
	mPosTimeKeys.Load(*attrib);
	mScaleTimeKeys.Load(*attrib);
	mRotateTimeKeys.Load(*attrib);

	mBoneData = new GfxBoneDesc();
	mBoneData->SetName(mBoneName.c_str());

	return true;
}

bool GfxBoneAnim::LoadHead_Ver1(XNDAttrib* attrib)
{
	std::string mBoneName;
	attrib->ReadText(mBoneName);
	std::string mParentName;
	attrib->ReadText(mParentName);

	mBoneData = new GfxBoneDesc();
	mBoneData->SetName(mBoneName.c_str());
	mBoneData->SetParent(mParentName.c_str());

	mPosTimeKeys.Load(*attrib);
	mScaleTimeKeys.Load(*attrib);
	mRotateTimeKeys.Load(*attrib);
	return true;
}
bool GfxBoneAnim::LoadHead_Ver2(XNDAttrib* attrib)
{
	std::string mBoneName;
	attrib->ReadText(mBoneName);
	std::string mParentName;
	attrib->ReadText(mParentName);

	mBoneData = new GfxBoneDesc();
	mBoneData->SetName(mBoneName.c_str());
	mBoneData->SetParent(mParentName.c_str());

	mPosTimeKeys.Load(*attrib);
	mScaleTimeKeys.Load(*attrib);
	mRotateTimeKeys.Load(*attrib);
	attrib->Read(mMotionDataSize);
	return true;
}

bool GfxBoneAnim::LoadData(XNDAttrib* attrib)
{
	UINT uAnimFrame = mPosTimeKeys.GetKeyCount();
	mPosFrames.resize(uAnimFrame);
	if (uAnimFrame > 0)
		attrib->Read(&mPosFrames[0], sizeof(v3dxVector3)*uAnimFrame);

	uAnimFrame = mScaleTimeKeys.GetKeyCount();
	mScaleFrames.resize(uAnimFrame);
	if (uAnimFrame > 0)
		attrib->Read(&mScaleFrames[0], sizeof(v3dxVector3)*uAnimFrame);

	uAnimFrame = mRotateTimeKeys.GetKeyCount();
	mRotateFrames.resize(uAnimFrame);
	if (uAnimFrame > 0)
		attrib->Read(&mRotateFrames[0], sizeof(v3dxQuaternion)*uAnimFrame);

	mMotionDatas.resize(mMotionDataSize);
	if (mMotionDataSize > 0)
		attrib->Read(&mMotionDatas[0], sizeof(GfxMotionState)*mMotionDataSize);

	return true;
}

NS_END

using namespace EngineNS;

extern "C"
{
	CSharpReturnAPI0(GfxBoneDesc*, EngineNS, GfxBoneAnim, GetBoneDesc);
	CSharpAPI1(EngineNS, GfxBoneAnim, GetBoneCurve, GfxBoneCurve*);
}