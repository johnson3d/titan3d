#pragma once
#include "TaskState.h"
#include "TaskScheduler.h"
#include <assert.h>

NS_BEGIN

template<class TResult>
struct FTask;

template<class TResult>
struct FFutureBase
{
	typedef FTask<TResult> promise_type;

	FFutureBase(const AutoRef<FTaskState<TResult>>& state)
	{
		mState = state;
	}
	bool await_ready() const noexcept
	{
		return false;
	}

	template<class _PromiseT>
	void await_suspend(std::experimental::coroutine_handle<_PromiseT> awaitingCoroutine) noexcept
	{
		mState->await_suspend(awaitingCoroutine);
	}


	FFutureBase(const FFutureBase&) = default;
	FFutureBase(FFutureBase&&) = default;
	FFutureBase& operator=(const FFutureBase&) = default;
	FFutureBase& operator=(FFutureBase&&) = default;

	AutoRef<FTaskState<TResult>>	mState;
	auto GetState() {
		return mState;
	}
};

template<class TResult>
struct FFuture : public FFutureBase<TResult>
{
	FFuture(const AutoRef<FTaskState<TResult>>& state)
		: FFutureBase<TResult>(state)
	{

	}
	FFuture(const FFuture&) = default;
	FFuture(FFuture&& f) = default;

	FFuture& operator = (const FFuture&) = default;
	FFuture& operator = (FFuture&& f) = default;

	void return_value(const TResult& val)
	{
		FFutureBase<TResult>::mState->SetValue(val);
	}
	void return_value(TResult&& val)
	{
		FFutureBase<TResult>::mState->SetValue(std::forward<TResult>(val));
	}
	TResult await_resume() noexcept
	{
		return FFutureBase<TResult>::mState->mResult;
	}
};

template<>
struct FFuture<void> : public FFutureBase<void>
{
	FFuture(const AutoRef<FTaskState<void>>& state)
		: FFutureBase<void>(state)
	{

	}

	FFuture(const FFuture&) = default;
	FFuture(FFuture&& f) = default;

	FFuture& operator = (const FFuture&) = default;
	FFuture& operator = (FFuture&& f) = default;

	void return_void()
	{

	}
	void await_resume() noexcept
	{

	}
};

template<class TResult>
struct FTaskBase
{
	FTaskBase()
		: mState(new FTaskState<TResult>())
	{

	}
	FTaskBase(FTaskState<TResult>&& state)
		: mState(std::move(state))
	{

	}
	FTaskBase& operator = (FTaskBase&& _Right) noexcept
	{
		if (this != _Right)
		{
			mState = std::move(_Right.mState);
		}
		return *this;
	}
	FTaskBase(const FTaskBase&) = delete;
	FTaskBase& operator = (const FTaskBase&) = delete;
	AutoRef<FTaskState<TResult>>		mState;
	FFuture<TResult> GetFuture()
	{
		return FFuture<TResult>(mState);
	}
	FFuture<TResult> get_return_object()
	{
		return FFuture<TResult>(mState);
	}
	std::experimental::suspend_never initial_suspend()
	{
		return {};
	}
	std::experimental::suspend_never final_suspend()
	{
		return {};
	}
	void unhandled_exception()
	{
	}
	auto GetState() {
		return mState;
	}
};

template<class TResult>
struct FTask : public FTaskBase<TResult>
{
	typedef FTask<TResult> promise_type;
	void return_value(const TResult& v)
	{
		FTaskBase<TResult>::mState->SetValue(v);
	}
	void return_value(TResult&& v)
	{
		FTaskBase<TResult>::mState->SetValue(std::forward<TResult>(v));
	}
};

template<>
struct FTask<void> : public FTaskBase<void>
{
	void return_void()
	{

	}
};

template<class _PromiseT>
void ITaskState::await_suspend(std::experimental::coroutine_handle<_PromiseT> awaitingCoroutine) noexcept
{
	mContinueExec = awaitingCoroutine;

	auto& promise = awaitingCoroutine.promise();

	auto parentState = promise.GetState().GetPtr();
	mScheduler = parentState->GetScheduler();

	assert(parentState != this);
	
	mParentState.StrongRef(parentState);
	parentState->mChildState = this;
	
	if (this->PostTargetThread != -1)
	{
		this->mPostCompleted = false;
		auto targetScheduler = TaskScheduler::GetSchedulerByTargetId(this->PostTargetThread);
		targetScheduler->PushPost(this);
	}
	else if(this->IsPostCompleted())
	{
		this->mPostCompleted = false;
		mScheduler->PushContinue(this);
	}
}

NS_END