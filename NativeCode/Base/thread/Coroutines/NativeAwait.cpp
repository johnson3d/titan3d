#include "NativeAwait.h"

NS_BEGIN

void FSemaphore::ReduceCount()
{
	--Count;
	if (Count == 0)
	{
		if (TaskState != nullptr)
		{
			TaskState->mScheduler->PushContinue(TaskState);
			TaskState = nullptr;
		}
	}
}

FFutureSemaphone FSemaphore::Await()
{
	FTaskSemaphone task;
	ITaskState* pNaked = task.mState;
	TaskState.StrongRef(pNaked);
	TaskState->mSemaphore = this;
	return task.GetFuture();
}

NS_END