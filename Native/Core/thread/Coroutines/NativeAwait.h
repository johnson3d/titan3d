#include "Task.h"

NS_BEGIN

template<class TResult>
struct FEventPoster
{
	typedef TResult(FPostFunction)(ITaskState* pState);
	static auto PostExec(std::function<FPostFunction> func, int targetId)
	{
		FTask<TResult> task;
		task.mState->mPostExec = func;
		task.mState->PostTargetThread = targetId;

		return task.GetFuture();
	}
};

struct FFutureSemaphone;

class FSemaphore : public VIUnknown
{
	std::atomic<int>		Count;
	AutoRef<ITaskState>		TaskState;
public:
	typedef FTask<void> promise_type;
	FSemaphore(int num)
	{
		Count = num;
	}
	int GetCount()
	{
		return Count;
	}
	void ReduceCount();

	FFutureSemaphone Await();
};

struct FTaskSemaphone;
struct FFutureSemaphone
{
	typedef FTaskSemaphone promise_type;

	FFutureSemaphone(const AutoRef<ITaskState>& state)
	{
		mState = state;
	}
	bool await_ready() const noexcept
	{
		return false;
	}
	void await_resume() noexcept
	{

	}
	template<class _PromiseT>
	void await_suspend(std::experimental::coroutine_handle<_PromiseT> awaitingCoroutine) noexcept
	{
		if (mState->mSemaphore != nullptr)
		{
			if (mState->mSemaphore->GetCount() != 0)
			{
				mState->mPostCompleted = false;
			}
			else
			{
				mState->mPostCompleted = true;
			}
			mState->await_suspend(awaitingCoroutine);
		}
	}


	FFutureSemaphone(const FFutureSemaphone&) = default;
	FFutureSemaphone(FFutureSemaphone&&) = default;
	FFutureSemaphone& operator=(const FFutureSemaphone&) = default;
	FFutureSemaphone& operator=(FFutureSemaphone&&) = default;

	AutoRef<ITaskState>	mState;
	auto GetState() {
		return mState;
	}
};

struct FTaskSemaphone
{
	FTaskSemaphone()
		: mState(new FTaskState<void>())
	{

	}
	FTaskSemaphone(ITaskState* state)
	{
		mState.StrongRef(state);
	}
	/*FTaskSemaphone& operator = (FTaskSemaphone&& _Right) noexcept
	{
		if (this != _Right)
		{
			mState = std::move(_Right.mState);
		}
		return *this;
	}*/
	FTaskSemaphone(const FTaskSemaphone&) = delete;
	FTaskSemaphone& operator = (const FTaskSemaphone&) = delete;
	AutoRef<ITaskState>		mState;
	FFutureSemaphone GetFuture()
	{
		return FFutureSemaphone(mState);
	}
	FFutureSemaphone get_return_object()
	{
		return FFutureSemaphone(mState);
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

NS_END