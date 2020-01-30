// vfxcritical.cpp
//
// Author : johnson3d
// More author :
// Create time :	 2004-5-12 10:22
// Modify time :	
//-----------------------------------------------------------------------------
//#include "../precompile.h"
#include "vfxcritical.h"
#include "../vfxSampCounter.h"
#include "../generic/vfx_temp_base.h"

#define new VNEW

typedef void(WINAPI *FMemoryFinalizer)();
extern FMemoryFinalizer OnMemoryFinal;
FMemoryFinalizer SavedOnMemoryFinal;

void VCriticalInfoStack::PushLock(LPCSTR file, int line)
{
	mStacks.push_back(VCriticalInfo(file, line));
}

void VCriticalInfoStack::PopLock()
{
	mStacks.pop_back();
}

VCriticalThread::VCriticalThread()
{
	
}

VCriticalThread::~VCriticalThread()
{
	for (auto i = mLockers.begin(); i != mLockers.end(); ++i)
	{
		EngineNS::Safe_Delete(i->second);
	}
	mLockers.clear();
}

void VCriticalThread::PushLock(VCritical* locker, LPCSTR file, int line)
{
	auto iter = mLockers.find(locker);
	if (iter != mLockers.end())
	{
		iter->second->PushLock(file, line);
	}
	else
	{
		auto info = new VCriticalInfoStack();
		mLockers.insert(std::make_pair(locker, info));
		info->PushLock(file, line);
	}
}

void VCriticalThread::PopLock(VCritical* locker)
{
	auto iter = mLockers.find(locker);
	if (iter != mLockers.end())
	{
		iter->second->PopLock();
	}
	else
	{
		VFX_LTRACE(ELTT_Resource, "Error!!!! VCriticalThread::PopLock find failed\r\n");
	}
}

vBOOL VCriticalInfoManager::IsDebugMTLocker = FALSE;

VCriticalInfoManager::VCriticalInfoManager()
{
	
}

VCriticalInfoManager::~VCriticalInfoManager()
{
	for (auto i = mThreads.begin(); i != mThreads.end(); ++i)
	{
		EngineNS::Safe_Delete(i->second);
	}
	mThreads.clear();
}

void VCriticalInfoManager::PushLock(VCritical* locker, LPCSTR file, int line)
{
	AUTO_SAMP("CriticalDebugger");
	if (IsDebugMTLocker == FALSE)
		return;

	mManagerLocker.Lock();
	auto threadid = GetCurrentThreadId();

	auto iter = mThreads.find(threadid);
	if (iter != mThreads.end())
	{
		iter->second->PushLock(locker, file, line);
	}
	else
	{
		auto mgr = new VCriticalThread();
		mThreads.insert(std::make_pair(threadid, mgr));
		mgr->PushLock(locker, file, line);
	}
	mManagerLocker.Unlock();
}

void VCriticalInfoManager::PopLock(VCritical* locker)
{
	AUTO_SAMP("CriticalDebugger");
	if (IsDebugMTLocker == FALSE)
		return;

	mManagerLocker.Lock();
	auto threadid = GetCurrentThreadId();
	auto iter = mThreads.find(threadid);
	if (iter != mThreads.end())
	{
		iter->second->PopLock(locker);
	}
	else
	{
		VFX_LTRACE(ELTT_SystemCore, "Error!!!! VCriticalInfoManager::PopLock find failed\r\n");
	}
	mManagerLocker.Unlock();
}
VString VCriticalInfoManager::PrintLockInfo()
{
	VString str = "Locker Dump Begin:\r\n";
	mManagerLocker.Lock();
	for(auto iter = mThreads.begin(); iter != mThreads.end(); ++iter)
	{
		auto thread = iter->second;
		INT64 threadId = iter->first;
		str += VStringA_FormatV("Thread:%llu:\r\n", threadId);
		std::map<VCritical*, VCriticalInfoStack*>& Lockers = thread->mLockers;
		for(auto j = Lockers.begin(); j != Lockers.end(); j++)
		{
			auto stacks = j->second->mStacks;
			if (stacks.size() > 0)
			{
				str += VStringA_FormatV("  Critical:%d\r\n", (INT_PTR)j->first);				
				for (auto k = stacks.begin(); k != stacks.end(); ++k)
				{
					str += VStringA_FormatV("    %s(%d)\r\n", (*k).mFile, (*k).mLine);
				}
			}
		}
	}
	str += "Locker Dump End\r\n";
	mManagerLocker.Unlock();
	return str;
}

VCritical::VCritical()
{
	//pthread_mutex_init(&m_Critical, NULL);
	//PTHREAD_PROCESS_PRIVATE
	//m_Critical = PTHREAD_MUTEX_INITIALIZER;
	//pthread_mutex_init(&m_Critical, &attr);
	pthread_mutexattr_t attr;
	pthread_mutexattr_init(&attr);
#if defined(ANDROID) || defined(PLATFORM_WIN)
	pthread_mutexattr_settype(&attr, PTHREAD_MUTEX_RECURSIVE_NP);
#elif defined(IOS)
	pthread_mutexattr_settype(&attr, PTHREAD_MUTEX_RECURSIVE);
#endif
	//pthread_mutexattr_setpshared(&attr, PTHREAD_PROCESS_PRIVATE);
	pthread_mutex_init(&m_Critical, &attr);
	pthread_mutexattr_destroy(&attr);
}
VCritical::~VCritical()
{
	pthread_mutex_destroy(&m_Critical);
}

VPagedCritical::VPagedCritical()
{
	mLockCount = 0;
}

VPagedCritical::~VPagedCritical()
{
#if defined(PLATFORM_WIN)
	//ASSERT(mLocker.m_Critical.LockCount == -1);
#endif
}

VCriticalInfoManager VCriticalInfoManager::Instance;
vfxMTLockerManager vfxMTLockerManager::Instance;

void WINAPI FMemoryFinalizer_MTLockerManager()
{
	vfxMTLockerManager::Instance.Cleanup();
	if (SavedOnMemoryFinal != nullptr)
		SavedOnMemoryFinal();
}

vfxMTLockerManager::vfxMTLockerManager()
{
	mAllocPoint = NULL;
	SavedOnMemoryFinal = OnMemoryFinal;
	OnMemoryFinal = FMemoryFinalizer_MTLockerManager;
}

vfxMTLockerManager::~vfxMTLockerManager()
{
	Cleanup();
}

void vfxMTLockerManager::Cleanup()
{
	for (size_t i = 0; i < mPools.size(); i++)
	{
		EngineNS::Safe_DeleteArray(mPools[i]);
	}
	mPools.clear();
	mAllocPoint = NULL;
}

void vfxMTLockerManager::NewPool()
{
	if (mAllocPoint != NULL)
		return;

#define POOL_SIZE	256

	VPagedCritical* pool = new VPagedCritical[POOL_SIZE];
	pool[POOL_SIZE - 1].NextLink = NULL;
	for (int i = 0;i<POOL_SIZE - 1;i++)
	{
		pool[i].NextLink = &pool[i + 1];
	}
	mPools.push_back(pool);

	mAllocPoint = &pool[0];
}

extern "C"
{
	 void VCriticalInfoManager_SetEnable(vBOOL enable)
	{
		VCriticalInfoManager::IsDebugMTLocker = enable;
	}

	 LPCSTR VCriticalInfoManager_PrintLockInfo()
	{
		static VString debugInfo; 
		debugInfo = VCriticalInfoManager::Instance.PrintLockInfo();
		return debugInfo.c_str();
	}
}