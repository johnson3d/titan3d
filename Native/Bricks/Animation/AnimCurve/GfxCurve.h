#pragma once
#include "../../../Graphics/GfxPreHead.h"
#include "GfxCurveTpl.h"

NS_BEGIN
struct CurveKey
{
	// DECLARE_SERIALIZE_OPTIMIZE_TRANSFER (Keyframe)
	inline static const char* GetTypeString() { return "Keyframe"; }
	inline static bool IsValid(const CurveKey & key) { return IsFinite(key.Value) && IsFinite(key.Time); }

	float Time;
	float Value;
	float InSlope;
	float OutSlope;

	CurveKey()
	{
		Value = 0.0F;
		InSlope = 0.0F;
		OutSlope = 0.0F;
		Time = 0.0F;
	}
	CurveKey(float t, const float& v)
	{
		Time = t;
		Value = v;
		InSlope = 0.0F;
		OutSlope = 0.0F;
	}


	friend bool operator<(const CurveKey& lhs, const CurveKey& rhs) { return lhs.Time < rhs.Time; }

	bool operator==(CurveKey const& other) const
	{
		return Time == other.Time &&
			Value == other.Value &&
			InSlope == other.InSlope &&
			OutSlope == other.OutSlope;
	}
};

struct Cache
{
	int Index;
	float Time;
	float TimeEnd;
	float Coeff[4];

	Cache() { Time = std::numeric_limits<float>::infinity(); Index = 0; TimeEnd = 0.0f; memset(&Coeff, 0, sizeof(Coeff)); }
	void Invalidate() { Time = std::numeric_limits<float>::infinity(); Index = 0; }
};
//enum WrapMode
//{
//	// Must be kept in sync with WrapMode in InternalUtility.bindings
//	Default = 0,
//	Clamp = 1 << 0,
//	Repeat = 1 << 1,
//	PingPong = 1 << 2,
//	ClampForever = 1 << 3
//};

class GfxCurve : public VIUnknown
{
public:
	typedef CurveKey Keyframe;
	typedef std::vector<Keyframe> KeyframeList;
	typedef typename KeyframeList::iterator iterator;
	typedef typename KeyframeList::const_iterator const_iterator;
public:
	RTTI_DEF(GfxCurve, 0x90480df55ce39abc, true);
	GfxCurve();
	~GfxCurve();

	/// Evaluates the AnimationCurve caching the segment.
	float Evaluate(float curveT) const { return EvaluateWitCache(curveT, NULL); };
	float EvaluateClamp(float curveT) const { return EvaluateClampWitCache(curveT, NULL); };
	float EvaluateWitCache(float curveT, Cache* cache = NULL) const;
	float EvaluateClampWitCache(float curveT, Cache* cache = NULL) const;

	bool IsValid() const { return mKeyframes.size() >= 1 && IsFinite(GetRange().first) && IsFinite(GetRange().second); }

	int AddKey(const Keyframe& key);

	/// Performs no error checking. And doesn't invalidate the cache!
	void AddKeyBackFast(const Keyframe& key) { mKeyframes.push_back(key); }

	const Keyframe& GetKey(int index) const { 
		if (index < mKeyframes.size()) 
			return mKeyframes[index]; 
		else
			return mKeyframes[0];
	}

	/// When changing the keyframe using GetKey you are not allowed to change the time!
	/// After modifying a key you have to call InvalidateCache
	Keyframe& GetKey(int index) { 
		if (index < mKeyframes.size())
			return mKeyframes[index];
		else
			return mKeyframes[0];
	}

	iterator begin() { return mKeyframes.begin(); }
	iterator end() { return mKeyframes.end(); }
	const_iterator begin() const { return mKeyframes.begin(); }
	const_iterator end() const { return mKeyframes.end(); }

	void InvalidateCache();

	int GetKeyCount() const { return (int)mKeyframes.size(); }

	void RemoveKeys(iterator begin, iterator end);

	/// Returns the first and last keyframe time
	std::pair<float, float> GetRange() const;

	// How does the curve before the first keyframe
	void SetPreInfinity(WrapMode pre);
	WrapMode GetPreInfinity() const;
	// How does the curve behave after the last keyframe
	void SetPostInfinity(WrapMode post);
	WrapMode GetPostInfinity() const;

	// How does the curve before the first keyframe
	void SetPreInfinityInternal(WrapMode pre) { mPreInfinity = pre; InvalidateCache(); }
	WrapMode GetPreInfinityInternal() const { return mPreInfinity; }
	// How does the curve behave after the last keyframe
	void SetPostInfinityInternal(WrapMode post) { mPostInfinity = post; InvalidateCache(); }
	WrapMode GetPostInfinityInternal() const { return mPostInfinity; }

	void Assign(const Keyframe& key) { mKeyframes.assign(&key, &key + 1); StripInvalidKeys(); InvalidateCache(); }
	void Assign(const Keyframe* begin, const Keyframe* end) { mKeyframes.assign(begin, end); StripInvalidKeys(); InvalidateCache(); }
	void Swap(KeyframeList& newArray) { mKeyframes.swap(newArray); StripInvalidKeys(); InvalidateCache(); }
	void Sort() { std::sort(mKeyframes.begin(), mKeyframes.end()); InvalidateCache(); }

	void StripInvalidKeys();

	void Reserve(int size) { mKeyframes.reserve(size); }
	//void ResizeUninitialized(int size) { mKeyframes.resize_uninitialized(size); }

	//void SetRotationOrder(math::RotationOrder order) { m_RotationOrder = order; }
	//math::RotationOrder GetRotationOrder() const { return (math::RotationOrder)m_RotationOrder; }

	///@TODO: Cleanup old code to completely get rid of this
	int FindIndex(const Cache& cache, float curveT) const;

	///@TODO: Cleanup old code to completely get rid of this
	/// Returns the closest keyframe index that is less than time.
	/// Returns -1 if time is outside the range of the curve
	int FindIndex(float time) const;

	void CalculateCacheData(Cache& cache, int lhs, int rhs, float timeOffset) const;


	bool operator==(GfxCurve const& other) const
	{
		return mKeyframes == other.mKeyframes &&
			mPreInfinity == other.mPreInfinity &&
			mPostInfinity == other.mPostInfinity;//&&
			//m_RotationOrder == other.m_RotationOrder;
	}

	inline bool operator!=(GfxCurve const& other) const { return !(*this == other); }


	float WrapTime(float curveT) const;


private:

	void FindIndexForSampling(const Cache& cache, float curveT, int& lhs, int& rhs) const;

	/// Evaluates the AnimationCurve directly.
	void EvaluateWithoutCache(float curveT, float& output) const;

protected:
	KeyframeList mKeyframes;
	WrapMode mPreInfinity;
	WrapMode mPostInfinity;
	mutable Cache mCache;
	mutable Cache mClampCache;
};

struct KeyframeCompare
{

	bool operator()(CurveKey const& k, float t) { return k.Time < t; }
	// These are necessary for debug STL (validation of predicates)

	bool operator()(CurveKey const& k1, CurveKey const& k2) { return k1.Time < k2.Time; }

	bool operator()(float t, CurveKey const& k) { return !operator()(k, t); }
};

NS_END