#pragma once

#include <experimental/coroutine>
#include <atomic>
#include <iostream>
#include <functional>
#include <thread>
#include <queue>
#include <string>

#include "../../../IUnknown.h"
#include "TaskScheduler.h"

NS_BEGIN

class TaskScheduler;
class FSemaphore;
template <typename T = void>
struct FFuture;

struct ITaskState : public VIUnknown
{
	static int NumOfITaskState;
	ITaskState();
	~ITaskState();
	virtual void ExecutePostFunction() = 0;
	virtual void ExecuteContinue() = 0;
	typedef void FOnAwaitSuspend(ITaskState* pState);
	
	TaskScheduler* mScheduler = nullptr;
	AutoRef<ITaskState> mParentState = nullptr;
	ITaskState* mChildState = nullptr;
	std::experimental::coroutine_handle<> mContinueExec;
	std::atomic<bool>				mPostCompleted;

	bool IsPostCompleted()
	{
		if (mPostCompleted == false)
			return false;
		if (mChildState != nullptr)
		{
			return mChildState->IsPostCompleted();
		}
		return true;
	}

	template<class _PromiseT>
	void await_suspend(std::experimental::coroutine_handle<_PromiseT> awaitingCoroutine) noexcept;

	TaskScheduler* GetScheduler()
	{
		return mScheduler;
	}

	int PostTargetThread;
	FSemaphore* mSemaphore;
};

template<class TResult>
struct FTaskStateBase : ITaskState
{
	typedef TResult(FPostFunction)(ITaskState* pState);
	std::function<FPostFunction>	mPostExec;

	virtual void ExecuteContinue() override
	{
		if(mContinueExec!=nullptr)
			mContinueExec();
	}
};

template<class TResult>
struct FTaskState : public FTaskStateBase<TResult>
{
	TResult							mResult;

	void SetValue(const TResult& t)
	{
		mResult = t;
	}
	void SetValue(TResult&& t)
	{
		mResult = std::forward<TResult>(t);
	}
	TResult& GetValue()
	{
		return mResult;
	}
	virtual void ExecutePostFunction() override
	{
		if (FTaskStateBase<TResult>::mPostExec != nullptr)
			mResult = FTaskStateBase<TResult>::mPostExec(this);
	}
};

template<>
struct FTaskState<void> : public FTaskStateBase<void>
{
	virtual void ExecutePostFunction() override
	{
		if(FTaskStateBase<void>::mPostExec!=nullptr)
			FTaskStateBase<void>::mPostExec(this);
	}
};

NS_END