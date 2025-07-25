#pragma once
#include "../IUnknown.h"
#include "vfxcritical.h"

NS_BEGIN

template<class VThreadContext>
class VThreadDispatcher : public IWeakRefObject
{
	thread_local static VThreadContext*		Context;
	std::vector<VThreadContext**>			mThreadContexts;
	VSLLock									mLocker;
	bool									mDisposed = false;
public:
	void FinalCleanup()
	{
		mDisposed = true;
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
		if (mDisposed)
			return nullptr;
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
	FContextTickableManager : public IWeakRefObject
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

class FTaskSession;
TR_CALLBACK(SV_CallConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)
typedef void(*FRunTasks)(FTaskSession* session);

class TR_CLASS()
	IParallelTask : public VIUnknown
{
public:
	virtual void DoWork(FTaskSession* session) = 0;
};

class TR_CLASS()
	FTaskSession : public VIUnknown
{
	std::vector<IParallelTask*> Tasks;
public:
	std::vector<IParallelTask*>& GetTasks() {
		return Tasks;
	}
	UINT GetNumOfTasks() {
		return (UINT)Tasks.size();
	}
	void Execute(int index)
	{
		Tasks[index]->DoWork(this);
		Count--;
	}
	std::atomic<int> Count;
};

class TR_CLASS()
	VParallelTaskManager : public VIUnknown
{
	static FRunTasks mRunTaskFunction;
public:
	static bool RunTasks(FTaskSession * session);
	static void Wait(FTaskSession* session);
	static void SetFunction(FRunTasks ptr) {
		mRunTaskFunction = ptr;
	}
};

NS_END