// vfxcritical.h
// 
// VictoryCore Code
// �̰߳�ȫ
//
// Author : johnson
// More author :
// Create time : 2002-6-13
// Modify time :
//-----------------------------------------------------------------------------

#ifndef __VFX_CRITICAL_H__
#define __VFX_CRITICAL_H__

#pragma once

//#include "../vfxinterface.h"
//#include "../vfxSampCounter.h"
#include "../debug/vfxdebug.h"
#include "../string/vfxstring.h"
#include <stack>

struct VCritical;

struct VCritical
{
	pthread_mutex_t	m_Critical;

	 VCritical();
	 ~VCritical();
	inline void Lock()
	{
		pthread_mutex_lock(&m_Critical);
	}
	inline void Unlock()
	{
		pthread_mutex_unlock(&m_Critical);
	}
	inline int TryLock() 
	{
		return pthread_mutex_trylock(&m_Critical);
	}
};

struct VSLLock
{
	VSLLock()
	{
		mLocker = 0;
	}
	std::atomic<long> mLocker;
	inline void Lock() {
		while (mLocker.exchange(1))
			Sleep(0);
	}
	inline void Unlock() {
		mLocker.exchange(0);
	}
};

struct VSLLockNoSleep
{
	VSLLockNoSleep()
	{
		mLocker = 0;
	}
	std::atomic<long> mLocker;
	inline void Lock() {
		while (mLocker.exchange(1))
		{
			;
		}
	}
	inline void Unlock() {
		mLocker.exchange(0);
	}
};

template <class LK>
struct VAutoVSLLock
{
	LK&	__lock;

	VAutoVSLLock(LK & t)
		:__lock(t)
	{
		__lock.Lock();
	};
	~VAutoVSLLock() {
		__lock.Unlock();
	}
};

//////////////////////////////////////////////////////////////////////////
struct VCriticalInfo
{
	VCriticalInfo(LPCSTR f, int l)
	{
		mFile = f;
		mLine = l;
	}
	LPCSTR		mFile;
	int			mLine;
};

struct VCriticalInfoStack
{
	std::vector<VCriticalInfo> mStacks;

	void PushLock(LPCSTR file, int line);
	void PopLock();
};

struct VCriticalThread
{
	VCriticalThread();
	~VCriticalThread();
	std::map<VCritical*, VCriticalInfoStack*>	mLockers;
	void PushLock(VCritical* locker, LPCSTR file, int line);
	void PopLock(VCritical* locker);
};

class VCriticalInfoManager
{
	std::map<INT64, VCriticalThread*> mThreads;

	VSLLock mManagerLocker;

	inline INT64 GetCurrentThreadId()
	{
#if defined(PLATFORM_WIN)
		//return (INT64)::GetCurrentThreadId();
		return (INT64)pthread_self().p;
#else
		return (INT64)pthread_self();
#endif
	}
public:
	static vBOOL IsDebugMTLocker;
	static VCriticalInfoManager Instance;

	VCriticalInfoManager();
	~VCriticalInfoManager();
	void PushLock(VCritical* locker, LPCSTR file, int line);

	void PopLock(VCritical* locker);

	VString PrintLockInfo();
};

//////////////////////////////////////////////////////////////////////////

template<class T = VCritical>
struct VAutoLockImpl
{
	T &	__lock;

	VAutoLockImpl(T & t, LPCSTR file, int line)
		:__lock(t)
	{
		if (VCriticalInfoManager::IsDebugMTLocker)
		{
			VCriticalInfoManager::Instance.PushLock(&__lock, file, line);
		}
		__lock.Lock();
	};
	~VAutoLockImpl(){
		__lock.Unlock();
		if (VCriticalInfoManager::IsDebugMTLocker)
		{
			VCriticalInfoManager::Instance.PopLock(&__lock);
		}
	}
};

#define VAutoLockEx(locker,index) VAutoLockInner(locker,__FILE__,__LINE__,index)
#define VAutoLock(locker) VAutoLockInner(locker,__FILE__,__LINE__,0)
#define VAutoLockInner(locker,file,line,index) VAutoLockImpl<VCritical> _vautolock_##line##index(locker, file, line)



struct VPagedCritical
{
	VCritical mLocker;
	VPagedCritical* NextLink;
	int mLockCount;

	VPagedCritical();
	~VPagedCritical();

	inline void Lock(LPCSTR file, int line)
	{
		if (VCriticalInfoManager::IsDebugMTLocker)
		{
			VCriticalInfoManager::Instance.PushLock(&mLocker, file, line);
		}
		mLocker.Lock();
	}
	inline void Unlock()
	{
		mLocker.Unlock();
		if (VCriticalInfoManager::IsDebugMTLocker)
		{
			VCriticalInfoManager::Instance.PopLock(&mLocker);
		}
	}
};

class vfxMTLockerManager
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
	VMem::vector<VPagedCritical*, MyAllocator>		mPools;

	VPagedCritical*					mAllocPoint;

	void NewPool();
public:
	VSLLock mSelfLocker;
	static vfxMTLockerManager Instance;

	 vfxMTLockerManager();
	 ~vfxMTLockerManager();
	void Cleanup();

	inline VPagedCritical* AllocLocker()
	{
		if (mAllocPoint == NULL)
		{
			NewPool();
		}
		VPagedCritical* temp = mAllocPoint;
		mAllocPoint = mAllocPoint->NextLink;
		return temp;
	}

	inline void FreeLocker(VPagedCritical* locker)
	{
		locker->NextLink = mAllocPoint;
		mAllocPoint = locker;
	}
};

class vfxObjectLocker
{
	VPagedCritical*		mLocker;
public:
	vfxObjectLocker()
		: mLocker(NULL)
	{

	}
	~vfxObjectLocker()
	{
		ASSERT(mLocker == NULL);
	}

	inline void Lock(LPCSTR file, DWORD line)
	{
		{
			VSLLock& locker = vfxMTLockerManager::Instance.mSelfLocker;
			locker.Lock();
			if (mLocker == NULL)
			{
				mLocker = vfxMTLockerManager::Instance.AllocLocker();
			}
			mLocker->mLockCount++;
			locker.Unlock();
		}

		mLocker->Lock(file, line);
	}
	inline void Unlock()
	{
		ASSERT(mLocker);
		mLocker->Unlock();

		{
			VSLLock& locker = vfxMTLockerManager::Instance.mSelfLocker;
			locker.Lock();
			mLocker->mLockCount--;
			if (mLocker->mLockCount == 0)
			{
				vfxMTLockerManager::Instance.FreeLocker(mLocker);
				mLocker = NULL;
			}
			locker.Unlock();
		}
	}
};

template<typename objType,int index>
struct VAutoObjectLockerImpl
{
	objType* mObject;
	VAutoObjectLockerImpl(objType* obj, LPCSTR file, DWORD line)
	{
		mObject = obj;
		if (obj != NULL)
		{
			auto locker = obj->GetLocker(index);
			if (locker)
			{
				locker->Lock(file, line);
			}
			else
			{
				VFX_LTRACE(ELTT_Error, "VAutoObjectLockerImpl[%s:%d] Locker is null", file, line);
			}
		}
	}
	~VAutoObjectLockerImpl()
	{
		if (mObject != NULL)
		{
			auto locker = mObject->GetLocker(index);
			if (locker)
			{
				locker->Unlock();
			}
		}
	}
};


#define VAutoObjectLockerEx(obj,index) VAutoObjectLockerInner(obj,index,__FILE__,__LINE__)
#define VAutoObjectLocker(obj) VAutoObjectLockerInner(obj,0,__FILE__,__LINE__)
#define VAutoObjectLockerInner(obj,index,file,line) VAutoObjectLockerImpl<std::remove_reference<decltype(*obj)>::type,index> _autoobjlock_##line##index(obj,file,line)

#endif // end __VFX_CRITICAL_H__

