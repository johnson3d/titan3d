#pragma once
#include "../../../Graphics/GfxPreHead.h"

NS_BEGIN

template<class T>
inline bool IsFinite(const T& value)
{
	return isfinite(value);
}

template<class T1>
inline T1 Zero() { 
	return T1(); 
}
template<>
inline v3dxQuaternion Zero() {
	return v3dxQuaternion::ZERO;
}

template<class T>
struct CurveKeyTpl
{
	// DECLARE_SERIALIZE_OPTIMIZE_TRANSFER (Keyframe)
	inline static const char* GetTypeString() { return "Keyframe"; }
	inline static bool IsValid(const CurveKeyTpl<T> & key) { return IsFinite(key.Value) && IsFinite(key.Time); }

	float Time;
	T Value;
	T InSlope;
	T OutSlope;

	CurveKeyTpl()
	{
		Value = Zero<T>();
		InSlope = Zero<T>();
		OutSlope = Zero<T>();
		Time = 0.0F;
	}
	CurveKeyTpl(float t, const T& v)
	{
		Time = t;
		Value = v;
		InSlope = Zero<T>();
		OutSlope = Zero<T>();
	}


	friend bool operator<(const CurveKeyTpl& lhs, const CurveKeyTpl& rhs) { return lhs.Time < rhs.Time; }

	bool operator==(CurveKeyTpl const& other) const
	{
		return Time == other.Time &&
			Value == other.Value &&
			InSlope == other.InSlope &&
			OutSlope == other.OutSlope;
	}
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
enum WrapMode
{
	// Values are serialized in CompressedMesh and must be preserved
	WrapMode_Clamp = 1,
	WrapMode_Repeat = 2,
	WrapMode_PingPong = 4,
	Default = 0
};

template<class T>
class GfxCurveTpl : public VIUnknown
{
public:
	typedef CurveKeyTpl<T> Keyframe;
	typedef std::vector<Keyframe> KeyframeList;
	typedef typename KeyframeList::iterator iterator;
	typedef typename KeyframeList::const_iterator const_iterator;
	struct CacheTpl
	{
		int Index;
		float Time;
		float TimeEnd;
		T Coeff[4];

		CacheTpl() { Time = std::numeric_limits<float>::infinity(); Index = 0; TimeEnd = 0.0f; memset(&Coeff, 0, sizeof(Coeff)); }
		void Invalidate() { Time = std::numeric_limits<float>::infinity(); Index = 0; }
	};
public:
	GfxCurveTpl()
	{
		mPreInfinity = Default;
		mPostInfinity = Default;
	}
	~GfxCurveTpl()
	{

	}
	void SetupStepped(T* coeff, const CurveKeyTpl<T>& lhs, const CurveKeyTpl<T>& rhs) const;
	void HandleSteppedCurve(const CurveKeyTpl<T>& lhs, const CurveKeyTpl<T>& rhs, T& value) const;
	/// Evaluates the AnimationCurve caching the segment.
	T Evaluate(float curveT) const { return EvaluateWitCache(curveT, NULL); }; 
	T EvaluateClamp(float curveT) const { return EvaluateClampWitCache(curveT, NULL); };
	T EvaluateWitCache(float curveT, CacheTpl* cache = NULL) const;
	T EvaluateClampWitCache(float curveT, CacheTpl* cache = NULL) const;

	bool IsValid() const { return mKeyframes.size() >= 1 && IsFinite(GetRange().first) && IsFinite(GetRange().second); }

	int AddKey(const Keyframe& key);

	/// Performs no error checking. And doesn't invalidate the cache!
	void AddKeyBackFast(const Keyframe& key) { mKeyframes.push_back(key); }
	KeyframeList& GetKeyData() { return mKeyframes; }
	const Keyframe& GetKey(int index) const
	{
		if (index < mKeyframes.size())
			return mKeyframes[index];
		else
		{
			ASSERT(false);
			static Keyframe error;
			return error;
		}
	}

	/// When changing the keyframe using GetKey you are not allowed to change the time!
	/// After modifying a key you have to call InvalidateCache
	Keyframe& GetKey(int index)
	{
		if (index < mKeyframes.size())
			return mKeyframes[index];
		else
		{
			ASSERT(false);
			static Keyframe error;
			return error;
		}
	}

	iterator begin() { return mKeyframes.begin(); }
	iterator end() { return mKeyframes.end(); }
	const_iterator begin() const { return mKeyframes.begin(); }
	const_iterator end() const { return mKeyframes.end(); }

	void InvalidateCache()
	{
		mCache.Time = std::numeric_limits<float>::infinity();
		mCache.Index = 0;
		mClampCache.Time = std::numeric_limits<float>::infinity();
		mClampCache.Index = 0;
	}

	int GetKeyCount() const { return (int)mKeyframes.size(); }

	void RemoveKeys(iterator begin, iterator end);

	/// Returns the first and last keyframe time
	std::pair<float, float> GetRange() const;

	// How does the curve before the first keyframe
	void SetPreInfinity(WrapMode pre) { mPreInfinity = pre; InvalidateCache(); }
	WrapMode GetPreInfinity() const { return mPreInfinity; }
	// How does the curve behave after the last keyframe
	void SetPostInfinity(WrapMode post) { mPostInfinity = post; InvalidateCache(); }
	WrapMode GetGetPostInfinity() const { return mPostInfinity; }

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
	int FindIndex(const CacheTpl& cache, float curveT) const;

	///@TODO: Cleanup old code to completely get rid of this
	/// Returns the closest keyframe index that is less than time.
	/// Returns -1 if time is outside the range of the curve
	int FindIndex(float time) const;

	void CalculateCacheData(CacheTpl& cache, int lhs, int rhs, float timeOffset) const;


	bool operator==(GfxCurveTpl const& other) const
	{
		return mKeyframes == other.mKeyframes &&
			mPreInfinity == other.mPreInfinity &&
			mPostInfinity == other.mPostInfinity;//&&
			//m_RotationOrder == other.m_RotationOrder;
	}

	inline bool operator!=(GfxCurveTpl const& other) const { return !(*this == other); }


	float WrapTime(float curveT) const;


	void EvaluateWithoutCache(float curveT, T& output) const;
	void FindIndexForSampling(float curveT, int& lhs, int& rhs) const { FindIndexForSampling(mCache, curveT, lhs, rhs); }
private:
	void FindIndexForSampling(const CacheTpl& cache, float curveT, int& lhs, int& rhs) const;


	/// Evaluates the AnimationCurve directly.

protected:
	KeyframeList mKeyframes;
	WrapMode mPreInfinity;
	WrapMode mPostInfinity;
	mutable CacheTpl mCache;
	mutable CacheTpl mClampCache;
};

template<class T>
void EngineNS::GfxCurveTpl<T>::SetupStepped(T* coeff, const CurveKeyTpl<T>& lhs, const CurveKeyTpl<T>& rhs) const
{

}
template<>
inline void EngineNS::GfxCurveTpl<float>::SetupStepped(float* coeff, const CurveKeyTpl<float>& lhs, const CurveKeyTpl<float>& rhs) const
{
	// If either of the tangents in the segment are set to stepped, make the constant value equal the value of the left key
	if (lhs.OutSlope == std::numeric_limits<float>::infinity() || rhs.InSlope == std::numeric_limits<float>::infinity())
	{
		coeff[0] = 0.0F;
		coeff[1] = 0.0F;
		coeff[2] = 0.0F;
		coeff[3] = lhs.Value;
	}
}
template<>
inline void EngineNS::GfxCurveTpl<v3dxVector3>::SetupStepped(v3dxVector3* coeff, const CurveKeyTpl<v3dxVector3>& lhs, const CurveKeyTpl<v3dxVector3>& rhs) const
{
	for (int i = 0; i < 3; i++)
	{
		// If either of the tangents in the segment are set to stepped, make the constant value equal the value of the left key
		if (lhs.OutSlope[i] == std::numeric_limits<float>::infinity() || rhs.InSlope[i] == std::numeric_limits<float>::infinity())
		{
			coeff[0][i] = 0.0F;
			coeff[1][i] = 0.0F;
			coeff[2][i] = 0.0F;
			coeff[3][i] = lhs.Value[i];
		}
	}
}
template<>
inline void EngineNS::GfxCurveTpl<v3dxQuaternion>::SetupStepped(v3dxQuaternion* coeff, const CurveKeyTpl<v3dxQuaternion>& lhs, const CurveKeyTpl<v3dxQuaternion>& rhs) const
{
	// If either of the tangents in the segment are set to stepped, make the constant value equal the value of the left key
	if (lhs.OutSlope[0] == std::numeric_limits<float>::infinity() || rhs.InSlope[0] == std::numeric_limits<float>::infinity() ||
		lhs.OutSlope[1] == std::numeric_limits<float>::infinity() || rhs.InSlope[1] == std::numeric_limits<float>::infinity() ||
		lhs.OutSlope[2] == std::numeric_limits<float>::infinity() || rhs.InSlope[2] == std::numeric_limits<float>::infinity() ||
		lhs.OutSlope[3] == std::numeric_limits<float>::infinity() || rhs.InSlope[3] == std::numeric_limits<float>::infinity())
	{
		for (int i = 0; i < 4; i++)
		{
			coeff[0][i] = 0.0F;
			coeff[1][i] = 0.0F;
			coeff[2][i] = 0.0F;
			coeff[3][i] = lhs.Value[i];
		}
	}
}

template<class T>
void EngineNS::GfxCurveTpl<T>::HandleSteppedCurve(const CurveKeyTpl<T>& lhs, const CurveKeyTpl<T>& rhs, T& value) const
{
	if (lhs.OutSlope == std::numeric_limits<float>::infinity() || rhs.InSlope == std::numeric_limits<float>::infinity())
		value = lhs.Value;
}
template<>
inline void EngineNS::GfxCurveTpl<float>::HandleSteppedCurve(const CurveKeyTpl<float>& lhs, const CurveKeyTpl<float>& rhs, float& value) const
{
	if (lhs.OutSlope == std::numeric_limits<float>::infinity() || rhs.InSlope == std::numeric_limits<float>::infinity())
		value = lhs.Value;
}
template<>
inline void EngineNS::GfxCurveTpl<v3dxVector3>::HandleSteppedCurve(const CurveKeyTpl<v3dxVector3>& lhs, const CurveKeyTpl<v3dxVector3>& rhs, v3dxVector3& value) const
{
	for (int i = 0; i < 3; i++)
	{
		if (lhs.OutSlope[i] == std::numeric_limits<float>::infinity() || rhs.InSlope[i] == std::numeric_limits<float>::infinity())
			value[i] = lhs.Value[i];
	}
}
template<>
inline void EngineNS::GfxCurveTpl<v3dxQuaternion>::HandleSteppedCurve(const CurveKeyTpl<v3dxQuaternion>& lhs, const CurveKeyTpl<v3dxQuaternion>& rhs, v3dxQuaternion& value) const
{
	if (lhs.OutSlope[0] == std::numeric_limits<float>::infinity() || rhs.InSlope[0] == std::numeric_limits<float>::infinity() ||
		lhs.OutSlope[1] == std::numeric_limits<float>::infinity() || rhs.InSlope[1] == std::numeric_limits<float>::infinity() ||
		lhs.OutSlope[2] == std::numeric_limits<float>::infinity() || rhs.InSlope[2] == std::numeric_limits<float>::infinity() ||
		lhs.OutSlope[3] == std::numeric_limits<float>::infinity() || rhs.InSlope[3] == std::numeric_limits<float>::infinity())
	{
		value = lhs.Value;
	}
}



// Returns float remainder for t / length
inline float Repeat(float t, float length)
{
	return t - floor(t / length) * length;
}

// Returns double remainder for t / length
inline double RepeatD(double t, double length)
{
	return t - floor(t / length) * length;
}
inline float Repeat(float t, float begin, float end)
{
	return Repeat(t - begin, end - begin) + begin;
}

inline double RepeatD(double t, double begin, double end)
{
	return RepeatD(t - begin, end - begin) + begin;
}
inline float PingPong(float t, float length)
{
	t = Repeat(t, length * 2.0f);
	t = length - Math::Abs(t - length);
	return t;
}
inline float PingPong(float t, float begin, float end)
{
	return PingPong(t - begin, end - begin) + begin;
}
template<class T>
inline T HermiteInterpolate(float t, const T&  p0, const T& m0, const T& m1, const T& p1)
{
	float t2 = t * t;
	float t3 = t2 * t;

	float a = 2.0F * t3 - 3.0F * t2 + 1.0F;
	float b = t3 - 2.0F * t2 + t;
	float c = t3 - t2;
	float d = -2.0F * t3 + 3.0F * t2;

	return a * p0 + b * m0 + c * m1 + d * p1;
}
struct KeyframeCompareTpl
{
	template<class T>
	bool operator()(CurveKeyTpl<T> const& k, float t) { return k.Time < t; }
	// These are necessary for debug STL (validation of predicates)
	template<class T>
	bool operator()(CurveKeyTpl<T> const& k1, CurveKeyTpl<T> const& k2) { return k1.Time < k2.Time; }
	template<class T>
	bool operator()(float t, CurveKeyTpl<T> const& k) { return !operator()(k, t); }
};


NS_END