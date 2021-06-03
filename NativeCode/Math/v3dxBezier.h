#ifndef __V3DXBEZIER__
#define __V3DXBEZIER__

#pragma once

#include "vfxGeomTypes.h"
#include "v3dxVector3.h"
#include "v3dxColor4.h"
#include "../Base/IUnknown.h"
#include "../Base/debug/vfxMemPoolObject.h"

#pragma pack(push,4)

NS_BEGIN

class v3dxBezier : public EngineNS::VIUnknown , public VMem::PooledObject<v3dxBezier, 128>
{
public:
	static const char* GetPooledObjectClassName() {
		return "v3dxBezier";
	}
	#pragma pack(push,4)
	struct sBezierPoint : public VMem::PooledObject<sBezierPoint, 128>
	{
		v3dxVector3 vPos;
		v3dxVector3 vCtrlPos1;
		v3dxVector3 vCtrlPos2;

		sBezierPoint() :
			vPos(0,0,0),
			vCtrlPos1(0,0,0),
			vCtrlPos2(0,0,0)
		{}
		static const char* GetPooledObjectClassName() {
			return "sBezierPoint";
		}
	};
	#pragma  pack(pop)

protected:
	std::vector<sBezierPoint*> m_nodeArray;
	float m_fMaxLength;

	vBOOL	mIs2D;
	void CalculateMaxLength();
	void CalculateMaxLength2D();
public:
	v3dxBezier(vBOOL Is2D=TRUE);
	virtual ~v3dxBezier(void);

	void AddNode(const v3dxVector3* pos, const v3dxVector3* ctrlPos1, const v3dxVector3* ctrlPos2);
	void InsertNode(int idx, const v3dxVector3* pos, const v3dxVector3* ctrlPos1, const v3dxVector3* ctrlPos2);
	void DeleteNode(int idx);
	void ClearNodes();
	
	v3dxVector3 GetValue(float fTime);
	inline void GetValue(float fTime, v3dxVector3* value)
	{
		*value = GetValue(fTime);
	}
	inline void GetPosition(int idx, v3dxVector3* pos)
	{
		v3dxBezier::sBezierPoint* node = GetNode(idx);
		if (node != NULL)
			*pos = node->vPos;
	}
	vBOOL IsInRangeX(float value);
	void GetRangeX(float* begin, float* end);
	vBOOL IsInRangeY(float value);
	void GetRangeY(float* begin, float* end);
	vBOOL IsInRangeZ(float value);
	void GetRangeZ(float* begin, float* end);

	void SetPosition(int idx, const v3dxVector3* pos);
	void SetControlPos1(int idx, const v3dxVector3* pos, vBOOL bWithPos2);
	void SetControlPos2(int idx, const v3dxVector3* pos, vBOOL bWithPos1);
	inline void GetControlPos1(int idx, v3dxVector3* pos)
	{
		v3dxBezier::sBezierPoint* node = GetNode(idx);
		if (node != NULL)
			*pos = node->vCtrlPos1;
	}
	inline void GetControlPos2(int idx, v3dxVector3* pos)
	{
		v3dxBezier::sBezierPoint* node = GetNode(idx);
		if (node != NULL)
			*pos = node->vCtrlPos2;
	}
	vBOOL CopyTo(v3dxBezier* cloneTo) const;

	inline int GetNodesCount() {
		return (int)m_nodeArray.size();
	}
	inline sBezierPoint* GetNode(int idx) {
		if (idx >= 0 && idx < (int)m_nodeArray.size())
			return m_nodeArray[idx];
		return NULL;
	}

	float GetValueY_2D(float fTime);
};

template <typename T>
class v3dxRange
{
public:
	T mBegin;
	T mEnd;

	v3dxRange()
	{
	}
	v3dxRange(const T& val)
		: mBegin(val), mEnd(val)
	{
	}
	v3dxRange(T bgn, T end)
		: mBegin(bgn), mEnd(end)
	{
	}

	void setValue(const T& val) {
		mBegin = mEnd = val;
	}
	T getValue(float slider) {
		return getMin() + (T)(slider * getRange());
	}
	T getMin() {
		return mBegin <= mEnd ? mBegin : mEnd;
	}
	T getMax() {
		return mBegin >= mEnd ? mBegin : mEnd;
	}
	void setRange(const T& bgn, const T& end) {
		mBegin = bgn;
		mEnd = end;
	}
	void setBegin(const T& bgn) {
		mBegin = bgn;
	}
	void setEnd(const T& end) {
		mEnd = end;
	}
	T getRange() {
		return getMax() - getMin();
	}
	bool isInRange(const T& val) {
		return val <= getMax() && val >= getMin();
	}
	T getMid() {
		return getMin() + (T)(getRange() * 0.5f);
	}
	T rand() {
		return getMin() + getRange() * Math::UnitRandom();
	}
};
typedef v3dxRange<int> v3dxIntRange;
typedef v3dxRange<float> v3dxScalarRange;
typedef v3dxRange<unsigned long> v3dxColorRange;

class v3dVariable : public VIUnknown
{
public:
	enum Type
	{
		Constant,
		ConstantRange,
		Curve,
	};
	v3dVariable(Type type)
		: mType(type) {
	}
	Type getType() {
		return (Type)mType;
	}
protected:
	unsigned char mType;
};

class v3dScalarVariable : public v3dVariable
{
public:
	v3dScalarVariable();
	v3dScalarVariable(float value);
	v3dScalarVariable(float begin, float end);
	v3dScalarVariable(const v3dxBezier& cv);

	virtual ~v3dScalarVariable();
	float getConstant();
	v3dxScalarRange *GetConstantRange();
	v3dxBezier *getCurve();
	float getValue(float slider);
	float getRandomValue();
	bool isInRange(float value);
	inline float GetValueBegin() {
		return mValueBegin;
	}
	inline float GetValueEnd() {
		return mValueEnd;
	}
	void setValue(float value);
	//V3D_API void setValue(float begin, float end, bool curve);
	void setValueBegin(float value);
	void setValueEnd(float value);
	//private:
	void changeType(Type type);

	void SetChangeToTypeEnable(Type type, bool enable);
	bool CanChangeToType(Type type);

	v3dxBezier* GetBezier();

	v3dScalarVariable *Clone();

	void CopyFrom(const v3dScalarVariable *src);

private:
	v3dxScalarRange * mConstantRange;
	v3dxBezier *mBezier2D;
	float mConstant;
	float mValueBegin;
	float mValueEnd;
	DWORD mChangeToMask;
};

class v3dColorVariable : public v3dVariable
{
public:
	v3dColorVariable();
	v3dColorVariable(const v3dxColor4& value);
	v3dColorVariable(const v3dxColor4& begin, const v3dxColor4& end);
	virtual ~v3dColorVariable();
	void setValue(const v3dxColor4 &clr);
	void setValue(const v3dxColor4 &bgn, const v3dxColor4 &end);
	v3dxColor4 getValue();
	v3dxColor4 getValueBegin();
	v3dxColor4 getValueEnd();
	v3dxColor4 getValue(float slider);
	v3dxColor4 getRandomValue();
	void CopyFrom(const v3dColorVariable *src);

private:
	void changeType(v3dVariable::Type type);

	union
	{
		unsigned long mConstant;
		v3dxColorRange *mConstantRange;
	};
};

class v3dFloat4Variable : public v3dVariable
{
public:
	v3dFloat4Variable();
	v3dFloat4Variable(const v3dVector4_t& value);
	v3dFloat4Variable(const v3dVector4_t& begin, const v3dVector4_t& end);
	virtual ~v3dFloat4Variable();
	void setValue(const v3dVector4_t &val);
	void setValue(const v3dVector4_t &bgn, const v3dVector4_t &end);
	v3dVector4_t getValue();
	v3dVector4_t getValueBegin();
	v3dVector4_t getValueEnd();
	v3dVector4_t getValue(float slider);
	v3dVector4_t getRandomValue();
	void CopyFrom(const v3dFloat4Variable *src);

private:
	void changeType(v3dVariable::Type type);

	struct _range
	{
		v3dVector4_t mBegin;
		v3dVector4_t mEnd;
	};
	union
	{
		v3dVector4_t mConstant;
		_range mRange;
	};
};

NS_END

#pragma pack(pop)

#endif