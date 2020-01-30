#include "vfxSampCounter.h"
#include "thread/vfxcritical.h"
#include "../CSharpAPI.h"

#define new VNEW

INT64 SampResult::Begin(v3dSampMgr* mgr)
{
	return mgr->Begin(this, false);
}

void SampResult::End(v3dSampMgr* mgr, INT64 begin)
{
	mgr->End(begin, this, false);
}

v3dSampMgr v3dSampMgr::Instance;

v3dSampMgr::v3dSampMgr()
{
	m_CurSamp = NULL;
	m_AvgCounter = 30;
	m_Freq = _GetPfmFreq();
}

//#define PushSampStack

v3dSampMgr::~v3dSampMgr()
{
	Cleanup();
}

void v3dSampMgr::Cleanup()
{
	for (auto i = m_Samps.begin(); i != m_Samps.end(); ++i)
	{
		EngineNS::Safe_Delete(i->second);
	}
	m_Samps.clear();

	m_CurSamp = NULL;
}

VSLLock gSampLocker;

SampResult* v3dSampMgr::FindSamp(LPCSTR name)
{
	VAutoVSLLock<VSLLock> al(gSampLocker);
	auto i = m_Samps.find(name);
	if (i == m_Samps.end())
	{
		SampResult* sr = new SampResult();
		sr->mEnable = FALSE;
		sr->mName = name;
		m_Samps.insert(std::make_pair(name, sr));
		return sr;
	}
	return i->second;
}

SampResult* v3dSampMgr::PureFindSamp(LPCSTR name)
{
	VAutoVSLLock<VSLLock> al(gSampLocker);
	std::map<HashString, SampResult*, StringCompare>::iterator i = m_Samps.find(name);
	if (i == m_Samps.end())
	{
		return NULL;
	}
	return i->second;
}

void v3dSampMgr::Update()
{
	static int UpdateCount = 0;
	UpdateCount++;
	bool NeedUpdate = false;
	if (UpdateCount >= m_AvgCounter)
	{
		UpdateCount = 0;
		NeedUpdate = true;
	}
	VAutoVSLLock<VSLLock> al(gSampLocker);
	for (std::map<HashString, SampResult*, StringCompare>::iterator i = m_Samps.begin(); i != m_Samps.end(); ++i)
	{
		SampResult* pSamp = i->second;

		pSamp->mHitInCurFrame = 0;
		pSamp->mTimeInCurFrame = 0;

		//pSamp->m_CurFrame++;
		//int avgCounter = pSamp->m_AvgCounter <= 0 ? m_AvgCounter : pSamp->m_AvgCounter;
		//if (pSamp->m_CurFrame >= avgCounter)
		if(NeedUpdate)
		{
			pSamp->mAvgTime = pSamp->mTimeInCounter / m_AvgCounter;
			pSamp->mAvgHit = pSamp->mHitInCounter / m_AvgCounter;

			pSamp->mAvgTime = pSamp->mAvgTime * 1000000 / m_Freq;

			if (pSamp->mHitInCounter > 0)
			{
				pSamp->mAvgTimePerHit = pSamp->mTimeInCounter / pSamp->mHitInCounter;
				pSamp->mAvgTimePerHit = pSamp->mAvgTimePerHit * 1000000 / m_Freq;
			}
			else
			{
				pSamp->mAvgTimePerHit = 0;
			}
			pSamp->mHitInCounter = 0;
			pSamp->mTimeInCounter = 0;
			pSamp->mMaxHitInCounter = 0;
			pSamp->mMaxTimeInCounter = 0;

			//pSamp->m_AvgTimePerHit = 0;

			pSamp->mCurFrame = 0;

			pSamp->ResetParents();
		}
	}
}

using namespace EngineNS;

extern "C"
{
	VFX_API v3dSampMgr* SDK_v3dSampMgr_GetInstance()
	{
		return &v3dSampMgr::Instance;
	}
	CSharpReturnAPI1(SampResult*, , v3dSampMgr, FindSamp, LPCSTR);
	CSharpReturnAPI1(SampResult*, , v3dSampMgr, PureFindSamp, LPCSTR);
	CSharpAPI0(, v3dSampMgr, Update);

	CSharpReturnAPI0(const char*, , SampResult, GetName);

	CSharpReturnAPI0(vBOOL, , SampResult, GetEnable);
	CSharpAPI1( , SampResult, SetEnable, bool);

	CSharpReturnAPI0(INT64, , SampResult, GetAvgTime);
	CSharpReturnAPI0(int, , SampResult, GetAvgHit);

	CSharpReturnAPI1(INT64, , SampResult, Begin, v3dSampMgr*);
	CSharpAPI2( , SampResult, End, v3dSampMgr*, INT64);
};