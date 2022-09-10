#pragma once
#include "../debug/vfxdebug.h"
#include "../string/vfxstring.h"

class vfxEvent
{
public:
	std::string mName;
	vBOOL	mIsValid;
	pthread_mutex_t mMutex;
	pthread_cond_t mCond;
	vBOOL mValid;
	vfxEvent();
	~vfxEvent();
	void CreateEvent(LPCSTR name);
	void DestroyEvent();

	bool IsValid();

	bool WaitOne(DWORD dwMilliseconds = INFINITE);

	void SetEvent();
	void Reset();
};
