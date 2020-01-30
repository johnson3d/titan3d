#include "GfxCurveTpl.h"
#include "../../../Graphics/GfxPreHead.h"
#define new VNEW
NS_BEGIN
template<class T>
inline void EvaluateCache(const typename GfxCurveTpl<T>::CacheTpl& cache, float curveT, T& output)
{
	//  DebugAssert (curveT >= cache.Time - kCurveTimeEpsilon && curveT <= cache.TimeEnd + kCurveTimeEpsilon);
	float t = curveT - cache.Time;
	output = (t * (t * (t * cache.Coeff[0] + cache.Coeff[1]) + cache.Coeff[2])) + cache.Coeff[3];
	//DebugAssert(IsFinite(output));
}

//////////////////////////////////////////////////////////////////////////////
template<class T>
T GfxCurveTpl<T>::EvaluateWitCache(float curveT, CacheTpl* cache /*= NULL*/) const
{
	int left, right;
	T output;
	if (GetKeyCount() == 0)
		return T(); 
	if (GetKeyCount() == 1)
		return mKeyframes.begin()->Value;

	// use custom cache for multi-threading support?
	if (!cache)
		cache = &mCache;

	if (curveT >= cache->Time && curveT < cache->TimeEnd)
	{
		//      assert (CompareApproximately (EvaluateCache (*cache, curveT), EvaluateWithoutCache (curveT), 0.001F));
		EvaluateCache<T>(*cache, curveT, output);
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
				cache->Coeff[0] = cache->Coeff[1] = cache->Coeff[2] = Zero<T>();
				cache->Coeff[3] = mKeyframes.back().Value;
			}
			else if (mPostInfinity == WrapMode_Repeat || mPostInfinity == Default)
			{
				wrappedTime = Math::clamp(Repeat(curveT, begTime, endTime), begTime, endTime);
				FindIndexForSampling(*cache, wrappedTime, left, right);
				CalculateCacheData(*cache, left, right, curveT - wrappedTime);
			}
			else if (mPostInfinity == WrapMode_PingPong)
			{
				wrappedTime = Math::clamp(PingPong(curveT, begTime, endTime), begTime, endTime);
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
				cache->Coeff[0] = cache->Coeff[1] = cache->Coeff[2] = Zero<T>();
				cache->Coeff[3] = mKeyframes[0].Value;
			}
			else if (mPreInfinity == WrapMode_Repeat || mPreInfinity == Default)
			{
				wrappedTime = Repeat(curveT, begTime, endTime);
				FindIndexForSampling(*cache, wrappedTime, left, right);
				CalculateCacheData(*cache, left, right, curveT - wrappedTime);
			}
			else if (mPreInfinity == WrapMode_PingPong)
			{
				wrappedTime = Math::clamp(PingPong(curveT, begTime, endTime), begTime, endTime);
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
		EvaluateCache<T>(*cache, curveT, output);
		return output;
	}
	else
	{
		return Zero<T>();
	}
}
template<class T>
T GfxCurveTpl<T>::EvaluateClampWitCache(float curveT, CacheTpl* cache /*= NULL*/) const
{
	if (GetKeyCount() == 1)
	{
		return mKeyframes[0].Value;
	}

	// use custom cache for multi-threading support?
	if (!cache)
		cache = &mClampCache;

	T output;
	if (curveT >= cache->Time && curveT < cache->TimeEnd)
	{
		//      assert (CompareApproximately (EvaluateCache (*cache, curveT), EvaluateWithoutCache (curveT), 0.001F));
		EvaluateCache<T>(*cache, curveT, output);
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
			cache->Coeff[0] = cache->Coeff[1] = cache->Coeff[2] = Zero<T>();
			cache->Coeff[3] = mKeyframes.back().Value;
		}
		else if (curveT < begTime)
		{
			cache->Time = curveT - 1000.0F;
			cache->TimeEnd = begTime;
			cache->Coeff[0] = cache->Coeff[1] = cache->Coeff[2] = Zero<T>();
			cache->Coeff[3] = mKeyframes[0].Value;
		}
		else
		{
			int lhs, rhs;
			FindIndexForSampling(*cache, curveT, lhs, rhs);
			CalculateCacheData(*cache, lhs, rhs, 0.0F);
		}

		//      assert (CompareApproximately (EvaluateCache (*cache, curveT), EvaluateWithoutCache (curveT), 0.001F));
		EvaluateCache<T>(*cache, curveT, output);
		return output;
	}
}
//template<class T>
//void GfxCurveTpl<T>::InvalidateCache()
//{
//	mCache.Time = std::numeric_limits<float>::infinity();
//	mCache.Index = 0;
//	mClampCache.Time = std::numeric_limits<float>::infinity();
//	mClampCache.Index = 0;
//}
template<class T>
int GfxCurveTpl<T>::AddKey(const Keyframe& key)
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
template<class T>
void EngineNS::GfxCurveTpl<T>::RemoveKeys(iterator begin, iterator end)
{
	InvalidateCache();
	mKeyframes.erase(begin, end);
}
template<class T>
std::pair<float, float> EngineNS::GfxCurveTpl<T>::GetRange() const
{
	if (!mKeyframes.empty())
		return std::make_pair(mKeyframes[0].Time, mKeyframes.back().Time);
	else
		return std::make_pair(std::numeric_limits<float>::infinity(), -std::numeric_limits<float>::infinity());
}
template<class T>
void GfxCurveTpl<T>::StripInvalidKeys()
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
template<class T>
void GfxCurveTpl<T>::CalculateCacheData(CacheTpl& cache, int lhsIndex, int rhsIndex, float timeOffset) const
{
	const Keyframe& lhs = mKeyframes[lhsIndex];
	const Keyframe& rhs = mKeyframes[rhsIndex];
	//  DebugAssert (timeOffset >= -0.001F && timeOffset - 0.001F <= rhs.Time - lhs.Time);
	cache.Index = lhsIndex;
	cache.Time = lhs.Time + timeOffset;
	cache.TimeEnd = rhs.Time + timeOffset;
	cache.Index = lhsIndex;

	float dx, length;
	T dy;
	T m1, m2, d1, d2;

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

template<class T>
float EngineNS::GfxCurveTpl<T>::WrapTime(float curveT) const
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
template<class T>
int GfxCurveTpl<T>::FindIndex(const CacheTpl& cache, float curveT) const
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
	const_iterator i = std::lower_bound(mKeyframes.begin(), mKeyframes.end(), curveT, KeyframeCompareTpl());
	int index = (int)std::distance(mKeyframes.begin(), i);
	index--;
	index = std::min<int>((int)mKeyframes.size() - 2, index);
	index = std::max<int>(0, index);

	return index;
}

///@TODO: Cleanup old code to completely get rid of this
template<class T>
int GfxCurveTpl<T>::FindIndex(float curveT) const
{
	std::pair<float, float> range = GetRange();
	if (curveT <= range.first || curveT >= range.second)
		return -1;

	const_iterator i = std::lower_bound(mKeyframes.begin(), mKeyframes.end(), curveT, KeyframeCompareTpl());
	assert(i != mKeyframes.end());
	int index = (int)std::distance(mKeyframes.begin(), i);
	index--;
	index = std::min<int>((int)mKeyframes.size() - 2, index);
	index = std::max<int>(0, index);

	assert(curveT >= mKeyframes[index].Time && curveT <= mKeyframes[index + 1].Time);
	return index;
}
template<class T>
void GfxCurveTpl<T>::FindIndexForSampling(const CacheTpl& cache, float curveT, int& lhs, int& rhs) const
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

template<class T>
void EngineNS::GfxCurveTpl<T>::EvaluateWithoutCache(float curveT, T& output) const
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
	if (curveT == lhs.Time)
		output = lhs.Value;
	if (curveT == rhs.Time)
		output = rhs.Value;

	float dx = rhs.Time - lhs.Time;
	T m1;
	T m2;
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
		m1 = Zero<T>();
		m2 = Zero<T>();
	}

	output = HermiteInterpolate(t, lhs.Value, m1, m2, rhs.Value);
	HandleSteppedCurve(lhs, rhs, output);
	//DebugAssert(IsFinite(output));
}
NS_END