#include "TaskScheduler.h"

NS_BEGIN

TaskScheduler* GSchedulerArray[32] = {};

TaskScheduler* TaskScheduler::GetCurrentInstance()
{
	static thread_local TaskScheduler Instance;
	return &Instance;
}

TaskScheduler* TaskScheduler::GetSchedulerByTargetId(int target)
{
	return GSchedulerArray[target];
}

void TaskScheduler::SetThreadInfo(const char* name, int id)
{
	GSchedulerArray[id] = this;
	TargetId = id;
	ThreadName = name;
}

void TaskScheduler::PushPost(ITaskState* state)
{
	state->AddRef();
	mPostExec.push(state);
}

void TaskScheduler::PushContinue(ITaskState* state)
{
	state->AddRef();
	mContinueExec.push(state);
}

void TaskScheduler::DoPost()
{
	while (mPostExec.size() > 0)
	{
		auto pState = mPostExec.front();
		mPostExec.pop();

		pState->ExecutePostFunction();
		pState->mPostCompleted = true;

		pState->GetScheduler()->PushContinue(pState);
		pState->Release();
	}
}

void TaskScheduler::DoContinue()
{
	while (mContinueExec.size() > 0)
	{
		auto pState = mContinueExec.front();
		mContinueExec.pop();

		while (pState != nullptr)
		{
			pState->ExecuteContinue();
			pState->mPostCompleted = true;

			if (pState->IsPostCompleted())
			{
				auto pParent = pState->mParentState;
				if (pParent != nullptr)
				{
					pParent->AddRef();
					if (pParent->mChildState == pState)
						pParent->mChildState = nullptr;
				}
				pState->mParentState = nullptr;
				pState->Release();
				pState = pParent;

				if (pState != nullptr && pState->IsPostCompleted() == false)
				{
					pState->Release();
					break;
				}
			}
			else
			{
				break;
			}
		}
	}
}

NS_END