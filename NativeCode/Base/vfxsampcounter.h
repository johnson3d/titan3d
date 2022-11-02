#pragma once

#include "IUnknown.h"
#include "debug/vfxdebug.h"
//#include "debug/vfxmemory.h"
//#include "generic/vfx_temp_base.h"
//#include "generic/vfxtemplate.h"
#include "string/vfxstring.h"
#include "string/vfxHashString.h"


#pragma pack(push,4)

NS_BEGIN

class v3dSampMgr;

struct TR_CLASS()
	SampResult : public VIUnknown
{
	friend v3dSampMgr;

	SampResult()
	{
		mEnable = TRUE;
		mParent = 0;
		mAvgTime = 0;
		mAvgHit = 0;
		mAvgCounter = 0;
		mCurFrame = 0;
		mHitInCurFrame = 0;
		mHitInCounter = 0;
		mMaxHitInCounter = 0;
		mTimeInCurFrame = 0;
		mTimeInCounter = 0;
		mMaxTimeInCounter = 0;
		mMaxTimeInLife = 0;
	}

	const char* GetName() const {
		return mName.c_str();
	}
	bool			mEnable;

	VStringA		mName;
	VStringA		mDescribe;

	SampResult*		mParent;

	struct ParentSamp
	{
		SampResult*		Samp;
		int				HitCount;
		void Reset()
		{
			HitCount = 0;
		}
	};
	typedef std::map<SampResult*, ParentSamp> ParentMap;
	ParentMap	m_Parents;

	void PushParent(SampResult* p)
	{
		auto it = m_Parents.find(p);
		if (it != m_Parents.end())
		{
			it->second.HitCount++;
		}
		ParentSamp temp;
		temp.Samp = p;
		temp.HitCount = 1;
		m_Parents.insert(std::make_pair(p, temp));
	}

	void ResetParents()
	{
		for (auto it = m_Parents.begin(); it != m_Parents.end(); ++it)
		{
			it->second.Reset();
		}
	}

	INT64 Begin(v3dSampMgr* mgr, bool bPushParent = true);
	void End(v3dSampMgr* mgr, INT64 begin);

	INT64			mAvgTime;
	int				mAvgHit;

	INT64			mAvgTimePerHit;

	int				mAvgCounter;
	int				mCurFrame;
	int				mHitInCurFrame;
	int				mHitInCounter;
	int				mMaxHitInCounter;
	INT64			mTimeInCurFrame;
	INT64			mTimeInCounter;
	INT64			mMaxTimeInCounter;

	INT64			mMaxTimeInLife;
};

class TR_CLASS()
	v3dSampMgr : public VIUnknown
{
	int UpdateCount;
public:
	typedef vfxHashString	HashString;
	typedef _HashStringCompare	StringCompare;

	typedef std::map<HashString, SampResult*, StringCompare> SampsMap;
	SampsMap				m_Samps;

	SampResult*				m_CurSamp;
	int						m_AvgCounter;
	INT64					m_Freq;

	inline INT64 GetHighCounter()
	{
#if defined PLATFORM_WIN
		INT64 qpc = 0;
		QueryPerformanceCounter((LARGE_INTEGER*)&qpc);
		return qpc;
#elif defined(IOS)
		struct timeval tv = { 0,0 };
		gettimeofday(&tv, NULL);
		INT64 ret = (INT64)(tv.tv_sec) * 1000 * 1000000;
		ret += tv.tv_usec * 1000;
		return ret;
#else
		struct timespec time1 = { 0, 0 };
		clock_gettime(CLOCK_MONOTONIC, &time1);
		INT64 ret = (INT64)(time1.tv_sec) * 1000000000;
		ret += time1.tv_nsec;
		return ret;
#endif
	}

	static INT64 _GetPfmFreq()
	{
		static INT64 Freq = 0;
		if (Freq == 0)
		{
#if defined(PLATFORM_WIN)
			QueryPerformanceFrequency((LARGE_INTEGER*)&Freq);
#else
			Freq = 1000000 * 1000;
#endif
		}
		return Freq;
	}
public:
	v3dSampMgr();
	~v3dSampMgr();
	void Cleanup();
	static thread_local AutoRef<v3dSampMgr> ThreadInstance;
	
	static v3dSampMgr* GetThreadInstance() {
		if (ThreadInstance == nullptr)
		{
			ThreadInstance = MakeWeakRef(new v3dSampMgr());
		}
		return ThreadInstance;
	}

	inline INT64 Begin(SampResult* pSampResult, bool bPushParent)
	{
		if (pSampResult->mEnable)
		{
			if (bPushParent)
			{
				/*if (m_CurSamp != nullptr)
				{
					m_CurSamp->mParent = pSampResult;
					m_CurSamp->PushParent(pSampResult);
				}*/
				pSampResult->mParent = m_CurSamp;
				m_CurSamp = pSampResult;
			}

			INT64 qpc = GetHighCounter();
			return qpc;
		}
		return 0;
	}

	inline void End(INT64 begin, SampResult* pSamp)
	{
		if (pSamp->mEnable == FALSE)
		{
			m_CurSamp = pSamp->mParent;
			return;
		}

		INT64 end = GetHighCounter();

		INT64 elapse = end - begin;
		//INT64 time = elapse;//*1000000/m_Freq;

		m_CurSamp = pSamp->mParent;

		pSamp->mHitInCurFrame++;
		pSamp->mHitInCounter++;
		if (pSamp->mHitInCurFrame>pSamp->mMaxHitInCounter)
			pSamp->mMaxHitInCounter = pSamp->mHitInCurFrame;

		pSamp->mTimeInCurFrame += elapse;
		pSamp->mTimeInCounter += elapse;
		if (pSamp->mTimeInCurFrame>pSamp->mMaxTimeInCounter)
			pSamp->mMaxTimeInCounter = pSamp->mTimeInCurFrame;

		if (pSamp->mTimeInCurFrame > pSamp->mMaxTimeInLife)
			pSamp->mMaxTimeInLife = pSamp->mTimeInCurFrame;
	}

	TR_FUNCTION(SV_NoBind)
	void Update();
	static void UpdateAllThreadInstance();

	void ClearSamps()
	{
		Cleanup();
	}		

	SampResult* FindSamp( const char* name );
	SampResult* PureFindSamp(const char* name);
	UINT GetSampNum() const{
		return (UINT)m_Samps.size();
	}
	void GetAllSamps(SampResult** ppSamps, UINT count) const
	{
		UINT index = 0;
		for (auto& i : m_Samps)
		{
			ppSamps[index] = i.second;
			++index;
			if (index >= count)
				return;
		}
	}
};

class v3dAutoSampEx
{
	INT64			m_Begin;
	SampResult*		m_SampResult;
public:
	v3dAutoSampEx(SampResult* pSampResult, bool bPushParent = true)
	{
		m_SampResult = pSampResult;
		m_Begin = v3dSampMgr::GetThreadInstance()->Begin(m_SampResult, bPushParent);
	}
	~v3dAutoSampEx()
	{
		v3dSampMgr::GetThreadInstance()->End( m_Begin , m_SampResult);
	}
};

#define VFXPERF

#ifdef VFXPERF
	//#define SAMP_BEGIN(name) v3dSampMgr::GetInstance()->Begin(name);
	//#define SAMP_END(begin,name) v3dSampMgr::GetInstance()->End(begin,name,v3dSampMgr::GetInstance()->m_AvgCounter);

	#define AUTO_SAMP_INNER(name,line) static thread_local EngineNS::SampResult* SR_##line = EngineNS::v3dSampMgr::GetThreadInstance()->FindSamp(name);\
				EngineNS::v3dAutoSampEx AS##line(SR_##line);

	#define AUTO_SAMP(name) AUTO_SAMP_INNER(name,__LINE__)
#else
	//#define SAMP_BEGIN 
	//#define SAMP_END(begin,name,avgCounter) 
	//#define AUTO_SAMP(name,counter)
	#define AUTO_SAMP(name)
#endif

NS_END

#pragma pack(pop)