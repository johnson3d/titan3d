#include "GfxCurve.h"
#include "..\..\..\BaseDefines\CoreRtti.h"
#include "GfxCurveTpl.h"
#define new VNEW
NS_BEGIN


inline void EvaluateCache(const Cache& cache, float curveT, float& output)
{
	//  DebugAssert (curveT >= cache.Time - kCurveTimeEpsilon && curveT <= cache.TimeEnd + kCurveTimeEpsilon);
	float t = curveT - cache.Time;
	output = (t * (t * (t * cache.Coeff[0] + cache.Coeff[1]) + cache.Coeff[2])) + cache.Coeff[3];
	//DebugAssert(IsFinite(output));
}
void SetupStepped(float* coeff, const CurveKey& lhs, const CurveKey& rhs)
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

void HandleSteppedCurve(const CurveKey& lhs, const CurveKey& rhs, float& value)
{
	if (lhs.OutSlope == std::numeric_limits<float>::infinity() || rhs.InSlope == std::numeric_limits<float>::infinity())
		value = lhs.Value;
}

void HandleSteppedTangent(const CurveKey& lhs, const CurveKey& rhs, float& tangent)
{
	if (lhs.OutSlope == std::numeric_limits<float>::infinity() || rhs.InSlope == std::numeric_limits<float>::infinity())
		tangent = std::numeric_limits<float>::infinity();
}
//////////////////////////////////////////////////////////////////////////////

RTTI_IMPL(EngineNS::GfxCurve, EngineNS::VIUnknown);


GfxCurve::GfxCurve()
{

}

GfxCurve::~GfxCurve()
{

}

float GfxCurve::EvaluateWitCache(float curveT, Cache* cache /*= NULL*/) const
{
	int left, right;
	float output;
	if (GetKeyCount() == 0)
		return 0;
	if (GetKeyCount() == 1)
		return mKeyframes.begin()->Value;

	// use custom cache for multi-threading support?
	if (!cache)
		cache = &mCache;

	if (curveT >= cache->Time && curveT < cache->TimeEnd)
	{
		//      assert (CompareApproximately (EvaluateCache (*cache, curveT), EvaluateWithoutCache (curveT), 0.001F));
		EvaluateCache(*cache, curveT, output);
		return output;
	}
	// @TODO: Optimize IsValid () away if by making the non-valid case always use the *cache codepath
	else if (IsValid())
	{
		float begTime = mKeyframes[0].Time;
		float endTime = mKeyframes.back().Time;
		float wrappedTime;

		if (curveT >= endTime)
		{
			if (mPostInfinity == WrapMode_Clamp)
			{
				cache->Time = endTime;
				cache->TimeEnd = std::numeric_limits<float>::infinity();
				cache->Coeff[0] = cache->Coeff[1] = cache->Coeff[2] = 0.0F;
				cache->Coeff[3] = mKeyframes.back().Value;
			}
			else if (mPostInfinity == WrapMode_Repeat)
			{
				wrappedTime = Math::clamp(Repeat(curveT, begTime, endTime), begTime, endTime);

				FindIndexForSampling(*cache, wrappedTime, left, right);
				CalculateCacheData(*cache, left, right, curveT - wrappedTime);
			}
			///@todo optimize pingpong by making it generate a cache too
			else
			{
				EvaluateWithoutCache(curveT, output);
				return output;
			}
		}
		else if (curveT < begTime)
		{
			if (mPreInfinity == WrapMode_Clamp)
			{
				cache->Time = curveT - 1000.0F;
				cache->TimeEnd = begTime;
				cache->Coeff[0] = cache->Coeff[1] = cache->Coeff[2] = 0.0F;
				cache->Coeff[3] = mKeyframes[0].Value;
			}
			else if (mPreInfinity == WrapMode_Repeat)
			{
				wrappedTime = Repeat(curveT, begTime, endTime);
				FindIndexForSampling(*cache, wrappedTime, left, right);
				CalculateCacheData(*cache, left, right, curveT - wrappedTime);
			}
			///@todo optimize pingpong by making it generate a cache too
			else
			{
				EvaluateWithoutCache(curveT, output);
				return output;
			}
		}
		else
		{
			FindIndexForSampling(*cache, curveT, left, right);
			CalculateCacheData(*cache, left, right, 0.0F);
		}

		//      assert (CompareApproximately (EvaluateCache (*cache, curveT), EvaluateWithoutCache (curveT), 0.001F));
		EvaluateCache(*cache, curveT, output);
		return output;
	}
	else
	{
		return 0.0F;
	}
}

float GfxCurve::EvaluateClampWitCache(float curveT, Cache* cache /*= NULL*/) const
{
	if (GetKeyCount() == 1)
	{
		return mKeyframes[0].Value;
	}

	// use custom cache for multi-threading support?
	if (!cache)
		cache = &mClampCache;

	float output;
	if (curveT >= cache->Time && curveT < cache->TimeEnd)
	{
		//      assert (CompareApproximately (EvaluateCache (*cache, curveT), EvaluateWithoutCache (curveT), 0.001F));
		EvaluateCache(*cache, curveT, output);
		return output;
	}
	else
	{
		//DebugAssert(IsValid());

		float begTime = mKeyframes[0].Time;
		float endTime = mKeyframes.back().Time;

		if (curveT > endTime)
		{
			cache->Time = endTime;
			cache->TimeEnd = std::numeric_limits<float>::infinity();
			cache->Coeff[0] = cache->Coeff[1] = cache->Coeff[2] = 0.0F;
			cache->Coeff[3] = mKeyframes.back().Value;
		}
		else if (curveT < begTime)
		{
			cache->Time = curveT - 1000.0F;
			cache->TimeEnd = begTime;
			cache->Coeff[0] = cache->Coeff[1] = cache->Coeff[2] = 0.0F;
			cache->Coeff[3] = mKeyframes[0].Value;
		}
		else
		{
			int lhs, rhs;
			FindIndexForSampling(*cache, curveT, lhs, rhs);
			CalculateCacheData(*cache, lhs, rhs, 0.0F);
		}

		//      assert (CompareApproximately (EvaluateCache (*cache, curveT), EvaluateWithoutCache (curveT), 0.001F));
		EvaluateCache(*cache, curveT, output);
		return output;
	}
}

void GfxCurve::InvalidateCache()
{
	mCache.Time = std::numeric_limits<float>::infinity();
	mCache.Index = 0;
	mClampCache.Time = std::numeric_limits<float>::infinity();
	mClampCache.Index = 0;
}

int GfxCurve::AddKey(const Keyframe& key)
{
	InvalidateCache();

	iterator i = std::lower_bound(mKeyframes.begin(), mKeyframes.end(), key);

	// is not included in container and value is not a duplicate
	if (i == end() || key < *i)
	{
		iterator ii = mKeyframes.insert(i, key);
		return (int)std::distance(mKeyframes.begin(), ii);
	}
	else
		return -1;
}

void EngineNS::GfxCurve::RemoveKeys(iterator begin, iterator end)
{
	InvalidateCache();
	mKeyframes.erase(begin, end);
}

std::pair<float, float> EngineNS::GfxCurve::GetRange() const
{
	if (!mKeyframes.empty())
		return std::make_pair(mKeyframes[0].Time, mKeyframes.back().Time);
	else
		return std::make_pair(std::numeric_limits<float>::infinity(), -std::numeric_limits<float>::infinity());
}

void GfxCurve::SetPreInfinity(WrapMode pre)
{
	mPreInfinity = (pre);
	InvalidateCache();
}

void GfxCurve::SetPostInfinity(WrapMode post)
{
	mPostInfinity = (post);
	InvalidateCache();
}

WrapMode GfxCurve::GetPreInfinity() const
{
	return (mPreInfinity);
}

WrapMode GfxCurve::GetPostInfinity() const
{
	return (mPostInfinity);
}

void GfxCurve::StripInvalidKeys()
{
	typename KeyframeList::iterator it = mKeyframes.begin();
	typename KeyframeList::iterator end = mKeyframes.end();
	while (it != end)
	{
		Keyframe& key = *it;
		if (!Keyframe::IsValid(key))
		{
			it = mKeyframes.erase(it);
			end = mKeyframes.end();
		}
		else
		{
			it++;
		}
	}
}

void GfxCurve::CalculateCacheData(Cache& cache, int lhsIndex, int rhsIndex, float timeOffset) const
{
	const Keyframe& lhs = mKeyframes[lhsIndex];
	const Keyframe& rhs = mKeyframes[rhsIndex];
	//  DebugAssert (timeOffset >= -0.001F && timeOffset - 0.001F <= rhs.Time - lhs.Time);
	cache.Index = lhsIndex;
	cache.Time = lhs.Time + timeOffset;
	cache.TimeEnd = rhs.Time + timeOffset;
	cache.Index = lhsIndex;

	float dx, length;
	float dy;
	float m1, m2, d1, d2;

	dx = rhs.Time - lhs.Time;
	dx = std::max<float>(dx, 0.0001F);
	dy = rhs.Value - lhs.Value;
	length = 1.0F / (dx * dx);

	m1 = lhs.OutSlope;
	m2 = rhs.InSlope;
	d1 = m1 * dx;
	d2 = m2 * dx;

	cache.Coeff[0] = (d1 + d2 - dy - dy) * length / dx;
	cache.Coeff[1] = (dy + dy + dy - d1 - d1 - d2) * length;
	cache.Coeff[2] = m1;
	cache.Coeff[3] = lhs.Value;
	SetupStepped(cache.Coeff, lhs, rhs);

// 	DebugAssert(IsFinite(cache.Coeff[0]));
// 	DebugAssert(IsFinite(cache.Coeff[1]));
// 	DebugAssert(IsFinite(cache.Coeff[2]));
// 	DebugAssert(IsFinite(cache.Coeff[3]));
}


float EngineNS::GfxCurve::WrapTime(float curveT) const
{
	float begTime = mKeyframes[0].Time;
	float endTime = mKeyframes.back().Time;

	if (curveT < begTime)
	{
		if (mPreInfinity == WrapMode_Clamp)
			curveT = begTime;
		else if (mPreInfinity == WrapMode_PingPong)
			curveT = PingPong(curveT, begTime, endTime);
		else
			curveT = Repeat(curveT, begTime, endTime);
	}
	else if (curveT > endTime)
	{
		if (mPostInfinity == WrapMode_Clamp)
			curveT = endTime;
		else if (mPostInfinity == WrapMode_PingPong)
			curveT = PingPong(curveT, begTime, endTime);
		else
			curveT = Repeat(curveT, begTime, endTime);
	}
	return curveT;
}

// When we look for the next index, how many keyframes do we just loop ahead instead of binary searching?
#define SEARCH_AHEAD 3
///@TODO: Cleanup old code to completely get rid of this
int GfxCurve::FindIndex(const Cache& cache, float curveT) const
{
#if SEARCH_AHEAD >= 0
	int cacheIndex = cache.Index;
	if (cacheIndex != -1)
	{
		// We can not use the cache time or time end since that is in unwrapped time space!
		float time = mKeyframes[cacheIndex].Time;

		if (curveT > time)
		{
			if (cacheIndex + SEARCH_AHEAD < static_cast<int>(mKeyframes.size()))
			{
				for (int i = 0; i < SEARCH_AHEAD; i++)
				{
					if (curveT < mKeyframes[cacheIndex + i + 1].Time)
						return cacheIndex + i;
				}
			}
		}
		else
		{
			if (cacheIndex - SEARCH_AHEAD >= 0)
			{
				for (int i = 0; i < SEARCH_AHEAD; i++)
				{
					if (curveT > mKeyframes[cacheIndex - i - 1].Time)
						return cacheIndex - i - 1;
				}
			}
		}
	}

#endif

	///@ use cache to index into next if not possible use binary search
	const_iterator i = std::lower_bound(mKeyframes.begin(), mKeyframes.end(), curveT, KeyframeCompare());
	int index = (int)std::distance(mKeyframes.begin(), i);
	index--;
	index = std::min<int>((int)mKeyframes.size() - 2, index);
	index = std::max<int>(0, index);

	return index;
}

///@TODO: Cleanup old code to completely get rid of this
int GfxCurve::FindIndex(float curveT) const
{
	std::pair<float, float> range = GetRange();
	if (curveT <= range.first || curveT >= range.second)
		return -1;

	const_iterator i = std::lower_bound(mKeyframes.begin(), mKeyframes.end(), curveT, KeyframeCompare());
	assert(i != mKeyframes.end());
	int index = (int)std::distance(mKeyframes.begin(), i);
	index--;
	index = std::min<int>((int)mKeyframes.size() - 2, index);
	index = std::max<int>(0, index);

	assert(curveT >= mKeyframes[index].Time && curveT <= mKeyframes[index + 1].Time);
	return index;
}

void GfxCurve::FindIndexForSampling(const Cache& cache, float curveT, int& lhs, int& rhs) const
{
	assert(curveT >= GetRange().first && curveT <= GetRange().second);
	int actualSize = (int)mKeyframes.size();
	const Keyframe* frames = &mKeyframes[0];

	// Reference implementation:
	// (index is the last value that is equal to or smaller than curveT)
#if 0
	int foundIndex = 0;
	for (int i = 0; i < actualSize; i++)
	{
		if (frames[i].Time <= curveT)
			foundIndex = i;
	}

	lhs = foundIndex;
	rhs = min<int>(lhs + 1, actualSize - 1);
	assert(curveT >= mKeyframes[lhs].Time && curveT <= mKeyframes[rhs].Time);
	assert(!(frames[rhs].Time == curveT && frames[lhs].Time != curveT));
	return;
#endif


#if SEARCH_AHEAD > 0
	int cacheIndex = cache.Index;
	if (cacheIndex != -1)
	{
		// We can not use the cache time or time end since that is in unwrapped time space!
		float time = mKeyframes[cacheIndex].Time;

		if (curveT > time)
		{
			for (int i = 0; i < SEARCH_AHEAD; i++)
			{
				int index = cacheIndex + i;
				if (index + 1 < actualSize && frames[index + 1].Time > curveT)
				{
					lhs = index;

					rhs = std::min<int>(lhs + 1, actualSize - 1);
					assert(curveT >= frames[lhs].Time && curveT <= frames[rhs].Time);
					assert(!(frames[rhs].Time == curveT && frames[lhs].Time != curveT));
					return;
				}
			}
		}
		else
		{
			for (int i = 0; i < SEARCH_AHEAD; i++)
			{
				int index = cacheIndex - i;
				if (index >= 0 && curveT >= frames[index].Time)
				{
					lhs = index;
					rhs = std::min<int>(lhs + 1, actualSize - 1);
					assert(curveT >= frames[lhs].Time && curveT <= mKeyframes[rhs].Time);
					assert(!(frames[rhs].Time == curveT && frames[lhs].Time != curveT));
					return;
				}
			}
		}
	}

#endif

	// Fall back to using binary search
	// upper bound (first value larger than curveT)
	int __len = actualSize;
	int __half;
	int __middle;
	int __first = 0;
	while (__len > 0)
	{
		__half = __len >> 1;
		__middle = __first + __half;

		if (curveT < frames[__middle].Time)
			__len = __half;
		else
		{
			__first = __middle;
			++__first;
			__len = __len - __half - 1;
		}
	}

	// If not within range, we pick the last element twice
	lhs = __first - 1;
	rhs = std::min<int>(actualSize - 1, __first);

	assert(lhs >= 0 && lhs < actualSize);
	assert(rhs >= 0 && rhs < actualSize);

	assert(curveT >= mKeyframes[lhs].Time && curveT <= mKeyframes[rhs].Time);
	assert(!(frames[rhs].Time == curveT && frames[lhs].Time != curveT));
}


void EngineNS::GfxCurve::EvaluateWithoutCache(float curveT, float& output) const
{
	if (GetKeyCount() == 1)
	{
		output = mKeyframes[0].Value;
		return;
	}

	curveT = WrapTime(curveT);

	int lhsIndex, rhsIndex;
	FindIndexForSampling(mCache, curveT, lhsIndex, rhsIndex);
	const Keyframe& lhs = mKeyframes[lhsIndex];
	const Keyframe& rhs = mKeyframes[rhsIndex];

	float dx = rhs.Time - lhs.Time;
	float m1;
	float m2;
	float t;
	if (dx != 0.0F)
	{
		t = (curveT - lhs.Time) / dx;
		m1 = lhs.OutSlope * dx;
		m2 = rhs.InSlope * dx;
	}
	else
	{
		t = 0.0F;
		m1 = 0.0F;
		m2 = 0.0F;
	}

	output = HermiteInterpolate(t, lhs.Value, m1, m2, rhs.Value);
	HandleSteppedCurve(lhs, rhs, output);
	//DebugAssert(IsFinite(output));
}
NS_END
using namespace EngineNS;

extern "C"
{
	CSharpReturnAPI1(int, EngineNS, GfxCurve, AddKey, CurveKey);
	CSharpReturnAPI0(int, EngineNS, GfxCurve, GetKeyCount);
	CSharpReturnAPI1(float, EngineNS, GfxCurve, Evaluate,float);
}