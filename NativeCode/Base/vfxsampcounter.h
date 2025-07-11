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

struct SampResult;

struct TR_CLASS()
	SampResult : public IWeakRefObject
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
	VStringA		mDebugSourceFile;
	int				mDebugSourceLine = 0;

	SampResult*		mParent;

	struct FParentSamp
	{
		SampResult*		Samp;
		int				HitCount;
		float			Ratio;
		FParentSamp()
		{
			Samp = nullptr;
			HitCount = 0;
			Ratio = 0;
		}
	};
	std::vector<FParentSamp>	m_Parents;

	void PushParent(SampResult* p)
	{
		for (auto& i : m_Parents)
		{
			if (i.Samp == p)
			{
				i.HitCount++;
				return;
			}
		}
		FParentSamp tmp;
		tmp.Samp = p;
		tmp.HitCount++;
		m_Parents.push_back(tmp);
	}

	void ResetParents()
	{
		int total = 0;
		for (auto& i : m_Parents)
		{
			total += i.HitCount;
		}

		for (auto& i : m_Parents)
		{
			i.Ratio = (float)i.HitCount / total;
			i.HitCount = 0;
		}
	}

	int GetNunOfCaller()
	{
		return (int)m_Parents.size();
	}
	SampResult* GetCaller(int index)
	{
		return m_Parents[index].Samp;
	}
	float GetCallerRatio(int index)
	{
		return m_Parents[index].Ratio;
	}
	const char* GetDebugSourceFile() {
		return mDebugSourceFile.c_str();
	}
	int GetDebugSourceLine() {
		return mDebugSourceLine;
	}

	INT64 Begin(v3dSampMgr* mgr, const char* file, int line);
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
	v3dSampMgr : public IWeakRefObject
{
	int UpdateCount;
public:
	typedef vfxHashString	HashString;
	typedef _HashStringCompare	StringCompare;

	typedef std::map<HashString, AutoRef<SampResult>, StringCompare> SampsMap;
	SampsMap				m_Samps;

	AutoRef<SampResult>		m_CurSamp;
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
	SampResult* GetCurrentSamp();
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
	static void FinalCleanup();

	inline INT64 Begin(SampResult* pSampResult)
	{
		ASSERT(m_CurSamp != pSampResult);
		pSampResult->mParent = m_CurSamp;
		m_CurSamp = pSampResult;
		if (pSampResult->mEnable)
		{
			if (m_CurSamp != nullptr)
			{
				pSampResult->PushParent(pSampResult->mParent);
			}

			INT64 qpc = GetHighCounter();
			return qpc;
		}
		return 0;
	}

	inline void End(INT64 begin, SampResult* pSamp)
	{
		m_CurSamp = pSamp->mParent;
		if (pSamp->mEnable == FALSE)
		{
			return;
		}

		INT64 end = GetHighCounter();

		INT64 elapse = end - begin;
		//INT64 time = elapse;//*1000000/m_Freq;

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
	v3dAutoSampEx(SampResult* pSampResult)
	{
		m_SampResult = pSampResult;
		m_Begin = v3dSampMgr::GetThreadInstance()->Begin(m_SampResult);
	}
	~v3dAutoSampEx()
	{
		v3dSampMgr::GetThreadInstance()->End(m_Begin, m_SampResult);
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