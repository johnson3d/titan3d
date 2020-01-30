#pragma once

#include "../BaseHead.h"
#include "debug/vfxdebug.h"
//#include "debug/vfxmemory.h"
#include "generic/vfx_temp_base.h"
#include "generic/vfxtemplate.h"
#include "string/vfxstring.h"
#include "string/vfxHashString.h"


#pragma pack(push,4)
class v3dSampMgr;

struct SampResult
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
	}

	VDef_ReadWrite(vBOOL, Enable, m);
	VDef_ReadOnly(AvgTime);
	VDef_ReadOnly(AvgHit);

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
	struct MyAllocator
	{
		static void * Malloc(size_t size)
		{
			return _vfxMemoryNew(size, __FILE__, __LINE__);
		}
		static void Free(void* p)
		{
			_vfxMemoryDelete(p, NULL, 0);
		}
	};
	typedef VMem::map<SampResult*, ParentSamp, MyAllocator> ParentMap;
	ParentMap	m_Parents;

	void PushParent(SampResult* p)
	{
		auto it = m_Parents.find(p);
		if(it!=m_Parents.end())
		{
			it->second.HitCount++;
		}
		ParentSamp temp;
		temp.Samp = p;
		temp.HitCount = 1;
		m_Parents.insert(std::make_pair(p,temp));
	}

	void ResetParents()
	{
		for(auto it = m_Parents.begin();it!=m_Parents.end();++it)
		{
			it->second.Reset();
		}
	}

	INT64 Begin(v3dSampMgr* mgr);
	void End(v3dSampMgr* mgr, INT64 begin);

	INT64			mAvgTime;
	int				mAvgHit;

	INT64			mAvgTimePerHit;

	///每隔多少帧统计平均值
	int				mAvgCounter;
	///int 当前经过了多少帧
	int				mCurFrame;
	///本帧进入统计的次数
	int				mHitInCurFrame;
	///平均统计周期内进入次数
	int				mHitInCounter;
	int				mMaxHitInCounter;
	///本帧消耗时间
	INT64			mTimeInCurFrame;
	///平均统计周期内消耗时间
	INT64			mTimeInCounter;
	INT64			mMaxTimeInCounter;
};

class v3dSampMgr
{
	struct MyAllocator
	{
		static void * Malloc(size_t size)
		{
			return _vfxMemoryNew(size, __FILE__, __LINE__);
		}
		static void Free(void* p)
		{
			_vfxMemoryDelete(p, NULL, 0);
		}
	};
public:
	typedef vfxHashString	HashString;
	typedef _HashStringCompare	StringCompare;

	typedef std::map<HashString, SampResult*, StringCompare, VMem::malloc_allocator<std::pair<const HashString, SampResult*>, MyAllocator> > SampsMap;
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
        struct timeval tv={0,0};
        gettimeofday(&tv,NULL);
        INT64 ret = (INT64)(tv.tv_sec) * 1000 * 1000000;
        ret  += tv.tv_usec * 1000;
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
	static v3dSampMgr Instance;

	inline INT64 Begin(SampResult* pSampResult, bool bPushParent)
	{
		if (pSampResult->mEnable)
		{
			if (bPushParent)
			{
				m_CurSamp->mParent = pSampResult;
				m_CurSamp->PushParent(pSampResult);
				m_CurSamp = pSampResult;
			}

			INT64 qpc = GetHighCounter();
			return qpc;
		}
		return 0;
	}

	inline void End(INT64 begin, SampResult* pSampResult, bool bPushParent)
	{
		SampResult* pSamp;
		pSamp = pSampResult;
		if (pSamp->mEnable == FALSE)
		{
			m_CurSamp = pSamp->mParent;
			return;
		}

		INT64 end = GetHighCounter();

		INT64 elapse = end - begin;
		INT64 time = elapse;//*1000000/m_Freq;

		m_CurSamp = pSamp->mParent;

		pSamp->mHitInCurFrame++;
		pSamp->mHitInCounter++;
		if (pSamp->mHitInCurFrame>pSamp->mMaxHitInCounter)
			pSamp->mMaxHitInCounter = pSamp->mHitInCurFrame;

		pSamp->mTimeInCurFrame += time;
		pSamp->mTimeInCounter += time;
		if (pSamp->mTimeInCurFrame>pSamp->mMaxTimeInCounter)
			pSamp->mMaxTimeInCounter = pSamp->mTimeInCurFrame;
	}

	 void Update();

	void ClearSamps()
	{
		m_Samps.clear();
	}		

	///找不到就新建Samp
	 SampResult* FindSamp( LPCSTR name );
	 SampResult* PureFindSamp(LPCSTR name);
};

class v3dAutoSampEx
{
	INT64			m_Begin;
	SampResult*		m_SampResult;
public:
	v3dAutoSampEx( SampResult* pSampResult )
	{
		m_SampResult = pSampResult;
		m_Begin = v3dSampMgr::Instance.Begin(m_SampResult, false);
	}
	~v3dAutoSampEx()
	{
		v3dSampMgr::Instance.End( m_Begin , m_SampResult, false );
	}
};

#define VFXPERF

#ifdef VFXPERF
	//#define SAMP_BEGIN(name) v3dSampMgr::GetInstance()->Begin(name);
	//#define SAMP_END(begin,name) v3dSampMgr::GetInstance()->End(begin,name,v3dSampMgr::GetInstance()->m_AvgCounter);

	#define AUTO_SAMP_INNER(name,line) static SampResult* SR_##line = v3dSampMgr::Instance.FindSamp(name);\
				v3dAutoSampEx AS##line(SR_##line);

	#define AUTO_SAMP(name) AUTO_SAMP_INNER(name,__LINE__)
#else
	#define SAMP_BEGIN 
	#define SAMP_END(begin,name,avgCounter) 
	//#define AUTO_SAMP(name,counter)
	#define AUTO_SAMP(name)
#endif

#pragma pack(pop)