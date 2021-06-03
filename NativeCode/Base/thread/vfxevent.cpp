#include "vfxevent.h"

#define new VNEW


#ifdef IOS
vfxEvent::vfxEvent()
{
	pthread_mutex_init(&mMutex, NULL);
	mCond = PTHREAD_COND_INITIALIZER;
}
vfxEvent::~vfxEvent()
{
	ASSERT(mCond.__sig == 0);
	DestroyEvent();

	if (0 != mMutex.__sig)
	{
		pthread_mutex_destroy(&mMutex);
		mMutex = PTHREAD_MUTEX_INITIALIZER;
	}
}
void vfxEvent::CreateEvent(LPCSTR name)
{
	mName = name;
	ASSERT(mCond.__sig == 0);
	DestroyEvent();


	//pthread_condattr_t attr;
	pthread_cond_init(&mCond, NULL);
}
void vfxEvent::DestroyEvent()
{
	if (0 != mCond.__sig)
	{
		pthread_cond_destroy(&mCond);
		mCond = PTHREAD_COND_INITIALIZER;
	}
}
bool vfxEvent::IsValid()
{
	return mCond.__sig != 0 ? true : false;
}
bool vfxEvent::WaitOne(DWORD dwMilliseconds)
{
	pthread_mutex_lock(&mMutex);
	if (dwMilliseconds == INFINITE)
	{
		pthread_cond_wait(&mCond, &mMutex);
	}
	else
	{
		timespec tm;
		tm.tv_sec = dwMilliseconds / 1000;
		tm.tv_nsec = (dwMilliseconds % 1000) * 1000000;
		pthread_cond_timedwait(&mCond, &mMutex, &tm);
	}
	pthread_mutex_unlock(&mMutex);
	return true;
}
void vfxEvent::SetEvent()
{
	pthread_mutex_lock(&mMutex);
	pthread_cond_signal(&mCond);
	pthread_mutex_unlock(&mMutex);
}
void vfxEvent::Reset()
{
	pthread_mutex_lock(&mMutex);
	if (0 != mCond.__sig)
	{
		pthread_cond_init(&mCond, NULL);
	}
	pthread_mutex_unlock(&mMutex);
}
#else
vfxEvent::vfxEvent()
{
	mValid = FALSE;
}
vfxEvent::~vfxEvent()
{
	//ASSERT(mValid == FALSE);
	DestroyEvent();
}
void vfxEvent::CreateEvent(LPCSTR name)
{
	ASSERT(mValid == FALSE);
	pthread_mutex_init(&mMutex, NULL);
	//pthread_condattr_t attr;		
	pthread_cond_init(&mCond, NULL);
	mValid = TRUE;
}
void vfxEvent::DestroyEvent()
{
	//ASSERT(mValid);
	if (mValid)
	{
		pthread_cond_destroy(&mCond);
		pthread_mutex_destroy(&mMutex);
		mValid = FALSE;
	}
}
bool vfxEvent::IsValid()
{
	return mValid ? true : false;
}
bool vfxEvent::WaitOne(DWORD dwMilliseconds)
{
	ASSERT(mValid);
	pthread_mutex_lock(&mMutex);
	if (dwMilliseconds == INFINITE)
	{
		pthread_cond_wait(&mCond, &mMutex);
	}
	else
	{
		timespec tm;
		tm.tv_sec = dwMilliseconds / 1000;
		tm.tv_nsec = (dwMilliseconds % 1000)*1000000;
		pthread_cond_timedwait(&mCond, &mMutex, &tm);
	}
	pthread_mutex_unlock(&mMutex);
	return true;
}
void vfxEvent::SetEvent()
{
	ASSERT(mValid);
	pthread_mutex_lock(&mMutex);
	pthread_cond_signal(&mCond);
	pthread_mutex_unlock(&mMutex);
}
void vfxEvent::Reset()
{
	ASSERT(mValid);
	//ASSERT(0 != mCond.value);
	pthread_mutex_lock(&mMutex);
	pthread_cond_init(&mCond, NULL);
	/*if (0 != mCond.value)
	{
		pthread_cond_init(&mCond, NULL);
	}*/
	pthread_mutex_unlock(&mMutex);
}
#endif