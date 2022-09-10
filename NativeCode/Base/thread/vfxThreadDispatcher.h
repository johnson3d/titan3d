#pragma once
#include "../IUnknown.h"
#include "vfxcritical.h"

NS_BEGIN

template<class VThreadContext>
class VThreadDispatcher : public VIUnknown
{
	thread_local static VThreadContext*		Context;
	std::vector<VThreadContext**>			mThreadContexts;
	VSLLock									mLocker;
public:
	void FinalCleanup()
	{
		VAutoVSLLock lk(mLocker);
		for (auto i : mThreadContexts)
		{
			auto pContext = (*i);
			pContext->FinalCleanup();
			(*i) = nullptr;
			delete pContext;
		}
		mThreadContexts.clear();
	}
	VThreadContext* GetThreadContext()
	{
		if (Context == nullptr)
		{
			Context = new VThreadContext();
			InitContext(Context);
			VAutoVSLLock lk(mLocker);
			mThreadContexts.push_back(&Context);
		}
		return Context;
	}
	virtual void InitContext(VThreadContext* context) = 0;
};

template<class VThreadContext>
thread_local VThreadContext* VThreadDispatcher<VThreadContext>::Context = nullptr;

typedef bool (FThreadContextTick)();

class TR_CLASS()
	FContextTickableManager : public VIUnknown
{
	std::vector<std::function<FThreadContextTick>>	mTickables;
public:
	static FContextTickableManager* GetInstance();
	void ThreadTick();
	UINT PushTickable(std::function<FThreadContextTick> evt)
	{
		auto result = (UINT)mTickables.size();
		mTickables.push_back(evt);
		return result;
	}
	void RemoveTickable(UINT index)
	{
		mTickables.erase(mTickables.begin() + index);
	}
	void ClearTickables()
	{
		mTickables.clear();
	}
};

NS_END