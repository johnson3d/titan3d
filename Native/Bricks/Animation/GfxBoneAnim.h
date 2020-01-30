#pragma once
#include "../../Graphics/GfxPreHead.h"
#include "Skeleton/GfxBone.h"

NS_BEGIN
class GfxBoneCurve;
class GfxBoneAnim : public VIUnknown
{
public:
	GfxBoneAnim();
	~GfxBoneAnim();
	
	bool LoadHead(XNDAttrib* attrib);
	bool LoadHead_Ver1(XNDAttrib* attrib);
	bool LoadHead_Ver2(XNDAttrib* attrib);
	bool LoadData(XNDAttrib* attrib);

	void Save2Xnd(XNDAttrib* headAttrib, XNDAttrib* dataAttrib);

	GfxBoneDesc* GetBoneDesc() {
		return mBoneData;
	}
	inline bool GetMotionData(INT64 time, GfxMotionState& motionData)
	{
		if (mMotionDatas.size() == 0)
			return false;
		UINT frame1 = 0;
		UINT frame2 = 0;
		float lerp = 0.0f;
		if (mPosTimeKeys.GetKeyCount() > 0)
		{
			mPosTimeKeys.GetTweenTimeParam(time, frame1, frame2, lerp);
			if (frame1 == frame2)
			{
				motionData = mMotionDatas[frame1];
			}
			else
			{
				v3dxVec3Lerp(&motionData.Position, &mMotionDatas[frame1].Position, &mMotionDatas[frame2].Position, lerp);
				v3dxVec3Lerp(&motionData.Velocity, &mMotionDatas[frame1].Velocity, &mMotionDatas[frame2].Velocity, lerp);
			}
		}
		return true;
	}
	inline bool GetTransform(INT64 time, GfxBoneTransform& transform)
	{
		UINT frame1 = 0;
		UINT frame2 = 0;
		float lerp = 0.0f;
		if (mPosTimeKeys.GetKeyCount() > 0)
		{
			mPosTimeKeys.GetTweenTimeParam(time, frame1, frame2, lerp);
			if (frame1 == frame2)
			{
				transform.Position = mPosFrames[frame1];
			}
			else
			{
				v3dxVec3Lerp(&transform.Position,&mPosFrames[frame1],&mPosFrames[frame2],lerp);
			}
		}
		if (mScaleTimeKeys.GetKeyCount() > 0)
		{
			mScaleTimeKeys.GetTweenTimeParam(time, frame1, frame2, lerp);
			if (frame1 == frame2)
			{
				transform.Scale = mScaleFrames[frame1];
			}
			else
			{
				v3dxVec3Lerp(&transform.Scale, &mScaleFrames[frame1], &mScaleFrames[frame2], lerp);
			}
		}
		if (mRotateTimeKeys.GetKeyCount() > 0)
		{
			mRotateTimeKeys.GetTweenTimeParam(time, frame1, frame2, lerp);
			if (frame1 == frame2)
			{
				transform.Rotation = mRotateFrames[frame1];
			}
			else
			{
				v3dxQuaternionSlerp(&transform.Rotation, &mRotateFrames[frame1], &mRotateFrames[frame2], lerp);
			}
		}
		return true;
	}
public:
	AutoRef<GfxBoneDesc>		mBoneData;
	
	TimeKeys					mPosTimeKeys;
	TimeKeys					mScaleTimeKeys;
	TimeKeys					mRotateTimeKeys;
	UINT						mMotionDataSize;
	
	bool IsHavePosFrames() { return mPosFrames.size() > 0?true : false; }
	bool IsHaveScaleFrames() { return mScaleFrames.size() > 0?true : false; }
	bool IsHaveRotateFrames() { return mRotateFrames.size() > 0?true : false; }
	bool IsHaveMotionDatas() { return mMotionDatas.size() > 0 ? true : false; }

	void GetBoneCurve(GfxBoneCurve* curve);


	std::vector<v3dxVector3>	mPosFrames;
	std::vector<v3dxVector3>	mScaleFrames;
	std::vector<v3dxQuaternion>	mRotateFrames;
	std::vector<GfxMotionState>	mMotionDatas;
};

NS_END