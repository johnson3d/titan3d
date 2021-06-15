#include "vfxsampcounter.h"
#include "CoreSDK.h"
#include "thread/vfxcritical.h"

#define new VNEW

NS_BEGIN

struct ThreadInstanceManager
{
	std::vector<v3dSampMgr*> AllInstance;
	ThreadInstanceManager()
	{

	}
	~ThreadInstanceManager()
	{
		for (auto i : AllInstance)
		{
			i->Cleanup();
			i->Release();
		}
		AllInstance.clear();
	}
	void RegInstance(v3dSampMgr* p)
	{
		for (size_t i = 0; i < AllInstance.size(); i++)
		{
			if (AllInstance[i] == p)
				return;
		}
		p->AddRef();
		AllInstance.push_back(p);
	}
	void UnregInstance(v3dSampMgr* p)
	{
		for (size_t i = 0; i < AllInstance.size(); i++)
		{
			if (AllInstance[i] == p)
			{
				p->Release();
				AllInstance.erase(AllInstance.begin() + i);
				return;
			}
		}
	}
};

ThreadInstanceManager GInstanceOfSampMgrs;
thread_local AutoRef<v3dSampMgr> v3dSampMgr::ThreadInstance = nullptr;

INT64 SampResult::Begin(v3dSampMgr* mgr, bool bPushParent)
{
	return mgr->Begin(this, bPushParent);
}

void SampResult::End(v3dSampMgr* mgr, INT64 begin)
{
	mgr->End(begin, this);
}

v3dSampMgr::v3dSampMgr()
{
	m_CurSamp = NULL;
	m_AvgCounter = 30;
	m_Freq = _GetPfmFreq();
	UpdateCount = 0;

	GInstanceOfSampMgrs.RegInstance(this);
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
		EngineNS::Safe_Release(i->second);
	}
	m_Samps.clear();

	m_CurSamp = NULL;

	GInstanceOfSampMgrs.UnregInstance(this);
}

SampResult* v3dSampMgr::FindSamp(const char* name)
{
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

SampResult* v3dSampMgr::PureFindSamp(const char* name)
{
	std::map<HashString, SampResult*, StringCompare>::iterator i = m_Samps.find(name);
	if (i == m_Samps.end())
	{
		return NULL;
	}
	return i->second;
}

void v3dSampMgr::UpdateAllThreadInstance()
{
	for (auto i : GInstanceOfSampMgrs.AllInstance)
	{
		i->Update();
	}
}

void v3dSampMgr::Update()
{
	UpdateCount++;
	bool NeedUpdate = false;
	if (UpdateCount >= m_AvgCounter)
	{
		UpdateCount = 0;
		NeedUpdate = true;
	}
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

NS_END